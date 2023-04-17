using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNotReady : BattleConditional
{
    public override TaskStatus OnUpdate()
    {
        //Debug.Log("Skill not ready");
        bool shouldBorderSkill = WhaleBossManager.Instance.borderSkillCdTimer <= 0 && WhaleBossManager.Instance.goingOutInBorder();
        bool shouldIceRain = WhaleBossManager.Instance.iceRainSkillCdTimer <= 0;
        bool shouldSmoke = WhaleBossManager.Instance.smokeSkillCdTimer <= 0;
        return (shouldIceRain || shouldBorderSkill || shouldSmoke)?TaskStatus.Failure:TaskStatus.Success;
    }
}
