# 🚀 QUICK START: DEDICATED SERVER TRONG 5 TIẾNG

**Mục tiêu**: Setup server headless + build client + test multiplayer trong 5 tiếng

---

## ⏱️ TIMELINE

```
✅ Hour 1 (0:00-1:00): Setup Unity Project & Scripts
✅ Hour 2 (1:00-2:00): Configure GameScene & NetworkManager
✅ Hour 3 (2:00-3:00): Build & Test Server
✅ Hour 4 (3:00-4:00): Build Client & Connection
✅ Hour 5 (4:00-5:00): Full Multiplayer Test
```

---

## 📋 HOUR 1: SETUP UNITY PROJECT (0:00-1:00)

### **Step 1: Mở Project Server** (5 phút)

```
1. Mở Unity Hub
2. Add Project: "Project Game AntKnow Server"
3. Open với Unity 6000.0.48f1
4. Đợi import xong (~2-3 phút)
```

### **Step 2: Verify Packages** (5 phút)

```
Window → Package Manager → Unity Registry

Kiểm tra đã cài:
✅ Netcode for GameObjects (2.5.1)
✅ Dedicated Server (1.6.1)
✅ Unity Transport (2.x)

Nếu chưa có → Install
```

### **Step 3: Create Folder Structure** (5 phút)

```
Assets/
├── Script/
│   ├── Server/          ← Tạo folder này
│   └── Editor/          ← Tạo folder này
└── Scenes/
    └── GameScene.unity  ← Đã có
```

### **Step 4: Copy Scripts** (10 phút)

**Đã tạo sẵn 3 files:**
```
✅ Assets/Script/Server/ServerBootstrap.cs
✅ Assets/Script/Server/ServerGameManager.cs
✅ Assets/Editor/ServerBuilder.cs
```

**Action:**
1. Copy 3 files trên vào đúng folder
2. Đợi Unity compile (~1 phút)
3. Check Console không có error

### **Step 5: Verify Domain Layer** (10 phút)

```
Kiểm tra đã có:
✅ Assets/Script/Domain/Entities/GameState.cs
✅ Assets/Script/Domain/Entities/PlayerState.cs
✅ Assets/Script/Domain/Entities/PropertyState.cs

Nếu chưa có → Copy từ "Project Game AntKnow"
```

### **Step 6: Create Server Config** (10 phút)

```
1. Right-click Assets/Script/Server
2. Create → ScriptableObject → Server Config
3. Name: "ServerConfig"
4. Configure:
   - Port: 7777
   - Max Players: 4
   - Max Turns: 50
   - Turn Time Limit: 60
```

### **Step 7: Test Compile** (5 phút)

```
1. Ctrl+R (Recompile)
2. Check Console: 0 errors
3. ✅ Hour 1 Complete!
```

---

## 📋 HOUR 2: CONFIGURE GAMESCENE (1:00-2:00)

### **Step 1: Open GameScene** (2 phút)

```
Assets/Scenes/GameScene.unity → Double-click
```

### **Step 2: Setup NetworkManager** (10 phút)

```
1. Hierarchy → Create Empty: "NetworkManager"
2. Add Component: NetworkManager
3. Configure:
   ✅ Don't Destroy On Load: ENABLED
   ✅ Run In Background: ENABLED

4. Add Component: UnityTransport
5. Configure:
   - Protocol Type: UnityTransport
   - Address: 0.0.0.0
   - Port: 7777
   - Server Listen Address: 0.0.0.0
```

### **Step 3: Create Player Prefab** (15 phút)

```
1. Hierarchy → Create Empty: "NetworkPlayer"
2. Add Component: NetworkObject
3. Configure NetworkObject:
   ✅ Don't Destroy With Owner: ENABLED
   ✅ Destroy With Scene: DISABLED

4. Add Component: NetworkTransform
5. Add visual (Cube hoặc Character model)
6. Drag to Assets/Prefabs/NetworkPlayer.prefab
7. Delete from Hierarchy
```

### **Step 4: Assign Player Prefab** (5 phút)

```
NetworkManager → Player Prefab:
✅ Assign: Assets/Prefabs/NetworkPlayer.prefab
```

### **Step 5: Add ServerBootstrap** (10 phút)

```
1. Hierarchy → Create Empty: "ServerBootstrap"
2. Add Component: ServerBootstrap
3. Configure:
   - Server Port: 7777
   - Max Players: 4
   - Auto Start Server: ✅
   - Server Name: "AntKnow Server"
   - Target Frame Rate: 30
   - Enable Detailed Logs: ✅
```

