using UnityEngine;

namespace AntKnow.Game
{
    /// <summary>
    /// Component gắn vào mỗi tile GameObject
    /// Quản lý visual của 1 tile: platform, text name, text price, spawned houses
    /// </summary>
    public class TileVisual : MonoBehaviour
    {
        [Header("Tile Structure")]
        [SerializeField] private Transform platform; // Platform con để spawn house lên
        [SerializeField] private TextMesh textName; // Text hiển thị tên ô đất (TextMesh, not TextMeshPro)
        [SerializeField] private TextMesh textPrice; // Text hiển thị giá (TextMesh, optional cho Property tiles)

        [Header("House/Hotel Markers")]
        [Tooltip("Transform markers cho house positions (4 markers)")]
        [SerializeField] private Transform[] houseMarkers = new Transform[4]; // HouseMarker1-4
        [Tooltip("Transform marker cho hotel position (1 marker)")]
        [SerializeField] private Transform hotelMarker; // HotelMarker
        
        [Header("Auto Find")]
        [SerializeField] private bool autoFindChildren = true;

        [Header("Info")]
        public int tileIndex = -1;

        [Header("Debug Visualization")]
        [SerializeField] private bool showHousePositions = false;
        [SerializeField] private Color housePositionColor = Color.green;
        [SerializeField] private Color hotelPositionColor = Color.blue;

        /// <summary>
        /// Public property to access tile index (for TileClickDetector)
        /// </summary>
        public int TileIndex => tileIndex;
        
        // Spawned houses on this tile
        private GameObject[] spawnedHouses = new GameObject[4]; // Max 4 houses
        private GameObject spawnedHotel = null;
        
        private void Awake()
        {
            if (autoFindChildren)
            {
                FindChildren();
            }
        }
        
