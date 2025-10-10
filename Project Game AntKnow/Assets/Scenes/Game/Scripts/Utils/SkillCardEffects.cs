using UnityEngine;
using System.Collections.Generic;

namespace AntKnow.Game
{
    /// <summary>
    /// Skill Card Effects - Define effectId và cooldown
    /// Theo format từ Firebase: effectId → cooldown
    /// </summary>
    public static class SkillCardEffects
    {
        // ===== EFFECT DEFINITIONS =====
        
        /// <summary>
        /// Lẩn trốn - Agility - Passive - CD:5
        /// Khi vào ô nhà người khác → tự động di chuyển lên 1 ô
        /// </summary>
        public const string AUTO_STEP_FORWARD = "autoStepForward";
        public const int AUTO_STEP_FORWARD_CD = 5;
        
        /// <summary>
        /// Siêu Sale - Intelligence - Passive - CD:5
        /// Khi mua nhà → giảm 30% tiền mua
        /// </summary>
        public const string PURCHASE_DISCOUNT = "purchaseDiscount";
        public const int PURCHASE_DISCOUNT_CD = 5;
        public const float PURCHASE_DISCOUNT_PERCENT = 0.30f;
        
        /// <summary>
        /// Bảo kê - Health - Active - CD:8
        /// Bảo vệ 1 nhà bất kì khỏi việc mua lại
        /// </summary>
        public const string PROTECT_PROPERTY = "protectProperty";
        public const int PROTECT_PROPERTY_CD = 8;
        
        /// <summary>
        /// Chăm chỉ - Luck - Active - CD:6
        /// Nhận thêm lương mặc định khi qua ô Start (lương x2)
        /// </summary>
        public const string EXTRA_START_SALARY = "extraStartSalary";
        public const int EXTRA_START_SALARY_CD = 6;
        
        // ===== COOLDOWN LOOKUP =====
        
        private static readonly Dictionary<string, int> effectCooldowns = new Dictionary<string, int>
        {
            { AUTO_STEP_FORWARD, AUTO_STEP_FORWARD_CD },
            { PURCHASE_DISCOUNT, PURCHASE_DISCOUNT_CD },
            { PROTECT_PROPERTY, PROTECT_PROPERTY_CD },
            { EXTRA_START_SALARY, EXTRA_START_SALARY_CD }
        };
        
        /// <summary>
        /// Get cooldown for effectId
        /// </summary>
        public static int GetCooldown(string effectId)
        {
            if (effectCooldowns.TryGetValue(effectId, out int cooldown))
            {
                return cooldown;
            }
            
            Debug.LogWarning($"[SkillCardEffects] Unknown effectId: {effectId}");
            return 0;
        }
        
        // ===== PASSIVE SKILL TRIGGERS =====
        
        /// <summary>
        /// Check và trigger passive skill: autoStepForward
        /// Gọi khi player vào ô nhà người khác
        /// </summary>
        public static bool TriggerAutoStepForward(PlayerGameController player)
        {
            if (player.HasSkillCard(AUTO_STEP_FORWARD) && player.IsSkillAvailable(AUTO_STEP_FORWARD))
            {
                Debug.Log($"[SkillCardEffects] {player.PlayerName} triggers AUTO_STEP_FORWARD!");
                player.UseSkillCard(AUTO_STEP_FORWARD, AUTO_STEP_FORWARD_CD);
                return true; // Will move +1 step
            }
            
            return false;
        }
        
        /// <summary>
        /// Check và apply discount: purchaseDiscount
        /// Gọi khi player mua nhà
        /// </summary>
        public static int ApplyPurchaseDiscount(PlayerGameController player, int originalPrice)
        {
            if (player.HasSkillCard(PURCHASE_DISCOUNT) && player.IsSkillAvailable(PURCHASE_DISCOUNT))
            {
                int discount = Mathf.RoundToInt(originalPrice * PURCHASE_DISCOUNT_PERCENT);
                int finalPrice = originalPrice - discount;
                
                Debug.Log($"[SkillCardEffects] {player.PlayerName} triggers PURCHASE_DISCOUNT! {originalPrice} → {finalPrice} (-{discount})");
                player.UseSkillCard(PURCHASE_DISCOUNT, PURCHASE_DISCOUNT_CD);
                
                return finalPrice;
            }
            
            return originalPrice;
        }
        
        // ===== ACTIVE SKILL TRIGGERS =====
        
        /// <summary>
        /// Check if player can use protectProperty
        /// </summary>
        public static bool CanUseProtectProperty(PlayerGameController player)
        {
            return player.HasSkillCard(PROTECT_PROPERTY) && player.IsSkillAvailable(PROTECT_PROPERTY);
        }
        
        /// <summary>
        /// Use protectProperty skill
        /// Return: tileIndex to protect
        /// </summary>
        public static int UseProtectProperty(PlayerGameController player, int tileIndex)
        {
            if (CanUseProtectProperty(player))
            {
                Debug.Log($"[SkillCardEffects] {player.PlayerName} uses PROTECT_PROPERTY on tile {tileIndex}!");
                player.UseSkillCard(PROTECT_PROPERTY, PROTECT_PROPERTY_CD);
                return tileIndex;
            }
            
            return -1; // Failed
        }
        
        /// <summary>
        /// Check if player can use extraStartSalary
        /// </summary>
        public static bool CanUseExtraStartSalary(PlayerGameController player)
        {
            return player.HasSkillCard(EXTRA_START_SALARY) && player.IsSkillAvailable(EXTRA_START_SALARY);
        }
        
        /// <summary>
        /// Use extraStartSalary skill
        /// Return: bonus salary (equals to base salary)
        /// </summary>
        public static int UseExtraStartSalary(PlayerGameController player, int baseSalary)
        {
            if (CanUseExtraStartSalary(player))
            {
                Debug.Log($"[SkillCardEffects] {player.PlayerName} uses EXTRA_START_SALARY! +{baseSalary} bonus");
                player.UseSkillCard(EXTRA_START_SALARY, EXTRA_START_SALARY_CD);
                return baseSalary; // Double salary
            }
            
            return 0; // No bonus
        }
    }
}

