using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// /对玩家造成伤害后 将玩家送至重生点的damager 如酸水  重生暂未实现 
/// </summary>作者：青瓜
public class RebornDamager:TwoTargetDamager
{
    public LayerMask rebornLayer;
    void Start()
    {
    }

    protected  override void makeDamage(DamageableBase damageable)
    {
        base.makeDamage(damageable);
        if(rebornLayer.Contains(damageable.gameObject) && (damageable as HpDamable).currentHp>0 )
        {
            GameObjectTeleporter.playerReborn();
        }
    }
}
