# 🎮 MULTIPLAYER ROADMAP - HOÀN CHỈNH

**Từ Demo Mode → Multiplayer Online với 2-4 người chơi**

---

## 🎯 **MỤC TIÊU CUỐI CÙNG**

```
User vào MenuScene
  ↓
Chọn "Tìm trận" (Matchmaking) HOẶC "Tạo phòng" (Lobby)
  ↓
Ghép với 2-4 người chơi khác
  ↓
Vào GameScene
  ↓
Chơi game multiplayer online
```

---

## ✅ **PHÂN TÍCH HIỆN TRẠNG**

### **Đã có sẵn:**

1. ✅ **LobbyUIManager.cs** - UI quản lý lobby
2. ✅ **CustomLobbyService** - Lobby logic (Unity Gaming Services)
3. ✅ **UGSAuthService** - Authentication với UGS
4. ✅ **GameSessionData.cs** - Transfer data giữa scenes
5. ✅ **GameManager.cs** - Load players từ lobby
6. ✅ **PlayerGameController.cs** - Networked player
7. ✅ **Netcode for GameObjects** - Multiplayer framework

### **Cần làm:**

1. ⏳ Fix button Roll (Demo Mode)
2. ⏳ Test gameplay mechanics
3. ⏳ Setup MenuScene UI
4. ⏳ Test Multiplayer với ParrelSync
5. ⏳ Polish & bug fixes

---

## 📋 **ROADMAP CHI TIẾT**

---

## **PHASE 1: FIX DEMO MODE (30 phút)** ⭐ BẮT ĐẦU TỪ ĐÂY

### **Mục tiêu:** Game chạy được với 1 player, tất cả mechanics hoạt động

### **Task 1.1: Fix Button Roll (10 phút)**

**Vấn đề:** Button Roll không click được

**Nguyên nhân có thể:**
- Button không được assign
- Button.interactable = false
- EventSystem missing
- Raycast blocker

**Cách fix:**
```
1. Check GameManager → Roll Button assigned
2. Check Button → Interactable = TRUE
3. Check EventSystem exists in scene
4. Check Canvas → Raycast Target settings
5. Test click
```

### **Task 1.2: Test Gameplay (15 phút)**

**Test flow:**
```
1. Play Mode
2. Click Roll Button
3. Dice roll (1-6 + 1-6)
4. Player moves to tile
5. Tile action (buy property, event, quiz, etc.)
6. Turn ends
7. Next turn starts
8. Repeat
```

**Verify:**
- ✅ Roll button works
- ✅ Dice animation
- ✅ Player movement (bounce effect)
- ✅ Tile actions
- ✅ UI updates (money, turn, etc.)
- ✅ No errors

### **Task 1.3: Fix Bugs (5 phút)**

**Common bugs:**
- Player không di chuyển
- UI không cập nhật
- Tile action không trigger
- Turn không chuyển

**Fix và test lại**

---

## **PHASE 2: KIỂM TRA CODE MULTIPLAYER (15 phút)**

### **Task 2.1: Review LobbyUIManager (5 phút)**

**File:** `Assets/Scenes/Menu/LobbyUIManager.cs`

**Features:**
- ✅ Create lobby
- ✅ Join lobby
- ✅ List lobbies
- ✅ Show players in lobby
- ✅ Start game (host only)
- ✅ Leave lobby

### **Task 2.2: Review CustomLobbyService (5 phút)**

**Check:**
- ✅ Unity Gaming Services integration
- ✅ Lobby creation/join logic
- ✅ Player sync
- ✅ Game start trigger

### **Task 2.3: Review GameSessionData (5 phút)**

**Check:**
- ✅ Transfer player data
- ✅ Transfer loadout data
- ✅ Transfer lobby info

---

## **PHASE 3: SETUP MENUSCENE (30 phút)**

### **Task 3.1: Verify MenuScene UI (10 phút)**

**Check Hierarchy:**
```
MenuScene
├── Canvas
│   ├── PanelHome
│   │   ├── Button "Tìm trận" (Matchmaking)
│   │   ├── Button "Tạo phòng" (Lobby)
│   │   └── Button "Cài đặt"
│   │
│   └── PanelCustomRoom (LobbyUIManager)
│       ├── PanelRoom (List lobbies)
│       ├── PanelCreateRoom (Create lobby popup)
│       └── PanelJoinRoom (In lobby)
│
└── Managers
    ├── MenuSceneManager
    ├── LobbyUIManager
    └── UGSAuthService
```

### **Task 3.2: Setup Buttons (10 phút)**

**Button "Tạo phòng":**
```
OnClick() → LobbyUIManager.OpenCustomRoomPanel()
```

