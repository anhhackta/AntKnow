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
        
        [Header("Auto Find")]
        [SerializeField] private bool autoFindChildren = true;
        
        [Header("Info")]
        public int tileIndex = -1;
        
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

            if (housePrefab == null || platform == null)
            {
                return;
            }

            // 4 vị trí cố định trên Platform (hình chữ nhật)
            // Positions trong local space của Platform
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(-0.15f, 0.1f, -0.15f),  // Top-left (X trái, Z xa)
                new Vector3(0.15f, 0.1f, -0.15f),   // Top-right (X phải, Z xa)
                new Vector3(-0.15f, 0.1f, 0.15f),   // Bottom-left (X trái, Z gần)
                new Vector3(0.15f, 0.1f, 0.15f)     // Bottom-right (X phải, Z gần)
            };

            for (int i = 0; i < count && i < 4; i++)
            {
                // Spawn as child of platform
                GameObject house = Instantiate(housePrefab, platform);

                // Set local position
                house.transform.localPosition = localPositions[i];

                // Set local scale (0.255 như khi để ngoài)
                house.transform.localScale = Vector3.one * 0.255f;

                // Fix rotation:
                // House: Y↑, X→giữa, Z←trái
                // Platform: Y↑, Z→giữa, X→phải
                // Rotate -90° around Y axis để align X của House với Z của Platform
                house.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

                // Set color to roof material
                SetHouseColor(house, playerColor, roofMaterialName);

                spawnedHouses[i] = house;
            }

            Debug.Log($"[TileVisual] Spawned {count} houses on tile {tileIndex}");
        }
        
        /// <summary>
        /// Spawn hotel (level 5) - Thay thế 4 houses
        ///
        /// AXES ORIENTATION:
        /// Platform: Y↑ (up), Z→ (vào giữa), X→ (phải)
        /// Hotel:    Z↑ (up), Y→ (vào giữa), X← (trái)
        ///
        /// ROTATION LOGIC:
        /// - Hotel Z (↑) align với Platform Y (↑)
        /// - Hotel Y (→giữa) align với Platform Z (→giữa)
        /// - Hotel -X (→phải) align với Platform X (→phải)
        /// → Rotate 90° around X axis, then 180° around Y axis
        /// → Combined: Quaternion.Euler(90f, 180f, 0f)
        /// </summary>
        public void SpawnHotel(GameObject hotelPrefab, Color playerColor, string roofMaterialName = "ngói")
        {
            ClearHouses();

            if (hotelPrefab == null || platform == null)
            {
                return;
            }

            // Spawn as child of platform
            spawnedHotel = Instantiate(hotelPrefab, platform);

            // Set local position (center of platform)
            spawnedHotel.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            // Set local scale (9 như khi để ngoài)
            spawnedHotel.transform.localScale = Vector3.one * 9f;

            // Fix rotation:
            // Hotel: Z↑, Y→giữa, X←trái
            // Platform: Y↑, Z→giữa, X→phải
            // Step 1: Rotate 90° around X → Z↑ becomes Y↑
            // Step 2: Rotate 180° around Y → X← becomes X→
            spawnedHotel.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);

            // Set color to roof material
            SetHouseColor(spawnedHotel, playerColor, roofMaterialName);

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
    }
}

