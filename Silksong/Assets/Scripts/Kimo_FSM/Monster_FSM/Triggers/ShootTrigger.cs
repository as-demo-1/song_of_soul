using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootTrigger : EnemyFSMBaseTrigger
{
    public float range;
    public LayerMask layer;
    public override void InitTrigger(FSMManager<EnemyStates, EnemyTriggers> fSMManager)
    {
        base.InitTrigger(fSMManager);
        triggerID = EnemyTriggers.ShootTrigger;
        targetState = EnemyStates.Enemy_Shoot_State;
    }
    public override bool IsTriggerReach(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        Collider2D target = Physics2D.OverlapCircle(fsm_Manager.transform.position, range, layer);
        if (target)
        {
            Enemy_Shoot_State shoot = (Enemy_Shoot_State)fsm_Manager.statesDic[EnemyStates.Enemy_Shoot_State];
            shoot.target = target.transform;
            return true;
        }
        return false;
    }
}