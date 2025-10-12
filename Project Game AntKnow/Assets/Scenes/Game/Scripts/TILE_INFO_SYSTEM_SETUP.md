# 🎯 HƯỚNG DẪN SETUP HỆ THỐNG TILE INFO

**Ngày:** 2025-10-12

---

## 📋 **TỔNG QUAN**

Hệ thống Tile Info cho phép user click vào bất kỳ tile nào trên board để xem thông tin chi tiết:
- Tên địa danh (Tokyo, Paris, New York, ...)
- Hình ảnh địa danh (Tokyo Tower, Eiffel Tower, ...)
- Bảng giá mua/thuê theo từng level (0-5)
- Trạng thái hiện tại (chủ, level, tiền thuê)

---

## 🗂️ **CẤU TRÚC FILES**

### **1. Data Layer:**
- `TileData.cs` - ScriptableObject chứa thông tin từng tile
- `TileData_Tokyo.asset` - Asset instance cho tile Tokyo (ví dụ)

### **2. Input Layer:**
- `TileClickDetector.cs` - Detect tile clicks bằng raycast

### **3. UI Layer:**
- `PanelTileInfo.cs` - Panel hiển thị thông tin tile

### **4. Visual Layer:**
- `TileVisual.cs` - Đã thêm property `TileIndex` để access từ TileClickDetector

---

## 🔧 **SETUP STEPS**

### **BƯỚC 1: Tạo TileData Assets (10 phút)**

#### **1.1. Tạo folder:**
```
Project → Assets → Data → TileData
```

#### **1.2. Tạo TileData asset cho từng tile:**
```
1. Right-click trong folder TileData
2. Create → AntKnow → Tile Data
3. Đặt tên: "TileData_Tokyo" (hoặc tên tile tương ứng)
4. Repeat cho tất cả 36 tiles
```

#### **1.3. Cấu hình TileData asset:**

**Ví dụ: TileData_Tokyo**
```
Inspector:
  Location Name: "Tokyo"
  Location Image: [Drag sprite Tokyo Tower vào đây]
  Description: "Thủ đô của Nhật Bản, nổi tiếng với Tokyo Tower"
  
  Land Price: 500
  
  Upgrade Prices (Size: 5):
    Element 0: 250  (Level 1 - 1 nhà)
    Element 1: 250  (Level 2 - 2 nhà)
    Element 2: 250  (Level 3 - 3 nhà)
    Element 3: 250  (Level 4 - 4 nhà)
    Element 4: 500  (Level 5 - Hotel)
  
  Rent Prices (Size: 6):
    Element 0: 50   (Level 0 - Đất trống)
    Element 1: 100  (Level 1 - 1 nhà)
    Element 2: 200  (Level 2 - 2 nhà)
    Element 3: 400  (Level 3 - 3 nhà)
    Element 4: 800  (Level 4 - 4 nhà)
    Element 5: 1600 (Level 5 - Hotel)
```

**Lặp lại cho tất cả tiles:**
- Tile 0: Start (không cần TileData)
- Tile 1-6: Properties
- Tile 7: Event (không cần TileData)
- Tile 8-15: Properties
- Tile 16: Event (không cần TileData)
- ... và tiếp tục

---

### **BƯỚC 2: Import Hình Ảnh Địa Danh (15 phút)**

#### **2.1. Chuẩn bị hình ảnh:**
```
Tìm hình ảnh cho từng địa danh:
- Tokyo: Tokyo Tower
- Paris: Eiffel Tower
- New York: Statue of Liberty
- London: Big Ben
- ... (tổng cộng ~26 địa danh)

Kích thước khuyến nghị: 512x512 hoặc 1024x1024
Format: PNG (có alpha channel)
```

#### **2.2. Import vào Unity:**
```
1. Project → Assets → Sprites → Locations
2. Drag all images vào folder
3. Select all images → Inspector:
   - Texture Type: Sprite (2D and UI)
   - Max Size: 1024
   - Apply
```

#### **2.3. Assign vào TileData:**
```
1. Select TileData_Tokyo asset
2. Inspector → Location Image
3. Drag sprite Tokyo Tower vào field
4. Repeat cho tất cả TileData assets
```

---

### **BƯỚC 3: Setup UI Panel (20 phút)**

#### **3.1. Tạo PanelTileInfo GameObject:**
```
1. Hierarchy → Canvas (hoặc tạo mới nếu chưa có)
2. Right-click Canvas → UI → Panel
3. Rename: "PanelTileInfo"
4. RectTransform:
   - Anchor: Center
   - Width: 400
   - Height: 600
   - Position: (0, 0, 0)
```

