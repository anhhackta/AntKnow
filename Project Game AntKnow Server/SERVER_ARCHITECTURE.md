# 🎮 ANTKNOW SERVER ARCHITECTURE

**Server-Authoritative Multiplayer Board Game**

---

## 📋 GAME FLOW OVERVIEW

### **1. Pre-Game (Client-Side)**
```
LoginScene → MenuScene → Lobby/Matchmaking
├── Firebase Auth (email/username login)
├── Unity Matchmaking (find match)
├── Unity Lobby (create/join lobby)
└── Load player loadout (2 skill cards + 5 equipment)
```

### **2. Game Start (Server-Side)**
```
Server receives 2-4 players
├── Initialize GameState
├── Load player stats from loadout
│   ├── Health, Agility, Intelligence, Luck, Resistance
│   ├── 2 skill cards (passive/active)
│   └── 5 equipment (hat, shirt, wings, shoes, mask)
├── Determine first player (random or highest stat)
└── Start turn system
```

### **3. Gameplay Loop (Server-Authoritative)**
```
Turn Loop (per player):
├── 1. Roll Dice (server-side RNG)
├── 2. Move Player (server updates position)
├── 3. Resolve Tile
│   ├── Property → PanelBuy (if unowned) or Pay Rent
│   ├── Event → Draw Event Card
│   ├── Special Tiles:
│   │   ├── Start → +200 salary
│   │   ├── Tax → -money
│   │   ├── Bonus → +money
│   │   ├── Accident → Go to jail
│   │   ├── Quiz → Answer question
│   │   ├── Travel → Teleport
│   │   └── Jail → Skip turns
│   └── Card System → Trigger passive/active skills
├── 4. End Turn
└── Next Player

Round = All players completed 1 turn
```

### **4. End Game Conditions**
```
Game ends when:
├── Max rounds reached (e.g., 50 rounds)
├── Only 1 player left (others bankrupt)
└── Time limit reached (optional)

Winner determination:
├── Last player standing, OR
├── Highest total assets (money + properties + houses/hotels)

Post-game:
├── Call Cloud Function: awardMatch(rank, durationSec)
├── Reward: AntCoin + XP based on rank (1st/2nd/3rd/4th)
└── Return to MenuScene
```

---

## 🏗️ SERVER ARCHITECTURE

### **Domain Layer (Pure C# Logic)**
```
Assets/Script/Domain/
├── Enums.cs ✅
│   ├── TileType (Start, Property, Tax, Bonus, Chance, Accident, Quiz, Travel, Jail, GoToJail, FreeParking)
│   ├── Owner (None, P1, P2, P3, P4)
│   ├── CardType (Passive, Active)
│   └── CardTrigger (Manual, StartOfTurn, EndOfTurn, OnQuizFail, OnRentPay, OnRentReceive, OnTravel, Custom)
│
├── Entities/
│   ├── GameState.cs ✅
│   │   ├── CurrentTurnPlayerId
│   │   ├── Round
│   │   ├── Players (List<PlayerState>)
│   │   ├── Properties (Dictionary<int, PropertyState>)
│   │   └── BoardLength
│   │
│   ├── PlayerState.cs ✅
│   │   ├── Id (1-4)
│   │   ├── Money
│   │   ├── NodeIndex (current tile 0-35)
│   │   ├── JailTurns (0 if free)
│   │   ├── Stats: Health, Agility, Intelligence, Luck, Resistance
│   │   └── OwnedPropertyIds (List<int>)
│   │
│   ├── PropertyState.cs ✅
│   │   ├── TileId
│   │   ├── Owner (enum)
│   │   ├── Level (0-5: houses)
│   │   ├── HasHotel (bool)
│   │   └── BasePrice
│   │
│   └── CardDefinition.cs ✅
│       ├── CardId
│       ├── Name
│       ├── Type (Passive/Active)
│       ├── Trigger
│       ├── Description
│       ├── Cost
│       ├── StatModifiers
│       ├── ResourceModifiers
│       └── CooldownTurns
│
└── Services/
    ├── TurnSystem.cs ✅
    │   ├── MoveAndResolve(steps)
    │   ├── ResolveTile(player, tileId)
    │   └── EndTurn()
    │
    ├── BoardRules.cs ✅
    │   ├── OnPassStart(player, salary)
    │   ├── OnTax(player, amount)
    │   ├── OnBonus(player, amount)
    │   ├── CanBuy(player, property)
    │   ├── Buy(player, property)
    │   ├── CalcRent(property, owner)
    │   ├── PayRent(payer, receiver, amount)
    │   ├── CanUpgradeHouse(player, property)
    │   ├── UpgradeHouse(player, property)
    │   ├── CanUpgradeHotel(player, property)
    │   └── UpgradeHotel(player, property)
    │
    ├── PropertyEconomy.cs ✅
    │   ├── GetBaseRent(level)
    │   ├── GetHouseCost(level)
    │   ├── GetHotelCost()
    │   └── GetUpgradeCost(level)
    │
    ├── CardRuleEngine.cs ✅
    │   ├── TriggerCards(trigger, context)
    │   ├── ApplyCardEffect(card, player)
    │   └── CheckCooldown(card)
    │
    └── DiceRng.cs ✅
        └── Roll() → (dice1, dice2)
```

