using UnityEngine;
using TMPro;

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
        [SerializeField] private TextMeshPro textName; // Text hiển thị tên ô đất
        [SerializeField] private TextMeshPro textPrice; // Text hiển thị giá
        
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
                // Tìm child có tag "Platform" hoặc name chứa "platform"
                foreach (Transform child in transform)
                {
                    if (child.name.ToLower().Contains("platform") || 
                        child.CompareTag("Platform"))
                    {
                        platform = child;
                        break;
                    }
                }
                
                // Nếu không tìm thấy, dùng child đầu tiên
                if (platform == null && transform.childCount > 0)
                {
                    platform = transform.GetChild(0);
                }
            }
            
            // Tìm text name
            if (textName == null)
            {
                TextMeshPro[] texts = GetComponentsInChildren<TextMeshPro>();
                foreach (var text in texts)
                {
                    if (text.name.ToLower().Contains("name"))
                    {
                        textName = text;
                        break;
                    }
                }
            }
            
            // Tìm text price
            if (textPrice == null)
            {
                TextMeshPro[] texts = GetComponentsInChildren<TextMeshPro>();
                foreach (var text in texts)
                {
                    if (text.name.ToLower().Contains("price"))
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
        public void SetTileInfo(int index, string name, int price)
        {
            tileIndex = index;
            
            if (textName != null)
            {
                textName.text = name;
            }
            
            if (textPrice != null)
            {
                if (price > 0)
                {
                    textPrice.text = $"{price}";
                }
                else
                {
                    textPrice.text = "";
                }
            }
        }
        
        /// <summary>
        /// Spawn houses (level 1-4)
        /// </summary>
        public void SpawnHouses(GameObject housePrefab, int count, Color playerColor, string roofMaterialName = "ngói")
        {
            ClearHouses();
            
            if (housePrefab == null || platform == null)
            {
                return;
            }
            
            // Calculate positions
            float spacing = 0.5f;
            float totalWidth = (count - 1) * spacing;
            float startX = -totalWidth / 2f;
            
            Vector3 platformPos = platform.position;
            Vector3 platformScale = platform.localScale;
            float platformHeight = platformScale.y;
            
            for (int i = 0; i < count && i < 4; i++)
            {
                Vector3 pos = platformPos + new Vector3(startX + i * spacing, platformHeight / 2f + 0.5f, 0);
                GameObject house = Instantiate(housePrefab, pos, Quaternion.identity, transform);
                
                // Set color to roof material
                SetHouseColor(house, playerColor, roofMaterialName);
                
                spawnedHouses[i] = house;
            }
        }
        
        /// <summary>
        /// Spawn hotel (level 5)
        /// </summary>
        public void SpawnHotel(GameObject hotelPrefab, Color playerColor, string roofMaterialName = "ngói")
        {
            ClearHouses();
            
            if (hotelPrefab == null || platform == null)
            {
                return;
            }
            
            Vector3 platformPos = platform.position;
            Vector3 platformScale = platform.localScale;
            float platformHeight = platformScale.y;
            
            Vector3 pos = platformPos + new Vector3(0, platformHeight / 2f + 1f, 0);
            spawnedHotel = Instantiate(hotelPrefab, pos, Quaternion.identity, transform);
            
            // Set color to roof material
            SetHouseColor(spawnedHotel, playerColor, roofMaterialName);
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

