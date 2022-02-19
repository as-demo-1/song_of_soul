using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWallTrigger :EnemyFSMBaseTrigger
{

    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerType = EnemyTriggers.HitWallTrigger;
        
    }
    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        return fsm_Manager.hitWall();
    }
}
