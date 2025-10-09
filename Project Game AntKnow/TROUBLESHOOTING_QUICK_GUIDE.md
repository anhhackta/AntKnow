# 🔧 TROUBLESHOOTING QUICK GUIDE

## 🎯 Quick Reference

Hướng dẫn nhanh để fix các lỗi thường gặp khi implement multiplayer.

---

## 🔴 CRITICAL ISSUES

### **Issue 1: Players Cannot Connect**

#### **Symptoms**
- "Connection failed" error
- Stuck at "Connecting..."
- NetworkManager not starting

#### **Quick Fixes**

**Fix 1: Check NetworkManager Setup**
```csharp
// In GameScene, check:
1. NetworkManager GameObject exists
2. UnityTransport component attached
3. NetworkGameManager component attached
```

**Fix 2: Check Relay Connection**
```csharp
// In MenuScene, before loading GameScene:
Debug.Log($"Relay Join Code: {RelayService.Instance.JoinCode}");
Debug.Log($"Is Host: {RelayService.Instance.IsHost}");

// Should see valid join code
```

**Fix 3: Check UGS Configuration**
```
1. Open Unity Dashboard
2. Check Project ID matches
3. Check Relay service enabled
4. Check quotas not exceeded
```

**Fix 4: Restart NetworkManager**
```csharp
if (NetworkManager.Singleton != null) {
    NetworkManager.Singleton.Shutdown();
    await Task.Delay(1000);
    NetworkManager.Singleton.StartHost(); // or StartClient()
}
```

---

### **Issue 2: NetworkVariables Not Syncing**

#### **Symptoms**
- Values different on each client
- Changes not reflected
- "NetworkVariable not initialized" error

#### **Quick Fixes**

**Fix 1: Check Server Authority**
```csharp
// ✅ CORRECT: Only server writes
if (IsServer) {
    myNetworkVar.Value = newValue;
}

// ❌ WRONG: Client writes (will be ignored)
myNetworkVar.Value = newValue;
```

**Fix 2: Check NetworkVariable Declaration**
```csharp
// ✅ CORRECT: Initialize in declaration
private NetworkVariable<int> playerMoney = new NetworkVariable<int>(1500);

// ❌ WRONG: Initialize in Awake/Start
private NetworkVariable<int> playerMoney;
void Awake() {
    playerMoney = new NetworkVariable<int>(1500); // Too late!
}
```

**Fix 3: Check OnValueChanged Subscription**
```csharp
public override void OnNetworkSpawn() {
    base.OnNetworkSpawn();
    
    // Subscribe to changes
    playerMoney.OnValueChanged += OnMoneyChanged;
}

public override void OnNetworkDespawn() {
    base.OnNetworkDespawn();
    
    // Unsubscribe
    playerMoney.OnValueChanged -= OnMoneyChanged;
}

void OnMoneyChanged(int oldValue, int newValue) {
    Debug.Log($"Money changed: {oldValue} → {newValue}");
    UpdateMoneyUI(newValue);
}
```

**Fix 4: Force Sync**
```csharp
// If stuck, force resync
if (IsServer) {
    // Trigger change detection
    var temp = myNetworkVar.Value;
    myNetworkVar.Value = temp;
}
```

---

### **Issue 3: ServerRpc Not Working**

#### **Symptoms**
- ServerRpc not called
- "Not a server" error
- No response from server

#### **Quick Fixes**

**Fix 1: Check Attribute**
```csharp
// ✅ CORRECT
[ServerRpc(RequireOwnership = false)]
void RequestRollDiceServerRpc(ulong clientId) {
    // ...
}

// ❌ WRONG: Missing attribute
void RequestRollDiceServerRpc(ulong clientId) {
    // Won't work!
}
```

**Fix 2: Check Method Name**
```csharp
// ✅ CORRECT: Must end with "ServerRpc"
[ServerRpc(RequireOwnership = false)]
void RequestRollDiceServerRpc() { }

// ❌ WRONG: Missing suffix
[ServerRpc(RequireOwnership = false)]
void RequestRollDice() { } // Won't work!
```

**Fix 3: Check NetworkBehaviour**
```csharp
// ✅ CORRECT: Class inherits NetworkBehaviour
public class NetworkGameManager : NetworkBehaviour {
    [ServerRpc(RequireOwnership = false)]
    void MyServerRpc() { }
}

// ❌ WRONG: Class is MonoBehaviour
public class NetworkGameManager : MonoBehaviour {
    [ServerRpc] // Won't work!
    void MyServerRpc() { }
}
```

