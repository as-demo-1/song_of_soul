using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        m_Animator.SetBool(IsGroundedParamHash, m_PlayerAnimatorStatesManager.PlayerController.IsGrounded);
        m_Animator.SetFloat(HorizontalSpeedParamHash, m_PlayerAnimatorStatesManager.PlayerController.RB.velocity.x);
        m_Animator.SetFloat(VerticalSpeedParamHash, m_PlayerAnimatorStatesManager.PlayerController.RB.velocity.y);
        m_Animator.SetBool(CanMoveParamHash, m_PlayerAnimatorStatesManager.CanMove);
        m_Animator.SetBool(CanJumpParamHash, m_PlayerAnimatorStatesManager.CanJump);
        m_Animator.SetInteger(CurrentStatusParamHash, (int)m_PlayerAnimatorStatesManager.CurrentPlayerStatus);
    }
}

public abstract class AnimatorParamsMapping
{
    protected Animator m_Animator;
    public AnimatorParamsMapping(AnimatorStatesManager animatorStatesManager)
    {
        this.m_Animator = animatorStatesManager.Animator;
    }
    public abstract void ParamsUpdate();
}
