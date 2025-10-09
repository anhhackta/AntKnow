# 🚀 MULTIPLAY QUICK START - 1 TIẾNG

**Build Linux server + Upload lên Unity Multiplay trong 1 tiếng**

---

## ⏱️ TIMELINE

```
✅ 0-15 phút: Cleanup project
✅ 15-30 phút: Build Linux server
✅ 30-45 phút: Upload to Multiplay
✅ 45-60 phút: Deploy & Test
```

---

## 📋 BƯỚC 1: CLEANUP PROJECT (0-15 phút)

### **Option A: Tự động (Recommended)**

```
1. Unity Editor → Tools → Clean Server Project 🧹
2. Check "Tôi hiểu và muốn tiếp tục"
3. Click "Preview Files to Delete" (xem trước)
4. Click "CLEAN PROJECT"
5. Confirm
6. Done! ✅
```

### **Option B: Thủ công**

```
Xóa các folder sau trong Assets/:
❌ Scenes/LoginScene.unity
❌ Scenes/MenuScene.unity
❌ Script/UI/
❌ Script/Presentation/
❌ Script/Client/
❌ Art/
❌ Audio/
❌ Animations/
❌ Materials/
❌ Prefabs/UI/

Giữ lại:
✅ Scenes/GameScene.unity
✅ Script/Server/
✅ Script/Domain/
✅ Prefabs/NetworkPlayer.prefab
```

### **Xóa Packages không cần**

```
Window → Package Manager → Packages: In Project

Xóa:
❌ TextMeshPro
❌ UI Toolkit
❌ Visual Effect Graph
❌ Shader Graph
❌ Cinemachine

Giữ:
✅ Netcode for GameObjects
✅ Unity Transport
✅ Dedicated Server
```

**✅ Checkpoint**: Project size giảm đáng kể, chỉ còn server essentials

---

## 📋 BƯỚC 2: BUILD LINUX SERVER (15-30 phút)

### **2.1. Configure Build Settings**

```
File → Build Settings

Platform: Dedicated Server ✅
Target Platform: Linux ✅
Architecture: x86_64 ✅

Scenes in Build:
✅ GameScene.unity (index 0)
❌ Remove all others
```

### **2.2. Configure Player Settings**

```
Build Settings → Player Settings

Other Settings:
✅ Scripting Backend: IL2CPP
✅ API Compatibility: .NET Standard 2.1

Quality:
✅ Default Quality: Very Low

NOTE: Unity 6 không có "Headless Mode" checkbox
→ Tự động enable khi build Dedicated Server platform
```

### **2.3. Build**

```
Unity Menu → Build → Build Linux Server for Multiplay 🚀

Hoặc:

File → Build Settings → Build

Wait: ~10-15 phút
```

### **2.4. Verify Output**

```
Builds/LinuxServer/
├── AntKnowServer.x86_64 ✅
├── AntKnowServer_Data/ ✅
├── UnityPlayer.so ✅
├── run_server.sh ✅
├── build_config.json ✅
└── UPLOAD_TO_MULTIPLAY.txt ✅
```

**✅ Checkpoint**: Build thành công, size ~50-100MB

---

## 📋 BƯỚC 3: UPLOAD TO MULTIPLAY (30-45 phút)

### **3.1. Zip Build Folder**

#### **Windows:**
```
1. Mở: Builds/LinuxServer/
2. Select all files
3. Right-click → Send to → Compressed (zipped) folder
4. Name: AntKnowServer_Linux_v1.0.0.zip
```

#### **Linux/Mac:**
```bash
cd Builds
zip -r AntKnowServer_Linux_v1.0.0.zip LinuxServer/
```

**Size**: ~50-100MB

### **3.2. Unity Dashboard**

```
1. Mở: https://dashboard.unity3d.com/
2. Login với Unity account
3. Select Organization
4. Select Project (hoặc Create New)
```

### **3.3. Enable Multiplay**

```
1. Left Menu → Multiplay
2. Click "Get Started" (nếu chưa enable)
3. Enable Multiplay Service
4. Accept terms
```

### **3.4. Upload Build**

```
1. Multiplay → Builds → Upload Build
2. Click "Upload Build"
3. Build Name: AntKnow Server v1.0.0
4. Select file: AntKnowServer_Linux_v1.0.0.zip
5. Upload
6. Wait: ~5-10 phút (tùy internet speed)
```

### **3.5. Configure Build**

```
Build Configuration:
├── Executable Path: AntKnowServer.x86_64
├── Command Line: -batchmode -nographics -logFile server.log -port 7777
├── Query Type: None
├── Server Type: Linux
└── Save
```

**✅ Checkpoint**: Build uploaded thành công

---

## 📋 BƯỚC 4: DEPLOY FLEET (45-60 phút)

### **4.1. Create Fleet**

```
1. Multiplay → Fleets → Create Fleet
2. Fleet Name: AntKnow Production
3. Select Build: AntKnow Server v1.0.0
```

### **4.2. Configure Fleet**

