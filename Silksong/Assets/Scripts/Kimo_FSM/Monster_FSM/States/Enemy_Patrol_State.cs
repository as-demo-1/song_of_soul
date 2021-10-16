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
    private float rayToGroundDistance;

    public override void Act_State(FSMManager<EnemyStates,EnemyTriggers> fSM_Manager)
    {
        // Move();
        if (isBack)
        {
            DetectionPlatformBoundary();
        }
    }
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        fsmManager.animator.Play("Enemy_Patrol",0);
        fsmManager.rigidbody2d.velocity = moveSpeed;
        UpdateFace();
        /*  if (isBack)
          {
              var rayHit= Physics2D.Raycast(fsmManager.transform.position + new Vector3((moveSpeed.x > 0 ? 1 : -1), 0, 0) * collider.bounds.size.x, Vector2.down);
              rayToGroundDistance = rayHit.distance+0.1f;
          }*/
    }
    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;
        stateID = EnemyStates.Enemy_Patrol_State;
        Collider2D collider = fsmManager.GetComponent<Collider2D>();
        rayToGroundDistance = collider.bounds.size.y * 0.5f + 0.1f;//暂未考虑collider的offset 
    }
    private void Turn()
    {
        //Debug.Log("turn");
        moveSpeed.x *= -1;
        fsmManager.rigidbody2d.velocity = moveSpeed;
        UpdateFace();
    }
    private void UpdateFace()
    {
        if(moveSpeed.x>0)
            fsmManager.transform.localScale = new Vector3(-1, 1, 1);
        else
            fsmManager.transform.localScale = new Vector3(1, 1, 1);
    }
    /* private void Move()
      {
          Debug.Log(fsmManager.gameObject.transform.position);
          Debug.Log(fsmManager.gameObject.transform.position + moveSpeed * Time.deltaTime);
          fsmManager.gameObject.transform.position +=  moveSpeed*Time.deltaTime;
         // fsmManager.rigidbody2d.MovePosition(fsmManager.gameObject.transform.position+ moveSpeed * Time.deltaTime);
      }*/
    public override void ExitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }

    private void DetectionPlatformBoundary()
    {
        //身前点
        Vector3 frontPoint = fsmManager.transform.position + new Vector3((moveSpeed.x > 0 ? 1 : -1), 0, 0) * (fsmManager.GetComponent<Collider2D>().bounds.size.x*0.5f);
  
        var rayHit=Physics2D.Raycast(frontPoint, Vector2.down);
        if(rayHit.distance>rayToGroundDistance)//到达边缘
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
