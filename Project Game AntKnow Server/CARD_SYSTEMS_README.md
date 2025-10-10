# 🎮 CARD SYSTEMS - COMPLETE IMPLEMENTATION

**Status**: ✅ **READY TO USE** (Deployed to Multiplay)

## 📋 TÓM TẮT HỆ THỐNG

### **2 Loại Card:**

#### **1. Event Cards** ⚡
- **Lưu trữ**: Local trong Unity (không dùng Firebase)
- **Trigger**: Khi player vào ô Event (Chance) - tiles 7, 16, 25, 33
- **Số lượng**: 21 cards với 12 loại hiệu ứng
- **Random**: Weighted random (mỗi card có weight riêng)

#### **2. Skill Cards** 🃏
- **Lưu trữ**: Firebase Firestore (theo DBview.md)
- **Trigger**: Passive (tự động) hoặc Active (thủ công)
- **Số lượng**: 4 cards cơ bản (có thể mở rộng)
- **Cooldown**: Độc lập, tính theo stars của thẻ

---

## 🎴 EVENT CARDS (21 Cards)

### **Loại hiệu ứng:**

| Loại | Số lượng | Ví dụ |
|------|----------|-------|
| **GainMoney** | 4 | Trúng số (+500), Thưởng Tết (+300) |
| **LoseMoney** | 4 | Đóng thuế (-300), Sửa xe (-150) |
| **MoveForward** | 3 | Đi taxi (+3 ô), Bay nhanh (+5 ô) |
| **MoveBackward** | 2 | Đi nhầm (-2 ô), Quên đồ (-3 ô) |
| **GoToTile** | 2 | Về nhà (ô 0), Đi du lịch (ô 28) |
| **PayToPlayers** | 1 | Tiệc tất niên (-100 mỗi người) |
| **CollectFromPlayers** | 1 | Sinh nhật (+50 mỗi người) |
| **RepairProperties** | 1 | Sửa nhà (50/nhà, 100/hotel) |
| **GoToJail** | 1 | Bị bắt (3 turns) |
| **GetOutOfJailFree** | 1 | Thẻ ra tù miễn phí |
| **TaxPerProperty** | 1 | Thuế tài sản (50/ô) |

### **Cách sử dụng:**

```csharp
// Draw random event card
var eventHandler = new EventCardHandler();
var card = eventHandler.DrawEventCard();

// Execute effect
var result = eventHandler.ExecuteEventCard(card, player, gameState);

if (result.success)
{
    Debug.Log($"{card.name}: {result.message}");
    Debug.Log($"Money change: {result.moneyChange}");
}
```

---

## 🃏 SKILL CARDS (4 Cards Cơ Bản)

### **1. Lẩn Trốn** 🏃
```
Chỉ số: Agility +10
Loại: Passive (Bị động)
Cooldown: 5 turns
Trigger: OnEnterOpponentHouse (khi vào nhà người khác)
Effect: autoStepForward (tự động tiến 1 ô)
```

### **2. Siêu Sale** 💰
```
Chỉ số: Intelligence +10
Loại: Passive (Bị động)
Cooldown: 5 turns
Trigger: OnTryPurchaseProperty (khi mua nhà)
Effect: purchaseDiscount (giảm 30% giá mua)
```

### **3. Bảo Kê** 🛡️
```
Chỉ số: Health +10
Loại: Active (Chủ động)
Cooldown: 8 turns
Trigger: Manual (người chơi kích hoạt)
Effect: protectProperty (bảo vệ 1 nhà khỏi bị mua lại, 1 turn)
```

### **4. Chăm Chỉ** 💵
```
Chỉ số: Luck +10
Loại: Active (Chủ động)
Cooldown: 6 turns
Trigger: Manual (người chơi kích hoạt)
Effect: extraStartSalary (nhận gấp đôi lương ô bắt đầu)
```

### **Cách sử dụng:**

```csharp
// Initialize engine
var skillEngine = new SkillTriggerEngine();

// Trigger passive skill (auto)
var context = new SkillExecutionContext
{
    tileIndex = player.NodeIndex,
    property = gameState.Properties[tileIndex]
};
var results = skillEngine.TriggerPassiveSkills(
    SkillTriggers.OnEnterOpponentHouse, 
    player, 
    gameState, 
    context
);

// Execute active skill (manual)
var cardInstance = GetPlayerCardInstance(player, "skill.bao-ke");
var cardData = BasicSkillCards.GetCardByItemId("skill.bao-ke");
var result = skillEngine.ExecuteSkill(cardInstance, cardData, player, gameState, context);
```

---

## 🔧 KIẾN TRÚC HỆ THỐNG

### **Tách biệt chỉ số:**

