using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderSkillReady : BattleConditional
{
    public override TaskStatus OnUpdate()
    {
        return (WhaleBossManager.Instance.goingOutInBorder() && WhaleBossManager.Instance.borderSkillCdTimer<=0) ? TaskStatus.Success : TaskStatus.Failure;
    }
}
