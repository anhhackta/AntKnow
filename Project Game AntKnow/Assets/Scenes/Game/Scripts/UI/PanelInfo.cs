using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin chi tiết của player
    /// Kích hoạt khi click vào PanelMe hoặc PanelPlayerPrefab
    /// </summary>
    public class PanelInfo : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image imageGender;
        [SerializeField] private TextMeshProUGUI textPlayerName;
        [SerializeField] private TextMeshProUGUI textMatchesPlayed;
        [SerializeField] private TextMeshProUGUI textMatchesWon;
        [SerializeField] private Button btnClose;
        
        [Header("Gender Sprites")]
        [SerializeField] private Sprite spriteMale;
        [SerializeField] private Sprite spriteFemale;
        
        private PlayerGameController currentPlayer;
        
        private void Awake()
        {
            // Setup close button
            if (btnClose != null)
            {
                btnClose.onClick.AddListener(Hide);
            }
            
            // Initially hidden
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Show panel with player info
        /// </summary>
        public void Show(PlayerGameController player)
        {
            if (player == null)
            {
                Debug.LogWarning("[PanelInfo] Player is null!");
                return;
            }
            
            currentPlayer = player;
            UpdateDisplay();
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Update display with current player data
        /// </summary>
        private void UpdateDisplay()
        {
            if (currentPlayer == null) return;
            
            // Update player name
            if (textPlayerName != null)
            {
                textPlayerName.text = currentPlayer.PlayerName;
            }
            
            // Update gender sprite
            if (imageGender != null)
            {
                if (currentPlayer.IsMale)
                {
                    imageGender.sprite = spriteMale != null ? spriteMale : CreateDefaultSprite(Color.blue);
                }
                else
                {
                    imageGender.sprite = spriteFemale != null ? spriteFemale : CreateDefaultSprite(Color.magenta);
                }
            }
            
            // Load player stats from Firebase
            LoadPlayerStats();
        }
        
        /// <summary>
        /// Load player stats from Firebase
        /// </summary>
        private void LoadPlayerStats()
        {
            if (currentPlayer == null) return;
            
            // Get player stats from GameDataManager or Firebase
            var gameDataManager = GameDataManager.Instance;
            if (gameDataManager != null)
            {
                // Update matches played
                if (textMatchesPlayed != null)
                {
                    textMatchesPlayed.text = $"Số trận chơi: {gameDataManager.currentMatchesPlayed}";
                }
                
                // Update matches won
                if (textMatchesWon != null)
                {
                    textMatchesWon.text = $"Số trận thắng: {gameDataManager.currentMatchesWon}";
                }
            }
            else
            {
                // Fallback values
                if (textMatchesPlayed != null)
                {
                    textMatchesPlayed.text = "Số trận chơi: 0";
                }
                
                if (textMatchesWon != null)
                {
                    textMatchesWon.text = "Số trận thắng: 0";
                }
            }
        }
        
        /// <summary>
        /// Create default sprite if gender sprites not assigned
        /// </summary>
        private Sprite CreateDefaultSprite(Color color)
        {
            // Create a simple colored square sprite
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            currentPlayer = null;
        }
        
        /// <summary>
        /// Check if panel is showing
        /// </summary>
        public bool IsShowing()
        {
            return gameObject.activeInHierarchy;
        }
        
        /// <summary>
        /// Get current player being displayed
        /// </summary>
        public PlayerGameController GetCurrentPlayer()
        {
            return currentPlayer;
        }
        
        /// <summary>
        /// Update stats display (call when stats change)
        /// </summary>
        public void RefreshStats()
        {
            if (IsShowing())
            {
                LoadPlayerStats();
            }
        }
    }
}
