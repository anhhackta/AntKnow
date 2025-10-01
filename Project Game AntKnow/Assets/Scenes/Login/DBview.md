* **Tất cả collections & subcollections** (Doc ID, field + kiểu dữ liệu)
* **Security Rules** (đã sửa cú pháp, build được)
* **4 Cloud Functions** (TypeScript) cho: mua shop, nâng cấp card, tiến hoá card, thưởng sau trận
* **C# Unity điểm chạm** (đăng nhập username/email, tạo user doc, set loadout, gọi CF, lấy quiz random)

---

# 1) Firestore schema (chuẩn gọn cho Login → Menu → Game)

## 1.1 `configs/gameplay`  (tạo tay)

* **Collection**: `configs` → **Document ID**: `gameplay`
* **Fields (Kiểu → Ví dụ)**

  * `version` (string) → `"1.0.0"`
  * `featureFlags` (map) → `{ durabilityEnabled: false }`
  * `exp` (map) → `{ xpPerLevel: 1000 }` *(dùng cho **upgrade card**)*
  * `cards` (map):

    * `maxEquipped` (map) → `{ total: 2, allowDuplicates: false }`
    * `upgrade` (map) → `{ feedSlots: 5, attributePerLevel: 2 }` *(mỗi **level** +2 vào **primaryStat**)*
    * `evolution` (map) →

      * `levelThresholds` (array<number>) → `[10,20,30,40,50]`
      * `cooldownReductionByStar` (array<number>) → `[0,1,2,3,4,5]` *(stars=0..5)*
      * `maxStars` (number) → `5`
      * `baseRateByNextStar` (array<number>) → `[1,0.8,0.6,0.4,0.2]`
  * `equipment` (map) → `{ slots: { hat:true, shirt:true, wings:true, shoes:true, mask:true } }`
  * `match` (map) → `{ minDurationSec:90, rewardAntCoin:{p1:500,p2:300,p3:200,p4:100}, rewardXp:{p1:300,p2:200,p3:150,p4:100} }`
  * `updatedAt` (timestamp, server)

> **Công thức trong game**
>
> * **stat card hiệu dụng** = `baseAttr + (level-1)*attributePerLevel`
> * **cooldown card hiệu dụng** = `max(1, cooldownBaseTurns - cooldownReductionByStar[stars])`

---

## 1.2 `items` (tạo tay – từ điển vật phẩm; **Doc ID = slug**)

### A) Equipment (5 món cơ bản)

* **Doc ID ví dụ**: `equip.hat.basic` (các slot khác tương tự)
* **Fields**:

  * `type` (string) → `"equipment"`
  * `status` (string) → `"active"`
  * `equipment` (map) → `{ slot: "hat" }` *(hoặc "shirt"|"wings"|"shoes"|"mask")*
  * `attributes` (map) → `{ health:2, agility:0, intelligence:0, luck:0, resistance:0 }` *(tuỳ)*
  * *(tuỳ)* `icon` (string URL)

> **Giai đoạn này không upgrade/evolve equipment** — mặc vào là cộng chỉ số.

### B) Skill cards (4 lá)

* **Doc ID**: `skill.lan-tron`

  * `type:"skill_card"`, `status:"active"`
  * `attributes:{ agility:10 }`
  * `skill:{ mode:"passive", primaryStat:"agility", cooldownBaseTurns:5, triggerId:"onEnterOpponentHouse", effectId:"autoStepForward", params:{ step:1 } }`
* **Doc ID**: `skill.sieu-sale`

  * `attributes:{ intelligence:10 }`
  * `skill:{ mode:"passive", primaryStat:"intelligence", cooldownBaseTurns:5, triggerId:"onTryPurchaseProperty", effectId:"purchaseDiscount", params:{ rate:0.3 } }`
* **Doc ID**: `skill.bao-ke`

  * `attributes:{ health:10 }`
  * `skill:{ mode:"active", primaryStat:"health", cooldownBaseTurns:8, effectId:"protectProperty", params:{ durationTurns:1 } }`
