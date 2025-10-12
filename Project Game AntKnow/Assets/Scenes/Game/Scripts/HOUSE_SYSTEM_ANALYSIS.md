# 🔍 PHÂN TÍCH HỆ THỐNG HOUSE MODELS

**Ngày:** 2025-10-12

---

## 📊 **VẤN ĐỀ 1: LOGIC MAPPING PANELBUY BUTTONS**

### **Code Flow Chi Tiết:**

#### **Bước 1: User click button trong PanelBuy**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
// Line 52-56: Setup button listeners
if (btnHouse1 != null) btnHouse1.onClick.AddListener(() => OnHouseButtonClicked(1));
if (btnHouse2 != null) btnHouse2.onClick.AddListener(() => OnHouseButtonClicked(2));
if (btnHouse3 != null) btnHouse3.onClick.AddListener(() => OnHouseButtonClicked(3));
if (btnHouse4 != null) btnHouse4.onClick.AddListener(() => OnHouseButtonClicked(4));
if (btnHotel != null) btnHotel.onClick.AddListener(() => OnHouseButtonClicked(5));
````
</augment_code_snippet>

**Mapping:**
```
Button "House 1" → OnHouseButtonClicked(1) → selectedLevel = 1
Button "House 2" → OnHouseButtonClicked(2) → selectedLevel = 2
Button "House 3" → OnHouseButtonClicked(3) → selectedLevel = 3
Button "House 4" → OnHouseButtonClicked(4) → selectedLevel = 4
Button "Hotel" → OnHouseButtonClicked(5) → selectedLevel = 5
```

---

#### **Bước 2: OnHouseButtonClicked() set selectedLevel**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
// Line 229-243
private void OnHouseButtonClicked(int level)
{
    // Toggle selection
    if (selectedLevel == level)
    {
        selectedLevel = 0; // Bỏ chọn
    }
    else
    {
        selectedLevel = level; // Chọn
    }
    
    UpdateHouseButtons();
    UpdatePrice();
}
````
</augment_code_snippet>

**Logic:**
- Click button lần 1 → selectedLevel = level (chọn)
- Click button lần 2 → selectedLevel = 0 (bỏ chọn)

---

#### **Bước 3: User click "MUA" → Callback với selectedLevel**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
// Line 341-355
private void OnBuyClicked()
{
    if (selectedLevel == 0) return; // Không mua nếu chưa chọn

    // Callback với level được chọn
    onBuyCallback?.Invoke(selectedLevel);
    Hide();
}
````
</augment_code_snippet>

**Kết quả:**
- selectedLevel được truyền vào callback
- GameManager nhận selectedLevel này

---

#### **Bước 4: GameManager xử lý selectedLevel**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
// Line 843-885
panelBuy.ShowBuy(tileName, basePrice, player.Money,
    (selectedLevel) =>
    {
        if (selectedLevel > 0)
        {
            // Buy property (level 0 = đất trống)
            bool buySuccess = propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);

            if (buySuccess)
            {
                // Upgrade if selected level > 1
                if (selectedLevel > 1)
                {
                    Debug.Log($"[GameManager] Attempting to upgrade to level {selectedLevel - 1}");
                    bool upgradeSuccess = propertyManager.UpgradeProperty(tileIndex, selectedLevel - 1, basePrice, player);
                }
            }
        }
    }
);
````
</augment_code_snippet>

**Logic:**
```
selectedLevel = 1:
  → BuyProperty() → property level = 0 (đất trống)
  → KHÔNG upgrade (vì selectedLevel <= 1)
  → Kết quả: 0 houses

selectedLevel = 2:
  → BuyProperty() → property level = 0
  → UpgradeProperty(targetLevel = 1) → property level = 1
  → Kết quả: 1 house

selectedLevel = 3:
  → BuyProperty() → property level = 0
  → UpgradeProperty(targetLevel = 2) → property level = 2
  → Kết quả: 2 houses

selectedLevel = 4:
  → BuyProperty() → property level = 0
  → UpgradeProperty(targetLevel = 3) → property level = 3
  → Kết quả: 3 houses

selectedLevel = 5:
  → BuyProperty() → property level = 0
  → UpgradeProperty(targetLevel = 4) → property level = 4
  → Kết quả: 4 houses
```

