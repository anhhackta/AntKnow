using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

namespace AntKnow.Server
{
    /// <summary>
    /// Server-Authoritative Game Manager
    /// Chỉ chạy trên server, điều khiển toàn bộ game logic
    /// </summary>
    public class ServerGameManager : NetworkBehaviour
    {
        public static ServerGameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int maxTurns = 25;
        [SerializeField] private float turnTimeLimit = 60f;
        [SerializeField] private int startingMoney = 2000;
        [SerializeField] private int minPlayersToStart = 2;
        [SerializeField] private float gameStartDelay = 5f;

        [Header("Board Settings")]
        [SerializeField] private int boardLength = 36;

        // Server-side game state (Domain layer)
        private GameState gameState;
        private TurnSystem turnSystem;
        private int currentPlayerIndex = 0;
        private bool gameActive = false;
        private float turnStartTime;

        // Network Variables (Server → Client sync)
        private NetworkVariable<int> currentTurn = new NetworkVariable<int>(1, 
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private NetworkVariable<int> currentPlayerTurn = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Player mapping: ClientId → PlayerId
        private Dictionary<ulong, int> clientToPlayerMap = new Dictionary<ulong, int>();

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                // Client-side: Listen to network variable changes
                isGameActive.OnValueChanged += OnGameActiveChanged;
                currentPlayerTurn.OnValueChanged += OnCurrentPlayerChanged;
                return;
            }

            // Server-side initialization
            Instance = this;
            Debug.Log("[ServerGameManager] Server spawned");

            // Register callbacks
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            else
            {
                isGameActive.OnValueChanged -= OnGameActiveChanged;
                currentPlayerTurn.OnValueChanged -= OnCurrentPlayerChanged;
            }
        }

        #region Server: Connection Management

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[ServerGameManager] Client {clientId} connected");

            // Check if we have enough players to start
            int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            Debug.Log($"[ServerGameManager] Total players: {connectedPlayers}");

            if (connectedPlayers >= minPlayersToStart && !gameActive)
            {
                Debug.Log($"[ServerGameManager] Enough players ({connectedPlayers}/{minPlayersToStart}). Starting game in {gameStartDelay}s...");
                Invoke(nameof(StartGame), gameStartDelay);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[ServerGameManager] Client {clientId} disconnected");

            if (gameActive)
            {
                Debug.LogWarning("[ServerGameManager] Player disconnected during game!");
                // TODO: Handle disconnect (pause game, replace with AI, etc.)
            }

            // Remove from mapping
            if (clientToPlayerMap.ContainsKey(clientId))
            {
                int playerId = clientToPlayerMap[clientId];
                clientToPlayerMap.Remove(clientId);
                Debug.Log($"[ServerGameManager] Removed player {playerId} from game");
            }
        }

        #endregion

        #region Server: Game Flow

        private void StartGame()
        {
            if (!IsServer || gameActive) return;

            Debug.Log("[ServerGameManager] ========== STARTING GAME ==========");
            gameActive = true;
            isGameActive.Value = true;

            // Initialize game state
            InitializeGameState();

            // Notify all clients
            NotifyGameStartClientRpc();

            // Start first turn
            Invoke(nameof(StartNextTurn), 2f);
        }

        private void InitializeGameState()
        {
            // Create domain GameState
            gameState = new GameState
            {
                BoardLength = boardLength,
                CurrentTurnPlayerId = 1
            };

            // Initialize properties from SimpleBoardConfig
            var tiles = SimpleBoardConfig.GetTiles();
            foreach (var tile in tiles)
            {
                if (tile.type == TileType.Property)
                {
                    gameState.Properties[tile.index] = new PropertyState
                    {
                        TileId = tile.index,
                        BasePrice = tile.basePrice,
                        Owner = Owner.None,
                        Level = 0,
                        HasHotel = false
                    };
                }
            }

            // Create player states for each connected client
            int playerId = 1;
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                ulong clientId = kvp.Key;

                var playerState = new PlayerState
                {
                    Id = playerId,
                    Money = startingMoney,
                    NodeIndex = 0,
                    JailTurns = 0,
                    Health = 100,
                    Agility = 100,
                    Intelligence = 100,
                    Luck = 100,
                    Resistance = 100
                };

                gameState.Players.Add(playerState);
                clientToPlayerMap[clientId] = playerId;

                Debug.Log($"[ServerGameManager] Player {playerId} initialized (Client {clientId})");
                playerId++;
            }

            // Initialize TurnSystem
            turnSystem = new TurnSystem(gameState, baseSalary: 200);