* **Doc ID**: `skill.cham-chi`

  * `attributes:{ luck:10 }`
  * `skill:{ mode:"active", primaryStat:"luck", cooldownBaseTurns:6, effectId:"extraStartSalary", params:{ multiplier:1.0 } }`

> `primaryStat` = **tên chỉ số chính** mà lá cộng và **tăng theo level**.

### C) EXP cards

* `exp.small` → `{ type:"exp_card", status:"active", exp:{ xpValue:500 } }`
* `exp.large` → `{ type:"exp_card", status:"active", exp:{ xpValue:2000 } }`

---

## 1.3 `shops` (tạo tay – shop đơn giản)

* **Doc**: `shops/default` → `{ title:"Shop Cơ Bản", status:"active" }`
* **Subcollection**: `shops/default/entries/*` (mỗi doc là 1 món)

  * `exp_small` → `{ itemId:"exp.small", type:"exp_card", stackable:true, qtyPerPurchase:1, priceAntCoin:200 }`
  * `hat_basic` → `{ itemId:"equip.hat.basic", type:"equipment", priceAntCoin:800 }`
  * `bao_ke` → `{ itemId:"skill.bao-ke", type:"skill_card", priceAntCoin:1200 }`
  * *(nếu bán bằng dCoin: `priceDCoin: xxx`)*

---

## 1.4 `quizzes` (tạo tay/import – **Doc ID = Auto**)

Fields:
`createdAt`(timestamp), `topic`(string), `question`(string), `difficulty`(string: easy/medium/hard), `options`(array 4 string), `correctAnswer`(number 0..3), `randomValue`(number 0..1)

---

## 1.5 `users/{uid}` và subcollections (code tạo)

* **Doc ID**: `uid` từ Firebase Auth
* **Fields**:
  `username`(string), `email`(string), `emailVerified`(bool),
  `ingameName`(string|null),
  `createdAt`(timestamp), `lastLoginAt`(timestamp),
  `level`(number=1), `xp`(number=0),
  `gender`(string),
  `currencies` ( map ) : `antCoin`(number=0), `dCoin`(number=0),
  `stats`(map:{matchesPlayed:number=0, wins:number=0}),
  `status`(string="active")

### A) `users/{uid}/inventory/*`

* **Card instance** (Doc ID = **Auto ID**):
  `{ type:"skill_card", itemId:"skill.lan-tron", level:1, stars:0, createdAt, updatedAt }`
* **Equipment instance** (Doc ID = **Auto ID**):
  `{ type:"equipment", itemId:"equip.hat.basic", createdAt, updatedAt }`
* **Stackable EXP** (Doc ID = **itemId** → `exp.small`):
  `{ type:"exp_card", itemId:"exp.small", qty:10, updatedAt }`

### B) `users/{uid}/loadouts/slot1`

`{ active:true, skillCardIds:["cardInstIdA","cardInstIdB"], equipmentSet:{ hatId, shirtId, wingsId, shoesId, maskId }, updatedAt }`

> **Client phải chặn**: 2 `skillCardIds` không trỏ tới **cùng `itemId`**.

### C) `handles/{usernameLower}` (code tạo khi đăng ký)

`{ uid, email }`

### D) `ingame_names/{ingameLower}` (code tạo khi đặt ingame lần đầu)

`{ uid }`

---

