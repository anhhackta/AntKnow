# AntKnow Game Flow (Map 1 – Local Prototype)

## 1. Tầng Kiến Trúc

- **Domain (thuần C#):** `GameState`, `PlayerState`, `PropertyState`, `TurnSystem`, `BoardRules`, `PropertyEconomy`, `DiceRng`.
- **Data (ScriptableObject):** `BoardConfig` (36 ô theo thứ tự), `TileDef` (dữ liệu từng ô), `PropertyRuleSet` (bảng % nâng nhà/thuê/mua lại/khách sạn).
- **Presentation (MonoBehaviour):** `GameController` (cầu nối Domain ↔ UI), `PlayerController` (di chuyển theo waypoint), `DiceView` (hiệu ứng xúc xắc), `BoardView` (hiển thị map từ data).
- **Integration:** Firebase Auth/Quiz (stub cho tới khi import SDK).

## 2. Chu Trình Một Lượt Chơi

1. **Người chơi bấm Roll** → `GameController.OnRoll()` gọi `TurnSystem.Roll()` (RNG code, không dùng vật lý).
2. `PlayerController.MoveBySteps(sum)` di chuyển nhân vật qua 36 waypoint; đồng bộ `NodeIndex`.
3. Khi dừng lại, `TurnSystem.MoveAndResolve(sum)` xử lý ô theo `TileType`:
   - **Start:** đủ vòng sẽ cộng lương (`BoardRules.OnPassStart`).
   - **Property:** tùy trạng thái sẽ mở UI Mua/Nâng/Mua lại và tính tiền thuê.
   - **Bonus/Tax/Chance/Accident/Quiz/Travel/Jail/GoToJail:** đọc tham số từ `TileDef.amount` hoặc `destNode`.
4. `TurnSystem.EndTurn()` đổi sang người chơi kế tiếp. UI (`RefreshUI`) cập nhật lượt và tiền.

## 3. Hệ Thống Đất & Nâng Cấp

- **Mua đất:** `BoardRules.CanBuy` → `BoardRules.Buy`; giá gốc lấy từ `TileDef.basePrice`.
- **Nâng nhà (Level 0 → 5):**
  - `PropertyEconomy.UpgradeCost(basePrice, level+1)` trả về chi phí dựa trên % cấu hình (`PropertyRuleSet.upgradeCostPctByLevel`).
  - UI gọi `GameController.OnUpgradeHouseCurrent()` (đứng trên ô của mình) → `BoardRules.UpgradeHouse`.
- **Khách sạn:**
  - Khi đã ở Level 5, có thể lên `HasHotel = true` nếu đủ tiền (`PropertyEconomy.HotelCost`).
  - UI gọi `OnUpgradeHotelCurrent()` (đứng trên ô) hoặc `OnUpgradeHotelAt(tileId)` (ví dụ đứng ở Start chọn ô của mình).
- **Mua lại (Takeover):**
  - `BoardRules.CanTakeover` kiểm tra chủ khác, chưa là khách sạn (theo `PropertyRuleSet.takeoverAllowedOnHotel`).
  - Giá mua lại dùng `PropertyEconomy.TakeoverCost`. Gọi `OnTakeoverCurrent()` để thực hiện.
- **Tiền thuê:**
  - Land/House: `PropertyEconomy.Rent(basePrice, level, hasHotel)` theo bảng `rentPctByLevel`.
  - Khách sạn: `hotelRentPct`.
  - Chủ nhận thêm % theo Intelligence; người trả được giảm theo Resistance (đã xử lý trong `BoardRules`).

## 4. Cấu Hình Map 1 (36 Ô)

- Tạo asset `BoardConfig` với 36 `TileDef` theo thứ tự di chuyển (Start → … → Start).
- 4 góc đề xuất: `Start (0)`, `Jail (9)`, `FreeParking (18)`, `GoToJail (27)`.
- Chèn các ô sự kiện (Bonus/Tax/Quiz/Travel/Accident) đúng vị trí mong muốn; mọi giá trị đặt ở `TileDef.amount`.
- Ô nhà thông thường đặt `type = Property`, `basePrice` tùy zone; `PropertyState` sẽ tự tạo khi game init.
- Waypoints trên scene phải đúng 36 điểm và khớp thứ tự với `tiles`.

## 5. UI & Scene Wiring

1. Thêm GameObject gắn `GameController`, gán:
   - `board` → asset `BoardConfig` 36 ô.
   - `propertyRules` → asset `PropertyRuleSet`.
   - `players[]` → các `PlayerController` trong scene.
   - `turnText`, `p1Money` … nếu dùng TextMeshPro.
2. Mỗi `PlayerController` gán mảng `waypoints` (Transforms) đủ 36 node.
3. Nút Roll gọi `GameController.OnRoll()`.
4. Popup khi đứng trên ô:
   - Mua → `OnBuyCurrent()`
   - Nâng nhà → `OnUpgradeHouseCurrent()`
   - Nâng khách sạn → `OnUpgradeHotelCurrent()` / `OnUpgradeHotelAt(tileId)`
   - Mua lại → `OnTakeoverCurrent()`
5. Gỡ/disable các script demo cũ (`BaseScript`, `Player1Script`, `Player2Script`, `DiceScript*`).

## 6. Firebase (Tuần 2+)

- `FirebaseAuthController`: đăng nhập/đăng ký, tạo `profiles/{uid}`.
- `FirebaseQuizService`: đọc câu hỏi theo category.
- Cần import Firebase SDK và thêm define `ANTKNOW_USE_FIREBASE` để kích hoạt.
- Firebase lưu Auth, hồ sơ, inventory, quiz, kết quả trận; realtime match sẽ dùng NGO/Relay.

## 7. Multiplayer (NGO + UGS)

- **Hạ tầng:** Netcode for GameObjects (NGO) + Unity Gaming Services (Lobby & Relay).
- **Scripts chính:**
  - `Assets/Script/Multiplayer/NetworkPlayerController.cs` — bọc `PlayerController`, host điều khiển chuyển động.
  - `Assets/Script/Multiplayer/NetworkGameController.cs` — host giữ Domain, client gửi yêu cầu qua `ServerRpc`, đồng bộ state bằng `NetworkList`. (Struct `PlayerData/PropertyData` implement `INetworkSerializable + IEquatable` theo yêu cầu của NGO.)
  - `Assets/Script/Integration/UgsLobbyRelayService.cs` — khởi tạo UGS, tạo/join Lobby, cấp Relay, cấu hình `UnityTransport`, gọi `NetworkManager.StartHost/Client()`.
- **Setup scene:**
  1. Thêm `NetworkManager` + `UnityTransport` + `UgsLobbyRelayService`.
  2. Thả `NetworkGameController` (gán BoardConfig, PropertyRuleSet, UI texts, mảng `NetworkPlayerController`).
  3. Mỗi nhân vật: `NetworkObject` + `NetworkTransform` + `NetworkPlayerController` + `PlayerController` (gán waypoint 36 ô).
- **Vòng đời lobby:** Host gọi `CreateLobbyAsync` → nhận join code → share cho client; client dùng `JoinLobbyByCodeAsync` hoặc `QuickJoinAsync`.
- **Luồng gameplay:** host chạy Domain (`GameState`, `TurnSystem`); client bấm Roll/Buy/Upgrade gọi `Request*()` → `ServerRpc` xác thực → host cập nhật state → `NetworkList` phát tới client → UI cập nhật.
- **Mở rộng:** `DiceRolledClientRpc` là hook để phát hiệu ứng; có thể thêm `ClientRpc` cho popup mua/nâng.

## 8. Thẻ bài & Quiz

- **Deck server-side:** `CardLibrary` (ScriptableObject) nạp danh sách thẻ; `CardDeckService` trộn và phát khi người chơi rơi vào ô Event (`TileType.Chance/Bonus/Accident`).
- **Loại thẻ:**
  - `CardType.Passive` (kích hoạt tự động tùy `CardTrigger`, ví dụ StartOfTurn, OnQuizFail).
  - `CardType.Active` (người chơi chủ động sử dụng → `NetworkGameController.RequestUseCard(cardId)`).
- **Luồng sự kiện:**
  1. Server gọi `DrawEventCardServerSide` khi Landing Event → broadcast `CardInventoryUpdated` & `CardDrawnClientRpc` cho UI.
  2. UI client render danh sách thẻ, gửi `RequestUseCard` khi muốn kích hoạt.
  3. `CardRuleEngine` áp dụng hiệu ứng (tiền, stat, v.v.), cập nhật `PlayerState` và đồng bộ ngược về client.
- **Quiz tile (Tra khảo):** server gọi `QuizRequested` event + `RequestQuizClientRpc(playerId, tileId)` → UI client mở popup quiz (đọc từ Firebase). Kết quả trả về server (ServerRpc riêng) để thưởng/phạt (`BoardRules` hoặc card penalty).
- **Cooldown passive:** `PlayerState.PassiveCooldown[cardId]` lưu lượt còn lại; `TurnSystem.StartTurn()` giảm cooldown và kích hoạt thẻ khi đủ điều kiện.

## 9. Dedicated Server Checklist

1. Build headless Linux scene `SceneGame` với `NetworkManager` cấu hình server-only (`StartServer` + Relay join code do Multiplay cấp).
2. Deploy qua Unity Multiplay (Game Server Hosting) → cung cấp build, config launch args, script đăng ký Relay (qua Cloud Code / matchmaker).
3. Menu/Matchmaking (MenuScene) gọi Cloud Code để spawn máy chủ, nhận Relay join code và Lobby info → client dùng `JoinLobbyByCodeAsync` + `UnityTransport.SetRelayServerData`.
4. Đảm bảo Firebase Auth đã đăng nhập trước khi join (scene Login). UID được dùng để đồng bộ profile, inventory, deck.
5. Khi trận kết thúc, server report kết quả (coins, XP, quiz points) lên Cloud Functions/Firebase Firestore.

## 8. Lưu Ý & Mở Rộng

- `PropertyRuleSet` cho phép tinh chỉnh nhanh các bảng % mà không sửa code.
- Có thể nhân bản `BoardConfig`/`PropertyRuleSet` cho Map 2 hoặc chế độ khác.
- `TurnSystem` hiện xử lý lượt cơ bản; tuần sau có thể bổ sung penalty jail, skill card, Luck/Agility modifiers.
- `DiceView` chỉ log kết quả → cần gắn animation/FX sau.
- `BoardView` là stub để sau này spawn prefab ô từ data.
