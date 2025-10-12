# 🐛 DEBUG: PROPERTY PURCHASE NOT CALLING PROPERTYMANAGER

**Ngày:** 2025-10-12

---

## 🔍 **VẤN ĐỀ**

### **Triệu chứng:**
- Khi mua property, tiền bị trừ ✅
- Platform đổi màu ✅
- Nhưng **KHÔNG CÓ** debug logs từ PropertyManager ❌
- Nhưng **KHÔNG CÓ** house models ❌

### **Console logs hiện tại:**
```
[PanelNotification] ShowNotification: Player 1 mua Jakarta (600)
[GameManager] Updated PanelMe - New money: 9400
[GameManager] Auto ending turn after delay
```

**THIẾU:**
```
❌ [PropertyManager] BuyProperty called - Tile: X, Player: 0, Price: 600
❌ [PropertyManager] Player 0 bought property X for 600 - COMPLETE
❌ [PropertyVisual] UpdatePropertyVisual called - Tile: X, Level: 0, Owner: 0
```

---

## 🔍 **PHÂN TÍCH**

### **Code flow mong đợi:**

```
GameManager.ShowBuyPanel()
  ↓
User clicks "MUA"
  ↓
onBuy callback (selectedLevel > 0)
  ↓
propertyManager.BuyProperty(tileId, playerIdx, basePrice, player) ← ⭐ SHOULD BE CALLED
  ↓
PropertyManager.BuyProperty()
  ↓
player.SubtractMoney(basePrice) ← ✅ WORKING (tiền bị trừ)
  ↓
UpdatePropertyVisual(tileId) ← ❌ NOT CALLED (no debug logs)
  ↓
PropertyVisual.UpdatePropertyVisual()
  ↓
tile.SpawnHouses() ← ❌ NOT CALLED
```

### **Vấn đề có thể là:**

1. ❌ `propertyManager` là null trong GameManager
2. ❌ `BuyProperty()` return false sớm (already owned / not enough money)
3. ❌ `BuyProperty()` không được gọi (logic error trong callback)
4. ❌ Tiền bị trừ ở chỗ khác (không phải trong PropertyManager.BuyProperty)

---

## ✅ **GIẢI PHÁP: THÊM DEBUG LOGS**

### **File 1: GameManager.cs**

**Thêm debug logs TRƯỚC và SAU khi gọi BuyProperty():**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
(selectedLevel) =>
{
    if (selectedLevel > 0)
    {
        // ⭐ DEBUG: Check before buying
        Debug.Log($"[GameManager] Attempting to buy property - Tile: {tileIndex}, Player: {playerIdx}, Price: {basePrice}, PlayerMoney: {player.Money}, SelectedLevel: {selectedLevel}");
        Debug.Log($"[GameManager] PropertyManager is null? {propertyManager == null}");

        // Buy property
        bool buySuccess = propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);

        Debug.Log($"[GameManager] BuyProperty returned: {buySuccess}");

        if (buySuccess)
        {
            Debug.Log($"[GameManager] {player.PlayerName} bought {tileName} for {basePrice}");

            // Upgrade if selected level > 1
            if (selectedLevel > 1)
            {
                Debug.Log($"[GameManager] Attempting to upgrade to level {selectedLevel - 1}");
                bool upgradeSuccess = propertyManager.UpgradeProperty(tileIndex, selectedLevel - 1, basePrice, player);
                Debug.Log($"[GameManager] UpgradeProperty returned: {upgradeSuccess}");
            }

            // Update UI
            if (panelGame != null)
            {
                panelGame.UpdateAllPanels();
                Debug.Log($"[GameManager] Updated PanelMe - New money: {player.Money}");
            }
        }
        else
        {
            Debug.LogError($"[GameManager] BuyProperty FAILED! Check PropertyManager logs above.");
        }
    }
}
````
</augment_code_snippet>

---

### **File 2: PropertyManager.cs**

