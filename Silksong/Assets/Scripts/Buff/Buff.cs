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
    public static Dictionary<uint, HpDamable> targets = new Dictionary<uint, HpDamable>();
    private static uint currentIndex = 0;
    
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

    public void ResetLayer()
    {
        layers = 0;
    }

    public GameObject electricMarkPrefeb = default;
    public void ShowPerformance(Transform perfPos, GameObject perf)
    {
        if (electricMarkPrefeb is null)
        {
            // TODO : 后续改为运行时实例化
            electricMarkPrefeb = GameObject.Instantiate(perf);
        }
        electricMarkPrefeb.transform.parent = perfPos;
        electricMarkPrefeb.transform.localPosition = Vector3.zero;
        electricMarkPrefeb.SetActive(true);
    }
    public void HidePerformance()
    {
        electricMarkPrefeb.SetActive(false);
    }

    public static void AddTarget(uint index, HpDamable target)
    {
        targets.Add(index, target);
        counter++;
    }

    public static void RemoveTarget(uint index)
    {
        targets.Remove(index);
        if (targets.Count < 1)
        {
            counter = 0;
        }
    }
    
    public static uint GetCurrentIndex()
    {
        return currentIndex++;
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
