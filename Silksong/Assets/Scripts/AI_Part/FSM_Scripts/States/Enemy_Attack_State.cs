using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Attack_State : EnemyFSMBaseState
{
    

    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
    }

    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
    }

}
