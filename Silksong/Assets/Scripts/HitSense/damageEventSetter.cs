using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class damageEventSetter : MonoBehaviour
{
   [SerializeField] private DamageTiming damageTiming;
   [SerializeField] private LayerMask targetLayer;
    public virtual void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        GameObject targetObj;
        if(damageTiming==DamageTiming.onMakeDamage)
        {
            targetObj = damageable.gameObject;
        }
        else
        {
            targetObj = damager.gameObject;
        }
        if (!targetLayer.Contains(targetObj)) return;
    }

    protected void Awake()
    {
        switch (damageTiming)
        {
            case DamageTiming.onMakeDamage:
                {
                    DamagerBase damager = GetComponent<DamagerBase>();
                    if(damager)
                    {
                        damager.makeDamageEvent.AddListener(damageEvent);
                    }
                    break;
                }
            case DamageTiming.onTakeDamage:
                {
                    DamageableBase damable = GetComponent<DamageableBase>();
                    if (damable)
                    {
                        damable.takeDamageEvent.AddListener(damageEvent);
                    }
                    break;
                }
            case DamageTiming.onDie:
                {
                    HpDamable damable = GetComponent<HpDamable>();
                    if (damable)
                    {
                        damable.onDieEvent.AddListener(damageEvent);
                    }
                    break;
                }
        }

    }
}


public enum DamageTiming
{ 
    onTakeDamage,
    onMakeDamage,
    onDie,

}