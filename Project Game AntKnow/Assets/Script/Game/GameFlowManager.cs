using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameFlowManager : NetworkBehaviour {
  [Header("Game Settings")]
  [SerializeField] string gameSceneName = "GameScene";
  [SerializeField] float gameStartDelay = 3f;

  public static GameFlowManager Instance { get; private set; }

  void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  void Start() {
    // Subscribe to network events
    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
  }

  void OnDestroy() {
    if (NetworkManager.Singleton != null) {
      NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
      NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
  }

  public void StartGame() {
    if (!IsHost) {
      Debug.LogWarning("Only host can start the game");
      return;
    }

    Debug.Log("Starting game...");
    
    // Notify all clients that game is starting
    StartGameClientRpc();
    
    // Load game scene after delay
    Invoke(nameof(LoadGameScene), gameStartDelay);
  }

  [ClientRpc]
  void StartGameClientRpc() {
    Debug.Log("Game is starting!");
    
    // Show countdown or loading screen
    if (MultiplayerManager.Instance != null) {
      MultiplayerManager.Instance.UpdateStatus("Game starting in " + gameStartDelay + " seconds...");
    }
  }

  void LoadGameScene() {
    if (IsHost) {
      // Load the game scene for all clients
      NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }
  }

  void OnClientConnected(ulong clientId) {
    Debug.Log($"Client {clientId} connected");
    
    if (IsHost) {
      // Update lobby info when someone joins
      UpdateLobbyInfo();
    }
  }

  void OnClientDisconnected(ulong clientId) {
    Debug.Log($"Client {clientId} disconnected");
    
    if (IsHost) {
      // Update lobby info when someone leaves
      UpdateLobbyInfo();
    }
  }

  void UpdateLobbyInfo() {
    if (MultiplayerManager.Instance != null) {
      MultiplayerManager.Instance.UpdateUI();
    }
  }

  // Called when game scene is loaded
  public void OnGameSceneLoaded() {
    Debug.Log("Game scene loaded - starting gameplay");
    
    // Initialize game logic here
    InitializeGameplay();
  }

  void InitializeGameplay() {
    // Add your game initialization logic here
    Debug.Log("Gameplay initialized");
  }

  // Public methods for other scripts
  public bool IsGameStarted { get; private set; }
  public int ConnectedPlayers => NetworkManager.Singleton?.ConnectedClients.Count ?? 0;
}
