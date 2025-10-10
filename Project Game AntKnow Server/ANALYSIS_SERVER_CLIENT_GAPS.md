# 📊 PHÂN TÍCH KHOẢNG CÁCH GIỮA SERVER VÀ CLIENT

## 🔍 TÓM TẮT VẤN ĐỀ

### **VẤNĐỀ CHÍNH:**
1. **Server thiếu Lobby/Matchmaking system** → Client có nhưng Server không có
2. **Loadout không được load từ Firebase** → Server khởi tạo stats cứng (100,100,100,100,100)
3. **Không có logic chọn người đi trước** → Server bắt đầu với Player 1
4. **Dice roll không check Luck trước** → Roll trực tiếp không áp dụng stats
5. **Skill Card system chưa tích hợp vào gameplay** → Có code nhưng không gọi
6. **Turn counting không đúng quy tắc** → Thiếu logic 8 turns = 1 round quiz

---

## 📋 CHI TIẾT PHÂN TÍCH

### **1. LOBBY & MATCHMAKING SYSTEM**

#### ❌ **Server: THIẾU HOÀN TOÀN**
```
Project Game AntKnow Server/Assets/Script/Server/
├── ServerBootstrap.cs ✅ (Auto-start server, connection approval)
├── ServerGameManager.cs ✅ (Game logic)
└── ❌ THIẾU:
    ├── LobbyManager.cs
    ├── MatchmakingService.cs
    └── RelayService.cs
```

#### ✅ **Client: ĐẦY ĐỦ**
```
Project Game AntKnow/Assets/Script/Services/
├── LobbyService.cs ✅
├── MatchmakerService.cs ✅
├── RelayService.cs ✅
└── UGSAuthService.cs ✅
```

**📌 YÊU CẦU:**
- Server cần tích hợp UGS Lobby & Relay
- Server cần nhận thông tin từ Lobby: player names, loadouts, stats
- Server cần xác định thứ tự người chơi từ lobby data

---

### **2. LOADOUT & STATS INTEGRATION**

#### ❌ **Server: HARDCODED**
```csharp:Project Game AntKnow Server/Assets/Script/Server/ServerGameManager.cs
// Line 169-180: Khởi tạo stats CỨNG
var playerState = new PlayerState
{
    Id = playerId,
    Money = startingMoney,
    NodeIndex = 0,
    JailTurns = 0,
    Health = 100,          // ❌ HARDCODED
    Agility = 100,         // ❌ HARDCODED
    Intelligence = 100,    // ❌ HARDCODED
    Luck = 100,            // ❌ HARDCODED
    Resistance = 100       // ❌ HARDCODED
};
```

#### ✅ **Client: CÓ LOADOUT SYSTEM**
```csharp:Project Game AntKnow/Assets/Script/Game/GameSessionData.cs
public class GameSessionData
{
    public List<SkillCardData> skillCards;        // ✅ Skill cards
    public EquipmentSetData equipmentSet;         // ✅ Equipment
    public int totalHealth;                       // ✅ Calculated
    public int totalAgility;
    public int totalIntelligence;
    public int totalLuck;
    public int totalResistance;
}
```

**📌 YÊU CẦU:**
- Server cần nhận loadout từ Client khi kết nối
- Server cần lưu skill cards của từng player
- Server cần tính toán stats từ equipment + cards

---

### **3. CHỌN NGƯỜI ĐI TRƯỚC**

#### ❌ **Server: KHÔNG CÓ LOGIC**
```csharp:Project Game AntKnow Server/Assets/Script/Server/ServerGameManager.cs
// Line 137-194: StartGame → InitializeGameState
private void InitializeGameState()
{
    gameState = new GameState
    {
        BoardLength = boardLength,
        CurrentTurnPlayerId = 1  // ❌ Luôn là Player 1
    };
    // ...
}
```

#### ❌ **Client: CŨNG KHÔNG CÓ**
- Không có UI chọn người đi trước
- Không có logic roll xúc xắc để xác định thứ tự

**📌 YÊU CẦU:**
- Thêm phase "Chọn người đi trước":
  - Option 1: All players roll dice, highest goes first
  - Option 2: Host chọn
  - Option 3: Random
- Sắp xếp player order trong `PanelPlayer` & `PanelPlayerMe`

---

### **4. DICE ROLL VỚI LUCK CHECK**

#### ❌ **Server: KHÔNG CHECK LUCK**
```csharp:Project Game AntKnow Server/Assets/Script/Server/ServerGameManager.cs
// Line 306-309: Roll dice trực tiếp
int dice1 = Random.Range(1, 7);
int dice2 = Random.Range(1, 7);
// ❌ Không check Luck trước khi roll
```

#### ✅ **Client: CÓ HÀM HELPERS**
```csharp:Project Game AntKnow/Assets/Script/Domain/Services/StatsCalculator.cs
// Line 13-27: CheckLuckForDouble
public static bool CheckLuckForDouble(int luckStat, out int diceValue)
{
    int luckPct = luckStat / 10; // 10 pts = 1%
    float doubleChance = luckPct / 100f;
    
    if (UnityEngine.Random.value < doubleChance)
    {
        diceValue = UnityEngine.Random.Range(1, 7);
        return true; // Is double
    }
    diceValue = 0;
    return false;
}
```

