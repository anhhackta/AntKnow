# 🎮 SIMPLE CHAT SINGLETON - FINAL SOLUTION

## 🎯 YÊU CẦU CỦA BẠN

> "Tôi chỉ cần đơn giản là khi click vào button chat hiện ra PanelChat và join vào phòng global channel name là GlobalChat đó chỉ vậy thôi. Còn lại trong các panel thì PanelChat vẫn hoạt động. Làm đơn giản như thế để sau khi vào trận trong game có thể tạo 1 global channel name để chat trong game (mỗi phòng global name ngẫu nhiên hay gì đó chẳng hạn)"

**Yêu cầu:**
- ✅ Click button chat → Hiện PanelChat + Join GlobalChat
- ✅ Switch panel (Home, Inventory, Upgrade, Shop) → PanelChat vẫn hoạt động
- ✅ Không bị đá về LoginScene khi switch panel
- ✅ Reusable cho game rooms (mỗi phòng có channel name riêng)

---

## ✅ GIẢI PHÁP - SINGLETON PATTERN

### **SimpleChatManager = Singleton + DontDestroyOnLoad**

**Tại sao?**
- ✅ **Singleton** → Chỉ có 1 instance duy nhất
- ✅ **DontDestroyOnLoad** → Không bị destroy khi switch panel/scene
- ✅ **Persist** → Chat vẫn hoạt động khi switch panel

**Code:**
```csharp
public class SimpleChatManager : MonoBehaviour
{
    // Singleton instance
    public static SimpleChatManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[SimpleChat] Singleton instance created and persisted");
        }
        else
        {
            Debug.LogWarning("[SimpleChat] Duplicate instance detected, destroying...");
            Destroy(gameObject);
            return;
        }
    }
}
```

---

## 🎵 ARCHITECTURE

### **MenuScene Hierarchy:**

```
MenuScene
├── Canvas
│   ├── PanelHome (active/inactive)
│   ├── PanelInventory (active/inactive)
│   ├── PanelUpgrade (active/inactive)
│   ├── PanelShop (active/inactive)
│   └── PanelChat (active/inactive) ← Chat panel ngang hàng với các panel khác
├── ChatManager (SimpleChatManager) ← Singleton, DontDestroyOnLoad
└── ... (other managers)
```

**Cách hoạt động:**
1. **ChatManager** (SimpleChatManager) → Singleton, không bị destroy
2. **PanelChat** → Chỉ ẩn/hiện (SetActive true/false)
3. Switch panel → ChatManager vẫn tồn tại → Chat vẫn hoạt động

---

## 🚀 FEATURES

### **1. Join GlobalChat khi click button** ✅

**Code:**
```csharp
public void ToggleChat()
{
    isPanelVisible = !isPanelVisible;
    
    if (chatPanel != null)
    {
        chatPanel.SetActive(isPanelVisible);
    }
    
    // Join chat when user opens panel for the first time
    if (isPanelVisible && !isConnected)
    {
        Debug.Log("[SimpleChat] User opened chat, connecting and joining GlobalChat...");
        ConnectToChat(); // Join "GlobalChat" by default
    }
    
    UpdateToggleButton();
}
```

**Flow:**
```
User clicks chat button
    ↓
ToggleChat() → isPanelVisible = true
    ↓
PanelChat.SetActive(true) → Hiện panel
    ↓
Check: !isConnected?
    ↓
YES → ConnectToChat() → Join "GlobalChat"
    ↓
User can send/receive messages
```

---

### **2. Reusable cho game rooms** ✅

**Code:**
```csharp
private string currentChannelName = "GlobalChat"; // Current channel name

/// <summary>
/// Join a specific channel (for game rooms)
/// Example: JoinChannelByName("GameRoom_12345")
/// </summary>
public async void JoinChannelByName(string channelName)
{
    if (string.IsNullOrEmpty(channelName))
    {
        Debug.LogError("[SimpleChat] Channel name is null or empty");
        return;
    }

    try
    {
        // Leave current channel if connected
        if (isConnected)
        {
            await LeaveChannel();
        }

        // Update current channel name
        currentChannelName = channelName;
        Debug.Log($"[SimpleChat] Switching to channel: {currentChannelName}");

        // Connect and join new channel
        await ConnectToChat();
    }
    catch (Exception e)
    {
        Debug.LogError($"[SimpleChat] Failed to join channel {channelName}: {e.Message}");
    }
}
```

**Usage trong GameScene:**
```csharp
// In GameScene, when user joins a room
string roomId = "Room_" + Random.Range(1000, 9999); // e.g., "Room_1234"
SimpleChatManager.Instance.JoinChannelByName(roomId);
```

---

### **3. Manual disconnect khi logout** ✅

**Code:**
```csharp
/// <summary>
/// Manually disconnect from chat (call when user logs out)
/// </summary>
public void ManualDisconnect()
{
    Debug.Log("[SimpleChat] Manual disconnect requested");
    DisconnectFromChat();
    isConnected = false;
    messages.Clear();
    UpdateChatDisplay();
}
```

**Usage:**
```csharp
// In AuthUIController, when user logs out
SimpleChatManager.Instance.ManualDisconnect();
```

---

## 🎵 FLOW DIAGRAM

### **MenuScene - Switch Panel:**

