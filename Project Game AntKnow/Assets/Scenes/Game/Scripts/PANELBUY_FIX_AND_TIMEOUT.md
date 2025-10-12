# ✅ PANELBUY FIX + TIMEOUT SYSTEM

**Sửa vấn đề PanelBuy không hiện + Thêm timeout 10 giây**

---

## 🐛 **VẤN ĐỀ: PANELBUY KHÔNG HIỆN**

### **Console logs:**
```
✅ "[GameManager] Showing PanelBuy for Taipei"
✅ "[PanelBuy] ShowBuy called: Taipei, Price: 650, Money: 10000"
✅ "[PanelBuy] Setting active to TRUE"
❌ "[PanelBuy] Panel is now active: False"
```

### **Nguyên nhân:**

**Parent GameObject bị inactive!**

`gameObject.SetActive(true)` được gọi, nhưng `gameObject.activeInHierarchy` vẫn là **False** vì parent bị inactive.

---

## ✅ **GIẢI PHÁP 1: TỰ ĐỘNG ACTIVATE PARENT**

### **Code đã thêm:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
public void ShowBuy(...)
{
    // ... setup code ...
    
    // ⭐ Check parent active
    Transform parent = transform.parent;
    if (parent != null && !parent.gameObject.activeSelf)
    {
        Debug.LogWarning($"[PanelBuy] Parent '{parent.name}' is inactive! Activating parent...");
        parent.gameObject.SetActive(true);
    }
    
    gameObject.SetActive(true);
    
    Debug.Log($"[PanelBuy] Panel is now active: {gameObject.activeInHierarchy}");
    
    // ⭐ If still not active, log full hierarchy
    if (!gameObject.activeInHierarchy)
    {
        Debug.LogError("[PanelBuy] Panel still not active! Checking hierarchy...");
        Transform current = transform;
        while (current != null)
        {
            Debug.LogError($"  - {current.name}: activeSelf={current.gameObject.activeSelf}, activeInHierarchy={current.gameObject.activeInHierarchy}");
            current = current.parent;
        }
    }
}
````
</augment_code_snippet>

### **Cách hoạt động:**

```
ShowBuy() called
  ↓
Check parent active?
  ├── Parent inactive → Activate parent
  └── Parent active → OK
  ↓
SetActive(true) on PanelBuy
  ↓
Check activeInHierarchy
  ├── True → ✅ Panel hiện
  └── False → ❌ Log full hierarchy để debug
```

---

## ✅ **GIẢI PHÁP 2: TIMEOUT 10 GIÂY**

### **Yêu cầu của bạn:**

> "Nếu vào ô đất trống hiện ra PanelBuy có thể mua hoặc skip hay trong vòng 10 giây không mua gì thì nhận định button skip"

### **Code đã thêm:**

**1. Timeout settings:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
[Header("Timeout Settings")]
[SerializeField] private float autoSkipTimeout = 10f; // 10 seconds
[SerializeField] private TextMeshProUGUI textTimer; // Optional timer display

private Coroutine timeoutCoroutine;
````
</augment_code_snippet>

**2. Start timeout khi show panel:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
public void ShowBuy(...)
{
    // ... setup code ...
    
    // ⭐ Start timeout coroutine
    if (timeoutCoroutine != null)
    {
        StopCoroutine(timeoutCoroutine);
    }
    timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
}
````
</augment_code_snippet>

**3. Timeout coroutine:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
private System.Collections.IEnumerator TimeoutCoroutine()
{
    float remainingTime = autoSkipTimeout;
    
    while (remainingTime > 0)
    {
        // Update timer display (if exists)
        if (textTimer != null)
        {
            textTimer.text = $"Thời gian: {Mathf.CeilToInt(remainingTime)}s";
        }
        
        yield return new WaitForSeconds(1f);
        remainingTime -= 1f;
    }
    
    // Timeout - Auto skip
    Debug.Log("[PanelBuy] Timeout! Auto skipping...");
    
    if (textTimer != null)
    {
        textTimer.text = "Hết giờ!";
    }
    
    yield return new WaitForSeconds(0.5f);
    
    // Auto skip
    onSkipCallback?.Invoke();
    Hide();
}
````
</augment_code_snippet>

**4. Stop timeout khi user chọn:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
private void OnBuyClicked()
{
    // ⭐ Stop timeout
    if (timeoutCoroutine != null)
    {
        StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = null;
    }
    
    onBuyCallback?.Invoke(selectedLevel);
    Hide();
}

