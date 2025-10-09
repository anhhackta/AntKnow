# 🔧 Task 1.1: Refactor và Tối Ưu Hóa Core Systems

## 🎯 Mục Tiêu

Integrate Domain Layer vào GameManager để có clean architecture trong khi vẫn giữ lại các features đã hoạt động.

---

## 📋 Checklist

### Phase A: Preparation (30 phút)
- [ ] Backup current GameManager.cs
- [ ] Review Domain layer classes
- [ ] Create test scene for validation
- [ ] Document current behavior

### Phase B: Integration (2-3 giờ)
- [ ] Add Domain references to GameManager
- [ ] Initialize Domain objects
- [ ] Migrate turn system logic
- [ ] Migrate property logic
- [ ] Test basic gameplay

### Phase C: Cleanup (1 giờ)
- [ ] Remove duplicate code
- [ ] Update comments
- [ ] Test all features
- [ ] Document changes

---

## 📝 Implementation Steps

### **Step 1: Add Domain References**

**File**: `Assets/Scenes/Game/Scripts/GameManager.cs`

**Add after line 19** (after PropertyManager):

```csharp
[Header("Domain Layer")]
[SerializeField] private BoardConfig boardConfig;
[SerializeField] private PropertyRuleSet propertyRuleSet;
[SerializeField] private CardLibrary cardLibrary;

// Domain objects
private GameState gameState;
private TurnSystem turnSystem;
private PropertyEconomy propertyEconomy;
private CardRuleEngine cardRuleEngine;
private DiceRng diceRng;
```

**Explanation:**
- BoardConfig: 36 tiles configuration
- PropertyRuleSet: Property rules (costs, rent percentages)
- CardLibrary: Card definitions
- Domain objects: Pure C# logic classes

---

### **Step 2: Initialize Domain Layer**

**Add new method** (after Awake):

```csharp
/// <summary>
/// Initialize Domain layer objects
/// </summary>
private void InitializeDomain()
{
    Debug.Log("[GameManager] Initializing Domain layer...");

    // Create GameState
    gameState = new GameState
    {
        BoardLength = boardManager.TotalTiles,
        CurrentTurnPlayerId = 1
    };

    // Create PropertyEconomy from PropertyRuleSet
    if (propertyRuleSet != null)
    {
        propertyEconomy = new PropertyEconomy(
            propertyRuleSet.upgradeCostPctByLevel,
            propertyRuleSet.rentPctByLevel,
            propertyRuleSet.hotelUpgradePct,
            propertyRuleSet.hotelRentPct,
            propertyRuleSet.takeoverPctByLevel,
            propertyRuleSet.takeoverAllowedOnHotel
        );
    }
    else
    {
        // Default values
        propertyEconomy = new PropertyEconomy(
            new int[] { 100, 150, 200, 250, 300 },
            new int[] { 25, 50, 75, 100, 125, 150 },
            400, 250,
            new int[] { 150, 200, 300, 400, 500, 600 },
            false
        );
    }

    // Create TurnSystem
    turnSystem = new TurnSystem(
        gameState,
        tileId => boardManager.GetTileType(tileId),
        tileId => gameState.Properties.ContainsKey(tileId) ? gameState.Properties[tileId] : null,
        tileId => boardManager.GetTileParams(tileId),
        200, // Base salary
        propertyEconomy,
        null // CardRuleEngine (will add later)
    );

    // Create DiceRng
    diceRng = new DiceRng(UnityEngine.Random.Range(0, int.MaxValue));

    Debug.Log("[GameManager] Domain layer initialized");
}
```

**Call in StartGame()** (after line 109):

```csharp
public void StartGame()
{
    Debug.Log("[GameManager] Starting game...");

    // Initialize Domain layer
    InitializeDomain(); // ADD THIS LINE

    // Initialize
    currentTurn = 1;
    // ... rest of code
}
```

---

### **Step 3: Sync Players to Domain**

**Add new method**:

