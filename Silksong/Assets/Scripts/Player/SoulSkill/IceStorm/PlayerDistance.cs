using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class PlayerDistance : Conditional
{
    public float radius;
    public float safeRdaius;
    public override TaskStatus OnUpdate()
    {
        if ((PlayerController.Instance.transform.position -transform.position).sqrMagnitude > radius)
        {
            return TaskStatus.Success;
        }
        else if((PlayerController.Instance.transform.position -transform.position).sqrMagnitude > safeRdaius)
        {
            return TaskStatus.Running;
        }
        else
        {
            return TaskStatus.Failure;
        }
         
             
            
    }
}
