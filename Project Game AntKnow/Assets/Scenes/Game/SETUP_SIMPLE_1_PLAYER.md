# 🎮 Setup Simple - 1 Player Test

## 🎯 Mục Tiêu:
```
✅ Test 1 player di chuyển
✅ Roll dice
✅ Bounce effect + look at center
✅ Mua nhà tự động
✅ Nhà spawn trên platform đúng
✅ Bỏ qua win conditions
✅ Code đơn giản, gọn gàng
```

---

## 📋 SETUP (15 phút):

### STEP 1: Chuẩn Bị Map (5 phút)

#### 1.1. Check GameObject "Tiles":
```
Hierarchy:
└── Tiles (parent)
    ├── Tile_0 (Start)
    ├── Tile_1 (Tokyo)
    ├── Tile_2 (Seoul)
    ├── ...
    └── Tile_35 (Melbourne)

Total: 36 tiles
```

#### 1.2. Check Tile Structure:

**Special Tiles (0, 10, 19, 28):**
```
Tile_0:
├── Cube (visual)
└── Text: "Ô Bắt Đầu"
```

**Event Tiles (7, 16, 25, 33):**
```
Tile_7:
├── Cube (visual)
└── Text: "Ô Event"
```

**Property Tiles (26 tiles còn lại):**
```
Tile_1:
├── Cube (visual)
├── Platform (cube mỏng dẹp) ⭐
├── Text Name: "Tokyo"
└── Text Price: "800"
```

#### 1.3. Tag Platform:
```
1. Select Platform trong mỗi Property Tile
2. Tag → Add Tag → "Platform"
3. Set Tag = "Platform"

Hoặc đổi tên thành "Platform" (chữ thường cũng được)
```

---

### STEP 2: Setup TileSetup (2 phút)

```
1. Select GameObject "Tiles"

2. Add Component → TileSetup

3. Settings:
   - Auto Setup On Awake: TRUE
   - Add Tile Visual Component: TRUE
   - Show Debug: TRUE

4. Right-click TileSetup → "Setup All Tiles"

5. Check Console:
   ✅ "[TileSetup] Setup complete! Total tiles: 36"
   ✅ "[TileVisual] Found platform by tag: Platform" (x26)
```

---

### STEP 3: Setup GameManager (5 phút)

```
1. Select GameManager

2. Check components:
   ✅ GameManager.cs
   ✅ PropertyManager.cs (add if missing)
   ✅ PropertyVisual.cs (add if missing)

3. GameManager settings:
   - Demo Mode: TRUE ⭐
   - Max Players: 1 ⭐ (test 1 player)
   - Starting Money: 2000
   - Property Manager: Drag PropertyManager
   - Board Manager: Drag BoardManager
   - Dice Controller: Drag DiceController
   - Player Prefab: Drag from Project
   - Roll Button: Drag button

4. PropertyManager settings:
   - Property Visual: Drag PropertyVisual
   - Board Manager: Drag BoardManager

5. PropertyVisual settings:
   - Tile Setup: Drag "Tiles" GameObject ⭐
   - House Prefab: Drag house prefab
   - Hotel Prefab: Drag hotel prefab
   - Roof Material Name: "ngói" (hoặc tên material bạn dùng)
   - Player Colors: [Red, Blue, Green, Yellow]
```

---

### STEP 4: Setup Player Prefab (3 phút)

```
1. Open Player Prefab

2. Add TurnIndicator:
   - Create Empty child: "TurnIndicator"
   - Position: (0, 2.5, 0)
   - Add Component → TurnIndicator
   - Bob Speed: 2
   - Bob Height: 0.3

3. Create Ping Visual:
   - Add Sphere child to TurnIndicator
   - Name: "Ping"
   - Scale: (0.5, 0.5, 0.5)
   - Material: Yellow with Emission
   - Remove Sphere Collider

4. Link:
   - TurnIndicator → Ping Object: Drag "Ping"
   - PlayerGameController → Turn Indicator: Drag "TurnIndicator"

5. Save Prefab
```

---

## ✅ TEST (5 phút):

