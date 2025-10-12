# ✅ DEMO MODE TURN SYSTEM FIX

**Sửa 2 vấn đề: PanelBuy không hiện + Chỉ roll được 1 lần**

---

## 🐛 **VẤN ĐỀ 1: PANELBUY KHÔNG HIỆN**

### **Triệu chứng:**
```
Console logs:
✅ "[GameManager] Property Beijing available for purchase: 700"
✅ "[GameManager] Showing PanelBuy for Beijing"

Nhưng:
❌ PanelBuy KHÔNG HIỆN trên màn hình
```

### **Nguyên nhân:**

**Chưa rõ - Cần debug thêm!**

Có thể:
1. PanelBuy GameObject bị SetActive(false) trong Hierarchy
2. PanelBuy bị che khuất bởi UI khác (z-order)
3. PanelBuy.ShowBuy() không được gọi (nhưng log cho thấy có gọi)
4. Canvas/EventSystem không hoạt động

### **✅ Đã thêm debug logs:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/UI/PanelBuy.cs" mode="EXCERPT">
````csharp
public void ShowBuy(string propName, int price, int playerMoney, System.Action<int> onBuy, System.Action onSkip)
{
    Debug.Log($"[PanelBuy] ShowBuy called: {propName}, Price: {price}, Money: {playerMoney}");
    
    // ... setup code ...
    
    Debug.Log($"[PanelBuy] Setting active to TRUE");
    gameObject.SetActive(true);
    
    Debug.Log($"[PanelBuy] Panel is now active: {gameObject.activeInHierarchy}");
}
````
</augment_code_snippet>

### **🔍 Cách debug:**

**Bước 1: Check Console logs mới**
```
Expected logs:
✅ "[PanelBuy] ShowBuy called: Beijing, Price: 700, Money: 10000"
✅ "[PanelBuy] Setting active to TRUE"
✅ "[PanelBuy] Panel is now active: True"

Nếu KHÔNG thấy logs:
→ ShowBuy() KHÔNG được gọi
→ Check GameManager.ShowBuyPanel()

Nếu thấy logs nhưng "active: False":
→ Parent GameObject bị inactive
→ Check Hierarchy
```

**Bước 2: Check Unity Hierarchy**
```
Play Mode → Pause khi log "Showing PanelBuy"
Hierarchy → Search "PanelBuy"
Check:
├── PanelBuy active? (checkbox ticked)
├── Parent Canvas active?
├── EventSystem exists?
└── RectTransform visible? (not off-screen)
```

**Bước 3: Check Inspector**
```
PanelBuy → Inspector:
├── Active: ✅ Checked
├── Canvas Renderer: Enabled
├── Image: Color alpha > 0
└── Position: On screen (not -9999)
```

---

## 🐛 **VẤN ĐỀ 2: CHỈ ROLL ĐƯỢC 1 LẦN**

### **Triệu chứng:**
```
1. Roll dice lần 1: ✅ OK
2. Player di chuyển: ✅ OK
3. ResolveTile(): ✅ OK
4. Sau đó: ❌ KHÔNG THỂ roll lần 2
```

### **Nguyên nhân:**

**Turn tự động kết thúc ngay sau ResolveTile()!**

**Code cũ:**
```csharp
// RollAndMove()
yield return player.MoveBySteps(diceResult);
ResolveTile(player);

// ❌ Auto end turn sau 1 giây
yield return new WaitForSeconds(1f);
EndTurn();
```

**Vấn đề:**
- PanelBuy hiện ra (nếu có)
- Nhưng 1 giây sau → EndTurn() được gọi
- Panel vẫn đang mở nhưng turn đã kết thúc
- User không thể roll lần 2

### **✅ Giải pháp:**

**KHÔNG tự động end turn - Chờ panel đóng!**

**1. RollAndMove() - Bỏ auto end turn:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
// Move player
yield return player.MoveBySteps(diceResult);

// Resolve tile
ResolveTile(player);

// ⭐ KHÔNG TỰ ĐỘNG END TURN
// Panel sẽ tự gọi EndTurn() khi user chọn xong
````
</augment_code_snippet>

**2. ResolveTile() - Auto end cho tiles không có panel:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
case TileType.Start:
    panelNotification.ShowNotification($"{player.PlayerName} đến Ô Bắt Đầu!");
    // ⭐ Auto end turn (no panel)
    StartCoroutine(AutoEndTurnAfterDelay(1f));
    break;

case TileType.Jail:
    player.SetJailCounter(2);
    panelNotification.ShowNotification($"{player.PlayerName} bị giam 2 lượt!");
    // ⭐ Auto end turn (no panel)
    StartCoroutine(AutoEndTurnAfterDelay(1f));
    break;
