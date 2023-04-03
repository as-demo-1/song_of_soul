using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InTurnTrigger :EnemyFSMBaseTrigger
{
    public List<string> targetList;
    [SerializeField] private int currentState;
    
    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        targetState = targetList[currentState];
        currentState = (currentState + 1) % targetList.Count;
        return true;
    }
}

