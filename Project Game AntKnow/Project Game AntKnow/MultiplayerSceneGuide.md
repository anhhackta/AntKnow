# Multiplayer Scene Deployment Guide (NGO + UGS)

## 1. Chuẩn Bị Packages

- Import các package Unity:
  - **Netcode for GameObjects** (com.unity.netcode.gameobjects)
  - **Unity Transport** (tự động với NGO)
  - **Authentication**, **Lobby**, **Relay** (Unity Gaming Services)
  - (Tuỳ chọn) **Multiplayer Playmode Tools** để debug.

## 2. Cấu Trúc Scene Multiplayer (SceneGame)

1. **NetworkManager**
   - GameObject “NetworkManager” → add `NetworkManager` component.
   - Add `UnityTransport` (chọn Protocol: DTLS).
   - (Nếu dùng addressable spawn) cấu hình player prefab tại `NetworkManager` → `Player Prefab`.

2. **UgsLobbyRelayService** (Assets/Script/Integration)
   - Add component lên NetworkManager (hoặc empty GameObject).
   - Expose các hàm public `CreateLobbyAsync`, `QuickJoinAsync`, `JoinLobbyByCodeAsync`, `LeaveLobbyAsync` cho UI.
   - Chỉnh `environmentName` (ví dụ “production”), `gameVersion`.

3. **NetworkGameController** (Assets/Script/Multiplayer)
   - GameObject “GameController” → add `NetworkObject` + `NetworkGameController`.
   - Gán dữ liệu:
     - `BoardConfig` (36 ô: Start, Jail, Travel, Quiz ở 4 góc + event/ô thường).
     - `PropertyRuleSet` (asset bảng giá/thuê/nâng nhà/khách sạn).
     - `CardLibrary` (danh sách thẻ bài chủ động/bị động, dùng cho ô event rút bài).
     - `PlayerControllers` → tham chiếu tới từng `NetworkPlayerController` trong scene.
     - `TurnText`, `MoneyTexts[]` → TextMeshPro UI.

4. **Mỗi Player**
   - Tạo GameObject “PlayerX”.
   - Components: `NetworkObject`, `NetworkTransform`, `PlayerController`, `NetworkPlayerController`.
   - Thiết lập `PlayerController`:
     - `waypoints` → array 36 transform theo thứ tự bàn cờ.
     - `moveSpeed` theo mong muốn.

5. **Waypoints**
   - Tạo empty parent `Waypoints` chứa 36 Transform đánh số theo đường đi.
   - Dùng chung cho các `PlayerController`.

## 3. UI & Luồng Multiplayer

- **Lobby màn hình chính**
  - Buttons:
    - “Host” → gọi `UgsLobbyRelayService.CreateLobbyAsync(lobbyName, maxPlayers)` (qua UnityEvent + `async void` wrapper).
    - “Quick Join” → gọi `QuickJoinAsync()`.
    - “Join Code” → input + `JoinLobbyByCodeAsync(code)`.
    - “Leave” → `LeaveLobbyAsync()`.

- **Trong trận**
  - Buttons sử dụng `NetworkGameController`:
    - Roll → `RequestRoll()`
    - Buy → `RequestBuy()`
    - Upgrade House/Hotel → `RequestUpgradeHouse()`, `RequestUpgradeHotel()`
    - Takeover → `RequestTakeover()`
    - Draw Event (ô Chance/Bonus/Accident) → `RequestDrawEventCard()` (có thể gọi tự động từ `NetworkGameController`).
    - Use Card → `RequestUseCard(cardId)` (UI lọc danh sách thẻ chủ động).
  - Dice FX: subscribe vào `DiceRolledClientRpc` (ví dụ thêm `DiceView.ShowRoll`).
  - Quiz UI: subscribe `NetworkGameController.QuizRequested` → hiện popup câu hỏi (đọc từ Firebase) → báo lại server (ServerRpc riêng) để xử lý thưởng/phạt.
  - Card UI: subscribe `NetworkGameController.CardInventoryUpdated` để refresh ô thẻ (chia Passive/Active, cooldown hiển thị từ `PlayerState.PassiveCooldown`).

