# 🏠 **HƯỚNG DẪN SETUP PROPERTY VISUAL SYSTEM**

## **📋 TỔNG QUAN**

Property Visual System quản lý việc spawn, hiển thị và đổi màu houses/hotels trên map tiles. System này tích hợp với PropertyManager để sync data.

---

## **🎯 BƯỚC 1: TẠO HOUSE/HOTEL PREFABS**

### **1.1 House Prefab Structure**

```
HousePrefab (GameObject)
├── HouseModel (GameObject với MeshRenderer)
│   ├── HouseMesh (MeshFilter + MeshRenderer)
│   ├── RoofMaterial (Material với tên "ngói")
│   └── HouseCollider (BoxCollider)
├── HouseEffects (Empty GameObject)
│   ├── PurchaseParticles (ParticleSystem)
│   └── PurchaseSound (AudioSource)
└── HouseController (Script - Optional)
```

### **1.2 Hotel Prefab Structure**

```
HotelPrefab (GameObject)
├── HotelModel (GameObject với MeshRenderer)
│   ├── HotelMesh (MeshFilter + MeshRenderer)
│   ├── RoofMaterial (Material với tên "ngói")
│   └── HotelCollider (BoxCollider)
├── HotelEffects (Empty GameObject)
│   ├── UpgradeParticles (ParticleSystem)
│   └── UpgradeSound (AudioSource)
└── HotelController (Script - Optional)
```

### **1.3 Material Setup**

**Tạo Materials:**
1. **Roof Material** tên "ngói":
   - Base Color: White (1, 1, 1, 1)
   - Metallic: 0
   - Smoothness: 0.5
   - Normal Map: [Optional roof texture]

2. **Player Color Materials**:
   - Player1_Material: Red (1, 0.2, 0.2, 1)
   - Player2_Material: Blue (0.2, 0.5, 1, 1)
   - Player3_Material: Green (0.2, 1, 0.2, 1)
   - Player4_Material: Yellow (1, 1, 0.2, 1)

---

## **🎯 BƯỚC 2: SETUP MAP TILES**

### **2.1 Tile Structure**

```
MapTile (GameObject - Cube)
├── TileVisual (TileVisual Script)
├── Platform (GameObject - Thin Cube)
│   ├── PlatformRenderer (MeshRenderer)
│   ├── PlatformCollider (BoxCollider)
│   └── [Houses/Hotels spawn here]
├── TileInfo (Empty GameObject)
│   ├── TextName (TextMeshPro - 3D)
│   └── TextPrice (TextMeshPro - 3D)
└── TileCollider (BoxCollider - Main tile)
```

### **2.2 Platform Setup**

**Platform GameObject:**
- **Position**: (0, 0.5, 0) - On top of main cube
- **Scale**: (0.8, 0.1, 0.8) - Thin platform
- **Material**: Default platform material
- **Tag**: "Platform"

### **2.3 TileVisual Script Setup**

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

## **🎯 BƯỚC 3: SETUP PROPERTY VISUAL MANAGER**

### **3.1 PropertyVisual Script Setup**

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

### **3.2 TileSetup Script Setup**

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

## **🎯 BƯỚC 4: SETUP BOARD MANAGER**

### **4.1 BoardManager Integration**

**Inspector Settings:**
```
Waypoints:
- Waypoints: [Array of 36 waypoint transforms]

Debug:
- Show Debug Info: true
```

**Waypoint Setup:**
1. **Tạo 36 Empty GameObjects** tên "Waypoint1" đến "Waypoint36"
2. **Position** chúng xung quanh board perimeter
3. **Assign** vào BoardManager waypoints array

### **4.2 Auto-Find Waypoints**

BoardManager sẽ tự động tìm waypoints nếu không assign:
- Tìm objects có tên chứa "Waypoint" hoặc "Tile"
- Tự động assign vào waypoints array

---

## **🎯 BƯỚC 5: SETUP GAME MANAGER INTEGRATION**

### **5.1 GameManager References**

**Inspector Settings:**
```
Managers:
- Board Manager: [Drag BoardManager]
- Property Manager: [Drag PropertyManager]

Players:
- Player Prefab: [Drag PlayerPrefab]

Services:
- Firebase Auth Service: [Drag FirebaseAuthService]
```

### **5.2 PropertyManager Integration**

**Inspector Settings:**
```
Visual:
- Property Visual: [Drag PropertyVisual]
- Board Manager: [Drag BoardManager]
```

---

## **🎯 BƯỚC 6: SETUP PLAYER PREFAB**

### **6.1 Player Prefab Structure**

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

### **6.2 PlayerGameController Setup**

**Inspector Settings:**
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

---

## **🎯 BƯỚC 7: SETUP UI PANELS**

### **7.1 PanelGame Setup**

**Inspector Settings:**
```
Panel Components:
- Panel Me: [Drag PanelPlayerMe]
- Panel Player Container: [Drag VerticalLayoutGroup parent]
- Panel Player Prefab: [Drag PanelPlayerPrefab]

Settings:
- Max Players: 4
```

### **7.2 PanelRoll Setup**

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

### **7.3 PanelBuy Setup**

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

---

## **🎯 BƯỚC 8: TESTING & VALIDATION**

### **8.1 Test Checklist**

**Property System:**
- [ ] Houses spawn correctly on platform
- [ ] House colors change based on owner
- [ ] Hotel replaces 4 houses
- [ ] PropertyManager syncs with PropertyVisual

**Player System:**
- [ ] Male model shows for male players
- [ ] Female model shows for female players
- [ ] Turn indicator appears on current player
- [ ] Bounce movement works correctly

**Map System:**
- [ ] 36 waypoints positioned correctly
- [ ] BoardManager finds waypoints automatically
- [ ] TileVisual shows correct info

**UI System:**
- [ ] All panels respond correctly
- [ ] Turn indicators work
- [ ] Dice animation plays
- [ ] Property interactions work

### **8.2 Common Issues & Solutions**

**Houses không spawn:**
- Check Platform GameObject assignment
- Verify TileVisual setup
- Check PropertyVisual prefab assignments

**House colors không đổi:**
- Check material setup với tên "ngói"
- Verify player color array
- Check SetHouseColor method

**Player models không hiện:**
- Check ModelParent assignment
- Verify male/female model prefabs
- Check SetupPlayerModel method

**Turn indicators không hiện:**
- Check TurnIndicator script assignment
- Verify ping object setup
- Check Show/Hide methods

---

## **✅ HOÀN THÀNH SETUP**

Sau khi hoàn thành tất cả các bước trên, bạn sẽ có:

- ✅ Complete Property Visual System
- ✅ House/Hotel spawning với colors
- ✅ Player models với gender selection
- ✅ Turn indicators với animation
- ✅ Map tiles với platforms
- ✅ Complete UI panel integration
- ✅ Multiplayer-ready game setup

**Game sẵn sàng để test và chơi!** 🎮✨
