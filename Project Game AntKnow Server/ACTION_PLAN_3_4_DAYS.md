# 🚀 ACTION PLAN: HOÀN THIỆN MULTIPLAYER GAME TRONG 3-4 NGÀY

**Mục tiêu**: Triển khai server headless + client + gameplay hoàn chỉnh trong 3-4 ngày

---

## 📅 DAY 1: SERVER SETUP & BASIC MULTIPLAYER (8-10 giờ)

### **Morning (4-5 giờ): Server Infrastructure**

#### **Hour 1-2: Setup Project & Scripts** ✅
```
✅ Mở "Project Game AntKnow Server"
✅ Copy 3 scripts:
   - ServerBootstrap.cs
   - ServerGameManager.cs
   - ServerBuilder.cs
✅ Verify Domain layer (GameState, PlayerState, PropertyState)
✅ Test compile (0 errors)
```

#### **Hour 3-4: Configure GameScene** ✅
```
✅ Setup NetworkManager
✅ Create NetworkPlayer prefab
✅ Add ServerBootstrap to scene
✅ Add ServerGameManager to scene
✅ Save scene
```

#### **Hour 5: Build & Test Server** ✅
```
✅ Configure build settings (Headless mode)
✅ Build server (Windows)
✅ Test server locally
✅ Verify port 7777 listening
```

### **Afternoon (4-5 giờ): Client Connection**

#### **Hour 6-7: Build Client**
```
✅ Switch to "Project Game AntKnow"
✅ Create ClientConnectionManager.cs
✅ Add connection UI to MenuScene
✅ Configure build settings (Client mode)
✅ Build client
```

#### **Hour 8-9: Test Multiplayer**
```
✅ Run server
✅ Run 2 clients
✅ Test connection
✅ Verify game starts with 2 players
✅ Test turn system
```

#### **Hour 10: Bug Fixes**
```
✅ Fix connection issues
✅ Fix turn system bugs
✅ Test disconnect handling
```

**✅ Day 1 Complete: Server + Client multiplayer hoạt động!**

---

## 📅 DAY 2: CORE GAMEPLAY SYNC (8-10 giờ)

### **Morning (4-5 giờ): Dice & Movement**

#### **Hour 1-2: Dice Rolling Sync**
```
Task: Implement server-authoritative dice rolling

Server (ServerGameManager.cs):
✅ RequestRollDiceServerRpc() - Already done
✅ Validate player turn
✅ Roll dice (Random.Range)
✅ Broadcast result to all clients

Client:
✅ Add "Roll Dice" button
✅ Call RequestRollDiceServerRpc()
✅ Listen to NotifyDiceRollClientRpc()
✅ Animate dice visual

Test:
✅ Player 1 rolls → All clients see same result
✅ Player 2 can't roll on Player 1's turn
```

#### **Hour 3-4: Player Movement Sync**
```
Task: Sync player position across all clients

Server:
✅ Update PlayerState.NodeIndex
✅ Broadcast new position

Client:
✅ Create NetworkPlayerController.cs
✅ Add NetworkVariable<int> position
✅ OnValueChanged → Move player visual
✅ Animate movement (lerp)

Test:
✅ Player moves → All clients see movement
✅ Smooth animation
✅ No desync
```

#### **Hour 5: Board Sync**
```
Task: Ensure all clients have same board state

✅ Sync tile states
✅ Sync property ownership
✅ Test with 4 players
```

### **Afternoon (4-5 giờ): Property System**

#### **Hour 6-7: Property Buy/Rent**
```
Task: Implement property transactions

Server (ServerGameManager.cs):
✅ RequestBuyPropertyServerRpc(int tileId)
   - Validate: Player has money
   - Validate: Property not owned
   - Deduct money
   - Set owner
   - Broadcast update

✅ HandleRentPayment(PlayerState player, PropertyState property)
   - Calculate rent
   - Transfer money
   - Broadcast update

Client:
✅ Show PanelBuy when land on property
✅ "Buy" button → RequestBuyPropertyServerRpc()
✅ Update UI when property bought
✅ Show rent payment notification

Test:
✅ Buy property → All clients see ownership
✅ Land on owned property → Pay rent
✅ Money syncs correctly
```

