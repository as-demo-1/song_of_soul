using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Attack_State : EnemyFSMBaseState
{
    public string AnimationName;
    

    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        fsmManager.animator.Play(AnimationName);
    }

    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
    }

}
