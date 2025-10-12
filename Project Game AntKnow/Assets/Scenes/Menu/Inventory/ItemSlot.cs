using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Slot chứa item (trong inventory hoặc loadout)
    /// Chỉ quản lý UI và validation, KHÔNG quản lý drag & drop
    /// </summary>
    public class ItemSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private GameObject emptyIndicator;

        [Header("Settings")]
        [SerializeField] private SlotType slotType = SlotType.InventoryItem;
        [SerializeField] private string equipmentSlot; // Chỉ dùng cho LoadoutEquipment: "hat", "shirt", "wings", "shoes", "mask"

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color filledColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;

        // Events
        public event Action<ItemSlot, InventoryItem> OnItemChanged;

        // Data
        private InventoryItem currentItem;
        private GameObject itemVisualObject;
        
        private void Awake()
        {
            UpdateVisual();
        }
        
        /// <summary>
        /// Set item vào slot
        /// </summary>
        public void SetItem(InventoryItem item)
        {
            currentItem = item;

            // Clear old visual
            if (itemVisualObject != null)
            {
                Destroy(itemVisualObject);
                itemVisualObject = null;
            }

            // Create new visual if item exists
            if (item != null)
            {
                CreateItemVisual(item);
            }

            UpdateVisual();

            // Trigger event
            OnItemChanged?.Invoke(this, item);
        }

        /// <summary>
        /// Tạo visual cho item (icon + draggable)
        /// </summary>
        private void CreateItemVisual(InventoryItem item)
        {
            // Create item visual GameObject
            itemVisualObject = new GameObject("ItemVisual");
            itemVisualObject.transform.SetParent(transform);

            var rectTransform = itemVisualObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup for drag transparency
            itemVisualObject.AddComponent<CanvasGroup>();

            // Add Image for icon
            var image = itemVisualObject.AddComponent<Image>();
            image.raycastTarget = true;

            // Load sprite from Resources
            if (item.itemData != null && !string.IsNullOrEmpty(item.itemData.icon))
            {
                LoadItemSprite(image, item.itemData.icon);
            }

            // Add DraggableItem component
            var draggable = itemVisualObject.AddComponent<DraggableItem>();
            draggable.SetItem(item);
            draggable.sourceSlot = this;
        }

        /// <summary>
        /// Load item sprite từ Resources
        /// </summary>
        private void LoadItemSprite(Image targetImage, string iconPath)
        {
            if (targetImage == null || string.IsNullOrEmpty(iconPath))
                return;

            // Sử dụng SpriteLoader để load sprite một cách linh hoạt
            SpriteLoader.LoadSpriteToImage(targetImage, iconPath);
        }
        
        /// <summary>
        /// Get item từ slot
        /// </summary>
        public InventoryItem GetItem()
        {
            return currentItem;
        }
        
        /// <summary>
        /// Clear item khỏi slot
        /// </summary>
        public void ClearItem()
        {
            SetItem(null);
        }
        
        /// <summary>
        /// Check xem slot có thể nhận item này không
        /// </summary>
        public bool CanAcceptItem(InventoryItem item)
        {
            if (item == null)
                return false;

            switch (slotType)
            {
                case SlotType.InventoryItem:
                    // Inventory item slots accept all items
                    return true;

                case SlotType.InventoryCard:
                    // Inventory card slots only accept skill cards
                    return item.IsSkillCard;

                case SlotType.LoadoutEquipment:
                    // Loadout equipment slots only accept equipment of matching slot
                    if (!item.IsEquipment)
                        return false;

                    if (item.itemData == null || item.itemData.equipment == null)
                        return false;

                    return item.itemData.equipment.slot == equipmentSlot;

                case SlotType.LoadoutCard:
                    // Loadout card slots only accept skill cards
                    if (!item.IsSkillCard)
                        return false;

                    // Check duplicate: Không cho 2 card cùng itemId trong loadout
                    return !IsCardDuplicateInLoadout(item);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Check xem card này đã có trong loadout chưa (không cho duplicate)
        /// </summary>
        private bool IsCardDuplicateInLoadout(InventoryItem item)
        {
            if (item == null || !item.IsSkillCard)
                return false;

            // Find all LoadoutCard slots
            var loadoutCardSlots = FindObjectsOfType<ItemSlot>();

            foreach (var slot in loadoutCardSlots)
            {
                // Skip this slot
                if (slot == this)
                    continue;

                // Only check LoadoutCard slots
                if (slot.slotType != SlotType.LoadoutCard)
                    continue;

                var slotItem = slot.GetItem();
                if (slotItem != null && slotItem.itemId == item.itemId)
                {
                    Debug.LogWarning($"[ItemSlot] Card duplicate detected: {item.itemData?.name} already in loadout!");
                    return true; // Duplicate found
                }
            }

            return false; // No duplicate
        }
        
        /// <summary>
        /// Update visual của slot
        /// </summary>
        private void UpdateVisual()
        {
            bool isEmpty = currentItem == null;
            
            // Update background
            if (backgroundImage != null)
            {
                backgroundImage.color = isEmpty ? emptyColor : filledColor;
            }
            
            // Update empty indicator
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(isEmpty);
            }
            
            // Update icon
            if (iconImage != null)
            {
                iconImage.enabled = !isEmpty;
                if (!isEmpty && currentItem.itemData != null)
                {
                    // TODO: Load sprite from currentItem.itemData.icon
                }
            }
            
            // Update quantity text (for stackable items)
            if (quantityText != null)
            {
                if (!isEmpty && currentItem.IsStackable && currentItem.qty > 1)
                {
                    quantityText.gameObject.SetActive(true);
                    quantityText.text = currentItem.qty.ToString();
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Highlight slot
        /// </summary>
        public void Highlight(bool highlight)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = highlight ? highlightColor : (currentItem == null ? emptyColor : filledColor);
            }
        }
    }
    
    public enum SlotType
    {
        InventoryItem,      // Slot trong PanelInventoryItem (chấp nhận tất cả items)
        InventoryCard,      // Slot trong PanelInventoryCard (chỉ chấp nhận skill cards)
        LoadoutEquipment,   // Slot trong LoadoutItems (chỉ chấp nhận equipment phù hợp)
        LoadoutCard         // Slot trong LoadoutCard (chỉ chấp nhận skill cards)
    }
}


