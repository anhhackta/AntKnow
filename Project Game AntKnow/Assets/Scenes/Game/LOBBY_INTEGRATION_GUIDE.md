# 🔗 Lobby Integration Guide

## 🎯 Goal: Kết nối GameScene với Lobby System

---

## 📋 Overview

### Flow:
```
MenuScene (Lobby)
  ↓
Create/Join Room
  ↓
Start Game (Host)
  ↓
Load GameScene
  ↓
Spawn Players từ Lobby
  ↓
Start Game
```

---

## 🔧 Step 1: Update LobbyUIManager (MenuScene)

### 1.1 Load Player Loadout Data
```csharp
// In LobbyUIManager.cs - OnGameStarting()
// This method ALREADY EXISTS, just need to add LoadLoadoutFromFirebase()

private async void OnGameStarting(string relayJoinCode)
{
    DebugLog($"Game starting with Relay code: {relayJoinCode}");

    // Setup GameSessionData
    var sessionData = GameSessionData.Instance;
    sessionData.SetFromGameDataManager(); // Load basic info from GameDataManager
    sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);

    bool isHost = CustomLobbyService.Instance.IsHost;
    string lobbyId = CustomLobbyService.Instance.CurrentLobby?.Id;
    sessionData.SetNetworkInfo(relayJoinCode, isHost, lobbyId);

    // Load loadout from Firebase (TODO: Implement in GameSessionData)
    sessionData.LoadLoadoutFromFirebase();

    // Calculate total stats
    sessionData.CalculateTotalStats();

    Debug.Log($"[LobbyUIManager] Session data ready: {sessionData.GetSummary()}");

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

    // Load game scene
    SceneManager.LoadScene(GameConfig.GAME_SCENE_NAME);
}
```

### 1.2 Implement LoadLoadoutFromFirebase() in GameSessionData
```csharp
// In GameSessionData.cs - Update LoadLoadoutFromFirebase()

public async void LoadLoadoutFromFirebase()
{
    try
    {
        var db = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
        var docRef = db.Collection("users").Document(firebaseUID)
                       .Collection("loadouts").Document("slot1");

        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            DebugLog("No loadout found, using defaults");
            return;
        }

        var data = snapshot.ToDictionary();

        // Load skill cards
        if (data.ContainsKey("skillCardIds"))
        {
            var cardIds = data["skillCardIds"] as List<object>;
            skillCardIds.Clear();
            skillCards.Clear();

            foreach (var cardId in cardIds)
            {
                skillCardIds.Add(cardId.ToString());
                // TODO: Load card data from inventory
            }
        }

        // Load equipment
        if (data.ContainsKey("equipmentSet"))
        {
            var equipment = data["equipmentSet"] as Dictionary<string, object>;

            if (equipment.ContainsKey("hatId"))
                equipmentSet.hatId = equipment["hatId"]?.ToString();
            if (equipment.ContainsKey("shirtId"))
                equipmentSet.shirtId = equipment["shirtId"]?.ToString();
            if (equipment.ContainsKey("wingsId"))
                equipmentSet.wingsId = equipment["wingsId"]?.ToString();
            if (equipment.ContainsKey("shoesId"))
                equipmentSet.shoesId = equipment["shoesId"]?.ToString();
            if (equipment.ContainsKey("maskId"))
                equipmentSet.maskId = equipment["maskId"]?.ToString();

            // TODO: Load equipment stats from items collection
        }

        DebugLog("Loadout loaded from Firebase");
    }
    catch (System.Exception e)
    {
        DebugLogError($"Failed to load loadout: {e.Message}");
    }
}
```

---

## 🔧 Step 2: Update GameManager (GameScene)

