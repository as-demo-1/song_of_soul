using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class FollowPlayerAction : Action
{
    public float radius;
    private Tween move;
    public override void OnStart()
    {
        move = transform.DOMove(PlayerController.Instance.transform.position, 3.0f);
    }

    public override TaskStatus OnUpdate()
    {
        if ((PlayerController.Instance.transform.position - transform.position).sqrMagnitude <
            radius)
        {
            Debug.Log("已经接近玩家位置");
            move?.Kill();
            return TaskStatus.Failure;
        }
    
        return base.OnUpdate();
    }

    public override void OnEnd()
    {
        //move?.Kill();
        base.OnEnd();
    }
}
