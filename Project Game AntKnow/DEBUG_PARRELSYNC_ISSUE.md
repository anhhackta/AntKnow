# 🐛 Debug ParrelSync Instance Closing Issue

## ❌ Vấn đề:
```
Instance 1: Login với user1
Instance 2: Login với user2
    ↓
Instance 1 tự động tắt
```

---

## 🔍 Nguyên nhân có thể:

### 1. **Cùng tài khoản Firebase Authentication** (Phổ biến nhất)

```
Firestore Database:
- users/uid1: { username: "user1", email: "test@gmail.com" }
- users/uid2: { username: "user2", email: "test@gmail.com" } ❌ CÙNG EMAIL!

Firebase Authentication:
- test@gmail.com / 123456

→ Cả 2 users đều dùng CÙNG 1 tài khoản Firebase Auth
→ Instance 2 login → Firebase invalidate session của Instance 1
→ Instance 1 bị logout/crash
```

### 2. **Firebase Auth chỉ cho phép 1 session/tài khoản**

```
Firebase Authentication Policy:
- 1 email = 1 tài khoản
- 1 tài khoản = 1 session active
- Login mới → Invalidate session cũ
```

### 3. **DontDestroyOnLoad conflict**

```
FirebaseAuthService có DontDestroyOnLoad
→ Khi Instance 1 detect session invalidated
→ Có thể trigger exception → Unity crash
```

---

## ✅ Giải pháp:

### Bước 1: Kiểm tra 2 tài khoản có THỰC SỰ khác nhau không

#### Check trong Firebase Console:

```
1. Vào Firebase Console
2. Authentication > Users
3. Kiểm tra:
   - Có 2 email KHÁC NHAU không?
   - user1@example.com ✅
   - user2@example.com ✅
   
   ❌ SAI: Cả 2 đều test@gmail.com
```

#### Check trong Firestore:

```
1. Vào Firestore Database
2. Collection "users":
   - uid1: { username: "user1", email: "user1@example.com" }
   - uid2: { username: "user2", email: "user2@example.com" }
   
3. Collection "handles":
   - user1: { email: "user1@example.com" }
   - user2: { email: "user2@example.com" }
```

---

### Bước 2: Xem Console Log để debug

Tôi đã thêm debug logs vào `FirebaseAuthService.cs`:

#### Instance 1 Console:
```
[FirebaseAuth] Attempting to sign in with email: user1@example.com
[FirebaseAuth] ✅ Sign in successful!
[FirebaseAuth] User ID: abc123
[FirebaseAuth] Email: user1@example.com
[FirebaseAuth] Display Name: user1
[FirebaseAuth] Auth state changed: User signed in
[FirebaseAuth] User ID: abc123
[FirebaseAuth] Email: user1@example.com
```

#### Instance 2 Console:
```
[FirebaseAuth] Attempting to sign in with email: user2@example.com
[FirebaseAuth] ✅ Sign in successful!
[FirebaseAuth] User ID: def456
[FirebaseAuth] Email: user2@example.com
[FirebaseAuth] Display Name: user2
[FirebaseAuth] Auth state changed: User signed in
[FirebaseAuth] User ID: def456
[FirebaseAuth] Email: user2@example.com
```

#### Nếu Instance 1 bị logout:
```
[FirebaseAuth] Auth state changed: User signed out! ⚠️
```

---

### Bước 3: Tạo 2 tài khoản THỰC SỰ khác nhau

#### Cách 1: Tạo trên Firebase Console (Khuyến khích)

```
Firebase Console > Authentication > Users > Add User:

User 1:
- Email: test1@gmail.com
- Password: 123456

User 2:
- Email: test2@gmail.com
- Password: 123456
```

#### Cách 2: Register trong game

```
Instance 1:
1. Click "Đăng ký"
2. Username: player1
3. Email: test1@gmail.com
4. Password: 123456
5. Tạo tài khoản

Instance 2:
1. Click "Đăng ký"
2. Username: player2
3. Email: test2@gmail.com
4. Password: 123456
5. Tạo tài khoản
```

---

### Bước 4: Verify trong Firestore

Sau khi tạo 2 tài khoản, check Firestore:

#### Collection "users":
```
users/
├── abc123 (UID 1)
│   ├── username: "player1"
│   ├── email: "test1@gmail.com" ✅
│   └── ...
└── def456 (UID 2)
    ├── username: "player2"
    ├── email: "test2@gmail.com" ✅
    └── ...
```

