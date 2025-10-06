# Step 2: Tạo House & Hotel Models (10 phút)

## Tạo Simple House Model:

### Trong Unity:

```
1. Tạo House Prefab:
   - Hierarchy → Right-click → 3D Object → Cube
   - Name: "HousePrefab"
   - Scale: (0.8, 1.2, 0.8)
   - Position: (0, 0.6, 0) - Đứng trên mặt đất

2. Tạo Roof (Mái nhà):
   - Right-click HousePrefab → 3D Object → Cube
   - Name: "Roof"
   - Scale: (1, 0.3, 1)
   - Position: (0, 0.75, 0)
   - Rotation: (0, 45, 0) - Xoay 45 độ

3. Tạo Material:
   - Project → Create → Material
   - Name: "HouseMat"
   - Color: White (sẽ đổi màu theo player)

4. Apply Material:
   - Drag HouseMat vào HousePrefab và Roof

5. Remove Colliders:
   - Select HousePrefab → Remove Box Collider
   - Select Roof → Remove Box Collider

6. Save as Prefab:
   - Drag HousePrefab vào Project/Prefabs/
   - Delete from Hierarchy
```

## Tạo Hotel Model:

```
1. Tạo Hotel Prefab:
   - Hierarchy → Right-click → 3D Object → Cube
   - Name: "HotelPrefab"
   - Scale: (1.2, 2, 1.2)
   - Position: (0, 1, 0)

2. Tạo Sign (Biển hiệu):
   - Right-click HotelPrefab → 3D Object → Cube
   - Name: "Sign"
   - Scale: (1.5, 0.2, 0.1)
   - Position: (0, 1.5, 0.6)

3. Tạo Material:
   - Project → Create → Material
   - Name: "HotelMat"
   - Color: Gold (255, 215, 0)

4. Apply Material:
   - Drag HotelMat vào HotelPrefab và Sign

5. Remove Colliders:
   - Remove all Box Colliders

6. Save as Prefab:
   - Drag HotelPrefab vào Project/Prefabs/
   - Delete from Hierarchy
```

## Test:
```
Drag HousePrefab và HotelPrefab vào Scene → Check visual
```

