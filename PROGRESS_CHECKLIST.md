# ✅ PROGRESS CHECKLIST - ANTKNOW MULTIPLAYER

**Sử dụng file này để theo dõi tiến độ của bạn**

---

## 📅 DAY 1: SERVER SETUP (Target: 8-10 giờ)

### **HOUR 1: Setup Project** (0:00-1:00)
- [ ] Mở Unity Hub
- [ ] Add "Project Game AntKnow Server"
- [ ] Open với Unity 6000.0.48f1
- [ ] Verify packages installed:
  - [ ] Netcode for GameObjects (2.5.1)
  - [ ] Dedicated Server (1.6.1)
  - [ ] Unity Transport
- [ ] Create folder: Assets/Script/Server/
- [ ] Create folder: Assets/Editor/
- [ ] Copy ServerBootstrap.cs
- [ ] Copy ServerGameManager.cs
- [ ] Copy ServerBuilder.cs
- [ ] Verify Domain layer exists:
  - [ ] GameState.cs
  - [ ] PlayerState.cs
  - [ ] PropertyState.cs
- [ ] Test compile (0 errors)

**✅ Hour 1 Complete:** [ ]

---

### **HOUR 2: Configure GameScene** (1:00-2:00)
- [ ] Open GameScene.unity
- [ ] Create GameObject: "NetworkManager"
  - [ ] Add NetworkManager component
  - [ ] Add UnityTransport component
  - [ ] Configure: Port 7777, Address 0.0.0.0
- [ ] Create NetworkPlayer prefab:
  - [ ] Create GameObject: "NetworkPlayer"
  - [ ] Add NetworkObject component
  - [ ] Add NetworkTransform component
  - [ ] Add visual (Cube/Character)
  - [ ] Save to: Assets/Prefabs/NetworkPlayer.prefab
  - [ ] Delete from Hierarchy
- [ ] Assign Player Prefab to NetworkManager
- [ ] Create GameObject: "ServerBootstrap"
  - [ ] Add ServerBootstrap component
  - [ ] Configure: Port 7777, Max Players 4
- [ ] Create GameObject: "ServerGameManager"
  - [ ] Add ServerGameManager component
  - [ ] Add NetworkObject component
  - [ ] Configure settings
- [ ] Save scene (Ctrl+S)

**✅ Hour 2 Complete:** [ ]

---

### **HOUR 3: Build Server** (2:00-3:00)
- [ ] File → Build Settings
- [ ] Platform: Windows, Mac, Linux
- [ ] Add Scene: GameScene.unity
- [ ] Remove: LoginScene, MenuScene
- [ ] Player Settings:
  - [ ] Server Build: ENABLED
  - [ ] Scripting Backend: IL2CPP
  - [ ] API Compatibility: .NET Standard 2.1
- [ ] Unity Menu → Build → Build Dedicated Server (Windows)
- [ ] Wait for build (~10-15 min)
- [ ] Verify output: Builds/Server_Windows_[timestamp]/AntKnowServer.exe

**✅ Hour 3 Complete:** [ ]

---

### **HOUR 4: Test Server** (3:00-4:00)
- [ ] Navigate to: Builds/Server_Windows_[timestamp]/
- [ ] Run: RunServer.bat
- [ ] Check server.log:
  - [ ] "Dedicated Server Mode Detected"
  - [ ] "Server listening on 0.0.0.0:7777"
  - [ ] "SERVER STARTED SUCCESSFULLY"
- [ ] Verify port: netstat -an | findstr 7777
- [ ] Expected: TCP 0.0.0.0:7777 LISTENING

**✅ Hour 4 Complete:** [ ]

---

### **HOUR 5: Build Client** (4:00-5:00)
- [ ] Switch to "Project Game AntKnow"
- [ ] Create folder: Assets/Script/Client/
- [ ] Copy ClientConnectionManager.cs
- [ ] Open MenuScene.unity
- [ ] Create Connection UI:
  - [ ] Canvas → Panel "ConnectionPanel"
  - [ ] InputField: "Server IP"
  - [ ] Button: "Connect"
  - [ ] Text: "Status"
- [ ] Add ClientConnectionManager to Canvas
- [ ] Assign UI references
- [ ] File → Build Settings:
  - [ ] Platform: Windows
  - [ ] Scenes: LoginScene, MenuScene, GameScene
  - [ ] Server Build: DISABLED
- [ ] Build → Builds/Client_Windows/AntKnow.exe
- [ ] Wait for build (~10-15 min)

