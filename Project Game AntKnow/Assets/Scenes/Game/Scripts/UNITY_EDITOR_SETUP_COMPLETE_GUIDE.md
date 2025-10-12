# 🎮 UNITY EDITOR SETUP - COMPLETE GUIDE

**Date**: October 12, 2025  
**Status**: Ready for Manual Setup

---

## 📋 SETUP OVERVIEW

### Current Status:
- ✅ All scripts compiled successfully
- ✅ PlayerGameController simplified (no NetworkVariables)
- ✅ Player color system implemented (4 colors)
- ✅ 36 tile data ready (SimpleBoardConfig)
- ✅ TextMesh support implemented
- ⚠️ **Unity Inspector assignments needed**

### What Needs Setup:
1. **Player Prefabs** (2 prefabs: Male + Female)
2. **GameManager References** (assign prefabs, UI panels, board)
3. **36 Tiles Setup** (use TileDataAutoSetup tool)
4. **UI Panel Images** (add ImageBackground components)
5. **Testing** (Demo Mode verification)

---

## 🎯 PART 1: CREATE PLAYER PREFABS

### **See**: [PLAYER_PREFAB_SETUP_GUIDE.md](./PLAYER_PREFAB_SETUP_GUIDE.md)

**Quick Steps**:
1. Create **PlayerMale.prefab**:
   - NetworkObject (Is Player Object = TRUE)
   - PlayerGameController (Is Male = **TRUE**)
   - MaleModel child (3D model with Animator)
   - Assign animator to controller

2. Create **PlayerFemale.prefab**:
   - NetworkObject (Is Player Object = TRUE)
   - PlayerGameController (Is Male = **FALSE**)
   - FemaleModel child (3D model with Animator)
   - Assign animator to controller

3. Save in: `Assets/Prefabs/Players/`

---

## 🎯 PART 2: GAMEMANAGER SETUP

### Open GameScene:
1. **Project** → `Assets/Scenes/Game/GameScene.unity`
2. **Double-click** to open
3. **Hierarchy** → Find **GameManager** GameObject

### Assign Player Prefabs:

```
GameManager (Inspector)
└── Game Manager (Script)
    └── Player Prefabs
        ├── Player Prefab Male: [Drag PlayerMale.prefab]
        └── Player Prefab Female: [Drag PlayerFemale.prefab]
```

### Assign UI References:

```
GameManager (Inspector)
└── Game Manager (Script)
    ├── UI References
    │   ├── Panel Me: [Hierarchy → UI → PanelMe]
    │   ├── Panel Buy: [Hierarchy → UI → PanelBuy]
    │   ├── Panel Sell: [Hierarchy → UI → PanelSell]
    │   ├── Panel Quiz: [Hierarchy → UI → PanelQuiz]
    │   ├── Panel Event: [Hierarchy → UI → PanelEvent]
    │   ├── Panel Fortune Wheel: [Hierarchy → UI → PanelFortuneWheel]
    │   ├── Panel Game: [Hierarchy → UI → PanelGame]
    │   ├── Player Panel Container: [Hierarchy → UI → PanelGame → PlayerPanels]
    │   └── Player Panel Prefab: [Project → Prefabs/UI/PanelPlayerPrefab]
    │
    ├── Board References
    │   ├── Board Manager: [Hierarchy → Board → BoardManager]
    │   ├── Board Center: (0, 0, 0) - manual input
    │   └── Tiles Parent: [Hierarchy → Board → Tiles]
    │
    └── Demo Mode
        └── Demo Mode: ✓ TRUE (for offline testing)
```

---

## 🎯 PART 3: SETUP 36 TILES

### Option A: Use Auto-Setup Tool (Recommended) ⚡

**See**: [TILE_SETUP_TEXTMESH_GUIDE.md](./TILE_SETUP_TEXTMESH_GUIDE.md)

1. **Select all 36 tiles** in Hierarchy (Ctrl+Click)
2. **Window** → **AntKnow Tools** → **Tile Data Auto Setup**
3. **Click** "Setup All Tiles"
4. **Result**: All tiles configured automatically! ✅

**Tool Features**:
- Auto-assigns TileVisual component
- Sets tile index (0-35)
- Sets tile name from SimpleBoardConfig
- Sets tile price (Property tiles only)
- Sets tile type (Property/Start/Event/etc.)
- Finds TextMesh components (TextName, TextPrice)
- Hides TextPrice for Special tiles (8 tiles)
- Logs progress in Console

---

### Option B: Manual Setup (Tedious) 🐌

**For each tile** (0-35):

