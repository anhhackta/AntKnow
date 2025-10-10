# 🚀 KẾ HOẠCH THỰC THI 30 GIỜ - HOST-CLIENT MODEL

## 📊 HIỆN TRẠNG CODE

### ✅ **ĐÃ CÓ (70%)**
```
GameManager.cs:
├── ✅ NetworkBehaviour (line 14)
├── ✅ LoadPlayersFromLobby() (line 137) - Load từ GameSessionData
├── ✅ GameSessionData.Instance (line 142) - Có stats, cards, equipment
├── ⚠️ NHƯNG chỉ spawn LOCAL player (line 175-185)
├── ❌ Không collect loadouts từ ALL players
└── ❌ Không có turn order selection

PlayerGameController.cs:
├── ✅ Initialize với 5 stats (line 90)
├── ✅ MoveBySteps với bounce effect (line 111)
├── ✅ Stats properties (line 53-57)
└── ✅ TurnIndicator (line 41)

DiceController.cs:
├── ✅ RollDice animation (line 51)
├── ⚠️ NHƯNG không check Luck (line 115)
└── ❌ Roll trực tiếp Random.Range()

GameSessionData.cs:
├── ✅ totalHealth, Agility, Intelligence, Luck, Resistance
├── ✅ skillCards list
├── ✅ equipmentSet
└── ✅ CalculateTotalStats() (line 141)
```

### ❌ **THIẾU (30%)**

1. **Multiplayer Player Spawning**
   - Chỉ spawn 1 player local
   - Không sync tất cả players

2. **Loadout Sync**
   - Không gửi loadout đến Host
   - Host không collect & validate

3. **Turn Order**
   - Không có phase chọn người đi trước
   - currentPlayerIndex = 0 (hardcoded)

4. **Luck-Based Dice**
   - Roll trực tiếp
   - Không check Luck

5. **Skill Card Integration**
   - Code có sẵn nhưng không gọi
   - Không trigger passive/active

6. **Turn System**
   - Không track "vòng tròn"
   - Không có quiz mỗi 8 turns

7. **Tile Resolution**
   - Thiếu logic cho Event, Quiz, Travel
   - Không server-authoritative

---

## 🎯 IMPLEMENTATION ROADMAP

### **PHASE 1: CORE MULTIPLAYER (8H)**

#### **TASK 1.1: Multiplayer Player Spawning (2h)**

**File cần sửa:** `GameManager.cs`

**Thay đổi:**
```csharp
// ❌ HIỆN TẠI (line 137-188)
private IEnumerator LoadPlayersFromLobby()
{
    // Chỉ spawn LOCAL player
    if (IsServer) {
        SpawnPlayerNetwork(localData...);
    } else {
        SpawnTestPlayer(localData...);
    }
}

// ✅ SỬA THÀNH
private IEnumerator LoadPlayersFromLobby()
{
    if (IsHost) {
        // HOST: Collect loadouts từ ALL clients
        yield return StartCoroutine(CollectAllPlayerLoadouts());
        
        // Spawn tất cả players
        SpawnAllPlayers();
    } else {
        // CLIENT: Send loadout to Host
        SendLoadoutToHostServerRpc();
    }
}
```

