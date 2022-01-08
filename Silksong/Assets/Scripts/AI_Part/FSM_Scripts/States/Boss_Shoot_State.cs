using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Boss_Shoot_State :Enemy_Idle_State
{
    public List<string> shootSequence;
    public int index=0;
    private ShootSystem ShotPoint;
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fSM_Manager.getTargetDir(true);
        ShotPoint = fSM_Manager.gameObject.transform.GetChild(1).gameObject.GetComponent<ShootSystem>();
        animationEvents.AddListener(() => { ShotPoint.Shoot(shootSequence[index]); });
    }

    public override void invokeAnimationEvent()
    {
        base.invokeAnimationEvent();
        index++;
        if (index >= shootSequence.Count)
            index = 0;
    }
}
