# 🎨 PLAYER COLOR SYSTEM - THIẾT KẾ

## 💡 Ý TƯỞNG

Mỗi player có 1 **màu riêng** (Player Color) được assign khi vào game:
- **Player 1**: Đỏ (Red)
- **Player 2**: Xanh dương (Blue)
- **Player 3**: Xanh lá (Green)
- **Player 4**: Vàng (Yellow)

Màu này sẽ được dùng cho:
1. ✅ **Panel background** (PanelMe/PanelPlayer) → Phân biệt người chơi
2. ✅ **House roof material** → Biết nhà của ai
3. ✅ **Turn indicator** (optional) → Highlight khi đến lượt

---

## 🏗️ KIẾN TRÚC

### **1. Player Color Assignment**

#### **GameManager.cs:**
```csharp
// Định nghĩa 4 màu cố định
private Color[] playerColors = new Color[]
{
    new Color(1f, 0.2f, 0.2f, 1f),    // Player 1: Red
    new Color(0.2f, 0.5f, 1f, 1f),    // Player 2: Blue
    new Color(0.2f, 1f, 0.2f, 1f),    // Player 3: Green
    new Color(1f, 1f, 0.2f, 1f)       // Player 4: Yellow
};

// Assign màu cho player khi spawn
private void SpawnPlayerNetwork(...)
{
    // ... spawn player ...
    
    // Assign player index và color
    int playerIndex = players.Count - 1; // 0, 1, 2, 3
    player.SetPlayerIndex(playerIndex);
    player.SetPlayerColor(playerColors[playerIndex]);
}
```

#### **PlayerGameController.cs:**
```csharp
public class PlayerGameController : MonoBehaviour
{
    // Player color
    private int playerIndex = 0; // 0-3
    private Color playerColor = Color.white;
    
    public int PlayerIndex => playerIndex;
    public Color PlayerColor => playerColor;
    
    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }
    
    public void SetPlayerColor(Color color)
    {
        playerColor = color;
    }
}
```

---

### **2. Panel Background Color**

#### **PanelPlayerMe.cs:**
```csharp
public class PanelPlayerMe : BasePlayerPanel
{
    [Header("UI Components")]
    [SerializeField] private Image imageBackground; // Background panel
    [SerializeField] private TextMeshProUGUI textPlayerName;
    [SerializeField] private TextMeshProUGUI textMoney;
    [SerializeField] private Image imageAvatar;
    
    [Header("Avatar Sprites")]
    [SerializeField] private Sprite spriteMale;
    [SerializeField] private Sprite spriteFemale;
    
    protected override void UpdateDisplay()
    {
        if (player == null) return;
        
        // Update name
        textPlayerName.text = player.PlayerName;
        
        // Update money
        textMoney.text = $"${player.Money}";
        
        // Update avatar sprite
        imageAvatar.sprite = player.IsMale ? spriteMale : spriteFemale;
        
        // ⭐ SET BACKGROUND COLOR
        if (imageBackground != null)
        {
            // Màu nền với alpha = 0.3 (trong suốt 70%)
            Color bgColor = player.PlayerColor;
            bgColor.a = 0.3f;
            imageBackground.color = bgColor;
        }
    }
}
```

#### **PanelPlayer.cs:**
```csharp
// Giống PanelPlayerMe
public class PanelPlayer : BasePlayerPanel
{
    [SerializeField] private Image imageBackground;
    // ... other components ...
    
    protected override void UpdateDisplay()
    {
        // ... update name, money, avatar ...
        
        // ⭐ SET BACKGROUND COLOR
        if (imageBackground != null)
        {
            Color bgColor = player.PlayerColor;
            bgColor.a = 0.3f;
            imageBackground.color = bgColor;
        }
    }
}
```

---

### **3. House Material Color**