```csharp
/// <summary>
/// Sync Unity players to Domain GameState
/// </summary>
private void SyncPlayersToDomain()
{
    gameState.Players.Clear();

    for (int i = 0; i < players.Count; i++)
    {
        var unityPlayer = players[i];
        var domainPlayer = new PlayerState
        {
            Id = i + 1,
            Money = unityPlayer.Money,
            NodeIndex = unityPlayer.CurrentTile,
            Health = unityPlayer.Health,
            Agility = unityPlayer.Agility,
            Intelligence = unityPlayer.Intelligence,
            Luck = unityPlayer.Luck,
            Resistance = unityPlayer.Resistance,
            JailTurns = unityPlayer.JailCounter
        };

        gameState.Players.Add(domainPlayer);
    }

    Debug.Log($"[GameManager] Synced {players.Count} players to Domain");
}
```

**Call after spawning players** (in StartGame, after SpawnTestPlayer or LoadPlayersFromLobby):

```csharp
// After spawning players
SyncPlayersToDomain(); // ADD THIS LINE

// Start first turn
StartTurn();
```

---

### **Step 4: Sync Domain to Unity**

**Add new method**:

```csharp
/// <summary>
/// Sync Domain GameState back to Unity players
/// </summary>
private void SyncDomainToPlayers()
{
    for (int i = 0; i < gameState.Players.Count && i < players.Count; i++)
    {
        var domainPlayer = gameState.Players[i];
        var unityPlayer = players[i];

        // Sync money
        if (unityPlayer.Money != domainPlayer.Money)
        {
            int diff = domainPlayer.Money - unityPlayer.Money;
            if (diff > 0)
                unityPlayer.AddMoney(diff);
            else
                unityPlayer.SubtractMoney(-diff);
        }

        // Sync position (will be handled by movement coroutine)
        // Sync jail
        unityPlayer.JailCounter = domainPlayer.JailTurns;
    }
}
```

---

### **Step 5: Refactor Roll and Move**

**Replace RollAndMove method** (around line 300):

```csharp
/// <summary>
/// Roll dice and move player (using Domain layer)
/// </summary>
private IEnumerator RollAndMove()
{
    isRolling = true;
    rollButton.interactable = false;

    // Get current player from Domain
    var domainPlayer = gameState.Players.Find(p => p.Id == currentPlayerIndex + 1);
    if (domainPlayer == null)
    {
        Debug.LogError("[GameManager] Domain player not found!");
        yield break;
    }

    // Roll dice using Domain DiceRng
    var roll = turnSystem.Roll((min, max) => UnityEngine.Random.Range(min, max));
    int diceResult = roll.die1 + roll.die2;

    Debug.Log($"[GameManager] Rolled: {roll.die1} + {roll.die2} = {diceResult}");

    // Animate dice
    if (diceController != null)
    {
        diceController.RollDice(roll.die1, roll.die2);
        yield return new WaitForSeconds(1.5f);
    }

    // Move player (Unity visual)
    PlayerGameController unityPlayer = CurrentPlayer;
    if (unityPlayer != null)
    {
        yield return StartCoroutine(unityPlayer.MoveBySteps(diceResult));
    }

    // Update Domain state
    turnSystem.MoveAndResolve(diceResult);

    // Sync Domain back to Unity
    SyncDomainToPlayers();

    // Resolve tile (will refactor this next)
    ResolveTile(domainPlayer.NodeIndex);

    isRolling = false;
}
```

---

### **Step 6: Refactor Property Logic**

**Replace ResolvePropertyTile method** (around line 350):

