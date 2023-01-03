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
        m_eventType = BattleEventType.PlayerNormalAtk;
    }
    
    public override bool AtkPerTarget(Hittable target)
    {
        if(!IsAtkSuccess(target)) return false; 
        target.GetDamage(Damage(_atk));
        target.GetRepel(repelDistance);
        return true;
    }

    protected override bool IsAtkSuccess(Hittable target)
    {
        //return true;
        float dis = (GetComponentInParent<Transform>().position - target.transform.position).magnitude;
        return (GetComponentInParent<Transform>().position - target.transform.position).magnitude <= _atkDistance;
    }

    int Damage(int atk)
    {
        return atk;
    }
}
