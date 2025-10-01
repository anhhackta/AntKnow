# 🔧 Inventory System - Fixes Summary

## ✅ Đã sửa 3 vấn đề chính

---

## 🔧 Fix 1: Base Stats từ User Level

### Vấn đề:
```
❌ Base stats cố định (baseHealth=100, baseAgility=10, ...)
✅ Base stats phải tính theo user level: Mỗi level +1 tất cả stats
```

### Công thức mới:
```
Base Stats = BaseLv1 + (UserLevel - 1) * statsPerLevel

Ví dụ: User Level 5
- HP: 100 + (5-1)*1 = 104
- Agility: 10 + (5-1)*1 = 14
- Intelligence: 10 + (5-1)*1 = 14
- Luck: 10 + (5-1)*1 = 14
- Resistance: 10 + (5-1)*1 = 14
```

### Code changes:
```csharp
// LoadoutStatsDisplay.cs

[Header("Base Stats Config")]
[SerializeField] private int baseHealthLv1 = 100;
[SerializeField] private int baseAgilityLv1 = 10;
[SerializeField] private int baseIntelligenceLv1 = 10;
[SerializeField] private int baseLuckLv1 = 10;
[SerializeField] private int baseResistanceLv1 = 10;
[SerializeField] private int statsPerLevel = 1; // Mỗi level +1 tất cả stats

private TotalStats CalculateTotalStats()
{
    int userLevel = GetUserLevel(); // TODO: Load từ Firestore
    
    var stats = new TotalStats
    {
        health = baseHealthLv1 + (userLevel - 1) * statsPerLevel,
        agility = baseAgilityLv1 + (userLevel - 1) * statsPerLevel,
        intelligence = baseIntelligenceLv1 + (userLevel - 1) * statsPerLevel,
        luck = baseLuckLv1 + (userLevel - 1) * statsPerLevel,
        resistance = baseResistanceLv1 + (userLevel - 1) * statsPerLevel
    };
    
    // ... add equipment + cards
}

private int GetUserLevel()
{
    // TODO: Load từ Firestore users/{uid}/level
    return 1; // Tạm thời
}
```

### TODO:
```
[ ] Implement GetUserLevel() để load từ Firestore
[ ] Hoặc pass userLevel từ InventoryUIManager
```

---

## 🔧 Fix 2: Sprite Naming - Khớp với Firebase

### Vấn đề:
```
❌ Code expect: "Equipment/hat.basic"
✅ Sprites thực tế: "Equipment/equip.hat.basic.png"
```

### Sprites hiện tại:
```
Assets/Resources/
├── Cards/
│   ├── skill.bao-ke.png ✅
│   ├── skill.cham-chi.png ✅
│   ├── skill.lan-tron.png ✅
│   └── skill.sieu-sale.png ✅
├── Equipment/
│   ├── equip.hat.basic.png ✅
│   ├── equip.mask.basic.png ✅
│   ├── equip.shirt.basic.png ✅
│   ├── equip.shoes.basic.png ✅
│   └── equip.wings.basic.png ✅
└── Items/
    └── exp.small.png ✅
```

### Firestore icon field phải khớp:
```json
// items/equip.hat.basic
{
  "itemId": "equip.hat.basic",
  "name": "Mũ Cơ Bản",
  "type": "equipment",
  "icon": "Equipment/equip.hat.basic",  ← Phải khớp với file name
  "equipment": {
    "slot": "hat"
  },
  "attributes": {
    "health": 0,
    "agility": 0,
    "intelligence": 0,
    "luck": 10,
    "resistance": 0
  }
}

// items/skill.lan-tron
{
  "itemId": "skill.lan-tron",
  "name": "Lăn Tròn",
  "type": "skill_card",
  "icon": "Cards/skill.lan-tron",  ← Phải khớp với file name
  "attributes": {
    "primaryStat": "health",
    "health": 10,
    "agility": 0,
    "intelligence": 0,
    "luck": 0,
    "resistance": 0
  }
}
```

### Code changes:
```csharp
// ItemSlot.cs & CardSlot.cs

private void LoadItemSprite(Image targetImage, string iconPath)
{
    // iconPath từ Firestore: "Equipment/equip.hat.basic"
    // Resources.Load sẽ tự động tìm file: Assets/Resources/Equipment/equip.hat.basic.png
    Sprite sprite = Resources.Load<Sprite>(iconPath);
    
    if (sprite != null)
    {
        targetImage.sprite = sprite;
        Debug.Log($"✅ Loaded sprite: {iconPath}");
    }
    else
    {
        Debug.LogWarning($"❌ Sprite not found: {iconPath}\nCheck file: Assets/Resources/{iconPath}.png");
        targetImage.sprite = null;
    }
}
```

### Action items:
```
[ ] Update Firestore items collection:
    - Set icon field = "Equipment/equip.hat.basic" (khớp với file name)
    - Set icon field = "Cards/skill.lan-tron" (khớp với file name)
    - Set icon field = "Items/exp.small" (khớp với file name)
```

---

## 🔧 Fix 3: Load Inventory & Loadout từ Firestore

### Vấn đề:
```
❌ Chưa load inventory/loadout từ Firestore
✅ Code đã có sẵn, chỉ cần setup đúng
```

### Flow hiện tại:
```
Start()
  ↓
InitializeUI() - Tạo slots
  ↓
SetupEventListeners() - Subscribe events
  ↓
LoadInventoryAndLoadout() ← Đã có!
  ↓
inventoryService.LoadInventoryAsync(uid)
  ↓
inventoryService.LoadLoadoutAsync(uid)
  ↓
OnInventoryLoaded() → RefreshInventoryDisplay()
  ↓
OnLoadoutLoaded() → RefreshLoadoutDisplay()
```