### Test 1: Game Start
```
1. Press Play ▶️

2. Check Console:
   ✅ "[TileSetup] Setup complete! Total tiles: 36"
   ✅ "[BoardManager] Loaded 36 tile data"
   ✅ "[PropertyVisual] Got 36 tiles from TileSetup"
   ✅ "[GameManager] Starting game..."
   ✅ "[GameManager] Spawned player: Player 1"
   ✅ No errors

3. Check Scene:
   ✅ 1 player spawned at Tile 0
   ✅ Yellow ping on player's head
   ✅ Ping bobs up/down
```

### Test 2: Movement
```
1. Click Roll Button

2. Check:
   ✅ Dice animate (roll animation)
   ✅ Dice shows number (1-6)
   ✅ Player moves to new tile
   ✅ Player bounces (parabola curve)
   ✅ Player looks at center
   ✅ Ping follows player
```

### Test 3: Buy Property
```
1. Roll until land on Property (Tile 1-35, except special tiles)

2. Check Console:
   ✅ "[GameManager] Property Tokyo available for purchase: 800"
   ✅ "[PropertyManager] Player 1 bought Tokyo for 800"
   ✅ "[PropertyManager] Money: 2000 → 1200"

3. Check Tile:
   ✅ 1 house spawns on platform
   ✅ House has red color (player 1)
   ✅ House on "ngói" material only
```

### Test 4: Upgrade Property
```
1. Roll until land on own property again

2. Check Console:
   ✅ "[PropertyManager] Upgraded Tokyo to level 2"

3. Check Tile:
   ✅ 2 houses spawn on platform
   ✅ Both houses red color
```

### Test 5: Pay Rent
```
(Skip for 1 player test)
```

---

## 🐛 Troubleshooting:

### Issue: Platform không tìm thấy
```
Fix:
1. Check Platform có tag "Platform"
2. Hoặc đổi tên thành "Platform"
3. Check TileVisual log: "Found platform by..."
```

### Issue: Houses không spawn
```
Fix:
1. Check House prefab linked
2. Check Roof Material Name đúng
3. Check Platform position
4. Check Console for errors
```

### Issue: Houses sai màu
```
Fix:
1. Check house prefab có material tên "ngói"
2. Check Player Colors array
3. Check TileVisual.SetHouseColor() log
```

### Issue: Player không di chuyển
```
Fix:
1. Check WaypointPath có 36 waypoints
2. Check BoardManager initialized
3. Check PlayerGameController.MoveToTile()
```

---

## 📋 Checklist:

### Map:
- [ ] 36 tiles trong "Tiles" GameObject
- [ ] Special tiles: 0, 10, 19, 28
- [ ] Event tiles: 7, 16, 25, 33
- [ ] Property tiles: 26 tiles còn lại
- [ ] Platform có tag "Platform" hoặc name "Platform"

### Components:
- [ ] TileSetup on "Tiles"
- [ ] TileVisual on all 36 tiles (auto-added)
- [ ] PropertyManager on GameManager
- [ ] PropertyVisual on GameManager
- [ ] TurnIndicator on Player Prefab

### References:
- [ ] PropertyVisual → Tile Setup
- [ ] PropertyVisual → House/Hotel prefabs
- [ ] PropertyManager → PropertyVisual
- [ ] GameManager → PropertyManager
- [ ] GameManager → BoardManager
- [ ] PlayerGameController → TurnIndicator

### Settings:
- [ ] Demo Mode = TRUE
- [ ] Max Players = 1
- [ ] Starting Money = 2000
- [ ] Roof Material Name = "ngói"

---

## 🎯 Expected Result:

```
✅ 1 player spawns
✅ Yellow ping on head
✅ Roll dice works
✅ Player moves with bounce
✅ Player looks at center
✅ Land on property → Auto buy
✅ Houses spawn on platform
✅ Houses have correct color
✅ Upgrade → More houses
✅ Level 5 → Hotel
✅ No errors
```

---

## 💡 Next Steps:

### After 1 player works:
```
1. Test 2 players (Max Players = 2)
2. Test pay rent
3. Add PanelBuy (manual buy)
4. Add PanelQuiz
5. Add special tiles logic
6. Add win conditions
7. Add multiplayer
```

---

**Chỉ test 1 player trước! Đơn giản, rõ ràng! 🎮**

