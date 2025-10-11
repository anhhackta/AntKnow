# 🎮 **HƯỚNG DẪN SETUP GAME VISUAL & GAMEPLAY CHI TIẾT**

## **📋 TỔNG QUAN**

Hướng dẫn này sẽ giúp bạn setup toàn bộ visual và gameplay cho game AntKnow, bao gồm:
- Player models (girl/main dựa trên gender)
- Turn indicators (sphere trên đầu)
- Map tiles với house/hotel placement
- Bounce movement effect
- Material system cho houses
- Prefabs và components

---

## **🎯 BƯỚC 1: SETUP PLAYER PREFAB**

### **1.1 Tạo Player Prefab Structure**

```
PlayerPrefab (GameObject)
├── PlayerGameController (Script)
├── ModelParent (Empty GameObject)
│   ├── MaleModel (Humanoid Model với Animator)
│   └── FemaleModel (Humanoid Model với Animator)
├── TurnIndicator (Empty GameObject)
│   └── TurnIndicator (Script)
└── NetworkObject (Network Component)
```

### **1.2 Setup PlayerGameController Component**

**Inspector Settings:**
```
Player Info:
- Player Name: "Player"
- Player ID: ""
- Is Male: true

Game State:
- Current Tile: 0
- Money: 1000
- Jail Counter: 0
- Skip Next Turn: false

Stats from Loadout:
- Health: 0
- Agility: 0
- Intelligence: 0
- Luck: 0
- Resistance: 0

Movement:
- Move Speed: 5
- Bounce Height: 0.5
- Bounce Duration: 0.3
- Board Manager: [Drag BoardManager từ scene]
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

### **1.3 Setup Humanoid Models**

**Male Model Setup:**
1. Import male humanoid model với idle animation
2. Đặt vào `ModelParent/MaleModel`
3. Thêm Animator component
4. Setup Animation Controller với:
   - Idle state (default)
   - Walk/Run states (nếu có)
   - Transition conditions

**Female Model Setup:**
1. Import female humanoid model với idle animation
2. Đặt vào `ModelParent/FemaleModel`
3. Thêm Animator component
4. Setup Animation Controller tương tự

---

## **🎯 BƯỚC 2: SETUP TURN INDICATOR**

### **2.1 TurnIndicator Component**

**Inspector Settings:**
```
Settings:
- Ping Object: [Auto-created sphere]
- Bob Speed: 2
- Bob Height: 0.3
- Offset: (0, 2.5, 0)
```

**Auto-created Sphere:**
- Primitive Type: Sphere
- Scale: (0.3, 0.3, 0.3)
- Material: Yellow color
- No Collider
- Parent: TurnIndicator GameObject

---

## **🎯 BƯỚC 3: SETUP MAP TILES**

### **3.1 Tile Structure**

```
MapTile (GameObject - Cube)
├── TileVisual (Script)
├── Platform (GameObject - Thin Cube)
│   └── [Houses/Hotels will be spawned here]
├── TextName (TextMeshPro)
├── TextPrice (TextMeshPro)
└── [House/Hotel Prefabs] (Spawned at runtime)
```

### **3.2 Platform Placement Rules**

**Platform GameObject:**
- **Position**: On top of main cube
- **Scale**: (0.8, 0.1, 0.8) - Thin platform
- **Tag**: "Platform"
- **Material**: Default platform material

**House Placement Directions:**
- **Z-axis**: Towards center of map (inward)
- **Y-axis**: Upward (stacking)
- **X-axis**: Left side (side-by-side)

**House Spacing:**
- Houses placed adjacent to each other on platform
- Max 4 houses before hotel
- Hotel replaces all 4 houses

### **3.3 TileVisual Component Setup**

**Inspector Settings:**
```
Tile Structure:
- Platform: [Drag Platform GameObject]
- Text Name: [Drag TextMeshPro for name]
- Text Price: [Drag TextMeshPro for price]

Auto Find:
- Auto Find Children: true

Info:
- Tile Index: [Set manually: 0-35]
```

---

## **🎯 BƯỚC 4: SETUP HOUSE/HOTEL PREFABS**

### **4.1 House Prefab Structure**

```
HousePrefab (GameObject)
├── MeshRenderer
├── MeshFilter
├── Material (with roof material named "ngói")
└── [Optional: Particle effects, lights]
```

### **4.2 Hotel Prefab Structure**

```
HotelPrefab (GameObject)
├── MeshRenderer
├── MeshFilter
├── Material (with roof material named "ngói")
└── [Optional: Particle effects, lights]
```

### **4.3 Material Setup**

**House/Hotel Materials:**
1. Create materials với tên "ngói" (roof material)
2. Setup material properties:
   - Base Color: White (default)
   - Metallic: 0
   - Smoothness: 0.5
   - Normal Map: [Optional]

**Color System:**
- Player 1: Red (1, 0.2, 0.2)
- Player 2: Blue (0.2, 0.5, 1)
- Player 3: Green (0.2, 1, 0.2)
- Player 4: Yellow (1, 1, 0.2)

---

## **🎯 BƯỚC 5: SETUP BOUNCE MOVEMENT**

### **5.1 Movement Animation**

**Bounce Effect Parameters:**
- **Bounce Height**: 0.5 units
- **Bounce Duration**: 0.3 seconds
- **Direction**: Towards board center
- **Curve**: Smooth sine wave

**Implementation:**
- Player moves along waypoint path
- Each step has bounce animation
- Player faces center during movement
- Smooth transition between waypoints

### **5.2 BoardManager Setup**

**Inspector Settings:**
```
Waypoints:
- Waypoints: [Array of 36 waypoint transforms]

