using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayOverTrigger :EnemyFSMBaseTrigger
{
    AnimatorStateInfo info;
    public override bool IsTriggerReach(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {

        if (fsm_Manager.animator != null)
        {
            info = fsm_Manager.animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 0.99)
                return true;
        }
        return false;
    }
}
