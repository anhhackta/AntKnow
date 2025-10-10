# 🎮 HƯỚNG DẪN SỬ DỤNG - MULTIPLAYER GAME

## 📝 **NHỮNG GÌ ĐÃ SỬA**

### **File đã sửa:**
```
Project Game AntKnow/Assets/Scenes/Game/Scripts/GameManager.cs
├── +400 lines code mới
├── Network structs: PlayerLoadoutData, TurnOrderRoll
├── Multiplayer methods: 10+ ServerRpc/ClientRpc
└── Quiz system: 6 methods mới
```

---

## 🚀 **CÁCH CHẠY GAME**

### **MODE 1: DEMO MODE (Local Test)**

**Không cần network, test nhanh 1 player**

```csharp
// In Unity Editor:
1. Mở Scene: "GameScene"
2. Select GameObject "GameManager"
3. Inspector → GameManager component
4. Set "Demo Mode" = TRUE ✅
5. Press Play ▶️

→ Game sẽ spawn 1 test player
→ Không cần lobby, không cần network
→ Test được: Dice roll, Movement, Property
```

---

### **MODE 2: MULTIPLAYER MODE (Host-Client)**

**Network multiplayer với UGS Lobby + Relay**

#### **SETUP:**
```
1. MenuScene → Create/Join Lobby
2. Lobby → Start Game
3. Load GameScene
4. GameManager tự động detect network
```

#### **FLOW:**
```
1. HOST & CLIENTS → OnNetworkSpawn()
   ├── Đọc GameSessionData (stats, cards, equipment)
   ├── CLIENT: SendLoadoutToHostServerRpc()
   └── HOST: Collect tất cả loadouts

2. HOST → SpawnAllPlayers()
   ├── Spawn 2-4 players
   └── StartTurnOrderSelection()

3. ALL PLAYERS → Roll for turn order
   ├── Host collect results
   ├── Sort players (high → low)
   └── NotifyTurnOrderFinalizedClientRpc()

4. GAME STARTS!
   ├── Player 1's turn
   ├── Roll dice (with Luck check!)
   ├── Move player
   ├── Resolve tile
   └── End turn

5. AFTER 8 ROUNDS → QUIZ ROUND
   ├── Quiz Player 1
   ├── Quiz Player 2
   ├── ...
   └── Resume game

6. GAME ENDS
   ├── Max turns (25) OR
   ├── Only 1 player left
   └── Calculate final scores
```

---

## 🎯 **FEATURES MỚI**

### **1. MULTIPLAYER PLAYER SPAWNING**

**Before:**
```csharp
// Chỉ spawn 1 player local
if (IsServer) {
    SpawnPlayerNetwork(localData, clientId);
}
```

**After:**
```csharp
// Spawn ALL players từ ALL clients
if (IsHost) {
    // Collect loadouts từ ALL clients
    yield return WaitUntil(() => playerLoadouts.Count >= connectedClients);
    
    // Spawn all
    SpawnAllPlayers();
}
else {
    // Send loadout to Host
    SendLoadoutToHostServerRpc(localLoadout);
}
```

**Benefits:**
- ✅ Host collect stats từ TẤT CẢ players
- ✅ Không hardcode stats
- ✅ Đúng với loadout đã chọn

---

### **2. TURN ORDER SELECTION**

**Flow:**
```
┌────────────────────────────────────────┐
│ HOST: StartTurnOrderSelection()       │
│   └─> NotifyTurnOrderPhaseClientRpc() │
└────────────────────────────────────────┘
              │
              ├─> Client 1: Roll = 8
              ├─> Client 2: Roll = 11
              ├─> Client 3: Roll = 6
              ├─> Client 4: Roll = 9
              │
              v
┌────────────────────────────────────────┐
│ HOST: FinalizeTurnOrder()             │
│   ├─> Sort: [11, 9, 8, 6]             │
│   ├─> Order: [P2, P4, P1, P3]         │
│   └─> NotifyTurnOrderFinalizedClientRpc() │
└────────────────────────────────────────┘
```

**Code:**
```csharp
// AUTO roll for turn order
private IEnumerator RollForTurnOrder()
{
    yield return new WaitForSeconds(0.5f);
    int diceResult = Random.Range(1, 7) + Random.Range(1, 7);
    SendTurnOrderRollServerRpc(diceResult);
}

// Host sort players
turnOrderRolls.Sort((a, b) => b.diceResult.CompareTo(a.diceResult));
```

---

### **3. LUCK-BASED DICE ROLL**

**Formula:**
```
Luck = 50 pts
→ Chance = 50 / 10 = 5%
→ Random.value < 0.05?
   → TRUE: Roll 1 die x2 (DOUBLE!) ⭐
   → FALSE: Roll 2 dice (normal)
```

