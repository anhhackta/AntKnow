using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Network-aware controller cho player trong multiplayer game
    /// Quản lý movement, stats, animation với NetworkObject sync
    /// </summary>
    public class PlayerGameController : NetworkBehaviour
    {
        [Header("Network Player Info")]
        public NetworkVariable<FixedString64Bytes> networkPlayerName = new NetworkVariable<FixedString64Bytes>("Player");
        public NetworkVariable<FixedString64Bytes> networkPlayerId = new NetworkVariable<FixedString64Bytes>("");
        public NetworkVariable<bool> networkIsMale = new NetworkVariable<bool>(true);
        
        [Header("Network Game State")]
        public NetworkVariable<int> networkCurrentTile = new NetworkVariable<int>(0);
        public NetworkVariable<int> networkMoney = new NetworkVariable<int>(1000);
        public NetworkVariable<int> networkJailCounter = new NetworkVariable<int>(0);
        public NetworkVariable<bool> networkSkipNextTurn = new NetworkVariable<bool>(false);
        
        [Header("Network Stats from Loadout")]
        public NetworkVariable<int> networkHealth = new NetworkVariable<int>(0);
        public NetworkVariable<int> networkAgility = new NetworkVariable<int>(0);
        public NetworkVariable<int> networkIntelligence = new NetworkVariable<int>(0);
        public NetworkVariable<int> networkLuck = new NetworkVariable<int>(0);
        public NetworkVariable<int> networkResistance = new NetworkVariable<int>(0);
        
        [Header("Skill Cards from Loadout")]
        public NetworkVariable<FixedString512Bytes> networkSkillCardIds = new NetworkVariable<FixedString512Bytes>(""); // Comma-separated effectIds
        private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>(); // effectId -> remaining cooldown (local only)
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float bounceHeight = 0.5f; // Độ cao nhảy lên
        [SerializeField] private float bounceDuration = 0.3f; // Thời gian nhảy
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Vector3 boardCenter = Vector3.zero; // Tâm bàn cờ

        [Header("Player Models")]
        [SerializeField] private GameObject maleModel; // Main character model
        [SerializeField] private GameObject femaleModel; // Girl character model
        
        [Header("Animation")]
        [SerializeField] private Animator maleAnimator;
        [SerializeField] private Animator femaleAnimator;
        
        [Header("Turn Indicator")]
        [SerializeField] private TurnIndicator turnIndicator;

        // Properties (Network-aware)
        public string PlayerName => networkPlayerName.Value.ToString();
        public string PlayerId => networkPlayerId.Value.ToString();
        public bool IsMale => networkIsMale.Value;
        public int CurrentTile => networkCurrentTile.Value;
        public int Money => networkMoney.Value;
        public int JailCounter => networkJailCounter.Value;
        public bool SkipNextTurn => networkSkipNextTurn.Value;
        
        // Stats (Network-aware)
        public int Health => networkHealth.Value;
        public int Agility => networkAgility.Value;
        public int Intelligence => networkIntelligence.Value;
        public int Luck => networkLuck.Value;
        public int Resistance => networkResistance.Value;
        
        // Skill Cards (Network-aware)
        public List<string> SkillCardIds 
        { 
            get 
            { 
                var ids = new List<string>();
                if (!string.IsNullOrEmpty(networkSkillCardIds.Value.ToString()))
                {
                    var cardIdsStr = networkSkillCardIds.Value.ToString();
                    ids.AddRange(cardIdsStr.Split(','));
                }
                return ids;
            } 
        }
        
        private bool isMoving = false;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Setup local components
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            // Auto-assign animators if not set
            if (maleAnimator == null && maleModel != null)
            {
                maleAnimator = maleModel.GetComponent<Animator>();
            }
            
            if (femaleAnimator == null && femaleModel != null)
            {
                femaleAnimator = femaleModel.GetComponent<Animator>();
            }

            if (turnIndicator == null)
            {
                turnIndicator = GetComponentInChildren<TurnIndicator>();
                if (turnIndicator == null)
                {
                    // Create turn indicator if not exists
                    GameObject indicatorObj = new GameObject("TurnIndicator");
                    indicatorObj.transform.SetParent(transform);
                    indicatorObj.transform.localPosition = new Vector3(0, 2.5f, 0); // Above player head
                    
                    // Add sphere mesh
                    var meshFilter = indicatorObj.AddComponent<MeshFilter>();
                    var meshRenderer = indicatorObj.AddComponent<MeshRenderer>();
                    meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                    
                    // Add bright material
                    var material = new Material(Shader.Find("Standard"));
                    material.color = Color.yellow;
                    meshRenderer.material = material;
                    
                    // Scale down
                    indicatorObj.transform.localScale = Vector3.one * 0.3f;
                    
                    turnIndicator = indicatorObj.AddComponent<TurnIndicator>();
                    indicatorObj.SetActive(false); // Hidden by default
                }
            }

            // Subscribe to network variable changes
            networkPlayerName.OnValueChanged += OnPlayerNameChanged;
            networkIsMale.OnValueChanged += OnIsMaleChanged;
            
            Debug.Log($"[PlayerGameController] Network spawned: {PlayerName} (IsOwner: {IsOwner})");
        }

        public override void OnNetworkDespawn()
        {
            // Unsubscribe from network variable changes
            networkPlayerName.OnValueChanged -= OnPlayerNameChanged;
            networkIsMale.OnValueChanged -= OnIsMaleChanged;
            
            base.OnNetworkDespawn();
        }

        private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
        {
            Debug.Log($"[PlayerGameController] Player name changed: {oldValue} -> {newValue}");
        }

        private void OnIsMaleChanged(bool oldValue, bool newValue)
        {
            Debug.Log($"[PlayerGameController] Player gender changed: {oldValue} -> {newValue}");
            SetupPlayerModel();
        }
        
        /// <summary>
        /// Initialize player data (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void InitializePlayerServerRpc(string name, string id, bool male, int hp, int agi, int intel, int lck, int res)
        {
            networkPlayerName.Value = name;
            networkPlayerId.Value = id;
            networkIsMale.Value = male;
            
            networkHealth.Value = hp;
            networkAgility.Value = agi;
            networkIntelligence.Value = intel;
            networkLuck.Value = lck;
            networkResistance.Value = res;
            
            networkMoney.Value = 1000;
            networkCurrentTile.Value = 0;
            
            Debug.Log($"[PlayerGameController] Server initialized {name} at tile {networkCurrentTile.Value} with {networkMoney.Value} money");
        }

        /// <summary>
        /// Initialize player data (Local method for compatibility)
        /// </summary>
        public void Initialize(string name, string id, bool male, int hp, int agi, int intel, int lck, int res)
        {
            if (IsServer)
            {
                // Server can directly set values
                networkPlayerName.Value = name;
                networkPlayerId.Value = id;
                networkIsMale.Value = male;
                
                networkHealth.Value = hp;
                networkAgility.Value = agi;
                networkIntelligence.Value = intel;
                networkLuck.Value = lck;
                networkResistance.Value = res;
                
                networkMoney.Value = 1000;
                networkCurrentTile.Value = 0;
                
                Debug.Log($"[PlayerGameController] Server initialized {name} at tile {networkCurrentTile.Value} with {networkMoney.Value} money");
            }
            else
            {
                // Client calls ServerRpc
                InitializePlayerServerRpc(name, id, male, hp, agi, intel, lck, res);
            }
        }
    
        /// <summary>
        /// Setup player model based on gender (Network-aware)
        /// With separate prefabs, this is much simpler
        /// </summary>
        private void SetupPlayerModel()
        {
            // With separate prefabs, the correct model is already active
            // Just validate that the prefab matches the gender
            if (IsMale)
            {
                if (maleModel != null && maleModel.activeInHierarchy)
                {
                    Debug.Log($"[PlayerGameController] Male prefab active for {PlayerName}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerGameController] Male prefab issue for {PlayerName}!");
                }
            }
            else
            {
                if (femaleModel != null && femaleModel.activeInHierarchy)
                {
                    Debug.Log($"[PlayerGameController] Female prefab active for {PlayerName}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerGameController] Female prefab issue for {PlayerName}!");
                }
            }
        }
    
        /// <summary>
        /// Set skill cards from loadout (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetSkillCardsServerRpc(string cardIdsStr)
        {
            networkSkillCardIds.Value = cardIdsStr;
            
            // Initialize cooldowns on server
            skillCooldowns.Clear();
            if (!string.IsNullOrEmpty(cardIdsStr))
            {
                var cardIds = cardIdsStr.Split(',');
                foreach (var effectId in cardIds)
                {
                    if (!string.IsNullOrEmpty(effectId))
                    {
                        skillCooldowns[effectId] = 0; // Start with 0 cooldown
                    }
                }
            }
            
            Debug.Log($"[PlayerGameController] Server set skill cards for {PlayerName}: {cardIdsStr}");
        }

        /// <summary>
        /// Set skill cards from loadout (Local method for compatibility)
        /// </summary>
        public void SetSkillCards(List<string> cardIds)
        {
            var cardIdsStr = string.Join(",", cardIds);
            
            if (IsServer)
            {
                // Server can directly set values
                networkSkillCardIds.Value = cardIdsStr;
                
                // Initialize cooldowns
                skillCooldowns.Clear();
                foreach (var effectId in cardIds)
                {
                    skillCooldowns[effectId] = 0; // Start with 0 cooldown
                }
                
                Debug.Log($"[PlayerGameController] Server set skill cards for {PlayerName}: {cardIdsStr}");
            }
            else
            {
                // Client calls ServerRpc
                SetSkillCardsServerRpc(cardIdsStr);
            }
        }
    
        /// <summary>
        /// Check if player has a skill card with specific effectId
        /// </summary>
        public bool HasSkillCard(string effectId)
        {
            return SkillCardIds.Contains(effectId);
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
            Debug.Log($"[PlayerGameController] {PlayerName} used skill {effectId}, cooldown: {cooldownTurns} turns");
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
                Debug.Log($"[PlayerGameController] {PlayerName} skill {key} cooldown: {skillCooldowns[key]}");
            }
        }
    }
        
        /// <summary>
        /// Move player by steps (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void MoveByStepsServerRpc(int steps)
        {
            if (isMoving)
            {
                Debug.LogWarning($"[PlayerGameController] {PlayerName} is already moving!");
                return;
            }

            StartCoroutine(MoveByStepsCoroutine(steps));
        }

        /// <summary>
        /// Move player by steps với bounce effect và look at center (Server Coroutine)
        /// </summary>
        private IEnumerator MoveByStepsCoroutine(int steps)
        {
            if (isMoving)
            {
                Debug.LogWarning($"[PlayerGameController] {PlayerName} is already moving!");
                yield break;
            }

            isMoving = true;
            SetAnimationClientRpc(true);

            int startTile = CurrentTile;
            int targetTile = (CurrentTile + steps) % boardManager.TotalTiles;

            Debug.Log($"[PlayerGameController] {PlayerName} moving from tile {startTile} to {targetTile} ({steps} steps)");

            // Move step by step
            for (int i = 0; i < steps; i++)
            {
                networkCurrentTile.Value = (networkCurrentTile.Value + 1) % boardManager.TotalTiles;
                Vector3 targetPos = boardManager.GetWaypointPosition(networkCurrentTile.Value);

                // Notify all clients about position update
                UpdatePositionClientRpc(targetPos, networkCurrentTile.Value);

                // Look at center trước khi di chuyển
                LookAtCenterClientRpc(targetPos);

                // Move to waypoint với bounce effect
                yield return StartCoroutine(MoveToWaypointWithBounce(targetPos));

                // Check if passed Start (tile 0)
                if (networkCurrentTile.Value == 0 && i > 0)
                {
                    OnPassStart();
                }
            }

            SetAnimationClientRpc(false);
            isMoving = false;

            Debug.Log($"[PlayerGameController] {PlayerName} reached tile {networkCurrentTile.Value}");
        }

        /// <summary>
        /// Move player by steps (Local method for compatibility)
        /// </summary>
        public IEnumerator MoveBySteps(int steps)
        {
            if (IsServer)
            {
                yield return StartCoroutine(MoveByStepsCoroutine(steps));
            }
            else
            {
                // Client calls ServerRpc
                MoveByStepsServerRpc(steps);
                yield break; // Client doesn't wait for server coroutine
            }
        }

        /// <summary>
        /// Move to waypoint với bounce effect
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
                // y = -4h * (t - 0.5)^2 + h
                // Đỉnh ở giữa (t = 0.5), độ cao = bounceHeight
                float bounceOffset = -4f * bounceHeight * Mathf.Pow(t - 0.5f, 2f) + bounceHeight;
                currentPos.y += bounceOffset;

                transform.position = currentPos;

                yield return null;
            }

            // Ensure final position
            transform.position = targetPos;
        }

        /// <summary>
        /// Update position on all clients
        /// </summary>
        [ClientRpc]
        private void UpdatePositionClientRpc(Vector3 position, int tileIndex)
        {
            if (!IsOwner) // Only update non-owner clients
            {
                transform.position = position;
            }
        }

        /// <summary>
        /// Look at center on all clients
        /// </summary>
        [ClientRpc]
        private void LookAtCenterClientRpc(Vector3 currentWaypointPos)
        {
            LookAtCenter(currentWaypointPos);
        }

        /// <summary>
        /// Set animation on all clients
        /// </summary>
        [ClientRpc]
        private void SetAnimationClientRpc(bool isRunning)
        {
            SetAnimation(isRunning);
        }

        /// <summary>
        /// Quay mặt về phía tâm bàn cờ
        /// </summary>
        private void LookAtCenter(Vector3 currentWaypointPos)
        {
            // Calculate direction từ waypoint về center
            Vector3 directionToCenter = boardCenter - currentWaypointPos;
            directionToCenter.y = 0; // Chỉ xoay theo trục Y

            if (directionToCenter.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToCenter);
                transform.rotation = targetRotation;
            }
        }
        
        /// <summary>
        /// Set animation state on the active model
        /// </summary>
        private void SetAnimation(bool isRunning)
        {
            Animator activeAnimator = GetActiveAnimator();
            if (activeAnimator != null)
            {
                activeAnimator.SetBool("isRunning", isRunning);
            }
        }
        
        /// <summary>
        /// Get the currently active animator (male or female)
        /// </summary>
        private Animator GetActiveAnimator()
        {
            if (IsMale && maleAnimator != null)
            {
                return maleAnimator;
            }
            else if (!IsMale && femaleAnimator != null)
            {
                return femaleAnimator;
            }
            
            return null;
        }
        
        /// <summary>
        /// Called when player passes Start tile (Server-side)
        /// </summary>
        private void OnPassStart()
        {
            if (!IsServer) return; // Only server handles game logic
            
            int baseMoney = 150;
            int healthBonus = Mathf.RoundToInt(baseMoney * Health / 100f);
            int totalMoney = baseMoney + healthBonus;
            
            AddMoneyServerRpc(totalMoney);
            
            Debug.Log($"[PlayerGameController] {PlayerName} passed Start! +{totalMoney} money (base: {baseMoney}, health bonus: {healthBonus})");
        }
        
        /// <summary>
        /// Add money (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void AddMoneyServerRpc(int amount)
        {
            networkMoney.Value += amount;
            Debug.Log($"[PlayerGameController] {PlayerName} money: {networkMoney.Value} (+{amount})");
        }
        
        /// <summary>
        /// Add money (Local method for compatibility)
        /// </summary>
        public void AddMoney(int amount)
        {
            if (IsServer)
            {
                networkMoney.Value += amount;
                Debug.Log($"[PlayerGameController] {PlayerName} money: {networkMoney.Value} (+{amount})");
            }
            else
            {
                AddMoneyServerRpc(amount);
            }
        }
        
        /// <summary>
        /// Subtract money (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SubtractMoneyServerRpc(int amount)
        {
            networkMoney.Value -= amount;
            Debug.Log($"[PlayerGameController] {PlayerName} money: {networkMoney.Value} (-{amount})");
        }
        
        /// <summary>
        /// Subtract money (Local method for compatibility)
        /// </summary>
        public void SubtractMoney(int amount)
        {
            if (IsServer)
            {
                networkMoney.Value -= amount;
                Debug.Log($"[PlayerGameController] {PlayerName} money: {networkMoney.Value} (-{amount})");
            }
            else
            {
                SubtractMoneyServerRpc(amount);
            }
        }
        
        /// <summary>
        /// Set jail counter (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetJailCounterServerRpc(int turns)
        {
            networkJailCounter.Value = turns;
            Debug.Log($"[PlayerGameController] {PlayerName} in jail for {networkJailCounter.Value} turns");
        }
        
        /// <summary>
        /// Set jail counter (Local method for compatibility)
        /// </summary>
        public void SetJailCounter(int turns)
        {
            if (IsServer)
            {
                networkJailCounter.Value = turns;
                Debug.Log($"[PlayerGameController] {PlayerName} in jail for {networkJailCounter.Value} turns");
            }
            else
            {
                SetJailCounterServerRpc(turns);
            }
        }
        
        /// <summary>
        /// Decrease jail counter (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void DecreaseJailCounterServerRpc()
        {
            if (networkJailCounter.Value > 0)
            {
                networkJailCounter.Value--;
                Debug.Log($"[PlayerGameController] {PlayerName} jail counter: {networkJailCounter.Value}");
            }
        }
        
        /// <summary>
        /// Decrease jail counter (Local method for compatibility)
        /// </summary>
        public void DecreaseJailCounter()
        {
            if (IsServer)
            {
                if (networkJailCounter.Value > 0)
                {
                    networkJailCounter.Value--;
                    Debug.Log($"[PlayerGameController] {PlayerName} jail counter: {networkJailCounter.Value}");
                }
            }
            else
            {
                DecreaseJailCounterServerRpc();
            }
        }
        
        /// <summary>
        /// Set skip next turn (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetSkipNextTurnServerRpc(bool skip)
        {
            networkSkipNextTurn.Value = skip;
            Debug.Log($"[PlayerGameController] {PlayerName} skip next turn: {networkSkipNextTurn.Value}");
        }
        
        /// <summary>
        /// Set skip next turn (Local method for compatibility)
        /// </summary>
        public void SetSkipNextTurn(bool skip)
        {
            if (IsServer)
            {
                networkSkipNextTurn.Value = skip;
                Debug.Log($"[PlayerGameController] {PlayerName} skip next turn: {networkSkipNextTurn.Value}");
            }
            else
            {
                SetSkipNextTurnServerRpc(skip);
            }
        }
        
        /// <summary>
        /// Check if player is bankrupt
        /// </summary>
        public bool IsBankrupt()
        {
            return networkMoney.Value < 0;
        }

        /// <summary>
        /// Show turn indicator
        /// </summary>
        public void ShowTurnIndicator()
        {
            if (turnIndicator != null)
            {
                turnIndicator.Show();
            }
        }

        /// <summary>
        /// Hide turn indicator
        /// </summary>
        public void HideTurnIndicator()
        {
            if (turnIndicator != null)
            {
                turnIndicator.Hide();
            }
        }
    }
}

