# 🎮 Final Setup Guide - Game Cờ Tỷ Phú

## 📋 Tổng quan

Hệ thống multiplayer hoàn chỉnh với 2 chế độ:
1. **Matchmaking** - Tìm trận tự động (button với countdown)
2. **Custom Lobby** - Tạo/join phòng (3 panel con)

## 🎯 UI Structure

### PanelHome (2 buttons)
```
PanelHome
├── Button "Tìm trận" → Start matchmaking
│   └── Hiện Button "Đang tìm... 60s" (có thể cancel)
└── Button "Tạo phòng" → Mở PanelCustomRoom
```

### PanelCustomRoom (3 panel con)
```
PanelCustomRoom (GameObject trống)
├── ButtonClose (thoát)
└── PanelContainer
    ├── PanelRoom (mặc định)
    │   ├── Button "Tạo phòng" → Mở PanelCreateRoom
    │   ├── Button "Làm mới"
    │   └── List phòng (tên + số người)
    ├── PanelCreateRoom (popup overlay trên PanelRoom)
    │   ├── Button "X" (đóng popup)
    │   ├── InputField (tên phòng)
    │   └── Button "Tạo phòng"
    └── PanelJoinRoom (ẩn 2 panel kia)
        ├── Text: Tên phòng
        ├── Text: Số người (2/4)
        ├── List người chơi
        ├── Button "Quay lại"
        └── Button "Bắt đầu" (chỉ host, ≥2 người)
```

## 📦 Files Created

### Scripts:
1. ✅ `GameConfig.cs` - Centralized config
2. ✅ `LobbyUIManager.cs` - Quản lý 3 panel con
3. ✅ `PanelHome.cs` - Updated với matchmaking UI
4. ✅ `MatchmakerService.cs` - Updated dùng GameConfig
5. ✅ `CustomLobbyService.cs` - Updated với QueryLobbiesAsync()
6. ✅ `RelayService.cs` - Updated dùng GameConfig
7. ✅ `NetworkGameManager.cs` - Updated dùng GameConfig

### Documentation:
1. ✅ `MATCHMAKER_CLOUD_SETUP.md` - Hướng dẫn Matchmaker trên Unity Cloud
2. ✅ `LOBBY_UI_HIERARCHY.md` - Chi tiết UI hierarchy
3. ✅ `MULTIPLAYER_SETUP.md` - Hướng dẫn tổng quan
4. ✅ `FINAL_SETUP_GUIDE.md` - File này

## 🔧 Unity Cloud Configuration

### Lobby Settings
```
Dashboard: https://cloud.unity.com/
→ Multiplayer > Lobby > Enable

Configuration:
- Minimum players slots: 2
- Maximum players slots: 4
- Active Lifespan: 300 seconds
- Disconnect Removal Time: 30 seconds
- Disconnect Host Migration Time: 60 seconds
```

### Relay Settings
```
Dashboard: https://cloud.unity.com/
→ Multiplayer > Relay > Enable

Configuration:
- Region: Asia Southeast (Singapore)
- Max Connections: 3 (tự động từ GameConfig)
```

### Matchmaker Settings (Optional - Có thể bỏ qua)
```
Dashboard: https://cloud.unity.com/
→ Multiplayer > Matchmaker > Enable

Queue "FindGame":
- Maximum players on a ticket: 1
- Default Queue Timeout: 60 seconds
- Default Pool: GamePool

Pool "GamePool":
- Min Players: 2
- Max Players: 4
- Team Count: 1

LƯU Ý: Hiện tại dùng Lobby thay Matchmaker, nên có thể BỎ QUA phần này.
```

## 🎨 UI Setup trong Unity

### 1. Setup PanelHome

```
PanelHome (existing)
├── Add: Button "buttonFindMatch" (Text: "Tìm trận")
├── Add: Button "buttonCustomRoom" (Text: "Tạo phòng")
└── Add: Button "buttonWaitGame" (Text: "Đang tìm... 60s")
    ├── Position: Gần buttonFindMatch
    ├── Initially: SetActive(false)
    └── Child: TextMeshProUGUI "textWaitTimer"
```

