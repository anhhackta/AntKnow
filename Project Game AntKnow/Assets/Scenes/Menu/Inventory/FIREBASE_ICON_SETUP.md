# 🔥 Firebase Icon Setup Guide

## 🎯 Vấn đề hiện tại

Bạn có sprites trong Unity Resources nhưng Firebase icon fields chưa được setup đúng để load sprites.

## 📁 Cấu trúc hiện tại

### Unity Resources:
```
Assets/Resources/
├── skill.bao-ke.png          ← Root level
├── skill.cham-chi.png        ← Root level  
├── skill.lan-tron.png        ← Root level
├── skill.sieu-sale.png       ← Root level
├── equip.hat.basic.png       ← Root level
├── equip.mask.basic.png      ← Root level
├── equip.shirt.basic.png     ← Root level
├── equip.shoes.basic.png     ← Root level
├── equip.wings.basic.png     ← Root level
├── exp.small.png             ← Root level
├── Cards/                    ← Empty folder
├── Equipment/                ← Empty folder
└── Items/                    ← Empty folder
```

### Firebase Items Collection:
```
items/
├── skill.bao-ke
├── skill.cham-chi
├── skill.lan-tron
├── skill.sieu-sale
├── equip.hat.basic
├── equip.mask.basic
├── equip.shirt.basic
├── equip.shoes.basic
├── equip.wings.basic
└── exp.small
```

## 🔧 Giải pháp

### Option 1: Update Firebase Icon Fields (Đơn giản nhất)

Vì sprites đang ở root level, bạn chỉ cần update Firebase icon fields:

#### Firebase Console Setup:
1. Mở Firebase Console > Firestore Database
2. Collection: `items`
3. Update từng document:

```json
// items/skill.bao-ke
{
  "itemId": "skill.bao-ke",
  "name": "Bảo Kê",
  "type": "skill_card",
  "icon": "skill.bao-ke",  ← Chỉ tên file, không có folder
  "attributes": {
    "primaryStat": "agility",
    "agility": 10,
    "health": 0,
    "intelligence": 0,
    "luck": 0,
    "resistance": 0
  },
  "skill": {
    "mode": "passive",
    "effect": "Tự động tiến lên 1 bước khi vào nhà đối thủ",
    "cooldownBaseTurns": 5
  }
}

// items/equip.hat.basic
{
  "itemId": "equip.hat.basic",
  "name": "Mũ Cơ Bản",
  "type": "equipment",
  "icon": "equip.hat.basic",  ← Chỉ tên file, không có folder
  "equipment": {
    "slot": "hat"
  },
  "attributes": {
    "health": 0,
    "agility": 0,
    "intelligence": 0,
    "luck": 10,
    "resistance": 0
  }
}

// items/exp.small
{
  "itemId": "exp.small",
  "name": "EXP Nhỏ",
  "type": "exp_card",
  "icon": "exp.small",  ← Chỉ tên file, không có folder
  "exp": {
    "xpValue": 500
  }
}
```

### Option 2: Organize Sprites to Folders (Tốt hơn)

#### Bước 1: Organize sprites trong Unity
1. Mở Unity Editor
2. Tạo GameObject và attach `OrganizeSprites` script
3. Right-click > "Organize Sprites to Folders"
4. Sprites sẽ được di chuyển vào:
   ```
   Assets/Resources/
   ├── Cards/
   │   ├── skill.bao-ke.png
   │   ├── skill.cham-chi.png
   │   ├── skill.lan-tron.png
   │   └── skill.sieu-sale.png
   ├── Equipment/
   │   ├── equip.hat.basic.png
   │   ├── equip.mask.basic.png
   │   ├── equip.shirt.basic.png
   │   ├── equip.shoes.basic.png
   │   └── equip.wings.basic.png
   └── Items/
       └── exp.small.png
   ```

#### Bước 2: Update Firebase với folder paths
```json
// items/skill.bao-ke
{
  "icon": "Cards/skill.bao-ke"  ← Có folder
}

// items/equip.hat.basic
{
  "icon": "Equipment/equip.hat.basic"  ← Có folder
}

// items/exp.small
{
  "icon": "Items/exp.small"  ← Có folder
}
```

## 🧪 Test Sprite Loading

### Bước 1: Test với SpriteLoader
1. Tạo GameObject và attach `TestSpriteLoading` script
2. Play game
3. Check Console logs:
   ```
   === Testing Sprite Loading with SpriteLoader ===
   --- Testing simple file names ---
   [SpriteLoader] ✅ Loaded sprite: skill.bao-ke
   [SpriteLoader] ✅ Loaded sprite: equip.hat.basic
   --- Testing paths with folders ---
   [SpriteLoader] ✅ Loaded sprite from Cards/: Cards/skill.bao-ke
   [SpriteLoader] ✅ Loaded sprite from Equipment/: Equipment/equip.hat.basic
   === Test Complete ===
   ```

### Bước 2: Test trong Inventory
1. Play game
2. Login với test account
3. Check Console logs:
   ```
   [InventoryService] Parsed item data: skill.bao-ke, icon: skill.bao-ke
   [SpriteLoader] ✅ Loaded sprite: skill.bao-ke
   [CardSlot] ✅ Loaded card sprite: skill.bao-ke
   ```

## 🎯 SpriteLoader Features

SpriteLoader tự động tìm sprite theo thứ tự:

1. **Direct path**: `"Cards/skill.bao-ke"` → `Assets/Resources/Cards/skill.bao-ke.png`
2. **Root fallback**: `"Cards/skill.bao-ke"` → `Assets/Resources/skill.bao-ke.png`
3. **Folder search**: `"skill.bao-ke"` → Tìm trong `Cards/`, `Equipment/`, `Items/`
4. **Fuzzy search**: Tìm sprite có tên tương tự

## 📋 Checklist

### Option 1 (Đơn giản):
- [ ] Update Firebase icon fields = tên file đơn giản
- [ ] Test sprite loading
- [ ] Verify inventory hiển thị sprites

### Option 2 (Tốt hơn):
- [ ] Run OrganizeSprites script
- [ ] Update Firebase icon fields = path có folder
- [ ] Test sprite loading
- [ ] Verify inventory hiển thị sprites

## 🔍 Debug Commands

### Test specific sprite:
```csharp
SpriteLoader.TestSprite("skill.bao-ke");
SpriteLoader.TestSprite("Cards/skill.bao-ke");
```

### List all sprites:
```csharp
SpriteLoader.ListAllSprites();
```

### Check current organization:
```csharp
// Attach OrganizeSprites script và right-click > "List Current Sprite Organization"
```

## 💡 Tips

1. **Firebase icon field** chỉ cần tên file (không cần .png)
2. **SpriteLoader** tự động handle cả root level và folder structure
3. **Test script** giúp debug nhanh sprite loading issues
4. **Organize script** giúp move sprites vào đúng folders

## 🚀 Kết quả mong đợi

Sau khi setup xong:
- Firebase icon fields point đến đúng sprite names
- SpriteLoader tự động tìm và load sprites
- Inventory hiển thị sprites đúng
- Console logs không có lỗi sprite loading

---

**Chọn Option 1 nếu muốn nhanh, Option 2 nếu muốn tổ chức tốt hơn! 🎯**
