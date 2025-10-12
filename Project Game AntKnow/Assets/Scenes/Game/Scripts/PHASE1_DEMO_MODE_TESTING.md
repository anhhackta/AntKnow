# 🧪 PHASE 1: DEMO MODE TESTING

**Fix button Roll và test gameplay**

---

## ✅ **ĐÃ SỬA**

### **Fix 1: RollAndMove() - Demo Mode không cần IsHost**

**Before:**
```csharp
private IEnumerator RollAndMove()
{
    PlayerGameController player = CurrentPlayer;
    
    if (!IsHost) // ❌ Demo Mode không có Host!
    {
        yield break;
    }
    // ...
}
```

**After:**
```csharp
private IEnumerator RollAndMove()
{
    PlayerGameController player = CurrentPlayer;
    
    // ⭐ Demo Mode: Không cần check IsHost
    if (!demoMode && !IsHost)
    {
        yield break; // Only Host can roll dice (multiplayer only)
    }
    // ...
}
```

---

### **Fix 2: NotifyDiceRolledClientRpc - Demo Mode không có network**

**Before:**
```csharp
// Notify all clients of dice result
NotifyDiceRolledClientRpc(currentPlayerIndex, die1, die2, diceResult, isDouble, wasLuckyDouble);
```

**After:**
```csharp
// Notify all clients of dice result (Multiplayer only)
if (!demoMode)
{
    NotifyDiceRolledClientRpc(currentPlayerIndex, die1, die2, diceResult, isDouble, wasLuckyDouble);
}
else
{
    // ⭐ Demo Mode: Show dice animation locally
    Debug.Log($"[Demo] Dice result: {die1} + {die2} = {diceResult}");
    if (panelRoll != null)
    {
        StartCoroutine(panelRoll.RollDice(die1, die2, isDouble, wasLuckyDouble));
    }
}
```

---

## 🛠️ **SETUP TRONG UNITY**

### **Bước 1: Verify GameManager Settings**

```
Hierarchy → Select GameManager
Inspector → Game Manager (Script)

Settings:
├── Demo Mode: ✓ TRUE ← ⭐ QUAN TRỌNG!
│
UI:
├── Roll Button: [Assign Button] ← ⭐ CẦN ASSIGN
├── Turn Text: [Assign Text]
├── Current Player Text: [Assign Text]
└── Time Text: [Assign Text]

UI Panels:
├── Panel Game: [PanelGame] ✅
├── Panel Roll: [PanelRoll] ← ⭐ CẦN ASSIGN
├── Panel Buy: [PanelBuy]
└── ...

Managers:
├── Board Manager: [BoardManager] ✅
├── Property Manager: [PropertyManager]
└── Panel Roll: [PanelRoll]

Players:
├── Player Prefab Male: [PlayerMale] ✅
└── Player Prefab Female: [PlayerFemale] ✅
```

---

### **Bước 2: Tạo/Verify Roll Button**

#### **Option A: Nếu chưa có Roll Button**

```
Canvas → Right-click → UI → Button - TextMeshPro
Name: ButtonRoll

RectTransform:
  Anchor: Bottom-Right
  Pivot: (1, 0)
  Pos X: -50
  Pos Y: 50
  Width: 150
  Height: 60

Button:
  Interactable: ✓ TRUE
  Normal Color: (0.2, 0.8, 0.2, 1) - Green
  Highlighted Color: (0.3, 1, 0.3, 1)
  Pressed Color: (0.1, 0.6, 0.1, 1)

Text (child):
  Text: "ROLL DICE"
  Font Size: 24
  Color: White
  Font Style: Bold
```

#### **Option B: Nếu đã có Roll Button**

```
1. Hierarchy → Find "ButtonRoll" or "RollButton"
2. Check Button component exists
3. Check Interactable = TRUE
4. Check Text = "ROLL DICE" or similar
```

---

### **Bước 3: Assign Roll Button vào GameManager**

```
1. Hierarchy → Select GameManager
2. Inspector → Game Manager (Script)
3. UI section → Roll Button: [None]
4. Drag ButtonRoll từ Hierarchy vào field
5. Verify: Roll Button: [ButtonRoll] ✅
```

---

### **Bước 4: Verify PanelRoll (Optional)**

**Nếu có PanelRoll:**
```
PanelRoll (GameObject)
├── Panel Roll (Script)
├── ImageDie1 (Image - dice 1)
├── ImageDie2 (Image - dice 2)
└── TextResult (TextMeshPro - "Total: 7")
```

**Nếu chưa có:**
- Có thể bỏ qua, game vẫn chạy
- Chỉ không có dice animation
- Console vẫn log dice result

---

## 🧪 **TESTING - STEP BY STEP**

### **Test 1: Verify Setup**

```
1. Select GameManager
2. Check Inspector:
   ✅ Demo Mode = TRUE
   ✅ Roll Button assigned
   ✅ Panel Game assigned
   ✅ Player Prefab Male assigned
   ✅ Board Manager assigned
```

---

### **Test 2: Play Mode - Basic**

```
1. Click Play
2. Check Console:
   ✅ "[GameManager] Starting game..."
   ✅ "[GameManager] Demo Mode: Starting game without network..."
   ✅ "[PlayerGameController] Initialized Player 1 (Male: True)"
   ✅ "[GameManager] Initialized PanelGame for Player 1"
   ✅ "[GameManager] Turn 1 - Player 1's turn"
   ✅ "[PlayerGameController] Turn indicator shown (Demo: True)"
   
3. Check Scene:
   ✅ Player 1 spawned at Tile 0
   ✅ Turn Indicator visible (yellow sphere)
   ✅ PanelMe shows player info
   ✅ Roll Button visible and enabled
```

