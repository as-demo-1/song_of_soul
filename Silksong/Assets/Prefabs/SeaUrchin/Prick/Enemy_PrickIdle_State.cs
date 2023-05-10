using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy_PrickIdle_State : EnemyFSMBaseState
{
    Transform seaUrchin;
    public float maxMoveRange = 1;
    public float maxMoveSpeed;
     
     Rigidbody2D rb;
    float t;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        t = 0;
        seaUrchin = enemyFSM.transform.parent;
        //this.dir = (seaUrchin.position - enemyFSM.transform.position).normalized;
        rb = enemyFSM.rigidbody2d;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        t=0;
        enemyFSM.transform.up= (enemyFSM.transform.position-seaUrchin.position  ).normalized;
    }//
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        //enemyFSM.transform.up = (enemyFSM.transform.position - seaUrchin.position).normalized;
        t += Time.fixedDeltaTime;
        rb.velocity = enemyFSM.transform.up * SinMove(t);
    }
    public float SinMove(float t)
    {
        float speed = 0;
        speed = Mathf.Cos(t * maxMoveSpeed) * maxMoveRange;
        return speed;
    }
}//
