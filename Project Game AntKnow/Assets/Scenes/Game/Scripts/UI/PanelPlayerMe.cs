using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin người chơi bản thân
    /// </summary>
    public class PanelPlayerMe : BasePlayerPanel
    {
        [Header("UI Components")]
        [SerializeField] private Image imageBackground; // Background màu player
        [SerializeField] private TextMeshProUGUI textPlayerName;
        [SerializeField] private TextMeshProUGUI textMoney;
        [SerializeField] private Image imageAvatar;
        
        [Header("Avatar Sprites")]
        [SerializeField] private Sprite spriteMale;
        [SerializeField] private Sprite spriteFemale;
        
        [Header("Background Settings")]
        [SerializeField] private float backgroundAlpha = 0.3f; // Độ trong suốt nền
        
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
            // PanelPlayerMe doesn't need to subscribe to events
            // It gets updated externally by PanelGame
        }
        
        /// <summary>
        /// Update display with current player data
        /// </summary>
        protected override void UpdateDisplay()
        {
            if (player == null) return;
            
            // Update name
            if (textPlayerName != null)
            {
                textPlayerName.text = player.PlayerName;
            }
            
            // Update money
            if (textMoney != null)
            {
                textMoney.text = $"${player.Money}";
            }
            
            // Update avatar sprite (nam/nữ)
            if (imageAvatar != null)
            {
                if (player.IsMale)
                {
                    imageAvatar.sprite = spriteMale;
                }
                else
                {
                    imageAvatar.sprite = spriteFemale;
                }
            }
            
            // ⭐ UPDATE BACKGROUND COLOR dựa trên player index
            if (imageBackground != null)
            {
                Color bgColor = player.GetPlayerColor();
                bgColor.a = backgroundAlpha; // Set alpha (transparency)
                imageBackground.color = bgColor;
            }
        }
        
        /// <summary>
        /// Update money display
        /// </summary>
        public void UpdateMoney(int money)
        {
            if (textMoney != null)
            {
                textMoney.text = $"${money}";
            }
        }
    }
}

