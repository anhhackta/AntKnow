using UnityEngine;
using Unity.Netcode;
using AntKnow.Services;
using AntKnow.Auth;

namespace AntKnow.Game
{
    /// <summary>
    /// Component tự động khởi động NetworkManager khi GameScene load
    /// Gắn vào GameObject có NetworkManager trong GameScene
    /// </summary>
    public class GameSceneNetworkStarter : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private bool hasStarted = false;

        private void Start()
        {
            StartNetworkAutomatically();
        }

        /// <summary>
        /// Tự động khởi động network dựa trên GameSessionData
        /// </summary>
        private void StartNetworkAutomatically()
        {
            if (hasStarted)
            {
                DebugLog("Network already started, skipping...");
                return;
            }

            // Get session data
            var sessionData = GameSessionData.Instance;
            if (sessionData == null)
            {
                DebugLogError("GameSessionData not found! Cannot start network.");
                return;
            }

            // Check if we have relay info
            if (string.IsNullOrEmpty(sessionData.relayJoinCode))
            {
                DebugLogError("No relay join code found! Cannot start network.");
                return;
            }

            bool isHost = sessionData.isHost;
            string relayJoinCode = sessionData.relayJoinCode;

            DebugLog($"Starting network as {(isHost ? "HOST" : "CLIENT")} with relay code: {relayJoinCode}");

            // Ensure transport is configured
            var relayService = RelayService.Instance;
            if (relayService == null)
            {
                DebugLogError("RelayService not found!");
                return;
            }

            // ✅ CRITICAL: Re-configure transport for GameScene
            bool transportConfigured = relayService.ConfigureTransportForGameScene();
            if (!transportConfigured)
            {
                DebugLogError("Failed to configure transport!");
                return;
            }

            // Start network
            if (isHost)
            {
                // Host should already have transport configured from CreateRelay
                bool started = relayService.StartHost();
                if (started)
                {
                    DebugLog("✓ Successfully started as Host");
                    hasStarted = true;
                }
                else
                {
                    DebugLogError("✗ Failed to start as Host");
                }
            }
            else
            {
                // Client should have transport configured from JoinRelay in MenuScene
                // But double-check and ensure transport is ready
                StartCoroutine(StartClientAfterDelay());
            }
        }

        /// <summary>
        /// Start client with small delay to ensure transport is ready
        /// </summary>
        private System.Collections.IEnumerator StartClientAfterDelay()
        {
            // Small delay to ensure transport is fully configured
            yield return new WaitForSeconds(0.5f);

            var relayService = RelayService.Instance;
            bool started = relayService.StartClient();
            
            if (started)
            {
                DebugLog("✓ Successfully started as Client");
                hasStarted = true;
            }
            else
            {
                DebugLogError("✗ Failed to start as Client");
            }
        }

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameSceneNetworkStarter] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[GameSceneNetworkStarter] {message}");
            }
        }

        #endregion
    }
}