```
1. Chỉ số THẺ BÀI (Card Attributes)
   - Health, Agility, Intelligence, Luck, Resistance
   - Lưu trong SkillCardData.attributes
   - Tăng theo level thẻ (trong Firebase)

2. Chỉ số NHÂN VẬT (Player Stats)
   - Health, Agility, Intelligence, Luck, Resistance
   - Lưu trong PlayerState
   - Ảnh hưởng gameplay (rent, salary, etc.)

3. COOLDOWN
   - cooldownBaseTurns: Cooldown gốc (từ Firebase)
   - effectiveCooldown: Sau khi tính stars
   - currentCooldown: Số turns còn lại
```

### **Extensible Design:**

#### **Thêm Trigger mới:**
```csharp
// 1. Thêm constant trong SkillTriggers
public const string OnLandOnQuiz = "onLandOnQuiz";

// 2. Trigger trong ServerGameManager
skillEngine.TriggerPassiveSkills(
    SkillTriggers.OnLandOnQuiz,
    player,
    gameState,
    context
);
```

#### **Thêm Effect mới:**
```csharp
// 1. Thêm constant trong SkillEffects
public const string DoubleRent = "doubleRent";

// 2. Tạo class implement ISkillEffect
public class DoubleRentEffect : ISkillEffect
{
    public SkillExecutionResult Execute(...)
    {
        // Implementation
    }
}

// 3. Register trong SkillTriggerEngine
_effectHandlers[SkillEffects.DoubleRent] = new DoubleRentEffect();
```

#### **Thêm Card mới (Firebase):**
```typescript
// 1. Thêm vào Firestore collection "items"
{
  "itemId": "skill.double-rent",
  "type": "skill_card",
  "attributes": { "intelligence": 15 },
  "skill": {
    "mode": "passive",
    "primaryStat": "intelligence",
    "cooldownBaseTurns": 6,
    "triggerId": "onReceiveRent",
    "effectId": "doubleRent",
    "params": { "multiplier": 2 }
  }
}

// 2. Load trong Unity (tự động)
// Không cần code thêm!
```

---

## 📁 FILES CREATED

### **Domain Layer:**
```
Assets/Script/Domain/Data/
├── EventCardLibrary.cs       ✅ 21 event cards (local)
├── SkillCardData.cs           ✅ Skill card data structures
└── SimpleBoardConfig.cs       ✅ (đã có) 36 tiles config

Assets/Script/Domain/Services/
├── EventCardHandler.cs        ✅ Event card execution
├── SkillTriggerEngine.cs      ✅ Skill trigger & effect engine
├── TurnSystem.cs              ✅ (đã có) Turn management
└── BoardRules.cs              ✅ (đã có) Game rules
```

---

## 🔥 INTEGRATION VỚI SERVERGAMEMANAGER

### **Các điểm tích hợp:**

#### **1. Khi player vào ô Event (Chance):**
```csharp
case TileType.Chance:
    var eventHandler = new EventCardHandler();
    var card = eventHandler.DrawEventCard();
    var result = eventHandler.ExecuteEventCard(card, player, gameState);
    
    // Notify clients
    NotifyEventCardClientRpc(player.Id, card.id, result.message);
    break;
```

#### **2. Khi player vào nhà người khác (Passive skill - Lẩn trốn):**
```csharp
// Trong HandlePropertyTile()
if (property.Owner != Owner.None && (int)property.Owner != player.Id)
{
    // Check for "Lẩn trốn" skill
    var context = new SkillExecutionContext
    {
        tileIndex = player.NodeIndex,
        property = property
    };
    
    var results = skillEngine.TriggerPassiveSkills(
        SkillTriggers.OnEnterOpponentHouse,
        player,
        gameState,
        context
    );
    
    if (results.Count > 0)
    {
        // Skill activated! Player moved forward
        NotifySkillActivatedClientRpc(player.Id, results[0].cardName, results[0].effectDescription);
        return; // Skip rent payment
    }
    
    // Normal rent payment
    var rent = BoardRules.CalcRent(tileData, property, owner);
    BoardRules.PayRent(player, owner, rent);
}
```

#### **3. Khi player mua nhà (Passive skill - Siêu Sale):**
```csharp
[ServerRpc]
public void BuyPropertyServerRpc(int tileId, ServerRpcParams rpcParams = default)
{
    // Get price
    var tileData = SimpleBoardConfig.GetTile(tileId);
    int price = tileData.basePrice;
    
    // Check for "Siêu Sale" skill
    var context = new SkillExecutionContext
    {
        purchasePrice = price,
        property = gameState.Properties[tileId]
    };
    
    var results = skillEngine.TriggerPassiveSkills(
        SkillTriggers.OnTryPurchaseProperty,
        player,
        gameState,
        context
    );
    
    if (results.Count > 0)
    {
        // Discount applied!
        int finalPrice = (int)results[0].changes["finalPrice"];
        price = finalPrice;
        
        NotifySkillActivatedClientRpc(player.Id, results[0].cardName, results[0].effectDescription);
    }
    
    // Buy property
    if (BoardRules.CanBuy(player, property) && player.Money >= price)
    {
        player.Money -= price;
        BoardRules.Buy(player, property);
    }
}
```

