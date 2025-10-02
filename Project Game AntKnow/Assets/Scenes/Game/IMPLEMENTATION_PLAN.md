# 🚀 GameScene Implementation Plan

## 📋 Phase Overview

**Total: 7 Phases, ~40-60 hours**

---

## Phase 0: Demo Version - Local Hosting (6-8 hours)

### Goal: Single player demo với hosting mode, test movement và basic UI

### Tasks:
- [ ] Create GameScene in Unity
- [ ] Setup 36 waypoints (circular path)
- [ ] Create simple BoardManager.cs (no ScriptableObject yet)
- [ ] Create PlayerGameController.cs (movement only)
- [ ] Create simple DiceController.cs (2D dice)
- [ ] Create PanelGameInfo (Turn, Player Name, Time)
- [ ] Create PanelPlayerMe (Name, Money)
- [ ] Test: Spawn 1 player → Roll dice → Move → Repeat

### Deliverables:
```
✅ 36 waypoints placed in scene
✅ 1 player spawns at tile 0
✅ Can roll dice (2D animation)
✅ Player moves waypoint by waypoint
✅ UI shows current turn and money
✅ Game runs in hosting mode (no multiplayer yet)
```

---

## Phase 1: Board Setup & Waypoints (4-6 hours)

### Tasks:
- [ ] Create BoardConfig ScriptableObject (36 tiles)
- [ ] Define tile types (Start, Property, Event, Quiz, Jail, Travel)
- [ ] Setup tile data (names, prices, rent)
- [ ] Update BoardManager.cs to use config
- [ ] Add tile type indicators (visual debug)
- [ ] Test waypoint path visualization

### Deliverables:
```
✅ BoardConfig asset với 36 tiles
✅ BoardManager loads config
✅ Visual debug: Show tile numbers, types
✅ Tile data complete (names, prices)
```

---

## Phase 2: Player Models & Movement (6-8 hours)

### Tasks:
- [ ] Import male/female models
- [ ] Setup Animator Controller (Idle, Run)
- [ ] Create PlayerGameController.cs
- [ ] Implement waypoint movement (MoveBySteps)
- [ ] Implement animation control (isRunning)
- [ ] Load player data từ loadout
- [ ] Spawn players tại tile 0
- [ ] Test movement với manual input

### Deliverables:
```
✅ Player models spawn correctly
✅ Movement smooth từ waypoint này đến waypoint khác
✅ Animation transitions (Idle ↔ Run)
✅ Stats loaded từ loadout
```

---

## Phase 3: Dice System (4-6 hours)

### Tasks:
- [ ] Create dice sprites (6 faces x 2 dice)
- [ ] Create DiceController.cs
- [ ] Implement 2D dice roll animation
- [ ] Implement luck stat effect (increase double chance)
- [ ] Create PanelDice UI
- [ ] Integrate với TurnManager
- [ ] Test dice roll → player movement

### Deliverables:
```
✅ Dice roll animation smooth
✅ Luck stat affects double chance
✅ Roll button only active on player's turn
✅ Dice result triggers player movement
```

---

## Phase 4: Turn System & Game Flow (6-8 hours)

### Tasks:
- [ ] Create TurnManager.cs
- [ ] Implement turn order (2-4 players)
- [ ] Implement turn flow (Roll → Move → Resolve → Next)
- [ ] Create GameManager.cs (main controller)
- [ ] Implement pass Start (+money with Health%)
- [ ] Create PanelPlayerMe (Name, Money only)
- [ ] Create PanelPlayer1/2/3 (Name, Money only, show if player exists)
- [ ] Update PanelGameInfo (Turn count, Current player name, Time elapsed)
- [ ] Max 25 turns per game
- [ ] Test full turn cycle

### Deliverables:
```
✅ Turn order works correctly
✅ PanelGameInfo shows current turn and player
✅ Money updates when pass Start
✅ Stats effects applied correctly
✅ Turn transitions smooth
✅ Game ends after 25 turns
```

---

## Phase 5: Property System (8-10 hours)

### Tasks:
- [ ] Create PropertyManager.cs
- [ ] Create PanelProperty UI (simplified):
  - [ ] 6 buttons: House 1-5, Hotel
  - [ ] Show property name
  - [ ] Show price when click button
  - [ ] 2 options: "Mua" (buy) or "Skip" (end turn)
- [ ] Implement buy property logic
- [ ] Implement upgrade property logic (Level 1-5 → Hotel)
- [ ] Implement rent calculation
- [ ] Apply stats effects:
  - [ ] Agility: -% buy price
  - [ ] Resistance: -% rent paid
  - [ ] Intelligence: +% rent received
- [ ] Implement property ownership display
- [ ] Test buy/upgrade/rent scenarios

### Deliverables:
```
✅ PanelProperty shows when land on empty tile
✅ Can select house level (1-5 or Hotel)
✅ Shows price for selected level
✅ Can buy or skip
✅ Can upgrade owned properties
✅ Rent calculated correctly with stats
✅ Money transactions work
```

---

## Phase 6: Special Tiles (10-12 hours)

### 6.1 Quiz System (4-5 hours)
- [ ] Create QuizManager.cs
- [ ] Create PanelQuiz UI
- [ ] Load questions từ Firestore quizzes collection (use valueRandom field)
- [ ] Implement answer checking
- [ ] Implement rewards (correct: bonus turn, wrong: skip turn)
- [ ] Test quiz flow

