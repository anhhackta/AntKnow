using UnityEngine;

/// <summary>
/// HƯỚNG DẪN SỬA LỖI PACKAGE UNITY
/// 
/// NẾU VẪN CÒN LỖI, HÃY LÀM THEO CÁC BƯỚC SAU:
/// 
/// 1. ĐÓNG UNITY EDITOR HOÀN TOÀN
/// 2. XÓA THỦ CÔNG CÁC FOLDER SAU:
///    - Library/PackageCache
///    - Library/ScriptAssemblies
///    - Library/ArtifactDB
///    - Library/ArtifactDB-lock
/// 
/// 3. MỞ LẠI UNITY EDITOR
/// 4. ĐỢI UNITY IMPORT LẠI PACKAGES
/// 5. KIỂM TRA PACKAGE MANAGER:
///    - Window → Package Manager
///    - Chọn "In Project"
///    - Xóa package nào có dấu lỗi
/// 
/// 6. NẾU VẪN LỖI, THỰC HIỆN:
///    - Edit → Project Settings → XR Plug-in Management
///    - Bỏ tick tất cả XR providers
///    - Restart Unity
/// 
/// 7. CUỐI CÙNG, NẾU VẪN LỖI:
///    - Tạo project mới
///    - Copy Assets folder từ project cũ
///    - Import lại packages
/// </summary>
public class UnityPackageFixGuide : MonoBehaviour {
  [Header("Troubleshooting Steps")]
  [TextArea(15, 25)]
  public string troubleshootingSteps = @"
UNITY PACKAGE FIX GUIDE:

IF YOU STILL HAVE ERRORS, FOLLOW THESE STEPS:

1. CLOSE UNITY EDITOR COMPLETELY
2. MANUALLY DELETE THESE FOLDERS:
   - Library/PackageCache
   - Library/ScriptAssemblies  
   - Library/ArtifactDB
   - Library/ArtifactDB-lock

3. REOPEN UNITY EDITOR
4. WAIT FOR UNITY TO REIMPORT PACKAGES
5. CHECK PACKAGE MANAGER:
   - Window → Package Manager
   - Select 'In Project'
   - Remove any packages with error icons

6. IF STILL ERRORS, TRY:
   - Edit → Project Settings → XR Plug-in Management
   - Uncheck all XR providers
   - Restart Unity

7. LAST RESORT:
   - Create new project
   - Copy Assets folder from old project
   - Reimport packages

CURRENT PACKAGE CONFIGURATION:
- com.unity.services.authentication: 3.2.0
- com.unity.services.lobbies: 1.0.0
- com.unity.services.matchmaker: 1.1.5
- com.unity.services.relay: 1.0.3
- com.unity.netcode.gameobjects: 2.5.1

DO NOT ADD:
- com.unity.services.multiplayer (causes conflicts)
";

  void Start() {
    Debug.Log("Unity Package Fix Guide loaded. Check the troubleshooting steps in the inspector.");
  }
}
