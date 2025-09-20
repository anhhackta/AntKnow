using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkRuntimeManager : MonoBehaviour {
  [SerializeField] string lobbyName = "AntKnow Lobby";
  [SerializeField] int maxPlayers = 4;
  [SerializeField] float lobbyHeartbeatInterval = 15f;
  [SerializeField] bool autoInitializeOnAwake = true;

  public static NetworkRuntimeManager Instance { get; private set; }

  NetworkManager _networkManager;
  UnityTransport _transport;
  Lobby _joinedLobby;
  Coroutine _heartbeatCoroutine;
  bool _servicesInitialized;
  string _playerName = "Player";

  void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
    EnsureNetworkComponents();

    if (autoInitializeOnAwake) _ = InitializeServicesAsync();
  }

  void OnDisable() {
    if (_heartbeatCoroutine != null) {
      StopCoroutine(_heartbeatCoroutine);
      _heartbeatCoroutine = null;
    }
  }

  void OnDestroy() {
    if (Instance == this) Instance = null;
  }

  void EnsureNetworkComponents() {
    _networkManager = GetComponent<NetworkManager>();
    if (_networkManager == null) _networkManager = gameObject.AddComponent<NetworkManager>();

    _transport = GetComponent<UnityTransport>();
    if (_transport == null) _transport = gameObject.AddComponent<UnityTransport>();

    if (_networkManager.NetworkConfig == null) _networkManager.NetworkConfig = new NetworkConfig();
    _networkManager.NetworkConfig.NetworkTransport = _transport;
    _networkManager.NetworkConfig.ConnectionApproval = false;
    _networkManager.NetworkConfig.CheckPhysics = false;
    _networkManager.NetworkConfig.CheckPhysics2D = false;
  }

  public async Task InitializeServicesAsync(string playerName = null) {
    if (!string.IsNullOrEmpty(playerName)) _playerName = playerName;

    if (!_servicesInitialized) {
      try {
        await UnityServices.InitializeAsync();
      } catch (InvalidOperationException) {
        // already initialized
      }

      if (!AuthenticationService.Instance.IsSignedIn) {
        try {
          await AuthenticationService.Instance.SignInAnonymouslyAsync(new SignInOptions { CreateAccount = true });
        } catch (AuthenticationException ex) {
          Debug.LogError($"Authentication failed: {ex}");
          throw;
        } catch (RequestFailedException ex) {
          Debug.LogError($"Authentication request failed: {ex}");
          throw;
        }
      }

      _servicesInitialized = true;
    }
  }

  public async void HostLobby() {
    try {
      await HostLobbyAsync();
    } catch (Exception ex) {
      Debug.LogError($"Failed to host lobby: {ex}");
    }
  }

  public async Task<string> HostLobbyAsync(string playerName = null) {
    await InitializeServicesAsync(playerName);

    int relayConnections = Mathf.Max(1, maxPlayers - 1);
    Allocation allocation;
    try {
      allocation = await RelayService.Instance.CreateAllocationAsync(relayConnections);
    } catch (RelayServiceException ex) {
      Debug.LogError($"Relay allocation failed: {ex}");
      throw;
    }

    string joinCode;
    try {
      joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
    } catch (RelayServiceException ex) {
      Debug.LogError($"Failed to fetch relay join code: {ex}");
      throw;
    }

    var lobbyOptions = new CreateLobbyOptions {
      Player = new Player(AuthenticationService.Instance.PlayerId,
                          null,
                          new Dictionary<string, PlayerDataObject> {
                            ["name"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)
                          }),
      Data = new Dictionary<string, DataObject> {
        ["relayJoinCode"] = new DataObject(DataObject.VisibilityOptions.Member, joinCode)
      }
    };

    try {
      _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
    } catch (LobbyServiceException ex) {
      Debug.LogError($"Failed to create lobby: {ex}");
      throw;
    }

    ConfigureTransportAsHost(allocation);

    if (!_networkManager.StartHost()) Debug.LogError("NetworkManager could not start host session.");

    StartLobbyHeartbeat();
    Debug.Log($"Lobby {_joinedLobby.LobbyCode} created with relay join code {joinCode}");
    return _joinedLobby.LobbyCode;
  }

  public async void JoinLobbyByCode(string lobbyCode) {
    try {
      await JoinLobbyByCodeAsync(lobbyCode);
    } catch (Exception ex) {
      Debug.LogError($"Failed to join lobby: {ex}");
    }
  }

  public async Task JoinLobbyByCodeAsync(string lobbyCode, string playerName = null) {
    await InitializeServicesAsync(playerName);

    var options = new JoinLobbyByCodeOptions {
      Player = new Player(AuthenticationService.Instance.PlayerId,
                          null,
                          new Dictionary<string, PlayerDataObject> {
                            ["name"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)
                          })
    };

    try {
      _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
    } catch (LobbyServiceException ex) {
      Debug.LogError($"Failed to join lobby: {ex}");
      throw;
    }

    if (!_joinedLobby.Data.TryGetValue("relayJoinCode", out var relayData) || string.IsNullOrEmpty(relayData.Value)) {
      throw new InvalidOperationException("Lobby does not contain relay join code data.");
    }

    JoinAllocation allocation;
    try {
      allocation = await RelayService.Instance.JoinAllocationAsync(relayData.Value);
    } catch (RelayServiceException ex) {
      Debug.LogError($"Relay join failed: {ex}");
      throw;
    }

    ConfigureTransportAsClient(allocation);

    if (!_networkManager.StartClient()) Debug.LogError("NetworkManager could not start client session.");

    Debug.Log($"Joined lobby {_joinedLobby.LobbyCode} using relay code {relayData.Value}");
  }

  public async Task QuickJoinAsync(string playerName = null) {
    await InitializeServicesAsync(playerName);

    try {
      _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions {
        Player = new Player(AuthenticationService.Instance.PlayerId,
                            null,
                            new Dictionary<string, PlayerDataObject> {
                              ["name"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)
                            })
      });
    } catch (LobbyServiceException ex) {
      Debug.LogError($"Quick join failed: {ex}");
      throw;
    }

    if (!_joinedLobby.Data.TryGetValue("relayJoinCode", out var relayData) || string.IsNullOrEmpty(relayData.Value)) {
      throw new InvalidOperationException("Quick joined lobby missing relay code.");
    }

    JoinAllocation allocation;
    try {
      allocation = await RelayService.Instance.JoinAllocationAsync(relayData.Value);
    } catch (RelayServiceException ex) {
      Debug.LogError($"Relay join failed: {ex}");
      throw;
    }

    ConfigureTransportAsClient(allocation);
    if (!_networkManager.StartClient()) Debug.LogError("NetworkManager could not start client session.");
  }

  public async Task LeaveLobbyAsync() {
    if (_heartbeatCoroutine != null) {
      StopCoroutine(_heartbeatCoroutine);
      _heartbeatCoroutine = null;
    }

    if (_joinedLobby != null && _servicesInitialized) {
      try {
        if (_networkManager != null && _networkManager.IsHost) await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
        else await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
      } catch (LobbyServiceException ex) {
        Debug.LogWarning($"Failed to leave lobby cleanly: {ex}");
      }
    }

    _joinedLobby = null;
    _networkManager?.Shutdown();
  }

  void ConfigureTransportAsHost(Allocation allocation) {
    var relayServerData = new RelayServerData(allocation, "dtls");
    _transport.SetRelayServerData(relayServerData);
  }

  void ConfigureTransportAsClient(JoinAllocation allocation) {
    var relayServerData = new RelayServerData(allocation, "dtls");
    _transport.SetRelayServerData(relayServerData);
  }

  void StartLobbyHeartbeat() {
    if (_heartbeatCoroutine != null) StopCoroutine(_heartbeatCoroutine);
    if (_joinedLobby == null) return;
    _heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
  }

  IEnumerator HeartbeatCoroutine() {
    var wait = new WaitForSecondsRealtime(lobbyHeartbeatInterval);
    while (_joinedLobby != null && _networkManager != null && _networkManager.IsHost) {
      var task = LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
      while (!task.IsCompleted) yield return null;
      if (task.Exception != null) Debug.LogWarning($"Heartbeat failed: {task.Exception}");
      yield return wait;
    }
    _heartbeatCoroutine = null;
  }

  public string CurrentLobbyCode => _joinedLobby?.LobbyCode;
  public string CurrentRelayJoinCode => _joinedLobby != null && _joinedLobby.Data.TryGetValue("relayJoinCode", out var data) ? data.Value : null;
  public bool IsHost => _networkManager != null && _networkManager.IsHost;
}
