# 🐛 FIXES: NullReferenceException & Button Roll

**Sửa 2 vấn đề khi test Demo Mode**

---

## 🐛 **VẤN ĐỀ 1: NullReferenceException**

### **Lỗi:**
```
NullReferenceException: Object reference not set to an instance of an object
AntKnow.Game.PlayerGameController+<MoveBySteps>d__60.MoveNext ()
(at Assets/Scenes/Game/Scripts/Player/PlayerGameController.cs:271)
```

### **Nguyên nhân:**

**Line 271:**
```csharp
int targetTile = (currentTile + steps) % boardManager.TotalTiles;
                                        ^^^^^^^^^^^^
                                        NULL!
```

**Tại sao boardManager null?**

1. **Multiplayer Mode:**
   - Player spawn → NetworkObject.Spawn()
   - OnNetworkSpawn() được gọi
   - SetupComponents() → boardManager = FindObjectOfType<BoardManager>()
   - ✅ boardManager được assign

2. **Demo Mode:**
   - Player spawn → Instantiate() (không có network)
   - OnNetworkSpawn() KHÔNG được gọi ❌
   - SetupComponents() KHÔNG chạy ❌
   - boardManager = null ❌

### **Giải pháp:**

**Tạo method SetupComponents() và gọi từ cả 2 chỗ:**

1. **OnNetworkSpawn()** - Multiplayer
2. **Initialize()** - Demo Mode

---

## ✅ **FIX 1: PlayerGameController.cs**

### **Tạo SetupComponents() method:**

```csharp
/// <summary>
/// Setup components - Called by OnNetworkSpawn (Multiplayer) or Initialize (Demo Mode)
/// </summary>
private void SetupComponents()
{
    // Setup BoardManager
    if (boardManager == null)
    {
        boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("[PlayerGameController] BoardManager not found in scene!");
        }
    }

    // Auto-find animator
    if (animator == null)
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Setup turn indicator
    if (turnIndicator == null)
    {
        turnIndicator = GetComponentInChildren<TurnIndicator>();
        if (turnIndicator == null)
        {
            // Create turn indicator...
        }
    }
    
    Debug.Log($"[PlayerGameController] Components setup complete (BoardManager: {boardManager != null})");
}
```

### **Gọi từ OnNetworkSpawn():**

```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    
    // Setup components (Multiplayer)
    SetupComponents();

    Debug.Log($"[PlayerGameController] Spawned: {playerName}");
}
```

### **Gọi từ Initialize():**

```csharp
public void Initialize(string name, string id, bool male, int hp, int agi, int intel, int lck, int res)
{
    // ... player data setup ...
    
    // ⭐ DEMO MODE: Setup components (OnNetworkSpawn không được gọi)
    SetupComponents();
    
    Debug.Log($"[PlayerGameController] Initialized {name}");
}
```

---

## 🐛 **VẤN ĐỀ 2: Button Roll mờ nhưng vẫn click được**

### **Nguyên nhân:**

**Có 2 buttons:**

1. **rollButton** (GameManager.rollButton)
   - Được assign trong Inspector
   - GameManager enable/disable button này
   - ✅ Hoạt động đúng

2. **btnRoll** (PanelRoll.btnRoll)
   - Nếu PanelRoll được assign
   - PanelRoll.SetRollButtonEnabled() control button này
   - ❌ KHÔNG được enable/disable → Hiển thị mờ

**Kết quả:**
- rollButton: interactable = TRUE → Click được
- btnRoll (PanelRoll): interactable = FALSE → Hiển thị mờ
- User thấy button mờ nhưng vẫn click được!

### **Giải pháp:**

**Sync cả 2 buttons:**
- Khi enable rollButton → cũng enable btnRoll (PanelRoll)
- Khi disable rollButton → cũng disable btnRoll (PanelRoll)

---

## ✅ **FIX 2: GameManager.cs**

### **StartTurn() - Enable cả 2 buttons:**

```csharp
private void StartTurn()
{
    // ... other code ...
    
    // Enable roll button
    if (rollButton != null)
    {
        rollButton.interactable = true;
    }
    
    // ⭐ Enable PanelRoll button (if exists)
    if (panelRoll != null)
    {
        panelRoll.SetRollButtonEnabled(true);
    }
}
```

### **OnRollButtonClicked() - Disable cả 2 buttons:**

```csharp
private void OnRollButtonClicked()
{
    // ... other code ...
    
    // Disable button
    if (rollButton != null)
    {
        rollButton.interactable = false;
    }
    
    // ⭐ Disable PanelRoll button (if exists)
    if (panelRoll != null)
    {
        panelRoll.SetRollButtonEnabled(false);
    }
    
    // Roll dice
    StartCoroutine(RollAndMove());
}
```

---

## 📊 **WORKFLOW SAU KHI SỬA**

### **Demo Mode:**

```
GameManager.StartGame()
  ↓
SpawnTestPlayer("Player 1")
  ↓
player.Initialize(name, stats)
  ├── Set player data
  └── SetupComponents() ← ⭐ MỚI THÊM
      ├── boardManager = FindObjectOfType<BoardManager>() ✅
      ├── animator = GetComponentInChildren<Animator>() ✅
      └── turnIndicator setup ✅
  ↓
StartTurn()
  ├── rollButton.interactable = TRUE ✅
  └── panelRoll.SetRollButtonEnabled(TRUE) ✅
  ↓
User clicks Roll Button
  ↓
OnRollButtonClicked()
  ├── rollButton.interactable = FALSE ✅
  └── panelRoll.SetRollButtonEnabled(FALSE) ✅
  ↓
RollAndMove()
  ↓
player.MoveBySteps(steps)
  ├── boardManager.TotalTiles ✅ (không null!)
  └── Movement successful ✅
```