**Code mới cần thêm:**
```csharp
// Dictionary lưu loadouts: clientId → PlayerLoadoutData
private Dictionary<ulong, PlayerLoadoutData> playerLoadouts = new Dictionary<ulong, PlayerLoadoutData>();

[System.Serializable]
public struct PlayerLoadoutData : INetworkSerializable
{
    public string playerName;
    public string playerId;
    public bool isMale;
    public int health;
    public int agility;
    public int intelligence;
    public int luck;
    public int resistance;
    // Skill cards sẽ implement sau
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref isMale);
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref agility);
        serializer.SerializeValue(ref intelligence);
        serializer.SerializeValue(ref luck);
        serializer.SerializeValue(ref resistance);
    }
}

// CLIENT → HOST: Gửi loadout
[ServerRpc(RequireOwnership = false)]
private void SendLoadoutToHostServerRpc(PlayerLoadoutData loadout, ServerRpcParams rpcParams = default)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    playerLoadouts[clientId] = loadout;
    
    Debug.Log($"[Host] Received loadout from Client {clientId}: {loadout.playerName}");
    
    // Check nếu đủ players → Start turn order selection
    if (playerLoadouts.Count >= NetworkManager.Singleton.ConnectedClients.Count)
    {
        StartTurnOrderSelection();
    }
}

// HOST: Spawn all players
private void SpawnAllPlayers()
{
    int playerIndex = 0;
    foreach (var kvp in playerLoadouts)
    {
        ulong clientId = kvp.Key;
        PlayerLoadoutData loadout = kvp.Value;
        
        SpawnPlayerNetwork(
            loadout.playerName,
            loadout.playerId,
            loadout.isMale,
            loadout.health,
            loadout.agility,
            loadout.intelligence,
            loadout.luck,
            loadout.resistance,
            clientId
        );
        
        playerIndex++;
    }
}
```

---

#### **TASK 1.2: Turn Order Selection (1.5h)**

**Logic:**
```
1. Tất cả players roll dice (cùng lúc)
2. Host collect results
3. Sort players theo dice result (high → low)
4. Update player order
5. Notify clients
```

**Code:**
```csharp
// Struct lưu roll result
[System.Serializable]
public struct TurnOrderRoll : INetworkSerializable
{
    public ulong clientId;
    public int diceResult;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref diceResult);
    }
}

private List<TurnOrderRoll> turnOrderRolls = new List<TurnOrderRoll>();
private bool isTurnOrderPhase = false;

// HOST: Bắt đầu phase chọn người đi trước
private void StartTurnOrderSelection()
{
    isTurnOrderPhase = true;
    turnOrderRolls.Clear();
    
    Debug.Log("[Host] Starting turn order selection...");
    
    // Notify all clients to roll dice
    NotifyTurnOrderPhaseClientRpc();
}

// ALL CLIENTS: Roll dice cho turn order
[ClientRpc]
private void NotifyTurnOrderPhaseClientRpc()
{
    // Show UI: "Rolling for turn order..."
    Debug.Log("[Client] Rolling for turn order...");
    
    // Auto roll (hoặc player click button)
    StartCoroutine(RollForTurnOrder());
}

private IEnumerator RollForTurnOrder()
{
    // Animation delay
    yield return new WaitForSeconds(0.5f);
    
    // Roll dice
    int diceResult = Random.Range(1, 7) + Random.Range(1, 7);
    
    // Send to Host
    SendTurnOrderRollServerRpc(diceResult);
}

// CLIENT → HOST: Gửi kết quả roll
[ServerRpc(RequireOwnership = false)]
private void SendTurnOrderRollServerRpc(int diceResult, ServerRpcParams rpcParams = default)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    
    turnOrderRolls.Add(new TurnOrderRoll {
        clientId = clientId,
        diceResult = diceResult
    });
    
    Debug.Log($"[Host] Client {clientId} rolled {diceResult} for turn order");
    
    // Check nếu tất cả đã roll
    if (turnOrderRolls.Count >= NetworkManager.Singleton.ConnectedClients.Count)
    {
        FinalizeTurnOrder();
    }
}

// HOST: Xác định turn order
private void FinalizeTurnOrder()
{
    // Sort by dice result (descending)
    turnOrderRolls.Sort((a, b) => b.diceResult.CompareTo(a.diceResult));
    
    // Update player order
    List<ulong> orderedClientIds = new List<ulong>();
    foreach (var roll in turnOrderRolls)
    {
        orderedClientIds.Add(roll.clientId);
    }
    
    // Reorder players list
    List<PlayerGameController> orderedPlayers = new List<PlayerGameController>();
    foreach (ulong clientId in orderedClientIds)
    {
        var player = players.Find(p => p.GetComponent<NetworkObject>().OwnerClientId == clientId);
        if (player != null)
        {
            orderedPlayers.Add(player);
        }
    }
    players = orderedPlayers;
    
    Debug.Log($"[Host] Turn order finalized. First player: {players[0].PlayerName}");
    
    // Notify clients
    NotifyTurnOrderFinalizedClientRpc();
    
    // Start game
    isTurnOrderPhase = false;
    currentPlayerIndex = 0;
    StartTurn();
}

[ClientRpc]
private void NotifyTurnOrderFinalizedClientRpc()
{
    Debug.Log("[Client] Turn order finalized!");
    // Update UI: Show player order in PanelPlayer
}
```

