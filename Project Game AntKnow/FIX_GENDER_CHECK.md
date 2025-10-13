# 🔧 FIX GENDER CHECK - 2 ĐIỀU KIỆN

## ✅ ĐÃ FIX VẤN ĐỀ

### **Vấn đề bạn nêu:**
> "Tôi xóa thử gender để demo mà lỗi này bỏ qua luôn SelectCharacterScene. Nói lại SelectCharacterScene kiểm tra 2 điều kiện: null tên ingame VÀ null giới tính gender. Nếu thiếu 1 trong 2 cần SelectCharacterScene liền"

### **Log bạn gặp:**
```
MenuScene: User data loaded from Firebase - Gender: , IngameName: hoang1
```
→ Gender rỗng nhưng vẫn bỏ qua SelectCharacterScene ❌

---

## ✅ GIẢI PHÁP

### **Check CẢ 2 điều kiện:**
- ✅ `currentIngameName` không null/empty
- ✅ `currentGender` không null/empty
- ✅ **Nếu thiếu 1 trong 2** → Load SelectCharacterScene

---

## 📋 CODE CHANGES

### **1. LoadingSceneController.cs** ✅

**Before:**
```csharp
// Chỉ check ingame name
bool hasGender = !string.IsNullOrEmpty(GameDataManager.Instance.currentIngameName);

if (hasGender)
{
    SceneManager.LoadScene("MenuScene");
}
```

**After:**
```csharp
// Check CẢ 2 điều kiện
bool hasIngameName = !string.IsNullOrEmpty(GameDataManager.Instance.currentIngameName);
bool hasGender = !string.IsNullOrEmpty(GameDataManager.Instance.currentGender);

if (hasIngameName && hasGender)
{
    // User has BOTH ingame name and gender
    Debug.Log($"LoadingScene: User has complete profile (Name: {GameDataManager.Instance.currentIngameName}, Gender: {GameDataManager.Instance.currentGender}), loading MenuScene");
    SceneManager.LoadScene("MenuScene");
}
else
{
    // User needs to select character (missing ingame name or gender)
    Debug.Log($"LoadingScene: User needs to select character (Name: {GameDataManager.Instance.currentIngameName}, Gender: {GameDataManager.Instance.currentGender}), loading SelectCharacterScene");
    SceneManager.LoadScene("SelectCharacterScene");
}
```

---

### **2. SelectCharacterController.cs** ✅

**Before:**
```csharp
// Chỉ check ingame name
if (!string.IsNullOrEmpty(gameDataManager.currentIngameName))
{
    SceneManager.LoadScene("MenuScene");
    return;
}
```

**After:**
```csharp
// Check CẢ 2 điều kiện
bool hasIngameName = !string.IsNullOrEmpty(gameDataManager.currentIngameName);
bool hasGender = !string.IsNullOrEmpty(gameDataManager.currentGender);

if (hasIngameName && hasGender)
{
    Debug.Log($"SelectCharacterScene: User already has complete profile (Name: {gameDataManager.currentIngameName}, Gender: {gameDataManager.currentGender}), going to MenuScene");
    SceneManager.LoadScene("MenuScene");
    return;
}

// Pre-fill ingame name if exists
if (hasIngameName && inputIngameName != null)
{
    inputIngameName.text = gameDataManager.currentIngameName;
    Debug.Log($"SelectCharacterScene: Pre-filled ingame name: {gameDataManager.currentIngameName}");
}

// Pre-select gender if exists
if (hasGender)
{
    selectedGender = gameDataManager.currentGender;
    UpdateSpotlightSelection();
    Debug.Log($"SelectCharacterScene: Pre-selected gender: {gameDataManager.currentGender}");
}
```

**Bonus:**
- ✅ **Pre-fill ingame name** nếu đã có (user chỉ cần chọn gender)
- ✅ **Pre-select gender** nếu đã có (user chỉ cần nhập name)

---

### **3. MenuSceneManager.cs** ✅

**Before:**
```csharp
// Chỉ check ingame name
if (gameDataManager.NeedsIngameNameSetup())
{
    SceneManager.LoadScene("SelectCharacterScene");
    return;
}
```

**After:**
```csharp
// Check CẢ 2 điều kiện
bool hasIngameName = !string.IsNullOrEmpty(gameDataManager.currentIngameName);
bool hasGender = !string.IsNullOrEmpty(gameDataManager.currentGender);

if (!hasIngameName || !hasGender)
{
    Debug.LogWarning($"MenuScene: User missing profile data (Name: {gameDataManager.currentIngameName}, Gender: {gameDataManager.currentGender}), redirecting to SelectCharacterScene");
    SceneManager.LoadScene("SelectCharacterScene");
    return;
}
```

---

## 🎵 FLOW DIAGRAM

### **Scenario 1: Có cả Name và Gender**
```
LoadingScene
    ↓
Check: Name = "hoang1", Gender = "male"
    ↓
hasIngameName = true, hasGender = true
    ↓
MenuScene ✅ (Bỏ qua SelectCharacterScene)
```

---