            Debug.Log($"[ServerGameManager] Game initialized with {gameState.Players.Count} players and {gameState.Properties.Count} properties");
        }

        private void StartNextTurn()
        {
            if (!IsServer || !gameActive) return;

            // Move to next player
            currentPlayerIndex = (currentPlayerIndex) % gameState.Players.Count;
            currentPlayerTurn.Value = currentPlayerIndex;
            turnStartTime = Time.time;

            var currentPlayer = gameState.Players[currentPlayerIndex];
            Debug.Log($"[ServerGameManager] ===== TURN {currentTurn.Value}: Player {currentPlayer.Id} =====");

            // Notify clients
            NotifyTurnStartClientRpc(currentPlayerIndex, currentPlayer.Id);

            // Increment for next time
            currentPlayerIndex++;
        }

        private void EndCurrentTurn()
        {
            if (!IsServer || !gameActive) return;

            Debug.Log($"[ServerGameManager] Turn {currentTurn.Value} ended");

            currentTurn.Value++;

            // Check end game conditions
            if (currentTurn.Value > maxTurns)
            {
                EndGame("Max turns reached");
                return;
            }

            // Start next turn
            Invoke(nameof(StartNextTurn), 1f);
        }

        private void EndGame(string reason)
        {
            if (!IsServer) return;

            Debug.Log($"[ServerGameManager] ========== GAME ENDED: {reason} ==========");
            gameActive = false;
            isGameActive.Value = false;

            // Calculate scores
            CalculateScores();

            // Notify clients
            NotifyGameEndClientRpc(reason);
        }

        private void CalculateScores()
        {
            Debug.Log("[ServerGameManager] Calculating final scores...");

            PlayerState winner = null;
            int highestAssets = 0;

            foreach (var player in gameState.Players)
            {
                // Calculate total assets = money + property values
                int totalAssets = BoardRules.CalculateTotalAssets(player, gameState);

                Debug.Log($"[ServerGameManager] Player {player.Id}: Money={player.Money}, Total Assets={totalAssets}");

                if (totalAssets > highestAssets)
                {
                    highestAssets = totalAssets;
                    winner = player;
                }
            }

            if (winner != null)
            {
                Debug.Log($"[ServerGameManager] ========== WINNER: Player {winner.Id} with {highestAssets} total assets! ==========");
            }
        }

        #endregion

        #region Server: Game Actions (ServerRpc)

        /// <summary>
        /// Client requests to roll dice
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RequestRollDiceServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!gameActive) return;

            ulong clientId = rpcParams.Receive.SenderClientId;
            
            // Get player ID from client ID
            if (!clientToPlayerMap.TryGetValue(clientId, out int playerId))
            {
                Debug.LogWarning($"[ServerGameManager] Unknown client {clientId} tried to roll dice");
                return;
            }

            // Validate: Is it this player's turn?
            int expectedPlayerId = gameState.Players[currentPlayerTurn.Value].Id;
            if (playerId != expectedPlayerId)
            {
                Debug.LogWarning($"[ServerGameManager] Player {playerId} tried to roll but it's Player {expectedPlayerId}'s turn");
                return;
            }

            Debug.Log($"[ServerGameManager] Player {playerId} rolling dice...");

            // Roll dice (server-authoritative)
            int dice1 = Random.Range(1, 7);
            int dice2 = Random.Range(1, 7);
            int total = dice1 + dice2;

            Debug.Log($"[ServerGameManager] Dice: {dice1} + {dice2} = {total}");

            // Update player position
            var player = gameState.Players.Find(p => p.Id == playerId);
            int oldPosition = player.NodeIndex;
            player.NodeIndex = (player.NodeIndex + total) % gameState.BoardLength;

            Debug.Log($"[ServerGameManager] Player {playerId} moved: {oldPosition} → {player.NodeIndex}");

            // Notify all clients
            NotifyDiceRollClientRpc(playerId, dice1, dice2, player.NodeIndex);

            // Resolve tile after 2 seconds (allow animation time)
            Invoke(nameof(ResolveTileForCurrentPlayer), 2f);
        }

        private void ResolveTileForCurrentPlayer()
        {
            var player = gameState.Players[currentPlayerTurn.Value];
            ResolveTile(player);
        }

        private void ResolveTile(PlayerState player)
        {
            Debug.Log($"[ServerGameManager] Resolving tile {player.NodeIndex} for Player {player.Id}");

            // Get tile data
            var tileData = SimpleBoardConfig.GetTileByWaypointIndex(player.NodeIndex);
            if (tileData == null)
            {
                Debug.LogWarning($"[ServerGameManager] Invalid tile index: {player.NodeIndex}");
                return;
            }

            Debug.Log($"[ServerGameManager] Tile: {tileData.name} (Type: {tileData.type})");

            // Resolve tile using TurnSystem
            // Note: TurnSystem.MoveAndResolve handles the resolution internally
            // For now, we'll handle specific cases here

            switch (tileData.type)
            {
                case TileType.Property:
                    HandlePropertyTile(player, tileData);
                    break;

                case TileType.Chance:
                    Debug.Log($"[ServerGameManager] Event tile - waiting for client interaction");
                    // Client will call EventCardServerRpc
                    break;

                case TileType.Quiz:
                    Debug.Log($"[ServerGameManager] Quiz tile - waiting for client interaction");
                    // Client will call QuizServerRpc
                    break;

                case TileType.Jail:
                    player.JailTurns = 3;
                    Debug.Log($"[ServerGameManager] Player {player.Id} sent to jail for 3 turns!");
                    break;

                case TileType.Travel:
                    Debug.Log($"[ServerGameManager] Travel tile - waiting for client to choose destination");
                    // Client will call TravelServerRpc
                    break;

                case TileType.Start:
                    Debug.Log($"[ServerGameManager] Start tile - no action");
                    break;

                default:
                    Debug.Log($"[ServerGameManager] Tile type {tileData.type} not implemented");
                    break;
            }
        }

        private void HandlePropertyTile(PlayerState player, SimpleTileData tileData)
        {
            if (!gameState.Properties.ContainsKey(tileData.index))
            {
                Debug.LogWarning($"[ServerGameManager] Property {tileData.index} not found in game state");
                return;
            }

            var property = gameState.Properties[tileData.index];

            if (property.Owner == Owner.None)
            {
                Debug.Log($"[ServerGameManager] Property {tileData.name} is available for purchase (Price: {tileData.basePrice})");
                // Client will call BuyPropertyServerRpc if player wants to buy
            }
            else if ((int)property.Owner != player.Id)
            {
                // Player must pay rent
                var owner = gameState.Players.Find(p => p.Id == (int)property.Owner);
                if (owner != null)
                {
                    int rent = BoardRules.CalcRent(tileData, property, owner);
                    BoardRules.PayRent(player, owner, rent);

                    Debug.Log($"[ServerGameManager] Player {player.Id} paid {rent} rent to Player {owner.Id}");

                    // Notify clients
                    NotifyRentPaidClientRpc(player.Id, owner.Id, rent);
                }
            }
            else
            {
                Debug.Log($"[ServerGameManager] Player {player.Id} landed on own property {tileData.name}");
                // No action needed
            }
        }

        [ClientRpc]
        private void NotifyRentPaidClientRpc(int payerId, int ownerId, int amount)
        {
            Debug.Log($"[Client] Player {payerId} paid {amount} rent to Player {ownerId}");
            // TODO: Update UI
            // For now, just end turn after 2 seconds

            Invoke(nameof(EndCurrentTurn), 2f);
        }

        #endregion

        #region Client RPCs (Server → Client notifications)

        [ClientRpc]
        private void NotifyGameStartClientRpc()
        {
            Debug.Log("[Client] 🎮 GAME STARTED!");
            // Client-side: Show game UI, hide lobby
        }

        [ClientRpc]
        private void NotifyTurnStartClientRpc(int playerIndex, int playerId)
        {
            Debug.Log($"[Client] 🎲 Turn started for Player {playerId}");
            // Client-side: Enable/disable controls based on turn
        }

        [ClientRpc]
        private void NotifyDiceRollClientRpc(int playerId, int dice1, int dice2, int newPosition)
        {
            Debug.Log($"[Client] 🎲 Player {playerId} rolled: {dice1} + {dice2} = {dice1 + dice2}");
            Debug.Log($"[Client] 🚶 Moving to position {newPosition}");
            // Client-side: Animate dice, move player
        }

        [ClientRpc]
        private void NotifyGameEndClientRpc(string reason)
        {
            Debug.Log($"[Client] 🏁 GAME ENDED: {reason}");
            // Client-side: Show results panel
        }

        #endregion

        #region Client: Network Variable Listeners

        private void OnGameActiveChanged(bool previous, bool current)
        {
            Debug.Log($"[Client] Game active changed: {previous} → {current}");
        }

        private void OnCurrentPlayerChanged(int previous, int current)
        {
            Debug.Log($"[Client] Current player changed: {previous} → {current}");
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            if (!IsServer || !gameActive) return;

            // Check turn timeout
            if (Time.time - turnStartTime > turnTimeLimit)
            {
                Debug.LogWarning($"[ServerGameManager] Turn timeout! Forcing end turn.");
                EndCurrentTurn();
            }
        }

        #endregion
    }
}

