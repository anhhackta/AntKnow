using UnityEngine;

[CreateAssetMenu(fileName="TileDef", menuName="AntKnow/TileDef")]
public class TileDef : ScriptableObject {
  public int tileId;            // index along the board path
  public string displayName;
  public TileType type;
  public int basePrice;         // Property/Tax/Bonus
  public int amount;            // ± money (Tax/Bonus) or travel fee
  public int destNode = -1;     // GoToJail/Travel fixed dest; -1 if none
}

