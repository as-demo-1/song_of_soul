using UnityEngine;

namespace Gamekit2D
{
    public class LocomotionWithGunSMB : SceneLinkedSMB<PlayerCharacter>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.TeleportToColliderBottom();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.UpdateFacing();
            m_MonoBehaviour.GroundedHorizontalMovement(true);
            m_MonoBehaviour.GroundedVerticalMovement();
            m_MonoBehaviour.CheckForCrouching();
            m_MonoBehaviour.CheckForGrounded();
            m_MonoBehaviour.CheckForPushing();
            m_MonoBehaviour.CheckForHoldingGun();
            m_MonoBehaviour.CheckAndFireGun();
            if (m_MonoBehaviour.CheckForJumpInput())
                m_MonoBehaviour.SetVerticalMovement(m_MonoBehaviour.jumpSpeed);
            else if (m_MonoBehaviour.CheckForMeleeAttackInput())
                m_MonoBehaviour.MeleeAttack();
        }

        public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo (0);
            if (!nextState.IsTag ("WithGun"))
                m_MonoBehaviour.ForceNotHoldingGun ();
        }
    }
}