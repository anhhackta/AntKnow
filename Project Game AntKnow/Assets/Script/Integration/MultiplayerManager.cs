using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerManager : MonoBehaviour {
  [Header("UI References")]
  [SerializeField] Button hostButton;
  [SerializeField] Button clientButton;
  [SerializeField] Button disconnectButton;
  [SerializeField] InputField ipAddressInput;
  [SerializeField] InputField portInput;
  [SerializeField] InputField playerNameInput;
  [SerializeField] Text statusText;
  [SerializeField] Text playerListText;

  [Header("Network Settings")]
  [SerializeField] string defaultIP = "127.0.0.1";
  [SerializeField] ushort defaultPort = 7777;
  [SerializeField] int maxPlayers = 4;

  public static MultiplayerManager Instance { get; private set; }

  bool _isHost;
  string _playerName = "Player";
  NetworkManager _networkManager;

  void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  void Start() {
    SetupUI();
    _networkManager = NetworkManager.Singleton;
    
    // Subscribe to network events
    if (_networkManager != null) {
      _networkManager.OnClientConnectedCallback += OnClientConnected;
      _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }
  }

  void OnDestroy() {
    if (_networkManager != null) {
      _networkManager.OnClientConnectedCallback -= OnClientConnected;
      _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }
  }

  void SetupUI() {
    if (hostButton) hostButton.onClick.AddListener(StartHost);
    if (clientButton) clientButton.onClick.AddListener(StartClient);
    if (disconnectButton) disconnectButton.onClick.AddListener(Disconnect);
    
    if (ipAddressInput) ipAddressInput.text = defaultIP;
    if (portInput) portInput.text = defaultPort.ToString();

    UpdateUI();
  }

  public void StartHost() {
    try {
      UpdateStatus("Starting host...");
      
      if (!string.IsNullOrEmpty(playerNameInput?.text)) {
        _playerName = playerNameInput.text;
      }

      if (_networkManager != null && _networkManager.StartHost()) {
        _isHost = true;
        UpdateStatus("Host started successfully!");
        Debug.Log("Host started");
      } else {
        UpdateStatus("Failed to start host");
      }

      UpdateUI();
    } catch (Exception ex) {
      Debug.LogError($"Failed to start host: {ex}");
      UpdateStatus($"Error: {ex.Message}");
    }
  }

  public void StartClient() {
    try {
      UpdateStatus("Connecting to host...");
      
      if (!string.IsNullOrEmpty(playerNameInput?.text)) {
        _playerName = playerNameInput.text;
      }

      // Get IP and port from input fields
      string ip = !string.IsNullOrEmpty(ipAddressInput?.text) ? ipAddressInput.text : defaultIP;
      ushort port = ushort.TryParse(portInput?.text, out ushort parsedPort) ? parsedPort : defaultPort;

      // Configure transport
      var transport = _networkManager.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
      if (transport != null) {
        transport.SetConnectionData(ip, port);
      }

      if (_networkManager != null && _networkManager.StartClient()) {
        _isHost = false;
        UpdateStatus($"Connecting to {ip}:{port}...");
        Debug.Log($"Connecting to {ip}:{port}");
      } else {
        UpdateStatus("Failed to start client");
      }

      UpdateUI();
    } catch (Exception ex) {
      Debug.LogError($"Failed to start client: {ex}");
      UpdateStatus($"Error: {ex.Message}");
    }
  }

  public void Disconnect() {
    try {
      if (_networkManager != null) {
        _networkManager.Shutdown();
      }
      
      _isHost = false;
      UpdateStatus("Disconnected");
      UpdateUI();
    } catch (Exception ex) {
      Debug.LogError($"Failed to disconnect: {ex}");
    }
  }

  void OnClientConnected(ulong clientId) {
    Debug.Log($"Client {clientId} connected");
    UpdateStatus($"Client {clientId} connected");
    UpdateUI();
  }

  void OnClientDisconnected(ulong clientId) {
    Debug.Log($"Client {clientId} disconnected");
    UpdateStatus($"Client {clientId} disconnected");
    UpdateUI();
  }

  public void UpdateUI() {
    bool isConnected = _networkManager != null && _networkManager.IsConnectedClient || _networkManager.IsHost;
    
    if (hostButton) hostButton.interactable = !isConnected;
    if (clientButton) clientButton.interactable = !isConnected;
    if (disconnectButton) disconnectButton.interactable = isConnected;

    if (playerListText && _networkManager != null) {
      int connectedClients = _networkManager.ConnectedClients.Count;
      playerListText.text = $"Connected Players: {connectedClients}/{maxPlayers}";
    }
  }

  public void UpdateStatus(string message) {
    if (statusText) {
      statusText.text = message;
    }
    Debug.Log($"[MultiplayerManager] {message}");
  }

  // Public getters for other scripts
  public bool IsConnected => _networkManager != null && (_networkManager.IsConnectedClient || _networkManager.IsHost);
  public bool IsHost => _isHost;
  public int PlayerCount => _networkManager?.ConnectedClients.Count ?? 0;
  public int MaxPlayers => maxPlayers;
}
