using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
///只受来自一个方向的伤害 单向门用 或者特殊怪物
/// </summary>作者：青瓜
public class OneDirectDamable : HpDamable
{
    public bool leftInvulnerable;//左边的伤害无效 若为false则右边的伤害无效
    public override void takeDamage(DamagerBase damager)
    {

       // hittedEffect();
        damageDirection = damager.transform.position - transform.position;
        if ((leftInvulnerable && damageDirection.x < 0) || (!leftInvulnerable && damageDirection.x>0))
        {
            return;
        }
        base.takeDamage(damager);
        
    }
}
