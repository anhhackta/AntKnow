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
    /// Service quản lý Custom Lobby - tạo phòng riêng và tham gia
    /// </summary>
    public class CustomLobbyService : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Events
        public static event Action<Lobby> OnLobbyCreated;
        public static event Action<Lobby> OnLobbyJoined;
        public static event Action OnLobbyLeft;
        public static event Action<string> OnLobbyError;
        public static event Action<List<Player>> OnPlayersUpdated;
        public static event Action<string> OnGameStarting; // Relay join code
        public static event Action<Player> OnPlayerJoined;
        public static event Action<Player> OnPlayerLeft;

        // Properties
        public Lobby CurrentLobby { get; private set; }
        public bool IsInLobby => CurrentLobby != null;
        public bool IsHost => CurrentLobby != null && CurrentLobby.HostId == UGSAuthService.PlayerId;
        public List<Player> Players => CurrentLobby?.Players ?? new List<Player>();

        private static CustomLobbyService _instance;
        public static CustomLobbyService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CustomLobbyService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CustomLobbyService");
                        _instance = go.AddComponent<CustomLobbyService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Coroutine heartbeatCoroutine;
        private Coroutine updateCoroutine;

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
        /// Tạo lobby mới
        /// </summary>
        public async Task<bool> CreateLobbyAsync(string lobbyName, bool isPrivate = false)
        {
            try
            {
                // Kiểm tra UGS authentication
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to create lobby");
                    OnLobbyError?.Invoke("Chưa đăng nhập UGS");
                    return false;
                }

                if (IsInLobby)
                {
                    DebugLogError("Already in a lobby");
                    OnLobbyError?.Invoke("Đã ở trong lobby");
                    return false;
                }

                DebugLog($"Creating lobby: {lobbyName}");

                var gameDataManager = GameDataManager.Instance;
                
                var createOptions = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentIngameName ?? gameDataManager.currentUsername) },
                            { "Level", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentLevel.ToString()) },
                            { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "false") }
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CustomRoom") },
                        { "CreatedAt", new DataObject(DataObject.VisibilityOptions.Public, DateTime.UtcNow.ToString()) },
                        { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "false") }
                    }
                };

                var lobby = await Unity.Services.Lobbies.LobbyService.Instance.CreateLobbyAsync(lobbyName, GameConfig.MAX_PLAYERS, createOptions);
                
                CurrentLobby = lobby;
                StartLobbyCoroutines();
                
                DebugLog($"Lobby created successfully: {lobby.Name} (ID: {lobby.Id})");
                OnLobbyCreated?.Invoke(lobby);
                OnPlayersUpdated?.Invoke(lobby.Players);
                
                return true;
            }
            catch (LobbyServiceException e)
            {
                DebugLogError($"Failed to create lobby: {e.Message}");
                OnLobbyError?.Invoke($"Không thể tạo phòng: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                DebugLogError($"Unexpected error creating lobby: {e.Message}");
                OnLobbyError?.Invoke($"Lỗi không xác định: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Query danh sách lobbies public
        /// </summary>
        public async Task<List<Lobby>> QueryLobbiesAsync()
        {
            try
            {
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to query lobbies");
                    return null;
                }

                DebugLog("Querying public lobbies...");

                var queryOptions = new QueryLobbiesOptions
                {
                    Count = GameConfig.LOBBY_QUERY_COUNT,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.GE),
                        new QueryFilter(QueryFilter.FieldOptions.MaxPlayers, GameConfig.MAX_PLAYERS.ToString(), QueryFilter.OpOptions.EQ)
                    }
                };

                var queryResponse = await Unity.Services.Lobbies.LobbyService.Instance.QueryLobbiesAsync(queryOptions);

                DebugLog($"Found {queryResponse.Results.Count} lobbies");
                return queryResponse.Results;
            }
            catch (LobbyServiceException e)
            {
                DebugLogError($"Failed to query lobbies: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tham gia lobby bằng ID
        /// </summary>
        public async Task<bool> JoinLobbyByIdAsync(string lobbyId)
        {
            try
            {
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to join lobby");
                    OnLobbyError?.Invoke("Chưa đăng nhập UGS");
                    return false;
                }

                if (IsInLobby)
                {
                    DebugLogError("Already in a lobby");
                    OnLobbyError?.Invoke("Đã ở trong lobby");
                    return false;
                }

                DebugLog($"Joining lobby with ID: {lobbyId}");

                var gameDataManager = GameDataManager.Instance;

                var joinOptions = new JoinLobbyByIdOptions
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentIngameName ?? gameDataManager.currentUsername) },
                            { "Level", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentLevel.ToString()) }
                        }
                    }
                };

                var lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);

                CurrentLobby = lobby;
                StartLobbyCoroutines();

                DebugLog($"Joined lobby successfully: {lobby.Name}");
                OnLobbyJoined?.Invoke(lobby);
                OnPlayersUpdated?.Invoke(lobby.Players);

                return true;
            }
            catch (LobbyServiceException e)
            {
                DebugLogError($"Failed to join lobby: {e.Message}");
                OnLobbyError?.Invoke($"Không thể tham gia: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tham gia lobby bằng code
        /// </summary>
        public async Task<bool> JoinLobbyByCodeAsync(string lobbyCode)
        {
            try
            {
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to join lobby");
                    OnLobbyError?.Invoke("Chưa đăng nhập UGS");
                    return false;
                }

                if (IsInLobby)
                {
                    DebugLogError("Already in a lobby");
                    OnLobbyError?.Invoke("Đã ở trong lobby");
                    return false;
                }

                DebugLog($"Joining lobby with code: {lobbyCode}");

                var gameDataManager = GameDataManager.Instance;
                
                var joinOptions = new JoinLobbyByCodeOptions
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentIngameName ?? gameDataManager.currentUsername) },
                            { "Level", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, gameDataManager.currentLevel.ToString()) },
                            { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "false") }
                        }
                    }
                };

                var lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
                
                CurrentLobby = lobby;
                StartLobbyCoroutines();
                
                DebugLog($"Joined lobby successfully: {lobby.Name}");
                OnLobbyJoined?.Invoke(lobby);
                OnPlayersUpdated?.Invoke(lobby.Players);
                
                return true;
            }
            catch (LobbyServiceException e)
            {
                DebugLogError($"Failed to join lobby: {e.Message}");
                OnLobbyError?.Invoke($"Không thể tham gia phòng: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                DebugLogError($"Unexpected error joining lobby: {e.Message}");
                OnLobbyError?.Invoke($"Lỗi không xác định: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Rời khỏi lobby
        /// </summary>
        public async Task<bool> LeaveLobbyAsync()
        {
            try
            {
                if (!IsInLobby)
                {
                    DebugLogError("Not in a lobby");
                    return false;
                }

                DebugLog("Leaving lobby...");

                string lobbyId = CurrentLobby.Id;
                
                StopLobbyCoroutines();
                
                await Unity.Services.Lobbies.LobbyService.Instance.RemovePlayerAsync(lobbyId, UGSAuthService.PlayerId);
                
                CurrentLobby = null;
                
                DebugLog("Left lobby successfully");
                OnLobbyLeft?.Invoke();
                
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Error leaving lobby: {e.Message}");
                OnLobbyError?.Invoke($"Lỗi rời phòng: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái ready của player
        /// </summary>
        public async Task<bool> SetPlayerReadyAsync(bool isReady)
        {
            try
            {
                if (!IsInLobby)
                {
                    DebugLogError("Not in a lobby");
                    return false;
                }

                DebugLog($"Setting player ready status: {isReady}");

                var updateOptions = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, isReady.ToString().ToLower()) }
                    }
                };

                var updatedLobby = await Unity.Services.Lobbies.LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, UGSAuthService.PlayerId, updateOptions);
                CurrentLobby = updatedLobby;
                
                OnPlayersUpdated?.Invoke(updatedLobby.Players);
                
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Error setting player ready: {e.Message}");
                OnLobbyError?.Invoke($"Lỗi cập nhật trạng thái: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Start game (chỉ host mới được gọi)
        /// </summary>
        public async Task<bool> StartGameAsync()
        {
            try
            {
                if (!IsHost)
                {
                    DebugLogError("Only host can start the game");
                    OnLobbyError?.Invoke("Chỉ chủ phòng mới có thể bắt đầu game");
                    return false;
                }

                // Check if all players are ready
                bool allReady = true;
                foreach (var player in CurrentLobby.Players)
                {
                    if (player.Data.TryGetValue("IsReady", out var readyData))
                    {
                        if (readyData.Value != "true")
                        {
                            allReady = false;
                            break;
                        }
                    }
                    else
                    {
                        allReady = false;
                        break;
                    }
                }

                if (!allReady)
                {
                    OnLobbyError?.Invoke("Tất cả người chơi phải sẵn sàng");
                    return false;
                }

                DebugLog("Starting game...");

                // Create Relay allocation (no parameters - uses GameConfig.RELAY_MAX_CONNECTIONS)
                var relayJoinCode = await RelayService.Instance.CreateRelayAsync();

                if (string.IsNullOrEmpty(relayJoinCode))
                {
                    OnLobbyError?.Invoke("Không thể tạo Relay");
                    return false;
                }

                // Update lobby with relay code
                var updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
                        { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") }
                    }
                };

                await Unity.Services.Lobbies.LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateOptions);
                
                DebugLog($"Game started with Relay code: {relayJoinCode}");
                OnGameStarting?.Invoke(relayJoinCode);
                
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Error starting game: {e.Message}");
                OnLobbyError?.Invoke($"Lỗi bắt đầu game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Bắt đầu các coroutines cho lobby
        /// </summary>
        private void StartLobbyCoroutines()
        {
            StopLobbyCoroutines();
            
            if (IsHost)
            {
                heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
            }
            
            updateCoroutine = StartCoroutine(UpdateLobbyCoroutine());
        }

        /// <summary>
        /// Dừng các coroutines
        /// </summary>
        private void StopLobbyCoroutines()
        {
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
            
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }

        /// <summary>
        /// Heartbeat để giữ lobby sống (chỉ host)
        /// </summary>
        private IEnumerator HeartbeatCoroutine()
        {
            while (IsInLobby && IsHost)
            {
                yield return new WaitForSeconds(GameConfig.LOBBY_HEARTBEAT_INTERVAL);

                // Use Task to handle async operation in coroutine
                var heartbeatTask = SendHeartbeatAsync();
                yield return new WaitUntil(() => heartbeatTask.IsCompleted);

                if (heartbeatTask.Exception != null || !heartbeatTask.Result)
                {
                    DebugLogError($"Heartbeat failed");
                    break;
                }
            }
        }

        /// <summary>
        /// Send heartbeat async
        /// </summary>
        private async Task<bool> SendHeartbeatAsync()
        {
            try
            {
                await Unity.Services.Lobbies.LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                DebugLog("Heartbeat sent");
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Heartbeat failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật thông tin lobby
        /// </summary>
        private IEnumerator UpdateLobbyCoroutine()
        {
            while (IsInLobby)
            {
                yield return new WaitForSeconds(GameConfig.LOBBY_UPDATE_INTERVAL);

                // Use Task to handle async operation in coroutine
                var updateTask = UpdateLobbyInfoAsync();
                yield return new WaitUntil(() => updateTask.IsCompleted);

                if (updateTask.Exception != null || !updateTask.Result)
                {
                    break;
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
                var updatedLobby = await Unity.Services.Lobbies.LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);

                // Check for player changes
                var oldPlayerCount = CurrentLobby.Players.Count;
                var newPlayerCount = updatedLobby.Players.Count;

                CurrentLobby = updatedLobby;
                OnPlayersUpdated?.Invoke(updatedLobby.Players);

                // Check if game started
                if (updatedLobby.Data.TryGetValue("GameStarted", out var gameStartedData) && gameStartedData.Value == "true")
                {
                    if (updatedLobby.Data.TryGetValue("RelayJoinCode", out var relayData))
                    {
                        DebugLog("Game starting, received Relay code");
                        OnGameStarting?.Invoke(relayData.Value);
                        return false; // Stop updating
                    }
                }

                return true; // Continue updating
            }
            catch (Exception e)
            {
                DebugLogError($"Error updating lobby: {e.Message}");
                return false;
            }
        }

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[CustomLobbyService] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[CustomLobbyService] {message}");
            }
        }

        #endregion

        private void OnDestroy()
        {
            StopLobbyCoroutines();
        }
    }
}
