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

        [Header("Selection Cubes")]
        [SerializeField] private GameObject maleSelectionCube;
        [SerializeField] private GameObject femaleSelectionCube;
        [SerializeField] private Material selectedCubeMaterial;
        [SerializeField] private Material normalCubeMaterial;

        [Header("UI Elements")]
        [SerializeField] private InputField inputIngameName;
        [SerializeField] private Button buttonConfirmCharacter;
        [SerializeField] private Text textError;

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        private GameDataManager gameDataManager;
        private string selectedGender = "";
        private bool isProcessing = false;

        private void Start()
        {
            InitializeSelectCharacterScene();
        }

        private async void InitializeSelectCharacterScene()
        {
            // Get GameDataManager instance
            gameDataManager = GameDataManager.Instance;

            // Check if user is logged in
            if (!gameDataManager.isUserLoggedIn)
            {
                Debug.LogError("SelectCharacterScene: No user logged in, redirecting to LoginScene");
                SceneManager.LoadScene("LoginScene");
                return;
            }

            Debug.Log($"SelectCharacterScene: Initializing for user {gameDataManager.currentUsername}");

            // Find FirebaseAuthService
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Setup UI event listeners
            SetupEventListeners();

            // Initialize UI
            InitializeUI();

            // Check if user already has ingame name
            if (!string.IsNullOrEmpty(gameDataManager.currentIngameName))
            {
                Debug.Log("SelectCharacterScene: User already has ingame name, going to MenuScene");
                SceneManager.LoadScene("MenuScene");
                return;
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
            // Add collider if not exists
            if (model.GetComponent<Collider>() == null)
            {
                model.AddComponent<BoxCollider>();
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
            UpdateCubeSelection();
        }

        public void SelectCharacter(string gender)
        {
            selectedGender = gender;
            Debug.Log($"SelectCharacterScene: Selected gender: {gender}");

            // Update cube selection visual
            UpdateCubeSelection();
        }

        private void UpdateCubeSelection()
        {
            // Update male cube
            if (maleSelectionCube != null)
            {
                UpdateCubeVisual(maleSelectionCube, selectedGender == "male");
            }

            // Update female cube
            if (femaleSelectionCube != null)
            {
                UpdateCubeVisual(femaleSelectionCube, selectedGender == "female");
            }
        }

        private void UpdateCubeVisual(GameObject cube, bool isSelected)
        {
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (isSelected && selectedCubeMaterial != null)
                {
                    renderer.material = selectedCubeMaterial;
                }
                else if (normalCubeMaterial != null)
                {
                    renderer.material = normalCubeMaterial;
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
