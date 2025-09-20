using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(NetworkObject))]
public class NetworkGameController : NetworkBehaviour {
  [Header("Data")]
  [SerializeField] BoardConfig board;
  [SerializeField] PropertyRuleSet propertyRules;
  [SerializeField] CardLibrary cardLibrary;

  [Header("Players")]
  [SerializeField] NetworkPlayerController[] playerControllers; // order must match player slots

  [Header("UI")]
  [SerializeField] TextMeshProUGUI turnText;
  [SerializeField] TextMeshProUGUI[] moneyTexts; // up to 4 entries

  GameState _state;
  TurnSystem _turn;
  PropertyEconomy _econ;
  CardRuleEngine _cardRules;
  CardDeckService _eventDeck;
  readonly Dictionary<int, CardDefinition> _cardLookup = new();

  NetworkList<PlayerData> _playersData;
  NetworkList<PropertyData> _propertiesData;
  NetworkVariable<int> _turnSeed;
  NetworkVariable<int> _currentTurn;

  readonly Dictionary<ulong, int> _clientToPlayerIndex = new();
  readonly HashSet<int> _reservedSlots = new();
  bool _isProcessing;

  public static event Action<int, int[], int[]> CardInventoryUpdated;
  public static event Action<int, int> QuizRequested;

  struct PlayerData : INetworkSerializable, IEquatable<PlayerData> {
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

    public bool Equals(PlayerData other) =>
      Id == other.Id &&
      Money == other.Money &&
      NodeIndex == other.NodeIndex &&
      JailTurns == other.JailTurns;

