# ⏱️ KẾ HOẠCH 5-7 GIỜ - HOÀN THIỆN GAME CƠ BẢN

## 🎯 MỤC TIÊU TỔNG QUAN

**Flow hoàn chỉnh:**
```
Login → Menu Scene → Inventory/Loadout → Shop → Matchmaking/Lobby → Game → End Game → Menu Scene
```

**Yêu cầu:**
1. ✅ Inventory hiển thị items + skill cards
2. ✅ Loadout kéo thả equipment (5 slots) + skill cards (2 slots)
3. ✅ Stats tính toán đúng (base + equipment + cards)
4. ✅ Shop mua skill cards + items (trừ tiền)
5. ✅ Matchmaking/Lobby tìm trận
6. ✅ Game chạy được (multiplayer)
7. ✅ End game cộng tiền thưởng + XP + level
8. ✅ Quay về Menu Scene

---

## 📊 PHÂN TÍCH HIỆN TRẠNG

### ✅ ĐÃ CÓ (90%)
- ✅ **InventoryService** - Load/Save inventory + loadout
- ✅ **InventoryUIManager** - Hiển thị inventory + loadout
- ✅ **ItemSlot** - Slot UI cho items
- ✅ **LoadoutStatsDisplay** - Tính toán stats
- ✅ **GameManager** - Game logic hoàn chỉnh
- ✅ **LobbyUIManager** - Lobby system
- ✅ **GameSessionData** - Transfer data Menu → Game
- ✅ **Cloud Functions** - purchaseItem, awardMatch

### ⚠️ CẦN LÀM (10%)
- [ ] **Drag & Drop** - Kéo thả items vào loadout
- [ ] **ShopUI** - Panel shop mua items/cards
- [ ] **End Game Rewards** - Cộng tiền + XP + level
- [ ] **Scene Flow** - Game → Menu Scene
- [ ] **Bug fixes** - Test và fix lỗi

---

## 🕐 KẾ HOẠCH CHI TIẾT

### **PHASE 1: INVENTORY & LOADOUT (1.5 giờ)**

#### **Task 1.1: Drag & Drop System** (45 phút)
**File:** `Assets/Scenes/Menu/Inventory/DragDropHandler.cs` (NEW)

**Chức năng:**
- Kéo item từ Inventory → Loadout Equipment (5 slots)
- Kéo skill card từ Inventory → Loadout Cards (2 slots)
- Validation: Đúng slot type (hat → hat slot, etc.)
- Swap items giữa các slots
- Unequip: Kéo từ Loadout → Inventory

**Implementation:**
```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ItemSlot sourceSlot;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Start drag
        sourceSlot = GetComponent<ItemSlot>();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Follow mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Drop
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Check if dropped on valid slot
        var targetSlot = eventData.pointerEnter?.GetComponent<ItemSlot>();
        if (targetSlot != null && ValidateDrop(sourceSlot, targetSlot))
        {
            SwapItems(sourceSlot, targetSlot);
        }
    }
    
    private bool ValidateDrop(ItemSlot source, ItemSlot target)
    {
        // Validate slot type
        // Equipment → Equipment slot (same type)
        // Skill card → Card slot
        return true; // Implement validation
    }
    
    private void SwapItems(ItemSlot source, ItemSlot target)
    {
        var sourceItem = source.GetItem();
        var targetItem = target.GetItem();
        
        source.SetItem(targetItem);
        target.SetItem(sourceItem);
        
        // Save loadout
        InventoryUIManager.Instance.SaveLoadout();
    }
}
```

**Cần làm:**
- [ ] Tạo DragDropHandler.cs
- [ ] Add component vào ItemSlot prefab
- [ ] Test drag & drop
- [ ] Test validation (đúng slot type)

---

#### **Task 1.2: Fix Stats Calculation** (30 phút)
**File:** `Assets/Scenes/Menu/Inventory/LoadoutStatsDisplay.cs`

**Vấn đề hiện tại:**
- Skill cards chưa có effectId
- Cần lấy effectId từ itemData.skill

