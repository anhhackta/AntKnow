# ✅ PANELNOTIFICATION FIX

**Sửa lỗi: "Coroutine couldn't be started because the game object 'PanelNotification' is inactive!"**

---

## 🐛 **VẤN ĐỀ**

### **Lỗi khi vào ô Jail (Accident):**

```
[GameManager] Player 1 landed on Accident (Type: Jail)
[PlayerGameController] Player 1 in jail for 2 turns
[GameManager] Jail tile - Player 1 in jail for 2 turns

❌ Coroutine couldn't be started because the the game object 'PanelNotification' is inactive!
```

### **Nguyên nhân:**

**PanelNotification GameObject bị inactive!**

**Code cũ:**
```csharp
public void ShowNotification(string message)
{
    textNotification.text = message;
    
    gameObject.SetActive(true); // ⚠️ Set active
    
    // ❌ Start coroutine - Nhưng nếu parent inactive → GameObject vẫn inactive!
    notificationCoroutine = StartCoroutine(NotificationCoroutine());
}
```

**Vấn đề:**
1. `gameObject.SetActive(true)` được gọi
2. Nhưng nếu **parent inactive** → `gameObject.activeInHierarchy` vẫn là **False**
3. StartCoroutine() fail vì GameObject không active!

---

## ✅ **GIẢI PHÁP**

### **Tự động activate parent + Check activeInHierarchy:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelNotification.cs" mode="EXCERPT">
````csharp
public void ShowNotification(string message)
{
    Debug.Log($"[PanelNotification] ShowNotification: {message}");
    
    textNotification.text = message;
    
    // ⭐ Check and activate parent if needed
    Transform parent = transform.parent;
    if (parent != null && !parent.gameObject.activeSelf)
    {
        Debug.LogWarning($"[PanelNotification] Parent '{parent.name}' is inactive! Activating parent...");
        parent.gameObject.SetActive(true);
    }
    
    // ⭐ Activate this GameObject
    gameObject.SetActive(true);
    
    Debug.Log($"[PanelNotification] Panel is now active: {gameObject.activeInHierarchy}");
    
    // ⭐ If still not active, log error and return
    if (!gameObject.activeInHierarchy)
    {
        Debug.LogError("[PanelNotification] Panel still not active! Cannot start coroutine!");
        Debug.LogError("[PanelNotification] Checking hierarchy...");
        Transform current = transform;
        while (current != null)
        {
            Debug.LogError($"  - {current.name}: activeSelf={current.gameObject.activeSelf}, activeInHierarchy={current.gameObject.activeInHierarchy}");
            current = current.parent;
        }
        return; // ⭐ Don't start coroutine if inactive
    }
    
    // Stop previous coroutine if running
    if (notificationCoroutine != null)
    {
        StopCoroutine(notificationCoroutine);
    }
    
    // Start new notification coroutine
    notificationCoroutine = StartCoroutine(NotificationCoroutine());
}
````
</augment_code_snippet>

---

## 📊 **WORKFLOW - BEFORE vs AFTER**

### **❌ BEFORE (Broken):**

```
ShowNotification("Player 1 bị giam 2 lượt!")
  ↓
Set textNotification.text
  ↓
gameObject.SetActive(true)
  ├── Parent inactive? → GameObject vẫn inactive!
  └── activeInHierarchy = False
  ↓
StartCoroutine(NotificationCoroutine()) ← ❌ FAIL!
  ↓
Error: "Coroutine couldn't be started..."
```

---

### **✅ AFTER (Fixed):**

```
ShowNotification("Player 1 bị giam 2 lượt!")
  ↓
Set textNotification.text
  ↓
Check parent active?
  ├── Parent inactive → Activate parent
  └── Parent active → OK
  ↓
gameObject.SetActive(true)
  ↓
Check activeInHierarchy?
  ├── True → ✅ Start coroutine
  └── False → ❌ Log error + Return (don't start coroutine)
  ↓
StartCoroutine(NotificationCoroutine())
  ↓
Wait 1 second
  ↓
Hide()
```

---

## 🎯 **PANELNOTIFICATION USE CASES**

