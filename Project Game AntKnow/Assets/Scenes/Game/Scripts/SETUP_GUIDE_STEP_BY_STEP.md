# 📖 HƯỚNG DẪN TRIỂN KHAI CHI TIẾT

**Ngày:** 2025-10-12

---

## 🎯 **MỤC TIÊU**

Triển khai 2 hệ thống:
1. ✅ **PanelTileInfo** - Panel hiển thị thông tin tile khi click
2. ✅ **House/Hotel Markers** - 5 Transform markers cho mỗi tile

---

## 📋 **PHẦN 1: SETUP HOUSE/HOTEL MARKERS**

### **Bước 1.1: Chạy Editor Tool (2 phút)**

```
1. Unity Editor → Menu → Tools → AntKnow → Generate Tile Markers
2. Window "Tile Marker Generator" mở ra
3. Options:
   - Create for ALL tiles: ✅ (Check)
4. Click button "Generate Markers" (màu xanh, to)
5. Wait... (tool đang tạo markers cho 36 tiles)
6. Dialog "Success" xuất hiện: "Generated markers for 36 tiles!"
7. Click "OK"
```

**Kết quả:**
```
Hierarchy:
  Tile_1
    └── Platform
          └── Markers ← NEW!
                ├── HouseMarker1 (Empty GameObject)
                ├── HouseMarker2 (Empty GameObject)
                ├── HouseMarker3 (Empty GameObject)
                ├── HouseMarker4 (Empty GameObject)
                └── HotelMarker (Empty GameObject)
  Tile_2
    └── Platform
          └── Markers ← NEW!
                ├── HouseMarker1
                ├── HouseMarker2
                ├── HouseMarker3
                ├── HouseMarker4
                └── HotelMarker
  ... (36 tiles total)
```

---

### **Bước 1.2: Verify Markers (2 phút)**

```
1. Hierarchy → Select Tile_1
2. Inspector → TileVisual component:
   
   House/Hotel Markers:
     House Markers: Size = 4
       Element 0: HouseMarker1 ✅
       Element 1: HouseMarker2 ✅
       Element 2: HouseMarker3 ✅
       Element 3: HouseMarker4 ✅
     Hotel Marker: HotelMarker ✅
```

**Nếu markers KHÔNG được assign:**
```
1. Hierarchy → Tile_1 → Platform → Markers
2. Drag HouseMarker1 vào Inspector → TileVisual → House Markers → Element 0
3. Drag HouseMarker2 vào Element 1
4. Drag HouseMarker3 vào Element 2
5. Drag HouseMarker4 vào Element 3
6. Drag HotelMarker vào Hotel Marker field
```

---

### **Bước 1.3: Hiểu Logic House/Hotel (QUAN TRỌNG!)**

**⭐ YÊU CẦU CỦA BẠN:**
```
- 4 HouseMarkers: Hiển thị houses (Level 1-4)
- 1 HotelMarker: Hiển thị hotel (Level 5)
- Khi hotel xuất hiện → 4 houses biến mất
- Khi downgrade → Hotel biến mất, houses xuất hiện lại
```

**⭐ IMPLEMENTATION:**
```csharp
// TileVisual.cs - SpawnHouses()
public void SpawnHouses(GameObject housePrefab, int count, Color playerColor)
{
    ClearHouses(); // Xóa tất cả houses/hotel cũ
    
    // Spawn houses tại markers
    for (int i = 0; i < count && i < 4; i++)
    {
        Transform marker = houseMarkers[i];
        GameObject house = Instantiate(housePrefab);
        house.transform.position = marker.position;
        house.transform.rotation = marker.rotation;
        house.transform.SetParent(transform);
        spawnedHouses[i] = house;
    }
}

// TileVisual.cs - SpawnHotel()
public void SpawnHotel(GameObject hotelPrefab, Color playerColor)
{
    ClearHouses(); // ⭐ Xóa 4 houses trước khi spawn hotel
    
    // Spawn hotel tại hotel marker
    spawnedHotel = Instantiate(hotelPrefab);
    spawnedHotel.transform.position = hotelMarker.position;
    spawnedHotel.transform.rotation = hotelMarker.rotation;
    spawnedHotel.transform.SetParent(transform);
}

// TileVisual.cs - ClearHouses()
public void ClearHouses()
{
    // Xóa 4 houses
    for (int i = 0; i < 4; i++)
    {
        if (spawnedHouses[i] != null)
        {
            Destroy(spawnedHouses[i]);
            spawnedHouses[i] = null;
        }
    }
    
    // Xóa hotel
    if (spawnedHotel != null)
    {
        Destroy(spawnedHotel);
        spawnedHotel = null;
    }
}
```

**⭐ LOGIC:**
```
Level 0: ClearHouses() → Không có gì
Level 1: SpawnHouses(count=1) → 1 house tại HouseMarker1
Level 2: SpawnHouses(count=2) → 2 houses tại HouseMarker1, HouseMarker2
Level 3: SpawnHouses(count=3) → 3 houses
Level 4: SpawnHouses(count=4) → 4 houses
Level 5: SpawnHotel() → ClearHouses() trước → Spawn hotel tại HotelMarker
```

