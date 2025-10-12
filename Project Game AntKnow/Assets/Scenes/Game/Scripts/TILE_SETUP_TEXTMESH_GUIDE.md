# 🗺️ TILE SETUP GUIDE - TEXTMESH VERSION

**Date**: October 12, 2025  
**Important**: Dùng **TextMesh** (không phải TextMeshPro)

---

## 📋 QUY TẮC SETUP TILES

### ✅ Property Tiles (28 tiles) - CẦN 2 TEXT
```
Tile GameObject (e.g., "Tile_Tokyo")
├── Platform (cube mỏng để spawn houses)
├── TextName (TextMesh) ← "Tokyo"
└── TextPrice (TextMesh) ← "$800"
```

**Ví dụ**: Tokyo, Seoul, Bangkok, Singapore, Manila, Jakarta...

### ✅ Special Tiles (8 tiles) - CHỈ CẦN 1 TEXT
```
Tile GameObject (e.g., "Tile_Start")
├── Platform (optional, không spawn houses)
└── TextName (TextMesh) ← "Ô Bắt Đầu"
    (KHÔNG CẦN TextPrice!)
```

**Ví dụ**: 
- Tile 1: Ô Bắt Đầu
- Tile 7, 16, 25, 33: Ô Event
- Tile 10: Ô Tai Nạn
- Tile 19: Ô Tra Khảo
- Tile 28: Ô Du Lịch

---

## 🎯 CÁCH TẠO TEXT TRONG UNITY

### Tạo TextMesh (NOT TextMeshPro!):

1. **Right-click Tile GameObject** → **3D Object → 3D Text**
2. **Rename** to `TextName` hoặc `TextPrice`
3. **Inspector**:
   - Text: "Tokyo" (hoặc "$800")
   - Font Size: 24 (Name) hoặc 20 (Price)
   - Anchor: Middle Center
   - Alignment: Center
   - Color: White (Name) hoặc Yellow (Price)

4. **Position**:
   - TextName: Phía trên tile, dễ nhìn
   - TextPrice: Phía dưới TextName

---

## 🛠️ SETUP 36 TILES

### Option 1: Manual Setup (Chi tiết từng tile)

#### **Property Tiles** (28 tiles):
```
1. Select Tile GameObject
2. Add Component → TileVisual
3. Set Tile Index (0-35)
4. Right-click tile → 3D Object → 3D Text
   - Rename to "TextName"
   - Text = tile name (e.g., "Tokyo")
5. Right-click tile → 3D Object → 3D Text
   - Rename to "TextPrice"  
   - Text = price (e.g., "$800")
6. Assign Platform to TileVisual
7. Drag TextName → TileVisual.textName
8. Drag TextPrice → TileVisual.textPrice
```

#### **Special Tiles** (8 tiles):
```
1. Select Tile GameObject
2. Add Component → TileVisual
3. Set Tile Index (0, 6, 9, 15, 18, 24, 27, 32)
4. Right-click tile → 3D Object → 3D Text
   - Rename to "TextName"
   - Text = tile name (e.g., "Ô Bắt Đầu")
5. (SKIP TextPrice - không cần!)
6. Assign Platform (optional)
7. Drag TextName → TileVisual.textName
8. Leave textPrice empty (null is OK)
```

---

### Option 2: Auto Setup Tool (Recommended!)

**Điều kiện**: Tiles phải có TextName (và TextPrice cho Property tiles) đã tạo sẵn

```
1. Select Tile GameObject
2. Add Component → TileDataAutoSetup
3. Click "Setup This Tile" button
4. Tool tự động:
   ✓ Load data từ SimpleBoardConfig
   ✓ Set TextName text
   ✓ Set TextPrice text (nếu Property tile)
   ✓ Hide TextPrice (nếu Special tile)
   ✓ Assign references
```

**Batch Setup (All 36 tiles)**:
```
1. Create empty GameObject
2. Add Component → TileDataAutoSetup
3. Click "Setup ALL Tiles in Scene"
4. Confirm dialog
5. All 36 tiles configured! 🎉
```

