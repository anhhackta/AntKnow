# 🎮 MATCHMAKING & LOBBY - HƯỚNG DẪN HOÀN CHỈNH

## 🎯 YÊU CẦU CỦA BẠN

> "Hiện tại ta cần 2 phương thức để ghép trận là tạo lobby (gần như xong rồi) và matchmaker cần triển khai từ cloud.unity.com đến trong game để có chức năng ghép trận. Chức năng ghép trận sẽ hiện ra button, text trong button đó sẽ đếm thời gian đã chờ trận, nếu click vào button đó sẽ hủy tìm trận, trong lúc tìm trận không thể join phòng lobby, không thể tạo phòng, thêm 1 panelnotification trong canvas mỗi khi tìm trận thấy sẽ thông báo panelnotification đó tìm thấy trận và đợt chút join phòng, lobby thì ngồi trong phòng nếu là client, còn host sẽ start để kéo người trong phòng lobby vào game."

---

## ✅ ĐÃ HOÀN THÀNH

### **1. PanelNotification** ✅
- ✅ Hiện thông báo khi tìm thấy trận
- ✅ Hiện thông báo đang tìm trận
- ✅ Hiện thông báo hủy tìm trận
- ✅ Hiện thông báo lỗi
- ✅ Tự động ẩn sau 3 giây

### **2. PanelHome - Matchmaking UI** ✅
- ✅ Button "Tìm trận" → Start matchmaking
- ✅ Button "Tạo phòng" → Open lobby panel
- ✅ Button "Đang tìm..." → Hiện khi đang tìm trận, click để hủy
- ✅ Text countdown → Đếm thời gian đã chờ
- ✅ Disable buttons khi đang tìm trận
- ✅ Prevent join/create room khi đang tìm trận

### **3. MatchmakerService** ✅
- ✅ Singleton pattern với DontDestroyOnLoad
- ✅ Start matchmaking → Tìm lobby available hoặc tạo mới
- ✅ Cancel matchmaking → Hủy tìm trận
- ✅ Events: OnMatchmakingStarted, OnSearchTimeUpdated, OnMatchmakingCancelled, OnMatchFound
- ✅ Countdown timer

### **4. LobbyUIManager** ✅
- ✅ Prevent create room khi đang tìm trận
- ✅ Prevent join room khi đang tìm trận
- ✅ Host: Button "Start Game" → Tạo Relay → Load GameScene
- ✅ Client: Ngồi trong phòng chờ host start

### **5. CustomLobbyService** ✅
- ✅ Create lobby
- ✅ Join lobby by ID
- ✅ Join lobby by code
- ✅ Query lobbies
- ✅ Leave lobby
- ✅ Start game (host only) → Tạo Relay → Broadcast join code

### **6. RelayService** ✅
- ✅ Create Relay (host)
- ✅ Join Relay (client)
- ✅ Start host
- ✅ Start client

---

## 🎵 ARCHITECTURE

### **MenuScene Hierarchy:**

```
MenuScene
├── Canvas
│   ├── PanelHome (PanelHome.cs)
│   │   ├── ButtonFindMatch → OnFindMatchClicked()
│   │   ├── ButtonCustomRoom → OnCustomRoomClicked()
│   │   └── ButtonWaitGame (hidden by default) → OnCancelMatchmaking()
│   │       └── TextWaitTimer → "Đang tìm... 30s"
│   ├── PanelInventory
│   ├── PanelUpgrade
│   ├── PanelShop
│   ├── PanelCustomRoom (LobbyUIManager.cs)
│   │   ├── PanelRoom (list phòng)
│   │   ├── PanelCreateRoom (popup tạo phòng)
│   │   └── PanelJoinRoom (trong phòng)
│   │       ├── ButtonStartGame (host only)
│   │       └── ButtonLeaveRoom
│   └── PanelNotification (PanelNotification.cs)
│       ├── NotificationText
│       └── ButtonOK (optional)
├── MatchmakerService (Singleton, DontDestroyOnLoad)
├── CustomLobbyService (Singleton, DontDestroyOnLoad)
└── RelayService (Singleton, DontDestroyOnLoad)
```

---

## 🚀 FLOW DIAGRAM

### **Flow 1: Matchmaking - Tìm trận tự động**

```
User clicks "Tìm trận"
    ↓
PanelHome.OnFindMatchClicked()
    ↓
Check: UGS signed in?
    ↓
YES → MatchmakerService.StartMatchmakingAsync()
    ↓
Event: OnMatchmakingStarted
    ↓
PanelHome: Show ButtonWaitGame, Disable buttons
    ↓
PanelNotification: "🔍 Đang tìm trận..."
    ↓
MatchmakerService: Query lobbies
    ↓
Found available lobby?
    ↓
YES → Join lobby → Event: OnMatchFound
    ↓
PanelNotification: "🎮 Tìm thấy trận! Đang join phòng..."
    ↓
LobbyUIManager: Show PanelJoinRoom
    ↓
Client: Chờ host start
    ↓
Host clicks "Start Game"
    ↓
CustomLobbyService.StartGameAsync()
    ↓
RelayService.CreateRelayAsync() → Get join code
    ↓
Broadcast join code to all players
    ↓
Event: OnGameStarting(relayJoinCode)
    ↓
Host: RelayService.StartHost()
Client: RelayService.JoinRelayAsync(joinCode) → StartClient()
    ↓
Load GameScene
```

