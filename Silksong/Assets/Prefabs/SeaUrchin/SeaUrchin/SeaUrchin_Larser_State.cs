using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_Larser_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public string prickState;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        foreach (var prick in seaUrchin.pricks)
        {
            prick.ChangeState(prickState);
        }
    } 
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}
