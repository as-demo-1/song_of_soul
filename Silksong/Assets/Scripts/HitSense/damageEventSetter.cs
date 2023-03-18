using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageTiming
{
    onTakeDamage,
    onMakeDamage,
    onDie,

}

public abstract class damageEventSetter : MonoBehaviour
{
   [SerializeField] protected DamageTiming damageTiming;

   [SerializeField] 
   [Tooltip("choose layer which trigger the event.If set nothing,it is same as everything")]
    protected LayerMask targetLayer;
    public virtual void triggerDamageEvent(DamagerBase damager, DamageableBase damageable)
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

        if (targetLayer.Contains(targetObj) || targetLayer==0)
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
                        damager.makeDamageEvent.AddListener(triggerDamageEvent);
                    }
                    break;
                }
            case DamageTiming.onTakeDamage:
                {
                    DamageableBase damable = GetComponent<DamageableBase>();
                    if (damable)
                    {
                        damable.takeDamageEvent.AddListener(triggerDamageEvent);
                    }
                    break;
                }
            case DamageTiming.onDie:
                {
                    HpDamable damable = GetComponent<HpDamable>();
                    if (damable)
                    {
                        damable.onDieEvent.AddListener(triggerDamageEvent);
                    }
                    break;
                }
        }

    }
}


public abstract class makeDamageEventSetter : MonoBehaviour
{
    [SerializeField]
    [Tooltip("choose layer which trigger the event.If set nothing,it is same as everything")]
    protected LayerMask targetLayer;
    public virtual void triggerDamageEvent(DamagerBase damager, DamageableBase damageable)
    {
        GameObject targetObj = damageable.gameObject;

        if (targetLayer.Contains(targetObj) || targetLayer == 0)
        {
            damageEvent(damager, damageable);
        }
    }

    public abstract void damageEvent(DamagerBase damager, DamageableBase damageable);

    protected void Awake()
    {
        DamagerBase damager = GetComponent<DamagerBase>();
        if (damager)
        {
            damager.makeDamageEvent.AddListener(triggerDamageEvent);
        }   

    }
}

