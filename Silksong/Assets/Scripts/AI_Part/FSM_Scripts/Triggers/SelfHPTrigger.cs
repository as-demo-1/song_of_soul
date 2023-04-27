using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfHPTrigger :EnemyFSMBaseTrigger
{
    public int Percent;
    public bool isLower;
    public bool actOnce;
    private   HpDamable hpDamable;
    private bool acted = false;

    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        hpDamable = fsm_Manager.damageable as HpDamable;
        Debug.Log(fsm_Manager.damageable);
    }

    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        if (acted == true && actOnce == true)
        {
            return false;
        }
        base.IsTriggerReachInUpdate(fsm_Manager);
        hpDamable = fsm_Manager.damageable as HpDamable;
        int tem = hpDamable.CurrentHp * 100 / hpDamable.MaxHp;

        if(isLower)
        {
            if (tem < Percent)
            {
                acted = true;
                return true;
            }
            else
                return false;
        }else
        {
            if (tem < Percent)
                return false;
            else
            {
                acted = true;
                return true;
            }
        }
    }

}
