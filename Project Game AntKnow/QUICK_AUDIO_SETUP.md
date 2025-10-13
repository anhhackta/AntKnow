# 🚀 QUICK AUDIO SETUP (15 PHÚT)

## ✅ ĐÃ TẠO CODE!

Tôi đã tạo hệ thống âm thanh hoàn chỉnh:

1. ✅ **AudioManager** - Singleton quản lý tất cả âm thanh
2. ✅ **AudioHelper** - Auto play click sound cho buttons
3. ✅ **SettingsPanel** - Sync với AudioManager
4. ✅ **AuthUIController** - Play notification & start sounds
5. ✅ **AvatarPanel** - Show gender sprite & player name

---

## 🎯 BẠN CẦN LÀM GÌ?

### **BƯỚC 1: Create AudioManager** (2 phút)

**Trong MenuScene:**

```
1. Hierarchy → Right-click → Create Empty
2. Rename: "AudioManager"
3. Add Component → AudioManager (script)
4. Done! AudioManager sẽ tự động:
   - Create 2 AudioSources
   - DontDestroyOnLoad
   - Auto play menu music
```

---

### **BƯỚC 2: Assign Audio Clips** (5 phút)

**Trong AudioManager Inspector:**

```
[Background Music]
Menu Music: Drag audio clip
Game Music: Drag audio clip

[Sound Effects]
Btn Click Sound:      Drag audio clip
Notification Sound:   Drag audio clip
Start Sound:          Drag audio clip
Bounce Sound:         Drag audio clip (in game)
Profit Sound:         Drag audio clip (in game)
Loss Sound:           Drag audio clip (in game)
```

**Nếu chưa có audio clips:**
```
1. Tạo folder: Assets/Audio/Music/ và Assets/Audio/SFX/
2. Import audio files
3. Assign vào AudioManager
```

---

### **BƯỚC 3: Add AudioHelper to Buttons** (5 phút)

**Cách nhanh nhất:**

```
1. Select button (Login, Register, Start, Exit, etc.)
2. Add Component → AudioHelper
3. Check "Play Click Sound On Button" ✅
4. Done!
```

**Apply to all buttons:**
- Login button
- Register button
- Create Account button
- Back to Login button
- Close button
- Exit button
- Start button
- All menu buttons

---

### **BƯỚC 4: Update AvatarPanel** (3 phút)

**Trong LoginScene:**

```
1. Tìm AvatarPanel GameObject
2. AvatarPanel component:

[Gender Sprites]
Male Avatar Sprite:    Drag male sprite
Female Avatar Sprite:  Drag female sprite
Default Avatar Sprite: Drag default sprite

3. Done! AvatarPanel sẽ tự động show sprite theo gender
```

---

## 🧪 TEST NHANH (2 phút)

```
1. Play MenuScene
   ✅ Menu music plays

2. Click any button
   ✅ Click sound plays

3. Load GameScene
   ✅ Game music plays

4. Open Settings → Adjust volume
   ✅ Volume changes

5. Login → Click Start
   ✅ Start sound plays

6. Login with male/female account
   ✅ Correct avatar sprite shows
```

---

## 📋 AUDIO CLIPS NEEDED

### **Background Music (2 files):**
```
menu_music.mp3  → Menu/SelectCharacter/Loading scenes
game_music.mp3  → Game scene
```

### **Sound Effects (6 files):**
```
btn_click.wav      → Button click
notification.wav   → Notification panel
start.wav          → Start game button
bounce.wav         → Jump to tile (in game)
profit.wav         → Gain money (in game)
loss.wav           → Lose money (in game)
```

---

## 🎵 AUDIO FLOW

### **Scene Music:**
```
LoginScene        → PopupMusic (list music)
MenuScene         → AudioManager.menuMusic
SelectCharacter   → AudioManager.menuMusic
LoadingScene      → AudioManager.menuMusic
GameScene         → AudioManager.gameMusic
```