---

### **Bước 1.4: Test House Spawning (5 phút)**

```
1. Play Mode
2. Mua property (ví dụ: Tokyo)
3. Upgrade to Level 1:
   - Scene view: 1 house xuất hiện tại HouseMarker1 ✅
   - Hierarchy: Tile_1 → House(Clone) ✅
   
4. Upgrade to Level 2:
   - Scene view: 2 houses (HouseMarker1, HouseMarker2) ✅
   
5. Upgrade to Level 5 (Hotel):
   - Scene view: 4 houses biến mất ✅
   - Scene view: 1 hotel xuất hiện tại HotelMarker ✅
   - Hierarchy: Tile_1 → Hotel(Clone) ✅
```

**Nếu houses spawn sai vị trí:**
```
1. Stop Play Mode
2. Scene view → Select Tile_1
3. Hierarchy → Platform → Markers → HouseMarker1
4. Drag marker trong Scene view để adjust position
5. Repeat cho HouseMarker2, 3, 4, HotelMarker
6. Play Mode → Test lại
```

---

## 📋 **PHẦN 2: SETUP PANELTILEINFO**

### **Bước 2.1: Tạo UI Panel (10 phút)**

**2.1.1. Tạo Panel:**
```
1. Hierarchy → Canvas (nếu chưa có thì tạo: Right-click → UI → Canvas)
2. Right-click Canvas → UI → Panel
3. Rename: "PanelTileInfo"
4. Inspector → RectTransform:
   - Anchor: Center-Middle
   - Width: 400
   - Height: 500
   - Pos X: 0, Pos Y: 0
```

**2.1.2. Tạo Background:**
```
1. PanelTileInfo → Inspector → Image component:
   - Color: White (hoặc màu nền bạn thích)
   - Alpha: 200 (để hơi trong suốt)
```

**2.1.3. Tạo Image Location:**
```
1. Right-click PanelTileInfo → UI → Image
2. Rename: "ImageLocation"
3. Inspector → RectTransform:
   - Anchor: Top-Center
   - Width: 350
   - Height: 200
   - Pos X: 0, Pos Y: -20
```

**2.1.4. Tạo Text Location Name:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextLocationName"
3. Inspector → RectTransform:
   - Anchor: Top-Center
   - Width: 350
   - Height: 40
   - Pos X: 0, Pos Y: -230
4. TextMeshProUGUI component:
   - Text: "Tokyo"
   - Font Size: 28
   - Alignment: Center
   - Color: Black
   - Font Style: Bold
```

**2.1.5. Tạo Text Buy Price:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextBuyPrice"
3. Inspector → RectTransform:
   - Anchor: Top-Left
   - Width: 350
   - Height: 30
   - Pos X: 25, Pos Y: -280
4. TextMeshProUGUI component:
   - Text: "Giá mua: $800"
   - Font Size: 20
   - Alignment: Left
   - Color: Black
```

**2.1.6. Tạo Text Rent Price:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextRentPrice"
3. Inspector → RectTransform:
   - Anchor: Top-Left
   - Width: 350
   - Height: 30
   - Pos X: 25, Pos Y: -320
4. TextMeshProUGUI component:
   - Text: "Giá thuê (Level 0): $80"
   - Font Size: 20
   - Alignment: Left
   - Color: Black
```

**2.1.7. Tạo Text Owner:**
```
1. Right-click PanelTileInfo → UI → Text - TextMeshPro
2. Rename: "TextOwner"
3. Inspector → RectTransform:
   - Anchor: Top-Left
   - Width: 350
   - Height: 30
   - Pos X: 25, Pos Y: -360
4. TextMeshProUGUI component:
   - Text: "Chủ: Chưa có chủ"
   - Font Size: 20
   - Alignment: Left
   - Color: Blue
```

**2.1.8. Tạo Button Close:**
```
1. Right-click PanelTileInfo → UI → Button - TextMeshPro
2. Rename: "ButtonClose"
3. Inspector → RectTransform:
   - Anchor: Bottom-Center
   - Width: 150
   - Height: 50
   - Pos X: 0, Pos Y: 30
4. Button → Text (child):
   - Text: "Đóng"
   - Font Size: 24
```

---

### **Bước 2.2: Add PanelTileInfo Component (3 phút)**

```
1. Hierarchy → Select PanelTileInfo
2. Inspector → Add Component → Search "PanelTileInfo"
3. Click "PanelTileInfo" script
4. Component được add ✅
```

---

### **Bước 2.3: Assign References (5 phút)**

```
1. Hierarchy → Select PanelTileInfo
2. Inspector → PanelTileInfo component:

   UI References:
     Image Location: [Drag ImageLocation từ Hierarchy]
     Text Location Name: [Drag TextLocationName]
     Text Buy Price: [Drag TextBuyPrice]
     Text Rent Price: [Drag TextRentPrice]
     Text Owner: [Drag TextOwner]
     Btn Close: [Drag ButtonClose]
   
   Location Images:
     Size: 36 (nhập số 36)
     Element 0: [Drag sprite Tile 1 - Start]
     Element 1: [Drag sprite Tile 2 - Tokyo]
     Element 2: [Drag sprite Tile 3 - Seoul]
     ... (36 sprites total)
   
   Dependencies:
     Property Manager: [Drag PropertyManager từ Hierarchy]
     Board Manager: [Drag BoardManager từ Hierarchy]
