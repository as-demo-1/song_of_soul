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
        EventsManager.Instance.AddListener(fSMManager.gameObject, EventType.onTakeDamager, UpdateHittedBack);
    }
    private void UpdateHittedBack()
    {
        Vector2 dir = (fsmManager.transform.position - fsmManager.triggerCollider.transform.position).normalized;
        hittedBack = dir * HittedBackDistance;
        currPos = fsmManager.transform.position;

    }
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        fSM_Manager.animator.Play("Enemy_Hitted");
        
        Debug.Log(hittedBack);
    }
    public override void Act_State(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {

        base.Act_State(fSM_Manager);
        info = fSM_Manager.animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("Enemy_Hitted"))
            return;
        fSM_Manager.transform.position = Vector3.Lerp(currPos, currPos + (Vector3)hittedBack, info.normalizedTime);

    }
    public override void TriggerState(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        if (!info.IsName("Enemy_Hitted"))
            return;
        base.TriggerState(fsm_Manager);
    }
}
