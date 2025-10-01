# 🐛 Troubleshooting Guide

## ❌ Lỗi: "matchmakingPanel has not been assigned"

### Nguyên nhân:
```
PanelRoom.cs (script cũ) đang conflict với LobbyUIManager.cs (script mới)
→ Cả 2 đều subscribe vào CustomLobbyService events
→ PanelRoom.cs thiếu reference "matchmakingPanel" → Lỗi
```

### Giải pháp:
```
✅ ĐÃ SỬA: PanelRoom.cs tự động disable trong Awake()

Code đã thêm:
private void Awake()
{
    Debug.LogWarning("PanelRoom.cs is DEPRECATED! Use LobbyUIManager.cs instead.");
    this.enabled = false;
    return;
}
```

### Kiểm tra:
```
1. Chọn GameObject có PanelRoom component
2. Inspector > PanelRoom (Script)
3. Kiểm tra checkbox bị unchecked (disabled) ✅
4. Nếu vẫn enabled, uncheck manually
```

---

## ❌ Lỗi: "Already in a lobby"

### Nguyên nhân:
```
Tạo phòng lần 1 → Lỗi (vì PanelRoom.cs conflict)
→ Lobby được tạo nhưng UI không chuyển
→ Tạo phòng lần 2 → "Already in a lobby"
```

### Giải pháp:
```
✅ ĐÃ SỬA: LobbyUIManager tự động leave lobby cũ trước khi tạo mới

Code đã thêm:
// Check if already in lobby
if (CustomLobbyService.Instance.IsInLobby)
{
    DebugLogError("Already in a lobby! Leaving current lobby first...");
    await CustomLobbyService.Instance.LeaveLobbyAsync();
    await System.Threading.Tasks.Task.Delay(500); // Wait for cleanup
}
```

---

## ❌ Lỗi: Relay khiến game Pause trong Play Mode

### Nguyên nhân:
```
KHÔNG PHẢI LỖI!

Khi có exception trong async code:
→ Unity tự động pause Play Mode để debug
→ Relay không phải nguyên nhân, chỉ là nơi exception xảy ra
```

### Giải pháp:
```
1. Xem Console để tìm exception thực sự
2. Fix exception đó (thường là missing reference)
3. Relay sẽ hoạt động bình thường
```

### Disable Auto Pause (nếu cần):
```
Unity Editor:
Edit > Preferences > General
→ Uncheck "Error Pause" ❌ (không khuyến khích)
```

---

## ❌ Lỗi: Không chuyển đến PanelJoinRoom sau khi tạo phòng

### Nguyên nhân:
```
1. PanelRoom.cs conflict → CreateLobby fail
2. Event OnLobbyCreated không được trigger
3. LobbyUIManager không nhận event → Không chuyển panel
```

### Giải pháp:
```
✅ Disable PanelRoom.cs
✅ Chỉ dùng LobbyUIManager.cs

Flow đúng:
OnConfirmCreateClicked()
    ↓
CustomLobbyService.CreateLobbyAsync()
    ↓
OnLobbyCreated event triggered
    ↓
LobbyUIManager.OnLobbyCreated()
    ↓
ShowPanelJoinRoom()
```

---

## ❌ Lỗi: RoomItemPrefabs không hiện text đúng

### Nguyên nhân:
```
Thứ tự Text trong Hierarchy sai:
RoomItem
├── TextPlayerCount (index 0) ❌
└── TextRoomName (index 1) ❌

Code lấy:
allTexts[0] → Tên phòng (nhưng lại là TextPlayerCount)
allTexts[1] → Số người (nhưng lại là TextRoomName)
```

### Giải pháp:
```
Đúng thứ tự:
RoomItem
├── TextRoomName (index 0) ✅
└── TextPlayerCount (index 1) ✅

Cách fix:
1. Mở RoomItemPrefabs.prefab
2. Drag TextRoomName lên trên TextPlayerCount
3. Save prefab
```

---

## ❌ Lỗi: PlayerItemPrefabs không hiện tên người chơi

### Nguyên nhân:
```
1. Prefab không có Text component
2. Button.Interactable = true (nên là false)
3. Text bị ẩn (alpha = 0)
```

### Giải pháp:
```
1. Kiểm tra prefab có Text component
2. Uncheck Button.Interactable
3. Kiểm tra Text.Color alpha = 255
```

---

## ❌ Lỗi: List phòng không load

### Nguyên nhân:
```
1. roomListContainer chưa assign
2. roomItemPrefab chưa assign
3. Chưa sign in to UGS
```

