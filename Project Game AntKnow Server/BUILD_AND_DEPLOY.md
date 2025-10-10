# 🚀 BUILD AND DEPLOY - STEP BY STEP

**Hướng dẫn chi tiết để build và deploy server lên Unity Multiplay**

---

## ✅ PREREQUISITES

```
✅ Unity 6000.0.48f1 installed
✅ Linux Build Support (IL2CPP) installed
✅ Unity Gaming Services (UGS) account
✅ Project linked to UGS
✅ Code updated (0 compile errors)
```

---

## 📋 STEP-BY-STEP GUIDE

### **STEP 1: Open Unity Project** ⏱️ 2 min

```
1. Open Unity Hub
2. Click "Project Game AntKnow Server"
3. Wait for Unity to load
4. Verify Console shows 0 errors
```

**Expected Result:**
```
✅ Unity Editor opens
✅ Console shows: "All compiler errors have to be fixed before you can enter playmode!"
   (This is normal - we're not entering playmode, we're building)
✅ Or Console shows 0 errors
```

---

### **STEP 2: Verify Build Settings** ⏱️ 3 min

```
1. File → Build Settings
2. Verify:
   ✅ Platform: Dedicated Server
   ✅ Target Platform: Linux
   ✅ Architecture: x86_64
   ✅ Scripting Backend: IL2CPP
   ✅ Scenes in Build: (empty is OK for server)
3. Click "Switch Platform" if needed (wait 5-10 min)
4. Close Build Settings
```

**Expected Result:**
```
✅ Platform shows "Dedicated Server" with Unity icon
✅ Target Platform: Linux
✅ Architecture: x86_64
```

---

### **STEP 3: Build Linux Server** ⏱️ 10-15 min

```
Option A: Using Menu (Recommended)
1. Build → Build Linux Server for Multiplay
2. Choose output folder: "Builds/LinuxServer"
3. Click "Select Folder"
4. Wait for build to complete (10-15 min)

Option B: Using Build Settings
1. File → Build Settings
2. Click "Build"
3. Choose output folder: "Builds/LinuxServer"
4. Click "Select Folder"
5. Wait for build to complete (10-15 min)
```

**Expected Result:**
```
✅ Build completes successfully
✅ Folder "Builds/LinuxServer" contains:
   - AntKnowServer (executable)
   - AntKnowServer_Data/ (folder)
   - UnityPlayer.so
   - LinuxPlayer_s.debug (optional)
```

**If Build Fails:**
```
❌ Check Console for errors
❌ Fix errors and rebuild
❌ Common issues:
   - Missing Linux Build Support → Install from Unity Hub
   - Compile errors → Fix code errors first
   - Out of disk space → Free up space
```

---

### **STEP 4: Create Build Configuration** ⏱️ 5 min

```
1. Create file: Builds/LinuxServer/server.json
2. Content:
{
  "commandLineArguments": "-port $$port$$ -queryPort $$query_port$$ -logFile $$log_dir$$/server.log",
  "filePath": "AntKnowServer",
  "queryType": "sqp",
  "variables": {}
}

3. Save file
```

**Expected Result:**
```
✅ File "Builds/LinuxServer/server.json" created
✅ Content matches above
```

---

### **STEP 5: Compress Build** ⏱️ 2 min

```
1. Navigate to "Builds/LinuxServer"
2. Select all files:
   - AntKnowServer
   - AntKnowServer_Data/
   - UnityPlayer.so
   - server.json
   - LinuxPlayer_s.debug (if exists)
3. Right-click → Send to → Compressed (zipped) folder
4. Name: "AntKnowServer.zip"
5. Move to "Builds/" folder
```

**Expected Result:**
```
✅ File "Builds/AntKnowServer.zip" created
✅ Size: ~50-100 MB (depends on build)
```

---

### **STEP 6: Upload to Unity Multiplay** ⏱️ 10 min

```
1. Open Unity Dashboard: https://dashboard.unity3d.com/
2. Select your project
3. Go to: Multiplayer → Multiplay Hosting
4. Click "Builds" tab
5. Click "Upload Build"
6. Fill in:
   - Name: "AntKnow Server v1.0"
   - Build Type: "Server"
   - Platform: "Linux"
   - Upload: Select "Builds/AntKnowServer.zip"
7. Click "Upload"
8. Wait for upload to complete (5-10 min)
```

**Expected Result:**
```
✅ Build uploaded successfully
✅ Status: "Ready"
✅ Build ID: (copy this for next step)
```

---

### **STEP 7: Create Build Configuration (Dashboard)** ⏱️ 5 min

```
1. In Multiplay Dashboard
2. Click "Build Configurations" tab
3. Click "Create Build Configuration"
4. Fill in:
   - Name: "AntKnow Server Config"
   - Build: Select "AntKnow Server v1.0"
   - Query Type: "SQP"
   - Binary Path: "AntKnowServer"
   - Command Line: "-port $$port$$ -queryPort $$query_port$$ -logFile $$log_dir$$/server.log"
   - Variables: (leave empty)
5. Click "Create"
```

