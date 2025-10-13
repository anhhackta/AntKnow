# 🐛 DEBUG: INVENTORY & LOADOUT KHÔNG HIỂN THỊ

## 📊 PHÂN TÍCH VẤN ĐỀ

### ✅ Dữ liệu Firebase (ĐÚNG)
```
users/{uid}/inventory/{autoID}
├── createdAt: timestamp
├── itemId: "equip.wings.basic"
├── type: "equipment"
└── updatedAt: timestamp

users/{uid}/loadouts/slot1
├── active: true
├── equipmentSet: (map)
│   ├── hatId: "W6GUmqbcQnUKEvBhOikN"
│   ├── maskId: "QODsKFBesf63MA7BxkMC"
│   ├── shirtId: "XTRefx6yGzevb7ik1jZU"
│   ├── shoesId: "V6FuR1NjyVIKQKhdEBvh"
│   └── wingsId: "wUUwrkSxXBvIoX4eptJJ"
├── skillCardIds: (array)
│   ├── 0: "GsEqsehoEOHgkF99Lcvn"
│   └── 1: "WlDJV76zqmWl6DNR8Kw9"
└── updatedAt: timestamp
```

### ✅ Resources Folder (ĐÚNG)
```
Assets/Resources/Items/
├── equip.hat.basic.png
├── equip.mask.basic.png
├── equip.shirt.basic.png
├── equip.shoes.basic.png
├── equip.wings.basic.png
├── skill.bao-ke.png
├── skill.cham-chi.png
├── skill.lan-tron.png
├── skill.sieu-sale.png
└── exp.small.png
```

### ⚠️ VẤN ĐỀ CÓ THỂ XẢY RA

#### 1. **Firebase items collection chưa có data**
**Vấn đề:** 
- Inventory có `itemId: "equip.wings.basic"`
- Nhưng `items/equip.wings.basic` document không tồn tại trong Firestore
- → `GetItemDataAsync()` return null
- → `item.itemData = null`
- → Không load được icon

**Kiểm tra:**
```
Firestore Console → items collection
- Có document "equip.wings.basic" không?
- Có document "skill.bao-ke" không?
```

**Cấu trúc cần có:**
```
items/equip.wings.basic
├── name: "Wings Basic"
├── type: "equipment"
├── rarity: "common"
├── icon: "equip.wings.basic"  ← QUAN TRỌNG!
├── attributes: (map)
│   ├── health: 5
│   ├── agility: 10
│   └── ...
└── equipment: (map)
    ├── slot: "wings"
    └── durabilityMax: 100
```

#### 2. **Icon path không đúng**
**Vấn đề:**
- Firebase: `icon: "equip.wings.basic"`
- SpriteLoader tìm: `Resources.Load<Sprite>("equip.wings.basic")`
- File thực tế: `Assets/Resources/Items/equip.wings.basic.png`
- → Không tìm thấy vì thiếu folder "Items/"

**Fix:**
- Option 1: Firebase lưu `icon: "Items/equip.wings.basic"`
- Option 2: SpriteLoader tự động tìm trong subfolder (ĐÃ CÓ)

#### 3. **InventoryUIManager không được gọi**
**Vấn đề:**
- `InventoryUIManager.Start()` không được gọi
- → `LoadInventoryAndLoadout()` không chạy
- → Không load data

**Kiểm tra:**
- InventoryUIManager có được attach vào GameObject trong scene không?
- GameObject có active không?

#### 4. **FirebaseAuthService chưa login**
**Vấn đề:**
```csharp
if (firebaseAuthService == null || firebaseAuthService.Auth == null || firebaseAuthService.Auth.CurrentUser == null)
{
    Debug.LogError("[InventoryUI] User not logged in!");
    return;
}
```
- Nếu user chưa login → Không load được

---

## 🔧 CÁCH SỬA

### **FIX 1: Tạo items collection trong Firebase**

**Bước 1: Tạo document cho equipment**
```
Firestore Console → items collection → Add Document

Document ID: equip.hat.basic
Fields:
- name: "Hat Basic" (string)
- type: "equipment" (string)
- rarity: "common" (string)
- status: "active" (string)
- icon: "equip.hat.basic" (string)  ← QUAN TRỌNG!
- attributes: (map)
  - health: 5 (number)
  - agility: 0 (number)
  - intelligence: 0 (number)
  - luck: 0 (number)
  - resistance: 0 (number)
- equipment: (map)
  - slot: "hat" (string)
  - durabilityMax: 100 (number)
```

