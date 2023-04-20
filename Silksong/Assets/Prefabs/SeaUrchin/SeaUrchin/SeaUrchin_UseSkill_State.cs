using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_UseSkill_State : EnemyFSMBaseState
{
    SeaUrchin SeaUrchin;
    public List<string> targetState;
    public float waitTime;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        SeaUrchin = enemyFSM as SeaUrchin;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.StartCoroutine(UseSkill(enemyFSM));
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }//
    IEnumerator UseSkill(EnemyFSMManager enemyFSM)
    {
        yield return new WaitForSeconds(waitTime);
        int index= Random.Range(0, targetState.Count);
        enemyFSM.ChangeState(targetState[index]);
    }
}
