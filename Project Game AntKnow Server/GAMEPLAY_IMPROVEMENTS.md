# 🎮 GAMEPLAY IMPROVEMENTS - SERVER FIXES

## 📋 SUMMARY

Phân tích quy trình gameplay hiện tại và đề xuất các cải tiến cần thiết để server hoạt động đúng logic game.

---

## 🔄 QUY TRÌNH GAMEPLAY HIỆN TẠI

### **1. GAME FLOW (Đã Implement)**

```
┌─────────────────────────────────────────────────────────────┐
│  SERVER BOOTSTRAP → NETWORK SPAWN → WAIT FOR CLIENTS (2+)  │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│  START GAME → Initialize GameState, Properties, Players    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│  TURN LOOP:                                                 │
│    1. StartNextTurn() → Notify Clients                      │
│    2. Client: RequestRollDiceServerRpc()                    │
│    3. Server: Roll Dice (Random) → Update Position          │
│    4. ResolveTile() → Handle tile logic                     │
│    5. EndCurrentTurn() → Next Player                        │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│  END GAME → Calculate Scores → Determine Winner            │
└─────────────────────────────────────────────────────────────┘
```

### **2. TILE RESOLUTION (Hiện tại)**

| Tile Type    | Logic Hiện Tại                      | Status |
|--------------|-------------------------------------|--------|
| **Property** | Owner None → Log (no buy action)    | ⚠️     |
|              | Owner ≠ Player → Pay Rent           | ✅     |
| **Event**    | Log "waiting for client"            | ❌     |
| **Quiz**     | Log "waiting for client"            | ❌     |
| **Jail**     | Set JailTurns = 3                   | ⚠️     |
| **Travel**   | Log "waiting for client"            | ❌     |
| **Start**    | No action (missing salary)          | ❌     |

---

## ⚠️ CRITICAL ISSUES (Cần Fix Ngay)

### **❌ Issue 1: Thiếu Xử Lý Qua Ô Start**

**Vấn đề:**
```csharp
// ServerGameManager.cs Line 316
player.NodeIndex = (player.NodeIndex + total) % gameState.BoardLength;
```

- Chỉ update position, không cộng salary khi qua ô Start
- `TurnSystem.MoveAndResolve()` đã có logic này nhưng không dùng

**Fix:**
```csharp
// Use TurnSystem instead
int oldPosition = player.NodeIndex;
int newPosition = (oldPosition + total) % gameState.BoardLength;

// Check if passed Start (crossed 0)
if (oldPosition + total >= gameState.BoardLength)
{
    BoardRules.OnPassStart(player, 200); // +200 salary
    Debug.Log($"[ServerGameManager] Player {player.Id} passed Start! +200");
}

player.NodeIndex = newPosition;
```

---

### **❌ Issue 2: Thiếu ServerRpc cho Game Actions**

**Các ServerRpc cần thêm:**

#### **A. BuyPropertyServerRpc**
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestBuyPropertyServerRpc(int tileId, ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    var tileData = SimpleBoardConfig.GetTile(tileId);
    
    if (gameState.Properties.TryGetValue(tileId, out var property))
    {
        // Validate purchase
        if (!BoardRules.CanBuy(player, property))
        {
            NotifyErrorClientRpc(playerId, "Cannot buy this property!");
            return;
        }
        
        // Execute purchase
        BoardRules.Buy(player, property);
        
        Debug.Log($"[ServerGameManager] Player {playerId} bought {tileData.name} for {property.BasePrice}");
        
        // Notify all clients
        NotifyPropertyBoughtClientRpc(playerId, tileId, property.BasePrice);
        
        // End turn after purchase
        Invoke(nameof(EndCurrentTurn), 2f);
    }
}
```

#### **B. UpgradePropertyServerRpc**
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestUpgradePropertyServerRpc(int tileId, bool toHotel, ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    var tileData = SimpleBoardConfig.GetTile(tileId);
    var property = gameState.Properties[tileId];
    
    if (toHotel)
    {
        if (BoardRules.CanUpgradeHotel(player, property, tileData))
        {
            BoardRules.UpgradeHotel(player, property, tileData);
            Debug.Log($"[ServerGameManager] Player {playerId} upgraded {tileData.name} to HOTEL");
            NotifyPropertyUpgradedClientRpc(playerId, tileId, 5, true);
        }
    }
    else
    {
        if (BoardRules.CanUpgradeHouse(player, property, tileData))
        {
            BoardRules.UpgradeHouse(player, property, tileData);
            Debug.Log($"[ServerGameManager] Player {playerId} upgraded {tileData.name} to Level {property.Level}");
            NotifyPropertyUpgradedClientRpc(playerId, tileId, property.Level, false);
        }
    }
}
```

