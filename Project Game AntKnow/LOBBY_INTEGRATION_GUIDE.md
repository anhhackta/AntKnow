# 🎮 Lobby Integration Guide - Cấu trúc Menu

## 📋 Cấu trúc Menu hiện tại

```
MenuScene
└── PanelGame (Panel cha)
    ├── PanelMenu (chứa các button điều hướng)
    └── PanelsContainer (chứa các panel con)
        ├── PanelHome ⭐ (đang làm Lobby)
        │   ├── Image (theo giới tính: ManHome.png / GirlHome.png)
        │   └── PanelFindGame (luôn hiện)
        │       └── Button SubMenu (click hiện 2 button)
        │           ├── Button "Tìm trận" (QuickGame) → Matchmaker
        │           └── Button "Custom" → Mở PanelCustomRoom
        ├── PanelInventory (chưa phát triển)
        ├── PanelUpgrade (chưa phát triển)
        └── PanelShop (chưa phát triển)
```

---

## 🎯 Tích hợp PanelCustomRoom vào PanelHome

### Vị trí đặt PanelCustomRoom:

```
PanelHome (existing)
├── Image characterImage (ManHome/GirlHome)
├── PanelFindGame (existing)
│   └── Button SubMenu
│       ├── Button QuickGame
│       └── Button Custom → Mở PanelCustomRoom
└── PanelCustomRoom (NEW - thêm vào đây) ⭐
    ├── Initially: SetActive(false)
    ├── ButtonClose
    └── PanelContainer
        ├── PanelRoom
        ├── PanelCreateRoom
        └── PanelJoinRoom
```

### Setup trong Unity Editor:

```
1. Chọn PanelHome GameObject
2. Right-click > Create Empty Child
3. Đổi tên: "PanelCustomRoom"
4. Add Component: RectTransform (Anchors = Stretch)
5. SetActive(false) - Ẩn ban đầu
6. Tạo các panel con theo LOBBY_UI_HIERARCHY.md
```

---

## 🔧 Cách Prefabs hoạt động

### 1. RoomItemPrefab - Hiển thị thông tin phòng

#### Cấu trúc Prefab:
```
RoomItemPrefabs.prefab
├── Button component
├── Image (Background)
└── Children (2 Text components):
    ├── Text[0] - Tên phòng (bên trái)
    └── Text[1] - Số người (bên phải) "1/4"
```

#### Code nhận diện Text:
```csharp
// LobbyUIManager.cs - Line 272-311
GameObject item = Instantiate(roomItemPrefab, roomListContainer);

// Lấy TẤT CẢ Text components (cả Text và TextMeshProUGUI)
var allTexts = TextHelper.GetAllTexts(item);

if (allTexts.Count >= 2)
{
    // Text[0] = Tên phòng
    if (allTexts[0] is TextMeshProUGUI tmp1)
        tmp1.text = lobby.Name;
    else if (allTexts[0] is Text ui1)
        ui1.text = lobby.Name;
    
    // Text[1] = Số người
    if (allTexts[1] is TextMeshProUGUI tmp2)
        tmp2.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    else if (allTexts[1] is Text ui2)
        ui2.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
}
```

#### Thứ tự Text quan trọng:
```
⚠️ LƯU Ý: Code lấy Text theo thứ tự trong Hierarchy!

Đúng:
RoomItem
├── Text "RoomName" (index 0) ✅
└── Text "PlayerCount" (index 1) ✅

Sai:
RoomItem
├── Text "PlayerCount" (index 0) ❌ → Sẽ hiện tên phòng
└── Text "RoomName" (index 1) ❌ → Sẽ hiện số người
```

---

### 2. PlayerItemPrefab - Hiển thị tên người chơi

#### Cấu trúc Prefab:
```
PlayerItemPrefabs.prefab
├── Button component (interactable = false)
├── Image (Background)
└── Children (1 Text component):
    └── Text - Tên người chơi (ở giữa)
```

