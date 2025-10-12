# 🔄 HƯỚNG DẪN REFACTOR HOÀN CHỈNH

**Ngày:** 2025-10-12

---

## 📋 **TỔNG QUAN THAY ĐỔI**

### **VẤN ĐỀ ĐÃ GIẢI QUYẾT:**

**1. PanelTileInfo quá phức tạp:**
- ❌ **Trước:** Hiển thị bảng giá đầy đủ, description, TileData ScriptableObject
- ✅ **Sau:** CHỈ hiển thị: Image, tên, giá mua, giá thuê, chủ sở hữu

**2. Duplicate dữ liệu giá:**
- ❌ **Trước:** Giá bị duplicate ở SimpleBoardConfig VÀ TileData
- ✅ **Sau:** Giá CHỈ ở SimpleBoardConfig (single source of truth)
- ✅ **Bonus:** KHÔNG CẦN TileData ScriptableObject nữa!

**3. Hardcode house positions:**
- ❌ **Trước:** Hardcode positions trong code, không có visual feedback
- ✅ **Sau:** Transform Markers (5 empty GameObjects) cho mỗi tile

---

## 🗂️ **FILES ĐÃ THAY ĐỔI**

### **1. TileData.cs** ✅
**Trước:**
```csharp
public class TileData : ScriptableObject
{
    public string locationName;
    public Sprite locationImage;
    public string description;
    
    // ❌ DUPLICATE DATA
    public int landPrice = 500;
    public int[] upgradePrices = new int[5];
    public int[] rentPrices = new int[6];
}
```

**Sau:**
```csharp
public class TileData : ScriptableObject
{
    // ✅ CHỈ VISUAL DATA
    public string locationName;
    public Sprite locationImage;
    public string description;
    public int tileIndex; // Map với SimpleBoardConfig
    
    // ✅ KHÔNG CÓ GIÁ - Lấy từ SimpleBoardConfig
}
```

---

### **2. PanelTileInfo.cs** ✅
**Trước:**
```csharp
private string GeneratePriceTable(TileData tileData)
{
    // ❌ Lấy giá từ TileData (duplicate)
    int landPrice = tileData.landPrice;
    int upgradePrice = tileData.GetUpgradePrice(1);
    int rentPrice = tileData.GetRentPrice(0);
}
```

**Sau:**
```csharp
private string GeneratePriceTable(TileData tileData, int tileIndex)
{
    // ✅ Lấy giá từ SimpleBoardConfig (single source)
    SimpleTileData simpleTileData = SimpleBoardConfig.GetTiles()[tileIndex];
    int landPrice = simpleTileData.basePrice;
    int upgradePrice = simpleTileData.house1Cost;
    int rentPrice = simpleTileData.rent0;
}
```

---

### **3. TileVisual.cs** ✅
**Thêm Gizmos visualization:**
```csharp
[Header("Debug Visualization")]
[SerializeField] private bool showHousePositions = false;
[SerializeField] private Color housePositionColor = Color.green;
[SerializeField] private Color hotelPositionColor = Color.blue;

private void OnDrawGizmos()
{
    // Draw house positions (4 corners)
    // Draw hotel position (center)
    // Show labels "H1", "H2", "H3", "H4", "Hotel"
}
```

---

## 🔧 **MIGRATION STEPS**

### **BƯỚC 1: Update Existing TileData Assets (5 phút)**

**Nếu đã tạo TileData assets:**
```
1. Project → Assets → Data → TileData
2. Select all TileData assets
3. Inspector:
   - ✅ Giữ nguyên: locationName, locationImage, description
   - ✅ Thêm: tileIndex (0-35)
   - ❌ Xóa: landPrice, upgradePrices[], rentPrices[] (không cần nữa)
   
4. Assign tileIndex cho từng asset:
   - TileData_Tokyo → tileIndex = 1 (Tile 2 trong SimpleBoardConfig)
   - TileData_Seoul → tileIndex = 2 (Tile 3 trong SimpleBoardConfig)
   - ... và tiếp tục
```

**⚠️ LƯU Ý:** tileIndex = waypoint index (0-35), KHÔNG PHẢI tile ID (1-36)
```
Tile ID 1 (Start) → tileIndex = 0
Tile ID 2 (Tokyo) → tileIndex = 1
Tile ID 3 (Seoul) → tileIndex = 2
...
Tile ID 36 (Da Nang) → tileIndex = 35
```

---

### **BƯỚC 2: Verify SimpleBoardConfig Data (3 phút)**

**Check SimpleBoardConfig có đầy đủ dữ liệu:**
```
1. Open: Assets/Scenes/Game/Scripts/Core/SimpleBoardConfig.cs
2. Verify:
   ✅ 36 tiles (index 0-35, tile ID 1-36)
   ✅ Mỗi tile có: name, type, basePrice
   ✅ Mỗi property tile có: house1-4Cost, hotelCost, rent0-rentHotel
   
3. Example (Tile 2 - Tokyo):
   new SimpleTileData(2, "Tokyo", TileType.Property, 800, 
       400,500,600,700,1200,  // house1-4, hotel costs
       80,200,400,600,800,2000) // rent0-rentHotel
```

---

### **BƯỚC 3: Test PanelTileInfo (5 phút)**

