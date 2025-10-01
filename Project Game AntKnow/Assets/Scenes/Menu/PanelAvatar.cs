using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;

namespace AntKnow.Auth
{
    /// <summary>
    /// Panel hiển thị thông tin avatar người chơi (ingame name, level)
    /// </summary>
    public class PanelAvatar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textIngameName;
        [SerializeField] private TextMeshProUGUI textLevel;
        private GameDataManager gameDataManager;

        public void Initialize(GameDataManager gameDataManager)
        {
            this.gameDataManager = gameDataManager;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (gameDataManager == null) return;

            // Update ingame name
            if (textIngameName != null)
            {
                textIngameName.text = gameDataManager.GetDisplayName();
            }

            // Update level từ user data
            if (textLevel != null)
            {
                textLevel.text = $"Level {gameDataManager.currentLevel}";
            }

        }
        public void SetLevel(int level)
        {
            if (textLevel != null)
            {
                textLevel.text = $"Level {level}";
            }
        }
    }
}
