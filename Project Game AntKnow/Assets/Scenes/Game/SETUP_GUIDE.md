# 🎮 GameScene Setup Guide - Hướng Dẫn Duy Nhất

## 🎯 Mục Tiêu: Làm Game LOCAL Trước, Online Sau

### Phase 1: LOCAL SINGLE PLAYER (Làm trước)
```
✅ 1 player di chuyển trên board
✅ Roll dice
✅ Mua nhà
✅ Trả tiền thuê
✅ UI panels hoạt động
```

### Phase 2: MULTIPLAYER ONLINE (Làm sau)
```
⏳ 2-4 players online
⏳ Netcode synchronization
⏳ Server authoritative
```

---

## 📦 Scripts Hiện Có:

### Core Scripts (Đã có):
```
✅ GameManager.cs - Quản lý game flow
✅ BoardManager.cs - Quản lý 36 tiles
✅ DiceController.cs - Xúc xắc
✅ PlayerGameController.cs - Player movement
✅ PropertyManager.cs - Mua/bán/thuê nhà
✅ TurnIndicator.cs - Ping trên đầu player
```

### UI Scripts (Đã có):
```
✅ PanelPlayerMe.cs
✅ PanelPlayer.cs
✅ PanelBuy.cs
✅ PanelQuiz.cs
✅ PanelEvent.cs
✅ PanelHouseSell.cs
✅ PanelResult.cs
✅ PanelCard.cs
```

---

## 🔧 SETUP TRONG UNITY (15 phút)

### Step 1: Setup GameManager (5 phút)

```
1. Open Unity → GameScene

2. Select GameManager GameObject

3. Add PropertyManager:
   - Add Component → PropertyManager
   - GameManager Inspector → Property Manager: Drag PropertyManager

4. Check GameManager Inspector:
   ✅ Board Manager: Linked
   ✅ Dice Controller: Linked
   ✅ Property Manager: Linked
   ✅ Player Prefab: Linked
   ✅ Roll Button: Linked
   ✅ Demo Mode: TRUE (quan trọng!)
   ✅ Max Turns: 25
```

### Step 2: Setup Player Prefab (5 phút)

```
1. Open Player Prefab (Assets/Prefabs/Player.prefab)

2. Add TurnIndicator:
   - Right-click Player → Create Empty
   - Name: "TurnIndicator"
   - Position: (0, 2.5, 0)
   - Add Component → TurnIndicator

3. Link TurnIndicator:
   - Select Player root
   - PlayerGameController → Turn Indicator: Drag TurnIndicator

4. Save Prefab (Ctrl+S)
```

### Step 3: Setup Board (5 phút)

```
1. Check BoardManager có 36 waypoints:
   - Select BoardManager
   - Check Waypoint Path component
   - Should have 36 waypoints in circular path

2. Nếu chưa có waypoints:
   - Add Component → WaypointGenerator
   - Set: Waypoint Count = 36
   - Set: Radius = 10
   - Click "Generate Waypoints" button
```

---

## 🎮 TEST GAME (5 phút)

### Press Play:

```
1. Press Play ▶️

2. Check Console:
   ✅ "[GameManager] Starting game..."
   ✅ "[GameManager] Spawned player: Player 1"
   ✅ "[GameManager] Turn 1 - Player 1's turn"
   ✅ No errors

3. Check Scene:
   ✅ 1-2 players spawned at tile 0
   ✅ Yellow ping on current player's head
   ✅ Ping bobs up and down

4. Click Roll Button:
   ✅ Dice animate
   ✅ Player moves
   ✅ Player bounces
   ✅ Ping follows player

5. Land on Property:
   ✅ Auto buy if enough money
   ✅ Money decreases
   ✅ Console shows: "Player bought property X"

6. Land on Owned Property:
   ✅ Pay rent
   ✅ Money decreases (tenant)
   ✅ Money increases (owner)
   ✅ Console shows rent calculation
```

---

## 🐛 Troubleshooting:

### Lỗi: PropertyManager null
```
Fix: Add PropertyManager component to GameManager
```

### Lỗi: Player không spawn
```
Fix: Check Demo Mode = TRUE trong GameManager
```

### Lỗi: Ping không hiện
```
Fix: Check TurnIndicator added to Player Prefab
```

### Lỗi: Không có waypoints
```
Fix: Use WaypointGenerator to generate 36 waypoints
```

---

## 📋 Next Steps:

### Sau khi LOCAL game chạy được:

```
1. Add UI Panels (PanelBuy, PanelQuiz, etc.)
2. Add manual buy/upgrade (không auto)
3. Add special tiles (Quiz, Event, Travel, Jail)
4. Add card system
5. Add end game logic

→ Sau đó mới làm MULTIPLAYER ONLINE
```

---

## 💡 Quan Trọng:

### Demo Mode = TRUE:
```
- Spawn 2 test players
- Auto buy property
- Auto pay rent
- Không cần lobby/network
```

### Demo Mode = FALSE:
```
- Load players from lobby
- Cần network connection
- Cần GameSessionData
- Chỉ dùng khi làm multiplayer
```

---

## 🎯 Current Focus:

```
✅ LOCAL game works
✅ 1 player can move
✅ 1 player can buy property
✅ 1 player can pay rent
✅ UI shows correctly

→ Làm xong LOCAL trước
→ Sau đó mới làm ONLINE
```

---

**Press Play và test ngay! 🎮**