**Assign trong Inspector**:
- buttonFindMatch → Button component
- buttonCustomRoom → Button component
- buttonWaitGame → Button component
- textWaitTimer → TextMeshProUGUI component
- lobbyUIManager → LobbyUIManager component

### 2. Tạo PanelCustomRoom

```
Canvas
└── PanelCustomRoom (GameObject mới)
    ├── Add Component: LobbyUIManager
    ├── RectTransform: Anchors = Stretch
    ├── Initially: SetActive(false)
    └── Children:
        ├── ButtonClose (Button, top-right)
        └── PanelContainer (GameObject)
            ├── Image: Background (màu tối, alpha 0.8)
            └── Children:
                ├── PanelRoom
                ├── PanelCreateRoom
                └── PanelJoinRoom
```

### 3. Tạo PanelRoom (Panel mặc định)

```
PanelRoom (GameObject)
├── RectTransform: Center, Size = 800x600
├── Image: Background
└── Children:
    ├── Header
    │   └── TextTitle (TextMeshProUGUI: "Danh sách phòng")
    ├── ButtonCreateRoom (Button: "Tạo phòng")
    ├── ButtonResetList (Button: "Làm mới")
    └── ScrollView
        └── Content (Vertical Layout Group)
            └── RoomItemPrefab (spawn vào đây)
```

### 4. Tạo PanelCreateRoom (Popup)

```
PanelCreateRoom (GameObject)
├── RectTransform: Center, Size = 500x300
├── Image: Background (màu sáng hơn)
├── Canvas Group (alpha = 1)
├── Initially: SetActive(false)
└── Children:
    ├── Header (TextTitle: "Tạo phòng mới")
    ├── ButtonClosePopup (Button: "X", top-right)
    ├── InputRoomName (TMP_InputField)
    └── ButtonConfirmCreate (Button: "Tạo phòng")
```

### 5. Tạo PanelJoinRoom (Trong phòng)

```
PanelJoinRoom (GameObject)
├── RectTransform: Center, Size = 800x600
├── Image: Background
├── Initially: SetActive(false)
└── Children:
    ├── Header (TextRoomName: "Room Name")
    ├── InfoSection (TextPlayerCount: "2/4 người chơi")
    ├── ScrollView
    │   └── Content (Vertical Layout Group)
    │       └── PlayerItemPrefab (spawn vào đây)
    ├── ButtonLeaveRoom (Button: "Quay lại", bottom-left)
    └── ButtonStartGame (Button: "Bắt đầu", bottom-right)
```

### 6. Tạo Prefabs

**RoomItemPrefab**:
```
Assets/Prefabs/UI/RoomItem.prefab
├── Button component
├── Image: Background
└── TextMeshProUGUI: "Room Name (2/4)"
```

**PlayerItemPrefab**:
```
Assets/Prefabs/UI/PlayerItem.prefab
├── Image: Background
└── TextMeshProUGUI: "Player Name"
```

### 7. Assign References trong LobbyUIManager

```
LobbyUIManager (Inspector)
├── Main Container:
│   ├── panelCustomRoom → PanelCustomRoom GameObject
│   └── buttonClosePanelCustomRoom → ButtonClose
├── Panel Container:
│   └── panelContainer → PanelContainer GameObject
├── 3 Panel Con:
│   ├── panelRoom → PanelRoom GameObject
│   ├── panelCreateRoom → PanelCreateRoom GameObject
│   └── panelJoinRoom → PanelJoinRoom GameObject
├── PanelRoom:
│   ├── buttonCreateRoom → Button
│   ├── buttonResetList → Button
│   ├── roomListContainer → ScrollView/Content Transform
│   └── roomItemPrefab → RoomItem Prefab
├── PanelCreateRoom:
│   ├── buttonCloseCreateRoom → Button
│   ├── inputRoomName → TMP_InputField
│   └── buttonConfirmCreate → Button
└── PanelJoinRoom:
    ├── textRoomName → TextMeshProUGUI
    ├── textPlayerCount → TextMeshProUGUI
    ├── playerListContainer → ScrollView/Content Transform
    ├── playerItemPrefab → PlayerItem Prefab
    ├── buttonLeaveRoom → Button
    └── buttonStartGame → Button
```

## 🎮 Flow Testing

