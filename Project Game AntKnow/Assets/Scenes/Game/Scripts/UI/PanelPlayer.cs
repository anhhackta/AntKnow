using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin người chơi khác
    /// </summary>
    public class PanelPlayer : BasePlayerPanel
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textPlayerName;
        [SerializeField] private TextMeshProUGUI textMoney;
        [SerializeField] private Image imageAvatar;
        [SerializeField] private Image imageTurnIndicator;
        
        [Header("Avatar Colors")]
        [SerializeField] private Color maleColor = Color.blue;
        [SerializeField] private Color femaleColor = Color.magenta;
        
        /// <summary>
        /// Setup UI components
        /// </summary>
        protected override void SetupUI()
        {
            // UI components are already assigned in inspector
        }
        
        /// <summary>
        /// Subscribe to player events
        /// </summary>
        protected override void SubscribeToEvents()
        {
            // PanelPlayer doesn't need to subscribe to events
            // It gets updated externally by PanelGame
        }
        
        /// <summary>
        /// Update display with current player data
        /// </summary>
        protected override void UpdateDisplay()
        {
            if (player == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            // Update name
            if (textPlayerName != null)
            {
                textPlayerName.text = player.PlayerName;
            }
            
            // Update money
            if (textMoney != null)
            {
                textMoney.text = $"{player.Money}";
            }
            
            // Update avatar color
            if (imageAvatar != null)
            {
                imageAvatar.color = player.IsMale ? maleColor : femaleColor;
            }
        }
        
        /// <summary>
        /// Set turn indicator
        /// </summary>
        public void SetTurnActive(bool active)
        {
            if (imageTurnIndicator != null)
            {
                imageTurnIndicator.enabled = active;
            }
        }
        
        /// <summary>
        /// Update money display
        /// </summary>
        public void UpdateMoney(int money)
        {
            if (textMoney != null)
            {
                textMoney.text = $"{money}";
            }
        }
    }
}

