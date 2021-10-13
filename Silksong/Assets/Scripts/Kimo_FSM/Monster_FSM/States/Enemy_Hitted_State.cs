using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Hitted_State :EnemyFSMBaseState
{
    private  Vector2 hittedBack;
    private AnimatorStateInfo info;
    private Vector3 currPos;
    public float HittedBackDistance;
    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        EventsManager.Instance.AddListener(fSMManager.gameObject, EventType.onTakeDamage, UpdateHittedBack);
    }
    private void UpdateHittedBack()
    {
        Vector2 dir = ((Vector2)(fsmManager.transform.position) - fsmManager.damageable.damageDirection).normalized;
        hittedBack = dir * HittedBackDistance;
        currPos = fsmManager.transform.position;

    }
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        if(fsmManager.animator.HasState(0,Animator.StringToHash("Enermy_Hitted")))
        {
            fSM_Manager.animator.Play("Enermy_Hitted");
            info = fSM_Manager.animator.GetCurrentAnimatorStateInfo(0);
        }
        else
        {
            Debug.Log("no hit animation");
        }
        Debug.Log(hittedBack);
    }
    public override void Act_State(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {

        base.Act_State(fSM_Manager);  
        fSM_Manager.transform.position = Vector3.Lerp(currPos, currPos + (Vector3)hittedBack, info.normalizedTime);

    }
}
