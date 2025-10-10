using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Skill Trigger Engine - Xử lý trigger và effect của skill cards
/// Extensible: Dễ dàng thêm trigger và effect mới
/// </summary>
public class SkillTriggerEngine
{
    private readonly Dictionary<string, SkillCardData> _cardDatabase;
    private readonly Dictionary<string, ISkillEffect> _effectHandlers;
    private readonly Random _rng;

    public SkillTriggerEngine(Random rng = null)
    {
        _rng = rng ?? new Random();
        _cardDatabase = new Dictionary<string, SkillCardData>();
        _effectHandlers = new Dictionary<string, ISkillEffect>();

        // Register effect handlers
        RegisterEffectHandlers();
        
        // Load card database (từ Firebase sau, hardcode trước)
        LoadCardDatabase();
    }

    private void RegisterEffectHandlers()
    {
        // Movement effects
        _effectHandlers[SkillEffects.AutoStepForward] = new AutoStepForwardEffect();
        _effectHandlers[SkillEffects.AutoStepBackward] = new AutoStepBackwardEffect();
        
        // Money effects
        _effectHandlers[SkillEffects.GainMoney] = new GainMoneyEffect();
        _effectHandlers[SkillEffects.LoseMoney] = new LoseMoneyEffect();
        _effectHandlers[SkillEffects.ExtraStartSalary] = new ExtraStartSalaryEffect();
        
        // Purchase effects
        _effectHandlers[SkillEffects.PurchaseDiscount] = new PurchaseDiscountEffect();
        _effectHandlers[SkillEffects.UpgradeDiscount] = new UpgradeDiscountEffect();
        
        // Property effects
        _effectHandlers[SkillEffects.ProtectProperty] = new ProtectPropertyEffect();
    }

    private void LoadCardDatabase()
    {
        // Load từ BasicSkillCards (hardcode)
        // Sau này sẽ load từ Firebase
        foreach (var card in BasicSkillCards.GetBasicCards())
        {
            _cardDatabase[card.itemId] = card;
        }
    }

    /// <summary>
    /// Check if player has skill card with specific trigger
    /// </summary>
    public List<SkillCardInstance> GetTriggeredCards(PlayerState player, string triggerId, GameState gameState)
    {
        var triggered = new List<SkillCardInstance>();

        // Check passive cards
        foreach (int cardId in player.PassiveCardIds)
        {
            // TODO: Map cardId to SkillCardInstance
            // For now, we assume cardId is the index in a list
        }

        return triggered;
    }

    /// <summary>
    /// Execute skill effect
    /// </summary>
    public SkillExecutionResult ExecuteSkill(SkillCardInstance cardInstance, SkillCardData cardData, 
        PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        var result = new SkillExecutionResult
        {
            success = false,
            cardName = cardData.name,
            effectDescription = ""
        };

        // Check cooldown
        if (cardInstance.currentCooldown > 0)
        {
            result.message = $"{cardData.name} đang hồi chiêu ({cardInstance.currentCooldown} lượt)";
            return result;
        }

        // Get effect handler
        if (!_effectHandlers.TryGetValue(cardData.skill.effectId, out var handler))
        {
            result.message = $"Effect {cardData.skill.effectId} chưa được implement";
            return result;
        }

        // Execute effect
        try
        {
            result = handler.Execute(cardData, player, gameState, context);
            
            if (result.success)
            {
                // Set cooldown
                cardInstance.currentCooldown = cardInstance.effectiveCooldown;
            }
        }
        catch (Exception ex)
        {
            result.success = false;
            result.message = $"Lỗi khi thực hiện skill: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Trigger passive skills
    /// </summary>
    public List<SkillExecutionResult> TriggerPassiveSkills(string triggerId, PlayerState player, 
        GameState gameState, SkillExecutionContext context)
    {
        var results = new List<SkillExecutionResult>();

        // Find all cards with this trigger
        foreach (int cardId in player.PassiveCardIds)
        {
            // TODO: Implement card instance lookup
            // For now, skip
        }

        return results;
    }

    /// <summary>
    /// Update cooldowns at turn start
    /// </summary>
    public void UpdateCooldowns(PlayerState player)
    {
        // Reduce cooldown for all cards
        var cooldownKeys = player.PassiveCooldown.Keys.ToList();
        foreach (var cardId in cooldownKeys)
        {
            if (player.PassiveCooldown[cardId] > 0)
            {
                player.PassiveCooldown[cardId]--;
            }
        }
    }
}

/// <summary>
/// Skill execution context - Chứa thông tin về context khi skill được trigger
/// </summary>
public class SkillExecutionContext
{
    public int tileIndex;           // Tile đang đứng
    public PropertyState property;  // Property liên quan (nếu có)
    public int targetPlayerId;      // Player target (nếu có)
    public int purchasePrice;       // Giá mua (cho discount effects)
    public Dictionary<string, object> customData; // Custom data
}

/// <summary>
/// Skill execution result
/// </summary>
public class SkillExecutionResult
{
    public bool success;
    public string cardName;
    public string effectDescription;
    public string message;
    public Dictionary<string, object> changes; // Money change, position change, etc.
}

/// <summary>
/// Skill Effect Interface - Mỗi effect là 1 class riêng, dễ extend
/// </summary>
public interface ISkillEffect
{
    SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context);
}

// ============================================================
// EFFECT IMPLEMENTATIONS
// ============================================================

/// <summary>
/// Lẩn trốn: Auto step forward 1 tile khi vào nhà người khác
/// </summary>
public class AutoStepForwardEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        int step = GetParameter<int>(card.skill.parameters, "step", 1);
        int oldPos = player.NodeIndex;
        int newPos = (player.NodeIndex + step) % gameState.BoardLength;
        
