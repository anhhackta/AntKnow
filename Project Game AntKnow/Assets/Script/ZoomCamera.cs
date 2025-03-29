using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCamera : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private Camera cam;

    private Vector3 dragOrigin;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        HandleZoom();
    }

    private void HandleZoom()
    {
        // Lấy giá trị scroll từ chuột
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            // Lấy vị trí chuột trong không gian màn hình
            Vector3 mousePos = Input.mousePosition;
            
            // Chuyển đổi từ vị trí chuột sang vị trí trong không gian thế giới
            Vector3 beforeZoomWorldPos = cam.ScreenToWorldPoint(mousePos);
            
            // Thực hiện zoom
            float newSize = cam.orthographicSize - scrollInput * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            
            // Lấy vị trí mới sau khi zoom để điều chỉnh vị trí của camera
            Vector3 afterZoomWorldPos = cam.ScreenToWorldPoint(mousePos);
            
            // Di chuyển camera để giữ nguyên điểm zoom
            Vector3 adjustment = beforeZoomWorldPos - afterZoomWorldPos;
            transform.position += new Vector3(adjustment.x, adjustment.y, 0);
        }
    }
}