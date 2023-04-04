using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStage : BattleAction
{
    public string effectName;
    public override void OnStart()
    {
        base.OnStart();
        WhaleBossManager.Instance.stage = EBossBattleStage.StageTwo;

        GameObject effect = transform.Find("Effects").Find(effectName).gameObject;
        effect.SetActive(true);
        effect.GetComponent<ParticleSystem>().Play();

        transform.GetComponentInChildren<SetAnotherSkeletonMat>().setMat(true);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
