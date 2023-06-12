using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Barb_State : EnemyFSMBaseState
{
    public float Speed;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.transform.up = enemyFSM.transform.parent.position - enemyFSM.transform.position;
        enemyFSM.rigidbody2d.velocity = enemyFSM.transform.up * Speed;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        enemyFSM.transform.up = enemyFSM.transform.parent.position - enemyFSM.transform.position;
        enemyFSM.rigidbody2d.velocity = enemyFSM.transform.up * Speed;
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }
}//
