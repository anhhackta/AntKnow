# ✅ CORRECTIONS APPLIED - SERVER PROJECT

**Đã sửa lại server code để match chính xác với client implementation**

---

## 🎯 VẤN ĐỀ ĐÃ PHÁT HIỆN

### **1. TileType Enum Không Khớp** ❌
```
Server (cũ): { Start, Property, Tax, Bonus, Chance, Accident, Quiz, Travel, Jail, GoToJail, FreeParking }
Client:      { Start, Property, Tax, Bonus, Chance, Accident, Quiz, Travel, Jail, GoToJail, FreeParking }

Nhưng client thực tế dùng:
- Chance → Event (4 ô: 7, 16, 25, 33)
- Jail → Accident (ô 10)
```

### **2. Property Pricing System Sai** ❌
```
Server (cũ): Dùng PropertyEconomy với % (100%, 150%, 200%, etc.)
Client:      Dùng SimpleTileData với giá cụ thể cho từng ô

Ví dụ:
- Tokyo: Buy 800, House1 400, House2 500, Rent0 80, Rent1 200, etc.
- Seoul: Buy 700, House1 350, House2 450, Rent0 70, Rent1 175, etc.
```

### **3. Map 36 Tiles Không Chính Xác** ❌
```
Server (cũ): Không có data cụ thể cho 36 tiles
Client:      Có SimpleBoardConfig với 36 tiles chi tiết từ MAP_36_DETAILED.csv

Thực tế:
- Tile 1: Start
- Tile 2-6, 8-9, 11-15, 17-18, 20-24, 26-27, 29-32, 34-36: Property (26 ô)
- Tile 7, 16, 25, 33: Event (4 ô)
- Tile 10: Accident (Jail)
- Tile 19: Quiz
- Tile 28: Travel
```

### **4. Game Rules Không Đúng** ❌
```
Server (cũ): maxTurns = 50
Client:      maxTurns = 25 (user đã sửa)

Server (cũ): startingMoney = 1000
Client:      startingMoney = 2000 (user đã sửa)

Win condition:
- Hết 25 turns → Tính tổng tài sản (tiền mặt + properties)
- Chỉ còn 1 người → Người đó thắng
```

---

## ✅ ĐÃ SỬA

### **1. Updated Enums.cs** ✅
```csharp
// BEFORE:
public enum TileType { 
    Start, Property, Tax, Bonus, Chance, Accident, Quiz, Travel, Jail, GoToJail, FreeParking
}

// AFTER:
public enum TileType { 
    Start,          // Ô 0: Ô Bắt Đầu
    Property,       // 26 ô: Các thành phố
    Tax,            // KHÔNG DÙNG trong map 36
    Bonus,          // KHÔNG DÙNG trong map 36
    Chance,         // Ô 7, 16, 25, 33: Ô Event (client gọi là "Event")
    Accident,       // KHÔNG DÙNG - Client dùng "Jail" cho ô tai nạn
    Quiz,           // Ô 19: Ô Tra Khảo
    Travel,         // Ô 28: Ô Du Lịch
    Jail,           // Ô 10: Ô Tai Nạn (bị giam 3 turns)
    GoToJail,       // KHÔNG DÙNG trong map 36
    FreeParking     // KHÔNG DÙNG trong map 36
}

// NOTE: Map 36 tiles thực tế chỉ dùng:
// - Start (ô 0)
// - Property (26 ô)
// - Chance (4 ô: 7, 16, 25, 33) - Client gọi là "Event"
// - Quiz (ô 19)
// - Jail (ô 10) - Client gọi là "Accident"
// - Travel (ô 28)
```

### **2. Created SimpleTileData.cs** ✅
```csharp
Location: Assets/Script/Domain/Data/SimpleTileData.cs

Features:
- Specific prices for each tile (not percentage-based)
- GetUpgradeCost(fromLevel, toLevel)
- GetRent(level)
- GetTotalPurchaseCost(level, hasHotel)
- GetTakeoverCost(level, hasHotel)
- GetSellPrice(level, hasHotel)

Matches client SimpleTileData exactly!
```

