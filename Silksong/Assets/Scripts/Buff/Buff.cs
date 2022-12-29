using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    ElectricMark,
    SoulStatu,
    SpeedUp
}

public class Buff
{
    public BuffType buffType;
}

public class ElectricMark : Buff
{
    public ElectricMark()
    {
        buffType = BuffType.ElectricMark;
    }

    public static uint counter = 0;
}

public class SpeedUp : Buff
{
    private float _speedUpPerset;
    public SpeedUp()
    {
        buffType = BuffType.SpeedUp;
    }
}