#### Code nhận diện Text:
```csharp
// LobbyUIManager.cs - Line 411-456
GameObject item = Instantiate(playerItemPrefab, playerListContainer);

string playerName = "Player";
if (player.Data != null && player.Data.ContainsKey("PlayerName"))
    playerName = player.Data["PlayerName"].Value;

// Try TextMeshProUGUI first
var tmpText = item.GetComponentInChildren<TextMeshProUGUI>();
if (tmpText != null)
{
    tmpText.text = playerName;
}
else
{
    // Fallback to Unity UI Text
    var uiText = item.GetComponentInChildren<Text>();
    if (uiText != null)
    {
        uiText.text = playerName;
    }
}

// Disable button (chỉ hiển thị)
var button = item.GetComponent<Button>();
if (button != null)
    button.interactable = false;
```

---

## 🔍 TextHelper - Hỗ trợ cả Text và TextMeshProUGUI

### Code:
```csharp
// LobbyUIManager.cs - Line 11-62
public static class TextHelper
{
    // Lấy tất cả Text components (cả 2 loại)
    public static List<Component> GetAllTexts(GameObject obj)
    {
        List<Component> texts = new List<Component>();
        
        // Get all TextMeshProUGUI
        var tmpTexts = obj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var t in tmpTexts)
            texts.Add(t);
        
        // Get all Unity UI Text
        var uiTexts = obj.GetComponentsInChildren<Text>();
        foreach (var t in uiTexts)
            texts.Add(t);
        
        return texts;
    }
}
```

### Tại sao cần TextHelper?
```
Vấn đề: Bạn đã đổi từ TextMeshProUGUI sang Text trong Inspector
        nhưng code cũ chỉ hỗ trợ TextMeshProUGUI

Giải pháp: TextHelper tự động detect cả 2 loại Text
          → Prefab dùng Text hoặc TextMeshProUGUI đều OK
```

---

## 🎨 Tạo Prefabs đúng cách

### RoomItemPrefabs.prefab:

```
Bước 1: Tạo Button
1. Hierarchy > Right-click > UI > Button
2. Đổi tên: "RoomItem"

Bước 2: Thêm Text 1 (Tên phòng)
1. Right-click RoomItem > UI > Text (hoặc Text - TextMeshPro)
2. Đổi tên: "TextRoomName"
3. Position: Bên trái
4. Text: "Room Name"

Bước 3: Thêm Text 2 (Số người)
1. Right-click RoomItem > UI > Text (hoặc Text - TextMeshPro)
2. Đổi tên: "TextPlayerCount"
3. Position: Bên phải
4. Text: "1/4"

⚠️ QUAN TRỌNG: Thứ tự trong Hierarchy phải là:
   RoomItem
   ├── TextRoomName (index 0)
   └── TextPlayerCount (index 1)

Bước 4: Save Prefab
1. Drag RoomItem vào Assets/Scenes/Menu/
2. Đổi tên: "RoomItemPrefabs.prefab"
```

### PlayerItemPrefabs.prefab:

```
Bước 1: Tạo Button
1. Hierarchy > Right-click > UI > Button
2. Đổi tên: "PlayerItem"
3. Inspector > Button > Uncheck "Interactable"

Bước 2: Thêm Text (Tên người chơi)
1. Right-click PlayerItem > UI > Text (hoặc Text - TextMeshPro)
2. Đổi tên: "TextPlayerName"
3. Position: Ở giữa (Stretch)
4. Text: "Player Name"

Bước 3: Save Prefab
1. Drag PlayerItem vào Assets/Scenes/Menu/
2. Đổi tên: "PlayerItemPrefabs.prefab"
```

---

## 🔗 Assign References trong LobbyUIManager

