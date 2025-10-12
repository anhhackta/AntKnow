using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin đơn giản của tile khi user click
    /// CHỈ hiển thị: Hình ảnh, tên, giá mua, giá thuê, chủ sở hữu
    /// </summary>
    public class PanelTileInfo : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image imageLocation; // Hình ảnh địa danh
        [SerializeField] private TextMeshProUGUI textLocationName; // Tên địa danh
        [SerializeField] private TextMeshProUGUI textBuyPrice; // Giá mua
        [SerializeField] private TextMeshProUGUI textRentPrice; // Giá thuê hiện tại
        [SerializeField] private TextMeshProUGUI textOwner; // Tên chủ sở hữu
        [SerializeField] private Button btnClose; // Button đóng panel

        [Header("Location Images")]
        [Tooltip("36 sprites theo tên ô đất (index 0-35)")]
        [SerializeField] private Sprite[] locationSprites; // 36 sprites
        
        [Header("References")]
        [SerializeField] private PropertyManager propertyManager;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameManager gameManager;
        
        [Header("Settings")]
        [SerializeField] private bool closeOnOutsideClick = true;
        
        private int currentTileIndex = -1;
        
        private void Awake()
        {
            // Setup button listener
            if (btnClose != null)
            {
                btnClose.onClick.AddListener(Hide);
            }
            
            // Auto-find references if not assigned
            if (propertyManager == null)
            {
                propertyManager = FindObjectOfType<PropertyManager>();
            }
            
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }
            
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
            
            // Start hidden
            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            // Close on outside click (optional)
            if (closeOnOutsideClick && Input.GetMouseButtonDown(0))
            {
                // Check if click is outside panel
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    GetComponent<RectTransform>(), 
                    Input.mousePosition, 
                    null))
                {
                    Hide();
                }
            }
        }
        
        /// <summary>
        /// Show tile info panel for specific tile
        /// </summary>
        public void ShowTileInfo(int tileIndex)
        {
            currentTileIndex = tileIndex;

            // Get tile data from SimpleBoardConfig
            SimpleTileData tileData = SimpleBoardConfig.GetTiles()[tileIndex];

            if (tileData == null)
            {
                Debug.LogWarning($"[PanelTileInfo] No tile data found for tile {tileIndex}");
                return;
            }

            // Update UI
            UpdateDisplay(tileData, tileIndex);

            // Show panel
            gameObject.SetActive(true);

            Debug.Log($"[PanelTileInfo] Showing info for tile {tileIndex}: {tileData.name}");
        }
        
        /// <summary>
        /// Update display with tile data
        /// ⭐ SIMPLIFIED: Chỉ hiển thị image, name, buy price, rent price, owner
        /// </summary>
        private void UpdateDisplay(SimpleTileData tileData, int tileIndex)
        {
            // Update location image (từ locationSprites array)
            if (imageLocation != null)
            {
                if (locationSprites != null && tileIndex >= 0 && tileIndex < locationSprites.Length)
                {
                    Sprite sprite = locationSprites[tileIndex];
                    if (sprite != null)
                    {
                        imageLocation.sprite = sprite;
                        imageLocation.gameObject.SetActive(true);
                    }
                    else
                    {
                        imageLocation.gameObject.SetActive(false);
                    }
                }
                else
                {
                    imageLocation.gameObject.SetActive(false);
                }
            }

            // Update location name
            if (textLocationName != null)
            {
                textLocationName.text = tileData.name;
            }

            // Update buy price
            if (textBuyPrice != null)
            {
                if (tileData.type == TileType.Property)
                {
                    textBuyPrice.text = $"Giá mua: ${tileData.basePrice}";
                    textBuyPrice.gameObject.SetActive(true);
                }
                else
                {
                    textBuyPrice.gameObject.SetActive(false);
                }
            }

            // Update rent price (current level)
            if (textRentPrice != null)
            {
                if (tileData.type == TileType.Property && propertyManager != null)
                {
                    int level = propertyManager.GetPropertyLevel(tileIndex);
                    int rent = GetRentForLevel(tileData, level);
                    textRentPrice.text = $"Giá thuê (Level {level}): ${rent}";
                    textRentPrice.gameObject.SetActive(true);
                }
                else
                {
                    textRentPrice.gameObject.SetActive(false);
                }
            }

            // Update owner
            if (textOwner != null)
            {
                string ownerText = GetOwnerText(tileIndex);
                textOwner.text = ownerText;
            }
        }
        
        /// <summary>
        /// Get rent for specific level
        /// </summary>
        private int GetRentForLevel(SimpleTileData tileData, int level)
        {
            switch (level)
            {
                case 0: return tileData.rent0;
                case 1: return tileData.rent1;
                case 2: return tileData.rent2;
                case 3: return tileData.rent3;
                case 4: return tileData.rent4;
                case 5: return tileData.rentHotel;
                default: return 0;
            }
        }
        
        /// <summary>
        /// Get owner text
        /// </summary>
        private string GetOwnerText(int tileIndex)
        {
            if (propertyManager == null)
            {
                return "Chưa có chủ";
            }

            // Check if property is owned
            if (propertyManager.IsPropertyOwned(tileIndex))
            {
                int ownerIndex = propertyManager.GetPropertyOwner(tileIndex);
                string ownerName = GetPlayerName(ownerIndex);
                return $"Chủ: {ownerName}";
            }
            else
            {
                return "Chưa có chủ";
            }
        }
        
        /// <summary>
        /// Get player name by index
        /// </summary>
        private string GetPlayerName(int playerIndex)
        {
            if (gameManager == null)
            {
                return $"Player {playerIndex + 1}";
            }

            // ⭐ FIX: Get player from GameManager
            PlayerGameController player = gameManager.GetPlayer(playerIndex);
            if (player != null)
            {
                return player.PlayerName;
            }

            return $"Player {playerIndex + 1}";
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            currentTileIndex = -1;
            
            Debug.Log("[PanelTileInfo] Panel hidden");
        }
    }
}

