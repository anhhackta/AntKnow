using UnityEngine;
using System.Collections;

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
        [SerializeField] private int money = 1000;
        [SerializeField] private int jailCounter = 0;
        [SerializeField] private bool skipNextTurn = false;
        
        [Header("Stats from Loadout")]
        [SerializeField] private int health = 0;
        [SerializeField] private int agility = 0;
        [SerializeField] private int intelligence = 0;
        [SerializeField] private int luck = 0;
        [SerializeField] private int resistance = 0;
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private BoardManager boardManager;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        
        // Properties
        public string PlayerName => playerName;
        public string PlayerId => playerId;
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
                currentTile = (currentTile + 1) % boardManager.TotalTiles;
                Vector3 targetPos = boardManager.GetWaypointPosition(currentTile);
                
                // Move to waypoint
                while (Vector3.Distance(transform.position, targetPos) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                
                transform.position = targetPos;
                
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
    }
}

