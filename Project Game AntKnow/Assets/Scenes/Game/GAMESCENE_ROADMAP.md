# 🎮 GameScene Development Roadmap - Kế Hoạch Chi Tiết

## 📊 Phân Tích Hiện Trạng

### ✅ Đã Có (Existing Components)

#### **Domain Layer** (Pure C# Logic)
- ✅ `GameState.cs` - Core game state
- ✅ `PlayerState.cs` - Player data structure
- ✅ `PropertyState.cs` - Property data structure
- ✅ `TurnSystem.cs` - Turn management logic
- ✅ `BoardRules.cs` - Game rules (buy, upgrade, rent calculation)
- ✅ `PropertyEconomy.cs` - Economic calculations
- ✅ `DiceRng.cs` - Deterministic dice rolling
- ✅ `CardRuleEngine.cs` - Card effect logic
- ✅ `StatsCalculator.cs` - Stats calculation

#### **Data Layer** (ScriptableObjects)
- ✅ `BoardConfig.cs` - 36 tiles configuration
- ✅ `TileDef.cs` - Tile definition
- ✅ `PropertyRuleSet.cs` - Property rules (upgrade costs, rent percentages)
- ✅ `CardLibrary.cs` - Card definitions

#### **Presentation Layer** (Unity MonoBehaviours)
- ✅ `GameController.cs` - Bridge Domain ↔ UI (có NetworkBehaviour)
- ✅ `PlayerController.cs` - Player movement
- ✅ `WaypointPath.cs` - Waypoint system
- ✅ `BoardView.cs` - Board visualization (placeholder)
- ✅ `DiceView.cs` - Dice animation (placeholder)

#### **Game Scene Scripts** (Local Implementation)
- ✅ `GameManager.cs` - Main game controller (có NetworkBehaviour)
- ✅ `BoardManager.cs` - Board management
- ✅ `PlayerGameController.cs` - Player controller với stats
- ✅ `PropertyManager.cs` - Property management
- ✅ `DiceController.cs` - Dice controller
- ✅ `TurnIndicator.cs` - Turn indicator visual
- ✅ `PropertyVisual.cs` - Property visualization

#### **UI Panels** (Đã có nhưng chưa integrate)
- ✅ `PanelBuy.cs` - Buy property panel
- ✅ `PanelQuiz.cs` - Quiz panel
- ✅ `PanelEvent.cs` - Event panel
- ✅ `PanelHouseSell.cs` - Sell house panel
- ✅ `PanelResult.cs` - End game result
- ✅ `PanelCard.cs` - Card panel
- ✅ `PanelPlayerMe.cs` - Player info panel

#### **Multiplayer** (Đã có nhưng chưa hoàn thiện)
- ✅ `NetworkGameController.cs` - Network game controller
- ✅ `NetworkPlayerController.cs` - Network player controller
- ✅ `NetworkGameManager.cs` - Network manager

### ⚠️ Vấn Đề Hiện Tại (Current Issues)

1. **Duplicate Systems**: Có 2 bộ scripts song song:
   - `Assets/Script/Presentation/GameController.cs` (Domain-driven, có Network)
   - `Assets/Scenes/Game/Scripts/GameManager.cs` (Scene-specific, có Network)
   
2. **Architecture Confusion**: 
   - GameController sử dụng Domain layer (GameState, TurnSystem, BoardRules)
   - GameManager sử dụng direct approach (không dùng Domain layer)
   
3. **Network Integration**: 
   - Cả 2 systems đều kế thừa NetworkBehaviour
   - Chưa rõ system nào sẽ dùng cho multiplayer
   
4. **Incomplete Features**:
   - ❌ House/Hotel visual models chưa spawn
   - ❌ Card system chưa integrate
   - ❌ Quiz system chưa integrate
   - ❌ Special tiles chưa xử lý đầy đủ
   - ❌ UI panels chưa connect với game logic
   - ❌ End game logic chưa có

---

## 🎯 Chiến Lược Phát Triển (Development Strategy)

### **Quyết Định Kiến Trúc**

**Chọn 1 trong 2 approaches:**

#### **Option A: Domain-Driven (Recommended)** ⭐
- Sử dụng `GameController.cs` + Domain layer
- Ưu điểm: Clean architecture, dễ test, dễ maintain
- Nhược điểm: Cần refactor GameManager.cs

