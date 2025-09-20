using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class UgsLobbyRelayService : MonoBehaviour {
  [SerializeField] string environmentName = "production";
  [SerializeField] string gameVersion = "0.1.0";
  [SerializeField] float heartbeatInterval = 15f;

  Lobby _joinedLobby;
  Coroutine _heartbeatRoutine;
  bool _initialized;

  UnityTransport Transport => NetworkManager.Singleton != null ? NetworkManager.Singleton.GetComponent<UnityTransport>() : null;

  async void Awake() {
    await InitializeAsync();
  }

  async void OnDestroy() {
    if (_joinedLobby != null) {
      try {
        await LeaveLobbyAsync();
      } catch (Exception ex) {
        Debug.LogWarning($"Leave lobby failed: {ex.Message}");
      }
    }
  }

  public async Task InitializeAsync() {
    if (_initialized) return;
    try {
      var options = new InitializationOptions();
      options.SetEnvironmentName(environmentName);
      await UnityServices.InitializeAsync(options);
      if (!AuthenticationService.Instance.IsSignedIn) {
        await AuthenticationService.Instance.SignInAnonymouslyAsync(new SignInOptions { CreateAccount = true });
      }
      _initialized = true;
    } catch (Exception ex) {
      Debug.LogError($"UGS init failed: {ex.Message}");
      throw;
    }
  }

  public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate = false) {
    await InitializeAsync();
    try {
      var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions {
        IsPrivate = isPrivate,
        Data = new Dictionary<string, DataObject> {
          {"version", new DataObject(DataObject.VisibilityOptions.Public, gameVersion, DataObject.IndexOptions.S1)}
        }
      });

      _joinedLobby = lobby;
      StartHeartbeat();

      var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
      var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

      await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions {
        Data = new Dictionary<string, DataObject> {
          {"joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode)}
        }
      });

      SetupHostTransport(allocation);
      NetworkManager.Singleton.StartHost();
      return lobby;
    } catch (Exception ex) {
      Debug.LogError($"Create lobby failed: {ex.Message}");
      throw;
    }
  }

  public async Task<Lobby> QuickJoinAsync() {
    await InitializeAsync();
    try {
      var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions {
        Filter = new List<QueryFilter> {
          new QueryFilter(QueryFilter.FieldOptions.S1, gameVersion, QueryFilter.OpOptions.EQ)
        }
      });
      return await JoinLobbyInternalAsync(lobby);
    } catch (LobbyServiceException lse) when (lse.Reason == LobbyExceptionReason.NoOpenLobbies) {
      Debug.LogWarning("No open lobby found");
      return null;
    }
  }

  public async Task<Lobby> JoinLobbyByCodeAsync(string lobbyCode) {
    await InitializeAsync();
    try {
      var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
      return await JoinLobbyInternalAsync(lobby);
    } catch (Exception ex) {
      Debug.LogError($"Join lobby failed: {ex.Message}");
      throw;
    }
  }

  async Task<Lobby> JoinLobbyInternalAsync(Lobby lobby) {
    _joinedLobby = lobby;
    StartHeartbeat();

    if (!lobby.Data.TryGetValue("joinCode", out var joinCodeData)) {
      Debug.LogError("Lobby missing relay join code");
      return lobby;
    }

    var allocation = await RelayService.Instance.JoinAllocationAsync(joinCodeData.Value);
    SetupClientTransport(allocation);
    NetworkManager.Singleton.StartClient();
    return lobby;
  }

  public async Task LeaveLobbyAsync() {
    if (_joinedLobby == null) return;
    StopHeartbeat();
    try {
    await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
    } finally {
      _joinedLobby = null;
    }
  }

  void SetupHostTransport(Allocation allocation) {
    var transport = Transport;
    if (transport == null) {
      Debug.LogError("UnityTransport not found on NetworkManager");
      return;
    }
    var (host, port, secure) = ExtractEndpoint(allocation.ServerEndpoints, "dtls");
    if (string.IsNullOrEmpty(host)) {
      Debug.LogError("No relay endpoint available for host");
      return;
    }
    transport.SetRelayServerData(host, port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.ConnectionData, allocation.Key, secure);
  }

  void SetupClientTransport(JoinAllocation allocation) {
    var transport = Transport;
    if (transport == null) {
      Debug.LogError("UnityTransport not found on NetworkManager");
      return;
    }
    var (host, port, secure) = ExtractEndpoint(allocation.ServerEndpoints, "dtls");
    if (string.IsNullOrEmpty(host)) {
      Debug.LogError("No relay endpoint available for client");
      return;
    }
    transport.SetRelayServerData(host, port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.HostConnectionData, allocation.Key, secure);
  }

  (string host, ushort port, bool secure) ExtractEndpoint(IList<RelayServerEndpoint> endpoints, string preferred)
  {
    Unity.Services.Relay.Models.ServerEndpoint chosen = null;
    foreach (var ep in endpoints)
    {
      if (ep == null) continue;
      if (ep.ConnectionType == preferred){ chosen = ep; break; }
      if (chosen == null && ep.ConnectionType == "udp") chosen = ep;
    }
    if (chosen == null && endpoints.Count > 0) chosen = endpoints[0];
    if (chosen == null) return (null, 0, false);
    return (chosen.Host, (ushort)chosen.Port, chosen.Secure);
  }

  void StartHeartbeat() {
    if (_heartbeatRoutine != null) StopCoroutine(_heartbeatRoutine);
    if (_joinedLobby != null) {
      _heartbeatRoutine = StartCoroutine(HeartbeatCoroutine());
    }
  }

  void StopHeartbeat() {
    if (_heartbeatRoutine != null) {
      StopCoroutine(_heartbeatRoutine);
      _heartbeatRoutine = null;
    }
  }

  IEnumerator HeartbeatCoroutine() {
    var wait = new WaitForSeconds(heartbeatInterval);
    while (_joinedLobby != null) {
      yield return wait;
      try {
        awaitHeartbeat();
      } catch (Exception ex) {
        Debug.LogWarning($"Lobby heartbeat failed: {ex.Message}");
      }
    }
  }

  async void awaitHeartbeat() {
    if (_joinedLobby == null) return;
    await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
  }
}
