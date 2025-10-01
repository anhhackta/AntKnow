# 🎮 Matchmaker Setup trên Unity Cloud

## 📋 Tổng quan

Matchmaker là hệ thống tìm trận tự động của Unity Gaming Services. Nó sử dụng:
- **Queues** (Hàng đợi) - Nơi người chơi chờ tìm trận
- **Pools** (Nhóm) - Nhóm người chơi theo tiêu chí
- **Tickets** (Vé) - Đại diện cho 1 người chơi hoặc nhóm
- **Fleets** (Đội máy chủ) - Dedicated servers (nếu dùng Multiplay Hosting)

## 🔧 Setup trên cloud.unity.com

### Bước 1: Truy cập Unity Dashboard

1. Vào https://cloud.unity.com/
2. Chọn Organization của bạn
3. Chọn Project "AntKnow"
4. Vào **Multiplayer** > **Matchmaker**

### Bước 2: Enable Matchmaker

1. Click **Enable Matchmaker**
2. Chọn region: **Asia Southeast (Singapore)**
3. Click **Confirm**

---

## 🎯 Cấu hình Queues (Hàng đợi)

### Tạo Queue mới

1. Vào tab **Queues**
2. Click **Create Queue**

### Cấu hình Queue "FindGame"

```
Queue Name: FindGame
Description: Queue for finding 4-player matches
```

#### General Settings:
```
Queue Enabled: ✅ Yes
Maximum players on a ticket: 1
  → Mỗi ticket = 1 người chơi (không cho party)
  
Default Queue Timeout: 60 seconds
  → Thời gian tối đa chờ trong queue
  
Match Timeout: 300 seconds (5 phút)
  → Thời gian tối đa của 1 trận đấu
```

#### Pool Configuration:
```
Default Pool: GamePool
  → Tên pool sẽ tạo ở bước tiếp theo
```

---

## 🏊 Cấu hình Pools (Nhóm người chơi)

### Tạo Pool mới

1. Vào tab **Pools**
2. Click **Create Pool**

### Cấu hình Pool "GamePool"

```
Pool Name: GamePool
Description: Pool for 4-player board game matches
```

#### Match Logic:
```
Match Type: Standard Match
  → Ghép người chơi theo thứ tự vào queue

Min Players: 2
  → Tối thiểu 2 người để bắt đầu trận

Max Players: 4
  → Tối đa 4 người trong 1 trận

Team Count: 1
  → Không chia team (tất cả chơi chung)
```

#### Backfill Settings:
```
Enable Backfill: ❌ No
  → Không cho người vào giữa chừng
```

#### Match Properties (Optional):
```
Có thể thêm custom properties nếu cần:
- gameMode: "BoardGame"
- mapName: "DefaultBoard"
```

---

## 🖥️ Hosting Settings (Dedicated Server)

> **LƯU Ý**: Phần này chỉ cần khi bạn đã hoàn thành GameScene và muốn deploy dedicated server.
> Hiện tại có thể **BỎ QUA** và dùng Relay (P2P) thay thế.

### Khi nào cần Dedicated Server?

- ✅ Cần chống cheat (server authority)
- ✅ Game logic phức tạp
- ✅ Nhiều người chơi (>4 người)
- ❌ Demo/Testing (dùng Relay đủ)

### Cấu hình Fleet (khi cần)

1. Vào **Multiplay** > **Hosting**
2. Click **Create Fleet**

```
Fleet Name: AntKnowGameServers
Region: Asia Southeast (Singapore)

Build Configuration:
- Build Name: AntKnowServer
- Build Type: Linux Server
- Executable: ./AntKnowServer.x86_64

Server Configuration:
- Min Servers: 1
- Max Servers: 10
- Players per Server: 4

Scaling:
- Scale up when: Queue > 5 players
- Scale down when: Idle > 5 minutes
```

### Link Fleet với Pool

Quay lại **Matchmaker** > **Pools** > **GamePool**:

```
Hosting:
- Enable Dedicated Server: ✅ Yes
- Fleet: AntKnowGameServers
- Allocation Timeout: 30 seconds
```

---

## 🔗 Fleets Integration (Chi tiết sau)

### Workflow với Dedicated Server:

```
Player tìm trận
    ↓
Vào Queue "FindGame"
    ↓
Matchmaker ghép đủ người (2-4)
    ↓
Tạo Match trong Pool "GamePool"
    ↓
Request Fleet allocation
    ↓
Fleet spawn 1 dedicated server
    ↓
Server nhận match data
    ↓
Players connect to server
    ↓
Game starts
```

