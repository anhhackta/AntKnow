using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;

namespace AntKnow.Inventory
{
    /// <summary>
    /// UI Manager cho Inventory system
    /// </summary>
    public class InventoryUIManager : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject panelInventory;
        [SerializeField] private GameObject panelLoadout;
        
        [Header("Character Display")]
        [SerializeField] private Image characterImage;
        [SerializeField] private Sprite maleSprite;
        [SerializeField] private Sprite femaleSprite;
        
        [Header("Inventory Sub-Panels")]
        [SerializeField] private GameObject panelInventoryItem;
        [SerializeField] private GameObject panelInventoryCard;
        [SerializeField] private Button buttonShowItems;
        [SerializeField] private Button buttonShowCards;
        [SerializeField] private Button buttonSortItems;
        [SerializeField] private Button buttonSortCards;
        
        [Header("Inventory Item Slots (15 slots)")]
        [SerializeField] private Transform itemSlotsContainer;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private int maxItemSlots = 15;
        
        [Header("Inventory Card Slots (8 slots)")]
        [SerializeField] private Transform cardSlotsContainer;
        [SerializeField] private GameObject cardSlotPrefab;
        [SerializeField] private int maxCardSlots = 8;
        
        [Header("Loadout Equipment Slots (5 slots)")]
        [SerializeField] private ItemSlot hatSlot;
        [SerializeField] private ItemSlot shirtSlot;
        [SerializeField] private ItemSlot wingsSlot;
        [SerializeField] private ItemSlot shoesSlot;
        [SerializeField] private ItemSlot maskSlot;
        
        [Header("Loadout Card Slots")]
        [SerializeField] private ItemSlot passiveCardSlot;
        [SerializeField] private ItemSlot activeCardSlot;
        
        [Header("Services")]
        [SerializeField] private InventoryService inventoryService;
        [SerializeField] private FirebaseAuthService firebaseAuthService;

        [Header("Debug")]
        [SerializeField] private Button buttonTestLoad;

        private List<ItemSlot> itemSlots = new List<ItemSlot>();
        private List<ItemSlot> cardSlots = new List<ItemSlot>();

        private void Start()
        {
            Debug.Log("[InventoryUI] Start() called");
            InitializeUI();
            SetupEventListeners();
            LoadInventoryAndLoadout();
        }
        
        private void InitializeUI()
        {
            // Show items panel by default
            ShowItemsPanel();
            
            // Create item slots
            CreateSlots(itemSlotsContainer, itemSlotPrefab, maxItemSlots, itemSlots, SlotType.InventoryItem);
            
            // Create card slots
            CreateSlots(cardSlotsContainer, cardSlotPrefab, maxCardSlots, cardSlots, SlotType.InventoryCard);
        }
        
        private void CreateSlots(Transform container, GameObject prefab, int count, List<ItemSlot> slotList, SlotType slotType)
        {
            if (container == null || prefab == null)
            {
                Debug.LogError($"[InventoryUI] Container or prefab is null for {slotType}");
                return;
            }
            
            for (int i = 0; i < count; i++)
            {
                GameObject slotGO = Instantiate(prefab, container);
                slotGO.name = $"{slotType}_Slot_{i}";
                
                ItemSlot slot = slotGO.GetComponent<ItemSlot>();
                if (slot != null)
                {
                    slotList.Add(slot);
                }
            }
            
            Debug.Log($"[InventoryUI] Created {slotList.Count} slots for {slotType}");
        }
        
