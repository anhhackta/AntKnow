# 🎨 PANELINFO - SETUP HOÀN CHỈNH

**Layout đầy đủ với Level và Stats để người chơi biết sức mạnh của nhau**

---

## 📊 **LAYOUT CUỐI CÙNG**

```
┌─────────────────────────────────────┐
│         PLAYER INFO                 │ ← Title (Blue background)
├─────────────────────────────────────┤
│                                     │
│  [Avatar]      Player Name          │ ← ImageGender + TextPlayerName
│                Level: 5             │ ← TextLevel (Yellow)
│                Matches: 10/3        │ ← TextMatches (Green)
│                                     │
│  ─────────────────────────────────  │ ← Separator (optional)
│                                     │
│  Stats:                             │ ← TextStatsLabel
│  HP: 50  AGI: 30  INT: 40          │ ← TextStats (line 1)
│  LUCK: 20  RES: 25                 │ ← TextStats (line 2)
│                                     │
│         [ĐÓNG BUTTON]               │ ← BtnClose (Red)
│                                     │
└─────────────────────────────────────┘
```

---

## 📋 **HIERARCHY STRUCTURE**

```
PanelInfo (Image + Panel Info Script) - INACTIVE
├── TextTitle ("PLAYER INFO")
├── ImageGender (Avatar - 120x120)
├── InfoContainer (Empty GameObject)
│   ├── TextPlayerName
│   ├── TextLevel
│   └── TextMatches
├── SeparatorLine (Image - optional)
├── TextStatsLabel ("Stats:")
├── TextStats (HP, AGI, INT, LUCK, RES)
└── BtnClose (Button)
```

---

## 🛠️ **STEP-BY-STEP SETUP**

### **1. Tạo PanelInfo Root**

```
Canvas → Right-click → UI → Image
Name: PanelInfo
Add Component: Panel Info

RectTransform:
  Anchor: Center
  Pivot: (0.5, 0.5)
  Pos: (0, 0)
  Width: 600
  Height: 600

Image:
  Color: (0.1, 0.1, 0.1, 0.95)
  
GameObject:
  Active: ✗ FALSE ← ⭐ QUAN TRỌNG!
```

---

### **2. TextTitle**

```
PanelInfo → UI → Text - TextMeshPro
Name: TextTitle

RectTransform:
  Anchor: Top-Stretch
  Pos Y: 0
  Left: 0, Right: 0
  Height: 60

Add Component: Image (background)
  Color: (0.2, 0.4, 0.8, 0.8)

TextMeshPro:
  Text: "PLAYER INFO"
  Font Size: 28
  Alignment: Center, Middle
  Color: White
  Font Style: Bold
```

---

### **3. ImageGender (Avatar)**

```
PanelInfo → UI → Image
Name: ImageGender

RectTransform:
  Anchor: Top-Left
  Pivot: (0, 1)
  Pos X: 30
  Pos Y: -80
  Width: 120
  Height: 120

Image:
  Preserve Aspect: ✓ TRUE
  Color: White
```

---

### **4. InfoContainer**

```
PanelInfo → Create Empty
Name: InfoContainer

RectTransform:
  Anchor: Top-Stretch
  Pos Y: -80
  Left: 170
  Right: 30
  Height: 120
```

---

### **5. TextPlayerName**

```
InfoContainer → UI → Text - TextMeshPro
Name: TextPlayerName

RectTransform:
  Anchor: Top-Stretch
  Pos Y: 0
  Left: 0, Right: 0
  Height: 40

TextMeshPro:
  Text: "Player Name"
  Font Size: 28
  Alignment: Left, Middle
  Color: White
  Font Style: Bold
  Overflow: Ellipsis
```

---

### **6. TextLevel**

```
InfoContainer → UI → Text - TextMeshPro
Name: TextLevel

RectTransform:
  Anchor: Top-Stretch
  Pos Y: -45
  Left: 0, Right: 0
  Height: 35

TextMeshPro:
  Text: "Level: 5"
  Font Size: 22
  Alignment: Left, Middle
  Color: (1, 0.8, 0, 1) - Yellow
```

---

### **7. TextMatches**

