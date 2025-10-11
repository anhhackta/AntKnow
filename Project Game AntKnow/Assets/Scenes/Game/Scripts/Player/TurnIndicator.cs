using UnityEngine;
using Unity.Netcode;

namespace AntKnow.Game
{
    /// <summary>
    /// Network-aware ping indicator trên đầu player khi đến lượt
    /// </summary>
    public class TurnIndicator : NetworkBehaviour
    {
        [Header("Network Settings")]
        public NetworkVariable<bool> networkIsActive = new NetworkVariable<bool>(false);
        
        [Header("Settings")]
        [SerializeField] private GameObject pingObject; // Ping visual (sphere, arrow, etc.)
        [SerializeField] private float bobSpeed = 2f; // Tốc độ lên xuống
        [SerializeField] private float bobHeight = 0.3f; // Độ cao lên xuống
        [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0); // Offset từ player
        
        private Vector3 startPosition;
        private bool isActive = false;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Setup ping object
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
            
            // Subscribe to network variable changes
            networkIsActive.OnValueChanged += OnIsActiveChanged;
        }

        public override void OnNetworkDespawn()
        {
            // Unsubscribe from network variable changes
            networkIsActive.OnValueChanged -= OnIsActiveChanged;
            
            base.OnNetworkDespawn();
        }

        private void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            isActive = newValue;
            if (pingObject != null)
            {
                pingObject.SetActive(newValue);
            }
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
        /// Show ping indicator (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ShowServerRpc()
        {
            networkIsActive.Value = true;
        }
        
        /// <summary>
        /// Show ping indicator (Local method for compatibility)
        /// </summary>
        public void Show()
        {
            if (IsServer)
            {
                networkIsActive.Value = true;
            }
            else
            {
                ShowServerRpc();
            }
        }
        
        /// <summary>
        /// Hide ping indicator (Server-side)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void HideServerRpc()
        {
            networkIsActive.Value = false;
        }
        
        /// <summary>
        /// Hide ping indicator (Local method for compatibility)
        /// </summary>
        public void Hide()
        {
            if (IsServer)
            {
                networkIsActive.Value = false;
            }
            else
            {
                HideServerRpc();
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

