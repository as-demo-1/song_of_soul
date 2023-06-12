using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 反正dash就是这么个情况了
/// </summary>
public class SeaUrchin_Dash_State : EnemyFSMBaseState
{
    public string prickShrinkState;
    public float waitTime;
    public float dashSpeed;
    public float hookSpeed;
    public LayerMask layer;
    public string targetState;
    public int dashTime=3;
    SeaUrchin seaUrchin;
    int touchTime;
    RaycastHit2D hit;
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
        if (!seaUrchin.ifInWater)
        {
            foreach (var prick in seaUrchin.pricks)
                prick.ChangeState(prickShrinkState);
            enemyFSM.rigidbody2d.velocity = Vector2.zero;
            enemyFSM.StartCoroutine(DashMove(enemyFSM));
            touchTime = 0;
        }
        else
        {
            touchTime = dashTime;
            foreach (var prick in seaUrchin.pricks)
                prick.ChangeState(prickShrinkState);
            //foreach (var hook in seaUrchin.hooks)
            //    hook.SetPosition(1, enemyFSM.transform.position);
            enemyFSM.rigidbody2d.velocity = Vector2.zero;
            enemyFSM.StartCoroutine(DashMove_Hook(enemyFSM));
            
        }
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
         
    }//
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        if (enemyFSM.rigidbody2d.IsTouchingLayers(layer)&&Vector2.Distance(hit.point,enemyFSM.transform.position)<=1)
        {
            if (seaUrchin.ifInWater)
            {
                seaUrchin.hooks[touchTime].Catch();
            }
            enemyFSM.rigidbody2d.velocity = Vector2.zero;
            foreach (var prick in seaUrchin.pricks)
                prick.rigidbody2d.velocity = enemyFSM.rigidbody2d.velocity;
            touchTime--;
            if (touchTime < 0)
            {
                enemyFSM.ChangeState(targetState);
            }
            else
            {
                enemyFSM.StartCoroutine(DashMove_Hook(enemyFSM));
            }
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }
    IEnumerator DashMove(EnemyFSMManager enemyFSM)
    {
        yield return new WaitForSeconds(waitTime);
        enemyFSM.rigidbody2d.velocity = enemyFSM.getTargetDir().normalized*dashSpeed;
        hit = Physics2D.Raycast(enemyFSM.transform.position, enemyFSM.getTargetDir(), 100, layer);
        foreach (var prick in seaUrchin.pricks)
            prick.rigidbody2d.velocity = enemyFSM.rigidbody2d.velocity;
    }
    IEnumerator DashMove_Hook(EnemyFSMManager enemyFSM)
    {
        hit = Physics2D.Raycast(enemyFSM.transform.position, enemyFSM.getTargetDir(), 100, layer);
        Debug.Log("?");
        seaUrchin.hooks[touchTime].Shoot(hit.point);
        while (seaUrchin.hooks[touchTime].grappleRope.waveSize>0)
        {
            yield return null;
        }
        enemyFSM.rigidbody2d.velocity = (hit.point-(Vector2)enemyFSM.transform.position).normalized * dashSpeed;
        foreach (var prick in seaUrchin.pricks)
            prick.rigidbody2d.velocity = enemyFSM.rigidbody2d.velocity;
        yield return new WaitForSeconds(0.1f);

    }
}