**Lặp lại cho:**
- `equip.shirt.basic`
- `equip.wings.basic`
- `equip.shoes.basic`
- `equip.mask.basic`

**Bước 2: Tạo document cho skill cards**
```
Document ID: skill.bao-ke
Fields:
- name: "Bảo Kê" (string)
- type: "skill_card" (string)
- rarity: "rare" (string)
- status: "active" (string)
- icon: "skill.bao-ke" (string)  ← QUAN TRỌNG!
- attributes: (map)
  - health: 0 (number)
  - agility: 0 (number)
  - intelligence: 0 (number)
  - luck: 0 (number)
  - resistance: 10 (number)
  - primaryStat: "resistance" (string)
  - attributePerLevel: 2 (number)
- skill: (map)
  - mode: "passive" (string)
  - effect: "Giảm 20% tiền thuê nhà" (string)
  - effectId: "rentReduction" (string)  ← QUAN TRỌNG!
  - cooldownBaseTurns: 0 (number)
```

**Lặp lại cho:**
- `skill.cham-chi`
- `skill.lan-tron`
- `skill.sieu-sale`

---

### **FIX 2: Update SpriteLoader để tìm trong Items/**

**File:** `SpriteLoader.cs` (ĐÃ CÓ SẴN)

Code hiện tại đã hỗ trợ tìm trong `Items/` folder:
```csharp
// Try 3: Nếu path không có folder, thử trong các subfolders
if (!iconPath.Contains("/"))
{
    // Thử trong Items/
    sprite = Resources.Load<Sprite>($"Items/{iconPath}");
    if (sprite != null)
    {
        Debug.Log($"[SpriteLoader] ✅ Loaded sprite from Items/: Items/{iconPath}");
        return sprite;
    }
}
```

**→ Không cần sửa gì!**

---

### **FIX 3: Add Debug Logs**

**File:** `InventoryService.cs`

Thêm debug logs chi tiết:
```csharp
public async Task<List<InventoryItem>> LoadInventoryAsync(string uid)
{
    try
    {
        DebugLog($"Loading inventory for user: {uid}");
        
        var inventoryRef = firestore.Collection("users").Document(uid).Collection("inventory");
        var snapshot = await inventoryRef.GetSnapshotAsync();
        
        DebugLog($"Found {snapshot.Documents.Count} documents in inventory");
        
        cachedInventory.Clear();
        
        foreach (var doc in snapshot.Documents)
        {
            DebugLog($"Processing inventory doc: {doc.Id}");
            
            var item = ParseInventoryItem(doc);
            if (item != null)
            {
                DebugLog($"Parsed item: {item.itemId}, type: {item.type}");
                
                // Load item data from items collection
                item.itemData = await GetItemDataAsync(item.itemId);
                
                if (item.itemData != null)
                {
                    DebugLog($"✅ Loaded itemData for {item.itemId}, icon: {item.itemData.icon}");
                }
                else
                {
                    DebugLogError($"❌ Failed to load itemData for {item.itemId}");
                }
                
                cachedInventory.Add(item);
            }
        }
        
        DebugLog($"Loaded {cachedInventory.Count} items from inventory");
        OnInventoryLoaded?.Invoke(cachedInventory);
        
        return cachedInventory;
    }
    catch (Exception e)
    {
        DebugLogError($"Error loading inventory: {e.Message}\n{e.StackTrace}");
        OnInventoryError?.Invoke($"Lỗi tải inventory: {e.Message}");
        return new List<InventoryItem>();
    }
}
```

---

### **FIX 4: Add Test Button**

**File:** `InventoryUIManager.cs`

