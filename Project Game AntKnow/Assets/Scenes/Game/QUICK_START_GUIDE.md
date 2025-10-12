# 🚀 QUICK START GUIDE - TRIỂN KHAI GAME TRONG 30 PHÚT

## 📋 CHUẨN BỊ

### Yêu cầu:
- ✅ Unity Editor đã mở project
- ✅ Code đã compile không lỗi
- ✅ Firebase SDK đã import
- ✅ Netcode for GameObjects đã import
- ✅ TextMeshPro đã import

---

## ⚡ 30 PHÚT SETUP

### **PHÚT 1-5: Generate Board**

1. **Mở Tile Generator:**
   - Unity Menu → **AntKnow → Tile Generator**

2. **Settings:**
   - Tile Count: **36**
   - Tile Size: **2**
   - Tile Height: **0.2**
   - Platform Scale: **0.8**
   - Waypoint Height: **0.5**

3. **Click:**
   - **"Generate Tiles & Waypoints"**

4. **Kết quả:**
   - ✅ 36 tiles trong Map parent
   - ✅ 36 waypoints trong Waypoints parent
   - ✅ Mỗi tile có Platform, TextName, TextPrice

---

### **PHÚT 6-10: Generate UI Panels**

1. **Mở UI Panel Setup Helper:**
   - Unity Menu → **AntKnow → UI Panel Setup Helper**

2. **Click:**
   - **"Create ALL Panels"**

3. **Kết quả:**
   - ✅ Canvas tự động tạo
   - ✅ PanelGame (với PanelMe + PanelPlayerContainer)
   - ✅ PanelGameInfo
   - ✅ PanelRoll
   - ✅ PanelInfo (inactive)
   - ✅ PanelBuy (inactive)
   - ✅ PanelQuiz (inactive)
   - ✅ PanelEvent (inactive)
   - ✅ PanelNotification (inactive)

---

### **PHÚT 11-15: Tạo Player Prefabs**

#### **PlayerMale:**

1. **Hierarchy → Create Empty → "PlayerMale"**

2. **Add Components:**
   - **Network Object**
     - Is Player Object: ✓ TRUE
   - **Player Game Controller**
     - Is Male: ✓ TRUE
     - Money: 10000
     - Move Speed: 5
     - Bounce Height: 0.5

3. **Import Male Model:**
   - Drag male 3D model vào làm child
   - Rename: "MaleModel"
   - Assign Animator component

4. **Tạo Turn Indicator:**
   - PlayerMale → Create Empty → "TurnIndicator"
   - Position: (0, 2.5, 0)
   - Add Component: **Turn Indicator**
   - Add Component: **Network Object**
   - Child: 3D Object → Sphere
     - Scale: (0.3, 0.3, 0.3)
     - Material: Yellow

5. **Save Prefab:**
   - Drag PlayerMale to Project → Save as **PlayerMale.prefab**
   - Delete from Hierarchy

#### **PlayerFemale:**

6. **Repeat steps 1-5 nhưng:**
   - Name: "PlayerFemale"
   - Is Male: **FALSE**
   - Use female 3D model

---

### **PHÚT 16-20: Setup GameManager**

1. **Tạo GameManager:**
   - Hierarchy → Create Empty → "GameManager"

2. **Add Components:**
   - **Network Object**
     - Is Player Object: FALSE
   - **Game Manager**

3. **Assign Inspector:**

**Player Prefabs:**
- Player Prefab Male: [Drag PlayerMale.prefab]
- Player Prefab Female: [Drag PlayerFemale.prefab]

**UI:**
- Roll Button: [Drag Canvas/PanelRoll/BtnRoll]
- Turn Text: [Drag Canvas/PanelGameInfo/TextTurn]
- Current Player Text: [Drag Canvas/PanelGameInfo/TextCurrentPlayer]
- Time Text: [Drag Canvas/PanelGameInfo/TextTime]

**UI Panels:**
- Panel Buy: [Drag Canvas/PanelBuy]
- Panel Quiz: [Drag Canvas/PanelQuiz]
- Panel Event: [Drag Canvas/PanelEvent]
- Panel House Sell: [Drag Canvas/PanelHouseSell if exists]
- Panel Result: [Drag Canvas/PanelResult if exists]

**Game Settings:**
- Max Turns: **25**
- Demo Mode: ✓ **TRUE** (for testing)

---

### **PHÚT 21-25: Setup BoardManager & PropertyManager**

#### **BoardManager:**

1. **Tạo BoardManager:**
   - Hierarchy → Create Empty → "BoardManager"

2. **Add Component:**
   - **Board Manager**

3. **Assign Inspector:**
   - Map Parent: [Drag Map]
   - Waypoint Parent: [Drag Waypoints]

#### **PropertyManager:**

4. **Tạo PropertyManager:**
   - Hierarchy → Create Empty → "PropertyManager"

5. **Add Component:**
   - **Property Manager**

6. **Assign Inspector:**
   - Property Visual: [Tạo GameObject "PropertyVisual" → Add PropertyVisual.cs → Drag vào]
   - Board Manager: [Drag BoardManager]

---

### **PHÚT 26-30: Final Setup & Test**

1. **Assign GameManager References:**
   - Board Manager: [Drag BoardManager]
   - Property Manager: [Drag PropertyManager]

