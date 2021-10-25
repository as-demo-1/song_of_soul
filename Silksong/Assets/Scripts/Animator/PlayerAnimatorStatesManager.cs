using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class PlayerAnimatorStatesManager : AnimatorStatesManager
{
    public override Animator Animator { get; }
    public override StatusBehaviour CharacterStatusBehaviour { get; set; }
    public override AnimatorParamsMapping CharacterAnimatorParamsMapping { get; set; }
    public PlayerController PlayerController { get; set; }
    public PlayerStatus CurrentPlayerStatus { get; set; }
    public bool CanJump => PlayerController.CurrentAirExtraJumpCountLeft > 0 || PlayerController.IsGrounded;
    public bool CanMove { get; set; } = true;   

    public PlayerAnimatorStatesManager(PlayerController playerController, Animator animator, PlayerStatus currentPlayerStatus)
    {
        this.Animator = animator;
        this.CurrentPlayerStatus = currentPlayerStatus;
        this.PlayerController = playerController;
        this.CharacterStatusBehaviour = new PlayerStatusBehaviour(this.PlayerController);
        this.CharacterAnimatorParamsMapping = new PlayerAnimatorParamsMapping(this);
    }

    //public void Initialize(StatusBehaviour behaviour)
    //{

    //}

    public void BehaviourLateUpdate()
    {
        CharacterStatusBehaviour.StatusActiveBehaviour(CurrentPlayerStatus);
    }

    public void ChangePlayerStatus(PlayerStatus newStatus)
    {
        CharacterStatusBehaviour.StatusExitBehaviour(CurrentPlayerStatus);
        CurrentPlayerStatus = newStatus;
        CharacterStatusBehaviour.StatusEnterBehaviour(newStatus);
    }

}

public abstract class AnimatorStatesManager 
{ 
    public abstract Animator Animator { get; }
    public abstract StatusBehaviour CharacterStatusBehaviour { get; set; }
    public abstract AnimatorParamsMapping CharacterAnimatorParamsMapping { get; set; }

    public void ParamsLateUpdate() => this.CharacterAnimatorParamsMapping.ParamsUpdate();
}