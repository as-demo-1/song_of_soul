/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using UnityEngine;

    /// <summary>
    /// Character Mover used to move a character using a character controller.
    /// </summary>
    public class CharacterMover : ICharacterMover
    {

        private readonly Character m_Character;
        private readonly CharacterController m_CharacterController;
        private readonly Vector3 m_Gravity = new Vector3(0, -0.8f, 0);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        public CharacterMover(Character character)
        {
            m_Character = character;
            m_CharacterController = m_Character.CharacterController;
        }

        /// <summary>
        /// Update tick.
        /// </summary>
        public void Tick()
        {
            var movementInput = new Vector3(m_Character.CharacterInput.GetAxis(m_Character.HorizontalInput), 0, m_Character.CharacterInput.GetAxis(m_Character.VerticalInput));

            var movementRelativeCamera = m_Character.CharacterCamera.transform.TransformDirection(movementInput);
            movementRelativeCamera = new Vector3(movementRelativeCamera.x, 0, movementRelativeCamera.z).normalized;

            //CONVERT direction from local to world relative to camera
            m_CharacterController.SimpleMove((m_Character.Speed * movementRelativeCamera) + m_Gravity * Time.deltaTime);
        }
    }
}