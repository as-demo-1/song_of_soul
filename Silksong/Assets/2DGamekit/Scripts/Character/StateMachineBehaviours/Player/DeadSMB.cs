using UnityEngine;

namespace Gamekit2D
{
    public class DeadSMB : SceneLinkedSMB<PlayerCharacter>
    {
        public override void OnSLStatePostEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.SetMoveVector(m_MonoBehaviour.GetHurtDirection() * m_MonoBehaviour.hurtJumpSpeed);
        }

        public override void OnSLStateNoTransitionUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.CheckForGrounded();
            m_MonoBehaviour.AirborneVerticalMovement();
        }

        public override void OnSLStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.SetMoveVector(Vector2.zero);
        }
    }
}