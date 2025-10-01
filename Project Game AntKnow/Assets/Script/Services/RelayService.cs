using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace AntKnow.Services
{
    /// <summary>
    /// Service quản lý Unity Relay cho kết nối multiplayer
    /// </summary>
    public class RelayService : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Events
        public static event Action<string> OnRelayCreated; // Join code
        public static event Action OnRelayJoined;
        public static event Action<string> OnRelayError;

        // Properties
        public string CurrentJoinCode { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsConnected { get; private set; }

        private static RelayService _instance;
        public static RelayService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RelayService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RelayService");
                        _instance = go.AddComponent<RelayService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private UnityTransport transport;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Get Unity Transport component
            transport = FindObjectOfType<UnityTransport>();
            if (transport == null)
            {
                DebugLogError("UnityTransport component not found! Please add it to NetworkManager.");
            }
        }

        /// <summary>
        /// Tạo Relay allocation và trả về join code
        /// </summary>
        public async Task<string> CreateRelayAsync()
        {
            try
            {
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to create relay");
                    OnRelayError?.Invoke("Chưa đăng nhập UGS");
                    return null;
                }

                DebugLog($"Creating Relay allocation for {GameConfig.MAX_PLAYERS} players...");

                // Create allocation (maxConnections = MAX_PLAYERS - 1 vì host không tính)
                var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(GameConfig.RELAY_MAX_CONNECTIONS);
                
                // Get join code
                var joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                CurrentJoinCode = joinCode;
                IsHost = true;
                IsConnected = true;

                // Configure transport
                if (transport != null)
                {
                    transport.SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                    );
                    
                    DebugLog("Transport configured for host");
                }
                else
                {
                    DebugLogError("UnityTransport not found!");
                }

                DebugLog($"Relay created successfully. Join code: {joinCode}");
                OnRelayCreated?.Invoke(joinCode);
                
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                DebugLogError($"Failed to create relay: {e.Message}");
                OnRelayError?.Invoke($"Không thể tạo Relay: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                DebugLogError($"Unexpected error creating relay: {e.Message}");
                OnRelayError?.Invoke($"Lỗi không xác định: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tham gia Relay bằng join code
        /// </summary>
        public async Task<bool> JoinRelayAsync(string joinCode)
        {
            try
            {
                if (!UGSAuthService.IsSignedIn)
                {
                    DebugLogError("Must be signed in to UGS to join relay");
                    OnRelayError?.Invoke("Chưa đăng nhập UGS");
                    return false;
                }

                if (string.IsNullOrEmpty(joinCode))
                {
                    DebugLogError("Join code is null or empty");
                    OnRelayError?.Invoke("Mã tham gia không hợp lệ");
                    return false;
                }

                DebugLog($"Joining Relay with code: {joinCode}");

                // Join allocation
                var allocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
                
                CurrentJoinCode = joinCode;
                IsHost = false;
                IsConnected = true;

                // Configure transport
                if (transport != null)
                {
                    transport.SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData,
                        allocation.HostConnectionData
                    );
                    
                    DebugLog("Transport configured for client");
                }
                else
                {
                    DebugLogError("UnityTransport not found!");
                }

                DebugLog($"Joined Relay successfully with code: {joinCode}");
                OnRelayJoined?.Invoke();
                
                return true;
            }
            catch (RelayServiceException e)
            {
                DebugLogError($"Failed to join relay: {e.Message}");
                OnRelayError?.Invoke($"Không thể tham gia Relay: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                DebugLogError($"Unexpected error joining relay: {e.Message}");
                OnRelayError?.Invoke($"Lỗi không xác định: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Start NetworkManager as Host
        /// </summary>
        public bool StartHost()
        {
            try
            {
                if (!IsConnected || !IsHost)
                {
                    DebugLogError("Not connected to Relay as host");
                    return false;
                }

                if (NetworkManager.Singleton == null)
                {
                    DebugLogError("NetworkManager not found!");
                    return false;
                }

                DebugLog("Starting NetworkManager as Host...");
                
                bool started = NetworkManager.Singleton.StartHost();
                
                if (started)
                {
                    DebugLog("NetworkManager started as Host successfully");
                }
                else
                {
                    DebugLogError("Failed to start NetworkManager as Host");
                }
                
                return started;
            }
            catch (Exception e)
            {
                DebugLogError($"Error starting host: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Start NetworkManager as Client
        /// </summary>
        public bool StartClient()
        {
            try
            {
                if (!IsConnected || IsHost)
                {
                    DebugLogError("Not connected to Relay as client");
                    return false;
                }

                if (NetworkManager.Singleton == null)
                {
                    DebugLogError("NetworkManager not found!");
                    return false;
                }

                DebugLog("Starting NetworkManager as Client...");
                
                bool started = NetworkManager.Singleton.StartClient();
                
                if (started)
                {
                    DebugLog("NetworkManager started as Client successfully");
                }
                else
                {
                    DebugLogError("Failed to start NetworkManager as Client");
                }
                
                return started;
            }
            catch (Exception e)
            {
                DebugLogError($"Error starting client: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from Relay
        /// </summary>
        public void Disconnect()
        {
            try
            {
                DebugLog("Disconnecting from Relay...");
                
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    NetworkManager.Singleton.Shutdown();
                    DebugLog("NetworkManager shutdown");
                }

                CurrentJoinCode = null;
                IsHost = false;
                IsConnected = false;
                
                DebugLog("Disconnected from Relay");
            }
            catch (Exception e)
            {
                DebugLogError($"Error disconnecting: {e.Message}");
            }
        }

        /// <summary>
        /// Get current connection info
        /// </summary>
        public string GetConnectionInfo()
        {
            if (!IsConnected)
                return "Not connected";
                
            return $"Connected as {(IsHost ? "Host" : "Client")} - Join Code: {CurrentJoinCode}";
        }

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[RelayService] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[RelayService] {message}");
            }
        }

        #endregion

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsConnected)
            {
                DebugLog("Application paused, maintaining Relay connection");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsConnected)
            {
                DebugLog("Application lost focus, maintaining Relay connection");
            }
        }
    }
}
