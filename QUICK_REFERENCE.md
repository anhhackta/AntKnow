# ⚡ QUICK REFERENCE - ANTKNOW MULTIPLAYER

**Tham khảo nhanh cho các tác vụ thường dùng**

---

## 🚀 BUILD SERVER

### **Windows**
```
Unity Menu → Build → Build Dedicated Server (Windows)
Output: Builds/Server_Windows_[timestamp]/AntKnowServer.exe
```

### **Linux**
```
Unity Menu → Build → Build Dedicated Server (Linux)
Output: Builds/Server_Linux_[timestamp]/AntKnowServer
```

### **All Platforms**
```
Unity Menu → Build → Build All Servers (Win + Linux)
```

---

## 🏃 RUN SERVER

### **Windows**
```bash
cd Builds/Server_Windows_[timestamp]
RunServer.bat
```

### **Linux**
```bash
cd Builds/Server_Linux_[timestamp]
chmod +x run_server.sh
./run_server.sh
```

### **Manual**
```bash
# Windows
AntKnowServer.exe -batchmode -nographics -logFile server.log

# Linux
./AntKnowServer -batchmode -nographics -logFile server.log
```

---

## 🔧 CONFIGURATION

### **Server Settings** (ServerBootstrap.cs)
```csharp
serverPort = 7777
maxPlayers = 4
autoStartServer = true
targetFrameRate = 30
```

### **Game Settings** (ServerGameManager.cs)
```csharp
maxTurns = 50
turnTimeLimit = 60f
startingMoney = 1000
minPlayersToStart = 2
gameStartDelay = 5f
boardLength = 36
```

---

## 🧪 TESTING

### **Check Server Running**
```bash
# Windows
netstat -an | findstr 7777

# Linux/Mac
netstat -an | grep 7777

# Expected
TCP    0.0.0.0:7777    0.0.0.0:0    LISTENING
```

### **View Logs**
```bash
# Real-time
tail -f server.log

# Windows
Get-Content server.log -Wait
```

### **Test Connection**
```
Client:
1. Enter IP: 127.0.0.1 (local) or <SERVER_IP> (remote)
2. Port: 7777
3. Click Connect
```

---

## 🐛 TROUBLESHOOTING

### **Server Won't Start**
```
✅ Check port available: netstat -an | grep 7777
✅ Check firewall: Allow port 7777
✅ Check permissions: chmod +x AntKnowServer
✅ Check logs: tail -f server.log
```

### **Client Can't Connect**
```
✅ Server running? netstat -an | grep 7777
✅ Correct IP? ipconfig / ifconfig
✅ Firewall open? Port 7777
✅ Client using correct IP:Port?
```

### **Game Won't Start**
```
✅ Enough players? (min 2)
✅ ServerGameManager in scene?
✅ NetworkManager configured?
✅ Check server logs for errors
```

---

## ☁️ CLOUD DEPLOYMENT

### **AWS EC2**
```
1. Create t3.medium instance
2. Security Group: Allow TCP 7777
3. Upload server build
4. Run: ./run_server.sh
5. Clients connect to: <EC2_PUBLIC_IP>:7777
```

### **Google Cloud**
```
1. Create e2-medium VM
2. Firewall: Allow tcp:7777
3. Upload server build
4. Run: ./run_server.sh
5. Clients connect to: <VM_PUBLIC_IP>:7777
```

### **Unity Multiplay**
```
1. Unity Dashboard → Multiplay
2. Upload server build (Linux)
3. Configure fleet
4. Deploy
5. Use Matchmaker API
```

---

## 📊 MONITORING

### **Server Status**
```bash
# Check process
ps aux | grep AntKnowServer

# Check connections
netstat -an | grep 7777

# Check logs
tail -f server.log
```

### **Performance**
```
CPU: <10% (idle), <30% (4 players)
RAM: ~300-500MB
Network: ~10-50 KB/s per player
FPS: 30 (server)
```

