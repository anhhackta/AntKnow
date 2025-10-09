# 🚀 5-DAY MULTIPLAYER IMPLEMENTATION PLAN

## 📊 Executive Summary

**Deadline**: 5 ngày (60 giờ làm việc)  
**Goal**: GameScene multiplayer online hoạt động với 2-4 players  
**Architecture**: Server-Client với Netcode for GameObjects  
**Strategy**: Focus vào CORE FEATURES, bỏ qua nice-to-have

---

## 🎯 Phân Tích Hiện Trạng

### ✅ **Đã Có (Ready to Use)**

#### **Infrastructure**
- ✅ Unity Gaming Services (Lobby + Relay) - Đã setup trong MenuScene
- ✅ Netcode for GameObjects (NGO) - Đã có NetworkManager
- ✅ Firebase Auth + Firestore - Đã hoạt động
- ✅ GameSessionData - Truyền data từ Menu → Game

#### **Domain Layer (Pure Logic)**
- ✅ GameState, PlayerState, PropertyState
- ✅ TurnSystem - Turn management logic
- ✅ BoardRules - Buy/upgrade/rent logic
- ✅ PropertyEconomy - Economic calculations
- ✅ DiceRng - Deterministic dice rolling

#### **Game Scripts**
- ✅ GameManager.cs - Main controller (có NetworkBehaviour)
- ✅ BoardManager.cs - 36 tiles management
- ✅ PlayerGameController.cs - Player movement
- ✅ PropertyManager.cs - Property management
- ✅ DiceController.cs - Dice animation

#### **Network Scripts (Đã có nhưng chưa integrate)**
- ✅ NetworkGameManager.cs - Network manager
- ✅ GameController.cs - Domain-driven network controller
- ✅ NetworkPlayerController.cs - Network player controller

#### **UI Panels (Đã có)**
- ✅ PanelBuy, PanelQuiz, PanelEvent, PanelResult, PanelPlayerMe

### ⚠️ **Vấn Đề Cần Giải Quyết**

1. **Duplicate Systems**: 2 game controllers song song
   - GameManager.cs (Scene-specific, direct approach)
   - GameController.cs (Domain-driven, network-ready)

2. **Network Integration**: Chưa có synchronization
   - Player positions không sync
   - Property ownership không sync
   - Money không sync
   - Turn state không sync

3. **Missing Features**:
   - Card system chưa integrate
   - Quiz system chưa integrate
   - Special tiles chưa xử lý hết
   - End game logic chưa có

---

## 💡 Chiến Lược 5 Ngày

### **Core Philosophy**: KISS (Keep It Simple, Stupid)

1. **Sử dụng code đã có** - Không viết lại từ đầu
2. **Focus vào core gameplay** - Dice, movement, property, turn
3. **Bỏ qua nice-to-have** - Card system, quiz có thể đơn giản hóa
4. **Test thường xuyên** - Mỗi feature xong là test ngay
5. **Server-authoritative** - Server quyết định mọi thứ

### **Architecture Decision**: Hybrid Approach

```
GameManager.cs (Main Controller)
├── NetworkGameManager.cs (Network Layer)
│   ├── NetworkVariables (game state)
│   ├── ServerRpc (client requests)
│   └── ClientRpc (server broadcasts)
├── Domain Layer (Logic)
│   ├── GameState, TurnSystem, BoardRules
│   └── PropertyEconomy, DiceRng
└── Presentation Layer (Unity)
    ├── PlayerGameController (visual)
    ├── BoardManager (visual)
    └── PropertyManager (visual)
```

---

## 📅 DAY-BY-DAY BREAKDOWN

### **DAY 1: Network Foundation & Core Sync** (12 giờ)

#### **Morning (6h): Setup Network Infrastructure**

**8:00 - 10:00 (2h): Integrate NetworkGameManager**
- [ ] Add NetworkGameManager to GameScene
- [ ] Connect với GameManager
- [ ] Setup NetworkManager + UnityTransport
- [ ] Test connection với 2 instances (Build + Editor)

**10:00 - 12:00 (2h): Player Spawning Network**
- [ ] Implement networked player spawning
- [ ] Sync player data (name, stats, avatar)
- [ ] Test 2-4 players spawn correctly

**12:00 - 14:00 (2h): Basic State Sync**
- [ ] Create NetworkVariables for player positions
- [ ] Create NetworkVariables for player money
- [ ] Test basic sync

#### **Afternoon (6h): Core Synchronization**

