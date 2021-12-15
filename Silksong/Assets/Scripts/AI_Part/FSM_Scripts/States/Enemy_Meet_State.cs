using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Meet_State :EnemyFSMBaseState
{
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {

    }

    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
    }
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;
    }

    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        base.ExitState(fSM_Manager);
    }

}
