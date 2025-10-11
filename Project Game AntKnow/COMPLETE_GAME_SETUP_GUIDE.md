# 🎮 **HƯỚNG DẪN SETUP HOÀN CHỈNH GAME ANTKNOW**

## **📋 TỔNG QUAN**

Hướng dẫn này sẽ giúp bạn setup toàn bộ game AntKnow từ đầu đến cuối, bao gồm tất cả components, scripts, prefabs và UI panels để game có thể chạy được.

---

## **🎯 BƯỚC 1: SETUP SCENE STRUCTURE**

### **1.1 Scene Hierarchy**

```
GameScene
├── GameManager (GameObject)
│   └── GameManager (GameManager Script)
├── BoardManager (GameObject)
│   └── BoardManager (BoardManager Script)
├── PropertyManager (GameObject)
│   ├── PropertyManager (PropertyManager Script)
│   └── PropertyVisual (PropertyVisual Script)
├── TileSetup (GameObject)
│   └── TileSetup (TileSetup Script)
├── Waypoints (Empty GameObject)
│   ├── Waypoint1 (Empty GameObject)
│   ├── Waypoint2 (Empty GameObject)
│   └── ... (36 waypoints total)
├── MapTiles (Empty GameObject)
│   ├── Tile1 (MapTile GameObject)
│   ├── Tile2 (MapTile GameObject)
│   └── ... (36 tiles total)
├── UI (Canvas)
│   ├── PanelGame (GameObject)
│   ├── PanelGameInfo (GameObject)
│   ├── PanelRoll (GameObject)
│   ├── PanelBuy (GameObject)
│   ├── PanelQuiz (GameObject)
│   ├── PanelEvent (GameObject)
│   ├── PanelHouseSell (GameObject)
│   ├── PanelResult (GameObject)
│   ├── PanelNotification (GameObject)
│   └── PanelInfo (GameObject)
├── Services (Empty GameObject)
│   ├── UGSAuthService (UGSAuthService Script)
│   ├── CustomLobbyService (CustomLobbyService Script)
│   ├── MatchmakerService (MatchmakerService Script)
│   └── RelayService (RelayService Script)
└── Prefabs (Empty GameObject - for runtime spawning)
    ├── PlayerPrefab (Prefab)
    ├── HousePrefab (Prefab)
    ├── HotelPrefab (Prefab)
    └── FortuneWheelPrefab (Prefab)
```

---

## **🎯 BƯỚC 2: SETUP CORE MANAGERS**

### **2.1 GameManager Setup**

**GameObject:** GameManager
**Script:** GameManager (NetworkBehaviour)

**Inspector Settings:**
```
Managers:
- Board Manager: [Drag BoardManager]
- Panel Roll: [Drag PanelRoll]
- Property Manager: [Drag PropertyManager]

Players:
- Players: [Empty List]
- Player Prefab: [Drag PlayerPrefab]

UI:
- Roll Button: [Drag Button]
- Turn Text: [Drag TextMeshProUGUI]
- Current Player Text: [Drag TextMeshProUGUI]
- Time Text: [Drag TextMeshProUGUI]

UI Panels:
- Panel Buy: [Drag PanelBuy]
- Panel Quiz: [Drag PanelQuiz]
- Panel Event: [Drag PanelEvent]
- Panel House Sell: [Drag PanelHouseSell]
- Panel Result: [Drag PanelResult]
- Panel Card: [Drag PanelCard]

Game Settings:
- Max Turns: 25
- Demo Mode: false

Services:
- Firebase Auth Service: [Drag FirebaseAuthService]
```

### **2.2 BoardManager Setup**

**GameObject:** BoardManager
**Script:** BoardManager

**Inspector Settings:**
```
Waypoints:
- Waypoints: [Array of 36 waypoint transforms]

Debug:
- Show Debug Info: true
```

### **2.3 PropertyManager Setup**

**GameObject:** PropertyManager
**Script:** PropertyManager

**Inspector Settings:**
```
Visual:
- Property Visual: [Drag PropertyVisual]
- Board Manager: [Drag BoardManager]
```

---

## **🎯 BƯỚC 3: SETUP PROPERTY VISUAL SYSTEM**

### **3.1 PropertyVisual Setup**

**GameObject:** PropertyManager (same as above)
**Script:** PropertyVisual

**Inspector Settings:**
```
Prefabs:
- House Prefab: [Drag HousePrefab]
- Hotel Prefab: [Drag HotelPrefab]

Settings:
- Roof Material Name: "ngói"

Player Colors:
- Color 0: Red (1, 0.2, 0.2)
- Color 1: Blue (0.2, 0.5, 1)
- Color 2: Green (0.2, 1, 0.2)
- Color 3: Yellow (1, 1, 0.2)

Tiles:
- Tile Setup: [Drag TileSetup]
```

### **3.2 TileSetup Setup**

**GameObject:** TileSetup
**Script:** TileSetup

