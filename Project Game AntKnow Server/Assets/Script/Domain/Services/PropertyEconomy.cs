using System;

// Pure data-driven economy formulas for properties
public sealed class PropertyEconomy {
  readonly int[] _upgradeCostPctByLevel; // index = target level 1..5 (cost to reach that level)
  readonly int[] _rentPctByLevel;        // index = level 0..5 (0 = land only)
  readonly int   _hotelUpgradePct;       // times basePrice
  readonly int   _hotelRentPct;          // times basePrice
  readonly int[] _takeoverPctByLevel;    // index = level 0..5 (0 allowed? usually base land takeover)
  public readonly bool TakeoverAllowedOnHotel;

  // All percents are expressed as percent of basePrice (e.g., 150 => 1.5x)
  public PropertyEconomy(int[] upgradeCostPctByLevel,
                         int[] rentPctByLevel,
                         int hotelUpgradePct,
                         int hotelRentPct,
                         int[] takeoverPctByLevel,
                         bool takeoverAllowedOnHotel) {
    _upgradeCostPctByLevel = upgradeCostPctByLevel;
    _rentPctByLevel = rentPctByLevel;
    _hotelUpgradePct = hotelUpgradePct;
    _hotelRentPct = hotelRentPct;
    _takeoverPctByLevel = takeoverPctByLevel;
    TakeoverAllowedOnHotel = takeoverAllowedOnHotel;
  }

  public int UpgradeCost(int basePrice, int targetLevel) {
    if (targetLevel < 1 || targetLevel >= _upgradeCostPctByLevel.Length+1) return 0;
    int pct = _upgradeCostPctByLevel[targetLevel-1];
    return basePrice * pct / 100;
  }

  public int HotelCost(int basePrice) => basePrice * _hotelUpgradePct / 100;

  public int Rent(int basePrice, int level, bool hotel) {
    if (hotel) return basePrice * _hotelRentPct / 100;
    int idx = Math.Clamp(level, 0, _rentPctByLevel.Length-1);
    return basePrice * _rentPctByLevel[idx] / 100;
  }

  public int TakeoverCost(int basePrice, int level, bool hotel) {
    if (hotel && !TakeoverAllowedOnHotel) return int.MaxValue; // treat as infinite
    if (hotel) return basePrice * _hotelRentPct / 100; // or define separate hotel takeover pct
    int idx = Math.Clamp(level, 0, _takeoverPctByLevel.Length-1);
    return basePrice * _takeoverPctByLevel[idx] / 100;
  }
}

