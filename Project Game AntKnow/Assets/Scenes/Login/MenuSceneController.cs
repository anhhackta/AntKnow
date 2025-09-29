using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AntKnow.Auth;

namespace AntKnow.Auth
{
    /// <summary>
    /// Controller cho MenuScene - xử lý ingame name và tạo inventory/loadout
    /// </summary>
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

            // Check if user needs ingame name setup
            if (gameDataManager.NeedsIngameNameSetup())
            {
                ShowIngameNameSetup();
            }
            else
            {
                ShowMainMenu();
                await LoadUserDataAndInventory();
            }
        }

        private void SetupEventListeners()
        {
            if (buttonSetIngameName != null)
            {
                buttonSetIngameName.onClick.AddListener(OnSetIngameNameClicked);
            }

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

        private void ShowIngameNameSetup()
        {
            if (ingameNamePanel != null)
                ingameNamePanel.SetActive(true);
            
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            Debug.Log("MenuScene: Showing ingame name setup");
        }

        private void ShowMainMenu()
        {
            if (ingameNamePanel != null)
                ingameNamePanel.SetActive(false);
            
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

        private async void OnSetIngameNameClicked()
        {
            if (isProcessing) return;

            string ingameName = inputIngameName?.text?.Trim();
            
            // Validate ingame name
            if (!ValidateIngameName(ingameName))
                return;

            SetProcessing(true);
            ClearError();

            try
            {
                // Check if ingame name is already taken
                bool isTaken = await firebaseAuthService.IsIngameNameTakenAsync(ingameName);
                
                if (isTaken)
                {
                    ShowError("Tên game này đã được sử dụng");
                    SetProcessing(false);
                    return;
                }

                // Update ingame name
                bool success = await firebaseAuthService.UpdateIngameNameAsync(gameDataManager.currentUserId, ingameName);
                
                if (success)
                {
                    // Update GameDataManager
                    gameDataManager.UpdateIngameName(ingameName);
                    
                    Debug.Log($"MenuScene: Ingame name set successfully: {ingameName}");
                    
                    // Hide ingame name setup and show main menu
                    ShowMainMenu();
                    await LoadUserDataAndInventory();
                }
                else
                {
                    ShowError("Không thể đặt tên game, vui lòng thử lại");
                }
            }
            catch (Exception e)
            {
                ShowError($"Lỗi đặt tên game: {e.Message}");
                Debug.LogError($"MenuScene: Error setting ingame name: {e.Message}");
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private bool ValidateIngameName(string ingameName)
        {
            if (string.IsNullOrEmpty(ingameName))
            {
                ShowError("Vui lòng nhập tên game");
                return false;
            }

            if (ingameName.Length > 20)
            {
                ShowError("Tên game không được quá 20 ký tự");
                return false;
            }

            if (ingameName.Length < 2)
            {
                ShowError("Tên game phải có ít nhất 2 ký tự");
                return false;
            }

            // Check for special characters (chỉ cho phép chữ cái, số, và khoảng trắng)
            foreach (char c in ingameName)
            {
                if (!char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))
                {
                    ShowError("Tên game chỉ được chứa chữ cái, số và khoảng trắng");
                    return false;
                }
            }

            return true;
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

        private void ShowError(string message)
        {
            if (textError != null)
            {
                textError.text = message;
                textError.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (textError != null)
            {
                textError.text = "";
                textError.gameObject.SetActive(false);
            }
        }

        private void SetProcessing(bool processing)
        {
            isProcessing = processing;
            
            if (buttonSetIngameName != null)
                buttonSetIngameName.interactable = !processing;
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (buttonSetIngameName != null)
                buttonSetIngameName.onClick.RemoveListener(OnSetIngameNameClicked);
            
            if (buttonLogout != null)
                buttonLogout.onClick.RemoveListener(OnLogoutClicked);
            
            if (buttonPlay != null)
                buttonPlay.onClick.RemoveListener(OnPlayClicked);
            
            if (buttonInventory != null)
                buttonInventory.onClick.RemoveListener(OnInventoryClicked);
        }
    }
}