# 2) Security Rules (đơn giản, compile OK)

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    // users/{uid}
    match /users/{uid} {
      // Cho phép đọc để kiểm tra email (không cần đăng nhập)
      allow read: if true;
      
      // Chỉ user đã đăng nhập mới tạo được document của chính mình
      allow create: if request.auth != null && request.auth.uid == uid;
      
      // Chỉ user đã đăng nhập mới update được document của chính mình
      allow update: if request.auth != null && request.auth.uid == uid;
      
      // Không cho phép delete
      allow delete: if false;
    }

    // subcollections của user (inventory, loadouts): owner-only
    match /users/{uid}/inventory/{docId} {
      allow read, write: if request.auth != null && request.auth.uid == uid;
    }
    match /users/{uid}/loadouts/{docId} {
      allow read, write: if request.auth != null && request.auth.uid == uid;
    }

    // catalogs & shop: read-only
    match /configs/{doc} { allow read: if true; allow write: if false; }
    match /items/{doc}   { allow read: if true; allow write: if false; }
    match /quizzes/{doc} { allow read: if true; allow write: if false; }
    match /shops/{shopId} { allow read: if true; allow write: if false; }
    match /shops/{shopId}/entries/{entryId} { allow read: if true; allow write: if false; }

    // username / ingame name map
    match /handles/{name} { 
      allow read: if true; 
      allow create: if request.auth != null; 
      allow update, delete: if false; 
    }
    match /ingame_names/{name} { 
      allow read: if true; 
      allow create: if request.auth != null; 
      allow update, delete: if false; 
    }
  }
}
```

> Mọi cập nhật “nhạy cảm” (coin/xp/level/stats) phải đi qua **Cloud Functions** (Admin SDK). Client không thể sửa trực tiếp.

---

# 3) Cloud Functions (TypeScript)
```typescript
import { onCall, HttpsError } from "firebase-functions/v2/https";
import { logger } from "firebase-functions";
import { initializeApp } from "firebase-admin/app";
import { getFirestore, FieldValue } from "firebase-admin/firestore";

initializeApp();
const db = getFirestore();

// ---- helpers ----
function requireAuth(ctx: { auth?: { uid?: string } }) {
  const uid = ctx.auth?.uid;
  if (!uid) throw new HttpsError("unauthenticated", "Login required");
  return uid;
}
async function getCfg() {
  const snap = await db.doc("configs/gameplay").get();
  return (snap.data() || {}) as any;
}

// ===================================================================
// 1) PURCHASE: mua item trong shop (AntCoin hoặc DCoin)
// ===================================================================
export const purchaseItem = onCall(
  { region: "asia-southeast1", timeoutSeconds: 30, memory: "256MiB" },
  async (req) => {
    const uid = requireAuth(req);
    const { shopId, entryId, currency = "antCoin", quantity = 1 } = req.data || {};
    if (!shopId || !entryId) throw new HttpsError("invalid-argument", "shopId & entryId are required");
    if (!["antCoin", "dCoin"].includes(currency)) throw new HttpsError("invalid-argument", "currency must be antCoin or dCoin");
    const qty = Number(quantity || 1);
    if (!Number.isFinite(qty) || qty <= 0) throw new HttpsError("invalid-argument", "quantity must be > 0");

    const entryRef = db.doc(`shops/${shopId}/entries/${entryId}`);
    const userRef  = db.doc(`users/${uid}`);

    await db.runTransaction(async (tx) => {
      const [entrySnap, userSnap] = await Promise.all([tx.get(entryRef), tx.get(userRef)]);
      if (!entrySnap.exists) throw new HttpsError("not-found", "Shop entry not found");
      if (!userSnap.exists) throw new HttpsError("not-found", "User not found");

      const entry = entrySnap.data() as any;
      const user  = userSnap.data() as any;

      const price = Number((currency === "dCoin" ? entry.priceDCoin : entry.priceAntCoin) || 0);
      if (price <= 0) throw new HttpsError("failed-precondition", "Invalid price");

      const cost = price * qty;
      const bal  = Number(user[currency] || 0);
      if (bal < cost) throw new HttpsError("failed-precondition", "Not enough balance");

      // trừ tiền
      tx.update(userRef, { [currency]: bal - cost });

      // thêm vào inventory
      const invCol = userRef.collection("inventory");
      if (entry.stackable) {
        const docId = entry.itemId; // gộp theo itemId
        const invRef = invCol.doc(docId);
        const invSnap = await tx.get(invRef);
        const prev = invSnap.exists ? ((invSnap.data() as any).qty || 0) : 0;
        const addQty = Number(entry.qtyPerPurchase || qty);
        tx.set(invRef, {
          type: entry.type,
          itemId: entry.itemId,
          qty: prev + addQty,
          updatedAt: FieldValue.serverTimestamp()
        }, { merge: true });
      } else {
        for (let i = 0; i < qty; i++) {
          tx.set(invCol.doc(), {
            type: entry.type,
            itemId: entry.itemId,
            level: 1,
            stars: 0,
            createdAt: FieldValue.serverTimestamp(),
            updatedAt: FieldValue.serverTimestamp()
          });
        }
      }
    });

    return { ok: true };
  }
);

