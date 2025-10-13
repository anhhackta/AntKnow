# ✅ FINAL FIX - LOADING & SELECT CHARACTER SCENE

## 🎯 YÊU CẦU CỦA BẠN

### **1. SelectCharacterScene - Kiểm tra điều kiện TRƯỚC khi hiện UI**
> "Có thể kiểm tra điều kiện trong SelectCharacterScene mà nhanh hoặc ít nhất không hiện ra SelectCharacterScene khi kiểm tra điều kiện nếu có rồi bỏ qua, chưa tìm bắt nhập"

**Giải pháp:**
- ✅ Check profile trong `Awake()` (TRƯỚC khi scene hiện)
- ✅ Nếu có đủ Name + Gender → Load MenuScene NGAY, không hiện UI
- ✅ Nếu thiếu → Tiếp tục hiện UI để user nhập

---

### **2. Music - LoadingScene và SelectCharacterScene KHÔNG có âm thanh**
> "Về music 2 scene loading SelectCharacter này sẽ không có âm thanh nha"

**Giải pháp:**
- ✅ AudioManager stop music khi load LoadingScene
- ✅ AudioManager stop music khi load SelectCharacterScene
- ✅ Chỉ MenuScene và GameScene có music

---

### **3. LoadingScene - Reusable cho Menu → Game**
> "Về LoadingScene sau này có thể tận dụng để dùng khi MenuScene vào GameScene nên có thể ngắn gọn cho nó xử lí ổn việc này"

**Giải pháp:**
- ✅ Static configuration: `sourceScene`, `targetScene`, `checkProfile`
- ✅ Helper method: `LoadingSceneController.LoadWithConfig()`
- ✅ Có thể dùng cho: Login → Menu, Menu → Game, Game → Menu

---

## ✅ CODE CHANGES

### **1. SelectCharacterController.cs** ✅

**Before:**
```csharp
private void Start()
{
    InitializeSelectCharacterScene();
}

private async void InitializeSelectCharacterScene()
{
    // ... setup UI
    
    // Check profile AFTER UI is visible
    if (hasIngameName && hasGender)
    {
        SceneManager.LoadScene("MenuScene");
        return;
    }
}
```

**After:**
```csharp
private void Awake()
{
    // CRITICAL: Check profile BEFORE scene is visible
    gameDataManager = GameDataManager.Instance;

    // Check if user already has BOTH ingame name AND gender
    bool hasIngameName = !string.IsNullOrEmpty(gameDataManager.currentIngameName);
    bool hasGender = !string.IsNullOrEmpty(gameDataManager.currentGender);

    if (hasIngameName && hasGender)
    {
        // User has complete profile, skip this scene immediately
        Debug.Log($"SelectCharacterScene: User already has complete profile, skipping to MenuScene");
        SceneManager.LoadScene("MenuScene");
        return;
    }

    // User needs to select character, continue to Start()
    Debug.Log($"SelectCharacterScene: User needs to complete profile");
}

private void Start()
{
    InitializeSelectCharacterScene();
}

private async void InitializeSelectCharacterScene()
{
    // ... setup UI (only runs if profile is incomplete)
}
```

**Kết quả:**
- ✅ Check profile trong `Awake()` → TRƯỚC khi scene hiện
- ✅ Nếu có đủ → Load MenuScene ngay, **KHÔNG hiện UI**
- ✅ Nếu thiếu → Tiếp tục `Start()` để hiện UI

---

### **2. ManagerAudio.cs** ✅

**Before:**
```csharp
case "MenuScene":
case "SelectCharacterScene":
case "LoadingScene":
    PlayMenuMusic();
    break;
```

**After:**
```csharp
case "MenuScene":
    PlayMenuMusic();
    break;

case "GameScene":
    PlayGameMusic();
    break;

case "LoginScene":
case "LoadingScene":
case "SelectCharacterScene":
    // These scenes have no music (LoginScene uses PopupMusic)
    StopMusic();
    break;
```

**Kết quả:**
- ✅ LoadingScene → No music
- ✅ SelectCharacterScene → No music
- ✅ MenuScene → Menu music
- ✅ GameScene → Game music

---

### **3. LoadingSceneController.cs** ✅

**Added static configuration:**
```csharp
// Static configuration for reusable loading
public static string sourceScene = "LoginScene";  // Where we came from
public static string targetScene = "MenuScene";   // Where we're going
public static bool checkProfile = true;           // Check ingame name + gender?
```

**Updated LoadMenuSceneAsync():**
```csharp
// Determine next scene based on configuration
string nextScene = targetScene;

// If checkProfile is enabled, verify user has complete profile
if (checkProfile)
{
    bool hasIngameName = !string.IsNullOrEmpty(GameDataManager.Instance.currentIngameName);
    bool hasGender = !string.IsNullOrEmpty(GameDataManager.Instance.currentGender);

    if (hasIngameName && hasGender)
    {
        // User has complete profile, go to target scene
        nextScene = targetScene;
    }
    else
    {
        // User needs to select character
        nextScene = "SelectCharacterScene";
    }
}
else
{
    // No profile check, go directly to target scene
    Debug.Log($"LoadingScene: Loading {nextScene} (no profile check)");
}

SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
```

**Added helper methods:**
```csharp
/// <summary>
/// Configure LoadingScene before loading it
/// </summary>
public static void Configure(string source, string target, bool checkUserProfile = false)
{
    sourceScene = source;
    targetScene = target;
    checkProfile = checkUserProfile;
}

/// <summary>
/// Load LoadingScene with configuration
/// Example: LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false);
/// </summary>
public static void LoadWithConfig(string source, string target, bool checkUserProfile = false)
{
    Configure(source, target, checkUserProfile);
    SceneManager.LoadScene("LoadingScene");
}
```

