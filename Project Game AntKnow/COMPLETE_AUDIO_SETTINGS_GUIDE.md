# 🔊 COMPLETE AUDIO & SETTINGS SYSTEM GUIDE

## ✅ ĐÃ TẠO GÌ?

### **1. GlobalSettingsManager (Singleton)** ✅
**File:** `Assets/Scenes/Menu/GlobalSettingsManager.cs`

**Features:**
- ✅ **Singleton** (DontDestroyOnLoad) - Xuyên suốt game
- ✅ **Settings Panel** - Hiện ở Login, Menu, Game
- ✅ **Volume sync** - Login 50% → Menu 50% → Game 50%
- ✅ **Sync với AudioManager** - Background music
- ✅ **Sync với PopupMusic** - Login scene music
- ✅ **Display settings** - Fullscreen, Resolution

### **2. AudioManager (Singleton)** ✅
**File:** `Assets/Scenes/Menu/ManagerAudio.cs` → `AudioManager`

**Features:**
- ✅ Background music (Menu, Game)
- ✅ Sound effects (6 sounds)
- ✅ Auto switch music based on scene
- ✅ Volume control

### **3. AudioHelper** ✅
**File:** `Assets/Scenes/Menu/AudioHelper.cs`

**Features:**
- ✅ Auto play click sound on Button
- ✅ Easy to use

### **4. Unity Editor Tool** ✅
**File:** `Assets/Editor/AddAudioHelperToButtons.cs`

**Features:**
- ✅ Auto add AudioHelper to ALL buttons in scene
- ✅ One-click solution
- ✅ Menu: `AntKnow → Audio → Add AudioHelper to All Buttons`

### **5. Updated Files** ✅
- ✅ `AuthUIController.cs` - Show AvatarPanel after login
- ✅ `AvatarPanel.cs` - Show gender sprite & player name
- ✅ `PopupMusic.cs` - Sync with GlobalSettingsManager
- ✅ `PanelNotification.cs` (Game) - Play notification sound

---

## 🎯 SYSTEM ARCHITECTURE

### **Volume Sync Flow:**
```
GlobalSettingsManager (Singleton)
    ↓
    ├─→ AudioManager.SetMusicVolume()
    ├─→ AudioManager.SetSFXVolume()
    ├─→ PopupMusic.audioSource.volume
    └─→ PlayerPrefs (Save)

Login Scene (50% volume)
    ↓
Menu Scene (50% volume) ← Sync
    ↓
Game Scene (50% volume) ← Sync
```

### **Settings Panel Flow:**
```
LoginScene
    ├─→ GlobalSettingsManager (DontDestroyOnLoad)
    └─→ Settings Panel (visible)

MenuScene
    └─→ GlobalSettingsManager (same instance)
        └─→ Settings Panel (same panel)

GameScene
    └─→ GlobalSettingsManager (same instance)
        └─→ Settings Panel (same panel)
```

---

## 🚀 UNITY SETUP (20 PHÚT)

### **BƯỚC 1: Create GlobalSettingsManager** (3 phút)

**Trong LoginScene:**

```
1. Create Empty GameObject: "GlobalSettingsManager"
2. Add Component: GlobalSettingsManager
3. Create Settings Panel UI:
   - Create Panel: "SettingsPanel"
   - Add Slider: "MusicSlider"
   - Add Slider: "SFXSlider"
   - Add Text: "MusicValueText" (hiển thị %)
   - Add Text: "SFXValueText" (hiển thị %)
   - Add Toggle: "FullscreenToggle"
   - Add Dropdown: "ResolutionDropdown"
   - Add Button: "CloseButton"
```

**Assign references:**
```
GlobalSettingsManager component:
├── Settings Panel Root: SettingsPanel
├── Button Open Settings: (button để mở settings)
├── Button Close Settings: CloseButton
├── Music Slider: MusicSlider
├── SFX Slider: SFXSlider
├── Music Value Text: MusicValueText
├── SFX Value Text: SFXValueText
├── Fullscreen Toggle: FullscreenToggle
└── Resolution Dropdown: ResolutionDropdown
```

---

### **BƯỚC 2: Create AudioManager** (2 phút)

**Trong MenuScene (hoặc LoginScene):**

```
1. Create Empty GameObject: "AudioManager"
2. Add Component: AudioManager
3. Assign audio clips (như hướng dẫn trước)
```

---

### **BƯỚC 3: Add Settings Button to LoginScene** (2 phút)

**Trong LoginScene:**

```
1. Create Button: "BtnSettings"
2. Position: Top-right corner
3. Text: "Settings" hoặc icon ⚙️
4. Assign onClick:
   - Target: GlobalSettingsManager
   - Function: GlobalSettingsManager.OpenSettings()
```

---

### **BƯỚC 4: Auto Add AudioHelper to All Buttons** (1 phút)

**Trong Unity Editor:**

```
1. Open LoginScene
2. Menu → AntKnow → Audio → Add AudioHelper to All Buttons
3. Click "Add AudioHelper to All Buttons"
4. ✅ Done! All buttons now have AudioHelper

Repeat for:
- MenuScene
- GameScene
- SelectCharacterScene
```

---

### **BƯỚC 5: Update AvatarPanel Sprites** (2 phút)

