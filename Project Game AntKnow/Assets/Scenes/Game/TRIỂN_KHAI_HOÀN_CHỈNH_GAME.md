# 🎮 TRIỂN KHAI HOÀN CHỈNH GAME - CHẠY ĐƯỢC NGAY

**Mục tiêu:** Setup TOÀN BỘ để game CHẠY ĐƯỢC trong 1 giờ

---

## 📋 **PHẦN 1: TẠO PLAYER PREFABS (15 PHÚT)**

### **A. Tạo PlayerMale.prefab**

#### **Bước 1: Tạo GameObject**
1. Hierarchy → Right-click → Create Empty
2. Rename: **"PlayerMale"**

#### **Bước 2: Add NetworkObject Component**
1. Select PlayerMale
2. Add Component → **Network Object**
3. **Inspector → Network Object:**
   ```
   ✓ Is Player Object: TRUE
   Owner Permission: Owner
   Synchronize Transform: TRUE
   Interpolate: TRUE
   ```

#### **Bước 3: Add PlayerGameController Component**
1. Add Component → **Player Game Controller**
2. **Inspector → Player Game Controller:**
   ```
   Player Info:
   ├── Player Name: "Player" (sẽ set runtime)
   ├── Player Id: "" (sẽ set runtime)
   ├── Is Male: ✓ TRUE ← ⭐ QUAN TRỌNG!
   └── Player Index: 0 (sẽ set runtime)
   
   Movement:
   ├── Move Speed: 5
   ├── Bounce Height: 0.5
   ├── Bounce Duration: 0.3
   ├── Board Manager: (để trống - auto find)
   └── Board Center: (0, 0, 0)
   
   Animation:
   └── Animator: (sẽ assign sau khi add model)
   
   Turn Indicator:
   └── Turn Indicator: (để trống - auto create)
   ```

#### **Bước 4: Add Male Model**
1. **Project window** → Tìm male 3D model
2. **Drag model** vào PlayerMale làm **child**
3. **Rename child** → **"MaleModel"**
4. **Position:** (0, 0, 0)
5. **Ensure model có:**
   - MeshRenderer
   - Animator component
   - Animator Controller assigned

#### **Bước 5: Assign Animator**
1. Select **PlayerMale** (root)
2. Inspector → Player Game Controller
3. **Animator field:** Drag **MaleModel/Animator** vào đây

#### **Bước 6: Save Prefab**
1. **Drag PlayerMale** từ Hierarchy → Project window
2. Save vào: **Assets/Prefabs/Players/PlayerMale.prefab**
3. **Delete PlayerMale** từ Hierarchy

---

### **B. Tạo PlayerFemale.prefab**

#### **Option 1: Duplicate (Nhanh nhất)**
1. Project window → Right-click **PlayerMale.prefab** → Duplicate
2. Rename → **PlayerFemale**
3. Double-click PlayerFemale.prefab (mở Prefab mode)
4. Select root (PlayerFemale)
5. Inspector → Player Game Controller:
   - **Is Male: ✗ FALSE** ← ⭐ THAY ĐỔI NÀY!
6. Delete child **"MaleModel"**
7. Drag **female 3D model** vào làm child
8. Rename child → **"FemaleModel"**
9. Position: (0, 0, 0)
10. Assign Animator: Drag FemaleModel/Animator → Animator field
11. Save Prefab (Ctrl+S)
12. Exit Prefab mode

---

## 📋 **PHẦN 2: TẠO UI PANELS (20 PHÚT)**

### **A. Tạo Canvas**

1. Hierarchy → Right-click → UI → Canvas
2. Rename: **"Canvas"**
3. **Canvas component:**
   ```
   Render Mode: Screen Space - Overlay
   Pixel Perfect: ✓ TRUE
   ```
4. **Canvas Scaler:**
   ```
   UI Scale Mode: Scale With Screen Size
   Reference Resolution: 1920 x 1080
   Match: 0.5 (Width/Height)
   ```

---

### **B. Tạo PanelGame (Top-Left)**

