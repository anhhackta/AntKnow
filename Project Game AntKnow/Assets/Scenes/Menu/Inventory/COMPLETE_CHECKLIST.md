# ✅ Inventory & Loadout System - Complete Checklist

## 🎯 Checklist đầy đủ từ đầu đến cuối

---

## 📋 Phase 1: Tạo Prefabs (30 phút)

### 1.1 StarPrefab (2 phút)
- [ ] Create GameObject: "StarPrefab"
- [ ] Add RectTransform (16x16)
- [ ] Add Image component
- [ ] Assign sprite: Unity default "Knob" hoặc star icon
- [ ] Set color: Yellow (1, 1, 0, 1)
- [ ] Save as Prefab: `Assets/Scenes/Menu/Inventory/Prefabs/StarPrefab.prefab`

### 1.2 ItemSlotPrefab (10 phút)
- [ ] Create GameObject: "ItemSlotPrefab"
- [ ] Add RectTransform (80x80)
- [ ] Add children:
  - [ ] Background (Image) - Color: Gray (0.5, 0.5, 0.5, 0.5)
  - [ ] IconImage (Image) - Hidden by default
  - [ ] QuantityText (TextMeshPro) - Hidden by default, Anchor: Bottom Right
  - [ ] EmptyIndicator (TextMeshPro) - Text: "+", Color: Gray, Font Size: 24, Alignment: Center
- [ ] Add ItemSlot script
- [ ] Assign references:
  - [ ] iconImage → IconImage
  - [ ] backgroundImage → Background
  - [ ] quantityText → QuantityText
  - [ ] emptyIndicator → EmptyIndicator
- [ ] Settings:
  - [ ] slotType → InventoryItem
  - [ ] equipmentSlot → "" (trống)
- [ ] Colors:
  - [ ] emptyColor → (0.5, 0.5, 0.5, 0.5)
  - [ ] filledColor → (1, 1, 1, 1)
  - [ ] highlightColor → (1, 1, 0, 1)
- [ ] Save as Prefab: `Assets/Scenes/Menu/Inventory/Prefabs/ItemSlotPrefab.prefab`

### 1.3 CardSlotPrefab (15 phút)
- [ ] Create GameObject: "CardSlotPrefab"
- [ ] Add RectTransform (100x140)
- [ ] Add children:
  - [ ] Background (Image) - Color: Gray (0.5, 0.5, 0.5, 0.5)
  - [ ] CardImage (Image) - Sprite: None
  - [ ] LevelText (TextMeshPro) - Text: "Lv.1", Anchor: Top Right, Font Size: 10
  - [ ] PrimaryStatText (TextMeshPro) - Text: "HP: 10", Anchor: Middle, Font Size: 10
  - [ ] CooldownText (TextMeshPro) - Text: "CD: 3", Anchor: Bottom Left, Font Size: 10
  - [ ] StarsContainer (Empty GameObject) - Anchor: Bottom, Add HorizontalLayoutGroup (Spacing: 2)
- [ ] Add CardSlot script (kế thừa ItemSlot)
- [ ] Assign references (ItemSlot):
  - [ ] iconImage → CardImage
  - [ ] backgroundImage → Background
  - [ ] quantityText → None
  - [ ] emptyIndicator → None
- [ ] Settings (ItemSlot):
  - [ ] slotType → InventoryCard
  - [ ] equipmentSlot → "" (trống)
- [ ] Colors (ItemSlot):
  - [ ] emptyColor → (0.5, 0.5, 0.5, 0.5)
  - [ ] filledColor → (1, 1, 1, 1)
  - [ ] highlightColor → (1, 1, 0, 1)
- [ ] Assign references (CardSlot):
  - [ ] cardImage → CardImage
  - [ ] levelText → LevelText
  - [ ] primaryStatText → PrimaryStatText
  - [ ] cooldownText → CooldownText
  - [ ] starsContainer → StarsContainer
  - [ ] starPrefab → StarPrefab (drag prefab vào)
- [ ] Save as Prefab: `Assets/Scenes/Menu/Inventory/Prefabs/CardSlotPrefab.prefab`

---

## 📋 Phase 2: Setup PanelInventory (20 phút)

### 2.1 Create PanelInventory
- [ ] Create Panel: "PanelInventory"
- [ ] Add children:
  - [ ] CharacterImage (Image) - Sprite: Male/Female sprite
  - [ ] ButtonShowItems (Button) - Text: "Items"
  - [ ] ButtonShowCards (Button) - Text: "Cards"
  - [ ] PanelInventoryItem (Panel)
  - [ ] PanelInventoryCard (Panel) - Hidden by default

### 2.2 Setup PanelInventoryItem
- [ ] Create child: ItemSlotsContainer (Empty GameObject)
- [ ] Add GridLayoutGroup:
  - [ ] Cell Size: (80, 80)
  - [ ] Spacing: (10, 10)
  - [ ] Constraint: Fixed Column Count = 5
  - [ ] Child Alignment: Upper Left