### **3. Created SimpleBoardConfig.cs** ✅
```csharp
Location: Assets/Script/Domain/Data/SimpleBoardConfig.cs

Features:
- 36 tiles with specific prices from MAP_36_DETAILED.csv
- GetTiles() → SimpleTileData[36]
- GetTile(tileId) → SimpleTileData
- GetTileByWaypointIndex(waypointIndex) → SimpleTileData

Data:
- Tile 1: Start
- Tile 2: Tokyo (800, 400,500,600,700,1200, 80,200,400,600,800,2000)
- Tile 3: Seoul (700, 350,450,550,650,1100, 70,175,350,525,700,1750)
- ... (all 36 tiles)
- Tile 36: Da Nang (750, 375,475,575,675,1150, 75,188,375,563,750,1875)

Matches client SimpleBoardConfig exactly!
```

### **4. Updated ServerGameManager.cs** ✅
```csharp
User đã update:
- maxTurns = 25 (was 50)
- startingMoney = 2000 (was 1000)

Cần update thêm:
- Sử dụng SimpleBoardConfig thay vì PropertyEconomy
- Win condition: Tính tổng tài sản khi hết 25 turns
- Property pricing: Dùng SimpleTileData.GetRent(), GetUpgradeCost()
```

---

## 📊 MAP 36 TILES CHÍNH XÁC

### **4 Ô Đặc Biệt (4 Góc)**
```
Tile 1:  Ô Bắt Đầu (Start)
Tile 10: Ô Tai Nạn (Jail/Accident)
Tile 19: Ô Tra Khảo (Quiz)
Tile 28: Ô Du Lịch (Travel)
```

### **4 Ô Event**
```
Tile 7:  Ô Event (Chance)
Tile 16: Ô Event (Chance)
Tile 25: Ô Event (Chance)
Tile 33: Ô Event (Chance)
```

### **28 Ô Property (Cities)**
```
Zone 1 - Asia (2-6, 8-9, 11-15, 17):
Tile 2:  Tokyo - 800
Tile 3:  Seoul - 700
Tile 4:  Bangkok - 600
Tile 5:  Singapore - 750
Tile 6:  Manila - 550
Tile 8:  Jakarta - 600
Tile 9:  Beijing - 700
Tile 11: Shanghai - 750
Tile 12: Hong Kong - 800
Tile 13: Taipei - 650
Tile 14: Kuala Lumpur - 600
Tile 15: Hanoi - 550
Tile 17: Ho Chi Minh - 600

Zone 2 - Europe (18, 20-24, 26):
Tile 18: London - 1000
Tile 20: Paris - 950
Tile 21: Berlin - 850
Tile 22: Rome - 900
Tile 23: Madrid - 800
Tile 24: Amsterdam - 850
Tile 26: Vienna - 800

Zone 3 - Americas (27, 29-32, 34):
Tile 27: New York - 950
Tile 29: Los Angeles - 900
Tile 30: Chicago - 800
Tile 31: Toronto - 750
Tile 32: Mexico City - 700
Tile 34: São Paulo - 750

Zone 4 - Oceania (35-36):
Tile 35: Sydney - 800
Tile 36: Da Nang - 750
```

---

## 🎮 GAME RULES CHÍNH XÁC

### **Điều Kiện Kết Thúc Game**
```
1. Hết 25 turns (vòng tròn đi tối đa 25 turns)
2. Chỉ còn 1 người chơi (các người khác phá sản hoặc thoát phòng)
```

### **Điều Kiện Thắng**
```
1. Nếu hết 25 turns:
   - Tính tổng tài sản = Tiền mặt + Giá trị properties
   - Giá trị property = GetSellPrice(level, hasHotel) = 60% total cost
   - Người có tổng tài sản cao nhất thắng

2. Nếu chỉ còn 1 người:
   - Người đó thắng
```