```
InfoContainer → UI → Text - TextMeshPro
Name: TextMatches

RectTransform:
  Anchor: Top-Stretch
  Pos Y: -85
  Left: 0, Right: 0
  Height: 35

TextMeshPro:
  Text: "Matches: 10/3"
  Font Size: 22
  Alignment: Left, Middle
  Color: (0.5, 1, 0.5, 1) - Light Green
```

---

### **8. SeparatorLine (Optional)**

```
PanelInfo → UI → Image
Name: SeparatorLine

RectTransform:
  Anchor: Top-Stretch
  Pos Y: -220
  Left: 30, Right: 30
  Height: 2

Image:
  Color: (0.5, 0.5, 0.5, 0.5)
```

---

### **9. TextStatsLabel**

```
PanelInfo → UI → Text - TextMeshPro
Name: TextStatsLabel

RectTransform:
  Anchor: Top-Left
  Pos X: 30
  Pos Y: -240
  Width: 540
  Height: 35

TextMeshPro:
  Text: "Stats:"
  Font Size: 24
  Alignment: Left, Middle
  Color: (0.8, 0.8, 1, 1) - Light Blue
  Font Style: Bold
```

---

### **10. TextStats**

```
PanelInfo → UI → Text - TextMeshPro
Name: TextStats

RectTransform:
  Anchor: Top-Stretch
  Pos Y: -280
  Left: 30, Right: 30
  Height: 100

TextMeshPro:
  Text: "HP: 50  AGI: 30  INT: 40
LUCK: 20  RES: 25"
  Font Size: 22
  Alignment: Left, Top
  Color: (0.5, 1, 0.5, 1) - Green
  Line Spacing: 10
```

---

### **11. BtnClose**

```
PanelInfo → UI → Button - TextMeshPro
Name: BtnClose

RectTransform:
  Anchor: Bottom-Center
  Pos X: 0
  Pos Y: 20
  Width: 200
  Height: 60

Button:
  Normal Color: (0.8, 0.2, 0.2, 1) - Red
  Highlighted Color: (1, 0.3, 0.3, 1)
  Pressed Color: (0.6, 0.1, 0.1, 1)

Text:
  Text: "ĐÓNG"
  Font Size: 24
  Color: White
  Font Style: Bold
```

---

## 🔧 **ASSIGN REFERENCES**

### **Select PanelInfo → Inspector → Panel Info (Script):**

```
UI Components:
├── Image Gender: [Drag ImageGender]
├── Text Player Name: [Drag TextPlayerName]
├── Text Level: [Drag TextLevel]
├── Text Matches: [Drag TextMatches]
├── Text Stats: [Drag TextStats]
└── Btn Close: [Drag BtnClose]

Gender Sprites:
├── Sprite Male: [Assign male sprite]
└── Sprite Female: [Assign female sprite]
```

---

## 📊 **DỮ LIỆU HIỂN THỊ**

### **Nguồn dữ liệu:**

```
Player Name:
  ← currentPlayer.PlayerName (từ Firebase)

Level:
  ← GameDataManager.currentLevel (từ Firebase)

Matches:
  ← GameDataManager.currentMatchesPlayed / currentMatchesWon (từ Firebase)

Stats:
  ← currentPlayer.Health, Agility, Intelligence, Luck, Resistance
     (từ Loadout: Equipment + Skill Cards)

Gender Avatar:
  ← currentPlayer.IsMale (true → male sprite, false → female sprite)
```

---

## 🎯 **SCRIPT ĐÃ CẬP NHẬT**

### **PanelInfo.cs - LoadPlayerStats():**

