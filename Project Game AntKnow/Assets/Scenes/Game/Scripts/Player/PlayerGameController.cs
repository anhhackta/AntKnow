using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Controller cho player trong game
    /// NOTE: Mỗi prefab (Male/Female) đã có model riêng, không cần toggle
    /// REQUIREMENT: GameObject phải có NetworkObject component cho multiplayer!
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerGameController : NetworkBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private string playerId = "";
        [SerializeField] private bool isMale = true; // Set theo prefab (Male=true, Female=false)
        [SerializeField] private int playerIndex = 0; // 0-3 for colors (Red, Blue, Green, Yellow)
        
        [Header("Game State")]
        [SerializeField] private int currentTile = 0;
        [SerializeField] private int money = 10000; // Starting money
        [SerializeField] private int jailCounter = 0;
        [SerializeField] private bool skipNextTurn = false;
        
        [Header("Stats from Loadout")]
        [SerializeField] private int health = 0;
        [SerializeField] private int agility = 0;
        [SerializeField] private int intelligence = 0;
        [SerializeField] private int luck = 0;
        [SerializeField] private int resistance = 0;
        
        [Header("Skill Cards")]
        private List<string> skillCardIds = new List<string>(); // effectId list
        private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>(); // effectId -> cooldown turns
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float bounceHeight = 0.5f;
        [SerializeField] private float bounceDuration = 0.3f;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Vector3 boardCenter = Vector3.zero;

        [Header("Animation")]
        [SerializeField] private Animator animator; // Single animator (model đã có sẵn trong prefab)
        
        [Header("Turn Indicator")]
        [SerializeField] private TurnIndicator turnIndicator;

        // ===== EVENTS FOR UI UPDATES =====
        public event System.Action<int> OnMoneyChanged;
        public event System.Action<int> OnPositionChanged;
        public event System.Action<int> OnJailCounterChanged;

        // Public Properties (Simplified - no NetworkVariable overhead)
        public string PlayerName => playerName;
        public string PlayerId => playerId;
        public bool IsMale => isMale;
        public int PlayerIndex => playerIndex;
        public int CurrentTile => currentTile;
        public int Money => money;
        public int JailCounter => jailCounter;
        public bool SkipNextTurn => skipNextTurn;
        
        // Stats
        public int Health => health;
        public int Agility => agility;
        public int Intelligence => intelligence;
        public int Luck => luck;
        public int Resistance => resistance;
        
        // Skill Cards
        public List<string> SkillCardIds => skillCardIds;
        
        private bool isMoving = false;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Setup components (Multiplayer)
            SetupComponents();

            Debug.Log($"[PlayerGameController] Spawned: {playerName} (IsOwner: {IsOwner}, IsMale: {isMale})");
        }

        /// <summary>
        /// Setup components - Called by OnNetworkSpawn (Multiplayer) or Initialize (Demo Mode)
        /// </summary>
        private void SetupComponents()
        {
            // Setup local components
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
                if (boardManager == null)
                {
                    Debug.LogError("[PlayerGameController] BoardManager not found in scene!");
                }
            }

            // Auto-find animator if not assigned
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Setup turn indicator
            if (turnIndicator == null)
            {
                turnIndicator = GetComponentInChildren<TurnIndicator>();
                if (turnIndicator == null)
                {
                    // Create turn indicator if not exists
                    GameObject indicatorObj = new GameObject("TurnIndicator");
                    indicatorObj.transform.SetParent(transform);
                    indicatorObj.transform.localPosition = new Vector3(0, 2.5f, 0);

                    var meshFilter = indicatorObj.AddComponent<MeshFilter>();
                    var meshRenderer = indicatorObj.AddComponent<MeshRenderer>();
                    meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

                    var material = new Material(Shader.Find("Standard"));
                    material.color = Color.yellow;
                    material.SetFloat("_Metallic", 0.5f);
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.yellow * 2f);
                    meshRenderer.material = material;

                    indicatorObj.transform.localScale = Vector3.one * 0.3f;

                    turnIndicator = indicatorObj.AddComponent<TurnIndicator>();
                    indicatorObj.SetActive(false);
                }
            }

            Debug.Log($"[PlayerGameController] Components setup complete (BoardManager: {boardManager != null}, Animator: {animator != null}, TurnIndicator: {turnIndicator != null})");
        }
        
        /// <summary>
        /// Initialize player data - Called by GameManager when spawning
        /// NOTE:
        /// - playerName: Lấy từ Firebase (GameDataManager.currentIngameName)
        /// - money: LUÔN BẮT ĐẦU = 10000 (game cung cấp, không lấy từ Firebase)
        /// - currentTile: LUÔN BẮT ĐẦU = 0 (Start tile)
        /// - stats (hp, agi, intel, lck, res): Lấy từ loadout (equipment + skill cards)
        /// </summary>
        public void Initialize(string name, string id, bool male, int hp, int agi, int intel, int lck, int res)
        {
            // Player info từ Firebase
            playerName = name;
            playerId = id;
            isMale = male;

            // Stats từ loadout (equipment + skill cards)
            health = hp;
            agility = agi;
            intelligence = intel;
            luck = lck;
            resistance = res;

            // Game state - LUÔN BẮT ĐẦU TỪ ĐÂY
            money = 10000;      // ⭐ Starting money - game cung cấp
            currentTile = 0;    // ⭐ Start at tile 0 (Ô Bắt Đầu)
            jailCounter = 0;
            skipNextTurn = false;

            // ⭐ DEMO MODE: Setup components (OnNetworkSpawn không được gọi)
            SetupComponents();

            Debug.Log($"[PlayerGameController] Initialized {name} (Male: {male})");
            Debug.Log($"[PlayerGameController] Stats - HP:{hp} AGI:{agi} INT:{intel} LUCK:{lck} RES:{res}");
            Debug.Log($"[PlayerGameController] Starting - Money:{money} Tile:{currentTile}");
            
            // ✅ SYNC to all clients if this is server
            if (IsServer && IsSpawned)
            {
                SyncPlayerDataClientRpc(name, id, male, hp, agi, intel, lck, res, money, currentTile, playerIndex);
            }
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Sync player data after initialization
        /// </summary>
        [ClientRpc]
        private void SyncPlayerDataClientRpc(string name, string id, bool male, int hp, int agi, int intel, int lck, int res, int startMoney, int startTile, int index)
        {
            // Update all data on clients
            playerName = name;
            playerId = id;
            isMale = male;
            health = hp;
            agility = agi;
            intelligence = intel;
            luck = lck;
            resistance = res;
            money = startMoney;
            currentTile = startTile;
            playerIndex = index;
            
            Debug.Log($"[PlayerGameController] Synced player data: {name} (Money: {money}, Tile: {currentTile})");
        }
        
        /// <summary>
        /// Set player index (0-3) for color system
        /// </summary>
        public void SetPlayerIndex(int index)
        {
            playerIndex = index;
            Debug.Log($"[PlayerGameController] Set player index: {index} for {playerName}");
        }
        
        /// <summary>
        /// Get player color based on index (Red, Blue, Green, Yellow)
        /// </summary>
        public Color GetPlayerColor()
        {
            Color[] playerColors = new Color[]
            {
                new Color(1f, 0.2f, 0.2f, 1f),    // Player 0: Red
                new Color(0.2f, 0.5f, 1f, 1f),    // Player 1: Blue
                new Color(0.2f, 1f, 0.2f, 1f),    // Player 2: Green
                new Color(1f, 1f, 0.2f, 1f)       // Player 3: Yellow
            };
            
            int index = Mathf.Clamp(playerIndex, 0, 3);
            return playerColors[index];
        }
    
        /// <summary>
        /// Set skill cards from loadout
        /// </summary>
        public void SetSkillCards(List<string> cardIds)
        {
            skillCardIds = new List<string>(cardIds);
            
            // Initialize cooldowns
            skillCooldowns.Clear();
            foreach (var effectId in cardIds)
            {
                if (!string.IsNullOrEmpty(effectId))
                {
                    skillCooldowns[effectId] = 0; // Start with 0 cooldown
                }
            }
            
            Debug.Log($"[PlayerGameController] Set {cardIds.Count} skill cards for {playerName}");
        }
    
    
    /// <summary>
    /// Check if player has a skill card with specific effectId
    /// </summary>
    public bool HasSkillCard(string effectId)
    {
        return skillCardIds.Contains(effectId);
    }
    
    /// <summary>
    /// Check if skill card is available (not on cooldown)
    /// </summary>
    public bool IsSkillAvailable(string effectId)
    {
        if (!HasSkillCard(effectId)) return false;
        
        if (skillCooldowns.TryGetValue(effectId, out int cooldown))
        {
            return cooldown <= 0;
        }
        
        return true;
    }
    
    /// <summary>
    /// Use skill card (set cooldown)
    /// </summary>
    public void UseSkillCard(string effectId, int cooldownTurns)
    {
        if (HasSkillCard(effectId))
        {
            skillCooldowns[effectId] = cooldownTurns;
            Debug.Log($"[PlayerGameController] {playerName} used skill {effectId}, cooldown: {cooldownTurns} turns");
        }
    }
    
    /// <summary>
    /// Reduce all cooldowns by 1 (call at end of turn)
    /// </summary>
    public void ReduceCooldowns()
    {
        var keys = new List<string>(skillCooldowns.Keys);
        foreach (var key in keys)
        {
            if (skillCooldowns[key] > 0)
            {
                skillCooldowns[key]--;
                Debug.Log($"[PlayerGameController] {playerName} skill {key} cooldown: {skillCooldowns[key]}");
            }
        }
    }
        
        /// <summary>
        /// Move player by steps
        /// </summary>
        public IEnumerator MoveBySteps(int steps)
        {
            if (isMoving)
            {
                Debug.LogWarning($"[PlayerGameController] {playerName} is already moving!");
                yield break;
            }

            isMoving = true;
            SetAnimation(true);

            int startTile = currentTile;
            int targetTile = (currentTile + steps) % boardManager.TotalTiles;

            Debug.Log($"[PlayerGameController] {playerName} moving from tile {startTile} to {targetTile} ({steps} steps)");

            // Move step by step
            for (int i = 0; i < steps; i++)
            {
                int previousTile = currentTile;
                currentTile = (currentTile + 1) % boardManager.TotalTiles;
                Vector3 targetPos = boardManager.GetWaypointPosition(currentTile);

                // Look at center before moving
                LookAtCenter(targetPos);

                // Move to waypoint with bounce effect
                yield return StartCoroutine(MoveToWaypointWithBounce(targetPos));

                // ✅ FIX: Check if passed Start (tile 0)
                // Passed Start if: previous tile != 0 AND current tile == 0
                if (previousTile != 0 && currentTile == 0)
                {
                    OnPassStart();
                    Debug.Log($"[PlayerGameController] {playerName} passed Start! (from tile {previousTile} to 0)");
                }
            }

            SetAnimation(false);
            isMoving = false;

            Debug.Log($"[PlayerGameController] {playerName} finished moving to tile {currentTile}");
            
            // ✅ SYNC POSITION TO ALL CLIENTS
            if (IsServer)
            {
                SyncPositionClientRpc(currentTile);
            }
            
            // Fire event for local UI update
            OnPositionChanged?.Invoke(currentTile);

        }        /// <summary>
        /// Move to waypoint with bounce effect
        /// </summary>
        private IEnumerator MoveToWaypointWithBounce(Vector3 targetPos)
        {
            Vector3 startPos = transform.position;
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Linear movement
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

                // Add bounce (parabola)
                float bounceOffset = -4f * bounceHeight * Mathf.Pow(t - 0.5f, 2f) + bounceHeight;
                currentPos.y += bounceOffset;

                transform.position = currentPos;

                yield return null;
            }

            // Ensure final position
            transform.position = targetPos;
        }

        /// <summary>
        /// Look at center of board
        /// </summary>
        private void LookAtCenter(Vector3 currentWaypointPos)
        {
            Vector3 directionToCenter = boardCenter - currentWaypointPos;
            directionToCenter.y = 0;

            if (directionToCenter.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToCenter);
                transform.rotation = targetRotation;
            }
        }
        
        /// <summary>
        /// Set animation state (currently only Idle animation exists)
        /// </summary>
        private void SetAnimation(bool isRunning)
        {
            // Animator chỉ có Idle animation, không có isRunning parameter
            // TODO: Thêm Run animation và parameter khi có asset
            
            // if (animator != null)
            // {
            //     animator.SetBool("isRunning", isRunning);
            // }
        }
        
        /// <summary>
        /// Called when player passes Start tile
        /// </summary>
        private void OnPassStart()
        {
            int baseMoney = 150;
            int healthBonus = Mathf.RoundToInt(baseMoney * health / 100f);
            int totalMoney = baseMoney + healthBonus;

            AddMoney(totalMoney);

            Debug.Log($"[PlayerGameController] {playerName} passed Start! +{totalMoney} money (base: {baseMoney}, health bonus: {healthBonus})");

            // ✅ Update UI
            if (GameManager.Instance != null && GameManager.Instance.panelGame != null)
            {
                GameManager.Instance.panelGame.UpdateAllPanels();
            }
        }
        
        /// <summary>
        /// Add money
        /// </summary>
        public void AddMoney(int amount)
        {
            money += amount;
            Debug.Log($"[PlayerGameController] {playerName} money: {money} (+{amount})");
            
            // Fire event locally
            OnMoneyChanged?.Invoke(money);
            
            // Sync to all clients
            if (IsServer)
            {
                SyncMoneyClientRpc(money);
            }
        }
        
        /// <summary>
        /// Subtract money
        /// </summary>
        public void SubtractMoney(int amount)
        {
            money -= amount;
            Debug.Log($"[PlayerGameController] {playerName} money: {money} (-{amount})");
            
            // Fire event locally
            OnMoneyChanged?.Invoke(money);
            
            // Sync to all clients
            if (IsServer)
            {
                SyncMoneyClientRpc(money);
            }
        }
        
        /// <summary>
        /// Set jail counter
        /// </summary>
        public void SetJailCounter(int turns)
        {
            jailCounter = turns;
            Debug.Log($"[PlayerGameController] {playerName} in jail for {jailCounter} turns");
            
            // Fire event locally
            OnJailCounterChanged?.Invoke(jailCounter);
            
            // Sync to all clients
            if (IsServer)
            {
                SyncJailCounterClientRpc(jailCounter);
            }
        }
        
        // ===== NETWORK SYNC METHODS =====
        
        /// <summary>
        /// HOST → ALL CLIENTS: Sync money changes
        /// </summary>
        [ClientRpc]
        private void SyncMoneyClientRpc(int newMoney)
        {
            if (money != newMoney)
            {
                money = newMoney;
                OnMoneyChanged?.Invoke(money);
                Debug.Log($"[PlayerGameController] {playerName} money synced: {money}");
            }
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Sync jail counter changes
        /// </summary>
        [ClientRpc]
        private void SyncJailCounterClientRpc(int newJailCounter)
        {
            if (jailCounter != newJailCounter)
            {
                jailCounter = newJailCounter;
                OnJailCounterChanged?.Invoke(jailCounter);
                Debug.Log($"[PlayerGameController] {playerName} jail counter synced: {jailCounter}");
            }
        }
        
        /// <summary>
        /// HOST → ALL CLIENTS: Sync position changes (called after movement)
        /// </summary>
        [ClientRpc]
        private void SyncPositionClientRpc(int newTile)
        {
            if (currentTile != newTile)
            {
                currentTile = newTile;
                OnPositionChanged?.Invoke(currentTile);
                Debug.Log($"[PlayerGameController] {playerName} position synced: Tile {currentTile}");
            }
        }
        /// <summary>
        /// Decrease jail counter
        /// </summary>
        public void DecreaseJailCounter()
        {
            if (jailCounter > 0)
            {
                jailCounter--;
                Debug.Log($"[PlayerGameController] {playerName} jail counter: {jailCounter}");
            }
        }
        
        /// <summary>
        /// Set skip next turn
        /// </summary>
        public void SetSkipNextTurn(bool skip)
        {
            skipNextTurn = skip;
            Debug.Log($"[PlayerGameController] {playerName} skip next turn: {skipNextTurn}");
        }
        
        /// <summary>
        /// Check if player is bankrupt
        /// </summary>
        public bool IsBankrupt()
        {
            return money < 0;
        }

        /// <summary>
        /// Show turn indicator
        /// NOTE:
        /// - Multiplayer: ch? Host ho?c Owner m?i g?i request
        /// - Demo Mode: lu�n hi?n th?
        /// </summary>
        public void ShowTurnIndicator()
        {
            var networkObject = GetComponent<NetworkObject>();
            bool isDemoMode = (networkObject == null || !networkObject.IsSpawned);

            if (!isDemoMode && !IsServer && !IsOwner)
            {
                return;
            }

            if (turnIndicator != null)
            {
                turnIndicator.Show();
                Debug.Log($"[PlayerGameController] Turn indicator shown for {playerName} (Demo: {isDemoMode}, Owner: {IsOwner}, Server: {IsServer})");
            }
        }

        /// <summary>
        /// Hide turn indicator
        /// </summary>
        public void HideTurnIndicator()
        {
            var networkObject = GetComponent<NetworkObject>();
            bool isDemoMode = (networkObject == null || !networkObject.IsSpawned);

            if (!isDemoMode && !IsServer && !IsOwner)
            {
                return;
            }

            if (turnIndicator != null)
            {
                turnIndicator.Hide();
            }
        }
    }
}