**Button "Tìm trận":**
```
OnClick() → LobbyUIManager.QuickMatch()
```

### **Task 3.3: Test Lobby UI (10 phút)**

**Test:**
```
1. Click "Tạo phòng"
2. PanelCustomRoom opens
3. PanelRoom shows (list lobbies)
4. Click "Create Room"
5. PanelCreateRoom popup opens
6. Enter room name
7. Click "Confirm"
8. Lobby created
9. PanelJoinRoom shows
10. Wait for players
```

---

## **PHASE 4: MULTIPLAYER INTEGRATION (1 giờ)**

### **Task 4.1: GameSessionData Integration (15 phút)**

**File:** `Assets/Scenes/Game/Scripts/Data/GameSessionData.cs`

**Ensure:**
```csharp
public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance;
    
    // Player data
    public string playerName;
    public string firebaseUID;
    public bool isMale;
    
    // Loadout
    public EquipmentSet equipmentSet;
    public List<SkillCard> skillCards;
    
    // Stats (calculated from loadout)
    public int totalHealth;
    public int totalAgility;
    public int totalIntelligence;
    public int totalLuck;
    public int totalResistance;
    
    // Lobby info
    public string lobbyId;
    public List<LobbyPlayerData> lobbyPlayers;
}
```

### **Task 4.2: GameManager Load Players (20 phút)**

**File:** `Assets/Scenes/Game/Scripts/Core/GameManager.cs`

**Already implemented:**
```csharp
private IEnumerator LoadPlayersFromLobby()
{
    // Get session data
    var sessionData = GameSessionData.Instance;
    
    // Get lobby players
    var lobbyPlayers = sessionData.lobbyPlayers;
    
    // Spawn each player
    foreach (var lobbyPlayer in lobbyPlayers)
    {
        SpawnPlayerNetwork(
            lobbyPlayer.playerName,
            lobbyPlayer.playerId,
            lobbyPlayer.isMale,
            lobbyPlayer.stats...
        );
    }
}
```

**Verify:**
- ✅ Load players from GameSessionData
- ✅ Spawn networked players
- ✅ Initialize PanelGame for local player
- ✅ Add other players to PanelPlayerContainer

### **Task 4.3: Network Sync (15 phút)**

**Ensure:**
- ✅ PlayerGameController has NetworkObject
- ✅ NetworkObject settings correct (Owner permission)
- ✅ Turn sync across clients
- ✅ Dice roll sync
- ✅ Movement sync
- ✅ Money/property sync

### **Task 4.4: Start Game Flow (10 phút)**

**Flow:**
```
LobbyUIManager (MenuScene)
  ↓
Host clicks "Start Game"
  ↓
CustomLobbyService.StartGame()
  ↓
All clients receive OnGameStarting event
  ↓
Load GameScene
  ↓
GameManager.LoadPlayersFromLobby()
  ↓
Spawn all players
  ↓
Start game
```

---

## **PHASE 5: PARREL SYNC TESTING (30 phút)**

### **Task 5.1: Install ParrelSync (5 phút)**

**Steps:**
```
1. Window → Package Manager
2. Add package from git URL:
   https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync
3. Wait for install
```

### **Task 5.2: Create Clone (5 phút)**

**Steps:**
```
1. ParrelSync → Clones Manager
2. Click "Create new clone"
3. Wait for clone creation
4. Clone appears in list
```

### **Task 5.3: Test 2 Players (15 phút)**

**Steps:**
```
1. Main Editor: Play Mode
2. Clone Editor: Play Mode
3. Main: Create lobby "Test Room"
4. Clone: Join lobby "Test Room"
5. Main: Start game
6. Both: Load GameScene
7. Test gameplay:
   - Player 1 rolls dice
   - Player 1 moves
   - Turn switches to Player 2
   - Player 2 rolls dice
   - Player 2 moves
   - Repeat
```

**Verify:**
- ✅ Both players see each other
- ✅ Turn indicator shows correctly
- ✅ Dice roll syncs
- ✅ Movement syncs
- ✅ UI updates for both
- ✅ No desync

### **Task 5.4: Test 4 Players (5 phút)**

**Steps:**
```
1. Create 3 clones
2. All join same lobby
3. Host starts game
4. Test with 4 players
```

---

## **PHASE 6: MATCHMAKING (30 phút)** (OPTIONAL)

### **Task 6.1: Quick Match Logic (15 phút)**

**File:** `LobbyUIManager.cs`

