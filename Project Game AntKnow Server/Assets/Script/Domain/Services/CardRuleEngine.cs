using System;
using System.Collections.Generic;

public sealed class CardRuleEngine {
  readonly Dictionary<int, CardDefinition> _cards = new();

  public CardRuleEngine(IEnumerable<CardDefinition> cards) {
    foreach (var card in cards) {
      _cards[card.CardId] = card;
    }
  }

  public bool TryGetCard(int cardId, out CardDefinition card) => _cards.TryGetValue(cardId, out card);

  public void ApplyPassiveStartOfTurn(PlayerState player) {
    foreach (var cardId in player.PassiveCardIds) {
      if (!_cards.TryGetValue(cardId, out var card)) continue;
      if (card.Trigger != CardTrigger.StartOfTurn) continue;
      if (player.PassiveCooldown.TryGetValue(cardId, out var cd) && cd > 0) {
        player.PassiveCooldown[cardId] = cd - 1;
        continue;
      }
      ExecuteCard(player, card);
      if (card.CooldownTurns > 0) player.PassiveCooldown[cardId] = card.CooldownTurns;
    }
  }

  public bool ExecuteActiveCard(PlayerState player, CardDefinition card) {
    ExecuteCard(player, card);
    return true;
  }

  void ExecuteCard(PlayerState player, CardDefinition card) {
    foreach (var kv in card.ResourceModifiers) {
      switch (kv.Key) {
        case "Money":
          player.Money += kv.Value;
          break;
        case "Dice":
          // store dice bonus in PassiveCooldown dictionary with negative key pattern to simplify; gameplay layer can read later
          player.PassiveCooldown[-100 - card.CardId] = kv.Value; // convention: negative keys for temporary modifiers
          break;
        case "House":
          // house modifiers will be applied by gameplay UI when resolving property actions
          break;
        case "QuizBonus":
          player.PassiveCooldown[-200 - card.CardId] = kv.Value;
          break;
      }
    }
    if (card.StatModifiers.TryGetValue("Luck", out var luck)) player.Luck += luck;
    if (card.StatModifiers.TryGetValue("Resistance", out var resist)) player.Resistance += resist;
    if (card.StatModifiers.TryGetValue("Intelligence", out var intel)) player.Intelligence += intel;
    if (card.StatModifiers.TryGetValue("Health", out var hp)) player.Health += hp;
    if (card.StatModifiers.TryGetValue("Agility", out var agi)) player.Agility += agi;
  }
}
