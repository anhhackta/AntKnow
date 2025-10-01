# 🎨 Prefabs Setup Guide - Đơn giản

## 📋 Tổng quan

Cần tạo 2 prefabs đơn giản:
1. **RoomItemPrefab** - Button với 2 text (tên phòng + số người)
2. **PlayerItemPrefab** - Button với 1 text (tên người chơi)

---

## 1️⃣ RoomItemPrefab

### Cấu trúc:
```
RoomItem (GameObject)
├── Button component
├── Image (Background)
└── Children:
    ├── TextRoomName (TextMeshProUGUI) - Bên trái
    └── TextPlayerCount (TextMeshProUGUI) - Bên phải
```

### Hướng dẫn tạo từng bước:

#### Bước 1: Tạo GameObject
```
1. Trong Hierarchy, Right-click > UI > Button - TextMeshPro
2. Đổi tên thành "RoomItem"
3. Xóa Text con mặc định (nếu có)
```

#### Bước 2: Setup RoomItem (Root)
```
RoomItem (GameObject)
├── RectTransform:
│   ├── Width: 740
│   ├── Height: 80
│   └── Anchors: Top-Center
├── Button:
│   ├── Interactable: ✅ Yes
│   ├── Transition: Color Tint
│   ├── Normal Color: #FFFFFF
│   ├── Highlighted Color: #F0F0F0
│   └── Pressed Color: #C0C0C0
└── Image:
    ├── Color: #34495E (màu nền)
    └── Sprite: UI-Default (hoặc custom)
```

#### Bước 3: Tạo TextRoomName (Text bên trái)
```
1. Right-click RoomItem > UI > Text - TextMeshPro
2. Đổi tên thành "TextRoomName"

TextRoomName:
├── RectTransform:
│   ├── Anchors: Left-Center
│   ├── Pivot: (0, 0.5)
│   ├── Pos X: 20 (padding từ trái)
│   ├── Pos Y: 0
│   ├── Width: 400
│   └── Height: 60
└── TextMeshProUGUI:
    ├── Text: "Room Name"
    ├── Font Size: 24
    ├── Alignment: Left + Middle
    ├── Color: #FFFFFF
    └── Overflow: Ellipsis
```

#### Bước 4: Tạo TextPlayerCount (Text bên phải)
```
1. Right-click RoomItem > UI > Text - TextMeshPro
2. Đổi tên thành "TextPlayerCount"

TextPlayerCount:
├── RectTransform:
│   ├── Anchors: Right-Center
│   ├── Pivot: (1, 0.5)
│   ├── Pos X: -20 (padding từ phải)
│   ├── Pos Y: 0
│   ├── Width: 100
│   └── Height: 60
└── TextMeshProUGUI:
    ├── Text: "1/4"
    ├── Font Size: 20
    ├── Alignment: Right + Middle
    ├── Color: #BDC3C7
    └── Overflow: Ellipsis
```

#### Bước 5: Save as Prefab
```
1. Tạo folder: Assets/Prefabs/UI (nếu chưa có)
2. Drag RoomItem từ Hierarchy vào folder Assets/Prefabs/UI
3. Prefab được tạo: Assets/Prefabs/UI/RoomItem.prefab
4. Xóa RoomItem khỏi Hierarchy (đã save thành prefab)
```

---

## 2️⃣ PlayerItemPrefab

### Cấu trúc:
```
PlayerItem (GameObject)
├── Button component (interactable = false)
├── Image (Background)
└── Children:
    └── TextPlayerName (TextMeshProUGUI) - Ở giữa
```

### Hướng dẫn tạo từng bước:

#### Bước 1: Tạo GameObject
```
1. Trong Hierarchy, Right-click > UI > Button - TextMeshPro
2. Đổi tên thành "PlayerItem"
3. Xóa Text con mặc định (nếu có)
```

#### Bước 2: Setup PlayerItem (Root)
```
PlayerItem (GameObject)
├── RectTransform:
│   ├── Width: 740
│   ├── Height: 60
│   └── Anchors: Top-Center
├── Button:
│   ├── Interactable: ❌ No (chỉ hiển thị, không click)
│   ├── Transition: None
│   └── Navigation: None
└── Image:
    ├── Color: #34495E (màu nền)
    └── Sprite: UI-Default (hoặc custom)
```

#### Bước 3: Tạo TextPlayerName (Text ở giữa)
```
1. Right-click PlayerItem > UI > Text - TextMeshPro
2. Đổi tên thành "TextPlayerName"

TextPlayerName:
├── RectTransform:
│   ├── Anchors: Stretch (Left: 0, Right: 0, Top: 0, Bottom: 0)
│   ├── Left: 20 (padding)
│   ├── Right: -20 (padding)
│   ├── Top: 0
│   └── Bottom: 0
└── TextMeshProUGUI:
    ├── Text: "Player Name"
    ├── Font Size: 20
    ├── Alignment: Center + Middle
    ├── Color: #FFFFFF
    └── Overflow: Ellipsis
```

#### Bước 4: Save as Prefab
```
1. Drag PlayerItem từ Hierarchy vào folder Assets/Prefabs/UI
2. Prefab được tạo: Assets/Prefabs/UI/PlayerItem.prefab
3. Xóa PlayerItem khỏi Hierarchy (đã save thành prefab)
```