### **Step 6: Add ServerGameManager** (10 phút)

```
1. Hierarchy → Create Empty: "ServerGameManager"
2. Add Component: ServerGameManager
3. Add Component: NetworkObject
4. Configure NetworkObject:
   ✅ Don't Destroy With Owner: ENABLED
   ✅ Destroy With Scene: DISABLED

5. Configure ServerGameManager:
   - Max Turns: 50
   - Turn Time Limit: 60
   - Starting Money: 1000
   - Min Players To Start: 2
   - Game Start Delay: 5
   - Board Length: 36
```

### **Step 7: Save Scene** (3 phút)

```
Ctrl+S → Save GameScene
✅ Hour 2 Complete!
```

---

## 📋 HOUR 3: BUILD & TEST SERVER (2:00-3:00)

### **Step 1: Configure Build Settings** (10 phút)

```
File → Build Settings

Platform: Windows, Mac, Linux
✅ Switch Platform (nếu chưa)

Scenes in Build:
✅ Add: Assets/Scenes/GameScene.unity
❌ Remove: LoginScene, MenuScene (server không cần)

Architecture: x86_64
```

### **Step 2: Configure Player Settings** (10 phút)

```
Build Settings → Player Settings

Other Settings:
✅ Server Build: ENABLED
✅ Scripting Backend: IL2CPP
✅ API Compatibility: .NET Standard 2.1

Quality Settings:
✅ Quality Level: Very Low (server không cần graphics)
```

### **Step 3: Build Server** (15 phút)

```
Unity Menu → Build → Build Dedicated Server (Windows)

Hoặc:
Build Settings → Build

Đợi build (~10-15 phút)
Output: Builds/Server_Windows_[timestamp]/AntKnowServer.exe
```

### **Step 4: Test Server Locally** (15 phút)

```
1. Mở folder: Builds/Server_Windows_[timestamp]/
2. Double-click: RunServer.bat

Hoặc CMD:
cd Builds/Server_Windows_[timestamp]
AntKnowServer.exe -batchmode -nographics -logFile server.log

3. Check server.log:
✅ [ServerBootstrap] Dedicated Server Mode Detected
✅ [ServerBootstrap] Server listening on 0.0.0.0:7777
✅ [ServerBootstrap] ✅ SERVER STARTED SUCCESSFULLY
```

### **Step 5: Verify Server Running** (5 phút)

```
Check logs:
tail -f server.log

Hoặc Windows:
Get-Content server.log -Wait

Expected:
[ServerBootstrap] Waiting for clients to connect...
[ServerBootstrap] --- Server Status ---
[ServerBootstrap] Connected Players: 0/4
```

### **Step 6: Test Port** (5 phút)

```
Windows:
netstat -an | findstr 7777

Linux/Mac:
netstat -an | grep 7777

Expected:
TCP    0.0.0.0:7777    0.0.0.0:0    LISTENING
```

**✅ Hour 3 Complete! Server đang chạy!**

---

## 📋 HOUR 4: BUILD CLIENT (3:00-4:00)

### **Step 1: Switch to Main Project** (5 phút)

```
1. Unity Hub
2. Open: "Project Game AntKnow" (main project)
3. Đợi load
```

### **Step 2: Create Client Connection Script** (20 phút)

Create: `Assets/Script/Client/ClientConnectionManager.cs`

```csharp
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ClientConnectionManager : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private ushort serverPort = 7777;

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Button connectButton;
    [SerializeField] private UnityEngine.UI.InputField ipInputField;

    private void Start()
    {
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(ConnectToServer);
        }

        if (ipInputField != null)
        {
            ipInputField.text = serverIP;
        }
    }

    public void ConnectToServer()
    {
        // Get IP from input field
        if (ipInputField != null)
        {
            serverIP = ipInputField.text;
        }

        Debug.Log($"[Client] Connecting to {serverIP}:{serverPort}...");

        // Configure transport
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(serverIP, serverPort);

        // Start client
        bool started = NetworkManager.Singleton.StartClient();
        
        if (started)
        {
            Debug.Log("[Client] ✅ Connection initiated");
        }
        else
        {
            Debug.LogError("[Client] ❌ Failed to start client");
        }
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        Debug.Log("[Client] Disconnected");
    }
}
```

### **Step 3: Add to MenuScene** (10 phút)

