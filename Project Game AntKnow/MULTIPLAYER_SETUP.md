# 🎮 Multiplayer Setup - Game Cờ Tỷ Phú

## 📋 Unity Cloud Configuration

### Lobby Settings (Dashboard)
```
Minimum players slots: 2
Maximum players slots: 4
Active Lifespan: 300 seconds (5 minutes)
Disconnect Removal Time: 30 seconds
Disconnect Host Migration Time: 60 seconds
```

### Relay Settings
```
Region: Asia Southeast (Singapore)
Max Connections: 4
```

## 🎯 UI Flow

### 1. PanelHome
```
PanelHome
├── Button "Tìm trận"
└── Button "Tạo phòng"
```

**Button "Tìm trận"**:
- Click → Hiện Button "Đang tìm..." với countdown timer
- Không hiển thị số người online
- Click button "Đang tìm..." → Hủy tìm trận
- Trong lúc tìm: Có thể dùng Inventory, Shop, etc (trừ tạo phòng)

**Button "Tạo phòng"**:
- Click → Mở PanelCustomRoom

### 2. PanelCustomRoom
```
PanelCustomRoom
├── Button "Tạo phòng mới"
├── List phòng (ScrollView)
└── Button "Reload danh sách"
```

**Button "Tạo phòng mới"**:
- Click → Hiện popup nhỏ
  - InputField: Tên phòng
  - Button: Tạo
- Sau khi tạo → Vào PanelRoom (là chủ phòng)

**List phòng**:
- Hiển thị danh sách phòng public
- Click vào 1 phòng → Vào PanelRoom (là thành viên)

### 3. PanelRoom
```
PanelRoom
├── Text: Tên phòng
├── Text: Số người (2/4)
├── List người chơi
├── Button "Quay lại" (tất cả)
└── Button "Bắt đầu" (chỉ chủ phòng)
```

**Button "Quay lại"**:
- Thành viên: Rời phòng → Về PanelCustomRoom
- Chủ phòng: Xóa phòng → Về PanelCustomRoom

**Button "Bắt đầu"** (chỉ chủ phòng):
- Chuyển tất cả người trong phòng → GameScene
- Không cần ready, không có password

## 📦 Required Packages

```
Window > Package Manager > Unity Registry
- Authentication (com.unity.services.authentication)
- Lobby (com.unity.services.lobby)
- Relay (com.unity.services.relay)
- Netcode for GameObjects (com.unity.netcode.gameobjects)
```

## ⚙️ Setup Steps

### 1. Link Unity Project (5 phút)
```
Edit > Project Settings > Services
→ Create/Link Unity Project ID
```

### 2. Enable Services (10 phút)
```
https://dashboard.unity3d.com/
→ Vào project
→ Multiplayer > Lobby > Enable
→ Multiplayer > Relay > Enable
→ Configure Lobby settings (như trên)
```

### 3. Setup Services GameObject (10 phút)
```
MenuScene:
- Tạo GameObject "Services"
- Add components:
  - UGSAuthService
  - MatchmakerService
  - CustomLobbyService
  - RelayService
```

### 4. Setup NetworkManager (15 phút)
```
SceneGame:
- Tạo GameObject "NetworkManager"
- Add components:
  - NetworkManager
  - UnityTransport
- NetworkManager > Transport: UnityTransport
- UnityTransport > Protocol: DTLS
```

## 🔧 Configuration

### Global Settings (1 chỗ duy nhất)
Tất cả services sẽ dùng chung config từ `GameConfig.cs`:

```csharp
public static class GameConfig
{
    public const int MAX_PLAYERS = 4;
    public const int MIN_PLAYERS = 2;
    public const float SEARCH_TIMEOUT = 60f;
    public const float LOBBY_HEARTBEAT = 15f;
    public const float LOBBY_UPDATE = 2f;
}
```

## 🎮 Code Examples

### Tìm trận
```csharp
// Bắt đầu tìm
await MatchmakerService.Instance.StartMatchmakingAsync();

// Hủy tìm
MatchmakerService.Instance.CancelMatchmaking();

// Subscribe countdown
MatchmakerService.OnSearchTimeUpdated += (time) => {
    textTimer.text = $"{(int)time}s";
};
```

### Tạo phòng
```csharp
// Tạo phòng
await CustomLobbyService.Instance.CreateLobbyAsync(roomName, isPrivate: false);

// Lấy danh sách phòng
var lobbies = await CustomLobbyService.Instance.QueryLobbiesAsync();

// Join phòng
await CustomLobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

// Start game (chủ phòng)
await CustomLobbyService.Instance.StartGameAsync();
```

### Rời phòng
```csharp
await CustomLobbyService.Instance.LeaveLobbyAsync();
```

## 🐛 Common Issues

### "Must be signed in to UGS"
```csharp
await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
```

### "Lobby not found"
→ Lobby đã expire (5 phút), reload danh sách

### "Relay allocation failed"
→ Check Relay service enabled và quota

## 📝 Notes

- **Firebase Auth**: Dùng cho Login, Database, Inventory, Shop
- **Unity Auth**: Chỉ dùng khi Tìm trận hoặc Tạo phòng
- **Auto convert**: Firebase UID → Unity Auth (tự động)
- **Max players**: 4 người (cố định cho game cờ)
- **Min players**: 2 người (để test dễ hơn)
- **Lobby lifetime**: 5 phút (sau đó tự xóa)
- **No password**: Demo đơn giản
- **No ready system**: Chủ phòng start là chơi luôn

## 🚀 Quick Test

1. Build game ra executable
2. Chạy build + Unity Editor
3. Build: Click "Tạo phòng" → Tạo phòng "Test"
4. Editor: Click "Tạo phòng" → Reload → Join phòng "Test"
5. Build: Click "Bắt đầu"
6. Cả 2 sẽ load vào GameScene

---

**Version**: 2.0 (Simplified)
**Date**: 2025-10-01

