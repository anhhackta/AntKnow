# 🌐 NETWORK OBJECT SETTINGS - CHI TIẾT

**Hướng dẫn setup NetworkObject cho Player Prefabs và GameManager**

---

## 🎮 **PLAYER PREFABS - NetworkObject Settings**

### **PlayerMale.prefab & PlayerFemale.prefab**

#### **NetworkObject Component:**

```
┌─────────────────────────────────────────┐
│ Network Object                          │
├─────────────────────────────────────────┤
│ ✓ Is Player Object: TRUE                │ ← ⭐ QUAN TRỌNG!
│                                         │
│ Owner Permission: Owner                 │ ← ⭐ Chỉ owner điều khiển
│                                         │
│ ✓ Synchronize Transform: TRUE           │ ← ⭐ Sync vị trí
│                                         │
│ Interpolate: TRUE                       │ ← Smooth movement
│                                         │
│ Use Half Float Precision: FALSE         │
│                                         │
│ In Local Space: FALSE                   │
│                                         │
│ Slerpable: FALSE                        │
│                                         │
│ Use Quaternion Sync: FALSE              │
│                                         │
│ Use Quaternion Compression: FALSE       │
│                                         │
│ Threshold Values:                       │
│ ├── Position Threshold: 0.001           │
│ ├── Rotation Angle Threshold: 0.01      │
│ └── Scale Threshold: 0.01               │
└─────────────────────────────────────────┘
```

---

### **Giải thích từng setting:**

#### **1. Is Player Object: TRUE** ⭐
**Tại sao:**
- Đánh dấu đây là player object
- Netcode sẽ tự động assign ownership cho client
- Mỗi client sẽ control 1 player object

**Kết quả:**
- Player 1 (host) → IsOwner = TRUE cho PlayerMale của mình
- Player 2 (client) → IsOwner = TRUE cho PlayerFemale của mình
- Mỗi player chỉ điều khiển nhân vật của mình

---

#### **2. Owner Permission: Owner** ⭐
**Tại sao:**
- Chỉ owner mới có quyền gọi ServerRpc
- Ngăn người khác điều khiển nhân vật của bạn

**Ví dụ:**
```csharp
// Player 1 gọi:
player.MoveToTile(5); // ✅ OK - IsOwner = TRUE

// Player 2 cố gọi player của Player 1:
player1.MoveToTile(5); // ❌ FAIL - Not owner
```

**Options:**
- **Owner**: Chỉ owner (✅ DÙNG CÁI NÀY)
- **Server Only**: Chỉ server
- **Everyone**: Ai cũng được (không an toàn)

---

#### **3. Synchronize Transform: TRUE** ⭐
**Tại sao:**
- Tự động sync position, rotation, scale
- Không cần viết code sync thủ công
- Player di chuyển → tất cả clients thấy

**Kết quả:**
```
Player 1 di chuyển từ Tile 0 → Tile 5:
├── Player 1 (local): Thấy ngay lập tức
├── Player 2 (remote): Thấy sau ~50ms (network delay)
├── Player 3 (remote): Thấy sau ~50ms
└── Player 4 (remote): Thấy sau ~50ms
```

**Nếu FALSE:**
- Phải tự viết NetworkVariable<Vector3> position
- Phải tự sync thủ công
- Nhiều code hơn

---

#### **4. Interpolate: TRUE**
**Tại sao:**
- Smooth movement giữa các network updates
- Không bị giật lag

**Kết quả:**
```
Without Interpolate:
Player position: (0,0,0) → (5,0,0) → (10,0,0)
Visual: Giật giật, teleport

With Interpolate:
Player position: (0,0,0) → (1,0,0) → (2,0,0) → ... → (10,0,0)
Visual: Smooth, mượt mà
```

---

#### **5. Threshold Values**
**Position Threshold: 0.001**
- Chỉ sync khi position thay đổi > 0.001 units
- Tiết kiệm bandwidth

**Rotation Angle Threshold: 0.01**
- Chỉ sync khi rotation thay đổi > 0.01 degrees

**Scale Threshold: 0.01**
- Chỉ sync khi scale thay đổi > 0.01

---

## 🎯 **GAMEMANAGER - NetworkObject Settings**

### **GameManager GameObject**

#### **NetworkObject Component:**

