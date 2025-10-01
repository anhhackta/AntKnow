using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Slot chuyên dụng cho skill cards
    /// Kế thừa ItemSlot và thêm CardDisplay
    /// </summary>
    public class CardSlot : ItemSlot
    {
        [Header("Card Display")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text primaryStatText;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private Transform starsContainer;
        [SerializeField] private GameObject starPrefab;
        
        private List<GameObject> starObjects = new List<GameObject>();
        
        private void Start()
        {
            // Subscribe to item changed event
            OnItemChanged += OnCardChanged;
        }
        
        private void OnCardChanged(ItemSlot slot, InventoryItem item)
        {
            UpdateCardDisplay(item);
        }
        
        /// <summary>
        /// Update card display
        /// </summary>
        private void UpdateCardDisplay(InventoryItem card)
        {
            bool isEmpty = card == null || !card.IsSkillCard || card.itemData == null;
            
            // Hide all if empty
            if (isEmpty)
            {
                HideCardDisplay();
                return;
            }
            
            // Show card display
            ShowCardDisplay();
            
            
            // Level
            if (levelText != null)
            {
                levelText.text = $"Lv.{card.level}";
            }
            
            // Primary stat
            if (primaryStatText != null && card.itemData.attributes != null)
            {
                string primaryStat = card.itemData.attributes.primaryStat;
                int baseValue = GetAttributeValue(card.itemData.attributes, primaryStat);
                int perLevel = card.itemData.attributes.attributePerLevel;
                int totalValue = baseValue + (card.level - 1) * perLevel;
                
                primaryStatText.text = $"{GetStatDisplayName(primaryStat)}: {totalValue}";
            }
            
            // Cooldown
            if (cooldownText != null && card.itemData.skill != null)
            {
                int baseCooldown = card.itemData.skill.cooldownBaseTurns;
                int reduction = card.stars; // Mỗi sao giảm 1 turn
                int effectiveCooldown = Mathf.Max(1, baseCooldown - reduction);
                
                cooldownText.text = $"CD: {effectiveCooldown}";
            }
            
            // Stars
            UpdateStars(card.stars);

            // Card image
            if (cardImage != null)
            {
                cardImage.enabled = true;
                // Load sprite from Resources or Addressables
                LoadCardSprite(card.itemData.icon);
            }
        }

        /// <summary>
        /// Load card sprite từ Resources hoặc icon path
        /// </summary>
        private void LoadCardSprite(string iconPath)
        {
            if (cardImage == null || string.IsNullOrEmpty(iconPath))
                return;

            // iconPath từ Firestore: "Cards/skill.lan-tron"
            // Resources.Load sẽ tự động tìm file: Assets/Resources/Cards/skill.lan-tron.png
            Sprite sprite = Resources.Load<Sprite>(iconPath);

            if (sprite != null)
            {
                cardImage.sprite = sprite;
                Debug.Log($"[CardSlot] ✅ Loaded card sprite: {iconPath}");
            }
            else
            {
                Debug.LogWarning($"[CardSlot] ❌ Card sprite not found: {iconPath}\nCheck file: Assets/Resources/{iconPath}.png");
                cardImage.sprite = null;
            }
        }
        
        /// <summary>
        /// Hide card display elements
        /// </summary>
        private void HideCardDisplay()
        {
            if (cardImage != null) cardImage.enabled = false;
            if (levelText != null) levelText.gameObject.SetActive(false);
            if (primaryStatText != null) primaryStatText.gameObject.SetActive(false);
            if (cooldownText != null) cooldownText.gameObject.SetActive(false);
            ClearStars();
        }
        
        /// <summary>
        /// Show card display elements
        /// </summary>
        private void ShowCardDisplay()
        {
            if (cardImage != null) cardImage.enabled = true;
            if (levelText != null) levelText.gameObject.SetActive(true);
            if (primaryStatText != null) primaryStatText.gameObject.SetActive(true);
            if (cooldownText != null) cooldownText.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Update số sao hiển thị
        /// </summary>
        private void UpdateStars(int stars)
        {
            if (starsContainer == null || starPrefab == null)
                return;
            
            // Clear existing stars
            ClearStars();
            
            // Create new stars
            for (int i = 0; i < stars; i++)
            {
                GameObject star = Instantiate(starPrefab, starsContainer);
                star.SetActive(true);
                starObjects.Add(star);
            }
        }
        
        /// <summary>
        /// Clear all stars
        /// </summary>
        private void ClearStars()
        {
            foreach (var star in starObjects)
            {
                if (star != null)
                    Destroy(star);
            }
            starObjects.Clear();
        }
        
        /// <summary>
        /// Get attribute value by name
        /// </summary>
        private int GetAttributeValue(ItemAttributes attributes, string statName)
        {
            switch (statName)
            {
                case "health": return attributes.health;
                case "agility": return attributes.agility;
                case "intelligence": return attributes.intelligence;
                case "luck": return attributes.luck;
                case "resistance": return attributes.resistance;
                default: return 0;
            }
        }
        
        /// <summary>
        /// Get stat display name (localized)
        /// </summary>
        private string GetStatDisplayName(string statName)
        {
            switch (statName)
            {
                case "health": return "HP";
                case "agility": return "Agility";
                case "intelligence": return "Intelligence";
                case "luck": return "Luck";
                case "resistance": return "Resistance";
                default: return statName;
            }
        }
        
        private void OnDestroy()
        {
            OnItemChanged -= OnCardChanged;
        }
    }
}

