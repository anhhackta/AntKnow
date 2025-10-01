using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using AntKnow.Auth;
using AntKnow.Services;

namespace AntKnow.Multiplayer
{
    /// <summary>
    /// Network Game Manager - Quản lý game session với NGO
    /// </summary>
    public class NetworkGameManager : NetworkBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Events
        public static event Action OnGameStarted;
        public static event Action OnGameEnded;
        public static event Action<ulong> OnPlayerJoined;
        public static event Action<ulong> OnPlayerLeft;
        public static event Action<int> OnPlayerCountChanged;

        // Network Variables
        private NetworkVariable<int> playerCount = new NetworkVariable<int>(0);
        private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);

        // Properties
        public bool IsGameStarted => gameStarted.Value;
        public int PlayerCount => playerCount.Value;
        public bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

        private static NetworkGameManager _instance;
        public static NetworkGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NetworkGameManager>();
                }
                return _instance;
            }
        }

        private Dictionary<ulong, PlayerNetworkData> connectedPlayers = new Dictionary<ulong, PlayerNetworkData>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            DebugLog("NetworkGameManager spawned");

            if (IsServer)
            {
                // Server/Host setup
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                
                DebugLog("Server callbacks registered");
            }

            // Subscribe to network variable changes
            playerCount.OnValueChanged += OnPlayerCountValueChanged;
            gameStarted.OnValueChanged += OnGameStartedValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            playerCount.OnValueChanged -= OnPlayerCountValueChanged;
            gameStarted.OnValueChanged -= OnGameStartedValueChanged;
        }

        #region Server Callbacks

        private void OnClientConnected(ulong clientId)
        {
            DebugLog($"Client connected: {clientId}");

            if (IsServer)
            {
                playerCount.Value++;
                
                // Request player data from client
                RequestPlayerDataClientRpc(clientId);
                
                OnPlayerJoined?.Invoke(clientId);
                
                // Check if we should start the game
                if (playerCount.Value >= GameConfig.MAX_PLAYERS && !gameStarted.Value)
                {
                    StartCoroutine(StartGameDelayed());
                }
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            DebugLog($"Client disconnected: {clientId}");

            if (IsServer)
            {
                playerCount.Value--;
                
                if (connectedPlayers.ContainsKey(clientId))
                {
                    connectedPlayers.Remove(clientId);
                }
                
                OnPlayerLeft?.Invoke(clientId);
                
                // Handle game state if needed
                if (gameStarted.Value && playerCount.Value < GameConfig.MIN_PLAYERS)
                {
                    // Not enough players, end game
                    EndGame();
                }
            }
        }

        #endregion

        #region Network Variable Callbacks

        private void OnPlayerCountValueChanged(int oldValue, int newValue)
        {
            DebugLog($"Player count changed: {oldValue} -> {newValue}");
            OnPlayerCountChanged?.Invoke(newValue);
        }

        private void OnGameStartedValueChanged(bool oldValue, bool newValue)
        {
            DebugLog($"Game started changed: {oldValue} -> {newValue}");
            
            if (newValue)
            {
                OnGameStarted?.Invoke();
            }
        }

        #endregion

        #region Game Flow

        private IEnumerator StartGameDelayed()
        {
            DebugLog($"Starting game in {GameConfig.GAME_START_DELAY} seconds...");

            // Notify all clients
            NotifyGameStartingClientRpc(GameConfig.GAME_START_DELAY);

            yield return new WaitForSeconds(GameConfig.GAME_START_DELAY);

            StartGame();
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc()
        {
            if (IsServer && !gameStarted.Value)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            if (!IsServer) return;

            DebugLog("Starting game...");
            
            gameStarted.Value = true;
            
            // Initialize game state
            InitializeGameState();
            
            DebugLog("Game started successfully");
        }

        private void InitializeGameState()
        {
            // Override this in derived class or use events
            DebugLog("Initializing game state...");
            
            // Example: Spawn player objects, initialize board, etc.
        }

        public void EndGame()
        {
            if (!IsServer) return;

            DebugLog("Ending game...");
            
            gameStarted.Value = false;
            
            // Notify all clients
            NotifyGameEndedClientRpc();
            
            OnGameEnded?.Invoke();
            
            DebugLog("Game ended");
        }

        #endregion

        #region Player Data Management

        [ClientRpc]
        private void RequestPlayerDataClientRpc(ulong clientId)
        {
            // Only the target client should respond
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                var gameDataManager = GameDataManager.Instance;
                if (gameDataManager != null)
                {
                    string playerName = gameDataManager.currentIngameName ?? gameDataManager.currentUsername;
                    int level = gameDataManager.currentLevel;
                    
                    SubmitPlayerDataServerRpc(clientId, playerName, level);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitPlayerDataServerRpc(ulong clientId, string playerName, int level)
        {
            DebugLog($"Received player data from {clientId}: {playerName} (Level {level})");
            
            var playerData = new PlayerNetworkData
            {
                clientId = clientId,
                playerName = playerName,
                level = level
            };
            
            connectedPlayers[clientId] = playerData;
            
            // Broadcast to all clients
            UpdatePlayerDataClientRpc(clientId, playerName, level);
        }

        [ClientRpc]
        private void UpdatePlayerDataClientRpc(ulong clientId, string playerName, int level)
        {
            var playerData = new PlayerNetworkData
            {
                clientId = clientId,
                playerName = playerName,
                level = level
            };
            
            connectedPlayers[clientId] = playerData;
            
            DebugLog($"Player data updated: {playerName} (Level {level})");
        }

        public PlayerNetworkData GetPlayerData(ulong clientId)
        {
            if (connectedPlayers.ContainsKey(clientId))
            {
                return connectedPlayers[clientId];
            }
            return null;
        }

        public List<PlayerNetworkData> GetAllPlayerData()
        {
            return new List<PlayerNetworkData>(connectedPlayers.Values);
        }

        #endregion

        #region Client RPCs

        [ClientRpc]
        private void NotifyGameStartingClientRpc(float delay)
        {
            DebugLog($"Game starting in {delay} seconds...");
            // Show countdown UI or notification
        }

        [ClientRpc]
        private void NotifyGameEndedClientRpc()
        {
            DebugLog("Game ended notification received");
            OnGameEnded?.Invoke();
        }

        #endregion

        #region Utility Methods

        public bool IsPlayerConnected(ulong clientId)
        {
            return connectedPlayers.ContainsKey(clientId);
        }

        public int GetConnectedPlayerCount()
        {
            return connectedPlayers.Count;
        }

        public void DisconnectPlayer()
        {
            if (NetworkManager.Singleton != null)
            {
                if (IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                else
                {
                    NetworkManager.Singleton.Shutdown();
                }
                
                DebugLog("Player disconnected from game");
            }
        }

        #endregion

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[NetworkGameManager] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[NetworkGameManager] {message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Player network data structure
    /// </summary>
    [System.Serializable]
    public class PlayerNetworkData
    {
        public ulong clientId;
        public string playerName;
        public int level;
        public bool isReady;
    }
}
