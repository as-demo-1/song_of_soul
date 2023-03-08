using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boss_ViolinEnd_State : EnemyFSMBaseState
{

    private Vector2 dir;
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;

    }

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        dir = enemyFSM.getTargetDir(true).normalized;
        enemyFSM.rigidbody2d.DORotate(Mathf.Asin(dir.y), 0.5f);
        invokeAnimationEvent();
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.DORotate(-Mathf.Asin(dir.y), 1f);
        enemyFSM.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
    public override void invokeAnimationEvent()
    {
        base.invokeAnimationEvent();
       // Debug.Log("");
    }

}
