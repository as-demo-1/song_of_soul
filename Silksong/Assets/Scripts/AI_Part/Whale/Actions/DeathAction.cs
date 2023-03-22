using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAction : BattleAction
{
    public override void OnStart()
    {
        base.OnStart();
        animator.SetTrigger("die");
    }
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
