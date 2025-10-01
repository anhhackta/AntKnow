# 📦 Inventory & Loadout System - Summary

## ✅ Đã tạo hoàn chỉnh hệ thống Inventory & Loadout!

---

## 📁 Files đã tạo (7 files)

### 1. Data Classes (2 files)
```
✅ InventoryItem.cs          - Data class cho items/cards
✅ LoadoutData.cs            - Data class cho loadout
```

### 2. Services (1 file)
```
✅ InventoryService.cs       - Load/save inventory & loadout từ Firestore
```

### 3. UI Components (3 files)
```
✅ ItemSlot.cs               - Slot chứa item/card (với validation)
✅ DraggableItem.cs          - Drag & Drop component
✅ CardDisplay.cs            - Hiển thị thông tin card (level, stats, stars)
```

### 4. UI Manager (1 file)
```
✅ InventoryUIManager.cs     - UI Manager chính
```

### 5. Documentation (1 file)
```
✅ INVENTORY_SETUP_GUIDE.md  - Hướng dẫn setup chi tiết
```

---

## 🎯 Features

### ✅ Inventory System
- **15 slots** cho items (equipment, materials, exp cards)
- **8 slots** cho skill cards
- **Sort button** để sắp xếp items/cards
- **Drag & Drop** giữa các slots
- **Stackable items** (exp cards, materials) hiển thị quantity
- **Non-stackable items** (skill cards, equipment) hiển thị level

### ✅ Loadout System
- **5 equipment slots**: Hat, Shirt, Wings, Shoes, Mask
- **2 card slots**: Passive card, Active card
- **Drag & Drop** từ inventory vào loadout
- **Slot validation**: Chỉ chấp nhận đúng loại item
- **Auto save** loadout to Firestore

### ✅ Card Display
- **Card image** với rarity border (common/rare/epic/legendary)
- **Level** display
- **Primary stat** với calculation (base + level bonus)
- **Cooldown** với star reduction
- **Stars** display (0-5 stars)

### ✅ Drag & Drop
- **Visual feedback**: Semi-transparent khi drag
- **Slot validation**: Chỉ drop vào slot hợp lệ
- **Swap items**: Tự động swap nếu target slot có item
- **Return to original**: Nếu drop không hợp lệ

---

## 🗄️ Database Integration

### Firestore Collections:

#### `users/{uid}/inventory/{docId}`
```javascript
{
  type: "skill_card" | "equipment" | "exp_card" | "material",
  itemId: "skill.lan-tron",
  level: 1,
  stars: 0,
  qty: 10,  // For stackable items
  durability: 100,  // For equipment
  createdAt: Timestamp,
  updatedAt: Timestamp
}
```

#### `users/{uid}/loadouts/slot1`
```javascript
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

#### `items/{itemId}` (Catalog)
```javascript
{
  name: "Lá Bảo Kê",
  type: "skill_card",
  rarity: "rare",
  attributes: {
    primaryStat: "health",
    health: 10,
    attributePerLevel: 2
  },
  skill: {
    mode: "passive",
    effect: "Giảm 20% sát thương",
    cooldownBaseTurns: 3
  },
  icon: "path/to/icon.png"
}
```

---

## 🎨 UI Structure

### PanelInventory
```
PanelInventory
├── CharacterImage (gender-based)
├── ButtonShowItems / ButtonShowCards
├── PanelInventoryItem
│   ├── ItemSlotsContainer (GridLayout 5x3 = 15 slots)
│   └── ButtonSort
└── PanelInventoryCard
    ├── CardSlotsContainer (GridLayout 4x2 = 8 slots)
    └── ButtonSort
```

### PanelLoadout
```
PanelLoadout
├── LoadoutItems (5 equipment slots)
│   ├── HatSlot
│   ├── ShirtSlot
│   ├── WingsSlot
│   ├── ShoesSlot
│   └── MaskSlot
└── LoadoutCards (2 card slots)
    ├── PassiveCardSlot
    └── ActiveCardSlot
```

---

## 🔧 Setup Steps (Quick)

### 1. Tạo Prefabs (2 prefabs)
```
ItemSlotPrefab (80x80):
- Background Image
- Icon Image
- Quantity Text
- Level Text
- Empty Indicator

