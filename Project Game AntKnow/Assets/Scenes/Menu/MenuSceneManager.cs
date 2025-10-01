using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AntKnow.Auth;
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

            // Check if user has ingame name
            if (gameDataManager.NeedsIngameNameSetup())
            {
                Debug.LogError("MenuScene: User missing ingame name, redirecting to SelectCharacterScene");
                SceneManager.LoadScene("SelectCharacterScene");
                return;
            }

            Debug.Log($"MenuScene: Initializing for user {gameDataManager.currentUsername}");

            // Find FirebaseAuthService
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Setup UI
            SetupUI();
            SetupEventListeners();

            // Load user data and inventory
            await LoadUserDataAndInventory();

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
                Debug.LogError("MenuScene: PanelHome is null! Please assign it in the inspector.");
            }

            // PanelSliderManager sẽ tự động show panel đầu tiên
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
                        userData.currencies.dCoin
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

        private void OnDestroy()
        {
            // Clean up event listeners
            if (buttonSetting != null)
                buttonSetting.onClick.RemoveAllListeners();
        }
    }
}