    public override bool Equals(object obj) => obj is PlayerData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, Money, NodeIndex, JailTurns);
  }

  struct PropertyData : INetworkSerializable, IEquatable<PropertyData> {
    public int TileId;
    public int Owner; // cast from Owner enum
    public int Level;
    public bool HasHotel;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
      serializer.SerializeValue(ref TileId);
      serializer.SerializeValue(ref Owner);
      serializer.SerializeValue(ref Level);
      serializer.SerializeValue(ref HasHotel);
    }

    public bool Equals(PropertyData other) =>
      TileId == other.TileId &&
      Owner == other.Owner &&
      Level == other.Level &&
      HasHotel == other.HasHotel;

    public override bool Equals(object obj) => obj is PropertyData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(TileId, Owner, Level, HasHotel);
  }

  void Awake() {
    _playersData = new NetworkList<PlayerData>();
    _propertiesData = new NetworkList<PropertyData>();
    _turnSeed = new NetworkVariable<int>(0);
    _currentTurn = new NetworkVariable<int>(1);
  }

  public override void OnNetworkSpawn() {
    base.OnNetworkSpawn();

    _playersData.OnListChanged += HandlePlayerListChanged;
    _propertiesData.OnListChanged += HandlePropertyListChanged;
    _currentTurn.OnValueChanged += HandleTurnChanged;

    if (IsServer) {
      if (NetworkManager.Singleton != null) {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
      }
      InitializeAuthoritativeGame();
      PushStateToNetwork();
    }

    RefreshUI();
  }

  public override void OnNetworkDespawn() {
    base.OnNetworkDespawn();
    _playersData.OnListChanged -= HandlePlayerListChanged;
    _propertiesData.OnListChanged -= HandlePropertyListChanged;
    _currentTurn.OnValueChanged -= HandleTurnChanged;
    if (IsServer && NetworkManager.Singleton != null) {
      NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
      NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }
  }

  void InitializeAuthoritativeGame() {
    if (board == null || board.tiles == null || board.tiles.Length == 0) {
      Debug.LogError("BoardConfig missing for NetworkGameController");
      return;
    }

    _econ = propertyRules != null ? propertyRules.ToEconomy() : new PropertyEconomy(
      new int[] { 100, 150, 200, 250, 300 },
      new int[] { 25, 50, 75, 100, 125, 150 },
      400, 250,
      new int[] { 150, 200, 300, 400, 500, 600 },
      false
    );

    _state = new GameState {
      BoardLength = board.tiles.Length,
      CurrentTurnPlayerId = 1
    };

    var cardDefs = new List<CardDefinition>();
    if (cardLibrary != null && cardLibrary.cards != null) {
      foreach (var asset in cardLibrary.cards) {
        if (asset == null) continue;
        var def = asset.ToDefinition();
        cardDefs.Add(def);
        _cardLookup[def.CardId] = def;
      }
    }
    _cardRules = cardDefs.Count > 0 ? new CardRuleEngine(cardDefs) : null;
    var deckIds = new List<int>();
    foreach (var def in cardDefs) deckIds.Add(def.CardId);
    _turnSeed.Value = UnityEngine.Random.Range(0, int.MaxValue);
    if (deckIds.Count > 0) _eventDeck = new CardDeckService(deckIds, _turnSeed.Value);

    var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
    for (int i = 0; i < playerControllers.Length; i++) {
      var controller = playerControllers[i];
      if (controller == null) continue;

      int playerId = i + 1;
      controller.ServerInit(playerId, 0);

      var playerState = new PlayerState {
        Id = playerId,
        Money = 1500,
        NodeIndex = 0,
        JailTurns = 0
      };
      _state.Players.Add(playerState);

      if (i < connectedClients.Count) {
        ulong clientId = connectedClients[i].ClientId;
        _clientToPlayerIndex[clientId] = i;
        _reservedSlots.Add(i);
        if (clientId != NetworkManager.ServerClientId) {
          controller.NetworkObject.ChangeOwnership(clientId);
        }
      }
    }

    for (int i = 0; i < board.tiles.Length; i++) {
      var tile = board.tiles[i];
      if (tile != null && tile.type == TileType.Property) {
        _state.Properties[i] = new PropertyState {
          TileId = i,
          BasePrice = tile.basePrice,
          Level = 0,
          HasHotel = false
        };
      }
    }

    _turn = new TurnSystem(
      _state,
      tileId => board.tiles[tileId].type,
      tileId => _state.Properties.ContainsKey(tileId) ? _state.Properties[tileId] : null,
      tileId => (board.tiles[tileId].amount, board.tiles[tileId].destNode >= 0 ? (int?)board.tiles[tileId].destNode : null),
      baseSalary: 200,
      econ: _econ,
      cardRules: _cardRules
    );
    _turn.StartTurn();
  }

  void HandlePlayerListChanged(NetworkListEvent<PlayerData> changeEvent) => RefreshUI();
  void HandlePropertyListChanged(NetworkListEvent<PropertyData> changeEvent) => RefreshUI();
  void HandleTurnChanged(int previous, int next) => RefreshUI();

  void RefreshUI() {
    if (turnText != null) {
      turnText.text = $"Lượt: P{_currentTurn.Value}";
    }

    if (moneyTexts == null) return;
    for (int i = 0; i < moneyTexts.Length; i++) {
      if (moneyTexts[i] == null) continue;
      if (i < _playersData.Count) {
        var p = _playersData[i];
        moneyTexts[i].text = $"P{p.Id}: {p.Money}";
      } else {
        moneyTexts[i].text = string.Empty;
      }
    }
  }

  void PushStateToNetwork() {
    if (!IsServer || _state == null) return;

    _playersData.Clear();
    foreach (var ps in _state.Players) {
      _playersData.Add(new PlayerData {
        Id = ps.Id,
        Money = ps.Money,
        NodeIndex = ps.NodeIndex,
        JailTurns = ps.JailTurns
      });
    }

    _propertiesData.Clear();
    foreach (var kv in _state.Properties) {
      var pr = kv.Value;
      _propertiesData.Add(new PropertyData {
        TileId = pr.TileId,
        Owner = (int)pr.Owner,
        Level = pr.Level,
        HasHotel = pr.HasHotel
      });
    }

    _currentTurn.Value = _state.CurrentTurnPlayerId;
  }

  public void RequestRoll() {
    if (!IsClient) return;
    RollServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  void RollServerRpc(ServerRpcParams rpcParams = default) {
    if (_isProcessing || _turn == null) return;

    ulong senderId = rpcParams.Receive.SenderClientId;
    int playerIndex = GetPlayerIndex(senderId);
    if (playerIndex < 0) return;

    var playerState = _state.Players[playerIndex];
    if (playerState.Id != _state.CurrentTurnPlayerId) return;

    _isProcessing = true;
    var (d1, d2, sum, isDouble) = _turn.Roll();
    DiceRolledClientRpc(d1, d2, sum, isDouble);
    StartCoroutine(HandleRollCoroutine(playerIndex, sum));
  }

  IEnumerator HandleRollCoroutine(int playerIndex, int steps) {
    var controller = playerControllers[playerIndex];
    if (controller != null) {
      yield return controller.ServerMoveBySteps(steps);
    }

    _turn.MoveAndResolve(steps);
    HandleLandingEvents(playerIndex);
    PushStateToNetwork();

    _turn.EndTurn();
    _turn.StartTurn();
    PushStateToNetwork();

    _isProcessing = false;
  }

  public void RequestBuy() {
    if (!IsClient) return;
    BuyServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  void BuyServerRpc(ServerRpcParams rpcParams = default) {
    if (_turn == null) return;
    ulong senderId = rpcParams.Receive.SenderClientId;
    int playerIndex = GetPlayerIndex(senderId);
    if (playerIndex < 0) return;

    var player = _state.Players[playerIndex];
    if (player.Id != _state.CurrentTurnPlayerId) return;
    if (!_state.Properties.ContainsKey(player.NodeIndex)) return;
    var property = _state.Properties[player.NodeIndex];
    if (!BoardRules.CanBuy(player, property)) return;

    BoardRules.Buy(player, property);
    PushStateToNetwork();
  }

  public void RequestUpgradeHouse() {
    if (!IsClient) return;
    UpgradeHouseServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  void UpgradeHouseServerRpc(ServerRpcParams rpcParams = default) {
    if (_turn == null) return;
    ulong senderId = rpcParams.Receive.SenderClientId;
    int playerIndex = GetPlayerIndex(senderId);
    if (playerIndex < 0) return;

    var player = _state.Players[playerIndex];
    if (player.Id != _state.CurrentTurnPlayerId) return;
    if (!_state.Properties.ContainsKey(player.NodeIndex)) return;
    var property = _state.Properties[player.NodeIndex];
    if (!BoardRules.CanUpgradeHouse(player, property, _econ)) return;

    BoardRules.UpgradeHouse(player, property, _econ);
    PushStateToNetwork();
  }

  public void RequestUpgradeHotel() {
    if (!IsClient) return;
    UpgradeHotelServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  void UpgradeHotelServerRpc(ServerRpcParams rpcParams = default) {
    if (_turn == null) return;
    ulong senderId = rpcParams.Receive.SenderClientId;
    int playerIndex = GetPlayerIndex(senderId);
    if (playerIndex < 0) return;

    var player = _state.Players[playerIndex];
    if (!_state.Properties.ContainsKey(player.NodeIndex)) return;
    var property = _state.Properties[player.NodeIndex];
    if (!BoardRules.CanUpgradeHotel(player, property, _econ)) return;

    BoardRules.UpgradeHotel(player, property, _econ);
    PushStateToNetwork();
  }

  public void RequestTakeover() {
    if (!IsClient) return;
    TakeoverServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  void TakeoverServerRpc(ServerRpcParams rpcParams = default) {
    if (_turn == null) return;
    ulong senderId = rpcParams.Receive.SenderClientId;
    int buyerIndex = GetPlayerIndex(senderId);
    if (buyerIndex < 0) return;

    var buyer = _state.Players[buyerIndex];
    if (!_state.Properties.ContainsKey(buyer.NodeIndex)) return;
    var property = _state.Properties[buyer.NodeIndex];
    if (property.Owner == Owner.None || (int)property.Owner == buyer.Id) return;
    var seller = _state.Players.Find(p => p.Id == (int)property.Owner);
    if (seller == null) return;
    if (!BoardRules.CanTakeover(buyer, property, _econ)) return;

    BoardRules.BuyTakeover(buyer, seller, property, _econ);
    PushStateToNetwork();
  }

  public void RequestUseCard(int cardId) {
    if (!IsClient) return;
    UseCardServerRpc(cardId);
  }

  [ServerRpc(RequireOwnership = false)]
  void UseCardServerRpc(int cardId, ServerRpcParams rpcParams = default) {
    if (_cardRules == null) return;
    if (!_cardLookup.TryGetValue(cardId, out var card)) return;
    ulong senderId = rpcParams.Receive.SenderClientId;
    int playerIndex = GetPlayerIndex(senderId);
    if (playerIndex < 0) return;
    var player = _state.Players[playerIndex];
    if (card.Type != CardType.Active || card.Trigger != CardTrigger.Manual) return;
    if (!player.ActiveCardIds.Contains(cardId)) return;

    _cardRules.ExecuteActiveCard(player, card);
    player.ActiveCardIds.Remove(cardId);
    _eventDeck?.Discard(cardId);
    PushStateToNetwork();
    var passive = player.PassiveCardIds.ToArray();
    var active = player.ActiveCardIds.ToArray();
    CardInventoryUpdated?.Invoke(player.Id, passive, active);
    SendCardInventoryClientRpc(player.Id, passive, active);
  }

  public void RequestDrawEventCard() {
    if (!IsClient) return;
    DrawEventCardServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  void DrawEventCardServerRpc(ServerRpcParams rpcParams = default) {
    if (_eventDeck == null) return;
    ulong senderId = rpcParams.Receive.SenderClientId;
    int playerIndex = GetPlayerIndex(senderId);
    if (playerIndex < 0) return;
    var player = _state.Players[playerIndex];
    DrawEventCardServerSide(player);
  }

  [ClientRpc]
  void DiceRolledClientRpc(int d1, int d2, int sum, bool isDouble) {
    // Hook for playing dice animation/sfx on clients
    Debug.Log($"Dice rolled: {d1} + {d2} = {sum} (Double: {isDouble})");
  }

  [ClientRpc]
  void CardDrawnClientRpc(int playerId, int cardId) {
    Debug.Log($"Player {playerId} drew card {cardId}");
  }

  [ClientRpc]
  void SendCardInventoryClientRpc(int playerId, int[] passiveCards, int[] activeCards) {
    CardInventoryUpdated?.Invoke(playerId, passiveCards, activeCards);
  }

  [ClientRpc]
  void RequestQuizClientRpc(int playerId, int tileId) {
    QuizRequested?.Invoke(playerId, tileId);
  }

  int GetPlayerIndex(ulong clientId) {
    if (_clientToPlayerIndex.TryGetValue(clientId, out var index)) return index;
    return -1;
  }

  void HandleClientConnected(ulong clientId) {
    if (!IsServer) return;
    if (_clientToPlayerIndex.ContainsKey(clientId)) return;
    for (int i = 0; i < playerControllers.Length; i++) {
      if (_reservedSlots.Contains(i)) continue;
      var controller = playerControllers[i];
      if (controller == null) continue;
      _clientToPlayerIndex[clientId] = i;
      _reservedSlots.Add(i);
      controller.NetworkObject.ChangeOwnership(clientId);
      break;
    }
  }

  void HandleClientDisconnected(ulong clientId) {
    if (!IsServer) return;
    if (_clientToPlayerIndex.TryGetValue(clientId, out var index)) {
      _clientToPlayerIndex.Remove(clientId);
      _reservedSlots.Remove(index);
      var controller = playerControllers[index];
      if (controller != null) {
        controller.NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
      }
    }
  }

  void HandleLandingEvents(int playerIndex) {
    if (board == null || board.tiles == null) return;
    var player = _state.Players[playerIndex];
    var tile = board.tiles[player.NodeIndex];
    switch (tile.type) {
      case TileType.Chance:
      case TileType.Bonus:
      case TileType.Accident:
        DrawEventCardServerSide(player);
        break;
      case TileType.Quiz:
        QuizRequested?.Invoke(player.Id, player.NodeIndex);
        RequestQuizClientRpc(player.Id, player.NodeIndex);
        break;
    }
  }

  void DrawEventCardServerSide(PlayerState player) {
    if (_eventDeck == null) return;
    int cardId = _eventDeck.Draw();
    if (cardId < 0) return;
    if (!_cardLookup.TryGetValue(cardId, out var card)) return;
    if (card.Type == CardType.Passive) {
      if (!player.PassiveCardIds.Contains(cardId)) player.PassiveCardIds.Add(cardId);
    } else {
      player.ActiveCardIds.Add(cardId);
    }
    PushStateToNetwork();
    var passive = player.PassiveCardIds.ToArray();
    var active = player.ActiveCardIds.ToArray();
    CardInventoryUpdated?.Invoke(player.Id, passive, active);
    SendCardInventoryClientRpc(player.Id, passive, active);
    CardDrawnClientRpc(player.Id, cardId);
  }
}
