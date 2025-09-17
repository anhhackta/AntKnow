# 🔥 CẬP NHẬT FIRESTORE RULES

## ⚠️ **LỖI HIỆN TẠI**
```
Error checking username: Missing or insufficient permissions.
Error checking email: Missing or insufficient permissions.
```

## 🛠️ **GIẢI PHÁP**

### **Bước 1: Vào Firebase Console**
1. Mở https://console.firebase.google.com/
2. Chọn project **"db-antknow"**
3. Vào **Firestore Database** → **Rules**

### **Bước 2: Cập nhật Rules**
Thay thế rules hiện tại bằng:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    
    // Users collection - chỉ cho phép user đã đăng nhập
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Usernames collection - cho phép đọc để kiểm tra username
    match /usernames/{username} {
      allow read: if true; // ✅ Cho phép đọc để kiểm tra username
      allow create: if request.auth != null && 
                       request.resource.data.uid == request.auth.uid;
      allow update, delete: if false; // Không cho phép thay đổi username
    }
    
    // Quiz collection - giữ nguyên
    match /quizzes/{quizId} {
      allow read, create, update, delete: if true;
    }
  }
}
```

### **Bước 3: Publish Rules**
1. Click **"Publish"** button
2. Chờ vài giây để rules được áp dụng

### **Bước 4: Test lại**
1. Chạy game trong Unity
2. Thử nhập username/email
3. Sẽ thấy validation hoạt động bình thường

## 🎯 **TẠI SAO CẦN CẬP NHẬT?**

### **Vấn đề:**
- Rules hiện tại yêu cầu đăng nhập để đọc `usernames` collection
- Nhưng validation cần chạy trước khi user đăng nhập
- → Lỗi "Missing or insufficient permissions"

### **Giải pháp:**
- Cho phép đọc `usernames` collection mà không cần đăng nhập
- Vẫn bảo mật: chỉ cho phép tạo username mapping khi đã đăng nhập
- Không cho phép update/delete username (bảo mật)

## 🔐 **BẢO MẬT**

### **An toàn:**
- ✅ Chỉ cho phép đọc `usernames` (không thể thay đổi)
- ✅ Chỉ cho phép tạo username mapping cho chính mình
- ✅ Không cho phép update/delete username
- ✅ User data chỉ truy cập được khi đã đăng nhập

### **Không ảnh hưởng:**
- ❌ Không thể xem thông tin user khác
- ❌ Không thể thay đổi username của người khác
- ❌ Không thể truy cập dữ liệu cá nhân

## 🚀 **SAU KHI CẬP NHẬT**

### **Sẽ hoạt động:**
- ✅ Real-time validation username
- ✅ Real-time validation email
- ✅ Đăng ký tài khoản mới
- ✅ Đăng nhập bằng username/email
- ✅ Tất cả tính năng khác

### **Console logs sẽ thấy:**
```
Username có thể sử dụng ✅
Email có thể sử dụng ✅
```

---
**🎉 Cập nhật rules xong là validation sẽ hoạt động ngay! 🎮**
