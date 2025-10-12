# ✅ COMPLETE TILE SYSTEM - FULL IMPLEMENTATION

**Hệ thống tile hoàn chỉnh với panels, money changes, và property visuals**

---

## 🎯 **YÊU CẦU ĐÃ IMPLEMENT**

### **✅ 1. PanelBuy - Cho phép người chơi chọn**
- ✅ Hiện panel khi vào ô đất trống
- ✅ User chọn "Mua" hoặc "Bỏ qua"
- ✅ Trừ tiền khi mua
- ✅ Show notification khi mua/bỏ qua

### **✅ 2. Property Visual - Đổi màu platform**
- ✅ Platform đổi màu theo người chơi
- ✅ Hiển thị cấp nhà (House 1-4, Hotel)
- ✅ PropertyManager.UpdatePropertyVisual()

### **✅ 3. Event - Cộng/trừ tiền**
- ✅ Random event từ database
- ✅ Apply money changes (+/-)
- ✅ Show amount trong panel

### **✅ 4. Mua đất - Trừ tiền**
- ✅ PropertyManager.BuyProperty() trừ tiền
- ✅ Show notification với giá

---

## 📊 **TILE SYSTEM OVERVIEW**

| Tile Type | Panel | Money Change | Visual Change | User Choice |
|-----------|-------|--------------|---------------|-------------|
| **Start** | PanelNotification | +2000 (pass) | ❌ No | ❌ No |
| **Property (unowned)** | PanelBuy | -basePrice (buy) | ✅ Color + Level | ✅ Buy/Skip |
| **Property (owned by self)** | PanelBuy (upgrade) | -upgradeCost | ✅ Level | ✅ Upgrade/Skip |
| **Property (owned by other)** | PanelNotification | -rent | ❌ No | ❌ No |
| **Event** | PanelEvent | +/- random | ❌ No | ❌ No |
| **Quiz** | PanelQuiz | +/- (answer) | ❌ No | ✅ Answer |
| **Jail** | PanelNotification | 0 | ❌ No | ❌ No |
| **Travel** | PanelNotification | -100 | ❌ No | ❌ No |

---

## 🔧 **ĐÃ SỬA**

### **1. GameManager.cs - ShowBuyPanel()**

**Before:**
```csharp
panelBuy.ShowBuy(tileName, basePrice, player.Money, (selectedLevel) => {
    // Only 1 callback - missing onSkip!
});
```

**After:**
```csharp
panelBuy.ShowBuy(tileName, basePrice, player.Money, 
    // ⭐ onBuy callback
    (selectedLevel) => {
        if (selectedLevel > 0)
        {
            // Buy property
            propertyManager.BuyProperty(tileIndex, playerIdx, basePrice, player);
            
            // Show notification
            if (panelNotification != null)
            {
                panelNotification.ShowNotification($"{player.PlayerName} mua {tileName} ({basePrice})");
            }
        }
    },
    // ⭐ onSkip callback
    () => {
        Debug.Log($"[GameManager] {player.PlayerName} skipped buying {tileName}");
        
        if (panelNotification != null)
        {
            panelNotification.ShowNotification($"{player.PlayerName} bỏ qua {tileName}");
        }
    }
);
```

---

### **2. PanelEvent.cs - Apply Money Changes**

**Added EventCardData struct:**
```csharp
[System.Serializable]
public struct EventCardData
{
    public string message;
    public int moneyChange; // Positive = gain, Negative = lose
    
    public EventCardData(string msg, int money)
    {
        message = msg;
        moneyChange = money;
    }
}
```

**Updated event database:**
```csharp
[SerializeField] private EventCardData[] eventCards = {
    new EventCardData("Bạn nhận được tiền thưởng từ công ty!", 200),
    new EventCardData("Bạn trúng xổ số!", 500),
    new EventCardData("Bạn phải trả thuế!", -150),
    new EventCardData("Bạn bị mất ví!", -100),
    new EventCardData("Bạn nhận được tiền từ người thân!", 300),
    new EventCardData("Bạn phải sửa xe!", -200),
    new EventCardData("Bạn nhận được tiền hoàn thuế!", 250),
    new EventCardData("Bạn phải trả tiền bảo hiểm!", -180)
};
```

