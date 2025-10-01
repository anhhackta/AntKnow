# 🎨 Lobby UI Hierarchy - Chi tiết

## 📋 Cấu trúc tổng quan

```
Canvas
└── PanelCustomRoom (GameObject trống)
    ├── ButtonClose (Button thoát)
    └── PanelContainer (Panel cha)
        ├── PanelRoom (Panel mặc định)
        ├── PanelCreateRoom (Popup overlay)
        └── PanelJoinRoom (Panel khi vào phòng)
```

## 🎯 Chi tiết từng Panel

### 1. PanelCustomRoom (Root)

```
PanelCustomRoom (GameObject)
├── RectTransform: Anchors = Stretch (full screen)
├── Component: LobbyUIManager.cs
└── Children:
    ├── ButtonClose
    └── PanelContainer
```

**ButtonClose**:
```
- Position: Top-Right (X: -50, Y: -50)
- Size: 80x80
- Text: "X" hoặc icon close
- OnClick: LobbyUIManager.OnClosePanelCustomRoom()
```

---

### 2. PanelContainer (Panel cha chứa 3 panel con)

```
PanelContainer (GameObject)
├── RectTransform: Anchors = Stretch (full screen)
├── Component: Image (Background, màu tối, alpha 0.8)
└── Children:
    ├── PanelRoom
    ├── PanelCreateRoom
    └── PanelJoinRoom
```

---

### 3. PanelRoom (Panel mặc định - List phòng)

```
PanelRoom (GameObject)
├── RectTransform: Anchors = Center, Size = 800x600
├── Component: Image (Background)
└── Children:
    ├── Header
    │   └── TextTitle ("Danh sách phòng")
    ├── ButtonCreateRoom
    ├── ButtonResetList
    └── ScrollView (List phòng)
```

#### Chi tiết PanelRoom:

**Header**:
```
Header (GameObject)
├── Position: Top center
├── Size: 800x80
└── TextTitle (TextMeshProUGUI)
    ├── Text: "Danh sách phòng"
    ├── Font Size: 36
    ├── Alignment: Center
```

**ButtonCreateRoom**:
```
ButtonCreateRoom (Button)
├── Position: Below header, left side (X: -250, Y: -100)
├── Size: 200x60
├── Text: "Tạo phòng"
├── OnClick: LobbyUIManager.OnCreateRoomClicked()
```

**ButtonResetList**:
```
ButtonResetList (Button)
├── Position: Below header, right side (X: 250, Y: -100)
├── Size: 200x60
├── Text: "Làm mới"
├── OnClick: LobbyUIManager.OnResetListClicked()
```

**ScrollView (List phòng)**:
```
ScrollView (Scroll View)
├── Position: Below buttons (Y: -180)
├── Size: 760x400
├── Vertical Scrollbar: Yes
└── Content (Vertical Layout Group)
    ├── Spacing: 10
    ├── Padding: 10
    └── Child Alignment: Upper Center
    └── RoomItemPrefab (sẽ spawn vào đây)
```

**RoomItemPrefab** (Prefab riêng):
```
RoomItem (GameObject)
├── Size: 740x80
├── Component: Button
├── Component: Image (Background)
└── Children:
    ├── TextRoomName (TextMeshProUGUI)
    │   ├── Text: "Room Name"
    │   ├── Font Size: 24
    │   ├── Alignment: Center Left
    │   └── Position: Left side (X: -200)
    └── TextPlayerCount (TextMeshProUGUI)
        ├── Text: "1/4"
        ├── Font Size: 20
        ├── Alignment: Center Right
        └── Position: Right side (X: 200)
```

---

### 4. PanelCreateRoom (Popup overlay - Mở trên PanelRoom)

```
PanelCreateRoom (GameObject)
├── RectTransform: Anchors = Center, Size = 500x300
├── Component: Image (Background, màu sáng hơn)
├── Component: Canvas Group (để fade in/out)
└── Children:
    ├── Header
    ├── ButtonClosePopup
    ├── InputRoomName
    └── ButtonConfirmCreate
```

#### Chi tiết PanelCreateRoom:

**Header**:
```
Header (GameObject)
├── Position: Top center
├── Size: 500x60
└── TextTitle (TextMeshProUGUI)
    ├── Text: "Tạo phòng mới"
    ├── Font Size: 28
    ├── Alignment: Center
```

