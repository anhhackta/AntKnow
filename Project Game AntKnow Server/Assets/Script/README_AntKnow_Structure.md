AntKnow Code Structure (Week 1)

- Domain (pure C#): core game logic and state
  - Enums.cs
  - Entities/ (PlayerState, PropertyState, GameState)
  - Services/ (DiceRng, BoardRules, TurnSystem)

- Data: ScriptableObjects
  - TileDef.cs
  - BoardConfig.cs
  - PropertyRuleSet.cs (bảng % chi phí/nước thuê/mua lại/khách sạn)

- Presentation (Unity Monobehaviours):
  - PlayerController.cs (unified movement by nodeIndex)
  - GameController.cs (bridge Domain ↔ UI; Roll → Move → Resolve → EndTurn)
  - DiceView.cs (dice animation placeholder)
  - BoardView.cs (optional; future: instantiate board from BoardConfig)

- Integration:
  - FirebaseAuthController.cs (stubbed until SDK; enable with ANTKNOW_USE_FIREBASE)
  - FirebaseQuizService.cs (stubbed until SDK)

Scene wiring (quick):
- Create a BoardConfig asset (Assets/Create/AntKnow/BoardConfig). Fill 32 tiles in order.
- Add a GameObject with GameController; assign BoardConfig, PlayerController array, and optional TextMeshPro fields.
- On each Player GameObject, add PlayerController and assign the waypoints array (path around the board).

Notes:
- Old demo scripts (BaseScript, Player1Script, Player2Script, Dice* scripts) are kept but should be replaced by the new flow.
- Physical dice are deprecated in favor of DiceRng for deterministic networking later.

Property & Hotel Rules (36 ô, nhà 1–5 + Khách sạn)
- Data hoá qua `PropertyRuleSet` để dễ tinh chỉnh:
  - `upgradeCostPctByLevel[1..5]`: % giá gốc để nâng lên mỗi mức nhà.
  - `rentPctByLevel[0..5]`: % giá gốc thành tiền thuê theo mức nhà (0 = chưa xây).
  - `hotelUpgradePct`, `hotelRentPct`: % giá gốc cho chi phí lên khách sạn và tiền thuê khách sạn.
  - `takeoverPctByLevel[0..5]`: % giá gốc để mua lại (chỉ áp dụng khi chưa là khách sạn).
  - `takeoverAllowedOnHotel=false`: bật/tắt việc cho phép mua lại khách sạn (theo luật: mặc định KHÔNG cho).

- Luật triển khai trong Domain (`BoardRules` + `PropertyEconomy`):
  - Mua đất: `CanBuy/Buy` khi ô chưa có chủ.
  - Nâng nhà: `CanUpgradeHouse/UpgradeHouse` từ 0→1 … đến 5 nếu đủ tiền.
  - Khách sạn: `CanUpgradeHotel/UpgradeHotel` khi đang ở mức 5 và đủ tiền; gọi khi bạn đứng trên ô nhà của mình hoặc khi ở ô Start (UI trigger).
  - Mua lại (takeover): `CanTakeover/BuyTakeover` chỉ cho phép khi chưa là khách sạn; giá mua lại theo `takeoverPctByLevel`.
  - Tiền thuê: `CalcRent` dùng bảng % theo nhà/khách sạn (+% từ Intelligence của chủ và giảm từ Resistance của người trả).

UI Hook (trong `GameController`):
- `OnBuyCurrent`, `OnUpgradeHouseCurrent`, `OnUpgradeHotelCurrent`, `OnTakeoverCurrent` — gọi từ popup khi người chơi đứng trên ô.

Board 36 ô:
- Tạo `BoardConfig` có `tiles.Length = 36` theo thứ tự đi quanh bàn (4 góc đặc biệt + ô sự kiện + ô Property). Waypoints của người chơi cũng cần đủ 36 node theo đúng thứ tự.
