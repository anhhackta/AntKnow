# 🎯 UNITY EDITOR SETUP GUIDE - GAMESCENE

**Date**: October 12, 2025  
**Status**: Ready to setup (Đã có map + UI assets)

---

## 📋 OVERVIEW

Bạn đã có:
- ✅ Map với 36 tiles
- ✅ UI assets
- ✅ Code hoàn chỉnh
- ✅ SimpleBoardConfig với 36 tiles data (tên + giá)

Cần làm:
1. **Setup Tiles** - Link tile names & prices từ SimpleBoardConfig
2. **Setup UI Panels** - Add references và ImageBackground
3. **Setup GameManager** - Connect all references
4. **Test** - Demo mode

---

## 🗺️ PART 1: SETUP 36 TILES

### Step 1.1: Tìm Tiles trong Scene
1. Open **GameScene** 
2. Hierarchy → tìm folder/group chứa 36 tiles của map
3. Tiles có thể có tên: `Tile_0` đến `Tile_35` hoặc `Waypoint_0` đến `Waypoint_35`

**📝 Note**: Tile Index trong Unity là 0-35, nhưng Tile ID trong code là 1-36
- Tile_0 (Unity) = Tile 1 "Ô Bắt Đầu" (Code)
- Tile_35 (Unity) = Tile 36 "Da Nang" (Code)

### Step 1.2: Tạo TileVisual Component cho mỗi Tile
Với mỗi tile từ 0-35:

1. **Select tile** trong Hierarchy
2. **Add Component** → Search "TileVisual"
3. **Set Tile Index**:
   - Tile_0 → Tile Index = **0**
   - Tile_1 → Tile Index = **1**
   - ...
   - Tile_35 → Tile Index = **35**

4. **Assign Platform** (quan trọng cho houses!):
   - Mỗi tile có 1 platform/base object (nền phẳng để spawn houses)
   - Kéo platform object vào field `Platform` trong TileVisual component
   - Platform cần có Renderer component (để đổi màu theo owner)

### Step 1.3: Setup TextMeshPro cho Tile Names & Prices
Mỗi tile cần 2 TextMeshPro objects:

**Option A: Nếu chưa có Text trong Tiles**
1. Select Tile → Right-click → UI → Text - TextMeshPro
2. Tạo 2 texts:
   - `TextName` - Hiển thị tên tile (ví dụ: "Tokyo")
   - `TextPrice` - Hiển thị giá (ví dụ: "$800")
3. Position texts phía trên tile (visible from camera)
4. Font size: 
   - TextName: 24-36
   - TextPrice: 20-32
5. Alignment: Center

**Option B: Nếu đã có Text UI sẵn trong map**
1. Chỉ cần assign vào TileVisual component

**Assign vào TileVisual**:
1. Select Tile
2. Inspector → TileVisual component
3. Kéo `TextName` → field `Text Name`
4. Kéo `TextPrice` → field `Text Price`

### Step 1.4: Setup Material cho Houses (Important!)
Houses sẽ được spawn runtime, cần material "ngói" để đổi màu:

1. **Check House Prefab**:
   - Trong Project window, tìm House prefab của bạn
   - Double-click để mở Prefab mode
   - Tìm part "roof" (mái nhà)
   - Renderer → Materials → Tìm material có tên "ngói" hoặc tạo mới

2. **Create "ngói" Material** (nếu chưa có):
   - Project window → Right-click → Create → Material
   - Tên: `ngói` (EXACT name, lowercase)
   - Shader: Standard hoặc URP/Lit
   - Assign material này vào roof của house prefab

3. **Verify Material Name**:
   - Code sẽ tìm material có tên chứa "ngói"
   - Check TileVisual.cs line 247: `if (rend.material.name.Contains(materialName))`

---

## 🎨 PART 2: SETUP UI PANELS

### Step 2.1: Canvas Structure
Verify Canvas structure (should be like this):
```
Canvas
├── PanelGame (Container)
│   ├── ImageBackground ← ADD THIS!
│   ├── PanelMe (Local player)
│   │   ├── ImageBackground ← ADD THIS!
│   │   ├── TextPlayerName
│   │   ├── TextMoney
│   │   ├── ImageAvatar
│   │   └── ButtonInfo
│   └── PanelPlayersContainer (Other players)
│       └── (PanelPlayerPrefab instances spawned runtime)
├── PanelGameInfo
│   ├── TextCurrentPlayer
│   ├── TextTurn
│   └── TextTime
├── PanelRoll
│   ├── ButtonRoll
│   ├── TextResult
│   └── ImageDice
├── PanelBuy
│   ├── TextPropertyName
│   ├── TextPropertyPrice
│   ├── ButtonHouse1
│   ├── ButtonHouse2
│   ├── ButtonHouse3
│   ├── ButtonHouse4
│   ├── ButtonHotel ← IMPORTANT: Must have 5 buttons!
│   ├── ButtonBuy
│   └── ButtonClose
├── PanelQuiz (tôi sẽ guide sau)
├── PanelEvent (tôi sẽ guide sau)
├── PanelHouseSell
├── PanelResult
├── PanelNotification
└── PanelInfo
```