**⚠️ VẤN ĐỀ PHÁT HIỆN:**
- Button "Hotel" (selectedLevel = 5) → UpgradeProperty(targetLevel = 4) → 4 houses (KHÔNG PHẢI hotel!)
- Code KHÔNG XỬ LÝ hotel đúng cách!

---

#### **Bước 5: PropertyManager.UpgradeProperty()**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/PropertyManager.cs" mode="EXCERPT">
````csharp
public bool UpgradeProperty(int tileId, int targetLevel, int basePrice, PlayerGameController player)
{
    // ... checks ...
    
    // Upgrade
    player.SubtractMoney(totalCost);
    propertyLevels[tileId] = targetLevel; // ⭐ Set property level
    
    // Update visual
    UpdatePropertyVisual(tileId);
    
    return true;
}
````
</augment_code_snippet>

---

#### **Bước 6: PropertyVisual.UpdatePropertyVisual()**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Visual/PropertyVisual.cs" mode="EXCERPT">
````csharp
public void UpdatePropertyVisual(int tileId, int level, int ownerIndex, int rentPrice)
{
    // ...
    
    if (level >= 1 && level <= 4)
    {
        // Spawn houses (1-4)
        tile.SpawnHouses(housePrefab, level, playerColor, roofMaterialName);
    }
    else if (level == 5)
    {
        // Spawn hotel
        tile.SpawnHotel(hotelPrefab, playerColor, roofMaterialName);
    }
}
````
</augment_code_snippet>

**Logic:**
```
level = 0 → 0 houses (đất trống)
level = 1 → 1 house
level = 2 → 2 houses
level = 3 → 3 houses
level = 4 → 4 houses
level = 5 → 1 hotel
```

---

### **✅ MAPPING CHÍNH XÁC:**

```
Button "House 1" (selectedLevel = 1):
  → BuyProperty() → level 0
  → KHÔNG upgrade
  → PropertyVisual: level = 0
  → Kết quả: 0 houses (đất trống) ❌ SAI!

Button "House 2" (selectedLevel = 2):
  → BuyProperty() → level 0
  → UpgradeProperty(targetLevel = 1) → level 1
  → PropertyVisual: level = 1
  → SpawnHouses(count = 1)
  → Kết quả: 1 house ✅

Button "House 3" (selectedLevel = 3):
  → UpgradeProperty(targetLevel = 2) → level 2
  → SpawnHouses(count = 2)
  → Kết quả: 2 houses ✅

Button "House 4" (selectedLevel = 4):
  → UpgradeProperty(targetLevel = 3) → level 3
  → SpawnHouses(count = 3)
  → Kết quả: 3 houses ✅

Button "Hotel" (selectedLevel = 5):
  → UpgradeProperty(targetLevel = 4) → level 4
  → SpawnHouses(count = 4)
  → Kết quả: 4 houses ❌ SAI! (Nên là hotel)
```

---

### **🐛 VẤN ĐỀ PHÁT HIỆN:**

**1. Button "House 1" spawn 0 houses thay vì 1 house**
- User mong đợi: Click "House 1" → 1 house xuất hiện
- Thực tế: Click "House 1" → 0 houses (chỉ mua đất trống)

**2. Button "Hotel" spawn 4 houses thay vì hotel**
- User mong đợi: Click "Hotel" → 1 hotel xuất hiện
- Thực tế: Click "Hotel" → 4 houses (không phải hotel)

---

### **✅ GIẢI PHÁP:**

#### **Option 1: Sửa GameManager logic (RECOMMENDED)**

Thay đổi logic để selectedLevel trực tiếp map với property level:

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
// ❌ OLD (SAI):
if (selectedLevel > 1)
{
    propertyManager.UpgradeProperty(tileIndex, selectedLevel - 1, basePrice, player);
}

// ✅ NEW (ĐÚNG):
if (selectedLevel > 0)
{
    // selectedLevel = 1 → upgrade to level 1 (1 house)
    // selectedLevel = 2 → upgrade to level 2 (2 houses)
    // selectedLevel = 5 → upgrade to level 5 (hotel)
    propertyManager.UpgradeProperty(tileIndex, selectedLevel, basePrice, player);
}
````
</augment_code_snippet>

