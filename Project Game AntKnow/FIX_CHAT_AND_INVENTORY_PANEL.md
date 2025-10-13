# 🔧 FIX CHAT & INVENTORY PANEL ISSUES

## 🔴 VẤN ĐỀ BẠN GẶP

### **Vấn đề 1: Global Chat**
> "Global chat không tự join phòng mà chờ chat mới vào phòng. Button ẩn chat hoạt động chưa đúng. Chat ban đầu hiện nếu vậy sử dụng button ẩn hiện để kích hoạt vào phòng chat. Ban đầu ẩn chưa join globalchat nhưng khi click hiện chat ra thì join luôn. Làm chat tự nhiên nhất không có system gì tên thời gian và tên người gửi thôi"

**Yêu cầu:**
- ✅ Chat ban đầu ẨN, chưa join global chat
- ✅ Khi click button hiện chat → Join global chat luôn
- ✅ Format message đơn giản: `[HH:mm] Tên: Message` (không có "System")

---

### **Vấn đề 2: Inventory Panel**
> "Khi chuyển panel inventory thì bị đá quay về login scene là sao?"

**Log lỗi:**
```
[SimpleChat] Disconnecting from Vivox...
[FirebaseAuth] Duplicate instance detected! Destroying...
```

**Nguyên nhân:**
- SimpleChatManager gọi `DisconnectFromChat()` trong `OnDestroy()`
- Khi switch panel, SimpleChatManager bị destroy → Disconnect → Reload scene

---

## ✅ GIẢI PHÁP ĐÃ FIX

### **Fix 1: Chat ban đầu ẨN, join khi click hiện** ✅

**Changes:**
```csharp
[Header("Settings")]
[SerializeField] private bool autoConnect = false; // Don't auto-connect
[SerializeField] private bool showPanelByDefault = false; // Hide chat panel by default
```

**ToggleChat() - Join khi user mở chat:**
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
        Debug.Log("[SimpleChat] User opened chat, connecting and joining global chat...");
        ConnectToChat();
    }
    
    UpdateToggleButton();
}
```

**Kết quả:**
- ✅ Chat panel ẨN khi vào MenuScene
- ✅ Chưa join global chat
- ✅ Khi user click button hiện chat → Join global chat ngay

---

### **Fix 2: Format message đơn giản** ✅

**Before:**
```csharp
AddMessage("System", "Chào mừng đến với chat global!");
AddMessage("Admin", "Đây là chat test...");
```

**After:**
```csharp
// No welcome messages, just connect silently
// User will see messages when others chat
```

**Message format:**
```csharp
private void AddMessage(string sender, string message)
{
    // Simple format: [HH:mm] Sender: Message
    // No "System" prefix, just natural chat
    string formattedMessage = $"[{DateTime.Now:HH:mm}] {sender}: {message}";
    messages.Add(formattedMessage);
    
    UpdateChatDisplay();
}
```

**Kết quả:**
- ✅ Không có "System" messages
- ✅ Format đơn giản: `[14:30] hoang1: Hello!`
- ✅ Chỉ có thời gian + tên + message

---

### **Fix 3: KHÔNG disconnect khi switch panel** ✅

**Before:**
```csharp
private void OnDestroy()
{
    // ...
    DisconnectFromChat(); // ❌ Disconnect khi object bị destroy
}
```

**After:**
```csharp
private void OnDestroy()
{
    // Unsubscribe from Vivox events
    if (VivoxService.Instance != null)
    {
        VivoxService.Instance.LoggedIn -= OnVivoxLoggedIn;
        VivoxService.Instance.LoggedOut -= OnVivoxLoggedOut;
        VivoxService.Instance.ChannelJoined -= OnVivoxChannelJoined;
        VivoxService.Instance.ChannelMessageReceived -= OnVivoxMessageReceived;
    }
    
    // DON'T disconnect from chat when object is destroyed
    // This prevents disconnecting when switching panels in MenuScene
    // Only disconnect when user explicitly logs out or leaves MenuScene
    // DisconnectFromChat();
}
```

**Kết quả:**
- ✅ Switch panel → SimpleChatManager bị destroy → KHÔNG disconnect
- ✅ Không bị đá về LoginScene
- ✅ Chat vẫn connected khi switch panel

---

## 🎵 FLOW DIAGRAM

### **Chat Flow:**

```
MenuScene loads
    ↓
SimpleChatManager.Start()
    ↓
Chat panel ẨN (showPanelByDefault = false)
    ↓
Chưa join global chat (autoConnect = false)
    ↓
User clicks toggle button
    ↓
ToggleChat() → isPanelVisible = true
    ↓
Check: !isConnected?
    ↓
YES → ConnectToChat() ✅
    ↓
Join global chat
    ↓
User can send/receive messages
```

---

### **Panel Switch Flow:**

```
MenuScene - PanelHome active
    ↓
SimpleChatManager exists (connected to chat)
    ↓
User clicks Inventory button
    ↓
Switch to PanelInventory
    ↓
SimpleChatManager may be destroyed (if in PanelHome)
    ↓
OnDestroy() called
    ↓
Unsubscribe events ✅
    ↓
DON'T disconnect from chat ✅
    ↓
Chat still connected ✅
    ↓
