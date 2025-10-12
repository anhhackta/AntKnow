# 🗺️ TILE TYPES REFERENCE

**Date**: October 12, 2025

---

## 📋 36 TILES BREAKDOWN

### **Property Tiles** (28 tiles) - Có TÊN + GIÁ
Tiles có thể mua, nâng cấp houses, hotels
- Hiển thị: **Tên thành phố + Giá mua**
- Ví dụ: "Tokyo" + "$800"

### **Special Tiles** (8 tiles) - CHỈ CÓ TÊN, KHÔNG CÓ GIÁ
Tiles đặc biệt, không mua được
- Hiển thị: **Chỉ tên ô**
- TextPrice bị ẩn (`SetActive(false)`)

---

## 🎯 CHI TIẾT 36 TILES

| Tile # | Tên | Type | Giá | TextName | TextPrice |
|--------|-----|------|-----|----------|-----------|
| **1** | **Ô Bắt Đầu** | **Start** | **$0** | ✅ "Ô Bắt Đầu" | ❌ Hidden |
| 2 | Tokyo | Property | $800 | ✅ "Tokyo" | ✅ "$800" |
| 3 | Seoul | Property | $700 | ✅ "Seoul" | ✅ "$700" |
| 4 | Bangkok | Property | $600 | ✅ "Bangkok" | ✅ "$600" |
| 5 | Singapore | Property | $750 | ✅ "Singapore" | ✅ "$750" |
| 6 | Manila | Property | $550 | ✅ "Manila" | ✅ "$550" |
| **7** | **Ô Event** | **Event** | **$0** | ✅ "Ô Event" | ❌ Hidden |
| 8 | Jakarta | Property | $600 | ✅ "Jakarta" | ✅ "$600" |
| 9 | Beijing | Property | $700 | ✅ "Beijing" | ✅ "$700" |
| **10** | **Ô Tai Nạn** | **Jail** | **$0** | ✅ "Ô Tai Nạn" | ❌ Hidden |
| 11 | Shanghai | Property | $750 | ✅ "Shanghai" | ✅ "$750" |
| 12 | Hong Kong | Property | $800 | ✅ "Hong Kong" | ✅ "$800" |
| 13 | Taipei | Property | $650 | ✅ "Taipei" | ✅ "$650" |
| 14 | Kuala Lumpur | Property | $600 | ✅ "Kuala Lumpur" | ✅ "$600" |
| 15 | Hanoi | Property | $550 | ✅ "Hanoi" | ✅ "$550" |
| **16** | **Ô Event** | **Event** | **$0** | ✅ "Ô Event" | ❌ Hidden |
| 17 | Ho Chi Minh | Property | $600 | ✅ "Ho Chi Minh" | ✅ "$600" |
| 18 | London | Property | $1000 | ✅ "London" | ✅ "$1000" |
| **19** | **Ô Tra Khảo** | **Quiz** | **$0** | ✅ "Ô Tra Khảo" | ❌ Hidden |
| 20 | Paris | Property | $950 | ✅ "Paris" | ✅ "$950" |
| 21 | Berlin | Property | $850 | ✅ "Berlin" | ✅ "$850" |
| 22 | Rome | Property | $900 | ✅ "Rome" | ✅ "$900" |
| 23 | Madrid | Property | $800 | ✅ "Madrid" | ✅ "$800" |
| 24 | Amsterdam | Property | $850 | ✅ "Amsterdam" | ✅ "$850" |
| **25** | **Ô Event** | **Event** | **$0** | ✅ "Ô Event" | ❌ Hidden |
| 26 | Vienna | Property | $800 | ✅ "Vienna" | ✅ "$800" |
| 27 | New York | Property | $950 | ✅ "New York" | ✅ "$950" |
| **28** | **Ô Du Lịch** | **Travel** | **$0** | ✅ "Ô Du Lịch" | ❌ Hidden |
| 29 | Los Angeles | Property | $900 | ✅ "Los Angeles" | ✅ "$900" |
| 30 | Chicago | Property | $800 | ✅ "Chicago" | ✅ "$800" |
| 31 | Toronto | Property | $750 | ✅ "Toronto" | ✅ "$750" |
| 32 | Mexico City | Property | $700 | ✅ "Mexico City" | ✅ "$700" |
| **33** | **Ô Event** | **Event** | **$0** | ✅ "Ô Event" | ❌ Hidden |
| 34 | São Paulo | Property | $750 | ✅ "São Paulo" | ✅ "$750" |
| 35 | Sydney | Property | $800 | ✅ "Sydney" | ✅ "$800" |
| 36 | Da Nang | Property | $750 | ✅ "Da Nang" | ✅ "$750" |

---

## 🎨 VISUAL SETUP CHO MỖI TILE TYPE

### **Property Tiles** (28 tiles)
```
Tile GameObject
├── Platform (cube mỏng, có Renderer để đổi màu)
├── TextName (TextMeshPro) ← "Tokyo"
└── TextPrice (TextMeshPro) ← "$800"
```

**TextName**:
- Font Size: 24-36
- Alignment: Center
- Color: White hoặc Black (tùy background)
- Active: **TRUE** (always visible)

**TextPrice**:
- Font Size: 20-32
- Alignment: Center
- Color: Yellow/Gold (để nổi bật)
- Active: **TRUE** (visible khi chưa mua)
- **Changes to Rent Price** khi đã mua: "$80" → "$200" (khi có houses)

---

### **Special Tiles** (8 tiles)
```
Tile GameObject
├── Platform (optional, không cần đổi màu)
├── TextName (TextMeshPro) ← "Ô Bắt Đầu"
└── TextPrice (TextMeshPro) ← HIDDEN (SetActive(false))
```

