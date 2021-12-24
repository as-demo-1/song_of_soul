using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DamagerBase))]
public class damageTimeScaleEvent : damageEventSetter
{
    public float timeScale;
    public int frameCounts;

    public override void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        base.damageEvent(damager,damageable);
        TimeScaleManager.Instance.changeTimeScaleForFrames(frameCounts, timeScale);

    }

}
