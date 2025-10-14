using UnityEngine;
using UnityEngine.SceneManagement;
using AntKnow.Auth;

namespace AntKnow.Demo
{
    /// <summary>
    /// Demo Mode Manager - Single player demo without multiplayer
    /// Simulates a game session with AI opponents
    /// </summary>
    public class DemoModeManager : MonoBehaviour
    {
        [Header("Demo Settings")]
        [Tooltip("Number of AI opponents to spawn (1-3)")]
        public int aiPlayerCount = 3;
        
        [Tooltip("Auto start demo after this delay")]
        public float autoStartDelay = 2f;

        [Header("Demo Players")]
        public string[] demoPlayerNames = new string[]
        {
            "Bot Alpha",
            "Bot Beta", 
            "Bot Gamma"
        };

        private void Start()
        {
            Debug.Log("[DemoMode] ========== DEMO MODE STARTED ==========");
            Debug.Log($"[DemoMode] Spawning player + {aiPlayerCount} AI opponents");

            // Setup demo game session
            SetupDemoSession();

            // Start game after delay
            Invoke(nameof(StartDemoGame), autoStartDelay);
        }

        /// <summary>
        /// Setup demo game session data
        /// </summary>
        private void SetupDemoSession()
        {
            var gdm = GameDataManager.Instance;
            if (gdm == null)
            {
                Debug.LogError("[DemoMode] GameDataManager not found!");
                return;
            }

            // Ensure player data is loaded
            if (string.IsNullOrEmpty(gdm.currentUsername))
            {
                Debug.LogWarning("[DemoMode] Player data not loaded, using defaults");
                gdm.currentUsername = "Demo Player";
                gdm.currentAntCoin = 1000;
                gdm.currentDCoin = 100;
            }

            Debug.Log($"[DemoMode] Player: {gdm.currentUsername} (Coins: {gdm.currentAntCoin})");
        }

        /// <summary>
        /// Start the demo game
        /// </summary>
        private void StartDemoGame()
        {
            Debug.Log("[DemoMode] Starting demo game...");

            // TODO: Initialize game board
            // TODO: Spawn AI players
            // TODO: Start game loop

            // For now, just log
            Debug.Log("[DemoMode] ✅ Demo game started! (Connect GameManager here)");
        }

        /// <summary>
        /// Exit demo and return to menu
        /// </summary>
        public void ExitDemo()
        {
            Debug.Log("[DemoMode] Exiting demo mode...");
            SceneManager.LoadScene("MenuScene");
        }

        /// <summary>
        /// Restart demo
        /// </summary>
        public void RestartDemo()
        {
            Debug.Log("[DemoMode] Restarting demo...");
            SceneManager.LoadScene("DemoScene");
        }
    }
}
