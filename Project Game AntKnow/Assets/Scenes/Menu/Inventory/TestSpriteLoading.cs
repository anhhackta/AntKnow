using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Test script để verify sprites có load được không
    /// Attach vào GameObject và chạy trong Play mode
    /// </summary>
    public class TestSpriteLoading : MonoBehaviour
    {
        [Header("Test Sprites")]
        [SerializeField] private bool testOnStart = true;
        
        private void Start()
        {
            if (testOnStart)
            {
                TestAllSprites();
            }
        }
        
        [ContextMenu("Test All Sprites")]
        public void TestAllSprites()
        {
            Debug.Log("=== Testing Sprite Loading ===");
            
            // Test equipment sprites
            TestSprite("Equipment/equip.hat.basic");
            TestSprite("Equipment/equip.shirt.basic");
            TestSprite("Equipment/equip.wings.basic");
            TestSprite("Equipment/equip.shoes.basic");
            TestSprite("Equipment/equip.mask.basic");
            
            // Test card sprites
            TestSprite("Cards/skill.lan-tron");
            TestSprite("Cards/skill.bao-ke");
            TestSprite("Cards/skill.cham-chi");
            TestSprite("Cards/skill.sieu-sale");
            
            // Test item sprites
            TestSprite("Items/exp.small");
            
            Debug.Log("=== Test Complete ===");
        }
        
        private void TestSprite(string iconPath)
        {
            Sprite sprite = Resources.Load<Sprite>(iconPath);
            
            if (sprite != null)
            {
                Debug.Log($"✅ SUCCESS: Loaded sprite '{iconPath}' (Size: {sprite.texture.width}x{sprite.texture.height})");
            }
            else
            {
                Debug.LogError($"❌ FAILED: Sprite not found '{iconPath}'\nCheck file: Assets/Resources/{iconPath}.png");
            }
        }
        
        [ContextMenu("List All Resources")]
        public void ListAllResources()
        {
            Debug.Log("=== Listing All Sprites in Resources ===");
            
            // Load all sprites from Resources
            Sprite[] allSprites = Resources.LoadAll<Sprite>("");
            
            Debug.Log($"Found {allSprites.Length} sprites in Resources folder:");
            
            foreach (var sprite in allSprites)
            {
                Debug.Log($"- {sprite.name}");
            }
            
            Debug.Log("=== List Complete ===");
        }
    }
}

