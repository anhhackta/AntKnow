# 🎮 Setup Final - Hướng Dẫn Cuối Cùng

## 🎯 Bạn Đã Có Sẵn:
```
✅ 36 tiles trong scene
✅ Mỗi tile có:
   - Cube cha (GameObject chính)
   - Platform con (cube mỏng dẹp để đặt house)
   - Text Name (tên ô đất)
   - Text Price (giá)
✅ House/Hotel prefabs với material "ngói"
✅ Panels trong scene
```

---

## 📋 SETUP (30 phút):

### STEP 1: Add TileVisual vào mỗi tile (10 phút)

```
1. Select tile đầu tiên (Tile 0)

2. Add Component → TileVisual

3. TileVisual settings:
   - Auto Find Children: TRUE (tự động tìm platform, text)
   - Tile Index: 0

4. Repeat cho 35 tiles còn lại (Tile 1-35)
   - Hoặc dùng script để add hàng loạt

TIP: Để add hàng loạt, chọn tất cả tiles → Add Component → TileVisual
Sau đó manually set Tile Index cho từng tile
```

---

### STEP 2: Setup GameManager (5 phút)

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
   - Demo Mode: TRUE

   PropertyManager:
   - Property Visual: Drag PropertyVisual component
   - Board Manager: Drag BoardManager

   PropertyVisual:
   - House Prefab: Drag house prefab from Project
   - Hotel Prefab: Drag hotel prefab from Project
   - Roof Material Name: "ngói" (tên material để đổi màu)
   - Player Colors: [Red, Blue, Green, Yellow]
   - Tiles: Leave empty (auto find)
```

---

### STEP 3: Link Panels (10 phút)

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

4. Check mỗi panel đã có script:
   - PanelBuy → PanelBuy.cs
   - PanelQuiz → PanelQuiz.cs
   - etc.

5. Link references trong mỗi panel:
   PanelBuy:
   - Text Property Name
   - Text Owner Name
   - Text Price
   - Btn Buy, Btn Skip
   - Btn House1-4, Btn Hotel
```

---

### STEP 4: Setup Player Prefab (5 phút)

```
1. Open Player Prefab

2. Add TurnIndicator (nếu chưa có):
   - Create Empty child: "TurnIndicator"
   - Position: (0, 2.5, 0)
   - Add Component → TurnIndicator
   - Settings:
     * Bob Speed: 2
     * Bob Height: 0.3
     * Offset: (0, 2.5, 0)

3. Create Ping Visual:
   - Add child Sphere to TurnIndicator
   - Scale: (0.5, 0.5, 0.5)
   - Material: Yellow with Emission
   - Remove Sphere Collider

4. Link TurnIndicator:
   - TurnIndicator → Ping Object: Drag Sphere
   - PlayerGameController → Turn Indicator: Drag TurnIndicator

5. Save Prefab
```

---

## ✅ TEST (5 phút):

```
1. Press Play ▶️

2. Check Console:
   ✅ "[PropertyVisual] Found 36 tiles"
   ✅ "[GameManager] Starting game..."
   ✅ "[GameManager] Spawned player: Player 1"
   ✅ No errors

3. Check Scene:
   ✅ Players spawn
   ✅ Yellow ping on current player
   ✅ Ping bobs up/down

4. Click Roll:
   ✅ Dice animate
   ✅ Player moves
   ✅ Player bounces

5. Land on Property:
   ✅ PanelBuy appears
   ✅ Shows property name, price
   ✅ Can select house level
   ✅ Click Buy → Money decreases

6. Check Tile:
   ✅ Houses spawn on platform
   ✅ Houses have player color (on "ngói" material)
   ✅ Multiple houses for higher levels

7. Upgrade:
   ✅ Land on own property
   ✅ PanelBuy shows upgrade options
   ✅ Select higher level → More houses spawn

8. Hotel:
   ✅ Upgrade to level 5
   ✅ Hotel spawns (replaces houses)
   ✅ Hotel has player color
```

---

## 🐛 Troubleshooting:

### Issue: TileVisual không tìm thấy platform
```
Fix:
1. Check platform child có tên chứa "platform"
2. Hoặc manually link Platform field trong TileVisual
3. Hoặc add tag "Platform" cho platform GameObject
```

### Issue: Houses không spawn
```
Fix:
1. Check PropertyVisual → Tiles array (should auto-fill)
2. Check House/Hotel prefabs linked
3. Check TileVisual.tileIndex đúng (0-35)
4. Check Console for errors
```

### Issue: Houses sai màu
```
Fix:
1. Check Roof Material Name = "ngói" (đúng tên material)
2. Check house/hotel prefab có material tên "ngói"
3. Check Player Colors array có 4 màu
```

### Issue: PanelBuy không hiện
```
Fix:
1. Check PanelBuy linked trong GameManager
2. Check PanelBuy có PanelBuy.cs script
3. Check PanelBuy references linked
4. Check PanelBuy active in Hierarchy
```

### Issue: Text name/price không update
```
Fix:
1. Check TileVisual tìm thấy text components
2. Check text names chứa "name" hoặc "price"
3. Hoặc manually link Text Name, Text Price fields
```

---

## 📋 Checklist:

### Components:
- [ ] TileVisual added to all 36 tiles
- [ ] PropertyManager added to GameManager
- [ ] PropertyVisual added to GameManager
- [ ] TurnIndicator added to Player Prefab
- [ ] Panel scripts added to panels

### References:
- [ ] PropertyManager linked to GameManager
- [ ] PropertyVisual linked to PropertyManager
- [ ] House/Hotel prefabs linked to PropertyVisual
- [ ] Panels linked to GameManager
- [ ] Panel references linked (buttons, texts)
- [ ] Turn Indicator linked to PlayerGameController

### Settings:
- [ ] Demo Mode = TRUE
- [ ] Roof Material Name = "ngói"
- [ ] Player Colors set (4 colors)
- [ ] Tile Index set for each tile (0-35)

---

## 🎯 Expected Result:

```
✅ Game starts
✅ Players spawn with ping
✅ Roll dice works
✅ Player moves
✅ PanelBuy appears when landing on property
✅ Can buy property
✅ Houses spawn on tile platform
✅ Houses have correct player color (on "ngói" material)
✅ Can upgrade property
✅ More houses spawn
✅ Hotel spawns at level 5
✅ Text name/price update on tiles
```

---

## 💡 Tips:

### Để add TileVisual hàng loạt:
```
1. Select all tiles (Shift+Click)
2. Add Component → TileVisual
3. Manually set Tile Index cho từng tile
```

### Để check material name:
```
1. Select house/hotel prefab
2. Expand children
3. Check Renderer → Materials
4. Note material name (ví dụ: "ngói", "Ngói", "roof", etc.)
5. Set Roof Material Name trong PropertyVisual
```

### Để test nhanh:
```
1. Set Demo Mode = TRUE
2. Press Play
3. Click Roll nhiều lần
4. Check houses spawn
```

---

**Follow từng step! 🎮**

