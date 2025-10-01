# 🐛 Debug ParrelSync Clone Tự Động Tắt

## ❌ Vấn đề:

```
Tab 1 (Unity Editor gốc): Login → OK ✅
Tab 2 (ParrelSync Clone): Login → Tự động tắt ❌
```

---

## 🔍 Các nguyên nhân có thể:

### 1. **Cùng tài khoản Firebase Authentication** (90% khả năng)

```
Tab 1: Login với "player1" → Firebase Auth email: test@gmail.com
Tab 2: Login với "player1" → Firebase Auth email: test@gmail.com ❌ CÙNG!

→ Firebase chỉ cho 1 session active/tài khoản
→ Tab 2 login → Firebase invalidate session Tab 1
→ Tab 1 trigger OnAuthStateChanged (User signed out)
→ Tab 1 có thể crash/close
```

**Lưu ý**: Ngay cả khi username khác nhau trong Firestore, nếu **email trong Firebase Authentication giống nhau** thì vẫn bị conflict!

### 2. **Exception trong code**

```
Tab 2 login → Trigger exception
→ Unity Editor auto pause/close (nếu bật Error Pause)
→ Tab 2 tắt
```

### 3. **DontDestroyOnLoad conflict**

```
Tab 2 có FirebaseAuthService với DontDestroyOnLoad
→ Load scene mới → Duplicate instance
→ Conflict → Crash
```

### 4. **Memory/Resource issue**

```
2 Unity instances cùng chạy
→ RAM/CPU quá tải
→ Unity crash
```

---

## ✅ Giải pháp:

### Bước 1: Kiểm tra Console Log

Tôi đã thêm debug logs vào `FirebaseAuthService.cs`. Hãy test và xem Console:

#### Tab 1 (Unity Editor gốc):
```
1. Play game
2. Login với tài khoản A
3. Check Console:
   [FirebaseAuth] User ID: abc123
   [FirebaseAuth] Email: test1@gmail.com
4. Copy User ID và Email
```

#### Tab 2 (ParrelSync Clone):
```
1. Play game
2. Login với tài khoản B
3. Check Console TRƯỚC KHI BỊ TẮT:
   [FirebaseAuth] User ID: def456
   [FirebaseAuth] Email: test2@gmail.com
   
4. Kiểm tra:
   - User ID có KHÁC với Tab 1 không? (abc123 vs def456)
   - Email có KHÁC với Tab 1 không? (test1 vs test2)
   
5. Nếu GIỐNG NHAU → Đây là vấn đề!
```

#### Nếu Tab 2 bị tắt, check log:
```
Có thấy:
- "[FirebaseAuth] Auth state changed: User signed out!" ?
- Exception nào không?
- Error log nào không?
```

---

### Bước 2: Tạo 2 tài khoản THỰC SỰ khác nhau

#### Cách 1: Tạo trên Firebase Console

```
Firebase Console > Authentication > Users > Add User:

Tài khoản 1 (cho Tab 1):
- Email: test1@gmail.com
- Password: 123456

Tài khoản 2 (cho Tab 2):
- Email: test2@gmail.com
- Password: 123456
```

#### Cách 2: Register trong game

```
Tab 1:
1. Click "Đăng ký"
2. Username: player1
3. Email: test1@gmail.com ⭐ (EMAIL KHÁC)
4. Password: 123456
5. Tạo tài khoản

Tab 2:
1. Click "Đăng ký"
2. Username: player2
3. Email: test2@gmail.com ⭐ (EMAIL KHÁC)
4. Password: 123456
5. Tạo tài khoản
```

**LƯU Ý**: Email phải KHÁC NHAU! Username khác nhau không đủ!

---

### Bước 3: Verify trong Firebase Console

```
Firebase Console > Authentication > Users:

✅ ĐÚNG:
- test1@gmail.com (UID: abc123)
- test2@gmail.com (UID: def456)

❌ SAI:
- test@gmail.com (UID: abc123)
- test@gmail.com (UID: def456) ← KHÔNG THỂ có 2 UID cùng email!
```

---

### Bước 4: Tắt Error Pause trong Unity

```
Unity Editor:
1. Edit > Preferences > General
2. Tìm "Error Pause"
3. Uncheck ✅
4. Close Preferences
5. Test lại
```

Điều này tránh Unity tự động pause/close khi có error log.

---

### Bước 5: Check Firestore Rules

Nếu có permission error:

```
Firestore Console > Rules:

rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Cho phép đọc handles để check username
    match /handles/{handle} {
      allow read: if true;
      allow write: if request.auth != null;
    }
    
    // Cho phép đọc users để check email
    match /users/{userId} {
      allow read: if true;
      allow write: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

---

## 🎮 Testing Workflow

### Test 1: Verify 2 tài khoản khác nhau

```
Tab 1:
1. Play game
2. Login với player1 / test1@gmail.com
3. Console: [FirebaseAuth] User ID: abc123
4. Console: [FirebaseAuth] Email: test1@gmail.com

