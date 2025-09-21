using System.Collections.Generic;

public sealed class CardDefinition {
  public int CardId;
  public string Name;
  public CardType Type;
  public CardTrigger Trigger;
  public string Description;
  public int Cost;
  public Dictionary<string, int> StatModifiers = new();
  public Dictionary<string, int> ResourceModifiers = new();
  public int CooldownTurns;
}
