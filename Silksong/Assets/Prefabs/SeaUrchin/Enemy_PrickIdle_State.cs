using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy_PrickIdle_State : EnemyFSMBaseState
{
     Transform seaUrchin;
    public Vector2 dir;
    public Vector2 range;//¸¡¶¯·¶Î§
    public float maxMoveRange = 1;
    public float maxMoveSpeed;
    public float RotateSpeed;
     Rigidbody2D rb;
    float t;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        t = 0;
        seaUrchin = enemyFSM.transform.parent;
        this.dir = (seaUrchin.position - enemyFSM.transform.position).normalized;
        rb = enemyFSM.rigidbody2d;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        enemyFSM.transform.up = (enemyFSM.transform.position - seaUrchin.position).normalized;
        t += Time.fixedDeltaTime;
        rb.velocity = enemyFSM.transform.up * SinMove(t);
        enemyFSM.transform.RotateAround(seaUrchin.position, Vector3.forward, RotateSpeed * Time.fixedDeltaTime);
    }
    public float SinMove(float t)
    {
        float speed = 0;
        speed = Mathf.Sin(t * maxMoveSpeed) * maxMoveRange;
        return speed;
    }
}
