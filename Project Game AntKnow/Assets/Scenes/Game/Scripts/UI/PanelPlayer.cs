using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin người chơi khác
    /// </summary>
    public class PanelPlayer : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textPlayerName;
        [SerializeField] private TextMeshProUGUI textMoney;
        [SerializeField] private Image imageAvatar;
        [SerializeField] private Image imageTurnIndicator;
        
        [Header("Avatar Colors")]
        [SerializeField] private Color maleColor = Color.blue;
        [SerializeField] private Color femaleColor = Color.magenta;
        
        private PlayerGameController player;
        
        /// <summary>
        /// Initialize panel
        /// </summary>
        public void Initialize(PlayerGameController playerController)
        {
            player = playerController;
            
            if (player != null)
            {
                gameObject.SetActive(true);
                UpdateDisplay();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Update display
        /// </summary>
        public void UpdateDisplay()
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
        /// Update money
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

