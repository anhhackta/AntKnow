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

        private GameDataManager gameDataManager;

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
            AntKnow.Services.MatchmakerService.OnSearchTimeUpdated += OnSearchTimeUpdated;
            AntKnow.Services.MatchmakerService.OnMatchmakingCancelled += OnMatchmakingCancelled;
            AntKnow.Services.MatchmakerService.OnMatchFound += OnMatchFound;
        }

        private async void OnFindMatchClicked()
        {
            Debug.Log("PanelHome: Find Match button clicked");

            // Sign in to UGS if needed
            if (!AntKnow.Services.UGSAuthService.IsSignedIn)
            {
                bool signedIn = await AntKnow.Services.UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
                if (!signedIn)
                {
                    Debug.LogError("PanelHome: Failed to sign in to UGS");
                    return;
                }
            }

            // Start matchmaking
            bool started = await AntKnow.Services.MatchmakerService.Instance.StartMatchmakingAsync();
            if (started)
            {
                // Show wait button
                if (buttonWaitGame != null)
                    buttonWaitGame.gameObject.SetActive(true);
            }
        }

        private void OnCustomRoomClicked()
        {
            Debug.Log("PanelHome: Custom Room button clicked");

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

        private void OnSearchTimeUpdated(float remainingTime)
        {
            if (textWaitTimer != null)
            {
                int seconds = Mathf.CeilToInt(remainingTime);
                textWaitTimer.text = $"Đang tìm... {seconds}s";
            }
        }

        private void OnMatchmakingCancelled()
        {
            if (buttonWaitGame != null)
                buttonWaitGame.gameObject.SetActive(false);
        }

        private void OnMatchFound(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            Debug.Log($"PanelHome: Match found - {lobby.Name}");
            if (buttonWaitGame != null)
                buttonWaitGame.gameObject.SetActive(false);
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
            AntKnow.Services.MatchmakerService.OnSearchTimeUpdated -= OnSearchTimeUpdated;
            AntKnow.Services.MatchmakerService.OnMatchmakingCancelled -= OnMatchmakingCancelled;
            AntKnow.Services.MatchmakerService.OnMatchFound -= OnMatchFound;
        }
    }
}