**ButtonClosePopup**:
```
ButtonClosePopup (Button)
├── Position: Top-Right (X: -20, Y: -20)
├── Size: 40x40
├── Text: "X"
├── OnClick: LobbyUIManager.OnCloseCreateRoomClicked()
```

**InputRoomName**:
```
InputRoomName (TMP_InputField)
├── Position: Center (Y: 0)
├── Size: 400x60
├── Placeholder: "Nhập tên phòng..."
├── Character Limit: 20
```

**ButtonConfirmCreate**:
```
ButtonConfirmCreate (Button)
├── Position: Bottom center (Y: -100)
├── Size: 200x60
├── Text: "Tạo phòng"
├── OnClick: LobbyUIManager.OnConfirmCreateClicked()
```

---

### 5. PanelJoinRoom (Panel khi vào phòng - Ẩn 2 panel kia)

```
PanelJoinRoom (GameObject)
├── RectTransform: Anchors = Center, Size = 800x600
├── Component: Image (Background)
└── Children:
    ├── Header
    ├── InfoSection
    ├── ScrollView (List players)
    ├── ButtonLeaveRoom
    └── ButtonStartGame (chỉ host)
```

#### Chi tiết PanelJoinRoom:

**Header**:
```
Header (GameObject)
├── Position: Top center
├── Size: 800x80
└── TextRoomName (TextMeshProUGUI)
    ├── Text: "Room Name"
    ├── Font Size: 36
    ├── Alignment: Center
```

**InfoSection**:
```
InfoSection (GameObject)
├── Position: Below header (Y: -100)
├── Size: 800x60
└── TextPlayerCount (TextMeshProUGUI)
    ├── Text: "2/4 người chơi"
    ├── Font Size: 24
    ├── Alignment: Center
```

**ScrollView (List players)**:
```
ScrollView (Scroll View)
├── Position: Below info (Y: -180)
├── Size: 760x300
├── Vertical Scrollbar: Yes
└── Content (Vertical Layout Group)
    ├── Spacing: 10
    ├── Padding: 10
    └── Child Alignment: Upper Center
    └── PlayerItemPrefab (sẽ spawn vào đây)
```

**PlayerItemPrefab** (Prefab riêng):
```
PlayerItem (GameObject)
├── Size: 740x60
├── Component: Button (interactable = false, chỉ hiển thị)
├── Component: Image (Background)
└── Children:
    └── TextPlayerName (TextMeshProUGUI)
        ├── Text: "Player Name"
        ├── Font Size: 20
        ├── Alignment: Center
```

**ButtonLeaveRoom**:
```
ButtonLeaveRoom (Button)
├── Position: Bottom left (X: -250, Y: -520)
├── Size: 200x60
├── Text: "Quay lại"
├── OnClick: LobbyUIManager.OnLeaveRoomClicked()
```

**ButtonStartGame** (chỉ host):
```
ButtonStartGame (Button)
├── Position: Bottom right (X: 250, Y: -520)
├── Size: 200x60
├── Text: "Bắt đầu"
├── OnClick: LobbyUIManager.OnStartGameClicked()
├── Active: Chỉ khi IsHost = true
```

---

## 🎮 Panel Navigation Flow

### Mở PanelCustomRoom:
```
PanelHome.buttonCustomRoom.onClick
    ↓
LobbyUIManager.OpenCustomRoomPanel()
    ↓
panelCustomRoom.SetActive(true)
    ↓
ShowPanelRoom() → PanelRoom active, others inactive
    ↓
RefreshRoomList()
```

### Tạo phòng:
```
PanelRoom.buttonCreateRoom.onClick
    ↓
ShowPanelCreateRoom()
    ↓
PanelCreateRoom.SetActive(true) ← Overlay trên PanelRoom
    ↓
User nhập tên → buttonConfirmCreate.onClick
    ↓
CustomLobbyService.CreateLobbyAsync()
    ↓
OnLobbyCreated event
    ↓
ShowPanelJoinRoom() → Ẩn PanelRoom và PanelCreateRoom
```

### Join phòng:
```
PanelRoom.RoomItem.onClick
    ↓
CustomLobbyService.JoinLobbyByIdAsync()
    ↓
OnLobbyJoined event
    ↓
ShowPanelJoinRoom() → Ẩn PanelRoom
```

