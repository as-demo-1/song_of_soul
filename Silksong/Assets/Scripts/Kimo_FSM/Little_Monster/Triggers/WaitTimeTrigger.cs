using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class WaitTimeTrigger : EnemyFSMBaseTrigger
{
    public float maxTime;
    [DisplayOnly]
    public float timer;

    public WaitTimeTrigger():base()
    {
        maxTime = 0;
    }
    public WaitTimeTrigger(float time,EnemyStates targetState=EnemyStates.Enemy_Idle_State):base(targetState)
    {
        maxTime = time;
    }
    public override bool IsTriggerReach(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    { 
        timer += Time.deltaTime;
        if(timer>maxTime)
        {
            timer = 0;
            return true;
        }
        return false;
    }

    public override void InitTrigger(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        timer = 0;
    }
}