**Note**: Tool tự động tìm TextMesh với tên chứa:
- "Name" hoặc "name" → TextName
- "Price" hoặc "price" hoặc "gia" → TextPrice

---

## 📊 36 TILES CHECKLIST

### Property Tiles (28) - Cần TextName + TextPrice:
- [ ] Tile 2: Tokyo - $800
- [ ] Tile 3: Seoul - $700
- [ ] Tile 4: Bangkok - $600
- [ ] Tile 5: Singapore - $750
- [ ] Tile 6: Manila - $550
- [ ] Tile 8: Jakarta - $600
- [ ] Tile 9: Beijing - $700
- [ ] Tile 11: Shanghai - $750
- [ ] Tile 12: Hong Kong - $800
- [ ] Tile 13: Taipei - $650
- [ ] Tile 14: Kuala Lumpur - $600
- [ ] Tile 15: Hanoi - $550
- [ ] Tile 17: Ho Chi Minh - $600
- [ ] Tile 18: London - $1000
- [ ] Tile 20: Paris - $950
- [ ] Tile 21: Berlin - $850
- [ ] Tile 22: Rome - $900
- [ ] Tile 23: Madrid - $800
- [ ] Tile 24: Amsterdam - $850
- [ ] Tile 26: Vienna - $800
- [ ] Tile 27: New York - $950
- [ ] Tile 29: Los Angeles - $900
- [ ] Tile 30: Chicago - $800
- [ ] Tile 31: Toronto - $750
- [ ] Tile 32: Mexico City - $700
- [ ] Tile 34: São Paulo - $750
- [ ] Tile 35: Sydney - $800
- [ ] Tile 36: Da Nang - $750

### Special Tiles (8) - Chỉ cần TextName:
- [ ] Tile 1: Ô Bắt Đầu (Start)
- [ ] Tile 7: Ô Event (Event)
- [ ] Tile 10: Ô Tai Nạn (Jail)
- [ ] Tile 16: Ô Event (Event)
- [ ] Tile 19: Ô Tra Khảo (Quiz)
- [ ] Tile 25: Ô Event (Event)
- [ ] Tile 28: Ô Du Lịch (Travel)
- [ ] Tile 33: Ô Event (Event)

---

## 🎨 TEXT STYLING RECOMMENDATIONS

### TextName (TextMesh):
- **Font Size**: 28-36 (Property), 32-40 (Special)
- **Color**: 
  - Property tiles: White `#FFFFFF`
  - Special tiles: Colored by type
    - Start: Green `#00FF00`
    - Event: Orange `#FFA500`
    - Jail: Red `#FF0000`
    - Quiz: Blue `#0080FF`
    - Travel: Purple `#8000FF`
- **Character Size**: 0.1 - 0.2
- **Anchor**: Middle Center
- **Alignment**: Center
- **Rich Text**: Enabled (for bold/color)

### TextPrice (TextMesh):
- **Font Size**: 20-28
- **Color**: Yellow `#FFFF00` (stand out!)
- **Character Size**: 0.08 - 0.15
- **Anchor**: Middle Center
- **Alignment**: Center
- **Rich Text**: Enabled

---

## 🔧 CODE BEHAVIOR

### TileVisual.cs

**Fields**:
```csharp
[SerializeField] private TextMesh textName; // Required for all tiles
[SerializeField] private TextMesh textPrice; // Optional (null for Special tiles)
```

**SetTileInfo()**:
```csharp
public void SetTileInfo(int index, string name, int price, TileType tileType)
{
    tileIndex = index;
    textName.text = name; // Always set name
    
    if (textPrice != null) // Check if exists
    {
        if (tileType == TileType.Property && price > 0)
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
    // If textPrice is null (Special tiles), that's OK!
}
```