**Updated ShowRandomEvent():**
```csharp
public void ShowRandomEvent(System.Action<int> onClose = null)
{
    onCloseCallback = onClose;
    
    // Get random event
    EventCardData randomEvent = GetRandomEvent();
    currentMoneyChange = randomEvent.moneyChange;
    
    // Format message with money change
    string message = randomEvent.message;
    if (randomEvent.moneyChange > 0)
    {
        message += $"\n+{randomEvent.moneyChange} 💰";
    }
    else if (randomEvent.moneyChange < 0)
    {
        message += $"\n{randomEvent.moneyChange} 💰";
    }
    
    textEvent.text = message;
    gameObject.SetActive(true);
    StartCoroutine(AutoCloseCoroutine());
}
```

**Updated OnOKClicked():**
```csharp
private void OnOKClicked()
{
    StopAllCoroutines();
    onCloseCallback?.Invoke(currentMoneyChange); // ⭐ Pass money change
    Hide();
}
```

---

### **3. GameManager.cs - Apply Event Money Changes**

**Updated ResolveTile():**
```csharp
case TileType.Event:
    if (panelEvent != null)
    {
        panelEvent.ShowRandomEvent((moneyChange) => {
            // Apply money change
            if (moneyChange > 0)
            {
                player.AddMoney(moneyChange);
                Debug.Log($"[GameManager] {player.PlayerName} gained {moneyChange} from event");
            }
            else if (moneyChange < 0)
            {
                player.SubtractMoney(-moneyChange);
                Debug.Log($"[GameManager] {player.PlayerName} lost {-moneyChange} from event");
            }
        });
    }
    break;
```

---

## 🎨 **PROPERTY VISUAL SYSTEM**

### **PropertyManager.UpdatePropertyVisual()**

**Đã có sẵn trong PropertyManager.cs:**

```csharp
private void UpdatePropertyVisual(int tileId)
{
    if (propertyVisual == null || boardManager == null)
    {
        return;
    }

    int level = GetPropertyLevel(tileId);
    int ownerIndex = GetPropertyOwner(tileId);

    // Get rent price for display
    int basePrice = boardManager.GetTilePrice(tileId);
    int rent = CalculateRent(basePrice, level);
    float multiplier = propertyRentMultipliers.ContainsKey(tileId) ? propertyRentMultipliers[tileId] : 1f;
    int finalRent = StatsCalculator.CalculateFinalRent(rent, multiplier);

    propertyVisual.UpdatePropertyVisual(tileId, level, ownerIndex, finalRent);
}
```

**Được gọi tự động khi:**
- BuyProperty() - Mua nhà
- UpgradeProperty() - Nâng cấp
- SellProperty() - Bán nhà

**PropertyVisual sẽ:**
- ✅ Đổi màu platform theo ownerIndex
- ✅ Hiển thị house models (1-4) hoặc hotel (5)
- ✅ Update rent display

---

## 🛠️ **SETUP TRONG UNITY**

### **Bước 1: Assign PanelBuy vào GameManager**

```
Hierarchy → Find "PanelBuy"
GameManager → UI Panels → Panel Buy: [PanelBuy]
```

**Nếu chưa có PanelBuy:**
```
Canvas → UI → Image
Name: PanelBuy
Add Component: Panel Buy (Script)

Children:
├── TextPropertyName (TextMeshPro)
├── TextPrice (TextMeshPro)
├── ButtonBuy (Button - "MUA")
└── ButtonSkip (Button - "BỎ QUA")

Assign components vào script
```

---

### **Bước 2: Verify PanelEvent**

```
PanelEvent → Panel Event (Script)
Check:
├── Event Cards: Array of 8 events ✅
├── Text Event: [TextEvent] ✅
├── Btn OK: [ButtonOK] ✅
└── Auto Close Time: 3 ✅
```

---

### **Bước 3: Verify PropertyVisual**

```
Hierarchy → Find "PropertyVisual" or "BoardManager"
PropertyManager → Property Visual: [PropertyVisual]

PropertyVisual should have:
├── Player colors (4 colors for 4 players)
├── House models (prefabs for level 1-4)
├── Hotel model (prefab for level 5)
└── Platform materials
```

---

## 🧪 **TESTING**

### **Test 1: PanelBuy - Mua đất (5 phút)**

