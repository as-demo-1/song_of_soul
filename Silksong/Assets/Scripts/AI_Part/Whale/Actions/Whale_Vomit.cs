using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Vomit : AnimationOverAction
{
    public override void OnStart()
    {
        base.OnStart();
        animator.SetBool("vomit",true);
    }
    public override void OnEnd()
    {
        base.OnEnd();
        WhaleBossManager.Instance.resetVomitSkillCdTimer();
        animator.SetBool("vomiting", false);
    }
    public override TaskStatus OnUpdate()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("attack2"))
        {
            animator.SetBool("vomit", false);
            animator.SetBool("vomiting",true);
        }
        return base.OnUpdate();
    }
}
