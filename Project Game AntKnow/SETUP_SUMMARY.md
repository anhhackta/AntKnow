# 🎮 Setup Summary - Simplified Multiplayer System

## ✅ Đã hoàn thành

### 1. Centralized Configuration
- ✅ Tạo `GameConfig.cs` - Tất cả settings ở 1 chỗ
- ✅ Loại bỏ SerializeField dư thừa trong services
- ✅ MAX_PLAYERS = 4 (cố định)
- ✅ MIN_PLAYERS = 2

### 2. Simplified UI
- ✅ Tạo `PanelRoomSimple.cs` - UI đơn giản hơn
- ✅ Cập nhật `PanelHome.cs` - 2 buttons riêng biệt
- ✅ Loại bỏ các file MD dư thừa trong Menu folder

### 3. Updated Services
- ✅ `MatchmakerService.cs` - Dùng GameConfig
- ✅ `CustomLobbyService.cs` - Dùng GameConfig
- ✅ `RelayService.cs` - Dùng GameConfig
- ✅ `NetworkGameManager.cs` - Dùng GameConfig

### 4. Documentation
- ✅ `MULTIPLAYER_SETUP.md` - Hướng dẫn duy nhất
- ✅ `GameConfig.cs` - Centralized settings

## 📋 Unity Cloud Configuration

### Lobby Settings (Dashboard)
```
Minimum players slots: 2
Maximum players slots: 4
Active Lifespan: 300 seconds (5 phút)
Disconnect Removal Time: 30 seconds
Disconnect Host Migration Time: 60 seconds
```

### Relay Settings
```
Region: Asia Southeast (Singapore)
Max Connections: 3 (MAX_PLAYERS - 1)
```

## 🎯 UI Structure

### PanelHome (2 buttons)
```
PanelHome
├── Button "Tìm trận" → PanelRoomSimple.StartMatchmaking()
└── Button "Tạo phòng" → PanelRoomSimple.OpenCustomRoomPanel()
```

### Button "Tìm trận" Flow
```
Click "Tìm trận"
    ↓
Hiện Button "Đang tìm... 60s" (countdown)
    ↓
Có thể dùng Inventory, Shop (trừ tạo phòng)
    ↓
Click button → Hủy tìm trận
    ↓
Tìm thấy trận → Auto vào GameScene
```

### Button "Tạo phòng" Flow
```
Click "Tạo phòng"
    ↓
Mở PanelCustomRoom
├── Button "Tạo phòng mới" → Popup nhập tên → Tạo
├── List phòng (ScrollView)
└── Button "Reload danh sách"
    ↓
Click vào 1 phòng → Vào PanelRoom
├── Text: Tên phòng
├── Text: Số người (2/4)
├── List người chơi
├── Button "Quay lại" (tất cả)
└── Button "Bắt đầu" (chỉ chủ phòng)
    ↓
Chủ phòng click "Bắt đầu" → Tất cả vào GameScene
```

## 📦 Files Structure

```
Project Game AntKnow/
├── Assets/
│   ├── Script/
│   │   ├── GameConfig.cs                    ✅ NEW - Centralized config
│   │   ├── Services/
│   │   │   ├── UGSAuthService.cs            ✅ Updated
│   │   │   ├── MatchmakerService.cs         ✅ Updated - Dùng GameConfig
│   │   │   ├── LobbyService.cs              ✅ Updated - Dùng GameConfig
│   │   │   └── RelayService.cs              ✅ Updated - Dùng GameConfig
│   │   ├── Game/
│   │   │   └── GameSessionData.cs           ✅ Existing
│   │   └── Multiplayer/
│   │       └── NetworkGameManager.cs        ✅ Updated - Dùng GameConfig
│   └── Scenes/
│       └── Menu/
│           ├── PanelHome.cs                 ✅ Updated - 2 buttons
│           ├── PanelRoomSimple.cs           ✅ NEW - Simplified UI
│           └── MenuSceneManager.cs          ✅ Existing
└── MULTIPLAYER_SETUP.md                     ✅ NEW - Hướng dẫn duy nhất
```

