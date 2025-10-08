# 🗺️ Map 36 Tiles - Cấu Trúc Rõ Ràng

## 📋 36 Tiles Layout:

### Special Tiles (6 tiles):
```
Tile 0:  Ô Bắt Đầu (Start)
Tile 10: Ô Tai Nạn (Jail/Accident)
Tile 19: Ô Tra Khảo (Quiz)
Tile 28: Ô Du Lịch (Travel)
```

### Event Tiles (4 tiles):
```
Tile 7:  Ô Event
Tile 16: Ô Event
Tile 25: Ô Event
Tile 33: Ô Event
```

### Property Tiles (26 tiles - Các thành phố):
```
Tile 1:  Tokyo
Tile 2:  Seoul
Tile 3:  Bangkok
Tile 4:  Singapore
Tile 5:  Manila
Tile 6:  Jakarta

Tile 8:  Beijing
Tile 9:  Shanghai

Tile 11: Hong Kong
Tile 12: Taipei
Tile 13: Kuala Lumpur
Tile 14: Hanoi
Tile 15: Ho Chi Minh

Tile 17: London
Tile 18: Paris

Tile 20: Berlin
Tile 21: Rome
Tile 22: Madrid
Tile 23: Amsterdam
Tile 24: Vienna

Tile 26: New York
Tile 27: Los Angeles

Tile 29: Chicago
Tile 30: Toronto
Tile 31: Mexico City
Tile 32: São Paulo

Tile 34: Sydney
Tile 35: Da Nang
```

---

## 🏗️ Tile Structure:

### Special Tiles (0, 10, 19, 28):
```
GameObject: Tile_0 (Cube - GameObject chính)
└── Text: "Ô Bắt Đầu" (child)
```

### Event Tiles (7, 16, 25, 33):
```
GameObject: Tile_7 (Cube - GameObject chính)
└── Text: "Ô Event" (child)
```

### Property Tiles (26 tiles còn lại):
```
GameObject: Tile_1 (Cube - GameObject chính - ô đất)
└── Platform (child - cube mỏng dẹp - để đặt house)
    ├── Text Name: "Tokyo" (child of Platform)
    └── Text Price: "800" (child of Platform)
```

**Lưu ý:** GameObject chính LÀ Cube, không có parent!

---

## 💰 Giá Các Ô Đất:

### Zone 1 - Asia (Tiles 1-6, 8-9, 11-15): 500-800
```
Tile 1:  Tokyo - 800
Tile 2:  Seoul - 700
Tile 3:  Bangkok - 600
Tile 4:  Singapore - 750
Tile 5:  Manila - 550
Tile 6:  Jakarta - 600

Tile 8:  Beijing - 700
Tile 9:  Shanghai - 750

Tile 11: Hong Kong - 800
Tile 12: Taipei - 650
Tile 13: Kuala Lumpur - 600
Tile 14: Hanoi - 550
Tile 15: Ho Chi Minh - 600
```

### Zone 2 - Europe (Tiles 17-18, 20-24): 800-1000
```
Tile 17: London - 1000
Tile 18: Paris - 950

Tile 20: Berlin - 850
Tile 21: Rome - 900
Tile 22: Madrid - 800
Tile 23: Amsterdam - 850
Tile 24: Vienna - 800
```

### Zone 3 - Americas (Tiles 26-27, 29-32): 700-900
```
Tile 26: New York - 950
Tile 27: Los Angeles - 900

Tile 29: Chicago - 800
Tile 30: Toronto - 750
Tile 31: Mexico City - 700
Tile 32: São Paulo - 750
```

### Zone 4 - Oceania (Tiles 34-35): 700-800
```
Tile 34: Sydney - 800
Tile 35: Da Nang - 750
```

---

## 🎯 Tile Types:

```csharp
public enum TileType
{
    Start,      // Tile 0
    Property,   // 26 tiles (cities)
    Event,      // Tiles 7, 16, 25, 33
    Quiz,       // Tile 19
    Jail,       // Tile 10
    Travel      // Tile 28
}
```

---

## 📍 Waypoint Positions:

```
Waypoint 0:  Tile 0 (Start)
Waypoint 1:  Tile 1 (Tokyo)
Waypoint 2:  Tile 2 (Seoul)
...
Waypoint 35: Tile 35 (Da Nang)

Total: 36 waypoints in circular path
```

---

## 🏠 Platform Detection:

### Cách tìm Platform trong Property Tile:

```csharp
// Option 1: By name
Transform platform = tile.Find("Platform");

// Option 2: By tag
foreach (Transform child in tile)
{
    if (child.CompareTag("Platform"))
    {
        platform = child;
        break;
    }
}

// Option 3: By scale (cube mỏng dẹp)
foreach (Transform child in tile)
{
    if (child.localScale.y < 0.5f) // Mỏng
    {
        platform = child;
        break;
    }
}
```

---

## ✅ Checklist Setup:

### Trong Unity:
- [ ] GameObject "Tiles" chứa 36 tiles con
- [ ] Tile 0, 10, 19, 28: Chỉ có Cube + Text
- [ ] Tile 7, 16, 25, 33: Chỉ có Cube + Text
- [ ] 26 tiles còn lại: Cube + Platform + Text Name + Text Price
- [ ] Platform có tag "Platform" hoặc name "Platform"
- [ ] 36 waypoints tương ứng với 36 tiles

---

**Đây là cấu trúc CHUẨN cho map 36 tiles! 🗺️**