**Code:**
```csharp
int luckStat = player.Luck;
int luckPct = luckStat / 10; // 10 pts = 1%
float doubleChance = luckPct / 100f;

if (Random.value < doubleChance && luckStat > 0)
{
    // LUCK ACTIVATED!
    int die = Random.Range(1, 7);
    diceResult = die * 2;
    wasLuckyDouble = true;
}
```

**Examples:**
```
Luck = 0  → 0% chance
Luck = 10 → 1% chance
Luck = 50 → 5% chance
Luck = 100 → 10% chance
```

---

### **4. TURN & QUIZ SYSTEM**

**Tracking:**
```csharp
// 1 Round = ALL players finished their turn
if (currentPlayerIndex >= players.Count)
{
    currentPlayerIndex = 0;
    roundCounter++;
    
    // Every 8 rounds = 1 quiz
    if (roundCounter % 8 == 0)
    {
        StartQuizRound();
    }
}
```

**Quiz Flow:**
```
Round 8: QUIZ ROUND
├─> Host: QuizAllPlayersCoroutine()
├─> For each player:
│   ├─> NotifyShowQuizClientRpc(playerIndex)
│   ├─> Client shows quiz panel
│   ├─> Client sends answer → SendQuizAnswerServerRpc()
│   └─> Host applies penalty if wrong
└─> Resume game
```

**Penalties:**
```csharp
Random penalty:
├─> 0: Lose money (100-300)
├─> 1: Downgrade property
└─> 2: Skip next turn
```

---

## 🔧 **DEBUGGING**

### **Console Logs:**

**Host:**
```
[Host] Received loadout from Client 12345: Player1 (HP:50 AGI:40 INT:30 LUCK:50 RES:20)
[Host] All 4 loadouts received! Starting game...
[Host] Spawning 4 players...
[Host] === STARTING TURN ORDER SELECTION ===
[Host] Client 12345 rolled 8 for turn order
[Host] Position 1: Player2 (rolled 11)
[Host] Turn order finalized! First player: Player2
[Host] ⭐ LUCK ACTIVATED! Player2 rolled 6 x2 = 12
[Host] ========== ROUND 8 COMPLETED ==========
[Host] === STARTING QUIZ ROUND ===
```

**Client:**
```
[Client] Sending loadout to Host: Player1
[Client] Rolling for turn order...
[Client] Rolled 8 for turn order
[Client] ✅ Turn order finalized! Game starting...
[Client] ⭐⭐⭐ LUCK! Player2 rolled 6 x2 = 12 ⭐⭐⭐
[Client] 📝 My turn to answer quiz!
```

---

## ⚠️ **LƯU Ý**

### **1. GameSessionData PHẢI CÓ:**
```csharp
GameSessionData.Instance.totalHealth = 50;
GameSessionData.Instance.totalAgility = 40;
GameSessionData.Instance.totalIntelligence = 30;
GameSessionData.Instance.totalLuck = 50;
GameSessionData.Instance.totalResistance = 20;
```

**Nếu không có:**
→ Stats = 0
→ Luck không hoạt động
→ Player yếu

### **2. NetworkManager PHẢI READY:**
```csharp
if (NetworkManager.Singleton == null)
{
    Debug.LogError("[GameManager] NetworkManager not found!");
}

if (!NetworkManager.Singleton.IsListening)
{
    Debug.LogWarning("[GameManager] Network not listening!");
}
```

### **3. Player Prefab PHẢI CÓ NetworkObject:**
```
PlayerPrefab
├── NetworkObject component ✅
├── PlayerGameController ✅
└── NetworkTransform (optional)
```

---

## 🎯 **NEXT STEPS**

### **Còn thiếu:**
1. **Skill Card Integration** (4h)
   - Load cards từ `GameSessionData.skillCards`
   - Trigger passive skills
   - Show active skill panel
   
2. **Complete Tile Resolution** (2h)
   - Event Tile: Draw event card
   - Quiz Tile: Show quiz
   - Travel Tile: Player chọn đích đến
   - Jail Tile: Tù 2 turns

### **Bạn muốn tôi làm tiếp?**
→ YES: Tôi sẽ implement 2 tasks còn lại (6h)
→ NO: Test thử những gì đã có, báo lỗi nếu có

---

## 📞 **HỖ TRỢ**

**Nếu gặp lỗi:**
1. Check Console logs (`[Host]`, `[Client]`)
2. Check `GameSessionData.Instance != null`
3. Check `NetworkManager.Singleton.IsListening`
4. Set `Demo Mode = TRUE` để test local

**Nếu player không spawn:**
1. Check `playerPrefab` assigned
2. Check `NetworkObject` component
3. Check loadouts received (Console log)

**Nếu Luck không hoạt động:**
1. Check `player.Luck > 0`
2. Check Console log: "LUCK ACTIVATED!"
3. Try nhiều lần (chance thấp!)

---

**ĐÃ SẴN SÀNG! 🚀**

