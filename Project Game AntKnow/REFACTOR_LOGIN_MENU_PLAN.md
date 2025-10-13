# 🔄 REFACTOR LOGIN/MENU FLOW - PLAN

## ✅ LỖI ĐÃ SỬA
- [x] CS0428: Cannot convert method group 'Count' to non-delegate type 'object'
  - **Fix:** Thêm `()` → `snapshot.Documents.Count()`

---

## 🎯 YÊU CẦU REFACTOR

### **1. LoginScene - AuthUIController**

#### **PanelLogin:**
- ✅ Text: "Don't have an account? Create account" (English)
- ✅ Click "Create account" → Switch to RegisterPanel
- ✅ Toggle "Remember Me" → Save password locally
  - ON: Lưu username/email + password
  - OFF: Xóa saved credentials
- ✅ Login success → Show notification 2s → LoadingScene
- ✅ Login fail → Show notification 2s (error message)

#### **RegisterPanel:**
- ✅ All text in English
- ✅ Text: "Back to Login" → Click → Switch to LoginPanel
- ✅ Register success → Show notification 2s → LoadingScene
- ✅ Register fail → Show notification 2s (error message)

#### **PanelNotification:**
- ✅ Show notification 2 seconds
- ✅ Auto hide after 2s
- ✅ Messages in English

#### **BtnLog (Login/Logout Button):**
- ✅ Initially hidden (panelLog hidden)
- ✅ When PanelLogin/Register visible → BtnLog hidden
- ✅ When BtnClose clicked → Hide PanelLogin/Register → Show BtnLog
- ✅ BtnLog states:
  - **Not logged in:** Show "Login" sprite → Click → Show PanelLogin
  - **Logged in:** Show "Logout" sprite → Click → Logout → Redirect to LoginScene

#### **BtnExit:**
- ✅ New button to quit game
- ✅ Click → Application.Quit()

---

### **2. LoadingScene**

#### **Usage:**
- ✅ Login → LoadingScene → SelectCharacterScene → MenuScene
- ✅ MenuScene → LoadingScene → GameScene (future)

#### **Features:**
- ✅ Progress bar
- ✅ Loading tips (rotate every 3s)
- ✅ Background images (rotate every 15s)
- ✅ Loading steps text
- ✅ All text in English

---

### **3. MenuScene**

#### **Flow:**
- ✅ Check if user logged in
  - NO → Redirect to LoginScene
  - YES → Load user data → Show UI

#### **Features:**
- ✅ All text in English
- ✅ Auto load inventory & loadout (parallel)
- ✅ Show character based on gender
- ✅ Logout button → Redirect to LoginScene

---

## 📁 FILES CẦN SỬA

### **1. AuthUIController.cs** (MAJOR REFACTOR)
**Changes:**
- [ ] Change all Vietnamese text to English
- [ ] Fix PanelLogin/Register toggle logic
- [ ] Fix BtnLog show/hide logic
- [ ] Fix BtnLog sprite (Login/Logout)
- [ ] Add BtnExit
- [ ] Fix notification duration (2s)
- [ ] Save/Load credentials with PlayerPrefs
- [ ] Redirect to LoadingScene after login/register success

### **2. LoadingSceneController.cs** (MINOR CHANGES)
**Changes:**
- [ ] Change all Vietnamese text to English
- [ ] Update loading tips to English
- [ ] Update loading steps to English

### **3. SelectCharacterController.cs** (MINOR CHANGES)
**Changes:**
- [ ] Change all Vietnamese text to English

### **4. MenuSceneManager.cs** (MINOR CHANGES)
**Changes:**
- [ ] Change all Vietnamese text to English
- [ ] Ensure auto redirect to LoginScene if not logged in

### **5. PanelNotification.cs** (ALREADY OK)
**Status:** Already has 2s duration, just need to use English messages

---

## 🔧 IMPLEMENTATION STEPS

### **STEP 1: Fix AuthUIController** (30 phút)
1. Change all text to English
2. Fix panel toggle logic
3. Fix BtnLog show/hide
4. Add BtnExit
5. Fix notification duration
6. Save/Load credentials

