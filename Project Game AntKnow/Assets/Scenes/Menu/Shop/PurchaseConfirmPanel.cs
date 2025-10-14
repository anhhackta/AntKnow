using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace AntKnow.Shop
{
    /// <summary>
    /// Purchase confirmation panel
    /// Shows item details and confirm/cancel buttons
    /// </summary>
    public class PurchaseConfirmPanel : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panelRoot;
        public Image itemIconImage;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemDescriptionText;
        public TextMeshProUGUI priceText;
        public Image currencyIcon;
        public Button confirmButton;
        public Button cancelButton;
        public GameObject loadingIndicator;

        [Header("Quantity")]
        public GameObject quantityPanel;
        public TMP_InputField quantityInput;
        public Button increaseButton;
        public Button decreaseButton;

        private ShopItem currentItem;
        private int currentQuantity = 1;
        private bool isPurchasing = false;

        // Events
        public event Action<ShopItem, int> OnConfirmPurchase;

        private void Awake()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }

            if (increaseButton != null)
            {
                increaseButton.onClick.AddListener(() => SetQuantity(currentQuantity + 1));
            }

            if (decreaseButton != null)
            {
                decreaseButton.onClick.AddListener(() => SetQuantity(currentQuantity - 1));
            }

            if (quantityInput != null)
            {
                quantityInput.onEndEdit.AddListener(OnQuantityInputChanged);
            }

            // Start hidden
            Hide();
        }

        /// <summary>
        /// Show confirmation panel for item
        /// </summary>
        public void Show(ShopItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[PurchaseConfirmPanel] Cannot show panel with null item");
                return;
            }

            currentItem = item;
            currentQuantity = 1;
            isPurchasing = false;

            UpdateDisplay();

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        /// <summary>
        /// Hide confirmation panel
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            currentItem = null;
        }

        /// <summary>
        /// Update UI display
        /// </summary>
        private void UpdateDisplay()
        {
            if (currentItem == null) return;

            // Item name
            if (itemNameText != null)
            {
                itemNameText.text = currentItem.itemName;
            }

            // Description
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = currentItem.description;
            }

            // Price
            if (priceText != null)
            {
                int totalPrice = currentItem.price * currentQuantity;
                priceText.text = totalPrice.ToString();
            }

            // Icon
            if (itemIconImage != null && currentItem.iconImage != null)
            {
                itemIconImage.sprite = currentItem.iconImage.sprite;
            }

            // Quantity
            if (quantityInput != null)
            {
                quantityInput.text = currentQuantity.ToString();
            }

            // Loading indicator
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(isPurchasing);
            }

            // Buttons
            if (confirmButton != null)
            {
                confirmButton.interactable = !isPurchasing;
            }

            if (cancelButton != null)
            {
                cancelButton.interactable = !isPurchasing;
            }
        }

        /// <summary>
        /// Set quantity
        /// </summary>
        private void SetQuantity(int qty)
        {
            currentQuantity = Mathf.Max(1, qty);
            UpdateDisplay();
        }

        /// <summary>
        /// Handle quantity input change
        /// </summary>
        private void OnQuantityInputChanged(string value)
        {
            if (int.TryParse(value, out int qty))
            {
                SetQuantity(qty);
            }
            else
            {
                SetQuantity(1);
            }
        }

        /// <summary>
        /// Handle confirm button click
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            if (isPurchasing || currentItem == null) return;

            Debug.Log($"[PurchaseConfirmPanel] Confirm purchase: {currentItem.itemName} x{currentQuantity}");

            isPurchasing = true;
            UpdateDisplay();

            OnConfirmPurchase?.Invoke(currentItem, currentQuantity);
        }

        /// <summary>
        /// Handle cancel button click
        /// </summary>
        private void OnCancelButtonClicked()
        {
            if (isPurchasing) return;

            Debug.Log("[PurchaseConfirmPanel] Purchase canceled");
            Hide();
        }

        /// <summary>
        /// Show success message and close
        /// </summary>
        public void ShowSuccess()
        {
            isPurchasing = false;
            // TODO: Show success animation/message
            Debug.Log("[PurchaseConfirmPanel] Purchase successful!");
            Hide();
        }

        /// <summary>
        /// Show error message
        /// </summary>
        public void ShowError(string errorMessage)
        {
            isPurchasing = false;
            UpdateDisplay();

            // TODO: Show error message in UI
            Debug.LogError($"[PurchaseConfirmPanel] Purchase failed: {errorMessage}");
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }
        }
    }
}
