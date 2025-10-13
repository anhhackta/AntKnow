# 🔧 HƯỚNG DẪN SỬA LỖI INVENTORY & LOADOUT KHÔNG HIỂN THỊ

## 📋 TÓM TẮT VẤN ĐỀ

**Triệu chứng:**
- Inventory không hiển thị items
- Loadout không hiển thị equipment và skill cards
- Icons không load được

**Nguyên nhân chính:**
- ❌ Firebase `items` collection chưa có data
- ❌ Inventory có `itemId` nhưng không tìm thấy document tương ứng trong `items/{itemId}`
- ❌ `GetItemDataAsync()` return null → `item.itemData = null` → Không load được icon

---

## ✅ GIẢI PHÁP: 3 BƯỚC ĐƠN GIẢN

### **BƯỚC 1: Tạo items collection trong Firebase** (5 phút)

#### **Option A: Dùng Unity Editor Tool (KHUYẾN NGHỊ)**

1. **Mở Unity Editor**
2. **Menu → AntKnow → Create Items in Firebase**
3. **Click "CREATE ALL ITEMS (10 items)"**
4. **Đợi 30 giây** → Xem Console logs
5. **Kiểm tra Firestore Console** → Collection `items` có 10 documents

**Items sẽ được tạo:**
```
items/
├── equip.hat.basic
├── equip.shirt.basic
├── equip.wings.basic
├── equip.shoes.basic
├── equip.mask.basic
├── skill.bao-ke
├── skill.cham-chi
├── skill.lan-tron
├── skill.sieu-sale
└── exp.small
```

#### **Option B: Tạo thủ công trong Firestore Console**

**Ví dụ: Tạo equip.wings.basic**

```
Firestore Console → items collection → Add Document

Document ID: equip.wings.basic

Fields:
├── name: "Wings Basic" (string)
├── type: "equipment" (string)
├── rarity: "common" (string)
├── status: "active" (string)
├── icon: "equip.wings.basic" (string)  ← QUAN TRỌNG!
├── attributes: (map)
│   ├── health: 0 (number)
│   ├── agility: 10 (number)
│   ├── intelligence: 0 (number)
│   ├── luck: 5 (number)
│   └── resistance: 0 (number)
└── equipment: (map)
    ├── slot: "wings" (string)
    └── durabilityMax: 100 (number)
```

**Lặp lại cho tất cả items trong inventory của bạn!**

---

### **BƯỚC 2: Test Load Inventory** (2 phút)

1. **Mở Unity Editor**
2. **Play game → Login**
3. **Mở Menu Scene (Inventory)**
4. **Tìm GameObject có InventoryUIManager component**
5. **Right-click component → Test Load Inventory**
6. **Xem Console logs:**

**Logs thành công:**
```
=== TEST LOAD INVENTORY ===
✅ User ID: abc123...
Loading inventory...
[InventoryService] Loading inventory for user: abc123...
[InventoryService] Found 5 documents in inventory collection
[InventoryService] Processing inventory doc: W6GUmqbcQnUKEvBhOikN
[InventoryService] Parsed item: itemId=equip.wings.basic, type=equipment
[InventoryService] ✅ Loaded itemData for equip.wings.basic, icon: equip.wings.basic, name: Wings Basic
[SpriteLoader] ✅ Loaded sprite from Items/: Items/equip.wings.basic
✅ Loaded 5 items
  - Item: equip.wings.basic, Type: equipment, DocId: W6GUmqbcQnUKEvBhOikN
    ✅ ItemData: Icon=equip.wings.basic, Name=Wings Basic
=== TEST COMPLETE ===
```

**Logs lỗi (nếu items collection chưa có):**
```
[InventoryService] ❌ Failed to load itemData for equip.wings.basic - Check if items/equip.wings.basic exists in Firestore!
    ❌ ItemData is NULL! Check if items/equip.wings.basic exists in Firestore!
```

**→ Nếu thấy lỗi này, quay lại BƯỚC 1!**

---

### **BƯỚC 3: Verify UI hiển thị** (1 phút)

1. **Play game → Login → Menu Scene**
2. **Kiểm tra:**
   - ✅ Inventory slots hiển thị items với icons
   - ✅ Loadout equipment slots hiển thị equipment
   - ✅ Loadout card slots hiển thị skill cards
   - ✅ Stats hiển thị đúng

**Nếu vẫn không hiển thị:**
- Kiểm tra GameObject có InventoryUIManager active không
- Kiểm tra Canvas có active không
- Kiểm tra ItemSlot prefabs có đúng không

---

## 🐛 DEBUG CHECKLIST

### **1. Kiểm tra Firebase items collection**
```
Firestore Console → items collection
- [ ] Collection "items" tồn tại
- [ ] Có ít nhất 10 documents
- [ ] Mỗi document có field "icon"
- [ ] Mỗi document có field "name", "type", "rarity"
```

### **2. Kiểm tra Resources folder**
```
Assets/Resources/Items/
- [ ] equip.hat.basic.png
- [ ] equip.shirt.basic.png
- [ ] equip.wings.basic.png
- [ ] equip.shoes.basic.png
- [ ] equip.mask.basic.png
- [ ] skill.bao-ke.png
- [ ] skill.cham-chi.png
- [ ] skill.lan-tron.png
- [ ] skill.sieu-sale.png
- [ ] exp.small.png
```

