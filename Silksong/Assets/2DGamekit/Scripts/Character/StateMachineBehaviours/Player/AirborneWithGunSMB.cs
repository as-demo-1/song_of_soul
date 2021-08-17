using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class AirborneWithGunSMB : SceneLinkedSMB<PlayerCharacter>
    {
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.UpdateFacing();
            m_MonoBehaviour.UpdateJump();
            m_MonoBehaviour.AirborneHorizontalMovement();
            m_MonoBehaviour.AirborneVerticalMovement();
            m_MonoBehaviour.CheckForGrounded();
            m_MonoBehaviour.CheckForHoldingGun();
            if (m_MonoBehaviour.CheckForMeleeAttackInput())
                m_MonoBehaviour.MeleeAttack();
            m_MonoBehaviour.CheckAndFireGun();
            m_MonoBehaviour.CheckForCrouching();
        }

        public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo (0);
            if (!nextState.IsTag ("WithGun"))
                m_MonoBehaviour.ForceNotHoldingGun ();
        }
    }
}