using UnityEngine;

namespace AntKnow.Game
{
    /// <summary>
    /// Script để auto setup tất cả tiles
    /// Gắn vào GameObject "Tiles" (parent chứa tất cả tiles)
    /// </summary>
    public class TileSetup : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private bool addTileVisualComponent = true;
        
        [Header("Info")]
        [SerializeField] private int totalTiles = 0;
        
        [Header("Debug")]
        [SerializeField] private bool showDebug = true;
        
        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupAllTiles();
            }
        }
        
        /// <summary>
        /// Setup tất cả tiles con
        /// </summary>
        [ContextMenu("Setup All Tiles")]
        public void SetupAllTiles()
        {
            totalTiles = 0;

            // Load tile data
            SimpleTileData[] tileData = SimpleBoardConfig.GetTiles();

            // Lấy tất cả children
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                // Add TileVisual component nếu chưa có
                TileVisual tileVisual = child.GetComponent<TileVisual>();
                if (tileVisual == null && addTileVisualComponent)
                {
                    tileVisual = child.gameObject.AddComponent<TileVisual>();
                    if (showDebug) Debug.Log($"[TileSetup] Added TileVisual to {child.name}");
                }

                // Set tile index (waypoint index 0-35)
                if (tileVisual != null)
                {
                    tileVisual.tileIndex = i;

                    // Load tile info from data
                    if (i < tileData.Length)
                    {
                        SimpleTileData data = tileData[i];
                        tileVisual.SetTileInfo(i, data.name, data.basePrice, data.type); // ⭐ Added tileType parameter

                        if (showDebug) Debug.Log($"[TileSetup] Tile {i}: {data.name} - ${data.basePrice} ({data.type})");
                    }
                    else
                    {
                        if (showDebug) Debug.Log($"[TileSetup] Setup tile {i}: {child.name}");
                    }
                }

                totalTiles++;
            }

            Debug.Log($"[TileSetup] Setup complete! Total tiles: {totalTiles}");
        }
        
        /// <summary>
        /// Remove tất cả TileVisual components
        /// </summary>
        [ContextMenu("Remove All TileVisual")]
        public void RemoveAllTileVisual()
        {
            int removed = 0;
            
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                TileVisual tileVisual = child.GetComponent<TileVisual>();
                
                if (tileVisual != null)
                {
                    DestroyImmediate(tileVisual);
                    removed++;
                }
            }
            
            Debug.Log($"[TileSetup] Removed {removed} TileVisual components");
        }
        
        /// <summary>
        /// Get all TileVisual components
        /// </summary>
        public TileVisual[] GetAllTiles()
        {
            return GetComponentsInChildren<TileVisual>();
        }
    }
}

