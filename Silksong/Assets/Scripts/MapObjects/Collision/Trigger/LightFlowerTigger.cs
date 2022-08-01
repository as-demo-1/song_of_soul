using UnityEngine;
/// <summary>
/// 花开的触发器
/// </summary>作者：青神羽
public class LightFlowerTigger : Trigger2DBase
{
    public BurstFlowerCollider flower;
    protected override void enterEvent()
    {
        if (flower)
        {
            flower.Burst();
            Debug.Log("碰到了");
        }
    }

    protected override void exitEvent()
    {
        flower.Back();
        Debug.Log("出去了");
    }
}
