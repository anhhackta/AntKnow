using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using AntKnow.Auth;

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
        public float RemainingSearchTime { get; private set; }
        public Lobby CurrentMatch { get; private set; }

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

                DebugLog("Starting matchmaking...");
                IsSearching = true;
                RemainingSearchTime = GameConfig.MATCHMAKING_TIMEOUT;
                
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
            while (IsSearching && RemainingSearchTime > 0)
            {
                // Try to find or create a match
                yield return StartCoroutine(TryFindMatchCoroutine());
                
                if (!IsSearching) break;
                
                // Wait before retry
                yield return new WaitForSeconds(GameConfig.MATCHMAKING_RETRY_INTERVAL);
            }

            // Timeout
            if (IsSearching)
            {
                DebugLogError("Matchmaking timeout");
                OnMatchmakingError?.Invoke("Hết thời gian tìm trận");
                CancelMatchmaking();
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
                        await OnMatchJoined(joinedLobby);
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
                await OnMatchJoined(lobby);
                
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
        private async Task OnMatchJoined(Lobby lobby)
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
            
            OnMatchFound?.Invoke(lobby);
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

                // Check if lobby is full
                if (updatedLobby.Players.Count >= updatedLobby.MaxPlayers)
                {
                    DebugLog("Lobby is full, starting game...");
                    // Game will be started by lobby host
                    return false;
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
        /// Countdown timer
        /// </summary>
        private IEnumerator CountdownCoroutine()
        {
            while (IsSearching && RemainingSearchTime > 0)
            {
                OnSearchTimeUpdated?.Invoke(RemainingSearchTime);
                yield return new WaitForSeconds(1f);
                RemainingSearchTime -= 1f;
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
