using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToPosition : BattleAction
{
    public SharedVector2 targetPos;
    public float arriveDistance;
    public float speed;

    public override void OnStart()
    {
        base.OnStart();
        animator.SetBool("idle",true);
    }
    public override void OnEnd()
    {
        base.OnEnd();
        animator.SetBool("idle", false);
    }
    public override void OnFixedUpdate()
    {
        battleAgent.moveToTargetByFixedUpdate(targetPos.Value, speed);
    }
    public override TaskStatus OnUpdate()
    {
        float dis = Vector2.Distance(targetPos.Value, transform.position);
        if (dis > arriveDistance)
        {
       
            return TaskStatus.Running;
        }
        else return TaskStatus.Success;
    }
}
