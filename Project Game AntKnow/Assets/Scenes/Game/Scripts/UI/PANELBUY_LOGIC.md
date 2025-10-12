# 🏠 PANELBUY - LOGIC MUA/NÂNG CẤP NHÀ

## 📋 CẤU TRÚC UI

### **Buttons trong PanelBuy:**
```
PanelBuy
├── TextPropertyName (Tên ô đất)
├── TextOwnerName (Chủ sở hữu)
├── TextPrice (Giá tiền)
├── House Buttons:
│   ├── BtnHouse1 (House 1) - Level 1
│   ├── BtnHouse2 (House 2) - Level 2
│   ├── BtnHouse3 (House 3) - Level 3
│   ├── BtnHouse4 (House 4) - Level 4
│   └── BtnHotel (Hotel) - Level 5 ⭐
├── BtnBuy
└── BtnSkip
```

---

## 🎯 LOGIC HOẠT ĐỘNG

### **Case 1: Ô Trống (chưa có chủ)**
**Điều kiện:** `currentLevel = 0`

**Buttons State:**
- ✅ House 1-4: **ENABLED** (có thể chọn)
- ❌ Hotel: **DISABLED** (mờ đi, không thể chọn)

**Flow:**
1. Player đứng trên ô trống
2. Có thể chọn **House 1, 2, 3, hoặc 4**
3. Click button → Sáng xanh (selected)
4. Click lại → Tắt (deselected)
5. TextPrice hiển thị: `"Tổng: [giá đất + giá nhà]"`
6. BtnBuy sáng nếu đủ tiền

**Ví dụ:**
```
Chọn House 2:
- Giá đất: 100
- Giá House 1: 100
- Giá House 2: 100
→ Tổng: 300 (đất + 2 nhà)
```

---

### **Case 2: Ô Của Mình (Nâng Cấp)**
**Điều kiện:** `currentLevel = 1, 2, 3, hoặc 4`

#### **2.1 Có House 1-3 (currentLevel < 4)**
**Buttons State:**
- ❌ House 1-N (đã mua): **DISABLED** (mờ đi)
- ✅ House (N+1) - 4: **ENABLED** (có thể mua thêm)
- ❌ Hotel: **DISABLED** (chưa đủ 4 houses)

**Ví dụ currentLevel = 2 (có 2 houses):**
- ❌ House 1: DISABLED
- ❌ House 2: DISABLED
- ✅ House 3: ENABLED
- ✅ House 4: ENABLED
- ❌ Hotel: DISABLED

**Flow:**
1. Chọn House 3 hoặc 4
2. TextPrice: `"Nâng cấp: +[giá nhà]"`
3. BtnBuy sáng nếu đủ tiền

---

#### **2.2 Có House 4 (currentLevel = 4)** ⭐
**Buttons State:**
- ❌ House 1-4: **DISABLED** (đã mua hết)
- ✅ **Hotel: ENABLED** (CÓ THỂ MUA!)

**Flow:**
1. Chỉ có button Hotel sáng lên
2. Click Hotel → Sáng xanh
3. TextPrice: `"Nâng cấp Hotel: +[giá hotel]"`
4. BtnBuy sáng nếu đủ tiền
5. Buy → Replace 4 houses bằng 1 hotel

---

### **Case 3: Đã Có Hotel (currentLevel = 5)**
**Buttons State:**
- ❌ Tất cả buttons: **DISABLED**
- Panel không hiện (không thể nâng cấp thêm)

---

## 💰 TÍNH GIÁ

### **Giá Đất:**
- Chỉ tính **1 lần duy nhất** khi mua lần đầu (currentLevel = 0)
- `giá đất = basePrice`

### **Giá Houses:**
```csharp
House 1: basePrice
House 2: basePrice
House 3: basePrice
House 4: basePrice
```

**Ví dụ basePrice = 100:**
- Mua đất + House 1: 100 + 100 = **200**
- Mua đất + House 2: 100 + 100 + 100 = **300**
- Mua đất + House 4: 100 + 100 + 100 + 100 + 100 = **500**

### **Giá Hotel:**
```csharp
Hotel: basePrice * 4
```

**Ví dụ basePrice = 100:**
- Nâng cấp từ House 4 → Hotel: 100 * 4 = **400**
- Tổng đầu tư: 500 (4 houses) + 400 (hotel) = **900**

