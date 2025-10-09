# 🚀 DEPLOYMENT GUIDE - TRIỂN KHAI SERVER LÊN CLOUD

## 📋 MỤC LỤC

1. [Local Testing](#local-testing)
2. [LAN Deployment](#lan-deployment)
3. [Cloud Deployment (AWS)](#cloud-deployment-aws)
4. [Cloud Deployment (Google Cloud)](#cloud-deployment-google-cloud)
5. [Unity Multiplay](#unity-multiplay)
6. [Monitoring & Maintenance](#monitoring--maintenance)

---

## 🏠 LOCAL TESTING

### **Setup**
```bash
# 1. Build server
Unity → Build → Build Dedicated Server (Windows)

# 2. Run server
cd Builds/Server_Windows_[timestamp]
RunServer.bat

# 3. Run clients (same machine)
cd Builds/Client_Windows
AntKnow.exe
```

### **Connection**
```
Server IP: 127.0.0.1
Port: 7777
```

**✅ Use for**: Development, testing, debugging

---

## 🏢 LAN DEPLOYMENT

### **Setup Server**
```bash
# 1. Find server machine IP
ipconfig (Windows)
ifconfig (Linux/Mac)

Example: 192.168.1.100

# 2. Run server
RunServer.bat

# 3. Open firewall
Windows Firewall → Allow app → AntKnowServer.exe
Or: netsh advfirewall firewall add rule name="AntKnow Server" dir=in action=allow protocol=TCP localport=7777
```

### **Connect Clients**
```
Server IP: 192.168.1.100 (your LAN IP)
Port: 7777
```

**✅ Use for**: Office testing, LAN parties, local tournaments

---

## ☁️ CLOUD DEPLOYMENT (AWS)

### **Step 1: Create EC2 Instance** (10 phút)

```
1. AWS Console → EC2 → Launch Instance
2. Choose AMI:
   - Windows Server 2022 (for Windows build)
   - Ubuntu 22.04 LTS (for Linux build)
3. Instance Type:
   - t3.medium (2 vCPU, 4GB RAM) - Recommended
   - t3.small (2 vCPU, 2GB RAM) - Budget
4. Configure Security Group:
   - Add Rule: Custom TCP, Port 7777, Source: 0.0.0.0/0
   - Add Rule: SSH (22) or RDP (3389) for management
5. Launch & Download key pair
```

### **Step 2: Upload Server Build** (15 phút)

**Windows Server:**
```powershell
# 1. Connect via RDP
mstsc /v:<EC2_PUBLIC_IP>

# 2. Download server build (use browser or S3)
# 3. Extract to C:\AntKnowServer\
# 4. Run: C:\AntKnowServer\RunServer.bat
```

**Linux Server:**
```bash
# 1. Connect via SSH
ssh -i your-key.pem ubuntu@<EC2_PUBLIC_IP>

# 2. Upload server build
scp -i your-key.pem -r Builds/Server_Linux_* ubuntu@<EC2_PUBLIC_IP>:~/

# 3. Extract and run
cd ~/Server_Linux_*
chmod +x AntKnowServer
chmod +x run_server.sh
./run_server.sh
```

### **Step 3: Run as Service** (10 phút)

**Linux (systemd):**
```bash
# Create service file
sudo nano /etc/systemd/system/antknow-server.service

# Content:
[Unit]
Description=AntKnow Dedicated Server
After=network.target

[Service]
Type=simple
User=ubuntu
WorkingDirectory=/home/ubuntu/Server_Linux_[timestamp]
ExecStart=/home/ubuntu/Server_Linux_[timestamp]/AntKnowServer -batchmode -nographics -logFile /var/log/antknow-server.log
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target

# Enable and start
sudo systemctl enable antknow-server
sudo systemctl start antknow-server
sudo systemctl status antknow-server
```

**Windows (NSSM):**
```powershell
# 1. Download NSSM: https://nssm.cc/download
# 2. Install service
nssm install AntKnowServer "C:\AntKnowServer\AntKnowServer.exe" "-batchmode -nographics -logFile C:\AntKnowServer\server.log"

# 3. Start service
nssm start AntKnowServer
```

### **Step 4: Configure Auto-Restart** (5 phút)

```bash
# Linux: Already configured in systemd (Restart=always)

# Windows: NSSM auto-restart
nssm set AntKnowServer AppExit Default Restart
```

### **Step 5: Test Connection** (5 phút)

```
Client:
Server IP: <EC2_PUBLIC_IP>
Port: 7777

Expected:
✅ Connected!
✅ Game starts with 2 players
```

**💰 Cost**: ~$30-50/month (t3.medium, 24/7)

---

## ☁️ CLOUD DEPLOYMENT (GOOGLE CLOUD)

### **Step 1: Create Compute Engine VM**

```
1. GCP Console → Compute Engine → Create Instance
2. Machine type: e2-medium (2 vCPU, 4GB RAM)
3. Boot disk: Ubuntu 22.04 LTS
4. Firewall:
   ✅ Allow HTTP traffic
   ✅ Allow HTTPS traffic
5. Create
```

### **Step 2: Configure Firewall**

```
VPC Network → Firewall → Create Rule
- Name: antknow-server
- Targets: All instances
- Source IP ranges: 0.0.0.0/0
- Protocols and ports: tcp:7777
- Create
```

### **Step 3: Deploy Server**

```bash
# Same as AWS Linux deployment
# See AWS Step 2 & 3
```

**💰 Cost**: ~$25-40/month (e2-medium, 24/7)

---

## 🎮 UNITY MULTIPLAY (RECOMMENDED)

### **Why Unity Multiplay?**
```
✅ Auto-scaling (tự động tăng/giảm server)
✅ Global distribution (server gần người chơi)
✅ Integrated matchmaking
✅ Built-in monitoring
✅ Pay-per-use (chỉ trả khi có người chơi)
```

### **Setup** (30 phút)

```
1. Unity Dashboard → Multiplay → Get Started
2. Create Fleet:
   - Name: AntKnow Production
   - Build: Upload server build (Linux)
   - Regions: Select (e.g., Asia Southeast)
   - Min servers: 1
   - Max servers: 10

3. Configure Build:
   - Launch Parameters: -batchmode -nographics -logFile server.log
   - Port: 7777
   - Query Type: None

4. Deploy Fleet

5. Client Integration:
   - Use Matchmaker API
   - Get server IP/port from matchmaker
   - Connect client
```

### **Client Code**

```csharp
using Unity.Services.Matchmaker;

async void FindMatch()
{
    var ticket = await MatchmakerService.Instance.CreateTicketAsync(
        new List<Player> { new Player("player-id") },
        new MatchmakingOptions()
    );

    // Poll for match
    var assignment = await PollForMatchAsync(ticket.Id);
    
    // Connect to assigned server
    string serverIP = assignment.Ip;
    ushort serverPort = (ushort)assignment.Port;
    ConnectToServer(serverIP, serverPort);
}
```

**💰 Cost**: 
- Free tier: 20 CCU (concurrent users)
- Paid: $0.50 per CCU/month

---

## 📊 MONITORING & MAINTENANCE

### **Server Logs**

```bash
# Linux
tail -f /var/log/antknow-server.log

# Windows
Get-Content C:\AntKnowServer\server.log -Wait
```

### **Performance Monitoring**

```bash
# CPU & RAM
top (Linux)
Task Manager (Windows)

# Network
netstat -an | grep 7777
```

### **Health Check Script**

```bash
#!/bin/bash
# health_check.sh

PORT=7777
if netstat -an | grep -q ":$PORT.*LISTEN"; then
    echo "✅ Server is running"
    exit 0
else
    echo "❌ Server is down! Restarting..."
    systemctl restart antknow-server
    exit 1
fi
```

### **Cron Job (Auto Health Check)**

```bash
# Run every 5 minutes
crontab -e

# Add:
*/5 * * * * /home/ubuntu/health_check.sh >> /var/log/health_check.log 2>&1
```

### **Backup & Updates**

```bash
# Backup server data
tar -czf antknow-backup-$(date +%Y%m%d).tar.gz /home/ubuntu/Server_Linux_*

# Update server
systemctl stop antknow-server
# Upload new build
systemctl start antknow-server
```

---

## 🔒 SECURITY BEST PRACTICES

### **1. Firewall**
```
✅ Only open port 7777
❌ Don't open all ports
```

### **2. DDoS Protection**
```
Use CloudFlare or AWS Shield
Rate limiting: Max 10 connections/IP/minute
```

### **3. Authentication**
```
Implement player authentication
Validate client tokens
Ban malicious players
```

### **4. Updates**
```
Regular security patches
Update Unity packages
Monitor CVEs
```

---

## 📈 SCALING STRATEGY

### **Phase 1: Single Server (0-50 players)**
```
1 server: t3.medium
Cost: ~$30/month
```

### **Phase 2: Multi-Region (50-500 players)**
```
3 servers: Asia, US, EU
Load balancer
Cost: ~$150/month
```

### **Phase 3: Auto-Scaling (500+ players)**
```
Unity Multiplay
Auto-scale 1-50 servers
Cost: Variable ($250-2000/month)
```

---

## ✅ DEPLOYMENT CHECKLIST

```
Pre-Deployment:
✅ Server build tested locally
✅ Client build tested locally
✅ Multiplayer tested (2-4 players)
✅ Performance optimized
✅ Logs configured

Deployment:
✅ Cloud instance created
✅ Firewall configured (port 7777)
✅ Server uploaded
✅ Service configured (auto-restart)
✅ Health check setup

Post-Deployment:
✅ Test connection from client
✅ Monitor logs for errors
✅ Test with multiple clients
✅ Verify auto-restart works
✅ Setup monitoring alerts

Production:
✅ Backup strategy
✅ Update procedure
✅ Incident response plan
✅ Player support system
```

---

## 🆘 TROUBLESHOOTING

### **Server won't start**
```
Check:
1. Port 7777 available? (netstat -an | grep 7777)
2. Permissions correct? (chmod +x AntKnowServer)
3. Dependencies installed? (ldd AntKnowServer)
4. Check logs: tail -f server.log
```

### **Clients can't connect**
```
Check:
1. Server running? (systemctl status antknow-server)
2. Firewall open? (sudo ufw status)
3. Correct IP? (curl ifconfig.me)
4. Port forwarding? (if behind NAT)
```

### **High CPU/RAM usage**
```
Solutions:
1. Reduce target FPS (30 → 20)
2. Increase instance size
3. Optimize game logic
4. Add more servers (load balance)
```

---

**BẠN ĐÃ SẴN SÀNG DEPLOY! 🚀**

