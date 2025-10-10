# 📋 MERGE PLAN: Script/ → Game/Scripts/

## 🎯 **MỤC TIÊU**
Gộp code từ `@Script/` sang `@Game/Scripts/` theo kiến trúc **DDD + MVC**, loại bỏ trùng lặp.

---

## 📊 **PHÂN TÍCH CẤU TRÚC**

### **@Script/** (Domain-Driven Design)
```
Script/
├── Domain/                    # Pure C# Domain Logic
│   ├── Entities/             # Domain Entities (PlayerState, PropertyState, GameState)
│   ├── Services/             # Domain Services (BoardRules, TurnSystem, CardRuleEngine, StatsCalculator)
│   └── Enums.cs              # Domain Enums
│
├── Data/                     # ScriptableObjects (BoardConfig, TileDef, PropertyRuleSet)
├── Presentation/             # Unity MonoBehaviours (GameController, PlayerController, WaypointPath)
├── Services/                 # Infrastructure Services (UGS, Relay, Lobby, Matchmaker)
├── Game/                     # Game Session Data
├── Integration/              # Firebase, Multiplayer
├── Multiplayer/              # Network Controllers
└── Legacy/                   # Old code
```

### **@Game/Scripts/** (Current Implementation)
```
Game/Scripts/
├── GameManager.cs            # Main game controller (MVC Controller)
├── BoardManager.cs           # Board management
├── PropertyManager.cs        # Property management
├── PlayerGameController.cs   # Player controller
├── SimpleBoardConfig.cs      # Hardcoded 36 tiles
├── StatsCalculator.cs        # Stats calculation (DUPLICATE with Script/Domain/Services/)
├── SkillCardEffects.cs       # Skill card effects
├── DiceController.cs         # Dice UI controller
├── UI/                       # UI Panels (View layer)
└── WaypointGenerator.cs      # Waypoint generation
```

---

## 🔍 **TRÙNG LẶP PHÁT HIỆN**

### **1. StatsCalculator.cs**
- ❌ **Script/Domain/Services/StatsCalculator.cs** (Domain)
- ✅ **Game/Scripts/StatsCalculator.cs** (Already copied)
- **ACTION**: Keep Game/Scripts/StatsCalculator.cs, delete Script version