### Giải pháp:
```
1. Assign roomListContainer → ScrollView/Content
2. Assign roomItemPrefab → RoomItemPrefabs.prefab
3. Check log: "Signed in to UGS" ✅
```

---

## ❌ Lỗi: Button "Custom" không mở PanelCustomRoom

### Nguyên nhân:
```
1. lobbyUIManager reference null trong PanelHome
2. PanelCustomRoom chưa tạo
3. Button onClick chưa assign
```

### Giải pháp:
```
PanelHome Inspector:
├── Button Custom → onClick assigned ✅
└── Lobby UI Manager → PanelCustomRoom GameObject ✅

Check log:
"PanelHome: Custom Room button clicked" ✅
"Showing PanelRoom" ✅
```

---

## 🔍 Debug Checklist

### Khi tạo phòng:
```
Console logs (đúng):
[LobbyUIManager] Creating room: RoomName
[CustomLobbyService] Creating lobby: RoomName
[CustomLobbyService] Lobby created successfully
[LobbyUIManager] Lobby created: RoomName
[LobbyUIManager] Showing PanelJoinRoom

Console logs (sai):
[CustomLobbyService] Already in a lobby ❌
[CustomLobbyService] Unexpected error ❌
[PanelRoom] Lobby error ❌ → PanelRoom.cs vẫn enabled!
```

### Khi join phòng:
```
Console logs (đúng):
[LobbyUIManager] Room clicked: lobby-id
[CustomLobbyService] Joining lobby with ID: lobby-id
[CustomLobbyService] Joined lobby successfully
[LobbyUIManager] Lobby joined: RoomName
[LobbyUIManager] Showing PanelJoinRoom
```

---

## 🛠️ Quick Fixes

### Fix 1: Disable PanelRoom.cs
```
1. Tìm GameObject có PanelRoom component
2. Inspector > PanelRoom (Script)
3. Uncheck checkbox (disable script)
4. Hoặc Remove Component (nếu không cần)
```

### Fix 2: Clear lobby state
```
Play Mode:
1. Stop game
2. Play lại
3. Lobby state được reset

Hoặc trong code:
await CustomLobbyService.Instance.LeaveLobbyAsync();
```

### Fix 3: Check references
```
LobbyUIManager Inspector:
- ✅ All panels assigned
- ✅ All buttons assigned
- ✅ roomItemPrefab assigned
- ✅ playerItemPrefab assigned
- ✅ roomListContainer assigned
- ✅ playerListContainer assigned

PanelHome Inspector:
- ✅ lobbyUIManager assigned
- ✅ buttonCustomRoom onClick assigned
```

---

## 📝 Common Mistakes

### ❌ Mistake 1: Dùng cả PanelRoom.cs và LobbyUIManager.cs
```
→ Conflict events
→ Lỗi "matchmakingPanel not assigned"

✅ Fix: Chỉ dùng LobbyUIManager.cs
```

### ❌ Mistake 2: Thứ tự Text sai trong RoomItemPrefabs
```
→ Tên phòng hiện ở vị trí số người
→ Số người hiện ở vị trí tên phòng

✅ Fix: TextRoomName phải ở index 0
```

### ❌ Mistake 3: Quên assign references
```
→ NullReferenceException
→ UI không hoạt động

✅ Fix: Check tất cả references trong Inspector
```

### ❌ Mistake 4: Prefab không có Text component
```
→ Không hiện text
→ GetComponentInChildren<Text>() return null

✅ Fix: Add Text hoặc TextMeshProUGUI component
```

---

## 🎯 Testing Steps

### Test tạo phòng:
```
1. Play game
2. Click button "Custom"
3. PanelCustomRoom mở ✅
4. PanelRoom hiện ✅
5. Click "Tạo phòng"
6. PanelCreateRoom mở ✅
7. Nhập tên → Click "Tạo phòng"
8. PanelJoinRoom hiện ✅
9. Tên phòng hiện đúng ✅
10. Số người hiện "1/4" ✅
11. Tên người chơi hiện trong list ✅
```

### Test join phòng:
```
1. Build game ra 2 instances
2. Instance 1: Tạo phòng "Test"
3. Instance 2: Click "Custom"
4. Instance 2: Click "Làm mới"
5. Instance 2: Thấy "Test (1/4)" ✅
6. Instance 2: Click vào phòng
7. Instance 2: Vào PanelJoinRoom ✅
8. Instance 1: Thấy "2/4" ✅
9. Instance 1: Thấy 2 người trong list ✅
```

---

**Version**: 1.0
**Date**: 2025-10-01