```csharp
/// <summary>
/// Resolve property tile (using Domain layer)
/// </summary>
private void ResolvePropertyTile(int tileId)
{
    var domainPlayer = gameState.Players.Find(p => p.Id == currentPlayerIndex + 1);
    if (domainPlayer == null) return;

    // Get or create property in Domain
    if (!gameState.Properties.ContainsKey(tileId))
    {
        gameState.Properties[tileId] = new PropertyState
        {
            TileId = tileId,
            BasePrice = boardManager.GetTilePrice(tileId),
            Owner = Owner.None,
            Level = 0,
            HasHotel = false,
            RentMultiplier = 1f
        };
    }

    var property = gameState.Properties[tileId];

    // Check ownership
    if (property.Owner == Owner.None)
    {
        // Unowned - Auto buy in demo mode
        if (demoMode && BoardRules.CanBuy(domainPlayer, property))
        {
            Debug.Log($"[GameManager] Auto buying property {tileId} for {property.BasePrice}");
            BoardRules.Buy(domainPlayer, property);
            
            // Sync to Unity
            SyncDomainToPlayers();
            
            // Update visual
            propertyManager.BuyProperty(tileId, currentPlayerIndex, property.BasePrice, CurrentPlayer);
        }
        else
        {
            // Show buy panel (will implement in Task 1.6)
            Debug.Log($"[GameManager] Property {tileId} available for purchase");
        }
    }
    else if ((int)property.Owner != domainPlayer.Id)
    {
        // Owned by other player - Pay rent
        var owner = gameState.Players.Find(p => p.Id == (int)property.Owner);
        if (owner != null)
        {
            int rent = BoardRules.CalcRent(property, owner, propertyEconomy);
            BoardRules.PayRent(domainPlayer, owner, rent);
            
            Debug.Log($"[GameManager] Player {domainPlayer.Id} paid {rent} rent to Player {owner.Id}");
            
            // Sync to Unity
            SyncDomainToPlayers();
        }
    }
    else
    {
        // Owned by current player
        Debug.Log($"[GameManager] Player {domainPlayer.Id} landed on own property");
        // Show upgrade panel (will implement in Task 1.2)
    }
}
```

---

### **Step 7: Update BoardManager**

**Add new methods to BoardManager.cs**:

```csharp
/// <summary>
/// Get tile type (for Domain layer)
/// </summary>
public TileType GetTileType(int tileId)
{
    if (tileId < 0 || tileId >= tileData.Length)
        return TileType.Property;

    return tileData[tileId].tileType;
}

/// <summary>
/// Get tile params (for Domain layer)
/// </summary>
public (int amount, int? destNode) GetTileParams(int tileId)
{
    if (tileId < 0 || tileId >= tileData.Length)
        return (0, null);

    var tile = tileData[tileId];
    return (tile.amount, tile.destinationTile >= 0 ? tile.destinationTile : (int?)null);
}
```

---

### **Step 8: Test Integration**

**Test Checklist:**

1. **Press Play**
   - [ ] No errors in console
   - [ ] Domain layer initialized
   - [ ] Players synced to Domain

2. **Roll Dice**
   - [ ] Dice rolls correctly
   - [ ] Player moves
   - [ ] Domain state updates

3. **Land on Property**
   - [ ] Auto buy works (demo mode)
   - [ ] Money deducted
   - [ ] Property ownership updated

4. **Land on Owned Property**
   - [ ] Rent calculated correctly
   - [ ] Money transferred
   - [ ] Console shows rent payment

---

## 🐛 Troubleshooting

### Error: "BoardConfig is null"
**Fix**: Assign BoardConfig asset in GameManager Inspector

### Error: "PropertyRuleSet is null"
**Fix**: Assign PropertyRuleSet asset in GameManager Inspector

### Error: "TileType not found"
**Fix**: Ensure BoardManager has GetTileType method

### Error: "Players not syncing"
**Fix**: Check SyncPlayersToDomain is called after spawning

---

## ✅ Validation

After completing this task, you should have:

- ✅ Domain layer integrated into GameManager
- ✅ Clean separation of logic and presentation
- ✅ Working offline gameplay
- ✅ No duplicate code
- ✅ Ready for next tasks (Property system, Cards, etc.)

---

## 📊 Progress

- [x] Step 1: Add Domain references
- [x] Step 2: Initialize Domain layer
- [x] Step 3: Sync players to Domain
- [x] Step 4: Sync Domain to Unity
- [x] Step 5: Refactor Roll and Move
- [x] Step 6: Refactor Property logic
- [x] Step 7: Update BoardManager
- [ ] Step 8: Test integration

---

**Next Task**: 1.2 - Hoàn thiện Property System (House/Hotel)

**Estimated Time**: 3-4 giờ  
**Status**: Ready to implement ✅

