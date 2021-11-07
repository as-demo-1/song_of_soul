using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDistanceTrigger :EnemyFSMBaseTrigger
{
    public float checkRadius;
    public bool isEnterTrigger;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerID = EnemyTriggers.PlayerDistanceTrigger;
    }
    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    {
        Vector3 v = (fsm_Manager as EnemyFSMManager).getTargetDir();
        if(v.sqrMagnitude<checkRadius*checkRadius)
        {
            if (isEnterTrigger)
                return true;
            else
                return false;
        }
        else
        {
            if (isEnterTrigger)
                return false;
            else
                return true;
        }
    }
}
