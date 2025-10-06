using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Quản lý property system (mua, nâng cấp, thuê, bán)
    /// </summary>
    public class PropertyManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int[] upgradeCostPct = new int[] { 100, 150, 200, 250 }; // Level 1-4
        [SerializeField] private int[] rentPct = new int[] { 10, 25, 50, 75, 100, 250 }; // Level 0-5
        [SerializeField] private int hotelUpgradePct = 400;
        [SerializeField] private int hotelRentPct = 250;
        
        // Property ownership: tileId -> owner player index
        private Dictionary<int, int> propertyOwners = new Dictionary<int, int>();
        
        // Property levels: tileId -> level (0-5, 5 = hotel)
        private Dictionary<int, int> propertyLevels = new Dictionary<int, int>();
        
        // Property rent multipliers: tileId -> multiplier (1 or 2 from Agility)
        private Dictionary<int, float> propertyRentMultipliers = new Dictionary<int, float>();
        
        /// <summary>
        /// Check if property is owned
        /// </summary>
        public bool IsPropertyOwned(int tileId)
        {
            return propertyOwners.ContainsKey(tileId);
        }
        
        /// <summary>
        /// Get property owner index
        /// </summary>
        public int GetPropertyOwner(int tileId)
        {
            if (propertyOwners.ContainsKey(tileId))
            {
                return propertyOwners[tileId];
            }
            return -1;
        }
        
        /// <summary>
        /// Get property level
        /// </summary>
        public int GetPropertyLevel(int tileId)
        {
            if (propertyLevels.ContainsKey(tileId))
            {
                return propertyLevels[tileId];
            }
            return 0;
        }
        
        /// <summary>
        /// Buy property
        /// </summary>
        public bool BuyProperty(int tileId, int playerIndex, int basePrice, PlayerGameController player)
        {
            // Check if already owned
            if (IsPropertyOwned(tileId))
            {
                Debug.LogWarning($"[PropertyManager] Property {tileId} already owned!");
                return false;
            }
            
            // Check money
            if (player.Money < basePrice)
            {
                Debug.LogWarning($"[PropertyManager] Player {playerIndex} cannot afford property {tileId}");
                return false;
            }
            
            // Buy
            player.SubtractMoney(basePrice);
            propertyOwners[tileId] = playerIndex;
            propertyLevels[tileId] = 0; // Level 0 = đất trống
            propertyRentMultipliers[tileId] = 1f;
            
            // Check Agility: Nhân đôi tiền thuê
            if (StatsCalculator.CheckAgilityForDoubleRent(player.Agility))
            {
                propertyRentMultipliers[tileId] = 2f;
                Debug.Log($"[PropertyManager] AGILITY! Rent x2 for property {tileId}");
            }
            
            Debug.Log($"[PropertyManager] Player {playerIndex} bought property {tileId} for {basePrice}");
            return true;
        }
        
        /// <summary>
        /// Upgrade property
        /// </summary>
        public bool UpgradeProperty(int tileId, int targetLevel, int basePrice, PlayerGameController player)
        {
            // Check ownership
            int ownerIndex = GetPropertyOwner(tileId);
            if (ownerIndex == -1)
            {
                Debug.LogWarning($"[PropertyManager] Property {tileId} not owned!");
                return false;
            }
            
            int currentLevel = GetPropertyLevel(tileId);
            
            // Calculate upgrade cost
            int totalCost = CalculateUpgradeCost(basePrice, currentLevel, targetLevel);
            
            // Check money
            if (player.Money < totalCost)
            {
                Debug.LogWarning($"[PropertyManager] Player cannot afford upgrade");
                return false;
            }
            
            // Upgrade
            player.SubtractMoney(totalCost);
            propertyLevels[tileId] = targetLevel;
            
            // Check Agility: Nhân đôi tiền thuê
            if (StatsCalculator.CheckAgilityForDoubleRent(player.Agility))
            {
                propertyRentMultipliers[tileId] = 2f;
                Debug.Log($"[PropertyManager] AGILITY! Rent x2 for property {tileId}");
            }
            
            Debug.Log($"[PropertyManager] Property {tileId} upgraded to level {targetLevel} for {totalCost}");
            return true;
        }
        
        /// <summary>
        /// Pay rent
        /// </summary>
        public void PayRent(int tileId, int basePrice, PlayerGameController tenant, PlayerGameController owner)
        {
            int level = GetPropertyLevel(tileId);
            
            // Calculate base rent
            int baseRent = CalculateRent(basePrice, level);
            
            // Apply rent multiplier (Agility effect)
            float multiplier = propertyRentMultipliers.ContainsKey(tileId) ? propertyRentMultipliers[tileId] : 1f;
            int finalRent = StatsCalculator.CalculateFinalRent(baseRent, multiplier);
            
            // Tenant pays with Resistance
            var (payToOwner, cashback, actualLoss) = StatsCalculator.CalculateRentWithResistance(finalRent, tenant.Resistance);
            
            // Owner receives with Intelligence
            var (baseReceive, bonus, totalReceived) = StatsCalculator.CalculateRentWithIntelligence(payToOwner, owner.Intelligence);
            
            // Execute transaction
            tenant.SubtractMoney(actualLoss);
            owner.AddMoney(totalReceived);
            
            Debug.Log($"[PropertyManager] Rent: {tenant.PlayerName} pays {actualLoss} (cashback: {cashback}), {owner.PlayerName} receives {totalReceived} (bonus: {bonus})");
        }
        
        /// <summary>
        /// Calculate rent
        /// </summary>
        private int CalculateRent(int basePrice, int level)
        {
            if (level < 0 || level >= rentPct.Length)
            {
                return 0;
            }
            
            return basePrice * rentPct[level] / 100;
        }
        
        /// <summary>
        /// Calculate upgrade cost
        /// </summary>
        private int CalculateUpgradeCost(int basePrice, int currentLevel, int targetLevel)
        {
            int total = 0;
            
            // Giá đất (nếu mua lần đầu)
            if (currentLevel == 0 && targetLevel > 0)
            {
                total += basePrice;
            }
            
            // Giá houses
            for (int i = currentLevel + 1; i <= targetLevel && i <= 4; i++)
            {
                total += GetHousePrice(basePrice, i);
            }
            
            // Giá hotel
            if (targetLevel == 5)
            {
                total += GetHotelPrice(basePrice);
            }
            
            return total;
        }
        
        /// <summary>
        /// Get house price by level
        /// </summary>
        private int GetHousePrice(int basePrice, int level)
        {
            if (level < 1 || level > upgradeCostPct.Length)
            {
                return 0;
            }
            
            return basePrice * upgradeCostPct[level - 1] / 100;
        }
        
        /// <summary>
        /// Get hotel price
        /// </summary>
        private int GetHotelPrice(int basePrice)
        {
            return basePrice * hotelUpgradePct / 100;
        }
        
        /// <summary>
        /// Get property info for display
        /// </summary>
        public string GetPropertyInfo(int tileId, int basePrice)
        {
            if (!IsPropertyOwned(tileId))
            {
                return $"Chưa có chủ\nGiá: {basePrice}";
            }
            
            int level = GetPropertyLevel(tileId);
            int rent = CalculateRent(basePrice, level);
            float multiplier = propertyRentMultipliers.ContainsKey(tileId) ? propertyRentMultipliers[tileId] : 1f;
            int finalRent = StatsCalculator.CalculateFinalRent(rent, multiplier);
            
            string levelText = level == 5 ? "Hotel" : level == 0 ? "Đất trống" : $"House {level}";
            
            return $"{levelText}\nThuê: {finalRent}";
        }
    }
}

