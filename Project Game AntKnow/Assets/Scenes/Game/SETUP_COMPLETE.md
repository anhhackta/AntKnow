# 🎮 Setup Complete Guide - Hướng Dẫn Setup Đầy Đủ

## 🎯 Làm Theo Thứ Tự:

---

## STEP 1: Tạo Turn Indicator (5 phút)

### 1.1. Tạo Prefab:
```
1. Hierarchy → Right-click → 3D Object → Sphere
   Name: "TurnIndicatorPrefab"
   Scale: (0.5, 0.5, 0.5)

2. Create Material:
   Project → Create → Material → "TurnIndicatorMat"
   Color: Yellow (255, 255, 0)
   Emission: ON
   Emission Color: Yellow

3. Apply Material to Sphere

4. Remove Sphere Collider

5. Add Component → TurnIndicator
   Settings:
   - Bob Speed: 2
   - Bob Height: 0.3
   - Offset: (0, 2.5, 0)
   - Ping Object: Drag Sphere here

6. Save as Prefab:
   Drag to Project/Prefabs/TurnIndicatorPrefab
   Delete from Hierarchy
```

---

## STEP 2: Tạo House Prefab (5 phút)

### 2.1. Tạo House:
```
1. Hierarchy → 3D Object → Cube
   Name: "HousePrefab"
   Scale: (0.8, 1.2, 0.8)
   Position: (0, 0.6, 0)

2. Add Roof:
   Right-click HousePrefab → 3D Object → Cube
   Name: "Roof"
   Scale: (1, 0.3, 1)
   Position: (0, 0.75, 0)
   Rotation: (0, 45, 0)

3. Create Material:
   Project → Create → Material → "HouseMat"
   Color: White

4. Apply Material to HousePrefab and Roof

5. Remove all Box Colliders

6. Save as Prefab:
   Drag to Project/Prefabs/HousePrefab
   Delete from Hierarchy
```

---

## STEP 3: Tạo Hotel Prefab (5 phút)

### 3.1. Tạo Hotel:
```
1. Hierarchy → 3D Object → Cube
   Name: "HotelPrefab"
   Scale: (1.2, 2, 1.2)
   Position: (0, 1, 0)

2. Add Sign:
   Right-click HotelPrefab → 3D Object → Cube
   Name: "Sign"
   Scale: (1.5, 0.2, 0.1)
   Position: (0, 1.5, 0.6)

3. Create Material:
   Project → Create → Material → "HotelMat"
   Color: Gold (255, 215, 0)

4. Apply Material to HotelPrefab and Sign

5. Remove all Box Colliders

6. Save as Prefab:
   Drag to Project/Prefabs/HotelPrefab
   Delete from Hierarchy
```

---

## STEP 4: Setup Player Prefab (5 phút)

### 4.1. Add Turn Indicator:
```
1. Open Player Prefab (Assets/Prefabs/Player.prefab)

2. Drag TurnIndicatorPrefab vào Player (as child)
   Position: (0, 2.5, 0)

3. Link to PlayerGameController:
   Select Player root
   PlayerGameController → Turn Indicator: Drag TurnIndicatorPrefab

4. Save Prefab (Ctrl+S)
```

---

## STEP 5: Setup GameManager (10 phút)

### 5.1. Add PropertyManager:
```
1. Select GameManager GameObject

2. Add Component → PropertyManager

3. Add Component → PropertyVisual

4. Link PropertyManager:
   GameManager → Property Manager: Drag PropertyManager component

5. Link PropertyVisual:
   PropertyManager → Property Visual: Drag PropertyVisual component
   PropertyManager → Board Manager: Drag BoardManager GameObject

6. Link Prefabs to PropertyVisual:
   PropertyVisual → House Prefab: Drag HousePrefab from Project
   PropertyVisual → Hotel Prefab: Drag HotelPrefab from Project
```

### 5.2. Check GameManager Settings:
```
✅ Board Manager: Linked
✅ Dice Controller: Linked
✅ Property Manager: Linked
✅ Player Prefab: Linked
✅ Roll Button: Linked
✅ Demo Mode: TRUE (quan trọng!)
✅ Max Turns: 25
```

---

## STEP 6: Setup UI Panels (15 phút)

