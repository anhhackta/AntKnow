using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AntKnow.Auth;
using System.Threading.Tasks;

namespace AntKnow.Auth
{
    /// <summary>
    /// Controller cho LoadingScene - chuyển từ Login sang Menu
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("Loading UI")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image backgroundPanel;
        [SerializeField] private Text tipsText;
        [SerializeField] private CanvasGroup canvasGroup; // For fade out effect

        [Header("Background Images")]
        [SerializeField] private Sprite[] backgroundSprites;

        [Header("Game Tips/Facts")]
        [SerializeField] private string[] gameTips = {
            "💡 Tip: Use skill cards strategically to win!",
            "🎮 Fact: Each card can be upgraded and evolved to increase power.",
            "⚡ Tip: Combine equipment to optimize your stats.",
            "🏆 Fact: Winning matches will earn you AntCoin and experience.",
            "🎯 Tip: Read quiz questions carefully to answer correctly.",
            "💎 Tip: Use DCoin to buy special items.",
            "🔥 Fact: Cards with more stars have shorter cooldowns.",
            "🎪 Tip: Join more matches to accumulate experience."
        };

        [Header("Settings")]
        [SerializeField] private float tipChangeInterval = 3f; // Đổi tip mỗi 3 giây
        [SerializeField] private float backgroundChangeInterval = 15f; // Đổi background mỗi 15 giây
        [SerializeField] private float minLoadingTime = 2f; // Thời gian loading tối thiểu (để UI đẹp)
        [SerializeField] private float maxLoadingTime = 10f; // Thời gian loading tối đa (timeout)

        [Header("Loading Steps")]
        [SerializeField] private string[] loadingSteps = {
            "Connecting to Firebase...",
            "Loading user data...",
            "Checking inventory...",
            "Preparing loadout...",
            "Loading game configuration...",
            "Complete!"
        };

        // Static configuration for reusable loading
        public static string sourceScene = "LoginScene";  // Where we came from
        public static string targetScene = "MenuScene";   // Where we're going
        public static bool checkProfile = true;           // Check ingame name + gender?

        private int currentTipIndex = 0;
        private int currentBackgroundIndex = 0;
        private int currentStepIndex = 0;
        private FirebaseAuthService firebaseAuthService;
        private bool isLoadingComplete = false;

        private void Start()
        {
            // Kiểm tra user đã login chưa
            if (!GameDataManager.Instance.isUserLoggedIn)
            {
                Debug.LogError("LoadingScene: No user logged in, redirecting to LoginScene");
                SceneManager.LoadScene("LoginScene");
                return;
            }

            Debug.Log($"LoadingScene: Loading data for user {GameDataManager.Instance.currentUsername}");
            
            // Khởi tạo UI và services
            InitializeUI();
            InitializeServices();
            
            // Bắt đầu loading thực sự và các coroutines
            StartCoroutine(LoadMenuSceneAsync());
            StartCoroutine(ChangeTipsCoroutine());
            StartCoroutine(ChangeBackgroundCoroutine());
        }

        private void InitializeServices()
        {
            firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            if (firebaseAuthService == null)
            {
                Debug.LogError("LoadingScene: FirebaseAuthService not found!");
            }
        }

        private void InitializeUI()
        {
            // Set background đầu tiên
            if (backgroundPanel != null && backgroundSprites != null && backgroundSprites.Length > 0)
            {
                backgroundPanel.sprite = backgroundSprites[0];
            }

            // Hiển thị tip đầu tiên
            ShowCurrentTip();
        }

