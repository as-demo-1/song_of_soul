using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorParamsMapping : AnimatorParamsMapping
{
    PlayerAnimatorStatesControl m_PlayerAnimatorStatesControl;
    public PlayerAnimatorParamsMapping(PlayerAnimatorStatesControl playerAnimatorStatesControl) : base(playerAnimatorStatesControl)
    {
        this.m_PlayerAnimatorStatesControl = playerAnimatorStatesControl;
    }

    public int HorizontalInputParamHash { get; } = Animator.StringToHash("HorizontalInput");
    public int VerticalInputParamHash { get; } = Animator.StringToHash("VerticalInput");
    public int JumpIsDownParamHash { get; } = Animator.StringToHash("JumpIsDown");
    public int JumpIsValidParamHash { get; } = Animator.StringToHash("JumpIsValid");
    public int NormalAttackIsValidParamHash { get; } = Animator.StringToHash("NormalAttackIsValid");
    public int IsGroundedParamHash { get; } = Animator.StringToHash("IsGrounded");
    public int HorizontalSpeedParamHash { get; } = Animator.StringToHash("HorizontalSpeed");
    public int VerticalSpeedParamHash { get; } = Animator.StringToHash("VerticalSpeed");
    public int CanMoveParamHash { get; } = Animator.StringToHash("CanMove");
    public int CanJumpParamHash { get; } = Animator.StringToHash("CanJump");
    public int CanNormalAttackParamHash { get; } = Animator.StringToHash("CanNormalAttack");

    public int CurrentStatusParamHash { get; } = Animator.StringToHash("CurrentStatus");

    public override void ParamsUpdate()
    {
        m_Animator.SetInteger(HorizontalInputParamHash, (int)PlayerInput.Instance.horizontal.Value);
        m_Animator.SetInteger(VerticalInputParamHash, (int)PlayerInput.Instance.vertical.Value);
        m_Animator.SetBool(JumpIsDownParamHash, PlayerInput.Instance.jump.Down);
        m_Animator.SetBool(JumpIsValidParamHash, PlayerInput.Instance.jump.IsValid);
        m_Animator.SetBool(NormalAttackIsValidParamHash, PlayerInput.Instance.normalAttack.IsValid);
        m_Animator.SetBool(IsGroundedParamHash, m_PlayerAnimatorStatesControl.PlayerController.IsGrounded);
        m_Animator.SetFloat(HorizontalSpeedParamHash, m_PlayerAnimatorStatesControl.PlayerController.RB.velocity.x);
        m_Animator.SetFloat(VerticalSpeedParamHash, m_PlayerAnimatorStatesControl.PlayerController.RB.velocity.y);
        m_Animator.SetBool(CanMoveParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic[EPlayerStatus.CanMove]);
        m_Animator.SetBool(CanJumpParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic[EPlayerStatus.CanJump]);
        m_Animator.SetBool(CanNormalAttackParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic[EPlayerStatus.CanNormalAttack]);
        m_Animator.SetInteger(CurrentStatusParamHash, (int)m_PlayerAnimatorStatesControl.CurrentPlayerState);
    }
}

public abstract class AnimatorParamsMapping
{
    protected Animator m_Animator;
    public AnimatorParamsMapping(AnimatorStatesControl animatorStatesManager)
    {
        this.m_Animator = animatorStatesManager.Animator;
    }
    public abstract void ParamsUpdate();
}
