using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseBump : BattleConditional
{
    public override TaskStatus OnUpdate()
    {
        if (WhaleBossManager.Instance.stage == EBossBattleStage.StageOne)
        {
            float yTarget = target.Value.transform.position.y;
            float ySelf = transform.position.y;
            float a = Random.Range(0f, 1f);
            if (Mathf.Abs(yTarget-ySelf)<5)
            {      
                if(a>0.4f)
                {
                    return TaskStatus.Failure;
                }
                else
                {
                    return TaskStatus.Success;
                }
            }
            else
            {
                return TaskStatus.Success;
            }
        }
        else if(WhaleBossManager.Instance.stage==EBossBattleStage.StageTwo)
        {
            float a = Random.Range(0f,1f);
            if(a<=0.5f)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Success;
    }
}