**Thêm debug logs trong BuyProperty():**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/PropertyManager.cs" mode="EXCERPT">
````csharp
public bool BuyProperty(int tileId, int playerIndex, int basePrice, PlayerGameController player)
{
    Debug.Log($"[PropertyManager] BuyProperty called - Tile: {tileId}, Player: {playerIndex}, Price: {basePrice}");

    // Check if already owned
    if (IsPropertyOwned(tileId))
    {
        Debug.LogWarning($"[PropertyManager] Property {tileId} already owned!");
        return false;
    }
    
    // Check money
    if (player.Money < basePrice)
    {
        Debug.LogWarning($"[PropertyManager] Player {playerIndex} cannot afford property {tileId}. Money: {player.Money}, Price: {basePrice}");
        return false;
    }
    
    Debug.Log($"[PropertyManager] Checks passed, buying property {tileId}...");

    // Buy
    player.SubtractMoney(basePrice);
    propertyOwners[tileId] = playerIndex;
    propertyLevels[tileId] = 0;
    propertyRentMultipliers[tileId] = 1f;

    Debug.Log($"[PropertyManager] Property ownership set. Calling UpdatePropertyVisual...");

    // Update visual
    UpdatePropertyVisual(tileId);

    Debug.Log($"[PropertyManager] Player {playerIndex} bought property {tileId} for {basePrice} - COMPLETE");
    return true;
}
````
</augment_code_snippet>

---

## 🧪 **TESTING - CRITICAL**

### **Bước 1: Save và Compile**

```
1. Save all files (Ctrl+S) ← ⭐ LÀM NGAY
2. Return to Unity
3. Wait for compile
4. Check Console for compile errors
```

---

### **Bước 2: Test mua property**

```
1. Play Mode
2. Roll đến property tile
3. Click "MUA" (level 1 = buy land only)
4. Check Console logs:
```

---

## 📊 **EXPECTED CONSOLE LOGS**

### **Scenario 1: PropertyManager được gọi ĐÚNG**

```
[GameManager] Attempting to buy property - Tile: 5, Player: 0, Price: 600, PlayerMoney: 10000, SelectedLevel: 1
[GameManager] PropertyManager is null? False ← ⭐ MUST BE FALSE
[PropertyManager] BuyProperty called - Tile: 5, Player: 0, Price: 600 ← ⭐ MUST SEE THIS
[PropertyManager] Checks passed, buying property 5...
[PropertyManager] Property ownership set. Calling UpdatePropertyVisual...
[PropertyManager] UpdatePropertyVisual - Tile: 5, Level: 0, Owner: 0, Rent: 50
[PropertyVisual] UpdatePropertyVisual called - Tile: 5, Level: 0, Owner: 0
[PropertyVisual] Set platform color for empty land (level 0) on tile 5
[PropertyManager] Player 0 bought property 5 for 600 - COMPLETE
[GameManager] BuyProperty returned: True ← ⭐ SUCCESS
[GameManager] Player 1 bought Jakarta for 600
[GameManager] Updated PanelMe - New money: 9400
```

**Kết quả:**
- ✅ PropertyManager được gọi
- ✅ UpdatePropertyVisual được gọi
- ✅ Platform đổi màu
- ✅ Tiền bị trừ

---

### **Scenario 2: PropertyManager là NULL**

```
[GameManager] Attempting to buy property - Tile: 5, Player: 0, Price: 600, PlayerMoney: 10000, SelectedLevel: 1
[GameManager] PropertyManager is null? True ← ❌ PROBLEM!

NullReferenceException: Object reference not set to an instance of an object
```

**Giải pháp:**
```
1. Unity → Hierarchy → Find "GameManager" GameObject
2. Inspector → GameManager component
3. Managers section:
   - Property Manager: [ASSIGN PropertyManager GameObject] ← ⭐ FIX THIS
4. Save scene (Ctrl+S)
5. Play Mode again
```

---

