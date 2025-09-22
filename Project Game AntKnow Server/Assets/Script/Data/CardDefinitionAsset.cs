using UnityEngine;

[CreateAssetMenu(fileName = "CardDefinition", menuName = "AntKnow/Card Definition")]
public class CardDefinitionAsset : ScriptableObject {
  public int cardId;
  public string displayName;
  [TextArea] public string description;
  public CardType type;
  public CardTrigger trigger;
  public int cost;
  public int cooldownTurns;
  public int moneyDelta;
  public int houseDelta;
  public int diceModifier;
  public int quizRewardBonus;

  public CardDefinition ToDefinition() {
    var def = new CardDefinition {
      CardId = cardId,
      Name = displayName,
      Type = type,
      Trigger = trigger,
      Description = description,
      Cost = cost,
      CooldownTurns = cooldownTurns
    };
    if (moneyDelta != 0) def.ResourceModifiers["Money"] = moneyDelta;
    if (houseDelta != 0) def.ResourceModifiers["House"] = houseDelta;
    if (diceModifier != 0) def.ResourceModifiers["Dice"] = diceModifier;
    if (quizRewardBonus != 0) def.ResourceModifiers["QuizBonus"] = quizRewardBonus;
    return def;
  }
}
