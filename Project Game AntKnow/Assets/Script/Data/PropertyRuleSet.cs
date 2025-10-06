using UnityEngine;

[CreateAssetMenu(fileName="PropertyRuleSet", menuName="AntKnow/PropertyRuleSet")]
public class PropertyRuleSet : ScriptableObject {
  [Header("Upgrade cost to reach level (1..4) as % of basePrice")]
  [Tooltip("Level 1: 100%, Level 2: 150%, Level 3: 200%, Level 4: 250%")]
  public int[] upgradeCostPctByLevel = new int[] { 100, 150, 200, 250 };

  [Header("Rent % of basePrice by level (0..5)")]
  [Tooltip("Level 0: 10%, Level 1: 25%, Level 2: 50%, Level 3: 75%, Level 4: 100%, Level 5 (Hotel): 250%")]
  public int[] rentPctByLevel = new int[] { 10, 25, 50, 75, 100, 250 };

  [Header("Hotel costs/rent as % of basePrice")]
  [Tooltip("Hotel upgrade: 400%, Hotel rent: 250%")]
  public int hotelUpgradePct = 400;
  public int hotelRentPct = 250;

  [Header("Takeover & Sell (Not used - calculated dynamically)")]
  [Tooltip("Takeover = 120% total cost, Sell = 60% total cost")]
  public int[] takeoverPctByLevel = new int[] { 120, 120, 120, 120, 120, 120 }; // Not used

  [Header("Allow takeover when hotel?")]
  public bool takeoverAllowedOnHotel = true;

  public PropertyEconomy ToEconomy() {
    return new PropertyEconomy(upgradeCostPctByLevel, rentPctByLevel, hotelUpgradePct, hotelRentPct, takeoverPctByLevel, takeoverAllowedOnHotel);
  }
}