#### **C. DrawEventCardServerRpc**
```csharp
private EventCardHandler eventCardHandler; // Initialize in StartGame()

[ServerRpc(RequireOwnership = false)]
public void RequestDrawEventCardServerRpc(ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    
    // Draw random event card
    var card = eventCardHandler.DrawEventCard();
    
    Debug.Log($"[ServerGameManager] Player {playerId} drew: {card.name}");
    
    // Execute card effect
    var result = eventCardHandler.ExecuteEventCard(card, player, gameState);
    
    // Notify all clients
    NotifyEventCardDrawnClientRpc(playerId, card.id, card.name, result.message);
    
    // End turn after event
    Invoke(nameof(EndCurrentTurn), 3f);
}
```

#### **D. AnswerQuizServerRpc**
```csharp
[ServerRpc(RequireOwnership = false)]
public void SubmitQuizAnswerServerRpc(int questionId, int answerIndex, ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    
    // Validate answer (TODO: Load from Firebase)
    bool isCorrect = ValidateQuizAnswer(questionId, answerIndex);
    
    if (isCorrect)
    {
        player.Money += 100; // Bonus for correct answer
        Debug.Log($"[ServerGameManager] Player {playerId} answered correctly! +100");
        NotifyQuizResultClientRpc(playerId, true, 100);
    }
    else
    {
        player.Money -= 50; // Penalty for wrong answer
        Debug.Log($"[ServerGameManager] Player {playerId} answered incorrectly! -50");
        NotifyQuizResultClientRpc(playerId, false, -50);
    }
    
    // End turn after quiz
    Invoke(nameof(EndCurrentTurn), 2f);
}

private bool ValidateQuizAnswer(int questionId, int answerIndex)
{
    // TODO: Load correct answer from Firebase
    // For now, return random for testing
    return Random.Range(0, 2) == 0;
}
```

#### **E. TravelServerRpc**
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestTravelToTileServerRpc(int targetTileIndex, ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    
    // Validate travel (must be from Travel tile)
    var currentTile = SimpleBoardConfig.GetTileByWaypointIndex(player.NodeIndex);
    if (currentTile.type != TileType.Travel)
    {
        Debug.LogWarning($"[ServerGameManager] Player {playerId} not on Travel tile!");
        return;
    }
    
    // Travel to target tile
    int oldPosition = player.NodeIndex;
    player.NodeIndex = targetTileIndex;
    
    Debug.Log($"[ServerGameManager] Player {playerId} traveled: {oldPosition} → {targetTileIndex}");
    
    // Notify clients
    NotifyTravelClientRpc(playerId, oldPosition, targetTileIndex);
    
    // Resolve target tile
    Invoke(nameof(ResolveTileForCurrentPlayer), 2f);
}
```

#### **F. UseSkillCardServerRpc**
```csharp
private SkillTriggerEngine skillEngine; // Initialize in StartGame()