**Expected Result:**
```
✅ Build Configuration created
✅ Status: "Active"
✅ Config ID: (copy this for next step)
```

---

### **STEP 8: Create Fleet** ⏱️ 5 min

```
1. In Multiplay Dashboard
2. Click "Fleets" tab
3. Click "Create Fleet"
4. Fill in:
   - Name: "AntKnow Fleet"
   - Build Configuration: Select "AntKnow Server Config"
   - Regions: Select your region (e.g., "Asia Pacific - Singapore")
   - Fleet Type: "Multiplay"
   - Scaling:
     - Min Servers: 1
     - Max Servers: 10
     - Target Usage: 70%
5. Click "Create"
```

**Expected Result:**
```
✅ Fleet created
✅ Status: "Deploying..."
✅ Wait 2-5 minutes for servers to start
```

---

### **STEP 9: Verify Deployment** ⏱️ 5 min

```
1. In Multiplay Dashboard
2. Click "Servers" tab
3. Verify:
   ✅ At least 1 server shows "Online"
   ✅ Server IP and Port visible
   ✅ Query Port visible
4. Click on server to view logs
5. Check logs for:
   ✅ "[ServerGameManager] Server spawned"
   ✅ No errors
```

**Expected Result:**
```
✅ Server status: "Online"
✅ Logs show server started successfully
✅ No errors in logs
```

---

### **STEP 10: Test Connection** ⏱️ 10 min

```
Option A: Using Unity Editor (Client Project)
1. Open "Project Game AntKnow" (client)
2. Open MenuScene
3. Enter Play Mode
4. Click "Join Game"
5. Enter server IP and port
6. Click "Connect"
7. Verify connection successful

Option B: Using Build (Client)
1. Build client project
2. Run client executable
3. Join game with server IP and port
4. Verify connection successful
```

**Expected Result:**
```
✅ Client connects to server
✅ Server logs show: "[ServerGameManager] Client X connected"
✅ Client shows: "Connected to server"
```

---

## 🎉 SUCCESS!

```
✅ Server built successfully
✅ Server uploaded to Multiplay
✅ Fleet deployed
✅ Server online
✅ Client can connect
```

**YOU DID IT! 🚀**

---

## 🐛 TROUBLESHOOTING

### **Build Fails**
```
Problem: Build fails with errors
Solution:
1. Check Console for errors
2. Fix compile errors
3. Rebuild
4. If still fails, check:
   - Linux Build Support installed?
   - Enough disk space?
   - Unity version correct (6000.0.48f1)?
```

### **Upload Fails**
```
Problem: Upload to Multiplay fails
Solution:
1. Check internet connection
2. Check file size (max 2 GB)
3. Try again
4. If still fails:
   - Check Unity Dashboard status
   - Contact Unity Support
```

### **Server Won't Start**
```
Problem: Server status shows "Error" or "Offline"
Solution:
1. Check server logs in Dashboard
2. Look for errors:
   - Missing dependencies?
   - Port conflicts?
   - Configuration errors?
3. Fix errors and redeploy
```

### **Client Can't Connect**
```
Problem: Client can't connect to server
Solution:
1. Verify server is "Online"
2. Check server IP and port
3. Check firewall settings
4. Check client code:
   - Correct IP and port?
   - NetworkManager configured?
   - Transport configured?
```

---

## 📊 TIMELINE SUMMARY

```
Step 1: Open Unity (2 min)
Step 2: Verify Build Settings (3 min)
Step 3: Build Linux Server (10-15 min)
Step 4: Create Build Config (5 min)
Step 5: Compress Build (2 min)
Step 6: Upload to Multiplay (10 min)
Step 7: Create Build Config (5 min)
Step 8: Create Fleet (5 min)
Step 9: Verify Deployment (5 min)
Step 10: Test Connection (10 min)

Total: ~60-70 minutes
```

---

## 📖 NEXT STEPS

### **After Successful Deployment**
```
1. ✅ Test full gameplay
2. ✅ Monitor server logs
3. ✅ Check server performance
4. ✅ Scale fleet if needed
5. ✅ Update client to use server IP
6. ✅ Test with multiple clients
7. ✅ Deploy to production
```

### **Ongoing Maintenance**
```
1. Monitor server health
2. Check logs for errors
3. Update server code as needed
4. Rebuild and redeploy
5. Scale fleet based on usage
```

---

## 🔗 USEFUL LINKS

```
Unity Dashboard: https://dashboard.unity3d.com/
Multiplay Docs: https://docs.unity.com/multiplay/
Netcode Docs: https://docs-multiplayer.unity3d.com/
Unity Forums: https://forum.unity.com/
```

---

**GOOD LUCK! 🚀**

**Next file**: `MULTIPLAY_QUICK_START.md` (alternative guide)

