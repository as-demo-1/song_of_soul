using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAnimatorStatesControl
{
    public  Animator Animator { get; }
    //角色在不同的state上的行为，在animator的state上的SMB上设置对应动画的state
    public  PlayerStatesBehaviour CharacterStatesBehaviour { get; set; }
    //animator参数映射
    public  PlayerAnimatorParamsMapping CharacterAnimatorParamsMapping { get; }
    //提供一种设定角色state之间转换的状态，比如一个状态中从什么时候开始能够攻击，在animator的state上的SMB上设置对应动画的status
    public PlayerStatusDic PlayerStatusDic { get; private set; }

    public PlayerController PlayerController { get; set; }
    public EPlayerState CurrentPlayerState { get; set; }

    public PlayerAnimatorStatesControl(PlayerController playerController, Animator animator, EPlayerState currentPlayerState)
    {
        this.Animator = animator;
        this.PlayerController = playerController;
        this.CurrentPlayerState = currentPlayerState;
        this.CharacterStatesBehaviour = new PlayerStatesBehaviour(this.PlayerController);
        this.CharacterAnimatorParamsMapping = new PlayerAnimatorParamsMapping(this);
        this.PlayerStatusDic = new PlayerStatusDic(this.PlayerController,CharacterAnimatorParamsMapping);
        PlayerSMBEvents.Initialise(this.Animator,PlayerController);
    }


    public void ParamsUpdate() => this.CharacterAnimatorParamsMapping.ParamsUpdate();

    public void BehaviourLateUpdate()
    {
        CharacterStatesBehaviour.StatesActiveBehaviour(CurrentPlayerState);
    }

    public void ChangePlayerState(EPlayerState newState)
    {
        CharacterStatesBehaviour.StatesExitBehaviour(CurrentPlayerState,newState);
        EPlayerState t = CurrentPlayerState;
        CurrentPlayerState = newState;
        CharacterStatesBehaviour.StatesEnterBehaviour(CurrentPlayerState,t);

    }
}
