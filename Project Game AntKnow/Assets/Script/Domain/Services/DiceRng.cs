using System;

public static class DiceRng {
  static System.Random _rng = new System.Random();
  public static (int d1,int d2,int sum,bool isDouble) Roll2() {
    int d1=_rng.Next(1,7), d2=_rng.Next(1,7);
    return (d1,d2,d1+d2,d1==d2);
  }
}

