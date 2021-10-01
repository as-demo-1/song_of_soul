using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWallTrigger :EnemyFSMBaseTrigger
{
    private bool isHitWall;
    private bool isSigned=false;
    public override void InitTrigger(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerID = EnemyTriggers.HitWallTrigger;
        isHitWall = false;
        isSigned = false;
        
    }
    public override bool IsTriggerReach(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        if(!isSigned)
        {
            EventsManager.Instance.AddListener(fsm_Manager.gameObject, EventType.onEnemyHitWall, HitTheWall);
            isSigned = true;
        }

        if (isHitWall)
        {
            isHitWall = false;
            return true;
        }
        else
            return false;
    }
    private void HitTheWall()
    {
        isHitWall = true;
    }
}
