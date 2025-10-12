# ✅ CHECKLIST TRIỂN KHAI GAMESCENE

## 🎯 PHASE 1: SETUP SCENE (2-3 giờ)

### **Canvas UI Setup**
- [ ] Mở GameScene.unity trong Unity Editor
- [ ] Kiểm tra Canvas đã có chưa (nếu chưa thì Create > UI > Canvas)

#### **PanelGame (Luôn hiện)**
- [ ] Tạo Panel "PanelGame"
- [ ] Add script `PanelGame.cs`
- [ ] Tạo con "PanelMe":
  - [ ] Add script `PanelPlayerMe.cs`
  - [ ] Add Button component
  - [ ] Tạo Image "ImageAvatar"
  - [ ] Tạo 2 TextMeshProUGUI: "TextName", "TextMoney"
  - [ ] Import 2 sprites: Male, Female avatar
  - [ ] Assign sprites vào script
- [ ] Tạo con "PanelPlayerContainer":
  - [ ] Add VerticalLayoutGroup (spacing: 10)
- [ ] Tạo Prefab "PanelPlayerPrefab":
  - [ ] Add script `PanelPlayer.cs`
  - [ ] Add Button component
  - [ ] Add Image "ImageAvatar"
  - [ ] Add 2 TextMeshProUGUI: "TextName", "TextMoney"
  - [ ] Assign sprites (Male, Female)
  - [ ] Lưu vào Assets/Prefabs/UI/
- [ ] Assign PanelPlayerPrefab vào PanelGame Inspector

#### **PanelGameInfo (Luôn hiện)**
- [ ] Tạo Panel "PanelGameInfo"
- [ ] Add script `PanelGameInfo.cs`
- [ ] Tạo 3 TextMeshProUGUI:
  - [ ] "TextTurn" (text: "Turn: 1/25")
  - [ ] "TextTime" (text: "Time: 00:00")
  - [ ] "TextCurrentPlayer" (text: "Current: -")
- [ ] Assign vào script Inspector
- [ ] Set Max Turns = 25

#### **PanelRoll (Luôn hiện)**
- [ ] Tạo Panel "PanelRoll"
- [ ] Add script `PanelRoll.cs`
- [ ] Tạo 2 Image: "Dice1", "Dice2"
- [ ] Tạo 1 TextMeshProUGUI: "TextResult" (text: "0")
- [ ] Tạo 1 Button: "BtnRoll" (text: "Roll")
- [ ] Import 6 dice sprites (faces 1-6)
- [ ] Assign sprites vào Dice Sprites array (size 6)
- [ ] Assign components vào script Inspector
- [ ] Set Roll Duration = 1.5, Frame Interval = 0.1

#### **PanelInfo (Ẩn)**
- [ ] Tạo Panel "PanelInfo"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelInfo.cs`
- [ ] Tạo Image "ImageGender"
- [ ] Tạo 3 TextMeshProUGUI:
  - [ ] "TextPlayerName"
  - [ ] "TextMatchesPlayed" (text: "Số trận chơi: 0")
  - [ ] "TextMatchesWon" (text: "Số trận thắng: 0")
- [ ] Tạo Button "BtnClose" (text: "Close")
- [ ] Import 2 sprites: Male, Female
- [ ] Assign components + sprites vào script

#### **PanelBuy (Ẩn)**
- [ ] Tạo Panel "PanelBuy"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelBuy.cs`
- [ ] Tạo 3 TextMeshProUGUI:
  - [ ] "TextPropertyName" (text: "Property")
  - [ ] "TextOwnerName" (text: "Owner")
  - [ ] "TextPrice" (text: "Price: 0")
- [ ] Tạo 5 Buttons với text:
  - [ ] "BtnHouse1" (text: "House 1")
  - [ ] "BtnHouse2" (text: "House 2")
  - [ ] "BtnHouse3" (text: "House 3")
  - [ ] "BtnHouse4" (text: "House 4")
  - [ ] "BtnHotel" (text: "Hotel") - **Chỉ enable khi có House 4**
- [ ] Tạo 2 Buttons:
  - [ ] "BtnBuy" (text: "Buy")
  - [ ] "BtnSkip" (text: "Skip")
- [ ] Assign components vào script
- [ ] Set colors:
  - Normal: White (1,1,1)
  - Selected: Green (0,1,0)
  - Disabled: Gray (0.5,0.5,0.5)
  - Cannot Afford: Red (1,0,0)

