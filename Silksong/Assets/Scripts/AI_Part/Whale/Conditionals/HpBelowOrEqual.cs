using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class HpBelowOrEqual : BattleConditional
{
    public int val;
    public override TaskStatus OnUpdate()
    {
        return damable.CurrentHp<= val ? TaskStatus.Success : TaskStatus.Failure;
    }
}