**📌 YÊU CẦU:**
- Server cần check Luck TRƯỚC khi roll:
  1. Check Luck → trúng → roll 1 dice x2
  2. Không trúng → roll 2 dice bình thường

---

### **5. SKILL CARD INTEGRATION**

#### ⚠️ **Server: CÓ CODE NHƯNG KHÔNG DÙNG**
```
Project Game AntKnow Server/Assets/Script/Domain/Services/
├── SkillTriggerEngine.cs ✅ (Code hoàn chỉnh)
├── EventCardHandler.cs ✅ (Code hoàn chỉnh)
└── ❌ KHÔNG ĐƯỢC GỌI trong ServerGameManager.cs
```

#### ❌ **Thiếu logic:**
1. Trigger passive skills khi event xảy ra
2. Hiện panel chọn active skill sau mỗi turn
3. Cooldown management
4. Apply skill effects vào gameplay

**📌 YÊU CẦU:**
- Tích hợp `SkillTriggerEngine` vào turn flow:
  ```
  Roll Dice → Check Luck → Move → Resolve Tile
  → Trigger Passive Skills (onEnterOpponentHouse, etc.)
  → Show Active Skill Panel (if available)
  → End Turn
  ```

---

### **6. TURN & QUIZ SYSTEM**

#### ❌ **Server: LOGIC SAI**
```csharp:Project Game AntKnow Server/Assets/Script/Server/ServerGameManager.cs
// Line 17: maxTurns = 25
[SerializeField] private int maxTurns = 25;

// ❌ Không có logic: 8 turns = 1 round quiz
// ❌ Không track "vòng tròn" (all players = 1 turn)
```

**📌 YÊU CẦU:**
- Turn counting:
  - 1 Turn = Tất cả players đã đi 1 lượt
  - Mỗi 8 turns → Quiz round (all players answer quiz)
- Quiz penalty:
  - Đúng → Không penalty
  - Sai → Random penalty (lose money, downgrade house, skip 1 turn)

---

### **7. TILE RESOLUTION LOGIC**

#### ⚠️ **Client: ĐẦY ĐỦ HƠN SERVER**

**Client có:**
```csharp:Project Game AntKnow/Assets/Scenes/Game/Scripts/GameManager.cs
// Line 360-403: ResolveTile với 7 cases
- Ô trống → Panel Buy
- Ô Event → Panel Event
- Ô Tai Nạn → Jail 2 turns
- Ô nhà mình → Panel Upgrade
- Ô nhà người khác → Pay rent + Panel Takeover
- Ô Tra Khảo → Panel Quiz
- Ô Du Lịch → Panel Travel
```

**Server chỉ có:**
```csharp:Project Game AntKnow Server/Assets/Script/Server/ServerGameManager.cs
// Line 351-382: Switch với 5 cases
- Property
- Chance (Event) - "waiting for client"
- Quiz - "waiting for client"
- Jail
- Travel - "waiting for client"
```

**📌 VẤNĐỀ:**
- Server rely on Client để xử lý Event/Quiz/Travel
- Client có thể cheat vì logic ở client-side

**📌 YÊU CẦU:**
- Di chuyển ALL logic về Server
- Client chỉ hiện UI và gửi choice về Server

---

## ✅ NHỮNG GÌ ĐÃ TỐT

### **1. Server Architecture**
- ✅ Domain-Driven Design (DDD) rõ ràng
- ✅ Server-authoritative game state
- ✅ Clean separation: Domain vs Network layer
- ✅ SimpleBoardConfig với tile data từ CSV

### **2. Card Systems**
- ✅ SkillCardData structure hoàn chỉnh
- ✅ EventCardLibrary với 12 event types
- ✅ SkillTriggerEngine extensible
- ✅ Support passive/active skills

### **3. Stats System**
- ✅ 5 stats implemented: Health, Agility, Intelligence, Luck, Resistance
- ✅ Stats affect gameplay (rent, salary, etc.)
- ✅ StatsCalculator helpers

---

## 🎯 KẾ HOẠCH SỬA CHỮA (Priority Order)

### **CRITICAL (Phải có ngay)**

#### 1️⃣ **Server Lobby Integration** ⏱️ 3 hours
- [ ] Copy LobbyService, RelayService từ Client sang Server
- [ ] Server subscribe to Lobby events
- [ ] Load player data từ Lobby → GameState

#### 2️⃣ **Loadout System** ⏱️ 2 hours
- [ ] Client gửi loadout khi connect (ServerRpc)
- [ ] Server lưu loadout cho mỗi player
- [ ] Server calculate stats từ equipment + cards

