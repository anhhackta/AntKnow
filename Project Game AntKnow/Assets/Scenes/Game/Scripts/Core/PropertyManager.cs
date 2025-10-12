using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Quản lý property system (mua, nâng cấp, thuê, bán)
    /// </summary>
    public class PropertyManager : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private PropertyVisual propertyVisual;
        [SerializeField] private BoardManager boardManager;

        // Property ownership: tileId -> owner player index
        private Dictionary<int, int> propertyOwners = new Dictionary<int, int>();

        // Property levels: tileId -> level (0-5, 5 = hotel)
        private Dictionary<int, int> propertyLevels = new Dictionary<int, int>();

        // Property rent multipliers: tileId -> multiplier (1 or 2 from Agility)
        private Dictionary<int, float> propertyRentMultipliers = new Dictionary<int, float>();

        private void Awake()
        {
            if (propertyVisual == null)
            {
                propertyVisual = GetComponent<PropertyVisual>();
                if (propertyVisual == null)
                {
                    Debug.LogError("[PropertyManager] PropertyVisual component not found! House models will not display!");
                }
                else
                {
                    Debug.Log("[PropertyManager] PropertyVisual component found");
                }
            }

            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
                if (boardManager == null)
                {
                    Debug.LogError("[PropertyManager] BoardManager not found!");
                }
            }
        }
        
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
            Debug.Log($"[PropertyManager] BuyProperty called - Tile: {tileId}, Player: {playerIndex}, Price: {basePrice}");

            // Check if already owned
            if (IsPropertyOwned(tileId))
            {
                Debug.LogWarning($"[PropertyManager] Property {tileId} already owned!");
                return false;
            }

            // Check money
            if (player.Money < basePrice)
            {
                Debug.LogWarning($"[PropertyManager] Player {playerIndex} cannot afford property {tileId}. Money: {player.Money}, Price: {basePrice}");
                return false;
            }

            Debug.Log($"[PropertyManager] Checks passed, buying property {tileId}...");

            // Buy
            player.SubtractMoney(basePrice);
            propertyOwners[tileId] = playerIndex;
            propertyLevels[tileId] = 0; // Level 0 = đất trống
            propertyRentMultipliers[tileId] = 1f;

            Debug.Log($"[PropertyManager] Property ownership set. Calling UpdatePropertyVisual...");

            // Check Agility: Nhân đôi tiền thuê
            if (StatsCalculator.CheckAgilityForDoubleRent(player.Agility))
            {
                propertyRentMultipliers[tileId] = 2f;
                Debug.Log($"[PropertyManager] AGILITY! Rent x2 for property {tileId}");
            }

            // Update visual (level 0 = no visual)
            UpdatePropertyVisual(tileId);

            Debug.Log($"[PropertyManager] Player {playerIndex} bought property {tileId} for {basePrice} - COMPLETE");
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

            // Update visual
            UpdatePropertyVisual(tileId);

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
        /// Calculate rent from tile data
        /// </summary>
        private int CalculateRent(int basePrice, int level)
        {
            SimpleTileData tileData = GetTileData(basePrice);
            if (tileData == null)
            {
                return 0;
            }

            return tileData.GetRent(level);
        }

        /// <summary>
        /// Calculate upgrade cost from tile data
        /// </summary>
        private int CalculateUpgradeCost(int basePrice, int currentLevel, int targetLevel)
        {
            SimpleTileData tileData = GetTileData(basePrice);
            if (tileData == null)
            {
                return 0;
            }

            return tileData.GetUpgradeCost(currentLevel, targetLevel);
        }

        /// <summary>
        /// Get tile data by base price (temporary solution)
        /// </summary>
        private SimpleTileData GetTileData(int basePrice)
        {
            SimpleTileData[] allTiles = SimpleBoardConfig.GetTiles();

            foreach (var tile in allTiles)
            {
                if (tile.basePrice == basePrice)
                {
                    return tile;
                }
            }

            return null;
        }

        /// <summary>
        /// Get tile data by tile ID (better solution)
        /// </summary>
        private SimpleTileData GetTileDataById(int tileId)
        {
            SimpleTileData[] allTiles = SimpleBoardConfig.GetTiles();

            if (tileId >= 0 && tileId < allTiles.Length)
            {
                return allTiles[tileId];
            }

            return null;
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

        /// <summary>
        /// Update property visual
        /// </summary>
        private void UpdatePropertyVisual(int tileId)
        {
            if (propertyVisual == null)
            {
                Debug.LogError($"[PropertyManager] PropertyVisual is null! Cannot update visual for tile {tileId}");
                return;
            }

            if (boardManager == null)
            {
                Debug.LogError($"[PropertyManager] BoardManager is null! Cannot update visual for tile {tileId}");
                return;
            }

            int level = GetPropertyLevel(tileId);
            int ownerIndex = GetPropertyOwner(tileId);

            // Get rent price for display
            int basePrice = boardManager.GetTilePrice(tileId);
            int rent = CalculateRent(basePrice, level);
            float multiplier = propertyRentMultipliers.ContainsKey(tileId) ? propertyRentMultipliers[tileId] : 1f;
            int finalRent = StatsCalculator.CalculateFinalRent(rent, multiplier);

            Debug.Log($"[PropertyManager] UpdatePropertyVisual - Tile: {tileId}, Level: {level}, Owner: {ownerIndex}, Rent: {finalRent}");

            propertyVisual.UpdatePropertyVisual(tileId, level, ownerIndex, finalRent);
        }
    }
}

