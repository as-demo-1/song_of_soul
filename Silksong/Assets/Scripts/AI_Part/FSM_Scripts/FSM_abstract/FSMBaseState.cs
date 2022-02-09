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
    [DisplayOnly]
    public  T1 stateType;
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

    public virtual void FixAct_State(FSMManager<T1, T2> fSM_Manager) { }
    /// <summary>
    /// 遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerState(FSMManager<T1,T2> fsm_Manager)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReach(fsm_Manager))
            {
               // Debug.Log(triggers[i] + "     " + triggers[i].targetState);
                fsm_Manager.ChangeState(triggers[i].targetState);
                break;
            }
        }
    }
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
            if (triggers[i].IsTriggerReach(fsm_Manager.fsmManager))
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
