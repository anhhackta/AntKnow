# 🧹 CLEAN SERVER SETUP - UNITY 6 + MULTIPLAY

**Dọn dẹp project server, build Linux cho Unity Multiplay**

---

## 🎯 MỤC TIÊU

```
✅ Project server gọn nhẹ (chỉ giữ server essentials)
✅ Build Linux server cho Unity Multiplay
✅ Cấu hình đúng cho Unity 6000.0.48f1
✅ Không cần Headless Mode checkbox (Unity 6 tự động)
✅ Upload lên Multiplay hosting
```

---

## 📋 BƯỚC 1: DỌN DẸP PROJECT (15 phút)

### **1.1. Xóa các file/folder không cần thiết**

#### **Xóa trong Assets/**
```
Assets/
├── Scenes/
│   ├── ❌ LoginScene.unity (xóa - client only)
│   ├── ❌ MenuScene.unity (xóa - client only)
│   └── ✅ GameScene.unity (GIỮ LẠI)
│
├── Prefabs/
│   ├── ❌ UI Prefabs (xóa - client only)
│   └── ✅ NetworkPlayer.prefab (GIỮ LẠI)
│
├── Script/
│   ├── ✅ Server/ (GIỮ LẠI)
│   ├── ✅ Domain/ (GIỮ LẠI)
│   ├── ❌ UI/ (xóa - client only)
│   ├── ❌ Presentation/ (xóa - client only)
│   └── ❌ Client/ (xóa - client only)
│
├── ❌ Art/ (xóa - server không cần graphics)
├── ❌ Audio/ (xóa - server không cần audio)
├── ❌ Animations/ (xóa - server không cần)
└── ❌ Materials/ (xóa - server không cần)
```

#### **Giữ lại chỉ:**
```
Assets/
├── Scenes/
│   └── GameScene.unity ✅
├── Prefabs/
│   └── NetworkPlayer.prefab ✅
├── Script/
│   ├── Server/
│   │   ├── ServerBootstrap.cs ✅
│   │   └── ServerGameManager.cs ✅
│   └── Domain/
│       ├── Entities/
│       │   ├── GameState.cs ✅
│       │   ├── PlayerState.cs ✅
│       │   └── PropertyState.cs ✅
│       └── Services/
│           ├── TurnSystem.cs ✅
│           ├── BoardRules.cs ✅
│           └── PropertyEconomy.cs ✅
└── Editor/
    └── ServerBuilder.cs ✅
```

### **1.2. Xóa Packages không cần thiết**

```
Window → Package Manager → Packages: In Project

Xóa:
❌ TextMeshPro (server không cần UI)
❌ UI Toolkit (server không cần UI)
❌ Visual Effect Graph (server không cần VFX)
❌ Shader Graph (server không cần shaders)
❌ Cinemachine (server không cần camera)

Giữ lại:
✅ Netcode for GameObjects (2.5.1)
✅ Unity Transport (2.x)
✅ Dedicated Server (1.6.1)
✅ Multiplayer Tools (optional - for debugging)
```

---

## 📋 BƯỚC 2: CẤU HÌNH UNITY 6 SERVER BUILD (20 phút)

### **2.1. Build Settings**

```
File → Build Settings

Platform: Dedicated Server ✅
Target Platform: Linux ✅
Architecture: x86_64 ✅

Scenes in Build:
✅ Assets/Scenes/GameScene.unity (index 0)
❌ Remove all other scenes
```

### **2.2. Player Settings (Unity 6 - Không có Headless Mode checkbox)**

```
Build Settings → Player Settings

=== Company & Product ===
Company Name: YourCompany
Product Name: AntKnowServer
Version: 1.0.0

=== Icon ===
(Không cần - server không có UI)

=== Resolution and Presentation ===
(Không cần config - Unity 6 tự động headless cho Dedicated Server build)

=== Other Settings ===
✅ Scripting Backend: IL2CPP
✅ API Compatibility Level: .NET Standard 2.1
✅ Active Input Handling: Input System Package (hoặc Both)

❌ KHÔNG CẦN tìm "Headless Mode" checkbox
   → Unity 6 tự động enable khi build Dedicated Server platform

=== Quality ===
Default Quality Level: Very Low ✅
(Server không cần graphics quality cao)

=== Audio ===
Disable Audio: ✅ (optional - server không cần audio)
```