---

### **Test 3: Roll Dice**

```
1. Play Mode
2. Click Roll Button
3. Check Console:
   ✅ "[Host] Player Player 1 rolled X + Y = Z"
   ✅ "[Demo] Dice result: X + Y = Z"
   
4. Check Scene:
   ✅ Roll Button disabled (grayed out)
   ✅ Player starts moving
   ✅ Bounce animation
   ✅ Player reaches target tile
   
5. Wait for movement to complete
6. Check Console:
   ✅ "[GameManager] Player 1 landed on tile Z"
   ✅ Tile action logs (buy property, event, etc.)
   
7. Wait 1 second
8. Check:
   ✅ Turn ends
   ✅ "[GameManager] Turn 2 - Player 1's turn"
   ✅ Roll Button enabled again
```

---

### **Test 4: Full Gameplay Loop**

```
1. Play Mode
2. Roll dice (Turn 1)
3. Player moves
4. Tile action (e.g., buy property)
5. Turn ends
6. Roll dice (Turn 2)
7. Player moves
8. Tile action
9. Repeat 5-10 turns
10. Check:
    ✅ No errors
    ✅ Player moves correctly
    ✅ Money updates
    ✅ Properties can be bought
    ✅ UI updates
```

---

### **Test 5: Tile Actions**

**Test different tiles:**

```
Tile 0 (Start):
  ✅ Player spawns here
  ✅ Passing gives +2000 money

Tile 1-26 (Properties):
  ✅ Can buy if unowned
  ✅ Pay rent if owned by others (N/A in demo)
  ✅ Can upgrade if owned

Tile 27-30 (Events):
  ✅ Event panel shows
  ✅ Event effect applies

Tile 31 (Quiz):
  ✅ Quiz panel shows
  ✅ Answer question
  ✅ Reward/penalty

Tile 32 (Jail):
  ✅ Player goes to jail
  ✅ Jail counter = 3
  ✅ Skip next 3 turns

Tile 33 (Travel):
  ✅ Player teleports to random tile
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: Roll Button không click được**

**Check:**
```
1. GameManager → Roll Button assigned?
   ❌ Not assigned → Assign ButtonRoll

2. Button → Interactable = TRUE?
   ❌ FALSE → Set TRUE

3. EventSystem exists in scene?
   ❌ Missing → Add EventSystem (GameObject → UI → Event System)

4. Canvas → Raycast Target?
   ✅ Button Image: Raycast Target = TRUE
   ❌ Other UI blocking → Set Raycast Target = FALSE
```

---

### **Problem 2: Player không di chuyển**

**Check Console:**
```
"[Host] Player Player 1 rolled X + Y = Z" ✅
"[Demo] Dice result: X + Y = Z" ✅
"[PlayerGameController] Moving X steps" ❌ MISSING

→ Check PlayerGameController.MoveBySteps()
→ Check BoardManager.GetWaypointPosition()
```

---

### **Problem 3: Tile action không trigger**

**Check Console:**
```
"[GameManager] Player 1 landed on tile Z" ✅
"[GameManager] Resolving tile Z" ❌ MISSING

→ Check GameManager.ResolveTile()
→ Check tile type detection
```

---

### **Problem 4: Turn không chuyển**

**Check:**
```
1. EndTurn() được gọi?
   ✅ Check Console: "[GameManager] Turn 2 - Player 1's turn"
   
2. Roll Button enabled lại?
   ✅ Check Button.interactable = TRUE
   
3. Turn counter tăng?
   ✅ Check turnText: "Turn: 2/25"
```

---

## ✅ **SUCCESS CRITERIA**

### **Phase 1 hoàn thành khi:**

- [x] Code đã sửa (RollAndMove, NotifyDiceRolledClientRpc)
- [ ] Roll Button assigned vào GameManager
- [ ] Play Mode: No errors
- [ ] Click Roll Button: Works
- [ ] Player moves: Bounce animation
- [ ] Tile actions: Trigger correctly
- [ ] Turn system: Works (1 → 2 → 3 → ...)
- [ ] UI updates: Money, turn, etc.
- [ ] Can play 10+ turns without errors

---

## 📝 **NEXT STEPS**

### **Sau khi Phase 1 hoàn thành:**

```
✅ Phase 1: Demo Mode works (30 phút)
  ↓
⏳ Phase 2: Review Multiplayer code (15 phút)
  ↓
⏳ Phase 3: Setup MenuScene (30 phút)
  ↓
⏳ Phase 4: Multiplayer Integration (1 giờ)
  ↓
⏳ Phase 5: ParrelSync Testing (30 phút)
  ↓
⏳ Phase 6: Matchmaking (30 phút)
  ↓
⏳ Phase 7: Polish (30 phút)
  ↓
✅ DONE: Multiplayer game hoàn chỉnh!
```

---

## 💡 **TIPS**

### **1. Console Logs - Quan trọng!**

**Luôn check Console để debug:**
```
✅ Green logs: Success
⚠️ Yellow logs: Warnings
❌ Red logs: Errors
```

### **2. Demo Mode Settings**

**Luôn verify:**
```
GameManager → Demo Mode: ✓ TRUE
```

**Nếu FALSE:**
- Game sẽ chờ network connection
- Roll Button không hoạt động
- Không spawn player

### **3. Step-by-step Testing**

**Đừng test tất cả cùng lúc:**
```
1. Test spawn player ✅
2. Test Roll Button ✅
3. Test movement ✅
4. Test tile actions ✅
5. Test full loop ✅
```

---

**Bạn sẵn sàng test chưa? Hãy làm theo từng bước trong TESTING section!** 🚀

