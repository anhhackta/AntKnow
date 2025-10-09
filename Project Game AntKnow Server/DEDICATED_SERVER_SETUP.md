# 🖥️ DEDICATED SERVER SETUP - 5 HOURS PLAN

## ⏱️ HOUR 1: PROJECT SETUP & BUILD SETTINGS (0:00-1:00)

### **Step 1: Configure Build Settings**

#### **1.1. Open Build Settings**
```
File → Build Settings
```

#### **1.2. Add Scenes**
```
Add Open Scenes:
✅ GameScene.unity (Main gameplay scene)

Remove:
❌ LoginScene.unity (Server không cần login)
❌ MenuScene.unity (Server không cần menu)
```

#### **1.3. Platform Settings**
```
Platform: Windows, Mac, Linux
Target Platform: Dedicated Server ✅

Architecture:
- Windows: x86_64
- Linux: x86_64 (for cloud deployment)
```

#### **1.4. Server Build Settings**
```
Build Settings → Player Settings → Other Settings:

✅ Server Build: ENABLED
✅ Headless Mode: ENABLED (no graphics)
✅ Scripting Backend: IL2CPP (faster)
✅ API Compatibility Level: .NET Standard 2.1
```

### **Step 2: Install Dedicated Server Package**

#### **2.1. Package Manager**
```
Window → Package Manager → Unity Registry

Search: "Dedicated Server"
Install: "Dedicated Server" package (1.6.1)
```

#### **2.2. Verify Installation**
```
Packages/manifest.json should have:
"com.unity.dedicated-server": "1.6.1"
```

### **Step 3: Create Server Bootstrap Script**

#### **3.1. Create Script**
```
Location: Assets/Script/Server/ServerBootstrap.cs
```

```csharp
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace AntKnow.Server
{
    /// <summary>
    /// Server Bootstrap - Auto-start server on headless build
    /// </summary>
    public class ServerBootstrap : MonoBehaviour
    {
        [Header("Server Settings")]
        [SerializeField] private ushort serverPort = 7777;
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private bool autoStartServer = true;

        private NetworkManager networkManager;

        private void Awake()
        {
            // Check if running as dedicated server
            if (!Application.isBatchMode && !autoStartServer)
            {
                Debug.Log("[ServerBootstrap] Not running as dedicated server, skipping auto-start");
                return;
            }

            Debug.Log("[ServerBootstrap] Dedicated Server Mode Detected");
            InitializeServer();
        }

        private void InitializeServer()
        {
            networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[ServerBootstrap] NetworkManager not found!");
                return;
            }

            // Configure transport
            var transport = networkManager.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("0.0.0.0", serverPort);
                Debug.Log($"[ServerBootstrap] Server listening on port {serverPort}");
            }

            // Set max connections
            networkManager.NetworkConfig.ConnectionApproval = true;
            networkManager.ConnectionApprovalCallback = ApprovalCheck;

            // Start server
            bool started = networkManager.StartServer();
            if (started)
            {
                Debug.Log($"[ServerBootstrap] ✅ Server started successfully on port {serverPort}");
                Debug.Log($"[ServerBootstrap] Max players: {maxPlayers}");
            }
            else
            {
                Debug.LogError("[ServerBootstrap] ❌ Failed to start server!");
            }
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            // Check max players
            if (networkManager.ConnectedClients.Count >= maxPlayers)
            {
                response.Approved = false;
                response.Reason = "Server full";
                Debug.LogWarning($"[ServerBootstrap] Connection rejected: Server full ({maxPlayers}/{maxPlayers})");
                return;
            }

            // Approve connection
            response.Approved = true;
            response.CreatePlayerObject = true;
            Debug.Log($"[ServerBootstrap] ✅ Client approved. Players: {networkManager.ConnectedClients.Count + 1}/{maxPlayers}");
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[ServerBootstrap] Server shutting down...");
        }
    }
}
```

#### **3.2. Add to GameScene**
```
1. Open GameScene.unity
2. Create Empty GameObject: "ServerBootstrap"
3. Add Component: ServerBootstrap.cs
4. Configure:
   - Server Port: 7777
   - Max Players: 4
   - Auto Start Server: ✅
```

### **Step 4: Configure NetworkManager for Server**

#### **4.1. Find NetworkManager in GameScene**
```
Hierarchy → NetworkManager GameObject
```

#### **4.2. Configure NetworkManager**
```
Inspector → NetworkManager:

✅ Don't Destroy On Load: ENABLED
✅ Run In Background: ENABLED

Network Transport: UnityTransport
  - Protocol Type: UnityTransport
  - Connection Data:
    * Address: 0.0.0.0 (listen all interfaces)
    * Port: 7777
    * Server Listen Address: 0.0.0.0
```