// ===================================================================
// 2) UPGRADE CARD: tăng level card bằng EXP card (tăng stat theo config)
// ===================================================================
export const upgradeCard = onCall(
  { region: "asia-southeast1", timeoutSeconds: 30, memory: "256MiB" },
  async (req) => {
    const uid = requireAuth(req);
    const { invDocId, use } = req.data || {};
    if (!invDocId || typeof use !== "object") {
      throw new HttpsError("invalid-argument", "invDocId & use map required");
    }

    const cfg = await getCfg();
    const xpPerLevel = Number(cfg?.exp?.xpPerLevel || 1000);
    const userRef = db.doc(`users/${uid}`);
    const cardRef = userRef.collection("inventory").doc(String(invDocId));

    await db.runTransaction(async (tx) => {
      const cardSnap = await tx.get(cardRef);
      if (!cardSnap.exists) throw new HttpsError("not-found", "Card instance not found");
      const card = cardSnap.data() as any;
      if (card.type !== "skill_card") throw new HttpsError("failed-precondition", "Target must be skill_card");

      let totalXP = 0;
      for (const [itemIdRaw, qtyRaw] of Object.entries(use)) {
        const itemId = String(itemIdRaw);
        const qty = Number(qtyRaw || 0);
        if (qty <= 0) continue;

        const stkRef = userRef.collection("inventory").doc(itemId);
        const [stkSnap, defSnap] = await Promise.all([
          tx.get(stkRef),
          tx.get(db.doc(`items/${itemId}`))
        ]);

        const have = stkSnap.exists ? ((stkSnap.data() as any).qty || 0) : 0;
        if (have < qty) throw new HttpsError("failed-precondition", `Not enough ${itemId}`);
        const xpValue = Number((defSnap.data() as any)?.exp?.xpValue || 0);
        totalXP += xpValue * qty;

        tx.update(stkRef, {
          qty: have - qty,
          updatedAt: FieldValue.serverTimestamp()
        });
      }

      const gainedLv = Math.floor(totalXP / xpPerLevel);
      if (gainedLv > 0) {
        tx.update(cardRef, {
          level: Math.max(1, (card.level || 1) + gainedLv),
          updatedAt: FieldValue.serverTimestamp()
        });
      }
    });

    return { ok: true };
  }
);

