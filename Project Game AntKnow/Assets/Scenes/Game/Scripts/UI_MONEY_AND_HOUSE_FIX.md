# ✅ UI MONEY UPDATE & HOUSE MODELS FIX

**Ngày:** 2025-10-12

---

## 🐛 **VẤN ĐỀ**

### **Vấn đề 1: Tiền không cập nhật UI**
- Khi mua property, tiền bị trừ trong code (Console log) nhưng PanelMe KHÔNG cập nhật
- PanelMe vẫn hiển thị số tiền cũ
- Yêu cầu: Cập nhật PanelMe sau mỗi thay đổi tiền

### **Vấn đề 2: House models không hiển thị**
- Khi mua/upgrade property, house models KHÔNG xuất hiện trên platform
- Platform chỉ đổi màu nhưng không có house 3D models
- Yêu cầu: Hiển thị house models tương ứng với level (1-4 houses, 5 = hotel)

---

## 🔍 **PHÂN TÍCH**

### **Vấn đề 1: Root Cause**

**Code flow:**
```
PropertyManager.BuyProperty()
  ↓
player.SubtractMoney(basePrice) ← ✅ Tiền bị trừ
  ↓
money -= amount ← ✅ Biến money updated
  ↓
❌ KHÔNG CÓ event/callback để notify UI!
  ↓
PanelMe KHÔNG được refresh
```

**Giải pháp:**
- GameManager gọi `panelGame.UpdateAllPanels()` sau mỗi thay đổi tiền
- PanelGame.UpdateAllPanels() → PanelMe.UpdateDisplayPublic() → UpdateMoney()

---

### **Vấn đề 2: Root Cause**

**Có thể do:**
1. ❌ PropertyVisual component không được assign trong PropertyManager
2. ❌ housePrefab/hotelPrefab không được assign trong PropertyVisual
3. ❌ TileVisual không có Platform reference
4. ❌ SpawnHouses() không được gọi

**Giải pháp:**
- Thêm debug logs để track:
  - PropertyVisual có được tìm thấy không?
  - housePrefab/hotelPrefab có null không?
  - UpdatePropertyVisual() có được gọi không?
  - SpawnHouses() có được gọi không?

---

## ✅ **GIẢI PHÁP ĐÃ TRIỂN KHAI**

### **Fix 1: Update PanelMe sau mỗi thay đổi tiền**

**File:** `GameManager.cs`

#### **1.1. Sau khi mua property:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
// Buy property
propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);

// Upgrade if selected level > 0
if (selectedLevel > 1)
{
    propertyManager.UpgradeProperty(tileIndex, selectedLevel - 1, basePrice, player);
}

// ⭐ UPDATE UI - Refresh PanelMe to show new money
if (panelGame != null)
{
    panelGame.UpdateAllPanels();
    Debug.Log($"[GameManager] Updated PanelMe - New money: {player.Money}");
}
````
</augment_code_snippet>

#### **1.2. Sau khi trả tiền thuê:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
propertyManager.PayRent(tileIndex, basePrice, player, owner);

// ⭐ UPDATE UI - Refresh panels to show new money
if (panelGame != null)
{
    panelGame.UpdateAllPanels();
    Debug.Log($"[GameManager] Updated panels after rent - Player: {player.Money}, Owner: {owner.Money}");
}
````
</augment_code_snippet>

#### **1.3. Sau khi Event card:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
if (moneyChange > 0)
{
    player.AddMoney(moneyChange);
}
else if (moneyChange < 0)
{
    player.SubtractMoney(-moneyChange);
}

// ⭐ UPDATE UI - Refresh panels to show new money
if (panelGame != null)
{
    panelGame.UpdateAllPanels();
    Debug.Log($"[GameManager] Updated panels after event - Player money: {player.Money}");
}
````
</augment_code_snippet>

#### **1.4. Sau khi Travel:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
player.SubtractMoney(100);

// ⭐ UPDATE UI - Refresh panels to show new money
if (panelGame != null)
{
    panelGame.UpdateAllPanels();
    Debug.Log($"[GameManager] Updated panels after travel - Player money: {player.Money}");
}
````
</augment_code_snippet>

---

### **Fix 2: Debug logs cho House Models**

**File:** `PropertyManager.cs`

#### **2.1. Awake() - Check components:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/PropertyManager.cs" mode="EXCERPT">
````csharp
private void Awake()
{
    if (propertyVisual == null)
    {
        propertyVisual = GetComponent<PropertyVisual>();
        if (propertyVisual == null)
        {
            Debug.LogError("[PropertyManager] PropertyVisual component not found! House models will not display!");
        }
        else
        {
            Debug.Log("[PropertyManager] PropertyVisual component found");
        }
    }

    if (boardManager == null)
    {
        boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("[PropertyManager] BoardManager not found!");
        }
    }
}
````
</augment_code_snippet>

