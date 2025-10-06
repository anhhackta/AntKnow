using System;
using UnityEngine;

/// <summary>
/// Tính toán các hiệu ứng từ stats
/// Quy đổi: 10 điểm = 1%
/// </summary>
public static class StatsCalculator
{
    /// <summary>
    /// Luck: Tăng khả năng xúc xắc đôi
    /// </summary>
    public static bool CheckLuckForDouble(int luckStat, out int diceValue)
    {
        int luckPct = luckStat / 10; // 10 pts = 1%
        float doubleChance = luckPct / 100f;
        
        if (UnityEngine.Random.value < doubleChance)
        {
            // Trúng! Roll 1 con nhưng tính 2 viên
            diceValue = UnityEngine.Random.Range(1, 7);
            return true; // Is double
        }
        
        diceValue = 0;
        return false; // Not double, roll normally
    }
    
    /// <summary>
    /// Resistance: Giảm thiệt hại khi thuê nhà
    /// Trả đủ tiền cho chủ nhà, nhưng nhận lại % tiền
    /// </summary>
    public static (int payToOwner, int cashback, int actualLoss) CalculateRentWithResistance(
        int baseRent, 
        int resistanceStat)
    {
        int resistancePct = resistanceStat / 10; // 10 pts = 1%
        int cashback = baseRent * resistancePct / 100;
        int actualLoss = baseRent - cashback;
        
        return (baseRent, cashback, actualLoss);
    }
    
    /// <summary>
    /// Intelligence: Tăng thu nhập từ nhà
    /// Nhận thêm tiền khi người khác thuê nhà
    /// </summary>
    public static (int baseRent, int bonus, int totalReceived) CalculateRentWithIntelligence(
        int baseRent, 
        int intelligenceStat)
    {
        int intelligencePct = intelligenceStat / 10; // 10 pts = 1%
        int bonus = baseRent * intelligencePct / 100;
        int totalReceived = baseRent + bonus;
        
        return (baseRent, bonus, totalReceived);
    }
    
    /// <summary>
    /// Health: Nhận thêm tiền khi qua ô Start
    /// </summary>
    public static (int baseSalary, int bonus, int totalReceived) CalculateSalaryWithHealth(
        int baseSalary, 
        int healthStat)
    {
        int healthPct = healthStat / 10; // 10 pts = 1%
        int bonus = baseSalary * healthPct / 100;
        int totalReceived = baseSalary + bonus;
        
        return (baseSalary, bonus, totalReceived);
    }
    
    /// <summary>
    /// Agility: Khi mua/nâng cấp nhà, có tỉ lệ nhân đôi tiền thuê
    /// </summary>
    public static bool CheckAgilityForDoubleRent(int agilityStat)
    {
        int agilityPct = agilityStat / 10; // 10 pts = 1%
        float doubleChance = agilityPct / 100f;
        
        return UnityEngine.Random.value < doubleChance;
    }
    
    /// <summary>
    /// Tính tổng tiền thuê với multiplier
    /// </summary>
    public static int CalculateFinalRent(int baseRent, float rentMultiplier)
    {
        return Mathf.RoundToInt(baseRent * rentMultiplier);
    }
}

