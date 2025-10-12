# 🎯 TILE ACTIONS IMPLEMENTATION

**Implement đầy đủ tile actions với panels và notifications**

---

## 🎮 **TILE TYPES & ACTIONS**

### **Map 36 Tiles:**

```
Tile 0: Start (Ô Bắt Đầu)
Tiles 1-6, 8-9, 11-15, 17-18, 20-24, 26-27, 29-32, 34-35: Property (26 tiles)
Tiles 7, 16, 25, 33: Event (4 tiles)
Tile 10: Jail (Ô Tai Nạn)
Tile 19: Quiz (Ô Tra Khảo)
Tile 28: Travel (Ô Du Lịch)
```

---

## ✅ **ĐÃ IMPLEMENT**

### **1. TileType.Start (Ô Bắt Đầu)**

**Action:**
- Hiển thị notification: "{PlayerName} đến Ô Bắt Đầu!"
- Bonus +2000 đã được xử lý trong `PlayerGameController.OnPassStart()`

**Code:**
```csharp
case TileType.Start:
    if (panelNotification != null)
    {
        panelNotification.ShowNotification($"{player.PlayerName} đến Ô Bắt Đầu!");
    }
    break;
```

---

### **2. TileType.Property (Nhà đất)**

**3 trường hợp:**

#### **A. Property chưa có chủ:**
- Show **PanelBuy** (mua nhà)
- Player có thể mua hoặc bỏ qua

**Code:**
```csharp
if (!propertyManager.IsPropertyOwned(tileIndex))
{
    ShowBuyPanel(player, tileIndex, tileName, basePrice);
}
```

#### **B. Property của chính mình:**
- Show **PanelBuy** (upgrade panel)
- Player có thể upgrade: House 1-4, Hotel

**Code:**
```csharp
if (ownerIndex == playerIndex)
{
    ShowUpgradePanel(player, tileIndex, tileName, basePrice);
}
```

#### **C. Property của người khác:**
- Tự động trả rent
- Show **PanelNotification**: "{PlayerName} trả {rent} cho {OwnerName}"
- Không show panel

**Code:**
```csharp
else
{
    PlayerGameController owner = players[ownerIndex];

    // Get money before paying rent
    int moneyBefore = player.Money;

    propertyManager.PayRent(tileIndex, basePrice, player, owner);

    // Calculate actual rent paid (money lost)
    int rentPaid = moneyBefore - player.Money;

    if (panelNotification != null)
    {
        panelNotification.ShowNotification($"{player.PlayerName} trả {rentPaid} cho {owner.PlayerName}");
    }
}
```

---

### **3. TileType.Event (Ô Event)**

**Action:**
- Show **PanelEvent** với random event
- Event tự động apply effect (tiền +/-)
- Panel tự động đóng sau 3 giây

**Code:**
```csharp
case TileType.Event:
    if (panelEvent != null)
    {
        panelEvent.ShowRandomEvent(() => {
            Debug.Log($"[GameManager] Event panel closed");
        });
    }
    break;
```

**Event cards:**
- "Bạn nhận được tiền thưởng từ công ty: +200"
- "Bạn trúng xổ số: +500"
- "Bạn phải trả thuế: -150"
- "Bạn bị mất ví: -100"
- ... (8 events total)

---

### **4. TileType.Quiz (Ô Tra Khảo)**

**Action:**
- Show **PanelQuiz** với câu hỏi từ Firebase
- Player trả lời trong 30 giây
- Đúng: +reward, Sai: -penalty

**Code:**
```csharp
case TileType.Quiz:
    if (panelQuiz != null)
    {
        panelQuiz.Show((isCorrect) => {
            if (isCorrect)
            {
                Debug.Log($"[GameManager] {player.PlayerName} answered correctly!");
            }
            else
            {
                Debug.Log($"[GameManager] {player.PlayerName} answered incorrectly!");
            }
        });
    }
    break;
```

---

### **5. TileType.Jail (Ô Tai Nạn)**

**Action:**
- Set jail counter = 2 (bị giam 2 lượt)
- Show **PanelNotification**: "{PlayerName} bị giam 2 lượt!"
- Player skip 2 turns

**Code:**
```csharp
case TileType.Jail:
    player.SetJailCounter(2);
    if (panelNotification != null)
    {
        panelNotification.ShowNotification($"{player.PlayerName} bị giam 2 lượt!");
    }
    break;
```

---

### **6. TileType.Travel (Ô Du Lịch)**

**Action:**
- Trừ 100 tiền
- Show **PanelNotification**: "{PlayerName} đi du lịch! -100"