**Fix:**
```csharp
private void AddCardStats(TotalStats stats, InventoryItem card)
{
    if (card == null || card.itemData == null) return;
    
    var attr = card.itemData.attributes;
    string primaryStat = attr.primaryStat;
    
    // Get base value
    int baseValue = GetAttributeValue(attr, primaryStat);
    
    // Calculate với level scaling
    int totalValue = baseValue + (card.level - 1) * attr.attributePerLevel;
    
    // Add to stats
    switch (primaryStat)
    {
        case "health": stats.health += totalValue; break;
        case "agility": stats.agility += totalValue; break;
        case "intelligence": stats.intelligence += totalValue; break;
        case "luck": stats.luck += totalValue; break;
        case "resistance": stats.resistance += totalValue; break;
    }
    
    Debug.Log($"[LoadoutStats] Card {card.itemData.name} Lv.{card.level}: {primaryStat}+{totalValue}");
}
```

**Cần làm:**
- [ ] Fix AddCardStats() method
- [ ] Test stats calculation
- [ ] Verify với Firebase data

---

#### **Task 1.3: Save Loadout to GameSessionData** (15 phút)
**File:** `Assets/Scenes/Menu/Inventory/InventoryUIManager.cs`

**Thêm method:**
```csharp
public void PrepareGameSession()
{
    var sessionData = GameSessionData.Instance;
    var loadout = inventoryService.GetCachedLoadout();
    var inventory = inventoryService.GetCachedInventory();
    
    // Get skill cards
    sessionData.skillCards.Clear();
    foreach (var cardId in loadout.skillCardIds)
    {
        var card = inventory.FirstOrDefault(i => i.docId == cardId);
        if (card != null && card.itemData != null)
        {
            sessionData.skillCards.Add(new SkillCardData
            {
                docId = card.docId,
                itemId = card.itemId,
                effectId = card.itemData.skill?.effectId ?? "",
                level = card.level,
                stars = card.stars
            });
        }
    }
    
    // Get equipment stats
    var stats = loadoutStatsDisplay.CalculateTotalStats();
    sessionData.totalHealth = stats.health;
    sessionData.totalAgility = stats.agility;
    sessionData.totalIntelligence = stats.intelligence;
    sessionData.totalLuck = stats.luck;
    sessionData.totalResistance = stats.resistance;
    
    Debug.Log($"[InventoryUI] GameSessionData prepared - HP:{stats.health} AGI:{stats.agility} INT:{stats.intelligence} LUCK:{stats.luck} RES:{stats.resistance}");
}
```

**Cần làm:**
- [ ] Add PrepareGameSession() method
- [ ] Call trước khi vào game
- [ ] Test data transfer

---

### **PHASE 2: SHOP SYSTEM (1.5 giờ)**

#### **Task 2.1: ShopUI Panel** (45 phút)
**File:** `Assets/Scenes/Menu/Shop/ShopUIManager.cs` (NEW)

**UI Structure:**
```
PanelShop
├── Header (Title, Close button)
├── Currency Display (AntCoin, DCoin)
├── Tabs (Items, Skill Cards)
├── Shop Grid (ScrollView)
│   └── ShopItem Prefab (Icon, Name, Price, Buy button)
└── Confirmation Popup
```