**Kết quả sau khi fix:**
```
Button "House 1" → selectedLevel = 1 → UpgradeProperty(level = 1) → 1 house ✅
Button "House 2" → selectedLevel = 2 → UpgradeProperty(level = 2) → 2 houses ✅
Button "House 3" → selectedLevel = 3 → UpgradeProperty(level = 3) → 3 houses ✅
Button "House 4" → selectedLevel = 4 → UpgradeProperty(level = 4) → 4 houses ✅
Button "Hotel" → selectedLevel = 5 → UpgradeProperty(level = 5) → 1 hotel ✅
```

---

#### **Option 2: Đổi tên buttons (Alternative)**

Nếu muốn giữ logic hiện tại, đổi tên buttons:

```
Button "Chỉ mua đất" → selectedLevel = 1 → 0 houses
Button "Đất + 1 nhà" → selectedLevel = 2 → 1 house
Button "Đất + 2 nhà" → selectedLevel = 3 → 2 houses
Button "Đất + 3 nhà" → selectedLevel = 4 → 3 houses
Button "Đất + 4 nhà" → selectedLevel = 5 → 4 houses
Button "Đất + Hotel" → selectedLevel = 6 → 1 hotel
```

Nhưng cần thêm case cho hotel:
```csharp
if (selectedLevel == 6)
{
    propertyManager.UpgradeProperty(tileIndex, 5, basePrice, player); // level 5 = hotel
}
else if (selectedLevel > 1)
{
    propertyManager.UpgradeProperty(tileIndex, selectedLevel - 1, basePrice, player);
}
```

---

## 🔧 **VẤN ĐỀ 2: HOUSE PREFAB BỊ KÉO DÀI**

### **Yêu cầu thông tin:**

**1. Console logs khi spawn houses:**
```
Cần tìm dòng:
[TileVisual] House 0 - Platform scale: (?, ?, ?), Compensated local scale: (?, ?, ?)
```

**Ví dụ:**
```
[TileVisual] House 0 - Platform scale: (2, 0.1, 1), Compensated local scale: (0.1275, 2.55, 0.255)
                                        ↑ X = 2 (PROBLEM!)
```

**2. Unity Inspector - Platform Transform:**
```
Hierarchy → Tile_X → Platform
Inspector → Transform:
  - Position: (?, ?, ?)
  - Rotation: (?, ?, ?)
  - Scale: (?, ?, ?) ← ⭐ QUAN TRỌNG
```

**3. Unity Inspector - House Prefab:**
```
Project → Assets → Prefabs → HousePrefab
Inspector → Transform:
  - Scale: (?, ?, ?) ← ⭐ QUAN TRỌNG
```

---

### **Phân tích công thức compensate:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Visual/TileVisual.cs" mode="EXCERPT">
````csharp
Vector3 platformScale = platform.localScale;
Vector3 desiredWorldScale = Vector3.one * 0.255f; // (0.255, 0.255, 0.255)
Vector3 compensatedScale = new Vector3(
    desiredWorldScale.x / platformScale.x, // 0.255 / platformScale.x
    desiredWorldScale.y / platformScale.y, // 0.255 / platformScale.y
    desiredWorldScale.z / platformScale.z  // 0.255 / platformScale.z
);
house.transform.localScale = compensatedScale;
````
</augment_code_snippet>

**Ví dụ tính toán:**

**Case 1: Platform scale = (1, 0.1, 1) - UNIFORM X/Z**
```
compensatedScale.x = 0.255 / 1 = 0.255 ✅
compensatedScale.y = 0.255 / 0.1 = 2.55 ✅
compensatedScale.z = 0.255 / 1 = 0.255 ✅

World scale = (1, 0.1, 1) × (0.255, 2.55, 0.255) = (0.255, 0.255, 0.255) ✅ ĐÚNG
```

**Case 2: Platform scale = (2, 0.1, 1) - NON-UNIFORM X/Z**
```
compensatedScale.x = 0.255 / 2 = 0.1275 ❌ NHỎ HƠN
compensatedScale.y = 0.255 / 0.1 = 2.55 ✅
compensatedScale.z = 0.255 / 1 = 0.255 ✅

World scale = (2, 0.1, 1) × (0.1275, 2.55, 0.255) = (0.255, 0.255, 0.255) ✅ ĐÚNG về số học

NHƯNG:
- House localScale.x = 0.1275 (nhỏ hơn localScale.z = 0.255)
- House bị SQUASHED theo trục X (bị ép lại)
- Khi nhìn trong Scene view, house trông bị KÉO DÀI theo trục Z (vì X nhỏ hơn)
```

