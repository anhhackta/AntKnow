# 📋 QUICK REFERENCE - Hướng Dẫn Nhanh Cho Developer

**Cập nhật**: 09/10/2025  
**Mục đích**: Reference nhanh cho developer khi bắt đầu làm việc

---

## 🎯 CURRENT STATUS (Trạng thái hiện tại)

```
Overall Progress: ~45-50%

✅ LoginScene:     100% Complete
✅ MenuScene:       95% Complete
🔄 GameScene:       40% Complete
⏳ Multiplayer:     20% Complete
```

**Phase hiện tại**: Phase 1 - Offline Gameplay  
**Priority task**: House/Hotel Integration

---

## 📁 IMPORTANT FILES (Files quan trọng)

### **Documentation** (Đọc trước khi bắt đầu):
```
PROJECT_STATUS.md           ← Overview tổng quan, đọc ĐẦU TIÊN
NEXT_TASKS.md              ← Task list chi tiết với code examples
5_DAY_IMPLEMENTATION_PLAN.md   ← Multiplayer roadmap
GAMESCENE_DEVELOPMENT_PLAN.md  ← 3-phase strategy
```

### **Core Game Scripts**:
```
Assets/Scenes/Game/Scripts/
├── GameManager.cs              ← Main controller (LOCAL)
├── BoardManager.cs             ← 36 tiles management
├── PlayerGameController.cs     ← Player movement
├── PropertyManager.cs          ← Property system
└── DiceController.cs           ← Dice rolling

Assets/Script/Domain/
├── Entities/
│   ├── GameState.cs
│   ├── PlayerState.cs
│   └── PropertyState.cs
└── Services/
    ├── BoardRules.cs
    ├── PropertyEconomy.cs
    ├── TurnSystem.cs
    └── CardRuleEngine.cs
```

### **Network Scripts** (Chưa integrate):
```
Assets/Script/Multiplayer/
├── NetworkGameManager.cs       ← Network manager
└── NetworkGameController.cs    ← Domain-driven controller
```

### **UI Panels**:
```
Assets/Scenes/Game/Scripts/UI/
├── PanelBuy.cs                ← Buy/upgrade properties
├── PanelQuiz.cs               ← Quiz system
├── PanelEvent.cs              ← Event handling
├── PanelResult.cs             ← End game results
└── PanelPlayerMe.cs           ← Player info
```

---

## 🚀 GETTING STARTED (Bắt đầu làm việc)

### **1. First Day Checklist**:
```
✅ 1. Read PROJECT_STATUS.md (15 phút)
✅ 2. Read NEXT_TASKS.md Task 1 (House/Hotel) (20 phút)
✅ 3. Open Unity project
✅ 4. Test current build (GameScene) (10 phút)
✅ 5. Backup project: git commit -m "Before Task 1"
✅ 6. Start Task 1.1: PropertyManager integration
```

### **2. Daily Workflow**:
```
Morning:
1. Review task for today (NEXT_TASKS.md)
2. Read acceptance criteria
3. Check files need to edit
4. Backup: git commit

Work:
1. Implement feature
2. Test immediately (don't wait)
3. Fix bugs
4. Commit: git commit -m "Task X.Y complete"

Evening:
1. Full test of implemented features
2. Update task status
3. Plan next day
```

---

## 🎯 TASK PRIORITIES (Ưu tiên tasks)

### **MUST DO (Phase 1)**: 12-17 days
```
Priority 1: House/Hotel Integration       2-3 days  ⭐⭐⭐
Priority 2: Card System Integration       3-4 days  ⭐⭐⭐
Priority 3: Special Tiles                 4-5 days  ⭐⭐⭐
Priority 4: Quiz System Integration       1-2 days  ⭐⭐
Priority 5: End Game Logic                2-3 days  ⭐⭐⭐
```

### **SHOULD DO (Phase 2)**: 10-14 days
```
Priority 6: Network Synchronization       5-7 days  ⭐⭐⭐⭐⭐
Priority 7: Server-Authoritative          3-4 days  ⭐⭐⭐⭐
Priority 8: Multiplayer Testing           2-3 days  ⭐⭐⭐
```

### **NICE TO HAVE (Phase 3)**: 15-21 days
```
Advanced features, VFX, Polish
```

---

## 🔥 CRITICAL ISSUES (Vấn đề quan trọng)

