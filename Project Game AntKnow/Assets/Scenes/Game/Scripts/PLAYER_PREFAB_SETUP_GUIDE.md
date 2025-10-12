# 🎮 PLAYER PREFAB SETUP GUIDE

**Date**: October 12, 2025  
**Architecture**: Separate Male/Female Prefabs (Simplified)

---

## ✅ SIMPLIFIED ARCHITECTURE

### **Old Way** (Complicated):
- 1 prefab với 2 models (male + female)
- Toggle models qua code
- NetworkVariables phức tạp
- Nhiều ServerRpc/ClientRpc

### **New Way** (Simple): ⭐
- **2 prefabs riêng biệt**
- Mỗi prefab có 1 model sẵn
- Không cần NetworkVariables
- Code đơn giản, trực tiếp

---

## 🎯 PREFAB STRUCTURE

### **PlayerMale.prefab**:
```
PlayerMale (Root)
├── NetworkObject (component)
├── PlayerGameController (component)
│   ├── Is Male: ✓ TRUE
│   └── Animator: Assigned to male model animator
├── MaleModel (3D model) ← Already active!
│   ├── Mesh
│   ├── Animator
│   └── Materials
└── TurnIndicator (optional, auto-created)
```

### **PlayerFemale.prefab**:
```
PlayerFemale (Root)
├── NetworkObject (component)
├── PlayerGameController (component)
│   ├── Is Male: ✗ FALSE
│   └── Animator: Assigned to female model animator
├── FemaleModel (3D model) ← Already active!
│   ├── Mesh
│   ├── Animator
│   └── Materials
└── TurnIndicator (optional, auto-created)
```

---

## 🛠️ STEP-BY-STEP CREATION

### Step 1: Create Male Prefab

1. **Hierarchy** → Right-click → **Create Empty**
2. **Rename** to `PlayerMale`
3. **Position** at (0, 0, 0)

4. **Add NetworkObject**:
   - Select PlayerMale
   - Add Component → **Network Object**
   - Settings:
     - ✓ Is Player Object: **TRUE**
     - Owner Permission: **Owner**

5. **Add PlayerGameController**:
   - Add Component → **Player Game Controller**
   - Inspector settings:
     - **Is Male**: ✓ **TRUE**
     - **Player Index**: 0 (default, sẽ set runtime)
     - **Money**: 10000
     - **Move Speed**: 5
     - **Bounce Height**: 0.5
     - **Bounce Duration**: 0.3

6. **Import Male 3D Model**:
   - Drag male character model vào làm **child** of PlayerMale
   - Rename to `MaleModel`
   - Position: (0, 0, 0)
   - Ensure model has **Animator** component

7. **Assign Animator**:
   - Select PlayerMale (root)
   - Inspector → PlayerGameController
   - Drag `MaleModel/Animator` → field **Animator**

8. **Create Prefab**:
   - Drag PlayerMale từ Hierarchy → Project window (Assets/Prefabs/Players/)
   - Prefab created! ✅
   - Delete PlayerMale from Hierarchy

---

### Step 2: Create Female Prefab

**Option A: Duplicate & Modify** (Recommended):
1. **Project window** → Duplicate `PlayerMale.prefab`
2. **Rename** to `PlayerFemale`
3. **Double-click** to open in Prefab mode
4. **Select root** (PlayerFemale)
5. **Inspector** → PlayerGameController:
   - **Is Male**: ✗ **FALSE** ← ⭐ Change this!
6. **Delete** `MaleModel` child
7. **Import Female 3D Model**:
   - Drag female character model as child
   - Rename to `FemaleModel`
   - Position: (0, 0, 0)
8. **Assign Animator**:
   - Drag `FemaleModel/Animator` → field **Animator**
9. **Save Prefab** (Ctrl + S)
10. **Exit Prefab mode**

**Option B: Create from Scratch**:
- Repeat Step 1 but with female model and `Is Male = FALSE`

---

## 🔧 GAMEMANAGER SETUP

### 1. Assign Prefabs in Inspector:

```
GameManager (GameObject in Scene)
└── GameManager (Script)
    └── Player Prefabs
        ├── Player Prefab Male: [Drag PlayerMale.prefab here]
        └── Player Prefab Female: [Drag PlayerFemale.prefab here]
```

### 2. GameManager Spawns Correct Prefab:

```csharp
// In GameManager.cs (already implemented)
GameObject prefabToUse = isMale ? playerPrefabMale : playerPrefabFemale;

// Example:
// Player with gender "male" → Spawns PlayerMale.prefab
// Player with gender "female" → Spawns PlayerFemale.prefab
```

---

## 🎨 PREFAB REQUIREMENTS

### ✅ Required Components:
1. **NetworkObject**
   - Needed for multiplayer
   - Set "Is Player Object" = TRUE

2. **PlayerGameController**
   - Main player logic script
   - Set `isMale` correctly (True for male, False for female)

3. **3D Model** (child GameObject)
   - Male model for PlayerMale.prefab
   - Female model for PlayerFemale.prefab
   - Must have **Animator** component

4. **Animator**
   - Animator Controller with "isRunning" bool parameter
   - Assigned to PlayerGameController.animator field

### ✅ Optional Components:
5. **Collider** (for physics)
6. **Rigidbody** (if needed)
7. **TurnIndicator** (auto-created by code if missing)

