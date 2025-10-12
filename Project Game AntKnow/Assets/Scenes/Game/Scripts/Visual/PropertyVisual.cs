using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Quản lý visual của properties (spawn houses/hotel trên tiles)
    /// Sử dụng TileVisual component trên mỗi tile
    /// </summary>
    public class PropertyVisual : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject housePrefab;
        [SerializeField] private GameObject hotelPrefab;

        [Header("Settings")]
        [SerializeField] private string roofMaterialName = "ngói"; // Tên material để đổi màu

        [Header("Player Colors")]
        [SerializeField] private Color[] playerColors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f),    // Red - Player 1
            new Color(0.2f, 0.5f, 1f),    // Blue - Player 2
            new Color(0.2f, 1f, 0.2f),    // Green - Player 3
            new Color(1f, 1f, 0.2f)       // Yellow - Player 4
        };

        [Header("Tiles")]
        [SerializeField] private TileSetup tileSetup; // Reference to Tiles GameObject

        private TileVisual[] tiles;

        private void Awake()
        {
            // Get tiles from TileSetup
            if (tileSetup != null)
            {
                tiles = tileSetup.GetAllTiles();
                Debug.Log($"[PropertyVisual] Got {tiles.Length} tiles from TileSetup");
            }
            else
            {
                // Fallback: Find TileSetup in scene
                tileSetup = FindObjectOfType<TileSetup>();
                if (tileSetup != null)
                {
                    tiles = tileSetup.GetAllTiles();
                    Debug.Log($"[PropertyVisual] Found TileSetup, got {tiles.Length} tiles");
                }
                else
                {
                    // Last resort: Find all TileVisual
                    tiles = FindObjectsOfType<TileVisual>();
                    Debug.Log($"[PropertyVisual] Found {tiles.Length} tiles (no TileSetup)");
                }
            }
        }
        
        /// <summary>
        /// Update property visual (houses + platform color + price)
        /// </summary>
        public void UpdatePropertyVisual(int tileId, int level, int ownerIndex, int rentPrice)
        {
            TileVisual tile = GetTile(tileId);
            if (tile == null)
            {
                Debug.LogWarning($"[PropertyVisual] Tile {tileId} not found!");
                return;
            }

            // Clear old houses
            tile.ClearHouses();

            Color playerColor = GetPlayerColor(ownerIndex);

            if (level == 0)
            {
                // Level 0 = empty land, no houses, but platform has color
                tile.SetPlatformColor(playerColor);
                tile.UpdatePrice(rentPrice, true); // Show rent price, isProperty = true
                return;
            }

            // Set platform color
            tile.SetPlatformColor(playerColor);

            // Update price to rent
            tile.UpdatePrice(rentPrice, true); // isProperty = true

            if (level >= 1 && level <= 4)
            {
                // Spawn houses (1-4)
                tile.SpawnHouses(housePrefab, level, playerColor, roofMaterialName);
            }
            else if (level == 5)
            {
                // Spawn hotel
                tile.SpawnHotel(hotelPrefab, playerColor, roofMaterialName);
            }
        }

        /// <summary>
        /// Reset property visual (when sold or not owned)
        /// </summary>
        public void ResetPropertyVisual(int tileId, int buyPrice)
        {
            TileVisual tile = GetTile(tileId);
            if (tile == null)
            {
                return;
            }

            // Clear houses
            tile.ClearHouses();

            // Reset platform color
            tile.ResetPlatformColor();

            // Reset price to buy price (isProperty = true)
            tile.UpdatePrice(buyPrice, true);
        }
        
        /// <summary>
        /// Get tile by index
        /// </summary>
        private TileVisual GetTile(int tileId)
        {
            if (tiles == null) return null;

            foreach (TileVisual tile in tiles)
            {
                if (tile != null && tile.tileIndex == tileId)
                {
                    return tile;
                }
            }

            return null;
        }

        /// <summary>
        /// Get player color
        /// </summary>
        private Color GetPlayerColor(int ownerIndex)
        {
            if (ownerIndex < 0 || ownerIndex >= playerColors.Length)
            {
                return Color.white;
            }

            return playerColors[ownerIndex];
        }

        /// <summary>
        /// Clear all property visuals
        /// </summary>
        public void ClearAll()
        {
            if (tiles == null) return;

            foreach (TileVisual tile in tiles)
            {
                if (tile != null)
                {
                    tile.ClearHouses();
                }
            }
        }
    }
}