#### **Bước 1: Tạo PanelGame**
1. Canvas → Right-click → Create Empty
2. Rename: **"PanelGame"**
3. Add Component → **Panel Game** (script)
4. **RectTransform:**
   ```
   Anchor: Top-Left
   Pivot: (0, 1)
   Pos X: 20
   Pos Y: -20
   Width: 300
   Height: 500
   ```

#### **Bước 2: Tạo PanelMe (child của PanelGame)**
1. PanelGame → Right-click → UI → Image
2. Rename: **"PanelMe"**
3. Add Component → **Panel Player Me** (script)
4. Add Component → **Button** (để click mở PanelInfo)
5. **RectTransform:**
   ```
   Anchor: Top-Stretch
   Pivot: (0.5, 1)
   Pos Y: 0
   Height: 100
   ```
6. **Image component:**
   ```
   Color: (0.2, 0.2, 0.2, 0.8) - Màu xám trong suốt
   ```

#### **Bước 3: Tạo UI cho PanelMe**

**3.1. ImageBackground (child của PanelMe):**
```
UI → Image
Name: ImageBackground
RectTransform: Stretch All (Left:0, Right:0, Top:0, Bottom:0)
Color: (1, 0, 0, 0.3) - Đỏ trong suốt (sẽ đổi màu runtime)
```

**3.2. ImageAvatar (child của PanelMe):**
```
UI → Image
Name: ImageAvatar
RectTransform:
  Anchor: Left-Center
  Pivot: (0, 0.5)
  Pos X: 10
  Pos Y: 0
  Width: 60
  Height: 60
Sprite: (assign male/female sprite sau)
```

**3.3. TextPlayerName (child của PanelMe):**
```
UI → Text - TextMeshPro
Name: TextPlayerName
RectTransform:
  Anchor: Top-Stretch
  Pivot: (0.5, 1)
  Pos Y: -10
  Left: 80
  Right: 10
  Height: 30
Text: "Player Name"
Font Size: 20
Alignment: Left, Top
Color: White
```

**3.4. TextMoney (child của PanelMe):**
```
UI → Text - TextMeshPro
Name: TextMoney
RectTransform:
  Anchor: Bottom-Stretch
  Pivot: (0.5, 0)
  Pos Y: 10
  Left: 80
  Right: 10
  Height: 30
Text: "$10000"
Font Size: 18
Alignment: Left, Bottom
Color: Yellow
```

#### **Bước 4: Assign references cho PanelMe**
1. Select **PanelMe**
2. Inspector → **Panel Player Me (Script):**
   ```
   UI Components:
   ├── Image Background: Drag ImageBackground
   ├── Text Player Name: Drag TextPlayerName
   ├── Text Money: Drag TextMoney
   └── Image Avatar: Drag ImageAvatar
   
   Avatar Sprites:
   ├── Sprite Male: (assign male sprite)
   └── Sprite Female: (assign female sprite)
   
   Background Settings:
   └── Background Alpha: 0.3
   ```

---

#### **Bước 5: Tạo PanelPlayerContainer (child của PanelGame)**
1. PanelGame → Right-click → Create Empty
2. Rename: **"PanelPlayerContainer"**
3. Add Component → **Vertical Layout Group**
4. **Vertical Layout Group:**
   ```
   Padding: Left:10, Right:10, Top:10, Bottom:10
   Spacing: 10
   Child Alignment: Upper Center
   ✓ Child Control Width: TRUE
   ✗ Child Control Height: FALSE
   ✓ Child Force Expand Width: TRUE
   ✗ Child Force Expand Height: FALSE
   ```
5. **RectTransform:**
   ```
   Anchor: Top-Stretch
   Pivot: (0.5, 1)
   Pos Y: -110 (dưới PanelMe)
   Left: 0
   Right: 0
   Height: 380
   ```

---

#### **Bước 6: Tạo PanelPlayerPrefab**

**6.1. Tạo prefab:**
1. PanelPlayerContainer → Right-click → UI → Image
2. Rename: **"PanelPlayerPrefab"**
3. Add Component → **Panel Player** (script)
4. Add Component → **Button**
5. **RectTransform:**
   ```
   Anchor: Top-Stretch
   Pivot: (0.5, 1)
   Height: 100
   ```

