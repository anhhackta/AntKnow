using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AntKnow.Auth;
using Firebase.Firestore;

namespace AntKnow.Auth
{
    /// <summary>
    /// Controller cho SelectCharacterScene - chọn giới tính và nhập ingame name
    /// </summary>
    public class SelectCharacterController : MonoBehaviour
    {
        [Header("Character Models (Click to select)")]
        [SerializeField] private GameObject maleCharacterModel;
        [SerializeField] private GameObject femaleCharacterModel;
        [SerializeField] private float modelRotationSpeed = 30f;

        [Header("Selection Spotlights")]
        [SerializeField] private Light maleSpotlight;
        [SerializeField] private Light femaleSpotlight;
        [SerializeField] private Color selectedSpotlightColor = Color.yellow;
        [SerializeField] private Color normalSpotlightColor = Color.white;
        [SerializeField] private float selectedSpotlightIntensity = 3f;
        [SerializeField] private float normalSpotlightIntensity = 1f;

        [Header("UI Elements")]
        [SerializeField] private InputField inputIngameName;
        [SerializeField] private Button buttonConfirmCharacter;
        [SerializeField] private Text textError;

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        private GameDataManager gameDataManager;
        private string selectedGender = "";
        private bool isProcessing = false;

        private void Awake()
        {
            // CRITICAL: Check profile BEFORE scene is visible
            gameDataManager = GameDataManager.Instance;

            // Check if user is logged in
            if (!gameDataManager.isUserLoggedIn)
            {
                Debug.LogError("SelectCharacterScene: No user logged in, redirecting to LoginScene");
                SceneManager.LoadScene("LoginScene");
                return;
            }

            // Check if user already has BOTH ingame name AND gender
            bool hasIngameName = !string.IsNullOrEmpty(gameDataManager.currentIngameName);
            bool hasGender = !string.IsNullOrEmpty(gameDataManager.currentGender);

            if (hasIngameName && hasGender)
            {
                // User has complete profile, skip this scene immediately
                Debug.Log($"SelectCharacterScene: User already has complete profile (Name: {gameDataManager.currentIngameName}, Gender: {gameDataManager.currentGender}), skipping to MenuScene");
                SceneManager.LoadScene("MenuScene");
                return;
            }

            // User needs to select character, continue to Start()
            Debug.Log($"SelectCharacterScene: User needs to complete profile (Name: {gameDataManager.currentIngameName}, Gender: {gameDataManager.currentGender})");
        }

        private void Start()
        {
            InitializeSelectCharacterScene();
        }

        private async void InitializeSelectCharacterScene()
        {
            Debug.Log($"SelectCharacterScene: Initializing UI for user {gameDataManager.currentUsername}");

            // Find FirebaseAuthService
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Setup UI event listeners
            SetupEventListeners();

            // Initialize UI
            InitializeUI();

            // Pre-fill ingame name if exists
            bool hasIngameName = !string.IsNullOrEmpty(gameDataManager.currentIngameName);
            if (hasIngameName && inputIngameName != null)
            {
                inputIngameName.text = gameDataManager.currentIngameName;
                Debug.Log($"SelectCharacterScene: Pre-filled ingame name: {gameDataManager.currentIngameName}");
            }

            // Pre-select gender if exists
            bool hasGender = !string.IsNullOrEmpty(gameDataManager.currentGender);
            if (hasGender)
            {
                selectedGender = gameDataManager.currentGender;
                UpdateSpotlightSelection();
                Debug.Log($"SelectCharacterScene: Pre-selected gender: {gameDataManager.currentGender}");
            }
        }

        private void SetupEventListeners()
        {
            // Add click listeners to 3D models
            if (maleCharacterModel != null)
            {
                AddModelClickListener(maleCharacterModel, "male");
            }

            if (femaleCharacterModel != null)
            {
                AddModelClickListener(femaleCharacterModel, "female");
            }

            if (buttonConfirmCharacter != null)
            {
                buttonConfirmCharacter.onClick.AddListener(OnConfirmCharacterClicked);
            }
        }