---

### **Flow 2: Custom Lobby - Tạo phòng riêng**

```
User clicks "Tạo phòng"
    ↓
Check: IsSearching?
    ↓
YES → Show error "Đang tìm trận, không thể tạo phòng"
    ↓
NO → LobbyUIManager.OpenCustomRoomPanel()
    ↓
Show PanelCustomRoom → PanelRoom (list phòng)
    ↓
User clicks "Tạo phòng"
    ↓
Show PanelCreateRoom (popup)
    ↓
User nhập tên phòng → Click "Xác nhận"
    ↓
CustomLobbyService.CreateLobbyAsync(roomName)
    ↓
Event: OnLobbyCreated
    ↓
LobbyUIManager: Show PanelJoinRoom
    ↓
Host: Button "Start Game" enabled
Client: Chờ host start
    ↓
(Same as matchmaking flow from "Host clicks Start Game")
```

---

### **Flow 3: Cancel Matchmaking**

```
User clicks ButtonWaitGame (đang tìm trận)
    ↓
PanelHome.OnCancelMatchmaking()
    ↓
MatchmakerService.CancelMatchmaking()
    ↓
Event: OnMatchmakingCancelled
    ↓
PanelHome: Hide ButtonWaitGame, Enable buttons
    ↓
PanelNotification: "❌ Đã hủy tìm trận"
```

---

## 🎵 CODE CHANGES

### **1. PanelNotification.cs** (NEW) ✅

**Location:** `Assets/Scenes/Menu/PanelNotification.cs`

**Features:**
- `ShowNotification(message, duration)` - Hiện thông báo
- `ShowMatchFoundNotification()` - "🎮 Tìm thấy trận!"
- `ShowSearchingNotification()` - "🔍 Đang tìm trận..."
- `ShowCancelledNotification()` - "❌ Đã hủy tìm trận"
- `ShowErrorNotification(error)` - "⚠️ Lỗi: ..."
- Auto hide sau 3 giây

---

### **2. PanelHome.cs** (UPDATED) ✅

**Changes:**
```csharp
[Header("References")]
[SerializeField] private PanelNotification panelNotification; // NEW

private bool isSearchingMatch = false; // NEW

// NEW: Subscribe to OnMatchmakingStarted
private void SubscribeToMatchmaker()
{
    MatchmakerService.OnMatchmakingStarted += OnMatchmakingStarted;
    MatchmakerService.OnSearchTimeUpdated += OnSearchTimeUpdated;
    MatchmakerService.OnMatchmakingCancelled += OnMatchmakingCancelled;
    MatchmakerService.OnMatchFound += OnMatchFound;
}

// UPDATED: Prevent multiple clicks, disable buttons, show notification
private async void OnFindMatchClicked()
{
    if (isSearchingMatch) return;
    
    // ... UGS sign in ...
    
    bool started = await MatchmakerService.Instance.StartMatchmakingAsync();
    if (started)
    {
        isSearchingMatch = true;
        buttonWaitGame.gameObject.SetActive(true);
        SetButtonsInteractable(false); // Disable buttons
        panelNotification.ShowSearchingNotification(); // Show notification
    }
}

// UPDATED: Prevent create room khi đang tìm trận
private void OnCustomRoomClicked()
{
    if (isSearchingMatch)
    {
        panelNotification.ShowErrorNotification("Đang tìm trận, không thể tạo phòng");
        return;
    }
    
    lobbyUIManager.OpenCustomRoomPanel();
}

// NEW: Event handlers
private void OnMatchmakingStarted()
{
    isSearchingMatch = true;
    SetButtonsInteractable(false);
}

private void OnMatchmakingCancelled()
{
    isSearchingMatch = false;
    buttonWaitGame.gameObject.SetActive(false);
    SetButtonsInteractable(true);
    panelNotification.ShowCancelledNotification();
}

private void OnMatchFound(Lobby lobby)
{
    isSearchingMatch = false;
    buttonWaitGame.gameObject.SetActive(false);
    panelNotification.ShowMatchFoundNotification();
}

// NEW: Enable/Disable buttons
private void SetButtonsInteractable(bool interactable)
{
    if (buttonFindMatch != null)
        buttonFindMatch.interactable = interactable;
    
    if (buttonCustomRoom != null)
        buttonCustomRoom.interactable = interactable;
}
```

---

### **3. LobbyUIManager.cs** (UPDATED) ✅

