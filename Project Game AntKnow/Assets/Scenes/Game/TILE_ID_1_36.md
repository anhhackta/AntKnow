# ✅ Tile ID 1-36 - Đã Sửa Lại

## 🎯 Đã Fix:

### Trước (SAI):
```
Tile ID: 0-35
Ô góc: 0, 10, 19, 28
Ô Event: 7, 16, 25, 33
```

### Bây Giờ (ĐÚNG):
```
Tile ID: 1-36
Ô góc: 1, 10, 19, 28
Ô Event: 7, 16, 25, 33
```

---

## 🗺️ Map 36 Tiles (Tile ID 1-36):

### Ô Góc (4 tiles):
```
Tile 1:  Ô Bắt Đầu (Start)
Tile 10: Ô Tai Nạn (Jail)
Tile 19: Ô Tra Khảo (Quiz)
Tile 28: Ô Du Lịch (Travel)
```

### Ô Event (4 tiles):
```
Tile 7:  Ô Event
Tile 16: Ô Event
Tile 25: Ô Event
Tile 33: Ô Event
```

### Ô Property (28 tiles):
```
Zone 1 - Asia (Tiles 2-6, 8-9, 11-15, 17):
Tile 2:  Tokyo
Tile 3:  Seoul
Tile 4:  Bangkok
Tile 5:  Singapore
Tile 6:  Manila
Tile 8:  Jakarta
Tile 9:  Beijing
Tile 11: Shanghai
Tile 12: Hong Kong
Tile 13: Taipei
Tile 14: Kuala Lumpur
Tile 15: Hanoi
Tile 17: Ho Chi Minh

Zone 2 - Europe (Tiles 18, 20-24, 26):
Tile 18: London
Tile 20: Paris
Tile 21: Berlin
Tile 22: Rome
Tile 23: Madrid
Tile 24: Amsterdam
Tile 26: Vienna

Zone 3 - Americas (Tiles 27, 29-32, 34):
Tile 27: New York
Tile 29: Los Angeles
Tile 30: Chicago
Tile 31: Toronto
Tile 32: Mexico City
Tile 34: São Paulo

Zone 4 - Oceania (Tiles 35-36):
Tile 35: Sydney
Tile 36: Da Nang
```

---

## 🔧 Code Changes:

### SimpleBoardConfig.cs:
```csharp
// Array index 0-35, nhưng tile ID là 1-36
// Tile ID 1 = array[0], Tile ID 36 = array[35]

return new SimpleTileData[]
{
    // Tile 1: Start (Ô góc)
    new SimpleTileData(1, "Ô Bắt Đầu", TileType.Start, 0, ...),
    
    // Tile 2-6: Asia
    new SimpleTileData(2, "Tokyo", TileType.Property, 800, ...),
    new SimpleTileData(3, "Seoul", TileType.Property, 700, ...),
    ...
    
    // Tile 7: Event
    new SimpleTileData(7, "Ô Event", TileType.Event, 0, ...),
    
    ...
    
    // Tile 36: Da Nang
    new SimpleTileData(36, "Da Nang", TileType.Property, 750, ...)
};
```

### BoardManager.cs:
```csharp
// Convert waypoint index (0-35) to tile ID (1-36)
private int WaypointIndexToTileId(int waypointIndex)
{
    return waypointIndex + 1;
}

// Convert tile ID (1-36) to waypoint index (0-35)
private int TileIdToWaypointIndex(int tileId)
{
    return tileId - 1;
}

// Get tile type by waypoint index (0-35)
public TileType GetTileType(int waypointIndex)
{
    int tileId = WaypointIndexToTileId(waypointIndex);
    return tileData[tileId - 1].type; // Array index = tileId - 1
}
```

---

## 📊 Mapping:

### Waypoint Index → Tile ID:
```
Waypoint 0  → Tile 1  (Ô Bắt Đầu)
Waypoint 1  → Tile 2  (Tokyo)
Waypoint 2  → Tile 3  (Seoul)
...
Waypoint 6  → Tile 7  (Ô Event)
Waypoint 9  → Tile 10 (Ô Tai Nạn)
Waypoint 18 → Tile 19 (Ô Tra Khảo)
Waypoint 27 → Tile 28 (Ô Du Lịch)
...
Waypoint 35 → Tile 36 (Da Nang)
```

### Array Index → Tile ID:
```
array[0]  → Tile 1
array[1]  → Tile 2
array[2]  → Tile 3
...
array[35] → Tile 36
```

---

## ✅ Files Updated:

```
✅ SimpleBoardConfig.cs - Tile ID 1-36
✅ BoardManager.cs - Conversion methods
✅ MAP_36_DETAILED.csv - Index 1-36
✅ MAP_36_TILES.md - Updated structure
```

---

## 🎯 Kết Quả:

```
✅ Tile ID: 1-36 (ĐÚNG!)
✅ Ô góc: 1, 10, 19, 28
✅ Ô Event: 7, 16, 25, 33
✅ Ô Property: 28 tiles
✅ Total: 36 tiles
✅ Waypoint index: 0-35 (internal)
✅ Tile ID: 1-36 (user-facing)
```

---

## 💡 Lưu Ý:

### Trong Code:
```
- Waypoint index: 0-35 (internal, Unity array)
- Tile ID: 1-36 (game logic, user-facing)
- Array index: 0-35 (SimpleTileData array)
```

### Khi Debug:
```
Player at waypoint 0 → Tile 1 (Ô Bắt Đầu)
Player at waypoint 9 → Tile 10 (Ô Tai Nạn)
Player at waypoint 18 → Tile 19 (Ô Tra Khảo)
```

---

**Đã sửa lại đúng! Tile ID 1-36! 🎮**