### **Scenario 2: Có Name, KHÔNG có Gender** (Bạn demo)
```
LoadingScene
    ↓
Check: Name = "hoang1", Gender = ""
    ↓
hasIngameName = true, hasGender = false
    ↓
SelectCharacterScene ✅
    ↓
Pre-fill name: "hoang1"
User chỉ cần chọn gender
    ↓
Confirm → MenuScene
```

---

### **Scenario 3: Có Gender, KHÔNG có Name**
```
LoadingScene
    ↓
Check: Name = "", Gender = "male"
    ↓
hasIngameName = false, hasGender = true
    ↓
SelectCharacterScene ✅
    ↓
Pre-select gender: "male" (spotlight yellow)
User chỉ cần nhập name
    ↓
Confirm → MenuScene
```

---

### **Scenario 4: KHÔNG có cả Name và Gender**
```
LoadingScene
    ↓
Check: Name = "", Gender = ""
    ↓
hasIngameName = false, hasGender = false
    ↓
SelectCharacterScene ✅
    ↓
User chọn gender + nhập name
    ↓
Confirm → MenuScene
```

---

## 🧪 TEST CASES

### **Test 1: Có cả Name và Gender**
```
1. Login với account có Name = "hoang1", Gender = "male"
2. ✅ LoadingScene shows
3. ✅ SelectCharacterScene KHÔNG hiện
4. ✅ MenuScene loads trực tiếp
5. Check Console:
   ✅ "LoadingScene: User has complete profile (Name: hoang1, Gender: male), loading MenuScene"
```

---

### **Test 2: Có Name, KHÔNG có Gender** (Demo của bạn)
```
1. Xóa gender trong Firebase (Gender = "")
2. Login với account có Name = "hoang1", Gender = ""
3. ✅ LoadingScene shows
4. ✅ SelectCharacterScene hiện
5. ✅ InputField pre-filled: "hoang1"
6. ✅ User chỉ cần chọn gender
7. Click male model → Confirm
8. ✅ MenuScene loads
9. Check Console:
   ✅ "LoadingScene: User needs to select character (Name: hoang1, Gender: ), loading SelectCharacterScene"
   ✅ "SelectCharacterScene: Pre-filled ingame name: hoang1"
```

---

### **Test 3: Có Gender, KHÔNG có Name**
```
1. Xóa ingameName trong Firebase (Name = "")
2. Login với account có Name = "", Gender = "male"
3. ✅ LoadingScene shows
4. ✅ SelectCharacterScene hiện
5. ✅ Male spotlight yellow (pre-selected)
6. ✅ User chỉ cần nhập name
7. Enter name → Confirm
8. ✅ MenuScene loads
9. Check Console:
   ✅ "LoadingScene: User needs to select character (Name: , Gender: male), loading SelectCharacterScene"
   ✅ "SelectCharacterScene: Pre-selected gender: male"
```

---

### **Test 4: KHÔNG có cả Name và Gender**
```
1. Xóa cả ingameName và gender trong Firebase
2. Login với account có Name = "", Gender = ""
3. ✅ LoadingScene shows
4. ✅ SelectCharacterScene hiện
5. ✅ InputField empty
6. ✅ No spotlight selected (default male)
7. User chọn gender + nhập name → Confirm
8. ✅ MenuScene loads
```

---

### **Test 5: MenuScene Redirect**
```
1. Somehow vào MenuScene với incomplete profile
2. ✅ MenuScene checks profile
3. ✅ Redirect to SelectCharacterScene
4. Check Console:
   ✅ "MenuScene: User missing profile data (Name: ..., Gender: ...), redirecting to SelectCharacterScene"
```

---

## 📁 FILES MODIFIED

### **1. LoadingSceneController.cs** ✅
**Changes:**
- Check CẢ 2 điều kiện: `hasIngameName && hasGender`
- Debug log hiển thị cả Name và Gender

### **2. SelectCharacterController.cs** ✅
**Changes:**
- Check CẢ 2 điều kiện: `hasIngameName && hasGender`
- Pre-fill ingame name nếu đã có
- Pre-select gender nếu đã có

### **3. MenuSceneManager.cs** ✅
**Changes:**
- Check CẢ 2 điều kiện: `!hasIngameName || !hasGender`
- Debug log hiển thị cả Name và Gender

---

## 🎯 SUMMARY

**Vấn đề:**
- ❌ Chỉ check `currentIngameName`
- ❌ Bỏ qua SelectCharacterScene khi có Name nhưng không có Gender

**Đã fix:**
- ✅ Check CẢ 2 điều kiện: `currentIngameName` VÀ `currentGender`
- ✅ Nếu thiếu 1 trong 2 → Load SelectCharacterScene
- ✅ Pre-fill/Pre-select nếu đã có 1 trong 2

**Logic:**
```
if (hasIngameName && hasGender)
{
    // Có đủ cả 2 → MenuScene
}
else
{
    // Thiếu 1 trong 2 → SelectCharacterScene
}
```

**Bonus:**
- ✅ Pre-fill ingame name nếu đã có
- ✅ Pre-select gender nếu đã có
- ✅ User chỉ cần điền phần còn thiếu

---

**GO! GO! GO!** 🔥

