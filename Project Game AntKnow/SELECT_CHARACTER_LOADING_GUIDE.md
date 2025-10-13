# 🎮 SELECT CHARACTER & LOADING SCENE GUIDE

## ✅ ĐÃ FIX & IMPROVE

### **1. SelectCharacterScene** ✅

**Changes:**
- ✅ **Spotlight thay vì Cube** - Đẹp hơn, chuyên nghiệp hơn
- ✅ **CapsuleCollider** - Phù hợp với character models hơn BoxCollider
- ✅ **Spotlight colors** - Yellow (selected), White (normal)
- ✅ **Spotlight intensity** - 3.0 (selected), 1.0 (normal)

**Code:**
```csharp
[Header("Selection Spotlights")]
[SerializeField] private Light maleSpotlight;
[SerializeField] private Light femaleSpotlight;
[SerializeField] private Color selectedSpotlightColor = Color.yellow;
[SerializeField] private Color normalSpotlightColor = Color.white;
[SerializeField] private float selectedSpotlightIntensity = 3f;
[SerializeField] private float normalSpotlightIntensity = 1f;
```

---

### **2. LoadingScene - Smart Gender Check** ✅

**Problem:**
> "Khi login lần sau vì nó game phải mở SelectCharacterScene kiểm tra có gender chưa bỏ qua vậy làm xấu và có 1 khoảng hiện ra SelectCharacterScene dù đã có gender"

**Solution:**
- ✅ **Check gender TRƯỚC khi load scene**
- ✅ **Nếu có gender** → Load MenuScene trực tiếp
- ✅ **Nếu chưa có gender** → Load SelectCharacterScene
- ✅ **Không hiện SelectCharacterScene** nếu đã có gender

**Code:**
```csharp
// Check if user has gender (ingame name)
bool hasGender = !string.IsNullOrEmpty(GameDataManager.Instance.currentIngameName);

if (hasGender)
{
    // User already has gender, go directly to MenuScene
    Debug.Log("LoadingScene: User has gender, loading MenuScene");
    SceneManager.LoadScene("MenuScene");
}
else
{
    // User needs to select character
    Debug.Log("LoadingScene: User needs to select character, loading SelectCharacterScene");
    SceneManager.LoadScene("SelectCharacterScene");
}
```

---

### **3. MenuSceneManager - Fix PanelHome Error** ✅

**Problem:**
```
MenuScene: PanelHome is null! Please assign it in the inspector.
```

**Solution:**
- ✅ **Auto-find PanelHome** nếu null
- ✅ **Warning thay vì Error** - Không crash game
- ✅ **Fallback logic** - Tìm trong scene nếu chưa assign

**Code:**
```csharp
if (panelHome != null)
{
    panelHome.ForceUpdateCharacterSprite();
}
else
{
    Debug.LogWarning("MenuScene: PanelHome is null! Trying to find it...");
    panelHome = FindObjectOfType<PanelHome>();
    if (panelHome != null)
    {
        panelHome.ForceUpdateCharacterSprite();
    }
}
```

---

## 🚀 UNITY SETUP

### **SCENE 1: SelectCharacterScene** (5 phút)

**BƯỚC 1: Xóa Selection Cubes** (1 phút)
```
1. Find maleSelectionCube GameObject
2. Delete
3. Find femaleSelectionCube GameObject
4. Delete
```

**BƯỚC 2: Thêm Spotlights** (3 phút)
```
1. Create Spotlight: "MaleSpotlight"
   - Position: Above male model (e.g., 0, 3, -2)
   - Rotation: (50, 0, 0) - Point down at model
   - Color: White
   - Intensity: 1.0
   - Range: 10
   - Spot Angle: 30

2. Create Spotlight: "FemaleSpotlight"
   - Position: Above female model (e.g., 3, 3, -2)
   - Rotation: (50, 0, 0)
   - Color: White
   - Intensity: 1.0
   - Range: 10
   - Spot Angle: 30
```

**BƯỚC 3: Update SelectCharacterController** (1 phút)
```
1. Find SelectCharacterController GameObject
2. SelectCharacterController component:
   - Male Spotlight: Drag MaleSpotlight
   - Female Spotlight: Drag FemaleSpotlight
   - Selected Spotlight Color: Yellow (R:1, G:1, B:0)
   - Normal Spotlight Color: White (R:1, G:1, B:1)
   - Selected Spotlight Intensity: 3
   - Normal Spotlight Intensity: 1
```

**BƯỚC 4: Update Character Models** (Optional)
```
1. Select maleCharacterModel
2. Remove BoxCollider (if exists)
3. CapsuleCollider will be added automatically by script

4. Select femaleCharacterModel
5. Remove BoxCollider (if exists)
6. CapsuleCollider will be added automatically by script
```

---

### **SCENE 2: LoadingScene** (0 phút)

**Không cần làm gì!**
- ✅ Code đã tự động check gender
- ✅ Tự động skip SelectCharacterScene nếu đã có gender

---