CardSlotPrefab (100x140):
- Card Image
- Rarity Border
- Card Name Text
- Level Text
- Primary Stat Text
- Cooldown Text
- Stars Container
```

### 2. Setup PanelInventory
```
1. Create PanelInventory GameObject
2. Add InventoryUIManager script
3. Create ItemSlotsContainer với GridLayoutGroup (5 columns)
4. Create CardSlotsContainer với GridLayoutGroup (4 columns)
5. Assign prefabs và references
```

### 3. Setup PanelLoadout
```
1. Create PanelLoadout GameObject
2. Create 5 equipment slots (manual)
3. Create 2 card slots (manual)
4. Assign references trong InventoryUIManager
```

### 4. Setup Services
```
1. Create InventoryService GameObject
2. Service tự động DontDestroyOnLoad
3. Connect với FirebaseAuthService
```

---

## 📊 Code Examples

### Load Inventory:
```csharp
string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
var inventory = await InventoryService.Instance.LoadInventoryAsync(uid);
```

### Save Loadout:
```csharp
LoadoutData loadout = new LoadoutData();
loadout.skillCardIds.Add("cardDocId1");
loadout.equipmentSet.hatId = "equipDocId1";
await InventoryService.Instance.SaveLoadoutAsync(uid, loadout);
```

### Get Skill Cards:
```csharp
var skillCards = InventoryService.Instance.GetSkillCards();
```

---

## 🎮 User Flow

### Inventory Flow:
```
1. User login → Auto load inventory
2. Click "Items" button → Show PanelInventoryItem (15 slots)
3. Click "Cards" button → Show PanelInventoryCard (8 slots)
4. Click "Sort" → Sort items/cards, move empty slots to end
5. Drag item → Drop to another slot → Swap items
```

### Loadout Flow:
```
1. Open PanelLoadout
2. Drag equipment from inventory → Drop to equipment slot
3. Drag card from inventory → Drop to card slot
4. Auto save loadout to Firestore
5. Loadout ready for game
```

---

## 📈 Stats Calculation

### Card Primary Stat:
```
effectiveValue = baseValue + (level - 1) * attributePerLevel

Example: Lá Bảo Kê Lv.5
- Base health: 10
- Attribute per level: 2
→ Effective health = 10 + (5-1)*2 = 18
```

### Card Cooldown:
```
effectiveCooldown = max(1, baseCooldown - cooldownReduction[stars])

Cooldown reduction: [0, 1, 2, 3, 4] (by stars)

Example: 3-star card
- Base cooldown: 5 turns
→ Effective cooldown = max(1, 5-3) = 2 turns
```

---

## 🎨 Slot Types & Validation

| Slot Type | Accepts | Validation |
|-----------|---------|------------|
| **InventoryItem** | All items | ✅ Always accept |
| **InventoryCard** | Skill cards only | ✅ Check `item.IsSkillCard` |
| **LoadoutEquipment** | Equipment of matching slot | ✅ Check `item.itemData.equipment.slot == slotType` |
| **LoadoutCard** | Skill cards only | ✅ Check `item.IsSkillCard` |

---

## 🐛 Common Issues

### Items không hiện:
```
✅ Check: users/{uid}/inventory có data không?
✅ Check: ItemSlot prefab có assign references không?
✅ Check: Console log "Loaded X items"
```

### Drag & Drop không hoạt động:
```
✅ Check: Canvas có GraphicRaycaster?
✅ Check: Scene có EventSystem?
✅ Check: DraggableItem có CanvasGroup?
✅ Check: Image có raycastTarget = true?
```

### Loadout không save:
```
✅ Check: Firestore rules cho phép write?
✅ Check: User đã login?
✅ Check: Console log "Saving loadout..."
```

---

## 📝 Next Steps

### Phase 1: Setup UI (1-2 giờ)
- [ ] Tạo ItemSlotPrefab
- [ ] Tạo CardSlotPrefab
- [ ] Setup PanelInventory
- [ ] Setup PanelLoadout
- [ ] Assign references

### Phase 2: Test với mock data (30 phút)
- [ ] Tạo test items trong Firestore
- [ ] Load inventory
- [ ] Test drag & drop
- [ ] Test sort

### Phase 3: Integration (1 giờ)
- [ ] Connect với PanelHome
- [ ] Add button mở inventory
- [ ] Test full flow
- [ ] Fix bugs

### Phase 4: Polish (1 giờ)
- [ ] Add sprites/icons
- [ ] Add animations
- [ ] Add sound effects
- [ ] Add tooltips

---

## 🎯 Key Features Summary

| Feature | Status | Description |
|---------|--------|-------------|
| **Inventory Items** | ✅ | 15 slots, stackable support |
| **Inventory Cards** | ✅ | 8 slots, level/stars display |
| **Loadout Equipment** | ✅ | 5 slots (hat, shirt, wings, shoes, mask) |
| **Loadout Cards** | ✅ | 2 slots (passive, active) |
| **Drag & Drop** | ✅ | Full support với validation |
| **Sort** | ✅ | Sort by type/rarity/level |
| **Firestore Integration** | ✅ | Load/save inventory & loadout |
| **Card Stats** | ✅ | Calculate primary stat & cooldown |
| **Slot Validation** | ✅ | Only accept valid items |

---

**Version**: 1.0
**Date**: 2025-10-01
**Status**: Ready for setup ✅

