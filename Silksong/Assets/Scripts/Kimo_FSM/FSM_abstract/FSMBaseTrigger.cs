using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;
/// <summary>
/// 状态机中对Trigger的配置。可重写InitTrigger，IsTriggerReach方法。
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
[Serializable]
public class FSMBaseTrigger<T1, T2>
{
    [DisplayOnly]
    public T2 triggerID;
    public T1 targetState;
    public bool haveExitTime;
    [Range(0, 1)]
    public float exitTime;
    public FSMBaseTrigger()
    {
    }
    public FSMBaseTrigger(T1 targetState)
    {
        this.targetState = targetState;
    }
    public FSMBaseTrigger(T1 targetState, bool haveExitTime, float exitTime)
    {
        this.targetState = targetState;
        this.haveExitTime = haveExitTime;
        this.exitTime = exitTime;
    }

    public virtual void InitTrigger(FSMManager<T1,T2> fSMManager) { }
    /// <summary>
    /// 是否达到该条件的判断方法
    /// </summary>
    /// <param name="fsm_Manager">管理相应状态类的fsm_manager</param>
    /// <returns></returns>
    public virtual bool IsTriggerReach(FSMManager<T1, T2> fsm_Manager) => false;

    public bool IsExitTimeReached(FSMManager<T1, T2> fsm_Manager)
    {
        return haveExitTime && fsm_Manager.currentStateInfo.normalizedTime / 1 >= exitTime && fsm_Manager.currentStateInfo.shortNameHash == Animator.StringToHash(fsm_Manager.currentState.animName);
    }

}
public class EnemyFSMBaseTrigger : FSMBaseTrigger<EnemyStates, EnemyTriggers> 
{
    public EnemyFSMBaseTrigger(EnemyStates targetState):base(targetState){ }
    public EnemyFSMBaseTrigger(){ }
}
public class NPCFSMBaseTrigger : FSMBaseTrigger<NPCStates, NPCTriggers>
{
    public NPCFSMBaseTrigger(NPCStates targetState) : base(targetState) { }
    public NPCFSMBaseTrigger() { }
}
public class PlayerFSMBaseTrigger : FSMBaseTrigger<PlayerStates, PlayerTriggers>
{
    public PlayerFSMBaseTrigger(PlayerStates targetState) : base(targetState) { }
    public PlayerFSMBaseTrigger() { }
}