User can switch back to PanelHome
    ↓
SimpleChatManager recreated (if needed)
    ↓
Still connected to chat ✅
```

---

## 🧪 TEST CASES

### **Test 1: Chat ban đầu ẨN**
```
1. Load MenuScene
2. ✅ Chat panel ẨN (không hiện)
3. ✅ Chưa join global chat
4. Check Console:
   ✅ KHÔNG có "[SimpleChat] Connecting to chat..."
```

---

### **Test 2: Join chat khi click hiện**
```
1. MenuScene loaded (chat ẨN)
2. Click toggle button (hiện chat)
3. ✅ Chat panel hiện
4. ✅ Join global chat ngay
5. Check Console:
   ✅ "[SimpleChat] User opened chat, connecting and joining global chat..."
   ✅ "[SimpleChat] Connected to chat"
   ✅ "[SimpleChat] Joined global channel: GlobalChat"
```

---

### **Test 3: Format message đơn giản**
```
1. Join chat
2. Send message: "Hello!"
3. ✅ Display: "[14:30] hoang1: Hello!"
4. Receive message from other user
5. ✅ Display: "[14:31] player2: Hi there!"
6. ✅ KHÔNG có "System" messages
```

---

### **Test 4: Switch panel KHÔNG disconnect**
```
1. MenuScene - PanelHome active
2. Open chat → Join global chat
3. Switch to PanelInventory
4. ✅ KHÔNG bị đá về LoginScene
5. Check Console:
   ✅ KHÔNG có "[SimpleChat] Disconnecting from Vivox..."
   ✅ KHÔNG có "[FirebaseAuth] Duplicate instance detected!"
6. Switch back to PanelHome
7. ✅ Chat still connected
```

---

## 🚨 IMPORTANT NOTES

### **SimpleChatManager Placement**

**Vấn đề:** SimpleChatManager nằm ở đâu trong MenuScene?

**Option 1: SimpleChatManager trong PanelHome** (Có thể bị destroy khi switch panel)
```
MenuScene
├── Canvas
│   ├── PanelHome (active/inactive)
│   │   └── SimpleChatManager ← Bị destroy khi PanelHome inactive
│   └── PanelInventory (active/inactive)
```

**Option 2: SimpleChatManager ở root level** (Không bị destroy)
```
MenuScene
├── Canvas
│   ├── PanelHome (active/inactive)
│   └── PanelInventory (active/inactive)
├── SimpleChatManager ← KHÔNG bị destroy khi switch panel
```

**Khuyến nghị:**
- ✅ **Đặt SimpleChatManager ở root level** (ngoài các panels)
- ✅ Chat panel có thể ở trong Canvas, nhưng SimpleChatManager script nên ở GameObject riêng
- ✅ Như vậy khi switch panel, SimpleChatManager KHÔNG bị destroy

---

### **Unity Setup**

**BƯỚC 1: Kiểm tra SimpleChatManager placement**
```
1. Open MenuScene
2. Find SimpleChatManager GameObject
3. Check hierarchy:
   - Nếu SimpleChatManager nằm trong PanelHome → DI CHUYỂN ra ngoài
   - Nếu SimpleChatManager ở root level → OK
```

**BƯỚC 2: Move SimpleChatManager (nếu cần)**
```
1. Drag SimpleChatManager GameObject
2. Drop vào root level (cùng cấp với Canvas)
3. Hoặc tạo GameObject mới:
   - Create Empty GameObject: "ChatManager"
   - Add Component: SimpleChatManager
   - Assign references (chatPanel, chatInput, etc.)
```

**BƯỚC 3: Verify settings**
```
SimpleChatManager component:
- Auto Connect: ✗ (false)
- Show Panel By Default: ✗ (false)
- Use Mock Chat: ✗ (false) - Use real Vivox
```

---

## 📁 FILES MODIFIED

### **1. SimpleChatManager.cs** ✅

**Changes:**
- `autoConnect = false` - Don't auto-connect
- `showPanelByDefault = false` - Hide chat by default
- `Start()` - Don't call ConnectToChat()
- `ToggleChat()` - Join chat when user opens panel
- `AddMessage()` - Simple format (no "System")
- `ConnectToMockChat()` - No welcome messages
- `OnDestroy()` - DON'T disconnect from chat

---

## 🎯 SUMMARY

**Vấn đề 1: Chat**
- ❌ Auto-connect, hiện panel by default
- ❌ Có "System" messages
- ✅ Ban đầu ẨN, join khi click hiện
- ✅ Format đơn giản: `[HH:mm] Tên: Message`

**Vấn đề 2: Inventory Panel**
- ❌ Switch panel → Disconnect → Đá về LoginScene
- ✅ Switch panel → KHÔNG disconnect → Vẫn ở MenuScene

**Setup:**
- ✅ SimpleChatManager: `autoConnect = false`, `showPanelByDefault = false`
- ✅ SimpleChatManager placement: Root level (không trong panel)
- ✅ OnDestroy(): KHÔNG disconnect

**Kết quả:**
- ✅ Chat ẨN ban đầu, join khi click hiện
- ✅ Format message đơn giản, tự nhiên
- ✅ Switch panel KHÔNG bị đá về LoginScene

---

**GO! GO! GO!** 🔥