#### **Option B: Direct Approach**
- Sử dụng `GameManager.cs` + Scene scripts
- Ưu điểm: Đơn giản, trực tiếp
- Nhược điểm: Khó maintain, khó test, khó scale

**→ Recommendation: Chọn Option A (Domain-Driven)**

---

## 📋 Phase 1: Hoàn Thiện Offline Gameplay (4-6 tuần)

### **1.1: Refactor và Tối Ưu Hóa Core Systems** (1 tuần)

#### Mục tiêu:
- Merge 2 systems thành 1 unified system
- Sử dụng Domain-driven architecture
- Đảm bảo offline mode hoạt động hoàn hảo

#### Tasks:
1. **Analyze Current Code**
   - So sánh GameController.cs vs GameManager.cs
   - Xác định features nào cần giữ lại
   - Lập danh sách refactor

2. **Create Unified GameController**
   - Merge logic từ GameManager vào GameController
   - Giữ lại Domain layer integration
   - Add demo mode flag (offline/online)

3. **Refactor PlayerController**
   - Merge PlayerGameController features vào PlayerController
   - Add stats (Health, Agility, Intelligence, Luck, Resistance)
   - Keep movement system

4. **Update BoardManager**
   - Integrate với BoardConfig (Domain)
   - Ensure 36 tiles setup
   - Add tile type detection

5. **Testing**
   - Test basic movement
   - Test dice rolling
   - Test turn system

**Deliverables:**
- ✅ Unified GameController
- ✅ Clean architecture
- ✅ Working offline mode

---

### **1.2: Hoàn Thiện Property System** (1 tuần)

#### Mục tiêu:
- Implement đầy đủ buy/upgrade/hotel mechanics
- Spawn 3D models cho houses và hotels
- Tính toán rent theo stats

#### Tasks:
1. **Property Purchase Flow**
   - Show PanelBuy khi land on unowned property
   - Handle buy action
   - Update property ownership
   - Deduct money

2. **House Building System**
   - Add "Upgrade" button in PanelBuy
   - Check conditions (owner, money, level < 4)
   - Spawn house models (level 1-4)
   - Update PropertyVisual

3. **Hotel Upgrade System**
   - Check level == 4 condition
   - Show hotel upgrade option
   - Spawn hotel model (replace 4 houses)
   - Apply Agility bonus (double rent chance)

4. **Rent Calculation**
   - Use PropertyEconomy formulas
   - Apply Intelligence bonus (owner)
   - Apply Resistance reduction (tenant)
   - Show rent payment UI

5. **Visual Feedback**
   - Spawn house prefabs on tiles
   - Spawn hotel prefabs on tiles
   - Color-code by owner
   - Add animations

**Deliverables:**
- ✅ Full property system
- ✅ 3D house/hotel models
- ✅ Stats-based calculations

---

### **1.3: Implement Card System** (1 tuần)

#### Mục tiêu:
- Integrate CardLibrary và CardRuleEngine
- Implement passive và active cards
- Add cooldown system

#### Tasks:
1. **Load Player Loadout**
   - Get cards from GameSessionData
   - Initialize card states
   - Setup cooldowns

2. **Passive Card Effects**
   - Apply stat bonuses
   - Apply continuous effects
   - Trigger on specific events

3. **Active Card Usage**
   - Show PanelCard with available cards
   - Handle card activation
   - Apply card effects
   - Start cooldown

4. **Card Triggers**
   - OnTurnStart
   - OnLandOnTile
   - OnBuyProperty
   - OnPayRent
   - OnPassStart

5. **UI Integration**
   - Show active cards in PanelPlayerMe
   - Show cooldown timers
   - Highlight usable cards

**Deliverables:**
- ✅ Working card system
- ✅ Passive + Active cards
- ✅ Cooldown management

---

### **1.4: Implement Quiz System** (3-4 ngày)

#### Mục tiêu:
- Integrate Firebase Quiz Service
- Show quiz when landing on Quiz tiles
- Handle rewards/penalties

#### Tasks:
1. **Quiz Tile Detection**
   - Detect Quiz tile type
   - Pause game flow
   - Show PanelQuiz

2. **Load Quiz Questions**
   - Call FirebaseQuizService
   - Get random question
   - Display question + answers

3. **Answer Handling**
   - Check correct/wrong
   - Apply rewards (money, bonus turn)
   - Apply penalties (lose money, skip turn)
   - Resume game flow

