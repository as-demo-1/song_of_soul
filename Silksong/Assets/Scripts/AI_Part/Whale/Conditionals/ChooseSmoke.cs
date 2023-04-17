using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseSmoke : BattleConditional
{
    public override TaskStatus OnUpdate()
    {
        if(WhaleBossManager.Instance.stage==EBossBattleStage.StageOne)
        {
            return TaskStatus.Failure;
        }
        else
        {
            return WhaleBossManager.Instance.smokeSkillCdTimer < 0 ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