### 2.1 Load Players from Session
```csharp
// Already implemented in GameManager.cs

private IEnumerator LoadPlayersFromLobby()
{
    Debug.Log("[GameManager] Loading players from lobby...");
    
    // Get session data
    var sessionData = GameSessionData.Instance;
    if (sessionData == null || sessionData.players.Count == 0)
    {
        Debug.LogError("[GameManager] No players in session data!");
        yield break;
    }
    
    // Wait for network ready
    yield return new WaitForSeconds(1f);
    
    // Spawn players from session data
    foreach (var playerData in sessionData.players)
    {
        if (IsServer)
        {
            // Get client ID for this player
            // TODO: Map UID to clientId
            ulong clientId = 0; // Placeholder
            
            SpawnPlayerNetwork(
                playerData.playerName,
                playerData.uid,
                playerData.gender == "male",
                playerData.health,
                playerData.agility,
                playerData.intelligence,
                playerData.luck,
                playerData.resistance,
                clientId
            );
        }
    }
    
    Debug.Log($"[GameManager] Loaded {players.Count} players from lobby");
}
```

---

## 🔧 Step 3: Setup GameScene

### 3.1 Add NetworkManager
```
1. Create GameObject: "NetworkManager"
2. Add Component: NetworkManager
3. Configure:
   - Transport: Unity Transport
   - Player Prefab: PlayerPrefab (with NetworkObject)
```

### 3.2 Add GameManager
```
1. Create GameObject: "GameManager"
2. Add Component: GameManager
3. Configure:
   - Demo Mode: FALSE (for multiplayer)
   - Assign all references
```

### 3.3 Setup Player Prefab
```
1. Open PlayerPrefab
2. Add Component: NetworkObject
3. Configure:
   - Synchronize Transform: TRUE
   - Synchronize Position: TRUE
   - Synchronize Rotation: FALSE
   - Synchronize Scale: FALSE
```

---

## 🔧 Step 4: Test Multiplayer

### 4.1 Demo Mode Test (Single Player)
```
1. Open GameScene
2. Set GameManager.demoMode = TRUE
3. Press Play
4. Should spawn 2 test players
5. Test dice roll and movement
```

### 4.2 Multiplayer Test (2 Clients)
```
1. Build game (File > Build Settings > Build)
2. Run build (Client 1)
3. Run Unity Editor (Client 2 - Host)

Client 2 (Host):
1. Login
2. Create Room
3. Wait for Client 1

Client 1:
1. Login
2. Join Room
3. Wait for Host to start

Host:
1. Click "Start Game"
2. Both clients load GameScene
3. Players spawn
4. Test movement
```

---

## 🔧 Step 5: Debug Multiplayer

### 5.1 Check Console Logs
```
Host:
[GameManager] Starting game...
[GameManager] Loading players from lobby...
[GameManager] Loaded 2 players from lobby
[GameManager] Spawned network player: Player 1 (ClientId: 0)
[GameManager] Spawned network player: Player 2 (ClientId: 1)

Client:
[GameManager] Starting game...
[NetworkObject] Spawned as player object
```

### 5.2 Common Issues

#### Issue 1: Players not spawning
```
Error: "[GameManager] No players in session data!"
Fix: Make sure LobbyUIManager loads players before loading GameScene
```

#### Issue 2: Network not connected
```
Error: "[GameManager] NetworkManager not found!"
Fix: Make sure NetworkManager exists in GameScene
```

#### Issue 3: Player prefab missing NetworkObject
```
Error: "Cannot spawn object without NetworkObject component"
Fix: Add NetworkObject component to PlayerPrefab
```

---

## 📊 Data Flow

### Lobby → GameScene:
```
LobbyUIManager.OnGameStarting()
  ↓
Load players from lobby
  ↓
Load loadout from Firestore
  ↓
Store in GameSessionData
  ↓
Load GameScene
  ↓
GameManager.LoadPlayersFromLobby()
  ↓
Spawn players from GameSessionData
  ↓
Start game
```