**Trong LoginScene:**

```
1. Find AvatarPanel GameObject
2. AvatarPanel component:

[Gender Sprites]
├── Male Avatar Sprite: Drag male sprite
├── Female Avatar Sprite: Drag female sprite
└── Default Avatar Sprite: Drag default sprite
```

---

## 🎵 VOLUME SYNC EXPLAINED

### **Scenario 1: User adjusts volume in LoginScene**

```
1. User opens Settings Panel in LoginScene
2. Adjusts Music slider to 50%
3. GlobalSettingsManager:
   - Saves to PlayerPrefs
   - Updates AudioManager (if exists)
   - Updates PopupMusic.audioSource.volume
4. User goes to MenuScene
5. GlobalSettingsManager (same instance):
   - Loads from PlayerPrefs (50%)
   - Updates AudioManager.menuMusic volume (50%)
6. User goes to GameScene
7. GlobalSettingsManager (same instance):
   - AudioManager.gameMusic volume (50%)
```

### **Scenario 2: User adjusts volume in MenuScene**

```
1. User opens Settings Panel in MenuScene
2. Adjusts SFX slider to 80%
3. GlobalSettingsManager:
   - Saves to PlayerPrefs
   - Updates AudioManager.SetSFXVolume(0.8)
4. User goes to GameScene
5. All SFX sounds play at 80% volume
```

---

## 🧪 TEST CASES

### **Test 1: Settings Panel Xuyên Suốt**
```
1. Play LoginScene
2. Click Settings button
3. ✅ Settings Panel opens
4. Adjust Music to 50%
5. Close Settings
6. Load MenuScene
7. Click Settings button
8. ✅ Music slider shows 50%
9. Load GameScene
10. Click Settings button
11. ✅ Music slider still shows 50%
```

### **Test 2: Volume Sync (Login → Menu)**
```
1. Play LoginScene
2. Open Settings → Music 30%
3. ✅ PopupMusic volume = 30%
4. Load MenuScene
5. ✅ Menu music volume = 30%
```

### **Test 3: Volume Sync (Menu → Game)**
```
1. Play MenuScene
2. Open Settings → SFX 70%
3. Click any button
4. ✅ Click sound at 70% volume
5. Load GameScene
6. Click any button
7. ✅ Click sound at 70% volume
```

### **Test 4: All Buttons Have Click Sound**
```
1. Play LoginScene
2. Click Login button → ✅ Click sound
3. Click Register button → ✅ Click sound
4. Click Settings button → ✅ Click sound
5. Load MenuScene
6. Click any button → ✅ Click sound
7. Load GameScene
8. Click any button → ✅ Click sound
```

### **Test 5: AvatarPanel Shows After Login**
```
1. Play LoginScene
2. Login successfully
3. ✅ Notification "Login successful!" (2s)
4. ✅ Notification hides
5. ✅ PanelLog hides
6. ✅ AvatarPanel shows with:
   - Gender sprite (male/female)
   - Player name (or "New Player")
7. ✅ BtnLog shows (Logout sprite)
8. ✅ BtnStart shows
```

### **Test 6: Notification Sound**
```
1. Play LoginScene
2. Login with wrong password
3. ✅ Notification sound plays
4. Load GameScene
5. Trigger any notification
6. ✅ Notification sound plays
```

---

## 📁 FILE STRUCTURE

```
Assets/
├── Audio/
│   ├── Music/
│   │   ├── menu_music.mp3
│   │   └── game_music.mp3
│   └── SFX/
│       ├── btn_click.wav
│       ├── notification.wav
│       ├── start.wav
│       ├── bounce.wav
│       ├── profit.wav
│       └── loss.wav
│
├── Editor/
│   └── AddAudioHelperToButtons.cs (NEW)
│
├── Scenes/
│   ├── Login/
│   │   ├── AuthUIController.cs (UPDATED)
│   │   ├── AvatarPanel.cs (UPDATED)
│   │   └── PopupMusic.cs (UPDATED)
│   ├── Menu/
│   │   ├── AudioManager.cs (REFACTORED)
│   │   ├── AudioHelper.cs (NEW)
│   │   └── GlobalSettingsManager.cs (NEW)
│   └── Game/
│       └── Scripts/UI/
│           └── PanelNotification.cs (UPDATED)
```

---

## 🎯 SUMMARY

**Đã tạo:**
- ✅ GlobalSettingsManager (Singleton, DontDestroyOnLoad)
- ✅ Settings Panel xuyên suốt game
- ✅ Volume sync (Login → Menu → Game)
- ✅ Unity Editor Tool (Auto add AudioHelper)
- ✅ Updated 5 files

**Chưa làm (bạn cần làm):**
- [ ] Create GlobalSettingsManager GameObject (3 phút)
- [ ] Create Settings Panel UI (5 phút)
- [ ] Create AudioManager GameObject (2 phút)
- [ ] Add Settings button to LoginScene (2 phút)
- [ ] Run Unity Editor Tool (1 phút × 4 scenes = 4 phút)
- [ ] Update AvatarPanel sprites (2 phút)
- [ ] Test all 6 test cases (6 phút)

**Tổng thời gian: 24 phút!**

---

**GO! GO! GO!** 🔥

