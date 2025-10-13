# 🔊 FINAL AUDIO & SETTINGS GUIDE

## ✅ ĐÃ TẠO GÌ?

### **1. GlobalSettingsManager (Singleton)** ✅
**File:** `Assets/Scenes/Menu/GlobalSettingsManager.cs` (101 lines)

**Features:**
- ✅ **Singleton** (DontDestroyOnLoad) - Xuyên suốt game
- ✅ **Sử dụng SettingsPanel hiện có** - Không tạo UI mới
- ✅ **Sync PopupMusic** - LoginScene music volume
- ✅ **Open Settings button** - Mở SettingsPanel

**Cách hoạt động:**
```
GlobalSettingsManager (DontDestroyOnLoad)
    ↓
    ├─→ SettingsPanel (UI hiện có)
    │   ├─→ AudioManager.SetMusicVolume()
    │   ├─→ AudioManager.SetSFXVolume()
    │   └─→ PlayerPrefs.Save()
    └─→ PopupMusic.audioSource.volume (LoginScene)

LoginScene (50% volume)
    ↓
MenuScene (50% volume) ← SettingsPanel sync
    ↓
GameScene (50% volume) ← SettingsPanel sync
```

---

### **2. SettingsPanel (Đã có sẵn)** ✅
**File:** `Assets/Scenes/Menu/SettingsPanel.cs`

**Features:**
- ✅ Music slider (với icon thay đổi <=30% / >30%)
- ✅ SFX slider
- ✅ Fullscreen toggle
- ✅ Resolution dropdown (4 options)
- ✅ AudioMixer support (hoặc fallback AudioSource)
- ✅ PlayerPrefs save/load
- ✅ Sync với AudioManager

**Đã update:**
- ✅ Sync với AudioManager khi OnEnable
- ✅ Update AudioManager khi slider thay đổi

---

### **3. Updated Files** ✅
- ✅ `AuthUIController.cs` - Show AvatarPanel after login
- ✅ `AvatarPanel.cs` - Show gender sprite & player name
- ✅ `PopupMusic.cs` - Sync with GlobalSettingsManager
- ✅ `PanelNotification.cs` (Game) - Play notification sound

---

## 🎯 CÁCH THÊM ÂM THANH VÀO CODE

### **Nguyên tắc:**
- ❌ **KHÔNG** auto add AudioHelper vào tất cả buttons
- ✅ **Thêm thủ công** vào code khi cần
- ✅ **Kiểm soát** âm thanh cho từng button cụ thể

---

### **Cách 1: Thêm vào Button onClick (Recommended)**

**Ví dụ: AuthUIController.cs**

```csharp
private void SetupEventListeners()
{
    // Login button
    buttonLogin.onClick.AddListener(() =>
    {
        // Play click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        // Original logic
        OnLoginClicked();
    });
    
    // Register button
    buttonCreateAccount.onClick.AddListener(() =>
    {
        // Play click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        // Original logic
        OnRegisterClicked();
    });
    
    // Settings button
    buttonOpenSettings.onClick.AddListener(() =>
    {
        // Play click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        // Original logic
        GlobalSettingsManager.Instance.OpenSettings();
    });
}
```

---

### **Cách 2: Thêm vào method xử lý**

**Ví dụ: PanelBuy.cs**

```csharp
private void OnBuyClicked()
{
    // Play click sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayButtonClick();
    
    // Original logic
    if (currentProperty == null) return;
    // ... buy logic
}

private void OnSkipClicked()
{
    // Play click sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayButtonClick();
    
    // Original logic
    onSkip?.Invoke();
    gameObject.SetActive(false);
}
```

---

### **Cách 3: Sử dụng AudioHelper (Cho buttons đơn giản)**

**Khi nào dùng:**
- Button chỉ cần click sound, không có logic phức tạp
- Button trong prefab, không muốn sửa code

**Cách dùng:**
```
1. Select button GameObject
2. Add Component → AudioHelper
3. Check "Play Click Sound On Button" ✅
4. Done!
```

**Ví dụ buttons nên dùng AudioHelper:**
- Close button
- Back button
- Simple navigation buttons

**Ví dụ buttons KHÔNG nên dùng AudioHelper:**
- Login button (có logic phức tạp)
- Buy button (có validation)
- Skill button (có cooldown check)

---

### **Danh sách âm thanh có sẵn:**

```csharp
// Background Music
AudioManager.Instance.PlayMenuMusic();
AudioManager.Instance.PlayGameMusic();
AudioManager.Instance.StopMusic();
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();

// Sound Effects
AudioManager.Instance.PlayButtonClick();    // Button click
AudioManager.Instance.PlayNotification();   // Notification panel
AudioManager.Instance.PlayStart();          // Start game
AudioManager.Instance.PlayBounce();         // Jump to tile (in game)
AudioManager.Instance.PlayProfit();         // Gain money (in game)
AudioManager.Instance.PlayLoss();           // Lose money (in game)
```

---

### **Ví dụ thêm âm thanh vào các file:**

#### **1. AuthUIController.cs** ✅ (Đã thêm)

```csharp
// Notification sound
private void ShowNotification(string message, bool isError = false)
{
    // ... set text, color
    
    // Play notification sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayNotification();
    
    // ... show panel
}

// Start sound
private void OnStartButtonClicked()
{
    // Play start sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayStart();
    
    SceneManager.LoadScene("LoadingScene");
}
```

#### **2. PanelNotification.cs (Game)** ✅ (Đã thêm)

