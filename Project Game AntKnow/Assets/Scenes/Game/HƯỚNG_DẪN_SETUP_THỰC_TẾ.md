# 🎮 HƯỚNG DẪN SETUP THỰC TẾ - GAME MULTIPLAYER

## 📊 HIỆN TRẠNG THỰC TẾ CỦA BẠN

### ✅ **ĐÃ CÓ SẴN:**
1. **Map + Tiles + Waypoints** - Đã có trong scene
2. **36 tiles** theo MAP_36_TILES.md
3. **Code hoàn chỉnh** - PlayerGameController, GameManager, etc.
4. **2 models** (male/female) - Chỉ có animation idle

### ⏭️ **CẦN LÀM:**
1. **Setup UI Panels** - Chưa có trong scene
2. **Tạo Player Prefabs** - Chưa có prefabs
3. **Kết nối GameManager** - Assign references
4. **Network setup** - Multiplayer configuration

---

## 🎯 PHẦN 1: HIỂU RÕ PLAYER PREFAB SYSTEM

### **Cách hoạt động:**

```
GameManager có 2 prefabs:
├── PlayerMale.prefab (isMale = TRUE)
│   └── MaleModel (child) - Animator với idle animation
└── PlayerFemale.prefab (isMale = FALSE)
    └── FemaleModel (child) - Animator với idle animation
```

**Khi spawn player:**
```csharp
// GameManager.cs line 307
GameObject prefabToUse = isMale ? playerPrefabMale : playerPrefabFemale;
GameObject playerObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
```

**Không cần toggle model!** Mỗi prefab đã có model riêng sẵn.

---

## 🛠️ PHẦN 2: TẠO PLAYER PREFABS (10 PHÚT)

### **Bước 1: Tạo PlayerMale.prefab**

1. **Hierarchy → Create Empty → "PlayerMale"**

2. **Add Components:**
   - Add Component → **Network Object**
     - Is Player Object: ✓ **TRUE**
     - Owner Permission: **Owner**
   
   - Add Component → **Player Game Controller**
     - Is Male: ✓ **TRUE**
     - Money: **10000**
     - Move Speed: **5**
     - Bounce Height: **0.5**
     - Bounce Duration: **0.3**

3. **Import Male Model:**
   - Drag male 3D model vào làm **child** của PlayerMale
   - Rename child: **"MaleModel"**
   - Position: **(0, 0, 0)**
   - Ensure model có **Animator** component

4. **Assign Animator:**
   - Select PlayerMale (root)
   - Inspector → Player Game Controller
   - Animator field: **Drag MaleModel/Animator vào đây**