#### **2.2. UpdatePropertyVisual() - Debug logs:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/PropertyManager.cs" mode="EXCERPT">
````csharp
private void UpdatePropertyVisual(int tileId)
{
    if (propertyVisual == null)
    {
        Debug.LogError($"[PropertyManager] PropertyVisual is null! Cannot update visual for tile {tileId}");
        return;
    }

    if (boardManager == null)
    {
        Debug.LogError($"[PropertyManager] BoardManager is null! Cannot update visual for tile {tileId}");
        return;
    }

    // ... calculate level, owner, rent ...

    Debug.Log($"[PropertyManager] UpdatePropertyVisual - Tile: {tileId}, Level: {level}, Owner: {ownerIndex}, Rent: {finalRent}");

    propertyVisual.UpdatePropertyVisual(tileId, level, ownerIndex, finalRent);
}
````
</augment_code_snippet>

---

**File:** `PropertyVisual.cs`

#### **2.3. Awake() - Check prefabs:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Visual/PropertyVisual.cs" mode="EXCERPT">
````csharp
private void Awake()
{
    // ⭐ Check prefabs
    if (housePrefab == null)
    {
        Debug.LogError("[PropertyVisual] housePrefab is not assigned! Houses will not spawn!");
    }
    if (hotelPrefab == null)
    {
        Debug.LogError("[PropertyVisual] hotelPrefab is not assigned! Hotels will not spawn!");
    }

    // Get tiles from TileSetup...
}
````
</augment_code_snippet>

#### **2.4. UpdatePropertyVisual() - Debug logs:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Visual/PropertyVisual.cs" mode="EXCERPT">
````csharp
public void UpdatePropertyVisual(int tileId, int level, int ownerIndex, int rentPrice)
{
    Debug.Log($"[PropertyVisual] UpdatePropertyVisual called - Tile: {tileId}, Level: {level}, Owner: {ownerIndex}");

    TileVisual tile = GetTile(tileId);
    if (tile == null)
    {
        Debug.LogWarning($"[PropertyVisual] Tile {tileId} not found!");
        return;
    }

    tile.ClearHouses();
    Debug.Log($"[PropertyVisual] Cleared old houses on tile {tileId}");

    // ... set platform color ...

    if (level >= 1 && level <= 4)
    {
        if (housePrefab == null)
        {
            Debug.LogError($"[PropertyVisual] housePrefab is null! Cannot spawn houses on tile {tileId}");
            return;
        }
        Debug.Log($"[PropertyVisual] Spawning {level} houses on tile {tileId}");
        tile.SpawnHouses(housePrefab, level, playerColor, roofMaterialName);
    }
    else if (level == 5)
    {
        if (hotelPrefab == null)
        {
            Debug.LogError($"[PropertyVisual] hotelPrefab is null! Cannot spawn hotel on tile {tileId}");
            return;
        }
        Debug.Log($"[PropertyVisual] Spawning hotel on tile {tileId}");
        tile.SpawnHotel(hotelPrefab, playerColor, roofMaterialName);
    }
}
````
</augment_code_snippet>

---

## 🧪 **TESTING - CRITICAL**

### **Test 1: UI Money Update (5 phút)**

```
1. Save all files (Ctrl+S)
2. Return to Unity
3. Wait for compile
4. Play Mode
5. Roll đến property tile
6. Click "MUA" trong PanelBuy
7. Check:
   ✅ Console: "[GameManager] Updated PanelMe - New money: ..."
   ✅ PanelMe hiển thị số tiền MỚI (10000 - giá property)
   ✅ Số tiền cập nhật NGAY sau khi mua
   
8. Roll đến property của người khác
9. Check:
   ✅ Console: "[GameManager] Updated panels after rent - Player: ..., Owner: ..."
   ✅ PanelMe hiển thị số tiền sau khi trả rent
   
10. Roll đến Event tile
11. Check:
    ✅ Console: "[GameManager] Updated panels after event - Player money: ..."
    ✅ PanelMe hiển thị số tiền sau event (+/-)
```

---

### **Test 2: House Models (10 phút)**

```
1. Play Mode
2. Check Console ngay khi game start:
   
   Expected logs:
   ✅ "[PropertyManager] PropertyVisual component found"
   ✅ "[PropertyVisual] Got X tiles from TileSetup"
   
   If see errors:
   ❌ "[PropertyManager] PropertyVisual component not found!"
      → PropertyManager GameObject KHÔNG CÓ PropertyVisual component
      → Add PropertyVisual component to PropertyManager GameObject
   
   ❌ "[PropertyVisual] housePrefab is not assigned!"
      → Unity Inspector → PropertyVisual → Assign housePrefab
   
   ❌ "[PropertyVisual] hotelPrefab is not assigned!"
      → Unity Inspector → PropertyVisual → Assign hotelPrefab

3. Roll đến property tile
4. Click "MUA" (level 1 = buy land only)
5. Check Console:
   
   Expected logs:
   ✅ "[PropertyManager] UpdatePropertyVisual - Tile: X, Level: 0, Owner: 0, Rent: ..."
   ✅ "[PropertyVisual] UpdatePropertyVisual called - Tile: X, Level: 0, Owner: 0"
   ✅ "[PropertyVisual] Set platform color for empty land (level 0) on tile X"
   
6. Check Scene view:
   ✅ Platform đổi màu (Red/Blue/Green/Yellow)
   ✅ KHÔNG CÓ house models (vì level 0 = đất trống)

7. Roll lại đến property đã mua
8. Click "MUA" → Select level 2 (buy + upgrade to 1 house)
9. Check Console:
   
   Expected logs:
   ✅ "[PropertyManager] UpdatePropertyVisual - Tile: X, Level: 1, Owner: 0, Rent: ..."
   ✅ "[PropertyVisual] Spawning 1 houses on tile X"
   
   If see error:
   ❌ "[PropertyVisual] housePrefab is null! Cannot spawn houses on tile X"
      → Unity Inspector → PropertyVisual → Assign housePrefab!

10. Check Scene view:
    ✅ Platform có màu player
    ✅ 1 house model xuất hiện trên platform
    ✅ House roof có màu player

11. Upgrade to level 3, 4, 5:
    ✅ Level 2: 2 houses
    ✅ Level 3: 3 houses
    ✅ Level 4: 4 houses
    ✅ Level 5: 1 hotel (lớn hơn house)
```

