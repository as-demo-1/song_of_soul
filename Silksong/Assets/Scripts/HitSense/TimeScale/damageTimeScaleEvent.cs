using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damageTimeScaleEvent : damageEventSetter
{
   [SerializeField] private float timeScale;
   [SerializeField] private int frameCounts;

    public override void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        TimeScaleManager.Instance.changeTimeScaleForFrames(frameCounts, timeScale);
    }

}
//这个猜测是设置攻击事件的反馈时间的帧数和对应的时间？