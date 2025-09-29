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
