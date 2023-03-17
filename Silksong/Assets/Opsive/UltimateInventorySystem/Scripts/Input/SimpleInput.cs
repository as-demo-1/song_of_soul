/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Input
{
    using Opsive.Shared.Input;
    using System;
    using UnityEngine;

    /// <summary>
    /// The input type used to specify when to check for the input.
    /// </summary>
    [Serializable]
    public enum InputType
    {
        Automatic,              // The ability will try to be started every update.
        Manual,                 // The ability must be started with TryStartAbility.
        ButtonDown,             // The ability will start when the specified button is down.
        ButtonDownContinuous,   // The ability will continuously check for a button down to determine if the ability should start.
        DoublePress,            // The ability will start when the specified button is pressed twice.
        LongPress,              // The ability will start when the specified button has been pressed for more than the specified duration.
        Tap,                    // The ability will start when the specified button is quickly tapped.
        Axis,                   // The ability will start when the specified axis is a non-zero value.
        Custom                  // The ability will start after a user defined starter has indicated that the ability should start.
    }

    /// <summary>
    /// A struct of a string and a keycode used for input.
    /// </summary>
    [Serializable]
    public class SimpleInput
    {
        [Tooltip("The button name(s) that can start or stop the ability.")]
        [SerializeField] protected InputType m_InputType = InputType.ButtonDown;
        [Tooltip("The button name(s) that can start or stop the ability.")]
        [SerializeField] protected string m_InputName;
        [Tooltip("Specifies how long the button should be pressed down until the ability starts/stops. Only used when the ability has a start/stop type of LongPress.")]
        [SerializeField] protected float m_LongPressDuration = 0.5f;
        [Tooltip("Should the long press wait to be activated until the button has been released?")]
        [SerializeField] protected bool m_WaitForLongPressRelease;

        protected bool m_ButtonUp;
        protected float m_InputAxisValue;
        
        public InputType InputType => m_InputType;
        public string InputName => m_InputName;
        public bool ButtonUp => m_ButtonUp;
        public float InputAxisValue => m_InputAxisValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SimpleInput()
        { }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buttonName">The button name used by the input manager.</param>
        /// <param name="inputType">The input type.</param>
        public SimpleInput(string buttonName, InputType inputType)
        {
            m_InputName = buttonName;
            m_InputType = inputType;
            m_LongPressDuration = 0.5f;
        }

        /// <summary>
        /// Check if the input is active
        /// </summary>
        public bool CheckInput(PlayerInput playerInput)
        {
            if (m_InputType == InputType.Automatic) { return true; }

            // For any start types that require the button to be down make sure it has first returned to the up state before checking if it is down again.
            if (!m_ButtonUp && (m_InputType == InputType.ButtonDown || m_InputType == InputType.DoublePress ||
                                   m_InputType == InputType.LongPress || m_InputType == InputType.Tap)) {
                m_ButtonUp = !playerInput.GetButton(m_InputName);
            }

            if (m_ButtonUp &&
                (m_InputType == InputType.ButtonDown && playerInput.GetButtonDown(m_InputName)) ||
                (m_InputType == InputType.ButtonDownContinuous && playerInput.GetButton(m_InputName)) ||
                (m_InputType == InputType.DoublePress && playerInput.GetDoublePress(m_InputName)) ||
                (m_InputType == InputType.LongPress && playerInput.GetLongPress(m_InputName, m_LongPressDuration, m_WaitForLongPressRelease)) ||
                (m_InputType == InputType.Tap && playerInput.GetTap(m_InputName))) {
                m_ButtonUp = false;
                return true;
            }

            float axisValue;
            if (m_InputType == InputType.Axis && Mathf.Abs((axisValue = playerInput.GetAxisRaw(m_InputName))) > 0.00001f) {
                m_InputAxisValue = axisValue;
                return true;
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Hot bar input.
    /// </summary>
    [Serializable]
    public class IndexedInput : SimpleInput
    {
        public int Index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The hot bar slot index affected by this input.</param>
        public IndexedInput(int index, string inputName, InputType inputType) : base(inputName,inputType)
        {
            Index = index;
        }
    }
}