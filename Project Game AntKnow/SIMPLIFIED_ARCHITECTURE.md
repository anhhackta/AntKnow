# 🏗️ SIMPLIFIED ARCHITECTURE - 5 Day Implementation

## 🎯 Architecture Philosophy

**Goal**: Minimal complexity, maximum functionality  
**Strategy**: Reuse existing code, avoid rewrites  
**Pattern**: Server-authoritative with client prediction

---

## 📐 System Architecture

### **High-Level Overview**

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT (Unity)                        │
├─────────────────────────────────────────────────────────┤
│  UI Layer                                                │
│  ├── PanelBuy, PanelResult, PanelPlayerMe              │
│  └── Buttons, Notifications                             │
├─────────────────────────────────────────────────────────┤
│  Presentation Layer (Visual)                             │
│  ├── PlayerGameController (movement animation)          │
│  ├── BoardManager (tile visuals)                        │
│  ├── PropertyManager (house/hotel visuals)              │
│  └── DiceController (dice animation)                    │
├─────────────────────────────────────────────────────────┤
│  Network Layer (NGO)                                     │
│  ├── NetworkGameManager (NetworkBehaviour)              │
│  │   ├── NetworkVariables (read-only on client)         │
│  │   ├── ServerRpc (send requests to server)            │
│  │   └── ClientRpc (receive updates from server)        │
│  └── NetworkManager (Unity NGO)                         │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │ Network (Relay)
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    SERVER (Host)                         │
├─────────────────────────────────────────────────────────┤
│  Network Layer                                           │
│  ├── NetworkGameManager (NetworkBehaviour)              │
│  │   ├── NetworkVariables (write on server)             │
│  │   ├── ServerRpc (receive client requests)            │
│  │   └── ClientRpc (broadcast to all clients)           │
│  └── NetworkManager (Unity NGO)                         │
├─────────────────────────────────────────────────────────┤
│  Game Logic Layer (GameManager)                          │
│  ├── Turn management                                     │
│  ├── Dice rolling                                        │
│  ├── Movement calculation                                │
│  ├── Property transactions                               │
│  └── Game state management                               │
├─────────────────────────────────────────────────────────┤
│  Domain Layer (Pure C# Logic)                            │
│  ├── GameState (game state data)                        │
│  ├── TurnSystem (turn logic)                            │
│  ├── BoardRules (buy/rent/upgrade logic)                │
│  ├── PropertyEconomy (price calculations)               │
│  └── DiceRng (deterministic dice)                       │
└─────────────────────────────────────────────────────────┘
```

---

## 🔧 Component Breakdown

### **1. NetworkGameManager** (Network Layer)

**Role**: Bridge between Unity Network and Game Logic  
**Type**: NetworkBehaviour (singleton)  
**Location**: Attached to GameManager GameObject

#### **NetworkVariables** (Server → Client sync)

```csharp
// Game state
NetworkVariable<int> currentTurnPlayerId;
NetworkVariable<GamePhase> currentPhase;
NetworkVariable<int> currentRound;

// Dice state
NetworkVariable<int> lastDiceRoll1;
NetworkVariable<int> lastDiceRoll2;

// Player states (NetworkList)
NetworkList<PlayerNetworkData> players;

// Property states (NetworkList)
NetworkList<PropertyNetworkData> properties;
```

#### **ServerRpc** (Client → Server requests)

```csharp
[ServerRpc(RequireOwnership = false)]
void RequestRollDiceServerRpc(ulong clientId);

[ServerRpc(RequireOwnership = false)]
void RequestBuyPropertyServerRpc(ulong clientId, int tileId);

[ServerRpc(RequireOwnership = false)]
void RequestUpgradePropertyServerRpc(ulong clientId, int tileId);

[ServerRpc(RequireOwnership = false)]
void RequestEndTurnServerRpc(ulong clientId);
```

#### **ClientRpc** (Server → All Clients broadcasts)

```csharp
[ClientRpc]
void NotifyDiceRolledClientRpc(int dice1, int dice2, int playerId);

[ClientRpc]
void NotifyPlayerMovedClientRpc(int playerId, int fromTile, int toTile);

[ClientRpc]
void NotifyPropertyBoughtClientRpc(int tileId, int ownerId, int price);

[ClientRpc]
void NotifyMoneyChangedClientRpc(int playerId, int newMoney, string reason);

[ClientRpc]
void NotifyTurnChangedClientRpc(int newTurnPlayerId);

[ClientRpc]
void NotifyGameEndedClientRpc(int winnerId, int[] finalScores);
```

---

### **2. GameManager** (Game Logic Layer)

**Role**: Main game controller (server-side logic)  
**Type**: MonoBehaviour  
**Location**: GameScene

#### **Responsibilities**

- Initialize game state
- Handle turn flow
- Process dice rolls
- Calculate movement
- Validate property transactions
- Apply tile effects
- Detect end game

#### **Key Methods**

```csharp
// Initialization
void StartGame()
void SpawnPlayers()

// Turn management
void StartTurn(int playerId)
void EndTurn(int playerId)

// Dice & Movement
void RollDice(int playerId)
void MovePlayer(int playerId, int steps)

// Property
void BuyProperty(int playerId, int tileId)
void UpgradeProperty(int playerId, int tileId)
void PayRent(int playerId, int tileId)

// Tile effects
void ResolveTile(int playerId, int tileId)

// End game
void CheckEndConditions()
void EndGame()
```

---

### **3. Domain Layer** (Pure Logic)

**Role**: Game rules and calculations (no Unity dependencies)  
**Type**: Pure C# classes  
**Location**: `Assets/Script/Domain/`

#### **GameState.cs**
```csharp
public class GameState {
    public int BoardLength;
    public int CurrentTurnPlayerId;
    public List<PlayerState> Players;
    public Dictionary<int, PropertyState> Properties;
}
```

#### **TurnSystem.cs**
```csharp
public class TurnSystem {
    public int GetNextPlayer(GameState state);
    public bool CanPlayerAct(GameState state, int playerId);
    public void AdvanceTurn(GameState state);
}
```

#### **BoardRules.cs**
```csharp
public class BoardRules {
    public bool CanBuyProperty(PlayerState player, PropertyState property);
    public int CalculateRent(PropertyState property);
    public bool CanUpgradeProperty(PlayerState player, PropertyState property);
    public int CalculateUpgradeCost(PropertyState property);
}
```

#### **DiceRng.cs**
```csharp
public class DiceRng {
    public (int, int) Roll(int seed);
}
```

---

### **4. Presentation Layer** (Visual)

**Role**: Visual representation (client-side)  
**Type**: MonoBehaviours  
**Location**: `Assets/Scenes/Game/Scripts/`

#### **PlayerGameController.cs**
- Animate player movement
- Show turn indicator
- Update player UI

#### **BoardManager.cs**
- Manage tile visuals
- Highlight tiles

#### **PropertyManager.cs**
- Spawn house/hotel models
- Update property colors

#### **DiceController.cs**
- Animate dice roll
- Show dice result

---

## 🔄 Data Flow Examples

### **Example 1: Roll Dice**

```
1. CLIENT: Player clicks "Roll" button
   └─> NetworkGameManager.RequestRollDiceServerRpc(clientId)

2. SERVER: Receives request
   └─> Validate: Is it this player's turn?
   └─> GameManager.RollDice(playerId)
       └─> DiceRng.Roll(seed) → (dice1, dice2)
       └─> Update NetworkVariable: lastDiceRoll1, lastDiceRoll2
       └─> NetworkGameManager.NotifyDiceRolledClientRpc(dice1, dice2, playerId)

3. ALL CLIENTS: Receive notification
   └─> DiceController.AnimateDice(dice1, dice2)
   └─> Wait for animation
   └─> GameManager.MovePlayer(playerId, dice1 + dice2)
```

### **Example 2: Buy Property**

```
1. CLIENT: Player lands on property tile
   └─> Show PanelBuy (local UI)
   └─> Player clicks "Buy"
   └─> NetworkGameManager.RequestBuyPropertyServerRpc(clientId, tileId)

2. SERVER: Receives request
   └─> Validate: Does player have enough money?
   └─> Validate: Is property unowned?
   └─> BoardRules.CanBuyProperty(player, property) → true
   └─> Update GameState:
       └─> player.Money -= property.Price
       └─> property.Owner = playerId
   └─> Update NetworkVariables:
       └─> players[playerId].Money = newMoney
       └─> properties[tileId].Owner = playerId
   └─> NetworkGameManager.NotifyPropertyBoughtClientRpc(tileId, playerId, price)

3. ALL CLIENTS: Receive notification
   └─> PropertyManager.UpdatePropertyVisual(tileId, playerId)
   └─> Show notification: "Player X bought Property Y"
   └─> Update UI: Player money
```

### **Example 3: End Turn**

```
1. CLIENT: Player clicks "End Turn"
   └─> NetworkGameManager.RequestEndTurnServerRpc(clientId)

2. SERVER: Receives request
   └─> Validate: Is it this player's turn?
   └─> TurnSystem.AdvanceTurn(gameState)
       └─> currentTurnPlayerId = nextPlayerId
   └─> Update NetworkVariable: currentTurnPlayerId
   └─> NetworkGameManager.NotifyTurnChangedClientRpc(nextPlayerId)

3. ALL CLIENTS: Receive notification
   └─> Update turn indicator (yellow ping)
   └─> Enable/disable roll button based on local player
   └─> Show notification: "Player X's turn"
```

---

## 📊 Network Data Structures

### **PlayerNetworkData** (INetworkSerializable)

```csharp
public struct PlayerNetworkData : INetworkSerializable, IEquatable<PlayerNetworkData> {
    public int PlayerId;
    public FixedString64Bytes PlayerName;
    public int Money;
    public int CurrentTile;
    public int JailTurns;
    public bool IsActive;
    
    // Stats
    public int Health;
    public int Agility;
    public int Intelligence;
    public int Luck;
    public int Resistance;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Money);
        serializer.SerializeValue(ref CurrentTile);
        serializer.SerializeValue(ref JailTurns);
        serializer.SerializeValue(ref IsActive);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref Agility);
        serializer.SerializeValue(ref Intelligence);
        serializer.SerializeValue(ref Luck);
        serializer.SerializeValue(ref Resistance);
    }
    
    public bool Equals(PlayerNetworkData other) {
        return PlayerId == other.PlayerId && 
               Money == other.Money && 
               CurrentTile == other.CurrentTile;
    }
}
```

### **PropertyNetworkData** (INetworkSerializable)

```csharp
public struct PropertyNetworkData : INetworkSerializable, IEquatable<PropertyNetworkData> {
    public int TileId;
    public int OwnerId; // 0 = unowned
    public int Level; // 0-4 = houses, 5 = hotel
    public int BasePrice;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref TileId);
        serializer.SerializeValue(ref OwnerId);
        serializer.SerializeValue(ref Level);
        serializer.SerializeValue(ref BasePrice);
    }
    
    public bool Equals(PropertyNetworkData other) {
        return TileId == other.TileId && 
               OwnerId == other.OwnerId && 
               Level == other.Level;
    }
}
```

---

## 🎮 Game Flow

### **1. Game Start**

```
1. Players join lobby (MenuScene)
2. Host starts game
3. Load GameScene
4. NetworkGameManager.OnNetworkSpawn()
   └─> If IsServer:
       └─> Initialize GameState
       └─> Spawn players
       └─> Sync initial state
   └─> If IsClient:
       └─> Wait for initial state
       └─> Setup UI
