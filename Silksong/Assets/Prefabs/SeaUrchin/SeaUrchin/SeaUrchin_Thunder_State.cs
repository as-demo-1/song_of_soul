using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 然后大海胆在二阶段放电的时候，每隔x秒回生出锁链攻击
/// 二阶段闪电可以多次电击，频率和锁链伸出频率相同
/// </summary>
public class SeaUrchin_Thunder_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public string prickState;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin=enemyFSM as SeaUrchin;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if (seaUrchin.ifInWater)
        {
            foreach (var prick in seaUrchin.pricks)
            {
                prick.ChangeState(prickState);
            }
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}
