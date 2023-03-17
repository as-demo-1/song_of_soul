/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Input
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// A simple component that maps a button or a key to an UnityEvent.
    /// </summary>
    public class InputEvent : MonoBehaviour
    {
        [Tooltip("A unity event called when the button or key is pressed.")]
        [SerializeField] protected UnityEvent m_UnityEvent;
        [Tooltip("The input button name.")]
        [SerializeField] protected string m_ButtonName;
        [Tooltip("A input key code.")]
        [SerializeField] protected KeyCode m_KeyCode;

        /// <summary>
        /// Responds to user input.
        /// </summary>
        void Update()
        {
            if (string.IsNullOrWhiteSpace(m_ButtonName) == false && Input.GetButtonDown(m_ButtonName)) {
                m_UnityEvent.Invoke();
            } else if (Input.GetKeyDown(m_KeyCode)) {
                m_UnityEvent.Invoke();
            }
        }
    }
}
