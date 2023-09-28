using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillDamager : DamagerBase
{

    public bool isBullet;
    public void Awake()
    {
        hitAnythingEvent.AddListener(BulletHit);
    }

    protected override void makeDamage(DamageableBase Damageable)
    {
        makeDamageEvent.Invoke(this, Damageable);
        
    }

    private void BulletHit(GameObject hit)
    {
        //
        if (hit != null && hit.layer.Equals(LayerMask.NameToLayer("Ground")))
        {
            //Debug.Log("击中墙壁");
            Destroy(this.gameObject,0.2f);
            makeDamageEvent.Invoke(this, null);
        }
    }
}
