using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class HurtSMB : SceneLinkedSMB<PlayerCharacter>
    {
        public override void OnSLStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.SetMoveVector(m_MonoBehaviour.GetHurtDirection() * m_MonoBehaviour.hurtJumpSpeed);
            m_MonoBehaviour.StartFlickering ();
        }

        public override void OnSLStateNoTransitionUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(m_MonoBehaviour.IsFalling ())
                m_MonoBehaviour.CheckForGrounded();
            m_MonoBehaviour.AirborneVerticalMovement();
        }
    }
}