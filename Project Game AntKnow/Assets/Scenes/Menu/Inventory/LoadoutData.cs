using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Data class cho loadout (saved combat setup)
    /// </summary>
    [Serializable]
    public class LoadoutData
    {
        public string slotId = "slot1";
        public bool active = true;
        public DateTime updatedAt;
        
        // Skill cards (2 cards: passive + active)
        public List<string> skillCardIds = new List<string>();
        
        // Equipment set (5 slots)
        public EquipmentSet equipmentSet = new EquipmentSet();
        
        public LoadoutData()
        {
            updatedAt = DateTime.UtcNow;
        }
    }
    
    [Serializable]
    public class EquipmentSet
    {
        public string hatId;
        public string shirtId;
        public string wingsId;
        public string shoesId;
        public string maskId;
        
        public string GetSlotId(string slot)
        {
            switch (slot)
            {
                case "hat": return hatId;
                case "shirt": return shirtId;
                case "wings": return wingsId;
                case "shoes": return shoesId;
                case "mask": return maskId;
                default: return null;
            }
        }
        
        public void SetSlotId(string slot, string docId)
        {
            switch (slot)
            {
                case "hat": hatId = docId; break;
                case "shirt": shirtId = docId; break;
                case "wings": wingsId = docId; break;
                case "shoes": shoesId = docId; break;
                case "mask": maskId = docId; break;
            }
        }
        
        public bool IsSlotEmpty(string slot)
        {
            return string.IsNullOrEmpty(GetSlotId(slot));
        }
        
        public List<string> GetAllEquipmentIds()
        {
            var ids = new List<string>();
            if (!string.IsNullOrEmpty(hatId)) ids.Add(hatId);
            if (!string.IsNullOrEmpty(shirtId)) ids.Add(shirtId);
            if (!string.IsNullOrEmpty(wingsId)) ids.Add(wingsId);
            if (!string.IsNullOrEmpty(shoesId)) ids.Add(shoesId);
            if (!string.IsNullOrEmpty(maskId)) ids.Add(maskId);
            return ids;
        }
    }
}

