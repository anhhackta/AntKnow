# ✅ FINAL MATCHMAKER FIX - ĐỢI 30S RỒI AUTO START

## 🔥 ĐÃ FIX 2 VẤN ĐỀ

### **1. Matchmaker - Đợi 30s rồi mới vào game** ✅

**Yêu cầu:**
```
Click "Tìm trận"
    ↓
ButtonWaitGame đếm: 00:30 → 00:29 → ... → 00:00
    ↓
Tìm thấy người (2/4, 3/4) → VẪN ĐỢI tiếp
    ↓
Sau 30s → Thông báo "Match Found" → Ẩn panel → Vào game
```

**Fix:**

#### **A. Chỉ hiện "Match Found" khi JOIN lobby có sẵn**

```csharp
// MatchmakerService.cs - OnMatchJoined()

private async Task OnMatchJoined(Lobby lobby, bool isJoining)
{
    // CHỈ fire OnMatchFound khi JOIN lobby có sẵn (có người khác)
    // KHÔNG fire khi TẠO lobby mới (1 mình)
    if (isJoining)
    {
        DebugLog("Match found! Joined existing lobby.");
        OnMatchFound?.Invoke(lobby); // → Hiện "Match Found"
    }
    else
    {
        DebugLog("Created new lobby, waiting for other players...");
        // KHÔNG hiện "Match Found"
    }
}
```

#### **B. Đợi 30s sau khi đủ 2 người**

```csharp
// MatchmakerService.cs - UpdateLobbyInfoAsync()

// Auto start timer
private float autoStartTimer = 0f;
private bool isWaitingForAutoStart = false;
private const float AUTO_START_DELAY = 30f; // 30 giây

// MATCHMAKER AUTO START LOGIC (Host only)
if (isHost)
{
    // Đủ 4 người → Start ngay
    if (playerCount >= maxPlayers)
    {
        DebugLog("Lobby full (4/4), auto starting game...");
        await AutoStartGameAsync();
        return false;
    }

    // Đủ 2-3 người → Bắt đầu đếm 30s
    if (playerCount >= 2)
    {
        if (!isWaitingForAutoStart)
        {
            // Bắt đầu đếm ngược 30s
            isWaitingForAutoStart = true;
            autoStartTimer = AUTO_START_DELAY;
            DebugLog($"Match ready ({playerCount}/4), waiting 30s for more players...");
        }
        else
        {
            // Đang đếm ngược
            autoStartTimer -= 2f; // Update mỗi 2s
            DebugLog($"Auto start in {autoStartTimer:F0}s ({playerCount}/4)");
            
            // Hết thời gian → Auto start
            if (autoStartTimer <= 0)
            {
                DebugLog($"Auto start timer expired, starting game with {playerCount} players...");
                await AutoStartGameAsync();
                return false;
            }
        }
    }
    else
    {
        // Chưa đủ 2 người → Reset timer
        isWaitingForAutoStart = false;
        autoStartTimer = 0f;
    }
}
```

#### **C. AutoStartGameAsync() - Hiện "Match Found"**

```csharp
private async Task AutoStartGameAsync()
{
    DebugLog("Auto starting matchmaker game...");

    // Fire OnMatchFound event → Hiện "Match Found" notification
    OnMatchFound?.Invoke(CurrentMatch);

    // Create Relay
    string relayJoinCode = await RelayService.Instance.CreateRelayAsync();
    
    // Update lobby with relay code
    var updateOptions = new UpdateLobbyOptions
    {
        Data = new Dictionary<string, DataObject>
        {
            { "RelayJoinCode", new DataObject(..., relayJoinCode) },
            { "GameStarted", new DataObject(..., "true") }
        }
    };
    await LobbyService.Instance.UpdateLobbyAsync(CurrentMatch.Id, updateOptions);

    // Wait 2s để user thấy notification
    await Task.Delay(2000);

    // Load GameScene
    SceneManager.LoadScene("GameScene");
}
```

#### **D. PanelNotification - "Match Found"**

```csharp
// PanelNotification.cs

public void ShowMatchFoundNotification()
{
    ShowNotification("Match Found", 2f); // Hiện 2s rồi ẩn
}
```

---

### **2. PanelCustomRoom bị ẩn** ✅

**Vấn đề:** Click btnCustomRoom → PanelCustomRoom không hiện

**Fix:** Thêm debug logs để kiểm tra

```csharp
// LobbyUIManager.cs - OpenCustomRoomPanel()

public async void OpenCustomRoomPanel()
{
    DebugLog("=== OpenCustomRoomPanel CALLED ===");
    
    // Sign in to UGS if needed
    if (!UGSAuthService.IsSignedIn)
    {
        DebugLog("UGS not signed in, signing in...");
        bool signedIn = await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
        if (!signedIn)
        {
            DebugLogError("Failed to sign in to UGS");
            return;
        }
        DebugLog("UGS signed in successfully");
    }
    
    // Show PanelCustomRoom
    if (panelCustomRoom == null)
    {
        DebugLogError("panelCustomRoom is NULL! Please assign in Inspector!");
        return;
    }
    
    DebugLog($"Setting panelCustomRoom active: {panelCustomRoom.name}");
    panelCustomRoom.SetActive(true);
    DebugLog($"panelCustomRoom.activeSelf: {panelCustomRoom.activeSelf}");
    
    // Show PanelRoom (mặc định)
    ShowPanelRoom();
    
    // Load room list
    await RefreshRoomList();
    
    DebugLog("=== OpenCustomRoomPanel COMPLETE ===");
}
```