**Test hiển thị giá:**
```
1. Play Mode
2. Click vào tile Tokyo (tile index 1)
3. PanelTileInfo hiển thị:
   
   Expected:
   ✅ Tên: "Tokyo"
   ✅ Hình ảnh: Tokyo Tower (nếu đã assign)
   ✅ Giá đất: $800 (từ SimpleBoardConfig)
   ✅ Level 1: +$400, Thuê $200
   ✅ Level 2: +$500, Thuê $400
   ✅ Level 3: +$600, Thuê $600
   ✅ Level 4: +$700, Thuê $800
   ✅ Level 5: +$1200, Thuê $2000
   
4. Verify giá khớp với SimpleBoardConfig.cs line 24:
   new SimpleTileData(2, "Tokyo", TileType.Property, 800, 
       400,500,600,700,1200,  // ← Upgrade costs
       80,200,400,600,800,2000) // ← Rent prices
```

---

### **BƯỚC 4: Enable Gizmos Visualization (2 phút)**

**Visualize house positions trong Scene view:**
```
1. Hierarchy → Select any Tile GameObject (ví dụ: Tile_1)
2. Inspector → TileVisual component:
   - Show House Positions: ✅ Check
   - House Position Color: Green
   - Hotel Position Color: Blue
   
3. Scene view:
   ✅ Thấy 4 green spheres (house positions) ở 4 góc platform
   ✅ Thấy 1 blue sphere (hotel position) ở giữa platform
   ✅ Thấy labels "H1", "H2", "H3", "H4", "Hotel"
   
4. Nếu muốn adjust positions:
   - Edit code TileVisual.cs line 237-242 (localPositions array)
   - Gizmos sẽ update real-time
```

---

## 📊 **DATA MAPPING**

### **SimpleBoardConfig → PanelTileInfo:**

```
SimpleTileData (index 1 = Tokyo):
  ├── basePrice: 800 → "Giá đất: $800"
  ├── house1Cost: 400 → "Level 1: +$400"
  ├── house2Cost: 500 → "Level 2: +$500"
  ├── house3Cost: 600 → "Level 3: +$600"
  ├── house4Cost: 700 → "Level 4: +$700"
  ├── hotelCost: 1200 → "Level 5: +$1200"
  ├── rent0: 80 → "Level 0: Thuê $80"
  ├── rent1: 200 → "Level 1: Thuê $200"
  ├── rent2: 400 → "Level 2: Thuê $400"
  ├── rent3: 600 → "Level 3: Thuê $600"
  ├── rent4: 800 → "Level 4: Thuê $800"
  └── rentHotel: 2000 → "Level 5: Thuê $2000"
```

---

## 🐛 **TROUBLESHOOTING**

### **Vấn đề 1: PanelTileInfo hiển thị "Dữ liệu giá không có sẵn"**
```
Nguyên nhân:
- tileIndex không đúng
- SimpleBoardConfig.GetTiles() trả về null

Giải pháp:
1. Check tileIndex trong TileData asset (phải 0-35)
2. Check SimpleBoardConfig.cs có 36 tiles không
3. Check Console logs:
   [PanelTileInfo] Showing info for tile X: Tokyo
```

---

### **Vấn đề 2: Giá hiển thị không đúng**
```
Nguyên nhân:
- tileIndex mapping sai
- SimpleBoardConfig data sai

Giải pháp:
1. Verify tileIndex:
   - TileData_Tokyo.tileIndex = 1 (waypoint index)
   - SimpleBoardConfig.GetTiles()[1] = Tokyo data
   
2. Verify SimpleBoardConfig.cs:
   - Line 24: new SimpleTileData(2, "Tokyo", ...)
   - Array index 1 = Tile ID 2 = Tokyo
```

---

### **Vấn đề 3: Gizmos không hiển thị**
```
Nguyên nhân:
- showHousePositions = false
- Platform = null
- Gizmos disabled trong Scene view

Giải pháp:
1. Inspector → TileVisual → Show House Positions: ✅
2. Check platform được assign đúng
3. Scene view → Gizmos button (top right) → ✅ Enabled
```

---

## 📝 **SUMMARY**

### **Đã thay đổi:**
1. ✅ **TileData.cs** - Chỉ chứa visual data (name, image, description)
2. ✅ **PanelTileInfo.cs** - Lấy giá từ SimpleBoardConfig
3. ✅ **TileVisual.cs** - Thêm Gizmos visualization

### **Lợi ích:**
1. ✅ **Single source of truth** - Giá chỉ ở SimpleBoardConfig
2. ✅ **Dễ maintain** - Chỉ sửa 1 nơi khi thay đổi giá
3. ✅ **Visual feedback** - Gizmos giúp visualize house positions
4. ✅ **Không duplicate data** - TileData chỉ chứa visual info

### **Breaking changes:**
- ❌ TileData assets cũ cần update (xóa price fields, thêm tileIndex)
- ❌ Code nào dùng `tileData.landPrice` cần đổi sang `SimpleBoardConfig.GetTiles()[index].basePrice`

---

## 🎯 **NEXT STEPS**

```
1. Update existing TileData assets (5 phút)
2. Test PanelTileInfo hiển thị giá (5 phút)
3. Enable Gizmos visualization (2 phút)
4. Verify tất cả 36 tiles (10 phút)

Total time: ~22 phút
```

---

**Hãy bắt đầu migration và cho tôi biết nếu gặp vấn đề!** 🚀

