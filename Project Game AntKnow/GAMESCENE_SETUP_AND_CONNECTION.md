# 🎮 GAMESCENE SETUP & CONNECTION GUIDE

## 🔥 TRƯỚC KHI KẾT NỐI - CHECKLIST

### **1. Unity Editor - GameScene Setup**

#### **A. NetworkManager GameObject** ⭐ QUAN TRỌNG NHẤT

```
Hierarchy:
└── NetworkManager (GameObject)
    ├── NetworkManager (Component)
    └── UnityTransport (Component)
```

**NetworkManager Component Settings:**
```
✅ Network Config:
   - Protocol Version: 0
   - Network Transport: UnityTransport (drag component)
   - Player Prefab: KHÔNG GÁN (spawn manually)
   - Tick Rate: 30
   - Enable Scene Management: TRUE
   - Enable Network Logs: TRUE
```

**UnityTransport Component Settings:**
```
✅ Protocol Type: UnityTransport
✅ Use Relay: TRUE (sẽ được config runtime)
```

---

#### **B. GameManager GameObject** ⭐ QUAN TRỌNG

```
Hierarchy:
└── GameManager (GameObject)
    ├── GameManager (Component - NetworkBehaviour)
    └── NetworkObject (Component)
```

**NetworkObject Settings:**
```
✅ Is Player Object: FALSE
✅ Owner Permission: Server Only
✅ Synchronize Transform: FALSE
✅ Destroy With Scene: TRUE
```

**GameManager Component Settings:**
```
✅ Demo Mode: FALSE (để test multiplayer)
✅ Player Prefab Male: PlayerMale prefab
✅ Player Prefab Female: PlayerFemale prefab
✅ Board Manager: BoardManager GameObject
✅ Property Manager: PropertyManager GameObject
✅ All UI references assigned
```

---

#### **C. Player Prefabs** ⭐ QUAN TRỌNG

**PlayerMale.prefab & PlayerFemale.prefab:**

```
Prefab Structure:
└── PlayerMale/PlayerFemale (Root)
    ├── NetworkObject (Component)
    ├── PlayerGameController (Component - NetworkBehaviour)
    └── MaleModel/FemaleModel (Child GameObject)
        └── Animator (Component)
```

**NetworkObject Settings:**
```
✅ Is Player Object: TRUE
✅ Owner Permission: Owner
✅ Synchronize Transform: TRUE
✅ Interpolate: TRUE
✅ Use Half Float Precision: TRUE
```

**PlayerGameController Settings:**
```
✅ Is Male: TRUE/FALSE
✅ Move Speed: 5
✅ Bounce Height: 0.5
✅ Animator: MaleModel/FemaleModel Animator
```

---

#### **D. BoardManager GameObject**

```
Hierarchy:
└── BoardManager (GameObject)
    └── BoardManager (Component)
        └── Waypoints (36 transforms)
```

**BoardManager Settings:**
```
✅ Waypoints: Array of 36 transforms (Tile_0 → Tile_35)
✅ Show Debug Info: TRUE
```

---

#### **E. Services (DontDestroyOnLoad)**

Các services này đã được tạo trong MenuScene và persist sang GameScene:

```
✅ RelayService (Singleton)
✅ UGSAuthService (Singleton)
✅ GameDataManager (Singleton)
✅ ManagerAudio (Singleton)
```

---

## 🔌 CÁCH KẾT NỐI - FLOW CHI TIẾT

### **FLOW 1: MenuScene → LoadingScene → GameScene**