```
MenuScene loads
    ↓
ChatManager (Singleton) created → DontDestroyOnLoad
    ↓
PanelHome active, PanelChat hidden
    ↓
User clicks chat button
    ↓
PanelChat.SetActive(true) → Join "GlobalChat"
    ↓
User clicks Inventory button
    ↓
PanelHome.SetActive(false), PanelInventory.SetActive(true)
    ↓
ChatManager KHÔNG bị destroy ✅
    ↓
PanelChat vẫn hoạt động ✅
    ↓
User clicks chat button again
    ↓
PanelChat.SetActive(true) → Chat vẫn connected ✅
```

---

### **GameScene - Join Room Chat:**

```
GameScene loads
    ↓
ChatManager (Singleton) vẫn tồn tại (DontDestroyOnLoad)
    ↓
User joins room "Room_1234"
    ↓
SimpleChatManager.Instance.JoinChannelByName("Room_1234")
    ↓
Leave "GlobalChat" → Join "Room_1234"
    ↓
User can chat with players in room
    ↓
User leaves room
    ↓
SimpleChatManager.Instance.JoinChannelByName("GlobalChat")
    ↓
Back to global chat
```

---

## 🧪 TEST CASES

### **Test 1: Click button → Join GlobalChat**
```
1. Load MenuScene
2. PanelChat ẨN
3. Click chat button
4. ✅ PanelChat hiện
5. ✅ Join "GlobalChat"
6. Console: "[SimpleChat] User opened chat, connecting and joining GlobalChat..."
7. Console: "[SimpleChat] Successfully joined channel: GlobalChat"
```

---

### **Test 2: Switch panel → Chat vẫn hoạt động**
```
1. MenuScene - PanelHome active
2. Open chat → Join GlobalChat
3. Send message: "Hello!"
4. ✅ Message sent
5. Click Inventory button
6. ✅ PanelInventory active
7. ✅ KHÔNG bị đá về LoginScene
8. Console: KHÔNG có "Disconnecting from Vivox..."
9. Click chat button again
10. ✅ PanelChat hiện
11. ✅ Chat vẫn connected
12. ✅ Messages vẫn còn
```

---

### **Test 3: Join game room chat**
```
1. MenuScene - Connected to "GlobalChat"
2. Load GameScene
3. User joins room "Room_1234"
4. Code: SimpleChatManager.Instance.JoinChannelByName("Room_1234")
5. ✅ Leave "GlobalChat"
6. ✅ Join "Room_1234"
7. Console: "[SimpleChat] Switching to channel: Room_1234"
8. Console: "[SimpleChat] Successfully joined channel: Room_1234"
9. User can chat with players in room
```

---

### **Test 4: Manual disconnect khi logout**
```
1. MenuScene - Connected to chat
2. User clicks logout
3. Code: SimpleChatManager.Instance.ManualDisconnect()
4. ✅ Disconnect from chat
5. ✅ Messages cleared
6. Console: "[SimpleChat] Manual disconnect requested"
7. Console: "[SimpleChat] Disconnected from chat"
```

---

## 🚀 UNITY SETUP

### **BƯỚC 1: Tạo ChatManager GameObject**

```
1. Open MenuScene
2. Create Empty GameObject: "ChatManager"
3. Add Component: SimpleChatManager
4. Assign references:
   - Chat Panel: Drag PanelChat
   - Chat Input: Drag InputField
   - Chat Display: Drag TextMeshProUGUI
   - Chat Scroll Rect: Drag ScrollRect
   - Send Button: Drag Button
   - Toggle Button: Drag Button (chat toggle)
5. Settings:
   - Auto Connect: ✗ (false)
   - Show Panel By Default: ✗ (false)
   - Use Mock Chat: ✗ (false)
```

---

### **BƯỚC 2: Setup Chat Button**

```
1. Find chat toggle button (e.g., "BtnChat")
2. Button component → OnClick():
   - Add SimpleChatManager.ToggleChat()
```

---

### **BƯỚC 3: Verify Hierarchy**

```
MenuScene
├── Canvas
│   ├── PanelHome
│   ├── PanelInventory
│   ├── PanelUpgrade
│   ├── PanelShop
│   └── PanelChat ← Chat panel ngang hàng
├── ChatManager ← SimpleChatManager (Singleton)
└── ...
```

---

## 📁 FILES MODIFIED

### **SimpleChatManager.cs** ✅

**Changes:**
1. Added Singleton pattern with `Instance` property
2. Added `DontDestroyOnLoad` in `Awake()`
3. Changed `globalChannelName` to `currentChannelName` (dynamic)
4. Added `JoinChannelByName(string channelName)` for game rooms
5. Added `LeaveChannel()` to leave current channel
6. Added `ManualDisconnect()` for logout
7. Updated `OnDestroy()` to only cleanup singleton instance

---

## 🎯 SUMMARY

**Vấn đề:**
- ❌ Switch panel → SimpleChatManager bị destroy → Disconnect → Đá về LoginScene

**Giải pháp:**
- ✅ SimpleChatManager = Singleton + DontDestroyOnLoad
- ✅ Không bị destroy khi switch panel
- ✅ Chat vẫn hoạt động khi switch panel

**Features:**
- ✅ Click button → Join "GlobalChat"
- ✅ Switch panel → Chat vẫn hoạt động
- ✅ Reusable cho game rooms: `JoinChannelByName("Room_1234")`
- ✅ Manual disconnect khi logout: `ManualDisconnect()`

**Setup:**
- ✅ Tạo ChatManager GameObject (root level)
- ✅ Add SimpleChatManager component
- ✅ Assign references
- ✅ Chat button → OnClick() → SimpleChatManager.ToggleChat()

---

**GO! GO! GO!** 🔥