```
Regions:
✅ Asia Southeast (Singapore) - Gần Việt Nam nhất
⏸️ Asia Northeast (Tokyo) - Optional
⏸️ US West - Optional

Fleet Type: Multiplay Hosting

Scaling:
├── Min Servers: 1
├── Max Servers: 10
└── Players per Server: 4

Machine Type:
├── CPU: 1 vCPU (đủ cho 4 players)
├── RAM: 2GB
└── Disk: 10GB
```

### **4.3. Deploy**

```
1. Review configuration
2. Click "Create Fleet"
3. Click "Deploy"
4. Wait: ~5-10 phút
5. Status: Active ✅
```

### **4.4. Get Server Info**

```
Fleets → AntKnow Production → Servers

Server Info:
├── IP: <SERVER_IP> (copy this)
├── Port: 7777
├── Status: Running
└── Region: Asia Southeast
```

**✅ Checkpoint**: Server đang chạy trên Multiplay!

---

## 📋 BƯỚC 5: TEST CONNECTION (55-60 phút)

### **5.1. Update Client**

```
Project Game AntKnow:
1. Open MenuScene
2. ClientConnectionManager:
   - Default Server IP: <SERVER_IP> (from Multiplay)
   - Server Port: 7777
3. Save scene
4. Build client (optional)
```

### **5.2. Test Connection**

```
1. Run client (Editor hoặc Build)
2. Enter Server IP: <SERVER_IP>
3. Click "Connect"
4. Expected: "Connected!" ✅
```

### **5.3. Monitor Logs**

```
Multiplay Dashboard:
1. Fleets → AntKnow Production
2. Servers → Select server
3. Logs tab

Expected:
[ServerBootstrap] Dedicated Server Mode Detected
[ServerBootstrap] Server listening on 0.0.0.0:7777
[ServerBootstrap] ✅ SERVER STARTED SUCCESSFULLY
[ServerBootstrap] ✅ Client 1000 APPROVED
```

### **5.4. Test Multiplayer**

```
1. Run 2 clients
2. Both connect to <SERVER_IP>:7777
3. Game starts with 2 players
4. Test turn system
5. Test dice rolling
```

**✅ Checkpoint**: Multiplayer hoạt động trên cloud!

---

## ✅ SUCCESS CHECKLIST

```
✅ Project cleaned (chỉ server essentials)
✅ Linux server built (~50-100MB)
✅ Build uploaded to Multiplay
✅ Fleet created and deployed
✅ Server running (Status: Active)
✅ Client can connect from anywhere
✅ Multiplayer works (2-4 players)
✅ Logs show no errors
```

---

## 💰 CHI PHÍ

### **Free Tier**
```
✅ 20 CCU miễn phí
✅ Đủ cho testing
✅ Đủ cho soft launch (small audience)
```

### **Paid Tier**
```
$0.50 per CCU/month

Examples:
- 50 CCU = $25/month
- 100 CCU = $50/month
- 500 CCU = $250/month
```

---

## 🎯 NEXT STEPS

### **Option A: Test & Iterate**
```
1. Test với nhiều players
2. Monitor performance
3. Fix bugs
4. Update build (re-upload)
```

### **Option B: Add Matchmaking**
```
1. Enable Matchmaker service
2. Create queue
3. Integrate client
4. Auto-match players
```

### **Option C: Scale Up**
```
1. Increase max servers (10 → 50)
2. Add more regions (US, EU)
3. Upgrade machine type (2GB → 4GB)
4. Enable auto-scaling
```

---

## 🐛 TROUBLESHOOTING

### **Build upload fails**
```
✅ Check file size (<500MB)
✅ Check internet connection
✅ Try again (sometimes timeout)
✅ Use smaller build (remove debug symbols)
```

### **Server won't start**
```
✅ Check executable path: AntKnowServer.x86_64
✅ Check command line: -batchmode -nographics -logFile server.log -port 7777
✅ Check logs in Multiplay dashboard
✅ Rebuild with Development build
```

### **Client can't connect**
```
✅ Check server IP (copy from Multiplay)
✅ Check port: 7777
✅ Check server status: Running
✅ Check client firewall
```

### **Game won't start**
```
✅ Check min 2 players connected
✅ Check server logs
✅ Check ServerGameManager in scene
✅ Rebuild server
```

---

## 📊 MONITORING

### **Multiplay Dashboard**
```
Metrics:
├── Active Servers
├── Connected Players
├── CPU Usage
├── RAM Usage
└── Network Traffic

Logs:
├── Server logs (real-time)
├── Error logs
└── Connection logs
```

### **Alerts** (Optional)
```
Setup alerts for:
├── Server crashes
├── High CPU usage
├── High RAM usage
└── Connection failures
```

---

## 🎉 CONGRATULATIONS!

**Bạn đã:**
```
✅ Clean server project
✅ Build Linux server
✅ Upload to Unity Multiplay
✅ Deploy fleet
✅ Test multiplayer
✅ Server running 24/7 on cloud
✅ Players can connect from anywhere
✅ Auto-scaling enabled
```

**Total time**: ~1 hour
**Result**: Production-ready multiplayer server! 🚀

---

**BẠN ĐÃ HOÀN THÀNH! GAME ĐANG LIVE! 🎉**