### **Khi nào dùng PanelNotification?**

**Thông báo nhanh (1 giây) cho:**

1. **Ô Jail (Accident):**
   ```
   "Player 1 bị giam 2 lượt!"
   ```

2. **Ô Travel:**
   ```
   "Player 1 đi du lịch! -100"
   ```

3. **Ô Start:**
   ```
   "Player 1 đến Ô Bắt Đầu!"
   ```

4. **Pay Rent:**
   ```
   "Player 1 trả 500 cho Player 2"
   ```

5. **Turn Order:**
   ```
   "Player 1 đi thứ 1"
   ```

6. **Property Purchase (nếu không có PanelBuy):**
   ```
   "Player 1 mua Jakarta (600)"
   ```

7. **Skill Activated:**
   ```
   "Player 1 sử dụng skill: Double Dice"
   ```

8. **Bankruptcy:**
   ```
   "Player 1 đã phá sản!"
   ```

---

## 🧪 **TESTING**

### **Test 1: Ô Jail (Accident) - 5 phút**

```
1. Save all files
2. Return to Unity
3. Wait for compile
4. Play Mode
5. Roll dice đến tile 9 (Accident/Jail)
6. Check Console:
   
   Expected logs:
   ✅ "[GameManager] Player 1 landed on Accident (Type: Jail)"
   ✅ "[GameManager] Jail tile - Player 1 in jail for 2 turns"
   ✅ "[PanelNotification] ShowNotification: Player 1 bị giam 2 lượt!"
   
   If parent inactive:
   ⚠️ "[PanelNotification] Parent 'XXX' is inactive! Activating parent..."
   
   Then:
   ✅ "[PanelNotification] Panel is now active: True"
   
7. Expected behavior:
   ✅ PanelNotification hiện ra
   ✅ Text: "Player 1 bị giam 2 lượt!"
   ✅ Tự động đóng sau 1 giây
   ✅ Turn ends
   ✅ Can roll next turn (but skipped for 2 turns)
   
8. If error:
   ❌ "Panel is now active: False"
   → Check error logs (full hierarchy)
   → Screenshot và gửi cho tôi
```

---

### **Test 2: Ô Travel - 3 phút**

```
1. Play Mode
2. Roll dice đến tile 28 (Travel)
3. Expected:
   ✅ Console: "[PanelNotification] ShowNotification: Player 1 đi du lịch! -100"
   ✅ Console: "[PanelNotification] Panel is now active: True"
   ✅ PanelNotification hiện
   ✅ Text: "Player 1 đi du lịch! -100"
   ✅ Money -100
   ✅ Panel tự đóng sau 1 giây
   ✅ Turn ends
```

---

### **Test 3: Ô Start - 3 phút**

```
1. Play Mode
2. Roll dice để pass qua tile 0 (Start)
3. Expected:
   ✅ Console: "[PanelNotification] ShowNotification: Player 1 đến Ô Bắt Đầu!"
   ✅ PanelNotification hiện
   ✅ Money +2000 (pass Start bonus)
   ✅ Panel tự đóng sau 1 giây
```

---

### **Test 4: Pay Rent - 5 phút**

```
Cần 2 players để test (hoặc chờ Multiplayer):

1. Player 1 mua property
2. Player 2 roll đến property của Player 1
3. Expected:
   ✅ Console: "[PanelNotification] ShowNotification: Player 2 trả 500 cho Player 1"
   ✅ PanelNotification hiện
   ✅ Player 2 money giảm
   ✅ Player 1 money tăng
   ✅ Panel tự đóng sau 1 giây
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: Vẫn lỗi "Coroutine couldn't be started"**

**Check Console:**
```
Có log "[PanelNotification] Panel is now active: True/False"?
├── True → ✅ Should work
└── False → ❌ Check error logs
```

**If "False":**
```
Check error logs:
"  - PanelNotification: activeSelf=True, activeInHierarchy=False"
"  - PanelContainer: activeSelf=False, activeInHierarchy=False" ← ⚠️ PROBLEM
"  - Canvas: activeSelf=True, activeInHierarchy=True"

