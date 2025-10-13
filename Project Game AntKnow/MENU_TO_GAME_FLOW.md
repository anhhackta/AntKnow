# ✅ MENU → LOADING → GAME FLOW

## 🔥 FLOW HOÀN CHỈNH

### **1. MATCHMAKER (Tìm trận)**

```
MenuScene: Click "Tìm trận"
    ↓
MatchmakerService.StartMatchmakingAsync()
    ↓
Timer đếm: 00:00 → 00:01 → 00:02 → ...
    ↓
Case A: Tìm thấy lobby có sẵn (2/4, 3/4)
    → Join lobby
    → OnMatchFound (hiện "Match Found")
    → Đợi host start
    ↓
Case B: Tạo lobby mới (1/4)
    → Đợi người join
    → Đủ 2 người → Đếm 30s
    → Hết 30s HOẶC đủ 4 người → AutoStartGameAsync()
    ↓
AutoStartGameAsync() (Host only)
    ↓
1. Fire OnMatchFound event
2. Create Relay
3. Update lobby with relay code
4. Wait 2s (notification)
5. LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false)
    ↓
LoadingScene
    ↓
1. Show loading UI (tips, backgrounds)
2. Load map data (36 tiles)
3. Prepare game session
4. Fade out
5. Load GameScene
    ↓
GameScene
    ↓
1. NetworkManager connects (Relay)
2. GameManager.Start()
3. Load players from lobby
4. Spawn players
5. Start turn order selection
6. Start game
```

---

### **2. CUSTOM LOBBY (Tạo phòng)**

```
MenuScene: Click "Tạo phòng"
    ↓
LobbyUIManager.OpenCustomRoomPanel()
    ↓
PanelCustomRoom hiện
    ↓
Case A: Tạo phòng
    → CustomLobbyService.CreateLobbyAsync()
    → ShowPanelJoinRoom()
    → Chờ người join
    → Host click "Start Game" (≥2 players)
    ↓
Case B: Join phòng
    → CustomLobbyService.JoinLobbyAsync()
    → ShowPanelJoinRoom()
    → Chờ host start
    ↓
LobbyUIManager.OnStartGameClicked() (Host only)
    ↓
CustomLobbyService.StartGameAsync()
    ↓
1. Check all players ready
2. Create Relay
3. Update lobby with relay code
4. Fire OnGameStarting event
    ↓
LobbyUIManager.OnGameStarting(relayJoinCode)
    ↓
1. Setup GameSessionData
2. Join Relay (host/client)
3. LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false)
    ↓
LoadingScene
    ↓
(Same as Matchmaker)
    ↓
GameScene
    ↓
(Same as Matchmaker)
```

---

## 📁 FILES MODIFIED

### **1. MatchmakerService.cs** ✅

**AutoStartGameAsync():**
```csharp
private async Task AutoStartGameAsync()
{
    // Fire OnMatchFound event → Hiện "Match Found" notification
    OnMatchFound?.Invoke(CurrentMatch);

    // Create Relay
    string relayJoinCode = await RelayService.Instance.CreateRelayAsync();
    
    // Update lobby with relay code
    var updateOptions = new UpdateLobbyOptions
    {
        Data = new Dictionary<string, DataObject>
        {
            { "RelayJoinCode", new DataObject(..., relayJoinCode) },
            { "GameStarted", new DataObject(..., "true") }
        }
    };
    await LobbyService.Instance.UpdateLobbyAsync(CurrentMatch.Id, updateOptions);

    // Wait 2s để user thấy notification
    await Task.Delay(2000);

    // Load LoadingScene → GameScene
    LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
}
```

**UpdateLobbyInfoAsync() - Client:**
```csharp
// Check if game started
if (updatedLobby.Data.ContainsKey("GameStarted") && gameStarted == "true")
{
    string relayJoinCode = updatedLobby.Data["RelayJoinCode"].Value;

    if (!isHost)
    {
        // Client: Join relay
        await RelayService.Instance.JoinRelayAsync(relayJoinCode);
    }

    // Load LoadingScene → GameScene
    LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
}
```

---

### **2. LobbyUIManager.cs** ✅

**OnGameStarting():**
```csharp
private async void OnGameStarting(string relayJoinCode)
{
    DebugLog($"Game starting with Relay code: {relayJoinCode}");
    
    // Setup GameSessionData
    var sessionData = GameSessionData.Instance;
    sessionData.SetFromGameDataManager();
    sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);
    
    bool isHost = CustomLobbyService.Instance.IsHost;
    string lobbyId = CustomLobbyService.Instance.CurrentLobby?.Id;
    sessionData.SetNetworkInfo(relayJoinCode, isHost, lobbyId);
    
    // Join Relay
    if (isHost)
    {
        RelayService.Instance.StartHost();
    }
    else
    {
        await RelayService.Instance.JoinRelayAsync(relayJoinCode);
        RelayService.Instance.StartClient();
    }
    
    // Load LoadingScene → GameScene
    LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
}
```