---

## 🔧 **UNITY SETUP - NẾU HOUSE KHÔNG HIỂN THỊ**

### **Bước 1: Check PropertyManager GameObject**

```
1. Unity → Hierarchy → Find "PropertyManager" (hoặc GameObject có PropertyManager component)
2. Inspector → Check components:
   ✅ PropertyManager component
   ✅ PropertyVisual component ← ⭐ CRITICAL
   
3. If PropertyVisual component KHÔNG CÓ:
   → Add Component → PropertyVisual
```

---

### **Bước 2: Assign Prefabs trong PropertyVisual**

```
1. Unity → Hierarchy → PropertyManager GameObject
2. Inspector → PropertyVisual component
3. Assign:
   - House Prefab: [Drag house prefab from Project] ← ⭐ CRITICAL
   - Hotel Prefab: [Drag hotel prefab from Project] ← ⭐ CRITICAL
   - Roof Material Name: "ngói" (default)
   - Tile Setup: [Drag Tiles GameObject from Hierarchy]
```

**Tìm prefabs:**
```
Project → Assets → Prefabs → Properties:
- HousePrefab.prefab
- HotelPrefab.prefab
```

---

### **Bước 3: Check TileVisual components**

```
1. Unity → Hierarchy → Tiles → Tile_0 (any tile)
2. Inspector → TileVisual component
3. Check:
   ✅ Platform: [Assigned to Platform child GameObject]
   ✅ Text Price: [Assigned to TextPrice TextMeshPro]
   
4. If Platform KHÔNG assigned:
   → Drag "Platform" child GameObject to Platform field
```

---

## 📊 **EXPECTED CONSOLE LOGS**

### **Khi game start:**
```
[PropertyManager] PropertyVisual component found
[PropertyVisual] Got 36 tiles from TileSetup
```

### **Khi mua property (level 0 = đất trống):**
```
[GameManager] Player 1 bought Taipei for 650
[PropertyManager] UpdatePropertyVisual - Tile: 1, Level: 0, Owner: 0, Rent: 50
[PropertyVisual] UpdatePropertyVisual called - Tile: 1, Level: 0, Owner: 0
[PropertyVisual] Cleared old houses on tile 1
[PropertyVisual] Set platform color for empty land (level 0) on tile 1
[GameManager] Updated PanelMe - New money: 9350
```

### **Khi upgrade to level 1 (1 house):**
```
[PropertyManager] Property 1 upgraded to level 1 for 260
[PropertyManager] UpdatePropertyVisual - Tile: 1, Level: 1, Owner: 0, Rent: 100
[PropertyVisual] UpdatePropertyVisual called - Tile: 1, Level: 1, Owner: 0
[PropertyVisual] Cleared old houses on tile 1
[PropertyVisual] Set platform color on tile 1
[PropertyVisual] Spawning 1 houses on tile 1
[GameManager] Updated PanelMe - New money: 9090
```

---

## ✅ **SUMMARY**

### **Đã sửa:**

**Vấn đề 1: UI Money Update**
- ✅ GameManager gọi `panelGame.UpdateAllPanels()` sau:
  - Mua property
  - Trả tiền thuê
  - Event card
  - Travel
- ✅ PanelMe tự động refresh hiển thị số tiền mới

**Vấn đề 2: House Models Debug**
- ✅ Thêm debug logs vào PropertyManager.Awake()
- ✅ Thêm debug logs vào PropertyManager.UpdatePropertyVisual()
- ✅ Thêm debug logs vào PropertyVisual.Awake()
- ✅ Thêm debug logs vào PropertyVisual.UpdatePropertyVisual()
- ✅ Check housePrefab/hotelPrefab null trước khi spawn

---

## 🎯 **NEXT STEPS**

```
1. Save all files (Ctrl+S) ← ⭐ LÀM NGAY
2. Return to Unity
3. Wait for compile
4. Check Console for errors
5. Play Mode
6. Test UI money update (mua property, event, travel)
7. Check Console logs for house models
8. If housePrefab null → Assign trong Unity Inspector
9. Test house spawning (buy + upgrade)
10. Báo kết quả!
```

---

**Cho tôi biết kết quả testing nhé!** 🚀