private void OnSkipClicked()
{
    // ⭐ Stop timeout
    if (timeoutCoroutine != null)
    {
        StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = null;
    }
    
    onSkipCallback?.Invoke();
    Hide();
}
````
</augment_code_snippet>

---

## 📊 **TIMEOUT WORKFLOW**

```
PanelBuy.ShowBuy()
  ↓
Start TimeoutCoroutine()
  ↓
Count down: 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
  ├── User clicks "MUA" → Stop timeout → Buy
  ├── User clicks "BỎ QUA" → Stop timeout → Skip
  └── Timeout (0s) → Auto skip
  ↓
onSkipCallback?.Invoke()
  ↓
Hide panel
  ↓
EndTurn()
```

---

## 🎮 **SKILL CARDS - PHASE 2**

### **Câu hỏi của bạn:**

> "Đến các phần như skill card rồi kết thúc lượt hay do demo mode mà không có tính năng này phải triển khai online phase2?"

### **Trả lời:**

**Skill cards KHÔNG liên quan đến PanelBuy timeout!**

**Workflow đúng:**

```
Roll Dice
  ↓
Move Player
  ↓
ResolveTile()
  ├── Property → PanelBuy (10s timeout)
  ├── Event → PanelEvent
  ├── Quiz → PanelQuiz
  └── Other → Notification
  ↓
Panel closes (user choice or timeout)
  ↓
⭐ SKILL CARDS PHASE (chưa implement)
  ├── Player có thể dùng skill cards
  ├── Timeout 5-10 giây
  └── Hoặc click "Kết thúc lượt"
  ↓
EndTurn()
  ↓
Next player
```

**Skill cards phase:**
- ✅ Có thể implement trong Demo Mode
- ✅ Không cần Phase 2 (Multiplayer)
- ⏳ Chưa được implement (TODO)

**Nếu muốn thêm skill cards phase:**
1. Sau khi panel đóng
2. Hiện PanelSkillCards (hoặc button "Dùng Skill")
3. Timeout 10 giây
4. User chọn skill hoặc timeout → EndTurn()

---

## 🧪 **TESTING**

### **Test 1: Parent Activation (3 phút)**

```
1. Save all files
2. Return to Unity
3. Wait for compile
4. Play Mode
5. Roll dice đến property tile
6. Check Console:
   
   Expected logs:
   ✅ "[PanelBuy] ShowBuy called: Taipei, Price: 650"
   ✅ "[PanelBuy] Setting active to TRUE"
   
   If parent inactive:
   ⚠️ "[PanelBuy] Parent 'XXX' is inactive! Activating parent..."
   
   Then:
   ✅ "[PanelBuy] Panel is now active: True" ← ⭐ QUAN TRỌNG
   
7. If still "False":
   → Check error logs showing full hierarchy
   → Screenshot và gửi cho tôi
```

---

### **Test 2: Timeout System (15 giây)**

```
1. Play Mode
2. Roll dice đến property tile
3. PanelBuy hiện ra
4. Expected:
   ✅ Timer hiển thị: "Thời gian: 10s"
   ✅ Countdown: 9s, 8s, 7s...
   
5. Test Case A: User clicks "MUA" (5 giây)
   → Timer stops
   → Property bought
   → Panel closes
   → Turn ends
   
6. Test Case B: User clicks "BỎ QUA" (5 giây)
   → Timer stops
   → Property skipped
   → Panel closes
   → Turn ends
   
7. Test Case C: Timeout (chờ 10 giây)
   → Timer: 3s, 2s, 1s, 0s
   → Console: "[PanelBuy] Timeout! Auto skipping..."
   → Timer: "Hết giờ!"
   → Auto skip
   → Panel closes
   → Turn ends
```

---

### **Test 3: Multiple Turns với Timeout (30 giây)**

```
1. Play Mode
2. Turn 1:
   → Roll → Property → Wait timeout → Auto skip
   → Turn ends
   
3. Turn 2:
   → Roll → Property → Click "MUA"
   → Turn ends
   
4. Turn 3:
   → Roll → Property → Click "BỎ QUA"
   → Turn ends
   
5. Turn 4:
   → Roll → Event → Auto close
   → Turn ends
   
6. Verify:
   ✅ Timeout works mỗi lần
   ✅ Can roll multiple times
   ✅ No stuck state
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: Panel vẫn không hiện**

**Check Console:**
```
Có log "[PanelBuy] Parent 'XXX' is inactive! Activating parent..."?
├── YES → Parent được activate
│   └── Check "Panel is now active: True/False"?
│       ├── True → ✅ Panel should be visible
│       └── False → Check error logs (full hierarchy)
└── NO → Parent đã active
    └── Check "Panel is now active: True/False"?
        ├── True → ✅ Panel should be visible
        └── False → Check error logs
