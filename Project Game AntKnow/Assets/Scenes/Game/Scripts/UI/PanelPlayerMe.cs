using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin người chơi bản thân
    /// </summary>
    public class PanelPlayerMe : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textPlayerName;
        [SerializeField] private TextMeshProUGUI textMoney;
        [SerializeField] private Image imageAvatar;
        [SerializeField] private Image imageTurnIndicator; // Highlight khi đến lượt
        
        [Header("Avatar Colors")]
        [SerializeField] private Color maleColor = Color.blue;
        [SerializeField] private Color femaleColor = Color.magenta;
        
        private PlayerGameController player;
        
        /// <summary>
        /// Initialize panel với player data
        /// </summary>
        public void Initialize(PlayerGameController playerController)
        {
            player = playerController;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update display
        /// </summary>
        public void UpdateDisplay()
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
                textMoney.text = $"{player.Money}";
            }
            
            // Update avatar color (based on gender)
            if (imageAvatar != null)
            {
                // Bạn có thể thay bằng sprite thật
                imageAvatar.color = player.IsMale ? maleColor : femaleColor;
            }
        }
        
        /// <summary>
        /// Set turn indicator (highlight khi đến lượt)
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