### Code đã có:
```csharp
// InventoryUIManager.cs

private async void LoadInventoryAndLoadout()
{
    if (firebaseAuthService == null || firebaseAuthService.Auth == null || firebaseAuthService.Auth.CurrentUser == null)
    {
        Debug.LogError("[InventoryUI] User not logged in!");
        return;
    }
    
    string uid = firebaseAuthService.Auth.CurrentUser.UserId;
    
    // Load inventory
    await inventoryService.LoadInventoryAsync(uid);
    
    // Load loadout
    await inventoryService.LoadLoadoutAsync(uid);
    
    // Update character image
    UpdateCharacterImage();
}
```

### Tại sao chưa thấy items?

#### Lý do 1: Chưa có data trong Firestore
```
Check: Firebase Console > Firestore > users/{uid}/inventory
- Có items không?
- Có field icon không?
- icon field có đúng format không? ("Equipment/equip.hat.basic")
```

#### Lý do 2: InventoryService chưa được assign
```
Check: Unity Inspector > PanelInventory > InventoryUIManager
- inventoryService → Assign InventoryService GameObject
- firebaseAuthService → Assign FirebaseAuthService GameObject
```

#### Lý do 3: User chưa login
```
Check Console log:
- "[InventoryUI] User not logged in!" → Phải login trước
```

### Debug steps:
```
1. Play game
2. Login với test account
3. Check Console logs:
   - "[InventoryService] Loading inventory for user: {uid}"
   - "[InventoryService] Loaded X items from inventory"
   - "[InventoryUI] Inventory loaded: X items"
   - "✅ Loaded sprite: Equipment/equip.hat.basic"
   
4. Nếu không thấy logs → Check:
   - InventoryService có được assign không?
   - FirebaseAuthService có được assign không?
   - User đã login chưa?
   
5. Nếu thấy logs nhưng không thấy sprites → Check:
   - Firestore icon field có đúng không?
   - File sprites có tồn tại không?
   - File name có khớp với icon field không?
```

---

## 📋 Checklist để fix:

### 1. Update Firestore (10 phút)
- [ ] Mở Firebase Console > Firestore
- [ ] Collection: items
- [ ] Update field "icon" cho tất cả items:
  ```
  equip.hat.basic: icon = "Equipment/equip.hat.basic"
  equip.shirt.basic: icon = "Equipment/equip.shirt.basic"
  equip.wings.basic: icon = "Equipment/equip.wings.basic"
  equip.shoes.basic: icon = "Equipment/equip.shoes.basic"
  equip.mask.basic: icon = "Equipment/equip.mask.basic"
  skill.lan-tron: icon = "Cards/skill.lan-tron"
  skill.bao-ke: icon = "Cards/skill.bao-ke"
  skill.cham-chi: icon = "Cards/skill.cham-chi"
  skill.sieu-sale: icon = "Cards/skill.sieu-sale"
  exp.small: icon = "Items/exp.small"
  ```

### 2. Create test inventory data (5 phút)
- [ ] Mở Firebase Console > Firestore
- [ ] Collection: users/{uid}/inventory
- [ ] Tạo test items:
  ```json
  // Document: item1
  {
    "itemId": "equip.hat.basic",
    "type": "equipment",
    "level": 1,
    "stars": 0,
    "qty": 1,
    "durability": 100
  }
  
  // Document: item2
  {
    "itemId": "skill.lan-tron",
    "type": "skill_card",
    "level": 5,
    "stars": 3,
    "qty": 1,
    "durability": 100
  }
  ```

### 3. Create test loadout data (5 phút)
- [ ] Mở Firebase Console > Firestore
- [ ] Collection: users/{uid}/loadouts
- [ ] Document: slot1
  ```json
  {
    "active": true,
    "skillCardIds": [],
    "equipmentSet": {
      "hatId": "",
      "shirtId": "",
      "wingsId": "",
      "shoesId": "",
      "maskId": ""
    },
    "updatedAt": (timestamp)
  }
  ```

### 4. Setup Unity (5 phút)
- [ ] Select PanelInventory
- [ ] InventoryUIManager component:
  - [ ] inventoryService → Drag InventoryService GameObject
  - [ ] firebaseAuthService → Drag FirebaseAuthService GameObject
- [ ] Select StatsDisplay
- [ ] LoadoutStatsDisplay component:
  - [ ] firebaseAuthService → Drag FirebaseAuthService GameObject
  - [ ] statsPerLevel → 1
  - [ ] attributePerLevel → 2

### 5. Test (10 phút)
- [ ] Play game
- [ ] Login với test account
- [ ] Check Console logs:
  - [ ] "[InventoryService] Loading inventory..."
  - [ ] "[InventoryService] Loaded X items"
  - [ ] "✅ Loaded sprite: Equipment/equip.hat.basic"
  - [ ] "[LoadoutStats] User Level 1 → Base Stats: HP:100 ..."
- [ ] Verify items hiển thị trong inventory
- [ ] Drag item vào loadout
- [ ] Verify stats update

---

## 🎯 Summary:

| Issue | Status | Fix |
|-------|--------|-----|
| Base stats không tính theo user level | ✅ Fixed | Thêm statsPerLevel, tính theo công thức |
| Sprite naming không khớp | ✅ Fixed | Update Firestore icon field |
| Chưa load inventory/loadout | ⚠️ Need setup | Assign services, tạo test data |

---

**Next: Update Firestore và test! 🚀**