### **Scenario 3: BuyProperty KHÔNG được gọi**

```
[GameManager] Attempting to buy property - Tile: 5, Player: 0, Price: 600, PlayerMoney: 10000, SelectedLevel: 1
[GameManager] PropertyManager is null? False
❌ NO LOG: "[PropertyManager] BuyProperty called..."
[GameManager] Updated PanelMe - New money: 9400 ← Tiền vẫn bị trừ!
```

**Nghĩa là:**
- Tiền bị trừ Ở CHỖ KHÁC (không phải trong PropertyManager.BuyProperty)
- Có thể có code trùng lặp đang trừ tiền

**Giải pháp:**
- Search toàn bộ code cho `player.SubtractMoney(basePrice)` hoặc `player.SubtractMoney(600)`
- Tìm xem có chỗ nào khác đang trừ tiền không

---

### **Scenario 4: BuyProperty return FALSE**

```
[GameManager] Attempting to buy property - Tile: 5, Player: 0, Price: 600, PlayerMoney: 10000, SelectedLevel: 1
[GameManager] PropertyManager is null? False
[PropertyManager] BuyProperty called - Tile: 5, Player: 0, Price: 600
[PropertyManager] Property 5 already owned! ← ❌ PROBLEM
[GameManager] BuyProperty returned: False ← ❌ FAILED
[GameManager] BuyProperty FAILED! Check PropertyManager logs above.
```

**Nghĩa là:**
- Property đã được mua trước đó
- Hoặc logic check ownership bị lỗi

**Giải pháp:**
- Check `propertyOwners` dictionary
- Có thể cần reset game state

---

## 🎯 **NEXT STEPS**

```
1. Save all files (Ctrl+S) ← ⭐ LÀM NGAY
2. Return to Unity
3. Compile
4. Play Mode
5. Mua property
6. Check Console logs
7. So sánh với 4 scenarios trên
8. Báo kết quả cho tôi:
   - Scenario nào xảy ra?
   - Copy FULL console logs
   - Screenshot nếu cần
```

---

## 🔍 **DEBUGGING CHECKLIST**

### **Check 1: PropertyManager assigned?**
```
Unity → Hierarchy → GameManager
Inspector → GameManager component
Managers:
  - Property Manager: [PropertyManager GameObject] ← ⭐ MUST BE ASSIGNED
```

### **Check 2: PropertyVisual component exists?**
```
Unity → Hierarchy → PropertyManager GameObject
Inspector:
  - PropertyManager component ✅
  - PropertyVisual component ✅ ← ⭐ MUST EXIST
```

### **Check 3: Prefabs assigned?**
```
Unity → Hierarchy → PropertyManager GameObject
Inspector → PropertyVisual component:
  - House Prefab: [HousePrefab] ← ⭐ MUST BE ASSIGNED
  - Hotel Prefab: [HotelPrefab] ← ⭐ MUST BE ASSIGNED
```

---

## 📝 **SUMMARY**

### **Đã thêm:**
- ✅ Debug logs trong GameManager.ShowBuyPanel() callback
- ✅ Debug logs trong PropertyManager.BuyProperty()
- ✅ Check propertyManager null
- ✅ Check BuyProperty return value
- ✅ Error log nếu BuyProperty failed

### **Mục đích:**
- 🔍 Tìm xem PropertyManager.BuyProperty() có được gọi không
- 🔍 Tìm xem tại sao UpdatePropertyVisual() không được gọi
- 🔍 Tìm xem tiền bị trừ ở đâu (trong PropertyManager hay chỗ khác)

---

**Hãy test ngay và cho tôi biết Console logs đầy đủ!** 🚀

**Đặc biệt chú ý:**
- ⭐ "[GameManager] PropertyManager is null? True/False"
- ⭐ "[PropertyManager] BuyProperty called..." (có xuất hiện không?)
- ⭐ "[GameManager] BuyProperty returned: True/False"

