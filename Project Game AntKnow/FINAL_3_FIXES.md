# ✅ FINAL 3 FIXES - PANEL, TIMER, DEBUG

## 🔥 ĐÃ FIX 3 VẤN ĐỀ

### **1. PanelNotification - Ẩn/hiện CẢ PANEL** ✅

**Yêu cầu:** Ẩn/hiện cả GameObject panel, không phải chỉ Text

**Fix:**

```csharp
// PanelNotification.cs

private void Awake()
{
    // Ẩn panel ban đầu
    gameObject.SetActive(false);
}

public void ShowNotification(string message, float duration = -1f)
{
    // Hiện CẢ PANEL
    gameObject.SetActive(true);

    // Set text
    if (notificationText != null)
    {
        notificationText.text = message;
    }

    // Auto hide sau duration
    autoHideCoroutine = StartCoroutine(AutoHideCoroutine(duration));
}

public void HideNotification()
{
    // Ẩn CẢ PANEL
    gameObject.SetActive(false);
}
```

**Kết quả:**
- ✅ Panel ẩn ban đầu
- ✅ Hiện panel khi show notification
- ✅ Ẩn panel sau 2s (auto hide)

---

### **2. ButtonWaitGame - Text format "MM:SS"** ✅

**Yêu cầu:** Text hiển thị "00:30", "00:29", "00:28"... thay vì "Đang tìm... 30s"

**Fix:**

```csharp
// PanelHome.cs - OnSearchTimeUpdated()

private void OnSearchTimeUpdated(float remainingTime)
{
    if (textWaitTimer != null)
    {
        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        textWaitTimer.text = $"{minutes:00}:{seconds:00}";
    }
}
```

**Kết quả:**
- ✅ Text: "00:30" → "00:29" → "00:28" → ... → "00:01" → "00:00"
- ✅ Format MM:SS

---

### **3. PanelCustomRoom - Debug để tìm lỗi** ✅

**Vấn đề:** `panelCustomRoom.activeSelf: True` nhưng KHÔNG HIỆN

**Fix:** Thêm debug logs chi tiết

```csharp
// LobbyUIManager.cs - ShowPanelRoom()

private void ShowPanelRoom()
{
    DebugLog("=== ShowPanelRoom START ===");
    
    if (panelRoom == null)
    {
        DebugLogError("panelRoom is NULL!");
        return;
    }
    
    panelRoom.SetActive(true);
    if (panelCreateRoom != null) panelCreateRoom.SetActive(false);
    if (panelJoinRoom != null) panelJoinRoom.SetActive(false);
    
    DebugLog($"panelRoom.activeSelf: {panelRoom.activeSelf}");
    DebugLog($"panelRoom.activeInHierarchy: {panelRoom.activeInHierarchy}");
    
    // Check parent
    Transform parent = panelRoom.transform.parent;
    if (parent != null)
    {
        DebugLog($"panelRoom parent: {parent.name}, active: {parent.gameObject.activeSelf}");
    }
    
    DebugLog("=== ShowPanelRoom END ===");
}
```

**Check trong Console:**
```
[LobbyUIManager] === ShowPanelRoom START ===
[LobbyUIManager] panelRoom.activeSelf: True
[LobbyUIManager] panelRoom.activeInHierarchy: ??? ← CHECK NÀY
[LobbyUIManager] panelRoom parent: ???, active: ??? ← CHECK NÀY
[LobbyUIManager] === ShowPanelRoom END ===
```

**Nếu `activeInHierarchy: False`:**
→ **Parent panel bị ẩn!** Cần check:
1. PanelContainer active?
2. PanelCustomRoom active?
3. Canvas active?

---

## 🧪 TEST

### **Test 1: PanelNotification**
```
1. Play MenuScene
2. Click "Tìm trận"
3. ✅ PanelNotification KHÔNG hiện (vì tạo lobby mới)
4. Wait 30s
5. ✅ PanelNotification HIỆN: "Match Found"
6. Wait 2s
7. ✅ PanelNotification ẨN
8. ✅ Load GameScene
```

---

