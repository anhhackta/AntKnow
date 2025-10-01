# 🐛 Debug Guide - Inventory System

## ✅ Vấn đề 1: Firebase Auth tự động đăng nhập

### Hiện tượng:
```
Bật Play mode → Chưa đăng nhập nhưng đã thấy:
- [FirebaseAuth] User ID: nbf1k15NTadoSZP13yUJlEXyErx1
- [FirebaseAuth] Email: ruhojy@gmail.com
- User signed in: ruhojy@gmail.com
```

### Nguyên nhân:
```
✅ ĐÂY KHÔNG PHẢI LỖI!

Firebase Auth có tính năng "Session Persistence":
- Tự động lưu session khi user đăng nhập
- Tự động restore session khi mở lại app
- User KHÔNG cần đăng nhập lại mỗi lần mở game

Đây là tính năng MONG MUỐN cho UX tốt!
```

### Giải thích:
```
Flow bình thường:
1. User đăng nhập lần đầu → Firebase lưu session
2. Đóng game
3. Mở game lại → Firebase tự động restore session
4. OnAuthStateChanged() được gọi → User đã đăng nhập

Trong Unity Editor:
1. Play mode lần 1 → Đăng nhập → Session lưu
2. Stop Play mode
3. Play mode lần 2 → Session restore → Tự động đăng nhập
```

### Nếu muốn test từ đầu:

#### Option 1: Sign Out trước khi stop Play mode
```csharp
// Thêm button "Sign Out" trong game
public async void OnSignOutButtonClicked()
{
    await firebaseAuthService.SignOutAsync();
    Debug.Log("Signed out successfully!");
}
```

#### Option 2: Clear auth cache (Editor only)
```
Windows: %APPDATA%\..\LocalLow\<CompanyName>\<ProjectName>\
Mac: ~/Library/Application Support/<CompanyName>/<ProjectName>/

Xóa folder này để clear tất cả cache
```

#### Option 3: Sign out trong code (Editor only)
```csharp
// FirebaseAuthService.cs - Thêm vào InitAsync()

#if UNITY_EDITOR
if (auth.CurrentUser != null)
{
    Debug.LogWarning("[FirebaseAuth] Editor mode: Auto sign out for testing");
    auth.SignOut();
}
#endif
```

---

## 🔍 Vấn đề 2: Kiểm tra Firestore items có icon field

### Checklist:

#### Bước 1: Kiểm tra Firestore items collection
```
1. Mở Firebase Console
2. Firestore Database
3. Collection: items
4. Check từng document:
   - equip.hat.basic
   - equip.shirt.basic
   - equip.wings.basic
   - equip.shoes.basic
   - equip.mask.basic
   - skill.lan-tron
   - skill.bao-ke
   - skill.cham-chi
   - skill.sieu-sale
   - exp.small
```

#### Bước 2: Verify icon field format
```json
// ✅ ĐÚNG
{
  "itemId": "equip.hat.basic",
  "name": "Mũ Cơ Bản",
  "type": "equipment",
  "icon": "Equipment/equip.hat.basic",  ← Phải có field này!
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

// ❌ SAI - Thiếu icon field
{
  "itemId": "equip.hat.basic",
  "name": "Mũ Cơ Bản",
  "type": "equipment",
  // ← Thiếu icon field!
  "equipment": {
    "slot": "hat"
  }
}

// ❌ SAI - icon field sai format
{
  "itemId": "equip.hat.basic",
  "icon": "hat.basic",  ← Thiếu folder "Equipment/"
}
```

#### Bước 3: Verify sprites tồn tại
```
Assets/Resources/
├── Equipment/
│   ├── equip.hat.basic.png ✅
│   ├── equip.shirt.basic.png ✅
│   ├── equip.wings.basic.png ✅
│   ├── equip.shoes.basic.png ✅
│   └── equip.mask.basic.png ✅
├── Cards/
│   ├── skill.lan-tron.png ✅
│   ├── skill.bao-ke.png ✅
│   ├── skill.cham-chi.png ✅
│   └── skill.sieu-sale.png ✅
└── Items/
    └── exp.small.png ✅
```

#### Bước 4: Verify icon path khớp với file name
```
Firestore icon field → File name

"Equipment/equip.hat.basic" → Assets/Resources/Equipment/equip.hat.basic.png ✅
"Cards/skill.lan-tron" → Assets/Resources/Cards/skill.lan-tron.png ✅
"Items/exp.small" → Assets/Resources/Items/exp.small.png ✅

"Equipment/hat.basic" → Assets/Resources/Equipment/hat.basic.png ❌ (File không tồn tại)
"equip.hat.basic" → Assets/Resources/equip.hat.basic.png ❌ (Thiếu folder)
```

