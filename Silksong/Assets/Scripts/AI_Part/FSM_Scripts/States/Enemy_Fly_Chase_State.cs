using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Fly_Chase_State :EnemyFSMBaseState
{
    public int chaseSpeed;
    public float offsetValue=0;
    private Vector2 middlePos,dir,ver,offsetPos;
    private  GameObject target;


    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        target = enemyFSM.player;
        middlePos = (target.transform.position + enemyFSM.transform.position)/2;
        dir =(target.transform.position - enemyFSM.transform.position).normalized;
        ver = new Vector2(-dir.y / dir.x, 1).normalized;
        offsetPos = middlePos + ver * offsetValue;


    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}
