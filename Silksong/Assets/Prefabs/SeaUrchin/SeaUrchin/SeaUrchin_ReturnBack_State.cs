using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_ReturnBack_State : Enemy_ReturnBack_State
{
    SeaUrchin seaUrchin;
    public string prickState;//
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        foreach (var prick in seaUrchin.pricks)
            prick.rigidbody2d.velocity = enemyFSM.rigidbody2d.velocity;
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        foreach (var prick in seaUrchin.pricks)
            prick.ChangeState(prickState);
    }
}