---

## 🧪 Test Steps

### Test 1: Load inventory và check Console logs
```
1. Play game
2. Login (hoặc tự động login)
3. Check Console logs:

✅ Success logs:
[InventoryService] Loading inventory for user: nbf1k15NTadoSZP13yUJlEXyErx1
[InventoryService] Loaded 5 items from inventory
[InventoryUI] Inventory loaded: 5 items
[ItemSlot] ✅ Loaded sprite: Equipment/equip.hat.basic
[ItemSlot] ✅ Loaded sprite: Cards/skill.lan-tron

❌ Error logs:
[InventoryUI] User not logged in!
→ Fix: Đăng nhập trước

[InventoryService] Loaded 0 items from inventory
→ Fix: Tạo test data trong Firestore users/{uid}/inventory

[ItemSlot] ❌ Sprite not found: Equipment/equip.hat.basic
Check file: Assets/Resources/Equipment/equip.hat.basic.png
→ Fix: Kiểm tra file có tồn tại không, tên có đúng không

[InventoryService] ItemData not found for itemId: equip.hat.basic
→ Fix: Kiểm tra Firestore items collection có document này không
```

### Test 2: Verify sprites hiển thị
```
1. Play game
2. Login
3. Check PanelInventory:
   - Items có hiển thị sprites không?
   - Sprites có đúng không?
   
4. Nếu không thấy sprites:
   - Check Console logs
   - Check Firestore icon field
   - Check file sprites tồn tại
```

### Test 3: Drag & drop
```
1. Play game
2. Login
3. Drag item trong inventory
4. Drop vào loadout slot
5. Check Console logs:
   - "[LoadoutStats] Equipment X: HP+0 AGI+0 INT+0 LUCK+10 RES+0"
   - "[LoadoutStats] User Level 1 → Base Stats: HP:100 ..."
```

---

## 📋 Quick Fix Checklist

### Nếu không thấy items trong inventory:
- [ ] User đã đăng nhập chưa? (Check Console log)
- [ ] Firestore có data trong users/{uid}/inventory chưa?
- [ ] InventoryService đã được assign trong Unity Inspector chưa?
- [ ] FirebaseAuthService đã được assign chưa?

### Nếu không thấy sprites:
- [ ] Firestore items có field "icon" chưa?
- [ ] icon field có đúng format không? ("Equipment/equip.hat.basic")
- [ ] File sprites có tồn tại không? (Assets/Resources/Equipment/equip.hat.basic.png)
- [ ] File name có khớp với icon field không?

### Nếu stats không update:
- [ ] LoadoutStatsDisplay có được assign firebaseAuthService chưa?
- [ ] LoadoutStatsDisplay có được assign tất cả loadout slots chưa?
- [ ] Check Console log có "[LoadoutStats] ..." không?

---

## 🔧 Debug Commands

### Check current user:
```csharp
if (firebaseAuthService.Auth.CurrentUser != null)
{
    Debug.Log($"Current User: {firebaseAuthService.Auth.CurrentUser.Email}");
    Debug.Log($"User ID: {firebaseAuthService.Auth.CurrentUser.UserId}");
}
else
{
    Debug.Log("No user logged in");
}
```

### Force sign out:
```csharp
await firebaseAuthService.SignOutAsync();
Debug.Log("Signed out!");
```

### Check inventory service:
```csharp
var inventory = inventoryService.GetCachedInventory();
Debug.Log($"Cached inventory: {inventory.Count} items");

foreach (var item in inventory)
{
    Debug.Log($"Item: {item.itemData?.name}, Icon: {item.itemData?.icon}");
}
```

### Test sprite loading:
```csharp
string iconPath = "Equipment/equip.hat.basic";
Sprite sprite = Resources.Load<Sprite>(iconPath);

if (sprite != null)
{
    Debug.Log($"✅ Sprite loaded: {iconPath}");
}
else
{
    Debug.LogError($"❌ Sprite not found: {iconPath}");
}
```

---

## 📚 Summary

| Issue | Cause | Fix |
|-------|-------|-----|
| Tự động đăng nhập | Firebase session persistence | ✅ Đây là tính năng, không phải lỗi |
| Không thấy items | Chưa có data trong Firestore | Tạo test data trong users/{uid}/inventory |
| Không thấy sprites | icon field thiếu hoặc sai | Update Firestore icon field |
| Sprites not found | File name không khớp | Rename file hoặc update icon field |
| Stats không update | Chưa assign references | Assign trong Unity Inspector |

---

**Bạn đang gặp vấn đề nào? Hãy follow checklist trên để debug! 🚀**

