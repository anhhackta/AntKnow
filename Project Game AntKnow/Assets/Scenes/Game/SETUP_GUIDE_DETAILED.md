# 🎮 HƯỚNG DẪN SETUP GAMESCENE CHI TIẾT

## 📋 MỤC LỤC
1. [Cấu trúc Scene](#1-cấu-trúc-scene)
2. [Setup UI Panels](#2-setup-ui-panels)
3. [Setup Player Prefab](#3-setup-player-prefab)
4. [Setup Map & Tiles](#4-setup-map--tiles)
5. [Setup GameManager](#5-setup-gamemanager)
6. [Testing](#6-testing)

---

## 1. CẤU TRÚC SCENE

### **Hierarchy Tổng Quan**
```
GameScene
├── Canvas (UI)
│   ├── PanelGame (Luôn hiện)
│   ├── PanelGameInfo (Luôn hiện)
│   ├── PanelRoll (Luôn hiện)
│   ├── PanelInfo (Ẩn - kích hoạt khi cần)
│   ├── PanelBuy (Ẩn - kích hoạt khi cần)
│   ├── PanelQuiz (Ẩn - kích hoạt khi cần)
│   ├── PanelEvent (Ẩn - kích hoạt khi cần)
│   ├── PanelHouseSell (Ẩn - kích hoạt khi cần)
│   ├── PanelResult (Ẩn - kích hoạt khi cần)
│   └── PanelNotification (Ẩn - kích hoạt khi cần)
│
├── GameManager (NetworkObject)
├── BoardManager
├── PropertyManager
├── NetworkManager
│
├── Map (Board)
│   ├── Tile0 (Start)
│   ├── Tile1
│   ├── ...
│   └── Tile35
│
└── Waypoints
    ├── Waypoint0
    ├── Waypoint1
    ├── ...
    └── Waypoint35
```

---

## 2. SETUP UI PANELS

### **2.1 PanelGame** (Container - Luôn hiện)
**Hierarchy:**
```
PanelGame (Panel)
├── PanelMe (Vertical Layout Group)
│   ├── TextName (TMP)
│   └── TextMoney (TMP)
└── PanelPlayerContainer (Vertical Layout Group)
    └── [PanelPlayerPrefab instances will be added here]
```

**Components:**
- **PanelGame (GameObject)**
  - Script: `PanelGame.cs`
  - Inspector:
    - Panel Me: [Drag PanelMe]
    - Panel Player Container: [Drag PanelPlayerContainer]
    - Panel Player Prefab: [Drag PanelPlayerPrefab from Project]
    - Max Players: 4

**PanelMe Setup:**
- Script: `PanelPlayerMe.cs`
- Components:
  - Text Name (TMP)
  - Text Money (TMP)
  - Button (để click mở PanelInfo)

**PanelPlayerPrefab Setup:**
- Tạo prefab mới với structure:
  ```
  PanelPlayerPrefab
  ├── TextName (TMP)
  └── TextMoney (TMP)
  ```
- Script: `PanelPlayer.cs`
- Add Button component (để click mở PanelInfo)

---

### **2.2 PanelGameInfo** (Luôn hiện)
**Hierarchy:**
```
PanelGameInfo (Panel)
├── TextTurn (TMP)
├── TextTime (TMP)
└── TextCurrentPlayer (TMP)
```

**Components:**
- **PanelGameInfo (GameObject)**
  - Script: `PanelGameInfo.cs`
  - Inspector:
    - Text Turn: [Drag TextTurn]
    - Text Time: [Drag TextTime]
    - Text Current Player: [Drag TextCurrentPlayer]
    - Max Turns: 25

**Text Examples:**
- Turn: "Turn: 1/25"
- Time: "Time: 05:32"
- CurrentPlayer: "Current: Player1"

---

### **2.3 PanelRoll** (Luôn hiện)
**Hierarchy:**
```
PanelRoll (Panel)
├── Dice1 (Image)
├── Dice2 (Image)
├── TextResult (TMP)
└── BtnRoll (Button)
```

**Components:**
- **PanelRoll (GameObject)**
  - Script: `PanelRoll.cs`
  - Inspector:
    - Dice1 Image: [Drag Dice1]
    - Dice2 Image: [Drag Dice2]
    - Dice Sprites: [Array of 6 sprites, index 0-5 for dice faces 1-6]
    - Text Result: [Drag TextResult]
    - Btn Roll: [Drag BtnRoll]
    - Roll Duration: 1.5
    - Frame Interval: 0.1

**Dice Sprites:**
- Cần 6 sprites cho mặt xúc xắc 1-6
- Assign vào array theo thứ tự: sprite[0] = dice 1, sprite[1] = dice 2, ...

---

### **2.4 PanelInfo** (Ẩn - click để mở)
**Hierarchy:**
```
PanelInfo (Panel - Initially SetActive(false))
├── ImageGender (Image)
├── TextPlayerName (TMP)
├── TextMatchesPlayed (TMP)
├── TextMatchesWon (TMP)
└── BtnClose (Button)
```

**Components:**
- **PanelInfo (GameObject)**
  - Script: `PanelInfo.cs`
  - Inspector:
    - Image Gender: [Drag ImageGender]
    - Text Player Name: [Drag TextPlayerName]
    - Text Matches Played: [Drag TextMatchesPlayed]
    - Text Matches Won: [Drag TextMatchesWon]
    - Btn Close: [Drag BtnClose]
    - Sprite Male: [Drag male sprite]
    - Sprite Female: [Drag female sprite]

---

### **2.5 PanelBuy** (Ẩn - mua/nâng cấp nhà)
**Hierarchy:**
```
PanelBuy (Panel - Initially SetActive(false))
├── TextPropertyName (TMP)
├── TextOwnerName (TMP)
├── TextPrice (TMP)
├── HouseButtons (Horizontal Layout Group)
│   ├── BtnHouse1 (Button + Text "1")
│   ├── BtnHouse2 (Button + Text "2")
│   ├── BtnHouse3 (Button + Text "3")
│   ├── BtnHouse4 (Button + Text "4")
│   └── BtnHotel (Button + Text "Hotel")
├── BtnBuy (Button)
└── BtnSkip (Button)
```

**Components:**
- **PanelBuy (GameObject)**
  - Script: `PanelBuy.cs`
  - Inspector:
    - Text Property Name: [Drag TextPropertyName]
    - Text Owner Name: [Drag TextOwnerName]
    - Text Price: [Drag TextPrice]
    - Btn House 1-4: [Drag buttons]
    - Btn Hotel: [Drag BtnHotel]
    - Btn Buy: [Drag BtnBuy]
    - Btn Skip: [Drag BtnSkip]
    - Colors:
      - Normal Color: White
      - Selected Color: Green
      - Disabled Color: Gray (0.5, 0.5, 0.5)
      - Cannot Afford Color: Red

**Logic:**
- **Ô trống**: Chọn 1-4 nhà → Tính giá đất + nhà (Hotel mờ đi)
- **Ô của mình**: Nhà đã mua mờ đi, chỉ nâng cấp thêm
- **Hotel**: Chỉ enable khi đã có House 4 (currentLevel = 4)
- Button sáng xanh khi chọn
- Button Buy sáng khi đủ tiền

---

### **2.6 PanelQuiz** (Ẩn - trả lời câu hỏi)
**Hierarchy:**
```
PanelQuiz (Panel - Initially SetActive(false))
├── TextQuestion (TMP)
├── TextDifficulty (TMP)
├── TextTimer (TMP)
├── OptionsGroup (Vertical Layout Group)
│   ├── BtnOption1 (Button + Text)
│   ├── BtnOption2 (Button + Text)
│   ├── BtnOption3 (Button + Text)
│   └── BtnOption4 (Button + Text)
└── FortuneWheel (GameObject - Ẩn)
    └── [Wheel visual + animation]
```

**Components:**
- **PanelQuiz (GameObject)**
  - Script: `PanelQuiz.cs`
  - Inspector:
    - Text Question: [Drag TextQuestion]
    - Text Difficulty: [Drag TextDifficulty]
    - Text Timer: [Drag TextTimer]
    - Btn Options: [Array of 4 buttons]
    - Fortune Wheel: [Drag FortuneWheel GameObject]
    - Timer Duration: 15

**Logic:**
- Hiện câu hỏi từ Firebase
- 15 giây đếm ngược
- Click button → Check correctAnswer
- Đúng: Màu xanh, text "Trả lời đúng"
- Sai: Màu đỏ, text "Trả lời sai" → FortuneWheel

**FortuneWheel:**
- 3 slots: Trừ tiền / Hạ nhà / Không làm gì
- Tỉ lệ: 1/3 mỗi loại
- Tự quay và hiển thị kết quả 2 giây

---

### **2.7 PanelEvent** (Ẩn - event card)
**Hierarchy:**
```
PanelEvent (Panel - Initially SetActive(false))
├── TextEventInfo (TMP)
└── BtnOK (Button)
```

**Components:**
- **PanelEvent (GameObject)**
  - Script: `PanelEvent.cs`
  - Inspector:
    - Text Event Info: [Drag TextEventInfo]
    - Btn OK: [Drag BtnOK]
    - Auto Hide Delay: 3

---

### **2.8 PanelHouseSell** (Ẩn - bán nhà)
**Hierarchy:**
```
PanelHouseSell (Panel - Initially SetActive(false))
├── ScrollView
│   └── Content (Vertical Layout Group)
│       └── [PropertySellItemPrefab instances]
└── BtnSell (Button)
```

**PropertySellItemPrefab:**
```
PropertySellItem
├── Toggle
├── TextPropertyName (TMP)
├── TextLevel (TMP) - "House 1", "House 2", etc.
└── TextSellPrice (TMP) - 60% giá mua
```

**Components:**
- **PanelHouseSell (GameObject)**
  - Script: `PanelHouseSell.cs`
  - Inspector:
    - Scroll View Content: [Drag Content]
    - Property Sell Item Prefab: [Drag prefab]
    - Btn Sell: [Drag BtnSell]

---

### **2.9 PanelResult** (Ẩn - kết quả trận)
**Hierarchy:**
```
PanelResult (Panel - Initially SetActive(false))
├── Title (TMP) - "KẾT QUẢ TRẬN ĐẤU"
├── RankingList (Vertical Layout Group)
│   ├── RankItem1 (Top 1)
│   ├── RankItem2 (Top 2)
│   ├── RankItem3 (Top 3)
│   └── RankItem4 (Top 4)
└── BtnBackToMenu (Button)
```

**RankItem Structure:**
```
RankItem
├── TextRank (TMP) - "1.", "2.", etc.
├── TextPlayerName (TMP)
├── TextMoney (TMP)
└── TextReward (TMP) - "AntCoin: X, EXP: Y"
```

**Components:**
- **PanelResult (GameObject)**
  - Script: `PanelResult.cs`
  - Inspector:
    - Rank Items: [Array of 4 RankItem GameObjects]
    - Btn Back To Menu: [Drag button]

---

### **2.10 PanelNotification** (Ẩn - thông báo nhanh)
**Hierarchy:**
```
PanelNotification (Panel - Initially SetActive(false))
└── TextNotification (TMP)
```

**Components:**
- **PanelNotification (GameObject)**
  - Script: `PanelNotification.cs`
  - Inspector:
    - Text Notification: [Drag TextNotification]
    - Display Duration: 1

---

## 3. SETUP PLAYER PREFAB

### **3.1 Player Prefab Structure**
```
PlayerPrefab
├── PlayerGameController (Script)
├── NetworkObject
├── ModelParent (Empty)
│   ├── MaleModel (Humanoid với Animator)
│   └── FemaleModel (Humanoid với Animator)
└── TurnIndicator (Empty)
    ├── TurnIndicator (Script)
    └── Sphere (Primitive Sphere - 0.3, 0.3, 0.3)
```

### **3.2 PlayerGameController Setup**
**Inspector:**
```
Player Info:
- Player Name: ""
- Player ID: ""
- Is Male: true

Stats:
- Health: 0
- Agility: 0
- Intelligence: 0
- Luck: 0
- Resistance: 0

Movement:
- Move Speed: 5
- Bounce Height: 0.5
- Bounce Duration: 0.3
- Board Manager: [Assign in scene]
- Board Center: (0, 0, 0)

Models:
- Male Model: [Drag MaleModel from children]
- Female Model: [Drag FemaleModel from children]
- Model Parent: [Drag ModelParent]

Turn Indicator:
- Turn Indicator: [Drag TurnIndicator object]
```

### **3.3 TurnIndicator Setup**
**Components:**
- GameObject: TurnIndicator
- Script: `TurnIndicator.cs`
- Child: Sphere (Scale: 0.3, 0.3, 0.3)
- Material: Yellow color
- Position Offset: (0, 2.5, 0) - Trên đầu player

---

## 4. SETUP MAP & TILES

### **4.1 Tile Structure**
```
Tile (Cube - 2x0.5x2)
├── TileVisual (Script)
├── Platform (Cube - 0.8x0.1x0.8)
│   └── [Houses will spawn here]
├── TextName (TMP - WorldSpace)
└── TextPrice (TMP - WorldSpace)
```

### **4.2 TileVisual Setup**
**Inspector:**
```
Tile Structure:
- Platform: [Drag Platform GameObject]
- Text Name: [Drag TextName]
- Text Price: [Drag TextPrice]
- Auto Find Children: true

Info:
- Tile Index: [0-35]
```

### **4.3 Platform Placement**
- Position: Trên đỉnh tile cube
- Scale: (0.8, 0.1, 0.8)
- Tag: "Platform"

**House Placement Directions:**
- **Z-axis**: Hướng vào center (inward)
- **Y-axis**: Hướng lên (upward for stacking)
- **X-axis**: Bên trái (left for side-by-side)

### **4.4 House/Hotel Prefabs**
**HousePrefab:**
- 3D Model với MeshRenderer
- Material có tên "ngói" (roof material)
- Scale: Phù hợp với platform

**HotelPrefab:**
- 3D Model lớn hơn house
- Material có tên "ngói"
- Thay thế 4 houses khi upgrade

### **4.5 Material System**
**Player Colors:**
- Player 1: Red (1, 0.2, 0.2)
- Player 2: Blue (0.2, 0.5, 1)
- Player 3: Green (0.2, 1, 0.2)
- Player 4: Yellow (1, 1, 0.2)

**PropertyVisual Setup:**
```
Inspector:
- House Prefab: [Drag HousePrefab]
- Hotel Prefab: [Drag HotelPrefab]
- Roof Material Name: "ngói"
- Player Colors: [4 colors như trên]
```

---

## 5. SETUP GAMEMANAGER

### **5.1 GameManager GameObject**
**Components:**
- NetworkObject
- GameManager (Script)

**Inspector:**
```
Managers:
- Board Manager: [Drag BoardManager]
- Panel Roll: [Drag PanelRoll]
- Property Manager: [Drag PropertyManager]

Players:
- Player Prefab Male: [Drag male prefab]
- Player Prefab Female: [Drag female prefab]

UI:
- Roll Button: [Drag button from PanelRoll]
- Turn Text: [Drag text from PanelGameInfo]
- Current Player Text: [Drag text from PanelGameInfo]
- Time Text: [Drag text from PanelGameInfo]

UI Panels:
- Panel Buy: [Drag PanelBuy]
- Panel Quiz: [Drag PanelQuiz]
- Panel Event: [Drag PanelEvent]
- Panel House Sell: [Drag PanelHouseSell]
- Panel Result: [Drag PanelResult]
- Panel Card: [Drag PanelCard if exists]

Game Settings:
- Max Turns: 25
- Demo Mode: false

Services:
- Firebase Auth Service: [Drag from scene]
```

### **5.2 BoardManager Setup**
**Inspector:**
```
Waypoints:
- Waypoints: [Array of 36 Transform waypoints]

Debug:
- Show Debug Info: true
```

### **5.3 PropertyManager Setup**
**Inspector:**
```
Property Visual:
- Property Visual: [Drag PropertyVisual script in scene]

Settings:
- Max Properties: 36
```

---

## 6. TESTING

### **6.1 Checklist**
- [ ] All UI panels assigned
- [ ] Player prefabs assigned (male + female)
- [ ] BoardManager has 36 waypoints
- [ ] Tiles have Platform + TileVisual
- [ ] House/Hotel prefabs assigned
- [ ] Materials có "ngói" material
- [ ] NetworkManager in scene
- [ ] Firebase services connected

### **6.2 Test Mode**
1. Set **Demo Mode = true** trong GameManager
2. Play scene trong Editor
3. Kiểm tra:
   - 1 player spawn tại Waypoint 0
   - PanelGameInfo hiển thị đúng
   - PanelRoll button hoạt động
   - Dice roll animation
   - Player movement với bounce effect
   - Tiles resolve đúng

### **6.3 Multiplayer Test**
1. Set **Demo Mode = false**
2. Build game
3. Test Host + Client:
   - Host tạo room
   - Client join room
   - Start game
   - Check turn order selection
   - Check player sync
   - Check UI sync

---

## 📝 NOTES

### **Important:**
- Tất cả panels "Ẩn" phải có `SetActive(false)` trong Inspector
- NetworkObject REQUIRED trên GameManager
- Player prefabs phải có NetworkObject
- Waypoints phải đúng thứ tự 0-35
- Material "ngói" phải tồn tại trên house/hotel prefabs

### **Common Issues:**
1. **Panel không hiện**: Check SetActive(true) in code
2. **Dice không roll**: Check sprites assigned (array 6 items)
3. **Player không spawn**: Check NetworkObject + prefab assigned
4. **House không đổi màu**: Check material name = "ngói"
5. **Turn order sai**: Check Host-authoritative logic

---

**Version**: 1.0  
**Date**: 2025-10-12  
**Status**: Ready for implementation ✅
