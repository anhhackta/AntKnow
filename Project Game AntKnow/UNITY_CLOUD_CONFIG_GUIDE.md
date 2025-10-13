# ☁️ UNITY CLOUD CONFIG - LOBBY & MATCHMAKER

## 🎯 MỤC ĐÍCH

Cấu hình Unity Gaming Services (UGS) trên cloud.unity.com để sử dụng Lobby và Matchmaker

---

## 🚀 BƯỚC 1: TRUY CẬP UNITY CLOUD

### **1.1. Đăng nhập**

```
1. Mở browser: https://cloud.unity.com/
2. Đăng nhập với Unity account
3. Select Organization (nếu có nhiều)
```

---

### **1.2. Chọn Project**

```
1. Dashboard → Projects
2. Tìm project "AntKnow" (hoặc tên project của bạn)
3. Click vào project
```

---

## 🎮 BƯỚC 2: ENABLE LOBBY SERVICE

### **2.1. Vào Lobby Settings**

```
1. Left sidebar → Multiplayer
2. Click "Lobby"
3. Nếu chưa enable → Click "Enable Lobby"
```

---

### **2.2. Lobby Configuration**

**Default settings (KHÔNG CẦN SỬA):**

```
✅ Max Players per Lobby: 4 (hoặc theo GameConfig.MAX_PLAYERS)
✅ Max Lobbies: 100 (free tier)
✅ Lobby Timeout: 30 minutes
✅ Heartbeat Interval: 30 seconds
```

**Nếu muốn custom:**

```
1. Click "Settings"
2. Adjust:
   - Max Players per Lobby: 4
   - Lobby Timeout: 30 minutes
3. Click "Save"
```

---

## 🔍 BƯỚC 3: ENABLE MATCHMAKER (OPTIONAL)

### **3.1. Vào Matchmaker Settings**

```
1. Left sidebar → Multiplayer
2. Click "Matchmaker"
3. Nếu chưa enable → Click "Enable Matchmaker"
```

---

### **3.2. Matchmaker Configuration**

**Default settings (KHÔNG CẦN SỬA):**

```
✅ Queue Name: "default"
✅ Max Players: 4
✅ Min Players: 2
✅ Timeout: 60 seconds
```

**Nếu muốn custom:**

```
1. Click "Create Queue"
2. Settings:
   - Queue Name: "default"
   - Max Players: 4
   - Min Players: 2
   - Timeout: 60 seconds
3. Click "Create"
```

---

## 🌐 BƯỚC 4: ENABLE RELAY SERVICE

### **4.1. Vào Relay Settings**

```
1. Left sidebar → Multiplayer
2. Click "Relay"
3. Nếu chưa enable → Click "Enable Relay"
```

---

### **4.2. Relay Configuration**

**Default settings (KHÔNG CẦN SỬA):**

```
✅ Max Connections: 3 (cho 4 players: host + 3 clients)
✅ Region: Auto (closest region)
```

---

## 🔑 BƯỚC 5: VERIFY PROJECT ID

### **5.1. Get Project ID**

```
1. Dashboard → Project Settings
2. Copy "Project ID"
3. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
```

---

### **5.2. Verify trong Unity Editor**

```
1. Unity Editor → Edit → Project Settings
2. Services → General Settings
3. Verify:
   ✅ Project ID matches cloud.unity.com
   ✅ Organization ID matches
```

---

## 🧪 BƯỚC 6: TEST CONNECTION

### **6.1. Test trong Unity Editor**

```
1. Play MenuScene
2. Check Console:
   ✅ "MenuScene: Initializing Unity Gaming Services..."
   ✅ "MenuScene: UGS initialized and signed in successfully"
   ✅ "UGSAuthService: Signed in as [PlayerId]"
```

---

### **6.2. Test Lobby**

```
1. Click "Tạo phòng"
2. Create lobby "Test Room"
3. Check Console:
   ✅ "Lobby created successfully: Test Room"
   ✅ NO errors
```

---

### **6.3. Test Matchmaker**