**Add method:**
```csharp
public async void QuickMatch()
{
    // Sign in to UGS
    if (!UGSAuthService.IsSignedIn)
    {
        await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
    }
    
    // Try to find available lobby
    var lobbies = await CustomLobbyService.Instance.ListLobbiesAsync();
    
    Lobby availableLobby = null;
    foreach (var lobby in lobbies)
    {
        if (lobby.Players.Count < lobby.MaxPlayers)
        {
            availableLobby = lobby;
            break;
        }
    }
    
    if (availableLobby != null)
    {
        // Join existing lobby
        await CustomLobbyService.Instance.JoinLobbyByIdAsync(availableLobby.Id);
    }
    else
    {
        // Create new lobby
        await CustomLobbyService.Instance.CreateLobbyAsync("Quick Match", 4, false);
    }
}
```

### **Task 6.2: Auto Start (10 phút)**

**Logic:**
```
When lobby reaches 4 players:
  → Auto start game after 5 seconds countdown
```

### **Task 6.3: Test Matchmaking (5 phút)**

**Test:**
```
1. Player 1: Click "Tìm trận"
2. Creates lobby, waits
3. Player 2: Click "Tìm trận"
4. Joins Player 1's lobby
5. Player 3: Click "Tìm trận"
6. Joins same lobby
7. Player 4: Click "Tìm trận"
8. Joins same lobby
9. Auto start countdown
10. All load GameScene
```

---

## **PHASE 7: POLISH & BUG FIXES (30 phút)**

### **Task 7.1: UI Polish (10 phút)**

- ✅ Loading screens
- ✅ Error messages
- ✅ Lobby full message
- ✅ Connection lost handling

### **Task 7.2: Bug Fixes (15 phút)**

**Common bugs:**
- Desync issues
- Player disconnect handling
- Lobby cleanup
- Scene transition bugs

### **Task 7.3: Final Testing (5 phút)**

**Full flow test:**
```
1. Login
2. MenuScene
3. Create/Join lobby
4. Wait for players
5. Start game
6. Play full game
7. End game
8. Return to MenuScene
```

---

## ✅ **CHECKLIST TỔNG HỢP**

### **Phase 1: Demo Mode**
- [ ] Fix button Roll
- [ ] Test gameplay mechanics
- [ ] Fix bugs
- [ ] All features work with 1 player

### **Phase 2: Code Review**
- [ ] Review LobbyUIManager
- [ ] Review CustomLobbyService
- [ ] Review GameSessionData
- [ ] Understand multiplayer flow

### **Phase 3: MenuScene**
- [ ] Verify UI hierarchy
- [ ] Setup buttons
- [ ] Test lobby UI
- [ ] Create/join lobby works

### **Phase 4: Multiplayer**
- [ ] GameSessionData integration
- [ ] GameManager load players
- [ ] Network sync
- [ ] Start game flow

### **Phase 5: ParrelSync**
- [ ] Install ParrelSync
- [ ] Create clones
- [ ] Test 2 players
- [ ] Test 4 players

### **Phase 6: Matchmaking** (Optional)
- [ ] Quick match logic
- [ ] Auto start
- [ ] Test matchmaking

### **Phase 7: Polish**
- [ ] UI polish
- [ ] Bug fixes
- [ ] Final testing

---

## 🎯 **ĐỀ XUẤT CUỐI CÙNG**

### **Bắt đầu từ đâu?**

**✅ PHASE 1: Fix Demo Mode (30 phút)**

**Lý do:**
1. ✅ Nhanh nhất
2. ✅ Foundation vững
3. ✅ Tránh bug chồng chéo
4. ✅ Test gameplay logic trước

**Sau đó:**
- Phase 2-4: Multiplayer integration (2 giờ)
- Phase 5: ParrelSync testing (30 phút)
- Phase 6: Matchmaking (optional, 30 phút)
- Phase 7: Polish (30 phút)

**Tổng thời gian:** 3.5 - 4 giờ

---

## 📝 **SUMMARY**

**Câu trả lời cho câu hỏi của bạn:**

1. **Nên fix Demo Mode trước hay Multiplayer ngay?**
   → ✅ Fix Demo Mode trước (30 phút)

2. **Nếu làm Multiplayer, bắt đầu từ đâu?**
   → MenuScene → Lobby → GameScene (theo roadmap)

3. **Có nên dùng ParrelSync?**
   → ✅ CÓ! Rất quan trọng để test local multiplayer

4. **Matchmaking/Lobby đã có code chưa?**
   → ✅ ĐÃ CÓ! LobbyUIManager.cs và CustomLobbyService đã implement

**Next step:**
→ Fix button Roll trong Demo Mode (10 phút)
→ Test gameplay (15 phút)
→ Tiếp tục Phase 2-7

---

**Bạn sẵn sàng bắt đầu Phase 1 chưa?** 🚀

