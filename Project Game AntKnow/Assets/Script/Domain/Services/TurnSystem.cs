using System;

public sealed class TurnSystem {
  readonly GameState _g;
  readonly Func<int, TileType> _tileType;
  readonly Func<int, PropertyState> _prop;
  readonly Func<int, (int amount, int? destNode)> _tileParam;
  readonly int _baseSalary;
  readonly PropertyEconomy _econ;

  public TurnSystem(GameState g,
                    Func<int,TileType> tileType,
                    Func<int,PropertyState> prop,
                    Func<int,(int amount,int? destNode)> tileParam,
                    int baseSalary = 200,
                    PropertyEconomy econ = null) {
    _g=g; _tileType=tileType; _prop=prop; _tileParam=tileParam; _baseSalary=baseSalary; _econ = econ ?? new PropertyEconomy(
      new int[]{100,150,200,250,300},
      new int[]{25,50,75,100,125,150},
      400, 250,
      new int[]{150,200,300,400,500,600},
      false
    );
  }

  public (int,int,int,bool) Roll() => DiceRng.Roll2();

  public void MoveAndResolve(int steps) {
    var p = _g.Players.Find(x=>x.Id==_g.CurrentTurnPlayerId);
    int prev = p.NodeIndex, next = (prev+steps)%_g.BoardLength;
    if (prev+steps >= _g.BoardLength) BoardRules.OnPassStart(p, _baseSalary);
    p.NodeIndex = next;
    ResolveTile(p, next);
  }

  void ResolveTile(PlayerState p, int tileId) {
    var tp = _tileType(tileId);
    var pr = (tp==TileType.Property) ? _prop(tileId) : null;
    var param = _tileParam(tileId);

    switch(tp) {
      case TileType.Property:
        if (pr.Owner == Owner.None) { /* wait for UI buy */ }
        else if ((int)pr.Owner != p.Id) {
          var owner = _g.Players.Find(x=>x.Id==(int)pr.Owner);
          var rent  = BoardRules.CalcRent(pr, owner, _econ);
          BoardRules.PayRent(p, owner, rent);
        } else {
          // Optional: auto-upgrade to hotel when entering own tile and level==5
          if (pr.Level == 5 && !pr.HasHotel && BoardRules.CanUpgradeHotel(p, pr, _econ)) {
            // UI can call this explicitly; here just a placeholder for future behavior
          }
        }
        break;
      case TileType.Tax:   BoardRules.OnTax(p,  Math.Abs(param.amount)); break;
      case TileType.Bonus: BoardRules.OnBonus(p, Math.Abs(param.amount)); break;
      case TileType.Accident: p.JailTurns = 3; break;
      case TileType.Quiz: /* handled by UI */ break;
      case TileType.Travel:
        if (param.destNode.HasValue) p.NodeIndex = param.destNode.Value; // UI choice in future
        break;
      case TileType.GoToJail: p.JailTurns = 2; p.NodeIndex = param.destNode ?? 24; break;
      case TileType.Jail: /* visit */ break;
      case TileType.FreeParking:
      case TileType.Start: /* no-op */ break;
    }
  }

  public void EndTurn() {
    var ids = _g.Players.ConvertAll(x=>x.Id);
    int i = ids.IndexOf(_g.CurrentTurnPlayerId);
    _g.CurrentTurnPlayerId = ids[(i+1)%ids.Count];
  }
}