#### **PropertyVisual.cs:**
```csharp
public void SpawnHouse(int tileIndex, int level, int playerIndex)
{
    // Get player color
    Color playerColor = GetPlayerColor(playerIndex);
    
    // Spawn house prefab
    GameObject house = Instantiate(housePrefab, platform);
    
    // ⭐ CHANGE ROOF MATERIAL COLOR
    ChangeMaterialColor(house, playerColor);
}

private void ChangeMaterialColor(GameObject house, Color color)
{
    // Tìm tất cả materials trong house
    Renderer[] renderers = house.GetComponentsInChildren<Renderer>();
    
    foreach (var renderer in renderers)
    {
        // Tìm material có tên "ngói"
        foreach (var mat in renderer.materials)
        {
            if (mat.name.Contains("ngói"))
            {
                // Đổi màu material
                mat.color = color;
            }
        }
    }
}

private Color GetPlayerColor(int playerIndex)
{
    // Lấy màu từ array
    Color[] colors = new Color[]
    {
        new Color(1f, 0.2f, 0.2f, 1f),    // Red
        new Color(0.2f, 0.5f, 1f, 1f),    // Blue
        new Color(0.2f, 1f, 0.2f, 1f),    // Green
        new Color(1f, 1f, 0.2f, 1f)       // Yellow
    };
    
    return colors[playerIndex];
}
```

---

## 🎨 UI DESIGN

### **PanelMe/PanelPlayer Structure:**
```
PanelMe (GameObject)
├── ImageBackground (Image) ← ⭐ ĐỔI MÀU NỀN
│   └── Color: Player Color (alpha 0.3)
├── ImageAvatar (Image)
├── TextName (TMP)
└── TextMoney (TMP)
```

### **Visual Example:**
```
┌────────────────────────────────┐
│ [🔴 Red Background - 30%]      │ ← Player 1
│ 👤 Player1        💰 $1000    │
└────────────────────────────────┘

┌────────────────────────────────┐
│ [🔵 Blue Background - 30%]     │ ← Player 2
│ 👤 Player2        💰 $500     │
└────────────────────────────────┘

┌────────────────────────────────┐
│ [🟢 Green Background - 30%]    │ ← Player 3
│ 👤 Player3        💰 $800     │
└────────────────────────────────┘

┌────────────────────────────────┐
│ [🟡 Yellow Background - 30%]   │ ← Player 4
│ 👤 Player4        💰 $1200    │
└────────────────────────────────┘
```

---

## 🏠 HOUSE COLOR TRÊN MAP

### **Visual Example:**
```
Tile (Map)
├── Platform
│   ├── 🔴 Red House    ← Player 1's house
│   ├── 🔵 Blue House   ← Player 2's house
│   ├── 🟢 Green House  ← Player 3's house
│   └── 🟡 Yellow House ← Player 4's house
```

### **Material "ngói":**
- House prefab có material tên "ngói" (roof material)
- Khi spawn house → Đổi màu material = Player Color
- Khi nhìn map → Biết ngay nhà của ai

---

## 📝 IMPLEMENTATION STEPS

### **Step 1: Update PlayerGameController.cs**
```csharp
// Add these fields
private int playerIndex = 0;
private Color playerColor = Color.white;

public int PlayerIndex => playerIndex;
public Color PlayerColor => playerColor;

public void SetPlayerIndex(int index)
{
    playerIndex = index;
}

public void SetPlayerColor(Color color)
{
    playerColor = color;
}
```

### **Step 2: Update GameManager.cs**
```csharp
// Define player colors
private Color[] playerColors = new Color[]
{
    new Color(1f, 0.2f, 0.2f, 1f),    // Red
    new Color(0.2f, 0.5f, 1f, 1f),    // Blue
    new Color(0.2f, 1f, 0.2f, 1f),    // Green
    new Color(1f, 1f, 0.2f, 1f)       // Yellow
};

// Assign color khi spawn
private void SpawnPlayerNetwork(...)
{
    // ... existing code ...
    
    int playerIndex = players.Count - 1;
    player.SetPlayerIndex(playerIndex);
    player.SetPlayerColor(playerColors[playerIndex]);
}
```

