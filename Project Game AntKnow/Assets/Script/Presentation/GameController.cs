using System;
using System.Collections;
using System.Collections.Generic;
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

        }
        return (0, (int?)null);
      },
      baseSalary: 200,
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
  }

  public void OnUpgradeHouseCurrent() {
  }

  public void OnUpgradeHotelCurrent() {
  }

  public void OnUpgradeHotelAt(int tileId) {
  }

  public void OnTakeoverCurrent() {

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
