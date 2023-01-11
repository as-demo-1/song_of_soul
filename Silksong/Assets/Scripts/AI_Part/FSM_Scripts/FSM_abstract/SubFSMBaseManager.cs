using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemySubFSMManager:EnemyFSMBaseState
{
    /// <summary>
    /// 当前状态
    /// </summary>
    public EnemyFSMBaseState currentState;
#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public string currentStateName;
    /// <summary>
    /// 任意状态
    /// </summary>
    public EnemyFSMBaseState anyState;
    public string defaultStateName;
    /// <summary>
    /// 当前状态机包含的所以状态列表
    /// </summary>
    public Dictionary<string, EnemyFSMBaseState> statesDic = new Dictionary<string, EnemyFSMBaseState>();


    public List<Enemy_State_SO_Config> stateConfigs;
    public Enemy_State_SO_Config anyStateConfig;
    /// <summary>
    /// 用于初始化状态机的方法，添加所有状态，及其条件映射表，获取部分组件等。Awake时执行，可不使用基类方法手动编码加载
    /// </summary>
    /// 
    public void InitWithScriptableObject()
    {
        if (anyStateConfig != null)
        {

            anyState = (EnemyFSMBaseState)ObjectClone.CloneObject(anyStateConfig.stateConfig);
            anyState.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            for (int k = 0; k < anyStateConfig.triggerList.Count; k++)
            {
                anyState.triggers.Add(ObjectClone.CloneObject(anyStateConfig.triggerList[k]) as FSMBaseTrigger<EnemyStates, EnemyTriggers>);
                anyState.triggers[anyState.triggers.Count - 1].InitTrigger(this.fsmManager);
                //Debug.Log(this.gameObject.name+"  "+ anyState.triggers[anyState.triggers.Count - 1]+"  "+anyState.triggers[anyState.triggers.Count - 1].GetHashCode());
            }
            anyState.InitState(this.fsmManager);
        }
        for (int i = 0; i < stateConfigs.Count; i++)
        {
            EnemyFSMBaseState tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as EnemyFSMBaseState;
            tem.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            for (int k = 0; k < stateConfigs[i].triggerList.Count; k++)
            {
                tem.triggers.Add(ObjectClone.CloneObject(stateConfigs[i].triggerList[k]) as FSMBaseTrigger<EnemyStates, EnemyTriggers>);
                tem.triggers[tem.triggers.Count - 1].InitTrigger(this.fsmManager);
                //Debug.Log(this.gameObject.name + "  " + tem.triggers[tem.triggers.Count - 1] + "  " + tem.triggers[tem.triggers.Count - 1].GetHashCode());
            }
            statesDic.Add(stateConfigs[i].name, tem);
            tem.InitState(this.fsmManager);
        }
    }



    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        this.fsmManager = fSMManager;
        statesDic.Clear();
        InitWithScriptableObject();
    }

    public void ChangeState(string state)
    {
        //  Debug.Log(state.ToString()+"  "+gameObject.name);
        if (currentState != null)
            currentState.ExitState(fsmManager);

        if (statesDic.ContainsKey(state))
        {
            currentState = statesDic[state];
            currentStateName = state;
            currentState.EnterState(fsmManager);
        }
        else
        {
            Debug.LogError("敌人状态不存在 " + state);
        }
        
    }
    /// <summary>
    /// 相当于主FSM里面的Start
    /// </summary>
    /// <param name="fSM_Manager"></param>
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        if (statesDic.Count == 0)
            return;
        //默认状态设置
        currentStateName = defaultStateName;
        ChangeState(currentStateName);
        if (anyState != null)
            anyState.EnterState(fSM_Manager);

    }
    /// <summary>
    /// 相当于主FSM里面的Update
    /// </summary>
    /// <param name="fSM_Manager"></param>
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        this.fsmManager = fSM_Manager;
        base.Act_State(fSM_Manager);
        if (currentState != null)
        {
            //执行状态内容
            currentState.Act_State(fSM_Manager);
            //检测状态条件列表
            currentState.TriggerState(this);
        }
        else
        {
            Debug.LogError("currentState为空");
        }

        if (anyState != null)
        {
            anyState.Act_State(fSM_Manager);
            anyState.TriggerState(this);
        }
    }
    /// <summary>
    /// 子状态机的状态退出
    /// </summary>
    /// <param name="fSM_Manager"></param>
    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        base.ExitState(fSM_Manager);
        if(currentState!=null)
            currentState.ExitState(fSM_Manager);
    }

    #region collider event
    public override void OnCollisionEnter2D(EnemyFSMManager enemyFSM, Collision2D collision)
    {
        base.OnCollisionEnter2D(enemyFSM, collision);
        if(currentState!=null)
        {
            currentState.OnCollisionEnter2D(enemyFSM, collision);
            currentState.TriggerStateOnCollisionEnter(enemyFSM, collision);
        }
        else { Debug.LogError("current State is null.."); }
        if (anyState != null)
        {
            // Debug.Log(anyState.triggers.Count);
            anyState.OnCollisionEnter2D(enemyFSM, collision);
            anyState.TriggerStateOnCollisionEnter(enemyFSM, collision);
        }
    }

    public override void OnCollisionExit2D(EnemyFSMManager enemyFSM, Collision2D collision)
    {
        base.OnCollisionExit2D(enemyFSM, collision);
        if (currentState != null)
        {
            currentState.OnCollisionExit2D(enemyFSM, collision);
            currentState.TriggerStateOnCollisionExit(enemyFSM, collision);
        }
        else { Debug.LogError("current State is null.."); }
        if (anyState != null)
        {
            // Debug.Log(anyState.triggers.Count);
            anyState.OnCollisionExit2D(enemyFSM, collision);
            anyState.TriggerStateOnCollisionExit(enemyFSM, collision);
        }
    }

    public override void OnCollisionStay2D(EnemyFSMManager enemyFSM, Collision2D collision)
    {
        base.OnCollisionStay2D(enemyFSM, collision);
        if (currentState != null)
        {
            currentState.OnCollisionStay2D(enemyFSM, collision);
            currentState.TriggerStateOnCollisionStay(enemyFSM, collision);
        }
        else { Debug.LogError("current State is null.."); }
        if (anyState != null)
        {
            // Debug.Log(anyState.triggers.Count);
            anyState.OnCollisionStay2D(enemyFSM, collision);
            anyState.TriggerStateOnCollisionStay(enemyFSM, collision);
        }
    }

    public override void OnTriggerEnter2D(EnemyFSMManager enemyFSM, Collider2D collision)
    {
        base.OnTriggerEnter2D(enemyFSM, collision);
        if (currentState != null)
        {
            currentState.OnTriggerEnter2D(enemyFSM, collision);
            currentState.TriggerStateOnTriggerEnter(enemyFSM, collision);
        }
        else { Debug.LogError("current State is null.."); }
        if (anyState != null)
        {
            // Debug.Log(anyState.triggers.Count);
            anyState.OnTriggerEnter2D(enemyFSM, collision);
            anyState.TriggerStateOnTriggerEnter(enemyFSM, collision);
        }
    }

    public override void OnTriggerExit2D(EnemyFSMManager enemyFSM, Collider2D collision)
    {
        base.OnTriggerExit2D(enemyFSM, collision);
        if (currentState != null)
        {
            currentState.OnTriggerExit2D(enemyFSM, collision);
            currentState.TriggerStateOnTriggerExit(enemyFSM, collision);
        }
        else { Debug.LogError("current State is null.."); }
        if (anyState != null)
        {
            // Debug.Log(anyState.triggers.Count);
            anyState.OnTriggerExit2D(enemyFSM, collision);
            anyState.TriggerStateOnTriggerExit(enemyFSM, collision);
        }
    }

    public override void OnTriggerStay2D(EnemyFSMManager enemyFSM, Collider2D collision)
    {
        base.OnTriggerStay2D(enemyFSM, collision);
        if (currentState != null)
        {
            currentState.OnTriggerStay2D(enemyFSM, collision);
            currentState.TriggerStateOnTriggerStay(enemyFSM, collision);
        }
        else { Debug.LogError("current State is null.."); }
        if (anyState != null)
        {
            // Debug.Log(anyState.triggers.Count);
            anyState.OnTriggerStay2D(enemyFSM, collision);
            anyState.TriggerStateOnTriggerStay(enemyFSM, collision);
        }
    }
    #endregion
    public override void invokeAnimationEvent()
    {
        base.invokeAnimationEvent();
        currentState.invokeAnimationEvent();
    }
}

