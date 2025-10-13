# 🔧 SETTINGS PANEL SETUP GUIDE

## ✅ FIX LỖI COMPILE

**Lỗi:** `'GlobalSettingsManager' does not contain a definition for 'GetMusicVolume'`

**Đã fix:** ✅ Thêm lại methods `GetMusicVolume()` và `GetSFXVolume()` vào GlobalSettingsManager

---

## 🎯 KIẾN TRÚC HỆ THỐNG

### **Cách hoạt động:**

```
LoginScene
├── GlobalSettingsManager (DontDestroyOnLoad) ← Tạo ở đây
│   └── Sync PopupMusic volume
├── SettingsPanel (Copy từ MenuScene) ← UI riêng cho LoginScene
└── PopupMusic (List music)

MenuScene
├── GlobalSettingsManager (Same instance) ← Không tạo mới
├── SettingsPanel (Copy từ MenuScene) ← UI riêng cho MenuScene
└── AudioManager (Background music)

GameScene
├── GlobalSettingsManager (Same instance) ← Không tạo mới
├── SettingsPanel (Copy từ MenuScene) ← UI riêng cho GameScene
└── AudioManager (Same instance)
```

### **Sync Flow:**

```
User adjusts volume in LoginScene
    ↓
SettingsPanel.musicSlider.onValueChanged
    ↓
PlayerPrefs.SetFloat("SET_MUSIC", value)
    ↓
AudioManager.SetMusicVolume(value) (if exists)
    ↓
PopupMusic.audioSource.volume = value (LoginScene)

User loads MenuScene
    ↓
SettingsPanel.OnEnable()
    ↓
Load from PlayerPrefs.GetFloat("SET_MUSIC")
    ↓
AudioManager.SetMusicVolume(value)
    ↓
Menu music plays at same volume ✅
```

---

## 🚀 SETUP TỪNG SCENE

### **SCENE 1: MenuScene (Đã có sẵn)** ✅

**SettingsPanel đã có:**
- ✅ UI: Sliders, Toggle, Dropdown
- ✅ Script: SettingsPanel.cs
- ✅ Logic: Save/Load PlayerPrefs, Sync AudioManager

**Không cần làm gì!**

---

### **SCENE 2: LoginScene** (5 phút)

**BƯỚC 1: Copy SettingsPanel từ MenuScene** (2 phút)

```
1. Open MenuScene
2. Find SettingsPanel GameObject in Hierarchy
3. Right-click → Copy
4. Open LoginScene
5. Right-click Canvas → Paste
6. ✅ SettingsPanel copied with all children
```

**BƯỚC 2: Create GlobalSettingsManager** (2 phút)

```
1. Create Empty GameObject: "GlobalSettingsManager"
2. Add Component: GlobalSettingsManager
3. Assign references:
   - Settings Panel: Drag SettingsPanel component
   - Button Open Settings: Drag button Settings (tạo mới)
```

**BƯỚC 3: Create Settings Button** (1 phút)

```
1. Create Button: "BtnSettings"
2. Position: Top-right corner
3. Text: "Settings" hoặc icon ⚙️
4. Assign vào GlobalSettingsManager → Button Open Settings
```

**Test:**
```
1. Play LoginScene
2. Click Settings → ✅ Panel opens
3. Adjust Music to 50%
4. ✅ PopupMusic volume = 50%
5. Close Settings
6. Reopen Settings
7. ✅ Music slider shows 50%
```

---

### **SCENE 3: GameScene** (3 phút)

**BƯỚC 1: Copy SettingsPanel từ MenuScene** (2 phút)

```
1. Open MenuScene
2. Find SettingsPanel GameObject
3. Right-click → Copy
4. Open GameScene
5. Right-click Canvas → Paste
6. ✅ SettingsPanel copied
```

**BƯỚC 2: Create Settings Button** (1 phút)

```
1. Create Button: "BtnSettings"
2. Position: Top-right corner (hoặc trong pause menu)
3. Text: "Settings" hoặc icon ⚙️
4. Add onClick event:
   - Target: SettingsPanel
   - Function: SettingsPanel.panelRoot.SetActive(true)
   
   Hoặc dùng code:
   btnSettings.onClick.AddListener(() => {
       settingsPanel.panelRoot.SetActive(true);
   });
```

**Test:**
```
1. Play GameScene
2. Click Settings → ✅ Panel opens
3. Adjust Music to 30%
4. ✅ Game music volume = 30%
5. Load MenuScene
6. ✅ Menu music volume = 30%
```

---

## 📋 CHECKLIST SETUP

### **LoginScene:**
- [ ] Copy SettingsPanel từ MenuScene
- [ ] Create GlobalSettingsManager GameObject
- [ ] Assign SettingsPanel component
- [ ] Create Settings button
- [ ] Assign button to GlobalSettingsManager
- [ ] Test: Volume sync với PopupMusic

### **MenuScene:**
- [x] SettingsPanel đã có sẵn ✅
- [ ] Create AudioManager GameObject (nếu chưa có)
- [ ] Assign audio clips
- [ ] Test: Volume sync với AudioManager

### **GameScene:**
- [ ] Copy SettingsPanel từ MenuScene
- [ ] Create Settings button
- [ ] Add onClick event
- [ ] Test: Volume sync với AudioManager

---

## 🎵 CÁCH SYNC VOLUME GIỮA CÁC SCENE

### **Cách 1: Qua PlayerPrefs (Recommended)** ✅

