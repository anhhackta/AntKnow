using System.Collections.Generic;

public sealed class GameState {
  public int CurrentTurnPlayerId;
  public int BoardLength;
  public readonly List<PlayerState> Players = new();
  public readonly Dictionary<int, PropertyState> Properties = new();
}

