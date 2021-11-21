using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Enemy_Smash_Down_State : EnemyFSMBaseState
{
    public float jumpHight;
    public string jumpAnimation;
    public string smashDownAnimation;
    public bool isMoveWithCurve;
    public AnimationCurve jumpCurve;

    private Vector3 beginPos, endPos;
    private float hightest;
    private AnimatorStateInfo stateInfo;
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        fsmManager = enemyFSM;
        beginPos = enemyFSM.transform.position;
        endPos = new Vector2(enemyFSM.player.transform.position.x, beginPos.y);
        hightest = beginPos.y + jumpHight;
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
            if (t_tem < 0.99)
            {
               endPos = fsmManager.player.transform.position;
                if (isMoveWithCurve)
                {
                    Vector3 tem = new Vector2(Mathf.Lerp(beginPos.x, endPos.x, t_tem), Mathf.Lerp(beginPos.y, hightest, jumpCurve.Evaluate(t_tem)));
                    fsmManager.transform.Translate(tem - fsmManager.transform.position);
                }
                else 
                {
                    Vector3 tem = new Vector2(Mathf.Lerp(beginPos.x, endPos.x, t_tem), Mathf.Lerp(beginPos.y, hightest, t_tem));
                    fsmManager.transform.Translate(tem - fsmManager.transform.position);
                }
            }
            else
            {
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