        private IEnumerator LoadMenuSceneAsync()
        {
            float startTime = Time.time;
            float progress = 0f;
            int totalSteps = loadingSteps.Length;
            
            // Bắt đầu loading thực sự
            Task<bool> loadingTask = PerformRealLoading();
            
            while (!isLoadingComplete)
            {
                // Update progress bar dựa trên steps
                progress = (float)currentStepIndex / totalSteps;
                if (progressBar != null)
                    progressBar.value = progress;
                
                // Update loading step text
                if (currentStepIndex < loadingSteps.Length)
                {
                    Debug.Log($"LoadingScene: {loadingSteps[currentStepIndex]}");
                }
                
                // Check timeout
                if (Time.time - startTime > maxLoadingTime)
                {
                    Debug.LogWarning("LoadingScene: Loading timeout, proceeding anyway");
                    break;
                }
                
                yield return null;
            }
            
            // Ensure 100% progress
            if (progressBar != null)
                progressBar.value = 1f;
            
            // Wait minimum time để UI đẹp
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsedTime);
            }
            
            // Fade out loading screen
            yield return StartCoroutine(FadeOut());

            // Determine next scene based on configuration
            string nextScene = targetScene;

            // If checkProfile is enabled, verify user has complete profile
            if (checkProfile)
            {
                bool hasIngameName = !string.IsNullOrEmpty(GameDataManager.Instance.currentIngameName);
                bool hasGender = !string.IsNullOrEmpty(GameDataManager.Instance.currentGender);

                if (hasIngameName && hasGender)
                {
                    // User has complete profile, go to target scene
                    nextScene = targetScene;
                    Debug.Log($"LoadingScene: User has complete profile (Name: {GameDataManager.Instance.currentIngameName}, Gender: {GameDataManager.Instance.currentGender}), loading {nextScene}");
                }
                else
                {
                    // User needs to select character
                    nextScene = "SelectCharacterScene";
                    Debug.Log($"LoadingScene: User needs to select character (Name: {GameDataManager.Instance.currentIngameName}, Gender: {GameDataManager.Instance.currentGender}), loading {nextScene}");
                }
            }
            else
            {
                // No profile check, go directly to target scene
                Debug.Log($"LoadingScene: Loading {nextScene} (no profile check)");
            }

            // Load next scene (Single mode will unload LoadingScene)
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                // No CanvasGroup, just wait a bit
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            float fadeDuration = 0.5f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Configure LoadingScene before loading it
        /// </summary>
        /// <param name="source">Source scene name (where we came from)</param>
        /// <param name="target">Target scene name (where we're going)</param>
        /// <param name="checkUserProfile">Check if user has ingame name + gender?</param>
        public static void Configure(string source, string target, bool checkUserProfile = false)
        {
            sourceScene = source;
            targetScene = target;
            checkProfile = checkUserProfile;
            Debug.Log($"LoadingScene configured: {source} → {target} (checkProfile: {checkUserProfile})");
        }

        /// <summary>
        /// Load LoadingScene with configuration
        /// Example: LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false);
        /// </summary>
        public static void LoadWithConfig(string source, string target, bool checkUserProfile = false)
        {
            Configure(source, target, checkUserProfile);
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
        }

        private async Task<bool> PerformRealLoading()
        {
            try
            {
                string uid = GameDataManager.Instance.currentUserId;
                if (string.IsNullOrEmpty(uid))
                {
                    Debug.LogError("LoadingScene: No user UID available");
                    return false;
                }

                // Step 1: Kết nối Firebase (đã có sẵn)
                currentStepIndex = 1;
                await Task.Delay(200); // Simulate connection time
                
                // Step 2: Tải thông tin người dùng
                currentStepIndex = 2;
                var userData = await firebaseAuthService.GetUserDataAsync(uid);
                if (userData == null)
                {
                    Debug.LogError("LoadingScene: Failed to load user data");
                    return false;
                }
                
                // Step 3: Kiểm tra inventory
                currentStepIndex = 3;
                bool hasInventory = await firebaseAuthService.HasInventoryAsync(uid);
                Debug.Log($"LoadingScene: User has inventory: {hasInventory}");
                
                // Step 4: Chuẩn bị loadout (nếu chưa có inventory thì sẽ tạo trong MenuScene)
                currentStepIndex = 4;
                await Task.Delay(300); // Simulate loadout preparation
                
                // Step 5: Tải cấu hình game (có thể thêm logic tải configs)
                currentStepIndex = 5;
                await Task.Delay(200); // Simulate config loading
                
                // Step 6: Hoàn thành
                currentStepIndex = 6;
                isLoadingComplete = true;
                
                Debug.Log("LoadingScene: Real loading completed successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LoadingScene: Error during real loading: {e.Message}");
                isLoadingComplete = true; // Vẫn cho phép tiếp tục
                return false;
            }
        }

        private IEnumerator ChangeTipsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(tipChangeInterval);
                
                // Chuyển sang tip tiếp theo
                currentTipIndex = (currentTipIndex + 1) % gameTips.Length;
                ShowCurrentTip();
            }
        }

        private IEnumerator ChangeBackgroundCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(backgroundChangeInterval);
                
                // Chuyển sang background tiếp theo
                if (backgroundSprites != null && backgroundSprites.Length > 1)
                {
                    currentBackgroundIndex = (currentBackgroundIndex + 1) % backgroundSprites.Length;
                    ShowCurrentBackground();
                }
            }
        }

        private void ShowCurrentTip()
        {
            if (tipsText != null && gameTips.Length > 0)
            {
                tipsText.text = gameTips[currentTipIndex];
                Debug.Log($"LoadingScene: Showing tip {currentTipIndex + 1}/{gameTips.Length}");
            }
        }

        private void ShowCurrentBackground()
        {
            if (backgroundPanel != null && backgroundSprites != null && backgroundSprites.Length > 0)
            {
                backgroundPanel.sprite = backgroundSprites[currentBackgroundIndex];
                Debug.Log($"LoadingScene: Changed to background {currentBackgroundIndex + 1}/{backgroundSprites.Length}");
            }
        }

        /// <summary>
        /// Skip loading (for testing)
        /// </summary>
        public void SkipLoading()
        {
            StopAllCoroutines();
            SceneManager.LoadScene("SelectCharacterScene");
        }

        /// <summary>
        /// Add more tips/facts dynamically
        /// </summary>
        public void AddGameTip(string tip)
        {
            if (!string.IsNullOrEmpty(tip))
            {
                // Tạo array mới với tip thêm vào
                string[] newTips = new string[gameTips.Length + 1];
                for (int i = 0; i < gameTips.Length; i++)
                {
                    newTips[i] = gameTips[i];
                }
                newTips[gameTips.Length] = tip;
                gameTips = newTips;
                
                Debug.Log($"LoadingScene: Added new tip - {tip}");
            }
        }

        /// <summary>
        /// Set custom minimum loading time
        /// </summary>
        public void SetMinLoadingTime(float duration)
        {
            minLoadingTime = Mathf.Max(0.5f, duration);
            Debug.Log($"LoadingScene: Minimum loading time set to {minLoadingTime} seconds");
        }

        /// <summary>
        /// Set custom maximum loading time (timeout)
        /// </summary>
        public void SetMaxLoadingTime(float duration)
        {
            maxLoadingTime = Mathf.Max(minLoadingTime + 1f, duration);
            Debug.Log($"LoadingScene: Maximum loading time set to {maxLoadingTime} seconds");
        }

        /// <summary>
        /// Force complete loading (for testing)
        /// </summary>
        public void ForceCompleteLoading()
        {
            isLoadingComplete = true;
            currentStepIndex = loadingSteps.Length - 1;
            Debug.Log("LoadingScene: Loading force completed");
        }
    }
}
