using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using RandomGenerator = Unity.Mathematics.Random;

public class GameController : NetworkBehaviour {
  [Header("Data")]
  [SerializeField] BoardConfig board;
  [SerializeField] PropertyRuleSet propertyRules;
  [SerializeField] CardLibrary cardLibrary;
  [Header("Refs")]
  [SerializeField] PlayerController[] players;
  [SerializeField] TextMeshProUGUI turnText;
  [SerializeField] TextMeshProUGUI p1Money;
  [SerializeField] TextMeshProUGUI p2Money;
  [SerializeField] TextMeshProUGUI p3Money;
  [SerializeField] TextMeshProUGUI p4Money;
  [SerializeField] DiceView diceView;

  readonly NetworkVariable<GameMetaState> _metaState = new NetworkVariable<GameMetaState>(
    new GameMetaState {
      BoardLength = 0,
      CurrentTurnPlayerId = 0,
      PlayerCount = 0,
      Round = 1,
      SessionActive = false
    },
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );
  readonly NetworkVariable<DiceRollData> _lastDiceRoll = new NetworkVariable<DiceRollData>(
    new DiceRollData(),
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  NetworkList<PlayerStateData> _playerStates;
  NetworkList<PropertyStateData> _propertyStates;
  NetworkList<PlayerSlotData> _playerSlots;

  readonly Dictionary<ulong, int> _clientToPlayer = new();
  GameState _serverGame;
  TurnSystem _turnSystem;
  PropertyEconomy _economy;
  RandomGenerator _serverRandom;
  bool _randomInitialized;
  bool _serverStatePrepared;
  int _round = 1;
  int _localPlayerId = -1;

  void Awake() {
    _playerStates = new NetworkList<PlayerStateData>();
    _propertyStates = new NetworkList<PropertyStateData>();
    _playerSlots = new NetworkList<PlayerSlotData>();
  }


    _playerStates.OnListChanged += HandlePlayerStatesChanged;
    _propertyStates.OnListChanged += HandlePropertyStatesChanged;
    _playerSlots.OnListChanged += HandlePlayerSlotsChanged;
    _metaState.OnValueChanged += HandleMetaChanged;
    _lastDiceRoll.OnValueChanged += HandleDiceRollChanged;

    if (IsServer) {
      PrepareServerState();

      var manager = NetworkManager.Singleton;
      if (manager != null) {
        manager.OnClientConnectedCallback += HandleClientConnected;
        manager.OnClientDisconnectCallback += HandleClientDisconnected;
        foreach (var clientId in manager.ConnectedClientsIds) AssignPlayerSlot(clientId);
      }
    }

    RefreshUIFromNetwork();
    ApplyPlayersToBoard(true);
    UpdateLocalPlayerId();
  }

  public override void OnNetworkDespawn() {
    _playerStates.OnListChanged -= HandlePlayerStatesChanged;
    _propertyStates.OnListChanged -= HandlePropertyStatesChanged;
    _playerSlots.OnListChanged -= HandlePlayerSlotsChanged;
    _metaState.OnValueChanged -= HandleMetaChanged;
    _lastDiceRoll.OnValueChanged -= HandleDiceRollChanged;

    if (IsServer) {
      var manager = NetworkManager.Singleton;
      if (manager != null) {
        manager.OnClientConnectedCallback -= HandleClientConnected;
        manager.OnClientDisconnectCallback -= HandleClientDisconnected;
      }
    }

    base.OnNetworkDespawn();
  }

  void PrepareServerState() {
    if (_serverStatePrepared) return;

    int boardLength = board != null && board.tiles != null ? board.tiles.Length : 0;
    _serverGame = new GameState { BoardLength = boardLength, CurrentTurnPlayerId = 0 };

    if (board != null && board.tiles != null) {
      for (int i = 0; i < board.tiles.Length; i++) {
        var tile = board.tiles[i];
        if (tile != null && tile.type == TileType.Property) {
          _serverGame.Properties[i] = new PropertyState { TileId = i, BasePrice = tile.basePrice };
        }
      }
    }

    _economy = propertyRules != null ? propertyRules.ToEconomy() : new PropertyEconomy(
      new int[]{100,150,200,250,300},
      new int[]{25,50,75,100,125,150},
      400, 250,
      new int[]{150,200,300,400,500,600},
      false
    );
      tileId => {
        if (board != null && board.tiles != null && tileId >= 0 && tileId < board.tiles.Length) {
          var tile = board.tiles[tileId];
          if (tile != null) {
            int amount = tile.amount;
            int dest = tile.destNode;
            return (amount, dest >= 0 ? (int?)dest : null);
          }
        }
        return (0, (int?)null);
      },
      baseSalary: 200,
  }

