using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateCombinationNode :EnemyFSMBaseState
{
    public enum StateExecuteWay
    {
        Sequence = 0,
        Parallel = 1,
        Random = 2

    }
    //状态执行方式
    [Tooltip("注意：通过SO文件添加的状态，其自身trigger list将失效，由节点的trigger list接管控制\n" +
        "Sequence:顺序执行，trigger list为上下State切换的条件" +
        "如：第trigger1为state1切换的条件" +
        "也就是说，节点退出的条件为最后一个state所对应的trigger的条件\n" +
        "Parallel：严格并行执行，要保证并行的所有状态不存在动画或位移的冲突。trigger list 为节点退出条件\n" +
        "Random：随机执行。 trigger list为节点退出条件")]
    public StateExecuteWay executeWay;
    //状态SO配置list
    public List<Enemy_State_SO_Config> stateConfigs;

    //状态实例list
    private List<EnemyFSMBaseState> states;

    private int currSequenceIndex;
    private EnemyFSMBaseState currActingState;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        states = new List<EnemyFSMBaseState>();
        InitStateList(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if(executeWay==StateExecuteWay.Sequence)
        {
            SequenceEnter(enemyFSM);
        }else if(executeWay == StateExecuteWay.Parallel)
        {
            foreach(var state in states)
            {
                state.EnterState(enemyFSM);
            }
        }else if(executeWay ==StateExecuteWay.Random)
        {
            RandomEnter(enemyFSM);
        }
    }

    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if(executeWay!=StateExecuteWay.Parallel)
            currActingState.Act_State(enemyFSM);
        else
        {
            foreach (var state in states)
            {
                state.Act_State(enemyFSM);
            }
        }
    }

    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        if(executeWay!=StateExecuteWay.Parallel)
            currActingState.FixAct_State(enemyFSM);
        else
        {
            foreach (var state in states)
            {
                state.FixAct_State(enemyFSM);
            }
        }
    }

    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        if(executeWay!=StateExecuteWay.Parallel)
            currActingState.ExitState(enemyFSM);
        else
        {
            foreach (var state in states)
            {
                state.ExitState(enemyFSM);
            }
        }
    }


    public override void TriggerStateInUpdate(FSMManager<EnemyStates, EnemyTriggers> fsm_Manager)
    {
        if(executeWay !=StateExecuteWay.Sequence)
            base.TriggerStateInUpdate(fsm_Manager);
        else
        {
            if( triggers[currSequenceIndex].IsTriggerReachInUpdate(fsm_Manager))
            {
                if (currSequenceIndex < triggers.Count - 1)
                {
                    currSequenceIndex++;
                    if (currActingState != null)
                        currActingState.ExitState(fsmManager);
                    currActingState = states[currSequenceIndex];
                    currActingState.EnterState(fsmManager);
                }
                else if (currSequenceIndex == triggers.Count - 1)
                {
                    currSequenceIndex = 0;
                    if (currActingState != null)
                        currActingState.ExitState(fsmManager);
                    fsm_Manager.ChangeState(triggers[currSequenceIndex].targetState);
                }
            }
        }
    }
    public override void TriggerState(EnemySubFSMManager fsm_Manager)
    {
        fsmManager = fsm_Manager.fsmManager;
        if (executeWay != StateExecuteWay.Sequence)
            base.TriggerState(fsm_Manager);
        else
        {
            if (triggers[currSequenceIndex].IsTriggerReachInUpdate(fsm_Manager.fsmManager))
            {
                if (currSequenceIndex < triggers.Count - 1)
                {
                    currSequenceIndex++;
                    Debug.Log(currSequenceIndex);
                    if (currActingState != null)
                        currActingState.ExitState(fsmManager);
                    currActingState = states[currSequenceIndex];
                    currActingState.EnterState(fsmManager);
                }
                else if (currSequenceIndex == triggers.Count - 1)
                {
                    fsm_Manager.ChangeState(triggers[currSequenceIndex].targetState);
                    currSequenceIndex = 0;
                    if (currActingState != null)
                        currActingState.ExitState(fsmManager);
                }
            }
        }
    }

    public override void invokeAnimationEvent()
    {
        base.invokeAnimationEvent();
        if (executeWay != StateExecuteWay.Parallel)
            currActingState.invokeAnimationEvent();
        else
        {
            foreach (var state in states)
            {
                state.invokeAnimationEvent();
            }
        }
    }
    //只加载行为，不加载trigger list。
    private void InitStateList(EnemyFSMManager enemyFSM)
    {
        for(int i=0;i<stateConfigs.Count;i++)
        {
            var tem = ObjectClone.CloneObject(stateConfigs[i].stateConfig) as EnemyFSMBaseState;
            tem.triggers = new List<FSMBaseTrigger<EnemyStates, EnemyTriggers>>();
            states.Add(tem);
            tem.InitState(enemyFSM);
        }
        if(executeWay==StateExecuteWay.Sequence&&states.Count>triggers.Count)
        {
            Debug.LogError("Error," + this + "\nstate sequence count error");
        }
    }
    private void SequenceEnter(EnemyFSMManager enemyFSM)
    {
        currSequenceIndex = 0;
        currActingState = states[currSequenceIndex];
        currActingState.EnterState(enemyFSM);
        Debug.Log("Sequence Enter");
    }
    private void RandomEnter(EnemyFSMManager enemyFSM)
    {
        int ran = Random.Range(0, states.Count);
        currActingState = states[ran];
        currActingState.EnterState(enemyFSM);
    }
    
}
