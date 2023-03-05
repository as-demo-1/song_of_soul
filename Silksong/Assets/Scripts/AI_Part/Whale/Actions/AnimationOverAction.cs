using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  AnimationOverAction : BattleAction
{
    public float animationSpeed=1;
    public string animationName;
    public override void OnStart()
    {
        base.OnStart();
        if(animationSpeed!=1)
        animator.speed = animationSpeed;
    }

    public override void OnEnd()
    {
        animator.speed = 1;
    }


    public override TaskStatus OnUpdate()
    {
      
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1)
        {
            //Debug.Log(animationName+" over");
            return TaskStatus.Success;
        }
        else
        {
         // Debug.Log(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name + " " + stateInfo.normalizedTime);
            return TaskStatus.Running;
        }
    }

}