````
</augment_code_snippet>

**3. Panel callbacks - End turn sau khi user chọn:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
// PanelBuy - onBuy callback
(selectedLevel) => {
    if (selectedLevel > 0)
    {
        propertyManager.BuyProperty(...);
    }
    
    // ⭐ End turn after buying
    StartCoroutine(AutoEndTurnAfterDelay(0.5f));
},

// PanelBuy - onSkip callback
() => {
    Debug.Log($"{player.PlayerName} skipped buying");
    
    // ⭐ End turn after skipping
    StartCoroutine(AutoEndTurnAfterDelay(0.5f));
}
````
</augment_code_snippet>

**4. EndTurn() - Support Demo Mode:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
private void EndTurn()
{
    if (!isGameActive) return;
    
    // ⭐ Demo Mode OR Host can manage turns
    if (!demoMode && !IsHost) return;
    
    Debug.Log($"[GameManager] Turn ended. Player {currentPlayerIndex}/{players.Count - 1}");
    
    // ... next turn logic ...
}
````
</augment_code_snippet>

**5. AutoEndTurnAfterDelay() - Helper method:**

<augment_code_snippet path="Project Game AntKnow/Assets/Scenes/Game/Scripts/Core/GameManager.cs" mode="EXCERPT">
````csharp
/// <summary>
/// Auto end turn after delay (for tiles without panels)
/// </summary>
private IEnumerator AutoEndTurnAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    Debug.Log("[GameManager] Auto ending turn after delay");
    EndTurn();
}
````
</augment_code_snippet>

---

## 📊 **TURN FLOW - BEFORE vs AFTER**

### **❌ BEFORE (Broken):**

```
Roll Dice
  ↓
Move Player
  ↓
ResolveTile()
  ├── Show PanelBuy (if property)
  └── (panel still open)
  ↓
Wait 1 second ← ⚠️ PROBLEM
  ↓
EndTurn() ← ❌ Turn ends while panel open!
  ↓
❌ Cannot roll again (turn already ended)
```

---

### **✅ AFTER (Fixed):**

```
Roll Dice
  ↓
Move Player
  ↓
ResolveTile()
  ├── Property (unowned):
  │   ├── Show PanelBuy
  │   └── Wait for user choice
  │       ├── User clicks "MUA" → EndTurn()
  │       └── User clicks "BỎ QUA" → EndTurn()
  │
  ├── Event:
  │   ├── Show PanelEvent
  │   └── Auto close → EndTurn()
  │
  ├── Quiz:
  │   ├── Show PanelQuiz
  │   └── User answers → EndTurn()
  │
  └── Start/Jail/Travel:
      ├── Show notification
      └── Auto EndTurn() after 1s
  ↓
EndTurn()
  ↓
Next player's turn
  ↓
✅ Can roll again!
```

---

## 🧪 **TESTING**

### **Test 1: PanelBuy Debug (3 phút)**

```
1. Play Mode
2. Roll dice đến property tile
3. Check Console:
   
   Expected logs:
   ✅ "[GameManager] Showing PanelBuy for Beijing"
   ✅ "[PanelBuy] ShowBuy called: Beijing, Price: 700, Money: 10000"
   ✅ "[PanelBuy] Setting active to TRUE"
   ✅ "[PanelBuy] Panel is now active: True"
   
4. If "active: False":
   → Pause game
   → Check Hierarchy
   → Find PanelBuy
   → Check parent active
   
5. If panel visible:
   → Click "MUA" or "BỎ QUA"
   → Check turn ends
```

---

### **Test 2: Multiple Turns (5 phút)**

```
1. Play Mode
2. Roll dice (Turn 1)
3. Player moves
4. Panel shows (or notification)
5. Choose option (or wait)
6. Expected:
   ✅ Console: "[GameManager] Auto ending turn after delay"
   ✅ Console: "[GameManager] Turn ended. Player 0/0"
   ✅ Console: "[GameManager] Starting turn for Player 1"
   ✅ Button Roll enabled again
   
7. Roll dice (Turn 2)
8. Expected:
   ✅ Can roll again!
   ✅ Player moves
   ✅ Turn system works
   
9. Repeat 5-10 times
10. Verify:
    ✅ Can roll multiple times
    ✅ Turns cycle correctly
    ✅ No stuck state
```

---

### **Test 3: All Tile Types (10 phút)**