#### **PanelQuiz (Ẩn)**
- [ ] Tạo Panel "PanelQuiz"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelQuiz.cs`
- [ ] Tạo 3 TextMeshProUGUI:
  - [ ] "TextQuestion"
  - [ ] "TextDifficulty"
  - [ ] "TextTimer" (text: "15")
- [ ] Tạo 4 Buttons với text:
  - [ ] "BtnOption1" (Option A)
  - [ ] "BtnOption2" (Option B)
  - [ ] "BtnOption3" (Option C)
  - [ ] "BtnOption4" (Option D)
- [ ] Tạo GameObject "FortuneWheel" (ẩn)
- [ ] Assign components vào script
- [ ] Set Timer Duration = 15

#### **PanelEvent (Ẩn)**
- [ ] Tạo Panel "PanelEvent"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelEvent.cs`
- [ ] Tạo TextMeshProUGUI "TextEventInfo"
- [ ] Tạo Button "BtnOK" (text: "OK")
- [ ] Assign components vào script
- [ ] Set Auto Hide Delay = 3

#### **PanelHouseSell (Ẩn)**
- [ ] Tạo Panel "PanelHouseSell"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelHouseSell.cs`
- [ ] Tạo Scroll View:
  - [ ] Content với VerticalLayoutGroup
- [ ] Tạo Prefab "PropertySellItemPrefab":
  - [ ] Add Toggle
  - [ ] Add 3 TextMeshProUGUI: PropertyName, Level, SellPrice
  - [ ] Add script `PropertySellItem.cs`
  - [ ] Lưu vào Assets/Prefabs/UI/
- [ ] Tạo Button "BtnSell" (text: "Sell")
- [ ] Assign components + prefab vào script

#### **PanelResult (Ẩn)**
- [ ] Tạo Panel "PanelResult"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelResult.cs`
- [ ] Tạo TextMeshProUGUI "Title" (text: "KẾT QUẢ")
- [ ] Tạo 4 RankItem GameObjects:
  - [ ] Mỗi item có 4 TextMeshProUGUI: Rank, Name, Money, Reward
- [ ] Tạo Button "BtnBackToMenu"
- [ ] Assign components vào script

#### **PanelNotification (Ẩn)**
- [ ] Tạo Panel "PanelNotification"
- [ ] **SetActive(false)** trong Inspector
- [ ] Add script `PanelNotification.cs`
- [ ] Tạo TextMeshProUGUI "TextNotification"
- [ ] Assign vào script
- [ ] Set Display Duration = 1

---

### **Player Prefabs Setup**

#### **PlayerPrefabMale**
- [ ] Import male humanoid model
- [ ] Tạo prefab "PlayerPrefabMale"
- [ ] Add script `PlayerGameController.cs`
- [ ] Add `NetworkObject` component
- [ ] Tạo child "ModelParent" (Empty)
- [ ] Add male model vào ModelParent
- [ ] Add Animator component vào model
- [ ] Tạo child "TurnIndicator" (Empty):
  - [ ] Add script `TurnIndicator.cs`
  - [ ] Tạo child "Sphere" (Primitive Sphere)
  - [ ] Scale sphere: (0.3, 0.3, 0.3)
  - [ ] Material: Yellow color
- [ ] Assign components trong PlayerGameController:
  - [ ] Male Model
  - [ ] Model Parent
  - [ ] Turn Indicator
  - [ ] Move Speed: 5
  - [ ] Bounce Height: 0.5
  - [ ] Bounce Duration: 0.3
- [ ] Lưu prefab vào Assets/Prefabs/Players/

#### **PlayerPrefabFemale**
- [ ] Import female humanoid model
- [ ] Tạo prefab "PlayerPrefabFemale" (giống male)
- [ ] Thay male model bằng female model
- [ ] Lưu prefab vào Assets/Prefabs/Players/

---

### **Map & Tiles Setup**

#### **Create 36 Tiles**
- [ ] Tạo GameObject "Map" (Empty parent)
- [ ] Tạo Tile template:
  - [ ] Create Cube (Scale: 2, 0.5, 2)
  - [ ] Add script `TileVisual.cs`
  - [ ] Tạo child "Platform":
    - [ ] Cube (Scale: 0.8, 0.1, 0.8)
    - [ ] Position: Trên đỉnh tile
    - [ ] Tag: "Platform"
  - [ ] Tạo 2 TextMeshPro - WorldSpace:
    - [ ] "TextName" (tên ô)
    - [ ] "TextPrice" (giá)
  - [ ] Assign components vào TileVisual
- [ ] Duplicate 36 tiles
- [ ] Rename: Tile0 - Tile35
- [ ] Đặt theo vị trí board game (hình vuông)
- [ ] Set Tile Index cho mỗi tile (0-35)

#### **Create 36 Waypoints**
- [ ] Tạo GameObject "Waypoints" (Empty parent)
- [ ] Tạo 36 Empty GameObjects:
  - [ ] Rename: Waypoint0 - Waypoint35
  - [ ] Đặt ở giữa mỗi tile (để player đứng)
- [ ] Đảm bảo thứ tự đúng (0-35 theo chiều kim đồng hồ)

#### **House & Hotel Prefabs**
- [ ] Import house 3D models (4 variants)
- [ ] Tạo prefab "HousePrefab":
  - [ ] Add MeshRenderer
  - [ ] Tạo material có tên "ngói" (roof material)
  - [ ] Scale: Vừa với platform