### **Sound Effects:**
```
Button click      → AudioManager.PlayButtonClick()
Notification      → AudioManager.PlayNotification()
Start game        → AudioManager.PlayStart()
Jump to tile      → AudioManager.PlayBounce()
Gain money        → AudioManager.PlayProfit()
Lose money        → AudioManager.PlayLoss()
```

---

## 🔧 CODE USAGE

### **Play Sound Effects:**

```csharp
// In any script:
AudioManager.Instance.PlayButtonClick();
AudioManager.Instance.PlayNotification();
AudioManager.Instance.PlayStart();
AudioManager.Instance.PlayBounce();
AudioManager.Instance.PlayProfit();
AudioManager.Instance.PlayLoss();
```

### **Control Music:**

```csharp
// Auto play based on scene (no need to call manually)

// Manual control (if needed):
AudioManager.Instance.PlayMenuMusic();
AudioManager.Instance.PlayGameMusic();
AudioManager.Instance.StopMusic();
```

### **Volume Control:**

```csharp
// Set volume
AudioManager.Instance.SetMusicVolume(0.7f);
AudioManager.Instance.SetSFXVolume(1.0f);

// Get volume
float musicVol = AudioManager.Instance.GetMusicVolume();
float sfxVol = AudioManager.Instance.GetSFXVolume();
```

---

## 🐛 TROUBLESHOOTING

### **No sound plays:**
```
1. Check AudioManager exists
2. Check audio clips assigned
3. Check volume > 0
4. Check AudioListener in scene
```

### **Music doesn't switch:**
```
1. Check AudioManager has DontDestroyOnLoad
2. Check scene names match
```

### **Button click doesn't work:**
```
1. Check AudioHelper on button
2. Check "Play Click Sound On Button" checked
3. Check btnClickSound assigned
```

---

## 📁 FILES CREATED/MODIFIED

### **Created:**
1. ✅ `Assets/Scenes/Menu/AudioHelper.cs` (NEW)
2. ✅ `AUDIO_SYSTEM_SETUP.md` (Full documentation)
3. ✅ `QUICK_AUDIO_SETUP.md` (This file)

### **Modified:**
1. ✅ `Assets/Scenes/Menu/ManagerAudio.cs` → `AudioManager` (Refactored)
2. ✅ `Assets/Scenes/Menu/SettingsPanel.cs` (Sync with AudioManager)
3. ✅ `Assets/Scenes/Login/AuthUIController.cs` (Play sounds)
4. ✅ `Assets/Scenes/Login/AvatarPanel.cs` (Gender sprites)

---

## 📞 BÁO LẠI KẾT QUẢ

**Sau khi làm xong, cho tôi biết:**

1. ✅ AudioManager created?
2. ✅ Audio clips assigned?
3. ✅ AudioHelper added to buttons?
4. ✅ AvatarPanel sprites assigned?
5. ✅ Test OK? (music plays, sounds play, volume control works)

**Nếu OK → Tiếp tục Phase 1: Inventory & Loadout!**

**Nếu lỗi → Gửi screenshot Console logs!**

---

## 🎯 SUMMARY

**Đã làm:**
- ✅ AudioManager (Singleton, DontDestroyOnLoad)
- ✅ Auto switch music based on scene
- ✅ 6 sound effects (click, notification, start, bounce, profit, loss)
- ✅ Volume control (save/load)
- ✅ AvatarPanel gender sprites

**Chưa làm (bạn cần làm):**
- [ ] Create AudioManager GameObject (2 phút)
- [ ] Assign audio clips (5 phút)
- [ ] Add AudioHelper to buttons (5 phút)
- [ ] Update AvatarPanel sprites (3 phút)

**Tổng thời gian: 15 phút!**

---

**MỞ FILE:** `AUDIO_SYSTEM_SETUP.md` (Chi tiết đầy đủ)

**GO! GO! GO!** 🔥

