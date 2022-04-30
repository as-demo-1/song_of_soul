using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Patrol的竖直版本，和横版没有什么区别，只是方向不一样
 */
public class Enemy_VerticalPatrol_State : EnemyFSMBaseState
{
    //public bool isBack;
    public Vector3 moveSpeed;
    public bool noTurnState;//如有，在turn state中转身

    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        if (fsmManager.currentFacingLeft() ? moveSpeed.y > 0 : moveSpeed.y < 0)//使速度与面朝方向一致
        {
            moveSpeed *= -1;
        }
        fsmManager.rigidbody2d.velocity = moveSpeed;
    }

    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        if (fsmManager.currentFacingLeft() ? moveSpeed.y > 0 : moveSpeed.y < 0)//使速度与面朝方向一致
        {
            moveSpeed *= -1;
        }
        fsmManager.rigidbody2d.velocity = moveSpeed;
    }
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;
        stateType = EnemyStates.Enemy_Patrol_State;
    }

    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        
        fsmManager.rigidbody2d.velocity = Vector2.zero;
        //Debug.Log("patrol exit");
    }

}