### Rời phòng:
```
PanelJoinRoom.buttonLeaveRoom.onClick
    ↓
CustomLobbyService.LeaveLobbyAsync()
    ↓
OnLobbyLeft event
    ↓
ShowPanelRoom() → Ẩn PanelJoinRoom
    ↓
RefreshRoomList()
```

### Start game:
```
PanelJoinRoom.buttonStartGame.onClick (chỉ host)
    ↓
Check: playerCount >= MIN_PLAYERS (2)
    ↓
CustomLobbyService.StartGameAsync()
    ↓
OnGameStarting event
    ↓
Setup GameSessionData
    ↓
Join Relay (host/client)
    ↓
SceneManager.LoadScene("SceneGame")
```

---

## 🎨 Recommended Colors

```
PanelContainer Background: #000000 (alpha 0.8)
PanelRoom Background: #2C3E50
PanelCreateRoom Background: #34495E
PanelJoinRoom Background: #2C3E50

Button Normal: #3498DB
Button Hover: #2980B9
Button Pressed: #1F618D

Text Primary: #FFFFFF
Text Secondary: #BDC3C7

RoomItem Background: #34495E
RoomItem Hover: #4A5F7F

PlayerItem Background: #34495E
```

---

## 📝 Prefabs cần tạo (Đơn giản)

### 1. RoomItemPrefab
```
Assets/Prefabs/UI/RoomItem.prefab

Cấu trúc:
RoomItem (GameObject)
├── Button component
├── Image (Background)
└── Children:
    ├── TextRoomName (TextMeshProUGUI)
    │   └── Text: "Room Name"
    └── TextPlayerCount (TextMeshProUGUI)
        └── Text: "1/4"

Hướng dẫn tạo:
1. Tạo GameObject mới, đặt tên "RoomItem"
2. Add Component: Button
3. Add Component: Image (Background)
4. Tạo 2 Text con:
   - TextRoomName: Bên trái
   - TextPlayerCount: Bên phải
5. Save as Prefab: Assets/Prefabs/UI/RoomItem.prefab
```

### 2. PlayerItemPrefab
```
Assets/Prefabs/UI/PlayerItem.prefab

Cấu trúc:
PlayerItem (GameObject)
├── Button component (interactable = false)
├── Image (Background)
└── Children:
    └── TextPlayerName (TextMeshProUGUI)
        └── Text: "Player Name"

Hướng dẫn tạo:
1. Tạo GameObject mới, đặt tên "PlayerItem"
2. Add Component: Button
3. Uncheck "Interactable" (chỉ hiển thị, không click)
4. Add Component: Image (Background)
5. Tạo 1 Text con:
   - TextPlayerName: Ở giữa
6. Save as Prefab: Assets/Prefabs/UI/PlayerItem.prefab
```

---

## ✅ Setup Checklist

- [ ] Tạo PanelCustomRoom (GameObject trống)
- [ ] Thêm ButtonClose
- [ ] Tạo PanelContainer
- [ ] Tạo PanelRoom với ScrollView
- [ ] Tạo PanelCreateRoom (popup)
- [ ] Tạo PanelJoinRoom với player list
- [ ] Tạo RoomItemPrefab
- [ ] Tạo PlayerItemPrefab
- [ ] Add LobbyUIManager component
- [ ] Assign tất cả references trong Inspector
- [ ] Test navigation flow

---

## 🎯 Testing

### Test PanelRoom:
1. Click button "Tạo phòng" từ PanelHome
2. PanelCustomRoom mở, PanelRoom hiện
3. List phòng load (có thể trống)
4. Click "Làm mới" → List update

### Test PanelCreateRoom:
1. Từ PanelRoom, click "Tạo phòng"
2. PanelCreateRoom mở (overlay)
3. PanelRoom vẫn hiện phía sau
4. Nhập tên → Click "Tạo phòng"
5. Chuyển sang PanelJoinRoom

### Test PanelJoinRoom:
1. Tạo phòng hoặc join phòng
2. PanelJoinRoom hiện, 2 panel kia ẩn
3. List players hiện đúng
4. Button "Bắt đầu" chỉ hiện cho host
5. Click "Quay lại" → Về PanelRoom

---

**Version**: 1.0
**Date**: 2025-10-01

