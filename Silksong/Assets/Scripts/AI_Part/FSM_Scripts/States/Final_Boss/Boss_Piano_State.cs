using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boss_Piano_State : EnemyFSMBaseState
{
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;

    }

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        invokeAnimationEvent();
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
    public override void invokeAnimationEvent()
    {
        fsmManager.GetComponent<BossController>().PianoGenerate();
        base.invokeAnimationEvent();
       // Debug.Log("");
    }

}