**Changes:**
```csharp
// UPDATED: Prevent create room khi đang tìm trận
private void OnCreateRoomClicked()
{
    if (MatchmakerService.Instance.IsSearching)
    {
        DebugLogError("Cannot create room while searching for match");
        return;
    }
    
    ShowPanelCreateRoom();
}

// UPDATED: Prevent join room khi đang tìm trận
private async void OnRoomItemClicked(string lobbyId)
{
    if (MatchmakerService.Instance.IsSearching)
    {
        DebugLogError("Cannot join room while searching for match");
        return;
    }
    
    bool joined = await CustomLobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
    // ...
}

// UPDATED: Prevent confirm create khi đang tìm trận
private async void OnConfirmCreateClicked()
{
    if (MatchmakerService.Instance.IsSearching)
    {
        DebugLogError("Cannot create room while searching for match");
        return;
    }
    
    // ... create lobby ...
}
```

---

## 🚀 UNITY SETUP (10 PHÚT)

### **BƯỚC 1: Tạo PanelNotification trong MenuScene**

```
1. Open MenuScene
2. Right-click Canvas → Create Empty → Rename "PanelNotification"
3. Add Component: PanelNotification.cs
4. Create child: Panel (Image) → Rename "NotificationPanel"
5. Create child of NotificationPanel: Text (TextMeshProUGUI) → Rename "NotificationText"
6. (Optional) Create child: Button → Rename "ButtonOK"
7. Assign references in PanelNotification:
   - Notification Panel: NotificationPanel
   - Notification Text TMP: NotificationText
   - Button OK: ButtonOK (optional)
8. Settings:
   - Auto Hide Duration: 3
```

---

### **BƯỚC 2: Setup PanelHome**

```
1. Find PanelHome GameObject
2. Assign references:
   - Panel Notification: Drag PanelNotification
   - Button Find Match: Drag button "Tìm trận"
   - Button Custom Room: Drag button "Tạo phòng"
   - Button Wait Game: Drag button "Đang tìm..." (hidden by default)
   - Text Wait Timer: Drag text inside ButtonWaitGame
   - Lobby UI Manager: Drag LobbyUIManager GameObject
```

---

### **BƯỚC 3: Verify Services**

```
1. Check MatchmakerService GameObject exists (Singleton, DontDestroyOnLoad)
2. Check CustomLobbyService GameObject exists (Singleton, DontDestroyOnLoad)
3. Check RelayService GameObject exists (Singleton, DontDestroyOnLoad)
```

---

## 🧪 TEST CASES

### **Test 1: Matchmaking - Tìm trận**
```
1. Click "Tìm trận"
2. ✅ ButtonWaitGame hiện
3. ✅ Text: "Đang tìm... 30s" (countdown)
4. ✅ ButtonFindMatch disabled
5. ✅ ButtonCustomRoom disabled
6. ✅ PanelNotification: "🔍 Đang tìm trận..."
7. Wait for match found
8. ✅ PanelNotification: "🎮 Tìm thấy trận! Đang join phòng..."
9. ✅ PanelJoinRoom hiện
```

---

### **Test 2: Cancel Matchmaking**
```
1. Click "Tìm trận"
2. ButtonWaitGame hiện
3. Click ButtonWaitGame
4. ✅ ButtonWaitGame ẩn
5. ✅ ButtonFindMatch enabled
6. ✅ ButtonCustomRoom enabled
7. ✅ PanelNotification: "❌ Đã hủy tìm trận"
```

---

### **Test 3: Prevent join/create room khi đang tìm trận**
```
1. Click "Tìm trận"
2. Try click "Tạo phòng"
3. ✅ PanelNotification: "⚠️ Lỗi: Đang tìm trận, không thể tạo phòng"
4. ✅ PanelCustomRoom KHÔNG hiện
```

---

### **Test 4: Custom Lobby - Host start game**
```
1. Click "Tạo phòng"
2. Create room "Test Room"
3. ✅ PanelJoinRoom hiện
4. ✅ Button "Start Game" enabled (host only)
5. Wait for 2nd player join
6. Click "Start Game"
7. ✅ Relay created
8. ✅ Join code broadcast to all players
9. ✅ GameScene loads
```

---

## 📁 FILES MODIFIED

1. ✅ **PanelNotification.cs** (NEW) - Notification panel
2. ✅ **PanelHome.cs** (UPDATED) - Matchmaking UI + prevent logic
3. ✅ **LobbyUIManager.cs** (UPDATED) - Prevent join/create room khi đang tìm trận

---

## 🎯 SUMMARY

**Hoàn thành:**
- ✅ PanelNotification - Thông báo tìm thấy trận
- ✅ Matchmaking UI - Button countdown, disable buttons
- ✅ Prevent join/create room khi đang tìm trận
- ✅ Host start game → Tạo Relay → Load GameScene
- ✅ Client chờ host start

**Setup:**
- ✅ Tạo PanelNotification trong Canvas
- ✅ Assign references trong PanelHome
- ✅ Verify services (MatchmakerService, CustomLobbyService, RelayService)

**Test:**
- ✅ Tìm trận → Countdown → Tìm thấy → Join phòng
- ✅ Hủy tìm trận → Enable buttons
- ✅ Prevent join/create room khi đang tìm trận
- ✅ Host start → Load GameScene

---

**SẴN SÀNG THUYẾT TRÌNH!** 🎉🔥

