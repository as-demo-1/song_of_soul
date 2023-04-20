using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class SeaUrchin_Dash_State : EnemyFSMBaseState
{
     
    public string prickIdleState,prickShrinkState;
    public float waitTime;
    public float dashSpeed;
    SeaUrchin seaUrchin;
    float t;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if (enemyFSM.player == null)
            enemyFSM.player = GameObject.FindGameObjectWithTag("Player");
        foreach (var prick in seaUrchin.pricks)
            prick.ChangeState(prickShrinkState);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        t = 0;
        enemyFSM.StartCoroutine(DashMove(enemyFSM));
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
         
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        foreach (var prick in seaUrchin.pricks)
            prick.ChangeState(prickIdleState);
    }
    IEnumerator DashMove(EnemyFSMManager enemyFSM)
    {
        yield return new WaitForSeconds(waitTime);
        enemyFSM.rigidbody2d.velocity = enemyFSM.getTargetDir().normalized*dashSpeed;
    }
}
