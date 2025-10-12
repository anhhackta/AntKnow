using UnityEngine;
using UnityEngine.EventSystems;

namespace AntKnow.Game
{
    /// <summary>
    /// Click vào tile → Hiện PanelTileInfo
    /// </summary>
    public class TileClickDetector : MonoBehaviour
    {
        [SerializeField] private PanelTileInfo panelTileInfo;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            if (panelTileInfo == null)
            {
                panelTileInfo = FindObjectOfType<PanelTileInfo>();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // ⭐ RAYCAST TRƯỚC, check UI sau
                if (mainCamera == null)
                {
                    Debug.LogError("[TileClick] Camera is NULL!");
                    return;
                }

                // Raycast từ mouse
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    Debug.Log($"[TileClick] HIT: {hit.collider.gameObject.name}");

                    // Tìm TileVisual
                    TileVisual tile = hit.collider.GetComponent<TileVisual>();
                    if (tile == null)
                        tile = hit.collider.GetComponentInParent<TileVisual>();

                    if (tile != null)
                    {
                        Debug.Log($"[TileClick] Found tile! Index: {tile.TileIndex}");

                        if (panelTileInfo != null)
                        {
                            panelTileInfo.ShowTileInfo(tile.TileIndex);
                        }
                        else
                        {
                            Debug.LogError("[TileClick] PanelTileInfo is NULL!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[TileClick] Hit '{hit.collider.gameObject.name}' is not a tile!");
                    }
                }
                else
                {
                    Debug.LogWarning("[TileClick] Raycast hit NOTHING!");
                }
            }
        }
    }
}

