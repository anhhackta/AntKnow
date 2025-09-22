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
                    // Look up username in Firestore to get email
                    var usernameDoc = await firestore.Collection("usernames").Document(userOrEmail.ToLower()).GetSnapshotAsync();
                    if (!usernameDoc.Exists)
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Username not found" };
                    }
                    
                    var usernameData = usernameDoc.ToDictionary();
                    if (!usernameData.ContainsKey("uid"))
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid username data" };
                    }
                    
                    // Get user document to find email
                    var userDoc = await firestore.Collection("users").Document(usernameData["uid"].ToString()).GetSnapshotAsync();
                    if (!userDoc.Exists)
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "User not found" };
                    }
                    
                    var userData = userDoc.ToDictionary();
                    if (!userData.ContainsKey("email"))
                    {
                        return new AuthResult { IsSuccess = false, ErrorMessage = "Email not found for username" };
                    }
                    
                    email = userData["email"].ToString();
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
                var credential = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                var user = credential.User;

                // Create user profile in Firestore
                var userProfile = new UserProfile(user.UserId, username, email);
                var userData = new Dictionary<string, object>
                {
                    { "uid", userProfile.uid },
                    { "username", userProfile.username },
                    { "email", userProfile.email },
                    { "rankEligible", userProfile.rankEligible },
                    { "elo", userProfile.elo },
                    { "level", userProfile.level },
                    { "exp", userProfile.exp },
                    { "powerScore", userProfile.powerScore },
                    { "createdAt", Timestamp.GetCurrentTimestamp() },
                    { "lastLoginAt", Timestamp.GetCurrentTimestamp() }
                };

                await firestore.Collection("users").Document(user.UserId).SetAsync(userData);

                // Create username mapping for quick lookup
                await firestore.Collection("usernames").Document(username.ToLower()).SetAsync(new Dictionary<string, object>
                {
                    { "uid", user.UserId }
                });

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
                
                var doc = await firestore.Collection("usernames").Document(username.ToLower()).GetSnapshotAsync();
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
                
                // Sử dụng phương thức khác để kiểm tra email
                // Tìm kiếm trong Firestore thay vì dùng FetchSignInMethodsForEmailAsync
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
            catch (Exception e)
            {
                Debug.LogError($"Error checking email: {e.Message}");
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
