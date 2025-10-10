using UnityEngine;
using AntKnow.Game;

namespace AntKnow.Game.Utils
{
    /// <summary>
    /// Test script để kiểm tra toàn bộ game
    /// </summary>
    public class GameTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private bool runTestOnStart = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                TestGame();
            }
        }

        [ContextMenu("Test Game")]
        public void TestGame()
        {
            Debug.Log("[GameTest] 🎮 Starting Game Test...");

            // Test GameManager
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (gameManager == null)
            {
                Debug.LogError("[GameTest] ❌ GameManager not found!");
                return;
            }

            Debug.Log("[GameTest] ✅ GameManager found");

            // Test BoardManager
            var boardManager = FindObjectOfType<BoardManager>();
            if (boardManager == null)
            {
                Debug.LogError("[GameTest] ❌ BoardManager not found!");
                return;
            }

            Debug.Log($"[GameTest] ✅ BoardManager found - Total Tiles: {boardManager.TotalTiles}");

            // Test PropertyManager
            var propertyManager = FindObjectOfType<PropertyManager>();
            if (propertyManager == null)
            {
                Debug.LogError("[GameTest] ❌ PropertyManager not found!");
                return;
            }

            Debug.Log("[GameTest] ✅ PropertyManager found");

            // Test DiceController
            var diceController = FindObjectOfType<DiceController>();
            if (diceController == null)
            {
                Debug.LogError("[GameTest] ❌ DiceController not found!");
                return;
            }

            Debug.Log("[GameTest] ✅ DiceController found");

            // Test UI Panels
            var panelBuy = FindObjectOfType<PanelBuy>();
            var panelQuiz = FindObjectOfType<PanelQuiz>();
            var panelEvent = FindObjectOfType<PanelEvent>();

            Debug.Log($"[GameTest] UI Panels - Buy: {(panelBuy != null ? "✅" : "❌")}, Quiz: {(panelQuiz != null ? "✅" : "❌")}, Event: {(panelEvent != null ? "✅" : "❌")}");

            Debug.Log("[GameTest] 🎉 Game Test completed! Ready to play!");
        }

        [ContextMenu("Start Demo Game")]
        public void StartDemoGame()
        {
            if (gameManager != null)
            {
                Debug.Log("[GameTest] 🚀 Starting Demo Game...");
                gameManager.StartGame();
            }
        }
    }
}
