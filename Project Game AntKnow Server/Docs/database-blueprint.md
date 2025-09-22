# AntKnow Backend Database Blueprint

This document consolidates the key requirements for the Firebase/Firestore schema that supports the AntKnow multiplayer board game. The backend must integrate with **Netcode for GameObjects** and **Unity Gaming Services** (Relay, Lobby, and future Matchmaker), while ensuring the Login → Menu → Game scene flow behaves exactly as designed.

## 1. High-level collections

Collection names use lowercase with underscores. Field types follow the notation `S = string`, `N = number`, `B = boolean`, `T = timestamp`, `M{}` = map, `A<>` = array.

### 1.1 `users/{uid}` (Firebase Auth UID as document id)

| Field | Type | Notes |
| --- | --- | --- |
| `username` | S | Unique login handle. Stored in lowercase in `handles`. |
| `email` | S | Matches Firebase Auth account. |
| `emailVerified` | B | Reflects Auth state. |
| `ingameName` | S\|null | First-time users leave null; assigned in Menu scene. |
| `langPref` | S | e.g. `"vi"`. |
| `createdAt`, `lastLoginAt` | T | Audit fields. |
| `level` | N | Player level. |
| `xp` | N | Current EXP toward next level. |
| `elo` | N | Starts at 1000. Used for matchmaking. |
| `rankEligible` | B | true when level ≥ min requirement and not banned. |
| `rankLockReason` | S | Message explaining lock state. |
| `currencies` | M{`antCoin`:N, `dcoin`:N} | Soft (antCoin) and premium (dcoin) wallets. |
| `stats` | M{`matchesPlayed`:N, `wins`:N} | Core progression stats. |
| `powerScoreCache` | N | Optional pre-calculated power for matchmaking. |
| `status` | S | `"active"` or `"banned"`. |

Subcollections:

* `users/{uid}/inventory/{entryId}` — equipment, cards, consumables.
  * Fields: `itemId` (S), `type` (`"skill_card"|"exp_card"|"equipment"|"material"|"repair_hammer"`), `qty` (N for stackables), `level` (N), `stars` (N), `durability` (N for equipment only), timestamps, `status` (`"active"`).
* `users/{uid}/loadouts/{slotId}` — saved combat setups.
  * Fields: `active` (B), `skillCards` map (`passiveId`, `activeId`), `equipmentSet` map (hat, shirt, wings, shoes, mask slots), `updatedAt` (T).
* (Optional) `users/{uid}/quiz_progress/{questionId}` — quiz question history with `answeredAt` (T) and `correct` (B).

### 1.2 `handles/{usernameLower}`

Map lowercase usernames to their Auth account. Enables “login with username” despite Firebase using email/password.

| Field | Type | Notes |
| --- | --- | --- |
| `uid` | S | Firebase Auth UID. |
| `email` | S | Email to forward to `SignInWithEmailAndPassword`. |

### 1.3 `ingame_names/{ingameLower}`

Ensures unique display names set during the first visit to the Menu scene.

| Field | Type | Notes |
| --- | --- | --- |
| `uid` | S | Owner of the ingame name. |

### 1.4 `quizzes/{questionId}`

Question bank for the trivia phase.

| Field | Type | Notes |
| --- | --- | --- |
| `question` | S | Text prompt. |
| `options` | `A<S>` | Exactly four entries. |
| `correctIndex` | N | 0–3. |
| `topic`, `difficulty`, `lang` | S | Filtering fields. |
| `status` | S | `"active"` or `"inactive"`. |
| `randomValue` | N | Pre-generated float 0..1 for random fetches. |
| `createdAt`, `updatedAt` | T | Maintenance. |
| `tags` (optional) | `A<S>` | Additional metadata. |
| `authorUid` (optional) | S | Creator reference. |
| `stats` (optional) | M{`usedCount`:N, `correctCount`:N, `lastUsedAt`:T} | Usage metrics. |

### 1.5 `items/{itemId}`

Unified catalogue for equipment, skill cards, and consumables.

| Field | Type | Notes |
| --- | --- | --- |
| `name` | S | Base name. |
| `type` | S | `"skill_card"|"exp_card"|"equipment"|"material"|"repair_hammer"`. |
| `rarity` | S | Game-defined rarity tiers. |
| `status` | S | `"active"` or `"retired"`. |
| `attributes` | M{stat:N} | Intelligence, health, agility, luck, resistance bonuses. |
| `skill` | M{`mode`:S, `effect`:S, `cooldownTurns`:N} | Only for skill cards. |
| `equipment` | M{`slot`:S, `durabilityMax`:N} | Equipment slot metadata. |
| `exp` | M{`xpValue`:N} | EXP granted for exp cards. |
| `upgrade` | M{`costAntCoinPerLevel`:N, `preferCardExp`:B} | Upgrade rules. |
| `evolution` | M{`materials`: array of maps `{itemId:S, qty:N}`, `useSameTypeEquipment`:B} | Evolution recipe. |
| `lang` | M{`vi`:M{name,desc}, `en`:M{name,desc}} | Localised texts. |
| `icon` | S | CDN path or storage key. |
| `createdAt`, `updatedAt` | T | Maintenance fields. |

### 1.6 `event_cards/{cardId}`

Definitions for board events and instant effects.

| Field | Type | Notes |
| --- | --- | --- |
| `name` | M{locale:S} | Localised names. |
| `type` | S | `"board_event"|"instant"|"buff"`. |
| `effectId` | S | Key understood by game logic. |
| `params` | M{} | Effect parameters. |
| `rarity` | S | Loot weighting. |
| `status` | S | `"active"` or `"inactive"`. |
| `createdAt`, `updatedAt` | T | Maintenance. |

