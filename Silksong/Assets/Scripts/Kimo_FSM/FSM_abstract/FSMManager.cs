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
    public AudioSource audios;
    public Rigidbody2D rigidbody2d;

   // public Collider2D triggerCollider;
    public Collision2D collision;

    public DamageableBase damageable;

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
        //Debug.Log(state);
        if (currentState != null)
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
    }

    public FSMBaseState<T1,T2> AddState(T1 state)
    {
        //Debug.Log(triggerID);

        Type type = Type.GetType("Enemy"+state + "State");
        if (type == null)
        {
            Debug.LogError(state + "无法添加到" + "的states列表");
            Debug.LogError("检查stateID枚举值及对应类名，对应枚举命加上“_State”，如枚举值为Idle，状态类名为Idle_State，便于配置加载；");
            return null;
        }
        else
        {
            FSMBaseState<T1,T2> temp = Activator.CreateInstance(type) as FSMBaseState<T1,T2>;
            statesDic.Add(state,temp);
            return temp;
        }
    }
    public FSMBaseState<T1,T2> AddState(T1 state,FSMBaseState<T1,T2> stateClass)
    {
        statesDic.Add(state, stateClass);
        return stateClass;
    }
    public void RemoveState(T1 state)
    {
        if (statesDic.ContainsKey(state))
            statesDic.Remove(state);
    }
    /// <summary>
    /// 用于初始化状态机的方法，添加所有状态，及其条件映射表，获取部分组件等。Awake时执行，可不使用基类方法手动编码加载
    /// </summary>
    /// 

    public virtual void InitWithScriptableObject()
    {
    }
    public virtual void InitManager()
    {
        if (GetComponent<Animator>() != null)
        {
            animator = GetComponent<Animator>();
        }
        if (GetComponent<AudioSource>() != null)
        {
            audios = GetComponent<AudioSource>();
        }
        if (GetComponent<Rigidbody2D>() != null)
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
        }

        InitWithScriptableObject();
        ////组件获取
    }

    protected void Awake()
    {
        statesDic.Clear();
        InitManager();
        damageable = GetComponent<DamageableBase>();
    }

    protected void Start()
    {
        if (statesDic.Count == 0)
            return;
        //默认状态设置
        currentStateID = defaultStateID;
        ChangeState(currentStateID);
        if (anyState != null)
            anyState.EnterState(this);

        //// Debug code
        //foreach (var state in statesDic.Values)
        //    foreach (var value in state.triggers)
        //    {
        //        Debug.LogWarning(this + "  " + state + "  " + value + "  " + value.GetHashCode());
        //    }

    }

    private void Update()
    {

        if (currentState != null)
        {
            //执行状态内容
            currentState.Act_State(this);
            //检测状态条件列表
            currentState.TriggerState(this);
        }
        else
        {
            Debug.LogError("currentState为空");
        }

        if (anyState != null)
        {
            anyState.Act_State(this);
            anyState.TriggerState(this);
        }
    }

}

/// <summary>
///构建Enemy状态机管理器，并为其添加SO配置功能
/// </summary>
public class EnemyFSMManager : FSMManager<EnemyStates, EnemyTriggers> 
{
    public List<Enemy_State_SO_Config> stateConfigs;
    public Enemy_State_SO_Config anyStateConfig;
    public override void InitWithScriptableObject()
    {
        if(anyStateConfig!=null)
        {

            anyState = (FSMBaseState<EnemyStates, EnemyTriggers>)ObjectClone.CloneObject(anyStateConfig.stateConfig);
            anyState.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            for (int k=0;k<anyStateConfig.triggerList.Count; k++)
            {
                anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<EnemyStates, EnemyTriggers>);
                anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
            }
            anyState.InitState(this);
        }
        for (int i = 0; i < stateConfigs.Count; i++)
        {
            FSMBaseState<EnemyStates, EnemyTriggers> tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as FSMBaseState<EnemyStates, EnemyTriggers>;
            tem.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            for (int k=0;k< stateConfigs[i].triggerList.Count;k++)
            {
                tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<EnemyStates, EnemyTriggers>);
                tem.triggers[tem.triggers.Count-1].InitTrigger(this);
                //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
            }
            statesDic.Add(stateConfigs[i].stateID, tem);
            tem.InitState(this);
        }
    }
}
/// <summary>
///构建NPC状态机管理器，并为其添加SO配置功能
/// </summary>
public class NPCFSMManager : FSMManager<NPCStates, NPCTriggers>
{
    public List<NPC_State_SO_Config> stateConfigs;
    public Enemy_State_SO_Config anyStateConfig;
    public override void InitWithScriptableObject()
    {
        if (anyStateConfig != null)
        {
            anyState = (FSMBaseState<NPCStates, NPCTriggers>)ObjectClone.CloneObject(anyStateConfig.stateConfig);
            anyState.triggers = new List<FSMBaseTrigger<NPCStates, NPCTriggers>>();
            for (int k = 0; k < anyStateConfig.triggerList.Count; k++)
            {
                anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<NPCStates, NPCTriggers>);
                anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
            }
            anyState.InitState(this);
        }
        for (int i = 0; i < stateConfigs.Count; i++)
        {
            FSMBaseState<NPCStates, NPCTriggers> tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as FSMBaseState<NPCStates, NPCTriggers>;
            tem.triggers = new List<FSMBaseTrigger<NPCStates, NPCTriggers>>();
            for (int k = 0; k < stateConfigs[i].triggerList.Count; k++)
            {
                tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<NPCStates, NPCTriggers>);
                tem.triggers[tem.triggers.Count - 1].InitTrigger(this);
                //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
            }
            statesDic.Add(stateConfigs[i].stateID, tem);
            tem.InitState(this);
        }
    }
}
/// <summary>
/// 构建Player状态机管理器，默认没有添加SO配置功能，
/// 如需要，
/// 首先取消掉下面的注释
/// 然后打开Player_State_SO_Config脚本，取消关于Player_State_SO_Config类的注释即可。
/// 
/// </summary>
public class PlayerFSMManager : FSMManager<PlayerStates, PlayerTriggers> 
{
    //public List<Player_State_SO_Config> stateConfigs;
    //public Player_State_SO_Config anyStateConfig;
    //public override void InitWithScriptableObject()
    //{
    //    if (anyStateConfig != null)
    //    {
    //        anyState = (FSMBaseState<PlayerStates, PlayerTriggers>)ObjectClone.CloneObject(anyStateConfig.stateConfig);
    //        anyState.triggers = new List<FSMBaseTrigger<PlayerStates, PlayerTriggers>>();
    //        for (int k = 0; k < anyStateConfig.triggerList.Count; k++)
    //        {
    //            anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<PlayerStates, PlayerTriggers>);
    //            anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this);
    //            //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
    //        }
    //        anyState.InitState(this);
    //    }
    //    for (int i = 0; i < stateConfigs.Count; i++)
    //    {
    //        FSMBaseState<PlayerStates, PlayerTriggers> tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as FSMBaseState<PlayerStates, PlayerTriggers>;
    //        tem.triggers = new List<FSMBaseTrigger<PlayerStates, PlayerTriggers>>();
    //        for (int k = 0; k < stateConfigs[i].triggerList.Count; k++)
    //        {
    //            tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<PlayerStates, PlayerTriggers>);
    //            tem.triggers[tem.triggers.Count - 1].InitTrigger(this);
    //            //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
    //        }
    //        statesDic.Add(stateConfigs[i].stateID, tem);
    //        tem.InitState(this);
    //    }
    //}
}

