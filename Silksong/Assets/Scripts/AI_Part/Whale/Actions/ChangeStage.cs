using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStage : BattleAction
{
    public override void OnStart()
    {
        base.OnStart();
        WhaleBossManager.Instance.stage = EBossBattleStage.StageTwo;

        GameObject effect = transform.Find("changeStage").gameObject;
        transform.Find("changeStage").gameObject.SetActive(true);

        transform.GetComponentInChildren<SetAnotherSkeletonMat>().setMat(true);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
