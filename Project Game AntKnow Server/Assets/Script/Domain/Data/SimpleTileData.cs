using System;

/// <summary>
/// Simple tile data - Matches client SimpleTileData
/// Contains specific prices for each tile (not percentage-based)
/// </summary>
[Serializable]
public class SimpleTileData {
    public int index;           // Tile ID (1-36)
    public string name;         // Tile name
    public TileType type;       // Tile type
    public int basePrice;       // Buy price

    // Upgrade costs (specific for each tile)
    public int house1Cost;
    public int house2Cost;
    public int house3Cost;
    public int house4Cost;
    public int hotelCost;

    // Rent values (specific for each tile)
    public int rent0;           // Empty land
    public int rent1;           // 1 house
    public int rent2;           // 2 houses
    public int rent3;           // 3 houses
    public int rent4;           // 4 houses
    public int rentHotel;       // Hotel

    public SimpleTileData(int index, string name, TileType type, int basePrice) {
        this.index = index;
        this.name = name;
        this.type = type;
        this.basePrice = basePrice;

        // Default upgrade costs (50% of base price each)
        this.house1Cost = basePrice / 2;
        this.house2Cost = basePrice / 2;
        this.house3Cost = basePrice / 2;
        this.house4Cost = basePrice / 2;
        this.hotelCost = basePrice;

        // Default rent values (10%, 25%, 50%, 75%, 100%, 250%)
        this.rent0 = basePrice / 10;
        this.rent1 = basePrice / 4;
        this.rent2 = basePrice / 2;
        this.rent3 = basePrice * 3 / 4;
        this.rent4 = basePrice;
        this.rentHotel = basePrice * 5 / 2;
    }

    public SimpleTileData(int index, string name, TileType type, int basePrice,
        int house1, int house2, int house3, int house4, int hotel,
        int rent0, int rent1, int rent2, int rent3, int rent4, int rentHotel) {
        this.index = index;
        this.name = name;
        this.type = type;
        this.basePrice = basePrice;

        this.house1Cost = house1;
        this.house2Cost = house2;
        this.house3Cost = house3;
        this.house4Cost = house4;
        this.hotelCost = hotel;

        this.rent0 = rent0;
        this.rent1 = rent1;
        this.rent2 = rent2;
        this.rent3 = rent3;
        this.rent4 = rent4;
        this.rentHotel = rentHotel;
    }

    /// <summary>
    /// Get upgrade cost for specific level
    /// </summary>
    public int GetUpgradeCost(int fromLevel, int toLevel) {
        int totalCost = 0;

        for (int level = fromLevel + 1; level <= toLevel; level++) {
            switch (level) {
                case 1: totalCost += house1Cost; break;
                case 2: totalCost += house2Cost; break;
                case 3: totalCost += house3Cost; break;
                case 4: totalCost += house4Cost; break;
                case 5: totalCost += hotelCost; break;
            }
        }

        return totalCost;
    }

    /// <summary>
    /// Get rent for specific level
    /// </summary>
    public int GetRent(int level) {
        switch (level) {
            case 0: return rent0;
            case 1: return rent1;
            case 2: return rent2;
            case 3: return rent3;
            case 4: return rent4;
            case 5: return rentHotel;
            default: return 0;
        }
    }

    /// <summary>
    /// Get total purchase cost (base + all upgrades up to level)
    /// </summary>
    public int GetTotalPurchaseCost(int level, bool hasHotel) {
        int total = basePrice; // Land

        // Add upgrade costs
        if (level >= 1) total += house1Cost;
        if (level >= 2) total += house2Cost;
        if (level >= 3) total += house3Cost;
        if (level >= 4) total += house4Cost;
        if (hasHotel) total += hotelCost;

        return total;
    }

    /// <summary>
    /// Get takeover cost (120% of total purchase cost)
    /// </summary>
    public int GetTakeoverCost(int level, bool hasHotel) {
        int totalCost = GetTotalPurchaseCost(level, hasHotel);
        return totalCost * 120 / 100;
    }

    /// <summary>
    /// Get sell price (60% of total purchase cost)
    /// </summary>
    public int GetSellPrice(int level, bool hasHotel) {
        int totalCost = GetTotalPurchaseCost(level, hasHotel);
        return totalCost * 60 / 100;
    }
}