Thêm test button để debug:
```csharp
[Header("Debug")]
[SerializeField] private Button buttonTestLoad;

private void SetupEventListeners()
{
    // ...existing code...
    
    if (buttonTestLoad != null)
    {
        buttonTestLoad.onClick.AddListener(TestLoadInventory);
    }
}

[ContextMenu("Test Load Inventory")]
private async void TestLoadInventory()
{
    Debug.Log("=== TEST LOAD INVENTORY ===");
    
    if (firebaseAuthService == null)
    {
        Debug.LogError("FirebaseAuthService is null!");
        return;
    }
    
    if (firebaseAuthService.Auth == null)
    {
        Debug.LogError("Firebase Auth is null!");
        return;
    }
    
    if (firebaseAuthService.Auth.CurrentUser == null)
    {
        Debug.LogError("Current User is null! Please login first.");
        return;
    }
    
    string uid = firebaseAuthService.Auth.CurrentUser.UserId;
    Debug.Log($"User ID: {uid}");
    
    // Load inventory
    var inventory = await inventoryService.LoadInventoryAsync(uid);
    Debug.Log($"Loaded {inventory.Count} items");
    
    foreach (var item in inventory)
    {
        Debug.Log($"- Item: {item.itemId}, Type: {item.type}, ItemData: {(item.itemData != null ? "OK" : "NULL")}");
        if (item.itemData != null)
        {
            Debug.Log($"  Icon: {item.itemData.icon}, Name: {item.itemData.name}");
        }
    }
    
    // Load loadout
    var loadout = await inventoryService.LoadLoadoutAsync(uid);
    Debug.Log($"Loadout: {loadout.skillCardIds.Count} cards, {loadout.equipmentSet.GetAllEquipmentIds().Count} equipment");
    
    Debug.Log("=== TEST COMPLETE ===");
}
```

---

## 🧪 CÁCH TEST

### **Test 1: Kiểm tra Firebase items collection**
```
1. Mở Firestore Console
2. Vào collection "items"
3. Kiểm tra có documents:
   - equip.hat.basic
   - equip.shirt.basic
   - equip.wings.basic
   - equip.shoes.basic
   - equip.mask.basic
   - skill.bao-ke
   - skill.cham-chi
   - skill.lan-tron
   - skill.sieu-sale
   - exp.small
4. Mỗi document phải có field "icon"
```

### **Test 2: Test SpriteLoader**
```
1. Tạo GameObject trong scene
2. Add component: TestSpriteLoading
3. Click "Test All Sprites" trong Inspector
4. Xem Console logs:
   - ✅ = Load thành công
   - ❌ = Load thất bại
```

### **Test 3: Test InventoryService**
```
1. Mở InventoryUIManager trong Inspector
2. Click "Test Load Inventory" (Context Menu)
3. Xem Console logs:
   - Số lượng items loaded
   - ItemData có null không
   - Icon path có đúng không
```

### **Test 4: Test UI Display**
```
1. Play game
2. Login
3. Mở Inventory scene
4. Kiểm tra:
   - Inventory slots có hiển thị items không?
   - Loadout slots có hiển thị equipment không?
   - Icons có load được không?
```

---

## 📝 CHECKLIST SỬA LỖI

### **Bước 1: Tạo items collection** ✅
- [ ] Tạo 5 equipment documents
- [ ] Tạo 4 skill card documents
- [ ] Tạo 1 exp card document
- [ ] Mỗi document có field "icon"
- [ ] Mỗi document có field "name", "type", "rarity"

### **Bước 2: Test SpriteLoader** ✅
- [ ] Add TestSpriteLoading component
- [ ] Run "Test All Sprites"
- [ ] Verify tất cả sprites load được

### **Bước 3: Test InventoryService** ✅
- [ ] Add Test button vào InventoryUIManager
- [ ] Run "Test Load Inventory"
- [ ] Verify inventory load được
- [ ] Verify itemData không null
- [ ] Verify icon path đúng

### **Bước 4: Test UI** ✅
- [ ] Play game
- [ ] Login
- [ ] Open Inventory
- [ ] Verify items hiển thị
- [ ] Verify loadout hiển thị
- [ ] Verify icons hiển thị

---

## 🚀 HÀNH ĐỘNG TIẾP THEO

**Bạn muốn tôi:**
1. **Tạo script tự động tạo items collection?** (Firebase Cloud Functions hoặc Unity Editor Tool)
2. **Tạo test button ngay?** (Add vào InventoryUIManager)
3. **Xem log hiện tại?** (Để debug vấn đề cụ thể)

Cho tôi biết để tôi tiếp tục! 🔧

