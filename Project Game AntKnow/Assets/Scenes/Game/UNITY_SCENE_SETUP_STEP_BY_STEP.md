# 🎮 HƯỚNG DẪN SETUP UNITY SCENE CHO GAME MULTIPLAYER

## 📋 MỤC LỤC
1. [Canvas UI Setup](#canvas-ui-setup)
2. [Persistent Panels (Luôn hiện)](#persistent-panels)
3. [Conditional Panels (Kích hoạt khi cần)](#conditional-panels)
4. [GameManager Setup](#gamemanager-setup)
5. [Board & Tiles Setup](#board-tiles-setup)
6. [Player Prefabs Setup](#player-prefabs-setup)
7. [Testing Checklist](#testing-checklist)

---

## 1️⃣ CANVAS UI SETUP

### **Bước 1: Tạo Canvas chính**
1. Hierarchy → Right-click → **UI → Canvas**
2. Rename: `Canvas`
3. Inspector → Canvas:
   - Render Mode: **Screen Space - Overlay**
   - Canvas Scaler:
     - UI Scale Mode: **Scale With Screen Size**
     - Reference Resolution: **1920 x 1080**
     - Match: **0.5** (Width/Height)

---

## 2️⃣ PERSISTENT PANELS (Luôn hiện)

### **A. PanelGame** (Hiển thị thông tin players)

**Hierarchy:**
```
Canvas
└── PanelGame
    ├── PanelMe
    │   ├── TextName (TextMeshProUGUI)
    │   └── TextMoney (TextMeshProUGUI)
    └── PanelPlayerContainer (VerticalLayoutGroup)
        └── (PanelPlayerPrefab sẽ spawn runtime)
```

**Setup chi tiết:**

1. **Tạo PanelGame:**
   - Canvas → Right-click → **Create Empty**
   - Rename: `PanelGame`
   - Add Component: **PanelGame.cs**
   - RectTransform:
     - Anchor: **Top-Left**
     - Position: X=10, Y=-10
     - Width: 300, Height: 600

2. **Tạo PanelMe:**
   - PanelGame → Right-click → **UI → Panel**
   - Rename: `PanelMe`
   - Add Component: **PanelPlayerMe.cs**
   - Add Component: **Button** (để click hiện PanelInfo)
   - RectTransform:
     - Anchor: **Stretch-Top**
     - Height: 80
   - Children:
     - **TextName**: UI → Text - TextMeshPro
       - Text: "Player Name"
       - Font Size: 18
       - Alignment: Left
     - **TextMoney**: UI → Text - TextMeshPro
       - Text: "10000 AntCoin"
       - Font Size: 16
       - Color: Yellow

3. **Tạo PanelPlayerContainer:**
   - PanelGame → Right-click → **Create Empty**
   - Rename: `PanelPlayerContainer`
   - Add Component: **Vertical Layout Group**
     - Spacing: 10
     - Child Force Expand: Width ✓, Height ✗
   - RectTransform:
     - Anchor: **Stretch**
     - Top: -90 (dưới PanelMe)

4. **Tạo PanelPlayerPrefab:**
   - PanelPlayerContainer → Right-click → **UI → Panel**
   - Rename: `PanelPlayerPrefab`
   - Add Component: **PanelPlayer.cs**
   - Add Component: **Button**
   - Height: 70
   - Children:
     - **TextName**: Text - TextMeshPro
     - **TextMoney**: Text - TextMeshPro
   - **Drag to Project** → Save as Prefab
   - **Delete from Hierarchy** (sẽ spawn runtime)

5. **Assign PanelGame Inspector:**
   - Panel Me: [Drag PanelMe]
   - Panel Player Container: [Drag PanelPlayerContainer]
   - Panel Player Prefab: [Drag PanelPlayerPrefab from Project]
   - Max Players: 4

---

### **B. PanelGameInfo** (Turn/Time/CurrentPlayer)

**Hierarchy:**
```
Canvas
└── PanelGameInfo
    ├── TextTurn (TextMeshProUGUI)
    ├── TextTime (TextMeshProUGUI)
    └── TextCurrentPlayer (TextMeshProUGUI)
```

**Setup:**
1. Canvas → Create Empty → `PanelGameInfo`
2. Add Component: **PanelGameInfo.cs**
3. RectTransform:
   - Anchor: **Top-Center**
   - Position: Y=-10
   - Width: 600, Height: 100
4. Children (3 texts):
   - **TextTurn**: "Turn: 1/25"
   - **TextTime**: "Time: 00:00"
   - **TextCurrentPlayer**: "Current: Player 1"
5. Assign Inspector:
   - Text Turn: [Drag TextTurn]
   - Text Time: [Drag TextTime]
   - Text Current Player: [Drag TextCurrentPlayer]
   - Max Turns: 25

---

### **C. PanelRoll** (Dice + Roll Button)

**Hierarchy:**
```
Canvas
└── PanelRoll
    ├── Dice1 (Image)
    ├── Dice2 (Image)
    ├── TextResult (TextMeshProUGUI)
    └── BtnRoll (Button)
```

**Setup:**
1. Canvas → UI → Panel → `PanelRoll`
2. Add Component: **PanelRoll.cs**
3. RectTransform:
   - Anchor: **Bottom-Right**
   - Position: X=-10, Y=10
   - Width: 300, Height: 200
4. Children:
   - **Dice1**: UI → Image
     - Width: 80, Height: 80
     - Position: X=-100, Y=50
   - **Dice2**: UI → Image
     - Width: 80, Height: 80
     - Position: X=-20, Y=50
   - **TextResult**: Text - TextMeshPro
     - Text: "Result: 7"
     - Position: Y=0
   - **BtnRoll**: UI → Button
     - Text: "ROLL DICE"
     - Width: 200, Height: 50
     - Position: Y=-50

5. **Chuẩn bị Dice Sprites:**
   - Import 6 sprites (dice_1.png đến dice_6.png)
   - Assign vào Inspector:
     - Dice Sprites: Size = 6
     - Element 0: dice_1
     - Element 1: dice_2
     - ... Element 5: dice_6

6. Assign Inspector:
   - Dice1 Image: [Drag Dice1]
   - Dice2 Image: [Drag Dice2]
   - Dice Sprites: [Assign 6 sprites]
   - Text Result: [Drag TextResult]
   - Btn Roll: [Drag BtnRoll]

---

## 3️⃣ CONDITIONAL PANELS (Kích hoạt khi cần)

### **D. PanelInfo** (Player Info Popup)

**Hierarchy:**
```
Canvas
└── PanelInfo (INACTIVE)
    ├── ImageGender (Image)
    ├── TextPlayerName (TextMeshProUGUI)
    ├── TextMatchesPlayed (TextMeshProUGUI)
    ├── TextMatchesWon (TextMeshProUGUI)
    └── BtnClose (Button)
```

**Setup:**
1. Canvas → UI → Panel → `PanelInfo`
2. Add Component: **PanelInfo.cs**
3. **Set Active: FALSE** (ẩn ban đầu)
4. RectTransform:
   - Anchor: **Center**
   - Width: 400, Height: 500
5. Children:
   - **ImageGender**: UI → Image (100x100)
   - **TextPlayerName**: "Player Name"
   - **TextMatchesPlayed**: "Số trận chơi: 0"
   - **TextMatchesWon**: "Số trận thắng: 0"
   - **BtnClose**: Button "X" (top-right)

6. **Chuẩn bị Gender Sprites:**
   - Import sprite_male.png, sprite_female.png
   - Assign Inspector:
     - Sprite Male: [sprite_male]
     - Sprite Female: [sprite_female]

---

### **E. PanelBuy** (Buy/Upgrade Property)

**Hierarchy:**
```
Canvas
└── PanelBuy (INACTIVE)
    ├── TextPropertyName (TextMeshProUGUI)
    ├── TextPrice (TextMeshProUGUI)
    ├── HouseSelection
    │   ├── ToggleHouse1 (Toggle)
    │   ├── ToggleHouse2 (Toggle)
    │   ├── ToggleHouse3 (Toggle)
    │   └── ToggleHouse4 (Toggle)
    ├── BtnBuy (Button)
    └── BtnSkip (Button)
```

**Setup:**
1. Canvas → UI → Panel → `PanelBuy`
2. Add Component: **PanelBuy.cs**
3. **Set Active: FALSE**
4. RectTransform: Center, 500x600
5. Children:
   - **TextPropertyName**: "Property Name"
   - **TextPrice**: "Price: 1000"
   - **HouseSelection**: Empty GameObject với Horizontal Layout Group
     - 4 Toggles (House 1-4)
   - **BtnBuy**: "BUY"
   - **BtnSkip**: "SKIP"

---

### **F. PanelQuiz** (Quiz với Fortune Wheel)

**Hierarchy:**
```
Canvas
└── PanelQuiz (INACTIVE)
    ├── TextQuestion (TextMeshProUGUI)
    ├── TextDifficulty (TextMeshProUGUI)
    ├── TextTimer (TextMeshProUGUI)
    ├── BtnAnswer1 (Button)
    ├── BtnAnswer2 (Button)
    ├── BtnAnswer3 (Button)
    ├── BtnAnswer4 (Button)
    └── FortuneWheelObject (INACTIVE)
        ├── Wheel (Image - rotating)
        └── TextWheelResult (TextMeshProUGUI)
```

**Setup:**
1. Canvas → UI → Panel → `PanelQuiz`
2. Add Component: **PanelQuiz.cs**
3. **Set Active: FALSE**
4. RectTransform: Center, 800x600
5. Children:
   - **TextQuestion**: "Question text here"
   - **TextDifficulty**: "Easy"
   - **TextTimer**: "15s"
   - 4 Answer Buttons (vertical layout)
   - **FortuneWheelObject**: Panel (INACTIVE)
     - **Wheel**: Image (spinning animation)
     - **TextWheelResult**: "Penalty result"

---

### **G. PanelEvent** (Event Cards)

**Hierarchy:**
```
Canvas
└── PanelEvent (INACTIVE)
    ├── TextEvent (TextMeshProUGUI)
    └── BtnOK (Button)
```

**Setup:**
1. Canvas → UI → Panel → `PanelEvent`
2. Add Component: **PanelEvent.cs**
3. **Set Active: FALSE**
4. RectTransform: Center, 500x300
5. Children:
   - **TextEvent**: "Event description"
   - **BtnOK**: "OK"

---

### **H. PanelHouseSell** (Sell Properties)

**Hierarchy:**
```
Canvas
└── PanelHouseSell (INACTIVE)
    ├── ScrollView
    │   └── Content
    │       └── (PropertySellItemPrefab spawns here)
    └── BtnSell (Button)
```

**Setup:**
1. Canvas → UI → Scroll View → Rename `PanelHouseSell`
2. Add Component: **PanelHouseSell.cs**
3. **Set Active: FALSE**
4. RectTransform: Center, 600x700
5. **Tạo PropertySellItemPrefab:**
   - Content → UI → Panel → `PropertySellItemPrefab`
   - Add Component: **PropertySellItem.cs**
   - Children:
     - **Toggle**: Select property
     - **TextPropertyName**: "Property Name"
     - **TextLevel**: "House 2"
     - **TextSellPrice**: "Sell: 600"
   - Drag to Project → Save Prefab
   - Delete from Hierarchy

---

### **I. PanelResult** (Game End Results)

**Hierarchy:**
```
Canvas
└── PanelResult (INACTIVE)
    ├── TextTitle (TextMeshProUGUI)
    ├── ResultContainer (VerticalLayoutGroup)
    │   └── (ResultItemPrefab spawns here)
    └── BtnExit (Button)
```

**Setup:**
1. Canvas → UI → Panel → `PanelResult`
2. Add Component: **PanelResult.cs**
3. **Set Active: FALSE**
4. RectTransform: Center, 800x900

---

### **J. PanelNotification** (Quick Notifications)

**Hierarchy:**
```
Canvas
└── PanelNotification (INACTIVE)
    └── TextNotification (TextMeshProUGUI)
```

**Setup:**
1. Canvas → UI → Panel → `PanelNotification`
2. Add Component: **PanelNotification.cs**
3. **Set Active: FALSE**
4. RectTransform: Top-Center, 600x100
5. Background: Semi-transparent black
6. Text: Large, white, centered

---

## 4️⃣ GAMEMANAGER SETUP

### **Tạo GameManager GameObject**

1. Hierarchy → Create Empty → `GameManager`
2. Add Component: **Network Object**
   - Is Player Object: **FALSE**
3. Add Component: **GameManager.cs**

### **Assign Inspector:**

**Player Prefabs:**
- Player Prefab Male: [Drag PlayerMale.prefab]
- Player Prefab Female: [Drag PlayerFemale.prefab]

**UI:**
- Roll Button: [Drag BtnRoll from PanelRoll]
- Turn Text: [Drag TextTurn from PanelGameInfo]
- Current Player Text: [Drag TextCurrentPlayer]
- Time Text: [Drag TextTime]

**UI Panels:**
- Panel Buy: [Drag PanelBuy]
- Panel Quiz: [Drag PanelQuiz]
- Panel Event: [Drag PanelEvent]
- Panel House Sell: [Drag PanelHouseSell]
- Panel Result: [Drag PanelResult]
- Panel Card: [Drag PanelCard if exists]

**Game Settings:**
- Max Turns: 25
- Demo Mode: ✓ TRUE (for testing)

---

## 5️⃣ BOARD & TILES SETUP

### **Tạo Board Structure**

```
Scene
├── Map (Empty GameObject)
│   ├── Tile0 (Cube + TileVisual.cs)
│   ├── Tile1 (Cube + TileVisual.cs)
│   ├── ...
│   └── Tile35 (Cube + TileVisual.cs)
└── Waypoints (Empty GameObject)
    ├── Waypoint0 (Empty)
    ├── Waypoint1 (Empty)
    ├── ...
    └── Waypoint35 (Empty)
```

### **Tile Setup (Ví dụ Tile0):**

1. **Tạo Tile:**
   - Map → 3D Object → Cube → `Tile0`
   - Scale: (2, 0.2, 2)
   - Position: (0, 0, 0)

2. **Add TileVisual Component:**
   - Add Component: **TileVisual.cs**
   - Tile Index: 0

3. **Tạo Platform (con của Tile):**
   - Tile0 → 3D Object → Cube → `Platform`
   - Scale: (0.8, 0.1, 0.8)
   - Position: (0, 0.15, 0)
   - Material: Default (sẽ đổi màu runtime)

4. **Tạo TextName (TextMesh):**
   - Tile0 → 3D Object → 3D Text → `TextName`
   - Text: "Start"
   - Font Size: 50
   - Position: (0, 0.5, 0)
   - Rotation: (90, 0, 0) - nhìn từ trên xuống

5. **Tạo TextPrice (TextMesh - optional):**
   - Tile0 → 3D Object → 3D Text → `TextPrice`
   - Text: "1000"
   - Font Size: 40
   - Position: (0, 0.5, -0.5)

6. **Assign TileVisual Inspector:**
   - Platform: [Drag Platform]
   - Text Name: [Drag TextName]
   - Text Price: [Drag TextPrice]
   - Auto Find Children: ✓ TRUE

7. **Repeat cho Tile1-35** (hoặc dùng Editor script TileDataAutoSetup)

---

## 6️⃣ PLAYER PREFABS SETUP

### **PlayerMale Prefab:**

1. Hierarchy → Create Empty → `PlayerMale`
2. Add Component: **Network Object**
   - Is Player Object: **TRUE**
3. Add Component: **PlayerGameController.cs**
   - Is Male: ✓ **TRUE**
   - Money: 10000
   - Move Speed: 5
   - Bounce Height: 0.5

4. **Import Male Model:**
   - Drag male character model vào làm child
   - Rename: `MaleModel`
   - Assign Animator

5. **Tạo TurnIndicator:**
   - PlayerMale → Create Empty → `TurnIndicator`
   - Position: (0, 2.5, 0)
   - Add Component: **TurnIndicator.cs**
   - Add Component: **Network Object**
   - Child: 3D Object → Sphere → `Sphere`
     - Scale: (0.3, 0.3, 0.3)
     - Material: Yellow

6. **Drag to Project** → Save as `PlayerMale.prefab`

### **PlayerFemale Prefab:**
- Repeat tương tự nhưng:
  - Is Male: **FALSE**
  - Use female model

---

## 7️⃣ TESTING CHECKLIST

### **Test Demo Mode:**

1. ✅ GameManager → Demo Mode = TRUE
2. ✅ Play Scene
3. ✅ Verify:
   - [ ] 1 player spawns at Tile0
   - [ ] PanelGame shows player info
   - [ ] PanelGameInfo shows Turn 1/25
   - [ ] PanelRoll shows dice + button
   - [ ] Click Roll → Dice animation
   - [ ] Player moves with bounce effect
   - [ ] Turn indicator appears on player
   - [ ] No errors in Console

### **Test UI Panels:**

4. ✅ Click PanelMe → PanelInfo shows
5. ✅ Land on property → PanelBuy shows
6. ✅ Test all panels manually

---

## 🎯 NEXT STEPS

Sau khi setup xong scene:

1. **Phase 3:** Hoàn thiện UI Panel Scripts
2. **Phase 4:** Triển khai Game Flow
3. **Phase 5:** Property System
4. **Phase 6:** Player Movement
5. **Phase 7:** Firebase Integration
6. **Phase 8:** Testing & Bug Fixes

---

**Bạn muốn tôi:**
1. ✅ Tạo file hướng dẫn này (DONE)
2. ⏭️ Tạo Editor Script để auto-generate tiles?
3. ⏭️ Cập nhật các Panel scripts để kết nối với GameManager?
4. ⏭️ Tạo prefabs mẫu?

Hãy cho tôi biết bạn muốn bắt đầu từ đâu!

