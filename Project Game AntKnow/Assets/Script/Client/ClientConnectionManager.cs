using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Client
{
    /// <summary>
    /// Client Connection Manager
    /// Quản lý kết nối client tới dedicated server
    /// </summary>
    public class ClientConnectionManager : MonoBehaviour
    {
        [Header("Server Settings")]
        [SerializeField] private string defaultServerIP = "127.0.0.1";
        [SerializeField] private ushort serverPort = 7777;

        [Header("UI References")]
        [SerializeField] private Button connectButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject connectionPanel;
        [SerializeField] private GameObject gamePanel;

        [Header("Auto Connect")]
        [SerializeField] private bool autoConnect = false;
        [SerializeField] private float autoConnectDelay = 1f;

        private NetworkManager networkManager;
        private UnityTransport transport;

        private void Start()
        {
            InitializeUI();
            InitializeNetworking();

            if (autoConnect)
            {
                Invoke(nameof(ConnectToServer), autoConnectDelay);
            }
        }

        private void InitializeUI()
        {
            // Setup buttons
            if (connectButton != null)
            {
                connectButton.onClick.AddListener(ConnectToServer);
            }

            if (disconnectButton != null)
            {
                disconnectButton.onClick.AddListener(Disconnect);
                disconnectButton.gameObject.SetActive(false);
            }

            // Setup input field
            if (ipInputField != null)
            {
                ipInputField.text = defaultServerIP;
            }

            // Setup panels
            if (connectionPanel != null)
            {
                connectionPanel.SetActive(true);
            }

            if (gamePanel != null)
            {
                gamePanel.SetActive(false);
            }

            UpdateStatusText("Ready to connect");
        }

        private void InitializeNetworking()
        {
            networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[ClientConnection] NetworkManager not found!");
                UpdateStatusText("ERROR: NetworkManager not found");
                return;
            }

            transport = networkManager.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[ClientConnection] UnityTransport not found!");
                UpdateStatusText("ERROR: UnityTransport not found");
                return;
            }

            // Register callbacks
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public void ConnectToServer()
        {
            if (networkManager == null || transport == null)
            {
                Debug.LogError("[ClientConnection] NetworkManager or Transport not initialized");
                UpdateStatusText("ERROR: Not initialized");
                return;
            }

            // Get IP from input field
            string serverIP = defaultServerIP;
            if (ipInputField != null && !string.IsNullOrEmpty(ipInputField.text))
            {
                serverIP = ipInputField.text.Trim();
            }

            Debug.Log($"[ClientConnection] Connecting to {serverIP}:{serverPort}...");
            UpdateStatusText($"Connecting to {serverIP}:{serverPort}...");

            // Configure transport
            transport.SetConnectionData(serverIP, serverPort);

            // Start client
            bool started = networkManager.StartClient();

            if (started)
            {
                Debug.Log("[ClientConnection] ✅ Connection initiated");
                UpdateStatusText("Connecting...");

                // Disable connect button
                if (connectButton != null)
                {
                    connectButton.interactable = false;
                }
            }
            else
            {
                Debug.LogError("[ClientConnection] ❌ Failed to start client");
                UpdateStatusText("ERROR: Failed to start client");
            }
        }

        public void Disconnect()
        {
            if (networkManager != null && networkManager.IsClient)
            {
                Debug.Log("[ClientConnection] Disconnecting...");
                UpdateStatusText("Disconnecting...");
                networkManager.Shutdown();
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[ClientConnection] ✅ Connected! Client ID: {clientId}");
            UpdateStatusText($"Connected! (ID: {clientId})");

            // Show game UI
            if (connectionPanel != null)
            {
                connectionPanel.SetActive(false);
            }

            if (gamePanel != null)
            {
                gamePanel.SetActive(true);
            }

            // Show disconnect button
            if (disconnectButton != null)
            {
                disconnectButton.gameObject.SetActive(true);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[ClientConnection] ❌ Disconnected! Client ID: {clientId}");
            UpdateStatusText("Disconnected");

            // Show connection UI
            if (connectionPanel != null)
            {
                connectionPanel.SetActive(true);
            }

            if (gamePanel != null)
            {
                gamePanel.SetActive(false);
            }

            // Re-enable connect button
            if (connectButton != null)
            {
                connectButton.interactable = true;
            }

            // Hide disconnect button
            if (disconnectButton != null)
            {
                disconnectButton.gameObject.SetActive(false);
            }
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[ClientConnection] Status: {message}");
        }

        private void OnDestroy()
        {
            // Unregister callbacks
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        // Public methods for UI buttons
        public void OnConnectButtonClicked()
        {
            ConnectToServer();
        }

        public void OnDisconnectButtonClicked()
        {
            Disconnect();
        }

        // Helper: Connect to localhost (for testing)
        public void ConnectToLocalhost()
        {
            if (ipInputField != null)
            {
                ipInputField.text = "127.0.0.1";
            }
            ConnectToServer();
        }

        // Helper: Connect to LAN server
        public void ConnectToLAN(string lanIP)
        {
            if (ipInputField != null)
            {
                ipInputField.text = lanIP;
            }
            ConnectToServer();
        }
    }
}