---

## 🎵 USAGE EXAMPLES

### **Example 1: Login → Menu (with profile check)**
```csharp
// In LoginScene (AuthUIController.cs)
// Default configuration already set:
// sourceScene = "LoginScene"
// targetScene = "MenuScene"
// checkProfile = true

SceneManager.LoadScene("LoadingScene");
```

---

### **Example 2: Menu → Game (no profile check)**
```csharp
// In MenuScene (when user clicks "Start Game" button)
LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", checkUserProfile: false);
```

---

### **Example 3: Game → Menu (no profile check)**
```csharp
// In GameScene (when user clicks "Back to Menu" button)
LoadingSceneController.LoadWithConfig("GameScene", "MenuScene", checkUserProfile: false);
```

---

## 🎵 FLOW DIAGRAM

### **Scenario 1: Login → Menu (có đủ Name + Gender)**
```
LoginScene
    ↓
LoadingScene (checkProfile = true)
    ↓
Check: Name = "hoang1", Gender = "male"
    ↓
hasIngameName = true, hasGender = true
    ↓
Fade out
    ↓
Load MenuScene ✅
```

---

### **Scenario 2: Login → Menu (thiếu Gender)**
```
LoginScene
    ↓
LoadingScene (checkProfile = true)
    ↓
Check: Name = "hoang1", Gender = ""
    ↓
hasIngameName = true, hasGender = false
    ↓
Fade out
    ↓
Load SelectCharacterScene
    ↓
Awake() checks profile
    ↓
hasIngameName = true, hasGender = false
    ↓
Continue to Start() → Show UI ✅
    ↓
User selects gender → Confirm
    ↓
MenuScene
```

---

### **Scenario 3: Menu → Game**
```
MenuScene
    ↓
User clicks "Start Game"
    ↓
LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false)
    ↓
LoadingScene (checkProfile = false)
    ↓
No profile check
    ↓
Fade out
    ↓
Load GameScene ✅
```

---

## 🧪 TEST CASES

### **Test 1: SelectCharacterScene không hiện khi có đủ profile**
```
1. Login với account có Name = "hoang1", Gender = "male"
2. LoadingScene shows
3. ✅ LoadingScene fades out
4. ✅ SelectCharacterScene KHÔNG hiện (check trong Awake)
5. ✅ MenuScene loads trực tiếp
6. Check Console:
   ✅ "SelectCharacterScene: User already has complete profile, skipping to MenuScene"
```

---

### **Test 2: SelectCharacterScene hiện khi thiếu profile**
```
1. Login với account có Name = "hoang1", Gender = ""
2. LoadingScene shows
3. ✅ LoadingScene fades out
4. ✅ SelectCharacterScene loads
5. ✅ Awake() checks profile → Incomplete
6. ✅ Start() runs → UI shows
7. ✅ InputField pre-filled: "hoang1"
8. User selects gender → Confirm
9. ✅ MenuScene loads
```

---

### **Test 3: No music in LoadingScene & SelectCharacterScene**
```
1. LoginScene → LoadingScene
2. ✅ PopupMusic stops
3. ✅ No music plays in LoadingScene
4. LoadingScene → SelectCharacterScene
5. ✅ No music plays in SelectCharacterScene
6. SelectCharacterScene → MenuScene
7. ✅ Menu music starts
```

---

### **Test 4: Reusable LoadingScene (Menu → Game)**
```
1. MenuScene
2. Click "Start Game" button
3. Code: LoadingSceneController.LoadWithConfig("MenuScene", "GameScene", false)
4. ✅ LoadingScene shows
5. ✅ No profile check (checkProfile = false)
6. ✅ LoadingScene fades out
7. ✅ GameScene loads
8. ✅ Game music plays
```

---

## 📁 FILES MODIFIED

### **1. SelectCharacterController.cs** ✅
**Changes:**
- Moved profile check from `Start()` to `Awake()`
- Check profile BEFORE scene is visible
- Skip to MenuScene immediately if profile is complete

### **2. ManagerAudio.cs** ✅
**Changes:**
- LoadingScene → No music (StopMusic)
- SelectCharacterScene → No music (StopMusic)
- MenuScene → Menu music
- GameScene → Game music

### **3. LoadingSceneController.cs** ✅
**Changes:**
- Added static configuration: `sourceScene`, `targetScene`, `checkProfile`
- Updated `LoadMenuSceneAsync()` to use configuration
- Added `Configure()` method
- Added `LoadWithConfig()` helper method

---

## 🎯 SUMMARY

**Vấn đề 1: SelectCharacterScene hiện ra dù có đủ profile**
- ❌ Check profile trong `Start()` → UI đã hiện
- ✅ Check profile trong `Awake()` → TRƯỚC khi UI hiện

**Vấn đề 2: LoadingScene & SelectCharacterScene có music**
- ❌ Play menu music
- ✅ Stop music (no music)

**Vấn đề 3: LoadingScene không reusable**
- ❌ Hard-coded cho Login → Menu
- ✅ Static configuration + helper methods

**Kết quả:**
- ✅ SelectCharacterScene không hiện nếu có đủ profile
- ✅ LoadingScene & SelectCharacterScene không có music
- ✅ LoadingScene reusable cho Menu → Game, Game → Menu

---

**GO! GO! GO!** 🔥

