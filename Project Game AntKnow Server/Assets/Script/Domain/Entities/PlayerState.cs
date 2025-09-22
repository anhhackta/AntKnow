using System.Collections.Generic;

public sealed class PlayerState {
  public int Id;                 // 1..4
  public int Money;
  public int NodeIndex;          // 0..BoardLength-1
  public int JailTurns;          // 0 if free
  public int Luck, Resistance, Intelligence, Health, Agility;
  public readonly List<int> Owned = new();
  public readonly List<int> PassiveCardIds = new();
  public readonly List<int> ActiveCardIds = new();
  public readonly Dictionary<int, int> PassiveCooldown = new(); // cardId -> turns remaining
}

