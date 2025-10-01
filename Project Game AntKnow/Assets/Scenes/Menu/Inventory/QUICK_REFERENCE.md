# 📦 Inventory System - Quick Reference

## 🎯 3 Prefabs + 1 Panel cần tạo

| Prefab/Panel | Size | Script | Dùng cho |
|--------------|------|--------|----------|
| **ItemSlotPrefab** | 80x80 | ItemSlot | Items (equipment, materials, exp cards) |
| **CardSlotPrefab** | 100x140 | CardSlot + Button | Skill cards (click để phóng to) |
| **StarPrefab** | 16x16 | - | Hiển thị số sao trên card |
| **CardZoomPanel** | Full screen | CardZoomDisplay | Phóng to card khi click |

---

## 📊 SlotType & EquipmentSlot

### Khi nào điền gì?

| Vị trí | slotType | equipmentSlot |
|--------|----------|---------------|
| **PanelInventoryItem** (15 slots) | `InventoryItem` | `""` (trống) |
| **PanelInventoryCard** (8 slots) | `InventoryCard` | `""` (trống) |
| **HatSlot** (loadout) | `LoadoutEquipment` | `"hat"` |
| **ShirtSlot** (loadout) | `LoadoutEquipment` | `"shirt"` |
| **WingsSlot** (loadout) | `LoadoutEquipment` | `"wings"` |
| **ShoesSlot** (loadout) | `LoadoutEquipment` | `"shoes"` |
| **MaskSlot** (loadout) | `LoadoutEquipment` | `"mask"` |
| **PassiveCardSlot** (loadout) | `LoadoutCard` | `""` (trống) |
| **ActiveCardSlot** (loadout) | `LoadoutCard` | `""` (trống) |

### Quy tắc đơn giản:

```
slotType: TẤT CẢ slots đều phải có
equipmentSlot: CHỈ 5 slots equipment trong loadout mới cần
```

---

## 🔍 Card Zoom Feature

### Click vào card → Phóng to

```
User click vào CardSlot
    ↓
CardSlot.OnCardClicked()
    ↓
CardZoomDisplay.ShowCard(item)
    ↓
CardZoomPanel hiển thị (full screen)
    ↓
User xem thông tin chi tiết
    ↓
User click ngoài card / ESC / Close button
    ↓
CardZoomDisplay.HideCard()
```

### Thông tin hiển thị:

```
✅ Card image (lớn hơn)
✅ Card name
✅ Card description (effect)
✅ Level
✅ Primary stat (với calculation)
✅ Cooldown (với star reduction)
✅ Mode (Passive/Active)
✅ Stars
```

---

## 🖼️ Image Management

### Phương án: Unity Resources Folder

```
Assets/Resources/
├── Cards/              # Skill card images
│   ├── skill.lan-tron.png
│   ├── skill.bao-ke.png
│   └── ...
├── Equipment/          # Equipment images
│   ├── hat.mao-len.png
│   ├── shirt.ao-giap.png
│   └── ...
└── Items/              # Other items
    ├── exp.small.png
    ├── material.go.png
    └── ...
```

### Database:

```javascript
// Firestore: items/skill.lan-tron
{
  itemId: "skill.lan-tron",
  icon: "Cards/skill.lan-tron",  // Path trong Resources
  // ...
}
```

### Load sprite:

```csharp
Sprite sprite = Resources.Load<Sprite>("Cards/skill.lan-tron");
image.sprite = sprite;
```

**Xem IMAGE_MANAGEMENT.md để biết chi tiết!**

---

## ⭐ Số sao & Cooldown

### Công thức:

```
effectiveCooldown = max(1, baseCooldown - stars)
```

### Ví dụ:

```
Card có base cooldown = 5 turns

0 stars: 5 - 0 = 5 turns
1 star:  5 - 1 = 4 turns
2 stars: 5 - 2 = 3 turns
3 stars: 5 - 3 = 2 turns
4 stars: 5 - 4 = 1 turn
5 stars: 5 - 5 = 1 turn (min = 1)
```

### Hiển thị:

```
StarsContainer (HorizontalLayoutGroup)
└── Spawn StarPrefab x (số sao)

Visual:
★★★☆☆ (3 stars)
★★★★★ (5 stars)
```

---

## 📦 Item Types

| Type | Level | Stars | Quantity | Panel | Loadout |
|------|-------|-------|----------|-------|---------|
| **Skill Card** | ✅ | ✅ | ❌ | InventoryCard (8 slots) | ✅ 2 slots |
| **EXP Card** | ❌ | ❌ | ✅ | InventoryItem (15 slots) | ❌ |
| **Equipment** | ❌ | ❌ | ❌ | InventoryItem (15 slots) | ✅ 5 slots |
| **Material** | ❌ | ❌ | ✅ | InventoryItem (15 slots) | ❌ |

### Quy tắc:

```
Skill Card:
- Có level, stars
- Hiển thị trong PanelInventoryCard (8 slots)
- Có thể thêm vào loadout (2 slots: passive + active)

EXP Card:
- Có quantity (stackable)
- Hiển thị trong PanelInventoryItem (15 slots)
- KHÔNG thể thêm vào loadout

Equipment:
- Không có level, stars, quantity
- Hiển thị trong PanelInventoryItem (15 slots)
- Có thể thêm vào loadout (5 slots: hat, shirt, wings, shoes, mask)

Material:
- Có quantity (stackable)
- Hiển thị trong PanelInventoryItem (15 slots)
- KHÔNG thể thêm vào loadout
```

