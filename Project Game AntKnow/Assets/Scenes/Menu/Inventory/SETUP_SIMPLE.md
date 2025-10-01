# 📦 Inventory Setup - Hướng dẫn đơn giản

## 🎯 Tóm tắt nhanh

### 3 loại Prefabs:
1. **ItemSlotPrefab** - Cho items (equipment, materials, exp cards)
2. **CardSlotPrefab** - Cho skill cards
3. **StarPrefab** - Cho hiển thị số sao trên card

---

## 📋 Bước 1: Tạo ItemSlotPrefab (10 phút)

### Hierarchy:
```
ItemSlotPrefab (RectTransform 80x80)
├── Background (Image)
│   └── Color: Gray (0.5, 0.5, 0.5, 0.5)
├── IconImage (Image) - HIDDEN
│   └── Sprite: None (sẽ load từ Firestore)
├── QuantityText (TextMeshPro) - HIDDEN
│   └── Text: "x10"
│   └── Anchor: Bottom Right
└── EmptyIndicator (TextMeshPro)
    └── Text: "+"
    └── Color: Gray
    └── Font Size: 24
    └── Alignment: Center
```

### Script Setup:
```
Add Component: ItemSlot
├── UI References:
│   ├── iconImage → IconImage
│   ├── backgroundImage → Background
│   ├── quantityText → QuantityText
│   └── emptyIndicator → EmptyIndicator
├── Settings:
│   ├── slotType → InventoryItem
│   └── equipmentSlot → "" (để trống)
└── Colors:
    ├── emptyColor → (0.5, 0.5, 0.5, 0.5)
    ├── filledColor → (1, 1, 1, 1)
    └── highlightColor → (1, 1, 0, 1)
```

**Save as Prefab**: `ItemSlotPrefab.prefab`

---

## 📋 Bước 2: Tạo StarPrefab (2 phút)

### Hierarchy:
```
StarPrefab (RectTransform 16x16)
└── Image
    └── Sprite: Unity default "Knob" (hoặc star icon)
    └── Color: Yellow (1, 1, 0, 1)
```

**Save as Prefab**: `StarPrefab.prefab`

---

## 📋 Bước 3: Tạo CardSlotPrefab (15 phút)

### Hierarchy:
```
CardSlotPrefab (RectTransform 100x140)
├── Background (Image)
│   └── Color: Gray (0.5, 0.5, 0.5, 0.5)
├── CardButton (Button) ← Click để phóng to card
│   └── Transition: None
├── CardImage (Image)
│   └── Sprite: None (sẽ load từ Resources)
├── CardNameText (TextMeshPro)
│   └── Text: "Card Name"
│   └── Anchor: Top
│   └── Font Size: 12
├── LevelText (TextMeshPro)
│   └── Text: "Lv.1"
│   └── Anchor: Top Right
│   └── Font Size: 10
├── PrimaryStatText (TextMeshPro)
│   └── Text: "HP: 10"
│   └── Anchor: Middle
│   └── Font Size: 10
├── CooldownText (TextMeshPro)
│   └── Text: "CD: 3"
│   └── Anchor: Bottom Left
│   └── Font Size: 10
└── StarsContainer (RectTransform)
    └── Add Component: HorizontalLayoutGroup
        └── Spacing: 2
        └── Child Alignment: Middle Center
        └── Anchor: Bottom
```

### Script Setup:
```
Add Component: CardSlot (kế thừa ItemSlot)
├── UI References (ItemSlot):
│   ├── iconImage → CardImage
│   ├── backgroundImage → Background
│   ├── quantityText → None (cards không có quantity)
│   └── emptyIndicator → None (hoặc tạo text "Empty")
├── Settings (ItemSlot):
│   ├── slotType → InventoryCard
│   └── equipmentSlot → "" (để trống)
├── Colors (ItemSlot):
│   ├── emptyColor → (0.5, 0.5, 0.5, 0.5)
│   ├── filledColor → (1, 1, 1, 1)
│   └── highlightColor → (1, 1, 0, 1)
└── Card Display (CardSlot):
    ├── cardImage → CardImage
    ├── cardNameText → CardNameText
    ├── levelText → LevelText
    ├── primaryStatText → PrimaryStatText
    ├── cooldownText → CooldownText
    ├── starsContainer → StarsContainer
    ├── starPrefab → StarPrefab (drag prefab vào)
    ├── cardZoomPanel → CardZoomPanel (assign sau khi tạo)
    └── cardButton → CardButton
```

