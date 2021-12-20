using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DamagerBase))]
public class AttackTimeScale : MonoBehaviour, IDamageEvent
{
    public float timeScale;
    public int frameCounts;
    public LayerMask targetLayer;
    public void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        if (targetLayer.Contains(damageable.gameObject))
        {
            TimeScaleManager.Instance.changeTimeScaleForFrames(frameCounts, timeScale);
        }
    }

    private void Awake()
    {
        GetComponent<DamagerBase>().makeDamageEvent.AddListener(damageEvent);
    }

}
