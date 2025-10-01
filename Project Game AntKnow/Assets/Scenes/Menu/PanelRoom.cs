using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Lobbies.Models;
using AntKnow.Services;
using AntKnow.Game;

namespace AntKnow.Auth
{
    /// <summary>
    /// Panel Room - UI cho Matchmaking và Custom Lobby
    /// </summary>
    public class PanelRoom : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private GameObject customRoomPanel;
        [SerializeField] private GameObject lobbyPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button buttonFindMatch;
        [SerializeField] private Button buttonCustomRoom;
        [SerializeField] private Button buttonBack;

        [Header("Matchmaking UI")]
        [SerializeField] private TextMeshProUGUI textMatchmakingStatus;
        [SerializeField] private TextMeshProUGUI textSearchTimer;
        [SerializeField] private TextMeshProUGUI textPlayersCount;
        [SerializeField] private Button buttonCancelMatchmaking;
        [SerializeField] private Slider searchProgressBar;

        [Header("Custom Room UI")]
        [SerializeField] private Button buttonCreateRoom;
        [SerializeField] private Button buttonJoinRoom;
        [SerializeField] private TMP_InputField inputRoomName;
        [SerializeField] private TMP_InputField inputJoinCode;
        [SerializeField] private Toggle togglePrivateRoom;
        [SerializeField] private Button buttonBackFromCustom;

        [Header("Lobby UI")]
        [SerializeField] private TextMeshProUGUI textLobbyName;
        [SerializeField] private TextMeshProUGUI textLobbyCode;
        [SerializeField] private TextMeshProUGUI textLobbyPlayers;
        [SerializeField] private Transform playersListContainer;
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private Button buttonReady;
        [SerializeField] private Button buttonStartGame;
        [SerializeField] private Button buttonLeaveLobby;
        [SerializeField] private TextMeshProUGUI textReadyStatus;

        [Header("Error Display")]
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI textError;
        [SerializeField] private Button buttonCloseError;

        private bool isReady = false;

        private void Start()
        {
            InitializePanel();
            SetupEventListeners();
            ShowMainMenu();
        }

        private void OnEnable()
        {
            // Subscribe to service events
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            // Unsubscribe from service events
            UnsubscribeFromEvents();
        }

        private void InitializePanel()
        {
            // Hide all panels initially
            mainMenuPanel?.SetActive(false);
            matchmakingPanel?.SetActive(false);
            customRoomPanel?.SetActive(false);
            lobbyPanel?.SetActive(false);
            errorPanel?.SetActive(false);
        }

        private void SetupEventListeners()
        {
            // Main menu buttons
            buttonFindMatch?.onClick.AddListener(OnFindMatchClicked);
            buttonCustomRoom?.onClick.AddListener(OnCustomRoomClicked);
            buttonBack?.onClick.AddListener(OnBackClicked);

            // Matchmaking buttons
            buttonCancelMatchmaking?.onClick.AddListener(OnCancelMatchmakingClicked);

            // Custom room buttons
            buttonCreateRoom?.onClick.AddListener(OnCreateRoomClicked);
            buttonJoinRoom?.onClick.AddListener(OnJoinRoomClicked);
            buttonBackFromCustom?.onClick.AddListener(ShowMainMenu);

            // Lobby buttons
            buttonReady?.onClick.AddListener(OnReadyClicked);
            buttonStartGame?.onClick.AddListener(OnStartGameClicked);
            buttonLeaveLobby?.onClick.AddListener(OnLeaveLobbyClicked);

            // Error panel
            buttonCloseError?.onClick.AddListener(() => errorPanel?.SetActive(false));
        }

        private void SubscribeToEvents()
        {
            // Matchmaker events
            MatchmakerService.OnMatchmakingStarted += OnMatchmakingStarted;
            MatchmakerService.OnMatchmakingCancelled += OnMatchmakingCancelled;
            MatchmakerService.OnMatchmakingError += OnMatchmakingError;
            MatchmakerService.OnSearchTimeUpdated += OnSearchTimeUpdated;
            MatchmakerService.OnMatchFound += OnMatchFound;
            MatchmakerService.OnPlayersCountUpdated += OnPlayersCountUpdated;

            // Lobby events
            CustomLobbyService.OnLobbyCreated += OnLobbyCreated;
            CustomLobbyService.OnLobbyJoined += OnLobbyJoined;
            CustomLobbyService.OnLobbyLeft += OnLobbyLeft;
            CustomLobbyService.OnLobbyError += OnLobbyError;
            CustomLobbyService.OnPlayersUpdated += OnPlayersUpdated;
            CustomLobbyService.OnGameStarting += OnGameStarting;
        }

