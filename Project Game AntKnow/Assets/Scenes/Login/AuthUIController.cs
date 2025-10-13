using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using AntKnow.Auth;
using Firebase.Auth;

namespace AntKnow.Auth
{
    /// <summary>
    /// REFACTORED: Auth UI Controller with English text and improved flow
    /// Tab = Next field, Enter = Login/Register
    /// </summary>
    public class AuthUIController : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject panelLog;
        [SerializeField] private GameObject panelLogin;
        [SerializeField] private GameObject panelRegister;
        [SerializeField] private GameObject panelNotification;
        [SerializeField] private GameObject logButton;
        [SerializeField] private GameObject buttonStart;

        [Header("Tab Buttons (Outside Panels)")]
        [SerializeField] private Button buttonLoginTab;      // Tab button to switch to Login
        [SerializeField] private Button buttonRegisterTab;   // Tab button to switch to Register

        [Header("Login Panel")]
        [SerializeField] private TMP_InputField inputUsernameOrEmail;
        [SerializeField] private TMP_InputField inputPassword;
        [SerializeField] private Toggle toggleRememberMe;
        [SerializeField] private Button buttonLogin;
        [SerializeField] private Button buttonLoginWithGoogle;
        [SerializeField] private TMP_Text textInlineError;
        [SerializeField] private Button buttonSwitchToRegister; // "Create account" button INSIDE panel

        [Header("Register Panel")]
        [SerializeField] private TMP_InputField inputUsername;
        [SerializeField] private TMP_InputField inputEmail;
        [SerializeField] private TMP_InputField inputPassword1;
        [SerializeField] private TMP_InputField inputPassword2;
        [SerializeField] private TMP_Text textCheckUsername;
        [SerializeField] private TMP_Text textCheckEmail;
        [SerializeField] private TMP_Text textCheckPw1;
        [SerializeField] private TMP_Text textCheckPw2;
        [SerializeField] private Button buttonCreateAccount;
        [SerializeField] private Button buttonBackToLogin; // "Back to Login" button INSIDE panel

        [Header("Controls")]
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonLogButton;
        [SerializeField] private Button buttonStartButton;
        [SerializeField] private Button buttonExit; // NEW: Exit button

        [Header("LogButton Sprites")]
        [SerializeField] private Sprite spriteLogin;
        [SerializeField] private Sprite spriteLogout;

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        [Header("Avatar Panel")]
        [SerializeField] private AvatarPanel avatarPanel;

        [Header("Notification")]
        [SerializeField] private TMP_Text textNotification;
        [SerializeField] private float notificationDuration = 2f; // 2 seconds

        private bool isProcessing = false;
        private Coroutine notificationCoroutine;
        private UserData currentUserData;