// ===================================================================
// 3) EVOLVE CARD: tăng stars (giảm cooldown hiệu dụng), feed ≤ 5 card
// ===================================================================
export const evolveCard = onCall(
  { region: "asia-southeast1", timeoutSeconds: 30, memory: "256MiB" },
  async (req) => {
    const uid = requireAuth(req);
    const { invDocId, feed } = req.data || {};
    if (!invDocId || !Array.isArray(feed)) {
      throw new HttpsError("invalid-argument", "invDocId & feed[] required");
    }

    const cfg = await getCfg();
    const evo = cfg?.cards?.evolution || {};
    const thresholds: number[] = evo.levelThresholds || [10, 20, 30, 40, 50];
    const maxStars: number = Number(evo.maxStars ?? 5);
    const rates: number[] = evo.baseRateByNextStar || [1, 0.8, 0.6, 0.4, 0.2];

    const userRef = db.doc(`users/${uid}`);
    const cardRef = userRef.collection("inventory").doc(String(invDocId));

    await db.runTransaction(async (tx) => {
      const snap = await tx.get(cardRef);
      if (!snap.exists) throw new HttpsError("not-found", "Target card not found");
      const card = snap.data() as any;
      if (card.type !== "skill_card") throw new HttpsError("failed-precondition", "Target must be skill_card");

      const level = Number(card.level || 1);
      const stars = Number(card.stars || 0);
      if (stars >= maxStars) throw new HttpsError("failed-precondition", "Max stars reached");
      const needLv = thresholds[Math.min(stars, thresholds.length - 1)] || 0;
      if (level < needLv) throw new HttpsError("failed-precondition", `Require level >= ${needLv}`);

      // kiểm nguyên liệu
      const feedIds = (feed as string[]).slice(0, 5).map(String);
      const feedRefs = feedIds.map(id => userRef.collection("inventory").doc(id));
      const feedSnaps = await Promise.all(feedRefs.map(r => tx.get(r)));
      feedSnaps.forEach((s, i) => {
        if (!s.exists) throw new HttpsError("failed-precondition", `Feed ${feedIds[i]} not found`);
        const d = s.data() as any;
        if (d.type !== "skill_card") throw new HttpsError("failed-precondition", "Feed must be skill_card");
        if (feedIds[i] === String(invDocId)) throw new HttpsError("failed-precondition", "Cannot feed self");
      });

      // roll
      const rate = Number(rates[Math.min(stars, rates.length - 1)] || 0);
      const success = Math.random() < rate;

      // xoá nguyên liệu
      feedRefs.forEach(r => tx.delete(r));

      if (success) {
        tx.update(cardRef, { stars: stars + 1, updatedAt: FieldValue.serverTimestamp() });
      } else {
        logger.info(`evolveCard fail uid=${uid} inv=${invDocId} rate=${rate}`);
      }
    });

    return { ok: true };
  }
);

// ===================================================================
// 4) AWARD MATCH: cộng thưởng sau trận (rank 1..4), check thời lượng tối thiểu
// ===================================================================
export const awardMatch = onCall(
  { region: "asia-southeast1", timeoutSeconds: 30, memory: "256MiB" },
  async (req) => {
    const uid = requireAuth(req);
    const { rank, durationSec } = req.data || {};
    const r = Number(rank);
    if (![1, 2, 3, 4].includes(r)) throw new HttpsError("invalid-argument", "rank must be 1..4");

    const cfg = await getCfg();
    const minDur = Number(cfg?.match?.minDurationSec || 0);
    if (Number(durationSec || 0) < minDur) throw new HttpsError("failed-precondition", "Match too short");

    const ant = Number(cfg?.match?.rewardAntCoin?.[`p${r}`] || 0);
    const xp  = Number(cfg?.match?.rewardXp?.[`p${r}`] || 0);

    const userRef = db.doc(`users/${uid}`);
    await db.runTransaction(async (tx) => {
      const s = await tx.get(userRef);
      if (!s.exists) throw new HttpsError("not-found", "User not found");
      const u = s.data() as any;
      tx.update(userRef, {
        antCoin: (Number(u.antCoin) || 0) + ant,
        xp: (Number(u.xp) || 0) + xp,
        "stats.matchesPlayed": (Number(u.stats?.matchesPlayed) || 0) + 1,
        "stats.wins": (Number(u.stats?.wins) || 0) + (r === 1 ? 1 : 0),
        lastLoginAt: FieldValue.serverTimestamp()
      });
    });

    return { ok: true, antCoin: ant, xp };
  }
);
```
# 4) Unity – điểm chạm tối thiểu

### A) Login bằng **email hoặc username**

```csharp
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

async Task<string> ResolveEmailAsync(string input) {
  if (input.Contains("@")) return input;
  var key = ToLowerKey(input); // chuẩn hoá usernameLower
  var snap = await db.Collection("handles").Document(key).GetSnapshotAsync();
  return snap.Exists ? snap.GetValue<string>("email") : null;
}