2. **Assign PanelGame References:**
   - Select Canvas/PanelGame
   - Panel Me: [Drag PanelMe]
   - Panel Player Container: [Drag PanelPlayerContainer]
   - Panel Player Prefab: [Tạo prefab từ PanelPlayerPrefab]

3. **Assign PanelGameInfo References:**
   - Select Canvas/PanelGameInfo
   - Text Turn: [Drag TextTurn]
   - Text Time: [Drag TextTime]
   - Text Current Player: [Drag TextCurrentPlayer]
   - Max Turns: 25

4. **Assign PanelRoll References:**
   - Select Canvas/PanelRoll
   - Dice1 Image: [Drag Dice1]
   - Dice2 Image: [Drag Dice2]
   - Text Result: [Drag TextResult]
   - Btn Roll: [Drag BtnRoll]
   - Dice Sprites: [Import 6 dice sprites → Assign]

5. **Save Scene:**
   - Ctrl+S → Save as "GameScene"

6. **Test:**
   - Click **Play**
   - Expected:
     - ✅ 1 player spawns at Tile0
     - ✅ PanelGame shows player info
     - ✅ PanelGameInfo shows "Turn: 1/25"
     - ✅ PanelRoll shows dice + button
     - ✅ Click Roll → Dice animation
     - ✅ Player moves with bounce effect
     - ✅ No errors in Console

---

## ✅ CHECKLIST HOÀN THÀNH

### Scene Setup:
- [ ] 36 tiles generated
- [ ] 36 waypoints generated
- [ ] Canvas + 10 UI panels created
- [ ] PlayerMale.prefab created
- [ ] PlayerFemale.prefab created

### GameObjects:
- [ ] GameManager (with NetworkObject + GameManager.cs)
- [ ] BoardManager (with BoardManager.cs)
- [ ] PropertyManager (with PropertyManager.cs)
- [ ] PropertyVisual (with PropertyVisual.cs)

### References Assigned:
- [ ] GameManager → Player Prefabs
- [ ] GameManager → UI Panels
- [ ] GameManager → BoardManager, PropertyManager
- [ ] BoardManager → Map, Waypoints
- [ ] PropertyManager → PropertyVisual, BoardManager
- [ ] PanelGame → PanelMe, Container, Prefab
- [ ] PanelGameInfo → Texts
- [ ] PanelRoll → Dice, Button, Sprites

### Testing:
- [ ] Play Mode works
- [ ] Player spawns
- [ ] UI displays correctly
- [ ] No Console errors

---

## 🐛 TROUBLESHOOTING

### Lỗi: "Player prefabs not assigned"
**Fix:** GameManager Inspector → Assign PlayerMale.prefab và PlayerFemale.prefab

### Lỗi: "BoardManager not found"
**Fix:** GameManager Inspector → Assign BoardManager GameObject

### Lỗi: "Waypoint not found"
**Fix:** BoardManager Inspector → Assign Waypoints parent

### Lỗi: "TextMeshPro missing"
**Fix:** Window → TextMeshPro → Import TMP Essential Resources

### Lỗi: "Dice sprites not assigned"
**Fix:** PanelRoll Inspector → Dice Sprites → Assign 6 sprites (dice_1 to dice_6)

### Player không spawn:
**Fix:** 
1. Check GameManager → Demo Mode = TRUE
2. Check Player Prefabs assigned
3. Check Console for errors

### UI không hiện:
**Fix:**
1. Check Canvas → Render Mode = Screen Space Overlay
2. Check EventSystem exists in scene
3. Check Panel active state

---

## 🎯 NEXT STEPS

Sau khi hoàn thành Quick Start:

### **Immediate:**
1. Test roll dice → player movement
2. Test click PanelMe → PanelInfo shows
3. Test land on property → PanelBuy shows

### **Phase 3: Hoàn thiện UI Panels**
- Kết nối tất cả panels với GameManager
- Test Firebase quiz loading
- Test all panel interactions

### **Phase 4: Game Flow**
- Test turn order selection
- Test 8-round quiz system
- Test win conditions

### **Phase 5: Property System**
- Test buy/upgrade/sell properties
- Test visual updates (houses, colors)
- Test rent payment

### **Phase 6: Multiplayer**
- Test 2-4 players
- Test network synchronization
- Test turn rotation

---

## 📚 TÀI LIỆU THAM KHẢO

- **IMPLEMENTATION_ROADMAP.md** - Lộ trình chi tiết 8 phases
- **UNITY_SCENE_SETUP_STEP_BY_STEP.md** - Hướng dẫn setup từng bước
- **DBview.md** - Firebase schema
- **PANEL_SUMMARY.md** - Tóm tắt UI panels

---

## 💡 TIPS

1. **Save thường xuyên:** Ctrl+S sau mỗi bước
2. **Test từng bước:** Không chờ đến cuối mới test
3. **Check Console:** Luôn kiểm tra errors/warnings
4. **Backup Scene:** Duplicate scene trước khi thay đổi lớn
5. **Use Prefabs:** Tạo prefabs cho reusable components

---

**Bắt đầu ngay! 🚀**

Nếu gặp vấn đề, check TROUBLESHOOTING section hoặc hỏi tôi!

