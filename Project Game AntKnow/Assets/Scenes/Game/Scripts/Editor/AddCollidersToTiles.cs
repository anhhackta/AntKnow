using UnityEngine;
using UnityEditor;

namespace AntKnow.Game.Editor
{
    /// <summary>
    /// Add Box Colliders to all tiles
    /// </summary>
    public class AddCollidersToTiles
    {
        [MenuItem("Tools/AntKnow/Add Colliders to Tiles")]
        public static void AddColliders()
        {
            TileVisual[] tiles = Object.FindObjectsOfType<TileVisual>();
            int count = 0;
            
            foreach (var tile in tiles)
            {
                BoxCollider collider = tile.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = tile.gameObject.AddComponent<BoxCollider>();
                    collider.center = Vector3.zero;
                    collider.size = new Vector3(1f, 0.1f, 1f);
                    collider.isTrigger = false;
                    count++;
                    Debug.Log($"Added collider to {tile.name}");
                }
            }
            
            EditorUtility.DisplayDialog(
                "Success",
                $"Added colliders to {count} tiles!\nTotal tiles: {tiles.Length}",
                "OK"
            );
        }
    }
}