- [ ] Add ButtonSort (Button) - Text: "Sort"

### 2.3 Setup PanelInventoryCard
- [ ] Create child: CardSlotsContainer (Empty GameObject)
- [ ] Add GridLayoutGroup:
  - [ ] Cell Size: (100, 140)
  - [ ] Spacing: (10, 10)
  - [ ] Constraint: Fixed Column Count = 4
  - [ ] Child Alignment: Upper Left
- [ ] Add ButtonSort (Button) - Text: "Sort"

### 2.4 Add InventoryUIManager script
- [ ] Add Component: InventoryUIManager
- [ ] Assign Main Panels:
  - [ ] panelInventory → PanelInventory
  - [ ] panelLoadout → (assign sau)
- [ ] Assign Character Display:
  - [ ] characterImage → CharacterImage
  - [ ] maleSprite → (assign sprite)
  - [ ] femaleSprite → (assign sprite)
- [ ] Assign Inventory Sub-Panels:
  - [ ] panelInventoryItem → PanelInventoryItem
  - [ ] panelInventoryCard → PanelInventoryCard
  - [ ] buttonShowItems → ButtonShowItems
  - [ ] buttonShowCards → ButtonShowCards
  - [ ] buttonSortItems → PanelInventoryItem > ButtonSort
  - [ ] buttonSortCards → PanelInventoryCard > ButtonSort
- [ ] Assign Inventory Item Slots:
  - [ ] itemSlotsContainer → ItemSlotsContainer
  - [ ] itemSlotPrefab → ItemSlotPrefab
  - [ ] maxItemSlots → 15
- [ ] Assign Inventory Card Slots:
  - [ ] cardSlotsContainer → CardSlotsContainer
  - [ ] cardSlotPrefab → CardSlotPrefab
  - [ ] maxCardSlots → 8

---

## 📋 Phase 3: Setup PanelLoadout (25 phút)

### 3.1 Create PanelLoadout
- [ ] Create Panel: "PanelLoadout"
- [ ] Add children:
  - [ ] LoadoutEquipment (Panel)
  - [ ] LoadoutCards (Panel)
  - [ ] StatsDisplay (Panel)

### 3.2 Setup LoadoutEquipment (5 equipment slots)
- [ ] Create HatSlot (ItemSlotPrefab instance)
  - [ ] slotType → LoadoutEquipment
  - [ ] equipmentSlot → "hat"
- [ ] Create ShirtSlot (ItemSlotPrefab instance)
  - [ ] slotType → LoadoutEquipment
  - [ ] equipmentSlot → "shirt"
- [ ] Create WingsSlot (ItemSlotPrefab instance)
  - [ ] slotType → LoadoutEquipment
  - [ ] equipmentSlot → "wings"
- [ ] Create ShoesSlot (ItemSlotPrefab instance)
  - [ ] slotType → LoadoutEquipment
  - [ ] equipmentSlot → "shoes"
- [ ] Create MaskSlot (ItemSlotPrefab instance)
  - [ ] slotType → LoadoutEquipment
  - [ ] equipmentSlot → "mask"

### 3.3 Setup LoadoutCards (2 card slots)
- [ ] Create PassiveCardSlot (CardSlotPrefab instance)
  - [ ] slotType → LoadoutCard
  - [ ] equipmentSlot → "" (trống)
- [ ] Create ActiveCardSlot (CardSlotPrefab instance)
  - [ ] slotType → LoadoutCard
  - [ ] equipmentSlot → "" (trống)

### 3.4 Setup StatsDisplay ← QUAN TRỌNG!
- [ ] Create Panel: "StatsDisplay" trong PanelLoadout
- [ ] Add TextMeshPro components:
  - [ ] HealthText - Text: "HP: 100", Font Size: 14
  - [ ] AgilityText - Text: "Agility: 10", Font Size: 14
  - [ ] IntelligenceText - Text: "Intelligence: 10", Font Size: 14
  - [ ] LuckText - Text: "Luck: 10", Font Size: 14
  - [ ] ResistanceText - Text: "Resistance: 10", Font Size: 14
- [ ] Add LoadoutStatsDisplay script
- [ ] Assign Stats Text:
  - [ ] healthText → HealthText
  - [ ] agilityText → AgilityText
  - [ ] intelligenceText → IntelligenceText
  - [ ] luckText → LuckText
  - [ ] resistanceText → ResistanceText
- [ ] Assign Base Stats:
  - [ ] baseHealth → 100
  - [ ] baseAgility → 10
  - [ ] baseIntelligence → 10
  - [ ] baseLuck → 10
  - [ ] baseResistance → 10
- [ ] Assign Card Config:
  - [ ] attributePerLevel → 2 (từ configs/gameplay)
- [ ] Assign References:
  - [ ] hatSlot → HatSlot
  - [ ] shirtSlot → ShirtSlot
  - [ ] wingsSlot → WingsSlot
  - [ ] shoesSlot → ShoesSlot
  - [ ] maskSlot → MaskSlot
  - [ ] passiveCardSlot → PassiveCardSlot
  - [ ] activeCardSlot → ActiveCardSlot