**Code:**
```csharp
case TileType.Travel:
    player.SubtractMoney(100);
    if (panelNotification != null)
    {
        panelNotification.ShowNotification($"{player.PlayerName} đi du lịch! -100");
    }
    break;
```

---

## 📊 **PANEL SUMMARY**

### **Panels được sử dụng:**

| Tile Type | Panel | Duration | Auto Close |
|-----------|-------|----------|------------|
| Start | PanelNotification | 1s | ✅ Yes |
| Property (unowned) | PanelBuy | Manual | ❌ No |
| Property (owned by self) | PanelBuy (upgrade) | Manual | ❌ No |
| Property (owned by other) | PanelNotification | 1s | ✅ Yes |
| Event | PanelEvent | 3s | ✅ Yes |
| Quiz | PanelQuiz | 30s | ❌ No (answer required) |
| Jail | PanelNotification | 1s | ✅ Yes |
| Travel | PanelNotification | 1s | ✅ Yes |

---

## 🛠️ **SETUP TRONG UNITY**

### **Bước 1: Assign Panels vào GameManager**

```
Hierarchy → Select GameManager
Inspector → Game Manager (Script)

UI Panels:
├── Panel Game: [PanelGame] ✅
├── Panel Buy: [PanelBuy] ← ⭐ CẦN ASSIGN
├── Panel Quiz: [PanelQuiz] ← ⭐ CẦN ASSIGN
├── Panel Event: [PanelEvent] ← ⭐ CẦN ASSIGN
├── Panel House Sell: [PanelHouseSell]
├── Panel Result: [PanelResult]
├── Panel Card: [PanelCard]
└── Panel Notification: [PanelNotification] ← ⭐ CẦN ASSIGN
```

---

### **Bước 2: Tạo PanelNotification (nếu chưa có)**

```
Canvas → UI → Image
Name: PanelNotification
Position: Top-Center

RectTransform:
  Anchor: Top-Center
  Pivot: (0.5, 1)
  Pos X: 0
  Pos Y: -50
  Width: 400
  Height: 80

Image:
  Color: (0, 0, 0, 0.8) - Black semi-transparent

Add Component: Panel Notification (Script)

Children:
└── TextNotification (TextMeshPro)
    Text: "Notification"
    Font Size: 24
    Color: White
    Alignment: Center

Assign:
├── Text Notification: [TextNotification]
└── Display Duration: 1
```

---

### **Bước 3: Verify PanelEvent**

```
PanelEvent (GameObject)
├── Panel Event (Script)
├── TextEvent (TextMeshPro - event message)
└── ButtonOK (Button - close button)

Settings:
├── Text Event: [TextEvent]
├── Btn OK: [ButtonOK]
└── Auto Close Time: 3
```

---

### **Bước 4: Verify PanelQuiz**

```
PanelQuiz (GameObject)
├── Panel Quiz (Script)
├── TextQuestion (TextMeshPro)
├── ButtonA, ButtonB, ButtonC, ButtonD (Buttons)
└── TextTimer (TextMeshPro - countdown)

Settings:
├── Text Question: [TextQuestion]
├── Answer Buttons: [ButtonA, ButtonB, ButtonC, ButtonD]
├── Text Timer: [TextTimer]
└── Answer Time: 30
```

---

### **Bước 5: Verify PanelBuy**

```
PanelBuy (GameObject)
├── Panel Buy (Script)
├── TextPropertyName (TextMeshPro)
├── TextPrice (TextMeshPro)
├── ButtonBuy (Button - mua)
├── ButtonSkip (Button - bỏ qua)
└── Upgrade buttons (House 1-4, Hotel)

Settings:
├── Text Property Name: [TextPropertyName]
├── Text Price: [TextPrice]
├── Button Buy: [ButtonBuy]
└── Button Skip: [ButtonSkip]
```

---

## 🧪 **TESTING**

### **Test 1: Property Tiles (5 phút)**

```
1. Play Mode
2. Roll dice nhiều lần để đến property tiles
3. Test 3 trường hợp:

A. Property chưa có chủ:
   ✅ PanelBuy hiện ra
   ✅ Có button "Mua" và "Bỏ qua"
   ✅ Click "Mua" → Mua thành công
   ✅ Click "Bỏ qua" → Panel đóng

B. Property của mình:
   ✅ PanelBuy hiện ra (upgrade mode)
   ✅ Có buttons upgrade (House 1-4, Hotel)
   ✅ Click upgrade → Upgrade thành công

C. Property của người khác (cần 2 players):
   ✅ PanelNotification hiện: "Player 1 trả X cho Player 2"
   ✅ Notification tự tắt sau 1 giây
   ✅ Tiền bị trừ
```

