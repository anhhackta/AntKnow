# 🏗️ Architecture Analysis - Phân Tích Kiến Trúc

## 📊 Hiện Trạng: 2 Systems Song Song

### **System A: Domain-Driven Architecture**
Location: `Assets/Script/`

```
Domain Layer (Pure C#)
├── Entities/
│   ├── GameState.cs
│   ├── PlayerState.cs
│   └── PropertyState.cs
├── Services/
│   ├── TurnSystem.cs
│   ├── BoardRules.cs
│   ├── PropertyEconomy.cs
│   ├── DiceRng.cs
│   └── CardRuleEngine.cs
└── Enums.cs

Data Layer (ScriptableObjects)
├── BoardConfig.cs
├── TileDef.cs
├── PropertyRuleSet.cs
└── CardLibrary.cs

Presentation Layer (MonoBehaviours)
├── GameController.cs (NetworkBehaviour)
├── PlayerController.cs
├── WaypointPath.cs
├── BoardView.cs
└── DiceView.cs
```

**Ưu điểm:**
- ✅ Clean Architecture (Domain ↔ Data ↔ Presentation)
- ✅ Testable (Domain logic không phụ thuộc Unity)
- ✅ Maintainable (Tách biệt concerns)
- ✅ Scalable (Dễ mở rộng)
- ✅ Network-ready (GameController có NetworkBehaviour)

**Nhược điểm:**
- ⚠️ Phức tạp hơn
- ⚠️ Cần hiểu Domain-Driven Design
- ⚠️ Chưa hoàn thiện (còn placeholders)

---

### **System B: Direct Approach**
Location: `Assets/Scenes/Game/Scripts/`

```
Game Scripts (MonoBehaviours)
├── GameManager.cs (NetworkBehaviour)
├── BoardManager.cs
├── PlayerGameController.cs
├── PropertyManager.cs
├── DiceController.cs
├── TurnIndicator.cs
└── PropertyVisual.cs

UI Scripts
├── PanelBuy.cs
├── PanelQuiz.cs
├── PanelEvent.cs
├── PanelHouseSell.cs
├── PanelResult.cs
├── PanelCard.cs
└── PanelPlayerMe.cs
```

**Ưu điểm:**
- ✅ Đơn giản, trực tiếp
- ✅ Dễ hiểu cho beginners
- ✅ Nhanh implement
- ✅ Có nhiều features đã implement

**Nhược điểm:**
- ⚠️ Tight coupling (Logic + UI + Data)
- ⚠️ Khó test (Phụ thuộc Unity)
- ⚠️ Khó maintain (Logic rải rác)
- ⚠️ Khó scale (Thêm features = thêm complexity)

---

## 🎯 So Sánh Chi Tiết

### **GameController.cs vs GameManager.cs**

| Feature | GameController.cs | GameManager.cs |
|---------|-------------------|----------------|
| **Architecture** | Domain-Driven | Direct |
| **Network** | NetworkBehaviour ✅ | NetworkBehaviour ✅ |
| **Domain Layer** | Uses GameState, TurnSystem ✅ | No ❌ |
| **Player Management** | PlayerController[] | PlayerGameController[] |
| **Turn System** | TurnSystem.cs | Built-in |
| **Property Logic** | BoardRules.cs | PropertyManager.cs |
| **Dice Rolling** | DiceRng.cs | DiceController.cs |
| **Stats Calculation** | StatsCalculator.cs | Built-in |
| **Card System** | CardRuleEngine.cs | Not implemented |
| **Quiz System** | Not implemented | Not implemented |
| **UI Integration** | Minimal | Extensive |
| **Demo Mode** | No | Yes ✅ |
| **Completeness** | 60% | 70% |

---

## 🔍 Code Comparison

### **Turn System**

#### GameController.cs (Domain-Driven):
```csharp
// Uses TurnSystem from Domain layer
public void OnRoll() {
    if (IsServer) {
        var roll = _turnSystem.Roll((min, max) => UnityEngine.Random.Range(min, max));
        _lastDiceRoll.Value = new DiceRollData { Die1 = roll.die1, Die2 = roll.die2 };
        StartCoroutine(ServerResolveTurn(playerId, _lastDiceRoll.Value));
    }
}

IEnumerator ServerResolveTurn(int playerId, DiceRollData roll) {
    int steps = roll.Die1 + roll.Die2;
    _turnSystem.MoveAndResolve(steps); // Domain logic
    _turnSystem.EndTurn();
}
```

#### GameManager.cs (Direct):
```csharp
// Direct implementation
private void OnRollButtonClicked() {
    if (isRolling) return;
    StartCoroutine(RollAndMove());
}

private IEnumerator RollAndMove() {
    int diceResult = diceController.RollDice();
    yield return new WaitForSeconds(1f);
    
    PlayerGameController player = CurrentPlayer;
    yield return StartCoroutine(player.MoveBySteps(diceResult));
    
    ResolveTile(player.CurrentTile);
    EndTurn();
}
```

**Analysis:**
- GameController: Cleaner, uses Domain logic, network-ready
- GameManager: More direct, easier to understand, has demo mode

---

### **Property System**

