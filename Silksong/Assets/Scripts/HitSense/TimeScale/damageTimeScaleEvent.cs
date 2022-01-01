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
