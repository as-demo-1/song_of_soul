using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayOverTrigger :EnemyFSMBaseTrigger
{
    AnimatorStateInfo info;
    public string checkAnimationName;
    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    {

        if (fsm_Manager.animator != null)
        {
            info = fsm_Manager.animator.GetCurrentAnimatorStateInfo(0);
            if (checkAnimationName == string.Empty)
            {
                if (info.normalizedTime >= 0.99)
                    return true;
            }
            else
                if (info.normalizedTime >= 0.99&&info.IsName(checkAnimationName))
                    return true;
        }
        return false;
    }
}