#### **Hour 8-9: Money Sync**
```
Task: Sync player money across all clients

Server:
✅ NetworkVariable<int> money for each player
✅ Update on buy/rent/salary
✅ Broadcast changes

Client:
✅ Listen to money changes
✅ Update UI (money display)
✅ Animate money change (+/- effect)

Test:
✅ All money transactions sync
✅ No money duplication bugs
```

#### **Hour 10: Integration Test**
```
✅ Full game flow: Roll → Move → Buy → Rent
✅ Test with 4 players
✅ Fix bugs
```

**✅ Day 2 Complete: Core gameplay syncing!**

---

## 📅 DAY 3: ADVANCED FEATURES (8-10 giờ)

### **Morning (4-5 giờ): House/Hotel System**

#### **Hour 1-2: Property Upgrades**
```
Server:
✅ RequestUpgradePropertyServerRpc(int tileId, int level)
   - Validate: Player owns property
   - Validate: Has money
   - Deduct upgrade cost
   - Set property level
   - Broadcast update

Client:
✅ PanelBuy: Add upgrade buttons
✅ Show house/hotel models
✅ Sync visual across clients

Test:
✅ Upgrade to 4 houses → All clients see houses
✅ Upgrade to hotel → All clients see hotel
```

#### **Hour 3-4: Special Tiles (Priority)**
```
Implement top 3 special tiles:

1. Start Tile:
   ✅ Pass Start → +200 salary
   ✅ Sync money

2. Jail Tile:
   ✅ Send to jail
   ✅ Jail turns countdown
   ✅ Roll doubles to escape

3. Tax/Bonus Tiles:
   ✅ Apply tax/bonus
   ✅ Sync money

Test:
✅ All special tiles work
✅ Sync correctly
```

#### **Hour 5: Turn Timer**
```
✅ Server: Track turn start time
✅ Server: Auto-end turn after 60s
✅ Client: Show countdown timer
✅ Test timeout handling
```

### **Afternoon (4-5 giờ): End Game & Polish**

#### **Hour 6-7: End Game Logic**
```
Server:
✅ Check end conditions:
   - Max turns (50)
   - Only 1 player left
   - Time limit (30 min)
✅ Calculate scores
✅ Determine winner
✅ Broadcast results

Client:
✅ Show PanelResult
✅ Display rankings
✅ Show rewards
✅ "Back to Menu" button

Test:
✅ Game ends correctly
✅ Scores accurate
✅ Results sync
```

#### **Hour 8-9: Firebase Integration**
```
✅ Save game results to Firestore
✅ Update player stats
✅ Award coins/exp
✅ Test save/load
```

#### **Hour 10: Bug Fixes & Polish**
```
✅ Fix critical bugs
✅ Improve UI feedback
✅ Add sound effects
✅ Test edge cases
```

**✅ Day 3 Complete: Full gameplay working!**

---

## 📅 DAY 4: DEPLOYMENT & TESTING (8-10 giờ)

### **Morning (4-5 giờ): Cloud Deployment**

#### **Hour 1-2: Setup Cloud Server**
```
Option A: AWS EC2
✅ Create t3.medium instance
✅ Configure security group (port 7777)
✅ Upload server build
✅ Run as service

Option B: Unity Multiplay
✅ Create fleet
✅ Upload build
✅ Configure regions
✅ Deploy

Choose based on budget & scale needs
```

#### **Hour 3-4: Client Update**
```
✅ Update server IP in client
✅ Build production client
✅ Test connection to cloud server
✅ Fix connection issues
```

#### **Hour 5: Load Testing**
```
✅ Test with 4 players
✅ Test with 8 players (2 games)
✅ Monitor server performance
✅ Check for lag/desync
```

### **Afternoon (4-5 giờ): Final Testing & Launch**

