using UnityEngine;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Fortune Wheel Controller - Đơn giản với button click và quay mượt mà
    /// </summary>
    public class FortuneWheelController : MonoBehaviour
    {
        [Header("Wheel Components")]
        [SerializeField] private Rigidbody2D wheelRigidbody; // Rigidbody2D của wheel panel
        [SerializeField] private Transform pointer;          // Image pointer
        
        [Header("Physics Settings")]
        [SerializeField] private float rotatePower = 2000f;
        [SerializeField] private float stopPower = 800f;
        [SerializeField] private float maxAngularVelocity = 1440f;
        
        [Header("Wheel Sections")]
        [SerializeField] private float[] sectionAngles = { 0f, 120f, 240f }; // 3 sections
        
        private bool isSpinning = false;
        private bool isStopped = false;
        private float stopTimer = 0f;
        private System.Action<int> onResultCallback;
        
        private void Awake()
        {
            // Validate components
            if (wheelRigidbody == null)
            {
                Debug.LogError("[FortuneWheelController] Wheel Rigidbody2D not assigned!");
            }
            
            if (pointer == null)
            {
                Debug.LogError("[FortuneWheelController] Pointer Transform not assigned!");
            }
        }
        
        private void Update()
        {
            // Physics-based wheel stopping
            if (isSpinning && wheelRigidbody != null)
            {
                if (wheelRigidbody.angularVelocity > 0)
                {
                    // Gradually slow down wheel
                    wheelRigidbody.angularVelocity -= stopPower * Time.deltaTime;
                    wheelRigidbody.angularVelocity = Mathf.Clamp(wheelRigidbody.angularVelocity, 0, maxAngularVelocity);
                }
                
                // Check if wheel has stopped
                if (wheelRigidbody.angularVelocity <= 0.1f && isSpinning)
                {
                    isStopped = true;
                    stopTimer += Time.deltaTime;
                    
                    // Wait a bit then get result
                    if (stopTimer >= 0.5f)
                    {
                        GetReward();
                        isSpinning = false;
                        isStopped = false;
                        stopTimer = 0f;
                    }
                }
            }
        }
        
        /// <summary>
        /// Spin wheel với physics - Mượt mà như Roulette
        /// </summary>
        public void Spin(System.Action<int> onResult)
        {
            if (isSpinning) 
            {
                Debug.LogWarning("[FortuneWheelController] Already spinning!");
                return;
            }
            
            if (wheelRigidbody == null)
            {
                Debug.LogError("[FortuneWheelController] Rigidbody2D not assigned!");
                return;
            }
            
            onResultCallback = onResult;
            isSpinning = true;
            isStopped = false;
            stopTimer = 0f;
            
            // Apply torque to start spinning
            wheelRigidbody.AddTorque(rotatePower);
            
            Debug.Log("[FortuneWheelController] Starting physics-based wheel spin...");
        }
        
        /// <summary>
        /// Get reward based on final wheel angle - Physics-based
        /// </summary>
        private void GetReward()
        {
            if (wheelRigidbody == null) return;
            
            float currentAngle = wheelRigidbody.transform.eulerAngles.z;
            int result = GetSectionFromAngle(currentAngle);
            
            // Snap to exact section angle for visual clarity
            SnapToSection(result);
            
            Debug.Log($"[FortuneWheelController] Wheel stopped at angle {currentAngle}, section {result}: {GetSectionName(result)}");
            
            // Call callback with result
            onResultCallback?.Invoke(result);
        }
        
        /// <summary>
        /// Snap wheel to exact section angle
        /// </summary>
        private void SnapToSection(int sectionIndex)
        {
            if (sectionIndex >= 0 && sectionIndex < sectionAngles.Length)
            {
                float targetAngle = sectionAngles[sectionIndex];
                wheelRigidbody.transform.eulerAngles = new Vector3(0, 0, targetAngle);
                
                // Stop rigidbody rotation
                if (wheelRigidbody != null)
                {
                    wheelRigidbody.angularVelocity = 0f;
                }
            }
        }
        
        /// <summary>
        /// Get section index from wheel angle
        /// </summary>
        private int GetSectionFromAngle(float angle)
        {
            // Normalize angle to 0-360
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            
            // Check which section the pointer is in
            for (int i = 0; i < sectionAngles.Length; i++)
            {
                float sectionStart = sectionAngles[i];
                float sectionEnd = sectionAngles[(i + 1) % sectionAngles.Length];
                
                // Handle wrap-around (e.g., section from 240 to 0)
                if (sectionEnd < sectionStart) 
                {
                    sectionEnd += 360;
                }
                
                // Check if angle falls within this section
                if (angle >= sectionStart && angle < sectionEnd)
                {
                    return i;
                }
            }
            
            // Fallback to first section
            Debug.LogWarning($"[FortuneWheelController] Could not determine section for angle {angle}, defaulting to 0");
            return 0;
        }
        
        /// <summary>
        /// Get section name by index
        /// </summary>
        public string GetSectionName(int sectionIndex)
        {
            switch (sectionIndex)
            {
                case 0: return "Mất tiền";
                case 1: return "Hạ nhà";
                case 2: return "Bỏ qua";
                default: return "Unknown";
            }
        }
        
        
        /// <summary>
        /// Reset wheel to starting position
        /// </summary>
        public void ResetWheel()
        {
            if (wheelRigidbody != null)
            {
                wheelRigidbody.transform.eulerAngles = Vector3.zero;
                wheelRigidbody.angularVelocity = 0f;
                isSpinning = false;
                isStopped = false;
                stopTimer = 0f;
                Debug.Log("[FortuneWheelController] Wheel reset to starting position");
            }
        }
        
        /// <summary>
        /// Check if wheel is currently spinning
        /// </summary>
        public bool IsSpinning()
        {
            return isSpinning;
        }
        
        /// <summary>
        /// Get current wheel angle
        /// </summary>
        public float GetCurrentAngle()
        {
            return wheelRigidbody != null ? wheelRigidbody.transform.eulerAngles.z : 0f;
        }
        
        /// <summary>
        /// Set wheel to specific angle (for testing)
        /// </summary>
        public void SetWheelAngle(float angle)
        {
            if (wheelRigidbody != null)
            {
                wheelRigidbody.transform.eulerAngles = Vector3.forward * angle;
                wheelRigidbody.angularVelocity = 0f;
                Debug.Log($"[FortuneWheelController] Wheel set to angle {angle}");
            }
        }
        
        /// <summary>
        /// Test wheel sections (for debugging)
        /// </summary>
        [ContextMenu("Test All Sections")]
        public void TestAllSections()
        {
            Debug.Log("[FortuneWheelController] Testing all wheel sections:");
            
            for (int i = 0; i < sectionAngles.Length; i++)
            {
                float angle = sectionAngles[i];
                SetWheelAngle(angle);
                int result = GetSectionFromAngle(angle);
                
                Debug.Log($"Section {i}: Angle {angle}° -> Result {result} ({GetSectionName(result)})");
            }
        }
        
        /// <summary>
        /// Test random spin (for debugging)
        /// </summary>
        [ContextMenu("Test Random Spin")]
        public void TestRandomSpin()
        {
            if (!isSpinning)
            {
                Spin((result) => {
                    Debug.Log($"[FortuneWheelController] Test spin result: {result} ({GetSectionName(result)})");
                });
            }
            else
            {
                Debug.Log("[FortuneWheelController] Cannot test - wheel is already spinning!");
            }
        }
        
        /// <summary>
        /// Test physics-based smooth spin (for debugging)
        /// </summary>
        [ContextMenu("Test Physics Spin")]
        public void TestPhysicsSpin()
        {
            if (!isSpinning)
            {
                Debug.Log("[FortuneWheelController] Testing physics-based smooth spin...");
                Spin((result) => {
                    Debug.Log($"[FortuneWheelController] Physics spin completed! Result: {result} ({GetSectionName(result)})");
                });
            }
            else
            {
                Debug.Log("[FortuneWheelController] Cannot test - wheel is already spinning!");
            }
        }
    }
}