```csharp
public void ShowNotification(string message)
{
    // ... set text
    
    // Play notification sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayNotification();
    
    // ... show panel
}
```

#### **3. PanelBuy.cs** (Chưa thêm - Ví dụ)

```csharp
private void OnBuyClicked()
{
    // Play click sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayButtonClick();
    
    // ... buy logic
}

private void OnHouseButtonClicked(int level)
{
    // Play click sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayButtonClick();
    
    // ... house button logic
}
```

#### **4. PlayerGameController.cs** (Chưa thêm - Ví dụ)

```csharp
private void OnTileReached(int tileIndex)
{
    // Play bounce sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayBounce();
    
    // ... tile logic
}

private void OnMoneyGained(int amount)
{
    // Play profit sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayProfit();
    
    // ... update money
}

private void OnMoneyLost(int amount)
{
    // Play loss sound
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayLoss();
    
    // ... update money
}
```

---

## 🚀 UNITY SETUP (10 PHÚT)

### **BƯỚC 1: Setup GlobalSettingsManager** (3 phút)

**Trong LoginScene:**

```
1. Create Empty GameObject: "GlobalSettingsManager"
2. Add Component: GlobalSettingsManager
3. Assign references:
   - Settings Panel: Drag SettingsPanel component (đã có sẵn)
   - Button Open Settings: Drag button Settings (tạo mới)
```

**Tạo Settings Button:**
```
1. Create Button: "BtnSettings"
2. Position: Top-right corner
3. Text: "Settings" hoặc icon ⚙️
4. Assign vào GlobalSettingsManager
```

---

### **BƯỚC 2: Setup AudioManager** (2 phút)

**Trong MenuScene:**

```
1. Create Empty GameObject: "AudioManager"
2. Add Component: AudioManager
3. Assign audio clips:
   - Menu Music
   - Game Music
   - 6 SFX sounds
```

---

### **BƯỚC 3: Update AvatarPanel** (2 phút)

**Trong LoginScene:**

```
1. Find AvatarPanel GameObject
2. Assign sprites:
   - Male Avatar Sprite
   - Female Avatar Sprite
   - Default Avatar Sprite
```

---

### **BƯỚC 4: Thêm âm thanh vào code** (3 phút)

**Chọn buttons cần âm thanh:**

```
✅ Login button → Thêm vào code
✅ Register button → Thêm vào code
✅ Start button → Đã có
✅ Settings button → Thêm vào code
✅ Close buttons → Dùng AudioHelper
✅ Buy button → Thêm vào code
✅ Skip button → Thêm vào code
```

**Thêm code:**
```csharp
// Trong onClick listener hoặc method xử lý
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayButtonClick();
```

---

## 🧪 TEST CASES

### **Test 1: Settings Panel Xuyên Suốt**
```
1. Play LoginScene
2. Click Settings button → ✅ SettingsPanel opens
3. Adjust Music to 50%
4. ✅ PopupMusic volume = 50%
5. Load MenuScene
6. Click Settings button → ✅ Same panel, Music = 50%
7. Load GameScene
8. Click Settings button → ✅ Same panel, Music = 50%
```

### **Test 2: Volume Sync**
```
1. LoginScene → Settings → Music 30%
2. ✅ PopupMusic volume = 30%
3. MenuScene → ✅ Menu music = 30%
4. GameScene → ✅ Game music = 30%
5. Close game → Reopen
6. ✅ All music still at 30%
```

### **Test 3: Button Click Sounds**
```
1. LoginScene → Click Login button → ✅ Sound
2. Click Settings button → ✅ Sound
3. MenuScene → Click any button → ✅ Sound (nếu đã thêm code)
4. GameScene → Click any button → ✅ Sound (nếu đã thêm code)
```

### **Test 4: AvatarPanel Shows After Login**
```
1. Login successfully
2. ✅ Notification "Login successful!" (2s) + sound
3. ✅ PanelLog hides
4. ✅ AvatarPanel shows with gender sprite
5. ✅ BtnLog shows (Logout sprite)
6. ✅ BtnStart shows
```

---

## 📁 FILE STRUCTURE

```
Assets/
├── Scenes/
│   ├── Login/
│   │   ├── AuthUIController.cs (UPDATED - Show AvatarPanel)
│   │   ├── AvatarPanel.cs (UPDATED - Gender sprites)
│   │   └── PopupMusic.cs (UPDATED - Sync volume)
│   ├── Menu/
│   │   ├── AudioManager.cs (Existing)
│   │   ├── AudioHelper.cs (Existing)
│   │   ├── SettingsPanel.cs (Existing - Đã sync với AudioManager)
│   │   └── GlobalSettingsManager.cs (NEW - 101 lines)
│   └── Game/
│       └── Scripts/UI/
│           └── PanelNotification.cs (UPDATED - Play sound)
```

---

## 🎯 SUMMARY

**Đã tạo:**
- ✅ GlobalSettingsManager (Singleton, sử dụng SettingsPanel hiện có)
- ✅ Sync PopupMusic volume
- ✅ Updated 4 files

**Chưa làm (bạn cần làm):**
- [ ] Create GlobalSettingsManager GameObject (3 phút)
- [ ] Create Settings button (2 phút)
- [ ] Setup AudioManager (2 phút)
- [ ] Update AvatarPanel sprites (2 phút)
- [ ] Thêm âm thanh vào code (3 phút)
- [ ] Test (3 phút)

**Tổng thời gian: 15 phút!**

---

**GO! GO! GO!** 🔥