1. **Select Tile** in Hierarchy (e.g., "Tile_00")
2. **Add Component** → **Tile Visual**
3. **Inspector**:
   ```
   Tile Visual (Script)
   ├── Tile Index: 0 (for Tile_00, 1 for Tile_01, etc.)
   ├── Text Name: [Drag child TextMesh "TextName"]
   └── Text Price: [Drag child TextMesh "TextPrice"]
   ```
4. **Repeat 35 more times** 😅

**Special Tiles** (no price):
- Tile 0: Start → No TextPrice needed
- Tile 4, 20: Travel → No TextPrice
- Tile 8, 22: Event → No TextPrice
- Tile 12: Quiz → No TextPrice
- Tile 16: Jail → No TextPrice

**Property Tiles** (28 tiles):
- All others → Need both TextName and TextPrice

---

## 🎯 PART 4: UI PANEL SETUP

### Add ImageBackground to PanelMe:

1. **Hierarchy** → UI → **PanelMe**
2. **Inspector** → **Panel Player Me (Script)**
3. **Add new field**:
   ```
   Panel Player Me (Script)
   └── Image Background: [Drag PanelMe's Image component]
   ```

4. **How to find Image component**:
   - Select PanelMe in Hierarchy
   - Inspector → **Image (Script)** component
   - Drag this to **Image Background** field

---

### Add ImageBackground to PanelPlayerPrefab:

1. **Project** → `Assets/Prefabs/UI/PanelPlayerPrefab.prefab`
2. **Double-click** to open in Prefab mode
3. **Inspector** → **Panel Player (Script)**
4. **Add new field**:
   ```
   Panel Player (Script)
   └── Image Background: [Drag PanelPlayerPrefab's Image component]
   ```
5. **Save Prefab** (Ctrl + S)
6. **Exit Prefab mode**

---

### Verify UI Hierarchy:

```
Canvas
└── UI
    ├── PanelMe (always visible)
    │   ├── Image (background) ← Assign to imageBackground
    │   ├── TextPlayerName (TextMeshPro)
    │   ├── TextMoney (TextMeshPro)
    │   ├── ImageAvatar (Image)
    │   └── ... (other elements)
    │
    ├── PanelBuy (toggle)
    │   ├── ButtonHouse1
    │   ├── ButtonHouse2
    │   ├── ButtonHouse3
    │   ├── ButtonHouse4
    │   ├── ButtonHotel
    │   └── ButtonClose
    │
    ├── PanelSell (toggle)
    ├── PanelQuiz (toggle)
    ├── PanelEvent (toggle)
    ├── PanelFortuneWheel (toggle)
    │
    └── PanelGame (always visible)
        ├── ButtonRollDice
        ├── ButtonMenu
        └── PlayerPanels (container)
            ├── PanelPlayer1 (spawned runtime)
            ├── PanelPlayer2 (spawned runtime)
            └── ... (up to 4 players)
```

---

## 🎯 PART 5: BOARDMANAGER SETUP

### Assign References:

1. **Hierarchy** → **Board** → **BoardManager**
2. **Inspector**:
   ```
   Board Manager (Script)
   ├── Tiles (Array)
   │   ├── Size: 36
   │   ├── Element 0: [Drag Tile_00]
   │   ├── Element 1: [Drag Tile_01]
   │   └── ... (all 36 tiles)
   │
   └── Properties (Array) - Optional
       └── (Can be empty, properties managed by PropertyManager)
   ```

### Easy Way to Assign Tiles:

1. **Select BoardManager** in Hierarchy
2. **Inspector** → Board Manager → **Tiles**
3. **Size**: 36
4. **Select** Tile_00 to Tile_35 in Hierarchy (Ctrl+Click)
5. **Drag** all selected tiles → **Tiles** array
6. **Order**: Should auto-order by name (Tile_00, Tile_01, ..., Tile_35)
7. **Verify**: Element 0 = Tile_00, Element 35 = Tile_35

---

## 🎯 PART 6: TEST DEMO MODE

### Setup Test:

1. **GameManager** → Demo Mode = ✓ **TRUE**
2. **File** → **Save Scene** (Ctrl+S)
3. **Enter Play Mode** (Ctrl+P)

### Expected Result:

1. **Player Spawns**:
   - Male player spawns at Tile 0 (Start)
   - Model visible (male character)
   - No errors in Console

2. **UI Shows**:
   - **PanelMe**: Background color **RED** (Player 0)
   - **PanelMe**: Name "TestPlayer1"
   - **PanelMe**: Money "10000"
   - **PanelGame**: ButtonRollDice visible

3. **Tiles Show**:
   - **Tile 0** (Start): "Start" text, no price
   - **Tile 1** (Tokyo): "Tokyo" text, "$800" price
   - **Tile 2** (Seoul): "Seoul" text, "$800" price
   - ... (all 36 tiles)