[ServerRpc(RequireOwnership = false)]
public void RequestUseSkillCardServerRpc(string cardItemId, int targetTileId, ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    
    // Get card data
    var cardData = BasicSkillCards.GetCardByItemId(cardItemId);
    if (cardData == null)
    {
        Debug.LogWarning($"[ServerGameManager] Card {cardItemId} not found!");
        return;
    }
    
    // Create card instance (TODO: Load from player inventory)
    var cardInstance = new SkillCardInstance
    {
        instanceId = cardItemId,
        itemId = cardItemId,
        level = 1,
        stars = 0,
        effectiveCooldown = cardData.skill.cooldownBaseTurns,
        currentCooldown = 0 // Assume ready
    };
    
    // Create execution context
    var context = new SkillExecutionContext
    {
        tileIndex = player.NodeIndex,
        property = targetTileId > 0 && gameState.Properties.ContainsKey(targetTileId) 
            ? gameState.Properties[targetTileId] 
            : null
    };
    
    // Execute skill
    var result = skillEngine.ExecuteSkill(cardInstance, cardData, player, gameState, context);
    
    Debug.Log($"[ServerGameManager] Player {playerId} used skill {cardData.name}: {result.message}");
    
    // Notify clients
    NotifySkillUsedClientRpc(playerId, cardItemId, result.success, result.message);
}
```

---

### **❌ Issue 3: Thiếu Xử Lý Jail Logic**

**Vấn đề:**
- Set `JailTurns = 3` nhưng không skip turn
- Không giảm `JailTurns` mỗi turn
- Không có cách escape jail

**Fix trong StartNextTurn():**
```csharp
private void StartNextTurn()
{
    if (!IsServer || !gameActive) return;

    currentPlayerIndex = (currentPlayerIndex) % gameState.Players.Count;
    currentPlayerTurn.Value = currentPlayerIndex;
    turnStartTime = Time.time;

    var currentPlayer = gameState.Players[currentPlayerIndex];
    
    // ===== NEW: Handle Jail =====
    if (currentPlayer.JailTurns > 0)
    {
        Debug.Log($"[ServerGameManager] Player {currentPlayer.Id} is in jail for {currentPlayer.JailTurns} more turns");
        
        // Decrease jail turns
        currentPlayer.JailTurns--;
        
        // Notify client that turn is skipped
        NotifyTurnSkippedDueToJailClientRpc(currentPlayer.Id, currentPlayer.JailTurns);
        
        // Move to next player immediately
        currentPlayerIndex++;
        Invoke(nameof(StartNextTurn), 2f);
        return;
    }
    // ===== END NEW =====

    Debug.Log($"[ServerGameManager] ===== TURN {currentTurn.Value}: Player {currentPlayer.Id} =====");
    
    NotifyTurnStartClientRpc(currentPlayerIndex, currentPlayer.Id);
    currentPlayerIndex++;
}

// New ClientRpc
[ClientRpc]
private void NotifyTurnSkippedDueToJailClientRpc(int playerId, int turnsLeft)
{
    Debug.Log($"[Client] Player {playerId} skipped turn (Jail: {turnsLeft} turns left)");
    // Client: Show jail UI
}
```

**Option: Pay to escape jail:**
```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestEscapeJailServerRpc(ServerRpcParams rpcParams = default)
{
    if (!gameActive) return;
    
    ulong clientId = rpcParams.Receive.SenderClientId;
    if (!clientToPlayerMap.TryGetValue(clientId, out int playerId)) return;
    
    var player = gameState.Players.Find(p => p.Id == playerId);
    
    if (player.JailTurns <= 0)
    {
        Debug.LogWarning($"[ServerGameManager] Player {playerId} not in jail!");
        return;
    }
    
    int escapePrice = 100;
    if (player.Money < escapePrice)
    {
        NotifyErrorClientRpc(playerId, "Not enough money to escape jail!");
        return;
    }
    
    // Pay to escape
    player.Money -= escapePrice;
    player.JailTurns = 0;
    
    Debug.Log($"[ServerGameManager] Player {playerId} paid {escapePrice} to escape jail!");
    NotifyJailEscapeClientRpc(playerId, escapePrice);
}
```

---

### **❌ Issue 4: Bug trong NotifyRentPaidClientRpc**

**Vấn đề:**
```csharp
[ClientRpc]
private void NotifyRentPaidClientRpc(int payerId, int ownerId, int amount)
{
    Debug.Log($"[Client] Player {payerId} paid {amount} rent to Player {ownerId}");
    // TODO: Update UI
    // For now, just end turn after 2 seconds
    
    Invoke(nameof(EndCurrentTurn), 2f); // ❌ CRASH! EndCurrentTurn() is SERVER-ONLY
}
```

**Fix:**
```csharp
private void HandlePropertyTile(PlayerState player, SimpleTileData tileData)
{
    if (!gameState.Properties.ContainsKey(tileData.index))
    {
        Debug.LogWarning($"[ServerGameManager] Property {tileData.index} not found in game state");
        return;
    }

    var property = gameState.Properties[tileData.index];

    if (property.Owner == Owner.None)
    {
        Debug.Log($"[ServerGameManager] Property {tileData.name} is available for purchase (Price: {tileData.basePrice})");
        
        // Notify client to show buy panel
        NotifyPropertyAvailableClientRpc(player.Id, tileData.index, tileData.basePrice);
        
        // Don't end turn automatically - wait for client to buy or skip
    }
    else if ((int)property.Owner != player.Id)
    {
        var owner = gameState.Players.Find(p => p.Id == (int)property.Owner);
        if (owner != null)
        {
            int rent = BoardRules.CalcRent(tileData, property, owner);
            BoardRules.PayRent(player, owner, rent);

            Debug.Log($"[ServerGameManager] Player {player.Id} paid {rent} rent to Player {owner.Id}");

            // Notify clients
            NotifyRentPaidClientRpc(player.Id, owner.Id, rent);
            
            // ===== NEW: End turn on SERVER side =====
            Invoke(nameof(EndCurrentTurn), 2f);
            // ===== END NEW =====
        }
    }
    else
    {
        Debug.Log($"[ServerGameManager] Player {player.Id} landed on own property {tileData.name}");
        
        // Notify client to show upgrade options
        NotifyOwnPropertyLandedClientRpc(player.Id, tileData.index);
        
        // Don't end turn - wait for client to upgrade or skip
    }
}

