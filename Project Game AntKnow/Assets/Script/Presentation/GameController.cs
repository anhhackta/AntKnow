using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour {
  [Header("Data")]
  [SerializeField] BoardConfig board;
  [SerializeField] PropertyRuleSet propertyRules;
  [Header("Refs")]
  [SerializeField] PlayerController[] players;   // 2..4
  [SerializeField] TextMeshProUGUI turnText;
  [SerializeField] TextMeshProUGUI p1Money;
  [SerializeField] TextMeshProUGUI p2Money;
  [SerializeField] TextMeshProUGUI p3Money;
  [SerializeField] TextMeshProUGUI p4Money;

  GameState _g; TurnSystem _turn; PropertyEconomy _econ;

  void Start() {
    if (board == null || board.tiles == null || board.tiles.Length == 0) {
      Debug.LogWarning("BoardConfig is missing or empty. Create a BoardConfig asset and assign it.");
    }

    // 1) init GameState
    _g = new GameState { BoardLength = board != null && board.tiles != null ? board.tiles.Length : 32, CurrentTurnPlayerId = 1 };
    int n = players != null ? players.Length : 0;
    for (int i = 0; i < n; i++) {
      var ps = new PlayerState { Id = i + 1, Money = 1500, NodeIndex = 0 };
      _g.Players.Add(ps);
      if (players[i] != null) players[i].Init(ps.Id, 0);
    }
    // 2) property states
    if (board != null && board.tiles != null) {
      for (int i = 0; i < board.tiles.Length; i++) {
        if (board.tiles[i] != null && board.tiles[i].type == TileType.Property) {
          _g.Properties[i] = new PropertyState { TileId = i, BasePrice = board.tiles[i].basePrice };
        }
      }
    }
    // 3) Economy + TurnSystem with SO queries
    _econ = propertyRules != null ? propertyRules.ToEconomy() : new PropertyEconomy(
      new int[]{100,150,200,250,300},
      new int[]{25,50,75,100,125,150},
      400, 250,
      new int[]{150,200,300,400,500,600},
      false
    );
    _turn = new TurnSystem(
      _g,
      tileId => board != null && board.tiles != null && board.tiles[tileId] != null ? board.tiles[tileId].type : TileType.Start,
      tileId => _g.Properties.ContainsKey(tileId) ? _g.Properties[tileId] : null,
      tileId => {
        if (board != null && board.tiles != null && board.tiles[tileId] != null) {
          int amount = board.tiles[tileId].amount;
          int dest = board.tiles[tileId].destNode;
          return (amount, dest >= 0 ? (int?)dest : null);
        }
        return (0, null);
      },
      baseSalary: 200,
      econ: _econ
    );
    RefreshUI();
  }

  public void OnRoll() {
    var (d1, d2, sum, isDouble) = _turn.Roll();
    StartCoroutine(DoRoll(sum));
  }

  IEnumerator DoRoll(int sum) {
    var cur = _g.Players.Find(x => x.Id == _g.CurrentTurnPlayerId);
    var view = players[cur.Id - 1];
    if (view != null) yield return StartCoroutine(view.MoveBySteps(sum));
    _turn.MoveAndResolve(sum);
    RefreshUI();
    _turn.EndTurn();
    RefreshUI();
  }

  void RefreshUI() {
    if (turnText != null) turnText.text = $"Lượt: P{_g.CurrentTurnPlayerId}";
    TextMeshProUGUI[] moneyTxt = { p1Money, p2Money, p3Money, p4Money };
    for (int i = 0; i < moneyTxt.Length; i++) {
      if (moneyTxt[i] == null) continue;
      if (i < _g.Players.Count) moneyTxt[i].text = $"P{i + 1}: {_g.Players[i].Money}";
      else moneyTxt[i].text = string.Empty;
    }
  }

  // UI hook: buy when standing on an unowned property
  public void OnBuyCurrent() {
    var p = _g.Players.Find(x => x.Id == _g.CurrentTurnPlayerId);
    if (!_g.Properties.ContainsKey(p.NodeIndex)) return;
    var pr = _g.Properties[p.NodeIndex];
    if (BoardRules.CanBuy(p, pr)) BoardRules.Buy(p, pr);
    RefreshUI();
  }

  // UI hook: upgrade house at current tile (levels 0..4 -> 1..5)
  public void OnUpgradeHouseCurrent() {
    var p = _g.Players.Find(x => x.Id == _g.CurrentTurnPlayerId);
    if (!_g.Properties.ContainsKey(p.NodeIndex)) return;
    var pr = _g.Properties[p.NodeIndex];
    if (BoardRules.CanUpgradeHouse(p, pr, _econ)) BoardRules.UpgradeHouse(p, pr, _econ);
    RefreshUI();
  }

  // UI hook: upgrade to hotel if level==5
  public void OnUpgradeHotelCurrent() {
    var p = _g.Players.Find(x => x.Id == _g.CurrentTurnPlayerId);
    if (!_g.Properties.ContainsKey(p.NodeIndex)) return;
    var pr = _g.Properties[p.NodeIndex];
    if (BoardRules.CanUpgradeHotel(p, pr, _econ)) BoardRules.UpgradeHotel(p, pr, _econ);
    RefreshUI();
  }

  // UI hook: upgrade to hotel for a chosen owned tile when at Start (policy-based)
  public void OnUpgradeHotelAt(int tileId) {
    var p = _g.Players.Find(x => x.Id == _g.CurrentTurnPlayerId);
    if (!_g.Properties.ContainsKey(tileId)) return;
    var pr = _g.Properties[tileId];
    if (BoardRules.CanUpgradeHotel(p, pr, _econ)) BoardRules.UpgradeHotel(p, pr, _econ);
    RefreshUI();
  }

  // UI hook: takeover property from another player if allowed and affordable
  public void OnTakeoverCurrent() {
    var buyer = _g.Players.Find(x => x.Id == _g.CurrentTurnPlayerId);
    if (!_g.Properties.ContainsKey(buyer.NodeIndex)) return;
    var pr = _g.Properties[buyer.NodeIndex];
    if (pr.Owner == Owner.None || (int)pr.Owner == buyer.Id) return;
    var seller = _g.Players.Find(x => x.Id == (int)pr.Owner);
    if (BoardRules.CanTakeover(buyer, pr, _econ)) BoardRules.BuyTakeover(buyer, seller, pr, _econ);
    RefreshUI();
  }
}
