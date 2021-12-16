using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Enemy_Smash_Down_State : EnemyFSMBaseState
{
    public float jumpHight;
    public float downGravity;
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
            enemyFSM.animator.Play(jumpAnimation,0);
    }

    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        fsmManager = enemyFSM;
        stateInfo = enemyFSM.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(jumpAnimation))
        {
            float t_tem = stateInfo.normalizedTime;
            if (t_tem < 0.99)
            {
                if(t_tem > 0.2 && fsmManager.GetComponent<Collider2D>().IsTouchingLayers(1 << LayerMask.NameToLayer("Ground")))
                {
                    jumpOver();
                }
                //endPos = fsmManager.player.transform.position;
                if (isMoveWithCurve)
                {
                    Vector3 tem = new Vector2(Mathf.Lerp(beginPos.x, endPos.x, t_tem), Mathf.Lerp(beginPos.y, hightest, jumpCurve.Evaluate(t_tem)));
                    // fsmManager.transform.Translate(tem - fsmManager.transform.position);
                    fsmManager.rigidbody2d.MovePosition(tem);
                }
                else
                {
                    Vector3 tem = new Vector2(Mathf.Lerp(beginPos.x, endPos.x, t_tem), Mathf.Lerp(beginPos.y, hightest, t_tem));
                    //fsmManager.transform.Translate(tem - fsmManager.transform.position);
                    fsmManager.rigidbody2d.MovePosition(tem);
                }
            }
            
            else  
            {
                jumpOver();
            }
        }
        else if (stateInfo.IsName(smashDownAnimation))
        {

        }
    }

    private void jumpOver()
    {
        fsmManager.rigidbody2d.gravityScale = downGravity;
        if (smashDownAnimation != string.Empty)
            fsmManager.animator.Play(smashDownAnimation);
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}

