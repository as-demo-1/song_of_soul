using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Hitted_State :EnemyFSMBaseState
{
    //private  Vector2 hittedBack;
   // private AnimatorStateInfo info;
   // private Vector3 currPos;
    public float HittedBackDistance;
    private float hittedVelocity;//被击退时的速度
    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        if (fsmManager.animator.HasState(0, Animator.StringToHash("Enemy_Hitted")))
        {
            AnimationClip[] clips = fsmManager.animator.runtimeAnimatorController.animationClips;      
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Equals("Enemy_Hitted"))
                {
                    hittedVelocity = HittedBackDistance/clip.length;
                    break;
                }
            }         
        }
        else
        {
            Debug.LogError("no hit animation");
        }
        //EventsManager.Instance.AddListener(fSMManager.gameObject, EventType.onTakeDamage, UpdateHittedBack);
    }
    /*private void UpdateHittedBack()
    {
        Vector2 dir = ((Vector2)(fsmManager.transform.position) - fsmManager.damageable.damageDirection).normalized;
        hittedBack = dir * HittedBackDistance;
      //  currPos = fsmManager.transform.position;

    }*/
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        //Debug.Log("hitted_state");

            fSM_Manager.animator.Play("Enemy_Hitted",0,0);//如果受击过程中又受击 从头开始播放受击动画
               
            Vector2 dir = new Vector2(1,0);
            if (fsmManager.damageable.damageDirection.x>0)
            {
                dir.x=-1;
            }

            fsmManager.rigidbody2d.velocity = dir * hittedVelocity ;
            //Debug.Log(fsmManager.rigidbody2d.velocity);
        
    }
    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        base.ExitState(fSM_Manager);
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }

   /* public override void Act_State(EnemyFSMManager fSM_Manager)
    {

        // base.Act_State(fSM_Manager);  
        // fSM_Manager.transform.position = Vector3.Lerp(currPos, currPos + (Vector3)hittedBack, info.normalizedTime);
        Debug.Log(fsmManager.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        Debug.Log(Time.frameCount);

    }*/
}
