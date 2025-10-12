# 🎮 KẾ HOẠCH TRIỂN KHAI CHI TIẾT GAMESCENE

## 📋 YÊU CẦU TỔNG QUAN

### **UI Panels - Luôn Hiển Thị**
1. **PanelGame** (Container)
   - **PanelMe**: Name + Money của người chơi chính
   - **PanelPlayer**: VerticalLayoutGroup chứa các PanelPlayerPrefab (người chơi khác)
   
2. **PanelGameInfo**
   - Text Turn: X/25 (giảm sau mỗi vòng - tất cả người chơi đi xong)
   - Text Time: MM:SS (đếm từ lúc bắt đầu)
   - Text CurrentPlayer: Tên người đang đi

3. **PanelRoll**
   - 2 GameObject: Dice1, Dice2 (6 sprites tương ứng 1-6)
   - Text Result: "6", "8 (Đôi)"
   - Button Roll: Sáng khi đến lượt, mờ khi chưa đến

### **UI Panels - Kích Hoạt Khi Cần**
4. **PanelInfo**
   - Image Gender (2 sprites nam/nữ)
   - Text Name
   - Text Số trận chơi
   - Text Số trận thắng
   - Button Close
   - Kích hoạt: Click vào PanelMe hoặc PanelPlayerPrefab

5. **PanelBuy**
   - Text PropertyName (tên ô đất)
   - Text Price (tiền cần trả)
   - 4 Buttons House (1-4): Toggle chọn/bỏ chọn
   - Button Buy (sáng khi đủ tiền)
   - Button Skip
   - **Logic**:
     - Ô trống: Chọn 1-4 nhà, tính tiền đất + nhà
     - Ô của mình: Nhà đã mua mờ đi, chỉ nâng cấp thêm

6. **PanelQuiz**
   - Text Question
   - Text Difficulty
   - Text Timer (15s)
   - 4 Buttons Options (0-3)
   - **Logic**:
     - Vào ô Quiz: Chỉ người đó
     - Mỗi 8 turns: Tất cả người chơi
     - Đúng: Không phạt
     - Sai: Fortune Wheel (trừ tiền/hạ nhà/không làm gì)

7. **PanelEvent**
   - Text Event Info
   - Button OK
   - Auto ẩn sau 3s

8. **PanelHouseSell**
   - ScrollView: Content với PropertySellItemPrefab
     - Toggle
     - Text PropertyName
     - Text Level (House 0/1/2/3/4 hoặc Hotel)
     - Text SellPrice (60% giá mua)
   - Button Sell (sáng khi đủ tiền trả)

9. **PanelResult**
   - Hiển thị Top 1/2/3/4 (tùy số người)
   - Text Name
   - Text Money (tiền mặt + tài sản 100%)
   - Text Reward (AntCoin + EXP từ Cloud Function)

10. **PanelNotification**
    - Text thông báo nhanh (1s rồi tắt)
    - VD: "Player X đi trước", "Hết 25 turns", etc.

---

## 🎮 GAMEPLAY FLOW

### **1. Khởi Đầu**
- Tất cả người chơi spawn tại Waypoint 0 (ô Start)
- Chọn người đi trước (roll xúc xắc)
- Hiển thị thứ tự qua PanelNotification

### **2. Turn System**
- CurrentPlayer roll dice
- Di chuyển bằng bounce effect (hướng vào giữa map)
- Resolve tile:
  - **Ô trống**: PanelBuy (mua đất + nhà)
  - **Ô của mình**: PanelBuy (nâng cấp)
  - **Ô người khác**: Trả tiền thuê (nếu không đủ → PanelHouseSell)
  - **Ô Quiz**: PanelQuiz
  - **Ô Event**: PanelEvent
  - **Ô Start**: Nhận lương + HP
  - **Ô Jail**: Nhốt 2 turns
  - **Ô Travel**: Trả 100 đi nơi khác
- Kết thúc turn → Next player

