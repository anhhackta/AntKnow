# 🔊 AUDIO SYSTEM SETUP GUIDE

## ✅ ĐÃ TẠO GÌ?

### **1. AudioManager (Singleton)** ✅
**File:** `Assets/Scenes/Menu/ManagerAudio.cs` → Renamed to `AudioManager`

**Features:**
- ✅ Singleton pattern (DontDestroyOnLoad)
- ✅ Auto switch music based on scene
- ✅ Volume control (save/load from PlayerPrefs)
- ✅ Background music: Menu, Game
- ✅ Sound effects: Button click, Notification, Start, Bounce, Profit, Loss

### **2. AudioHelper** ✅
**File:** `Assets/Scenes/Menu/AudioHelper.cs`

**Features:**
- ✅ Auto play click sound on Button component
- ✅ Public methods to call from Unity Events

### **3. Updated Files** ✅
- ✅ `SettingsPanel.cs` - Sync with AudioManager
- ✅ `AuthUIController.cs` - Play notification & start sounds
- ✅ `AvatarPanel.cs` - Show gender sprite & player name

---

## 🎵 AUDIO STRUCTURE

### **Background Music:**
```
LoginScene        → PopupMusic (list music) - Không dùng AudioManager
MenuScene         → menuMusic (AudioManager)
SelectCharacter   → menuMusic (AudioManager)
LoadingScene      → menuMusic (AudioManager)
GameScene         → gameMusic (AudioManager)
```

### **Sound Effects:**
```
btnClickSound      → Mọi button khi click
notificationSound  → PanelNotification hiện lên
startSound         → Click button Start để vào game
bounceSound        → Nhảy qua 1 ô tile (in game)
profitSound        → Được thêm tiền (in game)
lossSound          → Bị trừ tiền (in game)
```

---

## 🚀 UNITY SETUP (15 PHÚT)

### **BƯỚC 1: Create AudioManager GameObject** (2 phút)

**Trong MenuScene (hoặc bất kỳ scene nào):**

```
1. Create Empty GameObject: "AudioManager"
2. Add Component: AudioManager (script)
3. AudioManager sẽ tự động:
   - Create 2 AudioSources (music, sfx)
   - DontDestroyOnLoad (persist across scenes)
   - Auto play menu music
```

---

### **BƯỚC 2: Assign Audio Clips** (5 phút)

**Trong Unity Inspector (AudioManager component):**

```
[Background Music]
├── Menu Music      → Drag audio clip (menu background music)
└── Game Music      → Drag audio clip (game background music)

[Sound Effects]
├── Btn Click Sound      → Drag audio clip (button click)
├── Notification Sound   → Drag audio clip (notification)
├── Start Sound          → Drag audio clip (start game)
├── Bounce Sound         → Drag audio clip (bounce/jump)
├── Profit Sound         → Drag audio clip (gain money)
└── Loss Sound           → Drag audio clip (lose money)

[Volume Settings]
├── Music Volume    → 0.7 (default)
└── SFX Volume      → 1.0 (default)
```

**Nếu chưa có audio clips:**
```
1. Tạo folder: Assets/Audio/
2. Tạo subfolders:
   - Assets/Audio/Music/
   - Assets/Audio/SFX/
3. Import audio files vào folders
4. Assign vào AudioManager
```

---

### **BƯỚC 3: Add AudioHelper to Buttons** (5 phút)

**Option 1: Auto play click sound (RECOMMENDED)**

```
1. Select button GameObject
2. Add Component: AudioHelper
3. Check "Play Click Sound On Button" ✅
4. Done! Button sẽ tự động play click sound
```

**Option 2: Manual setup**

```
1. Select button GameObject
2. In Button component → OnClick()
3. Add new event:
   - Target: AudioManager (GameObject)
   - Function: AudioManager.PlayButtonClick()
```

**Apply to all buttons:**
```
- Login button
- Register button
- Create Account button
- Back to Login button
- Close button
- Exit button
- Start button
- Settings button
- All menu buttons
```

---

### **BƯỚC 4: Setup Settings Panel** (3 phút)

**Trong MenuScene:**

```
1. Tìm SettingsPanel GameObject
2. SettingsPanel component đã được update
3. Khi user thay đổi volume:
   - SettingsPanel sẽ tự động sync với AudioManager
   - AudioManager sẽ save volume vào PlayerPrefs
```

**Test:**
```
1. Open Settings Panel
2. Adjust Music slider → Music volume changes
3. Adjust SFX slider → SFX volume changes
4. Close game → Reopen → Volume settings saved ✅
```

---

### **BƯỚC 5: Update AvatarPanel** (2 phút)

**Trong LoginScene:**

```
1. Tìm AvatarPanel GameObject
2. AvatarPanel component:

[Gender Sprites]
├── Male Avatar Sprite    → Drag male avatar sprite
├── Female Avatar Sprite  → Drag female avatar sprite
└── Default Avatar Sprite → Drag default sprite (if no gender)

3. AvatarPanel sẽ tự động:
   - Show male sprite nếu gender = "male" hoặc "nam"
   - Show female sprite nếu gender = "female" hoặc "nữ"
   - Show default sprite nếu chưa set gender
   - Show player name (hoặc "New Player" nếu chưa có)
```

---

## 🎮 USAGE IN CODE

### **Play Background Music:**

```csharp
// Auto play based on scene (AudioManager handles this)
// No need to call manually

// Manual control (if needed):
AudioManager.Instance.PlayMenuMusic();
AudioManager.Instance.PlayGameMusic();
AudioManager.Instance.StopMusic();
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();
```

