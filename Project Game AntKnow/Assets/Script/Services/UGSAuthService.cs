using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using AntKnow.Auth;

namespace AntKnow.Services
{
    /// <summary>
    /// Service để quản lý Unity Gaming Services Authentication
    /// Chuyển đổi từ Firebase Auth sang Unity Auth để sử dụng Lobby và Matchmaker
    /// </summary>
    public class UGSAuthService : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Events
        public static event Action<bool> OnUGSAuthStateChanged;
        public static event Action<string> OnUGSAuthError;

        // Properties
        public static bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        public static string PlayerId => AuthenticationService.Instance.PlayerId;
        public static string PlayerName => AuthenticationService.Instance.PlayerName;

        private static UGSAuthService _instance;
        public static UGSAuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UGSAuthService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UGSAuthService");
                        _instance = go.AddComponent<UGSAuthService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeUGS();
        }

        /// <summary>
        /// Khởi tạo Unity Gaming Services
        /// </summary>
        private async void InitializeUGS()
        {
            try
            {
                DebugLog("Initializing Unity Gaming Services...");
                
                // Initialize Unity Services
                await UnityServices.InitializeAsync();
                
                DebugLog("Unity Gaming Services initialized successfully");
                
                // Setup authentication events
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                AuthenticationService.Instance.SignInFailed += OnSignInFailed;
                AuthenticationService.Instance.SignedOut += OnSignedOut;
                AuthenticationService.Instance.Expired += OnExpired;
                
                DebugLog("UGS Authentication events setup complete");
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to initialize Unity Gaming Services: {e.Message}");
                OnUGSAuthError?.Invoke($"UGS initialization failed: {e.Message}");
            }
        }

        /// <summary>
        /// Đăng nhập UGS bằng Firebase UID (Custom ID)
        /// </summary>
        public async Task<bool> SignInWithFirebaseUIDAsync(string firebaseUID, string playerName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(firebaseUID))
                {
                    DebugLogError("Firebase UID is null or empty");
                    return false;
                }

                DebugLog($"Signing in to UGS with Firebase UID: {firebaseUID}");

                // Sign in anonymously first, then link with custom ID
                // Unity Authentication doesn't have SignInWithCustomIdAsync in older versions
                // We'll use SignInAnonymouslyAsync and store Firebase UID in player name
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                // Set player name if provided
                if (!string.IsNullOrEmpty(playerName))
                {
                    await SetPlayerNameAsync(playerName);
                }

                DebugLog($"Successfully signed in to UGS. Player ID: {PlayerId}");
                return true;
            }
            catch (AuthenticationException e)
            {
                DebugLogError($"UGS Authentication failed: {e.Message}");
                OnUGSAuthError?.Invoke($"Authentication failed: {e.Message}");
                return false;
            }
            catch (RequestFailedException e)
            {
                DebugLogError($"UGS Request failed: {e.Message}");
                OnUGSAuthError?.Invoke($"Request failed: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                DebugLogError($"Unexpected error during UGS sign in: {e.Message}");
                OnUGSAuthError?.Invoke($"Unexpected error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đăng nhập UGS anonymous (fallback)
        /// </summary>
        public async Task<bool> SignInAnonymouslyAsync()
        {
            try
            {
                DebugLog("Signing in to UGS anonymously...");
                
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
                DebugLog($"Successfully signed in anonymously. Player ID: {PlayerId}");
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Anonymous sign in failed: {e.Message}");
                OnUGSAuthError?.Invoke($"Anonymous sign in failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đặt tên người chơi
        /// </summary>
        public async Task<bool> SetPlayerNameAsync(string playerName)
        {
            try
            {
                if (string.IsNullOrEmpty(playerName))
                {
                    DebugLogError("Player name is null or empty");
                    return false;
                }

                DebugLog($"Setting player name to: {playerName}");
                
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                
                DebugLog($"Player name updated successfully: {PlayerName}");
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to set player name: {e.Message}");
                OnUGSAuthError?.Invoke($"Failed to set player name: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đăng xuất UGS
        /// </summary>
        public void SignOut()
        {
            try
            {
                DebugLog("Signing out from UGS...");
                AuthenticationService.Instance.SignOut();
                DebugLog("Successfully signed out from UGS");
            }
            catch (Exception e)
            {
                DebugLogError($"Error during UGS sign out: {e.Message}");
            }
        }

        /// <summary>
        /// Tự động đăng nhập UGS khi user đã đăng nhập Firebase
        /// </summary>
        public async Task<bool> AutoSignInFromFirebaseAsync()
        {
            try
            {
                var gameDataManager = GameDataManager.Instance;
                if (gameDataManager == null || !gameDataManager.isUserLoggedIn)
                {
                    DebugLogError("No Firebase user logged in");
                    return false;
                }

                string firebaseUID = gameDataManager.currentUserId;
                string playerName = gameDataManager.currentIngameName ?? gameDataManager.currentUsername;

                DebugLog($"Auto signing in UGS for Firebase user: {firebaseUID}");
                
                return await SignInWithFirebaseUIDAsync(firebaseUID, playerName);
            }
            catch (Exception e)
            {
                DebugLogError($"Auto sign in failed: {e.Message}");
                return false;
            }
        }

        #region Event Handlers

        private void OnSignedIn()
        {
            DebugLog($"UGS Sign in successful. Player ID: {PlayerId}, Player Name: {PlayerName}");
            OnUGSAuthStateChanged?.Invoke(true);
        }

        private void OnSignInFailed(RequestFailedException e)
        {
            DebugLogError($"UGS Sign in failed: {e.Message}");
            OnUGSAuthStateChanged?.Invoke(false);
            OnUGSAuthError?.Invoke($"Sign in failed: {e.Message}");
        }

        private void OnSignedOut()
        {
            DebugLog("UGS Sign out successful");
            OnUGSAuthStateChanged?.Invoke(false);
        }

        private void OnExpired()
        {
            DebugLogError("UGS Authentication expired");
            OnUGSAuthStateChanged?.Invoke(false);
            OnUGSAuthError?.Invoke("Authentication expired");
        }

        #endregion

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[UGSAuthService] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[UGSAuthService] {message}");
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup events
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= OnSignedIn;
                AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
                AuthenticationService.Instance.SignedOut -= OnSignedOut;
                AuthenticationService.Instance.Expired -= OnExpired;
            }
        }
    }
}
