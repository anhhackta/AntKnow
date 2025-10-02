# 🎮 GameScene Demo Setup Guide

## 🎯 Goal: Demo version với hosting mode, test movement và basic UI

---

## 📋 Step 1: Create GameScene (10 phút)

### 1.1 Create Scene
```
1. File > New Scene
2. Save as: Assets/Scenes/Game/GameScene.unity
3. Delete default objects (keep Main Camera, Directional Light)
```

### 1.2 Setup Camera
```
Main Camera:
- Position: (0, 20, -10)
- Rotation: (60, 0, 0)
- Projection: Perspective
- Field of View: 60
```

---

## 📋 Step 2: Create Waypoints (20 phút)

### 2.1 Create Parent Object
```
1. Create Empty GameObject: "BoardPath"
2. Position: (0, 0, 0)
```

### 2.2 Create 36 Waypoints
```
For i = 0 to 35:
  1. Create Empty GameObject: "Waypoint_{i:00}"
  2. Parent to BoardPath
  3. Position: Calculate circular path
  
Example positions (circular, radius = 10):
- Waypoint_00: (10, 0, 0)
- Waypoint_09: (0, 0, 10)
- Waypoint_18: (-10, 0, 0)
- Waypoint_27: (0, 0, -10)
```

### 2.3 Quick Script to Generate Waypoints
```csharp
// Attach to BoardPath, run in Editor
using UnityEngine;

public class WaypointGenerator : MonoBehaviour
{
    [ContextMenu("Generate 36 Waypoints")]
    void Generate()
    {
        // Clear existing
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        // Generate circular path
        float radius = 10f;
        int count = 36;
        
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            GameObject wp = new GameObject($"Waypoint_{i:00}");
            wp.transform.parent = transform;
            wp.transform.position = new Vector3(x, 0, z);
        }
        
        Debug.Log($"Generated {count} waypoints");
    }
}
```

---

## 📋 Step 3: Create UI Canvas (15 phút)

### 3.1 Create Canvas
```
1. Create UI > Canvas
2. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
```

### 3.2 Create PanelGameInfo
```
1. Create UI > Panel: "PanelGameInfo"
2. Parent to Canvas
3. Anchor: Top-Center
4. Size: (400, 100)
5. Position: (0, -50, 0)

Children:
- TextTurn (TextMeshProUGUI): "Turn: 1/25"
- TextCurrentPlayer (TextMeshProUGUI): "Current: Player 1"
- TextTime (TextMeshProUGUI): "Time: 00:00"
```

### 3.3 Create PanelDice
```
1. Create UI > Panel: "PanelDice"
2. Parent to Canvas
3. Anchor: Bottom-Center
4. Size: (400, 200)
5. Position: (0, 100, 0)

Children:
- ImageDice1 (Image): Dice sprite
- ImageDice2 (Image): Dice sprite
- TextResult (TextMeshProUGUI): "0 + 0 = 0"
- ButtonRoll (Button): "ROLL"
```

### 3.4 Create PanelPlayerMe
```
1. Create UI > Panel: "PanelPlayerMe"
2. Parent to Canvas
3. Anchor: Top-Left
4. Size: (200, 80)
5. Position: (100, -50, 0)

Children:
- TextName (TextMeshProUGUI): "Player 1"
- TextMoney (TextMeshProUGUI): "Money: 1000"
```

---

## 📋 Step 4: Create Player Prefab (15 phút)

### 4.1 Create Player GameObject
```
1. Create Empty GameObject: "Player"
2. Add Component: PlayerGameController
```

### 4.2 Add Model (Temporary - use Cube)
```
1. Create 3D Object > Cube
2. Parent to Player
3. Scale: (0.5, 1, 0.5)
4. Position: (0, 0.5, 0)
5. Add Material: PlayerMaterial (color: Red)
```

### 4.3 Add Animator (Optional for demo)
```
1. Add Component: Animator
2. Create Animator Controller: "PlayerAnimator"
3. Add parameter: isRunning (bool)
4. Create states: Idle, Run
5. Transitions: Idle <-> Run (condition: isRunning)
```

### 4.4 Save as Prefab
```
1. Drag Player to Assets/Prefabs/
2. Delete from scene
```

---

## 📋 Step 5: Setup Managers (10 phút)

### 5.1 Create BoardManager
```
1. Create Empty GameObject: "BoardManager"
2. Add Component: BoardManager
3. Assign WaypointsParent: BoardPath
4. Enable showDebugInfo
```

### 5.2 Create DiceController
```
1. Create Empty GameObject: "DiceController"
2. Add Component: DiceController
3. Assign dice sprites (6 sprites for faces 1-6)
4. Assign ImageDice1, ImageDice2
5. Assign TextResult
```

### 5.3 Create GameManager
```
1. Create Empty GameObject: "GameManager"
2. Add Component: GameManager
3. Assign:
   - BoardManager
   - DiceController
   - PlayerPrefab
   - ButtonRoll
   - TextTurn, TextCurrentPlayer, TextTime
4. Set maxTurns: 25
```

---

## 📋 Step 6: Create Dice Sprites (10 phút)

### 6.1 Option 1: Use Text (Quick)
```
Create 6 images with text "1", "2", "3", "4", "5", "6"
Save as: Assets/Resources/UI/Dice/dice_1.png ... dice_6.png
```

