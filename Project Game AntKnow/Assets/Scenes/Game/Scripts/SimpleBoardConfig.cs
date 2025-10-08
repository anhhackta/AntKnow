using UnityEngine;

namespace AntKnow.Game
{
    /// <summary>
    /// Simple board config - Hardcoded 36 tiles
    /// </summary>
    public class SimpleBoardConfig : MonoBehaviour
    {
        public static SimpleTileData[] GetTiles()
        {
            // NOTE: Array index 0-35, nhưng tile ID là 1-36
            // Tile ID 1 = array[0], Tile ID 36 = array[35]

            return new SimpleTileData[]
            {
                // Index, Name, Type, BuyPrice, H1, H2, H3, H4, Hotel, R0, R1, R2, R3, R4, RHotel

                // Tile 1: Start (Ô góc)
                new SimpleTileData(1, "Ô Bắt Đầu", TileType.Start, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                // Zone 1: Asia (2-6, 8-9, 11-15)
                new SimpleTileData(2, "Tokyo", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
                new SimpleTileData(3, "Seoul", TileType.Property, 700, 350,450,550,650,1100, 70,175,350,525,700,1750),
                new SimpleTileData(4, "Bangkok", TileType.Property, 600, 300,400,500,600,1000, 60,150,300,450,600,1500),
                new SimpleTileData(5, "Singapore", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875),
                new SimpleTileData(6, "Manila", TileType.Property, 550, 275,375,475,575,950, 55,138,275,413,550,1375),

                // Tile 7: Event
                new SimpleTileData(7, "Ô Event", TileType.Event, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                new SimpleTileData(8, "Jakarta", TileType.Property, 600, 300,400,500,600,1000, 60,150,300,450,600,1500),
                new SimpleTileData(9, "Beijing", TileType.Property, 700, 350,450,550,650,1100, 70,175,350,525,700,1750),

                // Tile 10: Ô góc (Tai Nạn)
                new SimpleTileData(10, "Ô Tai Nạn", TileType.Jail, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                new SimpleTileData(11, "Shanghai", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875),
                new SimpleTileData(12, "Hong Kong", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
                new SimpleTileData(13, "Taipei", TileType.Property, 650, 325,425,525,625,1050, 65,163,325,488,650,1625),
                new SimpleTileData(14, "Kuala Lumpur", TileType.Property, 600, 300,400,500,600,1000, 60,150,300,450,600,1500),
                new SimpleTileData(15, "Hanoi", TileType.Property, 550, 275,375,475,575,950, 55,138,275,413,550,1375),

                // Tile 16: Event
                new SimpleTileData(16, "Ô Event", TileType.Event, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                // Zone 2: Europe (17-18, 20-24)
                new SimpleTileData(17, "Ho Chi Minh", TileType.Property, 600, 300,400,500,600,1000, 60,150,300,450,600,1500),
                new SimpleTileData(18, "London", TileType.Property, 1000, 500,600,700,800,1400, 100,250,500,750,1000,2500),

                // Tile 19: Ô góc (Tra Khảo)
                new SimpleTileData(19, "Ô Tra Khảo", TileType.Quiz, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                new SimpleTileData(20, "Paris", TileType.Property, 950, 475,575,675,775,1350, 95,238,475,713,950,2375),
                new SimpleTileData(21, "Berlin", TileType.Property, 850, 425,525,625,725,1250, 85,213,425,638,850,2125),
                new SimpleTileData(22, "Rome", TileType.Property, 900, 450,550,650,750,1300, 90,225,450,675,900,2250),
                new SimpleTileData(23, "Madrid", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
                new SimpleTileData(24, "Amsterdam", TileType.Property, 850, 425,525,625,725,1250, 85,213,425,638,850,2125),

                // Tile 25: Event
                new SimpleTileData(25, "Ô Event", TileType.Event, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                // Zone 3: Americas (26-27, 29-32)
                new SimpleTileData(26, "Vienna", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
                new SimpleTileData(27, "New York", TileType.Property, 950, 475,575,675,775,1350, 95,238,475,713,950,2375),

                // Tile 28: Ô góc (Du Lịch)
                new SimpleTileData(28, "Ô Du Lịch", TileType.Travel, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                new SimpleTileData(29, "Los Angeles", TileType.Property, 900, 450,550,650,750,1300, 90,225,450,675,900,2250),
                new SimpleTileData(30, "Chicago", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
                new SimpleTileData(31, "Toronto", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875),
                new SimpleTileData(32, "Mexico City", TileType.Property, 700, 350,450,550,650,1100, 70,175,350,525,700,1750),

                // Tile 33: Event
                new SimpleTileData(33, "Ô Event", TileType.Event, 0, 0,0,0,0,0, 0,0,0,0,0,0),

                // Zone 4: Oceania (34-36)
                new SimpleTileData(34, "São Paulo", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875),
                new SimpleTileData(35, "Sydney", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
                new SimpleTileData(36, "Da Nang", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875)
            };
        }
    }
    
    /// <summary>
    /// Simple tile data
    /// </summary>
    [System.Serializable]
    public class SimpleTileData
    {
        public int index;
        public string name;
        public TileType type;
        public int basePrice;

        // Upgrade costs
        public int house1Cost;
        public int house2Cost;
        public int house3Cost;
        public int house4Cost;
        public int hotelCost;

        // Rent values
        public int rent0;  // Empty land
        public int rent1;  // 1 house
        public int rent2;  // 2 houses
        public int rent3;  // 3 houses
        public int rent4;  // 4 houses
        public int rentHotel;

        public SimpleTileData(int index, string name, TileType type, int basePrice)
        {
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
            int rent0, int rent1, int rent2, int rent3, int rent4, int rentHotel)
        {
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
        public int GetUpgradeCost(int fromLevel, int toLevel)
        {
            int totalCost = 0;

            for (int level = fromLevel + 1; level <= toLevel; level++)
            {
                switch (level)
                {
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
        public int GetRent(int level)
        {
            switch (level)
            {
                case 0: return rent0;
                case 1: return rent1;
                case 2: return rent2;
                case 3: return rent3;
                case 4: return rent4;
                case 5: return rentHotel;
                default: return 0;
            }
        }
    }
}