### **Server Layer (Unity Netcode)**
```
Assets/Script/Server/
├── ServerBootstrap.cs ✅
│   ├── Auto-start server in headless mode
│   ├── Configure network (port 7777)
│   ├── Connection approval (max 4 players)
│   └── Performance optimization
│
└── ServerGameManager.cs ✅
    ├── NetworkBehaviour (server-only)
    ├── Game state management
    ├── Player connection/disconnection
    ├── Turn system
    ├── Dice rolling (server RNG)
    ├── Tile resolution
    ├── Property buy/rent
    ├── Card system
    ├── End game logic
    │
    ├── ServerRpc (Client → Server):
    │   ├── RequestRollDiceServerRpc()
    │   ├── RequestBuyPropertyServerRpc(tileId)
    │   ├── RequestUpgradePropertyServerRpc(tileId, level)
    │   ├── RequestUseCardServerRpc(cardId)
    │   └── RequestEndTurnServerRpc()
    │
    └── ClientRpc (Server → All Clients):
        ├── NotifyGameStartClientRpc()
        ├── NotifyTurnStartClientRpc(playerIndex, playerId)
        ├── NotifyDiceRollClientRpc(playerId, dice1, dice2, newPosition)
        ├── NotifyPropertyBoughtClientRpc(playerId, tileId)
        ├── NotifyRentPaidClientRpc(payerId, receiverId, amount)
        ├── NotifyPropertyUpgradedClientRpc(tileId, level, hasHotel)
        ├── NotifyCardUsedClientRpc(playerId, cardId)
        └── NotifyGameEndClientRpc(winnerId, reason)
```

---

## 🎲 TILE SYSTEM (36 Tiles)

### **Tile Distribution**
```
Start (1):          Tile 0 - Xuất phát (+200 salary when pass)
Property (20):      Tiles 1,3,5,6,8,9,11,12,13,15,16,18,19,21,23,25,26,28,29,31
Tax (2):            Tiles 4,33 - Thuế (-10% money)
Bonus (2):          Tiles 2,17 - Thưởng (+100 money)
Chance (4):         Tiles 7,14,22,30 - Rút thẻ event
Accident (1):       Tile 10 - Tai nạn (go to jail)
Quiz (2):           Tiles 20,27 - Câu hỏi (correct: +50, wrong: -50)
Travel (2):         Tiles 24,32 - Du lịch (teleport)
Jail (1):           Tile 34 - Tù (visit or jailed)
GoToJail (1):       Tile 35 - Đi tù (send to jail)
```

### **Property Levels**
```
Level 0: Land only (base rent)
Level 1: 1 house (rent x2)
Level 2: 2 houses (rent x3)
Level 3: 3 houses (rent x4)
Level 4: 4 houses (rent x5)
Level 5: Hotel (rent x10)
```

---

## 🃏 CARD SYSTEM