**✅ Hour 5 Complete:** [ ]

---

### **HOUR 6-7: Test Multiplayer** (5:00-7:00)
- [ ] Terminal 1: Run server (RunServer.bat)
- [ ] Terminal 2: Run Client 1 (AntKnow.exe)
  - [ ] Enter IP: 127.0.0.1
  - [ ] Click Connect
  - [ ] Verify: "Connected!"
- [ ] Terminal 3: Run Client 2 (AntKnow.exe)
  - [ ] Enter IP: 127.0.0.1
  - [ ] Click Connect
  - [ ] Verify: "Connected!"
- [ ] Check server log:
  - [ ] "Client 1000 APPROVED. Players: 1/4"
  - [ ] "Client 1001 APPROVED. Players: 2/4"
  - [ ] "Starting game in 5s..."
  - [ ] "STARTING GAME"
- [ ] Check client logs:
  - [ ] "GAME STARTED!"
  - [ ] "Turn started for Player 1"
- [ ] Test turn system:
  - [ ] Player 1 can roll dice
  - [ ] Player 2 sees the roll
  - [ ] Turns alternate

**✅ Hour 6-7 Complete:** [ ]

---

### **HOUR 8: Bug Fixes** (7:00-8:00)
- [ ] Fix connection issues (if any)
- [ ] Fix turn system bugs (if any)
- [ ] Test disconnect handling
- [ ] Test reconnection
- [ ] Verify all features work

**✅ Hour 8 Complete:** [ ]

---

**✅ DAY 1 COMPLETE:** [ ]

**Day 1 Success Criteria:**
- [ ] Server builds and runs
- [ ] 2+ clients can connect
- [ ] Game starts automatically
- [ ] Turn system works
- [ ] Dice rolling syncs

---

## 📅 DAY 2: CORE GAMEPLAY (Target: 8-10 giờ)

### **HOUR 1-2: Dice & Movement** (0:00-2:00)
- [ ] Implement dice rolling sync
- [ ] Add "Roll Dice" button to client
- [ ] Test: All clients see same dice result
- [ ] Implement player movement sync
- [ ] Add NetworkVariable for position
- [ ] Animate player movement
- [ ] Test: All clients see movement

**✅ Hour 1-2 Complete:** [ ]

---

### **HOUR 3-4: Property System** (2:00-4:00)
- [ ] Implement RequestBuyPropertyServerRpc
- [ ] Validate: Player has money
- [ ] Validate: Property not owned
- [ ] Deduct money, set owner
- [ ] Broadcast update
- [ ] Show PanelBuy on client
- [ ] Test: Buy property syncs

**✅ Hour 3-4 Complete:** [ ]

---

### **HOUR 5-6: Rent System** (4:00-6:00)
- [ ] Implement HandleRentPayment
- [ ] Calculate rent based on property level
- [ ] Transfer money between players
- [ ] Broadcast update
- [ ] Show rent notification on client
- [ ] Test: Rent payment syncs

**✅ Hour 5-6 Complete:** [ ]

---

### **HOUR 7-8: Money Sync** (6:00-8:00)
- [ ] Add NetworkVariable for money
- [ ] Update on buy/rent/salary
- [ ] Sync to all clients
- [ ] Update UI on money change
- [ ] Add money change animation
- [ ] Test: All money transactions sync

**✅ Hour 7-8 Complete:** [ ]

---

### **HOUR 9-10: Integration Test** (8:00-10:00)
- [ ] Full game flow: Roll → Move → Buy → Rent
- [ ] Test with 4 players
- [ ] Fix bugs
- [ ] Verify all features work

**✅ Hour 9-10 Complete:** [ ]

---

**✅ DAY 2 COMPLETE:** [ ]

**Day 2 Success Criteria:**
- [ ] Dice rolling syncs
- [ ] Player movement syncs
- [ ] Can buy properties
- [ ] Rent payment works
- [ ] Money syncs correctly

---

## 📅 DAY 3: ADVANCED FEATURES (Target: 8-10 giờ)

### **HOUR 1-2: Property Upgrades** (0:00-2:00)
- [ ] Implement RequestUpgradePropertyServerRpc
- [ ] Validate: Player owns property
- [ ] Validate: Has money
- [ ] Deduct upgrade cost
- [ ] Set property level
- [ ] Show house/hotel models
- [ ] Test: Upgrades sync

**✅ Hour 1-2 Complete:** [ ]

---

