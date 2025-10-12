using UnityEngine;
using UnityEngine.EventSystems;

namespace AntKnow.Game
{
    /// <summary>
    /// Detect tile clicks using raycast from camera
    /// Singleton pattern để dễ dàng access từ bất kỳ đâu
    /// </summary>
    public class TileClickDetector : MonoBehaviour
    {
        public static TileClickDetector Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PanelTileInfo panelTileInfo;
        
        [Header("Settings")]
        [SerializeField] private LayerMask tileLayerMask = -1; // All layers by default
        [SerializeField] private float maxRaycastDistance = 100f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[TileClickDetector] Multiple instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            // Auto-find camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("[TileClickDetector] Main camera not found!");
                }
            }
            
            // Auto-find PanelTileInfo if not assigned
            if (panelTileInfo == null)
            {
                panelTileInfo = FindObjectOfType<PanelTileInfo>();
                if (panelTileInfo == null)
                {
                    Debug.LogWarning("[TileClickDetector] PanelTileInfo not found in scene!");
                }
            }
        }
        
        private void Update()
        {
            // Detect mouse click (left button)
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }
        
        /// <summary>
        /// Handle mouse click - raycast to detect tile
        /// </summary>
        private void HandleClick()
        {
            // Check if clicking on UI element (ignore if clicking on UI)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[TileClickDetector] Click on UI element, ignoring");
                }
                return;
            }
            
            if (mainCamera == null)
            {
                Debug.LogError("[TileClickDetector] Main camera is null!");
                return;
            }
            
            // Create ray from mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            // Raycast to detect tile
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxRaycastDistance, tileLayerMask))
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[TileClickDetector] Raycast hit: {hit.collider.gameObject.name}");
                }
                
                // Check if hit object is a tile or child of tile
                TileVisual tileVisual = hit.collider.GetComponent<TileVisual>();
                if (tileVisual == null)
                {
                    // Try to find TileVisual in parent
                    tileVisual = hit.collider.GetComponentInParent<TileVisual>();
                }
                
                if (tileVisual != null)
                {
                    OnTileClicked(tileVisual);
                }
                else
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"[TileClickDetector] Hit object is not a tile: {hit.collider.gameObject.name}");
                    }
                }
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[TileClickDetector] Raycast did not hit anything");
                }
            }
        }
        
        /// <summary>
        /// Called when a tile is clicked
        /// </summary>
        private void OnTileClicked(TileVisual tileVisual)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TileClickDetector] Tile clicked: {tileVisual.name}, Index: {tileVisual.TileIndex}");
            }
            
            // Show tile info panel
            if (panelTileInfo != null)
            {
                panelTileInfo.ShowTileInfo(tileVisual.TileIndex);
            }
            else
            {
                Debug.LogWarning("[TileClickDetector] PanelTileInfo is null, cannot show tile info");
            }
        }
        
        /// <summary>
        /// Enable/disable click detection
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TileClickDetector] Click detection {(enabled ? "enabled" : "disabled")}");
            }
        }
    }
}