  void HandleClientConnected(ulong clientId) {
    if (!IsServer) return;
    AssignPlayerSlot(clientId);
  }

  void HandleClientDisconnected(ulong clientId) {
    if (!IsServer || !_serverStatePrepared) return;
    if (!_clientToPlayer.TryGetValue(clientId, out int playerId)) return;

    _clientToPlayer.Remove(clientId);
    var player = _serverGame.Players.Find(x => x.Id == playerId);
    if (player != null) _serverGame.Players.Remove(player);

    if (players != null && playerId - 1 >= 0 && playerId - 1 < players.Length) {
      var view = players[playerId - 1];
      if (view != null) view.gameObject.SetActive(false);
    }

    UpdatePlayerSlot(playerId, 0, false);
    UpdateMetaAfterPlayerChange(true);
    SyncNetworkStateFromDomain();
  }

  void AssignPlayerSlot(ulong clientId) {
    if (!IsServer || !_serverStatePrepared) return;
    if (_clientToPlayer.ContainsKey(clientId)) return;

    int nextId = GetNextAvailablePlayerId();
    if (nextId <= 0) {
      Debug.LogWarning($"No player slots available for client {clientId}");
      return;
    }

    var newPlayer = new PlayerState { Id = nextId, Money = 1500, NodeIndex = 0 };
    _clientToPlayer[clientId] = nextId;
    _serverGame.Players.Add(newPlayer);
    _serverGame.Players.Sort((a, b) => a.Id.CompareTo(b.Id));

    if (players != null && nextId - 1 >= 0 && nextId - 1 < players.Length) {
      var view = players[nextId - 1];
      if (view != null) {
        view.gameObject.SetActive(true);
        view.Init(nextId, newPlayer.NodeIndex);
      }
    }

    UpdatePlayerSlot(nextId, clientId, true);
    UpdateMetaAfterPlayerChange(false);
    SyncNetworkStateFromDomain();
  }

