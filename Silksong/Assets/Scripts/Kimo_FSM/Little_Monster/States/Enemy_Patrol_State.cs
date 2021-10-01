using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 敌人Patrol状态，包含撞墙回头，平台边缘回头两个功能
/// </summary>
public class Enemy_Patrol_State : EnemyFSMBaseState
{
    public bool isBack;
    public Vector3 moveSpeed;
    public  float rayToGroundDistance;

    public override void Act_State(FSMManager<EnemyStates,EnemyTriggers> fSM_Manager)
    {
        fsmManager = fSM_Manager;
        Move();
        UpdateFace();
        if(isBack)
        {
            DetectionPlatformBoundary();
        }
    }
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        fsmManager = fSM_Manager;
        fsmManager.animator.Play("Enemy_Patrol");
        if (isBack)
        {
            var rayHit= Physics2D.Raycast(fsmManager.transform.position + new Vector3((moveSpeed.x > 0 ? 1 : -1), 0, 0) * fsmManager.GetComponent<Collider2D>().bounds.size.x, Vector2.down);
            rayToGroundDistance = rayHit.distance+0.1f;
        }
    }
    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.InitState(fSM_Manager);
        stateID = EnemyStates.Enemy_Patrol_State;

        if (isBack)
        {
            EventsManager.Instance.AddListener(fSM_Manager.gameObject, EventType.onEnemyHitWall, HitWall);
        }
    }
    private void Turn()
    {
        
        moveSpeed.x *= -1;
        
    }
    private void UpdateFace()
    {
        if(moveSpeed.x>0)
            fsmManager.transform.localScale = new Vector3(-1, 1, 1);
        else
            fsmManager.transform.localScale = new Vector3(1, 1, 1);
    }
    private void HitWall()
    {
        Turn();
    }
    private void Move()
    {
        fsmManager.gameObject.transform.position +=  moveSpeed*Time.deltaTime;
    }
    private void DetectionPlatformBoundary()
    {
        var rayHit=Physics2D.Raycast(fsmManager.transform.position +new Vector3((moveSpeed.x>0?1:-1),0,0) * fsmManager.GetComponent<Collider2D>().bounds.size.x, Vector2.down);
        if(rayHit.distance>rayToGroundDistance)
        {
            Turn();
        }
    }



}
