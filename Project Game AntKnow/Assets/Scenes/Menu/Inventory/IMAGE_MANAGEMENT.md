# 🖼️ Image/Icon Management - Hướng dẫn quản lý hình ảnh

## 🎯 Phương án: Sử dụng Unity Resources Folder

### ✅ Lý do chọn Resources:
```
✅ Đơn giản, không cần setup phức tạp
✅ Load nhanh, không cần download
✅ Dễ quản lý, dễ debug
✅ Phù hợp với game offline/local
✅ Không tốn bandwidth Firebase
```

### ❌ Không dùng Firebase Storage vì:
```
❌ Phức tạp (cần download, cache)
❌ Chậm (phải download mỗi lần)
❌ Tốn bandwidth
❌ Cần internet
❌ Khó debug
```

---

## 📁 Cấu trúc Folder

### Unity Project Structure:
```
Assets/
└── Resources/
    ├── Cards/              # Skill card images
    │   ├── skill.lan-tron.png
    │   ├── skill.bao-ke.png
    │   ├── skill.toc-do.png
    │   └── ...
    ├── Equipment/          # Equipment images
    │   ├── hat.mao-len.png
    │   ├── shirt.ao-giap.png
    │   ├── wings.canh-thien-than.png
    │   ├── shoes.giay-the-thao.png
    │   ├── mask.mat-na-ninja.png
    │   └── ...
    ├── Items/              # Other items (exp cards, materials)
    │   ├── exp.small.png
    │   ├── exp.medium.png
    │   ├── exp.large.png
    │   ├── material.go.png
    │   ├── material.sat.png
    │   └── ...
    └── UI/                 # UI elements
        ├── star.png
        ├── empty-slot.png
        └── ...
```

---

## 🗄️ Database Structure

### Firestore: `items/{itemId}`
```javascript
{
  itemId: "skill.lan-tron",
  name: "Lá Bảo Kê",
  type: "skill_card",
  icon: "Cards/skill.lan-tron",  // ← Path trong Resources folder
  // ... other fields
}
```

### Quy tắc đặt tên:
```
Format: "{Folder}/{itemId}"

Skill cards:    "Cards/skill.lan-tron"
Equipment:      "Equipment/hat.mao-len"
EXP cards:      "Items/exp.small"
Materials:      "Items/material.go"
```

---

## 💻 Code Implementation

### Load sprite từ Resources:

```csharp
// In CardSlot.cs, ItemSlot.cs
private void LoadCardSprite(string iconPath)
{
    if (cardImage == null || string.IsNullOrEmpty(iconPath))
        return;
    
    // iconPath = "Cards/skill.lan-tron"
    Sprite sprite = Resources.Load<Sprite>(iconPath);
    
    if (sprite != null)
    {
        cardImage.sprite = sprite;
    }
    else
    {
        Debug.LogWarning($"Sprite not found: {iconPath}");
        // Use default sprite
        cardImage.sprite = null;
    }
}
```

### Trong ItemSlot.cs:

```csharp
private void CreateItemVisual(InventoryItem item)
{
    // ... existing code ...
    
    // Add Image for icon
    var image = itemVisualObject.AddComponent<Image>();
    image.raycastTarget = true;
    
    // Load sprite
    if (item.itemData != null && !string.IsNullOrEmpty(item.itemData.icon))
    {
        Sprite sprite = Resources.Load<Sprite>(item.itemData.icon);
        if (sprite != null)
        {
            image.sprite = sprite;
        }
    }
    
    // ... rest of code ...
}
```

---

## 🎨 Tạo/Tìm Assets

### Option 1: Tạo placeholder sprites (Nhanh nhất)

```
1. Tạo folder: Assets/Resources/Cards/
2. Tạo placeholder images:
   - Mở Paint/Photoshop
   - Tạo image 256x256
   - Fill màu khác nhau cho mỗi card
   - Save as PNG: skill.lan-tron.png
3. Drag vào Unity Resources/Cards/
```

### Option 2: Dùng Unity default sprites

```
1. Trong Unity: Assets > Import Package > 2D
2. Dùng sprites có sẵn:
   - UI/Skin/Knob.psd
   - UI/Skin/UISprite.psd
3. Copy vào Resources folder
4. Rename theo itemId
```

### Option 3: Download free assets

```
Websites:
- OpenGameArt.org
- Itch.io (free assets)
- Kenney.nl (free game assets)
- Flaticon.com (icons)

Search keywords:
- "card game sprites"
- "equipment icons"
- "RPG items"
```

### Option 4: Generate với AI

```
Tools:
- DALL-E, Midjourney, Stable Diffusion
- Prompt: "pixel art card icon, fantasy style, transparent background"
```

---

## 📊 Mapping Database ↔ Assets

### Bước 1: Tạo naming convention

```
Database itemId = File name (không có extension)

Database:           File:
skill.lan-tron  →   skill.lan-tron.png
hat.mao-len     →   hat.mao-len.png
exp.small       →   exp.small.png
```

### Bước 2: Update Firestore

```javascript
// Firestore: items/skill.lan-tron
{
  itemId: "skill.lan-tron",
  icon: "Cards/skill.lan-tron",  // Path trong Resources
  // ...
}

// Firestore: items/hat.mao-len
{
  itemId: "hat.mao-len",
  icon: "Equipment/hat.mao-len",
  // ...
}
```

