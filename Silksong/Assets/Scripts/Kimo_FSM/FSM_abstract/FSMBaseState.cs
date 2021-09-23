using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 状态机中对状态的抽象,具体用法可参考Enemy状态机的构建模式。
/// </summary>
/// <typeparam name="T1">必须是枚举类型！！且为State枚举。</typeparam>
/// <typeparam name="T2">必须是枚举类型！！且为Trigger枚举。</typeparam>
[Serializable]
public  class FSMBaseState<T1,T2>
{
    protected FSMManager<T1,T2> fsmManager;
    [DisplayOnly]
    public  T1 stateID;
    [NonSerialized]
    public List<FSMBaseTrigger<T1,T2>> triggers = new List<FSMBaseTrigger<T1,T2>>();

    public void ClearTriggers()
    {
        triggers.Clear();
    }
    public FSMBaseState()
    {
        //InitState();
    }

    /// <summary>
    /// 状态初始化
    /// </summary>
    public virtual void InitState(FSMManager<T1,T2> fSMManager) { }



    public void AddTriggers(T2 triggerID,T1 targetState) 
    {
        //Debug.Log(triggerID);

        Type type = Type.GetType(triggerID.ToString());
        if (type == null)
        {
            Debug.LogError(triggerID + "无法添加到" + stateID + "的triggers列表");
            Debug.LogError("未找到对应的Trigger，检查该Trigger的类名是否与枚举名保持一致。");
        }
        else 
        {
            triggers.Add(Activator.CreateInstance(type) as FSMBaseTrigger<T1,T2>);
            triggers[triggers.Count - 1].targetState = targetState;
        }
    }
    public void AddTriggers(FSMBaseTrigger<T1,T2> trigger)
    {
        triggers.Add(trigger);
    }


    /// <summary>
    /// 进入状态时调用
    /// </summary>
    public virtual void EnterState(FSMManager<T1,T2> fSM_Manager) { }

    /// <summary>
    /// 退出状态时调用
    /// </summary>
    public virtual void ExitState(FSMManager<T1,T2> fSM_Manager) { }

    /// <summary>
    /// 状态持续及刷新
    /// </summary>
    public virtual void Act_State(FSMManager<T1,T2> fSM_Manager) { }
    /// <summary>
    /// 遍历Trigger并跳转到满足条件的对应trigger所指向的状态。
    /// </summary>
    public virtual void TriggerState(FSMManager<T1,T2> fsm_Manager)
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].IsTriggerReach(fsm_Manager))
            {
                Debug.Log(fsmManager+"  "+triggers[i]+" "+ triggers[i].GetHashCode());
                fsm_Manager.ChangeState(triggers[i].targetState);
            }
        }
    }
}


public class EnemyFSMBaseState : FSMBaseState<EnemyStates,EnemyTriggers> 
{

}
public class NPCFSMBaseState: FSMBaseState<NPCStates, NPCTriggers> 
{

}
public class PlayerFSMBaseState : FSMBaseState<PlayerStates, PlayerTriggers>
{

}
