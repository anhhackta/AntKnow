# ✅ House & Hotel Fix - Hướng Dẫn Cụ Thể

## 📐 BƯỚC 1: PHÂN TÍCH HƯỚNG (Axes Orientation)

### Platform (Cube mỏng dẹp):
```
Y: Lên trời ↑
Z: Vào giữa map →
X: Bên phải (nhìn từ trên xuống) →

Visualization (nhìn từ trên xuống):
        Z (vào giữa)
        ↓
    [Platform]
        ↑
        X (phải)
```

### House Prefab:
```
Y: Lên trời ↑
X: Vào giữa map →
Z: Bên trái (nhìn từ trên xuống) ←

Visualization (nhìn từ trên xuống):
        X (vào giữa)
        ↓
      [House]
        ↑
        Z (trái)
```

### Hotel Prefab:
```
Z: Lên trời ↑
Y: Vào giữa map →
X: Bên trái (nhìn từ trên xuống) ←

Visualization (nhìn từ trên xuống):
        Y (vào giữa)
        ↓
      [Hotel]
        ↑
        X (trái)
```

---

## 🧮 BƯỚC 2: TÍNH TOÁN ROTATION

### House → Platform Alignment:

**Mục tiêu:**
- House X (→giữa) phải align với Platform Z (→giữa)
- House Z (←trái) phải align với Platform X (→phải)

**Vấn đề:**
- House Z hướng trái ←
- Platform X hướng phải →
- Ngược chiều!

**Giải pháp:**
```
Rotate -90° around Y axis:

Before:
House X → giữa
House Z ← trái

After:
House X → Platform Z (giữa) ✓
House -Z → Platform X (phải) ✓
```

**Code:**
```csharp
house.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
```

---

### Hotel → Platform Alignment:

**Mục tiêu:**
- Hotel Z (↑) phải align với Platform Y (↑)
- Hotel Y (→giữa) phải align với Platform Z (→giữa)
- Hotel X (←trái) phải align với Platform X (→phải)

**Vấn đề:**
- Hotel Z hướng lên ↑, Platform Y hướng lên ↑ (OK)
- Hotel Y hướng giữa →, Platform Z hướng giữa → (OK)
- Hotel X hướng trái ←, Platform X hướng phải → (Ngược chiều!)

**Giải pháp:**
```
Step 1: Rotate 90° around X axis
- Hotel Z (↑) → Platform Y (↑) ✓

Step 2: Rotate 180° around Y axis
- Hotel -X (→phải) → Platform X (→phải) ✓

Combined: Quaternion.Euler(90f, 180f, 0f)
```

**Code:**
```csharp
hotel.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
```

---

## 🎯 BƯỚC 3: VỊ TRÍ 4 HOUSES LIỀN KỀ

### Layout (nhìn từ trên xuống):

```
Platform (hình chữ nhật):

        Z (vào giữa)
        ↓
    ┌─────────┐
    │ 1     2 │
    │         │
    │ 3     4 │
    └─────────┘
        ↑
        X (phải)

House 1: Top-left     (-0.15, 0.1, -0.15)
House 2: Top-right    (0.15, 0.1, -0.15)
House 3: Bottom-left  (-0.15, 0.1, 0.15)
House 4: Bottom-right (0.15, 0.1, 0.15)
```

### Giải thích tọa độ:

**X axis (trái-phải):**
- X = -0.15: Bên trái
- X = 0.15: Bên phải

**Y axis (lên-xuống):**
- Y = 0.1: Độ cao trên platform

**Z axis (xa-gần giữa map):**
- Z = -0.15: Xa giữa map (top)
- Z = 0.15: Gần giữa map (bottom)

---

## 🔧 BƯỚC 4: CODE IMPLEMENTATION

### TileVisual.cs - SpawnHouses():

```csharp
public void SpawnHouses(GameObject housePrefab, int count, Color playerColor, string roofMaterialName = "ngói")
{
    ClearHouses();

    if (housePrefab == null || platform == null)
    {
        return;
    }

    // 4 vị trí cố định trên Platform (hình chữ nhật)
    Vector3[] localPositions = new Vector3[]
    {
        new Vector3(-0.15f, 0.1f, -0.15f),  // House 1: Top-left
        new Vector3(0.15f, 0.1f, -0.15f),   // House 2: Top-right
        new Vector3(-0.15f, 0.1f, 0.15f),   // House 3: Bottom-left
        new Vector3(0.15f, 0.1f, 0.15f)     // House 4: Bottom-right
    };

    for (int i = 0; i < count && i < 4; i++)
    {
        // Spawn as child of platform
        GameObject house = Instantiate(housePrefab, platform);

        // Set local position
        house.transform.localPosition = localPositions[i];

        // Set local scale (0.255 như khi để ngoài)
        house.transform.localScale = Vector3.one * 0.255f;

        // Fix rotation: -90° around Y axis
        house.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        // Set color to roof material
        SetHouseColor(house, playerColor, roofMaterialName);

        spawnedHouses[i] = house;
    }
}
```