### Test Matchmaking:
1. Click "Tìm trận"
2. Button "Đang tìm... 60s" hiện ra
3. Countdown chạy từ 60 → 0
4. Có thể dùng Inventory, Shop trong lúc chờ
5. Click button "Đang tìm..." → Hủy tìm trận
6. Tìm thấy trận → Auto vào GameScene

### Test Custom Lobby:
1. Click "Tạo phòng"
2. PanelCustomRoom mở → PanelRoom hiện
3. List phòng load (có thể trống)
4. Click "Tạo phòng" → PanelCreateRoom mở (overlay)
5. Nhập tên → Click "Tạo phòng"
6. PanelJoinRoom hiện (2 panel kia ẩn)
7. List players hiện
8. Button "Bắt đầu" chỉ hiện cho host
9. Click "Quay lại" → Về PanelRoom

### Test Join Room:
1. Build game ra 2 instances
2. Instance 1: Tạo phòng "Test Room"
3. Instance 2: Click "Tạo phòng" → Click "Làm mới"
4. Instance 2: Click vào "Test Room (1/4)"
5. Instance 2: Vào PanelJoinRoom
6. Instance 1: Thấy "2/4 người chơi"
7. Instance 1 (host): Click "Bắt đầu"
8. Cả 2 vào GameScene

## 📝 GameConfig Settings

```csharp
// Assets/Script/GameConfig.cs
public static class GameConfig
{
    // Multiplayer
    public const int MAX_PLAYERS = 4;
    public const int MIN_PLAYERS = 2;
    
    // Matchmaking
    public const float MATCHMAKING_TIMEOUT = 60f;
    public const float MATCHMAKING_RETRY_INTERVAL = 5f;
    
    // Lobby
    public const float LOBBY_HEARTBEAT_INTERVAL = 15f;
    public const float LOBBY_UPDATE_INTERVAL = 2f;
    public const int LOBBY_QUERY_COUNT = 25;
    
    // Relay
    public const int RELAY_MAX_CONNECTIONS = 3; // MAX_PLAYERS - 1
    
    // Game
    public const float GAME_START_DELAY = 3f;
    public const string GAME_SCENE_NAME = "SceneGame";
    public const string MENU_SCENE_NAME = "MenuScene";
}
```

## ✅ Final Checklist

### Unity Cloud:
- [ ] Link Unity Project ID
- [ ] Enable Lobby service
- [ ] Configure Lobby settings (2-4 players, 300s)
- [ ] Enable Relay service (Asia Southeast)
- [ ] (Optional) Enable Matchmaker

### Unity Editor:
- [ ] Install packages (Authentication, Lobby, Relay, NGO)
- [ ] Tạo Services GameObject trong MenuScene
- [ ] Add: UGSAuthService, MatchmakerService, CustomLobbyService, RelayService
- [ ] Setup PanelHome (2 buttons + wait button)
- [ ] Tạo PanelCustomRoom với 3 panel con
- [ ] Tạo RoomItemPrefab và PlayerItemPrefab
- [ ] Assign tất cả references trong Inspector
- [ ] Setup NetworkManager trong GameScene

### Testing:
- [ ] Test matchmaking (2 instances)
- [ ] Test create room
- [ ] Test join room
- [ ] Test leave room
- [ ] Test start game (≥2 players)
- [ ] Test host migration (nếu có)

## 🚀 Next Steps

1. ✅ **Hoàn thành MenuScene** - Setup UI theo hướng dẫn
2. ✅ **Test local** - Build và test với 2 instances
3. ⏳ **Implement GameScene** - Game logic, board, dice, etc.
4. ⏳ **Build dedicated server** - Linux server (optional)
5. ⏳ **Deploy** - Multiplay Hosting (optional)

## 📚 Documentation Files

1. **FINAL_SETUP_GUIDE.md** ⭐ **BẮT ĐẦU TỪ ĐÂY**
2. **LOBBY_UI_HIERARCHY.md** - Chi tiết UI hierarchy
3. **MATCHMAKER_CLOUD_SETUP.md** - Matchmaker trên Unity Cloud
4. **MULTIPLAYER_SETUP.md** - Tổng quan hệ thống

---

**Version**: 2.0 (Detailed)
**Date**: 2025-10-01
**Status**: Ready for implementation

