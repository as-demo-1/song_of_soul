using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
/// <summary>
/// 状态机中对状态的抽象,具体用法可参考Enemy状态机的构建模式。
/// </summary>
/// <typeparam name="T1">必须是枚举类型！！且为State枚举。</typeparam>
/// <typeparam name="T2">必须是枚举类型！！且为Trigger枚举。</typeparam>
[Serializable]
public  class FSMBaseState<T1,T2> 
{
    //protected FSMManager<T1,T2> fsmManager;
#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public T1 stateType;
    [NonSerialized]
    public List<FSMBaseTrigger<T1,T2>> triggers = new List<FSMBaseTrigger<T1,T2>>();

    /// <summary>
    /// 状态初始化
    /// </summary>
    public virtual void InitState(FSMManager<T1,T2> fSMManager) { }

    /// <summary>
    /// 进入状态时调用
    /// </summary>
    public  virtual void EnterState(FSMManager<T1,T2> fSM_Manager) { }

    /// <summary>
    /// 退出状态时调用
    /// </summary>
    public virtual void ExitState(FSMManager<T1,T2> fSM_Manager) { }

    /// <summary>
    /// 状态持续及刷新
    /// </summary>
    public virtual void Act_State(FSMManager<T1,T2> fSM_Manager) { }
    /// <summary>
    /// Acting in fixUpdate 
    /// </summary>
    public virtual void FixAct_State(FSMManager<T1, T2> fSM_Manager) { }

    #region Colider Event
    /// <summary>
    /// invoke when TriggerEnter2D
    /// </summary>
    /// 
    public virtual void OnTriggerEnter2D(FSMManager<T1, T2> fSM_Manager,Collider2D collision) { }
    public virtual void OnTriggerStay2D(FSMManager<T1, T2> fSM_Manager,Collider2D collision) { }
    public virtual void OnTriggerExit2D(FSMManager<T1, T2> fSM_Manager, Collider2D collision) { }

    /// <summary>
    /// invoke when ColiderEnter2D
    /// </summary>
    public virtual void OnCollisionEnter2D(FSMManager<T1, T2> fSM_Manager, Collision2D collision) { }
    public virtual void OnCollisionExit2D(FSMManager<T1, T2> fSM_Manager, Collision2D collision) { }
    public virtual void OnCollisionStay2D(FSMManager<T1, T2> fSM_Manager, Collision2D collision) { }
    #endregion

    #region TriggerInvoke
    /// <summary>
    /// 在Update中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateInUpdate(FSMManager<T1,T2> fsm_Manager)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachInUpdate(fsm_Manager))
            {
               // Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在FixUpdate中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateInFixUpdate(FSMManager<T1, T2> fsm_Manager)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachInFixUpdate(fsm_Manager))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在OnColliderEnter中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateOnCollisionEnter(FSMManager<T1, T2> fsm_Manager,Collision2D collision)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachOnCollisionEnter(fsm_Manager,collision))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在OnColliderExit中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateOnCollisionExit(FSMManager<T1, T2> fsm_Manager, Collision2D collision)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachOnCollisionExit(fsm_Manager, collision))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在OnColliderStay中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateOnCollisionStay(FSMManager<T1, T2> fsm_Manager, Collision2D collision)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachOnCollisionStay(fsm_Manager, collision))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在OnTriggerEnter中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateOnTriggerEnter(FSMManager<T1, T2> fsm_Manager, Collider2D collision)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachOnTriggerEnter(fsm_Manager, collision))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在OnTriggerExit中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateOnTriggerExit(FSMManager<T1, T2> fsm_Manager, Collider2D collision)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachOnTriggerExit(fsm_Manager, collision))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    /// <summary>
    /// 在OnTriggerStay中遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerStateOnTriggerStay(FSMManager<T1, T2> fsm_Manager, Collider2D collision)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachOnTriggerStay(fsm_Manager, collision))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
    #endregion
}


public class EnemyFSMBaseState : FSMBaseState<EnemyStates,EnemyTriggers> 
{
    [NonSerialized]
    public  EnemyFSMManager fsmManager;
    public string defaultAnimationName;
    [NonSerialized]
    public UnityEvent animationEvents=new UnityEvent();
    //对一些触发函数进行二次封装
    //////////////////////////////////////////////////////////////////////////////////////////
    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSMManager)
    {
        base.InitState(fSMManager);
        InitState(fSMManager as EnemyFSMManager);
    }
    public virtual void InitState(EnemyFSMManager enemyFSM) { }
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.EnterState(fSM_Manager);

