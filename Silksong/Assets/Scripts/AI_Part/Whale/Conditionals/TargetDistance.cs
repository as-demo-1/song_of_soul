using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
public class TargetDistance : BattleConditional
{
    public float arriveDistance = 2f;
    public override TaskStatus OnUpdate()
    {
        float dis = Vector3.Distance(target.Value.transform.position, transform.position);
        return dis > arriveDistance ? TaskStatus.Success : TaskStatus.Failure;
    }
}