#### **3.2. Tạo UI elements con:**

**Background:**
```
PanelTileInfo → Image component:
  - Color: White (hoặc màu nền khác)
  - Alpha: 0.95 (hơi trong suốt)
```

**Image Location:**
```
1. Right-click PanelTileInfo → UI → Image
2. Rename: "ImageLocation"
3. RectTransform:
   - Anchor: Top
   - Width: 360
   - Height: 200
   - Pos Y: -20
4. Image component:
   - Preserve Aspect: True
```

**Text Location Name:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextLocationName"
3. RectTransform:
   - Anchor: Top
   - Width: 360
   - Height: 50
   - Pos Y: -240
4. TextMeshProUGUI:
   - Font Size: 32
   - Alignment: Center
   - Font Style: Bold
```

**Text Description:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextDescription"
3. RectTransform:
   - Anchor: Top
   - Width: 360
   - Height: 60
   - Pos Y: -310
4. TextMeshProUGUI:
   - Font Size: 16
   - Alignment: Center
   - Wrapping: Enabled
```

**Text Price Table:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextPriceTable"
3. RectTransform:
   - Anchor: Top
   - Width: 360
   - Height: 180
   - Pos Y: -470
4. TextMeshProUGUI:
   - Font Size: 14
   - Alignment: Left
   - Wrapping: Enabled
```

**Text Current Status:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextCurrentStatus"
3. RectTransform:
   - Anchor: Bottom
   - Width: 360
   - Height: 80
   - Pos Y: 60
4. TextMeshProUGUI:
   - Font Size: 14
   - Alignment: Left
```

**Button Close:**
```
1. Right-click PanelTileInfo → UI → Button - TextMeshPro
2. Rename: "ButtonClose"
3. RectTransform:
   - Anchor: Bottom
   - Width: 100
   - Height: 40
   - Pos Y: 10
4. Text: "Đóng"
```

#### **3.3. Add PanelTileInfo component:**
```
1. Select PanelTileInfo GameObject
2. Inspector → Add Component → PanelTileInfo
3. Assign references:
   - Image Location: Drag ImageLocation
   - Text Location Name: Drag TextLocationName
   - Text Description: Drag TextDescription
   - Text Price Table: Drag TextPriceTable
   - Text Current Status: Drag TextCurrentStatus
   - Btn Close: Drag ButtonClose
   
4. Tile Data Array:
   - Size: 36 (số lượng tiles)
   - Element 0: None (Start tile)
   - Element 1: TileData_Tokyo
   - Element 2: TileData_Paris
   - ... (assign tất cả TileData assets)
   
5. References:
   - Property Manager: Drag PropertyManager GameObject
   - Board Manager: Drag BoardManager GameObject
   - Game Manager: Drag GameManager GameObject
   
6. Settings:
   - Close On Outside Click: True
```

#### **3.4. Set panel inactive:**
```
1. Select PanelTileInfo GameObject
2. Inspector → Uncheck checkbox bên cạnh tên (deactivate)
```

---

### **BƯỚC 4: Setup Tile Click Detection (15 phút)**

#### **4.1. Add Colliders to Tiles:**
```
1. Hierarchy → Tiles → Tile_0
2. Select Platform child
3. Inspector → Add Component → Box Collider
4. Box Collider:
   - Center: (0, 0, 0)
   - Size: (1, 0.1, 1) (hoặc adjust theo platform size)
   
5. Repeat cho TẤT CẢ tiles (Tile_0 đến Tile_35)
```

**⚠️ TIP:** Sử dụng script để auto-add colliders:
```csharp
// Editor script (tạo file AddCollidersToTiles.cs trong folder Editor)
[MenuItem("Tools/Add Colliders to All Tiles")]
static void AddCollidersToAllTiles()
{
    TileVisual[] tiles = FindObjectsOfType<TileVisual>();
    foreach (var tile in tiles)
    {
        Transform platform = tile.transform.Find("Platform");
        if (platform != null && platform.GetComponent<Collider>() == null)
        {
            BoxCollider collider = platform.gameObject.AddComponent<BoxCollider>();
            Debug.Log($"Added collider to {tile.name}");
        }
    }
}
```

#### **4.2. Create TileClickDetector GameObject:**
```
1. Hierarchy → Create Empty
2. Rename: "TileClickDetector"
3. Inspector → Add Component → TileClickDetector
4. Assign references:
   - Main Camera: Drag Main Camera
   - Panel Tile Info: Drag PanelTileInfo
   
5. Settings:
   - Tile Layer Mask: Everything (hoặc chỉ layer của tiles)
   - Max Raycast Distance: 100
   - Enable Debug Logs: True (để test)
```

