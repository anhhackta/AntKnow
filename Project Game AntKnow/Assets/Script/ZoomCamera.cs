using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PanAndZoomMapCamera : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Pan Settings")]
    [SerializeField] private Vector2 mapMin = new Vector2(-50f, -50f); // Góc trái dưới của map
    [SerializeField] private Vector2 mapMax = new Vector2(50f, 50f);   // Góc phải trên của map

    private Camera cam;
    private Vector3 dragStartMousePos;
    private Vector3 dragStartCamPos;
    private bool isDragging;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0) return;

        // Lấy vị trí chuột trong thế giới trước khi zoom
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldBefore = cam.ScreenToWorldPoint(mousePos);

        // Zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);

        // Tính lại vị trí sau zoom để giữ điểm zoom cố định
        Vector3 worldAfter = cam.ScreenToWorldPoint(mousePos);
        Vector3 adjustment = worldBefore - worldAfter;
        transform.position += adjustment;

        // Sau khi zoom, giới hạn lại vị trí camera trong map
        ClampCameraToMap();
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartMousePos = Input.mousePosition;
            dragStartCamPos = transform.position;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        else if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 deltaMouse = currentMousePos - dragStartMousePos;

            // Chuyển delta chuột thành delta thế giới
            Vector3 deltaWorld = cam.ScreenToWorldPoint(dragStartMousePos) - cam.ScreenToWorldPoint(currentMousePos);

            // Di chuyển camera theo hướng kéo
            Vector3 newCamPos = dragStartCamPos + new Vector3(deltaWorld.x, deltaWorld.y, 0f);

            // Giới hạn camera trong map
            transform.position = ClampCameraPosition(newCamPos);
        }
    }

    private Vector3 ClampCameraPosition(Vector3 desiredPos)
    {
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        // Tính toán biên giới camera cần nằm trong map
        float minX = mapMin.x + camHalfWidth;
        float maxX = mapMax.x - camHalfWidth;
        float minY = mapMin.y + camHalfHeight;
        float maxY = mapMax.y - camHalfHeight;

        float clampedX = Mathf.Clamp(desiredPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPos.y, minY, maxY);

        return new Vector3(clampedX, clampedY, desiredPos.z);
    }

    private void ClampCameraToMap()
    {
        transform.position = ClampCameraPosition(transform.position);
    }
}