using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace AntKnow.Shop
{
    /// <summary>
    /// Individual shop item display
    /// Shows icon, name, price
    /// Handles click event to open purchase confirmation
    /// </summary>
    public class ShopItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Image currencyIcon; // AntCoin or DCoin icon
        public Button buyButton;

        [Header("Item Data")]
        public string shopId;
        public string entryId;
        public string itemName;
        public int price;
        public string currency; // "antCoin" or "dCoin"
        public string iconPath;
        public string description;

        // Event when item clicked
        public event Action<ShopItem> OnItemClicked;

        private void Awake()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyButtonClicked);
            }
        }

        /// <summary>
        /// Setup shop item data
        /// </summary>
        public void Setup(string shopId, string entryId, string itemName, int price, string currency, string iconPath, string description = "")
        {
            this.shopId = shopId;
            this.entryId = entryId;
            this.itemName = itemName;
            this.price = price;
            this.currency = currency;
            this.iconPath = iconPath;
            this.description = description;

            UpdateDisplay();
        }

        /// <summary>
        /// Update UI display
        /// </summary>
        private void UpdateDisplay()
        {
            if (nameText != null)
            {
                nameText.text = itemName;
            }

            if (priceText != null)
            {
                priceText.text = price.ToString();
            }

            // Load icon
            if (iconImage != null && !string.IsNullOrEmpty(iconPath))
            {
                LoadIcon(iconPath);
            }

            // Set currency icon
            if (currencyIcon != null)
            {
                // TODO: Load AntCoin/DCoin sprite
                // For now, just show/hide based on currency type
                currencyIcon.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Load item icon from Resources or Addressables
        /// </summary>
        private void LoadIcon(string path)
        {
            // Debug exact path
            Debug.Log($"[ShopItem] Attempting to load icon from: Resources/{path}");
            
            // Try Items folder first (where most icons are)
            string itemsPath = path.Replace("Icons/", "Items/");
            Sprite sprite = Resources.Load<Sprite>(itemsPath);
            
            // If not found, try original path
            if (sprite == null)
            {
                Debug.Log($"[ShopItem] Not found in Items/, trying: {path}");
                sprite = Resources.Load<Sprite>(path);
            }
            
            if (sprite != null)
            {
                iconImage.sprite = sprite;
                Debug.Log($"[ShopItem] ✅ Icon loaded successfully!");
            }
            else
            {
                Debug.LogError($"[ShopItem] ❌ Icon not found in Items/ or Icons/: {path}");
            }
        }

        /// <summary>
        /// Handle buy button click
        /// </summary>
        private void OnBuyButtonClicked()
        {
            Debug.Log($"[ShopItem] Buy button clicked: {itemName} ({price} {currency})");
            OnItemClicked?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(OnBuyButtonClicked);
            }
        }
    }
}
