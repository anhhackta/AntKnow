using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;

namespace AntKnow.Auth
{
    public class FirebaseAuthService : MonoBehaviour
    {
        [Header("Firebase Configuration")]
        public bool isInitialized = false;
        
        private FirebaseApp app;
        private FirebaseAuth auth;
        private FirebaseFirestore firestore;
        
        public FirebaseAuth Auth => auth;
        
        public event Action<FirebaseUser> OnUserSignedIn;
        public event Action OnUserSignedOut;
        public event Action<string> OnAuthError;

        /// <summary>
        /// Kiểm tra Firebase có thực sàng sẵn sàng không
        /// </summary>
        public bool IsFirebaseReady()
        {
            // Luôn kiểm tra các services, không dựa vào isInitialized
            return app != null && 
                   auth != null && 
                   firestore != null;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initialize Firebase services
        /// </summary>
        public async Task<bool> InitAsync()
        {
            try
            {
                Debug.Log("Starting Firebase initialization...");
                
                // Initialize Firebase
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    OnAuthError?.Invoke("Firebase initialization failed");
                    return false;
                }

                Debug.Log("Firebase dependencies resolved successfully");

                app = FirebaseApp.DefaultInstance;
                Debug.Log($"FirebaseApp initialized: {app != null}");

                auth = FirebaseAuth.DefaultInstance;
                Debug.Log($"FirebaseAuth initialized: {auth != null}");

                firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log($"Firestore initialized: {firestore != null}");

                // Listen to auth state changes
                if (auth != null)
                {
                    auth.StateChanged += OnAuthStateChanged;
                    Debug.Log("Auth state change listener added");
                }

                // Thêm delay nhỏ để đảm bảo tất cả services đã sẵn sàng
                await Task.Delay(100);

                isInitialized = true;
                Debug.Log("✅ Firebase initialized successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Firebase initialization error: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
                OnAuthError?.Invoke("Firebase initialization failed");
                return false;
            }
        }

        private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (auth.CurrentUser != null)
            {
                OnUserSignedIn?.Invoke(auth.CurrentUser);
            }
            else
            {
                OnUserSignedOut?.Invoke();
            }
        }

        /// <summary>
        /// Sign in with email or username and password
        /// </summary>
        public async Task<AuthResult> SignInWithEmailOrUsernameAsync(string userOrEmail, string password)
        {
            try
            {
                // Khởi tạo Firebase nếu chưa có
                if (!IsFirebaseReady())
                {
                    if (isInitialized)
                    {
                        // Nếu isInitialized = true nhưng services chưa có, khởi tạo
                        await InitAsync();
                    }
                    else
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Firebase not initialized" };
                    }
                }

                string email;
                
                // Kiểm tra firestore đã sẵn sàng chưa
                if (firestore == null)
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Firestore not initialized" };
                }

                // Check if input contains @ (email) or is username
                if (userOrEmail.Contains("@"))
                {
                    email = userOrEmail;
                }
                else
                {
                    // Look up username in handles collection to get email
                    var handleDoc = await firestore.Collection("handles").Document(userOrEmail.ToLower()).GetSnapshotAsync();
                    if (!handleDoc.Exists)
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Username not found" };
                    }
                    
                    var handleData = handleDoc.ToDictionary();
                    if (!handleData.ContainsKey("email"))
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Email not found for username" };
                    }
                    
