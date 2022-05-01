using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 就是用来测是否到下一个月亮碎片的范围内
 * minDistance最好比月亮碎片的摆动半径要大一点
 */
public class DistanceToMoonPointTrigger: EnemyFSMBaseTrigger
{
    public float minDistance;
    float sqrMinDistance;
    public override void InitTrigger(EnemyFSMManager enemyFSM)
    {
        sqrMinDistance = minDistance * minDistance;
        base.InitTrigger(enemyFSM);
    }
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        if((enemyFSM.transform.position - FindMoonPoinTrigger.moonPointDic[enemyFSM].transform.position).sqrMagnitude > sqrMinDistance
            ||enemyFSM.rigidbody2d.velocity.magnitude>1)
        {
            return false;
        }return true;
    }
}
