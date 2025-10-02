# 🎮 GameScene - Cờ Tỷ Phú Multiplayer - Design Document

## 📋 Tổng quan

Game cờ tỷ phú multiplayer 2-4 người chơi, bàn cờ 36 ô, với các ô đặc biệt, mua nhà, trả lời quiz, và event cards.

---

## 🗺️ Board Layout - 36 Tiles

### Tile Types:

| Tile ID | Type | Description |
|---------|------|-------------|
| 0 | Start | Ô bắt đầu, qua ô này +150 tiền (+Health%) |
| 1-6 | Property | Nhà bình thường (có thể mua) |
| 7 | Event | Ô event card ngẫu nhiên |
| 8-9 | Property | Nhà bình thường |
| 10 | Jail | Tại nạn, dừng 3 lượt hoặc đổ xúc sắc đôi |
| 11-15 | Property | Nhà bình thường |
| 16 | Event | Ô event card ngẫu nhiên |
| 17-18 | Property | Nhà bình thường |
| 19 | Quiz | Ô tra khảo, trả lời câu hỏi |
| 20-24 | Property | Nhà bình thường |
| 25 | Event | Ô event card ngẫu nhiên |
| 26-27 | Property | Nhà bình thường |
| 28 | Travel | Ô du lịch, -100 tiền |
| 29-32 | Property | Nhà bình thường |
| 33 | Event | Ô event card ngẫu nhiên |
| 34-35 | Property | Nhà bình thường |

**Total: 36 tiles (28 property, 4 event, 4 special)**

---

## 💰 Game Economy

### Starting Money:
```
Mỗi người chơi: 1000 tiền
Qua ô Start: +150 tiền (+Health%)
```

### Property System:
```
Nhà Level 1-5: Có thể mua/bán
Nhà Hotel: Nâng cấp từ Level 5, KHÔNG thể bán

Mua nhà: Trừ tiền (-Agility%)
Thuê nhà: Trả tiền cho chủ nhà (-Resistance%), chủ nhà nhận (+Intelligence%)
```

### Stats Effects:
```
Health: +% tiền khi qua ô Start
Agility: -% giá mua nhà
Intelligence: +% tiền nhận từ thuê nhà
Luck: +% tỷ lệ xúc sắc đôi
Resistance: -% tiền trả khi thuê nhà người khác
```

---

## 🎲 Dice System - 2D Dice

### Dice Roll:
```
2 viên xúc sắc 2D (Image sprites)
Mỗi viên: 6 mặt (1-6)
Animation: Đổi sprite nhanh để tạo hiệu ứng roll
Kết quả: Tổng 2 viên (2-12)
Xúc sắc đôi: 2 viên cùng số (Luck% tăng tỷ lệ)
```

### Dice Sprites:
```
Assets/Resources/UI/Dice/
├── dice_1.png
├── dice_2.png
├── dice_3.png
├── dice_4.png
├── dice_5.png
└── dice_6.png
```

---

## 🎴 Event Cards - 9 loại

### Event Card List:
```
1. Thanh tra: Tiến hành tra khảo (trigger quiz)
2. Đám cưới bạn: -100 tiền
3. Gặp lại cố nhân: +200 tiền
4. Trốn thuế phát hiện: -250 tiền
5. Tăng ca: +50 tiền
6. FreeLancer: +100 tiền
7. Khế ước quỷ dữ: Đổi 1 nhà bất kỳ với người chơi
8. Thừa kế gia sản: +500 tiền
9. Ăn gì chưa người đẹp: -120 tiền
```

### Event Card Sprites:
```
Assets/Resources/UI/EventCards/
├── event_thanh_tra.png
├── event_dam_cuoi.png
├── event_co_nhan.png
├── event_tron_thue.png
├── event_tang_ca.png
├── event_freelancer.png
├── event_khe_uoc.png
├── event_thua_ke.png
└── event_an_gi_chua.png
```

---

## 📱 UI Panels

### 1. PanelPlayerMe (Local Player)
```
- Avatar (gender-based)
- Player Name
- Current Money (with stats effects)
- Position on board
- Turn indicator (highlight when your turn)
```

### 2. PanelPlayer1/2/3 (Other Players)
```
- Avatar (gender-based)
- Player Name
- Current Money
- Position on board
- Turn indicator
```

### 3. PanelDice
```
- Dice1 Image (animated)
- Dice2 Image (animated)
- Button Roll (only active on your turn)
- Result Text (sum)
```

### 4. PanelProperty (Popup)
```
- Property Name
- Property Level (1-5 or Hotel)
- Owner Name
- Buy Price (-Agility%)
- Rent Price (-Resistance% for payer, +Intelligence% for owner)
- Button Buy (if available)
- Button Upgrade (if owner and can upgrade)
- Button Close
```

### 5. PanelQuiz (Popup)
```
- Question Text
- 4 Answer Buttons
- Timer (optional)
- Result (correct/wrong)
```

### 6. PanelEventCard (Popup)
```
- Event Card Image
- Event Description
- Effect Text
- Button OK
```

### 7. PanelGameInfo
```
- Current Round
- Current Turn Player
- Time Elapsed
```

---

## 🎭 Player Models

### Character Models:
```
Male Model: model_male.fbx
- Animation: Idle, Run

Female Model: model_female.fbx
- Animation: Idle, Run

Animator Controller:
- Parameter: isRunning (bool)
- Idle → Run: isRunning = true
- Run → Idle: isRunning = false
```