  void UpdatePlayerSlot(int playerId, ulong clientId, bool connected) {
    var slot = new PlayerSlotData { PlayerId = playerId, ClientId = clientId, IsConnected = connected };
    bool replaced = false;
    for (int i = 0; i < _playerSlots.Count; i++) {
      if (_playerSlots[i].PlayerId == playerId) {
        _playerSlots[i] = slot;
        replaced = true;
        break;
      }
    }

    if (!replaced) _playerSlots.Add(slot);
  }
  }

  void UpdateMetaAfterPlayerChange(bool fromDisconnect) {
    var meta = _metaState.Value;
    meta.PlayerCount = _serverGame.Players.Count;
    meta.SessionActive = _serverGame.Players.Count > 0;

    if (_serverGame.Players.Count == 0) {
      _serverGame.CurrentTurnPlayerId = 0;
      meta.CurrentTurnPlayerId = 0;
      _round = 1;
      meta.Round = _round;
    } else {
      int current = meta.CurrentTurnPlayerId;
      if (!_serverGame.Players.Any(p => p.Id == current)) {
        meta.CurrentTurnPlayerId = _serverGame.Players.Min(p => p.Id);
      }
      if (!fromDisconnect && _serverGame.CurrentTurnPlayerId == 0) {
        meta.CurrentTurnPlayerId = _serverGame.Players.Min(p => p.Id);
      }
      _serverGame.CurrentTurnPlayerId = meta.CurrentTurnPlayerId;
      meta.Round = _round;
    }

    _metaState.Value = meta;
  }

  void SyncNetworkStateFromDomain() {
    if (!IsServer || _serverGame == null) return;

    _playerStates.Clear();
    foreach (var player in _serverGame.Players.OrderBy(p => p.Id)) {
      _playerStates.Add(new PlayerStateData(player));
    }

    _propertyStates.Clear();
    foreach (var property in _serverGame.Properties.Values.OrderBy(p => p.TileId)) {
      _propertyStates.Add(new PropertyStateData(property));
    }

    var meta = _metaState.Value;
    meta.PlayerCount = _serverGame.Players.Count;
    meta.BoardLength = _serverGame.BoardLength;
    _metaState.Value = meta;

    RefreshUIFromNetwork();
  }

  void HandlePlayerStatesChanged(NetworkListEvent<PlayerStateData> change) {
    RefreshUIFromNetwork();
    ApplyPlayersToBoard(true);
  }

  void HandlePropertyStatesChanged(NetworkListEvent<PropertyStateData> change) {
    RefreshUIFromNetwork();
  }

  void HandlePlayerSlotsChanged(NetworkListEvent<PlayerSlotData> change) {
    UpdateLocalPlayerId();
  }

  void HandleMetaChanged(GameMetaState previous, GameMetaState current) {
    RefreshUIFromNetwork();
  }

  void HandleDiceRollChanged(DiceRollData previous, DiceRollData current) {
    if (!HasValidRoll(current)) return;
    diceView?.ShowRoll(current.Die1, current.Die2);
  }

  void RefreshUIFromNetwork() {
    var meta = _metaState.Value;
    if (turnText != null) {
      if (meta.SessionActive && meta.CurrentTurnPlayerId > 0) turnText.text = $"Lượt: P{meta.CurrentTurnPlayerId}";
      else turnText.text = "Đang chờ người chơi";
    }

    TextMeshProUGUI[] moneyTxt = { p1Money, p2Money, p3Money, p4Money };
    for (int i = 0; i < moneyTxt.Length; i++) {
      if (moneyTxt[i] == null) continue;
      if (i < _playerStates.Count) {
        var data = _playerStates[i];
        moneyTxt[i].text = $"P{data.Id}: {data.Money}";
      } else {
        moneyTxt[i].text = string.Empty;
      }
    }
  }

  void ApplyPlayersToBoard(bool teleport) {
    if (players == null) return;

    HashSet<int> active = new HashSet<int>();
    foreach (var data in _playerStates) {
      int index = data.Id - 1;
      if (index < 0 || index >= players.Length) continue;
      var view = players[index];
      if (view == null) continue;

      active.Add(index);
      if (teleport || view.PlayerId != data.Id || view.NodeIndex != data.NodeIndex) {
        view.gameObject.SetActive(true);
        view.Init(data.Id, data.NodeIndex);
      }
    }

    for (int i = 0; i < players.Length; i++) {
      var view = players[i];
      if (view == null) continue;
      if (!active.Contains(i)) view.gameObject.SetActive(false);
    }
  }

  void UpdateLocalPlayerId() {
    if (!IsClient || NetworkManager.Singleton == null) {
      _localPlayerId = -1;
      return;
    }

    ulong localId = NetworkManager.Singleton.LocalClientId;
    _localPlayerId = -1;
    foreach (var slot in _playerSlots) {
      if (slot.IsConnected && slot.ClientId == localId) {
        _localPlayerId = slot.PlayerId;
        break;
      }
    }
  }

  bool IsLocalPlayersTurn() {
    int current = _metaState.Value.CurrentTurnPlayerId;
    if (current <= 0) return false;
    if (IsServer) return _serverGame != null && _serverGame.CurrentTurnPlayerId == current;
    return _localPlayerId > 0 && _localPlayerId == current;
  }

  public void OnRoll() {
    if (!IsClient) {
      Debug.LogWarning("OnRoll called without an active Netcode client.");
      return;
    }

    if (!_metaState.Value.SessionActive) {
      Debug.LogWarning("Cannot roll before the session is active.");
      return;
    }

    if (!IsServer && !IsLocalPlayersTurn()) {
      Debug.LogWarning("It is not your turn to roll.");
      return;
    }

    if (IsServer) {
      var manager = NetworkManager.Singleton;
      ulong requester = manager != null ? manager.LocalClientId : NetworkManager.ServerClientId;
      ExecuteServerRoll(requester);
    } else {
      RequestRollServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void RequestRollServerRpc(ServerRpcParams serverRpcParams = default) {
    ExecuteServerRoll(serverRpcParams.Receive.SenderClientId);
  }

  void ExecuteServerRoll(ulong requesterClientId) {
    if (!IsServer || _serverGame == null || _serverGame.Players.Count == 0) return;

    int currentPlayerId = _serverGame.CurrentTurnPlayerId;
    if (currentPlayerId <= 0) return;

    if (requesterClientId != NetworkManager.ServerClientId) {
      if (!_clientToPlayer.TryGetValue(requesterClientId, out int requesterPlayerId) || requesterPlayerId != currentPlayerId) {
        Debug.LogWarning($"Client {requesterClientId} attempted to roll out of turn.");
        return;
      }
    }

    EnsureRandomInitialized();
    int d1 = _serverRandom.NextInt(1, 7);
    int d2 = _serverRandom.NextInt(1, 7);
    var roll = new DiceRollData(d1, d2);
    _lastDiceRoll.Value = roll;
    StartCoroutine(ServerResolveTurn(currentPlayerId, roll));
  }

  IEnumerator ServerResolveTurn(int playerId, DiceRollData roll) {
    var player = _serverGame.Players.Find(x => x.Id == playerId);
    if (player == null) yield break;

    int steps = roll.Die1 + roll.Die2;
    if (players != null && playerId - 1 >= 0 && playerId - 1 < players.Length) {
      var view = players[playerId - 1];
      if (view != null) yield return StartCoroutine(view.MoveBySteps(steps));
    }

    MovePlayerClientRpc(playerId, steps);

    _turnSystem.MoveAndResolve(steps);
    int previousPlayerId = playerId;
    _turnSystem.EndTurn();
    int nextPlayerId = _serverGame.CurrentTurnPlayerId;
    if (_serverGame.Players.Count > 0) {
      int minId = _serverGame.Players.Min(x => x.Id);
      if (nextPlayerId == minId && previousPlayerId != minId) _round++;
    }

    var meta = _metaState.Value;
    meta.CurrentTurnPlayerId = nextPlayerId;
    meta.Round = Mathf.Max(1, _round);
    meta.SessionActive = _serverGame.Players.Count > 0;
    _metaState.Value = meta;

    SyncNetworkStateFromDomain();
  }

  [ClientRpc]
  void MovePlayerClientRpc(int playerId, int steps, ClientRpcParams rpcParams = default) {
    if (IsServer || players == null) return;
    int index = playerId - 1;
    if (index < 0 || index >= players.Length) return;
    var view = players[index];
    if (view == null) return;
    StartCoroutine(view.MoveBySteps(steps));
  }

  public void OnBuyCurrent() {
    if (!IsClient) return;
    if (!_metaState.Value.SessionActive) return;
    if (!IsServer && !IsLocalPlayersTurn()) return;

    if (IsServer) {
      ServerBuyCurrent(_serverGame.CurrentTurnPlayerId);
    } else {
      RequestBuyCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void RequestBuyCurrentServerRpc(ServerRpcParams serverRpcParams = default) {
    if (!IsServer) return;
    if (_clientToPlayer.TryGetValue(serverRpcParams.Receive.SenderClientId, out int playerId)) {
      ServerBuyCurrent(playerId);
    }
  }

  void ServerBuyCurrent(int playerId) {
    if (_serverGame == null || _serverGame.CurrentTurnPlayerId != playerId) return;
    var player = _serverGame.Players.Find(x => x.Id == playerId);
    if (player == null) return;
    if (!_serverGame.Properties.TryGetValue(player.NodeIndex, out var property)) return;
    if (!BoardRules.CanBuy(player, property)) return;

    BoardRules.Buy(player, property);
    SyncNetworkStateFromDomain();
    NotifyTransaction($"P{playerId} purchased tile {property.TileId}");
  }

  public void OnUpgradeHouseCurrent() {
    if (!IsClient) return;
    if (!_metaState.Value.SessionActive) return;
    if (!IsServer && !IsLocalPlayersTurn()) return;

    if (IsServer) {
      ServerUpgradeHouseCurrent(_serverGame.CurrentTurnPlayerId);
    } else {
      RequestUpgradeHouseCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void RequestUpgradeHouseCurrentServerRpc(ServerRpcParams serverRpcParams = default) {
    if (!IsServer) return;
    if (_clientToPlayer.TryGetValue(serverRpcParams.Receive.SenderClientId, out int playerId)) {
      ServerUpgradeHouseCurrent(playerId);
    }
  }

  void ServerUpgradeHouseCurrent(int playerId) {
    if (_serverGame == null || _serverGame.CurrentTurnPlayerId != playerId) return;
    var player = _serverGame.Players.Find(x => x.Id == playerId);
    if (player == null) return;
    if (!_serverGame.Properties.TryGetValue(player.NodeIndex, out var property)) return;
    if (!BoardRules.CanUpgradeHouse(player, property, _economy)) return;

    BoardRules.UpgradeHouse(player, property, _economy);
    SyncNetworkStateFromDomain();
    NotifyTransaction($"P{playerId} upgraded tile {property.TileId} to level {property.Level}");
  }

  public void OnUpgradeHotelCurrent() {
    if (!IsClient) return;
    if (!_metaState.Value.SessionActive) return;
    if (!IsServer && !IsLocalPlayersTurn()) return;

    if (IsServer) {
      ServerUpgradeHotelCurrent(_serverGame.CurrentTurnPlayerId);
    } else {
      RequestUpgradeHotelCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void RequestUpgradeHotelCurrentServerRpc(ServerRpcParams serverRpcParams = default) {
    if (!IsServer) return;
    if (_clientToPlayer.TryGetValue(serverRpcParams.Receive.SenderClientId, out int playerId)) {
      ServerUpgradeHotelCurrent(playerId);
    }
  }

  void ServerUpgradeHotelCurrent(int playerId) {
    if (_serverGame == null || _serverGame.CurrentTurnPlayerId != playerId) return;
    var player = _serverGame.Players.Find(x => x.Id == playerId);
    if (player == null) return;
    if (!_serverGame.Properties.TryGetValue(player.NodeIndex, out var property)) return;
    if (!BoardRules.CanUpgradeHotel(player, property, _economy)) return;

    BoardRules.UpgradeHotel(player, property, _economy);
    SyncNetworkStateFromDomain();
    NotifyTransaction($"P{playerId} built a hotel on tile {property.TileId}");
  }

  public void OnUpgradeHotelAt(int tileId) {
    if (!IsClient) return;
    if (!_metaState.Value.SessionActive) return;
    if (!IsServer && !IsLocalPlayersTurn()) return;

    if (IsServer) {
      ServerUpgradeHotelAt(_serverGame.CurrentTurnPlayerId, tileId);
    } else {
      RequestUpgradeHotelAtServerRpc(tileId);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void RequestUpgradeHotelAtServerRpc(int tileId, ServerRpcParams serverRpcParams = default) {
    if (!IsServer) return;
    if (_clientToPlayer.TryGetValue(serverRpcParams.Receive.SenderClientId, out int playerId)) {
      ServerUpgradeHotelAt(playerId, tileId);
    }
  }

  void ServerUpgradeHotelAt(int playerId, int tileId) {
    if (_serverGame == null) return;
    var player = _serverGame.Players.Find(x => x.Id == playerId);
    if (player == null) return;
    if (!_serverGame.Properties.TryGetValue(tileId, out var property)) return;
    if (property.Owner != (Owner)playerId) return;
    if (!BoardRules.CanUpgradeHotel(player, property, _economy)) return;

    BoardRules.UpgradeHotel(player, property, _economy);
    SyncNetworkStateFromDomain();
    NotifyTransaction($"P{playerId} upgraded tile {property.TileId} to a hotel");
  }

  public void OnTakeoverCurrent() {
    if (!IsClient) return;
    if (!_metaState.Value.SessionActive) return;
    if (!IsServer && !IsLocalPlayersTurn()) return;

    if (IsServer) {
      ServerTakeoverCurrent(_serverGame.CurrentTurnPlayerId);
    } else {
      RequestTakeoverCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void RequestTakeoverCurrentServerRpc(ServerRpcParams serverRpcParams = default) {
    if (!IsServer) return;
    if (_clientToPlayer.TryGetValue(serverRpcParams.Receive.SenderClientId, out int playerId)) {
      ServerTakeoverCurrent(playerId);
    }
  }

  void ServerTakeoverCurrent(int playerId) {
    if (_serverGame == null || _serverGame.CurrentTurnPlayerId != playerId) return;
    var buyer = _serverGame.Players.Find(x => x.Id == playerId);
    if (buyer == null) return;
    if (!_serverGame.Properties.TryGetValue(buyer.NodeIndex, out var property)) return;
    if (property.Owner == Owner.None || (int)property.Owner == playerId) return;
    var seller = _serverGame.Players.Find(x => x.Id == (int)property.Owner);
    if (seller == null) return;
    if (!BoardRules.CanTakeover(buyer, property, _economy)) return;

    BoardRules.BuyTakeover(buyer, seller, property, _economy);
    SyncNetworkStateFromDomain();
    NotifyTransaction($"P{playerId} took over tile {property.TileId}");
  }

  void NotifyTransaction(string message) {
    if (string.IsNullOrEmpty(message)) return;
    Debug.Log(message);
    NotifyTransactionClientRpc(message);
  }

  [ClientRpc]
  void NotifyTransactionClientRpc(string message, ClientRpcParams rpcParams = default) {
    if (IsServer) return;
    Debug.Log(message);
  }

  void EnsureRandomInitialized() {
    if (_randomInitialized) return;
    uint seed = (uint)(DateTime.UtcNow.Ticks & 0xFFFFFFFF);
    if (seed == 0) seed = 1u;
    if ((seed & 1u) == 0u) seed |= 1u;
    _serverRandom = new RandomGenerator(seed);
    _randomInitialized = true;
  }

  static bool HasValidRoll(in DiceRollData data) => data.Die1 > 0 && data.Die2 > 0;
}

public struct GameMetaState : INetworkSerializable {
  public int BoardLength;
  public int CurrentTurnPlayerId;
  public int PlayerCount;
  public int Round;
  public bool SessionActive;

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref BoardLength);
    serializer.SerializeValue(ref CurrentTurnPlayerId);
    serializer.SerializeValue(ref PlayerCount);
    serializer.SerializeValue(ref Round);
    serializer.SerializeValue(ref SessionActive);
  }
}

public struct PlayerStateData : INetworkSerializable {
  public int Id;
  public int Money;
  public int NodeIndex;
  public int JailTurns;
  public int Luck;
  public int Resistance;
  public int Intelligence;
  public int Health;
  public int Agility;

  public PlayerStateData(PlayerState state) {
    Id = state.Id;
    Money = state.Money;
    NodeIndex = state.NodeIndex;
    JailTurns = state.JailTurns;
    Luck = state.Luck;
    Resistance = state.Resistance;
    Intelligence = state.Intelligence;
    Health = state.Health;
    Agility = state.Agility;
  }

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref Id);
    serializer.SerializeValue(ref Money);
    serializer.SerializeValue(ref NodeIndex);
    serializer.SerializeValue(ref JailTurns);
    serializer.SerializeValue(ref Luck);
    serializer.SerializeValue(ref Resistance);
    serializer.SerializeValue(ref Intelligence);
    serializer.SerializeValue(ref Health);
    serializer.SerializeValue(ref Agility);
  }
}

public struct PropertyStateData : INetworkSerializable {
  public int TileId;
  public int OwnerId;
  public int Level;
  public bool HasHotel;
  public int BasePrice;

  public PropertyStateData(PropertyState state) {
    TileId = state.TileId;
    OwnerId = (int)state.Owner;
    Level = state.Level;
    HasHotel = state.HasHotel;
    BasePrice = state.BasePrice;
  }

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref TileId);
    serializer.SerializeValue(ref OwnerId);
    serializer.SerializeValue(ref Level);
    serializer.SerializeValue(ref HasHotel);
    serializer.SerializeValue(ref BasePrice);
  }
}

public struct PlayerSlotData : INetworkSerializable {
  public int PlayerId;
  public ulong ClientId;
  public bool IsConnected;

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref PlayerId);
    serializer.SerializeValue(ref ClientId);
    serializer.SerializeValue(ref IsConnected);
  }
}

public struct DiceRollData : INetworkSerializable {
  public int Die1;
  public int Die2;
  public bool IsDouble;

  public DiceRollData(int die1, int die2) {
    Die1 = die1;
    Die2 = die2;
    IsDouble = die1 == die2;
  }

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref Die1);
    serializer.SerializeValue(ref Die2);
    serializer.SerializeValue(ref IsDouble);
  }
}
