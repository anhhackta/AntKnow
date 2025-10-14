using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using AntKnow.Auth;
using AntKnow.Game;

namespace AntKnow.Services
{
    /// <summary>
    /// Service quản lý matchmaking - tìm trận tự động
    /// </summary>
    public class MatchmakerService : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Events
        public static event Action OnMatchmakingStarted;
        public static event Action OnMatchmakingCancelled;
        public static event Action<string> OnMatchmakingError;
        public static event Action<float> OnSearchTimeUpdated; // Remaining time
        public static event Action<Lobby> OnMatchFound;
        public static event Action<int, int> OnPlayersCountUpdated; // current, max

        // Properties
        public bool IsSearching { get; private set; }
        public float ElapsedSearchTime { get; private set; } // Thời gian ĐÃ TÌM (đếm lên)
        public Lobby CurrentMatch { get; private set; }

        // Auto start timer (30s sau khi đủ 2 người)
        private float autoStartTimer = 0f;
        private bool isWaitingForAutoStart = false;
        private const float AUTO_START_DELAY = 30f; // 30 giây

        private static MatchmakerService _instance;
        public static MatchmakerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MatchmakerService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MatchmakerService");
                        _instance = go.AddComponent<MatchmakerService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Coroutine searchCoroutine;
        private Coroutine countdownCoroutine;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Bắt đầu tìm trận
        /// </summary>
        public async Task<bool> StartMatchmakingAsync()
        {
            try
            {
                // Kiểm tra UGS authentication
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to start matchmaking");
                    OnMatchmakingError?.Invoke("Chưa đăng nhập UGS");
                    return false;
                }

                if (IsSearching)
                {
                    DebugLogError("Already searching for a match");
                    return false;
                }

                // IMPORTANT: Leave any existing lobby first
                await LeaveCurrentLobbyAsync();

                DebugLog("Starting matchmaking...");
                IsSearching = true;
                ElapsedSearchTime = 0f; // Reset elapsed time

                OnMatchmakingStarted?.Invoke();

                // Start search coroutine
                searchCoroutine = StartCoroutine(SearchForMatchCoroutine());
                countdownCoroutine = StartCoroutine(CountdownCoroutine());

                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to start matchmaking: {e.Message}");
                OnMatchmakingError?.Invoke($"Lỗi tìm trận: {e.Message}");
                IsSearching = false;
                return false;
            }
        }

        /// <summary>
        /// Hủy tìm trận
        /// </summary>
        public void CancelMatchmaking()
        {
            try
            {
                DebugLog("Cancelling matchmaking...");
                
                IsSearching = false;
                
                if (searchCoroutine != null)
                {
                    StopCoroutine(searchCoroutine);
                    searchCoroutine = null;
                }
                
                if (countdownCoroutine != null)
                {
                    StopCoroutine(countdownCoroutine);
                    countdownCoroutine = null;
                }

                OnMatchmakingCancelled?.Invoke();
                DebugLog("Matchmaking cancelled");
            }
            catch (Exception e)
            {
                DebugLogError($"Error cancelling matchmaking: {e.Message}");
            }
        }

        /// <summary>
        /// Coroutine tìm trận
        /// </summary>
        private IEnumerator SearchForMatchCoroutine()
        {
            while (IsSearching)
            {
                // Try to find or create a match
                yield return StartCoroutine(TryFindMatchCoroutine());

                if (!IsSearching) break;

                // Wait before retry
                yield return new WaitForSeconds(GameConfig.MATCHMAKING_RETRY_INTERVAL);
            }
        }

        /// <summary>
        /// Thử tìm trận
        /// </summary>
        private IEnumerator TryFindMatchCoroutine()
        {
            Task<bool> findTask = TryFindMatchAsync();
            yield return new WaitUntil(() => findTask.IsCompleted);
            
            if (findTask.Exception != null)
            {
                DebugLogError($"Error finding match: {findTask.Exception.Message}");
            }
        }

        /// <summary>
        /// Tìm hoặc tạo trận đấu
        /// </summary>
        private async Task<bool> TryFindMatchAsync()
        {
            try
            {
                DebugLog("Searching for available matches...");

                // Try to join existing lobby first
                var queryOptions = new QueryLobbiesOptions
                {
                    Count = GameConfig.LOBBY_QUERY_COUNT,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.GE),
                        new QueryFilter(QueryFilter.FieldOptions.MaxPlayers, GameConfig.MAX_PLAYERS.ToString(), QueryFilter.OpOptions.EQ)
                    }
                };

                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
                
                if (queryResponse.Results.Count > 0)
                {
                    // Found available lobby, try to join
                    var targetLobby = queryResponse.Results[0];
                    DebugLog($"Found available lobby: {targetLobby.Name} ({targetLobby.Players.Count}/{targetLobby.MaxPlayers})");

                    try
                    {
                        var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(targetLobby.Id);
                        await OnMatchJoined(joinedLobby, isJoining: true); // TRUE = Join lobby có sẵn
                        return true;
                    }
                    catch (LobbyServiceException e)
                    {
                        DebugLogError($"Failed to join lobby: {e.Message}");
                        // Continue to create new lobby
                    }
                }

                // No available lobby found, create new one
                DebugLog("No available matches found, creating new lobby...");
                await CreateNewMatchLobby();
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Error in TryFindMatchAsync: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tạo lobby mới cho matchmaking
        /// </summary>
        private async Task CreateNewMatchLobby()
        {
            try
            {
                var gameDataManager = GameDataManager.Instance;
                string lobbyName = $"Match_{DateTime.Now:HHmmss}";
                
                var createOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentIngameName ?? gameDataManager.currentUsername) },
                            { "Level", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentLevel.ToString()) }
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Matchmaking") },
                        { "CreatedAt", new DataObject(DataObject.VisibilityOptions.Public, DateTime.UtcNow.ToString()) }
                    }
                };

                var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameConfig.MAX_PLAYERS, createOptions);
                await OnMatchJoined(lobby, isJoining: false); // FALSE = Tạo lobby mới

                DebugLog($"Created new match lobby: {lobby.Name}");
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to create match lobby: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xử lý khi tham gia trận thành công
        /// </summary>
        /// <param name="lobby">Lobby đã join</param>
        /// <param name="isJoining">TRUE = Join lobby có sẵn, FALSE = Tạo lobby mới</param>
        private async Task OnMatchJoined(Lobby lobby, bool isJoining)
        {
            CurrentMatch = lobby;
            IsSearching = false;

            // Stop search coroutines
            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
                searchCoroutine = null;
            }

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            DebugLog($"Joined match: {lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})");

            // CHỈ fire OnMatchFound khi JOIN lobby có sẵn (có người khác)
            // KHÔNG fire khi TẠO lobby mới (1 mình)
            if (isJoining)
            {
                DebugLog("Match found! Joined existing lobby.");
                OnMatchFound?.Invoke(lobby);
            }
            else
            {
                DebugLog("Created new lobby, waiting for other players...");
            }

            OnPlayersCountUpdated?.Invoke(lobby.Players.Count, lobby.MaxPlayers);

            // Start monitoring lobby for player changes
            StartCoroutine(MonitorLobbyCoroutine());
        }

        /// <summary>
        /// Monitor lobby để cập nhật số lượng người chơi
        /// </summary>
        private IEnumerator MonitorLobbyCoroutine()
        {
            while (CurrentMatch != null)
            {
                yield return new WaitForSeconds(2f);

                // Use Task to handle async operation in coroutine
                var updateTask = UpdateLobbyInfoAsync();
                yield return new WaitUntil(() => updateTask.IsCompleted);

                if (updateTask.Exception != null)
                {
                    DebugLogError($"Error monitoring lobby: {updateTask.Exception.Message}");
                    break;
                }

                if (!updateTask.Result)
                {
                    break; // Lobby update failed or game started
                }
            }
        }

        /// <summary>
        /// Update lobby info async
        /// </summary>
        private async Task<bool> UpdateLobbyInfoAsync()
        {
            try
            {
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentMatch.Id);
                CurrentMatch = updatedLobby;

                OnPlayersCountUpdated?.Invoke(updatedLobby.Players.Count, updatedLobby.MaxPlayers);

                int playerCount = updatedLobby.Players.Count;
                int maxPlayers = updatedLobby.MaxPlayers;
                bool isHost = updatedLobby.HostId == UGSAuthService.PlayerId;

                // Check if game started (relay code exists)
                if (updatedLobby.Data != null && updatedLobby.Data.ContainsKey("GameStarted"))
                {
                    string gameStarted = updatedLobby.Data["GameStarted"].Value;
                    if (gameStarted == "true")
                    {
                        DebugLog("Game started by host, joining...");

                        // Get relay code
                        if (updatedLobby.Data.ContainsKey("RelayJoinCode"))
                        {
                            string relayJoinCode = updatedLobby.Data["RelayJoinCode"].Value;

                            if (!isHost)
                            {
                                // Client: Setup GameSessionData
                                var sessionData = GameSessionData.Instance;
                                sessionData.SetFromGameDataManager();
                                sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);
                                sessionData.SetNetworkInfo(relayJoinCode, host: false, updatedLobby.Id);

                                // Client: Join relay to configure transport
                                await RelayService.Instance.JoinRelayAsync(relayJoinCode);
                                DebugLog("Client joined relay, transport configured");
                            }

                            // Load LoadingScene → GameScene
                            LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
                        }

                        return false;
                    }
                }

                // MATCHMAKER AUTO START LOGIC (Host only)
                if (isHost)
                {
                    // Đủ 4 người → Start ngay
                    if (playerCount >= maxPlayers)
                    {
                        DebugLog($"Lobby full ({playerCount}/{maxPlayers}), auto starting game...");
                        await AutoStartGameAsync();
                        return false;
                    }

                    // Đủ 2-3 người → Bắt đầu đếm 30s
                    if (playerCount >= 2)
                    {
                        if (!isWaitingForAutoStart)
                        {
                            // Bắt đầu đếm ngược 30s
                            isWaitingForAutoStart = true;
                            autoStartTimer = AUTO_START_DELAY;
                            DebugLog($"Match ready ({playerCount}/{maxPlayers}), waiting {AUTO_START_DELAY}s for more players...");
                        }
                        else
                        {
                            // Đang đếm ngược
                            autoStartTimer -= 2f; // Update mỗi 2s (theo MonitorLobbyCoroutine)
                            DebugLog($"Auto start in {autoStartTimer:F0}s ({playerCount}/{maxPlayers})");

                            // Hết thời gian → Auto start
                            if (autoStartTimer <= 0)
                            {
                                DebugLog($"Auto start timer expired, starting game with {playerCount} players...");
                                await AutoStartGameAsync();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // Chưa đủ 2 người → Reset timer
                        isWaitingForAutoStart = false;
                        autoStartTimer = 0f;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Error updating lobby info: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Auto start game (Matchmaker only)
        /// </summary>
        private async Task AutoStartGameAsync()
        {
            try
            {
                DebugLog("Auto starting matchmaker game...");

                // Fire OnMatchFound event → Hiện "Match Found" notification
                OnMatchFound?.Invoke(CurrentMatch);

                // Create Relay
                string relayJoinCode = await RelayService.Instance.CreateRelayAsync();
                if (string.IsNullOrEmpty(relayJoinCode))
                {
                    DebugLogError("Failed to create relay");
                    return;
                }

                // Update lobby with relay code
                var updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) },
                        { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") }
                    }
                };

                await LobbyService.Instance.UpdateLobbyAsync(CurrentMatch.Id, updateOptions);

                DebugLog($"Game starting with relay code: {relayJoinCode}");

                // Setup GameSessionData (Host)
                var sessionData = GameSessionData.Instance;
                sessionData.SetFromGameDataManager();
                sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);
                sessionData.SetNetworkInfo(relayJoinCode, host: true, CurrentMatch.Id);

                // Wait 2s để user thấy notification
                await Task.Delay(2000);

                // Load LoadingScene → GameScene
                LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to auto start game: {e.Message}");
            }
        }

        /// <summary>
        /// Leave current lobby if any
        /// </summary>
        private async Task LeaveCurrentLobbyAsync()
        {
            try
            {
                // Get joined lobbies
                var joinedLobbies = await LobbyService.Instance.GetJoinedLobbiesAsync();

                if (joinedLobbies != null && joinedLobbies.Count > 0)
                {
                    foreach (var lobbyId in joinedLobbies)
                    {
                        try
                        {
                            await LobbyService.Instance.RemovePlayerAsync(lobbyId, UGSAuthService.PlayerId);
                            DebugLog($"Left lobby: {lobbyId}");
                        }
                        catch (Exception e)
                        {
                            DebugLogError($"Failed to leave lobby {lobbyId}: {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to get joined lobbies: {e.Message}");
            }
        }

        /// <summary>
        /// Elapsed timer - Đếm thời gian ĐÃ TÌM (đếm lên)
        /// </summary>
        private IEnumerator CountdownCoroutine()
        {
            ElapsedSearchTime = 0f;

            while (IsSearching)
            {
                OnSearchTimeUpdated?.Invoke(ElapsedSearchTime);
                yield return new WaitForSeconds(1f);
                ElapsedSearchTime += 1f; // Đếm LÊN
            }
        }

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[MatchmakerService] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[MatchmakerService] {message}");
            }
        }

        #endregion
    }
}
