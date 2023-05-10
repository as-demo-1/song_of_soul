using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrickArrivedTrigger : EnemyFSMBaseTrigger
{
    
    SeaUrchin seaUrchin;
    public string targetName="Enemy_Stop_State";
    public override void InitTrigger(EnemyFSMManager enemyFSM)
    {
        base.InitTrigger(enemyFSM);
        seaUrchin=enemyFSM as SeaUrchin;
    }
    public override bool IsTriggerReachInUpdate(EnemyFSMManager enemyFSM)
    {
        foreach(var prick in seaUrchin.pricks){
            if(prick.currentStateName!=targetName){
                return false;
            }
        }
        return true;
    }
}
