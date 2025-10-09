using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace AntKnow.Server
{
    /// <summary>
    /// Server Bootstrap - Auto-start server on headless build
    /// Tự động khởi động server khi chạy ở chế độ headless
    /// </summary>
    public class ServerBootstrap : MonoBehaviour
    {
        [Header("Server Settings")]
        [SerializeField] private ushort serverPort = 7777;
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private bool autoStartServer = true;
        [SerializeField] private string serverName = "AntKnow Server";

        [Header("Performance")]
        [SerializeField] private int targetFrameRate = 30;
        [SerializeField] private bool enableDetailedLogs = true;

        private NetworkManager networkManager;

        private void Awake()
        {
            // Check if running as dedicated server (headless mode)
            bool isDedicatedServer = Application.isBatchMode || autoStartServer;
            
            if (!isDedicatedServer)
            {
                Log("Not running as dedicated server, skipping auto-start");
                return;
            }

            Log("=== DEDICATED SERVER MODE DETECTED ===");
            OptimizeServerPerformance();
            InitializeServer();
        }

        private void OptimizeServerPerformance()
        {
            // Set target frame rate (server doesn't need 60fps)
            Application.targetFrameRate = targetFrameRate;
            
            // Disable VSync
            QualitySettings.vSyncCount = 0;
            
            // Set lowest quality (no graphics needed)
            QualitySettings.SetQualityLevel(0, true);
            
            // Disable audio
            AudioListener.volume = 0;

            Log($"Server optimized: {targetFrameRate} FPS target, Quality: Lowest");
        }

        private void InitializeServer()
        {
            networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                LogError("NetworkManager not found in scene!");
                return;
            }

            // Configure transport
            var transport = networkManager.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("0.0.0.0", serverPort);
                Log($"Server configured to listen on 0.0.0.0:{serverPort}");
            }
            else
            {
                LogError("UnityTransport component not found!");
                return;
            }

            // Enable connection approval
            networkManager.NetworkConfig.ConnectionApproval = true;
            networkManager.ConnectionApprovalCallback = ApprovalCheck;

            // Register callbacks
            networkManager.OnServerStarted += OnServerStarted;

            // Start server
            bool started = networkManager.StartServer();
            if (started)
            {
                Log($"✅ SERVER STARTED SUCCESSFULLY");
                Log($"Server Name: {serverName}");
                Log($"Port: {serverPort}");
                Log($"Max Players: {maxPlayers}");
                Log($"Waiting for clients to connect...");
            }
            else
            {
                LogError("❌ FAILED TO START SERVER!");
            }
        }

        private void OnServerStarted()
        {
            Log("Server is now listening for connections");
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            ulong clientId = request.ClientNetworkId;

            // Check max players
            if (networkManager.ConnectedClients.Count >= maxPlayers)
            {
                response.Approved = false;
                response.Reason = "Server full";
                LogWarning($"Connection REJECTED from Client {clientId}: Server full ({maxPlayers}/{maxPlayers})");
                return;
            }

            // Approve connection
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = null; // Use default player prefab

            int currentPlayers = networkManager.ConnectedClients.Count + 1;
            Log($"✅ Client {clientId} APPROVED. Players: {currentPlayers}/{maxPlayers}");
        }

        private void OnApplicationQuit()
        {
            Log("=== SERVER SHUTTING DOWN ===");
            
            if (networkManager != null && networkManager.IsServer)
            {
                networkManager.Shutdown();
            }
        }

        // Logging helpers
        private void Log(string message)
        {
            if (enableDetailedLogs)
            {
                Debug.Log($"[ServerBootstrap] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ServerBootstrap] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[ServerBootstrap] {message}");
        }

        // Display server info in console
        private void Start()
        {
            if (Application.isBatchMode || autoStartServer)
            {
                InvokeRepeating(nameof(PrintServerStatus), 10f, 30f);
            }
        }

        private void PrintServerStatus()
        {
            if (networkManager != null && networkManager.IsServer)
            {
                int connectedPlayers = networkManager.ConnectedClients.Count;
                Log($"--- Server Status ---");
                Log($"Connected Players: {connectedPlayers}/{maxPlayers}");
                Log($"Uptime: {Time.realtimeSinceStartup:F0} seconds");
                Log($"--------------------");
            }
        }
    }
}