**6.2. Tạo UI giống PanelMe:**
- ImageBackground
- ImageAvatar
- TextPlayerName
- TextMoney

**6.3. Assign references:**
```
Panel Player (Script):
├── Image Background: Drag ImageBackground
├── Text Player Name: Drag TextPlayerName
├── Text Money: Drag TextMoney
├── Image Avatar: Drag ImageAvatar
├── Sprite Male: (assign)
├── Sprite Female: (assign)
└── Background Alpha: 0.3
```

**6.4. Save Prefab:**
1. Drag PanelPlayerPrefab → Project window
2. Save: **Assets/Prefabs/UI/PanelPlayerPrefab.prefab**
3. **Delete PanelPlayerPrefab** từ Hierarchy (giữ lại container trống)

---

#### **Bước 7: Assign references cho PanelGame**
1. Select **PanelGame**
2. Inspector → **Panel Game (Script):**
   ```
   Panel Components:
   ├── Panel Me: Drag PanelMe
   ├── Panel Player Container: Drag PanelPlayerContainer
   └── Panel Player Prefab: Drag PanelPlayerPrefab.prefab
   
   Settings:
   └── Max Players: 4
   ```

---

### **C. Tạo PanelGameInfo (Top-Center)**

1. Canvas → Right-click → UI → Image
2. Rename: **"PanelGameInfo"**
3. Add Component → **Panel Game Info** (script)
4. **RectTransform:**
   ```
   Anchor: Top-Center
   Pivot: (0.5, 1)
   Pos X: 0
   Pos Y: -20
   Width: 400
   Height: 100
   ```

**Tạo UI:**
```
TextTurn:
  Text: "Turn: 1/25"
  Pos Y: -10
  Height: 30
  
TextTime:
  Text: "Time: 00:00"
  Pos Y: -45
  Height: 25
  
TextCurrentPlayer:
  Text: "Current: Player 1"
  Pos Y: -75
  Height: 25
```

**Assign references:**
```
Panel Game Info (Script):
├── Text Turn: Drag TextTurn
├── Text Time: Drag TextTime
└── Text Current Player: Drag TextCurrentPlayer
```

---

### **D. Tạo PanelRoll (Bottom-Right)**

1. Canvas → Right-click → UI → Image
2. Rename: **"PanelRoll"**
3. Add Component → **Panel Roll** (script)
4. **RectTransform:**
   ```
   Anchor: Bottom-Right
   Pivot: (1, 0)
   Pos X: -20
   Pos Y: 20
   Width: 250
   Height: 200
   ```

**Tạo UI:**
```
Dice1 (Image):
  Width: 80, Height: 80
  Pos: (-150, 80)
  
Dice2 (Image):
  Width: 80, Height: 80
  Pos: (-60, 80)
  
TextResult (TextMeshPro):
  Text: "Result: 0"
  Pos Y: 30
  
BtnRoll (Button):
  Text: "ROLL DICE"
  Width: 200, Height: 50
  Pos Y: -20
```

**Assign references:**
```
Panel Roll (Script):
├── Dice 1: Drag Dice1
├── Dice 2: Drag Dice2
├── Text Result: Drag TextResult
├── Btn Roll: Drag BtnRoll
└── Dice Sprites: (assign 6 sprites: dice_1 to dice_6)
```

---

### **E. Tạo các Conditional Panels (INACTIVE)**

**Tạo nhanh:**
1. PanelInfo (Center, 600x400, INACTIVE)
2. PanelBuy (Center, 500x600, INACTIVE)
3. PanelQuiz (Center, 800x600, INACTIVE)
4. PanelEvent (Center, 500x300, INACTIVE)
5. PanelHouseSell (Center, 600x500, INACTIVE)
6. PanelResult (Center, 700x600, INACTIVE)
7. PanelNotification (Top-Center, 400x100, INACTIVE)

**Mỗi panel:**
- Add script tương ứng
- Tạo UI theo PANEL_SUMMARY.md
- Assign references
- **SetActive(false)**

---

## 📋 **PHẦN 3: SETUP GAMEMANAGER (10 PHÚT)**

### **A. Tạo GameManager GameObject**