**Implementation:**
```csharp
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopGrid;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Text antCoinText;
    [SerializeField] private Text dCoinText;
    
    [Header("Tabs")]
    [SerializeField] private Button tabItems;
    [SerializeField] private Button tabCards;
    
    private FirebaseFirestore firestore;
    private string currentTab = "items";
    
    public async void OpenShop()
    {
        shopPanel.SetActive(true);
        await LoadShopItems();
    }
    
    private async Task LoadShopItems()
    {
        // Load from Firestore: shops/default/entries
        var entriesRef = firestore.Collection("shops").Document("default").Collection("entries");
        var snapshot = await entriesRef.GetSnapshotAsync();
        
        // Clear grid
        foreach (Transform child in shopGrid)
        {
            Destroy(child.gameObject);
        }
        
        // Create shop items
        foreach (var doc in snapshot.Documents)
        {
            var entry = doc.ToDictionary();
            string type = entry["type"].ToString();
            
            // Filter by tab
            if (currentTab == "items" && type == "equipment") continue;
            if (currentTab == "cards" && type != "skill_card") continue;
            
            CreateShopItem(doc.Id, entry);
        }
    }
    
    private void CreateShopItem(string entryId, Dictionary<string, object> entry)
    {
        var itemObj = Instantiate(shopItemPrefab, shopGrid);
        var shopItem = itemObj.GetComponent<ShopItem>();
        
        string itemId = entry["itemId"].ToString();
        int priceAntCoin = Convert.ToInt32(entry["priceAntCoin"]);
        
        shopItem.Setup(entryId, itemId, priceAntCoin, OnBuyClicked);
    }
    
    private async void OnBuyClicked(string entryId, int price)
    {
        // Check money
        if (GameDataManager.Instance.currentAntCoin < price)
        {
            Debug.LogWarning("Not enough AntCoin!");
            return;
        }
        
        // Call Cloud Function: purchaseItem
        var functions = Firebase.Functions.FirebaseFunctions.DefaultInstance;
        var result = await functions.GetHttpsCallable("purchaseItem").CallAsync(new Dictionary<string, object>
        {
            { "shopId", "default" },
            { "entryId", entryId },
            { "currency", "antCoin" },
            { "quantity", 1 }
        });
        
        Debug.Log("Purchase successful!");
        
        // Reload inventory
        await InventoryService.Instance.LoadInventoryAsync(GameDataManager.Instance.currentUserId);
        
        // Update currency
        GameDataManager.Instance.currentAntCoin -= price;
        UpdateCurrencyDisplay();
    }
}
```

**Cần làm:**
- [ ] Tạo ShopUIManager.cs
- [ ] Tạo ShopItem.cs (prefab)
- [ ] Setup UI trong Unity
- [ ] Test mua items
- [ ] Test mua skill cards

---

#### **Task 2.2: ShopItem Prefab** (30 phút)
**File:** `Assets/Scenes/Menu/Shop/ShopItem.cs` (NEW)

```csharp
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    
    private string entryId;
    private int price;
    private Action<string, int> onBuyCallback;
    
    public void Setup(string id, string itemId, int priceAntCoin, Action<string, int> callback)
    {
        entryId = id;
        price = priceAntCoin;
        onBuyCallback = callback;
        
        // Load item data
        LoadItemData(itemId);
        
        priceText.text = $"{priceAntCoin} AC";
        buyButton.onClick.AddListener(OnBuyClicked);
    }
    
    private async void LoadItemData(string itemId)
    {
        // Load from Firestore: items/{itemId}
        var itemRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance.Collection("items").Document(itemId);
        var snapshot = await itemRef.GetSnapshotAsync();
        
        if (snapshot.Exists)
        {
            var data = snapshot.ToDictionary();
            nameText.text = data["name"].ToString();
            
            // Load icon (TODO: Load from Resources or URL)
        }
    }
    
    private void OnBuyClicked()
    {
        onBuyCallback?.Invoke(entryId, price);
    }
}
```

**Cần làm:**
- [ ] Tạo ShopItem.cs
- [ ] Tạo ShopItem prefab (UI)
- [ ] Test display
- [ ] Test buy button

---

#### **Task 2.3: Integrate Shop vào Menu** (15 phút)
**File:** `Assets/Scenes/Menu/MenuSceneManager.cs`

**Thêm:**
```csharp
[Header("Shop")]
[SerializeField] private ShopUIManager shopUIManager;

private void SetupEventListeners()
{
    // ...existing code...
    
    if (buttonShop != null)
    {
        buttonShop.onClick.AddListener(OnShopClicked);
    }
}

private void OnShopClicked()
{
    if (shopUIManager != null)
    {
        shopUIManager.OpenShop();
    }
}
```

**Cần làm:**
- [ ] Add ShopUIManager reference
- [ ] Add button listener
- [ ] Test open shop

---

### **PHASE 3: MATCHMAKING & LOBBY (1 giờ)**

#### **Task 3.1: Fix Lobby Flow** (30 phút)
**File:** `Assets/Scenes/Menu/LobbyUIManager.cs`

**Vấn đề:**
- Cần call `PrepareGameSession()` trước khi start game

