using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSfx : damageEventSetter
{
    public SfxSO sfxSO;

    public override void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        if (damageTiming!=DamageTiming.onMakeDamage)//暂时只需要受击特效
            //这个仅仅是读取对应位置并且生成受击特效，如果有后续互动类特效可以参考
        {
            Vector2 hittedPosition = Vector2.zero;
            hittedPosition = GetComponent<Collider2D>().bounds.ClosestPoint(damager.transform.position);
            SfxManager.Instance.creatHittedSfx(hittedPosition, hittedPosition - (Vector2)transform.position, sfxSO);
        }
    }
}
