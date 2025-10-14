using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using AntKnow.Auth;

namespace AntKnow.Shop
{
    /// <summary>
    /// Main shop UI controller
    /// Loads shop items from Firebase and handles purchase flow
    /// </summary>
    public class ShopUIManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject shopPanel;
        public Transform itemsContainer;
        public GameObject shopItemPrefab;
        // ⚠️ REMOVED: PurchaseConfirmPanel - mua trực tiếp không cần xác nhận

        [Header("Shop Settings")]
        public string shopId = "default";  // ✅ Changed from "main" to match Firebase
        public bool autoLoadOnStart = true;

        [Header("Debug")]
        public bool enableDebugLogs = true;

        private List<GameObject> spawnedItems = new List<GameObject>();
        private FirebaseFirestore db;

        private void Start()
        {
            Debug.Log("[ShopUIManager] ========== START ==========");
            Debug.Log($"[ShopUIManager] shopPanel = {shopPanel?.name ?? "NULL"}");
            Debug.Log($"[ShopUIManager] itemsContainer = {itemsContainer?.name ?? "NULL"}");
            Debug.Log($"[ShopUIManager] shopItemPrefab = {shopItemPrefab?.name ?? "NULL"}");
            Debug.Log($"[ShopUIManager] autoLoadOnStart = {autoLoadOnStart}");
            
            // Initialize Firebase
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log($"[ShopUIManager] Firebase initialized: {db != null}");

            // ⚠️ REMOVED: Purchase confirmation panel subscription
            
            // Subscribe to shop service events
            ShopService.Instance.OnPurchaseSuccess += HandlePurchaseSuccess;
            ShopService.Instance.OnPurchaseError += HandlePurchaseError;

            if (autoLoadOnStart)
            {
                Debug.Log("[ShopUIManager] Auto-loading shop items...");
                LoadShopItems();
            }
            else
            {
                Debug.LogWarning("[ShopUIManager] autoLoadOnStart is FALSE - items will NOT load automatically!");
            }
        }