### **HOUR 3-4: Special Tiles** (2:00-4:00)
- [ ] Implement Start tile (+200 salary)
- [ ] Implement Jail tile
- [ ] Implement Tax/Bonus tiles
- [ ] Test: All special tiles work

**✅ Hour 3-4 Complete:** [ ]

---

### **HOUR 5: Turn Timer** (4:00-5:00)
- [ ] Server: Track turn start time
- [ ] Server: Auto-end turn after 60s
- [ ] Client: Show countdown timer
- [ ] Test: Timeout handling

**✅ Hour 5 Complete:** [ ]

---

### **HOUR 6-7: End Game Logic** (5:00-7:00)
- [ ] Check end conditions (max turns, 1 player left)
- [ ] Calculate scores
- [ ] Determine winner
- [ ] Broadcast results
- [ ] Show PanelResult on client
- [ ] Test: Game ends correctly

**✅ Hour 6-7 Complete:** [ ]

---

### **HOUR 8-9: Firebase Integration** (7:00-9:00)
- [ ] Save game results to Firestore
- [ ] Update player stats
- [ ] Award coins/exp
- [ ] Test: Save/load works

**✅ Hour 8-9 Complete:** [ ]

---

### **HOUR 10: Bug Fixes & Polish** (9:00-10:00)
- [ ] Fix critical bugs
- [ ] Improve UI feedback
- [ ] Add sound effects
- [ ] Test edge cases

**✅ Hour 10 Complete:** [ ]

---

**✅ DAY 3 COMPLETE:** [ ]

**Day 3 Success Criteria:**
- [ ] House/hotel system works
- [ ] Special tiles implemented
- [ ] Game ends properly
- [ ] Results saved to Firebase

---

## 📅 DAY 4: DEPLOYMENT (Target: 8-10 giờ)

### **HOUR 1-2: Setup Cloud Server** (0:00-2:00)
- [ ] Choose platform (AWS/GCP/Unity Multiplay)
- [ ] Create VM/Fleet
- [ ] Configure security group (port 7777)
- [ ] Upload server build
- [ ] Run as service
- [ ] Verify server running

**✅ Hour 1-2 Complete:** [ ]

---

### **HOUR 3-4: Client Update** (2:00-4:00)
- [ ] Update server IP in client
- [ ] Build production client
- [ ] Test connection to cloud server
- [ ] Fix connection issues

**✅ Hour 3-4 Complete:** [ ]

---

### **HOUR 5: Load Testing** (4:00-5:00)
- [ ] Test with 4 players
- [ ] Test with 8 players (2 games)
- [ ] Monitor server performance
- [ ] Check for lag/desync

**✅ Hour 5 Complete:** [ ]

---

### **HOUR 6-7: Full Game Testing** (5:00-7:00)
- [ ] 4 players full game (start to end)
- [ ] Player disconnect mid-game
- [ ] Reconnection handling
- [ ] All features working
- [ ] No critical bugs

**✅ Hour 6-7 Complete:** [ ]

---

### **HOUR 8-9: Performance Optimization** (7:00-9:00)
- [ ] Server: Reduce FPS if needed
- [ ] Server: Optimize network traffic
- [ ] Client: Optimize rendering
- [ ] Client: Reduce network calls

**✅ Hour 8-9 Complete:** [ ]

---

### **HOUR 10: Launch Preparation** (9:00-10:00)
- [ ] Create player guide
- [ ] Setup monitoring
- [ ] Prepare support system
- [ ] Final smoke test
- [ ] 🚀 LAUNCH!

**✅ Hour 10 Complete:** [ ]

---

**✅ DAY 4 COMPLETE:** [ ]

**Day 4 Success Criteria:**
- [ ] Server deployed to cloud
- [ ] Clients can connect from internet
- [ ] 4 players can play full game
- [ ] No critical bugs
- [ ] 🚀 GAME LIVE!

---

## 🎉 FINAL CHECKLIST

### **Production Ready**
- [ ] Server running 24/7
- [ ] Clients can connect from anywhere
- [ ] All MUST HAVE features working
- [ ] No critical bugs
- [ ] Performance acceptable
- [ ] Monitoring active
- [ ] Support ready

### **Launch**
- [ ] Server deployed
- [ ] Client builds distributed
- [ ] Player guide available
- [ ] Support system ready
- [ ] 🚀 GAME LAUNCHED!

---

**CONGRATULATIONS! BẠN ĐÃ HOÀN THÀNH! 🎉**

**Total Time**: _____ hours  
**Launch Date**: _____/_____/_____  
**Players**: _____ concurrent users