#### **4.3. Configure Player Prefab**
```
NetworkManager → Player Prefab:
✅ Assign: Assets/Prefabs/NetworkPlayer.prefab

Make sure NetworkPlayer.prefab has:
✅ NetworkObject component
✅ NetworkTransform component
✅ NetworkPlayerController script
```

---

## ⏱️ HOUR 2: CONFIGURE NETCODE SERVER-AUTHORITATIVE (1:00-2:00)

### **Step 1: Create Server-Only GameManager**

#### **1.1. Modify GameManager for Server**

Create: `Assets/Script/Server/ServerGameManager.cs`

```csharp
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace AntKnow.Server
{
    /// <summary>
    /// Server-Authoritative Game Manager
    /// Runs ONLY on server, controls all game logic
    /// </summary>
    public class ServerGameManager : NetworkBehaviour
    {
        public static ServerGameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int maxTurns = 50;
        [SerializeField] private float turnTimeLimit = 60f;

        // Server-side game state
        private GameState gameState;
        private int currentPlayerIndex = 0;
        private bool gameActive = false;

        // Network Variables (Server → Client sync)
        private NetworkVariable<int> currentTurn = new NetworkVariable<int>(1);
        private NetworkVariable<int> currentPlayerTurn = new NetworkVariable<int>(0);

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            Instance = this;
            Debug.Log("[ServerGameManager] Server spawned");

            // Listen for player connections
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[ServerGameManager] Client {clientId} connected");

            // Check if we have enough players to start
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && !gameActive)
            {
                // Start game after 5 seconds
                Invoke(nameof(StartGame), 5f);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[ServerGameManager] Client {clientId} disconnected");

            // Handle player disconnect
            if (gameActive)
            {
                // Pause game or end game
                Debug.LogWarning("[ServerGameManager] Player disconnected during game!");
            }
        }

        private void StartGame()
        {
            if (!IsServer) return;

            Debug.Log("[ServerGameManager] Starting game...");
            gameActive = true;

            // Initialize game state
            InitializeGameState();

            // Notify all clients
            NotifyGameStartClientRpc();

            // Start first turn
            StartNextTurn();
        }

        private void InitializeGameState()
        {
            // Create domain GameState
            gameState = new GameState
            {
                BoardLength = 36,
                CurrentTurnPlayerId = 1
            };

            // Create player states for each connected client
            int playerId = 1;
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                var playerState = new PlayerState
                {
                    Id = playerId,
                    Money = 1000,
                    NodeIndex = 0,
                    JailTurns = 0,
                    Health = 100,
                    Agility = 100,
                    Intelligence = 100,
                    Luck = 100,
                    Resistance = 100
                };

                gameState.Players.Add(playerState);
                playerId++;
            }

            Debug.Log($"[ServerGameManager] Game initialized with {gameState.Players.Count} players");
        }

        private void StartNextTurn()
        {
            if (!IsServer || !gameActive) return;

            currentPlayerIndex = (currentPlayerIndex + 1) % gameState.Players.Count;
            currentPlayerTurn.Value = currentPlayerIndex;

            Debug.Log($"[ServerGameManager] Turn {currentTurn.Value}: Player {currentPlayerIndex + 1}");

            // Notify clients
            NotifyTurnStartClientRpc(currentPlayerIndex);
        }

        [ClientRpc]
        private void NotifyGameStartClientRpc()
        {
            Debug.Log("[Client] Game started!");
            // Client-side: Show game UI, hide lobby
        }

        [ClientRpc]
        private void NotifyTurnStartClientRpc(int playerIndex)
        {
            Debug.Log($"[Client] Turn started for player {playerIndex + 1}");
            // Client-side: Enable/disable controls based on turn
        }

        // Server RPC: Client requests to roll dice
        [ServerRpc(RequireOwnership = false)]
        public void RequestRollDiceServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            Debug.Log($"[ServerGameManager] Client {clientId} requested dice roll");

            // Validate: Is it this player's turn?
            // TODO: Check if clientId matches current player

            // Roll dice (server-authoritative)
            int dice1 = Random.Range(1, 7);
            int dice2 = Random.Range(1, 7);
            int total = dice1 + dice2;

            Debug.Log($"[ServerGameManager] Dice rolled: {dice1} + {dice2} = {total}");

            // Update player position
            var player = gameState.Players[currentPlayerIndex];
            int oldPosition = player.NodeIndex;
            player.NodeIndex = (player.NodeIndex + total) % gameState.BoardLength;

            Debug.Log($"[ServerGameManager] Player moved from {oldPosition} to {player.NodeIndex}");

            // Notify all clients
            NotifyDiceRollClientRpc(dice1, dice2, player.NodeIndex);

            // Resolve tile
            ResolveTile(player);
        }

        [ClientRpc]
        private void NotifyDiceRollClientRpc(int dice1, int dice2, int newPosition)
        {
            Debug.Log($"[Client] Dice: {dice1} + {dice2}, New position: {newPosition}");
            // Client-side: Animate dice, move player
        }

        private void ResolveTile(PlayerState player)
        {
            // TODO: Implement tile resolution logic
            Debug.Log($"[ServerGameManager] Resolving tile {player.NodeIndex} for player {player.Id}");

            // For now, just end turn after 2 seconds
            Invoke(nameof(EndCurrentTurn), 2f);
        }

        private void EndCurrentTurn()
        {
            if (!IsServer) return;

            currentTurn.Value++;

            // Check end game conditions
            if (currentTurn.Value >= maxTurns)
            {
                EndGame();
                return;
            }

            // Start next turn
            StartNextTurn();
        }

        private void EndGame()
        {
            Debug.Log("[ServerGameManager] Game ended!");
            gameActive = false;

            // Calculate scores
            // TODO: Implement scoring

            // Notify clients
            NotifyGameEndClientRpc();
        }

        [ClientRpc]
        private void NotifyGameEndClientRpc()
        {
            Debug.Log("[Client] Game ended!");
            // Client-side: Show results panel
        }
    }
}
```