### **SCENE 3: MenuScene** (1 phút)

**Fix PanelHome reference:**
```
1. Find MenuSceneManager GameObject
2. MenuSceneManager component:
   - Panel Home: Drag PanelHome GameObject (nếu chưa assign)
```

---

## 🎵 FLOW DIAGRAM

### **Login Flow (Lần đầu - Chưa có gender):**

```
LoginScene
    ↓
Login successful
    ↓
LoadingScene
    ↓
Check gender: currentIngameName = null
    ↓
Load SelectCharacterScene ← Hiện scene này
    ↓
User selects gender + enters name
    ↓
Save to Firebase
    ↓
Load MenuScene
```

---

### **Login Flow (Lần sau - Đã có gender):**

```
LoginScene
    ↓
Login successful
    ↓
LoadingScene
    ↓
Check gender: currentIngameName = "PlayerName"
    ↓
Load MenuScene ← Bỏ qua SelectCharacterScene ✅
```

---

## 🧪 TEST CASES

### **Test 1: First Login (No Gender)**
```
1. Create new account
2. Login
3. ✅ LoadingScene shows
4. ✅ SelectCharacterScene shows
5. Click male model
6. ✅ Male spotlight turns yellow (intensity 3)
7. ✅ Female spotlight stays white (intensity 1)
8. Click female model
9. ✅ Female spotlight turns yellow
10. ✅ Male spotlight turns white
11. Enter name → Confirm
12. ✅ Load MenuScene
```

### **Test 2: Second Login (Has Gender)**
```
1. Login with existing account (has gender)
2. ✅ LoadingScene shows
3. ✅ SelectCharacterScene KHÔNG hiện
4. ✅ Load MenuScene trực tiếp
```

### **Test 3: Spotlight Visual**
```
1. SelectCharacterScene
2. Default: Male selected
3. ✅ Male spotlight: Yellow, Intensity 3
4. ✅ Female spotlight: White, Intensity 1
5. Click female model
6. ✅ Spotlights swap colors/intensity
```

### **Test 4: MenuScene PanelHome**
```
1. Load MenuScene
2. Check Console:
   ✅ No error "PanelHome is null!"
   ✅ "MenuScene: Updating character display after data load..."
   ✅ Character sprite shows correctly
```

---

## 🐛 FIX MENUSCENE ERRORS

### **Error 1: SimpleChatManager not found**
```
MenuScene: SimpleChatManager not found
```

**Solution:**
```
1. Open MenuScene
2. Create Empty GameObject: "SimpleChatManager"
3. Add Component: SimpleChatManager
4. Assign to MenuSceneManager → Simple Chat Manager
```

**Hoặc:**
```
Ignore warning - Chat system is optional
Game vẫn chạy bình thường
```

---

### **Error 2: PanelHome is null**
```
MenuScene: PanelHome is null! Please assign it in the inspector.
```

**Solution:**
```
1. Find MenuSceneManager GameObject
2. MenuSceneManager component:
   - Panel Home: Drag PanelHome GameObject
```

**Đã fix:**
- ✅ Auto-find PanelHome nếu null
- ✅ Warning thay vì Error

---

## 📁 FILE STRUCTURE

```
SelectCharacterScene
├── Canvas
│   ├── InputField (Ingame Name)
│   ├── ButtonConfirm
│   └── TextError
├── MaleCharacterModel
│   └── CapsuleCollider (Auto-added)
├── FemaleCharacterModel
│   └── CapsuleCollider (Auto-added)
├── MaleSpotlight (NEW)
│   ├── Type: Spotlight
│   ├── Color: White → Yellow (when selected)
│   └── Intensity: 1 → 3 (when selected)
├── FemaleSpotlight (NEW)
│   ├── Type: Spotlight
│   ├── Color: White → Yellow (when selected)
│   └── Intensity: 1 → 3 (when selected)
└── SelectCharacterController
```

---

## 🎯 SUMMARY

**Đã fix:**
- ✅ SelectCharacterScene - Spotlight thay vì Cube
- ✅ LoadingScene - Check gender trước khi load scene
- ✅ MenuSceneManager - Auto-find PanelHome
- ✅ CapsuleCollider - Phù hợp với character models

**Cách hoạt động:**
```
LoadingScene:
    ↓
Check currentIngameName
    ↓
    ├─→ Có gender → MenuScene (Skip SelectCharacterScene)
    └─→ Chưa có → SelectCharacterScene

SelectCharacterScene:
    ↓
Click model
    ↓
Spotlight turns yellow (intensity 3)
    ↓
Enter name → Confirm
    ↓
Save to Firebase
    ↓
MenuScene
```

**Setup:**
- ✅ SelectCharacterScene - Xóa cubes, thêm spotlights (5 phút)
- ✅ LoadingScene - Không cần làm gì (0 phút)
- ✅ MenuScene - Assign PanelHome (1 phút)

**Tổng thời gian: 6 phút!**

---

**GO! GO! GO!** 🔥

