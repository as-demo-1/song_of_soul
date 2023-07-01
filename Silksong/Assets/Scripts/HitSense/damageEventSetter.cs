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
    /// <summary>
    /// 在这里设置了判断攻击并且对目标对象执行，是trigger的实现
    /// </summary>
    /// <param name="damager"></param>
    /// <param name="damageable"></param>
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
            //这里设置了受击的多个可能，分别为发出攻击时触发，被攻击者收到时触发，死亡时触发
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

/// <summary>
/// 这里判断了触发的伤害与目标是否为同一层
/// </summary>
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

