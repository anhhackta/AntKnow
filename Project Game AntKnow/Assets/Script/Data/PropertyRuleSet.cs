using UnityEngine;

[CreateAssetMenu(fileName="PropertyRuleSet", menuName="AntKnow/PropertyRuleSet")]
public class PropertyRuleSet : ScriptableObject {
  [Header("Upgrade cost to reach level (1..5) as % of basePrice")] public int[] upgradeCostPctByLevel = new int[] { 100, 150, 200, 250, 300 };
  [Header("Rent % of basePrice by level (0..5)")] public int[] rentPctByLevel = new int[] { 25, 50, 75, 100, 125, 150 };
  [Header("Hotel costs/rent as % of basePrice")] public int hotelUpgradePct = 400; public int hotelRentPct = 250;
  [Header("Takeover % of basePrice by level (0..5)")] public int[] takeoverPctByLevel = new int[] { 150, 200, 300, 400, 500, 600 };
  [Header("Allow takeover when hotel?")] public bool takeoverAllowedOnHotel = false;

  public PropertyEconomy ToEconomy() {
    return new PropertyEconomy(upgradeCostPctByLevel, rentPctByLevel, hotelUpgradePct, hotelRentPct, takeoverPctByLevel, takeoverAllowedOnHotel);
  }
}