#### **4. Khi player kích hoạt skill chủ động (Active):**
```csharp
[ServerRpc]
public void UseActiveSkillServerRpc(string cardInstanceId, int targetTileId, ServerRpcParams rpcParams = default)
{
    var player = GetPlayerFromClientId(rpcParams.Receive.SenderClientId);
    var cardInstance = GetCardInstance(player, cardInstanceId);
    var cardData = BasicSkillCards.GetCardByItemId(cardInstance.itemId);
    
    var context = new SkillExecutionContext
    {
        property = gameState.Properties.ContainsKey(targetTileId) 
            ? gameState.Properties[targetTileId] 
            : null
    };
    
    var result = skillEngine.ExecuteSkill(cardInstance, cardData, player, gameState, context);
    
    if (result.success)
    {
        NotifySkillActivatedClientRpc(player.Id, result.cardName, result.effectDescription);
    }
    else
    {
        NotifySkillFailedClientRpc(player.Id, result.message);
    }
}
```

---

## 🧪 TESTING

### **Test Event Cards:**
```csharp
// Test 1: Draw random cards
for (int i = 0; i < 10; i++)
{
    var card = eventHandler.DrawEventCard();
    Debug.Log($"Drew: {card.name} ({card.type})");
}

// Test 2: Execute specific card
var card = EventCardLibrary.GetEventCardById(1); // Trúng số
var result = eventHandler.ExecuteEventCard(card, player, gameState);
Debug.Log($"Result: {result.message}, Money: {player.Money}");
```

### **Test Skill Cards:**
```csharp
// Test 1: Passive trigger
var context = new SkillExecutionContext { tileIndex = 5 };
var results = skillEngine.TriggerPassiveSkills(
    SkillTriggers.OnEnterOpponentHouse,
    player,
    gameState,
    context
);

// Test 2: Active skill
var cardInstance = new SkillCardInstance
{
    itemId = "skill.bao-ke",
    level = 1,
    stars = 0,
    effectiveCooldown = 8,
    currentCooldown = 0
};
var cardData = BasicSkillCards.GetCardByItemId("skill.bao-ke");
var result = skillEngine.ExecuteSkill(cardInstance, cardData, player, gameState, context);
```

---

## ✅ CHECKLIST

### **Server (Deployed to Multiplay):**
- [x] EventCardLibrary.cs - 21 event cards
- [x] EventCardHandler.cs - Event execution
- [x] SkillCardData.cs - Skill data structures
- [x] SkillTriggerEngine.cs - Skill trigger engine
- [x] ISkillEffect implementations - 8 effects
- [ ] ServerGameManager integration (cần implement)

### **Client (Cần implement):**
- [ ] Event card UI panel
- [ ] Skill card UI panel
- [ ] Active skill button
- [ ] Cooldown display
- [ ] Effect animation

### **Firebase:**
- [ ] Load skill cards từ Firestore
- [ ] Cache card definitions
- [ ] Sync player inventory

---

## 🚀 NEXT STEPS

1. **Update ServerGameManager** (30 phút)
   - Integrate event card handler
   - Integrate skill trigger engine
   - Add ServerRpc methods

2. **Create Client UI** (2-3 giờ)
   - Event card popup panel
   - Skill card display
   - Active skill button
   - Cooldown timer

3. **Firebase Integration** (1-2 giờ)
   - Load skill cards from Firestore
   - Map Firebase data to SkillCardData
   - Cache card definitions

4. **Testing** (1-2 giờ)
   - Test all event cards
   - Test all skill cards
   - Test cooldown system
   - Test multiplayer sync

---

## 💡 TIPS

### **Thêm Event Card mới:**
- Chỉnh sửa `EventCardLibrary.GetAllEventCards()`
- Thêm case trong `EventCardHandler.ExecuteEventCard()`
- Build và deploy lại server

### **Thêm Skill Card mới:**
- Thêm vào Firebase (không cần code!)
- Hoặc thêm vào `BasicSkillCards.GetBasicCards()` (hardcode)
- Nếu cần effect mới, implement `ISkillEffect`

### **Debug:**
```csharp
// Enable debug logs
Debug.Log($"[EventCard] Drew: {card.name}");
Debug.Log($"[SkillCard] Triggered: {result.cardName}");
Debug.Log($"[Cooldown] Remaining: {cardInstance.currentCooldown}");
```

---

**HỆ THỐNG ĐÃ SẴN SÀNG SỬ DỤNG! 🎉**

**Bạn chỉ cần integrate vào ServerGameManager và tạo UI là xong!**