#### 3️⃣ **Turn Order Selection** ⏱️ 1.5 hours
- [ ] Phase "Chọn người đi trước" (all roll dice)
- [ ] Sắp xếp players theo dice result
- [ ] UI: Hiện thứ tự trong PanelPlayer

#### 4️⃣ **Luck-Based Dice Roll** ⏱️ 1 hour
- [ ] Check Luck % trước khi roll
- [ ] Trúng → roll 1 dice x2
- [ ] Không trúng → roll 2 dice bình thường

### **HIGH (Cần sớm)**

#### 5️⃣ **Skill Card Integration** ⏱️ 4 hours
- [ ] Trigger passive skills khi resolve tile
- [ ] Panel chọn active skill sau move
- [ ] Cooldown tracking
- [ ] Apply skill effects

#### 6️⃣ **Turn & Quiz System** ⏱️ 2 hours
- [ ] Track "vòng tròn" (all players = 1 turn)
- [ ] Mỗi 8 turns → Quiz round
- [ ] Quiz penalties

#### 7️⃣ **Complete Tile Resolution** ⏱️ 3 hours
- [ ] Event Card (server-side random + apply)
- [ ] Quiz (server validates answer)
- [ ] Travel (server validates destination)
- [ ] Takeover logic
- [ ] Jail với double dice escape

### **MEDIUM (Cần sau)**

#### 8️⃣ **UI Synchronization** ⏱️ 2 hours
- [ ] Server notify → Client show panels
- [ ] Client choice → Server validate & apply
- [ ] Real-time UI updates

#### 9️⃣ **Stats Effects** ⏱️ 2 hours
- [ ] Intelligence → +% rent received
- [ ] Resistance → -% rent paid (cashback)
- [ ] Health → +% salary
- [ ] Agility → x2 rent chance

---

## 📦 FILES CẦN TẠO/SỬA

### **Server Side**
```
CREATE:
├── Assets/Script/Server/LobbyManager.cs
├── Assets/Script/Server/PlayerLoadoutData.cs
├── Assets/Script/Server/TurnOrderManager.cs
└── Assets/Script/Server/ServerRpcHandlers.cs

MODIFY:
├── Assets/Script/Server/ServerGameManager.cs
│   ├── Add LoadoutManager
│   ├── Add TurnOrderSelection
│   ├── Add LuckBasedDiceRoll
│   ├── Add SkillCardIntegration
│   └── Add CompleteTileResolution
└── Assets/Script/Domain/Services/TurnSystem.cs
    └── Add QuizRoundTracking
```

### **Client Side**
```
MODIFY:
├── Assets/Script/Game/GameManager.cs
│   ├── Send loadout to server
│   └── Handle UI only (no game logic)
├── Assets/Scenes/Game/Scripts/PlayerGameController.cs
│   └── Initialize from server data
└── Assets/Scenes/Game/Scripts/UI/PanelPlayer.cs
    └── Sort by turn order
```

---

## ⏱️ THỜI GIAN ƯỚC TÍNH

| Task | Time | Priority |
|------|------|----------|
| Server Lobby Integration | 3h | CRITICAL |
| Loadout System | 2h | CRITICAL |
| Turn Order Selection | 1.5h | CRITICAL |
| Luck-Based Dice Roll | 1h | CRITICAL |
| Skill Card Integration | 4h | HIGH |
| Turn & Quiz System | 2h | HIGH |
| Complete Tile Resolution | 3h | HIGH |
| UI Synchronization | 2h | MEDIUM |
| Stats Effects | 2h | MEDIUM |
| **TOTAL** | **20.5 hours** | - |

**📌 REALISTIC TIMELINE:**
- Day 1 (8h): Critical tasks (1-4) = **7.5h**
- Day 2 (8h): High tasks (5-7) = **9h** → Split to 2 days
- Day 3 (4.5h): Finish High + Medium = **4h**

**TOTAL: 3 days** để hoàn thiện Server-Client integration

---

## 🚨 NHỮNG ĐIỀU CẦN LƯU Ý

### **Security**
- ❌ KHÔNG để logic game ở Client
- ✅ Client CHỈ show UI và send choices
- ✅ Server VALIDATE mọi action

### **Data Flow**
```
Correct Flow:
Client → ServerRpc (choice only)
Server → Validate → Update GameState → ClientRpc (result)
Client → Show UI result

Wrong Flow (❌ HIỆN TẠI):
Client → Execute logic → Update UI
Server → "waiting for client" ❌❌❌
```

### **Testing**
- Test với 2, 3, 4 players
- Test disconnect/reconnect
- Test cheat detection
- Test các edge cases (jail, travel, quiz, etc.)

---

## 📌 NEXT STEPS

1. ✅ Đọc file này
2. ⏳ Confirm với user về priority
3. ⏳ Bắt đầu implement theo order
4. ⏳ Test từng feature khi xong
5. ⏳ Integration test cuối cùng

---

**Tạo bởi:** AI Assistant  
**Ngày:** 2025-10-10  
**Mục đích:** Thống nhất Server-Client Architecture cho AntKnow Game

