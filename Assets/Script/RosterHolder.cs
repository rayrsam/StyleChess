using UnityEngine;

public static class RosterHolder
{
    private static Army WhiteArmy
    {
        get => WhiteArmy;
        set => WhiteArmy = value;
    }

    private static Army BlackArmy
    {
        get => BlackArmy;
        set => BlackArmy = value;
    }
}