using UnityEngine;
using TMPro;
using System.Collections.Generic;
using AntKnow.Auth;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Component hiển thị tổng stats từ loadout (equipment + cards)
    /// </summary>
    public class LoadoutStatsDisplay : MonoBehaviour
    {
        [Header("Stats Text")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text agilityText;
        [SerializeField] private TMP_Text intelligenceText;
        [SerializeField] private TMP_Text luckText;
        [SerializeField] private TMP_Text resistanceText;
        
        [Header("Base Stats Config")]
        [SerializeField] private int baseHealthLv1 = 100;
        [SerializeField] private int baseAgilityLv1 = 10;
        [SerializeField] private int baseIntelligenceLv1 = 10;
        [SerializeField] private int baseLuckLv1 = 10;
        [SerializeField] private int baseResistanceLv1 = 10;
        [SerializeField] private int statsPerLevel = 1; // Mỗi level +1 tất cả stats

        [Header("Card Config")]
        [SerializeField] private int attributePerLevel = 2; // Từ configs/gameplay: cards.upgrade.attributePerLevel

        [Header("References")]
        [SerializeField] private FirebaseAuthService firebaseAuthService;
        
        [Header("References")]
        [SerializeField] private ItemSlot hatSlot;
        [SerializeField] private ItemSlot shirtSlot;
        [SerializeField] private ItemSlot wingsSlot;
        [SerializeField] private ItemSlot shoesSlot;
        [SerializeField] private ItemSlot maskSlot;
        [SerializeField] private ItemSlot passiveCardSlot;
        [SerializeField] private ItemSlot activeCardSlot;
        
        private void Start()
        {
            // Subscribe to loadout slot changes
            if (hatSlot != null) hatSlot.OnItemChanged += OnLoadoutChanged;
            if (shirtSlot != null) shirtSlot.OnItemChanged += OnLoadoutChanged;
            if (wingsSlot != null) wingsSlot.OnItemChanged += OnLoadoutChanged;
            if (shoesSlot != null) shoesSlot.OnItemChanged += OnLoadoutChanged;
            if (maskSlot != null) maskSlot.OnItemChanged += OnLoadoutChanged;
            if (passiveCardSlot != null) passiveCardSlot.OnItemChanged += OnLoadoutChanged;
            if (activeCardSlot != null) activeCardSlot.OnItemChanged += OnLoadoutChanged;
            
            // Initial update
            UpdateStatsDisplay();
        }
        
        /// <summary>
        /// Callback khi loadout thay đổi
        /// </summary>
        private void OnLoadoutChanged(ItemSlot slot, InventoryItem item)
        {
            UpdateStatsDisplay();
        }
        
        /// <summary>
        /// Update stats display
        /// </summary>
        public void UpdateStatsDisplay()
        {
            // Calculate total stats
            var stats = CalculateTotalStats();
            
            // Update UI
            if (healthText != null)
            {
                healthText.text = $"HP: {stats.health}";
            }
            
            if (agilityText != null)
            {
                agilityText.text = $"Agility: {stats.agility}";
            }
            
            if (intelligenceText != null)
            {
                intelligenceText.text = $"Intelligence: {stats.intelligence}";
            }
            
            if (luckText != null)
            {
                luckText.text = $"Luck: {stats.luck}";
            }
            
            if (resistanceText != null)
            {
                resistanceText.text = $"Resistance: {stats.resistance}";
            }
            
            Debug.Log($"[LoadoutStats] Total Stats - HP:{stats.health} AGI:{stats.agility} INT:{stats.intelligence} LUCK:{stats.luck} RES:{stats.resistance}");
        }
        
        /// <summary>
        /// Calculate tổng stats từ base (user level) + equipment + cards
        /// </summary>
        private TotalStats CalculateTotalStats()
        {
            // Get user level từ Firestore
            int userLevel = GetUserLevel();

            // Calculate base stats từ user level
            // Công thức: baseStat = baseLv1 + (level - 1) * statsPerLevel
            var stats = new TotalStats
            {
                health = baseHealthLv1 + (userLevel - 1) * statsPerLevel,
                agility = baseAgilityLv1 + (userLevel - 1) * statsPerLevel,
                intelligence = baseIntelligenceLv1 + (userLevel - 1) * statsPerLevel,
                luck = baseLuckLv1 + (userLevel - 1) * statsPerLevel,
                resistance = baseResistanceLv1 + (userLevel - 1) * statsPerLevel
            };

            Debug.Log($"[LoadoutStats] User Level {userLevel} → Base Stats: HP:{stats.health} AGI:{stats.agility} INT:{stats.intelligence} LUCK:{stats.luck} RES:{stats.resistance}");
            
            // Add equipment stats
            AddItemStats(stats, hatSlot?.GetItem());
            AddItemStats(stats, shirtSlot?.GetItem());
            AddItemStats(stats, wingsSlot?.GetItem());
            AddItemStats(stats, shoesSlot?.GetItem());
            AddItemStats(stats, maskSlot?.GetItem());
            
            // Add card stats
            AddCardStats(stats, passiveCardSlot?.GetItem());
            AddCardStats(stats, activeCardSlot?.GetItem());
            
            return stats;
        }
        
        /// <summary>
        /// Add stats từ equipment item
        /// Lấy trực tiếp từ attributes trong Firestore (items/{itemId}/attributes)
        /// Ví dụ: equip.mask.basic có luck: 10 → Cộng 10 vào luck
        /// </summary>
        private void AddItemStats(TotalStats stats, InventoryItem item)
        {
            if (item == null || item.itemData == null || item.itemData.attributes == null)
                return;

            var attr = item.itemData.attributes;

            // Cộng trực tiếp từ attributes (không có level scaling cho equipment)
            stats.health += attr.health;
            stats.agility += attr.agility;
            stats.intelligence += attr.intelligence;
            stats.luck += attr.luck;
            stats.resistance += attr.resistance;

            Debug.Log($"[LoadoutStats] Equipment {item.itemData.name}: HP+{attr.health} AGI+{attr.agility} INT+{attr.intelligence} LUCK+{attr.luck} RES+{attr.resistance}");
        }
        
        /// <summary>
        /// Add stats từ skill card (với level scaling)
        /// Công thức: totalValue = baseValue + (level - 1) * attributePerLevel
        /// attributePerLevel lấy từ configs/gameplay: cards.upgrade.attributePerLevel = 2
        /// </summary>
        private void AddCardStats(TotalStats stats, InventoryItem card)
        {
            if (card == null || !card.IsSkillCard || card.itemData == null || card.itemData.attributes == null)
                return;

            var attr = card.itemData.attributes;
            string primaryStat = attr.primaryStat;

            // Get base value từ attributes
            int baseValue = GetAttributeValue(attr, primaryStat);

            // Calculate với level scaling
            // Ví dụ: Card Lv.5, base=10, attributePerLevel=2
            // → totalValue = 10 + (5-1)*2 = 18
            int totalValue = baseValue + (card.level - 1) * attributePerLevel;

            // Add to corresponding stat
            switch (primaryStat)
            {
                case "health":
                    stats.health += totalValue;
                    break;
                case "agility":
                    stats.agility += totalValue;
                    break;
                case "intelligence":
                    stats.intelligence += totalValue;
                    break;
                case "luck":
                    stats.luck += totalValue;
                    break;
                case "resistance":
                    stats.resistance += totalValue;
                    break;
            }

            Debug.Log($"[LoadoutStats] Card {card.itemData.name} Lv.{card.level}: {primaryStat}+{totalValue} (base:{baseValue} + {card.level-1}*{attributePerLevel})");
        }
        
        /// <summary>
        /// Get attribute value by name
        /// </summary>
        private int GetAttributeValue(ItemAttributes attributes, string statName)
        {
            switch (statName)
            {
                case "health": return attributes.health;
                case "agility": return attributes.agility;
                case "intelligence": return attributes.intelligence;
                case "luck": return attributes.luck;
                case "resistance": return attributes.resistance;
                default: return 0;
            }
        }
        
        /// <summary>
        /// Get user level từ Firestore (users/{uid}/level)
        /// </summary>
        private int GetUserLevel()
        {
            // TODO: Load từ Firestore users/{uid}/level
            // Tạm thời return 1 nếu chưa có data

            if (firebaseAuthService == null || firebaseAuthService.Auth == null || firebaseAuthService.Auth.CurrentUser == null)
            {
                Debug.LogWarning("[LoadoutStats] User not logged in, using level 1");
                return 1;
            }

            // TODO: Implement load level từ Firestore
            // var userDoc = await firestore.Collection("users").Document(uid).GetSnapshotAsync();
            // int level = userDoc.GetValue<int>("level");

            // Tạm thời return 1
            return 1;
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            if (hatSlot != null) hatSlot.OnItemChanged -= OnLoadoutChanged;
            if (shirtSlot != null) shirtSlot.OnItemChanged -= OnLoadoutChanged;
            if (wingsSlot != null) wingsSlot.OnItemChanged -= OnLoadoutChanged;
            if (shoesSlot != null) shoesSlot.OnItemChanged -= OnLoadoutChanged;
            if (maskSlot != null) maskSlot.OnItemChanged -= OnLoadoutChanged;
            if (passiveCardSlot != null) passiveCardSlot.OnItemChanged -= OnLoadoutChanged;
            if (activeCardSlot != null) activeCardSlot.OnItemChanged -= OnLoadoutChanged;
        }
    }
    
    /// <summary>
    /// Struct chứa tổng stats
    /// </summary>
    public struct TotalStats
    {
        public int health;
        public int agility;
        public int intelligence;
        public int luck;
        public int resistance;
    }
}

