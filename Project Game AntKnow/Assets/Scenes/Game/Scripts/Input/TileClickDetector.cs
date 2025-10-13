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
                // ✅ FIX: Check if clicking on UI first
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    // Clicking on UI - ignore tile clicks
                    return;
                }

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
                    // Tìm TileVisual
                    TileVisual tile = hit.collider.GetComponent<TileVisual>();
                    if (tile == null)
                        tile = hit.collider.GetComponentInParent<TileVisual>();

                    if (tile != null)
                    {
                        if (panelTileInfo != null)
                        {
                            panelTileInfo.ShowTileInfo(tile.TileIndex);
                        }
                    }
                }
            }
        }
    }
}

