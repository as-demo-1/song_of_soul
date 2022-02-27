using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTurnTrigger : EnemyFSMBaseTrigger
{
    public bool returnFalseWhenTurn;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
    }
    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        Vector2 targetDir = fsm_Manager.getTargetDir();
        bool faceLeft = fsm_Manager.currentFacingLeft();
        if(targetDir.x>0? faceLeft : !faceLeft)//需要转身
        {
            //Debug.Log("turn");
            return true&& (!returnFalseWhenTurn);
        }
        return returnFalseWhenTurn;
    }
}
