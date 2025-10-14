using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Lobbies.Models;
using AntKnow.Services;
using AntKnow.Game;

// Helper để hỗ trợ cả Text và TextMeshProUGUI
public static class TextHelper
{
    public static void SetText(GameObject obj, string text)
    {
        // Try TextMeshProUGUI first
        var tmpText = obj.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        // Fallback to Unity UI Text
        var uiText = obj.GetComponent<Text>();
        if (uiText != null)
        {
            uiText.text = text;
            return;
        }

        // Try children
        tmpText = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        uiText = obj.GetComponentInChildren<Text>();
        if (uiText != null)
        {
            uiText.text = text;
        }
    }

    public static List<Component> GetAllTexts(GameObject obj)
    {
        List<Component> texts = new List<Component>();

        // Get all TextMeshProUGUI
        var tmpTexts = obj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var t in tmpTexts)
            texts.Add(t);

        // Get all Unity UI Text
        var uiTexts = obj.GetComponentsInChildren<Text>();
        foreach (var t in uiTexts)
            texts.Add(t);

        return texts;
    }
}

namespace AntKnow.Auth
{
    /// <summary>
    /// Quản lý UI cho Lobby System
    /// Cấu trúc: PanelCustomRoom > Panel Container > 3 Panel con
    /// </summary>
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("Main Container")]
        [SerializeField] private GameObject panelCustomRoom; // GameObject trống chứa tất cả
        [SerializeField] private Button buttonClosePanelCustomRoom; // Button thoát
        
        [Header("Panel Container - Chứa 3 panel con")]
        [SerializeField] private GameObject panelContainer;
        
        [Header("3 Panel Con")]
        [SerializeField] private GameObject panelRoom; // Panel mặc định - List phòng
        [SerializeField] private GameObject panelCreateRoom; // Popup tạo phòng (overlay)
        [SerializeField] private GameObject panelJoinRoom; // Panel khi vào phòng
        
        [Header("PanelRoom - List phòng")]
        [SerializeField] private Button buttonCreateRoom; // Mở PanelCreateRoom
        [SerializeField] private Button buttonResetList; // Reload list
        [SerializeField] private Transform roomListContainer; // Container cho list
        [SerializeField] private GameObject roomItemPrefab; // Prefab cho 1 room item
        
        [Header("PanelCreateRoom - Tạo phòng")]
        [SerializeField] private Button buttonCloseCreateRoom; // Đóng popup
        [SerializeField] private InputField inputRoomName; // Nhập tên phòng
        [SerializeField] private Button buttonConfirmCreate; // Tạo phòng
        
        [Header("PanelJoinRoom - Trong phòng")]
        [SerializeField] private Text textRoomName; // Tên phòng
        [SerializeField] private Text textPlayerCount; // Số người (2/4)
        [SerializeField] private Transform playerListContainer; // Container cho list players
        [SerializeField] private GameObject playerItemPrefab; // Prefab cho 1 player item
        [SerializeField] private Button buttonLeaveRoom; // Quay lại PanelRoom
        [SerializeField] private Button buttonStartGame; // Start game (chỉ host)
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Lists
        private List<GameObject> roomListItems = new List<GameObject>();
        private List<GameObject> playerListItems = new List<GameObject>();

        private void Start()
        {
            SetupEventListeners();
            SubscribeToServices();
            
            // Hide all initially
            if (panelCustomRoom != null) panelCustomRoom.SetActive(false);
        }

        private void OnDestroy()
        {
            UnsubscribeFromServices();
        }

        #region Setup

        private void SetupEventListeners()
        {
            // Main
            if (buttonClosePanelCustomRoom != null)
                buttonClosePanelCustomRoom.onClick.AddListener(OnClosePanelCustomRoom);
            
            // PanelRoom
            if (buttonCreateRoom != null)
                buttonCreateRoom.onClick.AddListener(OnCreateRoomClicked);
            if (buttonResetList != null)
                buttonResetList.onClick.AddListener(OnResetListClicked);
            
            // PanelCreateRoom
            if (buttonCloseCreateRoom != null)
                buttonCloseCreateRoom.onClick.AddListener(OnCloseCreateRoomClicked);
            if (buttonConfirmCreate != null)
                buttonConfirmCreate.onClick.AddListener(OnConfirmCreateClicked);
            
            // PanelJoinRoom
            if (buttonLeaveRoom != null)
                buttonLeaveRoom.onClick.AddListener(OnLeaveRoomClicked);
            if (buttonStartGame != null)
                buttonStartGame.onClick.AddListener(OnStartGameClicked);
        }

        private void SubscribeToServices()
        {
            CustomLobbyService.OnLobbyCreated += OnLobbyCreated;
            CustomLobbyService.OnLobbyJoined += OnLobbyJoined;
            CustomLobbyService.OnLobbyLeft += OnLobbyLeft;
            CustomLobbyService.OnPlayersUpdated += OnPlayersUpdated;
            CustomLobbyService.OnGameStarting += OnGameStarting;
        }

        private void UnsubscribeFromServices()
        {
            CustomLobbyService.OnLobbyCreated -= OnLobbyCreated;
            CustomLobbyService.OnLobbyJoined -= OnLobbyJoined;
            CustomLobbyService.OnLobbyLeft -= OnLobbyLeft;
            CustomLobbyService.OnPlayersUpdated -= OnPlayersUpdated;
            CustomLobbyService.OnGameStarting -= OnGameStarting;
        }

        #endregion

        #region Public Methods (Called from PanelHome)

        /// <summary>
        /// Mở PanelCustomRoom (gọi từ PanelHome)
        /// </summary>
        public async void OpenCustomRoomPanel()
        {
            DebugLog("=== OpenCustomRoomPanel CALLED ===");

            // Sign in to UGS if needed
            if (!UGSAuthService.IsSignedIn)
            {
                DebugLog("UGS not signed in, signing in...");
                bool signedIn = await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
                if (!signedIn)
                {
                    DebugLogError("Failed to sign in to UGS");
                    return;
                }
                DebugLog("UGS signed in successfully");
            }

            // Show PanelCustomRoom
            if (panelCustomRoom == null)
            {
                DebugLogError("panelCustomRoom is NULL! Please assign in Inspector!");
                return;
            }

            DebugLog($"Setting panelCustomRoom active: {panelCustomRoom.name}");
            panelCustomRoom.SetActive(true);
            DebugLog($"panelCustomRoom.activeSelf: {panelCustomRoom.activeSelf}");

            // IMPORTANT: Force panelContainer active
            if (panelContainer == null)
            {
                DebugLogError("panelContainer is NULL! Please assign in Inspector!");
                return;
            }

            DebugLog($"Setting panelContainer active: {panelContainer.name}");
            panelContainer.SetActive(true);
            DebugLog($"panelContainer.activeSelf: {panelContainer.activeSelf}");

            // Show PanelRoom (mặc định)
            ShowPanelRoom();

            // Load room list
            await RefreshRoomList();

            DebugLog("=== OpenCustomRoomPanel COMPLETE ===");
        }

        #endregion

        #region Panel Navigation

        private void ShowPanelRoom()
        {
            DebugLog("=== ShowPanelRoom START ===");

            if (panelRoom == null)
            {
                DebugLogError("panelRoom is NULL!");
                return;
            }

            if (panelCreateRoom == null)
            {
                DebugLogError("panelCreateRoom is NULL!");
            }

            if (panelJoinRoom == null)
            {
                DebugLogError("panelJoinRoom is NULL!");
            }

            panelRoom.SetActive(true);
            if (panelCreateRoom != null) panelCreateRoom.SetActive(false);
            if (panelJoinRoom != null) panelJoinRoom.SetActive(false);

            DebugLog($"panelRoom.activeSelf: {panelRoom.activeSelf}");
            DebugLog($"panelRoom.activeInHierarchy: {panelRoom.activeInHierarchy}");

            // Check parent
            Transform parent = panelRoom.transform.parent;
            if (parent != null)
            {
                DebugLog($"panelRoom parent: {parent.name}, active: {parent.gameObject.activeSelf}");
            }

            DebugLog("=== ShowPanelRoom END ===");
        }

        private void ShowPanelCreateRoom()
        {
            // PanelCreateRoom mở overlay trên PanelRoom (không tắt PanelRoom)
            if (panelCreateRoom != null) panelCreateRoom.SetActive(true);
            
            DebugLog("Showing PanelCreateRoom");
        }

        private void ShowPanelJoinRoom()
        {
            // Ẩn 2 panel kia
            if (panelRoom != null) panelRoom.SetActive(false);
            if (panelCreateRoom != null) panelCreateRoom.SetActive(false);
            if (panelJoinRoom != null) panelJoinRoom.SetActive(true);
            
            DebugLog("Showing PanelJoinRoom");
        }

        private void OnClosePanelCustomRoom()
        {
            if (panelCustomRoom != null)
                panelCustomRoom.SetActive(false);
            
            DebugLog("Closed PanelCustomRoom");
        }

        #endregion

        #region PanelRoom Handlers

        private void OnCreateRoomClicked()
        {
            // Prevent if searching match
            if (AntKnow.Services.MatchmakerService.Instance.IsSearching)
            {
                DebugLogError("Cannot create room while searching for match");
                return;
            }

            ShowPanelCreateRoom();
        }

        private async void OnResetListClicked()
        {
            await RefreshRoomList();
        }

        private async System.Threading.Tasks.Task RefreshRoomList()
        {
            DebugLog("Refreshing room list...");

            // Debug check
            if (roomListContainer == null)
            {
                DebugLogError("roomListContainer is NULL! Please assign ScrollView/Viewport/Content to roomListContainer in Inspector!");
                return;
            }

            if (roomItemPrefab == null)
            {
                DebugLogError("roomItemPrefab is NULL! Please assign RoomItemPrefabs.prefab to roomItemPrefab in Inspector!");
                return;
            }

            DebugLog($"roomListContainer: {roomListContainer.name}");
            DebugLog($"roomItemPrefab: {roomItemPrefab.name}");

            // Clear old list
            foreach (var item in roomListItems)
            {
                if (item != null) Destroy(item);
            }
            roomListItems.Clear();

            // Query lobbies
            var lobbies = await CustomLobbyService.Instance.QueryLobbiesAsync();

            if (lobbies != null && roomListContainer != null && roomItemPrefab != null)
            {
                DebugLog($"Found {lobbies.Count} lobbies");

                foreach (var lobby in lobbies)
                {
                    GameObject item = Instantiate(roomItemPrefab, roomListContainer);
                    roomListItems.Add(item);

                    // Fix RectTransform để hiện trong ScrollView
                    var rectTransform = item.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.localScale = Vector3.one;
                        rectTransform.localPosition = Vector3.zero;
                    }

                    DebugLog($"Spawned room item: {lobby.Name} at parent: {roomListContainer.name}");

                    // Setup item: RoomItemPrefab có 2 text con
                    // Hỗ trợ cả Text và TextMeshProUGUI
                    var allTexts = TextHelper.GetAllTexts(item);

                    if (allTexts.Count >= 2)
                    {
                        // Text 1: Tên phòng
                        if (allTexts[0] is TextMeshProUGUI tmp1)
                            tmp1.text = lobby.Name;
                        else if (allTexts[0] is Text ui1)
                            ui1.text = lobby.Name;

                        // Text 2: Số người (1/4)
                        if (allTexts[1] is TextMeshProUGUI tmp2)
                            tmp2.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                        else if (allTexts[1] is Text ui2)
                            ui2.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                    }
                    else if (allTexts.Count == 1)
                    {
                        // Fallback: Nếu chỉ có 1 text, hiện cả 2 thông tin
                        string fullText = $"{lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})";
                        if (allTexts[0] is TextMeshProUGUI tmp)
                            tmp.text = fullText;
                        else if (allTexts[0] is Text ui)
                            ui.text = fullText;
                    }

                    var button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        string lobbyId = lobby.Id;
                        button.onClick.AddListener(() => OnRoomItemClicked(lobbyId));
                    }
                }
            }
        }

        private async void OnRoomItemClicked(string lobbyId)
        {
            DebugLog($"Room clicked: {lobbyId}");

            // Prevent if searching match
            if (AntKnow.Services.MatchmakerService.Instance.IsSearching)
            {
                DebugLogError("Cannot join room while searching for match");
                return;
            }

            // Join lobby
            bool joined = await CustomLobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            if (joined)
            {
                // Event OnLobbyJoined sẽ xử lý việc chuyển panel
            }
        }

        #endregion

        #region PanelCreateRoom Handlers

        private void OnCloseCreateRoomClicked()
        {
            if (panelCreateRoom != null)
                panelCreateRoom.SetActive(false);
        }

        private async void OnConfirmCreateClicked()
        {
            // Prevent if searching match
            if (AntKnow.Services.MatchmakerService.Instance.IsSearching)
            {
                DebugLogError("Cannot create room while searching for match");
                return;
            }

            string roomName = inputRoomName != null ? inputRoomName.text : "";
            if (string.IsNullOrEmpty(roomName))
                roomName = $"Room_{UnityEngine.Random.Range(1000, 9999)}";

            DebugLog($"Creating room: {roomName}");

            // Check if already in lobby
            if (CustomLobbyService.Instance.IsInLobby)
            {
                DebugLogError("Already in a lobby! Leaving current lobby first...");
                await CustomLobbyService.Instance.LeaveLobbyAsync();
                await System.Threading.Tasks.Task.Delay(500); // Wait for cleanup
            }

            // Create lobby
            bool created = await CustomLobbyService.Instance.CreateLobbyAsync(roomName, isPrivate: false);

            if (created)
            {
                // Event OnLobbyCreated sẽ xử lý việc chuyển panel
                if (panelCreateRoom != null)
                    panelCreateRoom.SetActive(false);
            }
            else
            {
                DebugLogError("Failed to create lobby");
            }
        }

        #endregion

        #region PanelJoinRoom Handlers

        private void OnLobbyCreated(Lobby lobby)
        {
            DebugLog($"Lobby created: {lobby.Name}");
            ShowPanelJoinRoom();
            UpdateJoinRoomUI(lobby);
        }

        private void OnLobbyJoined(Lobby lobby)
        {
            DebugLog($"Lobby joined: {lobby.Name}");
            ShowPanelJoinRoom();
            UpdateJoinRoomUI(lobby);
        }

        private void OnLobbyLeft()
        {
            DebugLog("Left lobby");
            ShowPanelRoom();
            RefreshRoomList();
        }

        private void OnPlayersUpdated(List<Player> players)
        {
            DebugLog($"Players updated: {players.Count}");

            // Update player count with status
            if (textPlayerCount != null)
            {
                string status = GetLobbyStatus(players.Count, GameConfig.MAX_PLAYERS);
                textPlayerCount.text = $"{players.Count}/{GameConfig.MAX_PLAYERS} - {status}";
            }

            // Update start button
            bool isHost = CustomLobbyService.Instance.IsHost;
            if (isHost && buttonStartGame != null)
            {
                int currentPlayers = players.Count;
                buttonStartGame.interactable = (currentPlayers >= 2);

                // Update button text
                var buttonText = buttonStartGame.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    if (currentPlayers >= GameConfig.MAX_PLAYERS)
                        buttonText.text = "Bắt đầu (Đủ người)";
                    else if (currentPlayers >= 2)
                        buttonText.text = $"Bắt đầu ({currentPlayers}/{GameConfig.MAX_PLAYERS})";
                    else
                        buttonText.text = "Chờ thêm người...";
                }
            }

            UpdatePlayerList(players);
        }

        private void UpdateJoinRoomUI(Lobby lobby)
        {
            // Update room name
            if (textRoomName != null)
                textRoomName.text = lobby.Name;

            // Update player count with status
            if (textPlayerCount != null)
            {
                int currentPlayers = lobby.Players.Count;
                int maxPlayers = lobby.MaxPlayers;
                string status = GetLobbyStatus(currentPlayers, maxPlayers);
                textPlayerCount.text = $"{currentPlayers}/{maxPlayers} - {status}";
            }

            // Show/hide start button (chỉ host)
            bool isHost = CustomLobbyService.Instance.IsHost;
            if (buttonStartGame != null)
            {
                buttonStartGame.gameObject.SetActive(isHost);

                // Enable start button nếu đủ min 2 players
                int currentPlayers = lobby.Players.Count;
                int maxPlayers = lobby.MaxPlayers;
                buttonStartGame.interactable = (currentPlayers >= 2);

                // Update button text
                var buttonText = buttonStartGame.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    if (currentPlayers >= maxPlayers)
                        buttonText.text = "Bắt đầu (Đủ người)";
                    else if (currentPlayers >= 2)
                        buttonText.text = $"Bắt đầu ({currentPlayers}/{maxPlayers})";
                    else
                        buttonText.text = "Chờ thêm người...";
                }
            }

            UpdatePlayerList(lobby.Players);
        }

        /// <summary>
        /// Get lobby status text
        /// </summary>
        private string GetLobbyStatus(int current, int max)
        {
            if (current >= max)
                return "Đủ người";
            else if (current >= 2)
                return "Có thể bắt đầu";
            else
                return "Chờ thêm người";
        }

        private void UpdatePlayerList(List<Player> players)
        {
            // Debug check
            if (playerListContainer == null)
            {
                DebugLogError("playerListContainer is NULL! Please assign ScrollView/Viewport/Content to playerListContainer in Inspector!");
                return;
            }

            if (playerItemPrefab == null)
            {
                DebugLogError("playerItemPrefab is NULL! Please assign PlayerItemPrefabs.prefab to playerItemPrefab in Inspector!");
                return;
            }

            // Clear old list
            foreach (var item in playerListItems)
            {
                if (item != null) Destroy(item);
            }
            playerListItems.Clear();

            DebugLog($"Updating player list: {players.Count} players");

            // Create new list
            if (playerListContainer != null && playerItemPrefab != null)
            {
                foreach (var player in players)
                {
                    GameObject item = Instantiate(playerItemPrefab, playerListContainer);
                    playerListItems.Add(item);

                    // Fix RectTransform để hiện trong ScrollView
                    var rectTransform = item.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.localScale = Vector3.one;
                        rectTransform.localPosition = Vector3.zero;
                    }

                    // PlayerItemPrefab: Button với 1 text con (tên người chơi)
                    // Hỗ trợ cả Text và TextMeshProUGUI
                    string playerName = "Player";
                    if (player.Data != null && player.Data.ContainsKey("PlayerName"))
                        playerName = player.Data["PlayerName"].Value;

                    // Try TextMeshProUGUI first
                    var tmpText = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        tmpText.text = playerName;
                    }
                    else
                    {
                        // Fallback to Unity UI Text
                        var uiText = item.GetComponentInChildren<Text>();
                        if (uiText != null)
                        {
                            uiText.text = playerName;
                        }
                    }

                    // Disable button click (chỉ hiển thị, không tương tác)
                    var button = item.GetComponent<Button>();
                    if (button != null)
                        button.interactable = false;
                }
            }
        }

        private async void OnLeaveRoomClicked()
        {
            DebugLog("Leave room clicked");
            await CustomLobbyService.Instance.LeaveLobbyAsync();
        }

        private async void OnStartGameClicked()
        {
            DebugLog("Start game clicked");
            
            if (CustomLobbyService.Instance.IsHost)
            {
                // Check minimum players
                int playerCount = CustomLobbyService.Instance.CurrentLobby?.Players.Count ?? 0;
                if (playerCount < GameConfig.MIN_PLAYERS)
                {
                    DebugLogError($"Not enough players: {playerCount}/{GameConfig.MIN_PLAYERS}");
                    return;
                }
                
                await CustomLobbyService.Instance.StartGameAsync();
            }
        }

        private async void OnGameStarting(string relayJoinCode)
        {
            DebugLog($"Game starting with Relay code: {relayJoinCode}");

            // Setup GameSessionData
            var sessionData = GameSessionData.Instance;
            sessionData.SetFromGameDataManager();
            sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);

            bool isHost = CustomLobbyService.Instance.IsHost;
            string lobbyId = CustomLobbyService.Instance.CurrentLobby?.Id;
            sessionData.SetNetworkInfo(relayJoinCode, isHost, lobbyId);

            // Join Relay để setup transport (nhưng KHÔNG start NetworkManager ở đây)
            if (isHost)
            {
                // Host: Create relay (đã tạo rồi trong StartGameAsync)
                // Không cần làm gì ở đây
                DebugLog("Host ready, relay already created");
            }
            else
            {
                // Client: Join relay để setup transport
                await RelayService.Instance.JoinRelayAsync(relayJoinCode);
                DebugLog("Client joined relay, transport configured");
            }

            // Load LoadingScene → GameScene
            // GameScene sẽ tự động StartHost/StartClient khi load xong
            LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
        }

        #endregion

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[LobbyUIManager] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[LobbyUIManager] {message}");
            }
        }

        #endregion
    }
}