### **Skill Cards (from Firebase)**
```
1. Lăn Trốn (Agility +10)
   - Passive
   - Trigger: OnEnterOpponentHouse
   - Effect: Auto step forward 1 tile
   - Cooldown: 5 turns

2. Siêu Sale (Intelligence +10)
   - Passive
   - Trigger: OnTryPurchaseProperty
   - Effect: 30% discount
   - Cooldown: 5 turns

3. Bảo Kê (Health +10)
   - Active
   - Effect: Protect property from rent for 1 turn
   - Cooldown: 8 turns

4. Chậm Chí (Luck +10)
   - Active
   - Effect: Double start salary (next pass)
   - Cooldown: 6 turns
```

### **Card Stats Calculation**
```
Effective Stat = baseAttr + (level - 1) * attributePerLevel
Effective Cooldown = max(1, cooldownBaseTurns - cooldownReductionByStar[stars])

Example:
- Lăn Trốn level 5, stars 2
- Agility = 10 + (5-1)*1 = 14
- Cooldown = max(1, 5 - 1) = 4 turns
```

---

## 🔧 SERVER CONFIGURATION

### **Network Settings**
```
Port: 7777
Max Players: 4
Transport: Unity Transport (UDP)
Netcode: Netcode for GameObjects 2.5.1
```

### **Game Settings**
```
Max Turns: 50
Turn Time Limit: 60 seconds
Starting Money: 1000
Min Players To Start: 2
Game Start Delay: 5 seconds
Board Length: 36 tiles
Base Salary: 200 (when pass Start)
```

### **Performance**
```
Target FPS: 30 (server)
Quality: Very Low (headless)
Audio: Disabled
VSync: Disabled
```

---

## 🚀 DEPLOYMENT

### **Build Target**
```
Platform: Dedicated Server
OS: Linux x86_64
Scripting Backend: IL2CPP
API: .NET Standard 2.1
Headless: Auto-enabled (Unity 6)
```

### **Hosting Options**
```
1. Unity Multiplay (Recommended)
   - Auto-scaling
   - Global distribution
   - Integrated matchmaking
   - $0.50/CCU/month

2. AWS EC2
   - t3.medium (2 vCPU, 4GB RAM)
   - ~$30-50/month

3. Google Cloud
   - e2-medium (2 vCPU, 4GB RAM)
   - ~$25-40/month
```

---

## 📊 CLIENT-SERVER COMMUNICATION

### **Client Responsibilities**
```
✅ UI rendering
✅ Animation
✅ Sound effects
✅ Input handling
✅ Send requests to server (ServerRpc)
✅ Listen to server updates (ClientRpc)
❌ NO game logic
❌ NO state validation
❌ NO RNG
```

### **Server Responsibilities**
```
✅ Game state management
✅ Turn system
✅ Dice rolling (RNG)
✅ Tile resolution
✅ Property buy/rent/upgrade
✅ Card system
✅ Money transactions
✅ Win condition checking
✅ Validate all client requests
✅ Broadcast updates to all clients
```

---

## 🎯 IMPLEMENTATION PRIORITY

### **Phase 1: Core Multiplayer** ✅
```
✅ Server-client connection
✅ Turn system
✅ Dice rolling
✅ Player movement
```

### **Phase 2: Gameplay** (Current)
```
⏳ Property buy/rent
⏳ Money sync
⏳ House/hotel upgrade
⏳ Special tiles (Start, Tax, Bonus, Jail)
```

### **Phase 3: Advanced** (Future)
```
⏳ Card system integration
⏳ Quiz system
⏳ Event cards
⏳ Travel tiles
⏳ End game + rewards
```

---

## 🔗 INTEGRATION WITH FIREBASE

### **Pre-Game (Client)**
```
1. Login → Firebase Auth
2. Load loadout → Firestore (users/{uid}/loadouts/slot1)
3. Get skill cards → Firestore (users/{uid}/inventory)
4. Get equipment → Firestore (users/{uid}/inventory)
```

### **In-Game (Server)**
```
Server receives player loadout data from client
Server applies stats to PlayerState
Server triggers card effects during gameplay
```

### **Post-Game (Client)**
```
1. Server determines winner
2. Client calls Cloud Function: awardMatch(rank, durationSec)
3. Firebase updates:
   - users/{uid}/antCoin += reward
   - users/{uid}/xp += reward
   - users/{uid}/stats.matchesPlayed += 1
   - users/{uid}/stats.wins += 1 (if rank == 1)
4. Return to MenuScene
```

---

**SERVER READY FOR IMPLEMENTATION! 🚀**