### **Issue 1: Duplicate Controllers**
```
Problem: GameManager.cs (local) vs GameController.cs (network)
Status: Using GameManager for now
Action: Will integrate NetworkGameController in Phase 2
```

### **Issue 2: Visual Updates Not Automated**
```
Problem: Game state changes don't trigger visual updates
Example: Buy property → No visual change
Action: Implement callbacks in Task 1
```

### **Issue 3: Systems Not Connected**
```
Problem: Domain, UI, Network layers tách rời
Action: Integration is main focus of Phase 1
```

---

## 📝 CODE PATTERNS (Mẫu code thường dùng)

### **Pattern 1: Connect Manager với Visual**
```csharp
// In Manager script
[SerializeField] private BoardManager boardManager;

public bool DoSomething()
{
    // Update logic
    UpdateGameState();
    
    // Update visual
    TileVisual visual = boardManager.GetTileVisual(tileId);
    visual.UpdateVisual();
    
    return true;
}
```

### **Pattern 2: Panel với Callback**
```csharp
// Show panel with callback
panelBuy.Show(data, (result) => {
    if (result.success)
    {
        // Handle success
    }
});

// In Panel script
private System.Action<ResultData> callback;

public void Show(Data data, System.Action<ResultData> callback)
{
    this.callback = callback;
    // ... setup UI ...
    gameObject.SetActive(true);
}

private void OnButtonClicked()
{
    var result = new ResultData { success = true };
    callback?.Invoke(result);
    Hide();
}
```

### **Pattern 3: Domain Layer Integration**
```csharp
// Use domain logic
PlayerState playerState = GetPlayerState(playerId);
PropertyState propertyState = GetPropertyState(tileId);

// Check rules
if (BoardRules.CanBuy(playerState, propertyState))
{
    // Apply logic
    BoardRules.Buy(playerState, propertyState);
    
    // Sync back to Unity
    SyncPlayerStateToUnity(playerState);
    SyncPropertyStateToUnity(propertyState);
}
```

---

## 🧪 TESTING CHECKLIST (Test trước khi commit)

### **Every Feature**:
```
✅ Feature works in happy path
✅ Edge cases handled (no money, max level, etc.)
✅ Visual updates correctly
✅ No console errors
✅ No performance issues (FPS stable)
✅ Money calculations correct
✅ Player state correct
```

### **Before Commit**:
```
✅ Full game test (start → play → end)
✅ All previous features still work
✅ No new bugs introduced
✅ Code commented
✅ Debug logs clean
```

---

## 🐛 COMMON ISSUES & FIXES (Lỗi thường gặp)

### **Issue: Prefab not spawning**
```
Check:
1. Prefab assigned in Inspector?
2. Parent Transform correct?
3. localPosition vs position?
4. localScale correct?
5. SetActive(true)?
```

### **Issue: Button not working**
```
Check:
1. EventSystem in scene?
2. Button interactable = true?
3. Listener added?
4. RaycastTarget enabled on image?
5. Canvas blocking clicks?
```

### **Issue: Money not updating**
```
Check:
1. playerController reference correct?
2. Money property has setter?
3. UI text reference assigned?
4. UpdateUI() called after change?
```

### **Issue: Visual not updating**
```
Check:
1. TileVisual reference correct?
2. UpdateVisual() method called?
3. Material/Color assigned?
4. GameObject active?
5. Layer/Culling mask?
```

---

## 🎮 TEST SCENARIOS (Kịch bản test)

### **Full Game Test** (30 phút):
```
1. Start game
2. Roll dice → Move player
3. Land on property → Buy it
4. Upgrade to houses (1→4)
5. Upgrade to hotel
6. Other player lands → Pay rent
7. Land on quiz → Answer question
8. Land on special tiles (jail, tax, etc.)
9. Use active card
10. Game ends → See results
```

### **Quick Smoke Test** (5 phút):
```
1. Start game
2. Roll dice 3 times
3. Buy 1 property
4. Upgrade 1 house
5. Check money correct
6. Exit game
```

---

## 🔗 USEFUL LINKS & RESOURCES

### **Unity**:
- Netcode for GameObjects: https://docs-multiplayer.unity3d.com/
- Unity Gaming Services: https://unity.com/products/gaming-services

