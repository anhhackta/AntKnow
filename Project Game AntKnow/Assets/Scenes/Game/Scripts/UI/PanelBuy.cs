using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel mua/nâng cấp nhà
    /// </summary>
    public class PanelBuy : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textPropertyName;
        [SerializeField] private TextMeshProUGUI textOwnerName;
        [SerializeField] private TextMeshProUGUI textPrice;
        [SerializeField] private Button btnBuy;
        [SerializeField] private Button btnSkip;
        
        [Header("House Buttons")]
        [SerializeField] private Button btnHouse1;
        [SerializeField] private Button btnHouse2;
        [SerializeField] private Button btnHouse3;
        [SerializeField] private Button btnHouse4;
        [SerializeField] private Button btnHotel;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color cannotAffordColor = Color.red;
        
        private int selectedLevel = 0; // 0 = chưa chọn, 1-4 = house, 5 = hotel
        private int currentMoney = 0;
        private int basePrice = 0;
        private int currentLevel = 0;
        private string propertyName = "";
        private string ownerName = "";
        
        private System.Action<int> onBuyCallback;
        private System.Action onSkipCallback;
        
        private void Awake()
        {
            // Setup button listeners
            if (btnHouse1 != null) btnHouse1.onClick.AddListener(() => OnHouseButtonClicked(1));
            if (btnHouse2 != null) btnHouse2.onClick.AddListener(() => OnHouseButtonClicked(2));
            if (btnHouse3 != null) btnHouse3.onClick.AddListener(() => OnHouseButtonClicked(3));
            if (btnHouse4 != null) btnHouse4.onClick.AddListener(() => OnHouseButtonClicked(4));
            if (btnHotel != null) btnHotel.onClick.AddListener(() => OnHouseButtonClicked(5));
            
            if (btnBuy != null) btnBuy.onClick.AddListener(OnBuyClicked);
            if (btnSkip != null) btnSkip.onClick.AddListener(OnSkipClicked);
        }
        
        /// <summary>
        /// Show panel mua nhà mới (ô trống)
        /// </summary>
        public void ShowBuy(string propName, int price, int playerMoney, System.Action<int> onBuy, System.Action onSkip)
        {
            propertyName = propName;
            basePrice = price;
            currentMoney = playerMoney;
            currentLevel = 0;
            ownerName = "";
            selectedLevel = 0;
            
            onBuyCallback = onBuy;
            onSkipCallback = onSkip;
            
            UpdateDisplay();
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Show panel nâng cấp nhà
        /// </summary>
        public void ShowUpgrade(string propName, int price, int level, int playerMoney, System.Action<int> onUpgrade, System.Action onSkip)
        {
            propertyName = propName;
            basePrice = price;
            currentMoney = playerMoney;
            currentLevel = level;
            ownerName = "Bạn";
            selectedLevel = 0;
            
            onBuyCallback = onUpgrade;
            onSkipCallback = onSkip;
            
            UpdateDisplay();
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Update display
        /// </summary>
        private void UpdateDisplay()
        {
            // Update property name
            if (textPropertyName != null)
            {
                textPropertyName.text = propertyName;
            }
            
            // Update owner name
            if (textOwnerName != null)
            {
                if (string.IsNullOrEmpty(ownerName))
                {
                    textOwnerName.text = "Chưa có chủ";
                }
                else
                {
                    textOwnerName.text = $"Chủ: {ownerName}";
                }
            }
            
            // Update house buttons
            UpdateHouseButtons();
            
            // Update price
            UpdatePrice();
        }
        
        /// <summary>
        /// Update house buttons
        /// </summary>
        private void UpdateHouseButtons()
        {
            // Disable buttons dưới current level
            SetButtonState(btnHouse1, currentLevel < 1, selectedLevel == 1);
            SetButtonState(btnHouse2, currentLevel < 2, selectedLevel == 2);
            SetButtonState(btnHouse3, currentLevel < 3, selectedLevel == 3);
            SetButtonState(btnHouse4, currentLevel < 4, selectedLevel == 4);
            SetButtonState(btnHotel, currentLevel < 5, selectedLevel == 5);
        }
        
        /// <summary>
        /// Set button state
        /// </summary>
        private void SetButtonState(Button btn, bool interactable, bool selected)
        {
            if (btn == null) return;
            
            btn.interactable = interactable;
            
            var colors = btn.colors;
            if (selected)
            {
                colors.normalColor = selectedColor;
            }
            else
            {
                colors.normalColor = normalColor;
            }
            btn.colors = colors;
        }
        
        /// <summary>
        /// On house button clicked
        /// </summary>
        private void OnHouseButtonClicked(int level)
        {
            // Toggle selection
            if (selectedLevel == level)
            {
                selectedLevel = 0; // Bỏ chọn
            }
            else
            {
                selectedLevel = level; // Chọn
            }
            
            UpdateHouseButtons();
            UpdatePrice();
        }
        
        /// <summary>
        /// Update price display
        /// </summary>
        private void UpdatePrice()
        {
            if (textPrice == null) return;
            
            if (selectedLevel == 0)
            {
                textPrice.text = "Chọn level để xem giá";
                textPrice.color = normalColor;
                btnBuy.interactable = false;
                return;
            }
            
            // Calculate total price
            int totalPrice = CalculateTotalPrice();
            
            // Check if can afford
            bool canAfford = currentMoney >= totalPrice;
            
            // Update text
            textPrice.text = $"Giá: {totalPrice}";
            textPrice.color = canAfford ? normalColor : cannotAffordColor;
            
            // Update buy button
            btnBuy.interactable = canAfford;
        }
        
        /// <summary>
        /// Calculate total price
        /// </summary>
        private int CalculateTotalPrice()
        {
            int total = 0;
            
            // Giá đất (nếu mua lần đầu)
            if (currentLevel == 0)
            {
                total += basePrice;
            }
            
            // Giá houses
            for (int i = currentLevel + 1; i <= selectedLevel && i <= 4; i++)
            {
                total += GetHousePrice(i);
            }
            
            // Giá hotel
            if (selectedLevel == 5)
            {
                total += GetHotelPrice();
            }
            
            return total;
        }
        
        /// <summary>
        /// Get house price by level
        /// </summary>
        private int GetHousePrice(int level)
        {
            switch (level)
            {
                case 1: return basePrice * 100 / 100; // 100%
                case 2: return basePrice * 150 / 100; // 150%
                case 3: return basePrice * 200 / 100; // 200%
                case 4: return basePrice * 250 / 100; // 250%
                default: return 0;
            }
        }
        
        /// <summary>
        /// Get hotel price
        /// </summary>
        private int GetHotelPrice()
        {
            return basePrice * 400 / 100; // 400%
        }
        
        /// <summary>
        /// On buy clicked
        /// </summary>
        private void OnBuyClicked()
        {
            if (selectedLevel == 0) return;
            
            onBuyCallback?.Invoke(selectedLevel);
            Hide();
        }
        
        /// <summary>
        /// On skip clicked
        /// </summary>
        private void OnSkipClicked()
        {
            onSkipCallback?.Invoke();
            Hide();
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

