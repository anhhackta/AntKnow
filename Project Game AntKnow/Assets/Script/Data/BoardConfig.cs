using UnityEngine;

[CreateAssetMenu(fileName="BoardConfig", menuName="AntKnow/BoardConfig")]
public class BoardConfig : ScriptableObject {
  public string boardName;
  public TileDef[] tiles; // ordered circular path
}

