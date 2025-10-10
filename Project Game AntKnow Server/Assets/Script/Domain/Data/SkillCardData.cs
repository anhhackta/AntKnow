using System;
using System.Collections.Generic;

/// <summary>
/// Skill Card Data - Loaded from Firebase
/// Chỉ số thẻ ≠ Chỉ số nhân vật ≠ Cooldown
/// </summary>
[Serializable]
public class SkillCardData
{
    public string itemId;           // Firebase itemId (e.g., "skill.lan-tron")
    public string name;             // Card name
    public string description;      // Card description
    
    // Card attributes (chỉ số thẻ - independent)
    public SkillCardAttributes attributes;
    
    // Skill data
    public SkillData skill;
}

/// <summary>
/// Skill card attributes (chỉ số thẻ)
/// Đây là chỉ số của THẺ BÀI, không phải chỉ số nhân vật
/// </summary>
[Serializable]
public class SkillCardAttributes
{
    public int health;
    public int agility;
    public int intelligence;
    public int luck;
    public int resistance;
}

/// <summary>
/// Skill data from Firebase
/// </summary>
[Serializable]
public class SkillData
{
    public string mode;             // "passive" or "active"
    public string primaryStat;      // "health", "agility", "intelligence", "luck", "resistance"
    public int cooldownBaseTurns;   // Base cooldown (chưa tính stars)
    public string triggerId;        // "onEnterOpponentHouse", "onTryPurchaseProperty", etc.
    public string effectId;         // "autoStepForward", "purchaseDiscount", "protectProperty", "extraStartSalary"
    public Dictionary<string, object> parameters; // Effect parameters
}

/// <summary>
/// Skill card instance (inventory item)
/// </summary>
[Serializable]
public class SkillCardInstance
{
    public string instanceId;       // Firestore document ID
    public string itemId;           // Reference to SkillCardData
    public int level;               // Card level (1-50)
    public int stars;               // Card stars (0-5)
    
    // Computed at runtime
    public int effectiveCooldown;   // cooldownBaseTurns - cooldownReduction[stars]
    public int currentCooldown;     // Current turns remaining
}

/// <summary>
/// Skill Trigger IDs (extensible)
/// </summary>
public static class SkillTriggers
{
    // Passive triggers
    public const string OnEnterOpponentHouse = "onEnterOpponentHouse";     // Lẩn trốn
    public const string OnTryPurchaseProperty = "onTryPurchaseProperty";   // Siêu Sale
    public const string OnStartOfTurn = "onStartOfTurn";
    public const string OnEndOfTurn = "onEndOfTurn";
    public const string OnPassStart = "onPassStart";
    public const string OnPayRent = "onPayRent";
    public const string OnReceiveRent = "onReceiveRent";
    public const string OnLandOnQuiz = "onLandOnQuiz";
    public const string OnLandOnEvent = "onLandOnEvent";
    
    // Active triggers (manual)
    public const string Manual = "manual";                                  // Bảo kê, Chăm chỉ
}

/// <summary>
/// Skill Effect IDs (extensible)
/// </summary>
public static class SkillEffects
{
    // Movement effects
    public const string AutoStepForward = "autoStepForward";        // Lẩn trốn: +1 ô
    public const string AutoStepBackward = "autoStepBackward";
    public const string TeleportToTile = "teleportToTile";
    
    // Money effects
    public const string GainMoney = "gainMoney";
    public const string LoseMoney = "loseMoney";
    public const string ExtraStartSalary = "extraStartSalary";      // Chăm chỉ: Lương x2
    
    // Purchase effects
    public const string PurchaseDiscount = "purchaseDiscount";      // Siêu Sale: -30%
    public const string UpgradeDiscount = "upgradeDiscount";
    public const string RentDiscount = "rentDiscount";
    
    // Property effects
    public const string ProtectProperty = "protectProperty";        // Bảo kê: Bảo vệ 1 nhà
    public const string FreeUpgrade = "freeUpgrade";
    public const string DoubleRent = "doubleRent";
    
    // Special effects
    public const string SkipJail = "skipJail";
    public const string ImmunityToRent = "immunityToRent";
    public const string StealProperty = "stealProperty";
}

/// <summary>
/// 4 Skill Cards Cơ Bản (hardcoded cho demo, sau này từ Firebase)
/// </summary>
public static class BasicSkillCards
{
    public static List<SkillCardData> GetBasicCards()
    {
        return new List<SkillCardData>
        {
            // 1. Lẩn trốn
            new SkillCardData
            {
                itemId = "skill.lan-tron",
                name = "Lẩn Trốn",
                description = "Khi vào ô nhà người khác, tự động di chuyển lên 1 ô",
                attributes = new SkillCardAttributes { agility = 10 },
                skill = new SkillData
                {
                    mode = "passive",
                    primaryStat = "agility",
                    cooldownBaseTurns = 5,
                    triggerId = SkillTriggers.OnEnterOpponentHouse,
                    effectId = SkillEffects.AutoStepForward,
                    parameters = new Dictionary<string, object> { { "step", 1 } }
                }
            },
            
            // 2. Siêu Sale
            new SkillCardData
            {
                itemId = "skill.sieu-sale",
                name = "Siêu Sale",
                description = "Mua nhà giảm được 30% tiền",
                attributes = new SkillCardAttributes { intelligence = 10 },
                skill = new SkillData
                {
                    mode = "passive",
                    primaryStat = "intelligence",
                    cooldownBaseTurns = 5,
                    triggerId = SkillTriggers.OnTryPurchaseProperty,
                    effectId = SkillEffects.PurchaseDiscount,
                    parameters = new Dictionary<string, object> { { "rate", 0.3 } }
                }
            },
            
            // 3. Bảo kê
            new SkillCardData
            {
                itemId = "skill.bao-ke",
                name = "Bảo Kê",
                description = "Bảo vệ 1 nhà bất kỳ khỏi việc mua lại khi người khác vào",
                attributes = new SkillCardAttributes { health = 10 },
                skill = new SkillData
                {
                    mode = "active",
                    primaryStat = "health",
                    cooldownBaseTurns = 8,
                    triggerId = SkillTriggers.Manual,
                    effectId = SkillEffects.ProtectProperty,
                    parameters = new Dictionary<string, object> { { "durationTurns", 1 } }
                }
            },
            
            // 4. Chăm chỉ
            new SkillCardData
            {
                itemId = "skill.cham-chi",
                name = "Chăm Chỉ",
                description = "Nhận thêm lương tháng 13 - nhận gấp đôi lương ô bắt đầu",
                attributes = new SkillCardAttributes { luck = 10 },
                skill = new SkillData
                {
                    mode = "active",
                    primaryStat = "luck",
                    cooldownBaseTurns = 6,
                    triggerId = SkillTriggers.Manual,
                    effectId = SkillEffects.ExtraStartSalary,
                    parameters = new Dictionary<string, object> { { "multiplier", 1.0 } }
                }
            }
        };
    }
    
    public static SkillCardData GetCardByItemId(string itemId)
    {
        return GetBasicCards().Find(c => c.itemId == itemId);
    }
}