### Bước 3: Verify trong Unity

```
1. Check file exists: Assets/Resources/Cards/skill.lan-tron.png
2. Test load:
   Sprite sprite = Resources.Load<Sprite>("Cards/skill.lan-tron");
   Debug.Log(sprite != null ? "Found!" : "Not found!");
```

---

## 🔧 Setup Steps

### Bước 1: Tạo Resources folders (2 phút)

```
1. Right-click Assets > Create > Folder: "Resources"
2. Right-click Resources > Create > Folder: "Cards"
3. Right-click Resources > Create > Folder: "Equipment"
4. Right-click Resources > Create > Folder: "Items"
5. Right-click Resources > Create > Folder: "UI"
```

### Bước 2: Tạo placeholder sprites (10 phút)

```
1. Tạo 5-10 placeholder images (256x256 PNG)
2. Đặt tên theo itemId:
   - skill.lan-tron.png
   - skill.bao-ke.png
   - hat.mao-len.png
   - exp.small.png
   - etc.
3. Drag vào Resources folders tương ứng
```

### Bước 3: Update Firestore (5 phút)

```
1. Mở Firebase Console > Firestore
2. Collection: items
3. Update field "icon" cho mỗi item:
   - skill.lan-tron: icon = "Cards/skill.lan-tron"
   - hat.mao-len: icon = "Equipment/hat.mao-len"
   - exp.small: icon = "Items/exp.small"
```

### Bước 4: Test (5 phút)

```
1. Play game
2. Load inventory
3. Check Console logs:
   - "Sprite not found" → File name không khớp
   - No errors → Success!
4. Verify sprites hiển thị trong slots
```

---

## 🐛 Troubleshooting

### Sprite không load:

```
✅ Check 1: File có trong Resources folder không?
   - Path: Assets/Resources/Cards/skill.lan-tron.png

✅ Check 2: File name khớp với icon path không?
   - Database: "Cards/skill.lan-tron"
   - File: skill.lan-tron.png (KHÔNG có .png trong path)

✅ Check 3: Texture Type đúng không?
   - Select sprite trong Unity
   - Inspector > Texture Type = "Sprite (2D and UI)"
   - Apply

✅ Check 4: Case-sensitive?
   - "Cards/skill.lan-tron" ≠ "cards/skill.lan-tron"
   - "skill.lan-tron" ≠ "Skill.Lan-Tron"
```

### Sprite bị mờ/pixelated:

```
✅ Select sprite trong Unity
✅ Inspector > Filter Mode = "Bilinear" hoặc "Trilinear"
✅ Inspector > Max Size = 2048 hoặc 4096
✅ Apply
```

### Sprite bị crop:

```
✅ Select sprite trong Unity
✅ Inspector > Sprite Mode = "Single"
✅ Inspector > Mesh Type = "Full Rect"
✅ Apply
```

---

## 📝 Checklist

### Setup:
- [ ] Tạo Resources folder structure
- [ ] Tạo placeholder sprites (hoặc download assets)
- [ ] Đặt tên files theo itemId
- [ ] Drag sprites vào Resources folders
- [ ] Set Texture Type = "Sprite (2D and UI)"

### Database:
- [ ] Update Firestore items collection
- [ ] Set icon field = "Folder/itemId" (không có .png)
- [ ] Verify naming convention

### Code:
- [ ] CardSlot.cs có LoadCardSprite() method
- [ ] ItemSlot.cs có LoadItemSprite() method
- [ ] Test load sprites trong Play mode

### Test:
- [ ] Play game
- [ ] Load inventory
- [ ] Verify sprites hiển thị
- [ ] Check Console logs (no errors)

---

## 🎯 Recommended Workflow

### Phase 1: Placeholder (Nhanh - 15 phút)
```
1. Tạo 10 placeholder images (solid colors)
2. Đặt tên theo itemId
3. Drag vào Resources
4. Update Firestore
5. Test → Verify system hoạt động
```

### Phase 2: Real Assets (Sau - 1-2 giờ)
```
1. Download/create real sprites
2. Replace placeholders
3. Adjust sizes/colors
4. Polish UI
```

---

## 💡 Tips

### Tip 1: Batch rename files
```
Windows: Use PowerShell
Get-ChildItem *.png | Rename-Item -NewName {$_.Name -replace "old","new"}

Mac/Linux: Use terminal
for f in *.png; do mv "$f" "${f/old/new}"; done
```

### Tip 2: Auto-generate icon paths
```csharp
// Helper method
public static string GetIconPath(string itemId, string type)
{
    string folder = type switch
    {
        "skill_card" => "Cards",
        "equipment" => "Equipment",
        _ => "Items"
    };
    
    return $"{folder}/{itemId}";
}
```

### Tip 3: Fallback sprite
```csharp
private Sprite defaultSprite; // Assign trong Inspector

private void LoadSprite(string iconPath)
{
    Sprite sprite = Resources.Load<Sprite>(iconPath);
    image.sprite = sprite ?? defaultSprite;
}
```

---

**Phương án đơn giản nhất: Dùng Unity Resources folder với placeholder sprites!**