### **Play Sound Effects:**

```csharp
// Button click
AudioManager.Instance.PlayButtonClick();

// Notification
AudioManager.Instance.PlayNotification();

// Start game
AudioManager.Instance.PlayStart();

// In game sounds
AudioManager.Instance.PlayBounce();   // Jump to tile
AudioManager.Instance.PlayProfit();   // Gain money
AudioManager.Instance.PlayLoss();     // Lose money
```

### **Volume Control:**

```csharp
// Set volume
AudioManager.Instance.SetMusicVolume(0.7f);  // 0.0 - 1.0
AudioManager.Instance.SetSFXVolume(1.0f);    // 0.0 - 1.0

// Get volume
float musicVol = AudioManager.Instance.GetMusicVolume();
float sfxVol = AudioManager.Instance.GetSFXVolume();
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
├── Scenes/
│   ├── Login/
│   │   ├── AuthUIController.cs (UPDATED)
│   │   └── AvatarPanel.cs (UPDATED)
│   └── Menu/
│       ├── ManagerAudio.cs → AudioManager (REFACTORED)
│       ├── AudioHelper.cs (NEW)
│       └── SettingsPanel.cs (UPDATED)
```

---

## 🧪 TEST CASES

### **Test 1: AudioManager Singleton**
```
1. Play MenuScene
2. Check Console: "AudioManager initialized"
3. Check Hierarchy: AudioManager (DontDestroyOnLoad)
4. Load GameScene
5. Check: AudioManager still exists (not destroyed)
✅ Singleton works
```

### **Test 2: Auto Music Switching**
```
1. Play MenuScene
2. ✅ Menu music plays
3. Load GameScene
4. ✅ Game music plays (menu music stops)
5. Load LoginScene
6. ✅ Music stops (LoginScene uses PopupMusic)
```

### **Test 3: Button Click Sound**
```
1. Play LoginScene
2. Click any button
3. ✅ Click sound plays
```

### **Test 4: Notification Sound**
```
1. Play LoginScene
2. Login with wrong password
3. ✅ Notification sound plays
4. ✅ Notification shows for 2s
```

### **Test 5: Start Sound**
```
1. Login successfully
2. Click Start button
3. ✅ Start sound plays
4. ✅ LoadingScene loads
```

### **Test 6: Volume Control**
```
1. Open Settings Panel
2. Adjust Music slider to 50%
3. ✅ Music volume changes
4. Adjust SFX slider to 80%
5. ✅ SFX volume changes
6. Close game → Reopen
7. ✅ Volume settings saved
```

### **Test 7: AvatarPanel Gender Sprite**
```
1. Login with male account
2. ✅ Male avatar sprite shows
3. Logout → Login with female account
4. ✅ Female avatar sprite shows
5. Login with new account (no gender)
6. ✅ Default avatar sprite shows
7. ✅ Player name shows (or "New Player")
```

---

## 🐛 TROUBLESHOOTING

### **Lỗi 1: No sound plays**
```
Solution:
1. Check AudioManager exists in scene
2. Check audio clips assigned in Inspector
3. Check volume > 0
4. Check AudioListener exists in scene
```

### **Lỗi 2: Music doesn't switch between scenes**
```
Solution:
1. Check AudioManager has DontDestroyOnLoad
2. Check OnSceneLoaded event is subscribed
3. Check scene names match (MenuScene, GameScene, LoginScene)
```

### **Lỗi 3: Button click sound doesn't play**
```
Solution:
1. Check AudioHelper component on button
2. Check "Play Click Sound On Button" is checked
3. Check btnClickSound assigned in AudioManager
```

### **Lỗi 4: Volume settings not saved**
```
Solution:
1. Check PlayerPrefs.Save() is called
2. Check keys: "MusicVolume", "SFXVolume"
3. Clear PlayerPrefs and test again
```

### **Lỗi 5: AvatarPanel doesn't show gender sprite**
```
Solution:
1. Check maleAvatarSprite and femaleAvatarSprite assigned
2. Check userData.gender value ("male", "female", "nam", "nữ")
3. Check Console log: "Avatar Panel updated for user: ..."
```

---

## 📞 NEXT STEPS

**Sau khi setup:**

1. ✅ Create AudioManager GameObject
2. ✅ Assign all audio clips
3. ✅ Add AudioHelper to buttons
4. ✅ Test all 7 test cases
5. ✅ Assign gender sprites to AvatarPanel

**Sau khi test OK:**

1. ✅ Import real audio files
2. ✅ Adjust volume levels
3. ✅ Test in build (not just Editor)

---

## 🎯 SUMMARY

**Đã tạo:**
- ✅ AudioManager (Singleton, DontDestroyOnLoad)
- ✅ AudioHelper (Auto play click sound)
- ✅ Updated SettingsPanel (Sync with AudioManager)
- ✅ Updated AuthUIController (Play notification & start sounds)
- ✅ Updated AvatarPanel (Show gender sprite & player name)

**Chưa làm (bạn cần làm):**
- [ ] Create AudioManager GameObject (2 phút)
- [ ] Assign audio clips (5 phút)
- [ ] Add AudioHelper to buttons (5 phút)
- [ ] Setup SettingsPanel (3 phút)
- [ ] Update AvatarPanel sprites (2 phút)

**Tổng thời gian: 17 phút!**

---

**GO! GO! GO!** 🔥

