using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrivedPos_Trigger : EnemyFSMBaseTrigger
{
    public bool localPos;
    public Vector2 target;
    public float checkRange;
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        if (localPos)
        {
            return (Vector2.Distance(enemyFSM.transform.localPosition, target) < checkRange);
        }
        else
        {
            return (Vector2.Distance(enemyFSM.transform.position, target) < checkRange);
        }
    }
}
