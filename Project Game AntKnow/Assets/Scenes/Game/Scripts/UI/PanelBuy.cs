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
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color cannotAffordColor = Color.red;

        [Header("Timeout Settings")]
        [SerializeField] private float autoSkipTimeout = 10f; // 10 seconds timeout
        [SerializeField] private TextMeshProUGUI textTimer; // Optional timer display

        private int selectedLevel = 0; // 0 = không mua, 1-4 = house level, 5 = hotel
        private int currentMoney = 0;
        private int basePrice = 0;
        private int currentLevel = 0;
        private string propertyName = "";
        private string ownerName = "";

        private System.Action<int> onBuyCallback;
        private System.Action onSkipCallback;

        private Coroutine timeoutCoroutine;
        
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

            // ⭐ KHÔNG set inactive trong Awake()
            // Để Unity Inspector quyết định initial state
            // ShowBuy() sẽ tự activate khi cần
        }
        
        /// <summary>
        /// Show panel mua nhà mới (ô trống)
        /// </summary>
        public void ShowBuy(string propName, int price, int playerMoney, System.Action<int> onBuy, System.Action onSkip)
        {
            Debug.Log($"[PanelBuy] ShowBuy called: {propName}, Price: {price}, Money: {playerMoney}");

            propertyName = propName;
            basePrice = price;
            currentMoney = playerMoney;
            currentLevel = 0;
            ownerName = "";
            selectedLevel = 0;

            onBuyCallback = onBuy;
            onSkipCallback = onSkip;

            UpdateDisplay();

            Debug.Log($"[PanelBuy] Setting active to TRUE");

            // ⭐ Check and activate ALL parents in hierarchy
            Transform current = transform.parent;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[PanelBuy] Parent '{current.name}' is inactive! Activating...");
                    current.gameObject.SetActive(true);
                }
                current = current.parent;
            }

            // ⭐ Activate this GameObject
            Debug.Log($"[PanelBuy] Before SetActive: activeSelf={gameObject.activeSelf}");
            gameObject.SetActive(true);
            Debug.Log($"[PanelBuy] After SetActive: activeSelf={gameObject.activeSelf}");

            Debug.Log($"[PanelBuy] Panel is now active: {gameObject.activeInHierarchy}");

            // ⭐ If still not active, log full hierarchy
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("[PanelBuy] Panel still not active! Checking hierarchy...");
                Transform node = transform;
                while (node != null)
                {
                    Debug.LogError($"  - {node.name}: activeSelf={node.gameObject.activeSelf}, activeInHierarchy={node.gameObject.activeInHierarchy}");
                    node = node.parent;
                }
                return; // ⭐ Don't start timeout if inactive
            }

            // ⭐ Start timeout coroutine
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
            }
            timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
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
            // Nếu là ô trống (currentLevel = 0): Tất cả button house sáng, hotel mờ (chưa có house 4)
            // Nếu là ô của mình: Button đã mua thì mờ đi, chỉ mua thêm được
            // Hotel chỉ mua được khi đã có House 4 (currentLevel = 4)
            
            SetButtonState(btnHouse1, currentLevel < 1, selectedLevel == 1);
            SetButtonState(btnHouse2, currentLevel < 2, selectedLevel == 2);
            SetButtonState(btnHouse3, currentLevel < 3, selectedLevel == 3);
            SetButtonState(btnHouse4, currentLevel < 4, selectedLevel == 4);
            
            // Hotel chỉ enable khi currentLevel = 4 (đã có 4 houses)
            SetButtonState(btnHotel, currentLevel >= 4, selectedLevel == 5);
        }
        
        /// <summary>
        /// Set button state
        /// </summary>
        private void SetButtonState(Button btn, bool interactable, bool selected)
        {
            if (btn == null) return;
            
            btn.interactable = interactable;
            
            var colors = btn.colors;
            if (!interactable)
            {
                // Đã mua rồi hoặc chưa đủ điều kiện - mờ đi
                colors.normalColor = disabledColor;
                colors.highlightedColor = disabledColor;
            }
            else if (selected)
            {
                // Đang chọn - sáng xanh
                colors.normalColor = selectedColor;
                colors.highlightedColor = selectedColor;
            }
            else
            {
                // Chưa chọn - màu bình thường
                colors.normalColor = normalColor;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
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
                // Nếu là ô trống: Hiển thị giá đất
                if (currentLevel == 0)
                {
                    textPrice.text = $"Giá đất: {basePrice}";
                    textPrice.color = normalColor;
                }
                else
                {
                    // Nếu là ô của mình nhưng chưa chọn nâng cấp
                    textPrice.text = "Chọn nhà để nâng cấp";
                    textPrice.color = normalColor;
                }
                btnBuy.interactable = false;
                return;
            }
            
            // Calculate total price
            int totalPrice = CalculateTotalPrice();
            
            // Check if can afford
            bool canAfford = currentMoney >= totalPrice;
            
            // Update text
            if (currentLevel == 0)
            {
                textPrice.text = $"Tổng: {totalPrice} (Đất + Nhà {selectedLevel})";
            }
            else
            {
                textPrice.text = $"Nâng cấp: +{totalPrice}";
            }
            
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
            
            // Giá houses (chỉ tính những nhà chưa mua)
            for (int i = currentLevel + 1; i <= selectedLevel && i <= 4; i++)
            {
                total += GetHousePrice(i);
            }
            
            // Giá hotel (nếu chọn hotel)
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
            // Mỗi nhà giá = basePrice
            return basePrice;
        }
        
        /// <summary>
        /// Get hotel price
        /// </summary>
        private int GetHotelPrice()
        {
            // Hotel = basePrice * 4 (hoặc theo công thức khác)
            return basePrice * 4;
        }
        
        /// <summary>
        /// On buy clicked
        /// </summary>
        private void OnBuyClicked()
        {
            if (selectedLevel == 0) return;

            // ⭐ Stop timeout
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            // Callback với level được chọn
            onBuyCallback?.Invoke(selectedLevel);
            Hide();
        }

        /// <summary>
        /// On skip clicked
        /// </summary>
        private void OnSkipClicked()
        {
            // ⭐ Stop timeout
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            onSkipCallback?.Invoke();
            Hide();
        }

        /// <summary>
        /// Timeout coroutine - Auto skip after X seconds
        /// </summary>
        private System.Collections.IEnumerator TimeoutCoroutine()
        {
            float remainingTime = autoSkipTimeout;

            while (remainingTime > 0)
            {
                // Update timer display (if exists)
                if (textTimer != null)
                {
                    textTimer.text = $"Thời gian: {Mathf.CeilToInt(remainingTime)}s";
                }

                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

            // Timeout - Auto skip
            Debug.Log("[PanelBuy] Timeout! Auto skipping...");

            if (textTimer != null)
            {
                textTimer.text = "Hết giờ!";
            }

            yield return new WaitForSeconds(0.5f);

            // Auto skip
            onSkipCallback?.Invoke();
            Hide();
        }

        /// <summary>
        /// Hide panel
        /// </summary>
        public void Hide()
        {
            // ⭐ Stop timeout when hiding
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            // Clear timer display
            if (textTimer != null)
            {
                textTimer.text = "";
            }

            gameObject.SetActive(false);
        }
    }
}

