# 📦 Inventory & Loadout System - Setup Guide

## 🎯 Tổng quan

Hệ thống Inventory và Loadout cho phép:
- Quản lý items (equipment, materials, exp cards)
- Quản lý skill cards
- Drag & Drop items/cards giữa các slots
- Loadout system để trang bị cho game
- Sort items/cards

---

## 📁 Cấu trúc Files

```
Assets/Scenes/Menu/Inventory/
├── Data Classes:
│   ├── InventoryItem.cs          # Data class cho items
│   ├── LoadoutData.cs            # Data class cho loadout
│
├── Services:
│   └── InventoryService.cs       # Service load/save inventory & loadout
│
├── UI Components:
│   ├── ItemSlot.cs               # Slot chứa item/card
│   ├── DraggableItem.cs          # Drag & Drop component
│   ├── CardDisplay.cs            # Hiển thị thông tin card
│   └── InventoryUIManager.cs     # UI Manager chính
│
└── INVENTORY_SETUP_GUIDE.md      # File này
```

---

## 🗄️ Database Structure

### Firestore Collections:

#### 1. `users/{uid}/inventory/{docId}`
```javascript
{
  type: "skill_card" | "exp_card" | "equipment" | "material" | "repair_hammer",
  itemId: "skill.lan-tron",  // Reference to items collection
  level: 1,                   // For non-stackable items
  stars: 0,                   // For skill cards
  qty: 10,                    // For stackable items
  durability: 100,            // For equipment
  createdAt: Timestamp,
  updatedAt: Timestamp,
  status: "active"
}
```

#### 2. `users/{uid}/loadouts/{slotId}`
```javascript
{
  active: true,
  skillCardIds: ["cardDocId1", "cardDocId2"],  // Max 2 cards
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

#### 3. `items/{itemId}` (Catalog)
```javascript
{
  name: "Lá Bảo Kê",
  type: "skill_card",
  rarity: "rare",
  status: "active",
  attributes: {
    primaryStat: "health",
    health: 10,
    attributePerLevel: 2
  },
  skill: {
    mode: "passive",
    effect: "Giảm 20% sát thương nhận vào",
    cooldownBaseTurns: 3
  },
  icon: "path/to/icon.png"
}
```

---

## 🎨 UI Hierarchy

### PanelInventory (Main Panel)
```
PanelInventory
├── CharacterImage (Image - gender-based sprite)
├── ButtonShowItems (Button)
├── ButtonShowCards (Button)
├── PanelInventoryItem (Sub-panel)
│   ├── ItemSlotsContainer (GridLayoutGroup)
│   │   └── ItemSlot x15 (Prefab instances)
│   └── ButtonSort (Button)
└── PanelInventoryCard (Sub-panel)
    ├── CardSlotsContainer (GridLayoutGroup)
    │   └── CardSlot x8 (Prefab instances)
    └── ButtonSort (Button)
```

### PanelLoadout (Separate Panel)
```
PanelLoadout
├── LoadoutItems (Equipment slots)
│   ├── HatSlot (ItemSlot)
│   ├── ShirtSlot (ItemSlot)
│   ├── WingsSlot (ItemSlot)
│   ├── ShoesSlot (ItemSlot)
│   └── MaskSlot (ItemSlot)
└── LoadoutCards (Card slots)
    ├── PassiveCardSlot (ItemSlot)
    └── ActiveCardSlot (ItemSlot)
```

---

## 🔧 Setup Steps

### Bước 1: Tạo Prefabs

#### A. ItemSlotPrefab (cho items)
```
1. Create Empty GameObject: "ItemSlotPrefab"
2. Add RectTransform (size: 80x80)
3. Add Image component (background)
4. Add ItemSlot script
5. Add children:
   - IconImage (Image)
   - QuantityText (TextMeshPro)
   - LevelText (TextMeshPro)
   - EmptyIndicator (Image/Text)
6. Assign references trong ItemSlot script
7. Save as Prefab
```

#### B. CardSlotPrefab (cho cards)
```
1. Create Empty GameObject: "CardSlotPrefab"
2. Add RectTransform (size: 100x140)
3. Add Image component (background)
4. Add ItemSlot script
5. Add CardDisplay script
6. Add children:
   - CardImage (Image)
   - RarityBorder (Image)
   - CardNameText (TextMeshPro)
   - LevelText (TextMeshPro)
   - PrimaryStatText (TextMeshPro)
   - CooldownText (TextMeshPro)
   - StarsContainer (HorizontalLayoutGroup)
     └── StarPrefab (Image - star icon)
7. Assign references
8. Save as Prefab
```

### Bước 2: Setup PanelInventory

```
1. Create Panel: "PanelInventory"
2. Add children:
   - CharacterImage (Image)
   - ButtonShowItems (Button)
   - ButtonShowCards (Button)
   - PanelInventoryItem
     └── ItemSlotsContainer (GridLayoutGroup)
   - PanelInventoryCard
     └── CardSlotsContainer (GridLayoutGroup)