        private void SetupEventListeners()
        {
            // Panel switching buttons
            if (buttonShowItems != null)
                buttonShowItems.onClick.AddListener(ShowItemsPanel);

            if (buttonShowCards != null)
                buttonShowCards.onClick.AddListener(ShowCardsPanel);

            // Sort buttons
            if (buttonSortItems != null)
                buttonSortItems.onClick.AddListener(SortItems);

            if (buttonSortCards != null)
                buttonSortCards.onClick.AddListener(SortCards);

            // Debug button
            if (buttonTestLoad != null)
                buttonTestLoad.onClick.AddListener(TestLoadInventory);

            // Inventory service events
            if (inventoryService != null)
            {
                InventoryService.OnInventoryLoaded += OnInventoryLoaded;
                InventoryService.OnLoadoutLoaded += OnLoadoutLoaded;
            }

            // Loadout slot events (auto save khi thay đổi)
            if (hatSlot != null) hatSlot.OnItemChanged += OnLoadoutSlotChanged;
            if (shirtSlot != null) shirtSlot.OnItemChanged += OnLoadoutSlotChanged;
            if (wingsSlot != null) wingsSlot.OnItemChanged += OnLoadoutSlotChanged;
            if (shoesSlot != null) shoesSlot.OnItemChanged += OnLoadoutSlotChanged;
            if (maskSlot != null) maskSlot.OnItemChanged += OnLoadoutSlotChanged;
            if (passiveCardSlot != null) passiveCardSlot.OnItemChanged += OnLoadoutSlotChanged;
            if (activeCardSlot != null) activeCardSlot.OnItemChanged += OnLoadoutSlotChanged;
        }
        
        private async void LoadInventoryAndLoadout()
        {
            // Check if user is logged in
            if (firebaseAuthService == null || firebaseAuthService.Auth == null || firebaseAuthService.Auth.CurrentUser == null)
            {
                Debug.LogWarning("[InventoryUI] User not logged in! Redirecting to LoginScene...");

                // Redirect to LoginScene
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
                return;
            }

            string uid = firebaseAuthService.Auth.CurrentUser.UserId;
            Debug.Log($"[InventoryUI] Loading inventory and loadout for user: {uid}");

            // Load inventory and loadout in parallel (FASTER!)
            var inventoryTask = inventoryService.LoadInventoryAsync(uid);
            var loadoutTask = inventoryService.LoadLoadoutAsync(uid);

            await System.Threading.Tasks.Task.WhenAll(inventoryTask, loadoutTask);

            Debug.Log("[InventoryUI] Inventory and loadout loaded successfully!");

            // Update character image
            UpdateCharacterImage();
        }
        
        private void OnInventoryLoaded(List<InventoryItem> inventory)
        {
            Debug.Log($"[InventoryUI] Inventory loaded: {inventory.Count} items");
            RefreshInventoryDisplay();
        }
        
        private void OnLoadoutLoaded(LoadoutData loadout)
        {
            Debug.Log($"[InventoryUI] Loadout loaded");
            RefreshLoadoutDisplay();
        }
        
        private void RefreshInventoryDisplay()
        {
            var inventory = inventoryService.GetCachedInventory();
            
            // Separate items and cards
            var items = inventory.Where(i => !i.IsSkillCard).ToList();
            var cards = inventory.Where(i => i.IsSkillCard).ToList();
            
            // Fill item slots
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (i < items.Count)
                {
                    itemSlots[i].SetItem(items[i]);
                }
                else
                {
                    itemSlots[i].ClearItem();
                }
            }
            
