# 🎮 ParrelSync Testing Guide

## ❌ Vấn đề: Instance 1 tự động tắt khi Instance 2 login

### Nguyên nhân:
```
Firebase Authentication chỉ cho phép 1 session active cho mỗi tài khoản

Instance 1: Login với test@gmail.com
    ↓
Firebase tạo session token ✅

Instance 2: Login với test@gmail.com (CÙNG tài khoản)
    ↓
Firebase tạo session token MỚI ✅
    ↓
Firebase INVALIDATE session token cũ ❌
    ↓
Instance 1 bị logout/crash
```

---

## ✅ Giải pháp: Dùng tài khoản khác nhau

### Cách 1: Manual Login (Đơn giản nhất)

```
Instance 1 (Editor):
- Email: test1@gmail.com
- Password: 123456

Instance 2 (ParrelSync Clone):
- Email: test2@gmail.com
- Password: 123456
```

### Cách 2: Auto Login với ParrelSyncHelper (Khuyến khích)

Tôi đã tạo `ParrelSyncHelper.cs` để tự động detect clone và login tài khoản khác!

---

## 🔧 Setup ParrelSyncHelper

### Bước 1: Tạo test accounts trên Firebase

```
Firebase Console:
1. Vào Authentication > Users
2. Click "Add User"
3. Tạo các tài khoản:
   - test1@gmail.com / 123456 (Editor gốc)
   - test2@gmail.com / 123456 (Clone 1)
   - test3@gmail.com / 123456 (Clone 2)
   - test4@gmail.com / 123456 (Clone 3)
```

### Bước 2: Sử dụng ParrelSyncHelper trong AuthUIController

```csharp
// AuthUIController.cs hoặc FirebaseAuthService.cs

using AntKnow.Auth;

private void Start()
{
    // Log thông tin clone
    ParrelSyncHelper.LogCloneInfo();
    
    // Auto fill test account (chỉ trong Editor)
#if UNITY_EDITOR
    if (inputEmail != null)
        inputEmail.text = ParrelSyncHelper.GetTestEmail();
    
    if (inputPassword != null)
        inputPassword.text = ParrelSyncHelper.GetTestPassword();
    
    Debug.Log($"Auto-filled test account: {ParrelSyncHelper.GetTestEmail()}");
#endif
}
```

### Bước 3: (Optional) Auto login khi start

```csharp
// AuthUIController.cs

private async void Start()
{
    ParrelSyncHelper.LogCloneInfo();
    
#if UNITY_EDITOR
    // Auto login trong Editor để test nhanh
    string email = ParrelSyncHelper.GetTestEmail();
    string password = ParrelSyncHelper.GetTestPassword();
    
    Debug.Log($"Auto-logging in with: {email}");
    await LoginWithEmail(email, password);
#endif
}
```

---

## 📋 ParrelSyncHelper API

### Kiểm tra clone:
```csharp
bool isClone = ParrelSyncHelper.IsClone();
// Editor gốc: false
// Clone: true
```

### Lấy clone number:
```csharp
int cloneNum = ParrelSyncHelper.GetCloneNumber();
// Editor gốc: 0
// Clone 1: 1
// Clone 2: 2
```

### Lấy test email:
```csharp
string email = ParrelSyncHelper.GetTestEmail();
// Editor gốc: test1@gmail.com
// Clone 1: test2@gmail.com
// Clone 2: test3@gmail.com
```

### Lấy test password:
```csharp
string password = ParrelSyncHelper.GetTestPassword();
// Tất cả: 123456
```

### Lấy player name:
```csharp
string playerName = ParrelSyncHelper.GetTestPlayerName();
// Editor gốc: Player1
// Clone 1: Player2
// Clone 2: Player3
```

### Log thông tin:
```csharp
ParrelSyncHelper.LogCloneInfo();
// [ParrelSync] Running on Clone 1
// [ParrelSync] Test Account: test2@gmail.com
```

---

## 🎮 Testing Workflow

### Bước 1: Tạo Clone với ParrelSync

```
Unity Editor:
1. Window > ParrelSync > Clones Manager
2. Click "Create new clone"
3. Đợi clone được tạo
4. Click "Open in New Editor"
```

### Bước 2: Test Multiplayer

```
Instance 1 (Editor gốc):
1. Play game
2. Auto login với test1@gmail.com ✅
3. Vào MenuScene
4. Click "Custom"
5. Click "Tạo phòng"
6. Tạo phòng "Test Room"
7. Đợi trong phòng

Instance 2 (Clone):
1. Play game
2. Auto login với test2@gmail.com ✅ (tài khoản KHÁC)
3. Vào MenuScene
4. Click "Custom"
5. Click "Làm mới"
6. Thấy "Test Room (1/4)"
7. Click vào phòng
8. Vào phòng thành công ✅
9. Instance 1 thấy "2/4" ✅
```