#### GameController.cs (Domain-Driven):
```csharp
public void OnBuyCurrent() {
    if (IsServer) {
        var player = _serverGame.Players.Find(x => x.Id == playerId);
        var property = _serverGame.Properties[player.NodeIndex];
        
        if (BoardRules.CanBuy(player, property)) {
            BoardRules.Buy(player, property); // Domain logic
            SyncNetworkStateFromDomain();
        }
    }
}
```

#### GameManager.cs (Direct):
```csharp
private void ResolvePropertyTile(int tileId) {
    if (!propertyManager.IsPropertyOwned(tileId)) {
        // Auto buy (demo mode)
        if (CurrentPlayer.Money >= 500) {
            propertyManager.BuyProperty(tileId, currentPlayerIndex, 500, CurrentPlayer);
        }
    } else {
        // Pay rent
        int ownerId = propertyManager.GetPropertyOwner(tileId);
        int rent = propertyManager.CalculateRent(tileId, players[ownerId]);
        CurrentPlayer.SubtractMoney(rent);
        players[ownerId].AddMoney(rent);
    }
}
```

**Analysis:**
- GameController: Uses BoardRules, cleaner separation
- GameManager: Direct implementation, has auto-buy for demo

---

## 💡 Recommendation: Hybrid Approach

### **Strategy: Merge Best of Both Worlds**

#### Phase 1: Keep GameManager, Add Domain Layer
```
GameManager.cs (Main Controller)
├── Uses Domain Layer for logic
│   ├── GameState (state management)
│   ├── TurnSystem (turn logic)
│   ├── BoardRules (property logic)
│   └── CardRuleEngine (card logic)
├── Keeps existing features
│   ├── Demo mode
│   ├── UI integration
│   └── Visual feedback
└── Add NetworkBehaviour features
    ├── Network variables
    ├── RPCs
    └── Client-server sync
```

#### Benefits:
- ✅ Keep working features from GameManager
- ✅ Add clean architecture from Domain layer
- ✅ Easier migration path
- ✅ Less refactoring needed

---

## 📋 Migration Plan

### **Step 1: Integrate Domain Layer into GameManager**

1. **Add Domain References**
   ```csharp
   public class GameManager : NetworkBehaviour {
       [Header("Domain")]
       [SerializeField] private BoardConfig boardConfig;
       [SerializeField] private PropertyRuleSet propertyRules;
       [SerializeField] private CardLibrary cardLibrary;
       
       private GameState gameState;
       private TurnSystem turnSystem;
       private PropertyEconomy propertyEconomy;
       private CardRuleEngine cardRuleEngine;
   }
   ```

2. **Initialize Domain Objects**
   ```csharp
   private void InitializeDomain() {
       gameState = new GameState();
       propertyEconomy = new PropertyEconomy(propertyRules);
       turnSystem = new TurnSystem(gameState, ...);
       cardRuleEngine = new CardRuleEngine(cardLibrary);
   }
   ```

3. **Replace Direct Logic with Domain Logic**
   ```csharp
   // Before:
   private void BuyProperty(int tileId) {
       propertyManager.BuyProperty(tileId, ...);
   }
   
   // After:
   private void BuyProperty(int tileId) {
       var player = gameState.Players[currentPlayerIndex];
       var property = gameState.Properties[tileId];
       
       if (BoardRules.CanBuy(player, property)) {
           BoardRules.Buy(player, property);
           UpdateVisuals(); // Sync visuals with domain state
       }
   }
   ```

### **Step 2: Keep Existing Features**

- ✅ Keep demo mode
- ✅ Keep UI panels
- ✅ Keep visual feedback
- ✅ Keep player spawning
- ✅ Keep turn indicator

### **Step 3: Add Network Layer (Phase 2)**

- Add NetworkVariables for game state
- Add RPCs for player actions
- Add client-server synchronization

---

## 🎯 Final Architecture

```
GameManager.cs (Unified Controller)
├── Domain Layer (Logic)
│   ├── GameState
│   ├── TurnSystem
│   ├── BoardRules
│   ├── PropertyEconomy
│   └── CardRuleEngine
├── Presentation Layer (Unity)
│   ├── BoardManager
│   ├── PlayerGameController
│   ├── PropertyManager (Visual only)
│   ├── DiceController
│   └── TurnIndicator
├── UI Layer
│   ├── PanelBuy
│   ├── PanelQuiz
│   ├── PanelEvent
│   └── Other panels
└── Network Layer (Phase 2)
    ├── NetworkVariables
    ├── RPCs
    └── Client-Server sync
```

---

## ✅ Decision: Hybrid Approach

**Rationale:**
1. Keep working code from GameManager
2. Add clean architecture from Domain layer
3. Easier to implement and test
4. Smoother migration to multiplayer
5. Best of both worlds

**Next Steps:**
1. ✅ Create migration plan
2. ✅ Integrate Domain layer into GameManager
3. ✅ Test offline gameplay
4. ✅ Add missing features (cards, quiz, special tiles)
5. ✅ Prepare for multiplayer (Phase 2)

---

**Version**: 1.0  
**Date**: 2025-10-08  
**Status**: Analysis Complete ✅

