using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;

namespace AntKnow.Auth
{
    /// <summary>
    /// Panel Home - hiển thị nhân vật đã chọn và các chức năng chính
    /// </summary>
    public class PanelHome : MonoBehaviour
    {
        [Header("Character Display")]
        [SerializeField] private Image characterImage; // 1 Image component duy nhất

        [Header("Character Sprites")]
        [SerializeField] private Sprite maleCharacterSprite;
        [SerializeField] private Sprite femaleCharacterSprite;

        [Header("Action Buttons")]
        [SerializeField] private Button buttonFindMatch; // Button "Tìm trận"
        [SerializeField] private Button buttonCustomRoom; // Button "Tạo phòng"

        [Header("Matchmaking UI")]
        [SerializeField] private Button buttonWaitGame; // Button hiện khi đang tìm trận
        [SerializeField] private Text textWaitTimer; // Text countdown

        [Header("References")]
        [SerializeField] private LobbyUIManager lobbyUIManager; // Reference to Lobby UI Manager
        [SerializeField] private PanelMatchNotification panelMatchNotification; // Reference to Match Notification

        private GameDataManager gameDataManager;
        private bool isSearchingMatch = false;

        private void Start()
        {
            InitializePanelHome();
        }

        private void InitializePanelHome()
        {
            gameDataManager = GameDataManager.Instance;
            SetupEventListeners();
            SubscribeToMatchmaker();

            // Hide wait button initially
            if (buttonWaitGame != null)
                buttonWaitGame.gameObject.SetActive(false);

            // Không cập nhật sprite ngay lập tức vì data chưa load
            // Sẽ được gọi từ MenuSceneManager sau khi load data
            Debug.Log("PanelHome: Initialized, waiting for data...");
        }

        private void SetupEventListeners()
        {
            // Setup buttons
            if (buttonFindMatch != null)
            {
                buttonFindMatch.onClick.AddListener(OnFindMatchClicked);
            }

            if (buttonCustomRoom != null)
            {
                buttonCustomRoom.onClick.AddListener(OnCustomRoomClicked);
            }

            if (buttonWaitGame != null)
            {
                buttonWaitGame.onClick.AddListener(OnCancelMatchmaking);
            }
        }

        private void SubscribeToMatchmaker()
        {
            AntKnow.Services.MatchmakerService.OnMatchmakingStarted += OnMatchmakingStarted;
            AntKnow.Services.MatchmakerService.OnSearchTimeUpdated += OnSearchTimeUpdated;
            AntKnow.Services.MatchmakerService.OnMatchmakingCancelled += OnMatchmakingCancelled;
            AntKnow.Services.MatchmakerService.OnMatchFound += OnMatchFound;
        }

        private async void OnFindMatchClicked()
        {
            Debug.Log("PanelHome: Find Match button clicked");

            // Prevent multiple clicks
            if (isSearchingMatch)
            {
                Debug.LogWarning("PanelHome: Already searching for match");
                return;
            }

            // Sign in to UGS if needed
            if (!AntKnow.Services.UGSAuthService.IsSignedIn)
            {
                bool signedIn = await AntKnow.Services.UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
                if (!signedIn)
                {
                    Debug.LogError("PanelHome: Failed to sign in to UGS");
                    if (panelMatchNotification != null)
                        panelMatchNotification.ShowErrorNotification("Không thể đăng nhập UGS");
                    return;
                }
            }

            // Start matchmaking
            bool started = await AntKnow.Services.MatchmakerService.Instance.StartMatchmakingAsync();
            if (started)
            {
                isSearchingMatch = true;

                // Show wait button
                if (buttonWaitGame != null)
                    buttonWaitGame.gameObject.SetActive(true);

                // Disable buttons
                SetButtonsInteractable(false);

                // Show notification
                if (panelMatchNotification != null)
                    panelMatchNotification.ShowSearchingNotification();
            }
        }

        private void OnCustomRoomClicked()
        {
            OnCustomRoomClickedFromSubmenu();
        }

        /// <summary>
        /// Public method for SubmenuPlay - Find Match
        /// </summary>
        public void OnFindMatchClickedFromSubmenu()
        {
            OnFindMatchClicked();
        }

        /// <summary>
        /// Public method for SubmenuPlay - Create Lobby
        /// </summary>
        public void OnCustomRoomClickedFromSubmenu()
        {
            Debug.Log("PanelHome: Custom Room button clicked");

            // Prevent if searching match
            if (isSearchingMatch)
            {
                Debug.LogWarning("PanelHome: Cannot create room while searching for match");
                if (panelMatchNotification != null)
                    panelMatchNotification.ShowErrorNotification("Đang tìm trận, không thể tạo phòng");
                return;
            }

            if (lobbyUIManager != null)
            {
                lobbyUIManager.OpenCustomRoomPanel();
            }
            else
            {
                Debug.LogError("PanelHome: LobbyUIManager reference is null!");
            }
        }

        private void OnCancelMatchmaking()
        {
            Debug.Log("PanelHome: Cancel matchmaking");
            AntKnow.Services.MatchmakerService.Instance.CancelMatchmaking();
        }

        /// <summary>
        /// Event: Matchmaking started
        /// </summary>
        private void OnMatchmakingStarted()
        {
            Debug.Log("PanelHome: Matchmaking started");
            isSearchingMatch = true;
            SetButtonsInteractable(false);
        }

        /// <summary>
        /// Event: Search time updated
        /// </summary>
        private void OnSearchTimeUpdated(float remainingTime)
        {
            if (textWaitTimer != null)
            {
                int seconds = Mathf.CeilToInt(remainingTime);
                textWaitTimer.text = $"Đang tìm... {seconds}s";
            }
        }