```
1. Chọn PanelCustomRoom GameObject
2. Add Component: LobbyUIManager
3. Assign references:

Main Container:
├── panelCustomRoom → PanelCustomRoom GameObject
└── buttonClosePanelCustomRoom → ButtonClose

Panel Container:
└── panelContainer → PanelContainer GameObject

3 Panel Con:
├── panelRoom → PanelRoom GameObject
├── panelCreateRoom → PanelCreateRoom GameObject
└── panelJoinRoom → PanelJoinRoom GameObject

PanelRoom:
├── buttonCreateRoom → Button "Tạo phòng"
├── buttonResetList → Button "Làm mới"
├── roomListContainer → ScrollView/Content Transform
└── roomItemPrefab → RoomItemPrefabs.prefab ⭐

PanelCreateRoom:
├── buttonCloseCreateRoom → Button "X"
├── inputRoomName → InputField (Text hoặc TMP_InputField)
└── buttonConfirmCreate → Button "Tạo phòng"

PanelJoinRoom:
├── textRoomName → Text (Text hoặc TextMeshProUGUI)
├── textPlayerCount → Text (Text hoặc TextMeshProUGUI)
├── playerListContainer → ScrollView/Content Transform
├── playerItemPrefab → PlayerItemPrefabs.prefab ⭐
├── buttonLeaveRoom → Button "Quay lại"
└── buttonStartGame → Button "Bắt đầu"
```

---

## 🎮 Kết nối PanelHome với LobbyUIManager

### PanelHome.cs:

```csharp
// Line 29
[SerializeField] private LobbyUIManager lobbyUIManager;

// Line 103-113
private void OnCustomRoomClicked()
{
    Debug.Log("PanelHome: Custom Room button clicked");

    if (lobbyUIManager != null)
    {
        lobbyUIManager.OpenCustomRoomPanel();
    }
    else
    {
        Debug.LogError("PanelHome: LobbyUIManager reference is null!");
    }
}
```

### Assign trong Unity:
```
1. Chọn PanelHome GameObject
2. Inspector > PanelHome (Script)
3. Lobby UI Manager → Drag PanelCustomRoom GameObject vào đây
   (PanelCustomRoom có LobbyUIManager component)
```

---

## 🐛 Về lỗi RelayService Disconnect

### Log bạn thấy:
```
[RelayService] Disconnecting from Relay...
[RelayService] Disconnected from Relay
```

### Giải thích:
```
✅ ĐÂY KHÔNG PHẢI LỖI!

Khi bạn stop Play Mode trong Unity:
1. Unity gọi OnDestroy() cho tất cả MonoBehaviour
2. RelayService.OnDestroy() → Disconnect()
3. Log "Disconnecting..." và "Disconnected" hiện ra

→ Đây là cleanup bình thường, đảm bảo Relay connection được đóng đúng cách
→ Không ảnh hưởng gì đến game
```

### Code:
```csharp
// RelayService.cs - Line 344-347
private void OnDestroy()
{
    Disconnect(); // Cleanup khi destroy
}
```

---

## ✅ Checklist Setup

### Prefabs:
- [ ] Tạo RoomItemPrefabs.prefab (2 Text: tên + số người)
- [ ] Tạo PlayerItemPrefabs.prefab (1 Text: tên người chơi)
- [ ] Kiểm tra thứ tự Text trong Hierarchy (RoomItem)
- [ ] Kiểm tra Button.Interactable = false (PlayerItem)

### UI Hierarchy:
- [ ] Tạo PanelCustomRoom trong PanelHome
- [ ] Tạo PanelContainer
- [ ] Tạo 3 panel con (PanelRoom, PanelCreateRoom, PanelJoinRoom)
- [ ] Tạo ScrollView cho room list
- [ ] Tạo ScrollView cho player list

### References:
- [ ] Assign LobbyUIManager vào PanelCustomRoom
- [ ] Assign tất cả references trong LobbyUIManager
- [ ] Assign lobbyUIManager trong PanelHome
- [ ] Assign roomItemPrefab
- [ ] Assign playerItemPrefab

### Testing:
- [ ] Click button "Custom" → PanelCustomRoom mở
- [ ] PanelRoom hiện, list phòng load
- [ ] Click "Tạo phòng" → PanelCreateRoom mở
- [ ] Tạo phòng → PanelJoinRoom hiện
- [ ] List players hiện đúng tên

---

**Version**: 1.0
**Date**: 2025-10-01