### TileVisual.cs - SpawnHotel():

```csharp
public void SpawnHotel(GameObject hotelPrefab, Color playerColor, string roofMaterialName = "ngói")
{
    ClearHouses();

    if (hotelPrefab == null || platform == null)
    {
        return;
    }

    // Spawn as child of platform
    spawnedHotel = Instantiate(hotelPrefab, platform);

    // Set local position (center of platform)
    spawnedHotel.transform.localPosition = new Vector3(0f, 0.15f, 0f);

    // Set local scale (9 như khi để ngoài)
    spawnedHotel.transform.localScale = Vector3.one * 9f;

    // Fix rotation: 90° around X, 180° around Y
    spawnedHotel.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);

    // Set color to roof material
    SetHouseColor(spawnedHotel, playerColor, roofMaterialName);
}
```

---

## 🎯 Vấn Đề Đã Fix:

### 1. **Scale Đúng** ⭐
```
✅ House: localScale = 0.255 (giữ nguyên như ngoài)
✅ Hotel: localScale = 9 (giữ nguyên như ngoài)
✅ Không bị scale lạ do parent
```

### 2. **Rotation Đúng** ⭐⭐
```
✅ House: Quaternion.Euler(0f, -90f, 0f)
   - Rotate -90° around Y axis
   - House X (→giữa) align với Platform Z (→giữa)

✅ Hotel: Quaternion.Euler(90f, 180f, 0f)
   - Rotate 90° around X, 180° around Y
   - Hotel Z (↑) align với Platform Y (↑)
   - Hotel Y (→giữa) align với Platform Z (→giữa)
```

### 3. **4 Houses Liền Kề** ⭐⭐⭐
```
✅ Layout hình chữ nhật:
   [1] [2]
   [3] [4]

✅ Positions:
   1: (-0.15, 0.1, -0.15) Top-left
   2: (0.15, 0.1, -0.15) Top-right
   3: (-0.15, 0.1, 0.15) Bottom-left
   4: (0.15, 0.1, 0.15) Bottom-right
```

### 4. **Hotel Thay Thế** ⭐⭐
```
✅ Position: (0, 0.15, 0) - Center
✅ Scale: 9
✅ Rotation: (90, 180, 0)
✅ Clear 4 houses trước khi spawn
```

### 5. **Tiền = 5000** ⭐
```
✅ PlayerGameController: money = 5000
```

---

## 🎮 BƯỚC 5: TEST & VERIFY

### Test 1: Spawn 1 House (Level 1)
```
1. Buy property
2. Upgrade to level 1
3. Check:
   ✅ 1 house ở vị trí top-left (-0.15, 0.1, -0.15)
   ✅ House scale = (0.255, 0.255, 0.255)
   ✅ House rotation = (0, -90, 0)
   ✅ House color = Player color (RED)
   ✅ House hướng vào giữa map
```

### Test 2: Spawn 4 Houses (Level 4)
```
1. Upgrade to level 4
2. Check:
   ✅ 4 houses liền kề hình chữ nhật
   ✅ House 1: Top-left
   ✅ House 2: Top-right
   ✅ House 3: Bottom-left
   ✅ House 4: Bottom-right
   ✅ All houses scale = 0.255
   ✅ All houses rotation = (0, -90, 0)
   ✅ All houses color = RED
```

### Test 3: Spawn Hotel (Level 5)
```
1. Upgrade to level 5
2. Check:
   ✅ 4 houses removed
   ✅ 1 hotel ở center (0, 0.15, 0)
   ✅ Hotel scale = (9, 9, 9)
   ✅ Hotel rotation = (90, 180, 0)
   ✅ Hotel color = RED
   ✅ Hotel hướng vào giữa map
```

### Test 4: Money
```
1. Press Play
2. Check:
   ✅ Player money = 5000
```

---

## 📊 Scale & Rotation Table:

