using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class PlayerAnimatorStatesManager : AnimatorStatesManager
{
    public override Animator Animator { get; }
    public AnimatorParamsMapping PlayerAnimatorParamsMapping { get; private set; }
    public PlayerStatus CurrentPlayerStatus { get; set; }
    public bool CanJump { get; set; } = true;
    public bool CanMove { get; set; } = true;
    public StatusBehaviour PlayerBehaviour { get; set; }

    //public void Initialize(StatusBehaviour behaviour)
    //{

    //}

    public void ParamsUpdate() => this.PlayerAnimatorParamsMapping.ParamsUpdate();

    public void BehaviourUpdate()
    {
        PlayerBehaviour.StatusActiveBehaviour(CurrentPlayerStatus);
    }

    public void ChangePlayerStatus(PlayerStatus newStatus)
    {
        PlayerBehaviour.StatusExitBehaviour(CurrentPlayerStatus);
        CurrentPlayerStatus = newStatus;
        PlayerBehaviour.StatusEnterBehaviour(newStatus);
    }

    public PlayerAnimatorStatesManager(Animator animator, PlayerStatus currentPlayerStatus)
    {
        this.Animator = animator;
        this.CurrentPlayerStatus = currentPlayerStatus;
        this.PlayerBehaviour = new PlayerStatusBehaviour();
        this.PlayerAnimatorParamsMapping = new PlayerAnimatorParamsMapping(this);
    }

}

public abstract class AnimatorStatesManager 
{ 
    public abstract Animator Animator { get; }
}


public class PlayerAnimatorParamsMapping : AnimatorParamsMapping
{
    PlayerAnimatorStatesManager m_PlayerAnimatorStatesManager;
    public PlayerAnimatorParamsMapping(PlayerAnimatorStatesManager playerAnimatorStatesManager) : base(playerAnimatorStatesManager) 
    {
        this.m_PlayerAnimatorStatesManager = playerAnimatorStatesManager;
    }

    public int HorizontalInputParamHash { get; } = Animator.StringToHash("HorizontalInput");
    public int VerticalInputParamHash { get; } = Animator.StringToHash("VerticalInput");
    public int JumpIsDownParamHash { get; } = Animator.StringToHash("JumpIsDown");
    public int JumpIsValidParamHash { get; } = Animator.StringToHash("JumpIsValid");
    public int IsGroundedParamHash { get; } = Animator.StringToHash("IsGrounded");
    public int HorizontalSpeedParamHash { get; } = Animator.StringToHash("HorizontalSpeed");
    public int VerticalSpeedParamHash { get; } = Animator.StringToHash("VerticalSpeed");
    public int CanMoveParamHash { get; } = Animator.StringToHash("CanMove");
    public int CanJumpParamHash { get; } = Animator.StringToHash("CanJump");

    public int CurrentStatusParamHash { get; } = Animator.StringToHash("CurrentStatus");

    public override void ParamsUpdate()
    {
        m_Animator.SetInteger(HorizontalInputParamHash, (int)PlayerInput.Instance.horizontal.Value);
        m_Animator.SetInteger(VerticalInputParamHash, (int)PlayerInput.Instance.vertical.Value);
        m_Animator.SetBool(JumpIsDownParamHash, PlayerInput.Instance.jump.Down);
        m_Animator.SetBool(JumpIsValidParamHash, PlayerInput.Instance.jump.IsValid);
        m_Animator.SetBool(IsGroundedParamHash, PlayerController.Instance.IsGrounded);
        m_Animator.SetFloat(HorizontalSpeedParamHash, PlayerController.Instance.RB.velocity.x);
        m_Animator.SetFloat(VerticalSpeedParamHash, PlayerController.Instance.RB.velocity.y);
        m_Animator.SetBool(CanMoveParamHash, m_PlayerAnimatorStatesManager.CanMove);
        m_Animator.SetBool(CanJumpParamHash, m_PlayerAnimatorStatesManager.CanJump);
        m_Animator.SetInteger(CurrentStatusParamHash, (int)m_PlayerAnimatorStatesManager.CurrentPlayerStatus);
    }
}

public abstract class AnimatorParamsMapping 
{
    protected Animator m_Animator;
    protected AnimatorStatesManager m_AnimatorStatesManager;
    public AnimatorParamsMapping(AnimatorStatesManager animatorStatesManager) 
    {
        this.m_AnimatorStatesManager = animatorStatesManager;
        this.m_Animator = animatorStatesManager.Animator;
    }
    public abstract void ParamsUpdate();
}