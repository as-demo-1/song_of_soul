using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 在等待钩子回来的trigger
 */
public class WaitHookBackTrigger : EnemyFSMBaseTrigger
{
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerType = EnemyTriggers.WaitHookBackTrigger;
    }
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        return !enemyFSM.transform.GetChild(0).gameObject.activeSelf;
    }
}
