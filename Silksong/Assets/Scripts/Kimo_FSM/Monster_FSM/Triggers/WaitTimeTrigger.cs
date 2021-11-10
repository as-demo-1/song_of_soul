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

    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    { 
        timer += Time.deltaTime;
        if(timer>maxTime)
        {
            timer = 0;
            return true;
        }
        return false;
    }

    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        timer = 0;
    }
}