### **Step 2: Add ServerGameManager to Scene**

```
1. Open GameScene.unity
2. Create Empty GameObject: "ServerGameManager"
3. Add Component: ServerGameManager.cs
4. Add Component: NetworkObject
5. Configure NetworkObject:
   ✅ Don't Destroy With Owner: ENABLED
   ✅ Destroy With Scene: DISABLED
```

---

## ⏱️ HOUR 3: IMPLEMENT CORE SERVER LOGIC (2:00-3:00)

### **Step 1: Create Server Build Configuration**

Create: `Assets/Script/Server/ServerConfig.cs`

```csharp
using UnityEngine;

namespace AntKnow.Server
{
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "AntKnow/Server Config")]
    public class ServerConfig : ScriptableObject
    {
        [Header("Network Settings")]
        public ushort port = 7777;
        public int maxPlayers = 4;
        public string serverName = "AntKnow Server";

        [Header("Game Settings")]
        public int maxTurns = 50;
        public float turnTimeLimit = 60f;
        public int startingMoney = 1000;

        [Header("Performance")]
        public int targetFrameRate = 30; // Server doesn't need 60fps
        public bool enableLogs = true;
    }
}
```

### **Step 2: Optimize Server Performance**

Create: `Assets/Script/Server/ServerOptimizer.cs`

```csharp
using UnityEngine;

namespace AntKnow.Server
{
    /// <summary>
    /// Optimize server performance for headless build
    /// </summary>
    public class ServerOptimizer : MonoBehaviour
    {
        [SerializeField] private ServerConfig config;

        private void Awake()
        {
            if (!Application.isBatchMode) return;

            OptimizeServer();
        }

        private void OptimizeServer()
        {
            // Set target frame rate (server doesn't need 60fps)
            Application.targetFrameRate = config.targetFrameRate;

            // Disable VSync
            QualitySettings.vSyncCount = 0;

            // Set low quality (no graphics needed)
            QualitySettings.SetQualityLevel(0, true);

            // Disable audio
            AudioListener.volume = 0;

            Debug.Log("[ServerOptimizer] Server optimized for headless mode");
        }
    }
}
```

---

## ⏱️ HOUR 4: BUILD & TEST SERVER (3:00-4:00)

### **Step 1: Create Build Script**

Create: `Assets/Editor/ServerBuilder.cs`

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;

public class ServerBuilder
{
    [MenuItem("Build/Build Dedicated Server (Windows)")]
    public static void BuildWindowsServer()
    {
        BuildServer(BuildTarget.StandaloneWindows64, "Windows");
    }

    [MenuItem("Build/Build Dedicated Server (Linux)")]
    public static void BuildLinuxServer()
    {
        BuildServer(BuildTarget.StandaloneLinux64, "Linux");
    }

    private static void BuildServer(BuildTarget target, string platformName)
    {
        string buildPath = $"Builds/Server_{platformName}/AntKnowServer";
        if (target == BuildTarget.StandaloneWindows64)
        {
            buildPath += ".exe";
        }

        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName = buildPath,
            target = target,
            options = BuildOptions.EnableHeadlessMode | BuildOptions.Development
        };

