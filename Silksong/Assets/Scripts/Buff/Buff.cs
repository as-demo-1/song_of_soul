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
    public static uint counter = 0;
    public static List<Hittable> targets = new List<Hittable>();
    
    private int layers;
    public ElectricMark()
    {
        layers = 0;
        buffType = BuffType.ElectricMark;
    }

    public void AddOneLayer()
    {
        layers++;
    }

    public int GetLayerNum()
    {
        return layers;
    }

    public static void LinkTargets()
    {
        
    }
    
}

public class SpeedUp : Buff
{
    private float _speedUpPerset;
    public SpeedUp()
    {
        buffType = BuffType.SpeedUp;
    }
}