#### **Hour 6-7: Full Game Testing**
```
Test scenarios:
✅ 4 players full game (start to end)
✅ Player disconnect mid-game
✅ Reconnection handling
✅ All features working
✅ No critical bugs
```

#### **Hour 8-9: Performance Optimization**
```
Server:
✅ Reduce target FPS if needed
✅ Optimize network traffic
✅ Add connection pooling

Client:
✅ Optimize rendering
✅ Reduce network calls
✅ Add loading screens
```

#### **Hour 10: Launch Preparation**
```
✅ Create player guide
✅ Setup monitoring
✅ Prepare support system
✅ Final smoke test
✅ 🚀 LAUNCH!
```

**✅ Day 4 Complete: GAME LIVE!**

---

## 📊 FEATURE PRIORITY MATRIX

### **MUST HAVE** (Days 1-2)
```
✅ Server-client connection
✅ Turn system
✅ Dice rolling
✅ Player movement
✅ Property buy
✅ Property rent
✅ Money sync
```

### **SHOULD HAVE** (Day 3)
```
✅ House/hotel upgrades
✅ Start tile (salary)
✅ Jail tile
✅ Tax/Bonus tiles
✅ End game logic
✅ Results screen
```

### **NICE TO HAVE** (If time permits)
```
⏳ Card system (simplified)
⏳ Quiz system (simplified)
⏳ Travel tile
⏳ Event tile
⏳ Advanced animations
⏳ Sound effects
```

---

## 🎯 SUCCESS CRITERIA

### **Day 1 Success:**
```
✅ Server builds and runs
✅ 2 clients can connect
✅ Game starts automatically
✅ Turn system works
```

### **Day 2 Success:**
```
✅ Dice rolling syncs
✅ Player movement syncs
✅ Can buy properties
✅ Rent payment works
✅ Money syncs correctly
```

### **Day 3 Success:**
```
✅ House/hotel system works
✅ Special tiles implemented
✅ Game ends properly
✅ Results saved to Firebase
```

### **Day 4 Success:**
```
✅ Server deployed to cloud
✅ Clients can connect from internet
✅ 4 players can play full game
✅ No critical bugs
✅ 🚀 GAME LIVE!
```

---

## 🆘 RISK MITIGATION

### **Risk 1: Server build fails**
```
Mitigation:
✅ Test build early (Day 1 Hour 5)
✅ Use provided build script
✅ Have backup: Use existing GameManager as fallback
```

### **Risk 2: Network sync issues**
```
Mitigation:
✅ Use NetworkVariables (auto-sync)
✅ Server-authoritative (no client trust)
✅ Test frequently (after each feature)
```

### **Risk 3: Behind schedule**
```
Mitigation:
✅ Cut NICE TO HAVE features
✅ Focus on MUST HAVE only
✅ Use existing code (don't rewrite)
```

### **Risk 4: Deployment issues**
```
Mitigation:
✅ Test locally first
✅ Use Unity Multiplay (easier)
✅ Have backup: LAN deployment
```

---

## 📝 DAILY CHECKLIST

### **End of Each Day:**
```
✅ Commit code to Git
✅ Test all features
✅ Document bugs found
✅ Update task list
✅ Plan next day
```

### **Before Moving to Next Feature:**
```
✅ Feature works locally
✅ Feature syncs in multiplayer
✅ No critical bugs
✅ Code committed
```

---

## 🎉 LAUNCH CHECKLIST

```
Pre-Launch:
✅ Server deployed
✅ Client builds ready
✅ All MUST HAVE features working
✅ No critical bugs
✅ Performance acceptable

Launch:
✅ Server running 24/7
✅ Monitoring active
✅ Support ready
✅ Player guide available

Post-Launch:
✅ Monitor logs
✅ Fix urgent bugs
✅ Collect feedback
✅ Plan updates
```

---

**BẠN ĐÃ CÓ KẾ HOẠCH CHI TIẾT! BẮT ĐẦU NGAY! 🚀**

**Next Step**: Mở `QUICK_START_5_HOURS.md` và bắt đầu Hour 1!

