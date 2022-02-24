using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorParamsMapping 
{
    protected Animator m_Animator;
    PlayerAnimatorStatesControl m_PlayerAnimatorStatesControl;

    public static bool HasControl=true;


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
    //public int CanMoveParamHash { get; } = Animator.StringToHash("CanMove");
    public int CanJumpParamHash { get; } = Animator.StringToHash("CanJump");
    public int CanNormalAttackParamHash { get; } = Animator.StringToHash("CanNormalAttack");

    public int CurrentStatesParamHash { get; } = Animator.StringToHash("CurrentStates");
    public int CanSprintParamHash { get; } = Animator.StringToHash("CanSprint");
    public int SprintIsValidParamHash { get; } = Animator.StringToHash("SprintIsValid");
    public int AirSprintLeftCountParamHash { get; } = Animator.StringToHash("AirSprintLeftCount");
    public int SprintReadyParamHash { get; } = Animator.StringToHash("SprintReady");
    public int CanBreakMoonParamHash { get; } = Animator.StringToHash("CanBreakMoon");

    public int BreakMoonIsValidParamHash { get; } = Animator.StringToHash("BreakMoonIsValid");

    public int HasBreakMoonPointParamHash { get; } = Animator.StringToHash("HasBreakMoonPoint");

    public int HurtParamHas { get; } = Animator.StringToHash("Hurt");

    public int DeadParamHas { get; } = Animator.StringToHash("IsDead");

    public int IsHealHeldParamHas { get; } = Animator.StringToHash("IsHealHeld");

    public int CanHealParamHas { get; } = Animator.StringToHash("CanHeal");

    public int CanToCatParamHas { get; } = Animator.StringToHash("CanToCat");
    public int ToCatIsValidParamHas { get; } = Animator.StringToHash("ToCatIsValid");
    public int IsCatParamHas { get; } = Animator.StringToHash("IsCat");
    public int HasUpSpaceForHumanParamHas { get; } = Animator.StringToHash("HasUpSpaceForHuman");

    public void ParamsUpdate()
    {
        if(HasControl)
        {
            m_Animator.SetInteger(HorizontalInputParamHash, (int)PlayerInput.Instance.horizontal.Value);
            m_Animator.SetInteger(VerticalInputParamHash, (int)PlayerInput.Instance.vertical.Value);

            m_Animator.SetBool(JumpIsValidParamHash, PlayerInput.Instance.jump.IsValid);

            m_Animator.SetBool(SprintIsValidParamHash, PlayerInput.Instance.sprint.IsValid);

            m_Animator.SetBool(NormalAttackIsValidParamHash, PlayerInput.Instance.normalAttack.IsValid);

            m_Animator.SetBool(BreakMoonIsValidParamHash, PlayerInput.Instance.breakMoon.IsValid);

            m_Animator.SetBool(IsHealHeldParamHas, PlayerInput.Instance.heal.Held);

            m_Animator.SetBool(ToCatIsValidParamHas, PlayerInput.Instance.toCat.IsValid);
        }
        else
        {
            m_Animator.SetInteger(HorizontalInputParamHash, 0);
            m_Animator.SetInteger(VerticalInputParamHash, 0);

            m_Animator.SetBool(JumpIsValidParamHash, false);

            m_Animator.SetBool(SprintIsValidParamHash, false);

            m_Animator.SetBool(NormalAttackIsValidParamHash, false);

            m_Animator.SetBool(BreakMoonIsValidParamHash, false);

            m_Animator.SetBool(IsHealHeldParamHas,false);

            m_Animator.SetBool(ToCatIsValidParamHas, false);
        }

        m_Animator.SetBool(IsGroundedParamHash, m_PlayerAnimatorStatesControl.PlayerController.isGroundedBuffer());

        m_Animator.SetFloat(HorizontalSpeedParamHash, m_PlayerAnimatorStatesControl.PlayerController.getRigidVelocity().x);
        m_Animator.SetFloat(VerticalSpeedParamHash, m_PlayerAnimatorStatesControl.PlayerController.getRigidVelocity().y);

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