---

#### **TASK 1.3: Luck-Based Dice Roll (1h)**

**File cần sửa:** `GameManager.cs` + `DiceController.cs`

**Logic:**
```
1. Host check Luck stat của current player
2. Calculate chance = Luck / 10 → %
3. Random.value < chance? Roll 1 dice x2 : Roll 2 dice
```

**Code:**
```csharp
// In GameManager.cs

// Sửa OnRollButtonClicked (line 317)
private void OnRollButtonClicked()
{
    if (players.Count == 0) return;
    
    PlayerGameController currentPlayer = players[currentPlayerIndex];
    
    // HOST: Roll dice với Luck check
    if (IsHost)
    {
        RollDiceWithLuck(currentPlayer);
    }
}

private void RollDiceWithLuck(PlayerGameController player)
{
    int diceResult;
    bool isDouble = false;
    bool wasLuckyDouble = false;
    
    // Check Luck
    int luckStat = player.Luck;
    int luckPct = luckStat / 10; // 10 pts = 1%
    float doubleChance = luckPct / 100f;
    
    if (Random.value < doubleChance)
    {
        // Trúng Luck! Roll 1 dice x2
        int die = Random.Range(1, 7);
        diceResult = die * 2;
        isDouble = true;
        wasLuckyDouble = true;
        
        Debug.Log($"[Host] LUCK ACTIVATED! Player {player.PlayerName} rolled {die} x2 = {diceResult}");
    }
    else
    {
        // Bình thường: Roll 2 dice
        int die1 = Random.Range(1, 7);
        int die2 = Random.Range(1, 7);
        diceResult = die1 + die2;
        isDouble = (die1 == die2);
        
        Debug.Log($"[Host] Player {player.PlayerName} rolled {die1} + {die2} = {diceResult}");
    }
    
    // Notify all clients
    NotifyDiceRolledClientRpc(currentPlayerIndex, diceResult, isDouble, wasLuckyDouble);
    
    // Move player
    StartCoroutine(RollAndMove(diceResult));
}

[ClientRpc]
private void NotifyDiceRolledClientRpc(int playerIndex, int result, bool isDouble, bool wasLuckyDouble)
{
    // Update UI
    if (wasLuckyDouble)
    {
        Debug.Log($"[Client] ⭐ LUCK! Player rolled {result}");
        // Show special effect
    }
    else
    {
        Debug.Log($"[Client] Player rolled {result}" + (isDouble ? " (DOUBLE!)" : ""));
    }
}
```

---

### **PHASE 2: GAMEPLAY LOGIC (8H)**

#### **TASK 2.1: Skill Card Integration (4h)**

**Files:**
- `GameManager.cs`
- `SkillCardManager.cs` (NEW)
- `PanelCard.cs` (modify)

**Logic:**
```
Turn Flow:
1. Roll Dice → Move → Land on Tile
2. Resolve Tile (property, event, quiz, etc.)
3. Trigger Passive Skills (auto)
   ├── Check triggers: onEnterOpponentHouse, onTryPurchaseProperty, etc.
   └── Apply effects
4. Show Active Skill Panel (if player has active skills ready)
   ├── Player chooses skill or skip
   └── Apply skill effect
5. End Turn
```

