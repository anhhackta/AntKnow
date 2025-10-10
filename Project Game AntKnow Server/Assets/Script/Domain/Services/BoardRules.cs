using System;

/// <summary>
/// Board rules - Using SimpleTileData for property pricing
/// </summary>
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
  // NEW: Using SimpleTileData instead of PropertyEconomy
  
  public static bool CanUpgradeHouse(PlayerState p, PropertyState pr, SimpleTileData tileData) {
    if (pr.Owner != (Owner)p.Id) return false;
    if (pr.HasHotel) return false;
    if (pr.Level >= 5) return false;
    
    // Get upgrade cost from SimpleTileData
    int cost = tileData.GetUpgradeCost(pr.Level, pr.Level + 1);
    return p.Money >= cost;
  }

  public static void UpgradeHouse(PlayerState p, PropertyState pr, SimpleTileData tileData) {
    int cost = tileData.GetUpgradeCost(pr.Level, pr.Level + 1);
    p.Money -= cost;
    pr.Level = Math.Min(5, pr.Level + 1);
  }

  public static bool CanUpgradeHotel(PlayerState p, PropertyState pr, SimpleTileData tileData) {
    if (pr.Owner != (Owner)p.Id) return false;
    if (pr.HasHotel) return false;
    if (pr.Level != 5) return false;
    
    int cost = tileData.hotelCost;
    return p.Money >= cost;
  }

  public static void UpgradeHotel(PlayerState p, PropertyState pr, SimpleTileData tileData) {
    int cost = tileData.hotelCost;
    p.Money -= cost;
    pr.HasHotel = true;
  }

  public static bool CanTakeover(PlayerState buyer, PropertyState pr, SimpleTileData tileData) {
    if (pr.Owner == Owner.None) return false;
    if ((int)pr.Owner == buyer.Id) return false;
    
    int cost = tileData.GetTakeoverCost(pr.Level, pr.HasHotel);
    return buyer.Money >= cost;
  }

  public static void BuyTakeover(PlayerState buyer, PlayerState seller, PropertyState pr, SimpleTileData tileData) {
    int cost = tileData.GetTakeoverCost(pr.Level, pr.HasHotel);
    buyer.Money -= cost;
    seller.Money += cost;

    // Transfer ownership
    if (seller.Owned.Contains(pr.TileId)) seller.Owned.Remove(pr.TileId);
    if (!buyer.Owned.Contains(pr.TileId)) buyer.Owned.Add(pr.TileId);
    pr.Owner = (Owner)buyer.Id;
  }

  public static int CalcRent(SimpleTileData tileData, PropertyState pr, PlayerState owner) {
    // Get base rent from SimpleTileData
    int rent = tileData.GetRent(pr.HasHotel ? 5 : pr.Level);
    
    // Apply Intelligence bonus (300 pts = +1%)
    int bonusPct = owner.Intelligence / 300;
    rent += rent * bonusPct / 100;
    
    return rent;
  }

  public static int PayRent(PlayerState payer, PlayerState owner, int rent) {
    int reducePct = payer.Resistance / 100;
    int pay = Math.Max(0, rent - rent * reducePct / 100);
    payer.Money -= pay;
    owner.Money += pay;
    return pay;
  }

  /// <summary>
  /// Calculate total assets for win condition
  /// Total assets = Money + Sum of all property sell prices
  /// </summary>
  public static int CalculateTotalAssets(PlayerState p, GameState gameState) {
    int totalAssets = p.Money;
    
    // Add value of all owned properties
    foreach (int tileId in p.Owned) {
      if (gameState.Properties.ContainsKey(tileId)) {
        var pr = gameState.Properties[tileId];
        var tileData = SimpleBoardConfig.GetTile(tileId);
        if (tileData != null) {
          // Sell price = 60% of total purchase cost
          int sellPrice = tileData.GetSellPrice(pr.Level, pr.HasHotel);
          totalAssets += sellPrice;
        }
      }
    }
    
    return totalAssets;
  }
}