### Bước 3: Test Start Game

```
Instance 1 (Host):
1. Trong phòng, thấy 2 người
2. Click "Bắt đầu" (chỉ host có button này)
3. Cả 2 instances load GameScene ✅

Instance 2 (Client):
1. Đợi host start
2. Auto load GameScene ✅
```

---

## 🐛 Troubleshooting

### Vấn đề 1: Instance 1 vẫn bị tắt

**Nguyên nhân**: Vẫn đang login cùng tài khoản

**Giải pháp**:
```
1. Check Console log:
   [ParrelSync] Running on Main Editor
   [ParrelSync] Test Account: test1@gmail.com ✅
   
   [ParrelSync] Running on Clone 1
   [ParrelSync] Test Account: test2@gmail.com ✅

2. Nếu cả 2 đều test1@gmail.com → ParrelSyncHelper chưa hoạt động
3. Check code đã dùng ParrelSyncHelper.GetTestEmail() chưa
```

### Vấn đề 2: ParrelSyncHelper không detect clone

**Nguyên nhân**: ParrelSync chưa cài hoặc clone chưa đúng

**Giải pháp**:
```
1. Check ParrelSync đã cài chưa:
   Window > ParrelSync > Clones Manager

2. Check project path có "_clone_" không:
   Debug.Log(Application.dataPath);
   
   Editor gốc: D:/ProjectGame/AntKnow/Assets
   Clone: D:/ProjectGame/AntKnow_clone_0/Assets ✅
```

### Vấn đề 3: Test accounts chưa tạo trên Firebase

**Giải pháp**:
```
1. Vào Firebase Console
2. Authentication > Users
3. Tạo test1@gmail.com, test2@gmail.com, test3@gmail.com
4. Password: 123456 (giống nhau)
```

---

## 📝 Example Code

### Auto Login trong AuthUIController:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;

public class AuthUIController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private Button buttonLogin;

    private void Start()
    {
        // Log clone info
        ParrelSyncHelper.LogCloneInfo();
        
        // Auto fill test account trong Editor
#if UNITY_EDITOR
        AutoFillTestAccount();
        
        // Optional: Auto login để test nhanh
        // AutoLogin();
#endif
        
        buttonLogin.onClick.AddListener(OnLoginClicked);
    }

    private void AutoFillTestAccount()
    {
        if (inputEmail != null)
            inputEmail.text = ParrelSyncHelper.GetTestEmail();
        
        if (inputPassword != null)
            inputPassword.text = ParrelSyncHelper.GetTestPassword();
        
        Debug.Log($"Auto-filled: {ParrelSyncHelper.GetTestEmail()}");
    }

    private async void AutoLogin()
    {
        string email = ParrelSyncHelper.GetTestEmail();
        string password = ParrelSyncHelper.GetTestPassword();
        
        Debug.Log($"Auto-logging in: {email}");
        
        // Call your login method
        await FirebaseAuthService.Instance.LoginWithEmailAsync(email, password);
    }

    private async void OnLoginClicked()
    {
        string email = inputEmail.text;
        string password = inputPassword.text;
        
        await FirebaseAuthService.Instance.LoginWithEmailAsync(email, password);
    }
}
```

---

## 🎯 Best Practices

### 1. Luôn dùng tài khoản khác nhau cho mỗi instance
```
✅ Instance 1: test1@gmail.com
✅ Instance 2: test2@gmail.com

❌ Instance 1: test@gmail.com
❌ Instance 2: test@gmail.com (CÙNG tài khoản → Lỗi!)
```

### 2. Dùng ParrelSyncHelper để auto detect
```csharp
// ✅ Tự động
string email = ParrelSyncHelper.GetTestEmail();

// ❌ Hardcode
string email = "test@gmail.com";
```

### 3. Log thông tin để debug
```csharp
ParrelSyncHelper.LogCloneInfo();
Debug.Log($"Logging in with: {email}");
```

### 4. Tạo đủ test accounts
```
Nếu test với 4 players:
- test1@gmail.com (Editor)
- test2@gmail.com (Clone 1)
- test3@gmail.com (Clone 2)
- test4@gmail.com (Clone 3)
```

---

## 📚 Resources

- [ParrelSync GitHub](https://github.com/VeriorPies/ParrelSync)
- [Firebase Auth Docs](https://firebase.google.com/docs/auth)
- [Unity Multiplayer Testing](https://docs.unity.com/netcode/current/tutorials/testing/testing_locally/)

---

**Version**: 1.0
**Date**: 2025-10-01