## 4. Domain & Đồng Bộ

- Host giữ toàn bộ `GameState`/`TurnSystem`.
- Client chỉ gửi yêu cầu (RPC) → host kiểm tra, cập nhật → `NetworkList<PlayerData/PropertyData>` broadcast + `CardInventory` gửi qua `ClientRpc`.
- Struct `PlayerData` & `PropertyData` implement `INetworkSerializable` + `IEquatable` (bắt buộc với `NetworkList`).
- `NetworkPlayerController` chỉ nhận lệnh từ server (di chuyển, đặt vị trí node).
- Deck thẻ bài (`CardDeckService`) và hiệu ứng (`CardRuleEngine`) chạy server-side; kết quả broadcast về client.
- `TurnSystem` gọi `CardRuleEngine.ApplyPassiveStartOfTurn` để auto-kích hoạt thẻ bị động (ví dụ buff/nerf).

## 5. Quy Trình Test

1. Mở Unity → Multiplayer Playmode: một Editor (host), một build (client) hoặc 2 processes Playmode.
2. Host chọn “Host” → Lobby tạo + Relay assign → `NetworkManager.StartHost()`.
3. Client nhập join code → join thành công → `NetworkManager.StartClient()`.
4. Cả hai màn hình vào scene → kiểm tra UI tiền/lượt cập nhật theo `NetworkGameController`.
5. Roll thử: host hoặc client (đang tới lượt) bấm Roll → mọi máy di chuyển đồng bộ.
6. Đặt nhân vật vào ô Event (Chance/Bonus/Accident) → kiểm tra card draw broadcast.
7. Đặt nhân vật vào ô Quiz → popup quiz xuất hiện (client + host) và server chờ kết quả.

## 6. Các Lưu Ý

- Nhớ cấu hình **UGS Project ID** trong Project Settings → Services.
- Authentication đang dùng `SignInAnonymouslyAsync`; nếu cần profile thực, gắn email/3rd party.
- Lobby Heartbeat chạy mỗi 15s; khi host thoát → lobby giải phóng.
- Đảm bảo `Scripting Define Symbols` chứa `ANTKNOW_USE_FIREBASE` khi tích hợp Firebase (không ảnh hưởng NGO).
- Dọn các script demo cũ khỏi scene (dice vật lý, BaseScript) để tránh conflict.
- Build Settings: add cả scene lobby/menu và scene map multiplayer.

- Map 36 ô:
  - Góc 0: Bắt đầu (Start).
  - Góc 9: Ở Tù (Jail visit / nơi bị chuyển khi phạm lỗi).
  - Góc 18: Du Lịch (Travel) → server đặt node theo `destNode` hoặc UI chọn.
  - Góc 27: Tra Khảo (Quiz) → kích hoạt câu hỏi Firebase.
  - 4 ô event rút bài: đánh dấu `TileType.Chance`/`Bonus`/`Accident` để `NetworkGameController` tự rút thẻ.
- Dedicated Server: build headless (Linux server) chạy scene này, dùng Multiplay/UGS để khởi host tự động (NetworkManager.StartServer + Relay allocation via cloud orchestration).

## 7. Bước Tiếp Theo

- Thêm UI cho danh sách người chơi (đọc `Lobby.Players`).
- Sync trực quan nâng nhà/khách sạn bằng mesh/material (theo `PropertyData` updates).
- Gắn quiz popup qua Firebase khi TileType = Quiz; trả lời đúng/sai gọi `ServerRpc` để thưởng/phạt (tiền, nhà).
- Tích hợp NGO Snapshot interpolation nếu cần mượt hơn.
- Tạo hệ thống skill/thẻ nâng cấp (cooldown hiển thị UI, thẻ bị động auto kích hoạt).