### **Test 2: ButtonWaitGame Timer**
```
1. Click "Tìm trận"
2. ✅ ButtonWaitGame hiện
3. ✅ Text: "00:30"
4. Wait 1s
5. ✅ Text: "00:29"
6. Wait 1s
7. ✅ Text: "00:28"
8. ...
9. ✅ Text: "00:01"
10. ✅ Text: "00:00"
11. ✅ Auto start game
```

---

### **Test 3: PanelCustomRoom Debug**
```
1. Click "Tạo phòng"
2. Check Console:

✅ Expected (GOOD):
[LobbyUIManager] === ShowPanelRoom START ===
[LobbyUIManager] panelRoom.activeSelf: True
[LobbyUIManager] panelRoom.activeInHierarchy: True ← GOOD!
[LobbyUIManager] panelRoom parent: PanelContainer, active: True ← GOOD!
[LobbyUIManager] === ShowPanelRoom END ===

❌ If activeInHierarchy: False (BAD):
[LobbyUIManager] panelRoom.activeInHierarchy: False ← BAD!
[LobbyUIManager] panelRoom parent: PanelContainer, active: False ← PARENT BỊ ẨN!

→ FIX: Check PanelContainer, PanelCustomRoom, Canvas
```

---

## 🔍 TROUBLESHOOTING - PANELCUSTOMROOM

### **Scenario 1: activeInHierarchy = False**

**Nguyên nhân:** Parent panel bị ẩn

**Fix:**
```
1. Unity Editor → Hierarchy
2. Find PanelCustomRoom
3. Check:
   - PanelCustomRoom active? ← Should be TRUE
   - PanelContainer active? ← Should be TRUE
   - Canvas active? ← Should be TRUE
4. If any FALSE → Set TRUE
```

---

### **Scenario 2: activeInHierarchy = True nhưng vẫn không hiện**

**Nguyên nhân:** RectTransform position sai hoặc bị che

**Fix:**
```
1. Unity Editor → Hierarchy
2. Select PanelCustomRoom
3. Inspector → RectTransform:
   - Anchors: Stretch (full screen)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
   - Scale: (1, 1, 1)
4. Check Z-order:
   - PanelCustomRoom phải ở TRÊN các panels khác
   - Hierarchy: Drag PanelCustomRoom xuống cuối (render last = on top)
```

---

### **Scenario 3: Canvas Scaler issue**

**Nguyên nhân:** Canvas Scaler không match resolution

**Fix:**
```
1. Select Canvas
2. Inspector → Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: 0.5
3. Play → Check lại
```

---

## 📁 FILES MODIFIED

1. ✅ **PanelNotification.cs** - Ẩn/hiện cả panel
2. ✅ **PanelHome.cs** - Timer format MM:SS
3. ✅ **LobbyUIManager.cs** - Debug logs

---

## 🎯 SUMMARY

**PanelNotification:**
- ✅ Ẩn/hiện cả GameObject panel
- ✅ Auto hide sau 2s

**ButtonWaitGame:**
- ✅ Text format: "00:30" → "00:29" → ... → "00:00"

**PanelCustomRoom:**
- ✅ Debug logs chi tiết
- ✅ Check activeInHierarchy
- ✅ Check parent active
- ✅ Troubleshooting guide

---

## 🚨 NEXT STEPS

### **1. Test PanelNotification**
```
Click "Tìm trận" → Wait 30s → Check "Match Found" hiện → Ẩn sau 2s
```

### **2. Test Timer**
```
Click "Tìm trận" → Check text "00:30" → "00:29" → ...
```

### **3. Debug PanelCustomRoom**
```
Click "Tạo phòng" → Check Console logs:
- activeInHierarchy: True? ← GOOD
- activeInHierarchy: False? ← BAD, check parent
```

### **4. Fix PanelCustomRoom (nếu cần)**
```
If activeInHierarchy: False
→ Check PanelContainer, PanelCustomRoom, Canvas active
→ Set all TRUE

If activeInHierarchy: True but not visible
→ Check RectTransform position
→ Check Z-order (drag to bottom of Hierarchy)
→ Check Canvas Scaler
```

---

**TEST NGAY VÀ CHO TÔI BIẾT KẾT QUẢ!** 🚀

Đặc biệt là Console log của PanelCustomRoom:
- `activeInHierarchy: True` hay `False`?
- `parent active: True` hay `False`?

Tôi sẽ fix tiếp dựa trên kết quả! 🔥