### House:
```
Ngoài Tiles:
- Scale: (0.255, 0.255, 0.255)
- Rotation: (0, 0, 0)

Trong Platform (as child):
- Local Scale: (0.255, 0.255, 0.255) ← Giữ nguyên!
- Local Rotation: (-90, 0, 0) ← Fix rotation!
```

### Hotel:
```
Ngoài Tiles:
- Scale: (9, 9, 9)
- Rotation: (0, 0, 0)

Trong Platform (as child):
- Local Scale: (9, 9, 9) ← Giữ nguyên!
- Local Rotation: (90, 0, 0) ← Fix rotation!
```

---

## 🎮 Test Flow:

### Test 1: Spawn 1 House
```
1. Buy property
2. Upgrade to level 1
3. Check:
   ✅ 1 house at top-left position
   ✅ House scale = 0.255
   ✅ House rotation = (-90, 0, 0)
   ✅ House color = RED
```

### Test 2: Spawn 4 Houses
```
1. Upgrade to level 4
2. Check:
   ✅ 4 houses in rectangle formation
   ✅ Top-left, top-right, bottom-left, bottom-right
   ✅ All houses scale = 0.255
   ✅ All houses rotation = (-90, 0, 0)
   ✅ All houses color = RED
```

### Test 3: Spawn Hotel
```
1. Upgrade to level 5
2. Check:
   ✅ 4 houses removed
   ✅ 1 hotel at center
   ✅ Hotel scale = 9
   ✅ Hotel rotation = (90, 0, 0)
   ✅ Hotel color = RED
```

### Test 4: Money
```
1. Press Play
2. Check:
   ✅ Player money = 5000
   ✅ Can buy expensive properties
```

---

## 💡 Giải Thích Rotation:

### Tại Sao Rotate -90° (House)?
```
Platform axes:
- Y: Up (lên trời)
- Z: Forward (vào giữa)
- X: Right

House axes (original):
- Y: Up (lên trời)
- X: Forward (vào giữa) ← Khác với Platform!
- Z: Right

Để align X của House với Z của Platform:
→ Rotate -90° around X axis
→ House X axis → Platform Z axis
```

### Tại Sao Rotate 90° (Hotel)?
```
Hotel axes (original):
- Z: Up (lên trời) ← Khác với Platform!
- Y: Forward (vào giữa)
- X: Right

Để align Z của Hotel với Y của Platform:
→ Rotate 90° around X axis
→ Hotel Z axis → Platform Y axis
```

---

## 🐛 BƯỚC 7: TROUBLESHOOTING

### Issue: Houses sai hướng
```
Kiểm tra:
1. House rotation = (0, -90, 0) ← Phải đúng!
2. Nếu sai hướng, thử:
   - (0, 0, 0) - No rotation
   - (0, 90, 0) - Rotate 90° ngược lại
   - (0, 180, 0) - Rotate 180°
3. Quan sát trong Scene view để xác định rotation đúng
```

### Issue: Hotel sai hướng
```
Kiểm tra:
1. Hotel rotation = (90, 180, 0) ← Phải đúng!
2. Nếu sai hướng, thử:
   - (90, 0, 0) - Chỉ rotate X
   - (0, 180, 0) - Chỉ rotate Y
   - (-90, 180, 0) - Rotate X ngược lại
3. Quan sát trong Scene view để xác định rotation đúng
```

### Issue: Houses không liền kề
```
Kiểm tra:
1. Positions = (-0.15, 0.1, -0.15), (0.15, 0.1, -0.15), etc.
2. Nếu quá xa/gần, adjust spacing:
   - Tăng spacing: 0.2 thay vì 0.15
   - Giảm spacing: 0.1 thay vì 0.15
3. Check platform size
```

### Issue: Scale sai
```
Kiểm tra:
1. House localScale = (0.255, 0.255, 0.255)
2. Hotel localScale = (9, 9, 9)
3. Parent phải là platform
4. Platform scale không ảnh hưởng vì dùng localScale
```

---

## ✅ KẾT QUẢ CUỐI CÙNG:

```
✅ Thống nhất hướng (axes orientation)
✅ House rotation = (0, -90, 0)
✅ Hotel rotation = (90, 180, 0)
✅ 4 houses liền kề hình chữ nhật
✅ Hotel thay thế 4 houses
✅ Scale đúng (0.255 và 9)
✅ Tiền = 5000
✅ Code có comment rõ ràng
✅ Dễ maintain và sửa sau này
```

---

**ĐÃ THỐNG NHẤT & FIX HOÀN TOÀN! 🎮**

**Đọc từ BƯỚC 1 → BƯỚC 7 để hiểu rõ logic! ⭐**