**Code: SkillCardManager.cs (NEW)**
```csharp
using UnityEngine;
using System.Collections.Generic;
using AntKnow.Game;

public class SkillCardManager : MonoBehaviour
{
    // Load từ Server Domain layer
    private SkillTriggerEngine triggerEngine;
    private Dictionary<int, List<SkillCardInstance>> playerSkills; // playerId → skills
    
    public void Initialize()
    {
        triggerEngine = new SkillTriggerEngine();
        playerSkills = new Dictionary<int, List<SkillCardInstance>>();
    }
    
    // Load skills từ GameSessionData
    public void LoadPlayerSkills(int playerId, List<SkillCardData> skillCards)
    {
        List<SkillCardInstance> instances = new List<SkillCardInstance>();
        
        foreach (var cardData in skillCards)
        {
            instances.Add(new SkillCardInstance {
                instanceId = System.Guid.NewGuid().ToString(),
                itemId = cardData.itemId,
                level = cardData.level,
                stars = cardData.stars,
                effectiveCooldown = CalculateCooldown(cardData),
                currentCooldown = 0
            });
        }
        
        playerSkills[playerId] = instances;
    }
    
    // Trigger passive skills
    public void TriggerPassiveSkills(string triggerId, PlayerGameController player, GameState gameState)
    {
        if (!playerSkills.ContainsKey(player.GetInstanceID())) return;
        
        var skills = playerSkills[player.GetInstanceID()];
        
        // Filter passive skills with matching trigger
        var triggered = skills.FindAll(s => {
            var cardData = BasicSkillCards.GetCardByItemId(s.itemId);
            return cardData != null && 
                   cardData.skill.mode == "passive" &&
                   cardData.skill.triggerId == triggerId &&
                   s.currentCooldown == 0;
        });
        
        // Execute each skill
        foreach (var skill in triggered)
        {
            var cardData = BasicSkillCards.GetCardByItemId(skill.itemId);
            var context = new SkillExecutionContext();
            
            var result = triggerEngine.ExecuteSkill(skill, cardData, player.GetPlayerState(), gameState, context);
            
            if (result.success)
            {
                Debug.Log($"[SkillCardManager] Triggered {cardData.name}: {result.message}");
                
                // Set cooldown
                skill.currentCooldown = skill.effectiveCooldown;
            }
        }
    }
    
    // Get available active skills
    public List<SkillCardInstance> GetAvailableActiveSkills(int playerId)
    {
        if (!playerSkills.ContainsKey(playerId)) return new List<SkillCardInstance>();
        
        var skills = playerSkills[playerId];
        
        return skills.FindAll(s => {
            var cardData = BasicSkillCards.GetCardByItemId(s.itemId);
            return cardData != null && 
                   cardData.skill.mode == "active" &&
                   s.currentCooldown == 0;
        });
    }
    
    // Update cooldowns
    public void UpdateCooldowns()
    {
        foreach (var kvp in playerSkills)
        {
            foreach (var skill in kvp.Value)
            {
                if (skill.currentCooldown > 0)
                {
                    skill.currentCooldown--;
                }
            }
        }
    }
    
    private int CalculateCooldown(SkillCardData cardData)
    {
        // Cooldown giảm theo stars
        int[] cooldownReduction = { 0, 1, 2, 3, 4, 5 }; // stars 0-5
        return Mathf.Max(1, cardData.skill.cooldownBaseTurns - cooldownReduction[cardData.stars]);
    }
}
```