---

## 🔧 Scripts

### ItemSlot.cs
```
Dùng cho:
- PanelInventoryItem (15 slots)
- PanelLoadout Equipment (5 slots)

Chức năng:
- Quản lý UI slot
- Validation (CanAcceptItem)
- Event OnItemChanged
```

### CardSlot.cs
```
Kế thừa: ItemSlot

Dùng cho:
- PanelInventoryCard (8 slots)
- PanelLoadout Cards (2 slots)

Chức năng:
- Tất cả chức năng của ItemSlot
- Hiển thị card info (level, stats, cooldown, stars)
- Click để phóng to card
- Load sprite từ Resources
```

### CardZoomDisplay.cs
```
Component cho CardZoomPanel

Chức năng:
- Hiển thị card phóng to
- Show/Hide panel
- ESC / Click ngoài / Close button để đóng
- Hiển thị thông tin chi tiết card
```

### DraggableItem.cs
```
Tự động tạo bởi ItemSlot

Chức năng:
- Drag & Drop
- Visual feedback
- Swap items
```

### InventoryService.cs
```
Singleton service

Chức năng:
- Load inventory từ Firestore
- Load/save loadout
- Cache data
- Events
```

### InventoryUIManager.cs
```
UI Manager chính

Chức năng:
- Tạo slots từ prefabs
- Switch panels (items/cards)
- Sort items/cards
- Refresh display
- Auto save loadout
```

---

## 💾 Khi nào lưu Loadout?

### Auto save khi:

```
✅ User drag equipment vào loadout slot
✅ User drag equipment ra khỏi loadout slot
✅ User drag card vào loadout slot
✅ User drag card ra khỏi loadout slot
✅ User swap items trong loadout
```

### Lưu vào đâu?

```
Firestore: users/{uid}/loadouts/slot1

{
  active: true,
  skillCardIds: ["cardDocId1", "cardDocId2"],
  equipmentSet: {
    hatId: "equipDocId1",
    shirtId: "equipDocId2",
    wingsId: "equipDocId3",
    shoesId: "equipDocId4",
    maskId: "equipDocId5"
  },
  updatedAt: Timestamp
}
```

---

## 🎨 EmptyIndicator

### Option 1: Text (Đơn giản)
```
TextMeshPro component
├── Text: "+"
├── Color: Gray (0.5, 0.5, 0.5, 0.5)
├── Font Size: 24
└── Alignment: Center
```

### Option 2: Image
```
Image component
├── Sprite: Unity default "Knob"
├── Color: Gray (0.5, 0.5, 0.5, 0.5)
└── Type: Simple
```

**Kết luận**: Dùng Text "+" cho đơn giản!

---

## ✅ Checklist Setup

### Prefabs:
- [ ] ItemSlotPrefab (80x80)
  - [ ] Background, IconImage, QuantityText, EmptyIndicator
  - [ ] ItemSlot script với references
- [ ] CardSlotPrefab (100x140)
  - [ ] Background, CardButton, CardImage, Texts, StarsContainer
  - [ ] CardSlot script với references
- [ ] StarPrefab (16x16)
  - [ ] Image với star sprite

### CardZoomPanel:
- [ ] CardZoomPanel (full screen)
  - [ ] Background button (semi-transparent black)
  - [ ] CardContainer với card info
  - [ ] Close button
  - [ ] CardZoomDisplay script
- [ ] Assign vào tất cả CardSlots

### PanelInventory:
- [ ] CharacterImage
- [ ] ButtonShowItems / ButtonShowCards
- [ ] PanelInventoryItem
  - [ ] ItemSlotsContainer với GridLayout (5 columns)
  - [ ] ButtonSort
- [ ] PanelInventoryCard
  - [ ] CardSlotsContainer với GridLayout (4 columns)
  - [ ] ButtonSort
- [ ] InventoryUIManager script

### PanelLoadout:
- [ ] LoadoutEquipment
  - [ ] HatSlot (slotType=LoadoutEquipment, equipmentSlot="hat")
  - [ ] ShirtSlot (slotType=LoadoutEquipment, equipmentSlot="shirt")
  - [ ] WingsSlot (slotType=LoadoutEquipment, equipmentSlot="wings")
  - [ ] ShoesSlot (slotType=LoadoutEquipment, equipmentSlot="shoes")
  - [ ] MaskSlot (slotType=LoadoutEquipment, equipmentSlot="mask")
- [ ] LoadoutCards
  - [ ] PassiveCardSlot (slotType=LoadoutCard)
  - [ ] ActiveCardSlot (slotType=LoadoutCard)

### Services:
- [ ] InventoryService GameObject
- [ ] Assign vào InventoryUIManager

---

## 🚀 Thứ tự triển khai

1. **Tạo StarPrefab** (2 phút)
2. **Tạo ItemSlotPrefab** (10 phút)
3. **Tạo CardSlotPrefab** (15 phút)
4. **Tạo CardZoomPanel** (10 phút)
5. **Setup PanelInventory** (20 phút)
6. **Setup PanelLoadout** (15 phút)
7. **Setup Services** (5 phút)
8. **Setup Resources folder + placeholder sprites** (15 phút)
9. **Test** (30 phút)

**Total: ~2 giờ**

---

**Xem SETUP_SIMPLE.md để biết chi tiết từng bước!**

