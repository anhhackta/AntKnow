using UnityEngine;
using UnityEditor;
using System.IO;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Editor script để organize sprites vào đúng folders
    /// Chạy trong Unity Editor để di chuyển sprites từ root vào subfolders
    /// </summary>
    public class OrganizeSprites : MonoBehaviour
    {
        [ContextMenu("Organize Sprites to Folders")]
        public void OrganizeSpritesToFolders()
        {
#if UNITY_EDITOR
            string resourcesPath = "Assets/Resources/";
            
            // Tạo folders nếu chưa có
            CreateFolderIfNotExists(resourcesPath + "Cards");
            CreateFolderIfNotExists(resourcesPath + "Equipment");
            CreateFolderIfNotExists(resourcesPath + "Items");
            
            // Move skill card sprites
            MoveSprite(resourcesPath + "skill.bao-ke.png", resourcesPath + "Cards/skill.bao-ke.png");
            MoveSprite(resourcesPath + "skill.cham-chi.png", resourcesPath + "Cards/skill.cham-chi.png");
            MoveSprite(resourcesPath + "skill.lan-tron.png", resourcesPath + "Cards/skill.lan-tron.png");
            MoveSprite(resourcesPath + "skill.sieu-sale.png", resourcesPath + "Cards/skill.sieu-sale.png");
            
            // Move equipment sprites
            MoveSprite(resourcesPath + "equip.hat.basic.png", resourcesPath + "Equipment/equip.hat.basic.png");
            MoveSprite(resourcesPath + "equip.mask.basic.png", resourcesPath + "Equipment/equip.mask.basic.png");
            MoveSprite(resourcesPath + "equip.shirt.basic.png", resourcesPath + "Equipment/equip.shirt.basic.png");
            MoveSprite(resourcesPath + "equip.shoes.basic.png", resourcesPath + "Equipment/equip.shoes.basic.png");
            MoveSprite(resourcesPath + "equip.wings.basic.png", resourcesPath + "Equipment/equip.wings.basic.png");
            
            // Move item sprites
            MoveSprite(resourcesPath + "exp.small.png", resourcesPath + "Items/exp.small.png");
            
            AssetDatabase.Refresh();
            Debug.Log("✅ Sprites organized successfully!");
#else
            Debug.LogWarning("This function only works in Unity Editor!");
#endif
        }
        
#if UNITY_EDITOR
        private void CreateFolderIfNotExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = Path.GetDirectoryName(folderPath).Replace("\\", "/");
                string folderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parentFolder, folderName);
                Debug.Log($"Created folder: {folderPath}");
            }
        }
        
        private void MoveSprite(string sourcePath, string targetPath)
        {
            if (File.Exists(sourcePath))
            {
                if (File.Exists(targetPath))
                {
                    Debug.LogWarning($"Target file already exists: {targetPath}");
                    return;
                }
                
                string error = AssetDatabase.MoveAsset(sourcePath, targetPath);
                if (string.IsNullOrEmpty(error))
                {
                    Debug.Log($"Moved: {sourcePath} → {targetPath}");
                }
                else
                {
                    Debug.LogError($"Failed to move {sourcePath}: {error}");
                }
            }
            else
            {
                Debug.LogWarning($"Source file not found: {sourcePath}");
            }
        }
#endif
        
        [ContextMenu("List Current Sprite Organization")]
        public void ListCurrentOrganization()
        {
            string resourcesPath = "Assets/Resources/";
            
            Debug.Log("=== CURRENT SPRITE ORGANIZATION ===");
            
            // Check root level
            string[] rootFiles = Directory.GetFiles(resourcesPath, "*.png");
            Debug.Log($"Root level sprites ({rootFiles.Length}):");
            foreach (string file in rootFiles)
            {
                string fileName = Path.GetFileName(file);
                Debug.Log($"  - {fileName}");
            }
            
            // Check Cards folder
            if (Directory.Exists(resourcesPath + "Cards"))
            {
                string[] cardFiles = Directory.GetFiles(resourcesPath + "Cards", "*.png");
                Debug.Log($"Cards folder sprites ({cardFiles.Length}):");
                foreach (string file in cardFiles)
                {
                    string fileName = Path.GetFileName(file);
                    Debug.Log($"  - {fileName}");
                }
            }
            
            // Check Equipment folder
            if (Directory.Exists(resourcesPath + "Equipment"))
            {
                string[] equipFiles = Directory.GetFiles(resourcesPath + "Equipment", "*.png");
                Debug.Log($"Equipment folder sprites ({equipFiles.Length}):");
                foreach (string file in equipFiles)
                {
                    string fileName = Path.GetFileName(file);
                    Debug.Log($"  - {fileName}");
                }
            }
            
            // Check Items folder
            if (Directory.Exists(resourcesPath + "Items"))
            {
                string[] itemFiles = Directory.GetFiles(resourcesPath + "Items", "*.png");
                Debug.Log($"Items folder sprites ({itemFiles.Length}):");
                foreach (string file in itemFiles)
                {
                    string fileName = Path.GetFileName(file);
                    Debug.Log($"  - {fileName}");
                }
            }
            
            Debug.Log("=== END LIST ===");
        }
    }
}