**Integration vào GameManager:**
```csharp
// In GameManager.cs

[Header("Systems")]
private SkillCardManager skillCardManager;

private void Awake()
{
    skillCardManager = gameObject.AddComponent<SkillCardManager>();
    skillCardManager.Initialize();
}

// Load skills khi spawn player
private void SpawnPlayerNetwork(...)
{
    // ... existing code ...
    
    // Load skills từ GameSessionData
    var sessionData = GameSessionData.Instance;
    if (sessionData != null && sessionData.skillCards != null)
    {
        skillCardManager.LoadPlayerSkills(player.GetInstanceID(), sessionData.skillCards);
    }
}

// Sửa ResolveTile để trigger passive skills
private void ResolveTile(PlayerGameController player)
{
    // ... existing tile resolution ...
    
    // AFTER resolve tile, trigger passive skills
    TriggerPassiveSkillsForTile(player, tileType);
}

private void TriggerPassiveSkillsForTile(PlayerGameController player, TileType tileType)
{
    // Xác định trigger ID
    string triggerId = "";
    
    switch (tileType)
    {
        case TileType.Property:
            var property = GetPropertyAtTile(player.CurrentTile);
            if (property != null && property.Owner != player.GetInstanceID())
            {
                triggerId = SkillTriggers.OnEnterOpponentHouse;
            }
            break;
        // ... other cases ...
    }
    
    if (!string.IsNullOrEmpty(triggerId))
    {
        skillCardManager.TriggerPassiveSkills(triggerId, player, GetGameState());
    }
}

// Show active skill panel
private void ShowActiveSkillPanel(PlayerGameController player)
{
    var availableSkills = skillCardManager.GetAvailableActiveSkills(player.GetInstanceID());
    
    if (availableSkills.Count > 0 && panelCard != null)
    {
        // Convert to CardData for UI
        List<CardData> cardDataList = new List<CardData>();
        foreach (var skill in availableSkills)
        {
            var cardData = BasicSkillCards.GetCardByItemId(skill.itemId);
            cardDataList.Add(new CardData {
                cardId = skill.instanceId.GetHashCode(),
                cardName = cardData.name,
                cardDescription = cardData.description,
                cooldownRemaining = skill.currentCooldown
            });
        }
        
        panelCard.Show(cardDataList, (cardId) => {
            // Player chose a skill
            OnSkillChosen(player, cardId);
        });
    }
    else
    {
        // No skills available, end turn
        EndTurn();
    }
}
```

---

#### **TASK 2.2: Turn & Quiz System (2h)**

**Tracking logic:**
```
- 1 Turn = Tất cả players đã đi 1 lượt
- Round counter++
- Mỗi 8 rounds → Quiz tất cả players
```

**Code:**
```csharp
// In GameManager.cs

private int roundCounter = 0; // Vòng tròn (all players = 1 round)
private const int QUIZ_INTERVAL = 8; // Quiz mỗi 8 rounds

// Sửa EndTurn
private void EndTurn()
{
    if (!isGameActive) return;
    
    // Move to next player
    currentPlayerIndex++;
    
    // Check if completed a round
    if (currentPlayerIndex >= players.Count)
    {
        currentPlayerIndex = 0;
        roundCounter++;
        currentTurn++;
        
        Debug.Log($"[GameManager] Round {roundCounter} completed. Turn {currentTurn}/{maxTurns}");
        
        // Update cooldowns
        skillCardManager.UpdateCooldowns();
        
        // Check for quiz round
        if (roundCounter % QUIZ_INTERVAL == 0)
        {
            StartQuizRound();
            return; // Don't start next turn yet
        }
        
        // Check end game
        if (currentTurn > maxTurns || CheckWinCondition())
        {
            EndGame();
            return;
        }
    }
    
    // Start next turn
    StartTurn();
}

// Quiz round for all players
private void StartQuizRound()
{
    Debug.Log($"[GameManager] === QUIZ ROUND {roundCounter / QUIZ_INTERVAL} ===");
    
    isGameActive = false; // Pause game
    
    // Show quiz to all players
    StartCoroutine(QuizAllPlayers());
}

private IEnumerator QuizAllPlayers()
{
    for (int i = 0; i < players.Count; i++)
    {
        var player = players[i];
        
        // Show quiz panel
        bool answeredCorrectly = false;
        bool quizCompleted = false;
        
        if (panelQuiz != null)
        {
            panelQuiz.Show((isCorrect) => {
                answeredCorrectly = isCorrect;
                quizCompleted = true;
                
                if (!isCorrect)
                {
                    ApplyQuizPenalty(player);
                }
            });
        }
        
        // Wait for answer
        yield return new WaitUntil(() => quizCompleted);
        
        // Delay between players
        yield return new WaitForSeconds(1f);
    }
    
    // Resume game
    isGameActive = true;
    StartTurn();
}

private void ApplyQuizPenalty(PlayerGameController player)
{
    // Random penalty
    int penaltyType = Random.Range(0, 3);
    
    switch (penaltyType)
    {
        case 0: // Lose money
            int moneyLoss = Random.Range(100, 300);
            player.SubtractMoney(moneyLoss);
            Debug.Log($"[Quiz] Player {player.PlayerName} lost {moneyLoss} money");
            break;
            
        case 1: // Downgrade property
            // TODO: Implement
            Debug.Log($"[Quiz] Player {player.PlayerName} downgraded a property");
            break;
            
        case 2: // Skip next turn
            player.SetSkipNextTurn(true);
            Debug.Log($"[Quiz] Player {player.PlayerName} will skip next turn");
            break;
    }
}
```