        private void UnsubscribeFromEvents()
        {
            // Matchmaker events
            MatchmakerService.OnMatchmakingStarted -= OnMatchmakingStarted;
            MatchmakerService.OnMatchmakingCancelled -= OnMatchmakingCancelled;
            MatchmakerService.OnMatchmakingError -= OnMatchmakingError;
            MatchmakerService.OnSearchTimeUpdated -= OnSearchTimeUpdated;
            MatchmakerService.OnMatchFound -= OnMatchFound;
            MatchmakerService.OnPlayersCountUpdated -= OnPlayersCountUpdated;

            // Lobby events
            CustomLobbyService.OnLobbyCreated -= OnLobbyCreated;
            CustomLobbyService.OnLobbyJoined -= OnLobbyJoined;
            CustomLobbyService.OnLobbyLeft -= OnLobbyLeft;
            CustomLobbyService.OnLobbyError -= OnLobbyError;
            CustomLobbyService.OnPlayersUpdated -= OnPlayersUpdated;
            CustomLobbyService.OnGameStarting -= OnGameStarting;
        }

        #region UI Navigation

        private void ShowMainMenu()
        {
            mainMenuPanel?.SetActive(true);
            matchmakingPanel?.SetActive(false);
            customRoomPanel?.SetActive(false);
            lobbyPanel?.SetActive(false);
        }

        private void ShowMatchmaking()
        {
            mainMenuPanel?.SetActive(false);
            matchmakingPanel?.SetActive(true);
            customRoomPanel?.SetActive(false);
            lobbyPanel?.SetActive(false);
        }

        private void ShowCustomRoom()
        {
            mainMenuPanel?.SetActive(false);
            matchmakingPanel?.SetActive(false);
            customRoomPanel?.SetActive(true);
            lobbyPanel?.SetActive(false);
        }

        private void ShowLobby()
        {
            mainMenuPanel?.SetActive(false);
            matchmakingPanel?.SetActive(false);
            customRoomPanel?.SetActive(false);
            lobbyPanel?.SetActive(true);
        }

        #endregion

        #region Button Handlers

        private async void OnFindMatchClicked()
        {
            Debug.Log("PanelRoom: Find Match clicked");
            
            // Ensure UGS is signed in
            if (!UGSAuthService.IsSignedIn)
            {
                Debug.Log("PanelRoom: Signing in to UGS...");
                bool signedIn = await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
                if (!signedIn)
                {
                    ShowError("Không thể đăng nhập UGS. Vui lòng thử lại.");
                    return;
                }
            }

            ShowMatchmaking();
            await MatchmakerService.Instance.StartMatchmakingAsync();
        }

        private void OnCustomRoomClicked()
        {
            Debug.Log("PanelRoom: Custom Room clicked");
            ShowCustomRoom();
        }

        private void OnBackClicked()
        {
            Debug.Log("PanelRoom: Back clicked");
            gameObject.SetActive(false);
        }

        private void OnCancelMatchmakingClicked()
        {
            Debug.Log("PanelRoom: Cancel Matchmaking clicked");
            MatchmakerService.Instance.CancelMatchmaking();
        }

        private async void OnCreateRoomClicked()
        {
            string roomName = inputRoomName.text;
            if (string.IsNullOrEmpty(roomName))
            {
                ShowError("Vui lòng nhập tên phòng");
                return;
            }

            Debug.Log($"PanelRoom: Creating room: {roomName}");
            
            // Ensure UGS is signed in
            if (!UGSAuthService.IsSignedIn)
            {
                bool signedIn = await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
                if (!signedIn)
                {
                    ShowError("Không thể đăng nhập UGS. Vui lòng thử lại.");
                    return;
                }
            }

            bool isPrivate = togglePrivateRoom.isOn;
            await CustomLobbyService.Instance.CreateLobbyAsync(roomName, isPrivate);
        }

        private async void OnJoinRoomClicked()
        {
            string joinCode = inputJoinCode.text;
            if (string.IsNullOrEmpty(joinCode))
            {
                ShowError("Vui lòng nhập mã phòng");
                return;
            }

            Debug.Log($"PanelRoom: Joining room with code: {joinCode}");
            
            // Ensure UGS is signed in
            if (!UGSAuthService.IsSignedIn)
            {
                bool signedIn = await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
                if (!signedIn)
                {
                    ShowError("Không thể đăng nhập UGS. Vui lòng thử lại.");
                    return;
                }
            }

            await CustomLobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
        }

        private async void OnReadyClicked()
        {
            isReady = !isReady;
            await CustomLobbyService.Instance.SetPlayerReadyAsync(isReady);
            UpdateReadyButton();
        }

        private async void OnStartGameClicked()
        {
            Debug.Log("PanelRoom: Start Game clicked");
            await CustomLobbyService.Instance.StartGameAsync();
        }

        private async void OnLeaveLobbyClicked()
        {
            Debug.Log("PanelRoom: Leave Lobby clicked");
            await CustomLobbyService.Instance.LeaveLobbyAsync();
        }

        #endregion

        #region Event Handlers - Matchmaking

        private void OnMatchmakingStarted()
        {
            Debug.Log("PanelRoom: Matchmaking started");
            if (textMatchmakingStatus != null)
                textMatchmakingStatus.text = "Đang tìm trận...";
        }