### Workflow với Relay (Hiện tại):

```
Player tìm trận
    ↓
Vào Queue "FindGame"
    ↓
Matchmaker ghép đủ người (2-4)
    ↓
Tạo Match trong Pool "GamePool"
    ↓
Host tạo Relay allocation
    ↓
Host share Relay join code
    ↓
Clients join Relay
    ↓
Game starts (P2P)
```

---

## 📊 Testing Matchmaker

### Test trên Dashboard

1. Vào **Matchmaker** > **Queues** > **FindGame**
2. Click **Test Queue**
3. Tạo 2-4 test tickets
4. Xem kết quả matching

### Test trong Game

```csharp
// Trong MatchmakerService.cs
public async Task<bool> StartMatchmakingAsync()
{
    // Create ticket
    var ticketOptions = new CreateTicketOptions
    {
        QueueName = "FindGame",
        Attributes = new Dictionary<string, object>
        {
            { "skill", GameDataManager.Instance.currentLevel },
            { "region", "asia-southeast" }
        }
    };
    
    var ticket = await MatchmakerService.Instance.CreateTicketAsync(ticketOptions);
    
    // Poll for match
    while (ticket.Status == TicketStatus.InQueue)
    {
        await Task.Delay(1000);
        ticket = await MatchmakerService.Instance.GetTicketAsync(ticket.Id);
    }
    
    if (ticket.Status == TicketStatus.Matched)
    {
        // Get match assignment
        var assignment = ticket.Assignment;
        string connectionInfo = assignment.ConnectionInfo;
        
        // Connect to match
        // ...
    }
}
```

---

## 🎯 Recommended Settings cho Game Cờ Tỷ Phú

### Queue "FindGame":
```
Maximum players on a ticket: 1
Default Queue Timeout: 60 seconds
Match Timeout: 300 seconds
Default Pool: GamePool
```

### Pool "GamePool":
```
Match Type: Standard Match
Min Players: 2
Max Players: 4
Team Count: 1
Enable Backfill: No
```

### Hosting (Tạm thời):
```
Enable Dedicated Server: ❌ No
→ Dùng Relay thay thế
```

---

## 🔄 Migration Plan: Relay → Dedicated Server

### Phase 1: Development (Hiện tại)
- ✅ Dùng Relay (P2P)
- ✅ Matchmaker với Lobby
- ✅ Test với 2-4 players

### Phase 2: GameScene Complete
- ✅ Hoàn thành game logic
- ✅ Test multiplayer gameplay
- ✅ Optimize network traffic

### Phase 3: Dedicated Server (Sau này)
- ✅ Build Linux dedicated server
- ✅ Create Fleet trên Multiplay
- ✅ Link Fleet với Pool
- ✅ Update MatchmakerService để connect server
- ✅ Deploy và test

---

## 📝 Notes

### Hiện tại (Phase 1):
- **Matchmaker**: Có thể BỎ QUA nếu chỉ dùng Lobby
- **Lobby**: Đủ cho demo và testing
- **Relay**: P2P connection, không cần server

### Khi nào dùng Matchmaker?
- ✅ Muốn tìm trận tự động (không cần tạo/join room)
- ✅ Muốn skill-based matching
- ✅ Muốn region-based matching
- ❌ Chỉ cần custom room (dùng Lobby đủ)

### Khi nào dùng Dedicated Server?
- ✅ Cần chống cheat
- ✅ Game logic phức tạp
- ✅ Nhiều người chơi
- ❌ Demo/Testing (Relay đủ)

---

## 🚀 Quick Start (Recommended)

### Cho Development:
1. ✅ Enable Lobby service
2. ✅ Enable Relay service
3. ❌ BỎ QUA Matchmaker (dùng Lobby thay thế)
4. ❌ BỎ QUA Dedicated Server (dùng Relay)

### Khi Production:
1. ✅ Enable Matchmaker
2. ✅ Configure Queue "FindGame"
3. ✅ Configure Pool "GamePool"
4. ✅ Deploy Dedicated Server
5. ✅ Link Fleet với Pool

---

## 📚 Resources

- [Unity Matchmaker Docs](https://docs.unity.com/matchmaker/)
- [Unity Multiplay Docs](https://docs.unity.com/multiplay/)
- [Unity Relay Docs](https://docs.unity.com/relay/)

---

**Kết luận**: Hiện tại tập trung vào **Lobby + Relay** là đủ. Matchmaker và Dedicated Server sẽ làm sau khi hoàn thành GameScene.

