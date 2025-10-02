using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using AntKnow.Auth;

namespace AntKnow.Game
{
    /// <summary>
    /// Main game controller
    /// Tích hợp với lobby system và multiplayer
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        [Header("Managers")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private DiceController diceController;

        [Header("Players")]
        [SerializeField] private List<PlayerGameController> players = new List<PlayerGameController>();
        [SerializeField] private GameObject playerPrefab;

        [Header("UI")]
        [SerializeField] private Button rollButton;
        [SerializeField] private TMPro.TextMeshProUGUI turnText;
        [SerializeField] private TMPro.TextMeshProUGUI currentPlayerText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

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
                // Demo: Spawn test players
                SpawnTestPlayer("Player 1", "test_player_1", true, 10, 10, 10, 10, 10);
                SpawnTestPlayer("Player 2", "test_player_2", false, 5, 15, 10, 20, 5);
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
        /// Load players from lobby (multiplayer)
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

            // Wait a bit for all clients to connect
            yield return new WaitForSeconds(1f);

            // For now, spawn local player from session data
            // TODO: Implement proper multiplayer player spawning
            string playerName = sessionData.playerName ?? "Player";
            string playerId = sessionData.firebaseUID ?? "unknown";
            bool isMale = sessionData.gender == "male";

            // Get stats from session
            int hp = sessionData.totalHealth;
            int agi = sessionData.totalAgility;
            int intel = sessionData.totalIntelligence;
            int lck = sessionData.totalLuck;
            int res = sessionData.totalResistance;

            Debug.Log($"[GameManager] Loading player: {playerName} (HP:{hp} AGI:{agi} INT:{intel} LUCK:{lck} RES:{res})");

            // Spawn player
            if (IsServer)
            {
                // Get local client ID
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                SpawnPlayerNetwork(playerName, playerId, isMale, hp, agi, intel, lck, res, clientId);
            }
            else
            {
                // Client: Spawn local player (non-network for now)
                SpawnTestPlayer(playerName, playerId, isMale, hp, agi, intel, lck, res);
            }

            Debug.Log($"[GameManager] Loaded {players.Count} players from lobby");
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
        private void SpawnPlayerNetwork(string name, string id, bool isMale, int hp, int agi, int intel, int lck, int res, ulong clientId)
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
                players.Add(player);

                Debug.Log($"[GameManager] Spawned network player: {name} (ClientId: {clientId})");
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
        /// Roll dice and move player
        /// </summary>
        private IEnumerator RollAndMove()
        {
            PlayerGameController player = CurrentPlayer;
            
            // Roll dice with luck stat
            yield return diceController.RollDice(player.Luck);
            
            int steps = diceController.LastSum;
            
            // Move player
            yield return player.MoveBySteps(steps);
            
            // Resolve tile
            ResolveTile(player);
            
            // End turn (for demo, auto end)
            yield return new WaitForSeconds(1f);
            EndTurn();
        }
        
        /// <summary>
        /// Resolve tile effect
        /// </summary>
        private void ResolveTile(PlayerGameController player)
        {
            int tileIndex = player.CurrentTile;
            TileType tileType = boardManager.GetTileType(tileIndex);
            string tileName = boardManager.GetTileName(tileIndex);
            
            Debug.Log($"[GameManager] {player.PlayerName} landed on {tileName} (Type: {tileType})");
            
            switch (tileType)
            {
                case TileType.Start:
                    // Already handled in MoveBySteps
                    break;
                    
                case TileType.Property:
                    // TODO: Show property panel
                    Debug.Log($"[GameManager] Property tile - TODO: Show buy/rent panel");
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
                    player.SetJailCounter(3);
                    Debug.Log($"[GameManager] Jail tile - {player.PlayerName} in jail for 3 turns");
                    break;
                    
                case TileType.Travel:
                    player.SubtractMoney(100);
                    Debug.Log($"[GameManager] Travel tile - {player.PlayerName} pays 100");
                    break;
            }
        }
        
        /// <summary>
        /// End turn
        /// </summary>
        private void EndTurn()
        {
            if (!isGameActive) return;
            
            Debug.Log($"[GameManager] Turn {currentTurn} ended");
            
            // Check win condition
            if (CheckWinCondition())
            {
                EndGame();
                return;
            }
            
            // Next player
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            
            // If back to first player, increment turn
            if (currentPlayerIndex == 0)
            {
                currentTurn++;
                
                // Check max turns
                if (currentTurn > maxTurns)
                {
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
    }
}

