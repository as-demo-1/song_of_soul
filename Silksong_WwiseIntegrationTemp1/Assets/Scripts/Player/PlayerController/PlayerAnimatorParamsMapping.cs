using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorParamsMapping 
{
    protected Animator m_Animator;
    PlayerAnimatorStatesControl m_PlayerAnimatorStatesControl;
    public PlayerAnimatorParamsMapping(PlayerAnimatorStatesControl playerAnimatorStatesControl) 
    {
        this.m_PlayerAnimatorStatesControl = playerAnimatorStatesControl;
        this.m_Animator = playerAnimatorStatesControl.Animator;
    }

    public int HorizontalInputParamHash { get; } = Animator.StringToHash("HorizontalInput");
    public int VerticalInputParamHash { get; } = Animator.StringToHash("VerticalInput");
    public int JumpIsValidParamHash { get; } = Animator.StringToHash("JumpIsValid");

    public int JumpLeftCountParamHash { get; } = Animator.StringToHash("JumpLeftCount");
    public int NormalAttackIsValidParamHash { get; } = Animator.StringToHash("NormalAttackIsValid");
    public int IsGroundedParamHash { get; } = Animator.StringToHash("IsGrounded");
    public int HorizontalSpeedParamHash { get; } = Animator.StringToHash("HorizontalSpeed");
    public int VerticalSpeedParamHash { get; } = Animator.StringToHash("VerticalSpeed");
    public int CanMoveParamHash { get; } = Animator.StringToHash("CanMove");
    public int CanJumpParamHash { get; } = Animator.StringToHash("CanJump");
    public int CanNormalAttackParamHash { get; } = Animator.StringToHash("CanNormalAttack");

    public int CurrentStatesParamHash { get; } = Animator.StringToHash("CurrentStates");
    public int CanSprintParamHash { get; } = Animator.StringToHash("CanSprint");
    public int SprintIsValidParamHash { get; } = Animator.StringToHash("SprintIsValid");
    public int AirSprintLeftCountParamHash { get; } = Animator.StringToHash("AirSprintLeftCount");
    public int SprintReadyParamHash { get; } = Animator.StringToHash("SprintReady");
    public int CanBreakMoonParamHash { get; } = Animator.StringToHash("CanBreakMoon");

    public int BreakMoonIsValidParamHash { get; } = Animator.StringToHash("BreakMoonIsValid");


    public void ParamsUpdate()
    {
        m_Animator.SetInteger(HorizontalInputParamHash, (int)PlayerInput.Instance.horizontal.Value);
        m_Animator.SetInteger(VerticalInputParamHash, (int)PlayerInput.Instance.vertical.Value);

        m_Animator.SetBool(JumpIsValidParamHash, PlayerInput.Instance.jump.IsValid);

        m_Animator.SetBool(SprintIsValidParamHash, PlayerInput.Instance.sprint.IsValid);

        m_Animator.SetBool(NormalAttackIsValidParamHash, PlayerInput.Instance.normalAttack.IsValid);

        m_Animator.SetBool(BreakMoonIsValidParamHash, PlayerInput.Instance.breakMoon.IsValid);

        m_Animator.SetBool(IsGroundedParamHash, m_PlayerAnimatorStatesControl.PlayerController.isGroundedBuffer());

        m_Animator.SetFloat(HorizontalSpeedParamHash, m_PlayerAnimatorStatesControl.PlayerController.getRigidVelocity().x);
        m_Animator.SetFloat(VerticalSpeedParamHash, m_PlayerAnimatorStatesControl.PlayerController.getRigidVelocity().y);

        m_Animator.SetBool(CanMoveParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic.getPlayerStatus(EPlayerStatus.CanMove));
        m_Animator.SetBool(CanJumpParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic.getPlayerStatus(EPlayerStatus.CanJump));
        m_Animator.SetBool(CanNormalAttackParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic.getPlayerStatus(EPlayerStatus.CanNormalAttack));
        m_Animator.SetBool(CanSprintParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic.getPlayerStatus(EPlayerStatus.CanSprint));
        m_Animator.SetBool(CanBreakMoonParamHash, m_PlayerAnimatorStatesControl.PlayerStatusDic.getPlayerStatus(EPlayerStatus.CanBreakMoon)
            &&  m_PlayerAnimatorStatesControl.CharacterStatesBehaviour.playerBreakMoon.currentTarget!=null);

        m_Animator.SetInteger(CurrentStatesParamHash, (int)m_PlayerAnimatorStatesControl.CurrentPlayerState);
    }
}

/*public abstract class AnimatorParamsMapping
{
    protected Animator m_Animator;
    public AnimatorParamsMapping(AnimatorStatesControl animatorStatesManager)
    {
        this.m_Animator = animatorStatesManager.Animator;
    }
    public abstract void ParamsUpdate();
}*/