**14:00 - 16:00 (2h): Player Position Sync**
- [ ] Implement ServerRpc for movement requests
- [ ] Implement ClientRpc for position updates
- [ ] Test movement sync với 2 players

**16:00 - 18:00 (2h): Game State Sync**
- [ ] Create NetworkVariable for current turn
- [ ] Create NetworkVariable for game phase
- [ ] Test turn state sync

**18:00 - 20:00 (2h): Testing & Bug Fixes**
- [ ] Test với 2 instances
- [ ] Fix connection issues
- [ ] Fix spawn issues
- [ ] Document bugs

**Deliverables:**
- ✅ 2-4 players can connect
- ✅ Players spawn correctly
- ✅ Basic state syncs
- ✅ No critical bugs

---

### **DAY 2: Turn System & Dice Network** (12 giờ)

#### **Morning (6h): Turn System**

**8:00 - 10:00 (2h): Networked Turn System**
- [ ] Implement ServerRpc for turn start
- [ ] Implement ServerRpc for turn end
- [ ] Sync current player turn
- [ ] Test turn switching

**10:00 - 12:00 (2h): Dice Rolling Network**
- [ ] Implement ServerRpc for dice roll request
- [ ] Server generates dice result (DiceRng)
- [ ] ClientRpc broadcasts dice result
- [ ] Test dice sync

**12:00 - 14:00 (2h): Dice Animation Sync**
- [ ] Sync dice animation across clients
- [ ] Show dice result on all clients
- [ ] Test visual sync

#### **Afternoon (6h): Movement Sync**

**14:00 - 16:00 (2h): Movement Network**
- [ ] Server calculates new position
- [ ] ClientRpc moves player visual
- [ ] Smooth movement animation
- [ ] Test movement sync

**16:00 - 18:00 (2h): Turn Indicator Sync**
- [ ] Sync turn indicator (yellow ping)
- [ ] Show on current player only
- [ ] Test indicator sync

**18:00 - 20:00 (2h): Testing & Polish**
- [ ] Test full turn flow (roll → move → end turn)
- [ ] Fix sync issues
- [ ] Fix animation issues
- [ ] Polish UX

**Deliverables:**
- ✅ Turn system works online
- ✅ Dice rolls sync correctly
- ✅ Players move in sync
- ✅ Turn indicator works

---

### **DAY 3: Property System Network** (12 giờ)

#### **Morning (6h): Property Ownership**

**8:00 - 10:00 (2h): Property Data Sync**
- [ ] Create NetworkList for properties
- [ ] Sync property ownership
- [ ] Sync property level
- [ ] Test property sync

**10:00 - 12:00 (2h): Buy Property Network**
- [ ] ServerRpc for buy request
- [ ] Server validates (money, ownership)
- [ ] Server updates property
- [ ] ClientRpc updates visuals
- [ ] Test buy flow

**12:00 - 14:00 (2h): Money Sync**
- [ ] NetworkVariable for each player money
- [ ] Server updates money
- [ ] ClientRpc updates UI
- [ ] Test money sync

#### **Afternoon (6h): Rent & Upgrades**

**14:00 - 16:00 (2h): Rent Payment Network**
- [ ] Server calculates rent (BoardRules)
- [ ] Server transfers money
- [ ] ClientRpc shows rent payment
- [ ] Test rent flow

**16:00 - 18:00 (2h): Upgrade Property Network**
- [ ] ServerRpc for upgrade request
- [ ] Server validates (money, level)
- [ ] Server updates property level
- [ ] ClientRpc spawns house/hotel models
- [ ] Test upgrade flow

**18:00 - 20:00 (2h): Testing & Bug Fixes**
- [ ] Test full property flow
- [ ] Test with 2-4 players
- [ ] Fix sync issues
- [ ] Fix visual issues

**Deliverables:**
- ✅ Property buy works online
- ✅ Rent payment works
- ✅ Upgrades work
- ✅ Money syncs correctly

---

### **DAY 4: Special Tiles & Polish** (12 giờ)

#### **Morning (6h): Special Tiles**

**8:00 - 10:00 (2h): Start Tile Network**
- [ ] Server gives salary when passing Start
- [ ] Apply Health bonus
- [ ] ClientRpc shows notification
- [ ] Test Start tile

**10:00 - 11:00 (1h): Jail Tile Network**
- [ ] Server handles jail logic
- [ ] Sync jail turns
- [ ] Test jail flow