### GameSessionData Structure:
```csharp
GameSessionData (Already exists in Assets/Script/Game/)
├── relayJoinCode: string
├── isHost: bool
├── lobbyId: string
├── firebaseUID: string
├── unityPlayerId: string
├── playerName: string
├── level: int
├── gender: string
├── antCoin: int
├── dCoin: int
├── skillCardIds: List<string>
├── skillCards: List<SkillCardData>
├── equipmentIds: Dictionary<string, string>
├── equipmentSet: EquipmentSetData
└── Calculated Stats:
    ├── totalHealth: int
    ├── totalAgility: int
    ├── totalIntelligence: int
    ├── totalLuck: int
    └── totalResistance: int

Methods:
- SetFromGameDataManager() - Load từ GameDataManager
- SetNetworkInfo(relayCode, isHost, lobbyId) - Set network info
- SetUnityPlayerId(playerId) - Set Unity player ID
- LoadLoadoutFromFirebase() - Load loadout (TODO)
- CalculateTotalStats() - Calculate total stats
- Clear() - Clear session data
```

---

## 🎮 Testing Checklist

### Demo Mode (Single Player):
- [ ] GameScene opens
- [ ] 2 test players spawn
- [ ] Can roll dice
- [ ] Players move correctly
- [ ] Turn system works

### Multiplayer Mode (2+ Players):
- [ ] Can create room in lobby
- [ ] Can join room in lobby
- [ ] Host can start game
- [ ] All clients load GameScene
- [ ] Players spawn for all clients
- [ ] All clients see same game state
- [ ] Dice rolls sync across clients
- [ ] Movement syncs across clients
- [ ] Turn changes sync across clients

---

## 🚀 Next Steps

### Phase 1: Basic Multiplayer (Current)
- [x] Setup GameSessionData
- [x] Load players from lobby
- [x] Spawn players in GameScene
- [ ] Test with 2 clients
- [ ] Fix sync issues

### Phase 2: Full Multiplayer Sync
- [ ] Sync dice rolls (RPC)
- [ ] Sync player movement (RPC)
- [ ] Sync money changes (NetworkVariable)
- [ ] Sync property ownership (NetworkVariable)
- [ ] Sync turn state (NetworkVariable)

### Phase 3: Advanced Features
- [ ] Handle disconnections
- [ ] Reconnect support
- [ ] Spectator mode
- [ ] Game replay

---

## 📝 Code Templates

### Load Loadout from Firestore:
```csharp
private async Task<LoadoutData> LoadPlayerLoadout(string uid)
{
    try
    {
        var db = FirebaseFirestore.DefaultInstance;
        var docRef = db.Collection("users").Document(uid)
                       .Collection("loadouts").Document("slot1");
        
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (snapshot.Exists)
        {
            var data = snapshot.ToDictionary();
            
            // Get equipment stats
            int health = 0, agility = 0, intelligence = 0, luck = 0, resistance = 0;
            
            if (data.ContainsKey("equipmentSet"))
            {
                var equipment = data["equipmentSet"] as Dictionary<string, object>;
                // TODO: Load equipment stats from items collection
            }
            
            // Get card stats
            if (data.ContainsKey("skillCardIds"))
            {
                var cardIds = data["skillCardIds"] as List<object>;
                // TODO: Load card stats from items collection
            }
            
            return new LoadoutData
            {
                gender = "male", // TODO: Get from user profile
                health = health,
                agility = agility,
                intelligence = intelligence,
                luck = luck,
                resistance = resistance
            };
        }
        
        // Return default if no loadout
        return new LoadoutData
        {
            gender = "male",
            health = 0,
            agility = 0,
            intelligence = 0,
            luck = 0,
            resistance = 0
        };
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[LobbyUIManager] Failed to load loadout: {e.Message}");
        return new LoadoutData { gender = "male", health = 0, agility = 0, intelligence = 0, luck = 0, resistance = 0 };
    }
}
```

---

**Bắt đầu từ Step 1 và test từng bước! 🚀**

