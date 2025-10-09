# 🎮 GameScene Development Plan - Kế Hoạch Phát Triển Tổng Thể

## 📊 Executive Summary

Dự án: **Game Cờ Tỷ Phú 3D Multiplayer**  
Mục tiêu: Hoàn thiện GameScene với đầy đủ gameplay mechanics  
Chiến lược: **3 Phases** - Offline → Online → Dedicated Server  
Timeline: **9-13 tuần**

---

## 🎯 Current Status

### ✅ Completed (LoginScene & MenuScene)
- Firebase Authentication
- Firestore Database
- Inventory & Loadout System
- Lobby & Matchmaking System
- Unity Gaming Services Integration

### 🔄 In Progress (GameScene)
- Basic gameplay mechanics (60% complete)
- Domain-driven architecture (partially implemented)
- UI panels (created but not integrated)
- Network layer (prepared but not active)

### ⏳ Pending
- House/Hotel system
- Card system
- Quiz system
- Special tiles handling
- End game logic
- Multiplayer synchronization
- Dedicated server build

---

## 📋 Development Roadmap

### **Phase 1: Offline Gameplay** (4-6 tuần) 🔄

**Goal**: Hoàn thiện tất cả game mechanics trong chế độ offline (1-2 players local)

#### Week 1: Core Systems Refactor
- **Task 1.1**: Integrate Domain Layer vào GameManager
  - Add Domain references (GameState, TurnSystem, BoardRules)
  - Sync Unity ↔ Domain state
  - Refactor turn system và property logic
  - **Status**: 📝 Implementation guide ready
  - **Deliverable**: Clean architecture với Domain layer

#### Week 2: Property System
- **Task 1.2**: Hoàn thiện Property System
  - Buy property flow với PanelBuy
  - Build houses (level 1-4) với 3D models
  - Upgrade to hotel (level 5) với 3D model
  - Rent calculation với stats bonuses
  - **Deliverable**: Full property system với visuals

#### Week 3: Card & Quiz Systems
- **Task 1.3**: Implement Card System
  - Load player loadout từ GameSessionData
  - Passive card effects (stat bonuses)
  - Active card usage với cooldowns
  - Card triggers (OnTurnStart, OnLandOnTile, etc.)
  - **Deliverable**: Working card system

- **Task 1.4**: Implement Quiz System
  - Firebase Quiz Service integration
  - PanelQuiz với timer
  - Rewards/penalties handling
  - **Deliverable**: Working quiz system

#### Week 4: Special Tiles & UI
- **Task 1.5**: Implement Special Tiles
  - Start (salary + Health bonus)
  - Jail (visit vs imprisoned)
  - Travel (teleport options)
  - Event (random cards)
  - Tax/Bonus (with Resistance)
  - GoToJail, FreeParking
  - **Deliverable**: All tiles working

- **Task 1.6**: Hoàn thiện UI Panels
  - Connect all panels với game logic
  - Polish UX và animations
  - **Deliverable**: Smooth UI experience

#### Week 5-6: End Game & Testing
- **Task 1.7**: Implement End Game Logic
  - End conditions (max turns, bankruptcy)
  - Score calculation
  - PanelResult với rankings
  - Save to Firebase
  - **Deliverable**: Complete game loop

- **Task 1.8**: Testing & Bug Fixes
  - Functional testing
  - Bug fixes
  - Performance optimization
  - Polish (animations, sounds, VFX)
  - **Deliverable**: Stable offline game

---

### **Phase 2: Multiplayer Online** (3-4 tuần) ⏳

**Goal**: Chuyển đổi sang multiplayer với Netcode for GameObjects

#### Week 7: Network Foundation
- **Task 2.1**: Setup Network Infrastructure
  - NetworkVariables cho game state
  - RPCs cho player actions
  - Client-server architecture
  - Connection handling

#### Week 8: State Synchronization
- **Task 2.2**: Implement State Sync
  - Player positions
  - Property ownership
  - Money và stats
  - Turn management

#### Week 9: Network Features
- **Task 2.3**: Network Gameplay Features
  - Networked dice rolling
  - Networked property transactions
  - Networked card usage
  - Networked quiz system

#### Week 10: Testing & Polish
- **Task 2.4**: Multiplayer Testing
  - 2-4 players testing
  - Latency handling
  - Disconnection handling
  - Bug fixes

---

### **Phase 3: Dedicated Server** (2-3 tuần) ⏳

**Goal**: Chuẩn bị dedicated server và deploy lên Unity Multiplay Hosting

#### Week 11: Server Build
- **Task 3.1**: Dedicated Server Setup
  - Server-only build configuration
  - Headless mode
  - Server-authoritative logic
  - Anti-cheat measures

#### Week 12: Deployment
- **Task 3.2**: Unity Multiplay Hosting
  - Build configuration
  - Fleet management
  - Matchmaking integration
  - Monitoring và logging

#### Week 13: Production Ready
- **Task 3.3**: Final Polish
  - Load testing
  - Performance optimization
  - Security audit
  - Documentation

---

## 📁 Key Documents

