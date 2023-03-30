using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanChangeToStageTwo : HpBelowOrEqual
{
    public override TaskStatus OnUpdate()
    {
        if (WhaleBossManager.Instance.stage != EBossBattleStage.StageOne) return TaskStatus.Failure; 
        return base.OnUpdate();
    }
}
