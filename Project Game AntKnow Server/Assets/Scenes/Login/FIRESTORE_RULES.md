# FIRESTORE SECURITY RULES CHO LOGIN SCENE

## 🔐 Rules được khuyến nghị

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    
    // ===== LOGIN SCENE RULES =====
    
    // Users collection - chỉ cho phép user đã đăng nhập
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
      allow create: if request.auth != null && request.auth.uid == userId;
    }
    
    // Usernames collection - chỉ cho phép đọc và tạo username mapping
    match /usernames/{username} {
      allow read: if request.auth != null;
      allow create: if request.auth != null && 
                       request.resource.data.uid == request.auth.uid;
      allow update, delete: if false; // Không cho phép update/delete username
    }
    
    // ===== QUIZ RULES (GIỮ NGUYÊN) =====
    
    // Tạm thời: cho phép đọc và ghi tất cả câu hỏi
    // ⚠️ Chỉ dùng khi đang phát triển, chưa có người dùng
    match /quizzes/{quizId} {
      allow read, create, update, delete: if true;
    }
  }
}
```

## 🛡️ Giải thích Rules

### **Users Collection:**
- `allow read, write: if request.auth != null && request.auth.uid == userId`
  - Chỉ cho phép user đã đăng nhập
  - Chỉ được đọc/ghi profile của chính mình

### **Usernames Collection:**
- `allow read: if request.auth != null`
  - Cho phép đọc để kiểm tra username đã tồn tại chưa
- `allow create: if request.auth != null && request.resource.data.uid == request.auth.uid`
  - Chỉ cho phép tạo username mapping cho chính mình
- `allow update, delete: if false`
  - Không cho phép thay đổi/xóa username (bảo mật)

## 🚀 Rules cho Production (Khi có nhiều user)

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    
    // Users collection - bảo mật cao
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Usernames collection - chỉ đọc
    match /usernames/{username} {
      allow read: if request.auth != null;
      allow create: if request.auth != null && 
                       request.resource.data.uid == request.auth.uid &&
                       !exists(/databases/$(database)/documents/usernames/$(username));
    }
    
    // Quiz collection - chỉ đọc cho user đã đăng nhập
    match /quizzes/{quizId} {
      allow read: if request.auth != null;
      allow write: if request.auth != null && 
                      get(/databases/$(database)/documents/users/$(request.auth.uid)).data.role == 'admin';
    }
  }
}
```

## 📝 Cách cập nhật Rules

1. **Vào Firebase Console:**
   - https://console.firebase.google.com/
   - Chọn project "db-antknow"

2. **Vào Firestore Database:**
   - Click "Rules" tab
   - Thay thế rules hiện tại bằng rules mới
   - Click "Publish"

3. **Test Rules:**
   - Chạy game và test đăng ký/đăng nhập
   - Kiểm tra Console logs xem có lỗi gì không

## ⚠️ Lưu ý quan trọng

- **Development**: Có thể dùng rules mở để test
- **Production**: Phải dùng rules bảo mật cao
- **Username mapping**: Không nên cho phép update/delete
- **User data**: Chỉ cho phép user truy cập data của chính mình

---
**🔐 Rules này sẽ bảo vệ dữ liệu user và ngăn chặn truy cập trái phép!**