**Save as Prefab**: `CardSlotPrefab.prefab`

---

## 📋 Bước 4: Setup PanelInventory (20 phút)

### Hierarchy:
```
PanelInventory
├── CharacterImage (Image)
│   └── Sprite: Male/Female sprite
├── ButtonShowItems (Button)
│   └── Text: "Items"
├── ButtonShowCards (Button)
│   └── Text: "Cards"
├── PanelInventoryItem (Panel)
│   ├── ItemSlotsContainer (Empty GameObject)
│   │   └── Add Component: GridLayoutGroup
│   │       ├── Cell Size: (80, 80)
│   │       ├── Spacing: (10, 10)
│   │       ├── Constraint: Fixed Column Count = 5
│   │       └── Child Alignment: Upper Left
│   └── ButtonSort (Button)
│       └── Text: "Sort"
└── PanelInventoryCard (Panel) - HIDDEN by default
    ├── CardSlotsContainer (Empty GameObject)
    │   └── Add Component: GridLayoutGroup
    │       ├── Cell Size: (100, 140)
    │       ├── Spacing: (10, 10)
    │       ├── Constraint: Fixed Column Count = 4
    │       └── Child Alignment: Upper Left
    └── ButtonSort (Button)
        └── Text: "Sort"
```

### Script Setup:
```
Add Component: InventoryUIManager
├── Main Panels:
│   ├── panelInventory → PanelInventory
│   └── panelLoadout → (assign sau)
├── Character Display:
│   ├── characterImage → CharacterImage
│   ├── maleSprite → (assign sprite)
│   └── femaleSprite → (assign sprite)
├── Inventory Sub-Panels:
│   ├── panelInventoryItem → PanelInventoryItem
│   ├── panelInventoryCard → PanelInventoryCard
│   ├── buttonShowItems → ButtonShowItems
│   ├── buttonShowCards → ButtonShowCards
│   ├── buttonSortItems → PanelInventoryItem > ButtonSort
│   └── buttonSortCards → PanelInventoryCard > ButtonSort
├── Inventory Item Slots:
│   ├── itemSlotsContainer → ItemSlotsContainer
│   ├── itemSlotPrefab → ItemSlotPrefab
│   └── maxItemSlots → 15
└── Inventory Card Slots:
    ├── cardSlotsContainer → CardSlotsContainer
    ├── cardSlotPrefab → CardSlotPrefab
    └── maxCardSlots → 8
```

---

## 📋 Bước 5: Setup PanelLoadout (20 phút)

### Hierarchy:
```
PanelLoadout
├── LoadoutEquipment (Panel)
│   ├── HatSlot (ItemSlotPrefab instance)
│   ├── ShirtSlot (ItemSlotPrefab instance)
│   ├── WingsSlot (ItemSlotPrefab instance)
│   ├── ShoesSlot (ItemSlotPrefab instance)
│   └── MaskSlot (ItemSlotPrefab instance)
└── LoadoutCards (Panel)
    ├── PassiveCardSlot (CardSlotPrefab instance)
    └── ActiveCardSlot (CardSlotPrefab instance)
```

### Script Setup cho từng slot:

#### HatSlot:
```
ItemSlot component:
├── slotType → LoadoutEquipment
└── equipmentSlot → "hat"
```

#### ShirtSlot:
```
ItemSlot component:
├── slotType → LoadoutEquipment
└── equipmentSlot → "shirt"
```

#### WingsSlot:
```
ItemSlot component:
├── slotType → LoadoutEquipment
└── equipmentSlot → "wings"
```

#### ShoesSlot:
```
ItemSlot component:
├── slotType → LoadoutEquipment
└── equipmentSlot → "shoes"
```

#### MaskSlot:
```
ItemSlot component:
├── slotType → LoadoutEquipment
└── equipmentSlot → "mask"
```

#### PassiveCardSlot & ActiveCardSlot:
```
CardSlot component:
├── slotType → LoadoutCard
└── equipmentSlot → "" (để trống)
```

