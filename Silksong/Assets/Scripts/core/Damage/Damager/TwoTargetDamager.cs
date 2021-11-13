using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// 对两类目标不同伤害的damager的基类   例如酸水,玩家的攻击
/// </summary>作者：青瓜
public class TwoTargetDamager : DamagerBase
{
    public LayerMask hittableLayers2;//另一目标
    public int damage2;//对另一目标的伤害


    public override int getDamage(DamageableBase target)//根据目标返回伤害
    {
        if (hittableLayers2.Contains(target.gameObject))
        {
            return damage2;
        }
        else
        {
            return damage;
        }
    }

    protected override void makeDamage(DamageableBase Damageable)
    {
        makeDamageEvent.Invoke(this, Damageable);
    }

}