## ⚙️ GameConfig Settings

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
    public const int RELAY_MAX_CONNECTIONS = MAX_PLAYERS - 1; // 3
    
    // Game
    public const float GAME_START_DELAY = 3f;
    public const string GAME_SCENE_NAME = "SceneGame";
    public const string MENU_SCENE_NAME = "MenuScene";
}
```

## 🎮 Setup Steps

### 1. Install Packages (15 phút)
```
Window > Package Manager > Unity Registry
- Authentication
- Lobby
- Relay
- Netcode for GameObjects
```

### 2. Link Unity Project (5 phút)
```
Edit > Project Settings > Services
→ Link Unity Project ID
```

### 3. Enable Services (10 phút)
```
Unity Dashboard → Multiplayer
→ Enable Lobby (configure settings như trên)
→ Enable Relay (chọn Asia Southeast)
```

### 4. Setup MenuScene (15 phút)
```
MenuScene:
- Tạo GameObject "Services"
  - Add: UGSAuthService
  - Add: MatchmakerService
  - Add: CustomLobbyService
  - Add: RelayService

- PanelHome:
  - Assign buttonFindMatch
  - Assign buttonCustomRoom
  - Assign panelRoomSimple reference

- Tạo PanelRoomSimple UI (theo hierarchy trong MULTIPLAYER_SETUP.md)
```

### 5. Setup GameScene (15 phút)
```
SceneGame:
- Tạo GameObject "NetworkManager"
  - Add: NetworkManager
  - Add: UnityTransport
- Configure NetworkManager > Transport: UnityTransport
```

## 🔧 Code Examples

### Tìm trận (từ PanelHome)
```csharp
// PanelHome.cs
private void OnFindMatchClicked()
{
    panelRoomSimple.StartMatchmaking();
}
```

### Tạo phòng (từ PanelHome)
```csharp
// PanelHome.cs
private void OnCustomRoomClicked()
{
    panelRoomSimple.OpenCustomRoomPanel();
}
```

### Trong PanelRoomSimple
```csharp
// Tìm trận
public async void StartMatchmaking()
{
    await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
    await MatchmakerService.Instance.StartMatchmakingAsync();
    buttonWaitGame.gameObject.SetActive(true);
}

// Mở custom room
public async void OpenCustomRoomPanel()
{
    await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
    panelCustomRoom.SetActive(true);
    RefreshRoomList();
}
```

## 🐛 Common Issues

### "Must be signed in to UGS"
→ Auto sign in được gọi trong StartMatchmaking() và OpenCustomRoomPanel()

### "Lobby not found"
→ Lobby expire sau 5 phút, click "Reload danh sách"

### "Relay allocation failed"
→ Check Relay service enabled và quota

## 📝 Notes

- **No password**: Demo đơn giản
- **No ready system**: Chủ phòng start là chơi luôn
- **No player count display**: Trong matchmaking không hiện số người
- **Can use other panels**: Trong lúc tìm trận có thể dùng Inventory, Shop
- **Auto start**: Khi đủ người hoặc chủ phòng click "Bắt đầu"

## ✅ Testing Checklist

- [ ] Cài đặt packages
- [ ] Link Unity Project
- [ ] Enable Lobby & Relay services
- [ ] Configure Lobby settings (2-4 players, 300s lifetime)
- [ ] Setup Services GameObject
- [ ] Setup PanelHome (2 buttons)
- [ ] Setup PanelRoomSimple UI
- [ ] Setup NetworkManager trong GameScene
- [ ] Build game
- [ ] Test matchmaking (2 instances)
- [ ] Test custom room (tạo, join, start)

## 🎯 Next Steps

1. ✅ **Đọc MULTIPLAYER_SETUP.md** - Hướng dẫn chi tiết
2. ✅ **Setup UI** - Theo hierarchy
3. ✅ **Test local** - Build và test
4. ⏳ **Implement game logic** - Board, Dice, Properties
5. ⏳ **Build dedicated server** - Linux server

---

**Version**: 2.0 (Simplified)
**Date**: 2025-10-01
**Changes**:
- Centralized config trong GameConfig.cs
- Simplified UI với PanelRoomSimple.cs
- Loại bỏ file MD dư thừa
- 2 buttons riêng biệt trong PanelHome
- No password, no ready system (demo)

