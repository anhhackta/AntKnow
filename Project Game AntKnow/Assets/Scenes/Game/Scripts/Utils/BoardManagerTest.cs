using UnityEngine;
using AntKnow.Game;

namespace AntKnow.Game.Utils
{
    /// <summary>
    /// Test script để kiểm tra BoardManager
    /// </summary>
    public class BoardManagerTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private bool runTestOnStart = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                TestBoardManager();
            }
        }

        [ContextMenu("Test BoardManager")]
        public void TestBoardManager()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (boardManager == null)
            {
                Debug.LogError("[BoardManagerTest] BoardManager not found!");
                return;
            }

            Debug.Log($"[BoardManagerTest] Testing BoardManager...");
            Debug.Log($"[BoardManagerTest] Total Tiles: {boardManager.TotalTiles}");

            // Test all tiles
            for (int i = 0; i < boardManager.TotalTiles; i++)
            {
                Vector3 pos = boardManager.GetWaypointPosition(i);
                TileType type = boardManager.GetTileType(i);
                string name = boardManager.GetTileName(i);
                int price = boardManager.GetTilePrice(i);

                Debug.Log($"[BoardManagerTest] Tile {i}: {name} (Type: {type}, Price: {price}, Pos: {pos})");
            }

            Debug.Log("[BoardManagerTest] ✅ Test completed!");
        }
    }
}
