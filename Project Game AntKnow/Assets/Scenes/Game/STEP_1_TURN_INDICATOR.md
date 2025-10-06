# Step 1: Tạo Turn Indicator (5 phút)

## Tạo Visual cho Turn Indicator:

### Trong Unity:

```
1. Hierarchy → Right-click → 3D Object → Sphere
   - Name: "TurnIndicatorPrefab"
   - Position: (0, 0, 0)
   - Scale: (0.5, 0.5, 0.5)

2. Tạo Material:
   - Project → Right-click → Create → Material
   - Name: "TurnIndicatorMat"
   - Color: Yellow (255, 255, 0)
   - Emission: Check ON
   - Emission Color: Yellow
   - Emission Intensity: 1

3. Apply Material:
   - Drag TurnIndicatorMat vào Sphere

4. Remove Collider:
   - Select Sphere
   - Remove Component: Sphere Collider

5. Add Script:
   - Add Component → TurnIndicator (script đã có)
   - Settings:
     * Bob Speed: 2
     * Bob Height: 0.3
     * Offset: (0, 2.5, 0)
     * Ping Object: Drag Sphere vào đây

6. Save as Prefab:
   - Drag TurnIndicatorPrefab từ Hierarchy vào Project/Prefabs/
   - Delete from Hierarchy

7. Add to Player Prefab:
   - Open Player Prefab
   - Drag TurnIndicatorPrefab vào Player (as child)
   - Position: (0, 2.5, 0)
   - PlayerGameController → Turn Indicator: Drag TurnIndicatorPrefab
   - Save Prefab
```

## Test:
```
Press Play → Check yellow sphere trên đầu player → Bobs up/down
```

