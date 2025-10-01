# 🔧 Fix Register Panel Freeze/Crash

## ❌ Vấn đề:

```
User gõ username/email trong Register panel
    ↓
Unity freeze/crash
    ↓
End task Unity Editor
```

---

## 🔍 Nguyên nhân:

### Vấn đề: Spam Firestore Queries

```csharp
// AuthUIController.cs - Line 118-119 (CŨ)
inputUsername.onValueChanged.AddListener(OnUsernameChanged);
inputEmail.onValueChanged.AddListener(OnEmailChanged);

// OnUsernameChanged được gọi MỖI KHI GÕ 1 CHỮ!
private async void OnUsernameChanged(string value)
{
    // Query Firestore để check username
    bool isTaken = await firebaseAuthService.IsUsernameTakenAsync(value);
}
```

### Kịch bản gây crash:

```
User gõ: "p"
    ↓
OnUsernameChanged("p") → Query Firestore ⏳

User gõ: "l"
    ↓
OnUsernameChanged("pl") → Query Firestore ⏳

User gõ: "a"
    ↓
OnUsernameChanged("pla") → Query Firestore ⏳

User gõ: "y"
    ↓
OnUsernameChanged("play") → Query Firestore ⏳

User gõ: "e"
    ↓
OnUsernameChanged("playe") → Query Firestore ⏳

User gõ: "r"
    ↓
OnUsernameChanged("player") → Query Firestore ⏳

User gõ: "1"
    ↓
OnUsernameChanged("player1") → Query Firestore ⏳

→ 7 queries đồng thời!
→ Firestore rate limit exceeded
→ Unity freeze/crash
```

---

## ✅ Giải pháp: Debounce Pattern

### Debounce là gì?

```
Debounce = Đợi user NGỪNG GÕ một khoảng thời gian trước khi thực hiện action

User gõ: "p" → Đợi 0.5s
User gõ: "l" → Cancel đợi cũ, đợi 0.5s mới
User gõ: "a" → Cancel đợi cũ, đợi 0.5s mới
User gõ: "y" → Cancel đợi cũ, đợi 0.5s mới
User gõ: "e" → Cancel đợi cũ, đợi 0.5s mới
User gõ: "r" → Cancel đợi cũ, đợi 0.5s mới
User gõ: "1" → Cancel đợi cũ, đợi 0.5s mới
User ngừng gõ 0.5s → Query Firestore 1 lần duy nhất ✅

→ Chỉ 1 query thay vì 7 queries!
```

---

## 🔧 Đã sửa:

### 1. Thêm debounce coroutines:

```csharp
// AuthUIController.cs - Line 61-67
private bool isProcessing = false;
private Coroutine notificationCoroutine;
private UserData currentUserData;

// Debounce coroutines để tránh spam Firestore queries
private Coroutine usernameCheckCoroutine;
private Coroutine emailCheckCoroutine;
```

### 2. Đổi OnUsernameChanged từ async void → void:

```csharp
// AuthUIController.cs - Line 258-295
private void OnUsernameChanged(string value)
{
    // Cancel coroutine cũ để tránh spam queries
    if (usernameCheckCoroutine != null)
    {
        StopCoroutine(usernameCheckCoroutine);
    }
    
    if (string.IsNullOrEmpty(value))
    {
        textCheckUsername.gameObject.SetActive(false);
        return;
    }

    // Kiểm tra Firebase service
    if (firebaseAuthService == null || !firebaseAuthService.IsFirebaseReady())
    {
        // Show error
        return;
    }

    // Start debounced check (đợi 0.5s sau khi user ngừng gõ)
    usernameCheckCoroutine = StartCoroutine(CheckUsernameDebounced(value));
}
```

### 3. Thêm CheckUsernameDebounced coroutine:

```csharp
// AuthUIController.cs - Line 297-339
private IEnumerator CheckUsernameDebounced(string username)
{
    // Đợi 0.5 giây
    yield return new WaitForSeconds(0.5f);
    
    textCheckUsername.gameObject.SetActive(true);
    textCheckUsername.text = "Đang kiểm tra...";
    textCheckUsername.color = Color.yellow;

    // Gọi async method trong coroutine
    var checkTask = firebaseAuthService.IsUsernameTakenAsync(username);
    yield return new WaitUntil(() => checkTask.IsCompleted);

    try
    {
        bool isTaken = checkTask.Result;
        
        if (isTaken)
        {
            textCheckUsername.text = "Username đã được sử dụng";
            textCheckUsername.color = Color.red;
        }
        else
        {
            textCheckUsername.text = "Username có thể sử dụng";
            textCheckUsername.color = Color.green;
        }
    }
    catch (Exception e)
    {
        textCheckUsername.text = "Lỗi kiểm tra username";
        textCheckUsername.color = Color.red;
        Debug.LogError($"Username check error: {e.Message}");
    }

    ValidateRegisterForm();
}
```

### 4. Tương tự cho OnEmailChanged:

