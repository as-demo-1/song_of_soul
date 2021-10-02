using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 状态机管理器的基类
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public abstract class FSMManager<T1,T2> : MonoBehaviour
{

    public Animator animator;
    public AnimatorStateInfo currentStateInfo;
    public AudioSource audio;
    public Rigidbody2D rigidbody;

    public Collider2D triggerCollider;
    public Collision2D collision;

    /// /// <summary>
    /// 当前状态
    /// </summary>
    public FSMBaseState<T1,T2> currentState;
    [DisplayOnly]
    public T1 currentStateID;
    /// <summary>
    /// 任意状态
    /// </summary>
    public FSMBaseState<T1,T2> anyState;
    public T1 defaultStateID;
    /// <summary>
    /// 当前状态机包含的所以状态列表
    /// </summary>
    public Dictionary<T1, FSMBaseState<T1,T2>> statesDic = new Dictionary<T1, FSMBaseState<T1,T2>>();
    /// <summary>
    /// 配置状态列表及其对应条件列表的SO文件
    /// </summary>


    public void ChangeState(T1 state)
    {
        if (currentState != null) { }
            currentState.ExitState(this);
        if (statesDic.ContainsKey(state))
        {
            currentState = statesDic[state];
            currentStateID = state;
        }
        else
        {
            Debug.LogError("敌人状态不存在");
        }
        currentState.EnterState(this);
        if (currentState.animName != null)
        {
            animator.Play(currentState.animName);
            currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            //Debug.LogWarning(currentStateInfo.normalizedTime);
            currentStateInfo = default;
            Debug.LogWarning(currentStateInfo.normalizedTime);
        }
    }

    //public FSMBaseState<T1,T2> AddState(T1 state)
    //{
    //    //Debug.Log(triggerID);

    //    Type type = Type.GetType("Enemy"+state + "State");
    //    if (type == null)
    //    {
    //        Debug.LogError(state + "无法添加到" + "的states列表");
    //        Debug.LogError("检查stateID枚举值及对应类名，对应枚举命加上“_State”，如枚举值为Idle，状态类名为Idle_State，便于配置加载；");
    //        return null;
    //    }
    //    else
    //    {
    //        FSMBaseState<T1,T2> temp = Activator.CreateInstance(type) as FSMBaseState<T1,T2>;
    //        statesDic.Add(state,temp);
    //        return temp;
    //    }
    //}
    //public FSMBaseState<T1,T2> AddState(T1 state,FSMBaseState<T1,T2> stateClass)
    //{
    //    statesDic.Add(state, stateClass);
    //    return stateClass;
    //}
    //public void RemoveState(T1 state)
    //{
    //    if (statesDic.ContainsKey(state))
    //        statesDic.Remove(state);
    //}
    /// <summary>
    /// 用于初始化状态机的方法，添加所有状态，及其条件映射表，获取部分组件等。Awake时执行，可不使用基类方法手动编码加载
    /// </summary>
    /// 

    public virtual void InitWithScriptableObject()
    {
    }
    public virtual void InitManager()
    {
        InitWithScriptableObject();
        ////组件获取
        if (GetComponent<Animator>() != null)
        {
            animator = GetComponent<Animator>();
        }
        if (GetComponent<AudioSource>() != null)
        {
            audio = GetComponent<AudioSource>();
        }
        if(GetComponent<Rigidbody2D>()!=null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }

    }

    private void Awake()
    {
        statesDic.Clear();
        InitManager();

    }

    private void Start()
    {
        if (statesDic.Count == 0)
            return;
        //默认状态设置
        currentStateID = defaultStateID;
        ChangeState(currentStateID);
        if (anyState != null)
            anyState.EnterState(this);
        foreach (var state in statesDic.Values)
            foreach (var value in state.triggers)
            {
                Debug.LogWarning(this + "  " + state + "  " + value + "  " + value.GetHashCode());
            }
    }

    private void Update()
    {
        if (anyState != null)
        {
            anyState.Act_State(this);
            anyState.TriggerState(this);
        }
        if (currentState != null)
        {
            //执行状态内容
            currentState.Act_State(this);
            //检测状态条件列表
            currentState.TriggerState(this);
            currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        else
        {
            Debug.LogError("currentState为空");
        }
    }

}

public abstract class FSMManagerInherit<T1, T2, T3, T4> : FSMManager<T1, T2>
{
    public List<State_SO_Config<T1, T2, T3, T4>> stateConfigs;
    public State_SO_Config<T1, T2, T3, T4> anyStateConfig;
    public override void InitWithScriptableObject()
    {
        if (anyStateConfig != null)
        {
            anyState = (FSMBaseState<T1, T2>)ObjectClone.CloneObject(anyStateConfig.stateConfig);
            anyState.triggers = new List<FSMBaseTrigger<T1, T2>>();
            for (int k = 0; k < anyStateConfig.triggerList.Count; k++)
            {
                anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<T1, T2>);
                anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
            }
            anyState.InitState(this);
        }
        for (int i = 0; i < stateConfigs.Count; i++)
        {
            FSMBaseState<T1, T2> tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as FSMBaseState<T1, T2>;
            tem.triggers = new List<FSMBaseTrigger<T1, T2>>();
            for (int k = 0; k < stateConfigs[i].triggerList.Count; k++)
            {
                tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<T1, T2>);
                tem.triggers[tem.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
            }
            statesDic.Add(stateConfigs[i].stateID, tem);
            tem.InitState(this);
        }
    }
}

/// <summary>
///构建Enemy状态机管理器，并为其添加SO配置功能
/// </summary>
public class EnemyFSMManager : FSMManagerInherit<EnemyStates, EnemyTriggers, EnemyFSMBaseState, EnemyFSMBaseTrigger> { }
/// <summary>
///构建NPC状态机管理器，并为其添加SO配置功能
/// </summary>
public class NPCFSMManager : FSMManagerInherit<NPCStates, NPCTriggers, NPCFSMBaseState, NPCFSMBaseTrigger> { }
/// <summary>
/// 构建Player状态机管理器，默认没有添加SO配置功能，
/// 如需要，
/// 首先取消掉下面的注释
/// 然后打开Player_State_SO_Config脚本，取消关于Player_State_SO_Config类的注释即可。
/// 
/// </summary>
public class PlayerFSMManager : FSMManagerInherit<PlayerStates, PlayerTriggers, PlayerFSMBaseState, PlayerFSMBaseTrigger> { }