```
Test each tile type:

1. Property (unowned):
   ✅ PanelBuy shows
   ✅ Click "MUA" → Turn ends
   ✅ Click "BỎ QUA" → Turn ends
   ✅ Can roll next turn

2. Property (owned by other):
   ✅ Notification shows rent
   ✅ Auto end turn after 1s
   ✅ Can roll next turn

3. Event:
   ✅ PanelEvent shows
   ✅ Money changes
   ✅ Auto end turn after panel closes
   ✅ Can roll next turn

4. Quiz:
   ✅ PanelQuiz shows
   ✅ Answer question
   ✅ Turn ends after answer
   ✅ Can roll next turn

5. Jail:
   ✅ Notification shows
   ✅ Auto end turn after 1s
   ✅ Can roll next turn (but skipped)

6. Travel:
   ✅ Notification shows
   ✅ Money -100
   ✅ Auto end turn after 1s
   ✅ Can roll next turn

7. Start:
   ✅ Notification shows
   ✅ Auto end turn after 1s
   ✅ Can roll next turn
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: PanelBuy vẫn không hiện**

**Check 1: Console logs**
```
Có log "[PanelBuy] ShowBuy called"?
├── YES → Panel được gọi, check Hierarchy
└── NO → ShowBuy() không được gọi, check GameManager
```

**Check 2: Hierarchy**
```
Play Mode → Pause
Hierarchy → Search "PanelBuy"
├── Found?
│   ├── Active? (checkbox)
│   ├── Parent active?
│   └── Position on screen?
└── Not found?
    → Panel chưa được tạo!
```

**Check 3: Inspector**
```
PanelBuy → Inspector:
├── Canvas Renderer: Enabled?
├── Image: Alpha > 0?
├── RectTransform: Anchored position?
└── Z-order: Not behind other panels?
```

---

### **Problem 2: Vẫn chỉ roll được 1 lần**

**Check Console:**
```
Expected after turn 1:
✅ "[GameManager] Auto ending turn after delay"
✅ "[GameManager] Turn ended. Player 0/0"
✅ "[GameManager] Starting turn for Player 1"

If missing:
→ EndTurn() không được gọi
→ Check AutoEndTurnAfterDelay()
→ Check panel callbacks
```

**Check Button Roll:**
```
After turn ends:
✅ Button Roll enabled?
✅ Button Roll interactable?
✅ PanelRoll button enabled?

If disabled:
→ StartTurn() không được gọi
→ Check EndTurn() → NextTurn() flow
```

---

### **Problem 3: Turn ends quá nhanh**

**Tăng delay:**
```csharp
// In callbacks
StartCoroutine(AutoEndTurnAfterDelay(2f)); // Thay vì 0.5f
```

---

### **Problem 4: Turn không bao giờ kết thúc**

**Check callbacks:**
```
Panel callbacks có gọi EndTurn()?
├── PanelBuy: onBuy + onSkip
├── PanelEvent: onClose
├── PanelQuiz: onAnswer
└── Tiles without panels: AutoEndTurnAfterDelay()
```

---

## ✅ **CHECKLIST**

### **Code:**
- [x] RollAndMove() - Bỏ auto end turn
- [x] ResolveTile() - Auto end cho tiles không panel
- [x] ShowBuyPanel() - End turn trong callbacks
- [x] PanelEvent callback - End turn
- [x] PanelQuiz callback - End turn
- [x] ResolvePropertyTile() - End turn khi pay rent
- [x] EndTurn() - Support Demo Mode
- [x] AutoEndTurnAfterDelay() - Helper method
- [x] PanelBuy.ShowBuy() - Debug logs

### **Testing:**
- [ ] Compile successful
- [ ] PanelBuy debug logs appear
- [ ] PanelBuy visible on screen
- [ ] Can roll multiple times
- [ ] Turn system cycles correctly
- [ ] All tile types work
- [ ] No stuck states

---

## 📝 **SUMMARY**

**Vấn đề 1: PanelBuy không hiện**
- ✅ Thêm debug logs vào PanelBuy.ShowBuy()
- ⏳ Chờ test để xác định nguyên nhân

**Vấn đề 2: Chỉ roll được 1 lần**
- ✅ Bỏ auto end turn trong RollAndMove()
- ✅ Panel callbacks tự end turn
- ✅ Tiles không panel auto end turn
- ✅ EndTurn() support Demo Mode

**Cần làm:**
- [ ] Save all files
- [ ] Return to Unity
- [ ] Wait for compile
- [ ] Play Mode và test
- [ ] Check Console logs
- [ ] Verify PanelBuy hiện
- [ ] Verify có thể roll nhiều lần
- [ ] Báo kết quả!

---

**DONE! Hệ thống turn đã được sửa!** 🎉