```
┌─────────────────────────────────────────┐
│ Network Object                          │
├─────────────────────────────────────────┤
│ ✗ Is Player Object: FALSE               │ ← ⭐ Không phải player
│                                         │
│ Owner Permission: Server Only           │ ← ⭐ Chỉ server điều khiển
│                                         │
│ ✗ Synchronize Transform: FALSE          │ ← Không cần sync transform
│                                         │
│ Interpolate: FALSE                      │
│                                         │
│ (Các settings khác giữ default)        │
└─────────────────────────────────────────┘
```

---

### **Giải thích:**

#### **1. Is Player Object: FALSE**
**Tại sao:**
- GameManager không phải player object
- Là singleton, chỉ có 1 instance
- Server quản lý

---

#### **2. Owner Permission: Server Only**
**Tại sao:**
- Chỉ server mới điều khiển game logic
- Clients không thể cheat
- Turn system, money, properties đều do server quản lý

**Ví dụ:**
```csharp
// Server:
gameManager.StartTurn(); // ✅ OK

// Client:
gameManager.StartTurn(); // ❌ FAIL - Server only
```

---

#### **3. Synchronize Transform: FALSE**
**Tại sao:**
- GameManager không di chuyển
- Không cần sync position/rotation
- Tiết kiệm bandwidth

---

## 📊 **SO SÁNH**

### **Player Prefabs vs GameManager:**

```
┌──────────────────────┬─────────────────┬─────────────────┐
│ Setting              │ Player Prefabs  │ GameManager     │
├──────────────────────┼─────────────────┼─────────────────┤
│ Is Player Object     │ ✓ TRUE          │ ✗ FALSE         │
│ Owner Permission     │ Owner           │ Server Only     │
│ Synchronize Transform│ ✓ TRUE          │ ✗ FALSE         │
│ Interpolate          │ ✓ TRUE          │ ✗ FALSE         │
└──────────────────────┴─────────────────┴─────────────────┘
```

---

## 🎮 **PLAYERGAMECONTROLLER - Các fields cần điền**

### **Inspector Fields:**

