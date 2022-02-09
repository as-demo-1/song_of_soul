using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个是先上飞再下砸的行为
/// 没有控制运动时间的参数，因为是根据动画的时间来算的，要减小运动时间，请调大动画播放速度。
/// </summary>
public class Enemy_Smash_Down_State : EnemyFSMBaseState
{
    public float jumpHight;
    public string jumpAnimation;
    public string smashDownAnimation;
    public bool isMoveWithCurve;
    public AnimationCurve jumpCurve;

    private Vector3 beginPos, endPos;
    private AnimatorStateInfo stateInfo;
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        fsmManager = enemyFSM;
        beginPos = enemyFSM.transform.position;
        endPos = new Vector2(enemyFSM.player.transform.position.x, beginPos.y);
        if (jumpAnimation != string.Empty)
            enemyFSM.animator.Play(jumpAnimation);
    }

    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        fsmManager = enemyFSM;
        stateInfo = enemyFSM.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(jumpAnimation))
        {
            float t_tem = stateInfo.normalizedTime;
            if (t_tem < 0.95)
            {
                endPos = fsmManager.player.transform.position;
                if (isMoveWithCurve)
                {
                    Vector3 tem = new Vector2(Mathf.Lerp(beginPos.x, endPos.x, t_tem), Mathf.Lerp(beginPos.y, beginPos.y+jumpHight, jumpCurve.Evaluate(t_tem)));
                    fsmManager.rigidbody2d.MovePosition(tem);
                    //fsmManager.transform.Translate(tem - fsmManager.transform.position);
                }
                else 
                {
                    Vector3 tem = new Vector2(Mathf.Lerp(beginPos.x, endPos.x, t_tem), Mathf.Lerp(beginPos.y, beginPos.y + jumpHight, t_tem));
                    fsmManager.rigidbody2d.MovePosition(tem);
                    //fsmManager.transform.Translate(tem - fsmManager.transform.position);
                }
            }
            else
            {
                fsmManager.rigidbody2d.gravityScale = 20f;
                if (smashDownAnimation != string.Empty)
                    fsmManager.animator.Play(smashDownAnimation);
            }
        }
        else if (stateInfo.IsName(smashDownAnimation))
        {

        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}