→ PanelContainer is inactive!
→ Need to activate PanelContainer manually in Unity
```

---

### **Problem 2: Panel không hiển thị**

**Check:**
```
1. Console: "Panel is now active: True"?
   ├── Yes → Panel active nhưng không visible
   │   └── Check:
   │       ├── Position on screen?
   │       ├── Canvas Renderer enabled?
   │       ├── Image alpha > 0?
   │       └── Z-order (not behind other panels)?
   └── No → Panel not active
       └── Check error logs
```

---

### **Problem 3: Panel hiện nhưng không tự đóng**

**Check:**
```
1. Console: Có error trong NotificationCoroutine()?
2. Display Duration setting:
   PanelNotification → Inspector → Display Duration: 1
   
3. If too fast/slow:
   → Adjust Display Duration (0.5 - 3 seconds)
```

---

## ✅ **CHECKLIST**

### **Code:**
- [x] PanelNotification.ShowNotification() - Auto activate parent
- [x] PanelNotification.ShowNotification() - Check activeInHierarchy
- [x] PanelNotification.ShowNotification() - Log error if inactive
- [x] PanelNotification.ShowNotification() - Return if inactive (don't start coroutine)
- [x] PanelNotification.ShowNotification() - Debug logs

### **Unity Setup:**
- [ ] PanelNotification exists in scene
- [ ] PanelNotification assigned to GameManager
- [ ] PanelNotification → Display Duration: 1
- [ ] PanelNotification → Text Notification assigned
- [ ] Parent Canvas active

### **Testing:**
- [ ] Compile successful
- [ ] Ô Jail → Notification hiện
- [ ] Ô Travel → Notification hiện
- [ ] Ô Start → Notification hiện
- [ ] Pay Rent → Notification hiện
- [ ] Panel tự đóng sau 1 giây
- [ ] No coroutine errors

---

## 📝 **SUMMARY**

### **Vấn đề:**
- ❌ Coroutine couldn't be started (PanelNotification inactive)
- ❌ Parent GameObject inactive

### **Giải pháp:**
- ✅ Tự động activate parent nếu inactive
- ✅ Check activeInHierarchy trước khi start coroutine
- ✅ Log error và return nếu vẫn inactive
- ✅ Debug logs để track issue

### **PanelNotification use cases:**
- ✅ Ô Jail: "Player 1 bị giam 2 lượt!"
- ✅ Ô Travel: "Player 1 đi du lịch! -100"
- ✅ Ô Start: "Player 1 đến Ô Bắt Đầu!"
- ✅ Pay Rent: "Player 1 trả 500 cho Player 2"
- ✅ Turn Order: "Player 1 đi thứ 1"
- ✅ Skill: "Player 1 sử dụng skill: Double Dice"
- ✅ Bankruptcy: "Player 1 đã phá sản!"

### **Cần làm:**
- [ ] Save all files
- [ ] Return to Unity
- [ ] Wait for compile
- [ ] Play Mode
- [ ] Roll đến ô Jail (tile 9)
- [ ] Verify notification hiện
- [ ] Test các ô khác (Travel, Start)
- [ ] Báo kết quả!

---

## 🎯 **NEXT STEP**

1. **Save all files** (Ctrl+S)
2. **Return to Unity**
3. **Wait for compile**
4. **Play Mode**
5. **Roll đến tile 9 (Accident/Jail)**
6. **Check Console:**
   ```
   Expected:
   ✅ "[PanelNotification] ShowNotification: Player 1 bị giam 2 lượt!"
   ✅ "[PanelNotification] Panel is now active: True"
   ```
7. **Expected behavior:**
   - ✅ PanelNotification hiện
   - ✅ Text: "Player 1 bị giam 2 lượt!"
   - ✅ Tự đóng sau 1 giây
   - ✅ No coroutine error
8. **Test các ô khác:**
   - Travel (tile 28)
   - Start (tile 0)
9. **Báo kết quả!**

---

**DONE! PanelNotification đã được sửa!** 🎉

**Giống như PanelBuy:**
- ✅ Tự động activate parent
- ✅ Check activeInHierarchy
- ✅ Log error nếu failed
- ✅ Không start coroutine nếu inactive