**Inspector Settings:**
```
Settings:
- Auto Setup On Awake: true
- Add Tile Visual Component: true

Info:
- Total Tiles: 36

Debug:
- Show Debug: true
```

---

## **🎯 BƯỚC 4: SETUP MAP TILES & WAYPOINTS**

### **4.1 Waypoints Setup**

1. **Tạo 36 Empty GameObjects** tên "Waypoint1" đến "Waypoint36"
2. **Position** chúng xung quanh board perimeter
3. **Parent** chúng dưới Waypoints GameObject
4. **Assign** vào BoardManager waypoints array

### **4.2 Map Tiles Setup**

**Cho mỗi tile (1-36):**
```
Tile1 (GameObject - Cube)
├── TileVisual (TileVisual Script)
├── Platform (GameObject - Thin Cube)
│   ├── PlatformRenderer (MeshRenderer)
│   └── PlatformCollider (BoxCollider)
├── TileInfo (Empty GameObject)
│   ├── TextName (TextMeshPro - 3D)
│   └── TextPrice (TextMeshPro - 3D)
└── TileCollider (BoxCollider)
```

**TileVisual Inspector:**
```
Tile Structure:
- Platform: [Drag Platform GameObject]
- Text Name: [Drag TextMeshPro for name]
- Text Price: [Drag TextMeshPro for price]

Auto Find:
- Auto Find Children: true

Info:
- Tile Index: 0 (for Tile1), 1 (for Tile2), etc.
```

---

## **🎯 BƯỚC 5: SETUP PREFABS**

### **5.1 PlayerPrefab Setup**

**Prefab Structure:**
```
PlayerPrefab (GameObject)
├── PlayerGameController (PlayerGameController Script)
├── NetworkObject (NetworkObject Component)
├── ModelParent (Empty GameObject)
│   ├── MaleModel (Humanoid Model với Animator)
│   └── FemaleModel (Humanoid Model với Animator)
├── TurnIndicator (Empty GameObject)
│   ├── TurnIndicator (TurnIndicator Script)
│   └── PingSphere (Sphere GameObject)
└── PlayerCollider (CapsuleCollider)
```

**PlayerGameController Inspector:**
```
Network Player Info:
- Network Player Name: "Player"
- Network Player ID: ""
- Network Is Male: true

Network Game State:
- Network Current Tile: 0
- Network Money: 1000
- Network Jail Counter: 0
- Network Skip Next Turn: false

Movement:
- Move Speed: 5
- Bounce Height: 0.5
- Bounce Duration: 0.3
- Board Manager: [Drag BoardManager]
- Board Center: (0, 0, 0)

Player Models:
- Male Model: [Drag male humanoid model]
- Female Model: [Drag female humanoid model]
- Model Parent: [Drag ModelParent GameObject]

Animation:
- Animator: [Auto-assigned from active model]

Turn Indicator:
- Turn Indicator: [Drag TurnIndicator GameObject]
```

### **5.2 HousePrefab Setup**

**Prefab Structure:**
```
HousePrefab (GameObject)
├── HouseModel (GameObject với MeshRenderer)
│   ├── HouseMesh (MeshFilter + MeshRenderer)
│   ├── RoofMaterial (Material với tên "ngói")
│   └── HouseCollider (BoxCollider)
└── HouseEffects (Empty GameObject)
    ├── PurchaseParticles (ParticleSystem)
    └── PurchaseSound (AudioSource)
```

### **5.3 HotelPrefab Setup**

**Prefab Structure:**
```
HotelPrefab (GameObject)
├── HotelModel (GameObject với MeshRenderer)
│   ├── HotelMesh (MeshFilter + MeshRenderer)
│   ├── RoofMaterial (Material với tên "ngói")
│   └── HotelCollider (BoxCollider)
└── HotelEffects (Empty GameObject)
    ├── UpgradeParticles (ParticleSystem)
    └── UpgradeSound (AudioSource)
```

---

## **🎯 BƯỚC 6: SETUP UI PANELS**

### **6.1 PanelGame Setup**

**GameObject:** PanelGame
**Script:** PanelGame

**Inspector Settings:**
```
Panel Components:
- Panel Me: [Drag PanelPlayerMe]
- Panel Player Container: [Drag VerticalLayoutGroup parent]
- Panel Player Prefab: [Drag PanelPlayerPrefab]

Settings:
- Max Players: 4
```

### **6.2 PanelRoll Setup**

**GameObject:** PanelRoll
**Script:** PanelRoll

**Inspector Settings:**
```
Dice Components:
- Dice 1 Image: [Drag Image for dice 1]
- Dice 2 Image: [Drag Image for dice 2]
- Dice Sprites: [Array of 6 dice sprites 1-6]

Result Display:
- Text Result: [Drag TextMeshProUGUI]

Roll Button:
- Btn Roll: [Drag Button]

Animation Settings:
- Roll Duration: 1.5
- Frame Interval: 0.1
```

### **6.3 PanelBuy Setup**

**GameObject:** PanelBuy
**Script:** PanelBuy

