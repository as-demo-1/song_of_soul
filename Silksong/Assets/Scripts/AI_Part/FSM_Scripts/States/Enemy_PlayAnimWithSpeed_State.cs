using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在攻击中位移到指定位置的行为，只建议用来执行单一轴向位移
/// </summary>
public class Enemy_PlayAnimWithSpeed_State : EnemyFSMBaseState
{
    public Vector2 moveSpeed;
    public float maxMoveDis;
    public float startWaitTime;
    private float t;

    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
    }

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        var dir = enemyFSM.getTargetDir(true);
        enemyFSM.animator.Play(defaultAnimationName);
        enemyFSM.StartCoroutine(WaitTime(enemyFSM,startWaitTime));
        enemyFSM.rigidbody2d.velocity = moveSpeed;

        t = maxMoveDis > dir.magnitude ? maxMoveDis : dir.magnitude / moveSpeed.magnitude;
    }

    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        t -= Time.deltaTime;
        if (t < 0)
        {
            enemyFSM.rigidbody2d.velocity = Vector2.zero;
        }
    }

    private IEnumerator WaitTime(EnemyFSMManager fsm,float time)
    {
        var tmp = fsm.animator.speed;
        fsm.animator.speed = 0;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        fsm.animator.speed = tmp;
    }
}