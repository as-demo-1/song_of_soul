using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Vomit : AnimationOverAction
{
    private int moveDir;
    public float moveSpeed;
    public override void OnStart()
    {
        base.OnStart();
        animator.SetBool("vomit",true);
        moveDir = 0;

    }
    public override void OnEnd()
    {
        base.OnEnd();
        WhaleBossManager.Instance.resetBorderSkillCdTimer();
        animator.SetBool("vomiting", false);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (WhaleBossManager.Instance.stage == EBossBattleStage.StageTwo && animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            Vector2 newPos = transform.position;
            newPos.y += moveDir * moveSpeed * Time.fixedDeltaTime;
            transform.position = newPos;
        }
    }
    public override TaskStatus OnUpdate()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("attack2"))
        {
            animator.SetBool("vomit", false);
            animator.SetBool("vomiting",true);
        }

        if (moveDir==0 && WhaleBossManager.Instance.stage == EBossBattleStage.StageTwo && animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))//when player y higher by 1,is the mid pos
        {
            float y = target.Value.transform.position.y - 1;
            float selfy = transform.position.y;
            moveDir = y > selfy ? 1 : -1;
            Debug.Log(moveDir);
        }

        return base.OnUpdate();
    }
}
