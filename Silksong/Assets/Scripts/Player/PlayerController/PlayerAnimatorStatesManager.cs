using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAnimatorStatesControl : AnimatorStatesControl
{
    public override Animator Animator { get; }
    public override StatesBehaviour CharacterStatesBehaviour { get; set; }
    public override AnimatorParamsMapping CharacterAnimatorParamsMapping { get; }
    public PlayerStatusDic PlayerStatusDic { get; private set; }

    public PlayerController PlayerController { get; set; }
    public PlayerState CurrentPlayerState { get; set; }

    public PlayerAnimatorStatesControl(PlayerController playerController, Animator animator, PlayerState currentPlayerState)
    {
        this.Animator = animator;
        this.PlayerController = playerController;
        this.CurrentPlayerState = currentPlayerState;
        this.CharacterStatesBehaviour = new PlayerStatesBehaviour(this.PlayerController);
        this.CharacterAnimatorParamsMapping = new PlayerAnimatorParamsMapping(this);
        this.PlayerStatusDic = new PlayerStatusDic(this.PlayerController);
        TestSMB.Initialise(this.Animator);
    }

    //public void Initialize(StatusBehaviour behaviour)
    //{

    //}

    public void PlayerStatusLateUpdate()
    {
        PlayerStatusFlagOverrideAsDefault();
        PlayerStatusDic[PlayerStatus.CanJump].SetFlag(PlayerController.CurrentAirExtraJumpCountLeft > 0 || PlayerController.IsGrounded);
        PlayerStatusDic[PlayerStatus.CanMove].SetFlag(true);

        void PlayerStatusFlagOverrideAsDefault()
        {
            foreach (var item in (Dictionary<PlayerStatus, PlayerStatusDic.PlayerStatusFlag>)PlayerStatusDic)
            {
                item.Value.SetFlag(true, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.Override);
            }
        }
    }

    public void BehaviourLateUpdate()
    {
        CharacterStatesBehaviour.StatesActiveBehaviour(CurrentPlayerState);
    }

    public void ChangePlayerState(PlayerState newState)
    {
        CharacterStatesBehaviour.StatesExitBehaviour(CurrentPlayerState);
        CurrentPlayerState = newState;
        CharacterStatesBehaviour.StatesEnterBehaviour(newState);
    }

}

public abstract class AnimatorStatesControl
{
    public abstract Animator Animator { get; }
    public abstract StatesBehaviour CharacterStatesBehaviour { get; set; }
    public abstract AnimatorParamsMapping CharacterAnimatorParamsMapping { get; }

    public void ParamsLateUpdate() => this.CharacterAnimatorParamsMapping.ParamsUpdate();
}