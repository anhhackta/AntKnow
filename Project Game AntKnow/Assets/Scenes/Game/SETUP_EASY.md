# 🎮 Setup Easy - Hướng Dẫn Đơn Giản

## 🎯 Bạn Có:
```
✅ GameObject "Tiles" chứa tất cả tiles con
✅ Mỗi tile con có:
   - Cube platform
   - Text name
   - Text price
✅ House/Hotel prefabs
✅ Panels trong scene
```

---

## 📋 SETUP SIÊU NHANH (10 phút):

### STEP 1: Setup Tiles GameObject (2 phút) ⭐

```
1. Select GameObject "Tiles" trong Hierarchy

2. Add Component → TileSetup

3. TileSetup settings:
   - Auto Setup On Awake: TRUE
   - Add Tile Visual Component: TRUE
   - Show Debug: TRUE

4. Right-click TileSetup component → "Setup All Tiles"
   → Sẽ tự động add TileVisual vào tất cả tiles con
   → Tự động set Tile Index (0, 1, 2, ..., 35)

5. Check Console:
   ✅ "[TileSetup] Setup complete! Total tiles: 36"
```

**Xong! Tất cả tiles đã có TileVisual với index đúng!** ⭐

---

### STEP 2: Setup GameManager (3 phút)

```
1. Select GameManager

2. Add Component → PropertyManager (nếu chưa có)

3. Add Component → PropertyVisual (nếu chưa có)

4. Link references:
   GameManager:
   - Property Manager: Drag PropertyManager component
   - Board Manager: Drag BoardManager
   - Dice Controller: Drag DiceController
   - Player Prefab: Drag from Project
   - Roll Button: Drag button
   - Demo Mode: TRUE ⭐

   PropertyManager:
   - Property Visual: Drag PropertyVisual component
   - Board Manager: Drag BoardManager

   PropertyVisual:
   - Tile Setup: Drag "Tiles" GameObject ⭐
   - House Prefab: Drag house prefab
   - Hotel Prefab: Drag hotel prefab
   - Roof Material Name: "ngói" (hoặc tên material bạn dùng)
   - Player Colors: [Red, Blue, Green, Yellow]
```

---

### STEP 3: Link Panels (3 phút)

```
1. Select GameManager

2. Expand "UI Panels" section

3. Link panels:
   - Panel Buy: Drag PanelBuy GameObject
   - Panel Quiz: Drag PanelQuiz GameObject
   - Panel Event: Drag PanelEvent GameObject
   - Panel House Sell: Drag PanelHouseSell GameObject
   - Panel Result: Drag PanelResult GameObject
   - Panel Card: Drag PanelCard GameObject

4. Check mỗi panel có script:
   - PanelBuy → PanelBuy.cs
   - PanelQuiz → PanelQuiz.cs
   - etc.
```

---

### STEP 4: Setup Player Prefab (2 phút)

```
1. Open Player Prefab

2. Add TurnIndicator:
   - Create Empty child: "TurnIndicator"
   - Position: (0, 2.5, 0)
   - Add Component → TurnIndicator

3. Create Ping Visual:
   - Add Sphere child to TurnIndicator
   - Scale: (0.5, 0.5, 0.5)
   - Material: Yellow with Emission
   - Remove Sphere Collider

4. Link:
   - TurnIndicator → Ping Object: Drag Sphere
   - PlayerGameController → Turn Indicator: Drag TurnIndicator

5. Save Prefab
```

---

## ✅ TEST (2 phút):

```
1. Press Play ▶️

2. Check Console:
   ✅ "[TileSetup] Setup complete! Total tiles: 36"
   ✅ "[PropertyVisual] Got 36 tiles from TileSetup"
   ✅ "[GameManager] Starting game..."
   ✅ No errors

3. Check Scene:
   ✅ Players spawn
   ✅ Yellow ping on current player

4. Click Roll:
   ✅ Dice animate
   ✅ Player moves

5. Land on Property:
   ✅ PanelBuy appears
   ✅ Shows property name, price
   ✅ Can select house level
   ✅ Click Buy

6. Check Tile:
   ✅ Houses spawn on platform
   ✅ Houses have player color (on "ngói" material)
```

---

## 🎯 Key Points:

### TileSetup Script:
```
✅ Gắn vào GameObject "Tiles" (parent)
✅ Tự động add TileVisual vào tất cả tiles con
✅ Tự động set Tile Index (0-35)
✅ Right-click → "Setup All Tiles" để chạy
✅ Chỉ cần làm 1 lần!
```

### PropertyVisual:
```
✅ Link "Tile Setup" field → GameObject "Tiles"
✅ Tự động lấy tất cả TileVisual từ TileSetup
✅ Không cần manually link từng tile
```

### TileVisual:
```
✅ Tự động tìm platform con
✅ Tự động tìm text name, text price
✅ Spawn houses lên platform
✅ Set màu cho material "ngói"
```

---

## 🐛 Troubleshooting:

### Issue: TileSetup không tìm thấy tiles
```
Fix:
1. Check GameObject "Tiles" có children không
2. Check TileSetup gắn đúng vào "Tiles" GameObject
3. Right-click TileSetup → "Setup All Tiles"
```

### Issue: PropertyVisual không tìm thấy tiles
```
Fix:
1. Check PropertyVisual → Tile Setup field linked
2. Check TileSetup đã chạy "Setup All Tiles"
3. Check Console log
```

### Issue: Houses không spawn
```
Fix:
1. Check House/Hotel prefabs linked
2. Check Roof Material Name đúng
3. Check TileVisual tìm thấy platform
4. Check Console for errors
```

### Issue: Text name/price không update
```
Fix:
1. Check TileVisual tìm thấy text components
2. Check text names chứa "name" hoặc "price"
3. Hoặc manually link trong TileVisual
```

---

## 📋 Checklist:

### Setup:
- [ ] TileSetup added to "Tiles" GameObject
- [ ] Right-click → "Setup All Tiles" executed
- [ ] PropertyManager added to GameManager
- [ ] PropertyVisual added to GameManager
- [ ] TurnIndicator added to Player Prefab

### References:
- [ ] PropertyVisual → Tile Setup: Linked to "Tiles"
- [ ] PropertyVisual → House/Hotel prefabs: Linked
- [ ] PropertyManager → Property Visual: Linked
- [ ] GameManager → Property Manager: Linked
- [ ] GameManager → Panels: All linked
- [ ] PlayerGameController → Turn Indicator: Linked

### Settings:
- [ ] Demo Mode = TRUE
- [ ] Roof Material Name = "ngói"
- [ ] Player Colors set (4 colors)
- [ ] Auto Setup On Awake = TRUE

---

## 💡 Tips:

### Để re-setup tiles:
```
1. Right-click TileSetup → "Remove All TileVisual"
2. Right-click TileSetup → "Setup All Tiles"
```

### Để check tile index:
```
1. Select any tile con
2. Check TileVisual component
3. Tile Index should be 0-35
```

### Để test nhanh:
```
1. Demo Mode = TRUE
2. Press Play
3. Click Roll
4. Check houses spawn
```

---

## 🎯 Expected Result:

```
✅ TileSetup auto adds TileVisual to all tiles
✅ All tiles have correct index (0-35)
✅ PropertyVisual gets tiles from TileSetup
✅ Game starts
✅ PanelBuy appears
✅ Houses spawn on platform
✅ Houses have correct color
✅ Text name/price update
```

---

**Chỉ cần 10 phút! 🎮**

**Key: Right-click TileSetup → "Setup All Tiles" ⭐**