**Inspector Settings:**
```
UI Components:
- Text Property Name: [Drag TextMeshProUGUI]
- Text Price: [Drag TextMeshProUGUI]
- Btn Buy: [Drag Button]
- Btn Skip: [Drag Button]

House Buttons:
- Btn House 1: [Drag Button]
- Btn House 2: [Drag Button]
- Btn House 3: [Drag Button]
- Btn House 4: [Drag Button]
- Btn Hotel: [Drag Button]

Colors:
- Normal Color: White
- Selected Color: Green
- Cannot Afford Color: Red
```

### **6.4 PanelQuiz Setup**

**GameObject:** PanelQuiz
**Script:** PanelQuiz

**Inspector Settings:**
```
UI Components:
- Text Question: [Drag TextMeshProUGUI]
- Text Difficulty: [Drag TextMeshProUGUI]
- Btn Answer 1: [Drag Button]
- Btn Answer 2: [Drag Button]
- Btn Answer 3: [Drag Button]
- Btn Answer 4: [Drag Button]
- Text Timer: [Drag TextMeshProUGUI]

Fortune Wheel:
- Fortune Wheel Object: [Drag FortuneWheel GameObject]
- Text Wheel Result: [Drag TextMeshProUGUI]

Settings:
- Answer Time: 15
- Is Annual Quiz: false
```

### **6.5 Other Panels Setup**

**PanelEvent, PanelHouseSell, PanelResult, PanelNotification, PanelInfo:**
- Setup tương tự với các UI components cần thiết
- Assign các Button, TextMeshProUGUI, Image components
- Setup callbacks và event handlers

---

## **🎯 BƯỚC 7: SETUP SERVICES**

### **7.1 UGSAuthService Setup**

**GameObject:** Services
**Script:** UGSAuthService

**Inspector Settings:**
```
Debug:
- Enable Debug Logs: true
```

### **7.2 Other Services Setup**

**CustomLobbyService, MatchmakerService, RelayService:**
- Setup tương tự với debug logs enabled
- Assign các references cần thiết

---

## **🎯 BƯỚC 8: SETUP MATERIALS**

### **8.1 House/Hotel Materials**

1. **Tạo Material** tên "ngói":
   - Base Color: White (1, 1, 1, 1)
   - Metallic: 0
   - Smoothness: 0.5

2. **Tạo Player Color Materials**:
   - Player1_Material: Red (1, 0.2, 0.2, 1)
   - Player2_Material: Blue (0.2, 0.5, 1, 1)
   - Player3_Material: Green (0.2, 1, 0.2, 1)
   - Player4_Material: Yellow (1, 1, 0.2, 1)

### **8.2 UI Materials**

- Setup materials cho buttons, panels, text
- Ensure proper sorting layers và rendering order

---

## **🎯 BƯỚC 9: FINAL INTEGRATION**

### **9.1 Connect All References**

1. **GameManager** → Connect tất cả manager references
2. **BoardManager** → Connect waypoints array
3. **PropertyManager** → Connect PropertyVisual và BoardManager
4. **All UI Panels** → Connect button handlers và callbacks
5. **All Services** → Connect authentication và networking

### **9.2 Test Scene Setup**

1. **Build Settings** → Add GameScene to build
2. **Player Settings** → Setup company name, product name
3. **Quality Settings** → Adjust for performance
4. **Input Manager** → Ensure proper input handling

---

## **🎯 BƯỚC 10: TESTING & VALIDATION**

### **10.1 Compile Check**

- [ ] No compile errors
- [ ] All scripts compile successfully
- [ ] All references assigned
- [ ] No missing components

### **10.2 Runtime Check**

- [ ] GameManager starts correctly
- [ ] BoardManager finds waypoints
- [ ] PropertyManager initializes
- [ ] UI panels respond to input
- [ ] Player spawning works
- [ ] Movement system works
- [ ] Property system works

### **10.3 Multiplayer Check**

- [ ] NetworkObject components present
- [ ] ServerRpc/ClientRpc methods work
- [ ] NetworkVariable synchronization works
- [ ] Player sync across clients
- [ ] Game state synchronization

---

## **✅ HOÀN THÀNH SETUP**

Sau khi hoàn thành tất cả các bước trên, bạn sẽ có:

- ✅ Complete Game Scene với tất cả components
- ✅ Property Visual System hoàn chỉnh
- ✅ Player System với multiplayer support
- ✅ UI System với tất cả panels
- ✅ Network Services integration
- ✅ Material và Prefab system
- ✅ Testing và validation complete

**Game AntKnow sẵn sàng để build và chạy!** 🎮✨

---

## **🚀 NEXT STEPS**

1. **Build Game** → Test on multiple platforms
2. **Deploy Services** → Setup Firebase và UGS services
3. **Performance Optimization** → LOD, batching, pooling
4. **Polish** → Effects, sounds, animations
5. **Final Testing** → Multiplayer testing với real users

**Chúc bạn thành công với project AntKnow!** 🎉