---

### **✅ GIẢI PHÁP:**

#### **Option A: Normalize Platform Scale (RECOMMENDED)**

**Unity Inspector:**
```
1. Hierarchy → Find all Tile GameObjects
2. For each Tile → Platform child:
   Inspector → Transform → Scale:
   - Change from (2, 0.1, 1) to (1, 0.1, 1) ← ⭐ X = Z = 1
   
3. Adjust Platform size bằng cách scale parent Tile thay vì Platform
```

**Lợi ích:**
- ✅ Code compensate hoạt động đúng
- ✅ House giữ nguyên aspect ratio
- ✅ Không cần sửa code

---

#### **Option B: Sửa Code Compensate**

Nếu không thể thay đổi Platform scale, sửa code:

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Visual/TileVisual.cs" mode="EXCERPT">
````csharp
// ⭐ FIX: Chỉ compensate cho Y axis (height)
// X và Z giữ nguyên để house không bị biến dạng
Vector3 platformScale = platform.localScale;
Vector3 desiredWorldScale = Vector3.one * 0.255f;

Vector3 compensatedScale = new Vector3(
    desiredWorldScale.x, // Giữ nguyên X (không compensate)
    desiredWorldScale.y / platformScale.y, // Chỉ compensate Y
    desiredWorldScale.z  // Giữ nguyên Z (không compensate)
);

house.transform.localScale = compensatedScale;

Debug.Log($"[TileVisual] House {i} - Platform scale: {platformScale}, Compensated scale: {compensatedScale}");
````
</augment_code_snippet>

**Nhưng:**
- ❌ House world scale sẽ bị ảnh hưởng bởi platform X/Z scale
- ❌ Nếu platform X = 2, house world scale X = 2 × 0.255 = 0.51 (lớn gấp đôi)

---

#### **Option C: Spawn House Không Phải Child Của Platform**

Spawn house ở world space, không làm child của platform:

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Visual/TileVisual.cs" mode="EXCERPT">
````csharp
// Spawn in world space
GameObject house = Instantiate(housePrefab);

// Calculate world position
Vector3 worldPos = platform.TransformPoint(localPositions[i]);
house.transform.position = worldPos;

// Set world scale (không bị ảnh hưởng bởi parent)
house.transform.localScale = Vector3.one * 0.255f;

// Set rotation
house.transform.rotation = platform.rotation * Quaternion.Euler(0f, -90f, 0f);

// Store reference (để ClearHouses() có thể destroy)
spawnedHouses[i] = house;
````
</augment_code_snippet>

**Lợi ích:**
- ✅ House không bị ảnh hưởng bởi platform scale
- ✅ Giữ nguyên aspect ratio
- ❌ Phức tạp hơn (phải tính world position/rotation)
- ❌ House không tự động move khi platform move

---

## 🧪 **TESTING STEPS**

### **Test 1: Kiểm tra Platform Scale**

```
1. Unity → Hierarchy → Tile_0 (hoặc bất kỳ tile nào)
2. Expand → Find "Platform" child
3. Inspector → Transform → Scale
4. Ghi lại giá trị: (X, Y, Z) = (?, ?, ?)
5. Báo cho tôi biết!
```

### **Test 2: Kiểm tra Console Logs**

```
1. Play Mode
2. Mua property với selectedLevel = 2
3. Check Console cho dòng:
   [TileVisual] House 0 - Platform scale: (?, ?, ?), Compensated local scale: (?, ?, ?)
4. Copy full log và gửi cho tôi
```

### **Test 3: Screenshot Scene View**

```
1. Play Mode
2. Mua property với 1 house
3. Scene view → Zoom vào house
4. Screenshot cho thấy house bị kéo dài
5. Gửi screenshot cho tôi
```

---

**Hãy cung cấp thông tin này để tôi có thể đưa ra giải pháp chính xác!** 🚀