```

---

### **Bước 2.4: Assign 36 Sprites (10 phút)**

**Chuẩn bị sprites:**
```
Project → Assets → Sprites → Locations (folder)
  - Tile_01_Start.png
  - Tile_02_Tokyo.png
  - Tile_03_Seoul.png
  - Tile_04_Bangkok.png
  - ... (36 sprites total)
```

**Assign vào array:**
```
1. Inspector → PanelTileInfo → Location Images
2. Size: 36
3. Drag sprites theo thứ tự:
   - Element 0: Tile_01_Start.png
   - Element 1: Tile_02_Tokyo.png
   - Element 2: Tile_03_Seoul.png
   - ...
   - Element 35: Tile_36_DaNang.png
```

**⚠️ LƯU Ý:** Index phải khớp với tile index (0-35)!

---

### **Bước 2.5: Setup Button Close Event (2 phút)**

```
1. Hierarchy → PanelTileInfo → ButtonClose
2. Inspector → Button component:
   - On Click():
     - Click "+" để add event
     - Drag PanelTileInfo vào Object field
     - Function: PanelTileInfo → HidePanel()
```

---

### **Bước 2.6: Setup TileClickDetector (5 phút)**

**2.6.1. Tạo GameObject:**
```
1. Hierarchy → Right-click → Create Empty
2. Rename: "TileClickDetector"
3. Inspector → Add Component → Search "TileClickDetector"
4. Click "TileClickDetector" script
```

**2.6.2. Assign References:**
```
1. Inspector → TileClickDetector component:
   - Panel Tile Info: [Drag PanelTileInfo từ Hierarchy]
   - Camera: [Drag Main Camera từ Hierarchy]
```

---

### **Bước 2.7: Add Colliders to Tiles (5 phút)**

**⚠️ QUAN TRỌNG:** Tiles cần có Collider để detect click!

```
1. Hierarchy → Select Tile_1
2. Inspector → Add Component → Box Collider
3. Box Collider:
   - Center: (0, 0, 0)
   - Size: (1, 0.1, 1) (adjust theo platform size)
4. Repeat cho tất cả 36 tiles
```

**Hoặc dùng script để add colliders cho tất cả tiles:**
```csharp
// Menu: Tools → AntKnow → Add Colliders to Tiles
[MenuItem("Tools/AntKnow/Add Colliders to Tiles")]
public static void AddCollidersToTiles()
{
    TileVisual[] tiles = FindObjectsOfType<TileVisual>();
    foreach (var tile in tiles)
    {
        if (tile.GetComponent<Collider>() == null)
        {
            BoxCollider collider = tile.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = new Vector3(1f, 0.1f, 1f);
        }
    }
    Debug.Log($"Added colliders to {tiles.Length} tiles!");
}
```

---

### **Bước 2.8: Test PanelTileInfo (5 phút)**

```
1. Play Mode
2. Click vào tile Tokyo (tile index 1)
3. Panel xuất hiện:
   ✅ Image: Tokyo Tower (hoặc sprite bạn assign)
   ✅ Tên: "Tokyo"
   ✅ Giá mua: "$800"
   ✅ Giá thuê (Level 0): "$80"
   ✅ Chủ: "Chưa có chủ"
4. Click button "Đóng"
5. Panel biến mất ✅
```

---

## 🎯 **TỔNG KẾT**

### **Checklist:**
```
PHẦN 1: HOUSE/HOTEL MARKERS
✅ Generate markers cho 36 tiles
✅ Verify markers được assign vào TileVisual
✅ Test house spawning (Level 1-4)
✅ Test hotel spawning (Level 5)
✅ Adjust marker positions nếu cần

PHẦN 2: PANELTILEINFO
✅ Tạo UI Panel với 5 elements
✅ Add PanelTileInfo component
✅ Assign references (UI elements, managers)
✅ Assign 36 sprites
✅ Setup button close event
✅ Setup TileClickDetector
✅ Add colliders to tiles
✅ Test click tile → Panel hiển thị
```

---

## 🐛 **TROUBLESHOOTING**

### **Vấn đề 1: Click tile không hiển thị panel**
```
Nguyên nhân:
- Tile không có Collider
- TileClickDetector không được setup
- Camera reference sai

Giải pháp:
1. Check tile có BoxCollider không
2. Check TileClickDetector → Panel Tile Info assigned
3. Check TileClickDetector → Camera assigned
```

### **Vấn đề 2: Panel hiển thị sai thông tin**
```
Nguyên nhân:
- Sprite index không khớp tile index
- PropertyManager/BoardManager không assigned

Giải pháp:
1. Verify locationSprites array (index 0-35)
2. Check PanelTileInfo → Dependencies assigned
```

---

**Hãy làm theo từng bước và cho tôi biết kết quả!** 🚀

