using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNotReady : BattleConditional
{
    public override TaskStatus OnUpdate()
    {
        //Debug.Log("Skill not ready");
        bool shouldVomit = WhaleBossManager.Instance.borderSkillCdTimer <= 0 && WhaleBossManager.Instance.goingOutInBorder();
        bool shouldNormalSkill = WhaleBossManager.Instance.normalSkillCdTimer <= 0;
        return (shouldNormalSkill || shouldVomit)?TaskStatus.Failure:TaskStatus.Success;
    }
}
