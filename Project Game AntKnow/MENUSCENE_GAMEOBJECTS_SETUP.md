# 🎮 MENUSCENE - DANH SÁCH GAMEOBJECTS CẦN TẠO

## 🎯 MỤC ĐÍCH

Tạo tất cả GameObjects trống + Components cần thiết cho MenuScene

---

## 📋 DANH SÁCH GAMEOBJECTS

### **1. SERVICES (Root Level - DontDestroyOnLoad)**

```
MenuScene (Root)
├── UGSAuthService (Empty GameObject)
│   └── Component: UGSAuthService.cs
├── MatchmakerService (Empty GameObject)
│   └── Component: MatchmakerService.cs
├── CustomLobbyService (Empty GameObject)
│   └── Component: CustomLobbyService.cs (LobbyService.cs)
├── RelayService (Empty GameObject)
│   └── Component: RelayService.cs
├── InventoryService (Empty GameObject)
│   └── Component: InventoryService.cs
└── SimpleChatManager (Empty GameObject)
    └── Component: SimpleChatManager.cs
```

**Cách tạo:**
```
1. Hierarchy → Right-click → Create Empty
2. Rename: "UGSAuthService"
3. Add Component: UGSAuthService
4. Repeat cho các services khác
```

---

### **2. MANAGERS (Root Level)**

```
MenuScene (Root)
├── MenuSceneManager (Empty GameObject)
│   └── Component: MenuSceneManager.cs
├── LobbyUIManager (Empty GameObject)
│   └── Component: LobbyUIManager.cs
└── GameDataManager (Empty GameObject - Singleton từ LoginScene)
    └── Component: GameDataManager.cs
```

---

### **3. CANVAS - UI HIERARCHY**

```
Canvas
├── PanelHome (Panel)
│   ├── Component: PanelHome.cs
│   ├── CharacterImage (Image)
│   ├── ButtonFindMatch (Button)
│   ├── ButtonCustomRoom (Button)
│   └── ButtonWaitGame (Button - Hidden by default)
│       └── TextWaitTimer (Text)
│
├── PanelInventory (Panel)
│   └── Component: InventoryUIManager.cs
│
├── PanelUpgrade (Panel)
│
├── PanelShop (Panel)
│
├── PanelCustomRoom (Empty GameObject - Container)
│   ├── PanelContainer (Panel)
│   │   ├── PanelRoom (Panel - List phòng)
│   │   │   ├── ButtonCreateRoom (Button)
│   │   │   ├── ButtonResetList (Button)
│   │   │   └── ScrollView (Scroll View)
│   │   │       └── Viewport → Content (roomListContainer)
│   │   │
│   │   ├── PanelCreateRoom (Panel - Popup tạo phòng)
│   │   │   ├── ButtonCloseCreateRoom (Button)
│   │   │   ├── InputRoomName (InputField)
│   │   │   └── ButtonConfirmCreate (Button)
│   │   │
│   │   └── PanelJoinRoom (Panel - Trong phòng)
│   │       ├── TextRoomName (Text)
│   │       ├── TextPlayerCount (Text)
│   │       ├── ScrollView (Scroll View)
│   │       │   └── Viewport → Content (playerListContainer)
│   │       ├── ButtonLeaveRoom (Button)
│   │       └── ButtonStartGame (Button)
│   │
│   └── ButtonClosePanelCustomRoom (Button)
│
├── PanelMatchNotification (Empty GameObject)
│   ├── Component: PanelMatchNotification.cs
│   └── NotificationText (Text - TMP)
│
├── SubmenuPlay (Empty GameObject)
│   ├── Component: SubmenuPlay.cs
│   └── SubmenuPanel (Panel - Hidden by default)
│       ├── ButtonFindMatch (Button)
│       └── ButtonCreateLobby (Button)
│
├── SettingsPanel (Panel)
│   └── Component: SettingsPanel.cs
│
└── TopBar (Panel)
    ├── PanelAvatar (Panel)
    │   └── Component: PanelAvatar.cs
    ├── PanelMoney (Panel)
    │   └── Component: PanelMoney.cs
    └── ButtonSettings (Button)
```

---

## 🚀 CÁCH TẠO NHANH (30 PHÚT)

### **BƯỚC 1: Tạo Services (10 phút)**

```
1. Create Empty GameObject: "UGSAuthService"
2. Add Component: UGSAuthService
3. Repeat:
   - MatchmakerService
   - CustomLobbyService
   - RelayService
   - InventoryService
   - SimpleChatManager
```

---

### **BƯỚC 2: Tạo Managers (5 phút)**

```
1. Create Empty GameObject: "MenuSceneManager"
2. Add Component: MenuSceneManager
3. Create Empty GameObject: "LobbyUIManager"
4. Add Component: LobbyUIManager
```

---

### **BƯỚC 3: Tạo PanelMatchNotification (2 phút)**

```
1. Canvas → Create Empty → "PanelMatchNotification"
2. Add Component: PanelMatchNotification.cs
3. Create child: Text - TextMeshPro → "NotificationText"
4. Assign:
   - PanelMatchNotification → Notification Text: NotificationText
```

---

### **BƯỚC 4: Tạo SubmenuPlay (5 phút)**

```
1. Canvas → Create Empty → "SubmenuPlay"
2. Add Component: SubmenuPlay.cs
3. Create child: Panel → "SubmenuPanel"
4. Create children of SubmenuPanel:
   - Button → "ButtonFindMatch" (Text: "🔍 Tìm trận")
   - Button → "ButtonCreateLobby" (Text: "🏠 Tạo lobby")
5. SubmenuPanel: SetActive(false)
6. Assign references in SubmenuPlay
```

