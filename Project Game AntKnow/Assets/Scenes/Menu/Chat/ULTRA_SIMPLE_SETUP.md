# Ultra Simple Chat Setup - Sử dụng Vivox Thật!

## 🎯 Chỉ 3 Bước - Hoạt động ngay!

**✅ Đã có Vivox package và example - Sử dụng Vivox thật!**

### Bước 1: Tạo GameObject
```
1. Right-click Canvas → Create Empty
2. Rename: "SimpleChat"
3. Add Component: SimpleChatManager
```

### Bước 2: Tạo UI cơ bản
```
Right-click SimpleChat → UI → Panel
Rename: "ChatPanel"

Trong ChatPanel tạo:
├── ScrollView (ScrollRect)
│   └── Content (GameObject)
│       └── ChatDisplay (Text - TMPro)
├── ChatInput (InputField - TMPro)  
├── SendButton (Button)
└── ToggleButton (Button)
```

### Bước 3: Assign References (30 giây)
```
Select SimpleChat GameObject
Trong SimpleChatManager Inspector:
- Chat Input: Drag ChatInput
- Chat Display: Drag ChatDisplay  
- Chat Scroll Rect: Drag ScrollView
- Send Button: Drag SendButton
- Toggle Button: Drag ToggleButton
- Chat Panel: Drag ChatPanel
```

## 🎉 Xong! Test ngay!

1. **Play Scene**
2. **Panel chat hiển thị sẵn**
3. **Chat tự động kết nối**
4. **Type message** và **Send**
5. **Thấy tin nhắn real-time**

## ✨ Tính năng:

- ✅ **Vivox Real Chat** - Sử dụng Vivox thật từ package
- ✅ **Global Channel** - Chat global với tất cả người chơi
- ✅ **Auto-connect** - Tự động kết nối khi vào MenuScene
- ✅ **Show by default** - Panel chat hiển thị sẵn
- ✅ **Enter để gửi** - Phím Enter gửi tin nhắn
- ✅ **Real-time messaging** - Tin nhắn real-time
- ✅ **Chat history scroll** - Kéo để xem chat trước đó
- ✅ **Auto-scroll** - Tự động scroll xuống tin nhắn mới

## 🔧 Vivox Settings:

**SimpleChatManager đã được cấu hình sẵn với:**
- ✅ **Server**: https://unity.vivox.com/appconfig/18968-proje-59535-udash
- ✅ **Domain**: mtu1xp.vivox.com
- ✅ **Issuer**: 18968-proje-59535-udash
- ✅ **Key**: 9diWIL6eBlHhlQCQzlu5dRJDIIwyQb2x
- ✅ **Channel**: GlobalChat
- ✅ **Use Mock Chat**: false (sử dụng Vivox thật)
- ✅ **Default Assembly**: Sử dụng Default Assembly để tránh reference conflicts

## 📋 UI Setup chi tiết:

### ChatPanel (Panel):
```
RectTransform: Width=400, Height=300, Position=(20,20,0)
Color: (0,0,0,200) - Semi-transparent
```

### ChatDisplay (Text - TMPro):
```
RectTransform: Width=380, Height=200, Position=(10,80,0)
Text: ""
Font Size: 12
Color: White
```

### ChatInput (InputField - TMPro):
```
RectTransform: Width=300, Height=30, Position=(10,10,0)
Placeholder: "Nhập tin nhắn..."
```

### SendButton (Button):
```
RectTransform: Width=70, Height=30, Position=(320,10,0)
Text: "Send"
```

### ToggleButton (Button):
```
RectTransform: Width=100, Height=40, Position=(20,20,0)
Text: "Chat"
```

## 🎯 Setup MenuSceneManager:

```
Select MenuSceneManager GameObject
Trong MenuSceneManager Inspector:
- Simple Chat Manager: Drag SimpleChat GameObject
```

## ✅ Test Checklist:

- [ ] SimpleChat GameObject created
- [ ] SimpleChatManager script attached
- [ ] ChatPanel with 4 UI elements created
- [ ] All references assigned in Inspector
- [ ] MenuSceneManager reference assigned
- [ ] Play mode test successful
- [ ] Chat opens/closes with button
- [ ] Messages send and display correctly

## 🚀 Ready to Use!

Chat system hoạt động ngay với Vivox thật:
- ✅ **Vivox Package** - Đã có sẵn
- ✅ **Simple setup** - Chỉ 3 bước
- ✅ **Single script** - SimpleChatManager
- ✅ **Real-time chat** - Chat thật với người chơi khác
- ✅ **Auto-connect** - Tự động kết nối khi login

**Chỉ cần 1 script + 5 UI elements + Vivox package!**
