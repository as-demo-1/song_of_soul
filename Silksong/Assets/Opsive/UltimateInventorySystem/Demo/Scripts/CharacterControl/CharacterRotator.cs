/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using UnityEngine;

    /// <summary>
    /// script used to rotate a character.
    /// </summary>
    public class CharacterRotator
    {
        private readonly Character m_Character;
        private readonly CharacterController m_CharacterController;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        public CharacterRotator(Character character)
        {
            m_Character = character;
            m_CharacterController = m_Character.CharacterController;
        }

        /// <summary>
        /// Rotate the character in respect to the camera.
        /// </summary>
        public void Tick()
        {

            var charVelocity = m_Character.CharacterInput != null
                ? new Vector2(m_Character.CharacterInput.GetAxis(m_Character.HorizontalInput), m_Character.CharacterInput.GetAxis(m_Character.VerticalInput))
                : Vector2.zero;
            if (Mathf.Abs(charVelocity.x) < 0.1f &&
                Mathf.Abs(charVelocity.y) < 0.1f) {
                return;
            }
            float targetRotation =
                Mathf.Atan2(charVelocity.x, charVelocity.y)
                * Mathf.Rad2Deg + m_Character.CharacterCamera.transform.eulerAngles.y;

            Quaternion lookAt = Quaternion.Slerp(m_Character.transform.rotation,
                Quaternion.Euler(0, targetRotation, 0),
                0.5f);

            m_Character.transform.rotation = lookAt;
        }

    }
}