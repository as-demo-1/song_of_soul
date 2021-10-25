using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Bump_State : EnemyFSMBaseState//Í»½ø³å×²
{
    public float BumpSpeed;

    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        stateID = EnemyStates.Enemy_Bump_State;
    }
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        //Debug.Log("bump");
        Vector2 dir = (fsmManager as EnemyFSMManager).getTargetDir(true);
        float speed = BumpSpeed;
        if (dir.x < 0)
        {
            speed *= -1;
        }
        fsmManager.rigidbody2d.velocity = new Vector2(speed, 0);
        fsmManager.animator.Play("Enemy_Bump",0,0);
    }

    public override void ExitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }
}