---

## 📋 PLAYERGA

MECONTROLLER FIELDS

### Inspector Fields to Assign:

```
PlayerGameController (Script)
├── [Header("Player Info")]
│   ├── Player Name: "Player" (default, set by Initialize())
│   ├── Player Id: "" (set by Initialize())
│   ├── Is Male: TRUE/FALSE ← ⭐ Set this per prefab!
│   └── Player Index: 0 (set by SetPlayerIndex())
│
├── [Header("Game State")]
│   ├── Current Tile: 0
│   ├── Money: 10000
│   ├── Jail Counter: 0
│   └── Skip Next Turn: FALSE
│
├── [Header("Stats from Loadout")]
│   ├── Health: 0 (set by Initialize())
│   ├── Agility: 0
│   ├── Intelligence: 0
│   ├── Luck: 0
│   └── Resistance: 0
│
├── [Header("Movement")]
│   ├── Move Speed: 5
│   ├── Bounce Height: 0.5
│   ├── Bounce Duration: 0.3
│   ├── Board Manager: [Auto-find or assign]
│   └── Board Center: (0, 0, 0)
│
├── [Header("Animation")]
│   └── Animator: [Drag model's Animator here] ← ⭐ Important!
│
└── [Header("Turn Indicator")]
    └── Turn Indicator: [Auto-created if empty]
```

---

## 🎯 TESTING PREFABS

### Test in Scene:

1. **Drag PlayerMale.prefab** into Hierarchy
2. **Select** it
3. **Verify**:
   - ✓ NetworkObject present
   - ✓ PlayerGameController present
   - ✓ Is Male = TRUE
   - ✓ MaleModel visible as child
   - ✓ Animator assigned

4. **Delete** from Hierarchy
5. **Repeat** with PlayerFemale.prefab:
   - ✓ Is Male = FALSE
   - ✓ FemaleModel visible as child

### Test in Play Mode (Demo):

1. **GameManager** → Demo Mode = TRUE
2. **Assign** both prefabs to GameManager
3. **Play**
4. **Expected**:
   - Male player spawns at tile 0
   - No errors in Console
   - Model visible and animating

---

## 🔄 RUNTIME FLOW

### 1. GameManager Spawns Player:
```csharp
// GameManager detects gender from loadout
bool isMale = sessionData.gender == "male";

// Selects correct prefab
GameObject prefabToUse = isMale ? playerPrefabMale : playerPrefabFemale;

// Spawns
GameObject playerObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
```

### 2. PlayerGameController Initializes:
```csharp
// GameManager calls Initialize()
player.Initialize(name, id, isMale, hp, agi, intel, lck, res);

// Sets player index for colors
player.SetPlayerIndex(playerIndex); // 0-3

// Sets skill cards
player.SetSkillCards(skillCardIds);
```

### 3. Player Ready to Play:
- Correct model active (male or female)
- Stats loaded from loadout
- Skill cards ready
- Color assigned (Red/Blue/Green/Yellow)
- Positioned at Start tile

---

## 🚨 COMMON ISSUES

### Issue 1: "Player prefabs not assigned"
**Fix**: Assign both PlayerMale and PlayerFemale prefabs in GameManager Inspector

### Issue 2: Wrong model spawns
**Fix**: Check prefab's `Is Male` field matches the model (Male=TRUE, Female=FALSE)

### Issue 3: Animator not working
**Fix**: Assign model's Animator to PlayerGameController.animator field

### Issue 4: NetworkObject error
**Fix**: Make sure both prefabs have NetworkObject component with "Is Player Object" = TRUE

### Issue 5: Player spawns but invisible
**Fix**: 
- Check model is child of prefab root
- Check model has MeshRenderer
- Check materials assigned

---

## 📝 CHECKLIST

### PlayerMale.prefab:
- [ ] Has NetworkObject component (Is Player Object = TRUE)
- [ ] Has PlayerGameController component
- [ ] Is Male = **TRUE**
- [ ] Has MaleModel child with 3D mesh
- [ ] MaleModel has Animator
- [ ] Animator assigned to PlayerGameController.animator field
- [ ] Saved in Assets/Prefabs/Players/ folder

### PlayerFemale.prefab:
- [ ] Has NetworkObject component (Is Player Object = TRUE)
- [ ] Has PlayerGameController component
- [ ] Is Male = **FALSE**
- [ ] Has FemaleModel child with 3D mesh
- [ ] FemaleModel has Animator
- [ ] Animator assigned to PlayerGameController.animator field
- [ ] Saved in Assets/Prefabs/Players/ folder

### GameManager:
- [ ] Player Prefab Male assigned
- [ ] Player Prefab Female assigned
- [ ] Demo mode works (spawns male player)
- [ ] No errors in Console

---

## 🎉 RESULT

**Before** (Complicated):
- 1 prefab, 2 models
- NetworkVariables everywhere
- Toggle models in code
- Confusing sync logic

**After** (Simple):
- 2 prefabs, 1 model each ✅
- Simple fields, no NetworkVariables ✅
- Model always correct ✅
- Clean, readable code ✅

---

**Now go create your player prefabs! 🚀**

**Next Steps**:
1. Create PlayerMale.prefab
2. Create PlayerFemale.prefab
3. Assign to GameManager
4. Test Play Mode
5. Success! ✅