### 6.2 Event Cards (3-4 hours) - SIMPLIFIED
- [ ] Create EventCardManager.cs
- [ ] Create PanelEventCard UI (simple text panel)
- [ ] NO sprites, just text display
- [ ] Implement random event selection (9 types)
- [ ] Show event text for 3 seconds, then auto-hide
- [ ] Implement card effects:
  - [ ] Money changes (+/- amounts)
  - [ ] Thanh tra (trigger quiz)
  - [ ] Khế ước quỷ dữ (swap property)
- [ ] Test all event cards

### 6.3 Jail & Travel (2 hours)
- [ ] Implement jail logic (3 turns or double)
- [ ] Implement travel tile (-100 money)
- [ ] Test special tiles

### Deliverables:
```
✅ Quiz loads từ Firestore
✅ Quiz rewards/penalties work
✅ Event cards draw randomly
✅ All event effects work correctly
✅ Jail and Travel tiles work
```

---

## Phase 7: Multiplayer Sync (8-10 hours)

### Tasks:
- [ ] Create NetworkSyncManager.cs
- [ ] Implement Netcode NetworkVariables
- [ ] Sync player positions
- [ ] Sync money changes
- [ ] Sync property ownership
- [ ] Sync turn state
- [ ] Implement RPC calls:
  - [ ] RollDiceServerRpc
  - [ ] MovePlayerServerRpc
  - [ ] BuyPropertyServerRpc
  - [ ] UpgradePropertyServerRpc
  - [ ] PayRentServerRpc
- [ ] Test multiplayer với 2-4 players
- [ ] Handle disconnections

### Deliverables:
```
✅ All players see same game state
✅ Dice rolls synced
✅ Movements synced
✅ Money changes synced
✅ Property changes synced
✅ Turn changes synced
✅ Disconnection handled gracefully
```

---

## Phase 8: Polish & Testing (6-8 hours)

### Tasks:
- [ ] Add sound effects (dice roll, buy, rent, etc.)
- [ ] Add UI animations (panel transitions)
- [ ] Add particle effects (money gain/loss)
- [ ] Optimize performance
- [ ] Bug fixing
- [ ] Balance testing (money, prices, stats effects)
- [ ] Multiplayer stress testing
- [ ] Create game end screen (winner display)

### Deliverables:
```
✅ Game feels polished
✅ No major bugs
✅ Performance smooth (60 FPS)
✅ Multiplayer stable
✅ Game end works correctly
```

---

## 📊 Development Timeline

### Week 1:
- Phase 1: Board Setup (Day 1-2)
- Phase 2: Player Models & Movement (Day 3-5)

### Week 2:
- Phase 3: Dice System (Day 1-2)
- Phase 4: Turn System (Day 3-5)

### Week 3:
- Phase 5: Property System (Day 1-5)

### Week 4:
- Phase 6: Special Tiles (Day 1-5)

### Week 5:
- Phase 7: Multiplayer Sync (Day 1-5)

### Week 6:
- Phase 8: Polish & Testing (Day 1-5)

**Total: ~6 weeks (part-time) or ~2-3 weeks (full-time)**

---

## 🎯 Priority Order

### Must Have (MVP):
1. ✅ Board & Waypoints
2. ✅ Player Movement
3. ✅ Dice Roll
4. ✅ Turn System
5. ✅ Property Buy/Rent
6. ✅ Pass Start (+money)
7. ✅ Basic Multiplayer Sync

### Should Have:
1. ✅ Property Upgrade (Level 1-5 → Hotel)
2. ✅ Stats Effects (Health, Agility, Intelligence, Luck, Resistance)
3. ✅ Quiz System
4. ✅ Event Cards
5. ✅ Jail & Travel

### Nice to Have:
1. ⭐ Sound Effects
2. ⭐ UI Animations
3. ⭐ Particle Effects
4. ⭐ Advanced Stats (leaderboard, history)
5. ⭐ Spectator Mode

---

## 🔧 Technical Stack

### Unity Packages:
```
- Netcode for GameObjects (Multiplayer)
- TextMeshPro (UI)
- Firebase SDK (Firestore for quizzes)
- DOTween (Animations - optional)
```

### External Services:
```
- Firebase Firestore (Quizzes, Game Sessions)
- Unity Gaming Services (Lobby, Relay)
```

---

## 📚 Next Steps

### Immediate (This Week):
1. **Read GAME_DESIGN_DOCUMENT.md** - Understand full design
2. **Setup GameScene** - Create scene, add Camera, Canvas
3. **Create Waypoints** - 36 waypoints in circular path
4. **Create BoardConfig** - ScriptableObject với 36 tiles
5. **Start Phase 1** - Board Setup & Waypoints

### Questions to Answer:
- [ ] Có sẵn male/female models chưa?
- [ ] Có sẵn dice sprites chưa?
- [ ] Có sẵn event card sprites chưa?
- [ ] Firestore quizzes collection đã có data chưa?
- [ ] Multiplayer lobby system đã hoàn thiện chưa?

---

**Bạn muốn bắt đầu từ Phase nào? 🚀**

