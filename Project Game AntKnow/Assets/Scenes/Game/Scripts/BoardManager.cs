using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Quản lý board game: 36 tiles, waypoints
    /// Sử dụng WaypointPath component để load waypoints
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private WaypointPath waypointPath;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private Transform[] waypoints;

        public int TotalTiles => waypoints?.Length ?? 0;

        private void Awake()
        {
            InitializeWaypoints();
        }

        /// <summary>
        /// Load waypoints từ WaypointPath component
        /// </summary>
        private void InitializeWaypoints()
        {
            // Try to find WaypointPath if not assigned
            if (waypointPath == null)
            {
                waypointPath = FindObjectOfType<WaypointPath>();
            }

            if (waypointPath == null)
            {
                Debug.LogError("[BoardManager] WaypointPath not found! Please add WaypointPath component to scene.");
                return;
            }

            // Get waypoints from WaypointPath
            waypoints = waypointPath.GetNodes();

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("[BoardManager] No waypoints found in WaypointPath!");
                return;
            }

            Debug.Log($"[BoardManager] Initialized {waypoints.Length} waypoints from WaypointPath");

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
        /// Get tile type by index (simplified for demo)
        /// </summary>
        public TileType GetTileType(int index)
        {
            // Hardcoded for demo
            if (index == 0) return TileType.Start;
            if (index == 10) return TileType.Jail;
            if (index == 19) return TileType.Quiz;
            if (index == 28) return TileType.Travel;
            if (index == 7 || index == 16 || index == 25 || index == 33) return TileType.Event;
            
            return TileType.Property;
        }
        
        /// <summary>
        /// Get tile name by index (simplified for demo)
        /// </summary>
        public string GetTileName(int index)
        {
            TileType type = GetTileType(index);

            switch (type)
            {
                case TileType.Start:
                    return "Ô Bắt Đầu";
                case TileType.Jail:
                    return "Tại Nạn";
                case TileType.Quiz:
                    return "Tra Khảo";
                case TileType.Travel:
                    return "Du Lịch";
                case TileType.Event:
                    return "Sự Kiện";
                case TileType.Property:
                    return $"Đất Số {index}";
                default:
                    return $"Ô {index}";
            }
        }

        /// <summary>
        /// Get tile base price (for property tiles)
        /// </summary>
        public int GetTilePrice(int index)
        {
            TileType type = GetTileType(index);

            if (type == TileType.Property)
            {
                // Demo: All properties cost 500
                // TODO: Load from BoardConfig
                return 500;
            }

            return 0;
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

