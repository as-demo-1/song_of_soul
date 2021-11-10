using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Wander_State : EnemyFSMBaseState
{
    public float wanderRange=3;//随机移动范围
    private Vector2 wanderCenter;
    public float forceToCenter = 2;
    public float wanderRadiu = 1;//每次随机移动的距离
    public float wanderJitter = 1;//越大就越不随机移动
    public float maxSpeed;
    private Vector2 targetPos = Vector2.right;
    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        //wanderCenter = fsmManager.transform.position;
        stateType = EnemyStates.Enemy_Wander_State;
    }
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        wanderCenter = fsmManager.transform.position;
    }
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        base.Act_State(fSM_Manager);
        Force(fsmManager);
        if (Vector2.Distance(wanderCenter, fsmManager.transform.position) > wanderRange)
        {
            fsmManager.rigidbody2d.AddForce((wanderCenter - (Vector2)fsmManager.transform.position).normalized * forceToCenter);
        }
        fsmManager.faceWithSpeed();
    }
    public void Force(EnemyFSMManager fsmManager)
    {
        targetPos.x += Random.Range(-1, 2);
        targetPos.y += Random.Range(-1, 2);
        targetPos.Normalize();
        targetPos *= wanderRadiu;

        Vector2 target = fsmManager.rigidbody2d.velocity.normalized * wanderJitter + targetPos;
        Vector2 desiredVelocity = target.normalized * maxSpeed;
        fsmManager.rigidbody2d.AddForce(desiredVelocity - fsmManager.rigidbody2d.velocity);
    }
}
