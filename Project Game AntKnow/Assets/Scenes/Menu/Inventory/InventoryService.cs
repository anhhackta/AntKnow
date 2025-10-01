using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using AntKnow.Auth;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Service quản lý inventory và loadout
    /// </summary>
    public class InventoryService : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public static event Action<List<InventoryItem>> OnInventoryLoaded;
        public static event Action<LoadoutData> OnLoadoutLoaded;
        public static event Action<string> OnInventoryError;
        
        // Cached data
        private List<InventoryItem> cachedInventory = new List<InventoryItem>();
        private LoadoutData cachedLoadout;
        private Dictionary<string, ItemData> itemDataCache = new Dictionary<string, ItemData>();
        
        private static InventoryService _instance;
        public static InventoryService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InventoryService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InventoryService");
                        _instance = go.AddComponent<InventoryService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private FirebaseFirestore firestore;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            firestore = FirebaseFirestore.DefaultInstance;
        }
        
        /// <summary>
        /// Load inventory của user
        /// </summary>
        public async Task<List<InventoryItem>> LoadInventoryAsync(string uid)
        {
            try
            {
                DebugLog($"Loading inventory for user: {uid}");
                
                var inventoryRef = firestore.Collection("users").Document(uid).Collection("inventory");
                var snapshot = await inventoryRef.GetSnapshotAsync();
                
                cachedInventory.Clear();
                
                foreach (var doc in snapshot.Documents)
                {
                    var item = ParseInventoryItem(doc);
                    if (item != null)
                    {
                        // Load item data from items collection
                        item.itemData = await GetItemDataAsync(item.itemId);
                        cachedInventory.Add(item);
                    }
                }
                
                DebugLog($"Loaded {cachedInventory.Count} items from inventory");
                OnInventoryLoaded?.Invoke(cachedInventory);
                
                return cachedInventory;
            }
            catch (Exception e)
            {
                DebugLogError($"Error loading inventory: {e.Message}");
                OnInventoryError?.Invoke($"Lỗi tải inventory: {e.Message}");
                return new List<InventoryItem>();
            }
        }
        
        /// <summary>
        /// Load loadout của user
        /// </summary>
        public async Task<LoadoutData> LoadLoadoutAsync(string uid, string slotId = "slot1")
        {
            try
            {
                DebugLog($"Loading loadout {slotId} for user: {uid}");
                
                var loadoutRef = firestore.Collection("users").Document(uid).Collection("loadouts").Document(slotId);
                var snapshot = await loadoutRef.GetSnapshotAsync();
                
                if (snapshot.Exists)
                {
                    cachedLoadout = ParseLoadout(snapshot);
                }
                else
                {
                    // Create default loadout
                    cachedLoadout = new LoadoutData { slotId = slotId };
                    await SaveLoadoutAsync(uid, cachedLoadout);
                }
                
                DebugLog($"Loaded loadout: {cachedLoadout.skillCardIds.Count} cards, {cachedLoadout.equipmentSet.GetAllEquipmentIds().Count} equipment");
                OnLoadoutLoaded?.Invoke(cachedLoadout);
                
                return cachedLoadout;
            }
            catch (Exception e)
            {
                DebugLogError($"Error loading loadout: {e.Message}");
                OnInventoryError?.Invoke($"Lỗi tải loadout: {e.Message}");
                return new LoadoutData();
            }
        }
        
        /// <summary>
        /// Save loadout
        /// </summary>
        public async Task<bool> SaveLoadoutAsync(string uid, LoadoutData loadout)
        {
            try
            {
                DebugLog($"Saving loadout {loadout.slotId} for user: {uid}");
                
                var loadoutRef = firestore.Collection("users").Document(uid).Collection("loadouts").Document(loadout.slotId);
                
                var data = new Dictionary<string, object>
                {
                    { "active", loadout.active },
                    { "skillCardIds", loadout.skillCardIds },
                    { "equipmentSet", new Dictionary<string, object>
                        {
                            { "hatId", loadout.equipmentSet.hatId ?? "" },
                            { "shirtId", loadout.equipmentSet.shirtId ?? "" },
                            { "wingsId", loadout.equipmentSet.wingsId ?? "" },
                            { "shoesId", loadout.equipmentSet.shoesId ?? "" },
                            { "maskId", loadout.equipmentSet.maskId ?? "" }
                        }
                    },
                    { "updatedAt", FieldValue.ServerTimestamp }
                };
                
                await loadoutRef.SetAsync(data);
                
                cachedLoadout = loadout;
                DebugLog("Loadout saved successfully");
                
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Error saving loadout: {e.Message}");
                OnInventoryError?.Invoke($"Lỗi lưu loadout: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get item data từ items collection (với cache)
        /// </summary>
        public async Task<ItemData> GetItemDataAsync(string itemId)
        {
            // Check cache
            if (itemDataCache.ContainsKey(itemId))
            {
                return itemDataCache[itemId];
            }
            
            try
            {
                var itemRef = firestore.Collection("items").Document(itemId);
                var snapshot = await itemRef.GetSnapshotAsync();
                
                if (snapshot.Exists)
                {
                    var itemData = ParseItemData(snapshot);
                    itemDataCache[itemId] = itemData;
                    return itemData;
                }
                else
                {
                    DebugLogError($"Item not found: {itemId}");
                    return null;
                }
            }
            catch (Exception e)
            {
                DebugLogError($"Error loading item data: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get cached inventory
        /// </summary>
        public List<InventoryItem> GetCachedInventory()
        {
            return cachedInventory;
        }
        
        /// <summary>
        /// Get cached loadout
        /// </summary>
        public LoadoutData GetCachedLoadout()
        {
            return cachedLoadout;
        }
        
        /// <summary>
        /// Filter inventory by type
        /// </summary>
        public List<InventoryItem> GetItemsByType(string type)
        {
            return cachedInventory.FindAll(item => item.type == type);
        }
        
        /// <summary>
        /// Get skill cards only
        /// </summary>
        public List<InventoryItem> GetSkillCards()
        {
            return GetItemsByType("skill_card");
        }
        
        /// <summary>
        /// Get equipment only
        /// </summary>
        public List<InventoryItem> GetEquipment()
        {
            return GetItemsByType("equipment");
        }
        
        // Helper methods
        private InventoryItem ParseInventoryItem(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();
            var item = new InventoryItem
            {
                docId = doc.Id,
                itemId = data.ContainsKey("itemId") ? data["itemId"].ToString() : "",
                type = data.ContainsKey("type") ? data["type"].ToString() : "",
                status = data.ContainsKey("status") ? data["status"].ToString() : "active"
            };
            
            // Parse timestamps
            if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp)
                item.createdAt = ((Timestamp)data["createdAt"]).ToDateTime();
            if (data.ContainsKey("updatedAt") && data["updatedAt"] is Timestamp)
                item.updatedAt = ((Timestamp)data["updatedAt"]).ToDateTime();
            
            // Parse level and stars for non-stackable items
            if (data.ContainsKey("level"))
                item.level = Convert.ToInt32(data["level"]);
            if (data.ContainsKey("stars"))
                item.stars = Convert.ToInt32(data["stars"]);
            
            // Parse qty for stackable items
            if (data.ContainsKey("qty"))
                item.qty = Convert.ToInt32(data["qty"]);
            
            // Parse durability for equipment
            if (data.ContainsKey("durability"))
                item.durability = Convert.ToInt32(data["durability"]);
            
            return item;
        }
        
        private LoadoutData ParseLoadout(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();
            var loadout = new LoadoutData
            {
                slotId = doc.Id,
                active = data.ContainsKey("active") ? Convert.ToBoolean(data["active"]) : true
            };
            
            // Parse skillCardIds
            if (data.ContainsKey("skillCardIds") && data["skillCardIds"] is List<object> cardList)
            {
                foreach (var cardId in cardList)
                {
                    loadout.skillCardIds.Add(cardId.ToString());
                }
            }
            
            // Parse equipmentSet
            if (data.ContainsKey("equipmentSet") && data["equipmentSet"] is Dictionary<string, object> equipSet)
            {
                if (equipSet.ContainsKey("hatId"))
                    loadout.equipmentSet.hatId = equipSet["hatId"]?.ToString();
                if (equipSet.ContainsKey("shirtId"))
                    loadout.equipmentSet.shirtId = equipSet["shirtId"]?.ToString();
                if (equipSet.ContainsKey("wingsId"))
                    loadout.equipmentSet.wingsId = equipSet["wingsId"]?.ToString();
                if (equipSet.ContainsKey("shoesId"))
                    loadout.equipmentSet.shoesId = equipSet["shoesId"]?.ToString();
                if (equipSet.ContainsKey("maskId"))
                    loadout.equipmentSet.maskId = equipSet["maskId"]?.ToString();
            }
            
            return loadout;
        }
        
        private ItemData ParseItemData(DocumentSnapshot doc)
        {
            // TODO: Implement full parsing
            // For now, return basic data
            var data = doc.ToDictionary();
            var itemData = new ItemData
            {
                itemId = doc.Id,
                name = data.ContainsKey("name") ? data["name"].ToString() : "",
                type = data.ContainsKey("type") ? data["type"].ToString() : "",
                rarity = data.ContainsKey("rarity") ? data["rarity"].ToString() : "common",
                status = data.ContainsKey("status") ? data["status"].ToString() : "active",
                icon = data.ContainsKey("icon") ? data["icon"].ToString() : ""
            };
            
            // TODO: Parse attributes, skill, equipment, exp, upgrade, lang
            
            return itemData;
        }
        
        private void DebugLog(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[InventoryService] {message}");
        }
        
        private void DebugLogError(string message)
        {
            Debug.LogError($"[InventoryService] {message}");
        }
    }
}