**Fix:**
```csharp
private async void OnGameStarting(string relayJoinCode)
{
    DebugLog($"Game starting with Relay code: {relayJoinCode}");
    
    // ⭐ PREPARE GAME SESSION DATA
    var inventoryUI = FindObjectOfType<InventoryUIManager>();
    if (inventoryUI != null)
    {
        inventoryUI.PrepareGameSession();
    }
    else
    {
        Debug.LogError("[LobbyUI] InventoryUIManager not found! Cannot prepare game session.");
    }
    
    // Setup GameSessionData
    var sessionData = GameSessionData.Instance;
    sessionData.SetFromGameDataManager();
    sessionData.SetUnityPlayerId(UGSAuthService.PlayerId);
    
    bool isHost = CustomLobbyService.Instance.IsHost;
    string lobbyId = CustomLobbyService.Instance.CurrentLobby?.Id;
    sessionData.SetNetworkInfo(relayJoinCode, isHost, lobbyId);
    
    // Join Relay
    if (isHost)
    {
        RelayService.Instance.StartHost();
    }
    else
    {
        await RelayService.Instance.JoinRelayAsync(relayJoinCode);
        RelayService.Instance.StartClient();
    }
    
    // Load game scene
    SceneManager.LoadScene(GameConfig.GAME_SCENE_NAME);
}
```

**Cần làm:**
- [ ] Add PrepareGameSession() call
- [ ] Test lobby flow
- [ ] Verify GameSessionData

---

#### **Task 3.2: Test Matchmaking** (30 phút)
**Cần test:**
- [ ] Create room
- [ ] Join room
- [ ] Start game (2-4 players)
- [ ] Verify loadout data transfer

---

### **PHASE 4: END GAME REWARDS (1.5 giờ)**

#### **Task 4.1: End Game Panel** (45 phút)
**File:** `Assets/Scenes/Game/Scripts/UI/PanelResult.cs`

**Thêm rewards logic:**
```csharp
public async void ShowResults(List<PlayerGameController> players, int localPlayerIndex)
{
    // ...existing code...
    
    // Calculate rewards
    int rank = GetPlayerRank(localPlayerIndex, players);
    var rewards = CalculateRewards(rank);
    
    // Display rewards
    DisplayRewards(rewards);
    
    // Call Cloud Function: awardMatch
    await AwardMatchRewards(rank, rewards);
    
    // Update GameDataManager
    UpdateLocalData(rewards);
}

private (int antCoin, int xp) CalculateRewards(int rank)
{
    // Rank 1: 500 AC, 200 XP
    // Rank 2: 300 AC, 150 XP
    // Rank 3: 200 AC, 100 XP
    // Rank 4: 100 AC, 50 XP
    
    int[] antCoinRewards = { 500, 300, 200, 100 };
    int[] xpRewards = { 200, 150, 100, 50 };
    
    int antCoin = antCoinRewards[rank - 1];
    int xp = xpRewards[rank - 1];
    
    return (antCoin, xp);
}

private async Task AwardMatchRewards(int rank, (int antCoin, int xp) rewards)
{
    var functions = Firebase.Functions.FirebaseFunctions.DefaultInstance;
    var result = await functions.GetHttpsCallable("awardMatch").CallAsync(new Dictionary<string, object>
    {
        { "rank", rank },
        { "antCoinReward", rewards.antCoin },
        { "xpReward", rewards.xp }
    });
    
    Debug.Log($"[PanelResult] Rewards awarded: {rewards.antCoin} AC, {rewards.xp} XP");
}

private void UpdateLocalData((int antCoin, int xp) rewards)
{
    var gameData = GameDataManager.Instance;
    gameData.currentAntCoin += rewards.antCoin;
    gameData.currentXp += rewards.xp;
    
    // Check level up
    int newLevel = CalculateLevel(gameData.currentXp);
    if (newLevel > gameData.currentLevel)
    {
        gameData.currentLevel = newLevel;
        Debug.Log($"[PanelResult] LEVEL UP! New level: {newLevel}");
    }
}

private int CalculateLevel(int xp)
{
    // Simple formula: level = floor(xp / 1000) + 1
    return Mathf.FloorToInt(xp / 1000f) + 1;
}
```

