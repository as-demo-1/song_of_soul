using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHittedTrigger :EnemyFSMBaseTrigger
{
    [DisplayOnly]
    public bool isHitted=false;
    public bool isInvincible;

    public override void InitTrigger(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        targetState = EnemyStates.Enemy_Hitted_State;
        EventsManager.Instance.AddListener(fsm_Manager.gameObject, EventType.onTakeDamager, Hitted);
    }

    private void Hitted()
    {
        if (!isInvincible)
            isHitted = true;
        Debug.Log(this.GetHashCode());

    }
    public override bool IsTriggerReach(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        if(isHitted)
        {
            Debug.Log(this.GetHashCode());
            isHitted = false;
            return true;
        }
        else
        {
            return false;
        }
    }
}
