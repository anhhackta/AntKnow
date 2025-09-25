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

  struct MetaState : INetworkSerializable, IEquatable<MetaState> {
    public bool SessionActive;
    public int CurrentTurnPlayerId;
    public int Round;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
      serializer.SerializeValue(ref SessionActive);
      serializer.SerializeValue(ref CurrentTurnPlayerId);
      serializer.SerializeValue(ref Round);
    }

    public bool Equals(MetaState other) =>
      SessionActive == other.SessionActive &&
      CurrentTurnPlayerId == other.CurrentTurnPlayerId &&
      Round == other.Round;

    public override bool Equals(object obj) => obj is MetaState other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(SessionActive, CurrentTurnPlayerId, Round);
  }

  struct PlayerStateData : INetworkSerializable, IEquatable<PlayerStateData> {
    public int Id;
    public int Money;
    public int NodeIndex;
    public int JailTurns;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
      serializer.SerializeValue(ref Id);
      serializer.SerializeValue(ref Money);
      serializer.SerializeValue(ref NodeIndex);
      serializer.SerializeValue(ref JailTurns);
    }

    public bool Equals(PlayerStateData other) =>
      Id == other.Id &&
      Money == other.Money &&
      NodeIndex == other.NodeIndex &&
      JailTurns == other.JailTurns;

    public override bool Equals(object obj) => obj is PlayerStateData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, Money, NodeIndex, JailTurns);
  }

  struct PlayerSlotData : INetworkSerializable, IEquatable<PlayerSlotData> {
    public int PlayerId;
    public bool IsConnected;
    public ulong ClientId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
      serializer.SerializeValue(ref PlayerId);
      serializer.SerializeValue(ref IsConnected);
      serializer.SerializeValue(ref ClientId);
    }

    public bool Equals(PlayerSlotData other) =>
      PlayerId == other.PlayerId &&
      IsConnected == other.IsConnected &&
      ClientId == other.ClientId;

    public override bool Equals(object obj) => obj is PlayerSlotData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(PlayerId, IsConnected, ClientId);
  }

  readonly NetworkVariable<MetaState> _metaState = new NetworkVariable<MetaState>(new MetaState {
    SessionActive = false,
    CurrentTurnPlayerId = 0,
    Round = 1
  });
  readonly NetworkList<PlayerStateData> _playerStates = new NetworkList<PlayerStateData>();
  readonly NetworkList<PlayerSlotData> _playerSlots = new NetworkList<PlayerSlotData>();
  readonly NetworkVariable<DiceRollData> _lastDiceRoll = new NetworkVariable<DiceRollData>(new DiceRollData {
    Die1 = 0,
    Die2 = 0,
    IsDouble = false
  });

  readonly Dictionary<ulong, int> _clientToPlayer = new Dictionary<ulong, int>();
  PropertyEconomy _economy;
  CardRuleEngine _cardRules;
  CardDeckService _eventDeck;
  GameState _serverGame;
  TurnSystem _turnSystem;
  RandomGenerator _serverRandom;
  bool _randomInitialized;
  int _round = 1;
  int _localPlayerId = -1;

  public override void OnNetworkSpawn() {
    base.OnNetworkSpawn();

    _playerStates.OnListChanged += HandlePlayerStatesChanged;
    _playerSlots.OnListChanged += HandlePlayerSlotsChanged;
    _metaState.OnValueChanged += HandleMetaChanged;
    _lastDiceRoll.OnValueChanged += HandleDiceRollChanged;

    if (IsServer) {
      InitializeServerState();
      SyncNetworkStateFromDomain();
      var meta = _metaState.Value;
      meta.CurrentTurnPlayerId = _serverGame != null ? _serverGame.CurrentTurnPlayerId : 0;
      meta.Round = Mathf.Max(1, _round);
      meta.SessionActive = _serverGame != null && _serverGame.Players.Count > 0;
      _metaState.Value = meta;
    } else {
      UpdateLocalPlayerId();
    }

    RefreshUIFromNetwork();
    ApplyPlayersToBoard(true);
  }

  public override void OnNetworkDespawn() {
    base.OnNetworkDespawn();
    _playerStates.OnListChanged -= HandlePlayerStatesChanged;
    _playerSlots.OnListChanged -= HandlePlayerSlotsChanged;
    _metaState.OnValueChanged -= HandleMetaChanged;
    _lastDiceRoll.OnValueChanged -= HandleDiceRollChanged;
  }

  void InitializeServerState() {
    if (!IsServer) return;

    _clientToPlayer.Clear();
    _playerStates.Clear();
    _playerSlots.Clear();
    _round = 1;
    _randomInitialized = false;

    if (board == null || board.tiles == null || board.tiles.Length == 0) {
      Debug.LogWarning("GameController missing BoardConfig or tiles.");
      _serverGame = null;
      return;
    }

    _economy = propertyRules != null ? propertyRules.ToEconomy() : new PropertyEconomy(
      new int[]{100,150,200,250,300},
      new int[]{25,50,75,100,125,150},
      400, 250,
      new int[]{150,200,300,400,500,600},
      false
    );

    BuildCardSystems();

    _serverGame = new GameState {
      BoardLength = board.tiles.Length,
      CurrentTurnPlayerId = 1
    };

    for (int i = 0; i < board.tiles.Length; i++) {
      var tile = board.tiles[i];
      if (tile != null && tile.type == TileType.Property) {
        _serverGame.Properties[i] = new PropertyState {
          TileId = i,
          Owner = Owner.None,
          Level = 0,
          HasHotel = false,
          BasePrice = tile.basePrice
        };
      }
    }

    int playerCount = players != null && players.Length > 0 ? players.Length : 4;
    ulong serverClientId = NetworkManager != null ? NetworkManager.ServerClientId : 0UL;

    for (int i = 0; i < playerCount; i++) {
      int playerId = i + 1;
      var playerState = new PlayerState {
        Id = playerId,
        Money = 1500,
        NodeIndex = 0,
        JailTurns = 0
      };
      _serverGame.Players.Add(playerState);
      _playerStates.Add(new PlayerStateData {
        Id = playerId,
        Money = playerState.Money,
        NodeIndex = playerState.NodeIndex,
        JailTurns = playerState.JailTurns
      });

      bool isHostSlot = i == 0;
      ulong clientId = isHostSlot ? serverClientId : 0UL;
      if (isHostSlot) _clientToPlayer[clientId] = playerId;
      _playerSlots.Add(new PlayerSlotData {
        PlayerId = playerId,
        IsConnected = isHostSlot,
        ClientId = clientId
      });

      if (players != null && i < players.Length && players[i] != null) {
        players[i].gameObject.SetActive(true);
        players[i].Init(playerId, 0);
      }
    }

    if (_serverGame.Players.Count > 0) {
      _serverGame.CurrentTurnPlayerId = _serverGame.Players[0].Id;
    }

    _turnSystem = new TurnSystem(
      _serverGame,
      tileId => TryGetTileType(tileId),
      tileId => TryGetPropertyState(tileId, out var prop) ? prop : null,
      tileId => GetTileParameters(tileId),
      baseSalary: 200,
      econ: _economy,
      cardRules: _cardRules
    );
  }

  void BuildCardSystems() {
    var cardDefs = new List<CardDefinition>();
    if (cardLibrary != null && cardLibrary.cards != null) {
      foreach (var asset in cardLibrary.cards) {
        if (asset == null) continue;
        var def = asset.ToDefinition();
        cardDefs.Add(def);
      }
    }

    _cardRules = cardDefs.Count > 0 ? new CardRuleEngine(cardDefs) : null;
    _eventDeck = cardDefs.Count > 0 ? new CardDeckService(cardDefs.ConvertAll(c => c.CardId), UnityEngine.Random.Range(0, int.MaxValue)) : null;
  }

  TileType TryGetTileType(int tileId) {
    if (board == null || board.tiles == null) return TileType.Start;
    if (tileId < 0 || tileId >= board.tiles.Length) return TileType.Start;
    var tile = board.tiles[tileId];
    return tile != null ? tile.type : TileType.Start;
  }

  (int amount, int? destNode) GetTileParameters(int tileId) {
    if (board == null || board.tiles == null || tileId < 0 || tileId >= board.tiles.Length) {
      return (0, null);
    }
    var tile = board.tiles[tileId];
    if (tile == null) return (0, null);
    int? dest = tile.destNode >= 0 ? tile.destNode : (int?)null;
    return (tile.amount, dest);
  }

  bool TryGetPropertyState(int tileId, out PropertyState property) {
    if (_serverGame != null && _serverGame.Properties.TryGetValue(tileId, out property)) return true;
    property = null;
    return false;
  }

  void SyncNetworkStateFromDomain() {
    if (!IsServer || _serverGame == null) return;

    _playerStates.Clear();
    foreach (var player in _serverGame.Players) {
      _playerStates.Add(new PlayerStateData {
        Id = player.Id,
        Money = player.Money,
        NodeIndex = player.NodeIndex,
        JailTurns = player.JailTurns
      });
    }

    var meta = _metaState.Value;
    meta.SessionActive = _serverGame.Players.Count > 0;
    meta.CurrentTurnPlayerId = _serverGame.CurrentTurnPlayerId;
    meta.Round = Mathf.Max(1, _round);
    _metaState.Value = meta;

    RefreshUIFromNetwork();
    ApplyPlayersToBoard(true);
  }

  void HandlePlayerStatesChanged(NetworkListEvent<PlayerStateData> change) {
    RefreshUIFromNetwork();
    ApplyPlayersToBoard(false);
  }

  void HandlePlayerSlotsChanged(NetworkListEvent<PlayerSlotData> change) {
    UpdateLocalPlayerId();
  }

  void HandleMetaChanged(MetaState previous, MetaState next) {
    RefreshUIFromNetwork();
  }

  void HandleDiceRollChanged(DiceRollData previous, DiceRollData next) {
    if (diceView != null && (next.Die1 != 0 || next.Die2 != 0)) {
      diceView.ShowRoll(next.Die1, next.Die2);
    }
  }

  void EnsureRandomInitialized() {
    if (_randomInitialized) return;
    uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
    _serverRandom = new RandomGenerator(seed);
    _randomInitialized = true;
  }

  bool TryResolvePlayerForRequest(ulong requesterClientId, out PlayerState player) {
    player = null;
    if (_serverGame == null) return false;

    int currentId = _serverGame.CurrentTurnPlayerId;
    if (currentId <= 0) return false;

    if (requesterClientId != (NetworkManager != null ? NetworkManager.ServerClientId : 0UL)) {
      if (!_clientToPlayer.TryGetValue(requesterClientId, out int mapped) || mapped != currentId) {
        return false;
      }
    }

    player = _serverGame.Players.Find(p => p.Id == currentId);
    return player != null;
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
    var (d1, d2) = _turnSystem != null
      ? _turnSystem.Roll((min, max) => _serverRandom.NextInt(min, max))
      : (_serverRandom.NextInt(1, 7), _serverRandom.NextInt(1, 7));
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
    if (IsServer) {
      var manager = NetworkManager.Singleton;
      ulong requester = manager != null ? manager.LocalClientId : NetworkManager.ServerClientId;
      ExecuteBuyCurrent(requester);
    } else {
      BuyCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void BuyCurrentServerRpc(ServerRpcParams rpcParams = default) {
    ExecuteBuyCurrent(rpcParams.Receive.SenderClientId);
  }

  void ExecuteBuyCurrent(ulong requesterClientId) {
    if (!IsServer || _serverGame == null) return;
    if (!TryResolvePlayerForRequest(requesterClientId, out var player)) return;
    if (!_serverGame.Properties.TryGetValue(player.NodeIndex, out var property)) return;
    if (!BoardRules.CanBuy(player, property)) return;

    BoardRules.Buy(player, property);
    SyncNetworkStateFromDomain();
  }

  public void OnUpgradeHouseCurrent() {
    if (!IsClient) return;
    if (IsServer) {
      var manager = NetworkManager.Singleton;
      ulong requester = manager != null ? manager.LocalClientId : NetworkManager.ServerClientId;
      ExecuteUpgradeHouse(requester);
    } else {
      UpgradeHouseServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void UpgradeHouseServerRpc(ServerRpcParams rpcParams = default) {
    ExecuteUpgradeHouse(rpcParams.Receive.SenderClientId);
  }

  void ExecuteUpgradeHouse(ulong requesterClientId) {
    if (!IsServer || _serverGame == null) return;
    if (!TryResolvePlayerForRequest(requesterClientId, out var player)) return;
    if (!_serverGame.Properties.TryGetValue(player.NodeIndex, out var property)) return;
    if (!BoardRules.CanUpgradeHouse(player, property, _economy)) return;

    BoardRules.UpgradeHouse(player, property, _economy);
    SyncNetworkStateFromDomain();
  }

  public void OnUpgradeHotelCurrent() {
    if (!IsClient) return;
    if (IsServer) {
      var manager = NetworkManager.Singleton;
      ulong requester = manager != null ? manager.LocalClientId : NetworkManager.ServerClientId;
      ExecuteUpgradeHotel(requester, -1);
    } else {
      UpgradeHotelCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void UpgradeHotelCurrentServerRpc(ServerRpcParams rpcParams = default) {
    ExecuteUpgradeHotel(rpcParams.Receive.SenderClientId, -1);
  }

  public void OnUpgradeHotelAt(int tileId) {
    if (!IsClient) return;
    if (IsServer) {
      var manager = NetworkManager.Singleton;
      ulong requester = manager != null ? manager.LocalClientId : NetworkManager.ServerClientId;
      ExecuteUpgradeHotel(requester, tileId);
    } else {
      UpgradeHotelAtServerRpc(tileId);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void UpgradeHotelAtServerRpc(int tileId, ServerRpcParams rpcParams = default) {
    ExecuteUpgradeHotel(rpcParams.Receive.SenderClientId, tileId);
  }

  void ExecuteUpgradeHotel(ulong requesterClientId, int tileIdOverride) {
    if (!IsServer || _serverGame == null) return;
    if (!TryResolvePlayerForRequest(requesterClientId, out var player)) return;
    int tileId = tileIdOverride >= 0 ? tileIdOverride : player.NodeIndex;
    if (!_serverGame.Properties.TryGetValue(tileId, out var property)) return;
    if (!BoardRules.CanUpgradeHotel(player, property, _economy)) return;

    BoardRules.UpgradeHotel(player, property, _economy);
    SyncNetworkStateFromDomain();
  }

  public void OnTakeoverCurrent() {
    if (!IsClient) return;
    if (IsServer) {
      var manager = NetworkManager.Singleton;
      ulong requester = manager != null ? manager.LocalClientId : NetworkManager.ServerClientId;
      ExecuteTakeover(requester);
    } else {
      TakeoverCurrentServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  void TakeoverCurrentServerRpc(ServerRpcParams rpcParams = default) {
    ExecuteTakeover(rpcParams.Receive.SenderClientId);
  }

  void ExecuteTakeover(ulong requesterClientId) {
    if (!IsServer || _serverGame == null) return;
    if (!TryResolvePlayerForRequest(requesterClientId, out var buyer)) return;
    if (!_serverGame.Properties.TryGetValue(buyer.NodeIndex, out var property)) return;
    if (!BoardRules.CanTakeover(buyer, property, _economy)) return;

    int sellerId = (int)property.Owner;
    var seller = _serverGame.Players.Find(x => x.Id == sellerId);
    if (seller == null) return;

    BoardRules.BuyTakeover(buyer, seller, property, _economy);
    SyncNetworkStateFromDomain();
  }

  bool TryGetTile(int tileId, out TileDef tile) {
    var tiles = board?.tiles;
    if (tiles != null && tileId >= 0 && tileId < tiles.Length) {
      tile = tiles[tileId];
      return tile != null;
    }

    tile = null;
    return false;
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
