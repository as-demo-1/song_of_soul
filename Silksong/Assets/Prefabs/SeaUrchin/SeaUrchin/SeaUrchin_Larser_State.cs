using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_Larser_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public string prickState;
    public float interval=5;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
    } 
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if(seaUrchin.ifInWater){
        foreach (var prick in seaUrchin.pricks)
        {
            prick.ChangeState(prickState);
        }
        }else{
            enemyFSM.StartCoroutine(WaitTimeShoot());
        }
    } 
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
    IEnumerator WaitTimeShoot(){
        foreach (var prick in seaUrchin.pricks)
        {
            prick.ChangeState(prickState);
            yield return new WaitForSeconds(interval);
        } 
    }
}
