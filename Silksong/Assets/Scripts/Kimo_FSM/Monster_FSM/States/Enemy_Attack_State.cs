using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Attack_State : EnemyFSMBaseState
{
    public string AnimationName;
    

    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        fsmManager.animator.Play(AnimationName,0,0);
    }

    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
    }

}
