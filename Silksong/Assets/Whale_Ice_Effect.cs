using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Ice_Effect : makeDamageEventSetter
{
    public SfxSO sfxSO;
    public override void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        //Debug.Log("2333");
        Vector2 hittedPosition = Vector2.zero;

        hittedPosition = GetComponent<Collider2D>().bounds.ClosestPoint(damageable.transform.position);
        SfxManager.Instance.creatHittedSfx(hittedPosition, damageable.transform.position-damager.transform.position, sfxSO);
        Destroy(damager.transform.parent.gameObject);
    }

  
}
