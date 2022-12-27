using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public enum EMonsterState
{
    None = 0,
    Attack,
    Hurt,
    Idle,
    Meet,
    Move,
    Turn,
    Die
}
[RequireComponent(typeof(Animator))]
public class MonsterFSM : MonoBehaviour
{
    private Animator _animator;
    private AudioSource _audio;
    private Hittable _hittable;
    public State<EMonsterState> monsterState;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _hittable = GetComponent<Hittable>();
        monsterState = new IdleState();
    }

    public void TransitionState(EMonsterState nextState)
    {
        if (monsterState.TransitionState(nextState))
        {
            monsterState = StateFactory.Instance.CreatMonsterState(nextState);
        };
    }

    void OnState()
    {
        
    }
    //private 
}

public abstract class State<T>
    where T : Enum
{
    public T stateEnum;
    //状态进入前，调用此函数
    public abstract void OnEnterState();
    //状态时，调用此函数
    public abstract void OnUpdateState();
    //状态退出前，调用此函数
    public abstract void OnExitState();
    //状态转换函数，通过此函数判断是否转移状态
    public abstract bool TransitionState(T id);
}

public class IdleState : State<EMonsterState>
{
    public IdleState()
    {
        stateEnum = EMonsterState.Idle;
    }

    public override void OnEnterState()
    {
        
    }
    public override void OnUpdateState()
    {
        //_animation.Play();
    }
    public override void OnExitState()
    {
        throw new NotImplementedException();
    }
    public override bool TransitionState(EMonsterState nextState)
    {
        switch (nextState)
        {
            case EMonsterState.Hurt:
                return true;
                break;
            default:
                break;
        }

        return false;
    }
}

public class HurtState : State<EMonsterState>
{
    public HurtState()
    {
        stateEnum = EMonsterState.Hurt;
    }

    public override void OnEnterState()
    {
        throw new NotImplementedException();
    }
    public override void OnUpdateState()
    {
        throw new NotImplementedException();
    }
    public override void OnExitState()
    {
        throw new NotImplementedException();
    }
    public override bool TransitionState(EMonsterState nextState)
    {
        throw new NotImplementedException();
    }
}

public class StateFactory : Singleton<StateFactory>
{
    public State<EMonsterState> CreatMonsterState(EMonsterState monsterState)
    {
        switch (monsterState)
        {
            case EMonsterState.Hurt:
                return new HurtState();
        }

        return new IdleState();
    }
}