### 1.7 `configs/gameplay`

Single configuration document storing tunable gameplay constants.

* `version` (S)
* `exp.xpPerLevel` (N)
* `rank.minLevel` (N), `rank.globalLock` (B)
* `rank.elo` map (`start`, `winDelta`, `lossDelta`)
* `rank.tiers` array of `{min:N, name:S}`
* `rank.matchmaking.powerTolerance` (N)
* `durability.lossPerMatch` (N), `durability.minToApplyStats` (N)
* `evolution.card` / `evolution.equipment` arrays of `{stars:N, baseRate:N}`

### 1.8 Match, economy, and seasonal data

* `matches/{matchId}` — post-match audit log with metadata, participant snapshots, and used question list.
* `leaderboards_seasons/{seasonId}/entries/{uid}` — seasonal ladder snapshots (`elo`, `points`, `wins`, `played`, `updatedAt`).
* `transactions/{txId}` — authoritative AntCoin/DCoin ledger entries.
* `orders/{orderId}` — in-app purchase receipts (future premium store).

## 2. Scene-driven flows

### 2.1 Login scene

1. **Sign-up**
   1. Collect `username`, `email`, `password`, `confirmPassword`.
   2. Call `CreateUserWithEmailAndPassword` → receive `uid`.
   3. Create `users/{uid}` with base stats (`level=1`, `xp=0`, `elo=1000`, `rankEligible=false`, `ingameName=null`, wallet defaults).
   4. Reserve username by writing `handles/{usernameLower}` with `{uid,email}` (fail transactionally if taken).
   5. Optionally set `presence/{uid}` = `{state:"login", updatedAt:T}` for online indicator.

2. **Sign-in**
   * If user typed an email → call `SignInWithEmailAndPassword(email, password)`.
   * If user typed a username → read `handles/{usernameLower}.email` then sign in with that email.
   * After Auth, fetch `users/{uid}`:
     * If `ingameName` exists → display name + online status directly on login screen; **Start** button loads Menu scene.
     * If `ingameName` is null → allow Start to proceed, Menu scene will enforce name selection.

### 2.2 Menu scene

* On first entry without `ingameName`, prompt for a unique name. Use a transaction / Cloud Function to create `ingame_names/{ingameLower}` and patch `users/{uid}.ingameName` atomically.
* Subscribe to `users/{uid}` and `configs/gameplay` for live updates (currencies, stats, restrictions).
* Core UI panels:
  1. **Find Match** — use selected loadout, compute `powerScore` (ignore equipment whose `durability <= 0`). Enable ranked mode only if `level ≥ configs.rank.minLevel`, `rankEligible == true`, and `rank.globalLock == false`.
  2. **Inventory** — list `inventory` subcollection, equip/unequip items, attach skill cards by updating `loadouts`.
  3. **Upgrade** — spend `exp_card` items or sacrifice duplicates to increase `level`/`stars`. Equipment durability decreases after matches; allow `repair_hammer` usage.
  4. **Shop** — fetch catalogue from `items` (or dedicated `shops` collection) and call secure Cloud Functions to process purchases.
  * (Future) Daily quests: add `daily_quests_def` and `users/{uid}/daily_quests/{yyyymmdd}` when feature arrives.

### 2.3 Game scene

* Receive chosen loadout + `powerScore` from Menu.
* When a match ends, send the full result payload to a trusted Cloud Function which will:
  * Persist `matches/{matchId}` log entries.
  * Adjust player `elo` (ranked only) based on config deltas.
  * Grant EXP and level-ups; clamp `xp` modulo `exp.xpPerLevel`.
  * Deduct durability for used equipment and set to zero when broken.
  * Credit currency rewards and record `transactions/{txId}` entries.

## 3. Recommended indexes

* `quizzes` — composite indexes on `(status ASC, lang ASC, randomValue ASC)` and optionally `(status, topic, difficulty, lang, randomValue)`.
* `leaderboards_seasons/{season}/entries` — single-field index on `elo DESC` or `points DESC`.
* `items` — single-field index on `type`, `rarity`, and `status`.

## 4. Security rule considerations

* `users/{uid}` and subcollections: allow read/write to owner only; block direct client edits of sensitive fields (`currencies`, `elo`, `level`, `xp`, `rankEligible`, `durability`, etc.). These updates must route through Cloud Functions with admin privileges.
* `handles`, `ingame_names`: writes only through Cloud Functions or callable endpoints to guarantee uniqueness and prevent abuse.
* `quizzes`, `items`, `event_cards`, `configs`: read allowed to all clients; writes restricted to admin roles.
* `matches`, `transactions`, `orders`: only trusted server processes or users with specific custom claims may create entries.

## 5. Implementation checklist

1. Seed `configs/gameplay` with initial EXP, ranking, durability, and evolution values.
2. Prepare empty collections `handles`, `ingame_names`, and `users` (Firestore auto-creates on first write).
3. Import baseline content: at least a handful of documents for `items`, `quizzes`, and `event_cards` for UI testing.
4. Build Cloud Functions for upgrades, purchases, post-match settlement, and username/ingame name reservation.
5. Define Firestore rules and indexes before shipping alpha builds to avoid runtime failures.

This blueprint keeps the Login → Menu → Game pipeline consistent while leaving space for future Netcode, Relay, Lobby, and Matchmaker integration layers.
