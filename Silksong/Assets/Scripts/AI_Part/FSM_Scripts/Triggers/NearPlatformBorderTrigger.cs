using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearPlatformBorderTrigger :EnemyFSMBaseTrigger
{
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
    }


    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    {
        return fsm_Manager.nearPlatformBoundary();
    }
}
