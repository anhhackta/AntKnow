using System;

public enum TileType { Start, Property, Tax, Bonus, Chance, Accident, Quiz, Travel, Jail, GoToJail, FreeParking }
public enum Owner { None = 0, P1 = 1, P2 = 2, P3 = 3, P4 = 4 }

public enum CardType { Passive, Active }
public enum CardTrigger {
  Manual,
  StartOfTurn,
  EndOfTurn,
  OnQuizFail,
  OnRentPay,
  OnRentReceive,
  OnTravel,
  Custom
}