```
1. Open MenuScene.unity
2. Create UI:
   - Canvas → Panel "ConnectionPanel"
   - InputField: "IP Address" (default: 127.0.0.1)
   - Button: "Connect to Server"

3. Add ClientConnectionManager to Canvas
4. Assign references:
   - Connect Button → Button
   - IP Input Field → InputField
```

### **Step 4: Configure Build Settings** (5 phút)

```
File → Build Settings

Scenes:
✅ LoginScene
✅ MenuScene
✅ GameScene

Player Settings:
❌ Server Build: DISABLED (client build)
```

### **Step 5: Build Client** (15 phút)

```
Build Settings → Build

Output: Builds/Client_Windows/AntKnow.exe

Đợi build (~10-15 phút)
```

**✅ Hour 4 Complete! Client đã build!**

---

## 📋 HOUR 5: FULL MULTIPLAYER TEST (4:00-5:00)

### **Test 1: Local Connection** (20 phút)

```
Terminal 1: Server
cd Builds/Server_Windows_[timestamp]
RunServer.bat

Terminal 2: Client 1
cd Builds/Client_Windows
AntKnow.exe

Terminal 3: Client 2
cd Builds/Client_Windows
AntKnow.exe
```

**Expected:**
```
Server log:
[ServerBootstrap] ✅ Client 1000 APPROVED. Players: 1/4
[ServerBootstrap] ✅ Client 1001 APPROVED. Players: 2/4
[ServerGameManager] Enough players (2/2). Starting game in 5s...
[ServerGameManager] ========== STARTING GAME ==========
[ServerGameManager] ===== TURN 1: Player 1 =====

Client 1 log:
[Client] ✅ Connection initiated
[Client] 🎮 GAME STARTED!
[Client] 🎲 Turn started for Player 1

Client 2 log:
[Client] ✅ Connection initiated
[Client] 🎮 GAME STARTED!
[Client] 🎲 Turn started for Player 1
```

### **Test 2: Dice Roll** (15 phút)

```
Client 1: Click "Roll Dice" button

Expected:
Server: [ServerGameManager] Player 1 rolling dice...
Server: [ServerGameManager] Dice: 3 + 4 = 7
Server: [ServerGameManager] Player 1 moved: 0 → 7

Client 1: [Client] 🎲 Player 1 rolled: 3 + 4 = 7
Client 2: [Client] 🎲 Player 1 rolled: 3 + 4 = 7
```

### **Test 3: Turn System** (10 phút)

```
Verify:
✅ Turn 1: Player 1 can roll
✅ Turn 2: Player 2 can roll
✅ Turn 3: Player 1 again
✅ Turns cycle correctly
```

### **Test 4: Disconnect Handling** (10 phút)

```
1. Close Client 2
2. Check server log:
   [ServerGameManager] Client 1001 disconnected
   [ServerGameManager] Player disconnected during game!

3. Game should continue or pause
```

### **Test 5: Performance Check** (5 phút)

```
Server:
✅ CPU: <10%
✅ RAM: <500MB
✅ FPS: ~30

Client:
✅ Smooth movement
✅ No lag
✅ Responsive UI
```

**✅ Hour 5 Complete! Multiplayer hoạt động!**

---

## 🎉 SUCCESS CHECKLIST

```
✅ Server builds successfully
✅ Server starts in headless mode
✅ Server listens on port 7777
✅ Client builds successfully
✅ Client connects to server
✅ 2+ clients can connect
✅ Game starts with 2 players
✅ Turn system works
✅ Dice rolling syncs
✅ Player movement syncs
✅ Disconnect handled gracefully
```

---

## 🚀 NEXT STEPS (Days 2-4)

### **Day 2: Core Gameplay**
- Implement property buy/rent
- Implement money sync
- Implement tile resolution

### **Day 3: Advanced Features**
- House/hotel system
- Special tiles
- Card system

### **Day 4: Polish & Deploy**
- Bug fixes
- Cloud deployment
- Final testing

---

## 🆘 TROUBLESHOOTING

### **Server không start**
```
Check:
1. Port 7777 có bị chiếm không? (netstat -an | findstr 7777)
2. Firewall có block không?
3. Check server.log có error gì
```

### **Client không connect được**
```
Check:
1. Server có đang chạy không?
2. IP address đúng chưa?
3. Port đúng chưa? (7777)
4. Firewall có block không?
```

### **Game không start**
```
Check:
1. Đủ 2 players chưa?
2. Check server log: "Starting game in 5s..."
3. ServerGameManager có trong scene không?
```

---

**BẠN ĐÃ HOÀN THÀNH! Server + Client multiplayer đang chạy! 🎉**