### **2. Domain Services (BoardRules, TurnSystem, PropertyEconomy)**
- ❌ **Script/Domain/Services/** → Complex, unused in current Game/
- **ACTION**: KHÔNG dùng, Game/Scripts/ đã implement riêng

### **3. WaypointPath.cs**
- ❌ **Script/Presentation/WaypointPath.cs** (Old)
- ✅ **Game/Scripts/** (Implicit in BoardManager)
- **ACTION**: Check xem có cần copy không

### **4. GameController.cs vs GameManager.cs**
- ❌ **Script/Presentation/GameController.cs** → Old network implementation
- ✅ **Game/Scripts/GameManager.cs** → New implementation
- **ACTION**: Keep GameManager.cs

### **5. PlayerController.cs vs PlayerGameController.cs**
- ❌ **Script/Presentation/PlayerController.cs** → Old waypoint system
- ✅ **Game/Scripts/PlayerGameController.cs** → New implementation
- **ACTION**: Keep PlayerGameController.cs

---

## ✅ **CẦN GIỮ LẠI TỪ @Script/**

### **1. Services/** (Infrastructure) → **GIỮ NGUYÊN!**
```
Services/
├── UGSAuthService.cs         ✅ Cần thiết (UGS Authentication)
├── LobbyService.cs           ✅ Cần thiết (UGS Lobby)
├── RelayService.cs           ✅ Cần thiết (UGS Relay)
├── MatchmakerService.cs      ✅ Cần thiết (Matchmaking)
└── GameConfig.cs             ✅ Cần thiết (Constants)
```

**ACTION**: Di chuyển sang `Game/Scripts/Services/`

### **2. Game/GameSessionData.cs** → **GIỮ NGUYÊN!**
```
GameSessionData.cs            ✅ Cần thiết (Player loadout, session data)
```

**ACTION**: Di chuyển sang `Game/Scripts/Data/`

### **3. Data/** (ScriptableObjects) → **XEM XÉT**
```
Data/
├── BoardConfig.asset/.cs     ❓ Unused (SimpleBoardConfig.cs đã thay thế)
├── TileDef.asset/.cs         ❓ Unused
└── PropertyRuleSet.asset/.cs ❓ Unused
```

**ACTION**: KHÔNG cần thiết, đã hardcode trong SimpleBoardConfig.cs

---

## 🗑️ **CẦN XÓA**

### **1. Legacy/** → **XÓA TOÀN BỘ**
- BaseScript.cs, DiceCheckZoneScript.cs, Player1Script.cs, Player2Script.cs
- **Old demo code, không dùng nữa**

### **2. Presentation/** → **XÓA (đã có trong Game/)**
- GameController.cs → Replaced by GameManager.cs
- PlayerController.cs → Replaced by PlayerGameController.cs
- BoardView.cs, DiceView.cs → Không dùng

### **3. Multiplayer/** → **XÓA (đã tích hợp vào GameManager.cs)**
- NetworkGameController.cs → Old network implementation
- NetworkGameManager.cs → Old network implementation
- NetworkPlayerController.cs → Old network implementation

### **4. Domain/** → **XÓA (không dùng trong current implementation)**
- Entities/, Services/ → Too complex, không cần thiết
- **Game/Scripts/ đã implement đơn giản hơn**

### **5. Integration/** → **XEM XÉT**
- FirebaseAuthController.cs → ❓ Check xem có dùng không
- FirebaseQuizService.cs → ❓ Check xem có dùng không
- MultiplayerManager.cs → ❌ Old, không dùng

### **6. Data/** → **XÓA (đã hardcode)**
- BoardConfig, TileDef, PropertyRuleSet → Không cần thiết

### **7. Documentation/** → **GIỮ**
- MultiplayerSetupGuide.cs, UnityPackageFixGuide.cs → Hữu ích

---

## 📁 **CẤU TRÚC MỚI TRONG @Game/Scripts/**

```
Game/Scripts/
├── Core/                     # Core game logic
│   ├── GameManager.cs       ✅ Main controller
│   ├── BoardManager.cs      ✅ Board management
│   ├── PropertyManager.cs   ✅ Property management
│   └── SimpleBoardConfig.cs ✅ 36 tiles hardcoded
│
├── Player/                   # Player logic
│   ├── PlayerGameController.cs    ✅ Player controller
│   └── TurnIndicator.cs          ✅ Turn indicator
│
├── Services/                 # Infrastructure services
│   ├── UGSAuthService.cs    ✅ (from Script/Services/)
│   ├── LobbyService.cs      ✅ (from Script/Services/)
│   ├── RelayService.cs      ✅ (from Script/Services/)
│   ├── MatchmakerService.cs ✅ (from Script/Services/)
│   └── GameConfig.cs        ✅ (from Script/Services/)
│
├── Data/                     # Data layer
│   └── GameSessionData.cs   ✅ (from Script/Game/)
│
├── Utils/                    # Utilities
│   ├── StatsCalculator.cs   ✅ Already here
│   ├── SkillCardEffects.cs  ✅ Already here
│   └── WaypointGenerator.cs ✅ Already here
│
├── Visual/                   # Visual layer
│   ├── PropertyVisual.cs    ✅ Already here
│   ├── TileVisual.cs        ✅ Already here
│   ├── TileSetup.cs         ✅ Already here
│   └── DiceController.cs    ✅ Already here
│
└── UI/                       # UI layer (View)
    ├── PanelBuy.cs          ✅ Already here
    ├── PanelQuiz.cs         ✅ Already here
    ├── PanelEvent.cs        ✅ Already here
    ├── PanelCard.cs         ✅ Already here
    ├── PanelHouseSell.cs    ✅ Already here
    ├── PanelResult.cs       ✅ Already here
    ├── PanelPlayer.cs       ✅ Already here
    ├── PanelPlayerMe.cs     ✅ Already here
    ├── CardButton.cs        ✅ Already here
    └── PropertySellItem.cs  ✅ Already here
```

---

## 🚀 **ACTION PLAN**

### **Phase 1: Di chuyển Services** ✅
1. Tạo folder `Game/Scripts/Services/`
2. Copy:
   - UGSAuthService.cs
   - LobbyService.cs
   - RelayService.cs
   - MatchmakerService.cs
   - GameConfig.cs

### **Phase 2: Di chuyển Data** ✅
1. Tạo folder `Game/Scripts/Data/`
2. Copy:
   - GameSessionData.cs

### **Phase 3: Tổ chức lại Game/Scripts/** ✅
1. Tạo các folders:
   - Core/
   - Player/
   - Utils/
   - Visual/
2. Move files vào đúng folders

### **Phase 4: Xóa @Script/ folder** ✅
1. Xóa toàn bộ `Script/` folder
2. Chỉ giữ lại **Documentation/** nếu cần

---

## ✅ **KẾT QUẢ MONG ĐỢI**

- ✅ Tất cả code gameplay trong `Game/Scripts/`
- ✅ Cấu trúc rõ ràng theo MVC + DDD
- ✅ Không còn trùng lặp
- ✅ Dễ bảo trì và mở rộng

---

**STATUS**: ⏳ READY TO EXECUTE

