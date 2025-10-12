using UnityEngine;

namespace AntKnow.Game
{
    /// <summary>
    /// ScriptableObject chứa VISUAL DATA của từng tile
    /// Chỉ chứa: locationName, locationImage, description
    ///
    /// KHÔNG chứa giá (prices) - Giá lấy từ SimpleBoardConfig
    /// Lý do: Single source of truth - tránh duplicate data
    /// </summary>
    [CreateAssetMenu(fileName = "TileData", menuName = "AntKnow/Tile Data", order = 1)]
    public class TileData : ScriptableObject
    {
        [Header("Visual Info")]
        [Tooltip("Tên địa danh (ví dụ: Tokyo, Paris, New York)")]
        public string locationName;

        [Tooltip("Hình ảnh địa danh (ví dụ: Tokyo Tower, Eiffel Tower)")]
        public Sprite locationImage;

        [Tooltip("Mô tả ngắn về địa danh")]
        [TextArea(2, 4)]
        public string description;

        [Header("Info")]
        [Tooltip("Tile index (0-35) - Dùng để map với SimpleBoardConfig")]
        public int tileIndex = -1;

        /// <summary>
        /// Validate tile data
        /// </summary>
        private void OnValidate()
        {
            if (tileIndex < 0 || tileIndex >= 36)
            {
                Debug.LogWarning($"[TileData] {name}: tileIndex {tileIndex} out of range (0-35)");
            }
        }
    }
}