### Step 2.2: Add ImageBackground to PanelMe
1. **Hierarchy** → Canvas → PanelGame → **PanelMe**
2. **Right-click PanelMe** → UI → Image
3. **Rename** to `ImageBackground`
4. **Move to top** in Hierarchy (first child of PanelMe) để render phía sau
5. **Inspector** → Rect Transform:
   - Anchors: **Stretch** (Alt + Shift + Click on Stretch preset)
   - Left: **0**, Top: **0**, Right: **0**, Bottom: **0**
   - (Fills entire PanelMe)

6. **Inspector** → Image component:
   - Source Image: **None** (hoặc solid white sprite nếu có)
   - Color: **White** (255, 255, 255, 255) - code sẽ set màu
   - Raycast Target: **UNCHECK** (không chặn click)

7. **Assign Reference**:
   - Select PanelMe
   - Inspector → **PanelPlayerMe (Script)**
   - Kéo `ImageBackground` → field **Image Background**
   - Set **Background Alpha** = **0.3**

### Step 2.3: Setup PanelPlayerPrefab
1. **Project window** → Tìm `PanelPlayerPrefab`
2. **Double-click** để mở Prefab mode
3. **Repeat Step 2.2** (add ImageBackground giống PanelMe)
4. **Assign Reference**:
   - Select PanelPlayerPrefab root
   - Inspector → **PanelPlayer (Script)**
   - Kéo `ImageBackground` → field **Image Background**
   - Set **Background Alpha** = **0.3**
5. **Save Prefab** (Ctrl + S)

### Step 2.4: Setup Avatar Sprites (Male/Female)
PanelMe và PanelPlayerPrefab cần sprites cho male/female avatars:

1. **Import Avatar Sprites** (nếu chưa có):
   - Project window → Assets/Sprites/Avatars (create folder if needed)
   - Kéo 2 images: `avatar_male.png`, `avatar_female.png`
   - Texture Type: **Sprite (2D and UI)**
   - Apply

2. **Assign to PanelMe**:
   - Select PanelMe
   - Inspector → PanelPlayerMe (Script)
   - Kéo `avatar_male` → field **Sprite Male**
   - Kéo `avatar_female` → field **Sprite Female**

3. **Assign to PanelPlayerPrefab**:
   - Open PanelPlayerPrefab
   - Repeat step 2

### Step 2.5: Verify PanelBuy Has 5 Buttons
**CRITICAL**: PanelBuy MUST have 5 buttons (4 houses + 1 hotel)

1. **Hierarchy** → Canvas → **PanelBuy**
2. **Check buttons**:
   - ✅ ButtonHouse1
   - ✅ ButtonHouse2
   - ✅ ButtonHouse3
   - ✅ ButtonHouse4
   - ✅ **ButtonHotel** ← MUST HAVE THIS!
   - ✅ ButtonBuy
   - ✅ ButtonClose

3. **If missing ButtonHotel**:
   - Right-click PanelBuy → UI → Button - TextMeshPro
   - Rename to `ButtonHotel`
   - Position it after ButtonHouse4
   - Text: "Hotel - $1200" (placeholder)

4. **Assign References** in PanelBuy.cs:
   - Select PanelBuy
   - Inspector → PanelBuy (Script)
   - Assign all 5 buttons to correct fields:
     - btnHouse1, btnHouse2, btnHouse3, btnHouse4, **btnHotel**

### Step 2.6: Setup Other Panels (Quick Check)
Verify these panels exist and have basic components:

- **PanelGameInfo**: TextCurrentPlayer, TextTurn, TextTime
- **PanelRoll**: ButtonRoll, TextResult
- **PanelHouseSell**: List of PropertySellItem (will setup later)
- **PanelResult**: TextWinner, ButtonReturn
- **PanelNotification**: TextMessage, ButtonClose
- **PanelInfo**: (Player detail panel, spawned from PanelGame)

---

## 🎮 PART 3: SETUP GAMEMANAGER

### Step 3.1: Create GameManager GameObject
1. **Hierarchy** → Right-click → Create Empty
2. **Rename** to `GameManager`
3. **Add Component** → Search "GameManager"

