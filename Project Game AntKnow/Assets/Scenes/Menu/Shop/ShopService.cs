using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Functions;
using Firebase.Extensions;

namespace AntKnow.Shop
{
    /// <summary>
    /// Service to handle shop purchases via Firebase Functions
    /// Calls purchaseItem cloud function
    /// </summary>
    public class ShopService : MonoBehaviour
    {
        private static ShopService _instance;
        public static ShopService Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ShopService");
                    _instance = go.AddComponent<ShopService>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Settings")]
        public bool enableDebugLogs = true;

        // Events
        public event Action<string, int> OnPurchaseSuccess; // (currency, amount spent)
        public event Action<string> OnPurchaseError;

        private FirebaseFunctions functions;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFirebase();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeFirebase()
        {
            try
            {
                functions = FirebaseFunctions.DefaultInstance;
                
                // Use Asia Southeast 1 region (same as Cloud Functions)
                functions.UseFunctionsEmulator("asia-southeast1");
                
                DebugLog("ShopService initialized with Firebase Functions");
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to initialize Firebase Functions: {e.Message}");
            }
        }

        /// <summary>
        /// Purchase item from shop
        /// </summary>
        /// <param name="shopId">Shop ID (e.g., "main")</param>
        /// <param name="entryId">Entry ID in shop</param>
        /// <param name="currency">Currency type: "antCoin" or "dCoin"</param>
        /// <param name="quantity">Quantity to purchase</param>
        public async Task<bool> PurchaseItem(string shopId, string entryId, string currency, int quantity = 1)
        {
            if (functions == null)
            {
                DebugLogError("Firebase Functions not initialized!");
                OnPurchaseError?.Invoke("Firebase not ready");
                return false;
            }

            DebugLog($"[ShopService] Purchasing: shopId={shopId}, entryId={entryId}, currency={currency}, qty={quantity}");

            try
            {
                // Call purchaseItem cloud function
                var data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "shopId", shopId },
                    { "entryId", entryId },
                    { "currency", currency },
                    { "quantity", quantity }
                };

                var callableReference = functions.GetHttpsCallable("purchaseItem");
                var result = await callableReference.CallAsync(data).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        DebugLogError($"Purchase failed: {task.Exception}");
                        OnPurchaseError?.Invoke(task.Exception.Message);
                        return false;
                    }
                    else if (task.IsCanceled)
                    {
                        DebugLogError("Purchase canceled");
                        OnPurchaseError?.Invoke("Purchase canceled");
                        return false;
                    }
                    else
                    {
                        DebugLog($"Purchase successful! Result: {task.Result.Data}");
                        OnPurchaseSuccess?.Invoke(currency, quantity);
                        return true;
                    }
                });

                return result;
            }
            catch (Exception e)
            {
                DebugLogError($"Exception during purchase: {e.Message}");
                OnPurchaseError?.Invoke(e.Message);
                return false;
            }
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ShopService] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[ShopService] {message}");
            }
        }
    }
}
