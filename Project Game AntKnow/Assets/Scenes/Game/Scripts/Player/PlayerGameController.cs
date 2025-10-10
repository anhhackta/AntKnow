using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Controller cho player trong game
    /// Quản lý movement, stats, animation
    /// </summary>
    public class PlayerGameController : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private string playerId;
        [SerializeField] private bool isMale = true;
        
        [Header("Game State")]
        [SerializeField] private int currentTile = 0;
        [SerializeField] private int money = 5000; // Tăng lên 5000
        [SerializeField] private int jailCounter = 0;
        [SerializeField] private bool skipNextTurn = false;
        
    [Header("Stats from Loadout")]
    [SerializeField] private int health = 0;
    [SerializeField] private int agility = 0;
    [SerializeField] private int intelligence = 0;
    [SerializeField] private int luck = 0;
    [SerializeField] private int resistance = 0;
    
    [Header("Skill Cards from Loadout")]
    [SerializeField] private List<string> skillCardIds = new List<string>(); // effectIds
    private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>(); // effectId -> remaining cooldown
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float bounceHeight = 0.5f; // Độ cao nhảy lên
        [SerializeField] private float bounceDuration = 0.3f; // Thời gian nhảy
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Vector3 boardCenter = Vector3.zero; // Tâm bàn cờ

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [Header("Turn Indicator")]
        [SerializeField] private TurnIndicator turnIndicator;

        // Properties
        public string PlayerName => playerName;
        public string PlayerId => playerId;
        public bool IsMale => isMale;
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
        
        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (turnIndicator == null)
            {
                turnIndicator = GetComponentInChildren<TurnIndicator>();
                if (turnIndicator == null)
                {
                    // Create turn indicator if not exists
                    GameObject indicatorObj = new GameObject("TurnIndicator");
                    indicatorObj.transform.SetParent(transform);
                    indicatorObj.transform.localPosition = Vector3.zero;
                    turnIndicator = indicatorObj.AddComponent<TurnIndicator>();
                }
            }
        }
        
    /// <summary>
    /// Initialize player data
    /// </summary>
    public void Initialize(string name, string id, bool male, int hp, int agi, int intel, int lck, int res)
    {
        playerName = name;
        playerId = id;
        isMale = male;
        
        health = hp;
        agility = agi;
        intelligence = intel;
        luck = lck;
        resistance = res;
        
        money = 1000;
        currentTile = 0;
        
        Debug.Log($"[PlayerGameController] Initialized {playerName} at tile {currentTile} with {money} money");
    }
    
    /// <summary>
    /// Set skill cards from loadout
    /// </summary>
    public void SetSkillCards(List<string> cardIds)
    {
        skillCardIds = new List<string>(cardIds);
        
        // Initialize cooldowns
        skillCooldowns.Clear();
        foreach (var effectId in skillCardIds)
        {
            skillCooldowns[effectId] = 0; // Start with 0 cooldown
        }
        
        Debug.Log($"[PlayerGameController] {playerName} loaded {skillCardIds.Count} skill cards: {string.Join(", ", skillCardIds)}");
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
        /// Move player by steps với bounce effect và look at center
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
                currentTile = (currentTile + 1) % boardManager.TotalTiles;
                Vector3 targetPos = boardManager.GetWaypointPosition(currentTile);

                // Look at center trước khi di chuyển
                LookAtCenter(targetPos);

                // Move to waypoint với bounce effect
                yield return StartCoroutine(MoveToWaypointWithBounce(targetPos));

                // Check if passed Start (tile 0)
                if (currentTile == 0 && i > 0)
                {
                    OnPassStart();
                }
            }

            SetAnimation(false);
            isMoving = false;

            Debug.Log($"[PlayerGameController] {playerName} reached tile {currentTile}");
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
        /// Set animation state
        /// </summary>
        private void SetAnimation(bool isRunning)
        {
            if (animator != null)
            {
                animator.SetBool("isRunning", isRunning);
            }
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
        }
        
        /// <summary>
        /// Add money
        /// </summary>
        public void AddMoney(int amount)
        {
            money += amount;
            Debug.Log($"[PlayerGameController] {playerName} money: {money} (+{amount})");
        }
        
        /// <summary>
        /// Subtract money
        /// </summary>
        public void SubtractMoney(int amount)
        {
            money -= amount;
            Debug.Log($"[PlayerGameController] {playerName} money: {money} (-{amount})");
        }
        
        /// <summary>
        /// Set jail counter
        /// </summary>
        public void SetJailCounter(int turns)
        {
            jailCounter = turns;
            Debug.Log($"[PlayerGameController] {playerName} in jail for {jailCounter} turns");
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