async Task<string> SignInAsync(string userOrEmail, string password) {
  string email = userOrEmail.Contains("@") ? userOrEmail : await ResolveEmailAsync(userOrEmail);
  var cred = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
  string uid = cred.UserId;

  await db.Collection("users").Document(uid).SetAsync(new {
    username = userOrEmail,
    email = email,
    emailVerified = cred.IsEmailVerified,
    ingameName = (string)null,
    createdAt = FieldValue.ServerTimestamp,
    lastLoginAt = FieldValue.ServerTimestamp,
    level = 1, xp = 0, antCoin = 0, dCoin = 0,
    stats = new { matchesPlayed = 0, wins = 0 },
    status = "active"
  }, SetOptions.MergeAll);

  return uid;
}
```

### B) Set loadout (5 equip + 2 card bất kỳ, **không trùng `itemId`**)

```csharp
async Task SaveLoadoutAsync(string uid, string[] cardInstIds, Dictionary<string,string> equipIds) {
  // tải 2 card để kiểm tra khác itemId
  if (cardInstIds.Length > 1) {
    var c1 = await db.Collection("users").Document(uid).Collection("inventory").Document(cardInstIds[0]).GetSnapshotAsync();
    var c2 = await db.Collection("users").Document(uid).Collection("inventory").Document(cardInstIds[1]).GetSnapshotAsync();
    if (c1.Exists && c2.Exists) {
      var id1 = c1.GetValue<string>("itemId");
      var id2 = c2.GetValue<string>("itemId");
      if (id1 == id2) throw new Exception("Không được mang 2 lá trùng loại.");
    }
  }
  await db.Collection("users").Document(uid).Collection("loadouts").Document("slot1")
    .SetAsync(new {
      active = true,
      skillCardIds = cardInstIds, // 0..2
      equipmentSet = new {
        hatId = equipIds.GetValueOrDefault("hat"),
        shirtId = equipIds.GetValueOrDefault("shirt"),
        wingsId = equipIds.GetValueOrDefault("wings"),
        shoesId = equipIds.GetValueOrDefault("shoes"),
        maskId = equipIds.GetValueOrDefault("mask")
      },
      updatedAt = FieldValue.ServerTimestamp
    }, SetOptions.MergeAll);
}
```

### C) Gọi Cloud Functions

```csharp
using Firebase.Functions;

// Mua trong shop
await FirebaseFunctions.DefaultInstance.GetHttpsCallable("purchaseItem")
  .CallAsync(new { shopId = "default", entryId = "exp_small", currency = "antCoin", quantity = 1 });

// Nâng cấp card
await FirebaseFunctions.DefaultInstance.GetHttpsCallable("upgradeCard")
  .CallAsync(new { invDocId = cardInstanceId, use = new Dictionary<string,int>{{"exp.small",2}} });

// Tiến hoá card
await FirebaseFunctions.DefaultInstance.GetHttpsCallable("evolveCard")
  .CallAsync(new { invDocId = cardInstanceId, feed = new []{ otherCardId1, otherCardId2 } });

// Thưởng sau trận
await FirebaseFunctions.DefaultInstance.GetHttpsCallable("awardMatch")
  .CallAsync(new { rank = 1, durationSec = (int)matchDurationSec });
```

### D) Lấy quiz ngẫu nhiên

```csharp
float anchor = UnityEngine.Random.Range(0f, 1f);
var q1 = db.Collection("quizzes").OrderBy("randomValue").StartAt(anchor).Limit(1);
var s1 = await q1.GetSnapshotAsync();
var doc = s1.Count > 0 ? s1.Documents[0]
  : (await db.Collection("quizzes").OrderBy("randomValue").StartAt(0f).Limit(1).GetSnapshotAsync()).Documents[0];
```

---

## Kết luận nhanh

* **Collections/Subcollections** bạn cần đã liệt kê **đầy đủ** (Doc ID, field & kiểu).
* **Rules** an toàn cơ bản (client chỉ đổi `ingameName/lastLoginAt`).
* **Cloud Functions** xử lý mọi thao tác nhạy cảm: **mua**, **nâng cấp**, **tiến hoá**, **thưởng**.
* **Unity snippets** cho toàn bộ luồng.