        private void AddModelClickListener(GameObject model, string gender)
        {
            // Add CapsuleCollider if not exists (better for character models)
            if (model.GetComponent<Collider>() == null)
            {
                var capsuleCollider = model.AddComponent<CapsuleCollider>();
                capsuleCollider.height = 2f;
                capsuleCollider.radius = 0.5f;
                capsuleCollider.center = new Vector3(0, 1f, 0);
            }

            // Add click handler
            var clickHandler = model.AddComponent<ModelClickHandler>();
            clickHandler.Initialize(this, gender);
        }

        private void InitializeUI()
        {
            // Setup 3D models and cubes
            Setup3DModels();
            
            // Clear error text
            ClearError();
        }

        private void Setup3DModels()
        {
            // Set initial selection to male
            selectedGender = "male";
            UpdateSpotlightSelection();
        }

        public void SelectCharacter(string gender)
        {
            selectedGender = gender;
            Debug.Log($"SelectCharacterScene: Selected gender: {gender}");

            // Update spotlight selection visual
            UpdateSpotlightSelection();
        }

        private void UpdateSpotlightSelection()
        {
            // Update male spotlight
            if (maleSpotlight != null)
            {
                UpdateSpotlightVisual(maleSpotlight, selectedGender == "male");
            }

            // Update female spotlight
            if (femaleSpotlight != null)
            {
                UpdateSpotlightVisual(femaleSpotlight, selectedGender == "female");
            }
        }

        private void UpdateSpotlightVisual(Light spotlight, bool isSelected)
        {
            if (spotlight != null)
            {
                if (isSelected)
                {
                    spotlight.color = selectedSpotlightColor;
                    spotlight.intensity = selectedSpotlightIntensity;
                }
                else
                {
                    spotlight.color = normalSpotlightColor;
                    spotlight.intensity = normalSpotlightIntensity;
                }
            }
        }

        private async void OnConfirmCharacterClicked()
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

                // Update ingame name and gender in Firestore
                bool success = await UpdateCharacterData(ingameName, selectedGender);
                
                if (success)
                {
                    // Update GameDataManager
                    gameDataManager.UpdateIngameName(ingameName);
                    gameDataManager.UpdateGender(selectedGender);
                    
                    Debug.Log($"SelectCharacterScene: Character data saved successfully - {ingameName} ({selectedGender})");
                    
                    // Go to MenuScene
                    SceneManager.LoadScene("MenuScene");
                }
                else
                {
                    ShowError("Không thể lưu thông tin nhân vật, vui lòng thử lại");
                }
            }
            catch (Exception e)
            {
                ShowError($"Lỗi lưu thông tin: {e.Message}");
                Debug.LogError($"SelectCharacterScene: Error saving character data: {e.Message}");
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private async Task<bool> UpdateCharacterData(string ingameName, string gender)
        {
            try
            {
                if (!firebaseAuthService.IsFirebaseReady())
                {
                    Debug.LogError("SelectCharacterScene: Firebase not ready");
                    return false;
                }

                // Get Firestore instance
                var firestore = FirebaseFirestore.DefaultInstance;

                // Update user document with ingame name and gender
                var userRef = firestore.Collection("users").Document(gameDataManager.currentUserId);
                await userRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "ingameName", ingameName },
                    { "gender", gender },
                    { "lastLoginAt", Timestamp.GetCurrentTimestamp() }
                });

                // Create ingame name mapping
                await firestore.Collection("ingame_names").Document(ingameName.ToLower()).SetAsync(new Dictionary<string, object>
                {
                    { "uid", gameDataManager.currentUserId }
                });

                Debug.Log($"SelectCharacterScene: Character data updated in Firestore");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SelectCharacterScene: Error updating character data: {e.Message}");
                return false;
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
            
            if (buttonConfirmCharacter != null)
                buttonConfirmCharacter.interactable = !processing;
        }

        private void Update()
        {
            // Rotate 3D models slowly
            if (maleCharacterModel != null)
            {
                maleCharacterModel.transform.Rotate(0, modelRotationSpeed * Time.deltaTime, 0);
            }

            if (femaleCharacterModel != null)
            {
                femaleCharacterModel.transform.Rotate(0, modelRotationSpeed * Time.deltaTime, 0);
            }
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (buttonConfirmCharacter != null)
                buttonConfirmCharacter.onClick.RemoveAllListeners();
        }
    }
}
