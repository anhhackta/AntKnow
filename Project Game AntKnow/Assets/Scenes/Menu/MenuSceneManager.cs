using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AntKnow.Auth;
using AntKnow.Chat;
using TMPro;

namespace AntKnow.Auth
{
    /// <summary>
    /// Manager chính cho MenuScene - quản lý tất cả panels và navigation
    /// </summary>
    public class MenuSceneManager : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private PanelSliderManager panelSliderManager;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject panelRoom; // Panel Room for matchmaking and lobby

        [Header("Top Bar Components")]
        [SerializeField] private PanelAvatar panelAvatar;
        [SerializeField] private PanelMoney panelMoney;
        [SerializeField] private Button buttonSetting;

        [Header("Navigation Buttons")]
        [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonInventory;
        [SerializeField] private Button buttonUpgrade;
        [SerializeField] private Button buttonShop;

        [Header("Panel Components")]
        [SerializeField] private PanelHome panelHome;

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;
        
        [Header("Simple Chat")]
        [SerializeField] private SimpleChatManager simpleChatManager;

        private GameDataManager gameDataManager;

        private void Start()
        {
            InitializeMenuScene();
        }

        private async void InitializeMenuScene()
        {
            // Get GameDataManager instance
            gameDataManager = GameDataManager.Instance;

            // Check if user is logged in
            if (!gameDataManager.isUserLoggedIn)
            {
                Debug.LogError("MenuScene: No user logged in, redirecting to LoginScene");
                SceneManager.LoadScene("LoginScene");
                return;
            }

            // Check if user has BOTH ingame name AND gender
            bool hasIngameName = !string.IsNullOrEmpty(gameDataManager.currentIngameName);
            bool hasGender = !string.IsNullOrEmpty(gameDataManager.currentGender);

            if (!hasIngameName || !hasGender)
            {
                Debug.LogWarning($"MenuScene: User missing profile data (Name: {gameDataManager.currentIngameName}, Gender: {gameDataManager.currentGender}), redirecting to SelectCharacterScene");
                SceneManager.LoadScene("SelectCharacterScene");
                return;
            }

            Debug.Log($"MenuScene: Initializing for user {gameDataManager.currentUsername}");

            // Find FirebaseAuthService
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Initialize UGS (Unity Gaming Services)
            await InitializeUGS();

            // Setup UI
            SetupUI();
            SetupEventListeners();

            // Load user data and inventory
            await LoadUserDataAndInventory();
            
            // Initialize chat system
            await InitializeChatSystem();

            // Update character display after data is loaded
            if (panelHome != null)
            {
                Debug.Log("MenuScene: Updating character display after data load...");
                Debug.Log($"MenuScene: Current gender: '{gameDataManager.currentGender}'");
                Debug.Log($"MenuScene: Current ingame name: '{gameDataManager.currentIngameName}'");
                panelHome.ForceUpdateCharacterSprite();
            }
            else
            {
                Debug.LogWarning("MenuScene: PanelHome is null! Trying to find it...");
                panelHome = FindObjectOfType<PanelHome>();
                if (panelHome != null)
                {
                    Debug.Log("MenuScene: PanelHome found, updating character display");
                    panelHome.ForceUpdateCharacterSprite();
                }
                else
                {
                    Debug.LogError("MenuScene: PanelHome not found in scene!");
                }
            }

            // PanelSliderManager sẽ tự động show panel đầu tiên
        }

        /// <summary>
        /// Initialize Unity Gaming Services (UGS)
        /// </summary>
        private async System.Threading.Tasks.Task InitializeUGS()
        {
            try
            {
                Debug.Log("MenuScene: Initializing Unity Gaming Services...");

                // Check if already initialized
                if (AntKnow.Services.UGSAuthService.IsSignedIn)
                {
                    Debug.Log("MenuScene: UGS already initialized and signed in");
                    return;
                }

                // Auto sign in from Firebase
                bool signedIn = await AntKnow.Services.UGSAuthService.Instance.AutoSignInFromFirebaseAsync();

                if (signedIn)
                {
                    Debug.Log("MenuScene: UGS initialized and signed in successfully");
                }
                else
                {
                    Debug.LogWarning("MenuScene: UGS initialized but failed to sign in (matchmaking will not work)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MenuScene: Failed to initialize UGS: {e.Message}");
            }
        }

        private void SetupUI()
        {
            // Initialize settings panel
            if (settingsPanel != null) settingsPanel.SetActive(false);

            // Initialize panel room (hidden by default)
            if (panelRoom != null) panelRoom.SetActive(false);

            // Initialize top bar components
            if (panelAvatar != null)
            {
                panelAvatar.Initialize(gameDataManager);
            }

            if (panelMoney != null)
            {
                panelMoney.Initialize();
            }

            // Setup panel slider manager
            if (panelSliderManager != null)
            {
                SetupPanelSliderManager();
            }
        }

        private void SetupPanelSliderManager()
        {
            // PanelSliderManager sẽ tự động setup panels và buttons
            // Chúng ta chỉ cần đảm bảo nó có đúng references
            Debug.Log("MenuScene: PanelSliderManager setup completed");
        }

        private void SetupEventListeners()
        {
            // Settings button
            if (buttonSetting != null)
                buttonSetting.onClick.AddListener(OnSettingsClicked);
        }

        private async Task LoadUserDataAndInventory()
        {
            try
            {
                Debug.Log("MenuScene: Loading user data and checking inventory");

                // Load user data from Firebase
                var userData = await firebaseAuthService.GetUserDataAsync(gameDataManager.currentUserId);
                if (userData != null)
                {
                    // Update GameDataManager with data from Firebase
                    gameDataManager.SetUserData(
                        userData.uid,
                        userData.username,
                        userData.email,
                        userData.ingameName,
                        userData.gender,
                        userData.level,
                        userData.xp,
                        userData.currencies.antCoin,
                        userData.currencies.dCoin,
                        userData.stats.matchesPlayed,
                        userData.stats.wins
                    );
                    Debug.Log($"MenuScene: User data loaded from Firebase - Gender: {userData.gender}, IngameName: {userData.ingameName}");
                    Debug.Log($"MenuScene: User data loaded from Firebase - AntCoin: {userData.currencies.antCoin}, DCoin: {userData.currencies.dCoin}");
                    
                    // Cập nhật PanelMoney sau khi load data
                    if (panelMoney != null)
                    {
                        panelMoney.UpdateCurrencyDisplay();
                        Debug.Log("MenuScene: PanelMoney updated with new currency data");
                    }
                }
                else
                {
                    Debug.LogError("MenuScene: Failed to load user data from Firebase");
                }

                // Check if user has inventory
                bool hasInventory = await firebaseAuthService.HasInventoryAsync(gameDataManager.currentUserId);
                
                if (!hasInventory)
                {
                    Debug.Log("MenuScene: Creating initial inventory and loadout");
                    bool success = await firebaseAuthService.CreateInitialInventoryAndLoadoutAsync(gameDataManager.currentUserId);
                    
                    if (success)
                    {
                        gameDataManager.SetInventoryLoaded(true);
                        gameDataManager.SetLoadoutLoaded(true);
                        Debug.Log("MenuScene: Initial inventory and loadout created successfully");
                    }
                    else
                    {
                        Debug.LogError("MenuScene: Failed to create initial inventory and loadout");
                    }
                }
                else
                {
                    Debug.Log("MenuScene: User already has inventory");
                    gameDataManager.SetInventoryLoaded(true);
                    gameDataManager.SetLoadoutLoaded(true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"MenuScene: Error loading user data: {e.Message}");
            }
        }

        public void ShowPanel(int panelIndex)
        {
            if (panelSliderManager != null)
            {
                panelSliderManager.Show(panelIndex);
                Debug.Log($"MenuScene: Switched to panel {panelIndex}");
            }
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
                Debug.Log($"MenuScene: Settings panel toggled - {settingsPanel.activeSelf}");
            }
        }

        /// <summary>
        /// Initialize simple chat system after user data is loaded
        /// </summary>
        private async Task InitializeChatSystem()
        {
            try
            {
                Debug.Log("MenuScene: Initializing simple chat system...");
                
                // Find simple chat manager if not assigned
                if (simpleChatManager == null)
                {
                    simpleChatManager = FindObjectOfType<SimpleChatManager>();
                }
                
                // Simple chat will auto-connect when it starts
                if (simpleChatManager != null)
                {
                    Debug.Log("MenuScene: Simple chat manager found, auto-connect enabled");
                }
                else
                {
                    Debug.LogWarning("MenuScene: SimpleChatManager not found");
                }
                
                // Small delay to ensure user data is ready
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                Debug.LogError($"MenuScene: Error initializing simple chat system: {e.Message}");
            }
        }
        
        /// <summary>
        /// Disconnect from chat when leaving menu
        /// </summary>
        private async void DisconnectFromChat()
        {
            try
            {
                if (simpleChatManager != null)
                {
                    Debug.Log("MenuScene: Disconnecting from simple chat...");
                    simpleChatManager.DisconnectFromChat();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"MenuScene: Error disconnecting from chat: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (buttonSetting != null)
                buttonSetting.onClick.RemoveAllListeners();
            
            // Disconnect from chat
            DisconnectFromChat();
        }
    }
}