```
MenuScene:
    ↓
1. User click "Start Game" (Matchmaker/Custom Lobby)
    ↓
2. RelayService.CreateRelayAsync() (Host)
   OR
   RelayService.JoinRelayAsync(code) (Client)
    ↓
3. UnityTransport configured with Relay data
    ↓
4. LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false)
    ↓
LoadingScene:
    ↓
5. Show loading UI (tips, backgrounds)
    ↓
6. Fade out
    ↓
7. SceneManager.LoadScene("GameScene")
    ↓
GameScene:
    ↓
8. RelayService.StartHost() OR RelayService.StartClient()
    ↓
9. NetworkManager.StartHost() OR NetworkManager.StartClient()
    ↓
10. GameManager.OnNetworkSpawn()
    ↓
11. GameManager.StartGame()
    ↓
12. GameManager.LoadPlayersFromLobby()
    ↓
13. Spawn players
    ↓
14. Start game
```

---

### **FLOW 2: RelayService Connection**

#### **Host Flow:**

```csharp
// MenuScene - LobbyUIManager.OnGameStarting()

// 1. Setup GameSessionData
var sessionData = GameSessionData.Instance;
sessionData.SetFromGameDataManager();
sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);
sessionData.SetNetworkInfo(relayJoinCode, isHost: true, lobbyId);

// 2. Start Host (configure transport)
RelayService.Instance.StartHost();
// → Calls NetworkManager.StartHost()

// 3. Load LoadingScene
LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false);
```

**RelayService.StartHost():**
```csharp
public bool StartHost()
{
    // Check if connected to Relay
    if (!IsConnected || !IsHost) return false;
    
    // Check NetworkManager
    if (NetworkManager.Singleton == null) return false;
    
    // Start NetworkManager as Host
    bool started = NetworkManager.Singleton.StartHost();
    
    return started;
}
```

---

#### **Client Flow:**

```csharp
// MenuScene - LobbyUIManager.OnGameStarting()

// 1. Setup GameSessionData
var sessionData = GameSessionData.Instance;
sessionData.SetFromGameDataManager();
sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);
sessionData.SetNetworkInfo(relayJoinCode, isHost: false, lobbyId);

// 2. Join Relay
await RelayService.Instance.JoinRelayAsync(relayJoinCode);

// 3. Start Client (configure transport)
RelayService.Instance.StartClient();
// → Calls NetworkManager.StartClient()

// 4. Load LoadingScene
LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false);
```

**RelayService.StartClient():**
```csharp
public bool StartClient()
{
    // Check if connected to Relay
    if (!IsConnected || IsHost) return false;
    
    // Check NetworkManager
    if (NetworkManager.Singleton == null) return false;
    
    // Start NetworkManager as Client
    bool started = NetworkManager.Singleton.StartClient();
    
    return started;
}
```

---

### **FLOW 3: GameScene Initialization**

#### **GameManager.Start():**

```csharp
private void Start()
{
    // Validate prefab assignments
    if (playerPrefabMale == null || playerPrefabFemale == null)
    {
        Debug.LogError("Player prefabs not assigned!");
        return;
    }

    // Wait for network ready
    if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
    {
        // Network already connected → Start game
        StartGame();
    }
    else if (demoMode)
    {
        // Demo mode: Start immediately without network
        StartGame();
    }
    else
    {
        Debug.LogWarning("Waiting for network connection...");
        // Will be called from OnNetworkSpawn()
    }
}
```

---

#### **GameManager.OnNetworkSpawn():**

```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    if (!demoMode)
    {
        // Network connected → Start game
        StartGame();
    }
}
```

**Khi nào được gọi:**
- Host: Ngay sau `NetworkManager.StartHost()`
- Client: Ngay sau `NetworkManager.StartClient()` và connect thành công

---

#### **GameManager.StartGame():**

```csharp
public void StartGame()
{
    Debug.Log("[GameManager] Starting game...");

    // Initialize
    currentTurn = 1;
    currentPlayerIndex = 0;
    gameStartTime = Time.time;
    isGameActive = true;

    // Setup UI
    if (rollButton != null)
    {
        rollButton.onClick.AddListener(OnRollButtonClicked);
    }

    // Spawn players
    if (demoMode)
    {
        // Demo: Spawn test player
        SpawnTestPlayer(...);
    }
    else
    {
        // Multiplayer: Load players from lobby/session data
        StartCoroutine(LoadPlayersFromLobby());
    }

    // Start first turn
    StartTurn();
}
```