### Assign vào InventoryUIManager:
```
InventoryUIManager:
├── Loadout Equipment Slots:
│   ├── hatSlot → HatSlot
│   ├── shirtSlot → ShirtSlot
│   ├── wingsSlot → WingsSlot
│   ├── shoesSlot → ShoesSlot
│   └── maskSlot → MaskSlot
└── Loadout Card Slots:
    ├── passiveCardSlot → PassiveCardSlot
    └── activeCardSlot → ActiveCardSlot
```

### Setup LoadoutStatsDisplay: ← MỚI!
```
1. Create Panel: "StatsDisplay" trong PanelLoadout
2. Add TextMeshPro components:
   - HealthText: "HP: 100"
   - AgilityText: "Agility: 10"
   - IntelligenceText: "Intelligence: 10"
   - LuckText: "Luck: 10"
   - ResistanceText: "Resistance: 10"
3. Add LoadoutStatsDisplay script
4. Assign references:
   - Stats Text: healthText, agilityText, etc.
   - Base Stats: baseHealth=100, baseAgility=10, etc.
   - References: hatSlot, shirtSlot, wingsSlot, shoesSlot, maskSlot, passiveCardSlot, activeCardSlot
```

---

## 📋 Bước 6: Setup Services (5 phút)

```
1. Create Empty GameObject: "InventoryService"
2. Add Component: InventoryService
3. Enable Debug Logs: ✓
```

### Assign vào InventoryUIManager:
```
InventoryUIManager:
├── Services:
│   ├── inventoryService → InventoryService
│   └── firebaseAuthService → FirebaseAuthService (existing)
```

---

## ✅ Checklist

### Prefabs:
- [ ] ItemSlotPrefab (80x80) với ItemSlot script
- [ ] CardSlotPrefab (100x140) với CardSlot script
- [ ] StarPrefab (16x16) với Image

### PanelInventory:
- [ ] CharacterImage
- [ ] ButtonShowItems / ButtonShowCards
- [ ] PanelInventoryItem với GridLayout (5 columns)
- [ ] PanelInventoryCard với GridLayout (4 columns)
- [ ] InventoryUIManager script với tất cả references

### PanelLoadout:
- [ ] 5 equipment slots (hat, shirt, wings, shoes, mask)
- [ ] 2 card slots (passive, active)
- [ ] StatsDisplay panel với LoadoutStatsDisplay script ← MỚI!
- [ ] Assign slotType và equipmentSlot đúng

### Services:
- [ ] InventoryService GameObject
- [ ] Assign vào InventoryUIManager

---

## 🎯 Giải thích quan trọng

### 1. SlotType vs EquipmentSlot

| Slot | slotType | equipmentSlot |
|------|----------|---------------|
| PanelInventoryItem (15 slots) | `InventoryItem` | `""` (trống) |
| PanelInventoryCard (8 slots) | `InventoryCard` | `""` (trống) |
| HatSlot | `LoadoutEquipment` | `"hat"` |
| ShirtSlot | `LoadoutEquipment` | `"shirt"` |
| WingsSlot | `LoadoutEquipment` | `"wings"` |
| ShoesSlot | `LoadoutEquipment` | `"shoes"` |
| MaskSlot | `LoadoutEquipment` | `"mask"` |
| PassiveCardSlot | `LoadoutCard` | `""` (trống) |
| ActiveCardSlot | `LoadoutCard` | `""` (trống) |

### 2. RarityBorder = Outline Component

**KHÔNG cần tạo thêm Image!**

Dùng **Outline component** của Unity UI:
```
CardImage (Image)
└── Add Component: Outline
    └── Effect Color: Đổi màu theo rarity
```

### 3. Số sao & Cooldown

```
Cooldown = max(1, baseCooldown - stars)

Ví dụ: Card có base cooldown = 5
- 0 stars: 5 - 0 = 5 turns
- 3 stars: 5 - 3 = 2 turns
- 5 stars: 5 - 5 = 1 turn (min)
```

### 4. Item Types

| Type | Level | Stars | Quantity | Panel |
|------|-------|-------|----------|-------|
| Skill Card | ✅ | ✅ | ❌ | InventoryCard (8 slots) |
| EXP Card | ❌ | ❌ | ✅ | InventoryItem (15 slots) |
| Equipment | ❌ | ❌ | ❌ | InventoryItem (15 slots) |
| Material | ❌ | ❌ | ✅ | InventoryItem (15 slots) |

---

**Total time: ~1 giờ**

Bây giờ bạn có thể bắt đầu tạo prefabs! 🚀