### **Step 3: Update PanelPlayerMe.cs & PanelPlayer.cs**
```csharp
// Add background image field
[SerializeField] private Image imageBackground;

// In UpdateDisplay()
if (imageBackground != null)
{
    Color bgColor = player.PlayerColor;
    bgColor.a = 0.3f; // 30% opacity
    imageBackground.color = bgColor;
}
```

### **Step 4: Setup UI trong Unity**
```
1. Select PanelMe GameObject
2. Add child "ImageBackground" (Image component)
3. Set as first child (background layer)
4. Anchor stretch (full size)
5. Set color = white (sẽ đổi màu bằng code)
6. Drag ImageBackground vào Inspector
7. Làm tương tự cho PanelPlayerPrefab
```

### **Step 5: Update PropertyVisual.cs**
```csharp
// Add method to change material color
private void ChangeMaterialColor(GameObject house, Color color)
{
    Renderer[] renderers = house.GetComponentsInChildren<Renderer>();
    foreach (var renderer in renderers)
    {
        foreach (var mat in renderer.materials)
        {
            if (mat.name.Contains("ngói"))
            {
                mat.color = color;
            }
        }
    }
}

// Call when spawn house
public void SpawnHouse(int tileIndex, int level, int playerIndex)
{
    // ... spawn house ...
    ChangeMaterialColor(houseObj, GetPlayerColor(playerIndex));
}
```

---

## 🎯 KẾT QUẢ

### **Trong Game:**
1. ✅ Mỗi player có panel với màu nền riêng
2. ✅ Nhìn vào panel → Biết ngay player nào
3. ✅ Nhìn vào map → Nhà có màu tương ứng với owner
4. ✅ Dễ phân biệt và track tiến độ

### **Benefits:**
- 🎨 **Visual clarity**: Phân biệt rõ ràng
- 🏠 **Property ownership**: Biết nhà của ai
- 🎮 **Game experience**: Dễ chơi, dễ theo dõi
- 🔄 **Consistency**: Cùng 1 màu cho panel + houses

---

## 💡 BONUS IDEAS

### **1. Turn Indicator với màu:**
```csharp
// Khi đến lượt, viền panel sáng lên
if (isMyTurn)
{
    // Add outline với màu đậm hơn
    Outline outline = GetComponent<Outline>();
    outline.effectColor = player.PlayerColor;
    outline.enabled = true;
}
```

### **2. Player name color:**
```csharp
// Tên player cùng màu với panel
textPlayerName.color = player.PlayerColor;
```

### **3. Money text color:**
```csharp
// Tiền âm = đỏ, tiền dương = xanh
if (player.Money < 0)
    textMoney.color = Color.red;
else
    textMoney.color = Color.white;
```

---

## ✅ CHECKLIST

### **Code:**
- [ ] Add `playerIndex` và `playerColor` vào PlayerGameController
- [ ] Add `playerColors` array vào GameManager
- [ ] Assign color khi spawn player
- [ ] Add `imageBackground` vào PanelPlayerMe
- [ ] Add `imageBackground` vào PanelPlayer
- [ ] Update `UpdateDisplay()` để set background color
- [ ] Update PropertyVisual để đổi màu house material

### **UI:**
- [ ] Add ImageBackground vào PanelMe (first child)
- [ ] Set anchor stretch full
- [ ] Assign vào Inspector
- [ ] Add ImageBackground vào PanelPlayerPrefab
- [ ] Assign vào Inspector

### **Testing:**
- [ ] Spawn 4 players → Check màu panel khác nhau
- [ ] Mua nhà → Check màu house tương ứng với owner
- [ ] Visual rõ ràng, dễ phân biệt

---

**Version**: 1.0  
**Date**: 2025-10-12  
**Status**: Ready to implement! 🎨
