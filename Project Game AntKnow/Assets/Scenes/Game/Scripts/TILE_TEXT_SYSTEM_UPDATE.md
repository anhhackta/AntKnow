# ✅ TILE TEXT SYSTEM UPDATE - COMPLETE

**Date**: October 12, 2025  
**Issue**: Special tiles (Start, Event, Jail, Quiz, Travel) không nên hiển thị giá
**Solution**: Ẩn TextPrice cho special tiles, chỉ Property tiles mới hiển thị giá

---

## 🔧 FILES MODIFIED

### 1. **TileVisual.cs**
**Location**: `Assets/Scenes/Game/Scripts/Visual/TileVisual.cs`

**Changes**:

#### Method: `SetTileInfo()`
```csharp
// OLD (2 parameters)
public void SetTileInfo(int index, string name, int price)

// NEW (3 parameters - added TileType)
public void SetTileInfo(int index, string name, int price, TileType tileType)
{
    tileIndex = index;
    textName.text = name; // Always show name
    
    if (tileType == TileType.Property && price > 0)
    {
        textPrice.text = $"${price}";
        textPrice.gameObject.SetActive(true); // Show price
    }
    else
    {
        textPrice.text = "";
        textPrice.gameObject.SetActive(false); // Hide price ← ⭐ KEY CHANGE
    }
}
```

#### Method: `UpdatePrice()`
```csharp
// OLD
public void UpdatePrice(int price)

// NEW (added isProperty parameter)
public void UpdatePrice(int price, bool isProperty = true)
{
    if (isProperty && price > 0)
    {
        textPrice.text = $"${price}";
        textPrice.gameObject.SetActive(true);
    }
    else
    {
        textPrice.text = "";
        textPrice.gameObject.SetActive(false); // ← ⭐ Hide instead of just clearing text
    }
}
```

**Why SetActive(false)?**
- Clearing text (`text = ""`) vẫn render empty TextMeshPro component
- `SetActive(false)` completely hides GameObject → better performance
- Cleaner visual, không có empty text box

---

### 2. **PropertyVisual.cs**
**Location**: `Assets/Scenes/Game/Scripts/Visual/PropertyVisual.cs`

**Changes**: Update all `UpdatePrice()` calls to include `isProperty` parameter

```csharp
// Line ~80: When setting empty land rent
tile.UpdatePrice(rentPrice, true); // isProperty = true

// Line ~88: When setting rent after buying houses
tile.UpdatePrice(rentPrice, true); // isProperty = true

// Line ~114: When resetting to buy price
tile.UpdatePrice(buyPrice, true); // isProperty = true
```

**Why explicitly pass `true`?**
- PropertyVisual chỉ handle Property tiles
- Explicit parameter makes code intention clear
- Prepare for future refactor (nếu cần handle special tiles)

---

### 3. **TileDataAutoSetup.cs** (Editor Tool)
**Location**: `Assets/Scenes/Game/Scripts/Editor/TileDataAutoSetup.cs`

**Changes**: Update `UpdateTextComponents()` method

```csharp
private void UpdateTextComponents(SimpleTileData data)
{
    // TextName - Always visible
    if (textName != null)
    {
        textName.text = data.name;
        textName.gameObject.SetActive(true); // ← Always show name
        EditorUtility.SetDirty(textName);
    }

    // TextPrice - Only for Property tiles
    if (textPrice != null)
    {
        if (data.type == TileType.Property && data.basePrice > 0)
        {
            textPrice.text = $"${data.basePrice}";
            textPrice.gameObject.SetActive(true); // Show price
        }
        else
        {
            textPrice.text = "";
            textPrice.gameObject.SetActive(false); // ← ⭐ Hide for special tiles
        }
        EditorUtility.SetDirty(textPrice);
    }
    
    // ... assign references ...
}
```

**Auto Setup Flow**:
1. Select tile GameObject
2. Add TileDataAutoSetup component
3. Click "Setup This Tile" button
4. Script reads SimpleBoardConfig data
5. Sets TextName text (always)
6. Sets TextPrice text + visibility (based on TileType)
7. Assigns references to TileVisual

---

## 📊 TILE BREAKDOWN

### **28 Property Tiles** - Show Name + Price
```
Tokyo        Bangkok      Singapore    Manila       Jakarta
Beijing      Shanghai     Hong Kong    Taipei       Kuala Lumpur
Hanoi        Ho Chi Minh  London       Paris        Berlin
Rome         Madrid       Amsterdam    Vienna       New York
Los Angeles  Chicago      Toronto      Mexico City  São Paulo
Sydney       Da Nang      Seoul
```

### **8 Special Tiles** - Show Name Only (Price Hidden)
| Tile # | Name | Type |
|--------|------|------|
| 1 | Ô Bắt Đầu | Start |
| 7 | Ô Event | Event |
| 10 | Ô Tai Nạn | Jail |
| 16 | Ô Event | Event |
| 19 | Ô Tra Khảo | Quiz |
| 25 | Ô Event | Event |
| 28 | Ô Du Lịch | Travel |
| 33 | Ô Event | Event |

---

## 🎯 VISUAL COMPARISON