#### Collection "handles":
```
handles/
├── player1
│   └── email: "test1@gmail.com" ✅
└── player2
    └── email: "test2@gmail.com" ✅
```

#### Firebase Authentication:
```
Users:
├── test1@gmail.com (UID: abc123) ✅
└── test2@gmail.com (UID: def456) ✅
```

---

## 🎮 Testing Workflow

### Test 1: Verify 2 tài khoản khác nhau

```
Instance 1:
1. Play game
2. Login với player1 / test1@gmail.com
3. Check Console:
   [FirebaseAuth] User ID: abc123
   [FirebaseAuth] Email: test1@gmail.com

Instance 2:
1. Play game
2. Login với player2 / test2@gmail.com
3. Check Console:
   [FirebaseAuth] User ID: def456 ✅ (KHÁC với abc123)
   [FirebaseAuth] Email: test2@gmail.com ✅ (KHÁC với test1@gmail.com)
```

### Test 2: Verify Instance 1 không bị logout

```
Instance 1:
1. Sau khi Instance 2 login
2. Check Console:
   - KHÔNG có "[FirebaseAuth] Auth state changed: User signed out!" ✅
   - Game vẫn chạy bình thường ✅
```

### Test 3: Test multiplayer

```
Instance 1:
1. Vào MenuScene
2. Click "Custom"
3. Click "Tạo phòng"
4. Tạo phòng "Test Room"

Instance 2:
1. Vào MenuScene
2. Click "Custom"
3. Click "Làm mới"
4. Thấy "Test Room (1/4)" ✅
5. Click vào phòng
6. Join thành công ✅

Instance 1:
1. Thấy "2/4" ✅
2. Thấy player2 trong danh sách ✅
```

---

## 🐛 Troubleshooting

### Vấn đề 1: Console log "User signed out!" khi Instance 2 login

**Nguyên nhân**: Cùng email trong Firebase Authentication

**Giải pháp**:
```
1. Check Console log của cả 2 instances
2. So sánh User ID và Email
3. Nếu giống nhau → Tạo tài khoản mới với email khác
```

### Vấn đề 2: Không thấy log "[FirebaseAuth]"

**Nguyên nhân**: Code chưa update

**Giải pháp**:
```
1. Check FirebaseAuthService.cs đã có debug logs chưa
2. Recompile Unity project
3. Play lại
```

### Vấn đề 3: Instance 1 vẫn bị tắt dù email khác nhau

**Nguyên nhân**: Có thể do lỗi khác (không phải Firebase Auth)

**Giải pháp**:
```
1. Check Console log trước khi crash:
   - Có exception nào không?
   - Có error log nào không?

2. Check Unity Editor:
   - Edit > Preferences > General
   - "Error Pause" có check không?
   - Nếu check → Uncheck

3. Check DontDestroyOnLoad:
   - FirebaseAuthService có DontDestroyOnLoad
   - Có thể conflict với scene loading
   - Thử comment DontDestroyOnLoad để test
```

### Vấn đề 4: Không tạo được tài khoản mới

**Nguyên nhân**: Firebase Authentication chưa enable Email/Password

**Giải pháp**:
```
Firebase Console:
1. Authentication > Sign-in method
2. Email/Password → Enable ✅
3. Save
```

---

## 📝 Checklist

### Trước khi test:
- [ ] Tạo 2 tài khoản với EMAIL KHÁC NHAU trên Firebase
- [ ] Verify trong Firebase Console > Authentication
- [ ] Verify trong Firestore > users collection
- [ ] Verify trong Firestore > handles collection
- [ ] Code đã có debug logs

### Khi test:
- [ ] Instance 1: Login với user1
- [ ] Check Console: User ID và Email
- [ ] Instance 2: Login với user2
- [ ] Check Console: User ID và Email (phải KHÁC Instance 1)
- [ ] Instance 1: Check Console không có "User signed out!"
- [ ] Instance 1: Game vẫn chạy bình thường

### Sau khi test:
- [ ] Cả 2 instances đều hoạt động
- [ ] Có thể tạo phòng và join phòng
- [ ] Không có crash hoặc logout

---

## 🎯 Kết luận

**Vấn đề chính**: Firebase Authentication chỉ cho phép 1 session active cho mỗi tài khoản (email)

**Giải pháp**: Tạo 2 tài khoản với EMAIL KHÁC NHAU

**Verify**: Check Console log để đảm bảo User ID và Email khác nhau

---

**Version**: 1.0
**Date**: 2025-10-01

