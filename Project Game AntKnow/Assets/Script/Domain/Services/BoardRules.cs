using System;

public static class BoardRules {
  public static void OnPassStart(PlayerState p, int baseSalary) {
    int bonusPct = p.Health / 300; // 300 pts = +1%
    p.Money += baseSalary + baseSalary * bonusPct / 100;
  }

  public static void OnTax(PlayerState p, int amount) {
    int reducePct = p.Resistance / 100; // 100 pts = -1%
    int pay = Math.Max(0, amount - amount * reducePct / 100);
    p.Money -= pay;
  }

  public static void OnBonus(PlayerState p, int amount) => p.Money += amount;

  public static bool CanBuy(PlayerState p, PropertyState pr) => pr.Owner == Owner.None && p.Money >= pr.BasePrice;

  public static void Buy(PlayerState p, PropertyState pr) {
    p.Money -= pr.BasePrice;
    pr.Owner = (Owner)p.Id;
    if (!p.Owned.Contains(pr.TileId)) p.Owned.Add(pr.TileId);
  }

  // --- Property upgrade / hotel / takeover helpers ---
  public static bool CanUpgradeHouse(PlayerState p, PropertyState pr, PropertyEconomy econ) {
    if (pr.Owner != (Owner)p.Id) return false;
    if (pr.HasHotel) return false;
    if (pr.Level >= 5) return false;
    int cost = econ.UpgradeCost(pr.BasePrice, pr.Level + 1); // cost to reach next level
    return p.Money >= cost;
  }

  public static void UpgradeHouse(PlayerState p, PropertyState pr, PropertyEconomy econ) {
    int cost = econ.UpgradeCost(pr.BasePrice, pr.Level + 1);
    p.Money -= cost;
    pr.Level = Math.Min(5, pr.Level + 1);
  }

  public static bool CanUpgradeHotel(PlayerState p, PropertyState pr, PropertyEconomy econ) {
    if (pr.Owner != (Owner)p.Id) return false;
    if (pr.HasHotel) return false;
    if (pr.Level != 5) return false;
    int cost = econ.HotelCost(pr.BasePrice);
    return p.Money >= cost;
  }

  public static void UpgradeHotel(PlayerState p, PropertyState pr, PropertyEconomy econ) {
    int cost = econ.HotelCost(pr.BasePrice);
    p.Money -= cost;
    pr.HasHotel = true;
  }

  public static bool CanTakeover(PlayerState buyer, PropertyState pr, PropertyEconomy econ) {
    if (pr.Owner == Owner.None) return false;
    if ((int)pr.Owner == buyer.Id) return false;
    if (pr.HasHotel && !econ.TakeoverAllowedOnHotel) return false;
    int cost = econ.TakeoverCost(pr.BasePrice, pr.Level, pr.HasHotel);
    return buyer.Money >= cost;
  }

  public static void BuyTakeover(PlayerState buyer, PlayerState seller, PropertyState pr, PropertyEconomy econ) {
    int cost = econ.TakeoverCost(pr.BasePrice, pr.Level, pr.HasHotel);
    buyer.Money -= cost;
    seller.Money += cost;
    // transfer ownership
    if (seller.Owned.Contains(pr.TileId)) seller.Owned.Remove(pr.TileId);
    if (!buyer.Owned.Contains(pr.TileId)) buyer.Owned.Add(pr.TileId);
    pr.Owner = (Owner)buyer.Id;
  }

  public static int CalcRent(PropertyState pr, PlayerState owner, PropertyEconomy econ) {
    int rent = econ.Rent(pr.BasePrice, pr.Level, pr.HasHotel);
    int bonusPct = owner.Intelligence / 300; // 300 pts = +1%
    rent += rent * bonusPct / 100;
    return rent;
  }

  public static void PayRent(PlayerState payer, PlayerState owner, int rent) {
    int reducePct = payer.Resistance / 100;
    int pay = Math.Max(0, rent - rent * reducePct / 100);
    payer.Money -= pay;
    owner.Money += rent;
  }
}
