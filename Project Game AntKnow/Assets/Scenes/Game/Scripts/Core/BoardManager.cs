using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Quản lý board game: 36 tiles, waypoints
    /// Tự động tìm waypoints trong scene
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] waypoints; // Manual assignment

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private SimpleTileData[] tileData;

        public int TotalTiles => waypoints?.Length ?? 0;

        private void Awake()
        {
            InitializeWaypoints();
            InitializeTileData();
        }

        /// <summary>
        /// Load tile data from SimpleBoardConfig
        /// </summary>
        private void InitializeTileData()
        {
            tileData = SimpleBoardConfig.GetTiles();
            Debug.Log($"[BoardManager] Loaded {tileData.Length} tile data (Tile ID 1-36)");
        }

        /// <summary>
        /// Convert waypoint index (0-35) to tile ID (1-36)
        /// </summary>
        private int WaypointIndexToTileId(int waypointIndex)
        {
            return waypointIndex + 1;
        }

        /// <summary>
        /// Convert tile ID (1-36) to waypoint index (0-35)
        /// </summary>
        private int TileIdToWaypointIndex(int tileId)
        {
            return tileId - 1;
        }

        /// <summary>
        /// Initialize waypoints - manual assignment hoặc auto-find
        /// </summary>
        private void InitializeWaypoints()
        {
            // Nếu waypoints chưa được assign, tự động tìm
            if (waypoints == null || waypoints.Length == 0)
            {
                // Tìm tất cả objects có tên chứa "Waypoint" hoặc "Tile"
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                List<Transform> foundWaypoints = new List<Transform>();
                
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains("Waypoint") || obj.name.Contains("Tile"))
                    {
                        foundWaypoints.Add(obj.transform);
                    }
                }
                
                if (foundWaypoints.Count > 0)
                {
                    waypoints = foundWaypoints.ToArray();
                    Debug.Log($"[BoardManager] Auto-found {waypoints.Length} waypoints");
                }
                else
                {
                    Debug.LogError("[BoardManager] No waypoints found! Please assign waypoints manually or create waypoint objects.");
                    return;
                }
            }

            Debug.Log($"[BoardManager] Initialized {waypoints.Length} waypoints");

            // Validate waypoints
            if (waypoints.Length != 36)
            {
                Debug.LogWarning($"[BoardManager] Expected 36 waypoints, found {waypoints.Length}");
            }
        }
        
        /// <summary>
        /// Get waypoint position by index
        /// </summary>
        public Vector3 GetWaypointPosition(int index)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("[BoardManager] Waypoints not initialized!");
                return Vector3.zero;
            }
            
            // Wrap index
            index = index % waypoints.Length;
            if (index < 0) index += waypoints.Length;
            
            return waypoints[index].position;
        }
        
        /// <summary>
        /// Get tile type by waypoint index (0-35)
        /// </summary>
        public TileType GetTileType(int waypointIndex)
        {
            int tileId = WaypointIndexToTileId(waypointIndex);

            if (tileData == null || tileId < 1 || tileId > tileData.Length)
            {
                return TileType.Property;
            }

            return tileData[tileId - 1].type; // Array index = tileId - 1
        }
        
        /// <summary>
        /// Get tile name by waypoint index (0-35)
        /// </summary>
        public string GetTileName(int waypointIndex)
        {
            int tileId = WaypointIndexToTileId(waypointIndex);

            if (tileData == null || tileId < 1 || tileId > tileData.Length)
            {
                return $"Tile {tileId}";
            }

            return tileData[tileId - 1].name; // Array index = tileId - 1
        }

        /// <summary>
        /// Get tile base price (for property tiles) by waypoint index (0-35)
        /// </summary>
        public int GetTilePrice(int waypointIndex)
        {
            int tileId = WaypointIndexToTileId(waypointIndex);

            if (tileData == null || tileId < 1 || tileId > tileData.Length)
            {
                return 0;
            }

            return tileData[tileId - 1].basePrice; // Array index = tileId - 1
        }

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || waypoints == null || waypoints.Length == 0)
                return;
            
            // Draw waypoint path
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                
                Vector3 current = waypoints[i].position;
                Vector3 next = waypoints[(i + 1) % waypoints.Length].position;
                
                Gizmos.DrawLine(current, next);
                Gizmos.DrawWireSphere(current, 0.3f);
            }
        }
    }
    
    /// <summary>
    /// Tile types
    /// </summary>
    public enum TileType
    {
        Start,      // Ô 0
        Property,   // Nhà bình thường
        Event,      // Ô 7, 16, 25, 33
        Quiz,       // Ô 19
        Jail,       // Ô 10
        Travel      // Ô 28
    }
}