**TextName**:
- Font Size: 28-40 (lớn hơn Property tiles)
- Alignment: Center
- Color: Tùy theo type:
  - **Start**: Green (success color)
  - **Event**: Orange (attention)
  - **Jail**: Red (danger)
  - **Quiz**: Blue (info)
  - **Travel**: Purple (special)
- Active: **TRUE**

**TextPrice**:
- Active: **FALSE** (ẩn hoàn toàn)

---

## 🔧 CODE BEHAVIOR

### TileVisual.cs

**SetTileInfo()** method:
```csharp
public void SetTileInfo(int index, string name, int price, TileType tileType)
{
    tileIndex = index;
    textName.text = name; // Always show name
    
    if (tileType == TileType.Property && price > 0)
    {
        textPrice.text = $"${price}";
        textPrice.gameObject.SetActive(true); // Show price
    }
    else
    {
        textPrice.text = "";
        textPrice.gameObject.SetActive(false); // Hide price
    }
}
```

**UpdatePrice()** method:
```csharp
public void UpdatePrice(int price, bool isProperty = true)
{
    if (isProperty && price > 0)
    {
        textPrice.text = $"${price}";
        textPrice.gameObject.SetActive(true);
    }
    else
    {
        textPrice.text = "";
        textPrice.gameObject.SetActive(false);
    }
}
```

---

## 🛠️ UNITY EDITOR SETUP

### Cách 1: Manual Setup (mỗi tile)
1. Select Tile GameObject
2. Add Component → TileVisual
3. Set Tile Index (0-35)
4. Assign Platform
5. Create 2 TextMeshPro children:
   - `TextName` - Always visible
   - `TextPrice` - Auto hidden cho special tiles
6. Assign to TileVisual fields

### Cách 2: Auto Setup (recommended!)
1. Select Tile GameObject
2. Add Component → **TileDataAutoSetup**
3. Click button **"Setup This Tile"**
4. Script tự động:
   - Load data từ SimpleBoardConfig
   - Set TextName text
   - Set TextPrice text (hoặc hide nếu special tile)
   - Assign references

### Cách 3: Batch Setup (all 36 tiles)
1. Tạo 1 empty GameObject
2. Add Component → TileDataAutoSetup
3. Click button **"Setup ALL Tiles in Scene"**
4. Confirm dialog
5. Script tự động setup tất cả TileVisual trong scene

---

## 📊 TILE DISTRIBUTION

### By Type:
- **Property**: 28 tiles (77.8%)
- **Event**: 4 tiles (11.1%) - Tiles: 7, 16, 25, 33
- **Start**: 1 tile (2.8%) - Tile: 1
- **Jail**: 1 tile (2.8%) - Tile: 10
- **Quiz**: 1 tile (2.8%) - Tile: 19
- **Travel**: 1 tile (2.8%) - Tile: 28

### By Zone:
- **Zone 1 (Asia)**: Tiles 2-6, 8-9, 11-15, 17 (14 properties)
- **Zone 2 (Europe)**: Tiles 18, 20-24, 26 (7 properties)
- **Zone 3 (Americas)**: Tiles 27, 29-32 (5 properties)
- **Zone 4 (Oceania + Special)**: Tiles 34-36 (3 properties)

### By Price Range:
- **$550-$600**: 9 tiles (cheapest)
- **$650-$800**: 14 tiles (mid-range)
- **$850-$1000**: 5 tiles (expensive)

---

## 🎮 GAMEPLAY LOGIC

### Property Tiles:
1. **Player lands** → Show PanelBuy
2. **Not owned** → Show "Buy Land" button + base price
3. **Owned by player** → Show upgrade buttons (House 1-4, Hotel)
4. **Owned by other** → Pay rent, no panel

### Special Tiles:
1. **Start (Tile 1)** → Nhận $200 bonus
2. **Event (7, 16, 25, 33)** → Random event (PanelEvent)
3. **Jail (Tile 10)** → Miss 1 turn
4. **Quiz (Tile 19)** → Answer question (PanelQuiz), correct = reward
5. **Travel (Tile 28)** → Teleport to random tile

---

## ✅ CHECKLIST

### Setup Verification:
- [ ] All 28 Property tiles show Name + Price
- [ ] All 8 Special tiles show only Name (Price hidden)
- [ ] TextPrice active state correct for each tile type
- [ ] TileVisual.tileIndex matches position (0-35)
- [ ] Platform assigned for all Property tiles (for house spawning)
- [ ] TextMeshPro components use correct font size
- [ ] Colors appropriate for tile types

### Code Verification:
- [ ] TileVisual.SetTileInfo() checks TileType
- [ ] TileVisual.UpdatePrice() has isProperty parameter
- [ ] PropertyVisual calls UpdatePrice(price, true)
- [ ] TileDataAutoSetup hides TextPrice for special tiles
- [ ] SimpleBoardConfig data matches this reference

---

## 🚨 DEBUGGING

### TextPrice shows "$0" on special tiles?
**Fix**: Update code to check `TileType`, not just `price > 0`

### TextPrice still visible on Event tiles?
**Fix**: Use `textPrice.gameObject.SetActive(false)` instead of `textPrice.text = ""`

### Auto setup không tìm thấy TextMeshPro?
**Fix**: Check object names contain "Name" or "Price"

### Price format sai ("800" thay vì "$800")?
**Fix**: Use `$"${price}"` format in all SetTileInfo/UpdatePrice calls

---

**Bây giờ bạn có thể setup 36 tiles chính xác! 🎯**
