using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DistanceCondition : Conditional
{

    public LayerMask followLayer;// 追踪目标所在图层
    public float radius;

    public SharedTransform targetEnemy;

    public override TaskStatus OnUpdate()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, radius, followLayer);
        if (collider2Ds.Length>0)
        {
            Debug.Log(collider2Ds[0].name);
            targetEnemy = collider2Ds[0].transform;
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}
