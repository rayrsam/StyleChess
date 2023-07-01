using System.Collections.Generic;
using UnityEngine;

public static class RosterHolder
{
    public static int RedactedArmy = 0;

    public static Army WhiteArmy = new Army();
    public static Army BlackArmy = new Army();

    public static void SetArmy(Army army)
    {
        if (RedactedArmy == 0) WhiteArmy.SetArmy(army.ArmyPiecesList[0], army.ArmyPiecesList[1]);
        else BlackArmy.SetArmy(army.ArmyPiecesList[0], army.ArmyPiecesList[1]);
    }
}