### 6.2 Option 2: Use Actual Dice Images
```
Find dice images online or create in Photoshop
Save as: Assets/Resources/UI/Dice/dice_1.png ... dice_6.png
```

### 6.3 Import Settings
```
Texture Type: Sprite (2D and UI)
Pixels Per Unit: 100
Filter Mode: Bilinear
Compression: None
```

---

## 📋 Step 7: Test Demo (5 phút)

### 7.1 Play Scene
```
1. Press Play
2. Check Console logs:
   - "[BoardManager] Initialized 36 waypoints"
   - "[GameManager] Starting game..."
   - "[GameManager] Spawned player: Player 1"
   - "[GameManager] Turn 1 - Player 1's turn"
```

### 7.2 Test Roll
```
1. Click "ROLL" button
2. Watch dice animation
3. Watch player move waypoint by waypoint
4. Check Console logs:
   - "[DiceController] Rolled: X + Y = Z"
   - "[PlayerGameController] Player 1 moving from tile 0 to Z"
   - "[PlayerGameController] Player 1 reached tile Z"
   - "[GameManager] Player 1 landed on ..."
```

### 7.3 Test Multiple Turns
```
1. Click ROLL multiple times
2. Watch turn counter increase
3. Watch player move around board
4. Check if pass Start gives money
```

---

## 📋 Step 8: Add Multiple Players (Optional - 10 phút)

### 8.1 Modify GameManager.StartGame()
```csharp
// Replace SpawnTestPlayer line with:
SpawnTestPlayer("Player 1", "p1", true, 10, 10, 10, 10, 10);
SpawnTestPlayer("Player 2", "p2", false, 5, 15, 10, 20, 5);
```

### 8.2 Create PanelPlayer1, PanelPlayer2, PanelPlayer3
```
Same as PanelPlayerMe, but:
- Position: Top-Right for Player1
- Position: Bottom-Left for Player2
- Position: Bottom-Right for Player3
```

### 8.3 Update UI in GameManager
```csharp
// Add fields:
[SerializeField] private TMPro.TextMeshProUGUI[] playerNameTexts;
[SerializeField] private TMPro.TextMeshProUGUI[] playerMoneyTexts;

// Update in Update():
for (int i = 0; i < players.Count; i++)
{
    if (i < playerNameTexts.Length)
    {
        playerNameTexts[i].text = players[i].PlayerName;
        playerMoneyTexts[i].text = $"Money: {players[i].Money}";
    }
}
```

---

## 📋 Step 9: Connect to Lobby (Later - 30 phút)

### 9.1 Load Players from Lobby
```csharp
// In GameManager.StartGame():
// Get players from LobbyManager
var lobbyPlayers = LobbyManager.Instance.GetPlayers();

foreach (var lobbyPlayer in lobbyPlayers)
{
    // Load loadout from Firestore
    var loadout = await LoadLoadoutFromFirestore(lobbyPlayer.uid);
    
    // Spawn player
    SpawnPlayer(
        lobbyPlayer.name,
        lobbyPlayer.uid,
        lobbyPlayer.gender == "male",
        loadout.health,
        loadout.agility,
        loadout.intelligence,
        loadout.luck,
        loadout.resistance
    );
}
```

### 9.2 Sync with Multiplayer (Later)
```
TODO: Add Netcode NetworkVariables
TODO: Add RPCs for dice roll, movement, etc.
```

---

## ✅ Checklist

### Demo Version (Phase 0):
- [ ] GameScene created
- [ ] 36 waypoints placed
- [ ] BoardManager setup
- [ ] PlayerGameController created
- [ ] DiceController created
- [ ] GameManager created
- [ ] UI panels created (PanelGameInfo, PanelDice, PanelPlayerMe)
- [ ] Dice sprites created
- [ ] Player prefab created
- [ ] Test: Can roll dice
- [ ] Test: Player moves correctly
- [ ] Test: Turn system works
- [ ] Test: Pass Start gives money

### Next Steps:
- [ ] Add property system (PanelProperty)
- [ ] Add quiz system (PanelQuiz)
- [ ] Add event cards (PanelEventCard)
- [ ] Add jail logic
- [ ] Add travel logic
- [ ] Connect to lobby
- [ ] Add multiplayer sync

---

## 🐛 Common Issues

### Issue 1: Waypoints not found
```
Error: "[BoardManager] WaypointsParent not assigned!"
Fix: Assign BoardPath to BoardManager.waypointsParent
```

### Issue 2: Player not moving
```
Error: "[PlayerGameController] BoardManager not found!"
Fix: Make sure BoardManager exists in scene
```

### Issue 3: Dice sprites not showing
```
Error: Dice images are blank
Fix: Assign dice sprites to DiceController.diceSprites array
```

### Issue 4: Roll button not working
```
Error: Nothing happens when click Roll
Fix: Assign ButtonRoll to GameManager.rollButton
```

---

## 🚀 Next Steps

1. **Test demo thoroughly** (30 phút)
2. **Add property system** (Phase 5)
3. **Add quiz system** (Phase 6.1)
4. **Add event cards** (Phase 6.2)
5. **Connect to lobby** (Phase 9)
6. **Add multiplayer sync** (Phase 7)

---

**Bắt đầu từ Step 1 và làm từng bước! 🎮**

