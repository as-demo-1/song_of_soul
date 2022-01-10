using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 敌人Patrol状态，包含撞墙回头，平台边缘回头两个功能
/// </summary>适用于地面怪物 
public class Enemy_Patrol_State : EnemyFSMBaseState
{
    public bool isBack;
    public Vector3 moveSpeed;
    public bool isFaceWithSpeed;

    
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        if (fsmManager.rigidbody2d.velocity.y < -0.1) //如果被击出平台 正常下落
            return;

        if (isBack)
        {
            DetectionPlatformBoundary();
        }
    }
 
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        fsmManager.rigidbody2d.velocity = moveSpeed;

        if(isFaceWithSpeed)
        fSM_Manager.faceWithSpeed();
        /*  if (isBack)
          {
              var rayHit= Physics2D.Raycast(fsmManager.transform.position + new Vector3((moveSpeed.x > 0 ? 1 : -1), 0, 0) * collider.bounds.size.x, Vector2.down);
              rayToGroundDistance = rayHit.distance+0.1f;
          }*/
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
    }

    private void Turn()
    {
       // Debug.Log("turn");
        moveSpeed.x *= -1;
        fsmManager.rigidbody2d.velocity = moveSpeed;

        if(isFaceWithSpeed)
        fsmManager.faceWithSpeed();
    }
    private void DetectionPlatformBoundary()
    {
        if(fsmManager.nearPlatformBoundary())
             Turn();

        Vector3 frontPoint = fsmManager.transform.position + new Vector3((moveSpeed.x > 0 ? 1 : -1), 0, 0) * (fsmManager.GetComponent<Collider2D>().bounds.size.x * 0.5f);
        Vector3 upPoint = fsmManager.transform.position + new Vector3(0, 0.1f, 0);
        if (Physics2D.OverlapArea(upPoint,frontPoint,1<<LayerMask.NameToLayer("Ground"))!=null)
        {
            //Debug.Log("hit wall");
            //if(!isChaseNotPatrol)
            Turn();
        }
    }



}
