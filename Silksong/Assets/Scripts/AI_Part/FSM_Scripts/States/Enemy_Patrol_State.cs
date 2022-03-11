using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 敌人Patrol状态，包含撞墙回头，平台边缘回头两个功能
/// </summary>适用于地面怪物 
public class Enemy_Patrol_State : EnemyFSMBaseState
{
    //public bool isBack;
    public Vector3 moveSpeed;
    public bool noTurnState;//如有，在turn state中转身

    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        if (fsmManager.rigidbody2d.velocity.y < -0.1) //如果被击出平台 正常下落
            return;

       if (noTurnState)
        {
            DetectionPlatformBoundary();
        }
    }
 
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        Turn();
        base.EnterState(fSM_Manager);
        if(fsmManager.currentFacingLeft()? moveSpeed.x>0 : moveSpeed.x<0)//使速度与面朝方向一致
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

    private void Turn()
    {
       // Debug.Log("turn");
        moveSpeed.x *= -1;
        fsmManager.rigidbody2d.velocity = moveSpeed;

        fsmManager.faceWithSpeed();

    }
    private void DetectionPlatformBoundary()
    {
        if (fsmManager.nearPlatformBoundary(fsmManager.rigidbody2d.velocity) || fsmManager.hitWall())
        { 
             Turn();
        }
    }



}
