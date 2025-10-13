# ☁️ UNITY MATCHMAKER - POOL & RULES SETUP

## 🎯 MỤC ĐÍCH

Hướng dẫn tạo **Matchmaker Pool** và **Rules** trên Unity Cloud để ghép trận tự động

---

## 📋 THÔNG TIN HIỆN TẠI

### **Lobby Config (ĐÃ TẠO)** ✅
```
Minimum players slots: 2
Maximum players slots: 4
Active Lifespan: 5m
Disconnect Removal Time: 30s
Disconnect Host Migration Time: 5s
```

### **Matchmaker Queue (ĐÃ TẠO)** ✅
```
Queue name: FindGame
Maximum players on a ticket: 1
Pools: 0 ← CẦN TẠO
Date created: September 30, 2025
```

---

## 🚀 BƯỚC 1: TẠO MATCHMAKER POOL

### **1.1. Truy cập Matchmaker**

```
1. https://cloud.unity.com/
2. Select project "AntKnow"
3. Left sidebar → Multiplayer → Matchmaker
4. Click vào Queue "FindGame"
```

---

### **1.2. Tạo Pool**

```
1. Scroll down → Section "Pools"
2. Click "Create Pool"
3. Fill form:
```

**Pool Settings:**

| Field | Value | Giải thích |
|-------|-------|------------|
| **Pool Name** | `DefaultPool` | Tên pool (có thể đặt tùy ý) |
| **Timeout** | `300` | 300 giây = 5 phút (thời gian tìm trận tối đa) |
| **Hosting Setting** | `Client Hosting` | Client làm host (không cần dedicated server) |

---

### **1.3. Hosting Setting - Client Hosting**

**Chọn:** `Client Hosting`

**Giải thích:**
- ✅ **Client Hosting**: 1 player trong match sẽ làm host (dùng Relay)
- ❌ **Dedicated Server**: Cần server riêng (phức tạp, tốn tiền)

**Cho game AntKnow:**
- ✅ Dùng **Client Hosting** (đơn giản, miễn phí)
- ✅ Host = player đầu tiên trong lobby
- ✅ Dùng Unity Relay để kết nối

---

## 🎯 BƯỚC 2: TẠO MATCHMAKING RULES (JSON)

### **2.1. Rules là gì?**

**Rules** = Điều kiện để ghép trận (match players)

**Ví dụ:**
- Ghép players cùng skill level
- Ghép players cùng region
- Ghép players cùng game mode

**Cho game AntKnow (đơn giản):**
- ✅ Ghép bất kỳ ai (không cần điều kiện)
- ✅ Min 2 players, Max 4 players
- ✅ Timeout 300s

---

### **2.2. Rules JSON - ĐƠN GIẢN NHẤT**

**Copy JSON này vào Rules:**

```json
{
  "name": "DefaultMatchmakingRule",
  "teams": [
    {
      "name": "Team",
      "teamCount": {
        "min": 1,
        "max": 1
      },
      "playerCount": {
        "min": 2,
        "max": 4
      }
    }
  ],
  "matchmaking": {
    "backfillEnabled": true,
    "relaxations": []
  }
}
```

**Giải thích:**

| Field | Value | Giải thích |
|-------|-------|------------|
| `name` | `DefaultMatchmakingRule` | Tên rule |
| `teams[0].name` | `Team` | Tên team (1 team duy nhất) |
| `teams[0].teamCount.min` | `1` | Tối thiểu 1 team |
| `teams[0].teamCount.max` | `1` | Tối đa 1 team |
| `teams[0].playerCount.min` | `2` | Tối thiểu 2 players |
| `teams[0].playerCount.max` | `4` | Tối đa 4 players |
| `backfillEnabled` | `true` | Cho phép thêm players vào match đang chờ |
| `relaxations` | `[]` | Không có relaxation (ghép strict) |

---

### **2.3. Rules JSON - CÓ RELAXATION (NÂNG CAO)**

**Nếu muốn ghép nhanh hơn (giảm yêu cầu theo thời gian):**

```json
{
  "name": "DefaultMatchmakingRule",
  "teams": [
    {
      "name": "Team",
      "teamCount": {
        "min": 1,
        "max": 1
      },
      "playerCount": {
        "min": 2,
        "max": 4
      }
    }
  ],
  "matchmaking": {
    "backfillEnabled": true,
    "relaxations": [
      {
        "type": "PlayerCountRelaxation",
        "atSeconds": 60,
        "value": {
          "min": 2,
          "max": 2
        }
      }
    ]
  }
}
```

