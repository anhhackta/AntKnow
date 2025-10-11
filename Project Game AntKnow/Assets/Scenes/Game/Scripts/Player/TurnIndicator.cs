using UnityEngine;

namespace AntKnow.Game
{
    /// <summary>
    /// Hiển thị ping indicator trên đầu player khi đến lượt
    /// </summary>
    public class TurnIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject pingObject; // Ping visual (sphere, arrow, etc.)
        [SerializeField] private float bobSpeed = 2f; // Tốc độ lên xuống
        [SerializeField] private float bobHeight = 0.3f; // Độ cao lên xuống
        [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0); // Offset từ player
        
        private Vector3 startPosition;
        private bool isActive = false;
        
        private void Awake()
        {
            if (pingObject == null)
            {
                // Create default ping object
                pingObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pingObject.transform.SetParent(transform);
                pingObject.transform.localScale = Vector3.one * 0.3f;
                
                // Set color
                var renderer = pingObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.yellow;
                }
                
                // Remove collider
                var collider = pingObject.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }
            
            pingObject.transform.SetParent(transform);
            startPosition = offset;
            pingObject.SetActive(false);
        }
        
        private void Update()
        {
            if (isActive && pingObject != null)
            {
                // Bob up and down
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                pingObject.transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
            }
        }
        
        /// <summary>
        /// Show ping indicator
        /// </summary>
        public void Show()
        {
            isActive = true;
            if (pingObject != null)
            {
                pingObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Hide ping indicator
        /// </summary>
        public void Hide()
        {
            isActive = false;
            if (pingObject != null)
            {
                pingObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Set ping color
        /// </summary>
        public void SetColor(Color color)
        {
            if (pingObject != null)
            {
                var renderer = pingObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }
    }
}