5. GameManager.StartGame()
   └─> Start first turn
```

### **2. Turn Flow**

```
1. StartTurn(playerId)
   └─> Enable roll button (for current player)
   └─> Show turn indicator

2. Player rolls dice
   └─> RequestRollDiceServerRpc()
   └─> Server: Roll dice, broadcast result
   └─> All clients: Animate dice

3. Player moves
   └─> Server: Calculate new position
   └─> All clients: Animate movement

4. Resolve tile
   └─> Server: Apply tile effect
   └─> All clients: Show UI/notification

5. Player ends turn
   └─> RequestEndTurnServerRpc()
   └─> Server: Advance turn
   └─> All clients: Update turn indicator
```

### **3. Game End**

```
1. Server detects end condition
   └─> Max turns reached OR
   └─> Only 1 player has money

2. Server calculates final scores
   └─> Money + Property values

3. Server broadcasts end game
   └─> NotifyGameEndedClientRpc(winnerId, scores)

4. All clients show PanelResult

5. Server saves results to Firebase
   └─> Call awardMatch Cloud Function
```

---

## 🚀 Implementation Priority

### **Phase 1: Foundation** (Day 1)
1. NetworkGameManager setup
2. Player spawning
3. Basic state sync

### **Phase 2: Core Gameplay** (Day 2-3)
1. Turn system
2. Dice rolling
3. Movement
4. Property buy/rent

### **Phase 3: Features** (Day 4)
1. Property upgrades
2. Special tiles
3. UI polish

### **Phase 4: Polish** (Day 5)
1. End game
2. Testing
3. Bug fixes

---

**Status**: Architecture defined ✅  
**Next**: Implement Day 1 tasks 🚀