### Step 3.2: Assign References - Player Prefabs
1. **Select GameManager**
2. **Inspector** → GameManager (Script)

**Player Prefabs**:
- Kéo **Male Player Prefab** → field `Player Prefab Male`
- Kéo **Female Player Prefab** → field `Player Prefab Female`

**Important**: Player prefabs MUST have:
- ✅ NetworkObject component
- ✅ PlayerGameController component
- ✅ Animator (optional)
- ✅ Collider (optional)

### Step 3.3: Assign References - Board & Property
**Board Manager**:
1. **Hierarchy** → Right-click → Create Empty
2. **Rename** to `BoardManager`
3. **Add Component** → "BoardManager"
4. **Kéo BoardManager GameObject** → GameManager field `Board Manager`

**Property Manager**:
1. **Hierarchy** → Right-click → Create Empty
2. **Rename** to `PropertyManager`
3. **Add Component** → "PropertyManager"
4. **Kéo PropertyManager GameObject** → GameManager field `Property Manager`

**Property Visual**:
1. **Hierarchy** → Right-click → Create Empty
2. **Rename** to `PropertyVisual`
3. **Add Component** → "PropertyVisual"
4. **Kéo PropertyVisual GameObject** → PropertyManager field `Property Visual`

### Step 3.4: Assign References - UI Panels
Select GameManager, assign all UI panels:

- `Panel Game` → Canvas/PanelGame
- `Panel Game Info` → Canvas/PanelGameInfo
- `Panel Roll` → Canvas/PanelRoll
- `Panel Buy` → Canvas/PanelBuy
- `Panel Quiz` → Canvas/PanelQuiz
- `Panel Event` → Canvas/PanelEvent
- `Panel House Sell` → Canvas/PanelHouseSell
- `Panel Result` → Canvas/PanelResult
- `Panel Notification` → Canvas/PanelNotification (hoặc tạo mới)
- `Panel Card` → Canvas/PanelCard (hoặc tạo mới)

### Step 3.5: Setup Waypoints in BoardManager
BoardManager cần 36 waypoints:

**Option A: Manual Assignment (Recommended)**
1. **Create Waypoints GameObject**:
   - Hierarchy → Create Empty → Rename to `Waypoints`
   - Position: (0, 0, 0)

2. **Create 36 children** (tedious but precise):
   - Right-click Waypoints → Create Empty
   - Rename: `Waypoint_0`, `Waypoint_1`, ..., `Waypoint_35`
   - Position waypoints theo vị trí thực tế trên map (circular path)

3. **Assign to BoardManager**:
   - Select BoardManager
   - Inspector → BoardManager (Script)
   - Expand `Waypoints` array → Size: **36**
   - Kéo 36 waypoints vào đúng thứ tự (0-35)

**Option B: Use Existing Tile Positions**
Nếu 36 tiles của bạn đã có position đúng:
1. Select BoardManager
2. Inspector → BoardManager (Script)
3. Expand `Waypoints` → Size: 36
4. Kéo 36 Tiles (có TileVisual) vào array theo thứ tự

**Waypoint Order** (Important!):
```
Start at Tile 0 (bottom-right corner)
→ Clockwise: 0 → 1 → 2 → ... → 35 → back to 0
```

### Step 3.6: Setup PropertyVisual Prefabs
1. **Select PropertyVisual** GameObject
2. **Inspector** → PropertyVisual (Script)

**Assign Prefabs**:
- `House Prefab` → Kéo House prefab (with "ngói" material)
- `Hotel Prefab` → Kéo Hotel prefab (with "ngói" material)

**Settings**:
- `Roof Material Name` = `ngói` (must match material name!)

**Player Colors** (already set in code, verify):
- Color 0: Red (255, 51, 51)
- Color 1: Blue (51, 128, 255)
- Color 2: Green (51, 255, 51)
- Color 3: Yellow (255, 255, 51)

**Assign TileSetup** (if you have TileSetup script):
- If you created a TileSetup.cs to manage all tiles, assign it here
- Otherwise, PropertyVisual will auto-find TileVisual components

### Step 3.7: GameManager Settings
Select GameManager, configure settings:

**Game Settings**:
- `Max Turns` = **25** (25 vòng)
- `Demo Mode` = **TRUE** (for testing)

**Services**:
- `Firebase Auth Service` → Tìm FirebaseAuthService trong scene hoặc để null (demo mode không cần)

---

## 🧪 PART 4: TESTING

### Test 4.1: Demo Mode Test (Offline, 1 Player)
1. **Set Demo Mode**:
   - Select GameManager
   - Inspector → Demo Mode: **CHECK**

