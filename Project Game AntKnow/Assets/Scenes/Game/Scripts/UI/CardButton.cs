using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Button cho mỗi card trong PanelCard
    /// </summary>
    public class CardButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image imageCard;
        [SerializeField] private TextMeshProUGUI textCardName;
        [SerializeField] private TextMeshProUGUI textCooldown;
        
        private int cardId;
        private System.Action<int> onClickCallback;
        
        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
        }
        
        /// <summary>
        /// Initialize card button
        /// </summary>
        public void Initialize(CardData card, System.Action<int> onClick)
        {
            cardId = card.cardId;
            onClickCallback = onClick;
            
            // Update UI
            if (imageCard != null && card.cardSprite != null)
            {
                imageCard.sprite = card.cardSprite;
            }
            
            if (textCardName != null)
            {
                textCardName.text = card.cardName;
            }
            
            // Check cooldown
            bool onCooldown = card.cooldownRemaining > 0;
            
            if (textCooldown != null)
            {
                if (onCooldown)
                {
                    textCooldown.text = $"Cooldown: {card.cooldownRemaining}";
                    textCooldown.gameObject.SetActive(true);
                }
                else
                {
                    textCooldown.gameObject.SetActive(false);
                }
            }
            
            // Disable button if on cooldown
            if (button != null)
            {
                button.interactable = !onCooldown;
            }
        }
        
        /// <summary>
        /// On clicked
        /// </summary>
        private void OnClicked()
        {
            onClickCallback?.Invoke(cardId);
        }
    }
}