### 6.1. Link PanelPlayerMe:
```
1. Select PanelPlayerMe GameObject

2. Add Component → PanelPlayerMe

3. Link references:
   - Text Player Name: Drag text component
   - Text Money: Drag text component
   - Image Avatar: Drag image component
   - Image Turn Indicator: Drag image component (optional)

4. Set colors:
   - Male Color: Blue (0, 100, 255)
   - Female Color: Magenta (255, 0, 255)
```

### 6.2. Link PanelBuy:
```
1. Select PanelBuy GameObject

2. Add Component → PanelBuy

3. Link references:
   - Text Property Name
   - Text Owner Name
   - Text Price
   - Btn Buy
   - Btn Skip
   - Btn House1, Btn House2, Btn House3, Btn House4, Btn Hotel
```

### 6.3. Link Other Panels:
```
Follow same pattern for:
- PanelQuiz
- PanelEvent
- PanelHouseSell
- PanelResult
- PanelCard
```

---

## STEP 7: Link Panels to GameManager (5 phút)

### 7.1. Add Panel References:
```
1. Select GameManager

2. Expand "UI Panels" section (nếu có)

3. Link panels:
   - Panel Player Me: Drag PanelPlayerMe
   - Panel Buy: Drag PanelBuy
   - Panel Quiz: Drag PanelQuiz
   - Panel Event: Drag PanelEvent
   - Panel House Sell: Drag PanelHouseSell
   - Panel Result: Drag PanelResult
   - Panel Card: Drag PanelCard
```

---

## ✅ TEST GAME (5 phút)

### Press Play:
```
1. Press Play ▶️

2. Check Console:
   ✅ "[GameManager] Starting game..."
   ✅ "[GameManager] Spawned player: Player 1"
   ✅ No errors

3. Check Scene:
   ✅ Players spawned
   ✅ Yellow ping on current player
   ✅ Ping bobs up/down

4. Click Roll:
   ✅ Dice animate
   ✅ Player moves
   ✅ Player bounces

5. Land on Property:
   ✅ Auto buy
   ✅ Money decreases
   ✅ NO HOUSES YET (level 0 = empty land)

6. Buy again to upgrade:
   ✅ Houses appear on tile
   ✅ Houses have player color
   ✅ Multiple houses for higher levels

7. Upgrade to Hotel:
   ✅ Hotel appears
   ✅ Hotel has gold color
```

---

## 🐛 Troubleshooting:

### Issue: Turn Indicator không hiện
```
Fix:
1. Check TurnIndicatorPrefab added to Player Prefab
2. Check linked in PlayerGameController
3. Check Ping Object linked in TurnIndicator
```

### Issue: Houses không spawn
```
Fix:
1. Check PropertyVisual component added to GameManager
2. Check House/Hotel prefabs linked
3. Check PropertyManager has PropertyVisual reference
4. Check Console for errors
```

### Issue: Houses sai màu
```
Fix:
1. Check Player Colors array in PropertyVisual
2. Check owner index correct
```

### Issue: Panels không hiện
```
Fix:
1. Check panel scripts added
2. Check references linked
3. Check panels linked to GameManager
```

---

## 📋 Checklist:

### Prefabs:
- [ ] TurnIndicatorPrefab created
- [ ] HousePrefab created
- [ ] HotelPrefab created
- [ ] Player Prefab updated

### Components:
- [ ] PropertyManager added to GameManager
- [ ] PropertyVisual added to GameManager
- [ ] TurnIndicator added to Player Prefab
- [ ] UI Panel scripts added

### References:
- [ ] PropertyManager linked to GameManager
- [ ] PropertyVisual linked to PropertyManager
- [ ] House/Hotel prefabs linked to PropertyVisual
- [ ] Turn Indicator linked to PlayerGameController
- [ ] UI Panels linked to GameManager

### Settings:
- [ ] Demo Mode = TRUE
- [ ] Player Colors set in PropertyVisual
- [ ] Bob Speed/Height set in TurnIndicator

---

## 🎯 Expected Result:

```
✅ Game starts
✅ Players spawn with ping indicator
✅ Roll dice works
✅ Player moves with bounce
✅ Buy property works
✅ Houses spawn on tiles
✅ Houses have correct color
✅ Hotel spawns at level 5
✅ Turn switches correctly
✅ Ping moves to next player
```

---

**Follow từng step một! 🎮**