```csharp
// AuthUIController.cs - Line 341-410
private void OnEmailChanged(string value)
{
    // Cancel coroutine cũ
    if (emailCheckCoroutine != null)
    {
        StopCoroutine(emailCheckCoroutine);
    }
    
    // ... validation ...
    
    // Start debounced check
    emailCheckCoroutine = StartCoroutine(CheckEmailDebounced(value));
}

private IEnumerator CheckEmailDebounced(string email)
{
    // Đợi 0.5 giây
    yield return new WaitForSeconds(0.5f);
    
    // Query Firestore
    var checkTask = firebaseAuthService.IsEmailTakenAsync(email);
    yield return new WaitUntil(() => checkTask.IsCompleted);
    
    // Show result
    // ...
}
```

---

## 📊 So sánh:

### Trước (Không có debounce):

```
User gõ "player1" (7 chữ):
→ 7 Firestore queries đồng thời
→ Rate limit exceeded
→ Unity freeze/crash ❌
```

### Sau (Có debounce):

```
User gõ "player1" (7 chữ):
→ Đợi 0.5s sau khi ngừng gõ
→ 1 Firestore query duy nhất
→ Unity hoạt động bình thường ✅
```

---

## 🎮 Testing:

### Test 1: Gõ username nhanh

```
1. Play game
2. Click "Đăng ký"
3. Gõ nhanh "player1" vào Username field
4. Đợi 0.5s
5. Thấy "Username có thể sử dụng" hoặc "Username đã được sử dụng" ✅
6. Unity KHÔNG freeze ✅
```

### Test 2: Gõ email nhanh

```
1. Gõ nhanh "test@gmail.com" vào Email field
2. Đợi 0.5s
3. Thấy "Email có thể sử dụng" hoặc "Email đã được đăng ký" ✅
4. Unity KHÔNG freeze ✅
```

### Test 3: Gõ và xóa nhiều lần

```
1. Gõ "player1"
2. Xóa hết
3. Gõ "player2"
4. Xóa hết
5. Gõ "player3"
6. Đợi 0.5s
7. Chỉ query "player3" 1 lần ✅
8. Unity KHÔNG freeze ✅
```

### Test 4: Tạo tài khoản

```
1. Username: player1
2. Email: test1@gmail.com
3. Password: 123456
4. Confirm Password: 123456
5. Click "Tạo tài khoản"
6. Tài khoản được tạo thành công ✅
7. Unity KHÔNG freeze ✅
```

---

## 🐛 Troubleshooting

### Vấn đề 1: Vẫn freeze khi gõ

**Nguyên nhân**: Code chưa update

**Giải pháp**:
```
1. Check AuthUIController.cs có debounce coroutines chưa (line 66-67)
2. Recompile Unity project
3. Play lại
```

### Vấn đề 2: Không thấy "Đang kiểm tra..."

**Nguyên nhân**: Debounce delay quá ngắn hoặc quá dài

**Giải pháp**:
```
Adjust delay trong CheckUsernameDebounced:
yield return new WaitForSeconds(0.5f); // Thử 0.3f hoặc 0.7f
```

### Vấn đề 3: Firestore permission error

**Nguyên nhân**: Firestore rules chưa cho phép read

**Giải pháp**:
```
Firestore Rules:
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /handles/{handle} {
      allow read: if true; // Cho phép đọc để check username
      allow write: if request.auth != null;
    }
    match /users/{userId} {
      allow read: if true; // Cho phép đọc để check email
      allow write: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

---

## 🎯 Tóm tắt:

| Vấn đề | Nguyên nhân | Giải pháp |
|--------|-------------|-----------|
| Unity freeze khi gõ | Spam Firestore queries | Debounce pattern |
| Quá nhiều queries | onValueChanged mỗi chữ | Đợi 0.5s sau khi ngừng gõ |
| Rate limit exceeded | 7+ queries đồng thời | Chỉ 1 query cuối cùng |

---

## 📝 Files đã sửa:

1. ✅ **AuthUIController.cs**
   - Line 66-67: Thêm debounce coroutines
   - Line 258-295: Đổi OnUsernameChanged → void + debounce
   - Line 297-339: Thêm CheckUsernameDebounced coroutine
   - Line 341-410: Đổi OnEmailChanged → void + debounce
   - Line 412+: Thêm CheckEmailDebounced coroutine

---

## 💡 Best Practices:

### 1. Luôn dùng debounce cho real-time validation
```csharp
// ✅ ĐÚNG: Debounce
inputField.onValueChanged.AddListener(OnValueChanged);
private void OnValueChanged(string value) {
    StartCoroutine(CheckDebounced(value, 0.5f));
}

// ❌ SAI: Không debounce
inputField.onValueChanged.AddListener(async (value) => {
    await CheckAsync(value); // Spam queries!
});
```

### 2. Cancel coroutine cũ trước khi start mới
```csharp
if (checkCoroutine != null) {
    StopCoroutine(checkCoroutine);
}
checkCoroutine = StartCoroutine(CheckDebounced(value));
```

### 3. Delay hợp lý: 0.3s - 0.7s
```
0.3s: Nhanh, nhưng vẫn có thể spam nếu gõ rất nhanh
0.5s: Cân bằng tốt (khuyến khích)
0.7s: Chậm, user phải đợi lâu
```

---

**Version**: 1.0
**Date**: 2025-10-01
**Status**: Fixed ✅