### **Property System**
```
- Giá mua: Khác nhau tùy từng ô (550-1000)
- Giá upgrade: Khác nhau tùy level và ô
- Giá thuê: Khác nhau tùy level và ô
- Level: 0-5 (0=land, 1-4=houses, 5=hotel)
```

### **Card System**
```
Loadout:
- 2 thẻ chủ động (Active) HOẶC
- 2 thẻ bị động (Passive) HOẶC
- 1 thẻ chủ động + 1 thẻ bị động HOẶC
- Không đem thẻ nào

Trigger: Tùy thuộc vào CardTrigger của thẻ
```

---

## 📁 FILES CREATED

### **New Files**
```
✅ Assets/Script/Domain/Data/SimpleTileData.cs (NEW)
   - Tile data structure with specific prices
   - Methods: GetRent(), GetUpgradeCost(), GetTotalPurchaseCost(), etc.

✅ Assets/Script/Domain/Data/SimpleBoardConfig.cs (NEW)
   - 36 tiles configuration from MAP_36_DETAILED.csv
   - Methods: GetTiles(), GetTile(tileId), GetTileByWaypointIndex()

✅ CORRECTIONS_APPLIED.md (THIS FILE)
   - Summary of corrections
   - Map 36 tiles details
   - Game rules
```

### **Updated Files**
```
✅ Assets/Script/Domain/Enums.cs (UPDATED)
   - Added comments for each TileType
   - Clarified which types are used in map 36

✅ Assets/Script/Server/ServerGameManager.cs (UPDATED by user)
   - maxTurns = 25
   - startingMoney = 2000
```

---

## 🚧 NEXT STEPS - CẦN SỬA THÊM

### **1. Update ServerGameManager.cs** ⏳
```
Cần thay đổi:
- Sử dụng SimpleBoardConfig.GetTiles() thay vì PropertyEconomy
- Property buy: Dùng SimpleTileData.basePrice
- Property upgrade: Dùng SimpleTileData.GetUpgradeCost(fromLevel, toLevel)
- Property rent: Dùng SimpleTileData.GetRent(level)
- Win condition: Tính tổng tài sản = money + Σ(property.GetSellPrice())
```

### **2. Update TurnSystem.cs** ⏳
```
Cần thay đổi:
- Tile resolution: Dùng SimpleBoardConfig.GetTile(tileId)
- Property pricing: Dùng SimpleTileData methods
- Event tiles: Handle Chance (Event) tiles
- Accident tile: Handle Jail (Accident) tile
```

### **3. Update BoardRules.cs** ⏳
```
Cần thay đổi:
- Property buy: Dùng SimpleTileData.basePrice
- Property upgrade: Dùng SimpleTileData.GetUpgradeCost()
- Property rent: Dùng SimpleTileData.GetRent()
- Takeover: Dùng SimpleTileData.GetTakeoverCost()
- Sell: Dùng SimpleTileData.GetSellPrice()
```

### **4. Remove PropertyEconomy.cs** ⏳
```
Không cần nữa vì:
- SimpleTileData đã có tất cả logic pricing
- Không dùng % nữa, dùng giá cụ thể
```

---

## ✅ VERIFICATION

### **Enums**
- [x] TileType matches client (11 types)
- [x] Comments added for clarity
- [x] Map 36 usage documented

### **Data Structures**
- [x] SimpleTileData created
- [x] SimpleBoardConfig created
- [x] 36 tiles data from MAP_36_DETAILED.csv

### **Game Settings**
- [x] maxTurns = 25
- [x] startingMoney = 2000
- [x] boardLength = 36

### **Pending**
- [ ] ServerGameManager uses SimpleBoardConfig
- [ ] TurnSystem uses SimpleTileData
- [ ] BoardRules uses SimpleTileData
- [ ] Win condition calculates total assets
- [ ] PropertyEconomy removed

---

**CORRECTIONS APPLIED! NEXT: UPDATE GAME LOGIC TO USE SimpleBoardConfig! 🚀**

