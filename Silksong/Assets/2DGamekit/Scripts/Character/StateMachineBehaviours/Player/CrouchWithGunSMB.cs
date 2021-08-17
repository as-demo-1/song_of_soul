using UnityEngine;

namespace Gamekit2D
{
    public class CrouchWithGunSMB : SceneLinkedSMB<PlayerCharacter>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.TeleportToColliderBottom();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.UpdateFacing();
            m_MonoBehaviour.CheckForCrouching();
            m_MonoBehaviour.CheckForHoldingGun();
            m_MonoBehaviour.CheckAndFireGun ();
            m_MonoBehaviour.CheckForGrounded();
            if (m_MonoBehaviour.CheckForFallInput())
            {
                if (m_MonoBehaviour.MakePlatformFallthrough())
                {
                    m_MonoBehaviour.ForceNotHoldingGun();
                }
            }
            m_MonoBehaviour.GroundedVerticalMovement();
            m_MonoBehaviour.GroundedHorizontalMovement(false);
        }

        public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo (0);
            if (!nextState.IsTag ("WithGun"))
                m_MonoBehaviour.ForceNotHoldingGun ();
        }
    }
}