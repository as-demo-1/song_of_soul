using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Bump_State : EnemyFSMBaseState//Í»½ø³å×²
{
    public float BumpSpeed;

    public float accumulateTime;
    public float bumpTime;
    public float brakeTime;
    private float timer;
    private Vector3 tem = new Vector3(1, 1, 1);
    private Vector2 dir;
    public string accumulateAnimation;
    public string bumpAnimation;
    public string brakeAnimation;

    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        stateType = EnemyStates.Enemy_Bump_State;
    }
    private void AnimationPlay(string name)
    {
        if(name!=string.Empty&&fsmManager.animator!=null)
        {
            fsmManager.animator.Play(name);
        }
    }
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        //Debug.Log("bump");
        dir = (fsmManager as EnemyFSMManager).getTargetDir(true);
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        timer += Time.deltaTime;
        if(timer>0&&timer<= accumulateTime&&tem[0] == 1)
        {
            tem[0] = 0;
            AnimationPlay(accumulateAnimation);

        }else if(timer>accumulateTime&&timer<=accumulateTime+bumpTime&&tem[1]==1)
        {
            tem[1] = 0;
            float speed = BumpSpeed;
            if (dir.x < 0)
            {
                speed *= -1;
            }
            fsmManager.rigidbody2d.velocity = new Vector2(speed, 0);
            AnimationPlay(bumpAnimation);
        }
        else if(timer> accumulateTime + bumpTime&&timer<= accumulateTime + bumpTime +brakeTime && tem[2] == 1)
        {
            tem[2] = 0;
            AnimationPlay(brakeAnimation);
        }
    }

    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        timer = 0;
        tem = new Vector3(1, 1, 1);
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }
}