        EnterState(fSM_Manager as EnemyFSMManager);

       
    }
    public virtual void EnterState(EnemyFSMManager enemyFSM) {
        enemyFSM.hasInvokedAnimationEvent = false;
        if (enemyFSM.animator != null && defaultAnimationName != string.Empty)
        {   
            enemyFSM.animator.Play(defaultAnimationName, 0);
        }
    }
    public override void Act_State(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.Act_State(fSM_Manager);
        Act_State(fSM_Manager as EnemyFSMManager);
    }
    public virtual void Act_State(EnemyFSMManager enemyFSM) { }

    public override void FixAct_State(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.FixAct_State(fSM_Manager);
        FixAct_State(fSM_Manager as EnemyFSMManager);
    }
    public virtual void FixAct_State(EnemyFSMManager enemyFSM) { }
    #region Colider Events
    public override void OnCollisionStay2D(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager, Collision2D collision)
    {
        base.OnCollisionStay2D(fSM_Manager, collision);
        OnCollisionStay2D(fSM_Manager as EnemyFSMManager, collision);
    }
    public virtual void OnCollisionStay2D(EnemyFSMManager enemyFSM,Collision2D collision) { }

    public override void OnCollisionEnter2D(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager, Collision2D collision)
    {
        base.OnCollisionEnter2D(fSM_Manager, collision);
        OnCollisionEnter2D(fSM_Manager as EnemyFSMManager, collision);
    }
    public virtual void OnCollisionEnter2D(EnemyFSMManager enemyFSM, Collision2D collision) { }

    public override void OnCollisionExit2D(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager, Collision2D collision)
    {
        base.OnCollisionExit2D(fSM_Manager, collision);
        OnCollisionExit2D(fSM_Manager as EnemyFSMManager, collision);
    }
    public virtual void OnCollisionExit2D(EnemyFSMManager enemyFSM, Collision2D collision) { }

    public override void OnTriggerEnter2D(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager, Collider2D collision)
    {
        base.OnTriggerEnter2D(fSM_Manager, collision);
        OnTriggerEnter2D(fSM_Manager as EnemyFSMManager, collision);
    }
    public virtual void OnTriggerEnter2D(EnemyFSMManager enemyFSM, Collider2D collision) { }

    public override void OnTriggerExit2D(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager, Collider2D collision)
    {
        base.OnTriggerExit2D(fSM_Manager, collision);
        OnTriggerExit2D(fSM_Manager as EnemyFSMManager, collision);
    }
    public virtual void OnTriggerExit2D(EnemyFSMManager enemyFSM, Collider2D collision) { }

    public override void OnTriggerStay2D(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager, Collider2D collision)
    {
        base.OnTriggerStay2D(fSM_Manager, collision);
        OnTriggerStay2D(fSM_Manager as EnemyFSMManager, collision);
    }
    public virtual void OnTriggerStay2D(EnemyFSMManager enemyFSM, Collider2D collision) { }
    #endregion
    public override void ExitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {

        base.ExitState(fSM_Manager);
        ExitState(fSM_Manager as EnemyFSMManager);
    }
    public virtual void ExitState(EnemyFSMManager enemyFSM) {  }

    public virtual void TriggerState(EnemySubFSMManager fsm_Manager)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReachInUpdate(fsm_Manager.fsmManager))
            {
                Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }

    public  virtual void invokeAnimationEvent()
    {
        animationEvents.Invoke();
       
    }
    /////////////////////////////////////////////////////////////////////////////////////////
    ///
}
public class NPCFSMBaseState: FSMBaseState<NPCStates, NPCTriggers> 
{

}
public class PlayerFSMBaseState : FSMBaseState<PlayerStates, PlayerTriggers>
{

}