1. Hierarchy → Create Empty
2. Rename: **"GameManager"**
3. Add Component → **Network Object**
   ```
   ✗ Is Player Object: FALSE
   Owner Permission: Server Only
   ✗ Synchronize Transform: FALSE
   ```
4. Add Component → **Game Manager** (script)

---

### **B. Assign References**

**Inspector → Game Manager (Script):**

```
Players:
├── Player Prefab Male: Drag PlayerMale.prefab
└── Player Prefab Female: Drag PlayerFemale.prefab

UI:
├── Roll Button: Drag Canvas/PanelRoll/BtnRoll
├── Turn Text: Drag Canvas/PanelGameInfo/TextTurn
├── Current Player Text: Drag Canvas/PanelGameInfo/TextCurrentPlayer
└── Time Text: Drag Canvas/PanelGameInfo/TextTime

UI Panels:
├── Panel Game: Drag Canvas/PanelGame
├── Panel Buy: Drag Canvas/PanelBuy
├── Panel Quiz: Drag Canvas/PanelQuiz
├── Panel Event: Drag Canvas/PanelEvent
├── Panel House Sell: Drag Canvas/PanelHouseSell
├── Panel Result: Drag Canvas/PanelResult
└── Panel Card: (nếu có)

Game Settings:
├── Max Turns: 25
├── Quiz Interval: 8
├── Salary Amount: 2000
└── ✓ Demo Mode: TRUE (for testing)
```

---

## 📋 **PHẦN 4: SETUP BOARD & PROPERTY (5 PHÚT)**

### **A. BoardManager**

1. Hierarchy → Create Empty → "BoardManager"
2. Add Component → **Board Manager**
3. **Inspector:**
   ```
   Map Parent: Drag "Tiles" hoặc "Map" GameObject
   Waypoint Parent: Drag "Waypoints" GameObject
   ```

### **B. PropertyManager**

1. Hierarchy → Create Empty → "PropertyManager"
2. Add Component → **Property Manager**

### **C. PropertyVisual**

1. Hierarchy → Create Empty → "PropertyVisual"
2. Add Component → **Property Visual**
3. **Inspector:**
   ```
   House Prefab: (assign nếu có)
   Hotel Prefab: (assign nếu có)
   Roof Material Name: "ngói"
   ```

### **D. Kết nối**

**PropertyManager:**
```
Property Visual: Drag PropertyVisual
Board Manager: Drag BoardManager
```

**GameManager:**
```
Board Manager: Drag BoardManager
Property Manager: Drag PropertyManager
```

---

## 📋 **PHẦN 5: TEST GAME (10 PHÚT)**

### **A. Final Checklist**

```
✓ PlayerMale.prefab created (isMale = TRUE)
✓ PlayerFemale.prefab created (isMale = FALSE)
✓ Canvas + PanelGame + PanelMe + PanelPlayerContainer
✓ PanelPlayerPrefab.prefab created
✓ PanelGameInfo created
✓ PanelRoll created
✓ GameManager created
✓ BoardManager created
✓ PropertyManager created
✓ All references assigned
✓ Demo Mode = TRUE
```

### **B. Play Test**

1. **Click Play**
2. **Expected:**
   - ✅ 1 player spawns tại Tile 0
   - ✅ PanelMe hiển thị name + money
   - ✅ PanelGameInfo hiển thị "Turn: 1/25"
   - ✅ PanelRoll hiển thị dice + button
   - ✅ Model hiển thị (male/female)
   - ✅ No errors

3. **Click "ROLL DICE":**
   - ✅ Dice animation
   - ✅ Player moves
   - ✅ Turn indicator hiện

---

## 🎯 **SUMMARY**

**Thời gian:**
- Player Prefabs: 15 phút
- UI Panels: 20 phút
- GameManager: 10 phút
- Board/Property: 5 phút
- **Tổng: 50 phút**

**Kết quả:**
- ✅ Game chạy được
- ✅ UI hoạt động
- ✅ Player spawn
- ✅ Roll dice works
- ✅ Movement works

**Next steps:**
- Test multiplayer (2-4 players)
- Test property system
- Test quiz system
- Polish UI

---

**BẮT ĐẦU NGAY! 🚀**