**Fix 4: Check IsSpawned**
```csharp
// Before calling ServerRpc
if (!IsSpawned) {
    Debug.LogError("NetworkObject not spawned yet!");
    return;
}

RequestRollDiceServerRpc();
```

---

### **Issue 4: ClientRpc Not Working**

#### **Symptoms**
- ClientRpc not called on clients
- Only server receives call
- "Not a client" error

#### **Quick Fixes**

**Fix 1: Check Attribute**
```csharp
// ✅ CORRECT
[ClientRpc]
void NotifyDiceRolledClientRpc(int dice1, int dice2) {
    // ...
}

// ❌ WRONG: Missing attribute
void NotifyDiceRolledClientRpc(int dice1, int dice2) {
    // Won't work!
}
```

**Fix 2: Check Method Name**
```csharp
// ✅ CORRECT: Must end with "ClientRpc"
[ClientRpc]
void NotifyDiceRolledClientRpc() { }

// ❌ WRONG: Missing suffix
[ClientRpc]
void NotifyDiceRolled() { } // Won't work!
```

**Fix 3: Check Server Calls It**
```csharp
// ✅ CORRECT: Server calls ClientRpc
[ServerRpc(RequireOwnership = false)]
void RequestRollDiceServerRpc() {
    if (!IsServer) return;
    
    int dice1 = Random.Range(1, 7);
    int dice2 = Random.Range(1, 7);
    
    // Broadcast to all clients
    NotifyDiceRolledClientRpc(dice1, dice2);
}

// ❌ WRONG: Client calls ClientRpc
void OnButtonClick() {
    NotifyDiceRolledClientRpc(1, 2); // Won't work!
}
```

---

### **Issue 5: Players Not Spawning**

#### **Symptoms**
- No players in scene
- "Player prefab not found" error
- Players spawn on server only

#### **Quick Fixes**

**Fix 1: Check NetworkManager Prefab List**
```
1. Select NetworkManager GameObject
2. Check "Network Prefabs List"
3. Add Player prefab to list
```

**Fix 2: Check Player Prefab Has NetworkObject**
```
1. Open Player prefab
2. Check NetworkObject component exists
3. Check "Is Player Object" = TRUE (if player-owned)
```

**Fix 3: Check Spawn Code**
```csharp
// ✅ CORRECT: Server spawns
if (IsServer) {
    GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    NetworkObject netObj = player.GetComponent<NetworkObject>();
    netObj.SpawnAsPlayerObject(clientId);
}

// ❌ WRONG: Client spawns
GameObject player = Instantiate(playerPrefab); // Won't sync!
```

**Fix 4: Check OnNetworkSpawn**
```csharp
public override void OnNetworkSpawn() {
    base.OnNetworkSpawn();
    
    Debug.Log($"Player spawned! IsServer: {IsServer}, IsOwner: {IsOwner}");
    
    // Initialize player
    if (IsServer) {
        // Server-side init
    }
    
    if (IsOwner) {
        // Owner-side init (local player)
    }
}
```

---

## 🟡 COMMON ISSUES

### **Issue 6: Dice Results Different on Each Client**

#### **Quick Fix**
```csharp
// ❌ WRONG: Each client rolls independently
void OnRollButton() {
    int dice1 = Random.Range(1, 7); // Different on each client!
    int dice2 = Random.Range(1, 7);
}

// ✅ CORRECT: Server rolls, broadcasts result
[ServerRpc(RequireOwnership = false)]
void RequestRollDiceServerRpc() {
    if (!IsServer) return;
    
    // Server rolls ONCE
    int dice1 = Random.Range(1, 7);
    int dice2 = Random.Range(1, 7);
    
    // Broadcast to all clients
    NotifyDiceRolledClientRpc(dice1, dice2);
}

[ClientRpc]
void NotifyDiceRolledClientRpc(int dice1, int dice2) {
    // All clients receive same result
    ShowDiceResult(dice1, dice2);
}
```

---

### **Issue 7: Money Not Syncing**

#### **Quick Fix**
```csharp
// ✅ CORRECT: Use NetworkVariable
private NetworkVariable<int> playerMoney = new NetworkVariable<int>(1500);

// Server updates
if (IsServer) {
    playerMoney.Value -= cost;
}

// All clients read
void UpdateMoneyUI() {
    moneyText.text = $"${playerMoney.Value}";
}
```

---

### **Issue 8: Property Ownership Not Syncing**