**11:00 - 12:00 (1h): Travel Tile Network**
- [ ] Server teleports player
- [ ] Sync position
- [ ] Test travel

**12:00 - 14:00 (2h): Other Special Tiles**
- [ ] Tax tile (simple deduction)
- [ ] Bonus tile (simple addition)
- [ ] GoToJail tile
- [ ] Test all tiles

#### **Afternoon (6h): Polish & Bug Fixes**

**14:00 - 16:00 (2h): UI Polish**
- [ ] Show PanelBuy on all clients
- [ ] Show notifications
- [ ] Polish animations
- [ ] Test UI sync

**16:00 - 18:00 (2h): Bug Fixes**
- [ ] Fix critical bugs
- [ ] Fix sync issues
- [ ] Fix visual glitches
- [ ] Test stability

**18:00 - 20:00 (2h): Basic Testing**
- [ ] Test 2 players full game
- [ ] Test 4 players full game
- [ ] Document remaining bugs
- [ ] Prioritize fixes

**Deliverables:**
- ✅ Special tiles work
- ✅ UI syncs correctly
- ✅ No critical bugs
- ✅ Game playable online

---

### **DAY 5: End-to-End Testing & Final Polish** (12 giờ)

#### **Morning (6h): End Game Logic**

**8:00 - 10:00 (2h): End Game Network**
- [ ] Server detects end conditions
- [ ] Calculate final scores
- [ ] ClientRpc shows PanelResult
- [ ] Test end game

**10:00 - 12:00 (2h): Save Results to Firebase**
- [ ] Server calls awardMatch Cloud Function
- [ ] Update player stats
- [ ] Test Firebase integration

**12:00 - 14:00 (2h): Reconnection Handling**
- [ ] Handle player disconnect
- [ ] Handle player reconnect (basic)
- [ ] Test disconnect scenarios

#### **Afternoon (6h): Final Testing & Polish**

**14:00 - 16:00 (2h): Full Game Testing**
- [ ] Test 2 players complete game
- [ ] Test 4 players complete game
- [ ] Test all features
- [ ] Document bugs

**16:00 - 18:00 (2h): Bug Fixes**
- [ ] Fix all critical bugs
- [ ] Fix high-priority bugs
- [ ] Test fixes

**18:00 - 20:00 (2h): Final Polish**
- [ ] Polish animations
- [ ] Polish UI
- [ ] Add sound effects (if time)
- [ ] Final testing

**Deliverables:**
- ✅ Complete game works online
- ✅ 2-4 players can play full game
- ✅ Results save to Firebase
- ✅ No critical bugs
- ✅ Ready for demo

---

## 📊 Feature Priority Matrix

### **MUST HAVE** (Core Features)
- ✅ Player connection & spawning
- ✅ Turn system
- ✅ Dice rolling
- ✅ Player movement
- ✅ Property buy
- ✅ Property rent
- ✅ Money sync
- ✅ Start tile (salary)
- ✅ End game logic

### **SHOULD HAVE** (Important)
- ✅ Property upgrades (houses)
- ✅ Hotel system
- ✅ Jail tile
- ✅ Travel tile
- ✅ Tax/Bonus tiles
- ✅ Turn indicator
- ✅ PanelBuy UI

### **NICE TO HAVE** (Optional - Bỏ qua nếu không kịp)
- ⏳ Card system (đơn giản hóa hoặc bỏ)
- ⏳ Quiz system (đơn giản hóa hoặc bỏ)
- ⏳ Event tiles (bỏ)
- ⏳ Animations polish
- ⏳ Sound effects
- ⏳ Advanced reconnection

---

## 🎯 Success Criteria

### **Minimum Viable Product (MVP)**
- [ ] 2-4 players can connect online
- [ ] Players can take turns
- [ ] Players can roll dice and move
- [ ] Players can buy properties
- [ ] Players can pay rent
- [ ] Game ends after max turns
- [ ] Results save to Firebase

### **Good Product**
- [ ] All MVP features ✅
- [ ] Property upgrades work
- [ ] Special tiles work
- [ ] UI syncs correctly
- [ ] No critical bugs

### **Great Product**
- [ ] All Good Product features ✅
- [ ] Smooth animations
- [ ] Polish UI/UX
- [ ] Handle disconnections
- [ ] Card system works (simplified)

---

**Next**: See `DAILY_CHECKLIST.md` for detailed hourly tasks  
**Status**: Ready to implement 🚀

