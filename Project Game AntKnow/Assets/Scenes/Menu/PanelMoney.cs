using System;
using UnityEngine;
using UnityEngine.UI;
using AntKnow.Auth;
using AntKnow.Shop;

namespace AntKnow.Auth
{
    /// <summary>
    /// Panel hiển thị tiền tệ và thời gian thực
    /// Auto-refresh khi có giao dịch shop
    /// </summary>
    public class PanelMoney : MonoBehaviour
    {
        [Header("Currency Display")]
        [SerializeField] private Text textAntCoin;
        [SerializeField] private Text textDCoin;

        [Header("Time Display")]
        [SerializeField] private Text textTime;

        [Header("Settings")]
        [SerializeField] private bool updateTime = true;
        [SerializeField] private bool autoRefreshOnPurchase = true;

        private GameDataManager gameDataManager;
        private string lastTimeString = "";

        public void Initialize()
        {
            gameDataManager = GameDataManager.Instance;
            UpdateCurrencyDisplay();

            // Subscribe to shop purchase events
            if (autoRefreshOnPurchase)
            {
                ShopService.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (autoRefreshOnPurchase)
            {
                ShopService.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
            }
        }

        /// <summary>
        /// Handle purchase success - refresh display
        /// </summary>
        private void OnPurchaseSuccess(string currency, int quantity)
        {
            Debug.Log($"[PanelMoney] Purchase detected, refreshing display...");
            
            // Wait a frame for GameDataManager to update
            StartCoroutine(RefreshAfterDelay());
        }

        /// <summary>
        /// Refresh display after short delay
        /// </summary>
        private System.Collections.IEnumerator RefreshAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            UpdateCurrencyDisplay();
        }

        private void Update()
        {
            if (updateTime && textTime != null)
            {
                UpdateTimeDisplay();
            }
        }

        public void UpdateCurrencyDisplay()
        {
            Debug.Log("=== PANELMONEY DEBUG ===");
            
            if (gameDataManager == null) 
            {
                Debug.LogError("PanelMoney: GameDataManager is null!");
                return;
            }
            
            Debug.Log($"PanelMoney: GameDataManager currentAntCoin: {gameDataManager.currentAntCoin}");
            Debug.Log($"PanelMoney: GameDataManager currentDCoin: {gameDataManager.currentDCoin}");
            Debug.Log($"PanelMoney: TextAntCoin component: {textAntCoin != null}");
            Debug.Log($"PanelMoney: TextDCoin component: {textDCoin != null}");

            // Update AntCoin từ user data
            if (textAntCoin != null)
            {
                textAntCoin.text = gameDataManager.currentAntCoin.ToString();
                Debug.Log($"PanelMoney: AntCoin text set to: {textAntCoin.text}");
            }
            else
            {
                Debug.LogError("PanelMoney: TextAntCoin is null!");
            }

            // Update DCoin từ user data
            if (textDCoin != null)
            {
                textDCoin.text = gameDataManager.currentDCoin.ToString();
                Debug.Log($"PanelMoney: DCoin text set to: {textDCoin.text}");
            }
            else
            {
                Debug.LogError("PanelMoney: TextDCoin is null!");
            }

            Debug.Log("=== END PANELMONEY DEBUG ===");
        }

        /// <summary>
        /// Public method to refresh display manually
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateCurrencyDisplay();
        }

        private void UpdateTimeDisplay()
        {
            if (textTime != null)
            {
                // Lấy thời gian hiện tại theo giờ Việt Nam (UTC+7)
                DateTime vietnamTime = DateTime.UtcNow.AddHours(7);
                string currentTimeString = vietnamTime.ToString("HH:mm");
                
                // Chỉ cập nhật UI khi thời gian thay đổi
                if (currentTimeString != lastTimeString)
                {
                    textTime.text = currentTimeString;
                    lastTimeString = currentTimeString;
                }
            }
        }

        public void SetAntCoin(int amount)
        {
            if (textAntCoin != null)
            {
                textAntCoin.text = amount.ToString();
            }
        }

        public void SetDCoin(int amount)
        {
            if (textDCoin != null)
            {
                textDCoin.text = amount.ToString();
            }
        }

        public void SetTimeUpdate(bool enabled)
        {
            updateTime = enabled;
        }
    }
}
