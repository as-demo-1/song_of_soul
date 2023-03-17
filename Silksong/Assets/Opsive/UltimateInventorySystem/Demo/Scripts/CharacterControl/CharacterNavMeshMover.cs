/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// A character mover that uses an NavMeshAgent.
    /// </summary>
    public class CharacterNavMeshMover : ICharacterMover
    {

        private readonly Vector3 m_Gravity = new Vector3(0, -0.8f, 0);
        private readonly Character m_Character;
        protected CharacterController m_CharacterController;
        protected NavMeshAgent m_NavMeshAgent;

        public Transform Target { get; set; }
        public NavMeshAgent NavMeshAgent => m_NavMeshAgent;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        public CharacterNavMeshMover(Character character)
        {
            m_Character = character;
            m_CharacterController = m_Character.CharacterController;
            m_NavMeshAgent = m_Character.GetComponent<NavMeshAgent>();
            m_NavMeshAgent.enabled = true;
            m_NavMeshAgent.speed = m_Character.Speed;
        }

        /// <summary>
        /// Tick update move.
        /// </summary>
        public void Tick()
        {
            if (m_NavMeshAgent.enabled == false) { return; }

            if (Target != null) {
                m_NavMeshAgent.destination = Target.position;
                var desiredVelocity = m_NavMeshAgent.desiredVelocity;

                m_NavMeshAgent.updatePosition = false;
                m_NavMeshAgent.updateRotation = false;

                var lookPos = m_NavMeshAgent.destination - m_Character.transform.position;
                lookPos.y = 0;

                if (lookPos != Vector3.zero) {
                    var targetRot = Quaternion.LookRotation(lookPos);
                    m_Character.transform.rotation = Quaternion.Slerp(m_Character.transform.rotation, targetRot, Time.deltaTime * m_NavMeshAgent.angularSpeed);

                }

                m_CharacterController.Move(desiredVelocity.normalized * (m_Character.Speed * Time.deltaTime) + m_Gravity * Time.deltaTime);
            } else {
                m_NavMeshAgent.destination = m_Character.transform.position;
                m_CharacterController.Move(Vector3.zero);
            }

            m_NavMeshAgent.velocity = m_CharacterController.velocity;
        }
    }
}