### **2.3. Dedicated Server Settings (Unity 6)**

```
Build Settings → Player Settings → Dedicated Server

Server Build: ✅ Automatically enabled when platform = Dedicated Server

Query Protocol: None
Max Players: 4
Server Tick Rate: 30
```

---

## 📋 BƯỚC 3: TẠO BUILD SCRIPT CHO LINUX (10 phút)

### **3.1. Update ServerBuilder.cs cho Unity 6**

<augment_code_snippet path="Project Game AntKnow Server/Assets/Editor/ServerBuilder.cs" mode="EXCERPT">
````csharp
[MenuItem("Build/Build Linux Server for Multiplay 🐧")]
public static void BuildLinuxServerForMultiplay()
{
    Debug.Log("========== BUILDING LINUX SERVER FOR MULTIPLAY ==========");

    BuildPlayerOptions buildOptions = new BuildPlayerOptions
    {
        scenes = new[] { "Assets/Scenes/GameScene.unity" },
        locationPathName = "Builds/LinuxServer/AntKnowServer.x86_64",
        target = BuildTarget.StandaloneLinux64,
        // Unity 6: Dedicated Server platform tự động headless
        subtarget = (int)StandaloneBuildSubtarget.Server,
        options = BuildOptions.Development // Remove EnableHeadlessMode
    };
````
</augment_code_snippet>

---

## 📋 BƯỚC 4: BUILD LINUX SERVER (15 phút)

### **4.1. Build từ Unity Editor**

```
Unity Menu → Build → Build Linux Server for Multiplay

Hoặc:

File → Build Settings
Platform: Dedicated Server
Target Platform: Linux
Architecture: x86_64
Build
```

### **4.2. Verify Build Output**

```
Builds/LinuxServer/
├── AntKnowServer.x86_64 ✅ (executable)
├── AntKnowServer_Data/ ✅ (game data)
│   ├── Managed/
│   ├── Plugins/
│   ├── Resources/
│   └── ...
└── UnityPlayer.so ✅ (Unity runtime)
```

### **4.3. Tạo run script cho testing local**

Tạo file: `Builds/LinuxServer/run_server.sh`

```bash
#!/bin/bash
echo "========================================="
echo "  AntKnow Dedicated Server - Linux"
echo "========================================="
echo ""

# Make executable
chmod +x AntKnowServer.x86_64

# Run server
./AntKnowServer.x86_64 \
  -batchmode \
  -nographics \
  -logFile server.log \
  -port 7777

echo ""
echo "Server stopped"
```

---

## 📋 BƯỚC 5: UPLOAD LÊN UNITY MULTIPLAY (30 phút)

### **5.1. Chuẩn bị Build cho Multiplay**

#### **Tạo file: build_config.json**
```json
{
  "buildName": "AntKnow Server",
  "buildVersion": "1.0.0",
  "executable": "AntKnowServer.x86_64",
  "queryType": "none",
  "binaryPath": "AntKnowServer.x86_64"
}
```

#### **Zip build folder**
```bash
cd Builds/LinuxServer
zip -r AntKnowServer_Linux_v1.0.0.zip .
```

### **5.2. Unity Dashboard - Tạo Project**

```
1. Mở: https://dashboard.unity3d.com/
2. Select Organization
3. Create New Project (nếu chưa có)
4. Project Name: AntKnow
```

### **5.3. Enable Multiplay Service**

```
1. Dashboard → Select Project "AntKnow"
2. Left Menu → Multiplay
3. Click "Get Started" (nếu chưa enable)
4. Enable Multiplay Service
```

### **5.4. Upload Build**

```
1. Multiplay → Builds → Upload Build
2. Build Name: AntKnow Server v1.0.0
3. Upload: AntKnowServer_Linux_v1.0.0.zip
4. Wait for upload (~5-10 minutes)
```

### **5.5. Configure Build**

```
Build Configuration:
├── Executable Path: AntKnowServer.x86_64
├── Command Line: -batchmode -nographics -logFile server.log -port 7777
├── Query Type: None
├── Server Type: Linux
└── Build ID: (auto-generated)
```

### **5.6. Create Fleet**

```
1. Multiplay → Fleets → Create Fleet
2. Fleet Name: AntKnow Production
3. Select Build: AntKnow Server v1.0.0
4. Regions:
   ✅ Asia Southeast (Singapore) - Gần Việt Nam
   ✅ Asia Northeast (Tokyo) - Optional
5. Fleet Type: Multiplay Hosting
6. Scaling:
   - Min Servers: 1
   - Max Servers: 10
   - Players per Server: 4
7. Machine Type:
   - CPU: 1 vCPU
   - RAM: 2GB
8. Create Fleet
```

### **5.7. Deploy Fleet**

```
1. Fleet → Deploy
2. Wait for deployment (~5-10 minutes)
3. Status: Active ✅
```

---

## 📋 BƯỚC 6: TEST SERVER (15 phút)

### **6.1. Get Server Info**

```
Multiplay Dashboard → Fleets → AntKnow Production
→ Servers → View Active Servers

Server Info:
- IP: <SERVER_IP>
- Port: 7777
- Status: Running
```

### **6.2. Test Connection từ Client**

```
Client (Project Game AntKnow):
1. Open MenuScene
2. ClientConnectionManager:
   - Server IP: <SERVER_IP> (from Multiplay)
   - Port: 7777
3. Click Connect
4. Expected: "Connected!"
```

### **6.3. Monitor Logs**

```
Multiplay Dashboard → Servers → Select Server → Logs

Expected:
[ServerBootstrap] Dedicated Server Mode Detected
[ServerBootstrap] Server listening on 0.0.0.0:7777
[ServerBootstrap] ✅ SERVER STARTED SUCCESSFULLY
[ServerBootstrap] ✅ Client connected
```

---

## 📋 BƯỚC 7: MATCHMAKING (Optional - 20 phút)

### **7.1. Enable Matchmaker Service**

```
Unity Dashboard → Matchmaker → Enable
```

### **7.2. Create Queue**

```
Matchmaker → Queues → Create Queue
- Queue Name: AntKnow Quick Match
- Min Players: 2
- Max Players: 4
- Timeout: 30s
- Fleet: AntKnow Production
```

### **7.3. Client Integration**

```csharp
using Unity.Services.Matchmaker;

async void FindMatch()
{
    var ticket = await MatchmakerService.Instance.CreateTicketAsync(
        new List<Player> { new Player(PlayerId) },
        new MatchmakingOptions { QueueName = "AntKnow Quick Match" }
    );

    var assignment = await PollForMatchAsync(ticket.Id);

    // Connect to assigned server
    ConnectToServer(assignment.Ip, assignment.Port);
}
```

---

## ✅ CHECKLIST HOÀN THÀNH

### **Project Cleanup**
- [ ] Xóa scenes không cần (LoginScene, MenuScene)
- [ ] Xóa UI/Presentation scripts
- [ ] Xóa Art/Audio/Animations
- [ ] Xóa packages không cần (TMPro, UI Toolkit, etc.)
- [ ] Chỉ giữ Server + Domain scripts

### **Build Configuration**
- [ ] Platform: Dedicated Server
- [ ] Target: Linux x86_64
- [ ] Scripting Backend: IL2CPP
- [ ] API: .NET Standard 2.1
- [ ] Scene: Chỉ GameScene

### **Build Success**
- [ ] Build Linux server thành công
- [ ] Output: AntKnowServer.x86_64
- [ ] Size: ~50-100MB (gọn nhẹ)

### **Multiplay Upload**
- [ ] Zip build folder
- [ ] Upload lên Multiplay
- [ ] Configure build settings
- [ ] Create fleet
- [ ] Deploy fleet
- [ ] Status: Active

### **Testing**
- [ ] Server running on Multiplay
- [ ] Client connect được
- [ ] Game starts với 2 players
- [ ] Logs hiển thị đúng

---

## 💰 CHI PHÍ MULTIPLAY

### **Free Tier**
```
✅ 20 CCU (Concurrent Users) miễn phí
✅ Đủ cho testing và small launch
```

### **Paid Tier**
```
$0.50 per CCU/month
Example:
- 100 CCU = $50/month
- 500 CCU = $250/month
```

---

## 🎯 KẾT QUẢ CUỐI CÙNG

```
✅ Project server gọn nhẹ (~50-100MB build)
✅ Linux server build cho Multiplay
✅ Upload lên Unity Multiplay hosting
✅ Auto-scaling (1-10 servers)
✅ Global distribution (Asia Southeast)
✅ Client connect từ anywhere
✅ Production-ready!
```

---

**BẠN ĐÃ SẴN SÀNG DEPLOY! 🚀**