# Login Scene Setup Instructions

## Overview
This Login scene implements Firebase Authentication with Email/Password and Google Sign-In, along with Cloud Firestore integration for the AntKnow game.

## Required Scripts
1. `UserProfile.cs` - User data model
2. `FirebaseAuthService.cs` - Firebase authentication and Firestore operations
3. `AuthUIController.cs` - UI management and event handling
4. `Logmainscene.cs` - Main scene controller
5. `LoginSceneSetup.cs` - Helper for UI setup

## Unity Scene Setup

### 1. Create Canvas and EventSystem
- Create a Canvas (Screen Space - Overlay)
- Add EventSystem to the scene

### 2. Create UI Hierarchy
```
Canvas
├─ PanelLog (GameObject with Image component)
│   ├─ Tabs (GameObject)
│   │   ├─ ButtonLoginTab (Button with Text)
│   │   └─ ButtonRegisterTab (Button with Text)
│   ├─ PanelLogin (GameObject with Image)
│   │   ├─ Input_UsernameOrEmail (TMP_InputField)
│   │   ├─ Input_Password (TMP_InputField, ContentType=Password)
│   │   ├─ Toggle_RememberMe (Toggle with Label)
│   │   ├─ Button_Login (Button with Text)
│   │   ├─ Button_LoginWithGoogle (Button with Text)
│   │   └─ Text_InlineError (TMP_Text, initially hidden)
│   ├─ PanelRegister (GameObject with Image)
│   │   ├─ Input_Username (TMP_InputField)
│   │   ├─ Input_Email (TMP_InputField)
│   │   ├─ Input_Password1 (TMP_InputField, ContentType=Password)
│   │   ├─ Input_Password2 (TMP_InputField, ContentType=Password)
│   │   ├─ Text_CheckUsername (TMP_Text, initially hidden)
│   │   ├─ Text_CheckEmail (TMP_Text, initially hidden)
│   │   ├─ Text_CheckPw1 (TMP_Text, initially hidden)
│   │   ├─ Text_CheckPw2 (TMP_Text, initially hidden)
│   │   └─ Button_CreateAccount (Button with Text)
│   ├─ PanelThongBao (GameObject with Image)
│   │   └─ Text_Notification (TMP_Text)
│   └─ Button_Close (Button with Text)
├─ LogButton (Button with Image, initially hidden)
└─ Button_Start (Button with Text, initially hidden)
```

### 3. Component Setup

#### Logmainscene GameObject
- Add `Logmainscene` script
- Assign `FirebaseAuthService` and `AuthUIController` references

#### FirebaseAuthService GameObject
- Create empty GameObject named "FirebaseAuthService"
- Add `FirebaseAuthService` script
- Make it DontDestroyOnLoad

#### AuthUIController GameObject
- Create empty GameObject named "AuthUIController"
- Add `AuthUIController` script
- Assign all UI references in the inspector

### 4. UI Component Settings

#### Input Fields
- Set ContentType to Password for password fields
- Add placeholder text
- Set character limit if needed

#### Buttons
- Assign appropriate sprites for LogButton (Login/Logout)
- Set button colors and text

#### Toggle
- Set label text to "Ghi nhớ đăng nhập"

#### Text Components
- Set appropriate colors for validation messages
- Initially hide validation texts

### 5. Firebase Configuration

#### google-services.json
- Place `google-services.json` in `Assets/Scenes/Login/` folder
- Ensure it's properly imported

#### Firebase Console Setup
1. Create Firebase project
2. Enable Authentication (Email/Password)
3. Enable Firestore Database
4. Download google-services.json
5. Configure OAuth for Google Sign-In (for production)

### 6. Scene Settings

#### Initial State
- PanelLog: Active
- PanelLogin: Active
- PanelRegister: Inactive
- LogButton: Inactive
- ButtonStart: Inactive
- PanelThongBao: Inactive

#### UI Positioning
- Center PanelLog on screen
- Position LogButton in corner
- Center ButtonStart on screen

## Testing Checklist

### Login Flow
- [ ] Enter valid username/email and password
- [ ] Test with invalid credentials
- [ ] Test Remember Me functionality
- [ ] Test Google Sign-In (requires OAuth setup)

### Register Flow
- [ ] Enter unique username
- [ ] Enter valid email
- [ ] Enter matching passwords (≥8 characters)
- [ ] Test real-time validation
- [ ] Test with duplicate username/email

### UI Interactions
- [ ] Switch between Login/Register tabs
- [ ] Close panel shows LogButton
- [ ] LogButton toggles between Login/Logout
- [ ] Start button loads MenuScene after login
- [ ] Error messages display correctly

### Remember Me
- [ ] Credentials saved when checked
- [ ] Credentials loaded on next session
- [ ] Credentials cleared when unchecked

## Notes
- Google Sign-In requires OAuth setup for Unity Editor/Standalone
- All async operations are properly handled with try/catch
- UI is disabled during processing to prevent multiple submissions
- Error messages are user-friendly and localized
- Remember Me uses simple Base64 encoding (not secure for production)

## Troubleshooting
- Ensure Firebase is properly initialized
- Check all UI references are assigned
- Verify google-services.json is in correct location
- Check console for Firebase initialization errors
- Ensure Firestore rules allow read/write for authenticated users
