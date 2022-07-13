using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 这个是先蓄力再冲刺攻击一段距离的行为
/// 没有控制蓄力时间的参数
/// </summary>
public class Boss_Rush_State : EnemyFSMBaseState
{
    public float chargeSpeed;
    public float rushDis;
    public float rushSpeed;
    public string chargeAnimation;
    public string rushAnimation;

    private Vector2 beginPos, endPos, curPos;
    private AnimatorStateInfo stateInfo;

    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        fsmManager = enemyFSM;
    }

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        Debug.Log("IN");
        base.EnterState(enemyFSM);
        fsmManager = enemyFSM;
        beginPos = fsmManager.transform.position;
        endPos = fsmManager.transform.position + fsmManager.transform.right * rushDis;
        if (chargeAnimation != string.Empty)
            enemyFSM.animator.Play(chargeAnimation);
    }

    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        fsmManager = enemyFSM;
        stateInfo = enemyFSM.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(chargeAnimation))
        {
            float tmp = stateInfo.normalizedTime;
            if (tmp > 0.95f)
            {
                if (rushAnimation != string.Empty)
                    fsmManager.animator.Play(rushAnimation);
            }
        }
        else
        {
            curPos = fsmManager.transform.position;
            if(curPos != endPos)
            {
                fsmManager.transform.position = Vector2.Lerp(curPos, endPos, rushSpeed);
            }
        }
    }

    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }

}