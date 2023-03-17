/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Opsive.UltimateInventorySystem.Demo.CharacterControl.Player;
    using UnityEngine;

    /// <summary>
    /// Makes the camera follow a character transform.
    /// </summary>
    public class CharacterCamera : MonoBehaviour
    {
        [Tooltip("The transform that the camera will follow.")]
        [SerializeField] protected Transform m_Follow;

        protected Vector3 m_StartOffset;

        // Start is called before the first frame update
        void Start()
        {
            if (m_Follow == null) {
                m_Follow = FindObjectOfType<PlayerCharacter>().transform;
            }

            m_StartOffset = transform.position - m_Follow.position;
        }

        /// <summary>
        /// Late Update to remove hiccups.
        /// </summary>
        void LateUpdate()
        {
            transform.position = m_Follow.position + m_StartOffset;
        }
    }
}
