using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Item trong list bán nhà
    /// </summary>
    public class PropertySellItem : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI textPropertyName;
        [SerializeField] private TextMeshProUGUI textLevel;
        [SerializeField] private TextMeshProUGUI textSellPrice;
        
        private int propertyId;
        private int sellPrice;
        private System.Action onSelectionChanged;
        
        public int PropertyId => propertyId;
        public int SellPrice => sellPrice;
        public bool IsSelected => toggle != null && toggle.isOn;
        
        private void Awake()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }
        
        /// <summary>
        /// Initialize item
        /// </summary>
        public void Initialize(PropertyData property, System.Action onChanged)
        {
            propertyId = property.propertyId;
            sellPrice = property.sellPrice;
            onSelectionChanged = onChanged;
            
            // Update UI
            if (textPropertyName != null)
            {
                textPropertyName.text = property.propertyName;
            }
            
            if (textLevel != null)
            {
                string levelText = property.level == 5 ? "Hotel" : $"House {property.level}";
                textLevel.text = levelText;
            }
            
            if (textSellPrice != null)
            {
                textSellPrice.text = $"{sellPrice}";
            }
            
            // Default: not selected
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }
        
        /// <summary>
        /// On toggle changed
        /// </summary>
        private void OnToggleChanged(bool isOn)
        {
            onSelectionChanged?.Invoke();
        }
    }
}

