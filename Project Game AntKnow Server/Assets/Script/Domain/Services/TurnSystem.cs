using System;

public sealed class TurnSystem {
  readonly GameState _g;
  readonly int _baseSalary;
  readonly CardRuleEngine _cardRules;

  public TurnSystem(GameState g,
                    int baseSalary = 200,
                    CardRuleEngine cardRules = null) {
    _g = g;
    _baseSalary = baseSalary;
    _cardRules = cardRules;
  }

  public void MoveAndResolve(int steps) {
    var p = _g.Players.Find(x=>x.Id==_g.CurrentTurnPlayerId);
    int prev = p.NodeIndex, next = (prev+steps)%_g.BoardLength;
    if (prev+steps >= _g.BoardLength) BoardRules.OnPassStart(p, _baseSalary);
    p.NodeIndex = next;
    ResolveTile(p, next);
  }

  void ResolveTile(PlayerState p, int waypointIndex) {
    // Get tile data from SimpleBoardConfig
    var tileData = SimpleBoardConfig.GetTileByWaypointIndex(waypointIndex);
    if (tileData == null) return;

    // Get property state if this is a property tile
    PropertyState pr = null;
    if (tileData.type == TileType.Property) {
      pr = _g.Properties.ContainsKey(tileData.index) ? _g.Properties[tileData.index] : null;
    }

    switch(tileData.type) {
      case TileType.Property:
        if (pr == null) break; // Should not happen
        if (pr.Owner == Owner.None) {
          /* wait for UI buy - client will call BuyServerRpc */
        }
        else if ((int)pr.Owner != p.Id) {
          var owner = _g.Players.Find(x=>x.Id==(int)pr.Owner);
          var rent = BoardRules.CalcRent(tileData, pr, owner);
          BoardRules.PayRent(p, owner, rent);
        } else {
          // Player landed on own property - no action needed
        }
        break;

      case TileType.Chance:
        /* Event card - handled by UI/ServerRpc */
        break;

      case TileType.Quiz:
        /* Quiz - handled by UI/ServerRpc */
        break;

      case TileType.Jail:
        // Ô Tai Nạn (Accident) - Bị giam 3 turns
        p.JailTurns = 3;
        break;

      case TileType.Travel:
        /* Du Lịch - handled by UI/ServerRpc (player chooses destination) */
        break;

      case TileType.Tax:
        // Not used in map 36
        break;

      case TileType.Bonus:
        // Not used in map 36
        break;

      case TileType.GoToJail:
        // Not used in map 36
        break;

      case TileType.Accident:
        // Not used in map 36 (we use Jail instead)
        break;

      case TileType.FreeParking:
      case TileType.Start:
        /* no-op */
        break;
    }
  }

  public void EndTurn() {
    var ids = _g.Players.ConvertAll(x=>x.Id);
    int i = ids.IndexOf(_g.CurrentTurnPlayerId);
    _g.CurrentTurnPlayerId = ids[(i+1)%ids.Count];
  }
}
