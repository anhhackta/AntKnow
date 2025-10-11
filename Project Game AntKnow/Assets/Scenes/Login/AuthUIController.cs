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
    public class AuthUIController : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject panelLog;
        [SerializeField] private GameObject panelLogin;
        [SerializeField] private GameObject panelRegister;
        [SerializeField] private GameObject panelThongBao;
        [SerializeField] private GameObject logButton;
        [SerializeField] private GameObject buttonStart;

        [Header("Login Panel")]
        [SerializeField] private TMP_InputField inputUsernameOrEmail;
        [SerializeField] private TMP_InputField inputPassword;
        [SerializeField] private Toggle toggleRememberMe;
        [SerializeField] private Button buttonLogin;
        [SerializeField] private Button buttonLoginWithGoogle;
        [SerializeField] private TMP_Text textInlineError;

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

        [Header("Tabs and Controls")]
        [SerializeField] private Button buttonLoginTab;
        [SerializeField] private Button buttonRegisterTab;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonLogButton;
        [SerializeField] private Button buttonStartButton;

        [Header("LogButton Sprites")]
        [SerializeField] private Sprite spriteLogin;
        [SerializeField] private Sprite spriteLogout;

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        [Header("Avatar Panel")]
        [SerializeField] private AvatarPanel avatarPanel;

        [Header("Notification")]
        [SerializeField] private TMP_Text textNotification;

        private bool isProcessing = false;
        private Coroutine notificationCoroutine;
        private UserData currentUserData;

        // Debounce coroutines để tránh spam Firestore queries
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
            panelThongBao.SetActive(false);
            logButton.SetActive(false);
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

            // Set placeholders
            inputUsernameOrEmail.placeholder.GetComponent<TMP_Text>().text = "Username hoặc Email";
            inputUsername.placeholder.GetComponent<TMP_Text>().text = "Username";
            inputEmail.placeholder.GetComponent<TMP_Text>().text = "Email";
            inputPassword.placeholder.GetComponent<TMP_Text>().text = "Password";
            inputPassword1.placeholder.GetComponent<TMP_Text>().text = "Password (≥8 ký tự)";
            inputPassword2.placeholder.GetComponent<TMP_Text>().text = "Confirm Password";
        }

        private void SetupEventListeners()
        {
            // Tab switching
            buttonLoginTab.onClick.AddListener(() => SwitchToLoginPanel());
            buttonRegisterTab.onClick.AddListener(() => SwitchToRegisterPanel());

            // Login panel
            buttonLogin.onClick.AddListener(OnLoginClicked);
            buttonLoginWithGoogle.onClick.AddListener(OnGoogleLoginClicked);

            // Register panel
            buttonCreateAccount.onClick.AddListener(OnRegisterClicked);

            // Real-time validation for register panel
            inputUsername.onValueChanged.AddListener(OnUsernameChanged);
            inputEmail.onValueChanged.AddListener(OnEmailChanged);
            inputPassword1.onValueChanged.AddListener(OnPassword1Changed);
            inputPassword2.onValueChanged.AddListener(OnPassword2Changed);

            // Other controls
            buttonClose.onClick.AddListener(OnClosePanelClicked);
            buttonLogButton.onClick.AddListener(OnLogButtonClicked);
            buttonStartButton.onClick.AddListener(OnStartButtonClicked);

            // Firebase auth events
            if (firebaseAuthService != null)
            {
                firebaseAuthService.OnUserSignedIn += OnUserSignedIn;
                firebaseAuthService.OnUserSignedOut += OnUserSignedOut;
                firebaseAuthService.OnAuthError += OnAuthError;
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
                ShowInlineError("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            SetProcessing(true);
            textInlineError.gameObject.SetActive(false);

            try
            {
                var result = await firebaseAuthService.SignInWithEmailOrUsernameAsync(userOrEmail, password);
                
                if (result.IsSuccess)
                {
                    ShowNotification("Đăng nhập thành công!", false);
                    
                    // Save credentials if remember me is checked
                    if (toggleRememberMe.isOn)
                    {
                        SaveCredentials(userOrEmail);
                    }
                    else
                    {
                        ClearSavedCredentials();
                    }

                    // Load user data and show avatar panel
                    await LoadUserDataAndShowAvatar(result.User.UserId);
                }
                else
                {
                    ShowInlineError(result.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                ShowInlineError($"Lỗi đăng nhập: {e.Message}");
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private void OnGoogleLoginClicked()
        {
            // Tạm thời khóa Google Login - đang update
            ShowNotification("Google Login đang được cập nhật, vui lòng sử dụng Email/Password", true);
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
                ShowNotification("Vui lòng kiểm tra lại thông tin", true);
                return;
            }

            SetProcessing(true);

            try
            {
                var result = await firebaseAuthService.RegisterAsync(username, email, password1);
                
                if (result.IsSuccess)
                {
                    ShowNotification("Tạo tài khoản thành công!", false);
                    
                    // Load user data and show avatar panel
                    await LoadUserDataAndShowAvatar(result.User.UserId);
                }
                else
                {
                    ShowNotification(result.ErrorMessage, true);
                }
            }
            catch (Exception e)
            {
                ShowNotification($"Lỗi tạo tài khoản: {e.Message}", true);
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private void OnUsernameChanged(string value)
        {
            // Cancel coroutine cũ để tránh spam queries
            if (usernameCheckCoroutine != null)
            {
                StopCoroutine(usernameCheckCoroutine);
            }

            if (string.IsNullOrEmpty(value))
            {
                textCheckUsername.gameObject.SetActive(false);
                return;
            }

            // Kiểm tra Firebase service có tồn tại không
            if (firebaseAuthService == null)
            {
                textCheckUsername.gameObject.SetActive(true);
                textCheckUsername.text = "Firebase service not found";
                textCheckUsername.color = Color.red;
                return;
            }

            // Kiểm tra Firebase đã sẵn sàng chưa
            if (!firebaseAuthService.IsFirebaseReady())
            {
                textCheckUsername.gameObject.SetActive(true);
                textCheckUsername.text = "Đang khởi tạo Firebase...";
                textCheckUsername.color = Color.yellow;
                return;
            }

            // Start debounced check (đợi 0.5s sau khi user ngừng gõ)
            usernameCheckCoroutine = StartCoroutine(CheckUsernameDebounced(value));
        }

        private IEnumerator CheckUsernameDebounced(string username)
        {
            // Đợi 0.5 giây
            yield return new WaitForSeconds(0.5f);

            textCheckUsername.gameObject.SetActive(true);
            textCheckUsername.text = "Đang kiểm tra...";
            textCheckUsername.color = Color.yellow;

            // Gọi async method trong coroutine
            var checkTask = firebaseAuthService.IsUsernameTakenAsync(username);
            yield return new WaitUntil(() => checkTask.IsCompleted);

            try
            {
                bool isTaken = checkTask.Result;

                if (isTaken)
                {
                    textCheckUsername.text = "Username đã được sử dụng";
                    textCheckUsername.color = Color.red;
                }
                else
                {
                    textCheckUsername.text = "Username có thể sử dụng";
                    textCheckUsername.color = Color.green;
                }
            }
            catch (Exception e)
            {
                // Xử lý lỗi permissions một cách thân thiện
                if (e.Message.Contains("permissions") || e.Message.Contains("Permission"))
                {
                    textCheckUsername.text = "Không thể kiểm tra username (cần cập nhật Firestore rules)";
                    textCheckUsername.color = Color.yellow;
                }
                else
                {
                    textCheckUsername.text = "Lỗi kiểm tra username";
                    textCheckUsername.color = Color.red;
                }
                Debug.LogError($"Username check error: {e.Message}");
            }

            ValidateRegisterForm();
        }

        private void OnEmailChanged(string value)
        {
            // Cancel coroutine cũ để tránh spam queries
            if (emailCheckCoroutine != null)
            {
                StopCoroutine(emailCheckCoroutine);
            }

            if (string.IsNullOrEmpty(value) || !value.Contains("@"))
            {
                textCheckEmail.gameObject.SetActive(false);
                return;
            }

            // Kiểm tra Firebase service có tồn tại không
            if (firebaseAuthService == null)
            {
                textCheckEmail.gameObject.SetActive(true);
                textCheckEmail.text = "Firebase service not found";
                textCheckEmail.color = Color.red;
                return;
            }

            // Kiểm tra Firebase đã sẵn sàng chưa
            if (!firebaseAuthService.IsFirebaseReady())
            {
                textCheckEmail.gameObject.SetActive(true);
                textCheckEmail.text = "Đang khởi tạo Firebase...";
                textCheckEmail.color = Color.yellow;
                return;
            }

            // Start debounced check (đợi 0.5s sau khi user ngừng gõ)
            emailCheckCoroutine = StartCoroutine(CheckEmailDebounced(value));
        }

        private IEnumerator CheckEmailDebounced(string email)
        {
            // Đợi 0.5 giây
            yield return new WaitForSeconds(0.5f);

            textCheckEmail.gameObject.SetActive(true);
            textCheckEmail.text = "Đang kiểm tra...";
            textCheckEmail.color = Color.yellow;

            // Gọi async method trong coroutine
            var checkTask = firebaseAuthService.IsEmailTakenAsync(email);
            yield return new WaitUntil(() => checkTask.IsCompleted);

            try
            {
                bool isTaken = checkTask.Result;
                
                if (isTaken)
                {
                    textCheckEmail.text = "Email đã được đăng ký";
                    textCheckEmail.color = Color.red;
                }
                else
                {
                    textCheckEmail.text = "Email có thể sử dụng";
                    textCheckEmail.color = Color.green;
                }
            }
            catch (Exception e)
            {
                // Xử lý lỗi permissions một cách thân thiện
                if (e.Message.Contains("permissions") || e.Message.Contains("Permission"))
                {
                    textCheckEmail.text = "Không thể kiểm tra email (cần cập nhật Firestore rules)";
                    textCheckEmail.color = Color.yellow;
                }
                else
                {
                    textCheckEmail.text = "Lỗi kiểm tra email";
                    textCheckEmail.color = Color.red;
                }
                Debug.LogError($"Email check error: {e.Message}");
            }

            ValidateRegisterForm();
        }

        private void OnPassword1Changed(string value)
        {
            textCheckPw1.gameObject.SetActive(!string.IsNullOrEmpty(value));
            
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (value.Length >= 8)
            {
                textCheckPw1.text = "Mật khẩu đủ mạnh";
                textCheckPw1.color = Color.green;
            }
            else
            {
                textCheckPw1.text = "Mật khẩu phải ≥8 ký tự";
                textCheckPw1.color = Color.red;
            }

            // Also check password2 if it's not empty
            if (!string.IsNullOrEmpty(inputPassword2.text))
            {
                OnPassword2Changed(inputPassword2.text);
            }

            ValidateRegisterForm();
        }

        private void OnPassword2Changed(string value)
        {
            textCheckPw2.gameObject.SetActive(!string.IsNullOrEmpty(value));
            
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (value == inputPassword1.text)
            {
                textCheckPw2.text = "Mật khẩu khớp";
                textCheckPw2.color = Color.green;
            }
            else
            {
                textCheckPw2.text = "Mật khẩu không khớp";
                textCheckPw2.color = Color.red;
            }

            ValidateRegisterForm();
        }

        private bool ValidateRegisterForm()
        {
            bool isValid = true;

            // Check username
            if (string.IsNullOrEmpty(inputUsername.text) || 
                (textCheckUsername.color != Color.green && textCheckUsername.color != Color.yellow))
            {
                isValid = false;
            }

            // Check email
            if (string.IsNullOrEmpty(inputEmail.text) || 
                !inputEmail.text.Contains("@") ||
                (textCheckEmail.color != Color.green && textCheckEmail.color != Color.yellow))
            {
                isValid = false;
            }

            // Check password1
            if (string.IsNullOrEmpty(inputPassword1.text) || 
                inputPassword1.text.Length < 8 ||
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

        private void OnClosePanelClicked()
        {
            panelLog.SetActive(false);
            logButton.SetActive(true);
            UpdateLogButtonSprite();
        }

        private void OnLogButtonClicked()
        {
            if (firebaseAuthService.Auth.CurrentUser != null)
            {
                // User is logged in, sign out
                firebaseAuthService.SignOutAsync();
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
            // Set user data in GameDataManager before switching scenes
            if (currentUserData != null)
            {
                GameDataManager.Instance.SetUserData(
                    currentUserData.uid,
                    currentUserData.username,
                    currentUserData.email,
                    currentUserData.ingameName
                );
                Debug.Log($"AuthUIController: User data set for scene transition - {currentUserData.username}");
            }
            
            // Load LoadingScene instead of directly to MenuScene
            SceneManager.LoadScene("LoadingScene");
        }


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
            
            // Hide avatar panel
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
            panelThongBao.SetActive(true);

            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            notificationCoroutine = StartCoroutine(HideNotificationAfterDelay(3f));
        }

        private IEnumerator HideNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            panelThongBao.SetActive(false);
        }

        private void SetProcessing(bool processing)
        {
            isProcessing = processing;
            buttonLogin.interactable = !processing;
            buttonLoginWithGoogle.interactable = !processing;
            buttonCreateAccount.interactable = !processing && ValidateRegisterForm();
        }

        private void SaveCredentials(string userOrEmail)
        {
            try
            {
                string encodedUser = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userOrEmail));
                PlayerPrefs.SetString("remember_me", "true");
                PlayerPrefs.SetString("saved_user", encodedUser);
                PlayerPrefs.Save();
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
                if (PlayerPrefs.GetString("remember_me") == "true")
                {
                    string encodedUser = PlayerPrefs.GetString("saved_user");
                    if (!string.IsNullOrEmpty(encodedUser))
                    {
                        string userOrEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedUser));
                        inputUsernameOrEmail.text = userOrEmail;
                        toggleRememberMe.isOn = true;
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
            PlayerPrefs.DeleteKey("remember_me");
            PlayerPrefs.DeleteKey("saved_user");
            PlayerPrefs.Save();
        }

        private async void CheckAuthState()
        {
            if (firebaseAuthService != null && firebaseAuthService.Auth != null)
            {
                if (firebaseAuthService.Auth.CurrentUser != null)
                {
                    await LoadUserDataAndShowAvatar(firebaseAuthService.Auth.CurrentUser.UserId);
                }
            }
        }

        /// <summary>
        /// Load user data and show avatar panel
        /// </summary>
        private async System.Threading.Tasks.Task LoadUserDataAndShowAvatar(string uid)
        {
            try
            {
                // Load user data from Firestore
                currentUserData = await firebaseAuthService.GetUserDataAsync(uid);
                
                if (currentUserData != null)
                {
                    // Hide login panel and show avatar panel
                    panelLog.SetActive(false);
                    logButton.SetActive(true);
                    buttonStart.SetActive(true);
                    
                    // Show avatar panel with user data
                    if (avatarPanel != null)
                    {
                        avatarPanel.ShowPanel(currentUserData);
                    }
                    
                    UpdateLogButtonSprite();
                    Debug.Log($"User data loaded and avatar panel shown: {currentUserData.username}");
                }
                else
                {
                    Debug.LogError("Failed to load user data");
                    ShowNotification("Lỗi tải dữ liệu người dùng", true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading user data: {e.Message}");
                ShowNotification("Lỗi tải dữ liệu người dùng", true);
            }
        }


        private void OnDestroy()
        {
            if (firebaseAuthService != null)
            {
                firebaseAuthService.OnUserSignedIn -= OnUserSignedIn;
                firebaseAuthService.OnUserSignedOut -= OnUserSignedOut;
                firebaseAuthService.OnAuthError -= OnAuthError;
            }

        }
    }
}