2. **Disable NetworkManager** (for offline test):
   - Tìm NetworkManager trong scene
   - Disable GameObject (hoặc remove NetworkManager component)

3. **Click Play**:
   - Should spawn 1 test player at Tile 0
   - PanelMe should show player info with **light red background** (Player 1)

4. **Check Console**:
   ```
   [GameManager] Demo Mode: Starting game without network...
   [GameManager] Spawned male test player: Player 1 (Index: 0)
   [BoardManager] Loaded 36 tile data (Tile ID 1-36)
   ```

5. **Verify UI**:
   - ✅ PanelMe has red background (alpha 30%)
   - ✅ Avatar shows male/female sprite (not color tint)
   - ✅ Name: "Player 1"
   - ✅ Money: "$10000" (not "10000")

### Test 4.2: Test Tiles & Data
1. **In Play Mode**, open Console
2. **Check BoardManager logs**:
   ```
   [BoardManager] Loaded 36 tile data (Tile ID 1-36)
   [BoardManager] Initialized 36 waypoints
   ```

3. **Select any Tile** in Hierarchy
4. **Inspector** → TileVisual component
5. **Verify**:
   - ✅ Tile Index matches position (0-35)
   - ✅ Platform assigned
   - ✅ TextName shows correct name (e.g., "Tokyo" for Tile 2)
   - ✅ TextPrice shows correct price (e.g., "$800" for Tile 2)

**Tile Data Reference** (từ SimpleBoardConfig.cs):
| Tile # | Name | Type | Price |
|--------|------|------|-------|
| 1 | Ô Bắt Đầu | Start | $0 |
| 2 | Tokyo | Property | $800 |
| 3 | Seoul | Property | $700 |
| 4 | Bangkok | Property | $600 |
| 5 | Singapore | Property | $750 |
| 6 | Manila | Property | $550 |
| 7 | Ô Event | Event | $0 |
| 8 | Jakarta | Property | $600 |
| 9 | Beijing | Property | $700 |
| 10 | Ô Tai Nạn | Jail | $0 |
| ... | ... | ... | ... |
| 36 | Da Nang | Property | $750 |

### Test 4.3: Test Movement (Basic)
1. **In Play Mode**
2. **Wait for PanelRoll** to appear
3. **Click "Roll Dice"** button
4. **Expected**:
   - Player moves along waypoints
   - Stops at correct tile
   - TileVisual shows tile name and price

### Test 4.4: Test Property Purchase
1. **Move player to a Property tile** (e.g., Tile 2 Tokyo)
2. **PanelBuy should appear**
3. **Verify**:
   - ✅ Property Name: "Tokyo"
   - ✅ Buy Price: "$800"
   - ✅ 5 buttons visible: House 1-4 + Hotel
   - ✅ Hotel button **DISABLED** (grayed out)
   - ✅ House buttons show prices ($400, $500, $600, $700)

4. **Click "Buy Land"**
5. **Expected**:
   - Player money decreases by $800
   - Platform color changes to **RED** (Player 1 color)
   - TextPrice shows rent: "$80"

6. **Land on same tile again**
7. **PanelBuy appears** with upgrade options
8. **Click "House 1"**
9. **Expected**:
   - Player money decreases by $400
   - **1 red house spawns** on platform
   - TextPrice shows new rent: "$200"

10. **Buy House 2, 3, 4** (repeat)
11. **After House 4**:
    - ✅ **Hotel button becomes ENABLED** (clickable)
    - Hotel price: "$1200"

12. **Click "Hotel"**
13. **Expected**:
    - 4 houses disappear
    - **1 red hotel spawns**
    - TextPrice shows hotel rent: "$2000"

### Test 4.5: Test Player Colors (Multi-player simulation)
**To test 4 player colors, modify Demo Mode spawn**:

1. **Open GameManager.cs** line 200
2. **Temporarily add more players**:
```csharp
// Demo mode: spawn 4 test players
SpawnTestPlayer("Player 1", "test_1", true, 10, 10, 10, 10, 10);
SpawnTestPlayer("Player 2", "test_2", false, 10, 10, 10, 10, 10);
SpawnTestPlayer("Player 3", "test_3", true, 10, 10, 10, 10, 10);
SpawnTestPlayer("Player 4", "test_4", false, 10, 10, 10, 10, 10);
```

3. **Click Play**
4. **Verify**:
   - PanelMe: **Red background** (Player 1)
   - PanelPlayer 1: **Blue background** (Player 2)
   - PanelPlayer 2: **Green background** (Player 3)
   - PanelPlayer 3: **Yellow background** (Player 4)