---

## 🔥 COMMON COMMANDS

### **Build**
```
Unity Menu → Build → Build Dedicated Server (Windows)
Unity Menu → Build → Build Dedicated Server (Linux)
Unity Menu → Build → Build All Servers
Unity Menu → Build → Clean Old Builds
Unity Menu → Build → Open Builds Folder
```

### **Server**
```bash
# Start
RunServer.bat (Windows)
./run_server.sh (Linux)

# Stop
Ctrl+C

# Logs
tail -f server.log
```

### **Client**
```bash
# Build
File → Build Settings → Build

# Run
AntKnow.exe (Windows)
./AntKnow (Linux)
```

---

## 📁 FILE LOCATIONS

### **Scripts**
```
Server:
- Assets/Script/Server/ServerBootstrap.cs
- Assets/Script/Server/ServerGameManager.cs
- Assets/Editor/ServerBuilder.cs

Client:
- Assets/Script/Client/ClientConnectionManager.cs

Domain:
- Assets/Script/Domain/Entities/GameState.cs
- Assets/Script/Domain/Entities/PlayerState.cs
- Assets/Script/Domain/Entities/PropertyState.cs
```

### **Builds**
```
Server:
- Builds/Server_Windows_[timestamp]/
- Builds/Server_Linux_[timestamp]/

Client:
- Builds/Client_Windows/
```

### **Logs**
```
Server: Builds/Server_*/server.log
Client: Unity Console
```

---

## 🎯 QUICK TASKS

### **Setup New Server**
```
1. Open Unity: Project Game AntKnow Server
2. Copy scripts to Assets/Script/Server/
3. Configure GameScene
4. Build: Unity Menu → Build → Build Dedicated Server
5. Run: RunServer.bat
```

### **Connect Client**
```
1. Open Unity: Project Game AntKnow
2. Copy ClientConnectionManager.cs
3. Add to MenuScene
4. Build client
5. Run, enter IP, connect
```

### **Deploy to Cloud**
```
1. Create VM (AWS/GCP)
2. Open port 7777
3. Upload server build
4. Run server
5. Update client IP
6. Test connection
```

---

## 📞 SUPPORT

### **Documentation**
- START_HERE.md - Entry point
- QUICK_START_5_HOURS.md - 5h guide
- ACTION_PLAN_3_4_DAYS.md - 3-4 day plan
- DEPLOYMENT_GUIDE.md - Cloud deployment
- README.md - Project overview

### **Logs**
- Server: server.log
- Client: Unity Console

### **Debugging**
- Check logs first
- Verify network (netstat)
- Test locally before cloud
- Use PROGRESS_CHECKLIST.md

---

## ⚡ EMERGENCY FIXES

### **Server Crashed**
```bash
# Restart
cd Builds/Server_Windows_[timestamp]
RunServer.bat

# Check logs
tail -f server.log
```

### **Port Already in Use**
```bash
# Find process
netstat -ano | findstr 7777

# Kill process (Windows)
taskkill /PID <PID> /F

# Kill process (Linux)
kill -9 <PID>
```

### **Firewall Blocking**
```bash
# Windows
netsh advfirewall firewall add rule name="AntKnow" dir=in action=allow protocol=TCP localport=7777

# Linux
sudo ufw allow 7777
```

---

## 🎉 SUCCESS CHECKLIST

### **Server**
```
✅ Builds successfully
✅ Runs in headless mode
✅ Listens on port 7777
✅ Accepts connections
✅ Logs show no errors
```

### **Client**
```
✅ Builds successfully
✅ Can connect to server
✅ Shows connection status
✅ Receives game updates
```

### **Multiplayer**
```
✅ 2-4 players can join
✅ Game starts automatically
✅ Turn system works
✅ Dice rolling syncs
✅ Movement syncs
```

---

**KEEP THIS FILE HANDY FOR QUICK REFERENCE! 📌**