```csharp
private void LoadPlayerStats()
{
    if (currentPlayer == null) return;
    
    var gameDataManager = GameDataManager.Instance;
    
    // Update Level
    if (textLevel != null)
    {
        int level = gameDataManager != null ? gameDataManager.currentLevel : 1;
        textLevel.text = $"Level: {level}";
    }
    
    // Update Matches (played/won)
    if (textMatches != null)
    {
        if (gameDataManager != null)
        {
            textMatches.text = $"Matches: {gameDataManager.currentMatchesPlayed}/{gameDataManager.currentMatchesWon}";
        }
        else
        {
            textMatches.text = "Matches: 0/0";
        }
    }
    
    // ⭐ Update Stats (HP, AGI, INT, LUCK, RES)
    if (textStats != null)
    {
        textStats.text = $"HP: {currentPlayer.Health}  AGI: {currentPlayer.Agility}  INT: {currentPlayer.Intelligence}\n" +
                         $"LUCK: {currentPlayer.Luck}  RES: {currentPlayer.Resistance}";
    }
}
```

---

## ✅ **CHECKLIST**

### **GameObject:**
- [ ] PanelInfo created
- [ ] SetActive = FALSE
- [ ] Panel Info script added
- [ ] Width: 600, Height: 600

### **UI Components:**
- [ ] TextTitle created
- [ ] ImageGender created
- [ ] InfoContainer created
- [ ] TextPlayerName created
- [ ] TextLevel created
- [ ] TextMatches created
- [ ] SeparatorLine created (optional)
- [ ] TextStatsLabel created
- [ ] TextStats created
- [ ] BtnClose created

### **References:**
- [ ] Image Gender assigned
- [ ] Text Player Name assigned
- [ ] Text Level assigned
- [ ] Text Matches assigned
- [ ] Text Stats assigned
- [ ] Btn Close assigned
- [ ] Sprite Male assigned
- [ ] Sprite Female assigned

---

## 🧪 **TESTING**

### **Test Display:**

1. **Play Mode**
2. **Click PanelMe**
3. **Verify PanelInfo shows:**
   ```
   ✅ Player name correct
   ✅ Gender avatar correct (male/female)
   ✅ Level correct (từ Firebase)
   ✅ Matches correct (10/3 format)
   ✅ Stats correct:
      - HP: 50
      - AGI: 30
      - INT: 40
      - LUCK: 20
      - RES: 25
   ```

### **Test với player khác:**

1. **Có 2+ players trong game**
2. **Click PanelPlayerPrefab (player khác)**
3. **Verify:**
   ```
   ✅ Hiển thị thông tin player đó
   ✅ Stats khác nhau (vì loadout khác)
   ✅ Level có thể khác
   ✅ Matches khác
   ```

**Lợi ích:**
- ✅ Người chơi biết sức mạnh của đối thủ
- ✅ Quyết định chiến thuật (mua đất, tránh player mạnh)
- ✅ Tăng tính cạnh tranh

---

## 🎨 **COLOR SCHEME**

```
Background: Dark Gray (0.1, 0.1, 0.1, 0.95)
Title: Blue (0.2, 0.4, 0.8, 0.8)
Player Name: White (Bold)
Level: Yellow (1, 0.8, 0, 1)
Matches: Light Green (0.5, 1, 0.5, 1)
Stats Label: Light Blue (0.8, 0.8, 1, 1)
Stats Values: Green (0.5, 1, 0.5, 1)
Close Button: Red (0.8, 0.2, 0.2, 1)
```

---

## 💡 **TIPS**

### **1. Stats Color Coding (Optional):**

Có thể đổi màu stats dựa trên giá trị:
```csharp
// High stats (>40): Green
// Medium stats (20-40): Yellow
// Low stats (<20): Red
```

### **2. Animation (Optional):**

Thêm fade in/out animation khi mở/đóng panel.

### **3. Background Blur (Optional):**

Thêm blur effect cho background để focus vào panel.

---

## 📝 **SUMMARY**

**PanelInfo hiển thị:**
1. ✅ Player Name (từ Firebase)
2. ✅ Gender Avatar (male/female sprite)
3. ✅ Level (từ Firebase)
4. ✅ Matches Played/Won (từ Firebase)
5. ✅ Stats: HP, AGI, INT, LUCK, RES (từ Loadout)

**Lợi ích:**
- ✅ Người chơi biết sức mạnh đối thủ
- ✅ Quyết định chiến thuật tốt hơn
- ✅ Tăng tính cạnh tranh
- ✅ UI đẹp, rõ ràng

---

**DONE! PanelInfo hoàn chỉnh! 🎉**

