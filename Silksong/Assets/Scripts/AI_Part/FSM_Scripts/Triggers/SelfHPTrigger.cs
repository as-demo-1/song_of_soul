using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfHPTrigger :EnemyFSMBaseTrigger
{
    public int Percent;
    public bool isLower;
    private   HpDamable hpDamable;

    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        hpDamable = fsm_Manager.damageable as HpDamable;
    }

    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        base.IsTriggerReachInUpdate(fsm_Manager);
        hpDamable = fsm_Manager.damageable as HpDamable;
        int tem = hpDamable.currentHp * 100 / hpDamable.maxHp;

        if(isLower)
        {
            if (tem < Percent)
                return true;
            else
                return false;
        }else
        {
            if (tem < Percent)
                return false;
            else
                return true;
        }
    }

}