        // Debounce coroutines
        private Coroutine usernameCheckCoroutine;
        private Coroutine emailCheckCoroutine;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            LoadRememberedCredentials();
            CheckAuthState();
        }

        private void InitializeUI()
        {
            // Set initial state
            panelLog.SetActive(true);
            panelLogin.SetActive(true);
            panelRegister.SetActive(false);
            panelNotification.SetActive(false);
            logButton.SetActive(false); // Hidden initially
            buttonStart.SetActive(false);

            // Hide validation texts initially
            textCheckUsername.gameObject.SetActive(false);
            textCheckEmail.gameObject.SetActive(false);
            textCheckPw1.gameObject.SetActive(false);
            textCheckPw2.gameObject.SetActive(false);
            textInlineError.gameObject.SetActive(false);

            // Set password input content type
            inputPassword.contentType = TMP_InputField.ContentType.Password;
            inputPassword1.contentType = TMP_InputField.ContentType.Password;
            inputPassword2.contentType = TMP_InputField.ContentType.Password;

            // Set placeholders (ENGLISH)
            inputUsernameOrEmail.placeholder.GetComponent<TMP_Text>().text = "Username or Email";
            inputUsername.placeholder.GetComponent<TMP_Text>().text = "Username";
            inputEmail.placeholder.GetComponent<TMP_Text>().text = "Email";
            inputPassword.placeholder.GetComponent<TMP_Text>().text = "Password";
            inputPassword1.placeholder.GetComponent<TMP_Text>().text = "Password (≥8 characters)";
            inputPassword2.placeholder.GetComponent<TMP_Text>().text = "Confirm Password";
        }

        private void Update()
        {
            // Handle Tab key for navigation
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HandleTabNavigation();
            }

            // Handle Enter key for login/register
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                HandleEnterKey();
            }
        }

        private void SetupEventListeners()
        {
            // Tab buttons (outside panels)
            if (buttonLoginTab != null)
                buttonLoginTab.onClick.AddListener(SwitchToLoginPanel);
            if (buttonRegisterTab != null)
                buttonRegisterTab.onClick.AddListener(SwitchToRegisterPanel);

            // Login panel
            buttonLogin.onClick.AddListener(OnLoginClicked);
            buttonLoginWithGoogle.onClick.AddListener(OnGoogleLoginClicked);
            if (buttonSwitchToRegister != null)
                buttonSwitchToRegister.onClick.AddListener(SwitchToRegisterPanel);

            // Register panel
            buttonCreateAccount.onClick.AddListener(OnRegisterClicked);
            if (buttonBackToLogin != null)
                buttonBackToLogin.onClick.AddListener(SwitchToLoginPanel);

            // Real-time validation for register panel
            inputUsername.onValueChanged.AddListener(OnUsernameChanged);
            inputEmail.onValueChanged.AddListener(OnEmailChanged);
            inputPassword1.onValueChanged.AddListener(OnPassword1Changed);
            inputPassword2.onValueChanged.AddListener(OnPassword2Changed);

            // Other controls
            buttonClose.onClick.AddListener(OnClosePanelClicked);
            buttonLogButton.onClick.AddListener(OnLogButtonClicked);
            buttonStartButton.onClick.AddListener(OnStartButtonClicked);
            buttonExit.onClick.AddListener(OnExitClicked);

            // Firebase auth events
            if (firebaseAuthService != null)
            {
                firebaseAuthService.OnUserSignedIn += OnUserSignedIn;
                firebaseAuthService.OnUserSignedOut += OnUserSignedOut;
                firebaseAuthService.OnAuthError += OnAuthError;
            }
        }

        private void HandleTabNavigation()
        {
            if (panelLogin.activeSelf)
            {
                // Login panel: UsernameOrEmail → Password → RememberMe → Login button
                if (inputUsernameOrEmail.isFocused)
                {
                    inputPassword.Select();
                }
                else if (inputPassword.isFocused)
                {
                    toggleRememberMe.Select();
                }
            }
            else if (panelRegister.activeSelf)
            {
                // Register panel: Username → Email → Password1 → Password2 → Create button
                if (inputUsername.isFocused)
                {
                    inputEmail.Select();
                }
                else if (inputEmail.isFocused)
                {
                    inputPassword1.Select();
                }
                else if (inputPassword1.isFocused)
                {
                    inputPassword2.Select();
                }
            }
        }

        private void HandleEnterKey()
        {
            if (panelLogin.activeSelf && !isProcessing)
            {
                // Press Enter in Login panel → Login
                OnLoginClicked();
            }
            else if (panelRegister.activeSelf && !isProcessing)
            {
                // Press Enter in Register panel → Register
                OnRegisterClicked();
            }
        }

        private void SwitchToLoginPanel()
        {
            panelLogin.SetActive(true);
            panelRegister.SetActive(false);
            textInlineError.gameObject.SetActive(false);
        }

        private void SwitchToRegisterPanel()
        {
            panelLogin.SetActive(false);
            panelRegister.SetActive(true);
            ValidateRegisterForm();
        }

        private async void OnLoginClicked()
        {
            if (isProcessing) return;

            string userOrEmail = inputUsernameOrEmail.text.Trim();
            string password = inputPassword.text;

            if (string.IsNullOrEmpty(userOrEmail) || string.IsNullOrEmpty(password))
            {
                ShowInlineError("Please fill in all fields");
                return;
            }

            SetProcessing(true);
            textInlineError.gameObject.SetActive(false);

            try
            {
                var result = await firebaseAuthService.SignInWithEmailOrUsernameAsync(userOrEmail, password);
                
                if (result.IsSuccess)
                {
                    ShowNotification("Login successful!", false);
                    
                    // Save credentials if remember me is checked
                    if (toggleRememberMe.isOn)
                    {
                        SaveCredentials(userOrEmail, password);
                    }
                    else
                    {
                        ClearSavedCredentials();
                    }

                    // Load user data
                    await LoadUserDataAndProceed(result.User.UserId);
                }
                else
                {
                    ShowInlineError(result.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                ShowInlineError($"Login error: {e.Message}");
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private void OnGoogleLoginClicked()
        {
            ShowNotification("Google Login is under development, please use Email/Password", true);
        }

        private async void OnRegisterClicked()
        {
            if (isProcessing) return;

            string username = inputUsername.text.Trim();
            string email = inputEmail.text.Trim();
            string password1 = inputPassword1.text;
            string password2 = inputPassword2.text;

            // Validate form
            if (!ValidateRegisterForm())
            {
                ShowNotification("Please check your information", true);
                return;
            }

            SetProcessing(true);

            try
            {
                var result = await firebaseAuthService.RegisterAsync(username, email, password1);
                
                if (result.IsSuccess)
                {
                    ShowNotification("Account created successfully!", false);
                    
                    // Load user data
                    await LoadUserDataAndProceed(result.User.UserId);
                }
                else
                {
                    ShowNotification(result.ErrorMessage, true);
                }
            }
            catch (Exception e)
            {
                ShowNotification($"Registration error: {e.Message}", true);
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private async System.Threading.Tasks.Task LoadUserDataAndProceed(string uid)
        {
            try
            {
                // Load user data
                currentUserData = await firebaseAuthService.GetUserDataAsync(uid);

                if (currentUserData != null)
                {
                    // Set user data in GameDataManager
                    GameDataManager.Instance.SetUserData(
                        currentUserData.uid,
                        currentUserData.username,
                        currentUserData.email,
                        currentUserData.ingameName,
                        currentUserData.gender,
                        currentUserData.level,
                        currentUserData.xp,
                        currentUserData.currencies.antCoin,
                        currentUserData.currencies.dCoin,
                        currentUserData.stats.matchesPlayed,
                        currentUserData.stats.wins
                    );

                    Debug.Log($"AuthUIController: User data loaded - {currentUserData.username}");

                    // Wait for notification to show (2s)
                    await System.Threading.Tasks.Task.Delay(2000);

                    // Hide notification
                    panelNotification.SetActive(false);

                    // Hide panelLog (Login/Register panel)
                    panelLog.SetActive(false);

                    // Show AvatarPanel with user info
                    if (avatarPanel != null)
                    {
                        avatarPanel.ShowPanel(currentUserData);
                        Debug.Log("AvatarPanel shown with user info");
                    }

                    // Show logButton with Logout sprite
                    logButton.SetActive(true);
                    UpdateLogButtonSprite();

                    // Show Start button
                    buttonStart.SetActive(true);

                    Debug.Log("Login successful - Panel hidden, AvatarPanel shown, Start button shown");
                }
                else
                {
                    ShowNotification("Failed to load user data", true);
                }
            }
            catch (Exception e)
            {
                ShowNotification($"Error loading user data: {e.Message}", true);
            }
        }

        private void OnClosePanelClicked()
        {
            panelLog.SetActive(false);
            logButton.SetActive(true);
            UpdateLogButtonSprite();
        }

        private void OnLogButtonClicked()
        {
            if (firebaseAuthService.Auth != null && firebaseAuthService.Auth.CurrentUser != null)
            {
                // User is logged in, sign out
                firebaseAuthService.SignOutAsync();
                GameDataManager.Instance.ClearUserData();
                ShowNotification("Logged out successfully", false);
                
                // Redirect to LoginScene
                SceneManager.LoadScene("LoginScene");
            }
            else
            {
                // User is not logged in, show login panel
                panelLog.SetActive(true);
                logButton.SetActive(false);
                SwitchToLoginPanel();
            }
        }

        private void OnStartButtonClicked()
        {
            if (currentUserData != null)
            {
                // Play start sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayStart();
                }

                SceneManager.LoadScene("LoadingScene");
            }
        }

        private void OnExitClicked()
        {
            Debug.Log("Exit button clicked - Quitting application");
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        // ===== VALIDATION METHODS =====

        private void OnUsernameChanged(string value)
        {
            if (usernameCheckCoroutine != null)
            {
                StopCoroutine(usernameCheckCoroutine);
            }

            if (string.IsNullOrEmpty(value))
            {
                textCheckUsername.gameObject.SetActive(false);
                return;
            }

            if (firebaseAuthService == null || !firebaseAuthService.IsFirebaseReady())
            {
                textCheckUsername.gameObject.SetActive(true);
                textCheckUsername.text = "Initializing Firebase...";
                textCheckUsername.color = Color.yellow;
                return;
            }

            usernameCheckCoroutine = StartCoroutine(CheckUsernameDebounced(value));
        }

        private IEnumerator CheckUsernameDebounced(string username)
        {
            yield return new WaitForSeconds(0.5f);

            textCheckUsername.gameObject.SetActive(true);
            textCheckUsername.text = "Checking...";
            textCheckUsername.color = Color.yellow;

            var checkTask = firebaseAuthService.IsUsernameTakenAsync(username);
            yield return new WaitUntil(() => checkTask.IsCompleted);

            try
            {
                bool isTaken = checkTask.Result;

                if (isTaken)
                {
                    textCheckUsername.text = "Username already taken";
                    textCheckUsername.color = Color.red;
                }
                else
                {
                    textCheckUsername.text = "Username available";
                    textCheckUsername.color = Color.green;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("permissions") || e.Message.Contains("Permission"))
                {
                    textCheckUsername.text = "Cannot check username (Firestore rules need update)";
                    textCheckUsername.color = Color.yellow;
                }
                else
                {
                    textCheckUsername.text = "Error checking username";
                    textCheckUsername.color = Color.red;
                }
                Debug.LogError($"Username check error: {e.Message}");
            }

            ValidateRegisterForm();
        }

        private void OnEmailChanged(string value)
        {
            if (emailCheckCoroutine != null)
            {
                StopCoroutine(emailCheckCoroutine);
            }

            if (string.IsNullOrEmpty(value) || !value.Contains("@"))
            {
                textCheckEmail.gameObject.SetActive(false);
                return;
            }

            if (firebaseAuthService == null || !firebaseAuthService.IsFirebaseReady())
            {
                textCheckEmail.gameObject.SetActive(true);
                textCheckEmail.text = "Initializing Firebase...";
                textCheckEmail.color = Color.yellow;
                return;
            }

            emailCheckCoroutine = StartCoroutine(CheckEmailDebounced(value));
        }

        private IEnumerator CheckEmailDebounced(string email)
        {
            yield return new WaitForSeconds(0.5f);

            textCheckEmail.gameObject.SetActive(true);
            textCheckEmail.text = "Checking...";
            textCheckEmail.color = Color.yellow;

            var checkTask = firebaseAuthService.IsEmailTakenAsync(email);
            yield return new WaitUntil(() => checkTask.IsCompleted);

            try
            {
                bool isTaken = checkTask.Result;

                if (isTaken)
                {
                    textCheckEmail.text = "Email already registered";
                    textCheckEmail.color = Color.red;
                }
                else
                {
                    textCheckEmail.text = "Email available";
                    textCheckEmail.color = Color.green;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("permissions") || e.Message.Contains("Permission"))
                {
                    textCheckEmail.text = "Cannot check email (Firestore rules need update)";
                    textCheckEmail.color = Color.yellow;
                }
                else
                {
                    textCheckEmail.text = "Error checking email";
                    textCheckEmail.color = Color.red;
                }
                Debug.LogError($"Email check error: {e.Message}");
            }

            ValidateRegisterForm();
        }

        private void OnPassword1Changed(string value)
        {
            textCheckPw1.gameObject.SetActive(true);

            if (string.IsNullOrEmpty(value))
            {
                textCheckPw1.text = "Password required";
                textCheckPw1.color = Color.red;
            }
            else if (value.Length < 8)
            {
                textCheckPw1.text = "Password must be at least 8 characters";
                textCheckPw1.color = Color.red;
            }
            else
            {
                textCheckPw1.text = "Password valid";
                textCheckPw1.color = Color.green;
            }

            ValidateRegisterForm();
            OnPassword2Changed(inputPassword2.text);
        }

        private void OnPassword2Changed(string value)
        {
            textCheckPw2.gameObject.SetActive(true);

            if (string.IsNullOrEmpty(value))
            {
                textCheckPw2.text = "Confirm password required";
                textCheckPw2.color = Color.red;
            }
            else if (value != inputPassword1.text)
            {
                textCheckPw2.text = "Passwords do not match";
                textCheckPw2.color = Color.red;
            }
            else
            {
                textCheckPw2.text = "Passwords match";
                textCheckPw2.color = Color.green;
            }

            ValidateRegisterForm();
        }

        private bool ValidateRegisterForm()
        {
            bool isValid = true;

            // Check username
            if (string.IsNullOrEmpty(inputUsername.text) ||
                textCheckUsername.color != Color.green)
            {
                isValid = false;
            }

            // Check email
            if (string.IsNullOrEmpty(inputEmail.text) ||
                textCheckEmail.color != Color.green)
            {
                isValid = false;
            }

            // Check password1
            if (string.IsNullOrEmpty(inputPassword1.text) ||
                textCheckPw1.color != Color.green)
            {
                isValid = false;
            }

            // Check password2
            if (string.IsNullOrEmpty(inputPassword2.text) ||
                textCheckPw2.color != Color.green)
            {
                isValid = false;
            }

            buttonCreateAccount.interactable = isValid && !isProcessing;
            return isValid;
        }

        // ===== FIREBASE AUTH EVENTS =====

        private void OnUserSignedIn(FirebaseUser user)
        {
            Debug.Log($"User signed in: {user.Email}");
            UpdateLogButtonSprite();
        }

        private void OnUserSignedOut()
        {
            Debug.Log("User signed out");
            buttonStart.SetActive(false);
            currentUserData = null;

            if (avatarPanel != null)
            {
                avatarPanel.HidePanel();
            }

            UpdateLogButtonSprite();
        }

        private void OnAuthError(string error)
        {
            ShowNotification(error, true);
        }

        // ===== UI HELPER METHODS =====

        private void UpdateLogButtonSprite()
        {
            var image = buttonLogButton.GetComponent<Image>();
            if (firebaseAuthService.Auth != null && firebaseAuthService.Auth.CurrentUser != null)
            {
                image.sprite = spriteLogout;
            }
            else
            {
                image.sprite = spriteLogin;
            }
        }

        private void ShowInlineError(string message)
        {
            textInlineError.text = message;
            textInlineError.gameObject.SetActive(true);
        }

        private void ShowNotification(string message, bool isError = false)
        {
            textNotification.text = message;
            textNotification.color = isError ? Color.red : Color.green;
            panelNotification.SetActive(true);

            // Play notification sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayNotification();
            }

            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            notificationCoroutine = StartCoroutine(HideNotificationAfterDelay(notificationDuration));
        }

        private IEnumerator HideNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            panelNotification.SetActive(false);
        }

        private void SetProcessing(bool processing)
        {
            isProcessing = processing;
            buttonLogin.interactable = !processing;
            buttonLoginWithGoogle.interactable = !processing;
            buttonCreateAccount.interactable = !processing && ValidateRegisterForm();
        }

        // ===== REMEMBER ME FUNCTIONALITY =====

        private void SaveCredentials(string userOrEmail, string password)
        {
            try
            {
                string encodedUser = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userOrEmail));
                string encodedPass = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

                PlayerPrefs.SetString("remember_me", "true");
                PlayerPrefs.SetString("saved_user", encodedUser);
                PlayerPrefs.SetString("saved_pass", encodedPass);
                PlayerPrefs.Save();

                Debug.Log("Credentials saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving credentials: {e.Message}");
            }
        }

        private void LoadRememberedCredentials()
        {
            try
            {
                string rememberMe = PlayerPrefs.GetString("remember_me", "false");

                if (rememberMe == "true")
                {
                    string encodedUser = PlayerPrefs.GetString("saved_user", "");
                    string encodedPass = PlayerPrefs.GetString("saved_pass", "");

                    if (!string.IsNullOrEmpty(encodedUser) && !string.IsNullOrEmpty(encodedPass))
                    {
                        string userOrEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedUser));
                        string password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedPass));

                        inputUsernameOrEmail.text = userOrEmail;
                        inputPassword.text = password;
                        toggleRememberMe.isOn = true;

                        Debug.Log("Credentials loaded successfully");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading credentials: {e.Message}");
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                PlayerPrefs.DeleteKey("remember_me");
                PlayerPrefs.DeleteKey("saved_user");
                PlayerPrefs.DeleteKey("saved_pass");
                PlayerPrefs.Save();

                Debug.Log("Credentials cleared");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error clearing credentials: {e.Message}");
            }
        }

        private void CheckAuthState()
        {
            if (firebaseAuthService != null && firebaseAuthService.Auth != null)
            {
                var currentUser = firebaseAuthService.Auth.CurrentUser;
                if (currentUser != null)
                {
                    Debug.Log($"User already logged in: {currentUser.Email}");
                    UpdateLogButtonSprite();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (firebaseAuthService != null)
            {
                firebaseAuthService.OnUserSignedIn -= OnUserSignedIn;
                firebaseAuthService.OnUserSignedOut -= OnUserSignedOut;
                firebaseAuthService.OnAuthError -= OnAuthError;
            }
        }
    }
}