        // Perform build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Server build succeeded: {buildPath}");
            Debug.Log($"Size: {report.summary.totalSize / (1024 * 1024)} MB");
        }
        else
        {
            Debug.LogError($"❌ Server build failed!");
        }
    }
}
```

### **Step 2: Build Server**

```
1. Unity Editor → Build → Build Dedicated Server (Windows)
2. Wait for build to complete (~5-10 minutes)
3. Output: Builds/Server_Windows/AntKnowServer.exe
```

### **Step 3: Test Server Locally**

```bash
# Run server
cd Builds/Server_Windows
./AntKnowServer.exe -batchmode -nographics -logFile server.log

# Check logs
tail -f server.log
```

Expected output:
```
[ServerBootstrap] Dedicated Server Mode Detected
[ServerBootstrap] Server listening on port 7777
[ServerBootstrap] ✅ Server started successfully on port 7777
```

---

## ⏱️ HOUR 5: DEPLOY & TEST CLIENT CONNECTION (4:00-5:00)

### **Step 1: Build Client**

```
1. Switch to "Project Game AntKnow" (main project)
2. File → Build Settings
3. Platform: Windows
4. Scenes:
   ✅ LoginScene
   ✅ MenuScene
   ✅ GameScene
5. Build Settings → Player Settings:
   ❌ Server Build: DISABLED
6. Build → Builds/Client_Windows/AntKnow.exe
```

### **Step 2: Configure Client to Connect to Server**

Edit: `Assets/Script/Services/RelayService.cs` or create new connection script

```csharp
// In client, connect to server IP
var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
transport.SetConnectionData("127.0.0.1", 7777); // Localhost for testing
NetworkManager.Singleton.StartClient();
```

### **Step 3: Test Connection**

```
Terminal 1: Run Server
cd Builds/Server_Windows
./AntKnowServer.exe -batchmode -nographics

Terminal 2: Run Client 1
cd Builds/Client_Windows
./AntKnow.exe

Terminal 3: Run Client 2
cd Builds/Client_Windows
./AntKnow.exe
```

Expected:
```
Server log:
[ServerBootstrap] Client approved. Players: 1/4
[ServerBootstrap] Client approved. Players: 2/4
[ServerGameManager] Starting game...
```

---

## 📦 DEPLOYMENT OPTIONS

### **Option 1: Local Network (LAN)**
```
1. Run server on one PC
2. Get server IP: ipconfig (Windows) or ifconfig (Linux)
3. Clients connect to: <SERVER_IP>:7777
```

### **Option 2: Cloud Server (AWS/GCP/Azure)**
```
1. Rent VPS (e.g., AWS EC2 t3.medium)
2. Upload server build
3. Open port 7777 in firewall
4. Run: ./AntKnowServer.exe -batchmode -nographics
5. Clients connect to: <PUBLIC_IP>:7777
```

### **Option 3: Unity Multiplay (Recommended)**
```
1. Unity Dashboard → Multiplay
2. Upload server build
3. Configure fleet
4. Auto-scaling, matchmaking included
5. Clients connect via Relay
```

---

## ✅ CHECKLIST - 5 HOURS

```
Hour 1: ✅ Setup Project & Build Settings
  ✅ Configure Dedicated Server build
  ✅ Create ServerBootstrap.cs
  ✅ Configure NetworkManager

Hour 2: ✅ Server-Authoritative Logic
  ✅ Create ServerGameManager.cs
  ✅ Implement turn system
  ✅ Implement ServerRpc/ClientRpc

Hour 3: ✅ Core Server Logic
  ✅ Create ServerConfig
  ✅ Optimize server performance
  ✅ Implement game state sync

Hour 4: ✅ Build & Test
  ✅ Create build script
  ✅ Build server executable
  ✅ Test server locally

Hour 5: ✅ Deploy & Client Connection
  ✅ Build client
  ✅ Test client-server connection
  ✅ Verify multiplayer works
```

---

## 🚀 NEXT STEPS (Days 2-4)

### **Day 2: Core Gameplay**
- Implement dice rolling sync
- Implement player movement sync
- Implement property buy/rent

### **Day 3: Advanced Features**
- Implement house/hotel system
- Implement special tiles
- Implement turn timer

### **Day 4: Polish & Testing**
- Bug fixes
- Performance optimization
- Full multiplayer testing

---

**Bạn đã sẵn sàng! Bắt đầu từ Hour 1 ngay bây giờ! 🚀**