3. Add InventoryUIManager script to PanelInventory
4. Assign all references
```

#### GridLayoutGroup Settings:
```
ItemSlotsContainer:
- Cell Size: 80x80
- Spacing: 10x10
- Constraint: Fixed Column Count = 5
- Child Alignment: Upper Left

CardSlotsContainer:
- Cell Size: 100x140
- Spacing: 10x10
- Constraint: Fixed Column Count = 4
- Child Alignment: Upper Left
```

### Bước 3: Setup PanelLoadout

```
1. Create Panel: "PanelLoadout"
2. Add LoadoutItems section:
   - Create 5 ItemSlot instances
   - Assign SlotType = LoadoutEquipment
   - Set equipmentSlot: "hat", "shirt", "wings", "shoes", "mask"
3. Add LoadoutCards section:
   - Create 2 ItemSlot instances
   - Assign SlotType = LoadoutCard
4. Assign references trong InventoryUIManager
```

### Bước 4: Setup Services

```
1. Create Empty GameObject: "InventoryService"
2. Add InventoryService script
3. Enable Debug Logs
4. Service sẽ tự động DontDestroyOnLoad
```

### Bước 5: Connect to FirebaseAuthService

```
1. Trong InventoryUIManager:
   - Assign FirebaseAuthService reference
   - Assign InventoryService reference
2. Service sẽ tự động load inventory khi user login
```

---

## 🎮 Usage

### Load Inventory:
```csharp
string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
var inventory = await InventoryService.Instance.LoadInventoryAsync(uid);
```

### Load Loadout:
```csharp
var loadout = await InventoryService.Instance.LoadLoadoutAsync(uid, "slot1");
```

### Save Loadout:
```csharp
LoadoutData loadout = new LoadoutData();
loadout.skillCardIds.Add("cardDocId1");
loadout.equipmentSet.hatId = "equipDocId1";
await InventoryService.Instance.SaveLoadoutAsync(uid, loadout);
```

### Get Items by Type:
```csharp
var skillCards = InventoryService.Instance.GetSkillCards();
var equipment = InventoryService.Instance.GetEquipment();
```

---

## 🎨 Drag & Drop Flow

```
1. User clicks and drags item
   ↓
2. DraggableItem.OnBeginDrag()
   - Save original position
   - Move to canvas root
   - Make semi-transparent
   ↓
3. DraggableItem.OnDrag()
   - Follow mouse/touch
   ↓
4. DraggableItem.OnEndDrag()
   - Raycast to find target slot
   - Check if target slot can accept item
   - If valid: Swap items
   - If invalid: Return to original position
```

---

## 🔄 Sort Logic

### Sort Items:
```
1. Collect all items from slots
2. Sort by: type → rarity
3. Refill slots (empty slots at end)
```

### Sort Cards:
```
1. Collect all cards from slots
2. Sort by: rarity → level (desc) → stars (desc)
3. Refill slots (empty slots at end)
```

---

## 📊 Card Stats Calculation

### Primary Stat:
```
effectiveValue = baseValue + (level - 1) * attributePerLevel

Example:
- Base health: 10
- Attribute per level: 2
- Level: 5
→ Effective health = 10 + (5-1)*2 = 18
```

### Cooldown:
```
effectiveCooldown = max(1, baseCooldown - cooldownReduction[stars])

Cooldown reduction by stars: [0, 1, 2, 3, 4]

Example:
- Base cooldown: 5 turns
- Stars: 3
→ Effective cooldown = max(1, 5-3) = 2 turns
```

---

## 🐛 Troubleshooting

### Items không hiện trong inventory:
```
1. Check Console log: "Loaded X items from inventory"
2. Check Firestore: users/{uid}/inventory có data không?
3. Check ItemSlot prefab có assign đúng references không?
```

### Drag & Drop không hoạt động:
```
1. Check Canvas có GraphicRaycaster component không?
2. Check EventSystem có trong scene không?
3. Check DraggableItem có CanvasGroup component không?
4. Check Image component có raycastTarget = true không?
```

### Loadout không save:
```
1. Check Console log: "Saving loadout..."
2. Check Firestore rules cho phép write không?
3. Check user đã login chưa?
```

---

## 📝 TODO

- [ ] Load sprites từ icon URL
- [ ] Implement full ItemData parsing (attributes, skill, equipment, etc.)
- [ ] Add animations cho drag & drop
- [ ] Add sound effects
- [ ] Add item tooltips
- [ ] Add confirmation dialog khi equip/unequip
- [ ] Add loadout validation (không cho 2 cards cùng itemId)
- [ ] Add equipment durability display
- [ ] Add card upgrade UI
- [ ] Add card evolution UI

---

**Version**: 1.0
**Date**: 2025-10-01