#### **Quick Fix**
```csharp
// ✅ CORRECT: Use NetworkList
private NetworkList<PropertyNetworkData> properties;

void Awake() {
    properties = new NetworkList<PropertyNetworkData>();
}

// Server updates
if (IsServer) {
    var prop = properties[tileId];
    prop.OwnerId = playerId;
    properties[tileId] = prop; // Triggers sync
}

// Subscribe to changes
public override void OnNetworkSpawn() {
    base.OnNetworkSpawn();
    properties.OnListChanged += OnPropertiesChanged;
}

void OnPropertiesChanged(NetworkListEvent<PropertyNetworkData> changeEvent) {
    // Update visuals
    UpdatePropertyVisual(changeEvent.Index);
}
```

---

### **Issue 9: Turn Indicator Not Showing**

#### **Quick Fix**
```csharp
// ✅ CORRECT: Show only on current player
[ClientRpc]
void NotifyTurnChangedClientRpc(int newTurnPlayerId) {
    foreach (var player in allPlayers) {
        bool isMyTurn = (player.PlayerId == newTurnPlayerId);
        player.turnIndicator.SetActive(isMyTurn);
    }
}
```

---

### **Issue 10: UI Panel Not Showing**

#### **Quick Fix**
```csharp
// ✅ CORRECT: Show only for specific player
[ClientRpc]
void ShowPanelBuyClientRpc(int targetPlayerId, int tileId, int price) {
    // Only show for target player
    if (localPlayerId == targetPlayerId) {
        panelBuy.Show(tileId, price);
    }
}
```

---

## 🟢 DEBUGGING TIPS

### **Tip 1: Add Debug Logs**

```csharp
void MyMethod() {
    Debug.Log($"[{(IsServer ? "Server" : "Client")}] MyMethod called");
    
    if (IsServer) {
        Debug.Log($"[Server] Processing...");
    }
    
    if (IsClient) {
        Debug.Log($"[Client] Received update");
    }
}
```

### **Tip 2: Use Conditional Compilation**

```csharp
#if UNITY_EDITOR
    Debug.Log("Running in Editor");
#else
    Debug.Log("Running in Build");
#endif
```

### **Tip 3: Check Network Stats**

```csharp
void Update() {
    if (Input.GetKeyDown(KeyCode.F1)) {
        Debug.Log($"IsServer: {IsServer}");
        Debug.Log($"IsClient: {IsClient}");
        Debug.Log($"IsHost: {IsHost}");
        Debug.Log($"IsOwner: {IsOwner}");
        Debug.Log($"NetworkObjectId: {NetworkObjectId}");
        Debug.Log($"OwnerClientId: {OwnerClientId}");
    }
}
```

### **Tip 4: Use Network Profiler**

```
1. Window → Analysis → Profiler
2. Add "Network Messages" module
3. Record while playing
4. Check RPC calls, NetworkVariable updates
```

---

## 📋 Quick Checklist

### **Before Testing**
- [ ] NetworkManager in scene
- [ ] UnityTransport configured
- [ ] Player prefab in Network Prefabs List
- [ ] Player prefab has NetworkObject
- [ ] All NetworkBehaviours have NetworkObject parent

### **When Adding ServerRpc**
- [ ] Method name ends with "ServerRpc"
- [ ] Has [ServerRpc] attribute
- [ ] Class inherits NetworkBehaviour
- [ ] Called from client
- [ ] Validates on server

### **When Adding ClientRpc**
- [ ] Method name ends with "ClientRpc"
- [ ] Has [ClientRpc] attribute
- [ ] Class inherits NetworkBehaviour
- [ ] Called from server
- [ ] Handles on all clients

### **When Adding NetworkVariable**
- [ ] Initialized in declaration
- [ ] Only server writes
- [ ] Subscribed to OnValueChanged
- [ ] Unsubscribed in OnNetworkDespawn

---

## 🚨 Emergency Commands

### **Reset Network State**
```csharp
if (NetworkManager.Singleton != null) {
    NetworkManager.Singleton.Shutdown();
}
```

### **Force Disconnect**
```csharp
if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient) {
    NetworkManager.Singleton.Shutdown();
}
```

### **Clear All Network Objects**
```csharp
var allNetObjs = FindObjectsOfType<NetworkObject>();
foreach (var obj in allNetObjs) {
    if (obj.IsSpawned) {
        obj.Despawn();
    }
}
```

---

**Status**: Troubleshooting guide ready ✅  
**Usage**: Reference when stuck 🔧

