using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ReturnBack_State : EnemyFSMBaseState
{
    public Vector2 basePos,baseDir;
    public float moveSpeed, rotateSpeed;
    public bool ifLocal;
    Vector2 dir;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        if(ifLocal)
            dir = basePos - (Vector2)enemyFSM.transform.localPosition;
        else 
            dir = basePos - (Vector2)enemyFSM.transform.position;
        enemyFSM.rigidbody2d.velocity = dir.normalized*moveSpeed;
    }//应该也积累了一些可以聊的东西了
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {

        base.FixAct_State(enemyFSM);
        if (ifLocal)
            dir = basePos - (Vector2)enemyFSM.transform.localPosition;
        else
            dir = basePos - (Vector2)enemyFSM.transform.position;
        //
        enemyFSM.rigidbody2d.velocity = dir.normalized * moveSpeed;
        if (Vector2.Angle(baseDir, enemyFSM.transform.up) > 1)
        {
            enemyFSM.transform.RotateAround(enemyFSM.transform.position,Vector3.forward, rotateSpeed * Time.fixedDeltaTime);
        }
    }
}