        /// <summary>
        /// Tự động tìm children
        /// </summary>
        private void FindChildren()
        {
            // Tìm platform (cube mỏng dẹp)
            if (platform == null)
            {
                // Method 1: Tìm child có tag "Platform"
                foreach (Transform child in transform)
                {
                    if (child.CompareTag("Platform"))
                    {
                        platform = child;
                        Debug.Log($"[TileVisual] Found platform by tag: {child.name}");
                        break;
                    }
                }

                // Method 2: Tìm child có name chứa "platform"
                if (platform == null)
                {
                    foreach (Transform child in transform)
                    {
                        if (child.name.ToLower().Contains("platform"))
                        {
                            platform = child;
                            Debug.Log($"[TileVisual] Found platform by name: {child.name}");
                            break;
                        }
                    }
                }

                // Method 3: Tìm cube mỏng dẹp (scale.y < 0.5)
                if (platform == null)
                {
                    foreach (Transform child in transform)
                    {
                        if (child.localScale.y < 0.5f && child.localScale.y > 0.01f)
                        {
                            platform = child;
                            Debug.Log($"[TileVisual] Found platform by scale: {child.name} (y={child.localScale.y})");
                            break;
                        }
                    }
                }

                // Method 4: Tìm child thứ 2 (thường là platform)
                if (platform == null && transform.childCount > 1)
                {
                    platform = transform.GetChild(1);
                    Debug.Log($"[TileVisual] Using child[1] as platform: {platform.name}");
                }
            }
            
            // Tìm text name (TextMesh)
            if (textName == null)
            {
                TextMesh[] texts = GetComponentsInChildren<TextMesh>();
                foreach (var text in texts)
                {
                    if (text.name.ToLower().Contains("name"))
                    {
                        textName = text;
                        break;
                    }
                }
            }
            
            // Tìm text price (TextMesh, optional)
            if (textPrice == null)
            {
                TextMesh[] texts = GetComponentsInChildren<TextMesh>();
                foreach (var text in texts)
                {
                    if (text.name.ToLower().Contains("price") || text.name.ToLower().Contains("gia"))
                    {
                        textPrice = text;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Set tile info
        /// </summary>
        public void SetTileInfo(int index, string name, int price, TileType tileType)
        {
            tileIndex = index;

            if (textName != null)
            {
                textName.text = name;
            }

            if (textPrice != null)
            {
                // Chỉ hiển thị giá cho Property tiles
                if (tileType == TileType.Property && price > 0)
                {
                    textPrice.text = $"${price}";
                }
                else
                {
                    // Ô đặc biệt (Start, Event, Jail, Quiz, Travel) không hiển thị giá
                    textPrice.text = "";
                    // Hoặc có thể ẩn luôn TextMeshPro component
                    textPrice.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update price text (for rent display)
        /// Only for Property tiles - Special tiles never show price
        /// </summary>
        public void UpdatePrice(int price, bool isProperty = true)
        {
            if (textPrice != null)
            {
                if (isProperty && price > 0)
                {
                    textPrice.text = $"${price}";
                    textPrice.gameObject.SetActive(true);
                }
                else
                {
                    textPrice.text = "";
                    textPrice.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Set platform color (when owned)
        /// </summary>
        public void SetPlatformColor(Color color)
        {
            if (platform == null) return;

            Renderer renderer = platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create new material instance
                Material newMat = new Material(renderer.material);
                newMat.color = color;
                renderer.material = newMat;

                Debug.Log($"[TileVisual] Tile {tileIndex} platform color: {color}");
            }
        }

        /// <summary>
        /// Reset platform color (when not owned)
        /// </summary>
        public void ResetPlatformColor()
        {
            if (platform == null) return;

            Renderer renderer = platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Reset to white
                Material newMat = new Material(renderer.material);
                newMat.color = Color.white;
                renderer.material = newMat;
            }
        }
        
        /// <summary>
        /// Spawn houses (level 1-4) - 4 houses liền kề trên Platform
        ///
        /// AXES ORIENTATION:
        /// Platform: Y↑ (up), Z→ (vào giữa), X→ (phải)
        /// House:    Y↑ (up), X→ (vào giữa), Z← (trái)
        ///
        /// ROTATION LOGIC:
        /// - House X (→giữa) align với Platform Z (→giữa)
        /// - House -Z (→phải) align với Platform X (→phải)
        /// → Rotate -90° around Y axis
        /// </summary>
        public void SpawnHouses(GameObject housePrefab, int count, Color playerColor, string roofMaterialName = "ngói")
        {
            ClearHouses();

            if (housePrefab == null)
            {
                Debug.LogWarning("[TileVisual] housePrefab is null!");
                return;
            }

            // ⭐ NEW: Sử dụng Transform Markers thay vì hardcode positions
            for (int i = 0; i < count && i < 4; i++)
            {
                // Check if marker exists
                if (houseMarkers == null || i >= houseMarkers.Length || houseMarkers[i] == null)
                {
                    Debug.LogWarning($"[TileVisual] HouseMarker{i + 1} not found! Skipping house {i + 1}");
                    continue;
                }

                Transform marker = houseMarkers[i];

                // Spawn house tại vị trí marker
                GameObject house = Instantiate(housePrefab);

                // Set position, rotation, scale từ marker
                house.transform.position = marker.position;
                house.transform.rotation = marker.rotation;
                house.transform.localScale = Vector3.one * 0.255f; // Uniform scale

                Debug.Log($"[TileVisual] House {i + 1} spawned at marker position: {marker.position}");

                // Set color to roof material
                SetHouseColor(house, playerColor, roofMaterialName);

                // Parent to tile (not marker, not platform)
                house.transform.SetParent(transform);

                spawnedHouses[i] = house;
            }

            Debug.Log($"[TileVisual] Spawned {count} houses on tile {tileIndex}");
        }
        
        /// <summary>
        /// Spawn hotel (level 5) - Thay thế 4 houses
        /// ✅ FIX: Hotel rotation same as house (use marker rotation + 90° Y offset if needed)
        /// </summary>
        public void SpawnHotel(GameObject hotelPrefab, Color playerColor, string roofMaterialName = "ngói")
        {
            ClearHouses();

            if (hotelPrefab == null)
            {
                Debug.LogWarning("[TileVisual] hotelPrefab is null!");
                return;
            }

            // ⭐ NEW: Sử dụng HotelMarker thay vì hardcode position
            if (hotelMarker == null)
            {
                Debug.LogWarning("[TileVisual] HotelMarker not found! Cannot spawn hotel");
                return;
            }

            // Spawn hotel tại vị trí marker
            spawnedHotel = Instantiate(hotelPrefab);

            // Set position from marker
            spawnedHotel.transform.position = hotelMarker.position;

            // ✅ FIX: Use same rotation as house markers (marker rotation)
            // If hotel model is rotated differently than house, add offset here
            spawnedHotel.transform.rotation = hotelMarker.rotation;

            // Uniform scale (larger than house)
            spawnedHotel.transform.localScale = Vector3.one * 0.5f; // ✅ Smaller scale (was 9f)

            Debug.Log($"[TileVisual] Hotel spawned at marker position: {hotelMarker.position}, rotation: {hotelMarker.rotation.eulerAngles}");

            // Set color to roof material
            SetHouseColor(spawnedHotel, playerColor, roofMaterialName);

            // Parent to tile (not marker, not platform)
            spawnedHotel.transform.SetParent(transform);

            Debug.Log($"[TileVisual] Spawned hotel on tile {tileIndex}");
        }
        
        /// <summary>
        /// Set color to specific material (roof)
        /// </summary>
        private void SetHouseColor(GameObject house, Color color, string materialName)
        {
            if (house == null) return;
            
            // Tìm tất cả renderers
            Renderer[] renderers = house.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                // Tìm material có tên chứa materialName (ví dụ: "ngói")
                Material[] materials = renderer.materials;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.ToLower().Contains(materialName.ToLower()))
                    {
                        // Tạo material instance mới
                        Material newMat = new Material(materials[i]);
                        newMat.color = color;
                        
                        // Replace material
                        materials[i] = newMat;
                    }
                }
                
                renderer.materials = materials;
            }
        }
        
        /// <summary>
        /// Clear all houses
        /// </summary>
        public void ClearHouses()
        {
            for (int i = 0; i < spawnedHouses.Length; i++)
            {
                if (spawnedHouses[i] != null)
                {
                    Destroy(spawnedHouses[i]);
                    spawnedHouses[i] = null;
                }
            }
            
            if (spawnedHotel != null)
            {
                Destroy(spawnedHotel);
                spawnedHotel = null;
            }
        }
        
        /// <summary>
        /// Get platform position (for spawning)
        /// </summary>
        public Vector3 GetPlatformPosition()
        {
            if (platform != null)
            {
                return platform.position;
            }
            return transform.position;
        }

        /// <summary>
        /// Draw Gizmos to visualize house/hotel spawn positions
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showHousePositions || platform == null)
            {
                return;
            }

            // House positions (same as in SpawnHouses())
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(-0.15f, 0.1f, -0.15f),  // House 1
                new Vector3(0.15f, 0.1f, -0.15f),   // House 2
                new Vector3(-0.15f, 0.1f, 0.15f),   // House 3
                new Vector3(0.15f, 0.1f, 0.15f)     // House 4
            };

            // Draw house positions
            Gizmos.color = housePositionColor;
            for (int i = 0; i < localPositions.Length; i++)
            {
                Vector3 worldPos = platform.TransformPoint(localPositions[i]);
                worldPos.y = platform.position.y + (platform.localScale.y / 2f) + 0.05f;

                // Draw sphere at position
                Gizmos.DrawWireSphere(worldPos, 0.05f);

                // Draw label
#if UNITY_EDITOR
                UnityEditor.Handles.Label(worldPos + Vector3.up * 0.1f, $"H{i + 1}");
#endif
            }

            // Draw hotel position (center)
            Gizmos.color = hotelPositionColor;
            Vector3 hotelLocalPos = new Vector3(0f, 0f, 0f);
            Vector3 hotelWorldPos = platform.TransformPoint(hotelLocalPos);
            hotelWorldPos.y = platform.position.y + (platform.localScale.y / 2f) + 0.1f;

            Gizmos.DrawWireSphere(hotelWorldPos, 0.08f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(hotelWorldPos + Vector3.up * 0.15f, "Hotel");
#endif
        }
    }
}