### **STEP 2: Fix LoadingScene** (10 phút)
1. Change tips to English
2. Change loading steps to English

### **STEP 3: Fix SelectCharacterScene** (10 phút)
1. Change text to English

### **STEP 4: Fix MenuScene** (10 phút)
1. Change text to English
2. Verify auto redirect

### **STEP 5: Test Full Flow** (20 phút)
1. Test Login → Loading → SelectCharacter → Menu
2. Test Register → Loading → SelectCharacter → Menu
3. Test Remember Me
4. Test Logout
5. Test Exit

---

## 📝 TEXT TRANSLATIONS

### **LoginPanel:**
```
Vietnamese → English
"Tên đăng nhập hoặc Email" → "Username or Email"
"Mật khẩu" → "Password"
"Ghi nhớ đăng nhập" → "Remember Me"
"Đăng nhập" → "Login"
"Đăng nhập với Google" → "Login with Google"
"Chưa có tài khoản? Tạo tài khoản" → "Don't have an account? Create account"
```

### **RegisterPanel:**
```
Vietnamese → English
"Tên đăng nhập" → "Username"
"Email" → "Email"
"Mật khẩu" → "Password"
"Nhập lại mật khẩu" → "Confirm Password"
"Tạo tài khoản" → "Create Account"
"Quay lại đăng nhập" → "Back to Login"
```

### **Notifications:**
```
Vietnamese → English
"Đăng nhập thành công!" → "Login successful!"
"Tạo tài khoản thành công!" → "Account created successfully!"
"Sai tên đăng nhập hoặc mật khẩu" → "Invalid username or password"
"Email đã được sử dụng" → "Email already in use"
"Tên đăng nhập đã tồn tại" → "Username already exists"
"Mật khẩu không khớp" → "Passwords do not match"
"Vui lòng điền đầy đủ thông tin" → "Please fill in all fields"
```

### **LoadingScene:**
```
Vietnamese → English
"Đang kết nối Firebase..." → "Connecting to Firebase..."
"Đang tải thông tin người dùng..." → "Loading user data..."
"Đang kiểm tra inventory..." → "Checking inventory..."
"Đang chuẩn bị loadout..." → "Preparing loadout..."
"Đang tải cấu hình game..." → "Loading game configuration..."
"Hoàn thành!" → "Complete!"

Tips:
"💡 Mẹo: Sử dụng skill cards..." → "💡 Tip: Use skill cards strategically..."
"🎮 Fact: Mỗi card có thể..." → "🎮 Fact: Each card can be upgraded..."
```

---

## 🧪 TEST CASES

### **Test 1: Login Flow**
```
1. Open LoginScene
2. Enter username/password
3. Check "Remember Me"
4. Click "Login"
5. See notification "Login successful!" (2s)
6. Redirect to LoadingScene
7. See loading progress
8. Redirect to SelectCharacterScene
9. Select character
10. Redirect to MenuScene
```

### **Test 2: Register Flow**
```
1. Open LoginScene
2. Click "Create account"
3. Switch to RegisterPanel
4. Enter username/email/password
5. Click "Create Account"
6. See notification "Account created successfully!" (2s)
7. Redirect to LoadingScene
8. ... (same as login)
```

### **Test 3: Remember Me**
```
1. Login with "Remember Me" ON
2. Close game
3. Reopen game
4. Username/password auto-filled
5. Click "Login" → Success
```

### **Test 4: Logout**
```
1. Login successfully
2. Go to MenuScene
3. Click "Logout" button
4. Redirect to LoginScene
5. BtnLog shows "Login" sprite
```

### **Test 5: Exit**
```
1. Open LoginScene
2. Click "Exit" button
3. Game closes
```

---

## 🚀 NEXT STEPS

**Bạn muốn:**
1. **Bắt đầu refactor ngay?** (Tôi sẽ sửa từng file)
2. **Xem code chi tiết trước?** (Tôi sẽ show code mẫu)
3. **Test flow hiện tại trước?** (Để hiểu rõ vấn đề)

Cho tôi biết để tôi tiếp tục! 🔧

