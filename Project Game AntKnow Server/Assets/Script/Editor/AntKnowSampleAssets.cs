// Editor utilities to generate sample assets for quick testing
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class AntKnowSampleAssets {
  [MenuItem("AntKnow/Generate Sample PropertyRuleSet")]
  public static void GeneratePropertyRuleSet() {
    var rs = ScriptableObject.CreateInstance<PropertyRuleSet>();
    string folder = "Assets/ScriptableObjects";
    if (!AssetDatabase.IsValidFolder(folder)) {
      AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
    }
    string assetPath = Path.Combine(folder, "PropertyRuleSet_Default.asset");
    AssetDatabase.CreateAsset(rs, assetPath);
    EditorUtility.SetDirty(rs);
    AssetDatabase.SaveAssets();
    Selection.activeObject = rs;
    Debug.Log($"Created {assetPath}");
  }

  [MenuItem("AntKnow/Generate Sample Board (36)")]
  public static void GenerateBoard36() {
    var board = ScriptableObject.CreateInstance<BoardConfig>();
    board.boardName = "Map1_Sample36";
    board.tiles = new TileDef[36];
    for (int i = 0; i < 36; i++) {
      var t = ScriptableObject.CreateInstance<TileDef>();
      t.tileId = i;
      t.displayName = $"Tile {i}";
      t.type = TileType.Property;
      t.basePrice = 100 + (i%6)*40; // sample pricing
      t.amount = 0; t.destNode = -1;
      board.tiles[i] = t;
    }
    // 4 corners
    board.tiles[0].displayName = "Start"; board.tiles[0].type = TileType.Start;
    board.tiles[9].displayName = "Jail (Visit)"; board.tiles[9].type = TileType.Jail;
    board.tiles[18].displayName = "Free Parking"; board.tiles[18].type = TileType.FreeParking;
    board.tiles[27].displayName = "Go To Jail"; board.tiles[27].type = TileType.GoToJail; board.tiles[27].destNode = 9;
    // Some events
    board.tiles[5].displayName = "+Bonus"; board.tiles[5].type = TileType.Bonus; board.tiles[5].amount = 100;
    board.tiles[14].displayName = "-Tax"; board.tiles[14].type = TileType.Tax; board.tiles[14].amount = 100;
    board.tiles[23].displayName = "+Bonus"; board.tiles[23].type = TileType.Bonus; board.tiles[23].amount = 100;
    // Quiz spots
    board.tiles[7].displayName = "Quiz"; board.tiles[7].type = TileType.Quiz;
    board.tiles[21].displayName = "Quiz"; board.tiles[21].type = TileType.Quiz;

    string folder = "Assets/ScriptableObjects";
    if (!AssetDatabase.IsValidFolder(folder)) {
      AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
    }
    // Save nested ScriptableObjects as sub-assets
    string assetPath = Path.Combine(folder, "BoardConfig_Map1_Sample36.asset");
    AssetDatabase.CreateAsset(board, assetPath);
    for (int i = 0; i < board.tiles.Length; i++) {
      AssetDatabase.AddObjectToAsset(board.tiles[i], assetPath);
    }
    EditorUtility.SetDirty(board);
    AssetDatabase.SaveAssets();
    Selection.activeObject = board;
    Debug.Log($"Created {assetPath}");
  }
}
#endif

