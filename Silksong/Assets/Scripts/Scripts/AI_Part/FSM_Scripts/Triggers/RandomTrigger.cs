using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RandomTrigger :EnemyFSMBaseTrigger
{
    public EnemyTriggers insideTriggerType;
    public EnemyFSMBaseTrigger insideTrigger;
    public List<string> targetList;
    
    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        bool tem= insideTrigger.IsTriggerReachInUpdate(fsm_Manager);
        if(tem)
        {
            targetState = targetList[Random.Range(0, targetList.Count)];
        }
        return tem;
    }
}