---

## 🎨 MÀU SẮC BUTTONS

### **Colors:**
- **Normal**: White (1, 1, 1) - Chưa chọn, có thể click
- **Selected**: Green (0, 1, 0) - Đang chọn
- **Disabled**: Gray (0.5, 0.5, 0.5) - Đã mua hoặc chưa đủ điều kiện
- **Cannot Afford**: Red (1, 0, 0) - Không đủ tiền (TextPrice màu đỏ)

### **Button States:**
```
[✅ House 1] → Normal (white) → Click → [🟢 House 1] Selected (green)
[❌ House 1] → Disabled (gray) - Không thể click
```

---

## 📝 FLOW MUA NHÀ

### **Ô Trống:**
```
1. Panel hiện
2. House 1-4 ENABLED, Hotel DISABLED
3. Player chọn House N (1-4)
4. Button sáng xanh
5. TextPrice: "Tổng: [đất + nhà]"
6. Click Buy
7. GameManager: BuyProperty(tileIndex, playerIdx, basePrice, player)
8. PropertyManager: UpgradeProperty(tileIndex, N, basePrice, player)
9. PropertyVisual: Spawn N houses trên tile
10. Panel hide
```

### **Nâng Cấp (currentLevel < 4):**
```
1. Panel hiện
2. House đã mua DISABLED, House chưa mua ENABLED, Hotel DISABLED
3. Player chọn House N (> currentLevel)
4. Button sáng xanh
5. TextPrice: "Nâng cấp: +[giá]"
6. Click Buy
7. PropertyManager: UpgradeProperty(tileIndex, N, basePrice, player)
8. PropertyVisual: Spawn thêm houses
9. Panel hide
```

### **Nâng Cấp Hotel (currentLevel = 4):**
```
1. Panel hiện
2. House 1-4 DISABLED, Hotel ENABLED ⭐
3. Player chọn Hotel
4. Button Hotel sáng xanh
5. TextPrice: "Nâng cấp Hotel: +[giá x4]"
6. Click Buy
7. PropertyManager: UpgradeProperty(tileIndex, 5, basePrice, player)
8. PropertyVisual: Destroy 4 houses → Spawn 1 hotel
9. Panel hide
```

---

## 🔧 CODE LOGIC

### **UpdateHouseButtons():**
```csharp
// House buttons
SetButtonState(btnHouse1, currentLevel < 1, selectedLevel == 1);
SetButtonState(btnHouse2, currentLevel < 2, selectedLevel == 2);
SetButtonState(btnHouse3, currentLevel < 3, selectedLevel == 3);
SetButtonState(btnHouse4, currentLevel < 4, selectedLevel == 4);

// Hotel button - CHỈ ENABLE KHI currentLevel = 4
SetButtonState(btnHotel, currentLevel >= 4, selectedLevel == 5);
```

### **CalculateTotalPrice():**
```csharp
int total = 0;

// Giá đất (nếu mua lần đầu)
if (currentLevel == 0)
    total += basePrice;

// Giá houses (từ currentLevel+1 đến selectedLevel)
for (int i = currentLevel + 1; i <= selectedLevel && i <= 4; i++)
    total += basePrice; // Mỗi nhà = basePrice

// Giá hotel (nếu chọn hotel)
if (selectedLevel == 5)
    total += basePrice * 4; // Hotel = basePrice * 4

return total;
```

---

## ✅ SUMMARY

### **Quy Tắc Chính:**
1. ✅ **Ô trống**: House 1-4 ENABLED, Hotel DISABLED
2. ✅ **Có 1-3 houses**: Nhà đã mua DISABLED, nhà chưa mua ENABLED, Hotel DISABLED
3. ✅ **Có 4 houses**: House 1-4 DISABLED, **Hotel ENABLED** ⭐
4. ✅ **Có hotel**: Tất cả DISABLED (không nâng cấp thêm)

### **Giá:**
- Đất: `basePrice` (1 lần)
- House: `basePrice` (mỗi nhà)
- Hotel: `basePrice * 4`

### **Visual:**
- Normal: White
- Selected: Green
- Disabled: Gray
- Cannot Afford: Red (text)

---

**Version**: 2.0 (Fixed - Added Hotel Button)  
**Date**: 2025-10-12  
**Status**: ✅ CORRECT LOGIC