        /// <summary>
        /// Event: Matchmaking cancelled
        /// </summary>
        private void OnMatchmakingCancelled()
        {
            Debug.Log("PanelHome: Matchmaking cancelled");
            isSearchingMatch = false;

            if (buttonWaitGame != null)
                buttonWaitGame.gameObject.SetActive(false);

            // Re-enable buttons
            SetButtonsInteractable(true);

            // Show notification
            if (panelMatchNotification != null)
                panelMatchNotification.ShowCancelledNotification();
        }

        /// <summary>
        /// Event: Match found
        /// </summary>
        private void OnMatchFound(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            Debug.Log($"PanelHome: Match found - {lobby.Name}");
            isSearchingMatch = false;

            if (buttonWaitGame != null)
                buttonWaitGame.gameObject.SetActive(false);

            // Show notification
            if (panelMatchNotification != null)
                panelMatchNotification.ShowMatchFoundNotification();
        }

        /// <summary>
        /// Enable/Disable buttons khi tìm trận
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {
            if (buttonFindMatch != null)
                buttonFindMatch.interactable = interactable;

            if (buttonCustomRoom != null)
                buttonCustomRoom.interactable = interactable;
        }

        public void UpdateCharacterDisplay()
        {
            // Cập nhật sprite dựa trên gender từ database
            UpdateCharacterSprite();
        }

        public void ForceUpdateCharacterSprite()
        {
            Debug.Log("PanelHome: Force updating character sprite...");
            
            // Kiểm tra xem có data chưa
            if (gameDataManager == null)
            {
                Debug.LogError("PanelHome: GameDataManager is null!");
                return;
            }
            
            if (string.IsNullOrEmpty(gameDataManager.currentGender))
            {
                Debug.LogWarning("PanelHome: Gender data not loaded yet, retrying in 0.5s...");
                Invoke(nameof(ForceUpdateCharacterSprite), 0.5f);
                return;
            }
            
            UpdateCharacterSprite();
        }

        private void UpdateCharacterSprite()
        {
            Debug.Log("=== PANELHOME DEBUG ===");
            
            if (characterImage == null) 
            {
                Debug.LogError("PanelHome: CharacterImage is null! Please assign it in the inspector.");
                return;
            }
            Debug.Log("✓ CharacterImage component found");

            if (gameDataManager == null)
            {
                Debug.LogError("PanelHome: GameDataManager is null!");
                return;
            }
            Debug.Log("✓ GameDataManager found");

            // Lấy gender từ database và cập nhật sprite
            string gender = gameDataManager.currentGender;
            Debug.Log($"PanelHome: Current gender from database: '{gender}'");
            Debug.Log($"PanelHome: Male sprite assigned: {maleCharacterSprite != null}");
            Debug.Log($"PanelHome: Female sprite assigned: {femaleCharacterSprite != null}");

            Sprite spriteToUse = null;

            if (gender == "male" && maleCharacterSprite != null)
            {
                spriteToUse = maleCharacterSprite;
                Debug.Log("PanelHome: Using MALE sprite");
            }
            else if (gender == "female" && femaleCharacterSprite != null)
            {
                spriteToUse = femaleCharacterSprite;
                Debug.Log("PanelHome: Using FEMALE sprite");
            }
            else
            {
                Debug.LogWarning($"PanelHome: No sprite found for gender '{gender}'");
                Debug.LogWarning($"PanelHome: Male sprite null: {maleCharacterSprite == null}");
                Debug.LogWarning($"PanelHome: Female sprite null: {femaleCharacterSprite == null}");
            }

            if (spriteToUse != null)
            {
                characterImage.sprite = spriteToUse;
                characterImage.enabled = true;
                Debug.Log($"PanelHome: ✓ SUCCESS - Updated character sprite to {gender}");
                Debug.Log($"PanelHome: Image enabled: {characterImage.enabled}");
                Debug.Log($"PanelHome: Image sprite: {characterImage.sprite != null}");
            }
            else
            {
                Debug.LogError($"PanelHome: ✗ FAILED - Could not set sprite for gender '{gender}'");
            }
            
            Debug.Log("=== END PANELHOME DEBUG ===");
        }

        // Không cần Update() nữa vì không có 3D model để xoay
        // Action buttons sẽ được xử lý trong panel con khác

        public void SetCharacterImage(Sprite sprite)
        {
            if (characterImage != null)
            {
                characterImage.sprite = sprite;
            }
        }

        // Loại bỏ dice animation để đơn giản hóa

        private void OnDestroy()
        {
            // Clean up event listeners
            if (buttonFindMatch != null)
            {
                buttonFindMatch.onClick.RemoveListener(OnFindMatchClicked);
            }

            if (buttonCustomRoom != null)
            {
                buttonCustomRoom.onClick.RemoveListener(OnCustomRoomClicked);
            }

            if (buttonWaitGame != null)
            {
                buttonWaitGame.onClick.RemoveListener(OnCancelMatchmaking);
            }

            // Unsubscribe from matchmaker
            AntKnow.Services.MatchmakerService.OnMatchmakingStarted -= OnMatchmakingStarted;
            AntKnow.Services.MatchmakerService.OnSearchTimeUpdated -= OnSearchTimeUpdated;
            AntKnow.Services.MatchmakerService.OnMatchmakingCancelled -= OnMatchmakingCancelled;
            AntKnow.Services.MatchmakerService.OnMatchFound -= OnMatchFound;
        }
    }
}