Debug:
- Show Debug Info: true
```

**Waypoint Setup:**
1. Create 36 empty GameObjects named "Waypoint1" to "Waypoint36"
2. Position them around the board perimeter
3. Assign to BoardManager waypoints array
4. BoardManager will auto-find if not assigned

---

## **🎯 BƯỚC 6: SETUP PROPERTY VISUAL SYSTEM**

### **6.1 PropertyVisual Component**

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
- Tile Setup: [Drag TileSetup GameObject]
```

### **6.2 TileSetup Component**

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

## **🎯 BƯỚC 7: SETUP GAME MANAGER INTEGRATION**

### **7.1 GameManager References**

**Inspector Settings:**
```
Managers:
- Board Manager: [Drag BoardManager]
- Dice Controller: [Drag DiceController]
- Property Manager: [Drag PropertyManager]

Players:
- Player Prefab: [Drag PlayerPrefab]

UI Panels:
- Panel Game: [Drag PanelGame]
- Panel Game Info: [Drag PanelGameInfo]
- Panel Roll: [Drag PanelRoll]
- Panel Buy: [Drag PanelBuy]
- Panel Quiz: [Drag PanelQuiz]
- Panel Event: [Drag PanelEvent]
- Panel House Sell: [Drag PanelHouseSell]
- Panel Result: [Drag PanelResult]
- Panel Notification: [Drag PanelNotification]

Services:
- Firebase Auth Service: [Drag FirebaseAuthService]
```

---

## **🎯 BƯỚC 8: SETUP UI PANELS**

### **8.1 PanelGame Setup**

**Inspector Settings:**
```
Panel Components:
- Panel Me: [Drag PanelPlayerMe]
- Panel Player Container: [Drag VerticalLayoutGroup parent]
- Panel Player Prefab: [Drag PanelPlayerPrefab]

Settings:
- Max Players: 4
```

### **8.2 PanelRoll Setup**

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

---

## **🎯 BƯỚC 9: TESTING & VALIDATION**

### **9.1 Test Checklist**

**Player Setup:**
- [ ] Male model shows for male players
- [ ] Female model shows for female players
- [ ] Turn indicator appears on current player
- [ ] Bounce movement works correctly

**Map Setup:**
- [ ] 36 waypoints positioned correctly
- [ ] Houses spawn on platform
- [ ] House colors change based on owner
- [ ] Hotel replaces 4 houses

**UI Integration:**
- [ ] All panels respond correctly
- [ ] Turn indicators work
- [ ] Dice animation plays
- [ ] Property interactions work

### **9.2 Common Issues & Solutions**

**Player Model Issues:**
- **Problem**: Model not showing
- **Solution**: Check ModelParent assignment and model prefabs

**Turn Indicator Issues:**
- **Problem**: Sphere not appearing
- **Solution**: Check TurnIndicator script assignment and ping object

**House Placement Issues:**
- **Problem**: Houses not spawning correctly
- **Solution**: Check Platform GameObject and TileVisual setup

**Movement Issues:**
- **Problem**: Player not moving smoothly
- **Solution**: Check waypoint positions and BoardManager setup

---

## **🎯 BƯỚC 10: FINAL INTEGRATION**

### **10.1 Scene Setup Order**

1. **Setup Map Tiles** (36 tiles with platforms)
2. **Setup Waypoints** (36 waypoints around board)
3. **Setup BoardManager** (assign waypoints)
4. **Setup PropertyVisual** (assign house/hotel prefabs)
5. **Create Player Prefab** (with both models)
6. **Setup UI Panels** (all panels in scene)
7. **Setup GameManager** (assign all references)
8. **Test Multiplayer** (host/client functionality)

### **10.2 Performance Optimization**

**LOD System:**
- Create LOD groups for houses/hotels
- Use simple meshes for distant objects

**Animation Optimization:**
- Use animation compression
- Limit simultaneous animations

**Material Optimization:**
- Use shared materials for same-colored objects
- Batch similar materials together

---

## **✅ HOÀN THÀNH SETUP**

Sau khi hoàn thành tất cả các bước trên, bạn sẽ có:

- ✅ Player models với gender selection
- ✅ Turn indicators với bounce animation
- ✅ Map tiles với house/hotel placement
- ✅ Bounce movement system
- ✅ Property visual system với colors
- ✅ Complete UI panel integration
- ✅ Multiplayer-ready game setup

**Game sẵn sàng để test và chơi!** 🎉