**Cách hoạt động:**
```
SettingsPanel.musicSlider.onValueChanged
    ↓
PlayerPrefs.SetFloat("SET_MUSIC", value)
    ↓
PlayerPrefs.Save()

Khi load scene mới:
    ↓
SettingsPanel.OnEnable()
    ↓
value = PlayerPrefs.GetFloat("SET_MUSIC", 1f)
    ↓
Apply to AudioManager / PopupMusic
```

**Code trong SettingsPanel.cs (đã có sẵn):**
```csharp
void OnEnable()
{
    LoadPrefs(out float mv, out float sv, out bool fs, out int resIx);
    
    // Sync with AudioManager if exists
    if (AudioManager.Instance != null)
    {
        mv = AudioManager.Instance.GetMusicVolume();
        sv = AudioManager.Instance.GetSFXVolume();
    }
    
    ApplyAll(mv, sv, fs, resIx, applyToSystem:true, save:false);
    RefreshUI(mv, sv, fs, resIx);
}
```

---

### **Cách 2: Qua GlobalSettingsManager (Backup)**

**Cách hoạt động:**
```
GlobalSettingsManager (DontDestroyOnLoad)
    ↓
GetMusicVolume() → PlayerPrefs.GetFloat("SET_MUSIC")
    ↓
Sync PopupMusic khi load LoginScene
```

**Code trong GlobalSettingsManager.cs:**
```csharp
public float GetMusicVolume()
{
    return PlayerPrefs.GetFloat("SET_MUSIC", 1f);
}

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "LoginScene")
    {
        SyncPopupMusicVolume();
    }
}
```

---

## 🧪 TEST VOLUME SYNC

### **Test 1: LoginScene → MenuScene**
```
1. Play LoginScene
2. Settings → Music 40%
3. ✅ PopupMusic volume = 40%
4. Click Start → Load MenuScene
5. ✅ Menu music volume = 40%
6. Settings → Music slider shows 40% ✅
```

### **Test 2: MenuScene → GameScene**
```
1. Play MenuScene
2. Settings → Music 60%
3. ✅ Menu music volume = 60%
4. Start game → Load GameScene
5. ✅ Game music volume = 60%
6. Settings → Music slider shows 60% ✅
```

### **Test 3: GameScene → LoginScene**
```
1. Play GameScene
2. Settings → Music 20%
3. ✅ Game music volume = 20%
4. Logout → Load LoginScene
5. ✅ PopupMusic volume = 20%
6. Settings → Music slider shows 20% ✅
```

### **Test 4: Close & Reopen Game**
```
1. Play LoginScene
2. Settings → Music 50%, SFX 80%
3. Close game
4. Reopen game → Play LoginScene
5. Settings → ✅ Music 50%, SFX 80%
6. ✅ Volume settings persisted
```

---

## 🐛 TROUBLESHOOTING

### **Lỗi 1: Volume không sync giữa scenes**
```
Solution:
1. Check PlayerPrefs keys: "SET_MUSIC", "SET_SFX"
2. Check SettingsPanel.OnEnable() được gọi
3. Check AudioManager.Instance exists
4. Debug.Log volume values
```

### **Lỗi 2: SettingsPanel không hiện**
```
Solution:
1. Check panelRoot.SetActive(true)
2. Check Canvas exists
3. Check SettingsPanel is child of Canvas
4. Check button onClick event
```

### **Lỗi 3: PopupMusic volume không sync**
```
Solution:
1. Check GlobalSettingsManager exists in LoginScene
2. Check OnSceneLoaded event subscribed
3. Check PopupMusic.audioSource exists
4. Debug.Log in SyncPopupMusicVolume()
```

### **Lỗi 4: AudioManager volume không sync**
```
Solution:
1. Check AudioManager.Instance exists
2. Check SettingsPanel.OnEnable() calls AudioManager.SetMusicVolume()
3. Check AudioManager.musicSource.volume
4. Debug.Log in AudioManager.SetMusicVolume()
```

---

## 📁 FILE STRUCTURE

```
LoginScene
├── Canvas
│   ├── SettingsPanel (Copy từ MenuScene)
│   │   ├── MusicSlider
│   │   ├── SFXSlider
│   │   ├── FullscreenToggle
│   │   ├── ResolutionDropdown
│   │   └── CloseButton
│   ├── BtnSettings (New)
│   └── ... other UI
├── GlobalSettingsManager (New)
└── PopupMusic (Existing)

MenuScene
├── Canvas
│   ├── SettingsPanel (Original)
│   └── ... other UI
└── AudioManager (Existing)

GameScene
├── Canvas
│   ├── SettingsPanel (Copy từ MenuScene)
│   ├── BtnSettings (New)
│   └── ... other UI
└── AudioManager (Same instance from MenuScene)
```

---

## 🎯 SUMMARY

**Cách sync volume:**
- ✅ **PlayerPrefs** - Lưu volume khi user thay đổi
- ✅ **SettingsPanel.OnEnable()** - Load volume khi scene load
- ✅ **AudioManager** - Apply volume to background music
- ✅ **GlobalSettingsManager** - Sync PopupMusic (LoginScene)

**Setup cho từng scene:**
- ✅ **LoginScene** - Copy SettingsPanel + Create GlobalSettingsManager (5 phút)
- ✅ **MenuScene** - Đã có sẵn (0 phút)
- ✅ **GameScene** - Copy SettingsPanel + Create Settings button (3 phút)

**Tổng thời gian: 8 phút!**

---

**GO! GO! GO!** 🔥