### Before (Wrong):
```
[Tokyo]          [Ô Event]        [Ô Tai Nạn]
 $800             $0               $0
 ↑                ↑                ↑
 OK            WRONG!           WRONG!
```

### After (Correct):
```
[Tokyo]          [Ô Event]        [Ô Tai Nạn]
 $800             (no price)       (no price)
 ↑                ↑                ↑
 ✅              ✅               ✅
```

---

## 🛠️ UNITY EDITOR USAGE

### Option 1: Manual Setup
```
For each tile:
1. Add TileVisual component
2. Create TextName (TextMeshPro) - always visible
3. Create TextPrice (TextMeshPro) - will be hidden if special tile
4. Assign to TileVisual fields
5. Code automatically handles visibility based on TileType
```

### Option 2: Auto Setup (Recommended!)
```
For each tile:
1. Add TileDataAutoSetup component
2. Click "Setup This Tile" button
3. Done! ✅

For all 36 tiles:
1. Create empty GameObject
2. Add TileDataAutoSetup component
3. Click "Setup ALL Tiles in Scene" button
4. Confirm
5. All 36 tiles setup automatically! 🎉
```

---

## 🎮 RUNTIME BEHAVIOR

### Property Tile (e.g., Tokyo - Tile 2)
```
Initial State:
- TextName: "Tokyo" ✅ Visible
- TextPrice: "$800" ✅ Visible

After Player Buys:
- TextName: "Tokyo" ✅ Still visible
- TextPrice: "$80" ✅ Shows rent (updated)
- Platform: Red color (Player 1)

After Adding House 1:
- TextName: "Tokyo" ✅ Still visible
- TextPrice: "$200" ✅ Shows new rent
- Houses: 1 red house spawned
```

### Special Tile (e.g., Ô Event - Tile 7)
```
Initial State:
- TextName: "Ô Event" ✅ Visible
- TextPrice: ❌ Hidden (SetActive(false))

Player Lands:
- TextName: "Ô Event" ✅ Still visible
- TextPrice: ❌ Still hidden
- Trigger: PanelEvent opens
```

---

## ✅ VERIFICATION CHECKLIST

### Code Level:
- [x] TileVisual.SetTileInfo() accepts TileType parameter
- [x] TileVisual.UpdatePrice() accepts isProperty parameter
- [x] PropertyVisual calls UpdatePrice() with isProperty = true
- [x] TileDataAutoSetup hides TextPrice for special tiles
- [x] All methods use SetActive(false) instead of just clearing text

### Unity Editor Level:
- [ ] All 36 tiles have TileVisual component
- [ ] All 36 tiles have TextName (TextMeshPro)
- [ ] All 36 tiles have TextPrice (TextMeshPro)
- [ ] Property tiles (28): TextPrice visible with correct price
- [ ] Special tiles (8): TextPrice hidden
- [ ] TextName visible for all tiles

### Runtime Level:
- [ ] Property tiles show price correctly
- [ ] Special tiles have no price visible
- [ ] Buying property updates price to rent
- [ ] Adding houses updates rent price
- [ ] TextPrice stays hidden on special tiles throughout game

---

## 🚨 COMMON ISSUES & FIXES

### Issue 1: Special tiles still show "$0"
**Cause**: Old code only checked `price > 0`, didn't check TileType  
**Fix**: ✅ Updated SetTileInfo() to check TileType parameter

### Issue 2: Empty TextPrice box visible on special tiles
**Cause**: Code cleared text but didn't hide GameObject  
**Fix**: ✅ Use `textPrice.gameObject.SetActive(false)`

### Issue 3: Auto setup doesn't hide price
**Cause**: TileDataAutoSetup needs update  
**Fix**: ✅ Updated UpdateTextComponents() to check TileType and SetActive(false)

### Issue 4: Compile error "TileType not found"
**Cause**: TileType enum in different namespace  
**Fix**: TileType defined in BoardManager.cs, already in AntKnow.Game namespace ✅

---

## 📝 NEXT STEPS

### Immediate (Unity Editor):
1. **Setup 36 tiles** using TileDataAutoSetup tool
2. **Verify** special tiles have TextPrice hidden
3. **Test in Play Mode** - check prices display correctly

### Short Term (Code):
1. Implement special tile behaviors:
   - Start: Give $200 bonus
   - Event: Random events (PanelEvent)
   - Jail: Skip turn
   - Quiz: Question (PanelQuiz)
   - Travel: Teleport

### Long Term (Polish):
1. Add icons to special tiles (instead of just text)
2. Animate TextPrice when updating rent
3. Add particle effects on special tiles
4. Sound effects for landing on special tiles

---

## 🎉 SUMMARY

**Problem**: Special tiles showed "$0" price, looked wrong  
**Solution**: Hide TextPrice GameObject for non-property tiles  
**Result**: Clean visuals - Property tiles show price, Special tiles show only name

**Files Changed**: 3 files (TileVisual.cs, PropertyVisual.cs, TileDataAutoSetup.cs)  
**Lines Modified**: ~30 lines total  
**Compilation**: ✅ No errors  
**Ready for**: Unity Editor setup

---

**Setup 36 tiles bây giờ sẽ rất dễ với TileDataAutoSetup tool! 🚀**