```
1. Click "Tìm trận"
2. Wait for match
3. Check Console:
   ✅ "Starting matchmaking..."
   ✅ "Searching for available matches..."
   ✅ "Found available lobby" OR "Creating new lobby"
   ✅ NO errors
```

---

## 🚨 TROUBLESHOOTING

### **Lỗi 1: "Singleton is not initialized"**

**Nguyên nhân:** Unity Services chưa được initialize

**Fix:**
```csharp
// MenuSceneManager.cs
private async void InitializeMenuScene()
{
    // ... check user logged in ...
    
    // Initialize UGS ← QUAN TRỌNG
    await InitializeUGS();
    
    // ... setup UI ...
}

private async Task InitializeUGS()
{
    try
    {
        // Check if already initialized
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Debug.Log("UGS already initialized");
            return;
        }
        
        // Initialize
        await UnityServices.InitializeAsync();
        
        // Sign in
        bool signedIn = await UGSAuthService.Instance.AutoSignInFromFirebaseAsync();
        
        if (signedIn)
        {
            Debug.Log("UGS initialized and signed in successfully");
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to initialize UGS: {e.Message}");
    }
}
```

---

### **Lỗi 2: "Project ID not found"**

**Nguyên nhân:** Project chưa được link với Unity Cloud

**Fix:**
```
1. Unity Editor → Edit → Project Settings
2. Services → General Settings
3. Click "Select Organization"
4. Select organization
5. Click "Create" hoặc "Link" project
6. Verify Project ID matches cloud.unity.com
```

---

### **Lỗi 3: "Lobby service not enabled"**

**Nguyên nhân:** Lobby chưa được enable trên cloud.unity.com

**Fix:**
```
1. https://cloud.unity.com/
2. Select project
3. Multiplayer → Lobby
4. Click "Enable Lobby"
5. Wait 1-2 minutes
6. Test lại trong Unity
```

---

### **Lỗi 4: "Authentication failed"**

**Nguyên nhân:** Firebase UID không được sign in vào UGS

**Fix:**
```csharp
// UGSAuthService.cs
public async Task<bool> AutoSignInFromFirebaseAsync()
{
    try
    {
        // Get Firebase UID
        var gameDataManager = GameDataManager.Instance;
        string firebaseUid = gameDataManager.currentUserId;
        
        if (string.IsNullOrEmpty(firebaseUid))
        {
            Debug.LogError("Firebase UID is null or empty");
            return false;
        }
        
        // Sign in with custom ID (Firebase UID)
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to sign in: {e.Message}");
        return false;
    }
}
```

---

## 📋 CHECKLIST

### **Unity Cloud (cloud.unity.com):**
- [ ] Đăng nhập thành công
- [ ] Project "AntKnow" tồn tại
- [ ] Lobby Service enabled
- [ ] Relay Service enabled
- [ ] Matchmaker enabled (optional)
- [ ] Project ID copied

### **Unity Editor:**
- [ ] Project Settings → Services → Project ID matches
- [ ] MenuSceneManager.InitializeUGS() được gọi
- [ ] UGSAuthService.IsSignedIn property có try-catch
- [ ] Console: "UGS initialized and signed in successfully"

### **Test:**
- [ ] Play MenuScene → No errors
- [ ] Click "Tạo phòng" → Lobby created
- [ ] Click "Tìm trận" → Matchmaking works
- [ ] 2 players join lobby → Both see each other

---

## 🎯 SUMMARY

**Unity Cloud Config:**
- ✅ Enable Lobby Service
- ✅ Enable Relay Service
- ✅ Enable Matchmaker (optional)
- ✅ Verify Project ID

**Unity Editor:**
- ✅ Initialize UGS trong MenuScene
- ✅ Fix UGSAuthService.IsSignedIn property
- ✅ Test connection

**Không cần config gì thêm:**
- ✅ Default settings đủ dùng
- ✅ Free tier: 100 lobbies, 1000 CCU
- ✅ Auto region selection

---

**THỜI GIAN: 10 PHÚT** ⏱️

**LÀM NGAY!** 🔥