                    email = handleData["email"].ToString();
                }

                // Sign in with email and password
                var credential = await auth.SignInWithEmailAndPasswordAsync(email, password);
                
                // Update last login time
                await UpdateLastLoginTimeAsync(credential.User.UserId);
                
                return new AuthResult { IsSuccess = true, User = credential.User };
            }
            catch (FirebaseException e)
            {
                string errorMessage = GetFirebaseErrorMessage(e);
                OnAuthError?.Invoke(errorMessage);
                return new AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
            }
            catch (Exception e)
            {
                string errorMessage = $"Sign in error: {e.Message}";
                OnAuthError?.Invoke(errorMessage);
                return new AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
            }
        }

        /// <summary>
        /// Sign in with Google (tạm thời khóa)
        /// </summary>
        public Task<AuthResult> SignInWithGoogleAsync()
        {
            // Tạm thời khóa Google Login - đang update
            return Task.FromResult(new AuthResult 
            { 
                IsSuccess = false, 
                ErrorMessage = "Google Login đang được cập nhật" 
            });
        }

        /// <summary>
        /// Register new user with username, email and password
        /// </summary>
        public async Task<AuthResult> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Khởi tạo Firebase nếu chưa có
                if (!IsFirebaseReady())
                {
                    if (isInitialized)
                    {
                        // Nếu isInitialized = true nhưng services chưa có, khởi tạo
                        await InitAsync();
                    }
                    else
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Firebase not initialized" };
                    }
                }

                // Kiểm tra firestore đã sẵn sàng chưa
                if (firestore == null)
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Firestore not initialized" };
                }

                // Check if username is already taken
                if (await IsUsernameTakenAsync(username))
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Username already taken" };
                }

                // Check if email is already taken
                if (await IsEmailTakenAsync(email))
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Email already registered" };
                }

                // Create Firebase Auth user
                Debug.Log($"Creating Firebase Auth user for email: {email}");
                var credential = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                var user = credential.User;
                Debug.Log($"Firebase Auth user created successfully: {user.UserId}");

                // Create user data with new structure (chỉ basic info, không có inventory/loadout)
                Debug.Log($"Creating user document in Firestore: {user.UserId}");
                var userData = new UserData(user.UserId, username, email);
                await firestore.Collection("users").Document(user.UserId).SetAsync(userData.ToFirestoreData());
                Debug.Log($"User document created successfully in Firestore");

                // Create username handle mapping
                Debug.Log($"Creating handle mapping for username: {username}");
                await firestore.Collection("handles").Document(username.ToLower()).SetAsync(new Dictionary<string, object>
                {
                    { "uid", user.UserId },
                    { "email", email }
                });
                Debug.Log($"Handle mapping created successfully");

                Debug.Log($"Registration completed successfully for user: {username}");
                return new AuthResult { IsSuccess = true, User = user };
            }
            catch (FirebaseException e)
            {
                string errorMessage = GetFirebaseErrorMessage(e);
                OnAuthError?.Invoke(errorMessage);
                return new AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
            }
            catch (Exception e)
            {
                string errorMessage = $"Registration error: {e.Message}";
                OnAuthError?.Invoke(errorMessage);
                return new AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
            }
        }

        /// <summary>
        /// Check if username is already taken
        /// </summary>
        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            try
            {
                // Khởi tạo Firebase nếu chưa có
                if (!IsFirebaseReady())
                {
                    Debug.Log("Firebase not ready, initializing...");
                    await InitAsync();
                    
                    // Kiểm tra lại sau khi khởi tạo
                    if (!IsFirebaseReady())
                    {
                        Debug.LogError("Failed to initialize Firebase");
                        return false;
                    }
                }
                
                // Kiểm tra firestore đã sẵn sàng chưa
                if (firestore == null)
                {
                    Debug.LogError("Firestore is null, cannot check username");
                    return false;
                }
                
                // Check in handles collection
                var doc = await firestore.Collection("handles").Document(username.ToLower()).GetSnapshotAsync();
                return doc.Exists;
            }
            catch (FirebaseException e)
            {
                // Xử lý lỗi permissions cụ thể
                if (e.ErrorCode.ToString() == "PermissionDenied" || e.Message.Contains("permissions"))
                {
                    Debug.LogWarning("Permission denied for username check - Firestore rules may need update");
                    return false; // Coi như username chưa tồn tại để cho phép đăng ký
                }
                Debug.LogError($"Firebase error checking username: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error checking username: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if email is already registered
        /// </summary>
        public async Task<bool> IsEmailTakenAsync(string email)
        {
            try
            {
                // Khởi tạo Firebase nếu chưa có
                if (!IsFirebaseReady())
                {
                    Debug.Log("Firebase not ready, initializing...");
                    await InitAsync();
                    
                    // Kiểm tra lại sau khi khởi tạo
                    if (!IsFirebaseReady())
                    {
                        Debug.LogError("Failed to initialize Firebase");
                        return false;
                    }
                }
                
                // Kiểm tra firestore đã sẵn sàng chưa
                if (firestore == null)
                {
                    Debug.LogError("Firestore is null, cannot check email");
                    return false;
                }
                
                // Check in users collection
                var query = await firestore.Collection("users").WhereEqualTo("email", email).GetSnapshotAsync();
                return query.Count > 0;
            }
            catch (FirebaseException e)
            {
                // Xử lý lỗi permissions cụ thể
                if (e.ErrorCode.ToString() == "PermissionDenied" || e.Message.Contains("permissions"))
                {
                    Debug.LogWarning("Permission denied for email check - Firestore rules may need update");
                    return false; // Coi như email chưa tồn tại để cho phép đăng ký
                }
                Debug.LogError($"Firebase error checking email: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sign out current user
        /// </summary>
        public Task SignOutAsync()
        {
            try
            {
                if (auth.CurrentUser != null)
                {
                    auth.SignOut();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Sign out error: {e.Message}");
                OnAuthError?.Invoke("Sign out failed");
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update last login time for current user
        /// </summary>
        private async Task UpdateLastLoginTimeAsync(string uid)
        {
            try
            {
                await firestore.Collection("users").Document(uid).UpdateAsync("lastLoginAt", Timestamp.GetCurrentTimestamp());
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating last login time: {e.Message}");
            }
        }

        /// <summary>
        /// Get user data from Firestore
        /// </summary>
        public async Task<UserData> GetUserDataAsync(string uid)
        {
            try
            {
                if (!IsFirebaseReady() || firestore == null)
                {
                    Debug.LogError("Firebase not ready for GetUserDataAsync");
                    return null;
                }

                var doc = await firestore.Collection("users").Document(uid).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    Debug.LogError($"User document not found: {uid}");
                    return null;
                }

                var data = doc.ToDictionary();
                return UserData.FromFirestoreData(uid, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting user data: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update user's ingame name
        /// </summary>
        public async Task<bool> UpdateIngameNameAsync(string uid, string ingameName)
        {
            try
            {
                if (!IsFirebaseReady() || firestore == null)
                {
                    Debug.LogError("Firebase not ready for UpdateIngameNameAsync");
                    return false;
                }

                // Check if ingame name is already taken
                if (await IsIngameNameTakenAsync(ingameName))
                {
                    Debug.LogWarning($"Ingame name '{ingameName}' is already taken");
                    return false;
                }

                // Update user document
                await firestore.Collection("users").Document(uid).UpdateAsync("ingameName", ingameName);

                // Create ingame name mapping
                await firestore.Collection("ingame_names").Document(ingameName.ToLower()).SetAsync(new Dictionary<string, object>
                {
                    { "uid", uid }
                });

                Debug.Log($"Ingame name updated successfully: {ingameName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating ingame name: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if ingame name is already taken
        /// </summary>
        public async Task<bool> IsIngameNameTakenAsync(string ingameName)
        {
            try
            {
                if (!IsFirebaseReady() || firestore == null)
                {
                    Debug.LogError("Firebase not ready for IsIngameNameTakenAsync");
                    return false;
                }

                var doc = await firestore.Collection("ingame_names").Document(ingameName.ToLower()).GetSnapshotAsync();
                return doc.Exists;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error checking ingame name: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update user data in Firestore
        /// </summary>
        public async Task<bool> UpdateUserDataAsync(UserData userData)
        {
            try
            {
                if (!IsFirebaseReady() || firestore == null)
                {
                    Debug.LogError("Firebase not ready for UpdateUserDataAsync");
                    return false;
                }

                await firestore.Collection("users").Document(userData.uid).SetAsync(userData.ToFirestoreData());
                Debug.Log($"User data updated successfully: {userData.username}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating user data: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create initial inventory and loadout for user (gọi từ MenuScene)
        /// </summary>
        public async Task<bool> CreateInitialInventoryAndLoadoutAsync(string uid)
        {
            try
            {
                if (!IsFirebaseReady() || firestore == null)
                {
                    Debug.LogError("Firebase not ready for CreateInitialInventoryAndLoadoutAsync");
                    return false;
                }

                Debug.Log($"Creating initial inventory and loadout for user: {uid}");
                
                var inventoryRef = firestore.Collection("users").Document(uid).Collection("inventory");
                var loadoutRef = firestore.Collection("users").Document(uid).Collection("loadouts");

                // Create initial skill cards (4 cards theo DBview.md)
                var skillCards = new[]
                {
                    new { itemId = "skill.lan-tron", level = 1, stars = 0 },
                    new { itemId = "skill.sieu-sale", level = 1, stars = 0 },
                    new { itemId = "skill.bao-ke", level = 1, stars = 0 },
                    new { itemId = "skill.cham-chi", level = 1, stars = 0 }
                };

                var cardInstanceIds = new List<string>();

                foreach (var card in skillCards)
                {
                    var docRef = inventoryRef.Document();
                    cardInstanceIds.Add(docRef.Id);
                    
                    await docRef.SetAsync(new Dictionary<string, object>
                    {
                        { "type", "skill_card" },
                        { "itemId", card.itemId },
                        { "level", card.level },
                        { "stars", card.stars },
                        { "createdAt", Timestamp.GetCurrentTimestamp() },
                        { "updatedAt", Timestamp.GetCurrentTimestamp() }
                    });
                }

                // Create initial equipment (5 items theo DBview.md)
                var equipment = new[]
                {
                    new { itemId = "equip.hat.basic" },
                    new { itemId = "equip.shirt.basic" },
                    new { itemId = "equip.wings.basic" },
                    new { itemId = "equip.shoes.basic" },
                    new { itemId = "equip.mask.basic" }
                };

                var equipmentInstanceIds = new List<string>();

                foreach (var equip in equipment)
                {
                    var docRef = inventoryRef.Document();
                    equipmentInstanceIds.Add(docRef.Id);
                    
                    await docRef.SetAsync(new Dictionary<string, object>
                    {
                        { "type", "equipment" },
                        { "itemId", equip.itemId },
                        { "createdAt", Timestamp.GetCurrentTimestamp() },
                        { "updatedAt", Timestamp.GetCurrentTimestamp() }
                    });
                }

                // Create initial EXP cards
                await inventoryRef.Document("exp.small").SetAsync(new Dictionary<string, object>
                {
                    { "type", "exp_card" },
                    { "itemId", "exp.small" },
                    { "qty", 5 }, // 5 EXP cards nhỏ ban đầu
                    { "updatedAt", Timestamp.GetCurrentTimestamp() }
                });

                // Create initial loadout (slot1) với 2 skill cards đầu tiên và 5 equipment
                await loadoutRef.Document("slot1").SetAsync(new Dictionary<string, object>
                {
                    { "active", true },
                    { "skillCardIds", new[] { cardInstanceIds[0], cardInstanceIds[1] } }, // 2 skill cards đầu tiên
                    { "equipmentSet", new Dictionary<string, object>
                        {
                            { "hatId", equipmentInstanceIds[0] },
                            { "shirtId", equipmentInstanceIds[1] },
                            { "wingsId", equipmentInstanceIds[2] },
                            { "shoesId", equipmentInstanceIds[3] },
                            { "maskId", equipmentInstanceIds[4] }
                        }
                    },
                    { "updatedAt", Timestamp.GetCurrentTimestamp() }
                });

                Debug.Log($"Initial inventory and loadout created successfully for user: {uid}");
                Debug.Log($"- Created {skillCards.Length} skill cards");
                Debug.Log($"- Created {equipment.Length} equipment items");
                Debug.Log($"- Created 1 EXP card stack");
                Debug.Log($"- Created 1 loadout slot");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating initial inventory and loadout: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if user has inventory
        /// </summary>
        public async Task<bool> HasInventoryAsync(string uid)
        {
            try
            {
                if (!IsFirebaseReady() || firestore == null)
                {
                    Debug.LogError("Firebase not ready for HasInventoryAsync");
                    return false;
                }

                var inventoryRef = firestore.Collection("users").Document(uid).Collection("inventory");
                var snapshot = await inventoryRef.Limit(1).GetSnapshotAsync();
                return snapshot.Count > 0;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error checking inventory: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get user-friendly error message from Firebase exception
        /// </summary>
        private string GetFirebaseErrorMessage(FirebaseException e)
        {
            // Sử dụng string thay vì enum để tránh lỗi conversion
            string errorCode = e.ErrorCode.ToString();
            
            switch (errorCode)
            {
                case "InvalidEmail":
                    return "Invalid email address";
                case "UserNotFound":
                    return "User not found";
                case "WrongPassword":
                    return "Incorrect password";
                case "EmailAlreadyInUse":
                    return "Email already registered";
                case "WeakPassword":
                    return "Password is too weak";
                case "TooManyRequests":
                    return "Too many requests. Please try again later";
                case "NetworkRequestFailed":
                    return "Network error. Please check your connection";
                default:
                    return $"Authentication error: {e.Message}";
            }
        }

        private void OnDestroy()
        {
            if (auth != null)
            {
                auth.StateChanged -= OnAuthStateChanged;
            }
        }
    }

    /// <summary>
    /// Result of authentication operations
    /// </summary>
    [Serializable]
    public class AuthResult
    {
        public bool IsSuccess;
        public FirebaseUser User;
        public string ErrorMessage;
    }
}
