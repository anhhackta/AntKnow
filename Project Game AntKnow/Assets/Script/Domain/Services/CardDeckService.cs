using System;
using System.Collections.Generic;

public sealed class CardDeckService {
  readonly List<int> _drawPile = new();
  readonly List<int> _discardPile = new();
  readonly Random _rng;

  public CardDeckService(IEnumerable<int> cardIds, int seed) {
    _rng = new Random(seed);
    _drawPile.AddRange(cardIds);
    Shuffle(_drawPile);
  }

  public int Draw() {
    if (_drawPile.Count == 0) Reshuffle();
    if (_drawPile.Count == 0) return -1;
    int card = _drawPile[^1];
    _drawPile.RemoveAt(_drawPile.Count - 1);
    return card;
  }

  public void Discard(int cardId) => _discardPile.Add(cardId);

  public void Reshuffle() {
    if (_discardPile.Count == 0) return;
    _drawPile.AddRange(_discardPile);
    _discardPile.Clear();
    Shuffle(_drawPile);
  }

  void Shuffle(List<int> list) {
    for (int i = list.Count - 1; i > 0; i--) {
      int swapIndex = _rng.Next(i + 1);
      (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
    }
  }
}