---

#### **TASK 2.3: Complete Tile Resolution (2h)**

**Sửa ResolveTile để server-authoritative:**

```csharp
private void ResolveTile(PlayerGameController player)
{
    if (!IsHost) return; // Only Host can resolve tiles
    
    int tileIndex = player.CurrentTile;
    TileType tileType = boardManager.GetTileType(tileIndex);
    string tileName = boardManager.GetTileName(tileIndex);
    int basePrice = boardManager.GetTilePrice(tileIndex);
    
    Debug.Log($"[Host] Resolving tile {tileIndex}: {tileName} ({tileType})");
    
    switch (tileType)
    {
        case TileType.Start:
            // Ô bắt đầu - no action (lương đã cộng khi qua)
            break;
            
        case TileType.Property:
            ResolvePropertyTile(player, tileIndex, tileName, basePrice);
            break;
            
        case TileType.Event:
            ResolveEventTile(player);
            break;
            
        case TileType.Quiz:
            ResolveQuizTile(player);
            break;
            
        case TileType.Jail:
            player.SetJailCounter(2); // 2 turns
            NotifyJailClientRpc(player.PlayerName);
            break;
            
        case TileType.Travel:
            ResolveTravelTile(player);
            break;
    }
    
    // Trigger passive skills
    TriggerPassiveSkillsForTile(player, tileType);
    
    // Show active skill panel (if any)
    ShowActiveSkillPanel(player);
}

// Event tile - server random và apply
private void ResolveEventTile(PlayerGameController player)
{
    // Use EventCardHandler từ Server Domain layer
    var eventHandler = new EventCardHandler();
    var eventCard = eventHandler.DrawEventCard();
    
    var result = eventHandler.ExecuteEventCard(eventCard, player.GetPlayerState(), GetGameState());
    
    // Apply changes
    if (result.moneyChange != 0)
    {
        if (result.moneyChange > 0)
            player.AddMoney(result.moneyChange);
        else
            player.SubtractMoney(-result.moneyChange);
    }
    
    // Notify clients
    NotifyEventCardClientRpc(player.PlayerName, eventCard.name, result.message);
}

[ClientRpc]
private void NotifyEventCardClientRpc(string playerName, string cardName, string message)
{
    if (panelEvent != null)
    {
        panelEvent.Show($"{playerName}: {cardName}\n{message}", null);
    }
}
```

---

### **PHASE 3: POLISH & TEST (6H)**

...

---

## 📝 SUMMARY

**Timeline:**
- **Day 1 (8h):** Phase 1 - Multiplayer spawning, Turn order, Luck dice
- **Day 2 (8h):** Phase 2 - Skill cards, Turn system, Tile resolution
- **Day 3 (6h):** Phase 3 - UI sync, Stats effects, End game
- **Buffer (8h):** Testing, bug fixes, documentation

**TOTAL: 30 Hours** ✅

**Confidence: 90%** - Có đủ thời gian và code base tốt

---

**BẠN ĐỒNG Ý? TÔI BẮT ĐẦU IMPLEMENT NGAY!** 🚀

