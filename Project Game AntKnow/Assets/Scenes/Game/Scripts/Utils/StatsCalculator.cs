using UnityEngine;

namespace AntKnow.Game
{
    /// <summary>
    /// Stats Calculator - Tính toán bonus từ stats
    /// Copy từ Domain layer
    /// </summary>
    public static class StatsCalculator
    {
        /// <summary>
        /// Luck: Tăng khả năng xúc xắc đôi
        /// Formula: 10 pts = 1%
        /// </summary>
        public static bool CheckLuckForDouble(int luckStat, out int diceValue)
        {
            int luckPct = luckStat / 10; // 10 pts = 1%
            float doubleChance = luckPct / 100f;
            
            if (Random.value < doubleChance)
            {
                // Luck triggered!
                diceValue = Random.Range(1, 7);
                return true;
            }
            
            diceValue = 0;
            return false;
        }

        /// <summary>
        /// Resistance: Giảm tiền thuê khi vào nhà người khác
        /// Formula: 10 pts = 1% giảm
        /// Chủ nhà vẫn nhận đủ, chỉ có người thuê được cashback
        /// </summary>
        public static (int payToOwner, int cashback, int actualLoss) CalculateRentWithResistance(
            int baseRent, 
            int resistanceStat)
        {
            int resistPct = resistanceStat / 10; // 10 pts = 1%
            int cashback = baseRent * resistPct / 100;
            int actualLoss = baseRent - cashback;
            
            return (baseRent, cashback, actualLoss);
        }

        /// <summary>
        /// Intelligence: Tăng tiền thuê nhận được
        /// Formula: 10 pts = 1% bonus
        /// </summary>
        public static (int baseRent, int bonus, int totalReceived) CalculateRentWithIntelligence(
            int baseRent, 
            int intelligenceStat)
        {
            int intPct = intelligenceStat / 10; // 10 pts = 1%
            int bonus = baseRent * intPct / 100;
            int totalReceived = baseRent + bonus;
            
            return (baseRent, bonus, totalReceived);
        }

        /// <summary>
        /// Health: Tăng lương khi qua ô bắt đầu
        /// Formula: 10 pts = 1% bonus
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
        /// Agility: Cơ hội nhân đôi tiền thuê khi mua/nâng cấp nhà
        /// Formula: 10 pts = 1% chance
        /// </summary>
        public static bool CheckAgilityForDoubleRent(int agilityStat)
        {
            int agilityPct = agilityStat / 10; // 10 pts = 1%
            float doubleChance = agilityPct / 100f;
            
            return Random.value < doubleChance;
        }

        /// <summary>
        /// Calculate final rent with multiplier (from Agility)
        /// </summary>
        public static int CalculateFinalRent(int baseRent, float rentMultiplier)
        {
            return Mathf.RoundToInt(baseRent * rentMultiplier);
        }
    }
}