- [ ] Tạo prefab "HotelPrefab" (tương tự, lớn hơn)
- [ ] Lưu vào Assets/Prefabs/Properties/

---

### **Managers Setup**

#### **GameManager**
- [ ] Tạo GameObject "GameManager"
- [ ] Add `NetworkObject` component
- [ ] Add script `GameManager.cs`
- [ ] Assign trong Inspector:
  - **Managers:**
    - [ ] Board Manager
    - [ ] Panel Roll
    - [ ] Property Manager
  - **Players:**
    - [ ] Player Prefab Male
    - [ ] Player Prefab Female
  - **UI:**
    - [ ] Roll Button (from PanelRoll)
    - [ ] Turn Text (from PanelGameInfo)
    - [ ] Current Player Text (from PanelGameInfo)
    - [ ] Time Text (from PanelGameInfo)
  - **UI Panels:**
    - [ ] Panel Buy
    - [ ] Panel Quiz
    - [ ] Panel Event
    - [ ] Panel House Sell
    - [ ] Panel Result
    - [ ] Panel Card (if exists)
  - **Game Settings:**
    - [ ] Max Turns: 25
    - [ ] Demo Mode: true (for testing)
  - **Services:**
    - [ ] Firebase Auth Service

#### **BoardManager**
- [ ] Tạo GameObject "BoardManager"
- [ ] Add script `BoardManager.cs`
- [ ] Assign Waypoints array (36 transforms)
- [ ] Set Show Debug Info: true

#### **PropertyManager**
- [ ] Tạo GameObject "PropertyManager"
- [ ] Add script `PropertyManager.cs`
- [ ] Tạo GameObject "PropertyVisual"
- [ ] Add script `PropertyVisual.cs`
- [ ] Assign trong PropertyVisual:
  - [ ] House Prefab
  - [ ] Hotel Prefab
  - [ ] Roof Material Name: "ngói"
  - [ ] Player Colors (4 colors):
    - Player 1: (1, 0.2, 0.2)
    - Player 2: (0.2, 0.5, 1)
    - Player 3: (0.2, 1, 0.2)
    - Player 4: (1, 1, 0.2)
- [ ] Assign PropertyVisual vào PropertyManager

#### **NetworkManager**
- [ ] Tạo GameObject "NetworkManager"
- [ ] Add `NetworkManager` component
- [ ] Add `UnityTransport` component
- [ ] Set Transport: UnityTransport
- [ ] Set Protocol: DTLS

---

## 🎯 PHASE 2: TEST DEMO MODE (30 phút)

- [ ] Set GameManager > Demo Mode = **true**
- [ ] Play scene trong Unity Editor
- [ ] Kiểm tra:
  - [ ] 1 player spawn tại Waypoint 0
  - [ ] PanelGameInfo hiển thị Turn: 1/25
  - [ ] PanelRoll button sáng lên
  - [ ] Click Roll → Dice animation
  - [ ] Player di chuyển với bounce effect
  - [ ] Player đến tile mới
  - [ ] PanelBuy hiện ra (nếu vào ô property)

---

## 🎯 PHASE 3: FIX BUGS & POLISH (1-2 giờ)

- [ ] Fix bất kỳ lỗi nào trong Demo Mode
- [ ] Kiểm tra tất cả panels show/hide đúng
- [ ] Kiểm tra dice animation mượt
- [ ] Kiểm tra player movement mượt
- [ ] Adjust UI positions/sizes cho đẹp
- [ ] Test mua nhà (PanelBuy)
- [ ] Test nâng cấp nhà

---

## 🎯 PHASE 4: MULTIPLAYER TEST (1 giờ)

- [ ] Set GameManager > Demo Mode = **false**
- [ ] Build game (File > Build Settings > Build)
- [ ] Start Host trong build
- [ ] Start Client trong Unity Editor
- [ ] Kiểm tra:
  - [ ] Cả 2 players spawn
  - [ ] Turn order selection
  - [ ] Turn system hoạt động
  - [ ] UI sync giữa host/client
  - [ ] Dice roll sync
  - [ ] Player movement sync

---

## 📝 NOTES

### **Thứ tự ưu tiên:**
1. **Canvas UI** (quan trọng nhất)
2. **Player Prefabs**
3. **Map & Tiles**
4. **Managers**
5. **Testing**

### **Nếu gặp lỗi:**
1. Check Console để xem error message
2. Kiểm tra tất cả references đã assign chưa
3. Kiểm tra SetActive state của panels
4. Kiểm tra NetworkObject components

### **Tips:**
- Làm từng bước, test từng bước
- Save scene thường xuyên (Ctrl+S)
- Backup scene trước khi thay đổi lớn
- Sử dụng Debug.Log để trace flow

---

**Estimated Time**: 4-6 giờ tổng cộng  
**Status**: Ready to start! 🚀  
**Date**: 2025-10-12
