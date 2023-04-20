using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_LaserMove_State : EnemyFSMBaseState
{
     
    public Vector2 moveDir,movePos,faceDir ;
    public float maxDis,moveSpeed;
    Transform parent;
    Vector2 startPos;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        parent = enemyFSM.transform.parent;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.transform.up = faceDir;
        enemyFSM.transform.position = (Vector2)parent.transform.position + movePos;
        startPos= enemyFSM.transform.position;
        enemyFSM.rigidbody2d.velocity = moveDir * moveSpeed;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
    }
}