---

## 🎨 Recommended Colors

```css
/* Background */
RoomItem Background: #34495E
PlayerItem Background: #34495E

/* Text */
Room Name: #FFFFFF (trắng)
Player Count: #BDC3C7 (xám nhạt)
Player Name: #FFFFFF (trắng)

/* Button States (RoomItem) */
Normal: #FFFFFF (alpha 255)
Highlighted: #F0F0F0 (alpha 255)
Pressed: #C0C0C0 (alpha 255)
```

---

## 📐 Size Reference

```
RoomItem:
- Width: 740px
- Height: 80px
- TextRoomName: 400px width (bên trái)
- TextPlayerCount: 100px width (bên phải)

PlayerItem:
- Width: 740px
- Height: 60px
- TextPlayerName: Stretch full width
```

---

## ✅ Testing Prefabs

### Test RoomItemPrefab:
```
1. Drag RoomItem.prefab vào Canvas
2. Kiểm tra:
   - ✅ Button có thể click
   - ✅ TextRoomName hiện bên trái
   - ✅ TextPlayerCount hiện bên phải
   - ✅ Hover effect hoạt động
3. Xóa khỏi Canvas sau khi test
```

### Test PlayerItemPrefab:
```
1. Drag PlayerItem.prefab vào Canvas
2. Kiểm tra:
   - ✅ Button KHÔNG thể click (interactable = false)
   - ✅ TextPlayerName hiện ở giữa
   - ✅ Không có hover effect
3. Xóa khỏi Canvas sau khi test
```

---

## 🔗 Assign Prefabs vào LobbyUIManager

### Trong Unity Editor:

```
1. Chọn GameObject có LobbyUIManager component
2. Trong Inspector, tìm section "PanelRoom":
   ├── roomItemPrefab → Drag RoomItem.prefab vào đây
   └── ...
3. Trong Inspector, tìm section "PanelJoinRoom":
   ├── playerItemPrefab → Drag PlayerItem.prefab vào đây
   └── ...
```

---

## 🎯 Code Integration

### LobbyUIManager sẽ spawn prefabs như sau:

**RoomItemPrefab**:
```csharp
// Spawn room item
GameObject item = Instantiate(roomItemPrefab, roomListContainer);

// Get 2 texts
var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
texts[0].text = lobby.Name;           // TextRoomName
texts[1].text = $"{count}/{max}";    // TextPlayerCount

// Setup button click
var button = item.GetComponent<Button>();
button.onClick.AddListener(() => OnRoomItemClicked(lobbyId));
```

**PlayerItemPrefab**:
```csharp
// Spawn player item
GameObject item = Instantiate(playerItemPrefab, playerListContainer);

// Get text
var text = item.GetComponentInChildren<TextMeshProUGUI>();
text.text = playerName;               // TextPlayerName

// Disable button (already disabled in prefab)
var button = item.GetComponent<Button>();
button.interactable = false;
```

---

## 🐛 Common Issues

### Issue 1: Text không hiện
```
Solution:
- Kiểm tra TextMeshProUGUI component có đúng không
- Kiểm tra Font Asset đã assign chưa
- Kiểm tra Color alpha = 255 (không trong suốt)
```

### Issue 2: Button không click được
```
Solution (RoomItem):
- Kiểm tra Button.Interactable = true
- Kiểm tra không có Canvas Group chặn raycast

Solution (PlayerItem):
- Đúng rồi! PlayerItem không nên click được
- Button.Interactable = false
```

### Issue 3: Layout bị lỗi
```
Solution:
- Kiểm tra RectTransform Anchors
- Kiểm tra Width/Height
- Kiểm tra Padding (Left, Right, Top, Bottom)
```

### Issue 4: Prefab không spawn
```
Solution:
- Kiểm tra prefab đã assign vào LobbyUIManager chưa
- Kiểm tra roomListContainer và playerListContainer đã assign chưa
- Kiểm tra Console có lỗi không
```

---

## 📝 Quick Checklist

### RoomItemPrefab:
- [ ] Tạo Button với Image background
- [ ] Thêm TextRoomName (bên trái)
- [ ] Thêm TextPlayerCount (bên phải)
- [ ] Button.Interactable = true
- [ ] Save as Assets/Prefabs/UI/RoomItem.prefab
- [ ] Assign vào LobbyUIManager.roomItemPrefab

### PlayerItemPrefab:
- [ ] Tạo Button với Image background
- [ ] Thêm TextPlayerName (ở giữa)
- [ ] Button.Interactable = false
- [ ] Save as Assets/Prefabs/UI/PlayerItem.prefab
- [ ] Assign vào LobbyUIManager.playerItemPrefab

---

## 🎉 Done!

Sau khi tạo xong 2 prefabs:
1. ✅ Assign vào LobbyUIManager
2. ✅ Test trong game
3. ✅ Kiểm tra list phòng hiện đúng
4. ✅ Kiểm tra list players hiện đúng

---

**Version**: 1.0 (Simple)
**Date**: 2025-10-01