### **3. Quiz System**
- **Ô Quiz**: Chỉ người đó
- **8 Turns**: Tất cả người chơi
- **Trả lời sai**: Fortune Wheel
  - 1/3: Trừ tiền random
  - 1/3: Hạ 1 nhà bất kỳ
  - 1/3: Không làm gì

### **4. Kết Thúc**
- Điều kiện:
  - Hết 25 turns
  - Chỉ còn 1 người không phá sản
- PanelResult hiển thị
- Tính reward (Cloud Function)

---

## 🏗️ CẤU TRÚC SCENE

### **Map Tiles**
```
MapTile (Cube)
├── Platform (Cube mỏng - 0.8x0.1x0.8)
│   └── Houses/Hotel spawn tại đây
├── TextName (TMP)
└── TextPrice (TMP)
```

**House Placement:**
- Z: Hướng vào trong (center)
- Y: Hướng lên (stack)
- X: Bên trái (side-by-side)

### **Player Prefab**
```
PlayerPrefab
├── PlayerGameController (Script)
├── ModelParent
│   ├── MaleModel (Gender = male)
│   └── FemaleModel (Gender = female)
├── TurnIndicator (Sphere - hiện khi đến lượt)
└── NetworkObject
```

### **Material System**
- House/Hotel: Tìm material có tên "ngói"
- Đổi màu theo owner:
  - Player 1: Red (1, 0.2, 0.2)
  - Player 2: Blue (0.2, 0.5, 1)
  - Player 3: Green (0.2, 1, 0.2)
  - Player 4: Yellow (1, 1, 0.2)

---

## 📝 BƯỚC TRIỂN KHAI

### **Phase 1: Refactor UI Panels (Ưu tiên)**
- [ ] 1.1: PanelGame (PanelMe + PanelPlayer)
- [ ] 1.2: PanelGameInfo (Turn/Time/CurrentPlayer)
- [ ] 1.3: PanelRoll (Dice animation)
- [ ] 1.4: PanelInfo (Player info popup)
- [ ] 1.5: PanelBuy (Buy/Upgrade properties)
- [ ] 1.6: PanelQuiz (Quiz system + Fortune Wheel)
- [ ] 1.7: PanelEvent (Event cards)
- [ ] 1.8: PanelHouseSell (Sell properties)
- [ ] 1.9: PanelResult (Game end)
- [ ] 1.10: PanelNotification (Quick messages)

### **Phase 2: Core Gameplay**
- [ ] 2.1: Turn System (Host-authoritative)
- [ ] 2.2: Dice Roll (Luck system)
- [ ] 2.3: Movement (Bounce effect)
- [ ] 2.4: Tile Resolution
- [ ] 2.5: Property System (Buy/Upgrade/Rent)

### **Phase 3: Advanced Features**
- [ ] 3.1: Quiz System (Firebase integration)
- [ ] 3.2: Event System (Random cards)
- [ ] 3.3: Fortune Wheel (Penalties)
- [ ] 3.4: Bankruptcy System

### **Phase 4: Network Sync**
- [ ] 4.1: Turn sync (RPCs)
- [ ] 4.2: Property sync (NetworkVariables)
- [ ] 4.3: UI sync (ClientRpc)
- [ ] 4.4: Game state sync

### **Phase 5: Polish & Testing**
- [ ] 5.1: Visual polish (animations, VFX)
- [ ] 5.2: Sound effects
- [ ] 5.3: Multiplayer testing
- [ ] 5.4: Bug fixes

---

## 🚀 BẮT ĐẦU

**Bước đầu tiên**: Refactor các UI Panels theo đúng yêu cầu.

Tôi sẽ bắt đầu với:
1. **PanelGame** và các panel con
2. **PanelGameInfo** (Turn/Time/CurrentPlayer)
3. **PanelRoll** (Dice system)

Sau đó kết nối với GameManager để test.

---

**Status**: Ready to implement! 🎮
**Date**: 2025-10-12
