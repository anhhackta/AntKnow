using UnityEngine;

/// <summary>
/// Hướng dẫn thiết lập Multiplayer System
/// 
/// CÁCH SỬ DỤNG:
/// 
/// 1. THIẾT LẬP SCENE:
///    - Tạo GameObject tên "MultiplayerManager" và attach script MultiplayerManager
///    - Tạo GameObject tên "GameFlowManager" và attach script GameFlowManager
///    - Tạo UI Canvas với các button và input field:
///      * Button "Create Lobby"
///      * Button "Quick Join" 
///      * Button "Join By Code"
///      * Button "Start Game"
///      * Button "Leave Lobby"
///      * InputField "Lobby Code"
///      * InputField "Player Name"
///      * Text "Lobby Status"
///      * Text "Player List"
/// 
/// 2. CẤU HÌNH NETWORK MANAGER:
///    - Thêm NetworkManager component vào scene
///    - Cấu hình NetworkManager settings
///    - Thêm UnityTransport component
/// 
/// 3. TÍNH NĂNG:
///    - Tìm trận: Click "Quick Join" để tự động tìm lobby có sẵn
///    - Tạo phòng: Click "Create Lobby" để tạo phòng mới
///    - Vào phòng: Nhập mã phòng và click "Join By Code"
///    - Bắt đầu: Host click "Start Game" để bắt đầu game
/// 
/// 4. API SỬ DỤNG:
///    - MultiplayerManager.Instance.CreateLobby()
///    - MultiplayerManager.Instance.QuickJoin()
///    - MultiplayerManager.Instance.JoinByCode(code)
///    - MultiplayerManager.Instance.StartGame()
///    - MultiplayerManager.Instance.LeaveLobby()
/// 
/// 5. KIỂM TRA TRẠNG THÁI:
///    - MultiplayerManager.Instance.IsInLobby
///    - MultiplayerManager.Instance.IsHost
///    - MultiplayerManager.Instance.LobbyCode
///    - MultiplayerManager.Instance.PlayerCount
/// </summary>
public class MultiplayerSetupGuide : MonoBehaviour {
  [Header("Setup Instructions")]
  [TextArea(10, 20)]
  public string instructions = @"
MULTIPLAYER SETUP GUIDE:

1. SCENE SETUP:
   - Create GameObject 'MultiplayerManager' with MultiplayerManager script
   - Create GameObject 'GameFlowManager' with GameFlowManager script
   - Setup UI Canvas with required buttons and inputs

2. NETWORK CONFIGURATION:
   - Add NetworkManager component to scene
   - Configure NetworkManager settings
   - Add UnityTransport component

3. FEATURES:
   - Find Match: Quick Join button
   - Create Room: Create Lobby button  
   - Join Room: Join By Code button
   - Start Game: Start Game button (Host only)

4. USAGE:
   - Call MultiplayerManager.Instance methods
   - Check status with public properties
   - Handle network events in GameFlowManager

5. PACKAGES REQUIRED:
   - Unity Netcode for GameObjects
   - Unity Services Authentication
   - Unity Services Lobbies
   - Unity Services Relay
   - Unity Services Matchmaker
";

  void Start() {
    Debug.Log("Multiplayer Setup Guide loaded. Check the instructions in the inspector.");
  }
}
