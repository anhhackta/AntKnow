using UnityEngine;

[CreateAssetMenu(fileName = "CardLibrary", menuName = "AntKnow/Card Library")]
public class CardLibrary : ScriptableObject {
  public CardDefinitionAsset[] cards;
}