### **3. Kiểm tra Console logs**
```
Play game → Login → Menu Scene → Xem Console

Logs cần có:
- [InventoryUI] Start() called
- [InventoryService] Loading inventory for user: ...
- [InventoryService] Found X documents in inventory collection
- [InventoryService] ✅ Loaded itemData for ...
- [SpriteLoader] ✅ Loaded sprite from Items/: ...
```

### **4. Kiểm tra Scene setup**
```
Menu Scene:
- [ ] GameObject có InventoryUIManager component
- [ ] InventoryUIManager.inventoryService assigned
- [ ] InventoryUIManager.firebaseAuthService assigned
- [ ] ItemSlot prefabs assigned
- [ ] Loadout slots assigned (hatSlot, shirtSlot, etc.)
```

---

## 🚨 LỖI THƯỜNG GẶP

### **Lỗi 1: ItemData is NULL**
**Nguyên nhân:** Firebase `items/{itemId}` không tồn tại

**Giải pháp:**
1. Mở Firestore Console
2. Kiểm tra collection `items`
3. Tạo document với ID = itemId (ví dụ: `equip.wings.basic`)
4. Hoặc dùng Unity Editor Tool: **Menu → AntKnow → Create Items in Firebase**

---

### **Lỗi 2: Sprite not found**
**Nguyên nhân:** File sprite không có trong Resources/Items/

**Giải pháp:**
1. Kiểm tra file `Assets/Resources/Items/{itemId}.png` có tồn tại không
2. Kiểm tra tên file khớp với `icon` field trong Firestore
3. Ví dụ: `icon: "equip.wings.basic"` → File: `equip.wings.basic.png`

---

### **Lỗi 3: User not logged in**
**Nguyên nhân:** FirebaseAuthService chưa login

**Giải pháp:**
1. Chạy game từ LoginScene
2. Đăng nhập trước khi vào Menu Scene
3. Kiểm tra `firebaseAuthService.Auth.CurrentUser != null`

---

### **Lỗi 4: Inventory không load**
**Nguyên nhân:** InventoryUIManager.Start() không được gọi

**Giải pháp:**
1. Kiểm tra GameObject có InventoryUIManager active
2. Kiểm tra component enabled
3. Xem Console có log "[InventoryUI] Start() called" không

---

## 📊 KIỂM TRA CUỐI CÙNG

### **Test Flow hoàn chỉnh:**

```
1. Unity Editor Tool
   ↓
   Menu → AntKnow → Create Items in Firebase
   ↓
   Click "CREATE ALL ITEMS"
   ↓
   ✅ Console: "✅ Created item: equip.wings.basic" (x10)

2. Firestore Console
   ↓
   Mở collection "items"
   ↓
   ✅ Thấy 10 documents

3. Unity Play Mode
   ↓
   Login → Menu Scene
   ↓
   Right-click InventoryUIManager → Test Load Inventory
   ↓
   ✅ Console: "✅ Loaded itemData for ..."
   ✅ Console: "[SpriteLoader] ✅ Loaded sprite from Items/: ..."

4. UI Display
   ↓
   Xem Inventory panel
   ↓
   ✅ Items hiển thị với icons
   ✅ Loadout hiển thị equipment
   ✅ Stats hiển thị đúng
```

---

## 🎯 KẾT QUẢ MONG ĐỢI

**Sau khi hoàn thành 3 bước:**

✅ **Inventory hiển thị đầy đủ items**
- Items có icons
- Items có tên
- Items có rarity color

✅ **Loadout hiển thị đầy đủ equipment**
- 5 equipment slots có items
- Icons load đúng
- Có thể drag & drop

✅ **Loadout hiển thị skill cards**
- 2 card slots có cards
- Icons load đúng
- Có thể drag & drop

✅ **Stats tính toán đúng**
- Base stats từ level
- Equipment stats cộng thêm
- Skill card stats cộng thêm

---

## 🚀 TIẾP THEO

**Sau khi inventory & loadout hiển thị OK:**

1. **Test Drag & Drop** (DraggableItem đã có sẵn)
   - Kéo item từ inventory vào loadout
   - Kéo equipment vào đúng slot (hat → hat slot)
   - Kéo skill card vào card slot

2. **Implement PrepareGameSession()**
   - Transfer loadout data vào GameSessionData
   - Extract effectId từ skill cards
   - Calculate total stats

3. **Implement Shop System**
   - Create ShopUIManager
   - Purchase items/cards
   - Deduct currency

4. **Test Full Flow**
   - Login → Menu → Shop → Buy → Inventory → Loadout → Game

---

## 📞 CẦN TRỢ GIÚP?

**Nếu vẫn gặp lỗi sau khi làm theo 3 bước:**

1. **Chụp screenshot Console logs**
2. **Chụp screenshot Firestore items collection**
3. **Chụp screenshot Resources/Items folder**
4. **Gửi cho tôi để debug**

---

## ✅ CHECKLIST HOÀN THÀNH

- [ ] **BƯỚC 1:** Tạo items collection (10 items)
- [ ] **BƯỚC 2:** Test Load Inventory (Console logs OK)
- [ ] **BƯỚC 3:** Verify UI hiển thị (Items + Loadout OK)
- [ ] **BONUS:** Test Drag & Drop
- [ ] **BONUS:** Test Stats calculation

**Sau khi hoàn thành checklist → Inventory & Loadout hoạt động 100%!** 🎉