5. **Save Prefab:**
   - Drag PlayerMale từ Hierarchy → Project window
   - Save vào folder: **Assets/Prefabs/Players/**
   - **Delete PlayerMale từ Hierarchy**

---

### **Bước 2: Tạo PlayerFemale.prefab**

**Option A: Duplicate (Nhanh nhất):**

1. **Project window** → Right-click **PlayerMale.prefab** → Duplicate
2. **Rename** → **PlayerFemale**
3. **Double-click** PlayerFemale.prefab để mở Prefab mode
4. **Select root** (PlayerFemale)
5. **Inspector → Player Game Controller:**
   - Is Male: ✗ **FALSE** ← ⭐ QUAN TRỌNG!
6. **Delete child "MaleModel"**
7. **Drag female 3D model** vào làm child
8. **Rename child** → **"FemaleModel"**
9. **Assign Animator:**
   - Drag FemaleModel/Animator → Animator field
10. **Save Prefab** (Ctrl+S)
11. **Exit Prefab mode**

**Option B: Tạo từ đầu:**
- Repeat Bước 1 nhưng với female model và `Is Male = FALSE`

---

### **Lưu ý về Animation:**

```
Cả 2 models dùng chung Animator Controller:
- Idle animation: Luôn chạy (default state)
- Không cần walk animation (vì chỉ có idle)
- Animator Controller đơn giản:
  └── Idle (default state)
```

**Nếu muốn thêm walk animation sau:**
```csharp
// PlayerGameController.cs line 152-160
private void SetAnimation(bool isMoving)
{
    if (animator != null)
    {
        animator.SetBool("isRunning", isMoving);
    }
}
```

---

## 🎨 PHẦN 3: SETUP UI PANELS (20 PHÚT)

### **Hiện trạng:** Bạn chưa có UI panels trong scene

### **Cần tạo 10 panels:**

#### **A. Persistent Panels (Luôn hiện):**

1. **PanelGame** - Hiển thị danh sách players
2. **PanelGameInfo** - Turn/Time/CurrentPlayer
3. **PanelRoll** - Dice + Roll button

#### **B. Conditional Panels (Kích hoạt khi cần):**

4. **PanelInfo** - Player info popup
5. **PanelBuy** - Buy/upgrade property
6. **PanelQuiz** - Quiz questions
7. **PanelEvent** - Event cards
8. **PanelHouseSell** - Sell properties
9. **PanelResult** - Game end results
10. **PanelNotification** - Quick notifications

---

### **Cách tạo nhanh:**

#### **Option 1: Tự động (Dùng Editor Tool)** ⭐ RECOMMENDED

1. **Unity Menu → AntKnow → UI Panel Setup Helper**
2. **Click "Create ALL Panels"**
3. **Done!** - 10 panels tự động tạo

#### **Option 2: Thủ công (Chi tiết)**

Xem file **UNITY_SCENE_SETUP_STEP_BY_STEP.md** section 2 & 3

---

### **Cấu trúc UI cần có:**

```
Canvas (Screen Space Overlay)
├── PanelGame (Top-Left)
│   ├── PanelMe (Button + PanelPlayerMe.cs)
│   │   ├── TextName
│   │   └── TextMoney
│   └── PanelPlayerContainer (VerticalLayoutGroup)
│       └── (PanelPlayerPrefab spawns here runtime)
│
├── PanelGameInfo (Top-Center)
│   ├── TextTurn ("Turn: 1/25")
│   ├── TextTime ("Time: 00:00")
│   └── TextCurrentPlayer ("Current: Player 1")
│
├── PanelRoll (Bottom-Right)
│   ├── Dice1 (Image)
│   ├── Dice2 (Image)
│   ├── TextResult ("Result: 7")
│   └── BtnRoll (Button)
│
├── PanelInfo (INACTIVE - Center)
│   ├── ImageGender
│   ├── TextPlayerName
│   ├── TextMatchesPlayed
│   ├── TextMatchesWon
│   └── BtnClose
│
├── PanelBuy (INACTIVE - Center)
│   ├── TextPropertyName
│   ├── TextPrice
│   ├── HouseSelection (4 toggles)
│   ├── BtnBuy
│   └── BtnSkip
│
├── PanelQuiz (INACTIVE - Center)
│   ├── TextQuestion
│   ├── TextDifficulty
│   ├── TextTimer
│   ├── BtnAnswer1-4
│   └── FortuneWheelObject (INACTIVE)
│
├── PanelEvent (INACTIVE - Center)
│   ├── TextEvent
│   └── BtnOK
│
├── PanelHouseSell (INACTIVE - Center)
│   ├── ScrollView
│   └── BtnSell
│
├── PanelResult (INACTIVE - Center)
│   ├── TextTitle
│   ├── ResultContainer
│   └── BtnExit
│
└── PanelNotification (INACTIVE - Top-Center)
    └── TextNotification
```

---

## 🔧 PHẦN 4: SETUP GAMEMANAGER (5 PHÚT)

### **Bước 1: Tạo GameManager GameObject**

1. **Hierarchy → Create Empty → "GameManager"**

2. **Add Components:**
   - Add Component → **Network Object**
     - Is Player Object: **FALSE**
   - Add Component → **Game Manager**

---

### **Bước 2: Assign References trong Inspector**

#### **Player Prefabs:**
```
GameManager (Script)
└── Player Prefabs
    ├── Player Prefab Male: [Drag PlayerMale.prefab]
    └── Player Prefab Female: [Drag PlayerFemale.prefab]
```

#### **UI:**
```
UI
├── Roll Button: [Drag Canvas/PanelRoll/BtnRoll]
├── Turn Text: [Drag Canvas/PanelGameInfo/TextTurn]
├── Current Player Text: [Drag Canvas/PanelGameInfo/TextCurrentPlayer]
└── Time Text: [Drag Canvas/PanelGameInfo/TextTime]
```

#### **UI Panels:**
```
UI Panels
├── Panel Buy: [Drag Canvas/PanelBuy]
├── Panel Quiz: [Drag Canvas/PanelQuiz]
├── Panel Event: [Drag Canvas/PanelEvent]
├── Panel House Sell: [Drag Canvas/PanelHouseSell]
├── Panel Result: [Drag Canvas/PanelResult]
└── Panel Card: [Drag Canvas/PanelCard if exists]
```

#### **Game Settings:**
```
Game Settings
├── Max Turns: 25
└── Demo Mode: ✓ TRUE (for testing)
```

---

## 🗺️ PHẦN 5: VERIFY MAP SETUP (2 PHÚT)

### **Kiểm tra Map đã có:**

```
Scene
├── Map (hoặc Tiles)
│   ├── Tile0 (Start)
│   ├── Tile1 (Tokyo)
│   ├── ...
│   └── Tile35 (Da Nang)
│
└── Waypoints
    ├── Waypoint0
    ├── Waypoint1
    ├── ...
    └── Waypoint35
```

### **Kiểm tra Tile Structure:**

**Property Tiles (ví dụ Tile1):**
```
Tile1 (Cube - GameObject chính)
└── Platform (Cube child - để spawn houses)
    ├── TextName (TextMesh: "Tokyo")
    └── TextPrice (TextMesh: "800")
```

**Special Tiles (Tile0, 10, 19, 28):**
```
Tile0 (Cube)
└── Text (TextMesh: "Ô Bắt Đầu")
```

---

## 🎮 PHẦN 6: SETUP BOARDMANAGER & PROPERTYMANAGER (3 PHÚT)

### **BoardManager:**

1. **Hierarchy → Create Empty → "BoardManager"**
2. **Add Component → Board Manager**
3. **Inspector:**
   - Map Parent: [Drag "Map" hoặc "Tiles" GameObject]
   - Waypoint Parent: [Drag "Waypoints" GameObject]

### **PropertyManager:**

1. **Hierarchy → Create Empty → "PropertyManager"**
2. **Add Component → Property Manager**

3. **Tạo PropertyVisual:**
   - Hierarchy → Create Empty → "PropertyVisual"
   - Add Component → **Property Visual**
   - Inspector:
     - House Prefab: [Assign house prefab nếu có]
     - Hotel Prefab: [Assign hotel prefab nếu có]
     - Roof Material Name: **"ngói"**

4. **Assign PropertyManager Inspector:**
   - Property Visual: [Drag PropertyVisual GameObject]
   - Board Manager: [Drag BoardManager GameObject]

5. **Assign GameManager:**
   - Select GameManager
   - Inspector → Game Manager
   - Board Manager: [Drag BoardManager]
   - Property Manager: [Drag PropertyManager]

---

## ✅ PHẦN 7: FINAL CHECKLIST

### **Scene Setup:**
- [ ] PlayerMale.prefab created (isMale = TRUE)
- [ ] PlayerFemale.prefab created (isMale = FALSE)
- [ ] Canvas + 10 UI panels created
- [ ] GameManager created với NetworkObject
- [ ] BoardManager created
- [ ] PropertyManager created
- [ ] PropertyVisual created

### **References Assigned:**
- [ ] GameManager → PlayerMale.prefab
- [ ] GameManager → PlayerFemale.prefab
- [ ] GameManager → UI components (buttons, texts)
- [ ] GameManager → UI Panels
- [ ] GameManager → BoardManager
- [ ] GameManager → PropertyManager
- [ ] BoardManager → Map parent
- [ ] BoardManager → Waypoints parent
- [ ] PropertyManager → PropertyVisual
- [ ] PropertyManager → BoardManager

### **Settings:**
- [ ] GameManager → Max Turns = 25
- [ ] GameManager → Demo Mode = TRUE
- [ ] NetworkObject → Is Player Object = FALSE (GameManager)
- [ ] NetworkObject → Is Player Object = TRUE (Player prefabs)

---

## 🧪 PHẦN 8: TEST DEMO MODE (2 PHÚT)

### **Bước 1: Verify Setup**

1. **Check Console** - Không có errors
2. **Check GameManager Inspector** - Tất cả references assigned
3. **Check Player Prefabs** - Animator assigned

### **Bước 2: Play Test**

1. **Click Play**
2. **Expected:**
   - ✅ 1 player spawns tại Tile0 (Waypoint0)
   - ✅ PanelGame hiển thị player info
   - ✅ PanelGameInfo hiển thị "Turn: 1/25"
   - ✅ PanelRoll hiển thị dice + button
   - ✅ Model hiển thị (male hoặc female)
   - ✅ Idle animation chạy
   - ✅ No errors in Console

3. **Test Roll Dice:**
   - Click "ROLL DICE" button
   - Expected:
     - ✅ Dice animation
     - ✅ Player moves với bounce effect
     - ✅ Player nhìn vào center khi di chuyển
     - ✅ Turn indicator (sphere) hiện trên đầu player

---

## 🐛 TROUBLESHOOTING

### **Lỗi: "Player prefabs not assigned"**
**Fix:** GameManager Inspector → Assign PlayerMale.prefab và PlayerFemale.prefab

### **Lỗi: "Animator is null"**
**Fix:** 
1. Check model có Animator component
2. PlayerGameController Inspector → Assign Animator field

### **Player không spawn:**
**Fix:**
1. GameManager → Demo Mode = TRUE
2. Check Player Prefabs assigned
3. Check Console errors

### **Model không hiển thị:**
**Fix:**
1. Check model là child của prefab root
2. Check model active = TRUE
3. Check model có MeshRenderer

### **Animation không chạy:**
**Fix:**
1. Check Animator Controller assigned
2. Check Animator có "Idle" state
3. Check model có Animator component

---

## 🎯 NEXT STEPS

Sau khi hoàn thành setup:

1. **Test multiplayer** - Tắt Demo Mode, test với 2-4 players
2. **Test UI panels** - Click PanelMe → PanelInfo shows
3. **Test property system** - Land on property → PanelBuy shows
4. **Test quiz system** - Land on quiz tile → PanelQuiz shows
5. **Polish & optimize** - Fix bugs, improve performance

---

## 📚 TÀI LIỆU THAM KHẢO

- **MAP_36_TILES.md** - Cấu trúc map chi tiết
- **PLAYERGAMECONTROLLER_REFACTOR_COMPLETE.md** - Giải thích refactor
- **PLAYER_PREFAB_SETUP_GUIDE.md** - Chi tiết player prefabs
- **UNITY_SCENE_SETUP_STEP_BY_STEP.md** - Setup UI chi tiết

---

**Bắt đầu từ PHẦN 2 - Tạo Player Prefabs!** 🚀