[ClientRpc]
private void NotifyRentPaidClientRpc(int payerId, int ownerId, int amount)
{
    Debug.Log($"[Client] Player {payerId} paid {amount} rent to Player {ownerId}");
    // Client: Update UI, show animation
    // NO SERVER LOGIC HERE
}

[ClientRpc]
private void NotifyPropertyAvailableClientRpc(int playerId, int tileId, int price)
{
    Debug.Log($"[Client] Property {tileId} available for {price}");
    // Client: Show buy panel
}

[ClientRpc]
private void NotifyOwnPropertyLandedClientRpc(int playerId, int tileId)
{
    Debug.Log($"[Client] Player {playerId} landed on own property {tileId}");
    // Client: Show upgrade panel
}
```

---

### **❌ Issue 5: Thiếu Network Sync cho Player Data**

**Vấn đề:**
- Player money, position chỉ lưu trong `GameState` (server-side)
- Client không biết các player khác có bao nhiêu tiền, ở đâu

**Fix: Thêm NetworkVariable hoặc NetworkList:**

**Option 1: NetworkList (Recommended)**
```csharp
// Add to ServerGameManager
private NetworkList<PlayerNetworkData> networkPlayers;

private struct PlayerNetworkData : INetworkSerializable
{
    public int playerId;
    public int money;
    public int nodeIndex;
    public int jailTurns;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref money);
        serializer.SerializeValue(ref nodeIndex);
        serializer.SerializeValue(ref jailTurns);
    }
}

private void Awake()
{
    networkPlayers = new NetworkList<PlayerNetworkData>();
}

// Sync after each change
private void SyncPlayerData(PlayerState player)
{
    if (!IsServer) return;
    
    for (int i = 0; i < networkPlayers.Count; i++)
    {
        if (networkPlayers[i].playerId == player.Id)
        {
            networkPlayers[i] = new PlayerNetworkData
            {
                playerId = player.Id,
                money = player.Money,
                nodeIndex = player.NodeIndex,
                jailTurns = player.JailTurns
            };
            return;
        }
    }
}
```

**Option 2: ClientRpc after each change**
```csharp
// Call this after EVERY money/position change
[ClientRpc]
private void SyncPlayerStateClientRpc(int playerId, int money, int nodeIndex, int jailTurns)
{
    Debug.Log($"[Client] Player {playerId} synced: Money={money}, Pos={nodeIndex}, Jail={jailTurns}");
    // Client: Update UI
}
```

---

## ✅ ĐỀ XUẤT TỐI ƯU

### **1. Refactor ServerGameManager**

Tách các ServerRpc vào modules riêng:
- `ServerPropertyManager.cs` - Buy, Upgrade, Takeover
- `ServerEventManager.cs` - Event cards
- `ServerQuizManager.cs` - Quiz logic
- `ServerSkillManager.cs` - Skill cards
- `ServerSyncManager.cs` - Network sync

### **2. Thêm Turn Management**

```csharp
private enum TurnPhase
{
    WaitingForRoll,      // Chờ player roll dice
    DiceRolled,          // Đã roll, đang di chuyển
    TileResolution,      // Đang xử lý ô đất
    WaitingForAction,    // Chờ player buy/upgrade/answer quiz
    TurnEnding           // Kết thúc lượt
}

