# 🎮 TẠO PLAYER ITEM PREFAB - 5 PHÚT

## 🎯 MỤC ĐÍCH

Tạo prefab để hiển thị player trong lobby (PanelJoinRoom)

**Cấu trúc:**
```
PlayerItemPrefab (Button)
└── Text (TMP_Text) → Tên người chơi
```

---

## 🚀 CÁCH TẠO (5 PHÚT)

### **BƯỚC 1: Tạo Button**

```
1. Hierarchy → Right-click → UI → Button - TextMeshPro
2. Rename: "PlayerItemPrefab"
3. Delete child "Text (Legacy)" (nếu có)
```

---

### **BƯỚC 2: Tạo Text (TMP)**

```
1. Right-click PlayerItemPrefab → UI → Text - TextMeshPro
2. Rename: "PlayerNameText"
3. Settings:
   - Text: "Player Name"
   - Font Size: 18
   - Alignment: Center
   - Color: White
```

---

### **BƯỚC 3: Setup Button**

```
1. Select PlayerItemPrefab
2. RectTransform:
   - Width: 300
   - Height: 50
3. Image (Background):
   - Color: Dark gray (R:50, G:50, B:50, A:255)
4. Button:
   - Interactable: ✗ (false) - Chỉ hiển thị, không click
   - Transition: None
```

---

### **BƯỚC 4: Setup Text**

```
1. Select PlayerNameText
2. RectTransform:
   - Anchor: Stretch (full)
   - Left: 10
   - Right: 10
   - Top: 5
   - Bottom: 5
3. TextMeshProUGUI:
   - Text: "Player Name"
   - Font Size: 18
   - Alignment: Center Middle
   - Color: White
   - Overflow: Ellipsis
```

---

### **BƯỚC 5: Tạo Prefab**

```
1. Drag PlayerItemPrefab từ Hierarchy → Assets/Scenes/Menu/
2. Prefab created: "PlayerItemPrefab.prefab"
3. Delete PlayerItemPrefab từ Hierarchy (giữ prefab trong Assets)
```

---

### **BƯỚC 6: Assign vào LobbyUIManager**

```
1. Find LobbyUIManager GameObject
2. Inspector → LobbyUIManager component
3. Assign:
   - Player Item Prefab: Drag "PlayerItemPrefab.prefab"
```

---

## 🎵 HIERARCHY STRUCTURE

### **PlayerItemPrefab.prefab:**

```
PlayerItemPrefab (Button)
├── RectTransform (300 x 50)
├── Image (Background: Dark gray)
├── Button (Interactable: false)
└── PlayerNameText (TextMeshProUGUI)
    ├── Text: "Player Name"
    ├── Font Size: 18
    ├── Alignment: Center Middle
    └── Color: White
```

---

## 🎵 CODE - LobbyUIManager.cs

**Cách sử dụng PlayerItemPrefab:**

```csharp
// Spawn player item
GameObject item = Instantiate(playerItemPrefab, playerListContainer);

// Get text component
var playerNameText = item.GetComponentInChildren<TMP_Text>();
if (playerNameText != null)
{
    playerNameText.text = player.Data["PlayerName"].Value;
}

// Add to list
playerListItems.Add(item);
```

---

## 🧪 TEST

### **Test 1: Prefab tạo đúng**
```
1. Open Assets/Scenes/Menu/
2. ✅ File "PlayerItemPrefab.prefab" tồn tại
3. Double-click prefab
4. ✅ Có Button component
5. ✅ Có child Text (TMP_Text)
```

### **Test 2: Assign vào LobbyUIManager**
```
1. Find LobbyUIManager
2. Inspector → Player Item Prefab
3. ✅ Assigned "PlayerItemPrefab.prefab"
```

### **Test 3: Lobby hiển thị players**
```
1. Create lobby
2. 2nd player joins
3. ✅ PanelJoinRoom hiển thị 2 player items
4. ✅ Tên players hiển thị đúng
```

---

## 🎯 SUMMARY

**Tạo:**
- ✅ PlayerItemPrefab (Button + Text)
- ✅ Width: 300, Height: 50
- ✅ Text: TMP_Text, Center, White

**Assign:**
- ✅ LobbyUIManager → Player Item Prefab

**Test:**
- ✅ Prefab tồn tại
- ✅ Assigned đúng
- ✅ Lobby hiển thị players

---

**THỜI GIAN: 5 PHÚT** ⏱️

**LÀM NGAY!** 🔥

