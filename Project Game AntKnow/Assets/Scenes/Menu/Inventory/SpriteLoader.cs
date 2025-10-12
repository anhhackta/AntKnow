using UnityEngine;
using UnityEngine.UI;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Helper class để load sprites từ Resources folder
    /// Hỗ trợ cả sprites ở root level và trong subfolders
    /// </summary>
    public static class SpriteLoader
    {
        /// <summary>
        /// Load sprite từ Resources folder
        /// Tự động tìm trong subfolders nếu không tìm thấy ở root
        /// </summary>
        /// <param name="iconPath">Path từ Firebase (ví dụ: "skill.bao-ke", "Cards/skill.bao-ke", "Equipment/equip.hat.basic")</param>
        /// <returns>Sprite hoặc null nếu không tìm thấy</returns>
        public static Sprite LoadSprite(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
                return null;

            // Try 1: Load trực tiếp với path từ Firebase
            Sprite sprite = Resources.Load<Sprite>(iconPath);
            if (sprite != null)
            {
                Debug.Log($"[SpriteLoader] ✅ Loaded sprite: {iconPath}");
                return sprite;
            }

            // Try 2: Nếu path có folder (ví dụ: "Cards/skill.bao-ke"), thử load ở root
            if (iconPath.Contains("/"))
            {
                string fileName = System.IO.Path.GetFileName(iconPath); // Lấy tên file
                sprite = Resources.Load<Sprite>(fileName);
                if (sprite != null)
                {
                    Debug.Log($"[SpriteLoader] ✅ Loaded sprite from root: {fileName} (original path: {iconPath})");
                    return sprite;
                }
            }

            // Try 3: Nếu path không có folder, thử trong các subfolders
            if (!iconPath.Contains("/"))
            {
                // Thử trong Cards/
                sprite = Resources.Load<Sprite>($"Cards/{iconPath}");
                if (sprite != null)
                {
                    Debug.Log($"[SpriteLoader] ✅ Loaded sprite from Cards/: Cards/{iconPath}");
                    return sprite;
                }

                // Thử trong Equipment/
                sprite = Resources.Load<Sprite>($"Equipment/{iconPath}");
                if (sprite != null)
                {
                    Debug.Log($"[SpriteLoader] ✅ Loaded sprite from Equipment/: Equipment/{iconPath}");
                    return sprite;
                }

                // Thử trong Items/
                sprite = Resources.Load<Sprite>($"Items/{iconPath}");
                if (sprite != null)
                {
                    Debug.Log($"[SpriteLoader] ✅ Loaded sprite from Items/: Items/{iconPath}");
                    return sprite;
                }
            }

            // Try 4: Tìm tất cả sprites có tên tương tự (fuzzy search)
            sprite = FindSpriteByName(iconPath);
            if (sprite != null)
            {
                Debug.Log($"[SpriteLoader] ✅ Found similar sprite: {sprite.name}");
                return sprite;
            }

            Debug.LogWarning($"[SpriteLoader] ❌ Sprite not found: {iconPath}");
            return null;
        }

        /// <summary>
        /// Load sprite và assign vào Image component
        /// </summary>
        public static void LoadSpriteToImage(Image targetImage, string iconPath)
        {
            if (targetImage == null)
                return;

            Sprite sprite = LoadSprite(iconPath);
            targetImage.sprite = sprite;
        }

        /// <summary>
        /// Tìm sprite có tên tương tự (fuzzy search)
        /// </summary>
        private static Sprite FindSpriteByName(string iconPath)
        {
            // Lấy tên file từ path
            string fileName = System.IO.Path.GetFileName(iconPath);
            
            // Load tất cả sprites trong Resources
            Sprite[] allSprites = Resources.LoadAll<Sprite>("");
            
            foreach (Sprite sprite in allSprites)
            {
                // So sánh tên file (không phân biệt hoa thường)
                if (string.Equals(sprite.name, fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            return null;
        }

        /// <summary>
        /// List tất cả sprites có sẵn trong Resources (debug helper)
        /// </summary>
        public static void ListAllSprites()
        {
            Sprite[] allSprites = Resources.LoadAll<Sprite>("");
            Debug.Log($"[SpriteLoader] Found {allSprites.Length} sprites in Resources:");
            
            foreach (Sprite sprite in allSprites)
            {
                Debug.Log($"  - {sprite.name} ({sprite.texture.width}x{sprite.texture.height})");
            }
        }

        /// <summary>
        /// Test load sprite với path cụ thể
        /// </summary>
        public static void TestSprite(string iconPath)
        {
            Debug.Log($"[SpriteLoader] Testing sprite: {iconPath}");
            Sprite sprite = LoadSprite(iconPath);
            
            if (sprite != null)
            {
                Debug.Log($"✅ SUCCESS: Loaded '{iconPath}' (Size: {sprite.texture.width}x{sprite.texture.height})");
            }
            else
            {
                Debug.LogError($"❌ FAILED: Sprite not found '{iconPath}'");
            }
        }
    }
}
