using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class ChomperRunToTargetSMB : SceneLinkedSMB<EnemyBehaviour>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateEnter(animator, stateInfo, layerIndex);

            m_MonoBehaviour.OrientToTarget();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

            m_MonoBehaviour.CheckTargetStillVisible();
            m_MonoBehaviour.CheckMeleeAttack();

            float amount = m_MonoBehaviour.speed * 2.0f;
            if (m_MonoBehaviour.CheckForObstacle(amount))
            {
                m_MonoBehaviour.ForgetTarget();
            }
            else
                m_MonoBehaviour.SetHorizontalSpeed(amount);
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateExit(animator, stateInfo, layerIndex);

            m_MonoBehaviour.SetHorizontalSpeed(0);
        }
    }
}