**Giải thích Relaxation:**
- Sau **60 giây** tìm trận
- Nếu chưa đủ 4 players
- → Ghép luôn với **2 players** (min = 2, max = 2)

**Kết quả:**
- 0-60s: Tìm 2-4 players
- 60s+: Ghép luôn với 2 players (nếu có)

---

## 🎵 BƯỚC 3: PASTE RULES VÀO UNITY CLOUD

### **3.1. Mở Rules Editor**

```
1. Unity Cloud → Matchmaker → Queue "FindGame"
2. Click vào Pool "DefaultPool"
3. Scroll down → Section "Rules"
4. Click "Edit Rules" hoặc "Add Rules"
```

---

### **3.2. Paste JSON**

```
1. Delete existing JSON (nếu có)
2. Paste JSON từ bước 2.2 hoặc 2.3
3. Click "Validate" → Check syntax
4. Click "Save"
```

---

### **3.3. Verify**

```
1. Pool "DefaultPool" → Rules section
2. ✅ Rules JSON hiển thị
3. ✅ Status: "Active"
```

---

## 🧪 BƯỚC 4: TEST MATCHMAKING

### **4.1. Test trong Unity Editor**

```
1. Play MenuScene
2. Click "Tìm trận"
3. Check Console:
   ✅ "Starting matchmaking..."
   ✅ "Searching for available matches..."
   ✅ "Found available lobby" OR "Creating new lobby"
   ✅ NO errors
```

---

### **4.2. Test với 2 clients**

```
1. Build game → Run 2 instances
2. Instance 1: Click "Tìm trận"
3. Instance 2: Click "Tìm trận"
4. ✅ Both join same lobby
5. ✅ Host clicks "Start Game"
6. ✅ Both load GameScene
```

---

## 📋 SUMMARY CONFIG

### **Lobby Config:**
```
Min players: 2
Max players: 4
Active Lifespan: 5m
Disconnect Removal Time: 30s
Disconnect Host Migration Time: 5s
```

### **Matchmaker Queue:**
```
Queue name: FindGame
Max players on ticket: 1
```

### **Matchmaker Pool:**
```
Pool name: DefaultPool
Timeout: 300s (5 phút)
Hosting: Client Hosting
```

### **Matchmaker Rules:**
```json
{
  "name": "DefaultMatchmakingRule",
  "teams": [
    {
      "name": "Team",
      "teamCount": { "min": 1, "max": 1 },
      "playerCount": { "min": 2, "max": 4 }
    }
  ],
  "matchmaking": {
    "backfillEnabled": true,
    "relaxations": []
  }
}
```

---

## 🚨 TROUBLESHOOTING

### **Lỗi 1: "Pool not found"**

**Fix:**
```
1. Unity Cloud → Matchmaker → Queue "FindGame"
2. Verify Pool "DefaultPool" exists
3. Status: Active
```

---

### **Lỗi 2: "Invalid rules JSON"**

**Fix:**
```
1. Copy JSON từ guide
2. Paste vào online JSON validator: https://jsonlint.com/
3. Fix syntax errors
4. Paste lại vào Unity Cloud
```

---

### **Lỗi 3: "Matchmaking timeout"**

**Nguyên nhân:** Không đủ players

**Fix:**
```
1. Tăng timeout: 300 → 600 (10 phút)
2. Hoặc thêm relaxation (ghép với 2 players sau 60s)
```

---

## 🎯 CHECKLIST

### **Unity Cloud:**
- [ ] Lobby Config: Min 2, Max 4
- [ ] Queue "FindGame" created
- [ ] Pool "DefaultPool" created
- [ ] Pool Timeout: 300s
- [ ] Pool Hosting: Client Hosting
- [ ] Rules JSON pasted & saved
- [ ] Rules Status: Active

### **Test:**
- [ ] MenuScene loads → No errors
- [ ] Click "Tìm trận" → Matchmaking starts
- [ ] 2 clients → Join same lobby
- [ ] Host starts → Both load GameScene

---

## 🎯 SUMMARY

**Tạo Pool:**
- ✅ Pool name: DefaultPool
- ✅ Timeout: 300s
- ✅ Hosting: Client Hosting

**Tạo Rules:**
- ✅ Copy JSON từ guide
- ✅ Paste vào Unity Cloud
- ✅ Validate & Save

**Test:**
- ✅ 2 clients tìm trận
- ✅ Join same lobby
- ✅ Start game

---

**THỜI GIAN: 10 PHÚT** ⏱️

**LÀM NGAY!** 🔥