### **Firebase**:
- Firestore: https://firebase.google.com/docs/firestore
- Auth: https://firebase.google.com/docs/auth

### **Project Docs**:
- All .md files in project root
- Code comments in scripts
- Unity Inspector tooltips

---

## 📞 WHEN STUCK (Khi gặp khó khăn)

### **1. Check Documentation**:
```
PROJECT_STATUS.md → Overview
NEXT_TASKS.md → Detailed steps
TROUBLESHOOTING_QUICK_GUIDE.md → Common issues
```

### **2. Debug Process**:
```
1. Add Debug.Log() everywhere
2. Check Inspector values
3. Test in isolation (disable other systems)
4. Revert to last working commit
5. Ask for help with specific error
```

### **3. Code Review Checklist**:
```
✅ References assigned in Inspector?
✅ Methods called in correct order?
✅ Null checks added?
✅ Coroutines stopped properly?
✅ Events subscribed/unsubscribed?
```

---

## 🎯 SUCCESS METRICS (Đánh giá thành công)

### **Task Complete When**:
```
✅ All acceptance criteria passed
✅ All test cases passed
✅ No critical bugs
✅ Code committed
✅ Documentation updated
```

### **Phase 1 Complete When**:
```
✅ Can play full game offline (start to end)
✅ All tiles work correctly
✅ House/Hotel system working
✅ Cards apply effects
✅ Quiz system integrated
✅ Game ends with results
✅ No critical bugs or crashes
```

---

## 💡 TIPS & BEST PRACTICES

### **General**:
- Commit often (every feature or sub-task)
- Test immediately (don't accumulate untested code)
- Comment complex logic
- Use meaningful variable names
- Keep methods small (<50 lines)

### **Unity Specific**:
- Always check null before using references
- Use SerializeField instead of public
- Cache GetComponent() calls
- Unsubscribe from events in OnDestroy
- Use Coroutines for delays, not Update loops

### **Performance**:
- Avoid FindObjectOfType in Update
- Use object pooling for frequently spawned objects
- Cache Transform references
- Batch UI updates
- Profile regularly (Profiler window)

---

## 📅 RECOMMENDED SCHEDULE (Lịch trình đề xuất)

### **Week 1**: House/Hotel + Card Loading
```
Mon-Tue: Task 1 (House/Hotel)
Wed-Thu: Task 2.1 (Load cards)
Fri: Task 3 (Quiz) + Testing
```

### **Week 2**: Card Triggers + Special Tiles (Part 1)
```
Mon-Tue: Task 2.2 (Card triggers)
Wed-Fri: Task 4 (Special tiles start)
```

### **Week 3**: Special Tiles (Part 2) + End Game
```
Mon-Tue: Task 4 (Special tiles finish)
Wed-Thu: Task 5 (End game)
Fri: Full testing + Bug fixes
```

---

## 📊 PROGRESS TRACKING

### **Update This Daily**:
```
Current Task: Task 1.1 (PropertyManager integration)
Status: 🔄 In Progress
Started: 09/10/2025
Blockers: None
Estimated Completion: 10/10/2025
```

### **Weekly Review** (Every Friday):
```
Tasks completed this week: 2/3
Issues encountered: 1 (visual glitch)
Blockers: None
Next week plan: Tasks 2.2, 4.1, 4.2
```

---

## ✅ TODAY'S TASK (Nhiệm vụ hôm nay)

**Start here if you're beginning Phase 1:**

```
Task 1.1: Connect PropertyManager với TileVisual

Files to edit:
- Assets/Scenes/Game/Scripts/PropertyManager.cs
- Assets/Scenes/Game/Scripts/TileVisual.cs

Estimated time: 4 hours

Steps:
1. Read NEXT_TASKS.md Task 1.1
2. Add GetTileVisual() method to PropertyManager
3. Call SpawnHouses() after upgrade
4. Test: Buy → Upgrade → Houses spawn
5. Commit when working

Acceptance:
- Buy property → Visual updates
- Upgrade level 1-4 → Houses spawn
- Upgrade hotel → Hotel spawns
- House colors correct
```

---

**Version**: 1.0  
**Last Updated**: 09/10/2025  
**Status**: Ready to Use ✅

---

**🎯 START WITH**: Read PROJECT_STATUS.md → Read NEXT_TASKS.md Task 1 → Begin coding!