        /// <summary>
        /// Load shop items from Firebase
        /// </summary>
        public async void LoadShopItems()
        {
            DebugLog($"Loading shop items from shops/{shopId}/entries...");

            ClearItems();

            try
            {
                var entriesRef = db.Collection("shops").Document(shopId).Collection("entries");
                var snapshot = await entriesRef.GetSnapshotAsync();

                DebugLog($"Found {snapshot.Count} shop entries");
                
                if (snapshot.Count == 0)
                {
                    Debug.LogWarning($"[ShopUIManager] ⚠️ NO ENTRIES in shops/{shopId}/entries! Check Firebase Console!");
                    return;
                }

                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();
                    
                    // Parse shop entry data
                    string itemId = GetString(data, "itemId");
                    string itemName = await GetItemName(itemId);
                    int priceAntCoin = GetInt(data, "priceAntCoin");
                    int priceDCoin = GetInt(data, "priceDCoin");
                    
                    // Determine which currency to use (prefer DCoin if available, otherwise AntCoin)
                    string currency = "antCoin";
                    int price = priceAntCoin;
                    
                    if (priceDCoin > 0)
                    {
                        currency = "dCoin";
                        price = priceDCoin;
                    }

                    // Create shop item
                    if (shopItemPrefab != null && itemsContainer != null)
                    {
                        GameObject itemObj = Instantiate(shopItemPrefab, itemsContainer);
                        ShopItem shopItem = itemObj.GetComponent<ShopItem>();

                        if (shopItem != null)
                        {
                            // Get item description from items collection
                            string description = await GetItemDescription(itemId);
                            string iconPath = $"Icons/{itemId}"; // Assumes icons are in Resources/Icons/

                            shopItem.Setup(shopId, doc.Id, itemName, price, currency, iconPath, description);
                            shopItem.OnItemClicked += OnShopItemClicked;

                            spawnedItems.Add(itemObj);

                            DebugLog($"Created shop item: {itemName} ({price} {currency})");
                        }
                    }
                }

                DebugLog($"Shop loaded with {spawnedItems.Count} items");
            }
            catch (System.Exception e)
            {
                DebugLogError($"Failed to load shop items: {e.Message}");
            }
        }

        /// <summary>
        /// Get item name from items collection
        /// </summary>
        private async System.Threading.Tasks.Task<string> GetItemName(string itemId)
        {
            try
            {
                var itemDoc = await db.Collection("items").Document(itemId).GetSnapshotAsync();
                if (itemDoc.Exists)
                {
                    var data = itemDoc.ToDictionary();
                    return GetString(data, "name", itemId);
                }
            }
            catch (System.Exception e)
            {
                DebugLogError($"Failed to get item name for {itemId}: {e.Message}");
            }

            return itemId;
        }

        /// <summary>
        /// Get item description from items collection
        /// </summary>
        private async System.Threading.Tasks.Task<string> GetItemDescription(string itemId)
        {
            try
            {
                var itemDoc = await db.Collection("items").Document(itemId).GetSnapshotAsync();
                if (itemDoc.Exists)
                {
                    var data = itemDoc.ToDictionary();
                    return GetString(data, "description", "");
                }
            }
            catch (System.Exception e)
            {
                DebugLogError($"Failed to get item description for {itemId}: {e.Message}");
            }

            return "";
        }

        /// <summary>
        /// Clear all spawned items
        /// </summary>
        private void ClearItems()
        {
            foreach (var item in spawnedItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            spawnedItems.Clear();
        }

        /// <summary>
        /// Handle shop item clicked - BUY DIRECTLY (no confirmation)
        /// </summary>
        private async void OnShopItemClicked(ShopItem item)
        {
            DebugLog($"Shop item clicked: {item.itemName} - Buying directly...");

            // Check if user has enough balance
            var gdm = GameDataManager.Instance;
            if (gdm == null)
            {
                DebugLogError("GameDataManager not found!");
                ShowErrorMessage("Error: Game data not loaded");
                return;
            }

            int balance = item.currency == "dCoin" ? gdm.currentDCoin : gdm.currentAntCoin;
            int totalCost = item.price;

            if (balance < totalCost)
            {
                DebugLogError($"Not enough {item.currency}! Need {totalCost}, have {balance}");
                ShowErrorMessage($"Not enough {item.currency}!");
                return;
            }

            DebugLog($"Purchasing: {item.itemName} for {totalCost} {item.currency}");

            // Call ShopService to purchase (quantity = 1)
            bool success = await ShopService.Instance.PurchaseItem(item.shopId, item.entryId, item.currency, 1);

            if (!success)
            {
                DebugLogError("Purchase failed");
                ShowErrorMessage("Purchase failed. Please try again.");
            }
            // Success/error handling done in ShopService events
        }

        /// <summary>
        /// Show error message (simple debug log for now)
        /// TODO: Implement UI toast/notification
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            DebugLogError($"Error message: {message}");
            // TODO: Show UI notification panel
        }

        /// <summary>
        /// Handle purchase success
        /// </summary>
        private void HandlePurchaseSuccess(string currency, int quantity)
        {
            DebugLog($"Purchase successful! Currency: {currency}, Qty: {quantity}");

            // ⚠️ REMOVED: PurchaseConfirmPanel.ShowSuccess()
            // TODO: Show success notification (toast/particle effect)

            // Refresh user data from Firebase
            var gdm = GameDataManager.Instance;
            if (gdm != null)
            {
                gdm.RefreshUserData();
            }
        }

        /// <summary>
        /// Handle purchase error
        /// </summary>
        private void HandlePurchaseError(string errorMessage)
        {
            DebugLogError($"Purchase error: {errorMessage}");

            // ⚠️ REMOVED: PurchaseConfirmPanel.ShowError()
            ShowErrorMessage(errorMessage);
        }

        /// <summary>
        /// Show shop panel
        /// </summary>
        public void ShowShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }

            LoadShopItems();
        }

        /// <summary>
        /// Hide shop panel
        /// </summary>
        public void HideShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        #region Helper Methods

        private string GetString(Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict.TryGetValue(key, out object value) && value != null)
            {
                return value.ToString();
            }
            return defaultValue;
        }

        private int GetInt(Dictionary<string, object> dict, string key, int defaultValue = 0)
        {
            if (dict.TryGetValue(key, out object value))
            {
                if (value is long longValue) return (int)longValue;
                if (value is int intValue) return intValue;
                if (int.TryParse(value.ToString(), out int parsed)) return parsed;
            }
            return defaultValue;
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ShopUIManager] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[ShopUIManager] {message}");
            }
        }

        #endregion

        private void OnDestroy()
        {
            // ⚠️ REMOVED: PurchaseConfirmPanel subscription
            
            ShopService.Instance.OnPurchaseSuccess -= HandlePurchaseSuccess;
            ShopService.Instance.OnPurchaseError -= HandlePurchaseError;

            ClearItems();
        }
    }
}
