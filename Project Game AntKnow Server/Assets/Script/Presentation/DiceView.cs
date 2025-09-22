using UnityEngine;

// Simple placeholder: triggers any dice FX/animation based on RNG results
public class DiceView : MonoBehaviour {
  // Hook this to play animation/sound per die result later
  public void ShowRoll(int d1, int d2) {
    Debug.Log($"DiceView: rolled {d1} and {d2}");
  }
}