```
1. Play Mode
2. Roll dice đến property tile (unowned)
3. Expected:
   ✅ Console: "[GameManager] Property Jakarta available for purchase: 600"
   ✅ Console: "[GameManager] Showing PanelBuy for Jakarta"
   ✅ PanelBuy hiện ra
   ✅ Có button "MUA" và "BỎ QUA"
   
4. Click "MUA":
   ✅ Console: "[GameManager] Player 1 bought Jakarta for 600"
   ✅ PanelNotification: "Player 1 mua Jakarta (600)"
   ✅ Money giảm 600
   ✅ Platform đổi màu (player color)
   ✅ Panel đóng
   
5. Click "BỎ QUA":
   ✅ Console: "[GameManager] Player 1 skipped buying Jakarta"
   ✅ PanelNotification: "Player 1 bỏ qua Jakarta"
   ✅ Money không đổi
   ✅ Platform không đổi màu
   ✅ Panel đóng
```

---

### **Test 2: Event - Cộng/trừ tiền (3 phút)**

```
1. Play Mode
2. Roll dice đến tile 7, 16, 25, hoặc 33
3. Expected:
   ✅ PanelEvent hiện ra
   ✅ Random event message
   ✅ Money amount hiển thị: "+200 💰" hoặc "-150 💰"
   ✅ Panel tự đóng sau 3 giây
   
4. After panel closes:
   ✅ Console: "[GameManager] Player 1 gained 200 from event" (nếu +)
   ✅ Console: "[GameManager] Player 1 lost 150 from event" (nếu -)
   ✅ Money thay đổi đúng
```

---

### **Test 3: Property Visual (5 phút)**

```
1. Play Mode
2. Mua 1 property
3. Check:
   ✅ Platform đổi màu (player 1 color)
   ✅ Level = 0 (đất trống)
   
4. Upgrade to House 1:
   ✅ House model xuất hiện
   ✅ Platform vẫn giữ màu
   
5. Upgrade to Hotel:
   ✅ Hotel model xuất hiện
   ✅ House models biến mất
```

---

## 🐛 **TROUBLESHOOTING**

### **Problem 1: PanelBuy không hiện**

**Check Console:**
```
"[GameManager] PanelBuy not assigned! Auto-buying property..."
```

**Solution:**
```
1. Hierarchy → Find PanelBuy
2. GameManager → Panel Buy: [PanelBuy]
3. Test lại
```

---

### **Problem 2: Event không cộng/trừ tiền**

**Check Console:**
```
"[GameManager] Player 1 gained X from event" ← Should see this
```

**If missing:**
```
1. Check PanelEvent.cs compiled correctly
2. Check EventCardData struct exists
3. Check callback được gọi
```

---

### **Problem 3: Platform không đổi màu**

**Check:**
```
1. PropertyManager → Property Visual assigned?
2. PropertyVisual → Player colors setup?
3. Console: "[PropertyManager] Updated property visual for tile X"
```

---

## ✅ **CHECKLIST**

### **Code:**
- [x] GameManager.cs - ShowBuyPanel() with 2 callbacks
- [x] GameManager.cs - Apply event money changes
- [x] PanelEvent.cs - EventCardData struct
- [x] PanelEvent.cs - ShowRandomEvent() with money
- [x] PanelEvent.cs - OnOKClicked() pass money change
- [x] No compile errors

### **Unity Setup:**
- [ ] PanelBuy exists and assigned
- [ ] PanelEvent exists and assigned
- [ ] PropertyVisual exists and assigned
- [ ] PropertyManager → Property Visual assigned
- [ ] All panels setup correctly

### **Testing:**
- [ ] PanelBuy shows when land on unowned property
- [ ] Click "MUA" → Money decreases, platform changes color
- [ ] Click "BỎ QUA" → Nothing changes
- [ ] Event tiles → Money changes (+/-)
- [ ] Property visual updates (color + level)

---

## 📝 **SUMMARY**

**Đã implement đầy đủ:**
- ✅ PanelBuy cho phép user chọn mua/bỏ qua
- ✅ Trừ tiền khi mua đất
- ✅ Event cộng/trừ tiền
- ✅ Property visual đổi màu + cấp nhà
- ✅ Notifications cho tất cả actions

**Cần làm:**
- [ ] Assign PanelBuy vào GameManager
- [ ] Test PanelBuy (mua/bỏ qua)
- [ ] Test Event (cộng/trừ tiền)
- [ ] Test Property visual (màu + cấp)

---

**DONE! Hệ thống tile hoàn chỉnh!** 🎉