        private void OnMatchmakingCancelled()
        {
            Debug.Log("PanelRoom: Matchmaking cancelled");
            ShowMainMenu();
        }

        private void OnMatchmakingError(string error)
        {
            Debug.LogError($"PanelRoom: Matchmaking error: {error}");
            ShowError(error);
            ShowMainMenu();
        }

        private void OnSearchTimeUpdated(float remainingTime)
        {
            if (textSearchTimer != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                textSearchTimer.text = $"{minutes:00}:{seconds:00}";
            }

            if (searchProgressBar != null)
            {
                searchProgressBar.value = remainingTime / 60f; // Assuming 60s max
            }
        }

        private void OnMatchFound(Lobby lobby)
        {
            Debug.Log($"PanelRoom: Match found: {lobby.Name}");
            ShowLobby();
            UpdateLobbyUI(lobby);
        }

        private void OnPlayersCountUpdated(int current, int max)
        {
            if (textPlayersCount != null)
                textPlayersCount.text = $"Người chơi: {current}/{max}";
                
            if (textLobbyPlayers != null)
                textLobbyPlayers.text = $"Người chơi: {current}/{max}";
        }

        #endregion

        #region Event Handlers - Lobby

        private void OnLobbyCreated(Lobby lobby)
        {
            Debug.Log($"PanelRoom: Lobby created: {lobby.Name}");
            ShowLobby();
            UpdateLobbyUI(lobby);
        }

        private void OnLobbyJoined(Lobby lobby)
        {
            Debug.Log($"PanelRoom: Lobby joined: {lobby.Name}");
            ShowLobby();
            UpdateLobbyUI(lobby);
        }

        private void OnLobbyLeft()
        {
            Debug.Log("PanelRoom: Left lobby");
            isReady = false;
            ShowMainMenu();
        }

        private void OnLobbyError(string error)
        {
            Debug.LogError($"PanelRoom: Lobby error: {error}");
            ShowError(error);
        }

        private void OnPlayersUpdated(List<Player> players)
        {
            Debug.Log($"PanelRoom: Players updated: {players.Count}");
            UpdatePlayersList(players);
            UpdateStartButton();
        }

        private async void OnGameStarting(string relayJoinCode)
        {
            Debug.Log($"PanelRoom: Game starting with Relay code: {relayJoinCode}");

            // 1. Setup GameSessionData
            var sessionData = GameSessionData.Instance;
            sessionData.SetFromGameDataManager();
            sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);

            bool isHost = CustomLobbyService.Instance.IsHost;
            string lobbyId = CustomLobbyService.Instance.CurrentLobby?.Id;
            sessionData.SetNetworkInfo(relayJoinCode, isHost, lobbyId);

            Debug.Log($"PanelRoom: Session data prepared");
            Debug.Log(sessionData.GetSummary());

            // 2. Join Relay
            if (isHost)
            {
                // Host already created the relay, just start
                RelayService.Instance.StartHost();
            }
            else
            {
                // Client needs to join relay
                await RelayService.Instance.JoinRelayAsync(relayJoinCode);
                RelayService.Instance.StartClient();
            }

            // 3. Load game scene
            Debug.Log("PanelRoom: Loading SceneGame...");
            SceneManager.LoadScene("SceneGame");
        }

        #endregion

        #region UI Updates

        private void UpdateLobbyUI(Lobby lobby)
        {
            if (textLobbyName != null)
                textLobbyName.text = lobby.Name;
                
            if (textLobbyCode != null)
                textLobbyCode.text = $"Mã phòng: {lobby.LobbyCode}";
                
            if (textLobbyPlayers != null)
                textLobbyPlayers.text = $"Người chơi: {lobby.Players.Count}/{lobby.MaxPlayers}";

            UpdatePlayersList(lobby.Players);
            UpdateStartButton();
        }

        private void UpdatePlayersList(List<Player> players)
        {
            if (playersListContainer == null || playerItemPrefab == null)
                return;

            // Clear existing items
            foreach (Transform child in playersListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new items
            foreach (var player in players)
            {
                GameObject item = Instantiate(playerItemPrefab, playersListContainer);
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string playerName = player.Data.ContainsKey("PlayerName") ? player.Data["PlayerName"].Value : "Player";
                    string readyStatus = player.Data.ContainsKey("IsReady") && player.Data["IsReady"].Value == "true" ? " ✓" : "";
                    text.text = $"{playerName}{readyStatus}";
                }
            }
        }

        private void UpdateReadyButton()
        {
            if (textReadyStatus != null)
                textReadyStatus.text = isReady ? "Hủy sẵn sàng" : "Sẵn sàng";
        }

        private void UpdateStartButton()
        {
            if (buttonStartGame != null)
            {
                buttonStartGame.gameObject.SetActive(CustomLobbyService.Instance.IsHost);
            }
        }

        private void ShowError(string message)
        {
            if (errorPanel != null && textError != null)
            {
                textError.text = message;
                errorPanel.SetActive(true);
            }
        }

        #endregion
    }
}
