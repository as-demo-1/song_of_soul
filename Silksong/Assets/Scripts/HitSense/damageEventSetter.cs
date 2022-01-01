using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class damageEventSetter : MonoBehaviour
{
   [SerializeField] protected DamageTiming damageTiming;
   [SerializeField] protected LayerMask targetLayer;
    public virtual void tryDamageEvent(DamagerBase damager, DamageableBase damageable)
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

        if (targetLayer.Contains(targetObj))
        {
            damageEvent(damager, damageable);
        }
    }

    public abstract void damageEvent(DamagerBase damager, DamageableBase damageable);

    protected void Awake()
    {
        switch (damageTiming)
        {
            case DamageTiming.onMakeDamage:
                {
                    DamagerBase damager = GetComponent<DamagerBase>();
                    if(damager)
                    {
                        damager.makeDamageEvent.AddListener(tryDamageEvent);
                    }
                    break;
                }
            case DamageTiming.onTakeDamage:
                {
                    DamageableBase damable = GetComponent<DamageableBase>();
                    if (damable)
                    {
                        damable.takeDamageEvent.AddListener(tryDamageEvent);
                    }
                    break;
                }
            case DamageTiming.onDie:
                {
                    HpDamable damable = GetComponent<HpDamable>();
                    if (damable)
                    {
                        damable.onDieEvent.AddListener(tryDamageEvent);
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