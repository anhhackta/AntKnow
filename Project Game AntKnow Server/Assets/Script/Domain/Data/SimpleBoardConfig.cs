using System;

/// <summary>
/// Simple board config - Hardcoded 36 tiles
/// MUST MATCH CLIENT SimpleBoardConfig.cs EXACTLY!
/// Data from MAP_36_DETAILED.csv
/// </summary>
public static class SimpleBoardConfig {
    /// <summary>
    /// Get all 36 tiles with specific prices
    /// Array index 0-35, tile ID 1-36
    /// </summary>
    public static SimpleTileData[] GetTiles() {
        // NOTE: Array index 0-35, nhưng tile ID là 1-36
        // Tile ID 1 = array[0], Tile ID 36 = array[35]

        return new SimpleTileData[] {
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
            new SimpleTileData(7, "Ô Event", TileType.Chance, 0, 0,0,0,0,0, 0,0,0,0,0,0),

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
            new SimpleTileData(16, "Ô Event", TileType.Chance, 0, 0,0,0,0,0, 0,0,0,0,0,0),

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
            new SimpleTileData(25, "Ô Event", TileType.Chance, 0, 0,0,0,0,0, 0,0,0,0,0,0),

            new SimpleTileData(26, "Vienna", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
            new SimpleTileData(27, "New York", TileType.Property, 950, 475,575,675,775,1350, 95,238,475,713,950,2375),

            // Tile 28: Ô góc (Du Lịch)
            new SimpleTileData(28, "Ô Du Lịch", TileType.Travel, 0, 0,0,0,0,0, 0,0,0,0,0,0),

            // Zone 3: Americas (29-32, 34)
            new SimpleTileData(29, "Los Angeles", TileType.Property, 900, 450,550,650,750,1300, 90,225,450,675,900,2250),
            new SimpleTileData(30, "Chicago", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
            new SimpleTileData(31, "Toronto", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875),
            new SimpleTileData(32, "Mexico City", TileType.Property, 700, 350,450,550,650,1100, 70,175,350,525,700,1750),

            // Tile 33: Event
            new SimpleTileData(33, "Ô Event", TileType.Chance, 0, 0,0,0,0,0, 0,0,0,0,0,0),

            // Zone 4: Oceania (34-36)
            new SimpleTileData(34, "São Paulo", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875),
            new SimpleTileData(35, "Sydney", TileType.Property, 800, 400,500,600,700,1200, 80,200,400,600,800,2000),
            new SimpleTileData(36, "Da Nang", TileType.Property, 750, 375,475,575,675,1150, 75,188,375,563,750,1875)
        };
    }

    /// <summary>
    /// Get tile data by tile ID (1-36)
    /// </summary>
    public static SimpleTileData GetTile(int tileId) {
        var tiles = GetTiles();
        if (tileId < 1 || tileId > tiles.Length) {
            return null;
        }
        return tiles[tileId - 1]; // Array index = tileId - 1
    }

    /// <summary>
    /// Get tile data by waypoint index (0-35)
    /// </summary>
    public static SimpleTileData GetTileByWaypointIndex(int waypointIndex) {
        var tiles = GetTiles();
        if (waypointIndex < 0 || waypointIndex >= tiles.Length) {
            return null;
        }
        return tiles[waypointIndex];
    }
}