### Model Setup:
```
1. Load model từ loadout (gender từ user profile)
2. Spawn tại tile 0 (Start)
3. Set animator controller
4. Set player color/material (P1: Red, P2: Blue, P3: Green, P4: Yellow)
```

---

## 🛤️ Waypoint System

### Waypoint Setup:
```
GameObject: BoardPath
├── Waypoint_00 (Start)
├── Waypoint_01
├── Waypoint_02
├── ...
└── Waypoint_35

Total: 36 waypoints (circular path)
```

### Movement:
```
1. Roll dice → Get steps
2. Calculate target waypoint: (currentIndex + steps) % 36
3. Move player model từ waypoint này đến waypoint khác
4. Set isRunning = true during movement
5. Set isRunning = false when reached
6. Resolve tile effect
```

---

## 🎯 Game Flow

### 1. Game Start:
```
1. Load players từ lobby (2-4 players)
2. Load loadout stats từ Firestore
3. Spawn player models tại tile 0
4. Initialize money: 1000 cho mỗi người
5. Random turn order
6. Start turn 1
```

### 2. Turn Flow:
```
1. Highlight current player panel
2. Enable Roll button cho current player
3. Player click Roll → Roll dice
4. Animate dice roll (2D sprites)
5. Show result
6. Move player model
7. Resolve tile effect:
   - Property: Show PanelProperty
   - Event: Draw random event card, show PanelEventCard
   - Quiz: Show PanelQuiz
   - Jail: Set jail counter
   - Travel: Deduct money
8. End turn → Next player
```

### 3. Property Resolution:
```
If tile is Property:
  If no owner:
    Show "Buy" option
    If player buys:
      Deduct money (with Agility discount)
      Set owner
      Set level = 1
  Else if owner is current player:
    Show "Upgrade" option (if level < 5)
    If player upgrades:
      Deduct money
      Increase level
    If level == 5:
      Show "Upgrade to Hotel" option
  Else (owner is other player):
    Calculate rent (with Resistance discount for payer, Intelligence bonus for owner)
    Deduct money from current player
    Add money to owner
```

### 4. Quiz Resolution:
```
1. Load random question từ Firestore quizzes collection
2. Show PanelQuiz với 4 answers
3. Player select answer
4. Check correct:
   - If correct: Player can roll again (bonus turn)
   - If wrong: Skip next turn
```

### 5. Event Card Resolution:
```
1. Draw random event card (1-9)
2. Show PanelEventCard với image và description
3. Execute effect:
   - Money change: Add/subtract money
   - Thanh tra: Trigger quiz
   - Khế ước quỷ dữ: Show property selection UI
4. Close panel
```

### 6. Jail Resolution:
```
If player in jail:
  jailCounter--
  If jailCounter == 0:
    Release player
  Else:
    Show "Roll for double" option
    If player rolls double:
      Release player
      Move by dice sum
    Else:
      Stay in jail
```

### 7. Win Condition:
```
Game ends when:
- 1 player has all properties (monopoly)
- OR all other players bankrupt (money < 0)
- OR max rounds reached (e.g. 50 rounds)

Winner: Player with most money + property value
```

---

## 📊 Data Structures

### PlayerGameState:
```csharp
public class PlayerGameState
{
    public string uid;
    public string playerName;
    public string gender; // "male" or "female"
    public int money;
    public int currentTile;
    public int jailCounter;
    public bool skipNextTurn;
    
    // Stats từ loadout
    public int health;
    public int agility;
    public int intelligence;
    public int luck;
    public int resistance;
    
    // Properties owned
    public List<int> ownedProperties;
}
```

### PropertyData:
```csharp
public class PropertyData
{
    public int tileId;
    public string propertyName;
    public int level; // 1-5 or 6 (hotel)
    public string ownerId; // uid
    public int buyPrice;
    public int rentPrice;
}
```

### QuizData:
```csharp
public class QuizData
{
    public string question;
    public List<string> answers; // 4 answers
    public int correctIndex; // 0-3
}
```

### EventCardData:
```csharp
public class EventCardData
{
    public int cardId; // 1-9
    public string cardName;
    public string description;
    public string iconPath;
    public EventType type;
    public int moneyChange;
}
```

---

## 🔧 Technical Implementation

### Scripts Structure:
```
Assets/Scenes/Game/
├── GameManager.cs - Main game controller
├── BoardManager.cs - Board setup, waypoints
├── TurnManager.cs - Turn flow, player order
├── DiceController.cs - 2D dice roll animation
├── PlayerGameController.cs - Player movement, stats
├── PropertyManager.cs - Property buy/sell/upgrade
├── QuizManager.cs - Quiz loading, checking
├── EventCardManager.cs - Event card drawing, effects
├── UIManager.cs - UI panels control
└── NetworkSyncManager.cs - Multiplayer sync (Netcode)
```

### Firestore Collections:
```
quizzes/{quizId}
{
  question: string,
  answers: array<string>,
  correctIndex: number
}

game_sessions/{sessionId}
{
  players: array<PlayerGameState>,
  properties: map<tileId, PropertyData>,
  currentTurn: number,
  currentRound: number,
  gameState: "playing" | "ended"
}
```

---

**Total estimated time: ~40-60 hours development**

**Priority: Core gameplay → UI → Multiplayer sync → Polish**