---

### **BƯỚC 5: Testing (10 phút)**

#### **5.1. Test click detection:**
```
1. Play Mode
2. Click vào bất kỳ tile nào
3. Check Console:
   ✅ [TileClickDetector] Raycast hit: Platform
   ✅ [TileClickDetector] Tile clicked: Tile_5, Index: 5
   ✅ [PanelTileInfo] Showing info for tile 5: Tokyo
```

#### **5.2. Test panel display:**
```
1. Click vào tile có TileData (ví dụ: Tokyo)
2. Check panel hiển thị:
   ✅ Hình ảnh Tokyo Tower
   ✅ Tên "Tokyo"
   ✅ Mô tả
   ✅ Bảng giá (Level 0-5)
   ✅ Trạng thái "Chưa có chủ"
   
3. Mua property đó
4. Click lại vào tile
5. Check:
   ✅ Trạng thái "Chủ: Player 1"
   ✅ Level: 0 (Đất trống)
   ✅ Tiền thuê: $50
```

#### **5.3. Test close panel:**
```
1. Click button "Đóng"
   ✅ Panel đóng
   
2. Click vào tile → Panel mở
3. Click ra ngoài panel
   ✅ Panel đóng (nếu Close On Outside Click = True)
```

---

## 🎨 **CUSTOMIZATION**

### **Thay đổi màu sắc:**
```
PanelTileInfo → Image:
  - Color: Thay đổi màu nền
  
TextLocationName:
  - Color: Thay đổi màu chữ
  - Font: Thay đổi font chữ
```

### **Thay đổi layout:**
```
Adjust RectTransform của các UI elements
Thay đổi Width, Height, Position để phù hợp với design
```

### **Thêm animation:**
```
1. Window → Animation
2. Select PanelTileInfo
3. Create animation clip: "PanelTileInfo_Show"
4. Animate:
   - Scale: (0, 0, 0) → (1, 1, 1)
   - Alpha: 0 → 1
5. Trigger animation trong PanelTileInfo.ShowTileInfo()
```

---

## 🐛 **TROUBLESHOOTING**

### **Vấn đề 1: Click không detect tile**
```
Kiểm tra:
✅ Tile có Collider không?
✅ TileClickDetector có enabled không?
✅ Main Camera được assign đúng không?
✅ Layer Mask có include layer của tiles không?
```

### **Vấn đề 2: Panel không hiển thị**
```
Kiểm tra:
✅ PanelTileInfo có active trong Hierarchy không? (sau khi click)
✅ Canvas có Canvas component không?
✅ Canvas Render Mode = Screen Space - Overlay?
✅ PanelTileInfo có trong Canvas không?
```

### **Vấn đề 3: Hình ảnh không hiển thị**
```
Kiểm tra:
✅ Sprite được import đúng (Texture Type = Sprite)?
✅ Sprite được assign vào TileData.locationImage?
✅ TileData được assign vào PanelTileInfo.tileDataArray?
✅ ImageLocation GameObject có active không?
```

### **Vấn đề 4: Thông tin không đúng**
```
Kiểm tra:
✅ TileData array index đúng không? (Element 5 = Tile 5)
✅ PropertyManager, BoardManager, GameManager được assign đúng không?
✅ Giá trong TileData khớp với giá trong BoardManager không?
```

---

## 📝 **SUMMARY**

### **Files đã tạo:**
- ✅ `TileData.cs` - ScriptableObject definition
- ✅ `TileClickDetector.cs` - Click detection system
- ✅ `PanelTileInfo.cs` - UI panel controller
- ✅ `TileVisual.cs` - Thêm property TileIndex

### **Assets cần tạo:**
- ⏳ 36 TileData assets (1 cho mỗi property tile)
- ⏳ ~26 location images (sprites)

### **UI cần setup:**
- ⏳ PanelTileInfo GameObject với 6 UI elements
- ⏳ TileClickDetector GameObject

### **Colliders cần add:**
- ⏳ Box Collider cho tất cả 36 tiles

---

## 🎯 **NEXT STEPS**

```
1. Tạo TileData assets cho tất cả tiles (10 phút)
2. Import hình ảnh địa danh (15 phút)
3. Setup UI Panel (20 phút)
4. Add Colliders to Tiles (15 phút)
5. Setup TileClickDetector (5 phút)
6. Testing (10 phút)

Total time: ~75 phút (1.25 giờ)
```

---

**Hãy bắt đầu với Bước 1 và cho tôi biết nếu gặp vấn đề!** 🚀