### 3.5 Assign Loadout vào InventoryUIManager
- [ ] Select PanelInventory
- [ ] InventoryUIManager > Loadout Equipment Slots:
  - [ ] hatSlot → HatSlot
  - [ ] shirtSlot → ShirtSlot
  - [ ] wingsSlot → WingsSlot
  - [ ] shoesSlot → ShoesSlot
  - [ ] maskSlot → MaskSlot
- [ ] InventoryUIManager > Loadout Card Slots:
  - [ ] passiveCardSlot → PassiveCardSlot
  - [ ] activeCardSlot → ActiveCardSlot

---

## 📋 Phase 4: Setup Services (5 phút)

- [ ] Create Empty GameObject: "InventoryService"
- [ ] Add Component: InventoryService
- [ ] Enable Debug Logs: ✓
- [ ] Assign vào InventoryUIManager:
  - [ ] inventoryService → InventoryService
  - [ ] firebaseAuthService → FirebaseAuthService (existing)

---

## 📋 Phase 5: Setup Resources Folder (15 phút)

### 5.1 Create Folder Structure
- [ ] Create folder: `Assets/Resources/`
- [ ] Create folder: `Assets/Resources/Cards/`
- [ ] Create folder: `Assets/Resources/Equipment/`
- [ ] Create folder: `Assets/Resources/Items/`
- [ ] Create folder: `Assets/Resources/UI/`

### 5.2 Create Placeholder Sprites (10-15 images)
- [ ] Tạo placeholder images (256x256 PNG):
  - [ ] skill.lan-tron.png → Cards/
  - [ ] skill.bao-ke.png → Cards/
  - [ ] skill.toc-do.png → Cards/
  - [ ] hat.mao-len.png → Equipment/
  - [ ] shirt.ao-giap.png → Equipment/
  - [ ] wings.canh-thien-than.png → Equipment/
  - [ ] shoes.giay-the-thao.png → Equipment/
  - [ ] mask.mat-na-ninja.png → Equipment/
  - [ ] exp.small.png → Items/
  - [ ] exp.medium.png → Items/
  - [ ] exp.large.png → Items/
  - [ ] material.go.png → Items/
  - [ ] material.sat.png → Items/

### 5.3 Update Firestore
- [ ] Mở Firebase Console > Firestore
- [ ] Collection: items
- [ ] Update field "icon" cho mỗi item:
  - [ ] skill.lan-tron: icon = "Cards/skill.lan-tron"
  - [ ] hat.mao-len: icon = "Equipment/hat.mao-len"
  - [ ] exp.small: icon = "Items/exp.small"
  - [ ] ... (tất cả items)

---

## 📋 Phase 6: Test (30 phút)

### 6.1 Test Inventory
- [ ] Play game
- [ ] Login với test account
- [ ] Verify inventory loads
- [ ] Check Console log: "Loaded X items from inventory"
- [ ] Verify sprites hiển thị trong slots
- [ ] Test click "Items" button → Show PanelInventoryItem
- [ ] Test click "Cards" button → Show PanelInventoryCard
- [ ] Test Sort button → Items/cards dồn lên trên

### 6.2 Test Drag & Drop
- [ ] Drag item trong inventory → Drop vào slot khác → Swap items
- [ ] Drag equipment vào loadout equipment slot → Accept nếu đúng slot
- [ ] Drag equipment vào loadout equipment slot sai → Reject
- [ ] Drag card vào loadout card slot → Accept
- [ ] Drag card trùng itemId vào loadout → Reject (duplicate check)

### 6.3 Test Stats Display ← QUAN TRỌNG!
- [ ] Drag equipment vào loadout → Stats update
- [ ] Check Console log: "Equipment X: HP+10 AGI+5 ..."
- [ ] Drag card vào loadout → Stats update
- [ ] Check Console log: "Card X Lv.5: health+18 (base:10 + 4*2)"
- [ ] Verify UI hiển thị đúng tổng stats
- [ ] Remove item từ loadout → Stats update

### 6.4 Test Auto Save
- [ ] Drag item vào loadout
- [ ] Check Console log: "Loadout saved successfully!"
- [ ] Reload game
- [ ] Verify loadout vẫn còn

---

## 📋 Phase 7: Cleanup & Organization (10 phút)

### 7.1 Organize Folder Structure
- [ ] Move all prefabs vào: `Assets/Scenes/Menu/Inventory/Prefabs/`
- [ ] Move all scripts vào: `Assets/Scenes/Menu/Inventory/`
- [ ] Move all documentation vào: `Assets/Scenes/Menu/Inventory/`

### 7.2 Final Check
- [ ] No compile errors
- [ ] No warnings (hoặc chỉ có warnings không quan trọng)
- [ ] All references assigned
- [ ] All prefabs saved
- [ ] All documentation complete

---

**Total Time: ~2 giờ 15 phút**

**Hoàn thành! 🎉**

