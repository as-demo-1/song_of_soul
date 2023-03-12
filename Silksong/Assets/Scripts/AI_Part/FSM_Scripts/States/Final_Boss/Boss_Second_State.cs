using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Second_State : EnemyFSMBaseState
{
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {

    }

    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        fsmManager.GetComponent<BossController>().ToSecond();
        fSM_Manager.GetComponent<BossController>().WordAttack();
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;
    }

    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        base.ExitState(fSM_Manager);
        if(fSM_Manager.animator.GetCurrentAnimatorStateInfo(0).normalizedTime>0.99)
        {
            fsmManager.turnFace();
        }
    }

}
