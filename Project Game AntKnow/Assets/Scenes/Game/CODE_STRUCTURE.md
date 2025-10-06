# 📁 Code Structure - Cấu Trúc Code

## 🎯 Hiện Trạng:

Game đang được thiết kế cho **MULTIPLAYER ONLINE** từ đầu, nhưng bây giờ cần test **LOCAL** trước.

---

## 📦 Scripts Chính:

### 1. GameManager.cs
```
Vai trò: Điều khiển toàn bộ game flow
Kế thừa: NetworkBehaviour (cho multiplayer)

Chế độ:
- Demo Mode = TRUE: Spawn 2 test players (LOCAL)
- Demo Mode = FALSE: Load players from lobby (ONLINE)

Methods quan trọng:
- Start() → StartGame()
- StartGame() → Spawn players
- StartTurn() → Enable roll button
- OnRollButtonClicked() → RollAndMove()
- ResolveTile() → Xử lý tile effect
- ResolvePropertyTile() → Mua/thuê nhà
```

### 2. PlayerGameController.cs
```
Vai trò: Điều khiển 1 player
Kế thừa: MonoBehaviour (không phải NetworkBehaviour)

Properties:
- PlayerName, Money, CurrentTile
- Stats: Health, Agility, Intelligence, Luck, Resistance
- JailCounter, SkipNextTurn

Methods:
- Initialize() → Setup player
- MoveBySteps() → Di chuyển với bounce effect
- AddMoney() / SubtractMoney()
- ShowTurnIndicator() / HideTurnIndicator()
```

### 3. BoardManager.cs
```
Vai trò: Quản lý 36 tiles
Kế thừa: MonoBehaviour

Methods:
- GetWaypointPosition(index) → Vị trí tile
- GetTileType(index) → Loại tile (Start, Property, Event, etc.)
- GetTileName(index) → Tên tile
- GetTilePrice(index) → Giá tile (500 for all properties)
```

### 4. PropertyManager.cs
```
Vai trò: Quản lý property system
Kế thừa: MonoBehaviour

Data:
- propertyOwners: Dictionary<tileId, playerIndex>
- propertyLevels: Dictionary<tileId, level>
- propertyRentMultipliers: Dictionary<tileId, multiplier>

Methods:
- BuyProperty() → Mua nhà
- UpgradeProperty() → Nâng cấp
- PayRent() → Trả tiền thuê
- CalculateRent() → Tính tiền thuê
```

### 5. DiceController.cs
```
Vai trò: Xúc xắc
Kế thừa: MonoBehaviour

Methods:
- RollDice(luckStat) → Roll với Luck effect
- LastSum → Tổng 2 xúc xắc
```

### 6. TurnIndicator.cs
```
Vai trò: Ping indicator trên đầu player
Kế thừa: MonoBehaviour

Methods:
- Show() → Hiện ping
- Hide() → Ẩn ping
- Update() → Bob up/down animation
```

---

## 🎮 Game Flow (LOCAL Mode):

```
1. Start()
   ↓
2. StartGame()
   ↓
3. SpawnTestPlayer("Player 1") - Demo mode
   SpawnTestPlayer("Player 2") - Demo mode
   ↓
4. StartTurn()
   ↓
5. UpdateTurnIndicators() - Show ping on current player
   ↓
6. Enable Roll Button
   ↓
7. Player clicks Roll
   ↓
8. OnRollButtonClicked()
   ↓
9. RollAndMove()
   - Roll dice
   - Move player
   - ResolveTile()
   ↓
10. ResolveTile()
    - Check tile type
    - If Property → ResolvePropertyTile()
    ↓
11. ResolvePropertyTile()
    - If not owned → Auto buy
    - If owned by other → Pay rent
    - If owned by self → (TODO: Show upgrade panel)
    ↓
12. EndTurn()
    ↓
13. Next player → StartTurn()
```

---

## 🔄 Multiplayer vs Local:

### LOCAL (Demo Mode = TRUE):
```
✅ Không cần NetworkManager
✅ Không cần Lobby
✅ Không cần GameSessionData
✅ Spawn 2 test players
✅ Auto buy/rent
✅ Chạy được ngay
```

### ONLINE (Demo Mode = FALSE):
```
❌ Cần NetworkManager.Singleton
❌ Cần Lobby connection
❌ Cần GameSessionData
❌ Load players from lobby
❌ Network synchronization
❌ Phức tạp hơn nhiều
```

---

## 🎯 Vấn Đề Hiện Tại:

### GameManager kế thừa NetworkBehaviour:
```
Problem: NetworkBehaviour cần NetworkManager
Solution: Giữ nguyên, nhưng dùng Demo Mode = TRUE để test LOCAL
```

### PlayerGameController không phải NetworkBehaviour:
```
Problem: Không sync được qua network
Solution: OK cho LOCAL, sau này sẽ tạo NetworkPlayerController
```

### PropertyManager không phải NetworkBehaviour:
```
Problem: Property data không sync qua network
Solution: OK cho LOCAL, sau này sẽ add NetworkVariables
```

---

## 🚀 Roadmap:

### Phase 1: LOCAL GAME (Hiện tại)
```
1. ✅ Setup GameManager với Demo Mode = TRUE
2. ✅ Test 1-2 players di chuyển
3. ✅ Test mua nhà
4. ✅ Test trả tiền thuê
5. ⏳ Add UI panels (PanelBuy, etc.)
6. ⏳ Add manual buy/upgrade
7. ⏳ Add special tiles
8. ⏳ Add end game
```

### Phase 2: MULTIPLAYER (Sau)
```
1. ⏳ Tạo NetworkPlayerController
2. ⏳ Add NetworkVariables to PropertyManager
3. ⏳ Add ServerRpc / ClientRpc methods
4. ⏳ Integrate với Lobby
5. ⏳ Test với 2-4 clients
6. ⏳ Deploy server
```

---

## 💡 Lưu Ý:

### Để test LOCAL:
```
1. Set Demo Mode = TRUE trong GameManager
2. Press Play
3. Game sẽ spawn 2 test players
4. Không cần lobby/network
```

### Để test ONLINE (sau này):
```
1. Set Demo Mode = FALSE
2. Start từ MenuScene
3. Join lobby
4. Load GameScene
5. Players sync qua network
```

---

## 📋 Files Quan Trọng:

### Core:
```
GameManager.cs - Main controller
PlayerGameController.cs - Player logic
BoardManager.cs - Board logic
PropertyManager.cs - Property logic
DiceController.cs - Dice logic
TurnIndicator.cs - Turn indicator
```

### UI (Chưa integrate):
```
PanelPlayerMe.cs - Player info panel
PanelBuy.cs - Buy/upgrade panel
PanelQuiz.cs - Quiz panel
PanelEvent.cs - Event panel
PanelHouseSell.cs - Sell panel
PanelResult.cs - Result panel
```

### Domain (Backend logic):
```
Assets/Script/Domain/Services/
- StatsCalculator.cs - Stats calculations
- PropertyEconomy.cs - Property formulas
- BoardRules.cs - Game rules
```

---

**Focus: Làm LOCAL game chạy được trước! 🎮**

