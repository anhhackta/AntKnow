using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using AntKnow.Auth;

namespace AntKnow.Game
{
    // ===== NETWORK STRUCTS =====
    
    /// <summary>
    /// Player loadout data - Client gửi lên Host
    /// </summary>
    [System.Serializable]
    public struct PlayerLoadoutData : INetworkSerializable
    {
        public string playerName;
        public string playerId;
        public bool isMale;
        public int health;
        public int agility;
        public int intelligence;
        public int luck;
        public int resistance;
        
        // Skill Cards (effectIds, separated by comma)
        public string skillCardIdsStr; // "autoStepForward,purchaseDiscount"
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref isMale);
            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref agility);
            serializer.SerializeValue(ref intelligence);
            serializer.SerializeValue(ref luck);
            serializer.SerializeValue(ref resistance);
            serializer.SerializeValue(ref skillCardIdsStr);
        }
        
        /// <summary>
        /// Get skill card IDs as list
        /// </summary>
        public List<string> GetSkillCardIds()
        {
            if (string.IsNullOrEmpty(skillCardIdsStr))
                return new List<string>();
            
            return new List<string>(skillCardIdsStr.Split(','));
        }
    }
    
    /// <summary>
    /// Turn order roll result
    /// </summary>
    [System.Serializable]
    public struct TurnOrderRoll : INetworkSerializable
    {
        public ulong clientId;
        public int diceResult;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref diceResult);
        }
    }
    
    /// <summary>
    /// Main game controller
    /// Tích hợp với lobby system và multiplayer
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        [Header("Managers")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private DiceController diceController;
        [SerializeField] private PropertyManager propertyManager;

        [Header("Players")]
        [SerializeField] private List<PlayerGameController> players = new List<PlayerGameController>();
        [SerializeField] private GameObject playerPrefab;

        [Header("UI")]
        [SerializeField] private Button rollButton;
        [SerializeField] private TMPro.TextMeshProUGUI turnText;
        [SerializeField] private TMPro.TextMeshProUGUI currentPlayerText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        [Header("UI Panels")]
        [SerializeField] private PanelBuy panelBuy;
        [SerializeField] private PanelQuiz panelQuiz;
        [SerializeField] private PanelEvent panelEvent;
        [SerializeField] private PanelHouseSell panelHouseSell;
        [SerializeField] private PanelResult panelResult;
        [SerializeField] private PanelCard panelCard;

        [Header("Game Settings")]
        [SerializeField] private int maxTurns = 25;
        [SerializeField] private bool demoMode = false; // True = spawn test players, False = load from lobby

        [Header("Services")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        // Game state
        private int currentTurn = 1;
        private int currentPlayerIndex = 0;
        private float gameStartTime;
        private bool isGameActive = false;
        
        // ===== MULTIPLAYER STATE =====
        private Dictionary<ulong, PlayerLoadoutData> playerLoadouts = new Dictionary<ulong, PlayerLoadoutData>();
        private List<TurnOrderRoll> turnOrderRolls = new List<TurnOrderRoll>();
        private bool isTurnOrderPhase = false;
        private int roundCounter = 0; // Vòng tròn (all players đã đi)
        private const int QUIZ_INTERVAL = 8; // Quiz mỗi 8 rounds

        public bool IsGameActive => isGameActive;
        public int CurrentTurn => currentTurn;
        public PlayerGameController CurrentPlayer => players.Count > 0 ? players[currentPlayerIndex] : null;

        private void Start()
        {
            // Find services if not assigned
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Wait for network ready
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                StartGame();
            }
            else if (demoMode)
            {
                // Demo mode: Start immediately without network
                StartGame();
            }
            else
            {
                Debug.LogWarning("[GameManager] Waiting for network connection...");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!demoMode)
            {
                StartGame();
            }
        }
        
        private void Update()
        {
            if (isGameActive)
            {
                UpdateTimeDisplay();
            }
        }
        
        /// <summary>
        /// Start game
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[GameManager] Starting game...");

            // Initialize
            currentTurn = 1;
            currentPlayerIndex = 0;
            gameStartTime = Time.time;
            isGameActive = true;

            // Setup UI
            if (rollButton != null)
            {
                rollButton.onClick.AddListener(OnRollButtonClicked);
            }

            // Spawn players
            if (demoMode)
            {
                // Demo: Spawn ONLY 1 test player
                SpawnTestPlayer("Player 1", "test_player_1", true, 10, 10, 10, 10, 10);
                Debug.Log("[GameManager] Demo Mode: Spawned 1 player only");
            }
            else
            {
                // Load players from lobby/session data
                StartCoroutine(LoadPlayersFromLobby());
            }

            // Start first turn
            StartTurn();
        }

        /// <summary>
        /// Load players from lobby (multiplayer) - NEW IMPLEMENTATION
        /// </summary>
        private IEnumerator LoadPlayersFromLobby()
        {
            Debug.Log("[GameManager] Loading players from lobby...");

            // Get session data
            var sessionData = GameSessionData.Instance;
            if (sessionData == null)
            {
                Debug.LogError("[GameManager] GameSessionData not found!");
                yield break;
            }

            // Get connected clients
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[GameManager] NetworkManager not found!");
                yield break;
            }

            // Wait for all clients to connect
            yield return new WaitForSeconds(1f);

            // Extract skill card effectIds from session data
            List<string> skillCardEffectIds = new List<string>();
            if (sessionData.skillCards != null && sessionData.skillCards.Count > 0)
            {
                foreach (var card in sessionData.skillCards)
                {
                    // Extract effectId directly from each card
                    if (!string.IsNullOrEmpty(card.effectId))
                    {
                        skillCardEffectIds.Add(card.effectId);
                    }
                }
            }
            
            // Create local loadout data
            PlayerLoadoutData localLoadout = new PlayerLoadoutData
            {
                playerName = sessionData.playerName ?? "Player",
                playerId = sessionData.firebaseUID ?? "unknown",
                isMale = sessionData.gender == "male",
                health = sessionData.totalHealth,
                agility = sessionData.totalAgility,
                intelligence = sessionData.totalIntelligence,
                luck = sessionData.totalLuck,
                resistance = sessionData.totalResistance,
                skillCardIdsStr = string.Join(",", skillCardEffectIds) // "autoStepForward,purchaseDiscount"
            };
            
            Debug.Log($"[GameManager] Local loadout skill cards: {localLoadout.skillCardIdsStr}");

            if (IsHost)
            {
                // HOST: Add own loadout
                ulong localClientId = NetworkManager.Singleton.LocalClientId;
                playerLoadouts[localClientId] = localLoadout;
                
                Debug.Log($"[Host] Added own loadout: {localLoadout.playerName}");
                
                // Wait for all clients to send their loadouts
                Debug.Log($"[Host] Waiting for {NetworkManager.Singleton.ConnectedClients.Count} client loadouts...");
                
                yield return new WaitUntil(() => 
                    playerLoadouts.Count >= NetworkManager.Singleton.ConnectedClients.Count
                );
                
                Debug.Log($"[Host] Received all {playerLoadouts.Count} loadouts!");
                
                // Spawn all players
                SpawnAllPlayers();
                
                // Start turn order selection
                StartTurnOrderSelection();
            }
            else
            {
                // CLIENT: Send loadout to Host
                Debug.Log($"[Client] Sending loadout to Host: {localLoadout.playerName}");
                SendLoadoutToHostServerRpc(localLoadout);
                
                // Wait for game to start
                Debug.Log("[Client] Waiting for Host to start game...");
            }
        }

        /// <summary>
        /// Spawn test player (demo)
        /// </summary>
        private void SpawnTestPlayer(string name, string id, bool isMale, int hp, int agi, int intel, int lck, int res)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[GameManager] Player prefab not assigned!");
                return;
            }

            // Spawn at tile 0
            Vector3 spawnPos = boardManager.GetWaypointPosition(0);
            GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            PlayerGameController player = playerObj.GetComponent<PlayerGameController>();
            if (player != null)
            {
                player.Initialize(name, id, isMale, hp, agi, intel, lck, res);
                players.Add(player);

                Debug.Log($"[GameManager] Spawned player: {name}");
            }
        }

        /// <summary>
        /// Spawn player for multiplayer
        /// </summary>
        private void SpawnPlayerNetwork(string name, string id, bool isMale, int hp, int agi, int intel, int lck, int res, ulong clientId, List<string> skillCardIds)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[GameManager] Player prefab not assigned!");
                return;
            }

            // Spawn at tile 0
            Vector3 spawnPos = boardManager.GetWaypointPosition(0);
            GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            // Spawn as network object
            NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.SpawnAsPlayerObject(clientId);
            }

            PlayerGameController player = playerObj.GetComponent<PlayerGameController>();
            if (player != null)
            {
                player.Initialize(name, id, isMale, hp, agi, intel, lck, res);
                player.SetSkillCards(skillCardIds); // SET SKILL CARDS!
                players.Add(player);

                Debug.Log($"[GameManager] Spawned network player: {name} (ClientId: {clientId}) with {skillCardIds.Count} skill cards");
            }
        }
        
        /// <summary>
        /// Start turn
        /// </summary>
        private void StartTurn()
        {
            if (!isGameActive) return;

            PlayerGameController player = CurrentPlayer;
            if (player == null) return;

            Debug.Log($"[GameManager] Turn {currentTurn} - {player.PlayerName}'s turn");

            // Update UI
            UpdateTurnDisplay();

            // Update turn indicators
            UpdateTurnIndicators();

            // Check jail
            if (player.JailCounter > 0)
            {
                Debug.Log($"[GameManager] {player.PlayerName} is in jail for {player.JailCounter} more turns");
                player.DecreaseJailCounter();

                // TODO: Show jail UI option (roll for double or wait)
                // For now, just end turn
                EndTurn();
                return;
            }

            // Check skip turn
            if (player.SkipNextTurn)
            {
                Debug.Log($"[GameManager] {player.PlayerName} skips this turn");
                player.SetSkipNextTurn(false);
                EndTurn();
                return;
            }

            // Enable roll button
            if (rollButton != null)
            {
                rollButton.interactable = true;
            }
        }

        /// <summary>
        /// Update turn indicators (ping trên đầu player)
        /// </summary>
        private void UpdateTurnIndicators()
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null)
                {
                    if (i == currentPlayerIndex)
                    {
                        players[i].ShowTurnIndicator();
                    }
                    else
                    {
                        players[i].HideTurnIndicator();
                    }
                }
            }
        }
        
        /// <summary>
        /// Roll button clicked
        /// </summary>
        private void OnRollButtonClicked()
        {
            if (!isGameActive) return;
            
            PlayerGameController player = CurrentPlayer;
            if (player == null) return;
            
            // Disable button
            if (rollButton != null)
            {
                rollButton.interactable = false;
            }
            
            // Roll dice
            StartCoroutine(RollAndMove());
        }
        
        /// <summary>
        /// Roll dice and move player - WITH LUCK CHECK (Server-Authoritative)
        /// </summary>
        private IEnumerator RollAndMove()
        {
            PlayerGameController player = CurrentPlayer;
            
            if (!IsHost)
            {
                yield break; // Only Host can roll dice
            }
            
            // === LUCK-BASED DICE ROLL ===
            int diceResult;
            bool isDouble = false;
            bool wasLuckyDouble = false;
            int die1 = 0, die2 = 0;
            
            // Check Luck stat
            int luckStat = player.Luck;
            int luckPct = luckStat / 10; // 10 pts = 1%
            float doubleChance = luckPct / 100f;
            
            if (Random.value < doubleChance && luckStat > 0)
            {
                // ⭐ LUCK ACTIVATED! Roll 1 dice x2
                int die = Random.Range(1, 7);
                die1 = die;
                die2 = die; // Same value
                diceResult = die * 2;
                isDouble = true;
                wasLuckyDouble = true;
                
                Debug.Log($"[Host] ⭐ LUCK ACTIVATED! Player {player.PlayerName} rolled {die} x2 = {diceResult}");
            }
            else
            {
                // Normal roll: 2 dice
                die1 = Random.Range(1, 7);
                die2 = Random.Range(1, 7);
                diceResult = die1 + die2;
                isDouble = (die1 == die2);
                
                Debug.Log($"[Host] Player {player.PlayerName} rolled {die1} + {die2} = {diceResult}");
            }
            
            // Notify all clients of dice result
            NotifyDiceRolledClientRpc(currentPlayerIndex, die1, die2, diceResult, isDouble, wasLuckyDouble);
            
            // Wait for dice animation
            yield return new WaitForSeconds(1.5f);
            
            // Move player
            yield return player.MoveBySteps(diceResult);
            
            // Resolve tile
            ResolveTile(player);
            
            // End turn (for now, auto end)
            yield return new WaitForSeconds(1f);
            EndTurn();
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Notify dice roll result
        /// </summary>
        [ClientRpc]
        private void NotifyDiceRolledClientRpc(int playerIndex, int die1, int die2, int result, bool isDouble, bool wasLuckyDouble)
        {
            string playerName = players.Count > playerIndex ? players[playerIndex].PlayerName : "Unknown";
            
            if (wasLuckyDouble)
            {
                Debug.Log($"[Client] ⭐⭐⭐ LUCK! {playerName} rolled {die1} x2 = {result} ⭐⭐⭐");
                // TODO: Show special effect
            }
            else if (isDouble)
            {
                Debug.Log($"[Client] 🎲 {playerName} rolled DOUBLE {die1}! Total: {result}");
            }
            else
            {
                Debug.Log($"[Client] 🎲 {playerName} rolled {die1} + {die2} = {result}");
            }
            
            // TODO: Update dice UI animation
        }
        
        /// <summary>
        /// Resolve tile effect
        /// </summary>
        private void ResolveTile(PlayerGameController player)
        {
            int tileIndex = player.CurrentTile;
            TileType tileType = boardManager.GetTileType(tileIndex);
            string tileName = boardManager.GetTileName(tileIndex);
            int basePrice = boardManager.GetTilePrice(tileIndex);

            Debug.Log($"[GameManager] {player.PlayerName} landed on {tileName} (Type: {tileType})");

            switch (tileType)
            {
                case TileType.Start:
                    // Already handled in MoveBySteps
                    break;

                case TileType.Property:
                    ResolvePropertyTile(player, tileIndex, tileName, basePrice);
                    break;

                case TileType.Event:
                    // TODO: Draw event card
                    Debug.Log($"[GameManager] Event tile - TODO: Draw event card");
                    break;

                case TileType.Quiz:
                    // TODO: Show quiz panel
                    Debug.Log($"[GameManager] Quiz tile - TODO: Show quiz");
                    break;

                case TileType.Jail:
                    player.SetJailCounter(2); // 2 turns in jail
                    Debug.Log($"[GameManager] Jail tile - {player.PlayerName} in jail for 2 turns");
                    break;

                case TileType.Travel:
                    player.SubtractMoney(100);
                    Debug.Log($"[GameManager] Travel tile - {player.PlayerName} pays 100");
                    break;
            }
        }

        /// <summary>
        /// Resolve property tile
        /// </summary>
        private void ResolvePropertyTile(PlayerGameController player, int tileIndex, string tileName, int basePrice)
        {
            if (propertyManager == null)
            {
                Debug.LogWarning("[GameManager] PropertyManager not assigned!");
                return;
            }

            int playerIndex = players.IndexOf(player);

            // Check if property is owned
            if (!propertyManager.IsPropertyOwned(tileIndex))
            {
                // Property chưa có chủ - Show buy panel
                Debug.Log($"[GameManager] Property {tileName} available for purchase: {basePrice}");

                ShowBuyPanel(player, tileIndex, tileName, basePrice);
            }
            else
            {
                // Property đã có chủ
                int ownerIndex = propertyManager.GetPropertyOwner(tileIndex);

                if (ownerIndex == playerIndex)
                {
                    // Chủ nhà đứng trên nhà của mình - Show upgrade panel
                    Debug.Log($"[GameManager] {player.PlayerName} landed on own property {tileName}");
                    ShowUpgradePanel(player, tileIndex, tileName, basePrice);
                }
                else
                {
                    // Thuê nhà người khác
                    PlayerGameController owner = players[ownerIndex];
                    Debug.Log($"[GameManager] {player.PlayerName} must pay rent to {owner.PlayerName}");

                    propertyManager.PayRent(tileIndex, basePrice, player, owner);

                    // Check bankruptcy
                    if (player.IsBankrupt())
                    {
                        Debug.Log($"[GameManager] {player.PlayerName} is bankrupt!");
                        ShowSellPanel(player);
                    }
                }
            }
        }

        /// <summary>
        /// Show buy panel
        /// </summary>
        private void ShowBuyPanel(PlayerGameController player, int tileIndex, string tileName, int basePrice)
        {
            int playerIdx = players.IndexOf(player);

            if (panelBuy == null)
            {
                // Auto buy for demo
                if (player.Money >= basePrice)
                {
                    propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);
                }
                return;
            }

            panelBuy.ShowBuy(tileName, basePrice, player.Money, (selectedLevel) =>
            {
                if (selectedLevel > 0)
                {
                    // Buy with selected level
                    propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);

                    if (selectedLevel > 0)
                    {
                        propertyManager.UpgradeProperty(tileIndex, selectedLevel, basePrice, player);
                    }
                }

                // Continue game
                StartCoroutine(ContinueAfterPanel());
            },
            () =>
            {
                // Skip
                StartCoroutine(ContinueAfterPanel());
            });
        }

        /// <summary>
        /// Show upgrade panel
        /// </summary>
        private void ShowUpgradePanel(PlayerGameController player, int tileIndex, string tileName, int basePrice)
        {
            if (panelBuy == null)
            {
                return;
            }

            int currentLevel = propertyManager.GetPropertyLevel(tileIndex);

            panelBuy.ShowUpgrade(tileName, basePrice, currentLevel, player.Money, (selectedLevel) =>
            {
                if (selectedLevel > currentLevel)
                {
                    propertyManager.UpgradeProperty(tileIndex, selectedLevel, basePrice, player);
                }

                // Continue game
                StartCoroutine(ContinueAfterPanel());
            },
            () =>
            {
                // Skip
                StartCoroutine(ContinueAfterPanel());
            });
        }

        /// <summary>
        /// Show sell panel (bankruptcy)
        /// </summary>
        private void ShowSellPanel(PlayerGameController player)
        {
            if (panelHouseSell == null)
            {
                Debug.LogWarning("[GameManager] PanelHouseSell not assigned!");
                return;
            }

            // TODO: Get owned properties and show sell panel
            Debug.Log("[GameManager] TODO: Show sell panel");
        }

        /// <summary>
        /// Continue game after panel closes
        /// </summary>
        private IEnumerator ContinueAfterPanel()
        {
            yield return new WaitForSeconds(0.5f);
            // Game continues automatically
        }
        
        /// <summary>
        /// End turn - WITH ROUND TRACKING & QUIZ SYSTEM
        /// </summary>
        private void EndTurn()
        {
            if (!isGameActive) return;
            if (!IsHost) return; // Only Host manages turns
            
            Debug.Log($"[GameManager] Turn ended. Player {currentPlayerIndex}/{players.Count - 1}");
            
            // Reduce skill card cooldowns for current player
            if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
            {
                players[currentPlayerIndex].ReduceCooldowns();
            }
            
            // Check win condition
            if (CheckWinCondition())
            {
                EndGame();
                return;
            }
            
            // Next player
            currentPlayerIndex++;
            
            // Check if completed a ROUND (all players finished their turn)
            if (currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
                roundCounter++;
                currentTurn++;
                
                Debug.Log($"[GameManager] ========== ROUND {roundCounter} COMPLETED ==========");
                Debug.Log($"[GameManager] Turn {currentTurn}/{maxTurns}");
                
                // Check for QUIZ ROUND (every 8 rounds)
                if (roundCounter % QUIZ_INTERVAL == 0)
                {
                    Debug.Log($"[GameManager] === QUIZ ROUND {roundCounter / QUIZ_INTERVAL} ===");
                    StartQuizRound();
                    return; // Don't start next turn yet
                }
                
                // Check max turns
                if (currentTurn > maxTurns)
                {
                    Debug.Log("[GameManager] Max turns reached! Calculating final scores...");
                    EndGame();
                    return;
                }
            }
            
            // Start next turn
            StartTurn();
        }
        
        /// <summary>
        /// Check win condition
        /// </summary>
        private bool CheckWinCondition()
        {
            // Count non-bankrupt players
            int activePlayers = 0;
            foreach (var player in players)
            {
                if (!player.IsBankrupt())
                {
                    activePlayers++;
                }
            }
            
            // Win if only 1 player left
            return activePlayers <= 1;
        }
        
        /// <summary>
        /// End game
        /// </summary>
        private void EndGame()
        {
            isGameActive = false;
            
            Debug.Log("[GameManager] Game ended!");
            
            // Find winner (player with most money)
            PlayerGameController winner = null;
            int maxMoney = int.MinValue;
            
            foreach (var player in players)
            {
                if (player.Money > maxMoney)
                {
                    maxMoney = player.Money;
                    winner = player;
                }
            }
            
            if (winner != null)
            {
                Debug.Log($"[GameManager] Winner: {winner.PlayerName} with {winner.Money} money!");
            }
            
            // TODO: Show game end screen
        }
        
        /// <summary>
        /// Update turn display
        /// </summary>
        private void UpdateTurnDisplay()
        {
            if (turnText != null)
            {
                turnText.text = $"Turn: {currentTurn}/{maxTurns}";
            }
            
            if (currentPlayerText != null && CurrentPlayer != null)
            {
                currentPlayerText.text = $"Current: {CurrentPlayer.PlayerName}";
            }
        }
        
        /// <summary>
        /// Update time display
        /// </summary>
        private void UpdateTimeDisplay()
        {
            if (timeText != null)
            {
                float elapsed = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        // ========== MULTIPLAYER METHODS ==========
        
        /// <summary>
        /// CLIENT → HOST: Send loadout data
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SendLoadoutToHostServerRpc(PlayerLoadoutData loadout, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            playerLoadouts[clientId] = loadout;
            
            Debug.Log($"[Host] Received loadout from Client {clientId}: {loadout.playerName} (HP:{loadout.health} AGI:{loadout.agility} INT:{loadout.intelligence} LUCK:{loadout.luck} RES:{loadout.resistance})");
            
            // Check if all loadouts received
            if (playerLoadouts.Count >= NetworkManager.Singleton.ConnectedClients.Count)
            {
                Debug.Log($"[Host] All {playerLoadouts.Count} loadouts received! Starting game...");
            }
        }
        
        /// <summary>
        /// HOST: Spawn all players from loadouts
        /// </summary>
        private void SpawnAllPlayers()
        {
            if (!IsHost) return;
            
            Debug.Log($"[Host] Spawning {playerLoadouts.Count} players...");
            
            int playerIndex = 0;
            foreach (var kvp in playerLoadouts)
            {
                ulong clientId = kvp.Key;
                PlayerLoadoutData loadout = kvp.Value;
                
                SpawnPlayerNetwork(
                    loadout.playerName,
                    loadout.playerId,
                    loadout.isMale,
                    loadout.health,
                    loadout.agility,
                    loadout.intelligence,
                    loadout.luck,
                    loadout.resistance,
                    clientId,
                    loadout.GetSkillCardIds() // GET SKILL CARDS!
                );
                
                playerIndex++;
            }
            
            Debug.Log($"[Host] Spawned {players.Count} players successfully!");
        }
        
        // ========== TURN ORDER SELECTION ==========
        
        /// <summary>
        /// HOST: Start turn order selection phase
        /// </summary>
        private void StartTurnOrderSelection()
        {
            if (!IsHost) return;
            
            isTurnOrderPhase = true;
            turnOrderRolls.Clear();
            
            Debug.Log("[Host] === STARTING TURN ORDER SELECTION ===");
            
            // Notify all clients to roll dice
            NotifyTurnOrderPhaseClientRpc();
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Roll dice for turn order
        /// </summary>
        [ClientRpc]
        private void NotifyTurnOrderPhaseClientRpc()
        {
            Debug.Log("[Client] Rolling for turn order...");
            
            // Auto roll after short delay
            StartCoroutine(RollForTurnOrder());
        }
        
        /// <summary>
        /// Roll dice for turn order (auto)
        /// </summary>
        private IEnumerator RollForTurnOrder()
        {
            // Animation delay
            yield return new WaitForSeconds(0.5f);
            
            // Roll 2 dice
            int diceResult = Random.Range(1, 7) + Random.Range(1, 7);
            
            Debug.Log($"[Client] Rolled {diceResult} for turn order");
            
            // Send to Host
            SendTurnOrderRollServerRpc(diceResult);
        }
        
        /// <summary>
        /// CLIENT → HOST: Send turn order roll result
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SendTurnOrderRollServerRpc(int diceResult, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            
            turnOrderRolls.Add(new TurnOrderRoll {
                clientId = clientId,
                diceResult = diceResult
            });
            
            Debug.Log($"[Host] Client {clientId} rolled {diceResult} for turn order");
            
            // Check if all clients have rolled
            if (turnOrderRolls.Count >= NetworkManager.Singleton.ConnectedClients.Count)
            {
                FinalizeTurnOrder();
            }
        }
        
        /// <summary>
        /// HOST: Finalize turn order based on rolls
        /// </summary>
        private void FinalizeTurnOrder()
        {
            if (!IsHost) return;
            
            // Sort by dice result (descending)
            turnOrderRolls.Sort((a, b) => b.diceResult.CompareTo(a.diceResult));
            
            Debug.Log("[Host] === FINALIZING TURN ORDER ===");
            
            // Reorder players list based on rolls
            List<PlayerGameController> orderedPlayers = new List<PlayerGameController>();
            
            for (int i = 0; i < turnOrderRolls.Count; i++)
            {
                ulong clientId = turnOrderRolls[i].clientId;
                
                // Find player with this clientId
                var player = players.Find(p => {
                    var netObj = p.GetComponent<NetworkObject>();
                    return netObj != null && netObj.OwnerClientId == clientId;
                });
                
                if (player != null)
                {
                    orderedPlayers.Add(player);
                    Debug.Log($"[Host] Position {i + 1}: {player.PlayerName} (rolled {turnOrderRolls[i].diceResult})");
                }
            }
            
            // Update players list
            players = orderedPlayers;
            
            Debug.Log($"[Host] Turn order finalized! First player: {players[0].PlayerName}");
            
            // Notify clients
            NotifyTurnOrderFinalizedClientRpc();
            
            // Start game
            isTurnOrderPhase = false;
            currentPlayerIndex = 0;
            StartTurn();
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Turn order finalized
        /// </summary>
        [ClientRpc]
        private void NotifyTurnOrderFinalizedClientRpc()
        {
            Debug.Log("[Client] ✅ Turn order finalized! Game starting...");
            // TODO: Update UI to show player order
        }
        
        // ========== QUIZ SYSTEM ==========
        
        /// <summary>
        /// HOST: Start quiz round for all players
        /// </summary>
        private void StartQuizRound()
        {
            if (!IsHost) return;
            
            isGameActive = false; // Pause game
            
            Debug.Log($"[Host] === STARTING QUIZ ROUND ===");
            
            // Start quiz coroutine
            StartCoroutine(QuizAllPlayersCoroutine());
        }
        
        /// <summary>
        /// Quiz all players one by one
        /// </summary>
        private IEnumerator QuizAllPlayersCoroutine()
        {
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                
                Debug.Log($"[Host] Quizzing player {i + 1}/{players.Count}: {player.PlayerName}");
                
                // TODO: Get quiz from Firebase
                // For now, use demo quiz
                
                bool answeredCorrectly = false;
                bool quizCompleted = false;
                
                // Notify client to show quiz
                NotifyShowQuizClientRpc(i);
                
                // Wait for answer (timeout after 30 seconds)
                float waitTime = 0f;
                float timeout = 30f;
                
                while (!quizCompleted && waitTime < timeout)
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }
                
                // If timeout, treat as wrong answer
                if (!quizCompleted)
                {
                    Debug.Log($"[Host] Player {player.PlayerName} timed out!");
                    answeredCorrectly = false;
                }
                
                // Apply penalty if wrong
                if (!answeredCorrectly)
                {
                    ApplyQuizPenalty(player);
                }
                
                // Delay between players
                yield return new WaitForSeconds(2f);
            }
            
            Debug.Log("[Host] === QUIZ ROUND COMPLETED ===");
            
            // Resume game
            isGameActive = true;
            StartTurn();
        }
        
        /// <summary>
        /// HOST → CLIENT: Show quiz panel
        /// </summary>
        [ClientRpc]
        private void NotifyShowQuizClientRpc(int playerIndex)
        {
            // Only show quiz for the specific player
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            var localPlayer = players.Find(p => {
                var netObj = p.GetComponent<NetworkObject>();
                return netObj != null && netObj.OwnerClientId == localClientId;
            });
            
            int localPlayerIndex = players.IndexOf(localPlayer);
            
            if (localPlayerIndex == playerIndex)
            {
                // This is my turn to answer quiz
                Debug.Log("[Client] 📝 My turn to answer quiz!");
                
                if (panelQuiz != null)
                {
                    panelQuiz.Show((isCorrect) => {
                        // Send answer to Host
                        SendQuizAnswerServerRpc(isCorrect);
                    });
                }
            }
            else
            {
                // Show waiting message
                Debug.Log($"[Client] Waiting for {players[playerIndex].PlayerName} to answer quiz...");
            }
        }
        
        /// <summary>
        /// CLIENT → HOST: Send quiz answer
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SendQuizAnswerServerRpc(bool isCorrect, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            
            Debug.Log($"[Host] Received quiz answer from Client {clientId}: {(isCorrect ? "CORRECT" : "WRONG")}");
            
            // Mark quiz as completed
            // (This will be handled by the coroutine)
        }
        
        /// <summary>
        /// Apply penalty for wrong quiz answer
        /// </summary>
        private void ApplyQuizPenalty(PlayerGameController player)
        {
            // Random penalty
            int penaltyType = Random.Range(0, 3);
            
            switch (penaltyType)
            {
                case 0: // Lose money
                    int moneyLoss = Random.Range(100, 300);
                    player.SubtractMoney(moneyLoss);
                    Debug.Log($"[Host] Quiz Penalty: {player.PlayerName} lost {moneyLoss} money");
                    NotifyPenaltyClientRpc($"{player.PlayerName} lost {moneyLoss} money!");
                    break;
                    
                case 1: // Downgrade property
                    // TODO: Implement property downgrade
                    Debug.Log($"[Host] Quiz Penalty: {player.PlayerName} downgraded a property (TODO)");
                    NotifyPenaltyClientRpc($"{player.PlayerName} downgraded a property!");
                    break;
                    
                case 2: // Skip next turn
                    player.SetSkipNextTurn(true);
                    Debug.Log($"[Host] Quiz Penalty: {player.PlayerName} will skip next turn");
                    NotifyPenaltyClientRpc($"{player.PlayerName} will skip next turn!");
                    break;
            }
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Notify penalty
        /// </summary>
        [ClientRpc]
        private void NotifyPenaltyClientRpc(string message)
        {
            Debug.Log($"[Client] ⚠️ {message}");
            // TODO: Show penalty UI
        }
    }
}

