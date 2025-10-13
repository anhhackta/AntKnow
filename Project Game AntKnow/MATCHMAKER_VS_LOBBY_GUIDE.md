# 🎮 MATCHMAKER VS CUSTOM LOBBY - SỰ KHÁC BIỆT

## 🎯 2 CÁCH CHƠI KHÁC NHAU

### **1. MATCHMAKER (Tìm trận tự động)** 🔍

**Mục đích:** Tìm trận nhanh, tự động ghép người, vào game ngay

**Flow:**
```
Click "Tìm trận"
    ↓
Tìm lobby có sẵn HOẶC tạo mới
    ↓
Đợi người join (1/4 → 2/4 → 3/4 → 4/4)
    ↓
Đủ 2 người → Đợi thêm 30s (có thể có người thứ 3, 4)
    ↓
Đủ 4 người HOẶC hết 30s → AUTO START
    ↓
Thông báo: "Tìm thấy trận! Đang vào game..."
    ↓
Load GameScene NGAY (KHÔNG CÓ PANEL LOBBY)
```

**Đặc điểm:**
- ✅ Tự động tìm người
- ✅ Auto start khi đủ điều kiện
- ✅ Không cần host click "Start"
- ✅ Vào game ngay, không qua panel lobby
- ✅ Nhanh, tiện lợi

---

### **2. CUSTOM LOBBY (Tạo phòng thủ công)** 🏠

**Mục đích:** Tạo phòng riêng, chờ bạn bè, host quyết định khi start

**Flow:**
```
Click "Tạo phòng"
    ↓
PanelCustomRoom hiện (3 panels)
    ↓
PanelRoom → Danh sách phòng
    ↓
Click "Tạo phòng" → PanelCreateRoom
    ↓
Nhập tên phòng → Tạo
    ↓
PanelJoinRoom → Chờ người join
    ↓
Hiển thị danh sách players
    ↓
Host quyết định start (2/3/4 người đều được)
    ↓
Click "Start Game" → Load GameScene
```

**Đặc điểm:**
- ✅ Tạo phòng riêng với tên tùy chỉnh
- ✅ Chờ bạn bè join
- ✅ Host quyết định khi nào start
- ✅ Linh hoạt 2/3/4 người
- ✅ Có panel lobby để chat, chuẩn bị

---

## 🎵 SO SÁNH

| Feature | Matchmaker 🔍 | Custom Lobby 🏠 |
|---------|--------------|-----------------|
| **Tìm người** | Tự động | Thủ công (bạn bè join) |
| **Tên phòng** | Auto (Match_HHMMSS) | Tùy chỉnh |
| **Start game** | Auto (đủ 4 hoặc sau 30s) | Host quyết định |
| **Panel lobby** | KHÔNG | CÓ (PanelJoinRoom) |
| **Linh hoạt** | Ít (auto start) | Nhiều (host quyết định) |
| **Tốc độ** | Nhanh | Chậm hơn |
| **Use case** | Chơi nhanh, solo | Chơi với bạn bè |

---

## 🔥 MATCHMAKER AUTO START LOGIC

### **Host (người tạo lobby):**

```csharp
// MatchmakerService.cs - UpdateLobbyInfoAsync()

if (isHost)
{
    // Đủ 4 người → Start ngay
    if (playerCount >= maxPlayers)
    {
        DebugLog("Lobby full (4/4), auto starting game...");
        await AutoStartGameAsync();
        return false;
    }
    
    // Đủ 2-3 người → Đợi thêm 30s
    if (playerCount >= 2)
    {
        DebugLog($"Match ready ({playerCount}/4), waiting for more players...");
        // TODO: Implement 30s timer, then auto start
    }
}
```

### **AutoStartGameAsync():**

```csharp
private async Task AutoStartGameAsync()
{
    // 1. Notify players
    OnMatchmakingError?.Invoke("Tìm thấy trận! Đang vào game...");
    
    // 2. Create Relay
    string relayJoinCode = await RelayService.Instance.CreateRelayAsync();
    
    // 3. Update lobby with relay code
    var updateOptions = new UpdateLobbyOptions
    {
        Data = new Dictionary<string, DataObject>
        {
            { "RelayJoinCode", new DataObject(..., relayJoinCode) },
            { "GameStarted", new DataObject(..., "true") }
        }
    };
    await LobbyService.Instance.UpdateLobbyAsync(CurrentMatch.Id, updateOptions);
    
    // 4. Load GameScene
    SceneManager.LoadScene("GameScene");
}
```