4. **UI Polish**
   - Timer for answers
   - Visual feedback
   - Sound effects

**Deliverables:**
- ✅ Working quiz system
- ✅ Firebase integration
- ✅ Rewards/penalties

---

### **1.5: Implement Special Tiles** (3-4 ngày)

#### Mục tiêu:
- Xử lý tất cả special tiles theo TileType enum

#### Tasks:
1. **Start Tile**
   - Give salary (200 + Health bonus)
   - Show notification

2. **Jail Tile**
   - Visit mode (just passing)
   - Imprisoned mode (from GoToJail)
   - Escape options (pay, roll double)

3. **Travel Tile**
   - Show destination options
   - Teleport player
   - No salary if not passing Start

4. **Event Tile**
   - Draw random card from CardLibrary
   - Apply card effect
   - Show PanelEvent

5. **Tax/Bonus Tiles**
   - Apply fixed amount
   - Apply Resistance reduction (Tax)
   - Show notification

6. **GoToJail Tile**
   - Teleport to Jail
   - Set jail turns = 2
   - Show animation

7. **FreeParking Tile**
   - No effect (rest tile)

**Deliverables:**
- ✅ All special tiles working
- ✅ Proper game flow
- ✅ Visual feedback

---

### **1.6: Hoàn Thiện UI Panels** (3-4 ngày)

#### Mục tiêu:
- Connect tất cả UI panels với game logic
- Ensure smooth UX

#### Tasks:
1. **PanelBuy**
   - Show property info
   - Buy button
   - Upgrade button (if owner)
   - Cancel button

2. **PanelQuiz**
   - Question display
   - Answer buttons
   - Timer
   - Result feedback

3. **PanelEvent**
   - Event description
   - Card effect
   - OK button

4. **PanelHouseSell**
   - List owned properties
   - Show sell price
   - Confirm sell

5. **PanelResult**
   - Final rankings
   - Player stats
   - Rewards
   - Back to menu button

6. **PanelCard**
   - Show active cards
   - Cooldown display
   - Use card button

7. **PanelPlayerMe**
   - Player info (money, stats)
   - Owned properties
   - Active cards

**Deliverables:**
- ✅ All panels working
- ✅ Smooth UX
- ✅ Visual polish

---

### **1.7: Implement End Game Logic** (2-3 ngày)

#### Mục tiêu:
- Handle game end conditions
- Calculate final scores
- Save results to Firebase

#### Tasks:
1. **End Conditions**
   - Max turns reached (25 turns)
   - Player bankruptcy (money < 0)
   - Only 1 player remaining

2. **Score Calculation**
   - Total money
   - Property values
   - Card bonuses
   - Ranking

3. **Result Display**
   - Show PanelResult
   - Display rankings
   - Show rewards (AntCoin, DCoin, EXP)

4. **Save to Firebase**
   - Update player stats
   - Save match history
   - Update leaderboard

**Deliverables:**
- ✅ End game logic
- ✅ Score calculation
- ✅ Firebase integration

---

### **1.8: Testing & Bug Fixes** (1 tuần)

#### Mục tiêu:
- Test toàn bộ gameplay
- Fix bugs
- Optimize performance

#### Tasks:
1. **Functional Testing**
   - Test all game flows
   - Test edge cases
   - Test UI interactions

2. **Bug Fixes**
   - Fix movement bugs
   - Fix property bugs
   - Fix UI bugs

3. **Performance Optimization**
   - Optimize rendering
   - Optimize calculations
   - Reduce memory usage

4. **Polish**
   - Add animations
   - Add sound effects
   - Add visual feedback

**Deliverables:**
- ✅ Stable offline game
- ✅ No critical bugs
- ✅ Good performance

---

## 📋 Phase 2: Multiplayer Online (3-4 tuần)

### Coming soon...

---

## 📋 Phase 3: Dedicated Server (2-3 tuần)

### Coming soon...

---

## 📊 Timeline Summary

| Phase | Duration | Status |
|-------|----------|--------|
| Phase 1: Offline | 4-6 tuần | 🔄 In Progress |
| Phase 2: Online | 3-4 tuần | ⏳ Pending |
| Phase 3: Server | 2-3 tuần | ⏳ Pending |
| **Total** | **9-13 tuần** | |

---

**Version**: 1.0  
**Date**: 2025-10-08  
**Status**: Planning Complete ✅

