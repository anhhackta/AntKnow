using UnityEngine;
using AntKnow.Auth;
using System.Threading.Tasks;

public class Logmainscene : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private FirebaseAuthService firebaseAuthService;
    [SerializeField] private AuthUIController authUIController;

    private async void Start()
    {
        // Initialize Firebase Auth Service
        if (firebaseAuthService == null)
        {
            firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            if (firebaseAuthService == null)
            {
                GameObject authServiceObj = new GameObject("FirebaseAuthService");
                firebaseAuthService = authServiceObj.AddComponent<FirebaseAuthService>();
            }
        }

        // Initialize Auth UI Controller
        if (authUIController == null)
        {
            authUIController = FindObjectOfType<AuthUIController>();
        }

        // Initialize Avatar Panel
        var avatarPanel = FindObjectOfType<AvatarPanel>();
        if (avatarPanel == null)
        {
            Debug.LogWarning("AvatarPanel not found in scene. Please setup AvatarPanel UI according to PANEL_AVATAR_SETUP.md");
        }

        // Initialize Firebase trước khi UI hoạt động
        Debug.Log("Đang khởi tạo Firebase...");
        bool initSuccess = await firebaseAuthService.InitAsync();
        if (!initSuccess)
        {
            Debug.LogError("Failed to initialize Firebase Auth Service");
            // Vẫn cho phép UI hoạt động nhưng sẽ hiển thị thông báo lỗi
        }
        else
        {
            Debug.Log("Firebase initialized successfully!");
        }
    }
}
