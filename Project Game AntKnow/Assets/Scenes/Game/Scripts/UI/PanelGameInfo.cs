using UnityEngine;
using TMPro;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông tin game: Turn, Time, CurrentPlayer
    /// </summary>
    public class PanelGameInfo : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textTurn;
        [SerializeField] private TextMeshProUGUI textTime;
        [SerializeField] private TextMeshProUGUI textCurrentPlayer;
        
        [Header("Settings")]
        [SerializeField] private int maxTurns = 25;
        
        private float gameStartTime;
        private bool isGameActive = false;
        
        private void Start()
        {
            // Initialize display
            UpdateTurnDisplay(1);
            UpdateTimeDisplay(0f);
            UpdateCurrentPlayerDisplay("");
        }
        
        /// <summary>
        /// Start game timer
        /// </summary>
        public void StartGame()
        {
            gameStartTime = Time.time;
            isGameActive = true;
        }
        
        /// <summary>
        /// Stop game timer
        /// </summary>
        public void StopGame()
        {
            isGameActive = false;
        }
        
        /// <summary>
        /// Update turn display
        /// </summary>
        public void UpdateTurnDisplay(int currentTurn)
        {
            if (textTurn != null)
            {
                textTurn.text = $"Turn: {currentTurn}/{maxTurns}";
            }
        }
        
        /// <summary>
        /// Update time display
        /// </summary>
        public void UpdateTimeDisplay(float elapsedTime)
        {
            if (textTime != null && isGameActive)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                textTime.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        /// <summary>
        /// Update current player display
        /// </summary>
        public void UpdateCurrentPlayerDisplay(string playerName)
        {
            if (textCurrentPlayer != null)
            {
                textCurrentPlayer.text = $"Current: {playerName}";
            }
        }
        
        /// <summary>
        /// Update all displays
        /// </summary>
        public void UpdateAllDisplays(int currentTurn, string currentPlayerName)
        {
            UpdateTurnDisplay(currentTurn);
            
            if (isGameActive)
            {
                float elapsedTime = Time.time - gameStartTime;
                UpdateTimeDisplay(elapsedTime);
            }
            
            UpdateCurrentPlayerDisplay(currentPlayerName);
        }
        
        private void Update()
        {
            // Update time display every frame when game is active
            if (isGameActive)
            {
                float elapsedTime = Time.time - gameStartTime;
                UpdateTimeDisplay(elapsedTime);
            }
        }
        
        /// <summary>
        /// Set max turns
        /// </summary>
        public void SetMaxTurns(int turns)
        {
            maxTurns = turns;
        }
        
        /// <summary>
        /// Get current game time
        /// </summary>
        public float GetCurrentGameTime()
        {
            if (isGameActive)
            {
                return Time.time - gameStartTime;
            }
            return 0f;
        }
    }
}