---

### **BƯỚC 5: Tạo PanelCustomRoom (10 phút)**

```
1. Canvas → Create Empty → "PanelCustomRoom"
2. Create child: Panel → "PanelContainer"
3. Create 3 children of PanelContainer:
   
   A. PanelRoom:
      - Panel → "PanelRoom"
      - Button → "ButtonCreateRoom"
      - Button → "ButtonResetList"
      - ScrollView → Viewport → Content (roomListContainer)
   
   B. PanelCreateRoom:
      - Panel → "PanelCreateRoom"
      - Button → "ButtonCloseCreateRoom"
      - InputField → "InputRoomName"
      - Button → "ButtonConfirmCreate"
   
   C. PanelJoinRoom:
      - Panel → "PanelJoinRoom"
      - Text → "TextRoomName"
      - Text → "TextPlayerCount"
      - ScrollView → Viewport → Content (playerListContainer)
      - Button → "ButtonLeaveRoom"
      - Button → "ButtonStartGame"

4. Create child of PanelCustomRoom:
   - Button → "ButtonClosePanelCustomRoom"

5. PanelCustomRoom: SetActive(false)
```

---

## 📝 ASSIGN REFERENCES

### **MenuSceneManager:**
```
Main Panels:
- mainPanel: (existing panel)
- panelSliderManager: (existing)
- settingsPanel: SettingsPanel
- panelRoom: PanelCustomRoom

Panel Components:
- panelHome: PanelHome

Services:
- firebaseAuthService: (existing)

Simple Chat:
- simpleChatManager: SimpleChatManager
```

---

### **LobbyUIManager:**
```
Main Container:
- panelCustomRoom: PanelCustomRoom
- buttonClosePanelCustomRoom: ButtonClosePanelCustomRoom

Panel Container:
- panelContainer: PanelContainer

3 Panel Con:
- panelRoom: PanelRoom
- panelCreateRoom: PanelCreateRoom
- panelJoinRoom: PanelJoinRoom

PanelRoom:
- buttonCreateRoom: ButtonCreateRoom
- buttonResetList: ButtonResetList
- roomListContainer: Content (trong ScrollView)
- roomItemPrefab: RoomItemPrefabs.prefab

PanelCreateRoom:
- buttonCloseCreateRoom: ButtonCloseCreateRoom
- inputRoomName: InputRoomName
- buttonConfirmCreate: ButtonConfirmCreate

PanelJoinRoom:
- textRoomName: TextRoomName
- textPlayerCount: TextPlayerCount
- playerListContainer: Content (trong ScrollView)
- playerItemPrefab: PlayerItemPrefab.prefab
- buttonLeaveRoom: ButtonLeaveRoom
- buttonStartGame: ButtonStartGame
```

---

### **PanelHome:**
```
Character Display:
- characterImage: (existing)

Action Buttons:
- buttonFindMatch: ButtonFindMatch (hoặc null nếu dùng SubmenuPlay)
- buttonCustomRoom: ButtonCustomRoom (hoặc null nếu dùng SubmenuPlay)

Matchmaking UI:
- buttonWaitGame: ButtonWaitGame
- textWaitTimer: TextWaitTimer

References:
- lobbyUIManager: LobbyUIManager
- panelMatchNotification: PanelMatchNotification
```

---

### **SubmenuPlay:**
```
UI Components:
- buttonPlay: (main Play button)
- submenuPanel: SubmenuPanel
- buttonFindMatch: ButtonFindMatch
- buttonCreateLobby: ButtonCreateLobby

References:
- panelHome: PanelHome
```

---

### **PanelMatchNotification:**
```
UI Components:
- notificationText: NotificationText (TMP_Text)

Settings:
- autoHideDuration: 3
```

---

## 🧪 VERIFY SETUP

### **Checklist Services:**
- [ ] UGSAuthService GameObject exists
- [ ] MatchmakerService GameObject exists
- [ ] CustomLobbyService GameObject exists
- [ ] RelayService GameObject exists
- [ ] InventoryService GameObject exists
- [ ] SimpleChatManager GameObject exists

### **Checklist Managers:**
- [ ] MenuSceneManager GameObject exists
- [ ] LobbyUIManager GameObject exists
- [ ] All references assigned

### **Checklist UI:**
- [ ] PanelMatchNotification exists
- [ ] SubmenuPlay exists
- [ ] PanelCustomRoom exists (3 panels inside)
- [ ] All buttons assigned

### **Checklist Prefabs:**
- [ ] RoomItemPrefabs.prefab exists
- [ ] PlayerItemPrefab.prefab exists

---

## 🎯 SUMMARY

**Tạo:**
- ✅ 6 Services (Empty GameObjects + Components)
- ✅ 2 Managers (Empty GameObjects + Components)
- ✅ PanelMatchNotification (Empty + Text)
- ✅ SubmenuPlay (Empty + Panel + 2 Buttons)
- ✅ PanelCustomRoom (Empty + 3 Panels)

**Assign:**
- ✅ MenuSceneManager references
- ✅ LobbyUIManager references
- ✅ PanelHome references
- ✅ SubmenuPlay references

**Thời gian:**
- ✅ 30 phút

---

**LÀM NGAY!** 🔥