### Planning Documents
1. **GAMESCENE_ROADMAP.md** - Chi tiết roadmap cho 3 phases
2. **ARCHITECTURE_ANALYSIS.md** - Phân tích kiến trúc và quyết định
3. **TASK_1_1_IMPLEMENTATION.md** - Implementation guide cho task đầu tiên

### Existing Documents
1. **SETUP_GUIDE.md** - Hướng dẫn setup GameScene
2. **CODE_STRUCTURE.md** - Cấu trúc code hiện tại
3. **MAP_36_TILES.md** - Chi tiết 36 tiles trên board

---

## 🏗️ Architecture Decision

### **Chosen Approach: Hybrid (Domain-Driven + Direct)**

**Rationale:**
- Keep working features from GameManager.cs
- Add clean architecture from Domain layer
- Easier migration path
- Best of both worlds

**Structure:**
```
GameManager.cs (Unified Controller)
├── Domain Layer (Logic)
│   ├── GameState, PlayerState, PropertyState
│   ├── TurnSystem, BoardRules, PropertyEconomy
│   └── CardRuleEngine, DiceRng
├── Presentation Layer (Unity)
│   ├── BoardManager, PlayerGameController
│   ├── PropertyManager (Visual), DiceController
│   └── TurnIndicator, PropertyVisual
├── UI Layer
│   └── All panels (Buy, Quiz, Event, etc.)
└── Network Layer (Phase 2)
    ├── NetworkVariables
    ├── RPCs
    └── Client-Server sync
```

---

## 📊 Progress Tracking

### Phase 1: Offline Gameplay
| Task | Status | Progress | ETA |
|------|--------|----------|-----|
| 1.1: Core Refactor | 🔄 In Progress | 10% | Week 1 |
| 1.2: Property System | ⏳ Pending | 0% | Week 2 |
| 1.3: Card System | ⏳ Pending | 0% | Week 3 |
| 1.4: Quiz System | ⏳ Pending | 0% | Week 3 |
| 1.5: Special Tiles | ⏳ Pending | 0% | Week 4 |
| 1.6: UI Panels | ⏳ Pending | 0% | Week 4 |
| 1.7: End Game | ⏳ Pending | 0% | Week 5 |
| 1.8: Testing | ⏳ Pending | 0% | Week 6 |

### Phase 2: Multiplayer Online
| Task | Status | Progress | ETA |
|------|--------|----------|-----|
| 2.1: Network Foundation | ⏳ Pending | 0% | Week 7 |
| 2.2: State Sync | ⏳ Pending | 0% | Week 8 |
| 2.3: Network Features | ⏳ Pending | 0% | Week 9 |
| 2.4: Testing | ⏳ Pending | 0% | Week 10 |

### Phase 3: Dedicated Server
| Task | Status | Progress | ETA |
|------|--------|----------|-----|
| 3.1: Server Build | ⏳ Pending | 0% | Week 11 |
| 3.2: Deployment | ⏳ Pending | 0% | Week 12 |
| 3.3: Production | ⏳ Pending | 0% | Week 13 |

---

## 🎯 Next Immediate Steps

### Today (Week 1, Day 1):
1. ✅ Review planning documents
2. ✅ Understand architecture decision
3. 🔄 Start Task 1.1 implementation
   - Add Domain references to GameManager
   - Initialize Domain layer
   - Test basic integration

### This Week:
1. Complete Task 1.1 (Core Refactor)
2. Test offline gameplay với Domain layer
3. Prepare for Task 1.2 (Property System)

---

## 📚 Resources

### Code Locations
- **Domain Layer**: `Assets/Script/Domain/`
- **Game Scripts**: `Assets/Scenes/Game/Scripts/`
- **Presentation**: `Assets/Script/Presentation/`
- **Multiplayer**: `Assets/Script/Multiplayer/`

### Key Files to Review
1. `GameManager.cs` - Main controller
2. `GameState.cs` - Domain state
3. `TurnSystem.cs` - Turn logic
4. `BoardRules.cs` - Game rules
5. `PropertyEconomy.cs` - Economic calculations

---

## ✅ Success Criteria

### Phase 1 Complete When:
- ✅ All game mechanics working offline
- ✅ 1-2 players can play full game
- ✅ All tiles functional
- ✅ Cards và quiz working
- ✅ End game logic complete
- ✅ No critical bugs
- ✅ Good performance (60 FPS)

### Phase 2 Complete When:
- ✅ 2-4 players online working
- ✅ State synchronized correctly
- ✅ No desync issues
- ✅ Smooth multiplayer experience

### Phase 3 Complete When:
- ✅ Dedicated server deployed
- ✅ Production-ready
- ✅ Monitoring active
- ✅ Documentation complete

---

## 🚀 Let's Start!

**Current Focus**: Task 1.1 - Refactor và Tối Ưu Hóa Core Systems

**Implementation Guide**: See `TASK_1_1_IMPLEMENTATION.md`

**Estimated Time**: 3-4 giờ

**Ready to begin!** 🎮

---

**Version**: 1.0  
**Date**: 2025-10-08  
**Status**: Planning Complete, Ready for Implementation ✅

