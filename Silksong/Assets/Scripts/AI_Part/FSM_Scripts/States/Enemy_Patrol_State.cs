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
    public float rayToGroundDistance;

    
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
<<<<<<< HEAD
        if (fsmManager.rigidbody2d.velocity.y < -0.1) //如果被击出平台 正常下落
            return;

=======
        // Move();
        fsmManager.faceWithSpeed();
>>>>>>> feature_Boss_AI
        if (isBack)
        {
            DetectionPlatformBoundary();
        }
    }
 
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        fsmManager.rigidbody2d.velocity = moveSpeed;
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
        Collider2D collider = fsmManager.GetComponent<Collider2D>();
        rayToGroundDistance = collider.bounds.extents.y - collider.offset.y+0.1f;
       // Debug.Log(collider.offset);
        //Debug.Log("gourndDistance "+rayToGroundDistance);
    }

    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }

    private void Turn()
    {
        //Debug.Log("turn");
        moveSpeed.x *= -1;
        fsmManager.rigidbody2d.velocity = moveSpeed;
       
    }
    private void DetectionPlatformBoundary()
    {

        //身前点
        Vector3 frontPoint = fsmManager.transform.position + new Vector3((moveSpeed.x > 0 ? 1 : -1), 0, 0) * (fsmManager.GetComponent<Collider2D>().bounds.size.x*0.5f);
  
        var rayHit=Physics2D.Raycast(frontPoint, Vector2.down,100,1<<LayerMask.NameToLayer("Ground"));
       // Debug.DrawRay(frontPoint,Vector2.down);
        //Debug.Log(rayHit.distance);
        if (rayHit.distance>rayToGroundDistance)//到达边缘
        {
            Turn();
        }
        Vector3 upPoint = fsmManager.transform.position + new Vector3(0, 0.1f, 0);
        if (Physics2D.OverlapArea(upPoint,frontPoint,1<<LayerMask.NameToLayer("Ground"))!=null)
        {
            //Debug.Log("hit wall");
            Turn();
        }
    }



}
