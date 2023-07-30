using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Monsters")]

public class PlayAnimation : Action
{
    private Animator animator;
    public string animation;
    public override void OnAwake()
    {

    }

    public override void OnStart()
    {
        animator = this.GetComponent<Animator>();
        animator.Play(animation);
    }

    public override TaskStatus OnUpdate()
    {
        AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(animatorStateInfo.IsName(animation))
        {
            return TaskStatus.Running;
        }
        return TaskStatus.Success;
    }
}
