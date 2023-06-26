using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class ChaseEnemyAction : Action
{
    public LayerMask followLayer;// 追踪目标所在图层
    public float radius;
    public float playerDis;
    private Tween move;
    public SharedTransform target;

    private Collider2D[] collider2Ds;
    // Start is called before the first frame update
    public override void OnStart()
    {
        collider2Ds = Physics2D.OverlapCircleAll(transform.position, radius, followLayer);
        if (collider2Ds.Length <= 0) return;
        Debug.Log(collider2Ds[0].name);
        foreach (var enemy in collider2Ds)
        {
            if (!((PlayerController.Instance.transform.position - enemy.transform.position)
                    .sqrMagnitude < playerDis)) continue;
            if (enemy.GetComponent<HpDamable>() == null) continue;
            Vector3 pos = (transform.position - enemy.transform.position).normalized * 5.0f +
                          enemy.transform.position;
            transform.DOMove(pos, 2.0f);
            target.Value = enemy.transform;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (collider2Ds.Length <= 0) return TaskStatus.Failure;
        // if ((PlayerController.Instance.transform.position -transform.position).sqrMagnitude > playerDis)
        // {
        //     move?.Kill();
        //     return TaskStatus.Failure;
        // }
        // else
        // {
        //     return TaskStatus.Running;
        // }
         return base.OnUpdate();
    }
}