---

### **Test 2: Event Tile (2 phút)**

```
1. Play Mode
2. Roll dice đến tile 7, 16, 25, hoặc 33
3. Expected:
   ✅ PanelEvent hiện ra
   ✅ Random event message
   ✅ Tiền +/- theo event
   ✅ Panel tự đóng sau 3 giây
```

---

### **Test 3: Quiz Tile (3 phút)**

```
1. Play Mode
2. Roll dice đến tile 19
3. Expected:
   ✅ PanelQuiz hiện ra
   ✅ Câu hỏi từ Firebase
   ✅ 4 đáp án A, B, C, D
   ✅ Timer đếm ngược 30 giây
   ✅ Click đáp án → Panel đóng
   ✅ Đúng: +reward, Sai: -penalty
```

---

### **Test 4: Jail Tile (2 phút)**

```
1. Play Mode
2. Roll dice đến tile 10
3. Expected:
   ✅ PanelNotification: "Player 1 bị giam 2 lượt!"
   ✅ Notification tự tắt sau 1 giây
   ✅ Player skip 2 turns
```

---

### **Test 5: Travel Tile (2 phút)**

```
1. Play Mode
2. Roll dice đến tile 28
3. Expected:
   ✅ PanelNotification: "Player 1 đi du lịch! -100"
   ✅ Notification tự tắt sau 1 giây
   ✅ Tiền bị trừ 100
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: PanelBuy không hiện**

**Check:**
```
1. GameManager → Panel Buy assigned?
   ❌ Not assigned → Assign PanelBuy

2. PanelBuy exists in scene?
   ❌ Missing → Create PanelBuy

3. Console log?
   "[GameManager] Property X available for purchase: Y" ✅
   "[GameManager] PanelBuy not assigned!" ❌
```

---

### **Problem 2: PanelEvent không hiện**

**Check:**
```
1. GameManager → Panel Event assigned?
   ❌ Not assigned → Assign PanelEvent

2. Tile type đúng?
   Console: "[GameManager] Player landed on X (Type: Event)" ✅
   
3. PanelEvent script?
   ❌ Missing → Add PanelEvent script
```

---

### **Problem 3: PanelQuiz không hiện**

**Check:**
```
1. GameManager → Panel Quiz assigned?
   ❌ Not assigned → Assign PanelQuiz

2. Firebase connection?
   ✅ Check Firebase initialized
   ❌ No connection → Quiz won't load

3. Quiz collection exists?
   Firestore → quizzes collection ✅
```

---

### **Problem 4: PanelNotification không hiện**

**Check:**
```
1. GameManager → Panel Notification assigned?
   ❌ Not assigned → Assign PanelNotification

2. PanelNotification active?
   ✅ Initially hidden (SetActive(false))
   ✅ ShowNotification() sets active

3. Display duration?
   Default: 1 second
   Too fast? → Increase displayDuration
```

---

## ✅ **CHECKLIST**

### **Code:**
- [x] GameManager.cs - Add PanelNotification field
- [x] GameManager.cs - ResolveTile() - TileType.Start
- [x] GameManager.cs - ResolveTile() - TileType.Event
- [x] GameManager.cs - ResolveTile() - TileType.Quiz
- [x] GameManager.cs - ResolveTile() - TileType.Jail
- [x] GameManager.cs - ResolveTile() - TileType.Travel
- [x] GameManager.cs - ResolvePropertyTile() - Pay rent notification

### **Unity Setup:**
- [ ] PanelNotification created
- [ ] GameManager → Panel Notification assigned
- [ ] GameManager → Panel Buy assigned
- [ ] GameManager → Panel Event assigned
- [ ] GameManager → Panel Quiz assigned
- [ ] All panels setup correctly

### **Testing:**
- [ ] Property tiles work (buy/upgrade/rent)
- [ ] Event tiles show PanelEvent
- [ ] Quiz tiles show PanelQuiz
- [ ] Jail tiles show notification
- [ ] Travel tiles show notification
- [ ] All notifications auto-close

---

## 📝 **SUMMARY**

**Đã implement:**
- ✅ TileType.Start → PanelNotification
- ✅ TileType.Property → PanelBuy (buy/upgrade) hoặc PanelNotification (rent)
- ✅ TileType.Event → PanelEvent
- ✅ TileType.Quiz → PanelQuiz
- ✅ TileType.Jail → PanelNotification
- ✅ TileType.Travel → PanelNotification

**Cần làm:**
- [ ] Assign panels vào GameManager
- [ ] Test từng tile type
- [ ] Verify panels hoạt động đúng

---

**DONE! Tile actions đã được implement đầy đủ!** 🎉

