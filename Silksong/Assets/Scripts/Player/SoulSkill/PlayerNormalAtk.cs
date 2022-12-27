using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerNormalAtk : Hitter
{
    public float repelDistance;
    private void Start()
    {
        base.RefreshAtk(GetComponentInParent<PlayerInfomation>().atk);
        m_actionName = "Player Normal Atk";
    }

    public override void AtkPerTarget(Hittable target)
    {
        base.AtkPerTarget(target);
        target.GetDamage(_atk);
        target.GetRepel(repelDistance);
        //target.
    }
}
