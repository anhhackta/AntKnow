public sealed class PropertyState {
  public int TileId;
  public Owner Owner = Owner.None;
  public int Level;              // 0..4 (houses), 5 = hotel
  public bool HasHotel;          // true when level = 5
  public int BasePrice;
  public float RentMultiplier = 1f; // Agility effect: 1 or 2
}