            // Fill card slots
            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (i < cards.Count)
                {
                    cardSlots[i].SetItem(cards[i]);
                }
                else
                {
                    cardSlots[i].ClearItem();
                }
            }
        }
        
        private void RefreshLoadoutDisplay()
        {
            var loadout = inventoryService.GetCachedLoadout();
            var inventory = inventoryService.GetCachedInventory();
            
            if (loadout == null)
                return;
            
            // Load equipment slots
            LoadEquipmentSlot(hatSlot, loadout.equipmentSet.hatId, inventory);
            LoadEquipmentSlot(shirtSlot, loadout.equipmentSet.shirtId, inventory);
            LoadEquipmentSlot(wingsSlot, loadout.equipmentSet.wingsId, inventory);
            LoadEquipmentSlot(shoesSlot, loadout.equipmentSet.shoesId, inventory);
            LoadEquipmentSlot(maskSlot, loadout.equipmentSet.maskId, inventory);
            
            // Load card slots
            if (loadout.skillCardIds.Count > 0)
            {
                var passiveCard = inventory.FirstOrDefault(i => i.docId == loadout.skillCardIds[0]);
                if (passiveCardSlot != null)
                    passiveCardSlot.SetItem(passiveCard);
            }
            
            if (loadout.skillCardIds.Count > 1)
            {
                var activeCard = inventory.FirstOrDefault(i => i.docId == loadout.skillCardIds[1]);
                if (activeCardSlot != null)
                    activeCardSlot.SetItem(activeCard);
            }
        }
        
        private void LoadEquipmentSlot(ItemSlot slot, string docId, List<InventoryItem> inventory)
        {
            if (slot == null)
                return;
            
            if (string.IsNullOrEmpty(docId))
            {
                slot.ClearItem();
            }
            else
            {
                var item = inventory.FirstOrDefault(i => i.docId == docId);
                slot.SetItem(item);
            }
        }
        
        private void UpdateCharacterImage()
        {
            // TODO: Get gender from UserData
            // For now, use male sprite
            if (characterImage != null && maleSprite != null)
            {
                characterImage.sprite = maleSprite;
            }
        }
        
        private void ShowItemsPanel()
        {
            if (panelInventoryItem != null)
                panelInventoryItem.SetActive(true);
            
            if (panelInventoryCard != null)
                panelInventoryCard.SetActive(false);
        }
        
        private void ShowCardsPanel()
        {
            if (panelInventoryItem != null)
                panelInventoryItem.SetActive(false);
            
            if (panelInventoryCard != null)
                panelInventoryCard.SetActive(true);
        }
        
        private void SortItems()
        {
            Debug.Log("[InventoryUI] Sorting items...");
            
            // Get all items from slots
            var items = new List<InventoryItem>();
            foreach (var slot in itemSlots)
            {
                var item = slot.GetItem();
                if (item != null)
                    items.Add(item);
            }
            
            // Sort: non-null items first, then by type, then by rarity
            items = items.OrderBy(i => i.type)
                        .ThenBy(i => i.itemData?.rarity)
                        .ToList();
            
            // Refill slots
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (i < items.Count)
                {
                    itemSlots[i].SetItem(items[i]);
                }
                else
                {
                    itemSlots[i].ClearItem();
                }
            }
        }
        
        private void SortCards()
        {
            Debug.Log("[InventoryUI] Sorting cards...");
            
            // Get all cards from slots
            var cards = new List<InventoryItem>();
            foreach (var slot in cardSlots)
            {
                var card = slot.GetItem();
                if (card != null)
                    cards.Add(card);
            }
            
            // Sort: by rarity, then by level, then by stars
            cards = cards.OrderBy(c => c.itemData?.rarity)
                        .ThenByDescending(c => c.level)
                        .ThenByDescending(c => c.stars)
                        .ToList();
            
            // Refill slots
            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (i < cards.Count)
                {
                    cardSlots[i].SetItem(cards[i]);
                }
                else
                {
                    cardSlots[i].ClearItem();
                }
            }
        }
        
        /// <summary>
        /// Callback khi loadout slot thay đổi → Auto save
        /// </summary>
        private async void OnLoadoutSlotChanged(ItemSlot slot, InventoryItem item)
        {
            Debug.Log($"[InventoryUI] Loadout slot changed: {slot.name}");
            await SaveCurrentLoadout();
        }

        /// <summary>
        /// Save loadout hiện tại lên Firestore
        /// </summary>
        private async System.Threading.Tasks.Task SaveCurrentLoadout()
        {
            if (firebaseAuthService == null || firebaseAuthService.Auth == null || firebaseAuthService.Auth.CurrentUser == null)
            {
                Debug.LogError("[InventoryUI] Cannot save loadout: User not logged in!");
                return;
            }

            string uid = firebaseAuthService.Auth.CurrentUser.UserId;

            // Build LoadoutData từ UI
            var loadout = new LoadoutData();

            // Get equipment IDs
            loadout.equipmentSet.hatId = hatSlot?.GetItem()?.docId;
            loadout.equipmentSet.shirtId = shirtSlot?.GetItem()?.docId;
            loadout.equipmentSet.wingsId = wingsSlot?.GetItem()?.docId;
            loadout.equipmentSet.shoesId = shoesSlot?.GetItem()?.docId;
            loadout.equipmentSet.maskId = maskSlot?.GetItem()?.docId;

            // Get card IDs
            loadout.skillCardIds.Clear();
            if (passiveCardSlot?.GetItem() != null)
                loadout.skillCardIds.Add(passiveCardSlot.GetItem().docId);
            if (activeCardSlot?.GetItem() != null)
                loadout.skillCardIds.Add(activeCardSlot.GetItem().docId);

            // Save to Firestore
            bool success = await inventoryService.SaveLoadoutAsync(uid, loadout);

            if (success)
            {
                Debug.Log("[InventoryUI] Loadout saved successfully!");
            }
            else
            {
                Debug.LogError("[InventoryUI] Failed to save loadout!");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            if (inventoryService != null)
            {
                InventoryService.OnInventoryLoaded -= OnInventoryLoaded;
                InventoryService.OnLoadoutLoaded -= OnLoadoutLoaded;
            }

            // Unsubscribe loadout slot events
            if (hatSlot != null) hatSlot.OnItemChanged -= OnLoadoutSlotChanged;
            if (shirtSlot != null) shirtSlot.OnItemChanged -= OnLoadoutSlotChanged;
            if (wingsSlot != null) wingsSlot.OnItemChanged -= OnLoadoutSlotChanged;
            if (shoesSlot != null) shoesSlot.OnItemChanged -= OnLoadoutSlotChanged;
            if (maskSlot != null) maskSlot.OnItemChanged -= OnLoadoutSlotChanged;
            if (passiveCardSlot != null) passiveCardSlot.OnItemChanged -= OnLoadoutSlotChanged;
            if (activeCardSlot != null) activeCardSlot.OnItemChanged -= OnLoadoutSlotChanged;
        }

        // ===== DEBUG METHODS =====

        [ContextMenu("Test Load Inventory")]
        private async void TestLoadInventory()
        {
            Debug.Log("=== TEST LOAD INVENTORY ===");

            if (firebaseAuthService == null)
            {
                Debug.LogError("❌ FirebaseAuthService is null!");
                return;
            }

            if (firebaseAuthService.Auth == null)
            {
                Debug.LogError("❌ Firebase Auth is null!");
                return;
            }

            if (firebaseAuthService.Auth.CurrentUser == null)
            {
                Debug.LogError("❌ Current User is null! Please login first.");
                return;
            }

            string uid = firebaseAuthService.Auth.CurrentUser.UserId;
            Debug.Log($"✅ User ID: {uid}");

            // Load inventory
            Debug.Log("Loading inventory...");
            var inventory = await inventoryService.LoadInventoryAsync(uid);
            Debug.Log($"✅ Loaded {inventory.Count} items");

            foreach (var item in inventory)
            {
                Debug.Log($"  - Item: {item.itemId}, Type: {item.type}, DocId: {item.docId}");
                if (item.itemData != null)
                {
                    Debug.Log($"    ✅ ItemData: Icon={item.itemData.icon}, Name={item.itemData.name}");
                }
                else
                {
                    Debug.LogError($"    ❌ ItemData is NULL! Check if items/{item.itemId} exists in Firestore!");
                }
            }

            // Load loadout
            Debug.Log("Loading loadout...");
            var loadout = await inventoryService.LoadLoadoutAsync(uid);
            Debug.Log($"✅ Loadout: {loadout.skillCardIds.Count} cards, {loadout.equipmentSet.GetAllEquipmentIds().Count} equipment");

            var equipIds = loadout.equipmentSet.GetAllEquipmentIds();
            foreach (var equipId in equipIds)
            {
                Debug.Log($"  - Equipment ID: {equipId}");
            }

            foreach (var cardId in loadout.skillCardIds)
            {
                Debug.Log($"  - Card ID: {cardId}");
            }

            Debug.Log("=== TEST COMPLETE ===");
        }
    }
}

