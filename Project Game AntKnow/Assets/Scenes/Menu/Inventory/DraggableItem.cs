using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AntKnow.Inventory
{
    /// <summary>
    /// Component cho phép drag & drop item/card
    /// </summary>
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image image;
        
        [Header("Settings")]
        [SerializeField] private float dragAlpha = 0.6f;
        
        // Data
        public InventoryItem inventoryItem;
        public ItemSlot sourceSlot;
        
        // Drag state
        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private Transform originalParent;
        private int originalSiblingIndex;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (image == null)
                image = GetComponent<Image>();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"[DraggableItem] Begin drag: {inventoryItem?.itemId}");
            
            // Save original state
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            
            // Move to canvas root để render trên tất cả UI
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
            
            // Make semi-transparent
            if (canvasGroup != null)
            {
                canvasGroup.alpha = dragAlpha;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // Follow mouse/touch
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"[DraggableItem] End drag: {inventoryItem?.itemId}");
            
            // Restore alpha
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
            
            // Check if dropped on a valid slot
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            ItemSlot targetSlot = null;
            foreach (var result in results)
            {
                targetSlot = result.gameObject.GetComponent<ItemSlot>();
                if (targetSlot != null && targetSlot != sourceSlot)
                    break;
            }
            
            if (targetSlot != null && targetSlot.CanAcceptItem(inventoryItem))
            {
                // Valid drop
                Debug.Log($"[DraggableItem] Dropped on valid slot: {targetSlot.name}");
                
                // Swap items
                var targetItem = targetSlot.GetItem();
                
                // Move this item to target slot
                targetSlot.SetItem(inventoryItem);
                transform.SetParent(targetSlot.transform);
                rectTransform.anchoredPosition = Vector2.zero;
                sourceSlot.ClearItem();
                
                // Move target item to source slot (if exists)
                if (targetItem != null && sourceSlot.CanAcceptItem(targetItem))
                {
                    sourceSlot.SetItem(targetItem);
                }
                
                // Update source slot reference
                sourceSlot = targetSlot;
            }
            else
            {
                // Invalid drop - return to original position
                Debug.Log($"[DraggableItem] Invalid drop, returning to original position");
                transform.SetParent(originalParent);
                transform.SetSiblingIndex(originalSiblingIndex);
                rectTransform.anchoredPosition = originalPosition;
            }
        }
        
        public void SetItem(InventoryItem item)
        {
            inventoryItem = item;
            
            // Update visual
            if (image != null && item != null && item.itemData != null)
            {
                // TODO: Load sprite from item.itemData.icon
                // For now, just enable the image
                image.enabled = true;
            }
        }
    }
}