```

**If error logs show hierarchy:**
```
Example:
"  - PanelBuy: activeSelf=True, activeInHierarchy=False"
"  - PanelContainer: activeSelf=False, activeInHierarchy=False" ← ⚠️ PROBLEM
"  - Canvas: activeSelf=True, activeInHierarchy=True"

→ PanelContainer is inactive!
→ Need to activate PanelContainer too
```

---

### **Problem 2: Timer không hiển thị**

**Check:**
```
1. PanelBuy → Inspector → Text Timer assigned?
   ├── YES → Check TextMeshProUGUI component
   └── NO → Assign TextTimer (optional)
   
2. If not assigned:
   → Timer vẫn hoạt động (countdown in background)
   → Chỉ không hiển thị text
   → Timeout vẫn work sau 10 giây
```

---

### **Problem 3: Timeout không hoạt động**

**Check Console:**
```
Expected after 10 seconds:
✅ "[PanelBuy] Timeout! Auto skipping..."

If missing:
→ TimeoutCoroutine() không chạy
→ Check StartCoroutine() được gọi
→ Check coroutine không bị stop sớm
```

---

### **Problem 4: Timeout quá nhanh/chậm**

**Adjust timeout:**
```
PanelBuy → Inspector → Auto Skip Timeout: 10

Change to:
- 5 seconds: Nhanh hơn
- 15 seconds: Chậm hơn
- 30 seconds: Rất chậm
```

---

## ✅ **CHECKLIST**

### **Code:**
- [x] PanelBuy.ShowBuy() - Auto activate parent
- [x] PanelBuy.ShowBuy() - Log full hierarchy if failed
- [x] PanelBuy - Timeout settings (10s)
- [x] PanelBuy - TimeoutCoroutine()
- [x] PanelBuy - Stop timeout on buy/skip
- [x] PanelBuy - Timer display (optional)

### **Unity Setup:**
- [ ] PanelBuy exists in scene
- [ ] PanelBuy assigned to GameManager
- [ ] PanelBuy → Auto Skip Timeout: 10
- [ ] PanelBuy → Text Timer: [Optional]
- [ ] Parent Canvas active

### **Testing:**
- [ ] Compile successful
- [ ] PanelBuy hiện ra (parent activated)
- [ ] Timer countdown 10s
- [ ] Click "MUA" → Timer stops
- [ ] Click "BỎ QUA" → Timer stops
- [ ] Timeout → Auto skip
- [ ] Can roll multiple times

---

## 📝 **SUMMARY**

### **Vấn đề 1: PanelBuy không hiện**
- ✅ Tự động activate parent nếu inactive
- ✅ Log full hierarchy nếu vẫn không active
- ⏳ Chờ test để verify

### **Vấn đề 2: Timeout 10 giây**
- ✅ Thêm timeout coroutine
- ✅ Countdown 10 → 0
- ✅ Auto skip khi timeout
- ✅ Stop timeout khi user chọn
- ✅ Optional timer display

### **Skill Cards Phase:**
- ⏳ Chưa implement
- ✅ Có thể làm trong Demo Mode
- ✅ Không cần Phase 2
- 💡 Workflow: Panel closes → Skill phase → EndTurn()

### **Cần làm:**
- [ ] Save all files
- [ ] Return to Unity
- [ ] Wait for compile
- [ ] Play Mode và test
- [ ] Verify PanelBuy hiện
- [ ] Verify timeout works
- [ ] Test multiple turns
- [ ] Báo kết quả!

---

## 🎯 **NEXT STEP**

1. **Save all files** (Ctrl+S)
2. **Return to Unity**
3. **Wait for compile**
4. **Play Mode**
5. **Roll đến property tile**
6. **Check Console:**
   - "Panel is now active: True"? ← ⭐ QUAN TRỌNG
   - Timer countdown?
7. **Test timeout:**
   - Chờ 10 giây → Auto skip
8. **Test user choice:**
   - Click "MUA" → Timer stops
   - Click "BỎ QUA" → Timer stops
9. **Báo kết quả!**

---

**DONE! PanelBuy fix + Timeout system hoàn chỉnh!** 🎉

**Về Skill Cards:**
- Đây là feature riêng biệt
- Không liên quan đến PanelBuy timeout
- Có thể implement sau (Phase 1 hoặc Phase 2)
- Workflow: ResolveTile → Panel → **Skill Phase** → EndTurn

