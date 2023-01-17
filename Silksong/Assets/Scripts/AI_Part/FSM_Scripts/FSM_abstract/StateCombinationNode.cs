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
    //״ִ̬�з�ʽ
    [Tooltip("ע�⣺ͨ��SO�ļ����ӵ�״̬��������trigger list��ʧЧ���ɽڵ��trigger list�ӹܿ���\n" +
        "Sequence:˳��ִ�У�trigger listΪ����State�л�������" +
        "�磺��trigger1Ϊstate1�л�������" +
        "Ҳ����˵���ڵ��˳�������Ϊ���һ��state����Ӧ��trigger������\n" +
        "Parallel���ϸ���ִ�У�Ҫ��֤���е�����״̬�����ڶ�����λ�Ƶĳ�ͻ��trigger list Ϊ�ڵ��˳�����\n" +
        "Random�����ִ�С� trigger listΪ�ڵ��˳�����")]
    public StateExecuteWay executeWay;
    //״̬SO����list
    public List<Enemy_State_SO_Config> stateConfigs;

    //״̬ʵ��list
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

    public override void FixAct_State_next(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State_next(enemyFSM);
        if(executeWay!=StateExecuteWay.Parallel)
            currActingState.FixAct_State_next(enemyFSM);
        else
        {
            foreach (var state in states)
            {
                state.FixAct_State_next(enemyFSM);
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
    //ֻ������Ϊ��������trigger list��
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