private TurnPhase currentTurnPhase = TurnPhase.WaitingForRoll;
```

### **3. Thêm Validation & Error Handling**

```csharp
private bool ValidatePlayerAction(ulong clientId, out int playerId, out PlayerState player)
{
    if (!clientToPlayerMap.TryGetValue(clientId, out playerId))
    {
        Debug.LogWarning($"[ServerGameManager] Unknown client {clientId}");
        return false;
    }
    
    player = gameState.Players.Find(p => p.Id == playerId);
    if (player == null)
    {
        Debug.LogError($"[ServerGameManager] Player {playerId} not found in game state!");
        return false;
    }
    
    return true;
}
```

### **4. Thêm Reconnection Handling**

```csharp
private void OnClientDisconnected(ulong clientId)
{
    Debug.Log($"[ServerGameManager] Client {clientId} disconnected");

    if (gameActive)
    {
        if (clientToPlayerMap.ContainsKey(clientId))
        {
            int playerId = clientToPlayerMap[clientId];
            
            // Mark player as disconnected (don't remove yet)
            var player = gameState.Players.Find(p => p.Id == playerId);
            if (player != null)
            {
                // TODO: Add IsConnected flag to PlayerState
                // player.IsConnected = false;
                
                // Wait for reconnection (30s timeout)
                StartCoroutine(WaitForReconnection(clientId, playerId));
            }
        }
    }
}

private IEnumerator WaitForReconnection(ulong clientId, int playerId)
{
    float timeout = 30f;
    float elapsed = 0f;
    
    while (elapsed < timeout)
    {
        // Check if client reconnected
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.Log($"[ServerGameManager] Player {playerId} reconnected!");
            // TODO: Sync game state to reconnected client
            yield break;
        }
        
        yield return new WaitForSeconds(1f);
        elapsed += 1f;
    }
    
    // Timeout - remove player from game
    Debug.LogWarning($"[ServerGameManager] Player {playerId} failed to reconnect. Removing from game.");
    RemovePlayerFromGame(playerId);
}
```

---

## 📊 PRIORITY IMPLEMENTATION

### **Phase 1: Critical Fixes (1-2 giờ)**
1. ✅ Fix qua ô Start (+200 salary)
2. ✅ Fix NotifyRentPaidClientRpc bug
3. ✅ Add BuyPropertyServerRpc
4. ✅ Add Jail skip turn logic

### **Phase 2: Core Features (2-3 giờ)**
5. ✅ Add UpgradePropertyServerRpc
6. ✅ Add DrawEventCardServerRpc + EventCardHandler
7. ✅ Add AnswerQuizServerRpc (stub)
8. ✅ Add network sync for player data

### **Phase 3: Advanced Features (3-4 giờ)**
9. ✅ Add TravelServerRpc
10. ✅ Add UseSkillCardServerRpc + SkillTriggerEngine
11. ✅ Add reconnection handling
12. ✅ Refactor into modules

---

## 🚀 NEXT STEPS

1. **Review file này** để confirm approach
2. **Implement Phase 1** fixes trước
3. **Test với 2 clients** xem có crash không
4. **Implement Phase 2** sau khi Phase 1 ổn
5. **Client integration** - Tạo UI để call các ServerRpc

Bạn muốn tôi implement Phase 1 ngay không? 🎯

