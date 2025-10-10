using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AntKnow.Game
{
    /// <summary>
    /// Helper script để generate 36 waypoints theo circular path
    /// Attach vào GameObject và dùng Context Menu để generate
    /// </summary>
    public class WaypointGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int waypointCount = 36;
        [SerializeField] private float radius = 10f;
        [SerializeField] private float tileHeight = 0.2f; // Độ cao của tile (để player đứng trên)
        [SerializeField] private bool createTileVisuals = true;
        
        [Header("Tile Visual Settings")]
        [SerializeField] private Vector3 tileSize = new Vector3(1.5f, 0.2f, 1.5f);
        [SerializeField] private Material tileMaterial;
        
        [ContextMenu("Generate Waypoints")]
        public void GenerateWaypoints()
        {
            // Clear existing waypoints
            ClearWaypoints();
            
            Debug.Log($"[WaypointGenerator] Generating {waypointCount} waypoints...");
            
            // Generate circular path
            for (int i = 0; i < waypointCount; i++)
            {
                // Calculate angle (clockwise from right)
                float angle = (i / (float)waypointCount) * 360f * Mathf.Deg2Rad;
                
                // Calculate position (circular)
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                
                // Create waypoint
                GameObject waypoint = new GameObject($"Waypoint_{i:00}");
                waypoint.transform.parent = transform;
                waypoint.transform.position = new Vector3(x, tileHeight, z);
                
                // Add tile visual
                if (createTileVisuals)
                {
                    CreateTileVisual(waypoint, i);
                }
                
                // Add label component for debugging
                var label = waypoint.AddComponent<WaypointLabel>();
                label.waypointIndex = i;
            }
            
            Debug.Log($"[WaypointGenerator] Generated {waypointCount} waypoints successfully!");
            
            #if UNITY_EDITOR
            // Mark scene as dirty
            EditorUtility.SetDirty(gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            #endif
        }
        
        [ContextMenu("Clear Waypoints")]
        public void ClearWaypoints()
        {
            // Remove all children
            while (transform.childCount > 0)
            {
                #if UNITY_EDITOR
                DestroyImmediate(transform.GetChild(0).gameObject);
                #else
                Destroy(transform.GetChild(0).gameObject);
                #endif
            }
            
            Debug.Log("[WaypointGenerator] Cleared all waypoints");
        }
        
        /// <summary>
        /// Create tile visual (cube) for waypoint
        /// </summary>
        private void CreateTileVisual(GameObject waypoint, int index)
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "Tile";
            tile.transform.parent = waypoint.transform;
            tile.transform.localPosition = Vector3.zero;
            tile.transform.localScale = tileSize;
            
            // Set material
            if (tileMaterial != null)
            {
                tile.GetComponent<Renderer>().material = tileMaterial;
            }
            else
            {
                // Default color based on tile type
                var renderer = tile.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Standard"));
                
                // Color by tile type
                if (index == 0)
                {
                    renderer.material.color = Color.green; // Start
                }
                else if (index == 10)
                {
                    renderer.material.color = Color.red; // Jail
                }
                else if (index == 19)
                {
                    renderer.material.color = Color.yellow; // Quiz
                }
                else if (index == 28)
                {
                    renderer.material.color = Color.cyan; // Travel
                }
                else if (index == 7 || index == 16 || index == 25 || index == 33)
                {
                    renderer.material.color = Color.magenta; // Event
                }
                else
                {
                    renderer.material.color = Color.white; // Property
                }
            }
            
            // Remove collider (not needed)
            Destroy(tile.GetComponent<Collider>());
        }
        
        private void OnDrawGizmos()
        {
            // Draw circle preview
            Gizmos.color = Color.yellow;
            
            int segments = 36;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
                
                Vector3 p1 = transform.position + new Vector3(Mathf.Cos(angle1) * radius, tileHeight, Mathf.Sin(angle1) * radius);
                Vector3 p2 = transform.position + new Vector3(Mathf.Cos(angle2) * radius, tileHeight, Mathf.Sin(angle2) * radius);
                
                Gizmos.DrawLine(p1, p2);
            }
            
            // Draw center
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
    
    /// <summary>
    /// Component để hiển thị waypoint index trong Scene view
    /// </summary>
    public class WaypointLabel : MonoBehaviour
    {
        public int waypointIndex;
        
        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            // Draw waypoint number
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            
            Vector3 labelPos = transform.position + Vector3.up * 0.5f;
            Handles.Label(labelPos, $"{waypointIndex}", style);
            
            // Draw sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            #endif
        }
    }
}