        player.NodeIndex = newPos;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Lẩn trốn! Di chuyển từ ô {oldPos} → {newPos}",
            message = $"Kỹ năng {card.name} kích hoạt!",
            changes = new Dictionary<string, object>
            {
                { "oldPosition", oldPos },
                { "newPosition", newPos },
                { "step", step }
            }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Auto step backward
/// </summary>
public class AutoStepBackwardEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        int step = GetParameter<int>(card.skill.parameters, "step", 1);
        int oldPos = player.NodeIndex;
        int newPos = (player.NodeIndex - step + gameState.BoardLength) % gameState.BoardLength;
        
        player.NodeIndex = newPos;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Di chuyển lùi từ ô {oldPos} → {newPos}",
            changes = new Dictionary<string, object>
            {
                { "oldPosition", oldPos },
                { "newPosition", newPos },
                { "step", -step }
            }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Siêu Sale: Purchase discount 30%
/// </summary>
public class PurchaseDiscountEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        double rate = GetParameter<double>(card.skill.parameters, "rate", 0.3);
        int originalPrice = context.purchasePrice;
        int discount = (int)(originalPrice * rate);
        int finalPrice = originalPrice - discount;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Siêu Sale! Giảm {(rate * 100):F0}% giá mua ({originalPrice} → {finalPrice})",
            message = $"Kỹ năng {card.name} kích hoạt!",
            changes = new Dictionary<string, object>
            {
                { "originalPrice", originalPrice },
                { "discount", discount },
                { "finalPrice", finalPrice },
                { "rate", rate }
            }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Upgrade discount
/// </summary>
public class UpgradeDiscountEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        double rate = GetParameter<double>(card.skill.parameters, "rate", 0.2);
        int originalPrice = context.purchasePrice;
        int discount = (int)(originalPrice * rate);
        int finalPrice = originalPrice - discount;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Giảm giá nâng cấp {(rate * 100):F0}%",
            changes = new Dictionary<string, object>
            {
                { "originalPrice", originalPrice },
                { "finalPrice", finalPrice }
            }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Bảo kê: Protect property from takeover
/// </summary>
public class ProtectPropertyEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        int duration = GetParameter<int>(card.skill.parameters, "durationTurns", 1);
        int propertyId = context.property?.TileId ?? 0;

        // Store protection in player cooldown dictionary (convention: negative key)
        int protectionKey = -1000 - propertyId;
        player.PassiveCooldown[protectionKey] = duration;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Bảo kê ô {propertyId} trong {duration} lượt!",
            message = $"Kỹ năng {card.name} kích hoạt!",
            changes = new Dictionary<string, object>
            {
                { "propertyId", propertyId },
                { "duration", duration }
            }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Chăm chỉ: Extra start salary (lương gấp đôi)
/// </summary>
public class ExtraStartSalaryEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        double multiplier = GetParameter<double>(card.skill.parameters, "multiplier", 1.0);
        int baseSalary = 200; // Default salary
        int bonus = (int)(baseSalary * multiplier);
        
        player.Money += bonus;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Chăm chỉ! Nhận thêm lương tháng 13: +{bonus} AntCoin",
            message = $"Kỹ năng {card.name} kích hoạt!",
            changes = new Dictionary<string, object>
            {
                { "bonus", bonus },
                { "multiplier", multiplier }
            }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Gain money effect
/// </summary>
public class GainMoneyEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        int amount = GetParameter<int>(card.skill.parameters, "amount", 100);
        player.Money += amount;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Nhận {amount} AntCoin",
            changes = new Dictionary<string, object> { { "amount", amount } }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

/// <summary>
/// Lose money effect
/// </summary>
public class LoseMoneyEffect : ISkillEffect
{
    public SkillExecutionResult Execute(SkillCardData card, PlayerState player, GameState gameState, SkillExecutionContext context)
    {
        int amount = GetParameter<int>(card.skill.parameters, "amount", 100);
        player.Money -= amount;
        if (player.Money < 0) player.Money = 0;

        return new SkillExecutionResult
        {
            success = true,
            cardName = card.name,
            effectDescription = $"Mất {amount} AntCoin",
            changes = new Dictionary<string, object> { { "amount", -amount } }
        };
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters != null && parameters.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
}