**Cần làm:**
- [ ] Add rewards calculation
- [ ] Add Cloud Function call
- [ ] Add level up logic
- [ ] Test rewards

---

#### **Task 4.2: Return to Menu Button** (30 phút)
**File:** `Assets/Scenes/Game/Scripts/UI/PanelResult.cs`

**Thêm button:**
```csharp
[SerializeField] private Button buttonReturnToMenu;

private void Start()
{
    if (buttonReturnToMenu != null)
    {
        buttonReturnToMenu.onClick.AddListener(OnReturnToMenuClicked);
    }
}

private void OnReturnToMenuClicked()
{
    // Cleanup network
    if (NetworkManager.Singleton != null)
    {
        NetworkManager.Singleton.Shutdown();
    }
    
    // Load Menu Scene
    SceneManager.LoadScene("MenuScene");
}
```

**Cần làm:**
- [ ] Add button to PanelResult
- [ ] Add listener
- [ ] Test return to menu
- [ ] Verify data persistence

---

#### **Task 4.3: Reload User Data in Menu** (15 phút)
**File:** `Assets/Scenes/Menu/MenuSceneManager.cs`

**Fix:**
```csharp
private async void InitializeMenuScene()
{
    // ...existing code...
    
    // ⭐ ALWAYS reload user data (để cập nhật rewards từ game)
    await LoadUserDataAndInventory();
    
    // Update UI
    if (panelMoney != null)
    {
        panelMoney.UpdateCurrencyDisplay();
    }
}
```

**Cần làm:**
- [ ] Ensure reload on scene load
- [ ] Test currency update
- [ ] Test level update

---

### **PHASE 5: TESTING & BUG FIXES (1.5 giờ)**

#### **Task 5.1: Full Flow Test** (1 giờ)
**Test cases:**
1. [ ] Login → Menu
2. [ ] Open Inventory → Drag items to loadout
3. [ ] Check stats calculation
4. [ ] Open Shop → Buy item → Check inventory
5. [ ] Open Shop → Buy skill card → Check inventory
6. [ ] Equip new items → Check stats
7. [ ] Create lobby → Wait for players
8. [ ] Start game → Verify loadout data
9. [ ] Play game → End game
10. [ ] Check rewards → Return to menu
11. [ ] Verify currency + level updated

#### **Task 5.2: Bug Fixes** (30 phút)
**Common issues:**
- [ ] Drag & drop not working
- [ ] Stats not updating
- [ ] Shop purchase fails
- [ ] Loadout not saving
- [ ] Game session data missing
- [ ] Rewards not applied
- [ ] Scene transition errors

---

## 📝 CHECKLIST TỔNG HỢP

### **PHASE 1: Inventory & Loadout** ✅
- [ ] DragDropHandler.cs created
- [ ] Drag & drop working
- [ ] Stats calculation fixed
- [ ] PrepareGameSession() implemented
- [ ] Test inventory + loadout

### **PHASE 2: Shop System** ✅
- [ ] ShopUIManager.cs created
- [ ] ShopItem.cs created
- [ ] Shop UI setup
- [ ] Purchase items working
- [ ] Purchase skill cards working
- [ ] Currency deduction working

### **PHASE 3: Matchmaking & Lobby** ✅
- [ ] PrepareGameSession() called before game
- [ ] Lobby flow tested
- [ ] GameSessionData verified

### **PHASE 4: End Game Rewards** ✅
- [ ] Rewards calculation implemented
- [ ] Cloud Function call working
- [ ] Level up logic working
- [ ] Return to menu button working
- [ ] Data persistence verified

### **PHASE 5: Testing** ✅
- [ ] Full flow tested (Login → Game → Menu)
- [ ] All bugs fixed
- [ ] Performance acceptable

---

## 🚀 BẮT ĐẦU NGAY

**Bạn muốn tôi:**
1. **Tạo tất cả files cần thiết?** (DragDropHandler, ShopUIManager, ShopItem, etc.)
2. **Fix từng phase một?** (Phase 1 → 2 → 3 → 4 → 5)
3. **Tạo hướng dẫn chi tiết hơn cho một task cụ thể?**

Hãy cho tôi biết bạn muốn bắt đầu từ đâu! ⏱️