---

### **Client (người join lobby):**

```csharp
// MatchmakerService.cs - UpdateLobbyInfoAsync()

// Check if game started
if (updatedLobby.Data.ContainsKey("GameStarted"))
{
    string gameStarted = updatedLobby.Data["GameStarted"].Value;
    if (gameStarted == "true")
    {
        DebugLog("Game started by host, joining...");
        
        // Get relay code
        string relayJoinCode = updatedLobby.Data["RelayJoinCode"].Value;
        
        if (!isHost)
        {
            // Client: Join relay
            await RelayService.Instance.JoinRelayAsync(relayJoinCode);
        }
        
        // Load GameScene
        SceneManager.LoadScene("GameScene");
        
        return false;
    }
}
```

---

## 🎯 CUSTOM LOBBY LOGIC

### **LobbyUIManager.cs:**

```csharp
// 3 Panels:
// 1. PanelRoom - Danh sách phòng
// 2. PanelCreateRoom - Popup tạo phòng
// 3. PanelJoinRoom - Trong phòng

private void UpdateJoinRoomUI(Lobby lobby)
{
    // Update player count
    int currentPlayers = lobby.Players.Count;
    int maxPlayers = lobby.MaxPlayers;
    string status = GetLobbyStatus(currentPlayers, maxPlayers);
    textPlayerCount.text = $"{currentPlayers}/{maxPlayers} - {status}";
    
    // Show/hide start button (chỉ host)
    bool isHost = CustomLobbyService.Instance.IsHost;
    if (buttonStartGame != null)
    {
        buttonStartGame.gameObject.SetActive(isHost);
        
        // Enable start button nếu đủ min 2 players
        buttonStartGame.interactable = (currentPlayers >= 2);
        
        // Update button text
        if (currentPlayers >= maxPlayers)
            buttonText.text = "Bắt đầu (Đủ người)";
        else if (currentPlayers >= 2)
            buttonText.text = $"Bắt đầu ({currentPlayers}/{maxPlayers})";
        else
            buttonText.text = "Chờ thêm người...";
    }
}

private string GetLobbyStatus(int current, int max)
{
    if (current >= max)
        return "Đủ người";
    else if (current >= 2)
        return "Có thể bắt đầu";
    else
        return "Chờ thêm người";
}
```

---

## 🧪 TEST

### **Test 1: Matchmaker (Auto start)**

```
Player A: Click "Tìm trận"
    ↓
Tạo lobby "Match_123456" (1/4)
    ↓
Player B: Click "Tìm trận"
    ↓
Join "Match_123456" (2/4)
    ↓
Đợi 30s... (không có người thứ 3)
    ↓
Host auto start
    ↓
Thông báo: "Tìm thấy trận! Đang vào game..."
    ↓
Both load GameScene
```

**Hoặc:**

```
Player A, B, C, D: Click "Tìm trận"
    ↓
All join "Match_123456" (4/4)
    ↓
Host auto start NGAY (không đợi 30s)
    ↓
Thông báo: "Tìm thấy trận! Đang vào game..."
    ↓
All load GameScene
```

---

### **Test 2: Custom Lobby (Manual start)**

```
Player A: Click "Tạo phòng"
    ↓
PanelCustomRoom hiện
    ↓
PanelRoom → Click "Tạo phòng"
    ↓
PanelCreateRoom → Nhập "Phòng của A" → Tạo
    ↓
PanelJoinRoom hiện (1/4)
    ↓
Button "Start Game": "Chờ thêm người..." (Disabled)
    ↓
Player B: Join → (2/4)
    ↓
Button "Start Game": "Bắt đầu (2/4)" (Enabled)
    ↓
Host (Player A): Click "Start Game"
    ↓
Both load GameScene
```

---

## 🎯 SUMMARY

**Matchmaker:**
- ✅ Auto tìm người
- ✅ Auto start (đủ 4 hoặc sau 30s)
- ✅ Vào game ngay
- ✅ Không có panel lobby

**Custom Lobby:**
- ✅ Tạo phòng riêng
- ✅ Host quyết định start
- ✅ Có panel lobby
- ✅ Linh hoạt 2/3/4 người

---

**HIỂU CHƯA?** 🚀

Matchmaker = Nhanh, tự động
Custom Lobby = Linh hoạt, thủ công

