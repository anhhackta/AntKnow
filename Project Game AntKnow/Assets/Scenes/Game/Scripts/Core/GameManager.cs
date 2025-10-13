using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using AntKnow.Auth;
using Firebase.Firestore;

namespace AntKnow.Game
{
    // ===== HELPER CLASSES =====

    /// <summary>
    /// Quiz data helper (for simplified quiz system)
    /// </summary>
    public class QuizData
    {
        public string question;
        public string[] options;
        public int correctAnswer;
    }

    /// <summary>
    /// Serialized quiz payload for network transport
    /// </summary>
    [System.Serializable]
    public struct QuizQuestionPayload : INetworkSerializable
    {
        public string question;
        public string option0;
        public string option1;
        public string option2;
        public string option3;
        public int correctAnswer;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref question);
            serializer.SerializeValue(ref option0);
            serializer.SerializeValue(ref option1);
            serializer.SerializeValue(ref option2);
            serializer.SerializeValue(ref option3);
            serializer.SerializeValue(ref correctAnswer);
        }

        public static QuizQuestionPayload FromQuizData(QuizData data)
        {
            QuizQuestionPayload payload = new QuizQuestionPayload
            {
                question = data?.question ?? "Question",
                correctAnswer = data?.correctAnswer ?? 0
            };

            string[] options = data?.options ?? System.Array.Empty<string>();
            payload.option0 = options.Length > 0 ? options[0] : "Option 1";
            payload.option1 = options.Length > 1 ? options[1] : "Option 2";
            payload.option2 = options.Length > 2 ? options[2] : "Option 3";
            payload.option3 = options.Length > 3 ? options[3] : "Option 4";

            return payload;
        }

        public QuizData ToQuizData()
        {
            return new QuizData
            {
                question = question,
                options = new[] { option0, option1, option2, option3 },
                correctAnswer = Mathf.Clamp(correctAnswer, 0, 3)
            };
        }
    }

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
    /// REQUIREMENT: GameObject phải có NetworkObject component!
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class GameManager : NetworkBehaviour
    {
        // ===== SINGLETON =====
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private PanelRoll panelRoll;
        [SerializeField] private PropertyManager propertyManager;

        [Header("Players")]
        [SerializeField] private List<PlayerGameController> players = new List<PlayerGameController>();
        [SerializeField] private GameObject playerPrefabMale;   // Male player prefab
        [SerializeField] private GameObject playerPrefabFemale; // Female player prefab

        [Header("UI")]
        [SerializeField] private Button rollButton;
        [SerializeField] private TMPro.TextMeshProUGUI turnText;
        [SerializeField] private TMPro.TextMeshProUGUI currentPlayerText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        [Header("UI Panels")]
        public PanelGame panelGame; // ⭐ Panel chính quản lý PanelMe và PanelPlayer (changed to public)
        [SerializeField] private PanelBuy panelBuy;
        [SerializeField] private PanelQuiz panelQuiz;
        [SerializeField] private PanelEvent panelEvent;
        [SerializeField] private PanelHouseSell panelHouseSell;
        [SerializeField] private PanelResult panelResult;
        [SerializeField] private PanelCard panelCard;
        [SerializeField] private PanelNotification panelNotification; // ⭐ Thông báo nhanh

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
        private int quizSessionCounter = 0;
        private int activeQuizSessionId = -1;
        private int activeQuizPlayerIndex = -1;
        private ulong activeQuizClientId = ulong.MaxValue;
        private bool activeQuizAnswerReceived = false;
        private bool activeQuizAnswerCorrect = false;
        private QuizQuestionPayload activeQuizPayload;
        private const float QUIZ_TIMEOUT_SECONDS = 30f;
        private int localQuizSessionId = -1;

        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[GameManager] Multiple GameManager instances detected! Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Get player by index
        /// </summary>
        public PlayerGameController GetPlayer(int index)
        {
            if (index >= 0 && index < players.Count)
            {
                return players[index];
            }
            return null;
        }

        private void Start()
        {
            // Find services if not assigned
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Validate prefab assignments
            if (playerPrefabMale == null || playerPrefabFemale == null)
            {
                Debug.LogError("[GameManager] Player prefabs not assigned! Please assign both playerPrefabMale and playerPrefabFemale in Inspector.");
                return;
            }

            // Wait for network ready
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                StartGame();
            }
            else if (demoMode)
            {
                // Demo mode: Start immediately without network
                Debug.Log("[GameManager] Demo Mode: Starting game without network...");
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
            GameObject prefabToUse = isMale ? playerPrefabMale : playerPrefabFemale;
            
            if (prefabToUse == null)
            {
                Debug.LogError($"[GameManager] {(isMale ? "Male" : "Female")} player prefab not assigned!");
                return;
            }

            // Spawn at tile 0
            Vector3 spawnPos = Vector3.zero;
            if (boardManager != null)
            {
                spawnPos = boardManager.GetWaypointPosition(0);
            }
            else
            {
                Debug.LogWarning("[GameManager] BoardManager not assigned! Spawning at origin.");
            }
            
            GameObject playerObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity);

            PlayerGameController player = playerObj.GetComponent<PlayerGameController>();
            if (player != null)
            {
                player.Initialize(name, id, isMale, hp, agi, intel, lck, res);
                players.Add(player);

                // ⭐ SET PLAYER INDEX cho màu sắc
                player.SetPlayerIndex(players.Count - 1);

                Debug.Log($"[GameManager] Spawned {(isMale ? "male" : "female")} test player: {name} (Index: {players.Count - 1})");

                // ⭐ INITIALIZE PANELGAME với local player (Demo Mode)
                if (demoMode && panelGame != null)
                {
                    panelGame.Initialize(player);
                    Debug.Log($"[GameManager] Initialized PanelGame for {name}");
                }
            }
        }

        /// <summary>
        /// Spawn player for multiplayer
        /// </summary>
        private void SpawnPlayerNetwork(string name, string id, bool isMale, int hp, int agi, int intel, int lck, int res, ulong clientId, List<string> skillCardIds)
        {
            GameObject prefabToUse = isMale ? playerPrefabMale : playerPrefabFemale;
            
            if (prefabToUse == null)
            {
                Debug.LogError($"[GameManager] {(isMale ? "Male" : "Female")} player prefab not assigned!");
                return;
            }

            // Spawn at tile 0
            Vector3 spawnPos = Vector3.zero;
            if (boardManager != null)
            {
                spawnPos = boardManager.GetWaypointPosition(0);
            }
            else
            {
                Debug.LogWarning("[GameManager] BoardManager not assigned! Spawning at origin.");
            }
            
            GameObject playerObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity);

            // Spawn as network object
            NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.SpawnAsPlayerObject(clientId);
                Debug.Log($"[GameManager] NetworkObject spawned for client {clientId}");
            }
            else
            {
                Debug.LogError($"[GameManager] NetworkObject component not found on player prefab!");
            }

            PlayerGameController player = playerObj.GetComponent<PlayerGameController>();
            if (player != null)
            {
                player.Initialize(name, id, isMale, hp, agi, intel, lck, res);
                player.SetSkillCards(skillCardIds); // SET SKILL CARDS!
                players.Add(player);
                
                // ⭐ SET PLAYER INDEX cho màu sắc (0 = Red, 1 = Blue, 2 = Green, 3 = Yellow)
                player.SetPlayerIndex(players.Count - 1);

                Debug.Log($"[GameManager] Spawned network player: {name} (ClientId: {clientId}, Index: {players.Count - 1}) with {skillCardIds.Count} skill cards");
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

            // ⭐ Enable PanelRoll button (if exists)
            if (panelRoll != null)
            {
                panelRoll.SetRollButtonEnabled(true);
            }
        }

        /// <summary>
        /// Update turn indicators (ping trên đầu player)
        /// </summary>
        private void UpdateTurnIndicators()
        {
            if (!demoMode && !IsServer)
            {
                return;
            }
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

            // ⭐ Disable PanelRoll button (if exists)
            if (panelRoll != null)
            {
                panelRoll.SetRollButtonEnabled(false);
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

            // ⭐ Demo Mode: Không cần check IsHost
            if (!demoMode && !IsHost)
            {
                yield break; // Only Host can roll dice (multiplayer only)
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

            // Notify all clients of dice result (Multiplayer only)
            if (!demoMode)
            {
                NotifyDiceRolledClientRpc(currentPlayerIndex, die1, die2, diceResult, isDouble, wasLuckyDouble);
            }
            else
            {
                // ⭐ Demo Mode: Show dice animation locally
                Debug.Log($"[Demo] Dice result: {die1} + {die2} = {diceResult}");
                if (panelRoll != null)
                {
                    StartCoroutine(panelRoll.RollDice(die1, die2, isDouble, wasLuckyDouble));
                }
            }

            // Wait for dice animation
            yield return new WaitForSeconds(1.5f);
            
            // Move player
            yield return player.MoveBySteps(diceResult);

            // Resolve tile
            ResolveTile(player);

            // ⭐ KHÔNG TỰ ĐỘNG END TURN - Chờ panel đóng
            // Panel sẽ tự gọi EndTurn() khi user chọn xong
            // Nếu không có panel (Start, Jail, Travel, etc.) → Auto end turn
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
                    // Already handled in MoveBySteps (OnPassStart)
                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} đến Ô Bắt Đầu!");
                    }
                    // ⭐ Auto end turn (no panel)
                    StartCoroutine(AutoEndTurnAfterDelay(1f));
                    break;

                case TileType.Property:
                    ResolvePropertyTile(player, tileIndex, tileName, basePrice);
                    break;

                case TileType.Event:
                    // ⭐ Show event panel
                    Debug.Log($"[GameManager] Event tile - Showing event panel");
                    if (panelEvent != null)
                    {
                        panelEvent.ShowRandomEvent((moneyChange) => {
                            // Apply money change
                            if (moneyChange > 0)
                            {
                                player.AddMoney(moneyChange);
                                Debug.Log($"[GameManager] {player.PlayerName} gained {moneyChange} from event");
                            }
                            else if (moneyChange < 0)
                            {
                                player.SubtractMoney(-moneyChange);
                                Debug.Log($"[GameManager] {player.PlayerName} lost {-moneyChange} from event");
                            }

                            // ⭐ UPDATE UI - Refresh panels to show new money
                            if (panelGame != null)
                            {
                                panelGame.UpdateAllPanels();
                                Debug.Log($"[GameManager] Updated panels after event - Player money: {player.Money}");
                            }

                            // ⭐ End turn after event
                            StartCoroutine(AutoEndTurnAfterDelay(0.5f));
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[GameManager] PanelEvent not assigned!");
                        // ⭐ Auto end turn if no panel
                        StartCoroutine(AutoEndTurnAfterDelay(1f));
                    }
                    break;

                case TileType.Quiz:
                    // ✅ SIMPLIFIED: Use PanelNotification for quiz
                    Debug.Log($"[GameManager] Quiz tile - Loading question...");
                    StartCoroutine(HandleQuizTile(player));
                    break;

                case TileType.Jail:
                    player.SetJailCounter(2); // 2 turns in jail
                    Debug.Log($"[GameManager] Jail tile - {player.PlayerName} in jail for 2 turns");
                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} bị giam 2 lượt!");
                    }
                    // ⭐ Auto end turn (no panel)
                    StartCoroutine(AutoEndTurnAfterDelay(1f));
                    break;

                case TileType.Travel:
                    player.SubtractMoney(100);
                    Debug.Log($"[GameManager] Travel tile - {player.PlayerName} pays 100");

                    // ⭐ UPDATE UI - Refresh panels to show new money
                    if (panelGame != null)
                    {
                        panelGame.UpdateAllPanels();
                        Debug.Log($"[GameManager] Updated panels after travel - Player money: {player.Money}");
                    }

                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} đi du lịch! -100");
                    }
                    // ⭐ Auto end turn (no panel)
                    StartCoroutine(AutoEndTurnAfterDelay(1f));
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

                    // Get money before paying rent
                    int moneyBefore = player.Money;

                    propertyManager.PayRent(tileIndex, basePrice, player, owner);

                    // Calculate actual rent paid (money lost)
                    int rentPaid = moneyBefore - player.Money;

                    // ⭐ UPDATE UI - Refresh panels to show new money
                    if (panelGame != null)
                    {
                        panelGame.UpdateAllPanels();
                        Debug.Log($"[GameManager] Updated panels after rent - Player money: {player.Money}, Owner money: {owner.Money}");
                    }

                    // ⭐ Show notification
                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} trả {rentPaid} cho {owner.PlayerName}");
                    }

                    // Check bankruptcy
                    if (player.IsBankrupt())
                    {
                        Debug.Log($"[GameManager] {player.PlayerName} is bankrupt!");
                        ShowSellPanel(player);
                        // Turn will end after sell panel closes
                    }
                    else
                    {
                        // ⭐ End turn after paying rent
                        StartCoroutine(AutoEndTurnAfterDelay(1f));
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
                Debug.LogWarning("[GameManager] PanelBuy not assigned! Auto-buying property...");

                // Auto buy for demo
                if (player.Money >= basePrice)
                {
                    propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);

                    // ⭐ Show notification
                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} mua {tileName} ({basePrice})");
                    }
                }
                else
                {
                    Debug.Log($"[GameManager] {player.PlayerName} không đủ tiền mua {tileName}");

                    // ⭐ Show notification
                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} không đủ tiền!");
                    }
                }
                return;
            }

            Debug.Log($"[GameManager] Showing PanelBuy for {tileName}");

            panelBuy.ShowBuy(tileName, basePrice, player.Money,
                // ⭐ onBuy callback
                (selectedLevel) =>
                {
                    if (selectedLevel > 0)
                    {
                        // ⭐ DEBUG: Check before buying
                        Debug.Log($"[GameManager] Attempting to buy property - Tile: {tileIndex}, Player: {playerIdx}, Price: {basePrice}, PlayerMoney: {player.Money}, SelectedLevel: {selectedLevel}");
                        Debug.Log($"[GameManager] PropertyManager is null? {propertyManager == null}");

                        // Buy property
                        bool buySuccess = propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);

                        Debug.Log($"[GameManager] BuyProperty returned: {buySuccess}");

                        if (buySuccess)
                        {
                            Debug.Log($"[GameManager] {player.PlayerName} bought {tileName} for {basePrice}");

                            // Show notification
                            if (panelNotification != null)
                            {
                                panelNotification.ShowNotification($"{player.PlayerName} mua {tileName} ({basePrice})");
                            }

                            // ⭐ FIX: Upgrade to selectedLevel directly
                            // selectedLevel = 1 → level 1 (1 house)
                            // selectedLevel = 2 → level 2 (2 houses)
                            // selectedLevel = 5 → level 5 (hotel)
                            if (selectedLevel > 0)
                            {
                                Debug.Log($"[GameManager] Attempting to upgrade to level {selectedLevel}");
                                bool upgradeSuccess = propertyManager.UpgradeProperty(tileIndex, selectedLevel, basePrice, player);
                                Debug.Log($"[GameManager] UpgradeProperty returned: {upgradeSuccess}");
                            }

                            // ⭐ UPDATE UI - Refresh PanelMe to show new money
                            if (panelGame != null)
                            {
                                panelGame.UpdateAllPanels();
                                Debug.Log($"[GameManager] Updated PanelMe - New money: {player.Money}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[GameManager] BuyProperty FAILED! Check PropertyManager logs above.");
                        }
                    }

                    // ⭐ End turn after buying/skipping
                    StartCoroutine(AutoEndTurnAfterDelay(0.5f));
                },
                // ⭐ onSkip callback
                () =>
                {
                    Debug.Log($"[GameManager] {player.PlayerName} skipped buying {tileName}");

                    // Show notification
                    if (panelNotification != null)
                    {
                        panelNotification.ShowNotification($"{player.PlayerName} bỏ qua {tileName}");
                    }

                    // ⭐ End turn after skipping
                    StartCoroutine(AutoEndTurnAfterDelay(0.5f));
                }
            );
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
        /// Auto end turn sau delay (dùng cho các ô không mở panel riêng)
        /// </summary>
        private IEnumerator AutoEndTurnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Debug.Log("[GameManager] Auto ending turn after delay");
            EndTurn();
        }

        private void ResetActiveQuizState()
        {
            activeQuizSessionId = -1;
            activeQuizPlayerIndex = -1;
            activeQuizClientId = ulong.MaxValue;
            activeQuizAnswerReceived = false;
            activeQuizAnswerCorrect = false;
            activeQuizPayload = default;
        }

        private QuizData NormalizeQuizData(QuizData quizData)
        {
            if (quizData == null)
            {
                quizData = new QuizData
                {
                    question = "2 + 2 = ?",
                    options = new[] { "3", "4", "5", "6" },
                    correctAnswer = 1
                };
            }

            if (quizData.options == null || quizData.options.Length < 4)
            {
                string[] fallbackOptions = { "3", "4", "5", "6" };
                string[] fixedOptions = new string[4];
                for (int i = 0; i < fixedOptions.Length; i++)
                {
                    if (quizData.options != null && i < quizData.options.Length && !string.IsNullOrEmpty(quizData.options[i]))
                    {
                        fixedOptions[i] = quizData.options[i];
                    }
                    else
                    {
                        fixedOptions[i] = fallbackOptions[i];
                    }
                }
                quizData.options = fixedOptions;
            }

            quizData.correctAnswer = Mathf.Clamp(quizData.correctAnswer, 0, 3);
            return quizData;
        }

        private IEnumerator ExecuteQuizSession(PlayerGameController player, QuizData quizData, bool showStartNotification, System.Action<bool> onResult)
        {
            if (player == null)
            {
                yield break;
            }

            int playerIndex = players.IndexOf(player);
            if (playerIndex < 0)
            {
                Debug.LogWarning("[GameManager] ExecuteQuizSession called for unknown player");
                yield break;
            }

            if (!demoMode && !IsHost)
            {
                yield break;
            }

            quizData = NormalizeQuizData(quizData);

            quizSessionCounter++;
            activeQuizSessionId = quizSessionCounter;
            activeQuizPlayerIndex = playerIndex;
            activeQuizAnswerReceived = false;
            activeQuizAnswerCorrect = false;
            activeQuizPayload = QuizQuestionPayload.FromQuizData(quizData);

            ulong clientId = 0;
            var networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                clientId = networkObject.OwnerClientId;
            }
            activeQuizClientId = clientId;

            int sessionId = activeQuizSessionId;
            Debug.Log($"[GameManager] Starting quiz session {sessionId} for {player.PlayerName} (Client {clientId})");

            if (showStartNotification && panelNotification != null)
            {
                panelNotification.ShowNotification($"{player.PlayerName} dang tra loi cau hoi...");
            }

            if (demoMode)
            {
                if (panelQuiz != null)
                {
                    panelQuiz.Show(quizData, isCorrect =>
                    {
                        activeQuizAnswerReceived = true;
                        activeQuizAnswerCorrect = isCorrect;
                    }, false, QUIZ_TIMEOUT_SECONDS);
                }
                else
                {
                    activeQuizAnswerReceived = true;
                    activeQuizAnswerCorrect = Random.Range(0, 2) == 0;
                }
            }
            else
            {
                StartQuizClientRpc(sessionId, playerIndex, activeQuizPayload, QUIZ_TIMEOUT_SECONDS);
            }

            float elapsed = 0f;
            while (!activeQuizAnswerReceived && elapsed < QUIZ_TIMEOUT_SECONDS)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            bool answeredCorrectly = activeQuizAnswerReceived && activeQuizAnswerCorrect;

            if (!activeQuizAnswerReceived)
            {
                Debug.Log($"[GameManager] Quiz session {sessionId} timed out for {player.PlayerName}");
                if (!demoMode)
                {
                    QuizTimeoutClientRpc(sessionId, playerIndex);
                }
            }

            if (!demoMode)
            {
                QuizAnsweredClientRpc(sessionId, playerIndex, answeredCorrectly);
            }
            else if (panelQuiz != null)
            {
                panelQuiz.Hide();
            }

            if (demoMode && panelNotification != null)
            {
                panelNotification.ShowNotification(answeredCorrectly ? $"{player.PlayerName} tra loi dung!" : $"{player.PlayerName} tra loi sai!");
            }

            if (!demoMode)
            {
                QuizCompleteClientRpc(sessionId);
            }

            ResetActiveQuizState();

            onResult?.Invoke(answeredCorrectly);
        }

        /// <summary>
        /// Handle quiz tile (host authoritative quiz round)
        /// </summary>
        private IEnumerator HandleQuizTile(PlayerGameController player)
        {
            if (player == null)
            {
                yield break;
            }

            if (!demoMode && !IsHost)
            {
                yield break;
            }

            QuizData quizData = null;
            yield return StartCoroutine(LoadRandomQuizCoroutine(data => quizData = data));

            bool answeredCorrectly = false;
            yield return StartCoroutine(ExecuteQuizSession(player, quizData, true, result => answeredCorrectly = result));

            if (!answeredCorrectly)
            {
                ApplyQuizPenalty(player);
            }
            else
            {
                Debug.Log($"[GameManager] {player.PlayerName} answered quiz correctly.");
            }

            if (panelGame != null)
            {
                panelGame.UpdateAllPanels();
            }

            yield return new WaitForSeconds(2f);
            EndTurn();
        }

        [ClientRpc]
        private void StartQuizClientRpc(int sessionId, int playerIndex, QuizQuestionPayload quizPayload, float timeoutSeconds)
        {
            if (demoMode)
            {
                return;
            }

            localQuizSessionId = sessionId;
            QuizData quizData = quizPayload.ToQuizData();

            string playerName = (playerIndex >= 0 && playerIndex < players.Count)
                ? players[playerIndex].PlayerName
                : $"Player {playerIndex + 1}";

            int localPlayerIndex = -1;
            if (NetworkManager.Singleton != null)
            {
                ulong localClientId = NetworkManager.Singleton.LocalClientId;
                var localPlayer = players.Find(p =>
                {
                    var netObj = p.GetComponent<NetworkObject>();
                    return netObj != null && netObj.OwnerClientId == localClientId;
                });
                localPlayerIndex = players.IndexOf(localPlayer);
            }

            if (localPlayerIndex == playerIndex)
            {
                Debug.Log("[Client] Local player answering quiz");
                if (panelQuiz != null)
                {
                    panelQuiz.Show(quizData, isCorrect =>
                    {
                        SubmitQuizAnswerServerRpc(sessionId, isCorrect);
                    }, false, timeoutSeconds);
                }
                else
                {
                    Debug.LogWarning("[Client] PanelQuiz missing, auto-submitting wrong answer");
                    SubmitQuizAnswerServerRpc(sessionId, false);
                }
            }
            else if (panelNotification != null)
            {
                panelNotification.ShowNotification($"{playerName} dang tra loi cau hoi...");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitQuizAnswerServerRpc(int sessionId, bool isCorrect, ServerRpcParams rpcParams = default)
        {
            if (demoMode)
            {
                return;
            }

            if (sessionId != activeQuizSessionId)
            {
                Debug.LogWarning($"[GameManager] Ignoring quiz answer for session {sessionId} (active {activeQuizSessionId})");
                return;
            }

            ulong senderClientId = rpcParams.Receive.SenderClientId;
            if (senderClientId != activeQuizClientId)
            {
                Debug.LogWarning($"[GameManager] Quiz answer from unexpected client {senderClientId}");
                return;
            }

            activeQuizAnswerReceived = true;
            activeQuizAnswerCorrect = isCorrect;

            Debug.Log($"[GameManager] Quiz answer received for session {sessionId}: {(isCorrect ? "CORRECT" : "WRONG")}");
        }

        [ClientRpc]
        private void QuizAnsweredClientRpc(int sessionId, int playerIndex, bool isCorrect)
        {
            string playerName = (playerIndex >= 0 && playerIndex < players.Count)
                ? players[playerIndex].PlayerName
                : $"Player {playerIndex + 1}";

            if (panelNotification != null)
            {
                panelNotification.ShowNotification(isCorrect
                    ? $"{playerName} tra loi dung!"
                    : $"{playerName} tra loi sai!");
            }

            if (localQuizSessionId == sessionId)
            {
                localQuizSessionId = -1;
            }
        }

        [ClientRpc]
        private void QuizTimeoutClientRpc(int sessionId, int playerIndex)
        {
            string playerName = (playerIndex >= 0 && playerIndex < players.Count)
                ? players[playerIndex].PlayerName
                : $"Player {playerIndex + 1}";
            if (panelNotification != null)
            {
                panelNotification.ShowNotification($"{playerName} het gio tra loi!");
            }

            if (localQuizSessionId == sessionId && panelQuiz != null)
            {
                panelQuiz.Hide();
            }

            if (localQuizSessionId == sessionId)
            {
                localQuizSessionId = -1;
            }
        }

        [ClientRpc]
        private void QuizCompleteClientRpc(int sessionId)
        {
            if (localQuizSessionId == sessionId)
            {
                localQuizSessionId = -1;
            }
        }

        private IEnumerator LoadRandomQuizCoroutine(System.Action<QuizData> callback)
        {
            QuizData quizData = null;
            bool loaded = false;

            // Call async method
            LoadRandomQuizAsync((data) => {
                quizData = data;
                loaded = true;
            });

            // Wait for load
            float timeout = 5f;
            float elapsed = 0f;
            while (!loaded && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            callback?.Invoke(quizData);
        }

        /// <summary>
        /// Load random quiz from Firebase (async)
        /// </summary>
        private async void LoadRandomQuizAsync(System.Action<QuizData> callback)
        {
            try
            {
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                float anchor = Random.Range(0f, 1f);

                Query query = db.Collection("quizzes")
                    .OrderBy("randomValue")
                    .StartAt(anchor)
                    .Limit(1);

                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count == 0)
                {
                    query = db.Collection("quizzes").OrderBy("randomValue").Limit(1);
                    snapshot = await query.GetSnapshotAsync();
                }

                if (snapshot.Count > 0)
                {
                    DocumentSnapshot doc = snapshot.Documents.First();
                    Dictionary<string, object> data = doc.ToDictionary();

                    QuizData quizData = new QuizData
                    {
                        question = data.ContainsKey("question") ? data["question"].ToString() : "",
                        correctAnswer = data.ContainsKey("correctAnswer") ? System.Convert.ToInt32(data["correctAnswer"]) : 0
                    };

                    // Parse options
                    if (data.ContainsKey("options") && data["options"] is List<object> optionsList)
                    {
                        quizData.options = new string[4];
                        for (int i = 0; i < 4 && i < optionsList.Count; i++)
                        {
                            quizData.options[i] = optionsList[i]?.ToString() ?? "";
                        }
                    }

                    callback?.Invoke(quizData);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] Error loading quiz: {e.Message}");
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// Try to downgrade a random property owned by player
        /// </summary>
        private bool TryDowngradeRandomProperty(PlayerGameController player)
        {
            if (propertyManager == null) return false;

            int playerIndex = players.IndexOf(player);
            List<int> ownedProperties = new List<int>();

            // Find all owned properties with level > 0
            for (int i = 0; i < 36; i++)
            {
                if (propertyManager.IsPropertyOwned(i) &&
                    propertyManager.GetPropertyOwner(i) == playerIndex &&
                    propertyManager.GetPropertyLevel(i) > 0)
                {
                    ownedProperties.Add(i);
                }
            }

            if (ownedProperties.Count == 0) return false;

            // Random property
            int randomIndex = Random.Range(0, ownedProperties.Count);
            int tileIndex = ownedProperties[randomIndex];
            int currentLevel = propertyManager.GetPropertyLevel(tileIndex);

            // Downgrade using PropertyManager
            propertyManager.SetPropertyLevel(tileIndex, currentLevel - 1);

            Debug.Log($"[GameManager] Downgraded property {tileIndex} from level {currentLevel} to {currentLevel - 1}");
            return true;
        }
        
        /// <summary>
        /// End turn - WITH ROUND TRACKING & QUIZ SYSTEM
        /// </summary>
        private void EndTurn()
        {
            if (!isGameActive) return;

            // ⭐ Demo Mode OR Host can manage turns
            if (!demoMode && !IsHost) return;

            Debug.Log($"[GameManager] Turn ended. Player {currentPlayerIndex}/{players.Count - 1}");

            // Reduce skill card cooldowns for current player
            if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
            {
                players[currentPlayerIndex].ReduceCooldowns();
            }

            // ✅ FIX: Demo Mode - Stay on same player
            if (demoMode)
            {
                Debug.Log("[GameManager] Demo Mode - Starting next turn for same player");
                currentTurn++;

                // Check max turns
                if (currentTurn > maxTurns)
                {
                    Debug.Log("[GameManager] Max turns reached!");
                    EndGame();
                    return;
                }

                // Start next turn immediately
                StartTurn();
                return;
            }

            // ⭐ MULTIPLAYER MODE BELOW

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
                bool answeredCorrectly = false;
                QuizData quizData = null;
                yield return StartCoroutine(LoadRandomQuizCoroutine(data => quizData = data));

                yield return StartCoroutine(ExecuteQuizSession(player, quizData, true, result => answeredCorrectly = result));

                if (!answeredCorrectly)
                {
                    ApplyQuizPenalty(player);
                }

                if (panelGame != null)
                {
                    panelGame.UpdateAllPanels();
                }

                yield return new WaitForSeconds(2f);
            }

            Debug.Log("[Host] === QUIZ ROUND COMPLETED ===");

            isGameActive = true;
            StartTurn();
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
