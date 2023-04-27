using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ReturnBack_State : EnemyFSMBaseState
{
    public Vector2 basePos,baseDir;
    public float moveSpeed, rotateSpeed;
    Vector2 dir;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        dir = basePos - (Vector2)enemyFSM.transform.localPosition;
        enemyFSM.rigidbody2d.velocity = dir.normalized*moveSpeed;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        if (Vector2.Angle(baseDir, enemyFSM.transform.up) > 1)
        {
            enemyFSM.transform.RotateAround(enemyFSM.transform.position,Vector3.forward, rotateSpeed * Time.fixedDeltaTime);
        }
    }
}