---

### **3. LoadingSceneController.cs** (Existing)

**Static configuration:**
```csharp
public static string sourceScene = "LoginScene";  // Where we came from
public static string targetScene = "MenuScene";   // Where we're going
public static bool checkProfile = true;           // Check ingame name + gender?
```

**LoadWithConfig():**
```csharp
public static void LoadWithConfig(string source, string target, bool checkUserProfile = false)
{
    Configure(source, target, checkUserProfile);
    UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
}
```

**LoadMenuSceneAsync():**
```csharp
private IEnumerator LoadMenuSceneAsync()
{
    // ... loading logic ...
    
    // Determine next scene based on configuration
    string nextScene = targetScene;
    
    if (checkProfile)
    {
        // Check if user has ingame name + gender
        if (hasName && hasGender)
        {
            nextScene = targetScene; // MenuScene
        }
        else
        {
            nextScene = "SelectCharacterScene";
        }
    }
    
    // Load next scene
    SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
}
```

---

## 🎯 LOADING SCENE TASKS

### **Current:**
- ✅ Show loading UI (tips, backgrounds)
- ✅ Check user profile (optional)
- ✅ Fade out before loading next scene

### **TODO (Optional):**
- ⏳ Load map data (36 tiles) - Currently loaded in GameScene
- ⏳ Preload assets (player prefabs, UI) - Currently loaded in GameScene
- ⏳ Show loading progress bar - Currently fake progress

**Note:** Map data và assets đã được load trong GameScene, không cần thiết phải load trong LoadingScene. LoadingScene chỉ cần:
1. Show loading UI
2. Fade out
3. Load GameScene

---

## 🧪 TEST FLOW

### **Test 1: Matchmaker → Game**
```
1. MenuScene: Click "Tìm trận"
2. ✅ Timer: 00:00 → 00:01 → 00:02...
3. Wait 30s (or 4 players)
4. ✅ Notification: "Match Found" (2s)
5. ✅ LoadingScene hiện
6. ✅ Loading tips + backgrounds
7. ✅ Fade out
8. ✅ GameScene load
9. ✅ Players spawn
10. ✅ Game starts
```

---

### **Test 2: Custom Lobby → Game**
```
1. MenuScene: Click "Tạo phòng"
2. ✅ PanelCustomRoom hiện
3. ✅ Tạo phòng / Join phòng
4. ✅ PanelJoinRoom hiện
5. ✅ Host click "Start Game" (≥2 players)
6. ✅ LoadingScene hiện
7. ✅ Loading tips + backgrounds
8. ✅ Fade out
9. ✅ GameScene load
10. ✅ Players spawn
11. ✅ Game starts
```

---

## 🎮 GAMESCENE INITIALIZATION

### **GameManager.Start():**
```csharp
private void Start()
{
    // Wait for network ready
    if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
    {
        StartGame();
    }
    else if (demoMode)
    {
        // Demo mode: Start immediately without network
        StartGame();
    }
}
```

### **GameManager.StartGame():**
```csharp
public void StartGame()
{
    // Initialize
    currentTurn = 1;
    currentPlayerIndex = 0;
    gameStartTime = Time.time;
    isGameActive = true;

    // Spawn players
    if (demoMode)
    {
        SpawnTestPlayer(...);
    }
    else
    {
        // Load players from lobby/session data
        StartCoroutine(LoadPlayersFromLobby());
    }

    // Start first turn
    StartTurn();
}
```

### **GameManager.LoadPlayersFromLobby():**
```csharp
private IEnumerator LoadPlayersFromLobby()
{
    // Get local loadout
    var localLoadout = await LoadLocalPlayerLoadout();
    
    if (IsHost)
    {
        // HOST: Add own loadout
        playerLoadouts[localClientId] = localLoadout;
        
        // Wait for all clients to send their loadouts
        yield return new WaitUntil(() => 
            playerLoadouts.Count >= NetworkManager.Singleton.ConnectedClients.Count
        );
        
        // Spawn all players
        SpawnAllPlayers();
        
        // Start turn order selection
        StartTurnOrderSelection();
    }
    else
    {
        // CLIENT: Send loadout to host
        SendLoadoutToHostServerRpc(localLoadout);
    }
}
```

---

## 🎯 SUMMARY

**Flow:**
1. ✅ MenuScene → Matchmaker/Custom Lobby
2. ✅ Start game → LoadingScene
3. ✅ LoadingScene → GameScene
4. ✅ GameScene → Initialize multiplayer game

**Files Modified:**
1. ✅ MatchmakerService.cs - Load LoadingScene
2. ✅ LobbyUIManager.cs - Load LoadingScene
3. ✅ LoadingSceneController.cs - Already supports config

**Next Steps:**
1. ✅ Test Matchmaker flow
2. ✅ Test Custom Lobby flow
3. ✅ Verify GameScene initialization
4. ✅ Test multiplayer connection

---

**SẴN SÀNG BUILD GAME!** 🚀

