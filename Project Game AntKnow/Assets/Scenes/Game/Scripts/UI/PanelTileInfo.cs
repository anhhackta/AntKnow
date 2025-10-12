using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin tile: Tên, giá mua, giá thuê, chủ
    /// </summary>
    public class PanelTileInfo : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image imageLocation;
        [SerializeField] private TextMeshProUGUI textLocationName;
        [SerializeField] private TextMeshProUGUI textBuyPrice;
        [SerializeField] private TextMeshProUGUI textRentPrice;
        [SerializeField] private TextMeshProUGUI textOwner;
        [SerializeField] private Button btnClose;
        [SerializeField] private Sprite[] locationSprites; // 36 sprites

        private PropertyManager propertyManager;
        private GameManager gameManager;
        
        private void Start()
        {
            if (btnClose != null)
                btnClose.onClick.AddListener(Hide);

            propertyManager = FindObjectOfType<PropertyManager>();
            gameManager = FindObjectOfType<GameManager>();

            gameObject.SetActive(false);
        }
        
        public void ShowTileInfo(int tileIndex)
        {
            Debug.Log($"[PanelTileInfo] ShowTileInfo called for index {tileIndex}");

            SimpleTileData tile = SimpleBoardConfig.GetTiles()[tileIndex];
            if (tile == null)
            {
                Debug.LogError($"[PanelTileInfo] Tile data is NULL for index {tileIndex}!");
                return;
            }

            Debug.Log($"[PanelTileInfo] Tile data found: {tile.name}");

            // Tên
            if (textLocationName != null)
                textLocationName.text = tile.name;

            // Hình ảnh
            if (imageLocation != null && locationSprites != null && tileIndex < locationSprites.Length)
            {
                imageLocation.sprite = locationSprites[tileIndex];
                imageLocation.gameObject.SetActive(locationSprites[tileIndex] != null);
            }

            // Giá mua
            if (textBuyPrice != null)
            {
                if (tile.type == TileType.Property)
                {
                    textBuyPrice.text = $"Price Buy: ${tile.basePrice}";
                    textBuyPrice.gameObject.SetActive(true);
                }
                else
                {
                    textBuyPrice.gameObject.SetActive(false);
                }
            }

            // Giá thuê
            if (textRentPrice != null && propertyManager != null)
            {
                if (tile.type == TileType.Property)
                {
                    int level = propertyManager.GetPropertyLevel(tileIndex);
                    int rent = GetRent(tile, level);
                    textRentPrice.text = $"Price Rent (Lv{level}): ${rent}";
                    textRentPrice.gameObject.SetActive(true);
                }
                else
                {
                    textRentPrice.gameObject.SetActive(false);
                }
            }

            // Chủ sở hữu
            if (textOwner != null && propertyManager != null)
            {
                if (propertyManager.IsPropertyOwned(tileIndex))
                {
                    int ownerIndex = propertyManager.GetPropertyOwner(tileIndex);
                    string ownerName = GetPlayerName(ownerIndex);
                    textOwner.text = $"Owner: {ownerName}";
                }
                else
                {
                    textOwner.text = "";
                }
            }

            Debug.Log("[PanelTileInfo] Activating panel...");
            gameObject.SetActive(true);
            Debug.Log($"[PanelTileInfo] Panel active: {gameObject.activeSelf}");
        }
        
        private int GetRent(SimpleTileData tile, int level)
        {
            switch (level)
            {
                case 0: return tile.rent0;
                case 1: return tile.rent1;
                case 2: return tile.rent2;
                case 3: return tile.rent3;
                case 4: return tile.rent4;
                case 5: return tile.rentHotel;
                default: return 0;
            }
        }

        private string GetPlayerName(int playerIndex)
        {
            if (gameManager == null)
                return $"Player {playerIndex + 1}";

            PlayerGameController player = gameManager.GetPlayer(playerIndex);
            return player != null ? player.PlayerName : $"Player {playerIndex + 1}";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

