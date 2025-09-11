public sealed class PropertyState {
  public int TileId;
  public Owner Owner = Owner.None;
  public int Level;              // 0..5 (houses), 0 = land only
  public bool HasHotel;          // true when upgraded beyond level 5
  public int BasePrice;
}
