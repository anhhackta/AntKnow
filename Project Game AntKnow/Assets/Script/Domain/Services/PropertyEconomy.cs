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

  /// <summary>
  /// Tính tiền thuê (TẤT CẢ level đều có tiền thuê, kể cả Level 0)
  /// </summary>
  public int Rent(int basePrice, int level, bool hotel) {
    if (hotel) return basePrice * _hotelRentPct / 100;
    int idx = Math.Clamp(level, 0, _rentPctByLevel.Length-1);
    return basePrice * _rentPctByLevel[idx] / 100;
  }

  /// <summary>
  /// Tính giá mua lại = Tổng chi phí * 120%
  /// </summary>
  public int TakeoverCost(int basePrice, int level, bool hotel) {
    int totalCost = CalculateTotalPurchaseCost(basePrice, level, hotel);
    return totalCost * 120 / 100; // 120%
  }

  /// <summary>
  /// Tính giá bán = Tổng chi phí * 60%
  /// </summary>
  public int SellPrice(int basePrice, int level, bool hotel) {
    int totalCost = CalculateTotalPurchaseCost(basePrice, level, hotel);
    return totalCost * 60 / 100; // 60%
  }

  /// <summary>
  /// Tính tổng chi phí đã bỏ ra để mua/nâng cấp nhà
  /// </summary>
  private int CalculateTotalPurchaseCost(int basePrice, int level, bool hotel) {
    int total = basePrice; // Đất trống (Level 0)

    // Add upgrade costs
    if (level >= 1) total += UpgradeCost(basePrice, 1); // House 1
    if (level >= 2) total += UpgradeCost(basePrice, 2); // House 2
    if (level >= 3) total += UpgradeCost(basePrice, 3); // House 3
    if (level >= 4) total += UpgradeCost(basePrice, 4); // House 4
    if (hotel)      total += HotelCost(basePrice);       // Hotel

    return total;
  }
}

