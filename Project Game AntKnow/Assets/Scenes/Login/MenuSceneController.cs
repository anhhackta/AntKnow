using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AntKnow.Auth;

namespace AntKnow.Auth
{
    /// <summary>
    /// Controller cho MenuScene - LEGACY - Sử dụng MenuSceneManager thay thế
    /// </summary>
    [System.Obsolete("Use MenuSceneManager instead")]
    public class MenuSceneController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject ingameNamePanel;
        [SerializeField] private GameObject mainMenuPanel;

        [Header("Ingame Name Input")]
        [SerializeField] private InputField inputIngameName;
        [SerializeField] private Button buttonSetIngameName;
        [SerializeField] private Text textError;

        [Header("Main Menu")]
        [SerializeField] private Text textWelcome;
        [SerializeField] private Button buttonPlay;
        [SerializeField] private Button buttonInventory;
        [SerializeField] private Button buttonLogout;

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        private GameDataManager gameDataManager;
        private bool isProcessing = false;

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

            Debug.Log($"MenuScene: Initializing for user {gameDataManager.currentUsername}");

            // Find FirebaseAuthService
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Setup UI event listeners
            SetupEventListeners();

            // User should already have ingame name from SelectCharacterScene
            if (gameDataManager.NeedsIngameNameSetup())
            {
                Debug.LogError("MenuScene: User missing ingame name, redirecting to SelectCharacterScene");
                SceneManager.LoadScene("SelectCharacterScene");
                return;
            }

            ShowMainMenu();
            await LoadUserDataAndInventory();
        }

        private void SetupEventListeners()
        {
            if (buttonLogout != null)
            {
                buttonLogout.onClick.AddListener(OnLogoutClicked);
            }

            if (buttonPlay != null)
            {
                buttonPlay.onClick.AddListener(OnPlayClicked);
            }

            if (buttonInventory != null)
            {
                buttonInventory.onClick.AddListener(OnInventoryClicked);
            }
        }


        private void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            // Update welcome text
            if (textWelcome != null)
            {
                textWelcome.text = $"Chào mừng, {gameDataManager.GetDisplayName()}!";
            }

            Debug.Log("MenuScene: Showing main menu");
        }

        private async Task LoadUserDataAndInventory()
        {
            try
            {
                Debug.Log("MenuScene: Loading user data and checking inventory");

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


        private void OnLogoutClicked()
        {
            Debug.Log("MenuScene: Logout clicked");
            
            // Clear user data
            gameDataManager.ClearUserData();
            
            // Sign out from Firebase
            if (firebaseAuthService != null)
            {
                firebaseAuthService.SignOutAsync();
            }
            
            // Load LoginScene
            SceneManager.LoadScene("LoginScene");
        }

        private void OnPlayClicked()
        {
            Debug.Log("MenuScene: Play clicked - Loading GameScene");
            // TODO: Load GameScene
            SceneManager.LoadScene("GameScene");
        }

        private void OnInventoryClicked()
        {
            Debug.Log("MenuScene: Inventory clicked - Loading InventoryScene");
            // TODO: Load InventoryScene
            // SceneManager.LoadScene("InventoryScene");
        }


        private void OnDestroy()
        {
            // Clean up event listeners
            if (buttonLogout != null)
                buttonLogout.onClick.RemoveListener(OnLogoutClicked);
            
            if (buttonPlay != null)
                buttonPlay.onClick.RemoveListener(OnPlayClicked);
            
            if (buttonInventory != null)
                buttonInventory.onClick.RemoveListener(OnInventoryClicked);
        }
    }
}