```
┌─────────────────────────────────────────────────────────┐
│ Player Game Controller (Script)                         │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ▼ Player Info                                           │
│   ├── Player Name: "Player"                             │ ← Để default, sẽ set runtime
│   ├── Player Id: ""                                     │ ← Để trống, sẽ set runtime
│   ├── ✓ Is Male: TRUE (Male) / FALSE (Female)          │ ← ⭐ SET THEO PREFAB!
│   └── Player Index: 0                                   │ ← Để 0, sẽ set runtime
│                                                         │
│ ▼ Game State                                            │
│   ├── Current Tile: 0                                   │ ← Để 0 (start tile)
│   ├── Money: 10000                                      │ ← Để 10000 (starting money)
│   ├── Jail Counter: 0                                   │ ← Để 0
│   └── Skip Next Turn: FALSE                             │ ← Để FALSE
│                                                         │
│ ▼ Stats from Loadout                                    │
│   ├── Health: 0                                         │ ← Để 0, sẽ set runtime từ loadout
│   ├── Agility: 0                                        │ ← Để 0, sẽ set runtime từ loadout
│   ├── Intelligence: 0                                   │ ← Để 0, sẽ set runtime từ loadout
│   ├── Luck: 0                                           │ ← Để 0, sẽ set runtime từ loadout
│   └── Resistance: 0                                     │ ← Để 0, sẽ set runtime từ loadout
│                                                         │
│ ▼ Movement                                              │
│   ├── Move Speed: 5                                     │ ← ⭐ SET = 5
│   ├── Bounce Height: 0.5                                │ ← ⭐ SET = 0.5
│   ├── Bounce Duration: 0.3                              │ ← ⭐ SET = 0.3
│   ├── Board Manager: None                               │ ← Để trống, auto find runtime
│   └── Board Center: (0, 0, 0)                           │ ← Để (0,0,0)
│                                                         │
│ ▼ Animation                                             │
│   └── Animator: [Drag MaleModel/Animator here]         │ ← ⭐ ASSIGN ANIMATOR!
│                                                         │
│ ▼ Turn Indicator                                        │
│   └── Turn Indicator: None                              │ ← Để trống, auto create runtime
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

### **Các fields QUAN TRỌNG cần set:**

#### **1. Is Male** ⭐⭐⭐
```
PlayerMale.prefab:   Is Male = TRUE
PlayerFemale.prefab: Is Male = FALSE
```
**Tại sao quan trọng:**
- GameManager dùng để chọn prefab nào spawn
- UI dùng để hiển thị avatar (male/female sprite)
- Không thể sai!

---

#### **2. Animator** ⭐⭐⭐
```
PlayerMale.prefab:   Animator = MaleModel/Animator
PlayerFemale.prefab: Animator = FemaleModel/Animator
```
**Tại sao quan trọng:**
- Không có animator → không có animation
- Idle animation không chạy
- Game vẫn chạy nhưng model đứng im như tượng

**Cách assign:**
1. Mở Prefab mode
2. Expand model child
3. Tìm Animator component
4. Drag vào field Animator

---

#### **3. Movement Settings** ⭐⭐
```
Move Speed: 5        → Tốc độ di chuyển
Bounce Height: 0.5   → Độ cao bounce
Bounce Duration: 0.3 → Thời gian bounce
```
**Tại sao quan trọng:**
- Ảnh hưởng visual của movement
- Quá nhanh → giật
- Quá chậm → nhàm chán

---

### **Các fields KHÔNG CẦN set (auto runtime):**

```
✗ Player Name       → Set bởi GameManager.Initialize()
✗ Player Id         → Set bởi GameManager.Initialize()
✗ Player Index      → Set bởi GameManager.SetPlayerIndex()
✗ Stats (HP/AGI...) → Set bởi GameManager.Initialize()
✗ Board Manager     → Auto find bởi OnNetworkSpawn()
✗ Turn Indicator    → Auto create bởi OnNetworkSpawn()
```

---

## ✅ **CHECKLIST CUỐI CÙNG**

### **PlayerMale.prefab:**
- [ ] NetworkObject: Is Player Object = TRUE
- [ ] NetworkObject: Owner Permission = Owner
- [ ] NetworkObject: Synchronize Transform = TRUE
- [ ] NetworkObject: Interpolate = TRUE
- [ ] PlayerGameController: Is Male = TRUE
- [ ] PlayerGameController: Animator = MaleModel/Animator
- [ ] PlayerGameController: Move Speed = 5
- [ ] PlayerGameController: Bounce Height = 0.5
- [ ] MaleModel child exists
- [ ] MaleModel has Animator component

### **PlayerFemale.prefab:**
- [ ] NetworkObject: Is Player Object = TRUE
- [ ] NetworkObject: Owner Permission = Owner
- [ ] NetworkObject: Synchronize Transform = TRUE
- [ ] NetworkObject: Interpolate = TRUE
- [ ] PlayerGameController: Is Male = FALSE
- [ ] PlayerGameController: Animator = FemaleModel/Animator
- [ ] PlayerGameController: Move Speed = 5
- [ ] PlayerGameController: Bounce Height = 0.5
- [ ] FemaleModel child exists
- [ ] FemaleModel has Animator component

### **GameManager:**
- [ ] NetworkObject: Is Player Object = FALSE
- [ ] NetworkObject: Owner Permission = Server Only
- [ ] NetworkObject: Synchronize Transform = FALSE
- [ ] GameManager: Player Prefab Male assigned
- [ ] GameManager: Player Prefab Female assigned
- [ ] GameManager: All UI references assigned
- [ ] GameManager: Demo Mode = TRUE

---

## 🎯 **TESTING**

### **Test NetworkObject:**

1. **Play in Editor**
2. **Check Console:**
   ```
   ✅ "[PlayerGameController] Spawned: Player (IsOwner: True, IsMale: True)"
   ✅ "[GameManager] Player spawned successfully"
   ✅ No errors
   ```

3. **Check Hierarchy:**
   ```
   PlayerMale(Clone)
   ├── NetworkObject (active)
   ├── PlayerGameController (active)
   ├── MaleModel (active)
   │   └── Animator (playing idle)
   └── TurnIndicator (inactive, will show on turn)
   ```

4. **Test Movement:**
   - Click "ROLL DICE"
   - Player should move smoothly
   - No teleporting
   - Bounce effect visible

---

**DONE! Game sẽ chạy được với settings này! 🚀**