**UpdatePrice()**:
```csharp
public void UpdatePrice(int price, bool isProperty = true)
{
    if (textPrice != null) // Check if exists
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
}
```

**Key Points**:
- TextPrice có thể là `null` → Code không crash
- Chỉ Property tiles cần TextPrice
- Special tiles: textPrice = null → OK!

---

## ✅ VERIFICATION

### Property Tile (e.g., Tokyo - Tile 2):
```
Tile_Tokyo
├── Platform ✓
├── TextName ✓ (text = "Tokyo")
└── TextPrice ✓ (text = "$800")

TileVisual component:
- Tile Index: 1 (0-based, so Tile 2 = index 1)
- Platform: Assigned ✓
- Text Name: Assigned ✓
- Text Price: Assigned ✓
```

### Special Tile (e.g., Ô Bắt Đầu - Tile 1):
```
Tile_Start
├── Platform (optional)
└── TextName ✓ (text = "Ô Bắt Đầu")
    (NO TextPrice child!)

TileVisual component:
- Tile Index: 0
- Platform: Optional
- Text Name: Assigned ✓
- Text Price: None (null) ✓ ← This is correct!
```

---

## 🚨 COMMON MISTAKES

### ❌ Mistake 1: Dùng TextMeshPro thay vì TextMesh
**Fix**: Right-click → **3D Object → 3D Text** (NOT UI → Text - TextMeshPro)

### ❌ Mistake 2: Tạo TextPrice cho Special tiles
**Fix**: Special tiles KHÔNG CẦN TextPrice. Bỏ qua bước tạo TextPrice.

### ❌ Mistake 3: TextPrice = null gây lỗi
**Fix**: Code đã handle `if (textPrice != null)` → Không crash nếu null

### ❌ Mistake 4: TextName có text "$0" cho Special tiles
**Fix**: TextName chỉ chứa tên tile (e.g., "Ô Event"), không có giá

### ❌ Mistake 5: TileIndex sai (1-36 thay vì 0-35)
**Fix**: Unity index 0-35, Tile ID trong code 1-36
- Tile 1 "Ô Bắt Đầu" = index 0
- Tile 36 "Da Nang" = index 35

---

## 🎮 TESTING

### Test Property Tile:
1. Play Mode
2. Move player to Property tile (e.g., Tokyo)
3. **Expected**:
   - TextName shows "Tokyo" ✓
   - TextPrice shows "$800" ✓
4. Buy property
5. **Expected**:
   - TextPrice updates to rent (e.g., "$80") ✓
6. Add houses
7. **Expected**:
   - TextPrice updates to new rent ✓

### Test Special Tile:
1. Play Mode
2. Move player to Special tile (e.g., Ô Event)
3. **Expected**:
   - TextName shows "Ô Event" ✓
   - TextPrice is NOT visible ✓ (or doesn't exist)
4. Trigger event
5. **Expected**:
   - PanelEvent opens ✓

---

## 📝 SUMMARY

### Property Tiles (28):
- ✅ Cần 2 TextMesh: TextName + TextPrice
- ✅ TextName: Tên thành phố
- ✅ TextPrice: Giá mua ($800, $700, etc.)

### Special Tiles (8):
- ✅ Chỉ cần 1 TextMesh: TextName
- ✅ TextName: Tên ô đặc biệt (Ô Bắt Đầu, Ô Event, etc.)
- ❌ KHÔNG CẦN TextPrice

### Code:
- ✅ TileVisual.cs sử dụng TextMesh (not TextMeshPro)
- ✅ textPrice có thể null (không crash)
- ✅ Auto Setup Tool tự động handle Property vs Special

---

**Giờ bạn có thể setup tiles chính xác! 🚀**

**Workflow**:
1. Tạo TextName cho tất cả 36 tiles (3D Text)
2. Tạo TextPrice chỉ cho 28 Property tiles
3. Add TileVisual component
4. Set Tile Index
5. Assign references
6. (Optional) Dùng TileDataAutoSetup tool để auto-set text content