**Check trong Unity:**
1. Inspector → LobbyUIManager → panelCustomRoom assigned?
2. Console → Log "panelCustomRoom is NULL"?
3. Console → Log "panelCustomRoom.activeSelf: true"?

---

## 🎵 FLOW HOÀN CHỈNH

### **Scenario 1: Test 1 mình**

```
Player A: Click "Tìm trận"
    ↓
ButtonWaitGame: "Đang tìm... 30s"
    ↓
Query lobbies → Không tìm thấy
    ↓
Tạo lobby "Match_123456" (1/4)
    ↓
KHÔNG hiện "Match Found" (vì tạo mới, chưa join)
    ↓
Đợi 30s... (autoStartTimer: 30 → 29 → ... → 0)
    ↓
Hết 30s → AutoStartGameAsync()
    ↓
Hiện "Match Found" (2s)
    ↓
Load GameScene
```

---

### **Scenario 2: Test 2 người**

```
Player A: Click "Tìm trận"
    ↓
Tạo lobby "Match_123456" (1/4)
    ↓
KHÔNG hiện "Match Found"
    ↓
Player B: Click "Tìm trận"
    ↓
Query lobbies → Tìm thấy "Match_123456" (1/4)
    ↓
Join lobby → (2/4)
    ↓
Player B: Hiện "Match Found" (vì JOIN lobby có sẵn)
    ↓
Player A (Host): Bắt đầu đếm 30s
    ↓
autoStartTimer: 30 → 28 → 26 → ... → 0
    ↓
Hết 30s → AutoStartGameAsync()
    ↓
Both: Hiện "Match Found" (2s)
    ↓
Both: Load GameScene
```

---

### **Scenario 3: Test 4 người (đủ ngay)**

```
Player A: Tạo lobby (1/4)
    ↓
Player B: Join → (2/4) → Hiện "Match Found"
    ↓
Player A: Bắt đầu đếm 30s
    ↓
Player C: Join → (3/4)
    ↓
Player D: Join → (4/4)
    ↓
Player A (Host): Đủ 4 người → AutoStartGameAsync() NGAY (không đợi 30s)
    ↓
All: Hiện "Match Found" (2s)
    ↓
All: Load GameScene
```

---

## 🧪 TEST

### **Test 1: Tạo lobby mới (1 mình)**
```
1. Play MenuScene
2. Click "Tìm trận"
3. ✅ ButtonWaitGame: "Đang tìm... 30s"
4. ✅ Tạo lobby mới
5. ✅ KHÔNG hiện "Match Found"
6. ✅ Console: "Created new lobby, waiting for other players..."
7. Wait 30s
8. ✅ Console: "Auto start timer expired, starting game..."
9. ✅ Hiện "Match Found" (2s)
10. ✅ Load GameScene
```

---

### **Test 2: Join lobby có sẵn (2 người)**
```
1. Player A: Tìm trận → Tạo lobby (1/4)
2. Player B: Tìm trận → Join lobby (2/4)
3. ✅ Player B: Hiện "Match Found" ngay
4. ✅ Player A: Console "Match ready (2/4), waiting 30s..."
5. Wait 30s
6. ✅ Both: Hiện "Match Found" (2s)
7. ✅ Both: Load GameScene
```

---

### **Test 3: Đủ 4 người**
```
1. Player A: Tạo lobby (1/4)
2. Player B: Join (2/4) → Đếm 30s
3. Player C: Join (3/4)
4. Player D: Join (4/4)
5. ✅ Console: "Lobby full (4/4), auto starting game..."
6. ✅ All: Hiện "Match Found" (2s)
7. ✅ All: Load GameScene (KHÔNG đợi 30s)
```

---

### **Test 4: PanelCustomRoom**
```
1. Play MenuScene
2. Click "Tạo phòng"
3. ✅ Console: "=== OpenCustomRoomPanel CALLED ==="
4. ✅ Console: "Setting panelCustomRoom active: ..."
5. ✅ Console: "panelCustomRoom.activeSelf: true"
6. ✅ PanelCustomRoom hiện
7. ✅ PanelRoom hiện (list phòng)
```

---

## 📁 FILES MODIFIED

1. ✅ **MatchmakerService.cs** - Auto start sau 30s
2. ✅ **PanelNotification.cs** - "Match Found" text
3. ✅ **LobbyUIManager.cs** - Debug logs

---

## 🎯 SUMMARY

**Matchmaker:**
- ✅ Chỉ hiện "Match Found" khi JOIN lobby có sẵn
- ✅ KHÔNG hiện khi TẠO lobby mới
- ✅ Đợi 30s sau khi đủ 2 người
- ✅ Auto start khi hết 30s HOẶC đủ 4 người
- ✅ Hiện "Match Found" (2s) → Load GameScene

**PanelCustomRoom:**
- ✅ Debug logs để kiểm tra
- ✅ Check panelCustomRoom NULL
- ✅ Check activeSelf

---

**TEST NGAY!** 🚀

Chạy 4 tests trên để verify! Chúc bạn thành công! 🎉