4. **Console**:
   - No errors ✅
   - Debug log: "Demo mode: Creating test player"
   - Debug log: "Player spawned: TestPlayer1 at position (x, y, z)"

---

### If Errors Occur:

#### Error: "Player prefabs not assigned"
**Fix**: Assign PlayerMale and PlayerFemale to GameManager

#### Error: "NullReferenceException: boardManager"
**Fix**: Assign BoardManager to GameManager.boardManager field

#### Error: "Tiles array is empty"
**Fix**: Assign all 36 tiles to BoardManager.tiles array

#### Error: "TextMesh not found"
**Fix**: 
- Check tiles have TextMesh child named "TextName"
- Property tiles need TextMesh child named "TextPrice"
- Use TileDataAutoSetup tool to auto-find

#### Error: "Image component not found"
**Fix**: Assign Image component to PanelMe.imageBackground and PanelPlayerPrefab.imageBackground

---

## 🎯 PART 7: TEST MOVEMENT (Optional)

### Manual Test:

1. **Play Mode** → Player spawned at Tile 0
2. **Hierarchy** → Select **PlayerMale** (spawned instance)
3. **Inspector** → Player Game Controller
4. **Add breakpoint** or use Debug:
   ```csharp
   // In GameManager or test script
   PlayerGameController player = players[0];
   player.MoveBySteps(6); // Move 6 tiles
   ```

5. **Expected**:
   - Player moves to Tile 6
   - Smooth animation (bounce, rotation)
   - Walking animation plays
   - No errors

---

## 🎯 PART 8: TEST MULTIPLAYER (Future)

### Local Multiplayer (ParrelSync):

**See**: [PARRELSYNC_TESTING_GUIDE.md](../PARRELSYNC_TESTING_GUIDE.md)

1. **Disable Demo Mode**: GameManager.demoMode = **FALSE**
2. **Open Clone** (ParrelSync)
3. **Host** in main editor
4. **Join** from clone
5. **Test**:
   - 2 players spawn (different genders if selected)
   - Player 0: Red background
   - Player 1: Blue background
   - Both can roll dice
   - Movement synced

---

## 📋 SETUP CHECKLIST

### Player Prefabs:
- [ ] PlayerMale.prefab created
- [ ] PlayerFemale.prefab created
- [ ] Both have NetworkObject + PlayerGameController
- [ ] Both assigned to GameManager

### GameManager:
- [ ] Player prefabs assigned (Male + Female)
- [ ] UI panels assigned (PanelMe, PanelBuy, etc.)
- [ ] BoardManager assigned
- [ ] Board center set (0, 0, 0)
- [ ] Tiles parent assigned
- [ ] Demo mode enabled for testing

### 36 Tiles:
- [ ] All tiles have TileVisual component
- [ ] Tile indices set (0-35)
- [ ] TextMesh components found
- [ ] Property tiles show price (28 tiles)
- [ ] Special tiles hide price (8 tiles)
- [ ] All tiles assigned to BoardManager.tiles array

### UI Panels:
- [ ] PanelMe has imageBackground assigned
- [ ] PanelPlayerPrefab has imageBackground assigned
- [ ] PanelBuy has 5 buttons (House 1-4 + Hotel)
- [ ] All panels in UI hierarchy

### Testing:
- [ ] Demo mode works (player spawns)
- [ ] No errors in Console
- [ ] Tile texts display correctly
- [ ] UI panels show correctly
- [ ] Player color system works

---

## 🎉 COMPLETION

**When all checkboxes ✅**:
- 🎮 GameScene ready for gameplay
- 🔧 All systems connected
- 🐛 No errors
- 🚀 Ready for feature development (Quiz, Event, Fortune Wheel)
- 🎯 Ready for multiplayer testing

---

## 📚 RELATED DOCUMENTS

1. **[PLAYER_PREFAB_SETUP_GUIDE.md](./PLAYER_PREFAB_SETUP_GUIDE.md)** - Player prefab creation
2. **[TILE_SETUP_TEXTMESH_GUIDE.md](./TILE_SETUP_TEXTMESH_GUIDE.md)** - Tile setup with auto-tool
3. **[PLAYERGAMECONTROLLER_REFACTOR_COMPLETE.md](./PLAYERGAMECONTROLLER_REFACTOR_COMPLETE.md)** - Code refactor explanation
4. **[PLAYER_COLOR_IMPLEMENTATION_COMPLETE.md](../PLAYER_COLOR_IMPLEMENTATION_COMPLETE.md)** - Color system details

---

**Estimated Time**: 2-4 hours for complete setup  
**Difficulty**: ⭐⭐⭐ (Medium - mostly drag-and-drop)  
**Status**: Ready to start! 🚀
