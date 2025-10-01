using System;
using UnityEngine;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Data class cho item trong inventory
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        // Common fields
        public string docId;           // Document ID trong Firestore
        public string itemId;          // Reference tới items collection
        public string type;            // "skill_card" | "exp_card" | "equipment" | "material" | "repair_hammer"
        public DateTime createdAt;
        public DateTime updatedAt;
        
        // For non-stackable items (skill_card, equipment)
        public int level = 1;
        public int stars = 0;
        
        // For stackable items (exp_card, material, repair_hammer)
        public int qty = 0;
        
        // For equipment only
        public int durability = 100;
        
        // Status
        public string status = "active";
        
        // Cached item data from items collection
        public ItemData itemData;
        
        public bool IsStackable => type == "exp_card" || type == "material" || type == "repair_hammer";
        public bool IsSkillCard => type == "skill_card";
        public bool IsEquipment => type == "equipment";
    }
    
    /// <summary>
    /// Data class cho item definition từ items collection
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string itemId;
        public string name;
        public string type;
        public string rarity;          // "common" | "rare" | "epic" | "legendary"
        public string status;
        
        // Attributes (for equipment and skill cards)
        public ItemAttributes attributes;
        
        // Skill data (for skill cards only)
        public SkillData skill;
        
        // Equipment data (for equipment only)
        public EquipmentData equipment;
        
        // EXP data (for exp cards only)
        public ExpData exp;
        
        // Upgrade data
        public UpgradeData upgrade;
        
        // Localization
        public LocalizedText lang;
        
        // Icon
        public string icon;
    }
    
    [Serializable]
    public class ItemAttributes
    {
        public int health = 0;
        public int agility = 0;
        public int intelligence = 0;
        public int luck = 0;
        public int resistance = 0;
        
        // For skill cards
        public string primaryStat;     // "health" | "agility" | "intelligence" | "luck" | "resistance"
        public int attributePerLevel = 0;
    }
    
    [Serializable]
    public class SkillData
    {
        public string mode;            // "passive" | "active"
        public string effect;          // Effect description
        public int cooldownBaseTurns = 0;
    }
    
    [Serializable]
    public class EquipmentData
    {
        public string slot;            // "hat" | "shirt" | "wings" | "shoes" | "mask"
        public int durabilityMax = 100;
    }
    
    [Serializable]
    public class ExpData
    {
        public int xpValue = 0;
    }
    
    [Serializable]
    public class UpgradeData
    {
        public int costAntCoinPerLevel = 100;
        public bool preferCardExp = false;
    }
    
    [Serializable]
    public class LocalizedText
    {
        public LanguageText vi;
        public LanguageText en;
    }
    
    [Serializable]
    public class LanguageText
    {
        public string name;
        public string desc;
    }
}

