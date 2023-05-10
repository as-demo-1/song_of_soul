using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_UseSkill_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public List<string> targetState;
    public float waitTime;
    public string prickIdle;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        Debug.Log("Use Skill");
        foreach (var prick in seaUrchin.pricks)
            prick.ChangeState(prickIdle);
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
        Debug.Log("ʹ�� " + targetState[index]);
        enemyFSM.ChangeState(targetState[index]);
    }
}
