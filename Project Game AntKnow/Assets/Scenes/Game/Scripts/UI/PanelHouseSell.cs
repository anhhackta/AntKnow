using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel bán nhà khi không đủ tiền trả
    /// </summary>
    public class PanelHouseSell : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Transform contentParent; // Parent cho list items
        [SerializeField] private GameObject propertyItemPrefab; // Prefab cho mỗi property item
        [SerializeField] private TextMeshProUGUI textDebt; // Số tiền cần trả
        [SerializeField] private TextMeshProUGUI textTotalSell; // Tổng tiền bán được
        [SerializeField] private TextMeshProUGUI textStatus; // Thông báo đủ tiền chưa
        [SerializeField] private Button btnSell;
        
        private List<PropertySellItem> propertyItems = new List<PropertySellItem>();
        private int debtAmount = 0;
        private int totalSellAmount = 0;
        
        private System.Action<List<int>> onSellCallback; // List of property IDs to sell
        
        private void Awake()
        {
            if (btnSell != null)
            {
                btnSell.onClick.AddListener(OnSellClicked);
            }
        }
        
        /// <summary>
        /// Show panel
        /// </summary>
        public void Show(int debt, List<PropertyData> ownedProperties, System.Action<List<int>> onSell)
        {
            debtAmount = debt;
            onSellCallback = onSell;
            totalSellAmount = 0;
            
            // Clear old items
            ClearPropertyItems();
            
            // Create property items
            foreach (var property in ownedProperties)
            {
                CreatePropertyItem(property);
            }
            
            UpdateDisplay();
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Clear property items
        /// </summary>
        private void ClearPropertyItems()
        {
            foreach (var item in propertyItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            propertyItems.Clear();
        }
        
        /// <summary>
        /// Create property item
        /// </summary>
        private void CreatePropertyItem(PropertyData property)
        {
            if (propertyItemPrefab == null || contentParent == null) return;
            
            GameObject itemObj = Instantiate(propertyItemPrefab, contentParent);
            PropertySellItem item = itemObj.GetComponent<PropertySellItem>();
            
            if (item != null)
            {
                // Calculate sell price (60% of purchase price)
                int sellPrice = Mathf.RoundToInt(property.sellPrice * 0.6f);
                property.sellPrice = sellPrice;
                
                item.Initialize(property, OnPropertySelectionChanged);
                propertyItems.Add(item);
            }
        }
        
        /// <summary>
        /// On property selection changed
        /// </summary>
        private void OnPropertySelectionChanged()
        {
            // Recalculate total sell amount
            totalSellAmount = 0;
            
            foreach (var item in propertyItems)
            {
                if (item.IsSelected)
                {
                    totalSellAmount += item.SellPrice;
                }
            }
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update display
        /// </summary>
        private void UpdateDisplay()
        {
            // Update debt text
            if (textDebt != null)
            {
                textDebt.text = $"Cần trả: {debtAmount}";
            }
            
            // Update total sell text
            if (textTotalSell != null)
            {
                textTotalSell.text = $"Bán được: {totalSellAmount}";
            }
            
            // Check if enough money
            bool isEnough = totalSellAmount >= debtAmount;
            
            // Update status text
            if (textStatus != null)
            {
                if (isEnough)
                {
                    textStatus.text = "✓ Đủ tiền trả";
                    textStatus.color = Color.green;
                }
                else
                {
                    textStatus.text = $"✗ Còn thiếu {debtAmount - totalSellAmount}";
                    textStatus.color = Color.red;
                }
            }
            
            // Update sell button
            if (btnSell != null)
            {
                btnSell.interactable = isEnough;
            }
        }
        
        /// <summary>
        /// On sell clicked
        /// </summary>
        private void OnSellClicked()
        {
            // Get selected property IDs
            List<int> selectedPropertyIds = new List<int>();
            
            foreach (var item in propertyItems)
            {
                if (item.IsSelected)
                {
                    selectedPropertyIds.Add(item.PropertyId);
                }
            }
            
            // Callback
            onSellCallback?.Invoke(selectedPropertyIds);
            
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
    
    /// <summary>
    /// Property data for sell panel
    /// </summary>
    [System.Serializable]
    public class PropertyData
    {
        public int propertyId;
        public string propertyName;
        public int level;
        public int sellPrice;
    }
}

