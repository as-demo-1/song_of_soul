using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTarget : BattleAction
{
    public override TaskStatus OnUpdate()
    {
        string tag = "Player";
        target.Value = GameObject.FindWithTag(tag);
    
         return target.Value != null ? TaskStatus.Success : TaskStatus.Failure;
    }
}