---

#### **GameManager.LoadPlayersFromLobby():**

```csharp
private IEnumerator LoadPlayersFromLobby()
{
    // Get local player loadout
    var localLoadout = await LoadLocalPlayerLoadout();
    
    if (IsHost)
    {
        // HOST: Add own loadout
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        playerLoadouts[localClientId] = localLoadout;
        
        Debug.Log($"[Host] Added own loadout: {localLoadout.playerName}");
        
        // Wait for all clients to send their loadouts
        Debug.Log($"[Host] Waiting for {NetworkManager.Singleton.ConnectedClients.Count} client loadouts...");
        
        yield return new WaitUntil(() => 
            playerLoadouts.Count >= NetworkManager.Singleton.ConnectedClients.Count
        );
        
        Debug.Log($"[Host] Received all {playerLoadouts.Count} loadouts!");
        
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

## 🧪 TEST CHECKLIST

### **Test 1: Unity Editor Setup**
```
1. Open GameScene
2. ✅ NetworkManager exists
3. ✅ UnityTransport exists
4. ✅ GameManager has NetworkObject
5. ✅ Player prefabs assigned
6. ✅ BoardManager waypoints assigned (36)
7. ✅ All UI references assigned
```

### **Test 2: Demo Mode (Single Player)**
```
1. GameManager → Demo Mode: TRUE
2. Play GameScene
3. ✅ 1 player spawns
4. ✅ Game starts
5. ✅ Can roll dice
6. ✅ Can move
```

### **Test 3: Multiplayer Mode (2 Players)**
```
1. GameManager → Demo Mode: FALSE
2. Build game (File → Build Settings → Build)
3. Run build (Player 1 - Host)
4. Run Unity Editor (Player 2 - Client)
5. MenuScene → Tìm trận / Tạo phòng
6. ✅ Both connect
7. ✅ LoadingScene shows
8. ✅ GameScene loads
9. ✅ 2 players spawn
10. ✅ Game starts
```

---

## 🚨 COMMON ISSUES & FIXES

### **Issue 1: NetworkManager not found**
```
Error: "NetworkManager not found!"
Fix: Add NetworkManager GameObject to GameScene
```

### **Issue 2: UnityTransport not found**
```
Error: "UnityTransport not found!"
Fix: Add UnityTransport component to NetworkManager
```

### **Issue 3: Player prefabs not assigned**
```
Error: "Player prefabs not assigned!"
Fix: Assign PlayerMale and PlayerFemale prefabs in GameManager Inspector
```

### **Issue 4: NetworkObject missing on GameManager**
```
Error: "NetworkObject component required!"
Fix: Add NetworkObject component to GameManager GameObject
```

### **Issue 5: Players not spawning**
```
Error: "No players spawned"
Fix: 
1. Check Demo Mode = FALSE
2. Check NetworkManager.IsListening = TRUE
3. Check OnNetworkSpawn() called
```

---

## 🎯 SUMMARY

**Setup Required:**
1. ✅ NetworkManager + UnityTransport
2. ✅ GameManager + NetworkObject
3. ✅ Player Prefabs + NetworkObject
4. ✅ BoardManager + Waypoints
5. ✅ All UI references

**Connection Flow:**
1. ✅ MenuScene → Create/Join Relay
2. ✅ LoadingScene → Load GameScene
3. ✅ GameScene → Start Host/Client
4. ✅ GameManager → Spawn players
5. ✅ Game starts

**Next Steps:**
1. ✅ Setup GameScene theo checklist
2. ✅ Test Demo Mode
3. ✅ Test Multiplayer Mode
4. ✅ Build và test 2 players

---

**SẴN SÀNG KẾT NỐI!** 🚀