---

## 🧪 **TESTING**

### **Test 1: Compile (1 phút)**

```
1. Save all files (Ctrl+S)
2. Return to Unity
3. Wait for compile
4. Check Console:
   ✅ No errors
   ❌ Errors → Check error message
```

---

### **Test 2: BoardManager Setup (2 phút)**

```
1. Play Mode
2. Check Console:
   ✅ "[PlayerGameController] Components setup complete (BoardManager: True)"
   ❌ "BoardManager: False" → BoardManager not in scene!
   
3. If BoardManager: False:
   - Check Hierarchy → BoardManager exists?
   - Check BoardManager has BoardManager script?
```

---

### **Test 3: Roll Dice (3 phút)**

```
1. Play Mode
2. Check Roll Button:
   ✅ Button NOT grayed out (enabled)
   ✅ Button color normal (not dimmed)
   
3. Click Roll Button
4. Expected:
   ✅ Console: "[Host] Player Player 1 rolled X + Y = Z"
   ✅ Console: "[Demo] Dice result: X + Y = Z"
   ✅ Button grayed out (disabled)
   ✅ Player moves (NO NullReferenceException!)
   ✅ Player reaches target tile
   ✅ Turn ends
   ✅ Button enabled again (not grayed out)
```

---

### **Test 4: Multiple Turns (5 phút)**

```
1. Play Mode
2. Roll dice 5-10 times
3. Check:
   ✅ No NullReferenceException
   ✅ Button visual state correct (enabled/disabled)
   ✅ Player moves correctly each time
   ✅ No errors
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: Vẫn NullReferenceException**

**Check Console:**
```
"[PlayerGameController] Components setup complete (BoardManager: False)"
```

**Solutions:**
```
1. Check Hierarchy → BoardManager exists?
   ❌ Missing → Create BoardManager GameObject
   
2. Check BoardManager has script?
   ❌ Missing → Add BoardManager script
   
3. Check BoardManager.TotalTiles?
   ❌ 0 → Setup tiles/waypoints
```

---

### **Problem 2: Button vẫn mờ**

**Check:**
```
1. PanelRoll assigned vào GameManager?
   ✅ Assigned → Good
   ❌ Not assigned → Button mờ là bình thường (PanelRoll control)
   
2. Nếu không dùng PanelRoll:
   - Không assign PanelRoll vào GameManager
   - Chỉ dùng rollButton
   - Button sẽ không mờ
```

---

### **Problem 3: Click không hoạt động**

**Check:**
```
1. EventSystem exists?
   ❌ Missing → Add EventSystem
   
2. Button.onClick listener?
   ✅ Check Console: "[GameManager] Starting game..."
   → Listener được add trong StartGame()
   
3. Button Raycast Target?
   ✅ Button Image: Raycast Target = TRUE
```

---

## ✅ **CHECKLIST**

### **Code:**
- [x] PlayerGameController.cs - Tạo SetupComponents()
- [x] PlayerGameController.cs - Gọi từ OnNetworkSpawn()
- [x] PlayerGameController.cs - Gọi từ Initialize()
- [x] GameManager.cs - Enable PanelRoll button (StartTurn)
- [x] GameManager.cs - Disable PanelRoll button (OnRollButtonClicked)

### **Unity Setup:**
- [ ] BoardManager exists in scene
- [ ] BoardManager has script
- [ ] BoardManager setup (tiles/waypoints)
- [ ] GameManager → Roll Button assigned
- [ ] GameManager → Demo Mode = TRUE
- [ ] (Optional) GameManager → Panel Roll assigned

### **Testing:**
- [ ] Compile successful
- [ ] Play Mode - No errors
- [ ] Console: "Components setup complete (BoardManager: True)"
- [ ] Roll Button NOT grayed out
- [ ] Click Roll Button - Works
- [ ] Player moves - NO NullReferenceException
- [ ] Button visual state correct
- [ ] Can play 5-10 turns

---

## 📝 **SUMMARY**

### **Vấn đề 1: NullReferenceException**

**Nguyên nhân:**
- Demo Mode không gọi OnNetworkSpawn()
- boardManager không được setup
- MoveBySteps() crash khi access boardManager.TotalTiles

**Giải pháp:**
- Tạo SetupComponents() method
- Gọi từ cả OnNetworkSpawn() (Multiplayer) và Initialize() (Demo Mode)
- boardManager được setup cho cả 2 modes

---

### **Vấn đề 2: Button mờ nhưng click được**

**Nguyên nhân:**
- Có 2 buttons: rollButton và btnRoll (PanelRoll)
- rollButton enabled → Click được
- btnRoll disabled → Hiển thị mờ

**Giải pháp:**
- Sync cả 2 buttons
- Enable/disable cả rollButton và PanelRoll.btnRoll
- Visual state đồng bộ với interactable state

---

## 🎯 **NEXT STEP**

**Sau khi sửa:**

1. Save all files
2. Return to Unity
3. Wait for compile
4. Play Mode
5. Test Roll Dice
6. Verify:
   - ✅ No NullReferenceException
   - ✅ Button visual correct
   - ✅ Player moves
   - ✅ Game works!

**Nếu hoạt động:**
- ✅ Phase 1 Task 1.2 DONE
- ⏳ Tiếp tục Phase 1 Task 1.3 (Fix bugs)

---

**DONE! Cả 2 vấn đề đã được sửa! 🎉**

