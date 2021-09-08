using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateParamsManager
{
    public int ParamHashHorizontal { get; private set; } = Animator.StringToHash("Horizontal");
    public int ParamHashNormaliedSpeed { get; private set; } = Animator.StringToHash("NormaliedSpeed");
    public int ParamHashAttackSimple { get; private set; } = Animator.StringToHash("AttackSimple");
    public int ParamHashDash { get; private set; } = Animator.StringToHash("Dash");
    public int ParamHashJump { get; private set; } = Animator.StringToHash("Jump");
    public int ParamHashTurnAround { get; private set; } = Animator.StringToHash("TurnAround");
    public int ParamHashIsGrounded { get; private set; } = Animator.StringToHash("IsGrounded");
    public int ParamHashIsFalling { get; private set; } = Animator.StringToHash("IsFalling");
    public int ParamHashCanWalk { get; private set; } = Animator.StringToHash("CanWalk");
    public int ParamHashCanJump { get; private set; } = Animator.StringToHash("CanJump");
    public int ParamHashCanDash { get; private set; } = Animator.StringToHash("CanDash");
    public int ParamHashCanAttack { get; private set; } = Animator.StringToHash("CanAttack");
    public void ParamsUpdate(Animator animator)
    {
        SetAnimatorCancelParamsToFalse(animator);
    }

    void SetAnimatorCancelParamsToFalse(Animator animator)
    {
        animator.SetBool(ParamHashCanWalk, false);
        animator.SetBool(ParamHashCanJump, false);
        animator.SetBool(ParamHashCanDash, false);
        animator.SetBool(ParamHashCanAttack, false);
    }
}