Tab 2:
1. Play game
2. Login với player2 / test2@gmail.com
3. Console: [FirebaseAuth] User ID: def456 ✅ (KHÁC abc123)
4. Console: [FirebaseAuth] Email: test2@gmail.com ✅ (KHÁC test1)
5. Tab 2 KHÔNG bị tắt ✅
```

### Test 2: Verify Tab 1 không bị logout

```
Tab 1 (sau khi Tab 2 login):
1. Check Console:
   - KHÔNG có "[FirebaseAuth] Auth state changed: User signed out!" ✅
2. Game vẫn chạy bình thường ✅
3. Có thể vào MenuScene ✅
```

### Test 3: Test multiplayer

```
Tab 1:
1. Vào MenuScene
2. Click "Custom"
3. Click "Tạo phòng"
4. Tạo phòng "Test Room"

Tab 2:
1. Vào MenuScene
2. Click "Custom"
3. Click "Làm mới"
4. Thấy "Test Room (1/4)" ✅
5. Click vào phòng
6. Join thành công ✅

Tab 1:
1. Thấy "2/4" ✅
2. Thấy player2 trong danh sách ✅
```

---

## 🐛 Troubleshooting

### Vấn đề 1: Tab 2 vẫn bị tắt dù email khác nhau

**Check Console log**:
```
Tab 2 Console (trước khi tắt):
- Có exception nào không?
- Có error log nào không?
- Có "[FirebaseAuth] Exception in OnAuthStateChanged" không?
```

**Giải pháp**:
```
1. Copy toàn bộ Console log
2. Gửi cho tôi để debug
3. Check Unity Editor log file:
   Windows: %LOCALAPPDATA%\Unity\Editor\Editor.log
```

### Vấn đề 2: Không thấy debug log "[FirebaseAuth]"

**Nguyên nhân**: Code chưa update

**Giải pháp**:
```
1. Check FirebaseAuthService.cs có debug logs chưa
2. Recompile Unity project (Ctrl+R)
3. Close và reopen ParrelSync clone
4. Play lại
```

### Vấn đề 3: Tab 2 bị tắt ngay khi start Play

**Nguyên nhân**: Có thể do Firebase initialization error

**Giải pháp**:
```
1. Check Console log ngay khi Play:
   - "Starting Firebase initialization..."
   - "Firebase dependencies resolved successfully"
   - "✅ Firebase initialized successfully!"
   
2. Nếu không thấy → Firebase init failed
3. Check google-services.json có trong project không
4. Check Firebase project ID đúng không
```

### Vấn đề 4: "Duplicate instance detected!"

**Nguyên nhân**: Có 2 FirebaseAuthService trong scene

**Giải pháp**:
```
1. Check Hierarchy:
   - Có bao nhiêu FirebaseAuthService GameObject?
   - Nếu > 1 → Xóa bớt, chỉ giữ 1
   
2. Check DontDestroyOnLoad scene:
   - Window > General > Hierarchy
   - Tìm "DontDestroyOnLoad" section
   - Có bao nhiêu FirebaseAuthService?
```

---

## 📝 Checklist

### Trước khi test:
- [ ] Tạo 2 tài khoản với EMAIL KHÁC NHAU
- [ ] Verify trong Firebase Console > Authentication
- [ ] Tắt "Error Pause" trong Unity Preferences
- [ ] Code đã có debug logs (FirebaseAuthService.cs)
- [ ] Recompile Unity project

### Khi test Tab 1:
- [ ] Play game
- [ ] Login thành công
- [ ] Check Console: User ID và Email
- [ ] Copy User ID và Email

### Khi test Tab 2:
- [ ] Play game
- [ ] Login với tài khoản KHÁC Tab 1
- [ ] Check Console: User ID và Email (phải KHÁC Tab 1)
- [ ] Tab 2 KHÔNG bị tắt
- [ ] Check Console Tab 1: Không có "User signed out!"

### Sau khi test:
- [ ] Cả 2 tabs đều hoạt động
- [ ] Có thể tạo phòng và join phòng
- [ ] Không có crash hoặc logout

---

## 🎯 Kết luận

**Vấn đề chính**: Firebase Authentication chỉ cho phép 1 session active cho mỗi email

**Giải pháp**: Tạo 2 tài khoản với EMAIL KHÁC NHAU (không chỉ username khác)

**Verify**: 
1. Check Console log: User ID và Email phải KHÁC NHAU
2. Tab 2 không bị tắt
3. Tab 1 không có "User signed out!" log

---

## 📊 Debug Checklist

Nếu Tab 2 vẫn bị tắt, hãy gửi cho tôi:

```
1. Console log của Tab 1 (toàn bộ)
2. Console log của Tab 2 (trước khi tắt)
3. Firebase Console screenshot:
   - Authentication > Users (list tài khoản)
4. Firestore Console screenshot:
   - users collection
   - handles collection
5. Unity Editor.log file (nếu có crash)
```

---

**Version**: 1.0
**Date**: 2025-10-01

