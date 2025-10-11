using UnityEngine;
using AntKnow.Game;

namespace AntKnow.Game
{
    /// <summary>
    /// Base class cho tất cả player panels để giảm duplicate code
    /// </summary>
    public abstract class BasePlayerPanel : MonoBehaviour
    {
        [Header("Player Reference")]
        protected PlayerGameController player;
        
        [Header("UI Components")]
        [SerializeField] protected GameObject panelObject;
        
        /// <summary>
        /// Initialize panel với player controller
        /// </summary>
        public virtual void Initialize(PlayerGameController playerController)
        {
            this.player = playerController;
            
            if (player == null)
            {
                Debug.LogError($"[{GetType().Name}] Player controller is null!");
                return;
            }
            
            SetupUI();
            SubscribeToEvents();
            UpdateDisplay();
            
            Debug.Log($"[{GetType().Name}] Initialized for player: {player.PlayerName}");
        }
        
        /// <summary>
        /// Setup UI components - Override in derived classes
        /// </summary>
        protected abstract void SetupUI();
        
        /// <summary>
        /// Subscribe to player events - Override in derived classes
        /// </summary>
        protected abstract void SubscribeToEvents();
        
        /// <summary>
        /// Update display with current player data - Override in derived classes
        /// </summary>
        protected abstract void UpdateDisplay();
        
        /// <summary>
        /// Show panel
        /// </summary>
        public virtual void Show()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public virtual void Hide()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Check if panel is showing
        /// </summary>
        public virtual bool IsShowing()
        {
            if (panelObject != null)
            {
                return panelObject.activeInHierarchy;
            }
            else
            {
                return gameObject.activeInHierarchy;
            }
        }
        
        /// <summary>
        /// Get current player
        /// </summary>
        public PlayerGameController GetPlayer()
        {
            return player;
        }
        
        /// <summary>
        /// Public method to update display - calls protected override
        /// </summary>
        public void UpdateDisplayPublic()
        {
            UpdateDisplay();
        }
        
        /// <summary>
        /// Public method to refresh display - calls protected override
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }
        
        /// <summary>
        /// Cleanup when destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// Unsubscribe from events - Override in derived classes
        /// </summary>
        protected virtual void UnsubscribeFromEvents()
        {
            // Override in derived classes if needed
        }
    }
}