5. **Buy properties with different players** (manually in code or via debug commands)
6. **Verify houses have correct colors**:
   - Player 1 property → Red houses
   - Player 2 property → Blue houses
   - Player 3 property → Green houses
   - Player 4 property → Yellow houses

---

## ✅ SETUP CHECKLIST

### Tiles Setup:
- [ ] All 36 tiles have TileVisual component
- [ ] Tile Index set correctly (0-35)
- [ ] Platform assigned to each TileVisual
- [ ] TextName assigned to each TileVisual
- [ ] TextPrice assigned to each TileVisual
- [ ] House prefab has "ngói" material on roof
- [ ] Hotel prefab has "ngói" material on roof

### UI Setup:
- [ ] ImageBackground added to PanelMe (first child)
- [ ] ImageBackground added to PanelPlayerPrefab (first child)
- [ ] ImageBackground assigned in PanelPlayerMe.cs
- [ ] ImageBackground assigned in PanelPlayer.cs
- [ ] Background Alpha set to 0.3 (both panels)
- [ ] Avatar sprites assigned (male/female)
- [ ] PanelBuy has 5 buttons (House 1-4 + Hotel)
- [ ] All buttons assigned in PanelBuy.cs

### GameManager Setup:
- [ ] GameManager GameObject created
- [ ] Player Prefab Male assigned
- [ ] Player Prefab Female assigned
- [ ] BoardManager assigned
- [ ] PropertyManager assigned
- [ ] PropertyVisual assigned
- [ ] All UI panels assigned (10 panels)
- [ ] Demo Mode enabled for testing

### BoardManager Setup:
- [ ] 36 Waypoints created and positioned
- [ ] Waypoints assigned to BoardManager array (size 36)
- [ ] Waypoint order is clockwise from Tile 0

### PropertyVisual Setup:
- [ ] House Prefab assigned
- [ ] Hotel Prefab assigned
- [ ] Roof Material Name = "ngói"
- [ ] Player Colors verified (Red, Blue, Green, Yellow)

### Testing:
- [ ] Demo mode runs without errors
- [ ] Player spawns at Tile 0
- [ ] PanelMe shows red background
- [ ] Tiles show correct names and prices
- [ ] Can roll dice and move
- [ ] Can buy property
- [ ] Platform changes color to red
- [ ] Can buy houses (1-4)
- [ ] Houses spawn with red color
- [ ] Hotel button enables after House 4
- [ ] Can buy hotel
- [ ] Hotel spawns with red color

---

## 🚨 COMMON ISSUES & FIXES

### Issue 1: "Waypoints not initialized!"
**Fix**: Assign 36 waypoints to BoardManager manually

### Issue 2: Hotel button missing in PanelBuy
**Fix**: Add ButtonHotel, assign to btnHotel field in PanelBuy.cs

### Issue 3: Houses spawn but no color
**Fix**: 
- Check House prefab has material named "ngói"
- Check PropertyVisual.roofMaterialName = "ngói"
- Verify SetHouseColor() finds material in TileVisual.cs

### Issue 4: PanelMe has no background color
**Fix**:
- Add ImageBackground as first child
- Assign to imageBackground field
- Set backgroundAlpha = 0.3

### Issue 5: Tile names/prices don't show
**Fix**:
- Assign TextName and TextPrice to TileVisual
- Check TileVisual.tileIndex matches SimpleBoardConfig data
- Verify BoardManager loads SimpleBoardConfig

### Issue 6: NetworkObject error in Demo Mode
**Fix**:
- Disable NetworkManager GameObject
- OR remove NetworkObject from player prefabs for offline test
- OR keep Demo Mode enabled (GameManager handles it)

---

## 🎉 NEXT AFTER SETUP

After completing this setup:

1. **Multiplayer Test** (with ParrelSync):
   - Disable Demo Mode
   - Enable NetworkManager
   - Test with 2-4 clients
   - Verify colors sync across clients

2. **Implement Quiz System**:
   - Firebase integration for questions
   - PanelQuiz UI and logic
   - Trigger quiz every 8 rounds

3. **Implement Event System**:
   - Random events (gain/lose money, move, etc.)
   - PanelEvent UI and logic
   - Trigger on Event tiles (7, 16, 25, 33)

4. **Implement Fortune Wheel**:
   - Wheel animation
   - Random rewards
   - Integration with game flow

5. **Polish & Testing**:
   - Add animations (panel transitions, dice roll, player movement)
   - Sound effects
   - Particle effects (confetti on hotel purchase, etc.)
   - Full game test (25 turns, end game flow)

---

**Good luck với setup! 🚀 Let me know nếu gặp issue nào!**
