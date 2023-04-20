using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_Shoot_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public float interval; 
    public string prickState,prickBackState;
    public string nextState;
    int index;
    float t;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
        index = 0;
        t = 0;
    }//
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if (index < seaUrchin.pricks.Count)
        {
            t+=Time.deltaTime;
            if (t > interval)
            {
                seaUrchin.pricks[index].ChangeState(prickState);
                t = 0;
                index++;
            }
        }
        else if(index == seaUrchin.pricks.Count)
        {
            t += Time.deltaTime;
            if (t > 2)
            {
                enemyFSM.ChangeState(nextState);
                index++;
            }
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        foreach(var prick in seaUrchin.pricks)
        {
            prick.ChangeState(prickBackState);
        }
    }
}
