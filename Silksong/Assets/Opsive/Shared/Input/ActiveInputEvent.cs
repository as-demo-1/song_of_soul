/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input
{
    /// <summary>
    /// The ActiveInputEvent class allows the object to receive input callbacks while the object is active.
    /// </summary>
    public class ActiveInputEvent
    {
        // Specifies any inputs that should be received while the object is active.
        public enum Type
        {
            ButtonDown,     // The object will receive input when the specified button is down.
            ButtonUp,       // The object will receive input when the specified button is up.
            DoublePress,    // The object will receive input when the specified input is pressed twice.
            LongPress,      // The object will receive input when the specified button has been pressed for more the than the specified duration.
            Axis            // The object will receive input for the axis.
        }

        private Type m_InputType;
        private string m_InputName;
        private float m_LongPressDuration;
        private bool m_WaitForLongPressRelease;
        private string m_EventName;

        public string EventName { get { return m_EventName; } }

        /// <summary>
        /// Initializes the ActiveInputEvent object to the specified values.
        /// </summary>
        /// <param name="inputType">Specifies how the event should be triggered.</param>
        /// <param name="inputName">The button name that will trigger the event.</param>
        /// <param name="eventName">The event to execute.</param>
        public void Initialize(Type inputType, string inputName, string eventName)
        {
            Initialize(inputType, inputName, 0, false, eventName);
        }

        /// <summary>
        /// Initializes the ActiveInputEvent object to the specified values.
        /// </summary>
        /// <param name="inputType">Specifies how the event should be triggered.</param>
        /// <param name="inputName">The button name that will trigger the event.</param>
        /// <param name="longPressDuration">Specifies how long the button should be pressed until the event is executed. Only used with a Type of LongPress.</param>
        /// <param name="waitForLongPressRelease">Should the long press wait to be activated until the button has been released?</param>
        /// <param name="eventName">The event to execute.</param>
        public void Initialize(Type inputType, string inputName, float longPressDuration, bool waitForLongPressRelease, string eventName)
        {
            m_InputType = inputType;
            m_InputName = inputName;
            m_LongPressDuration = longPressDuration;
            m_WaitForLongPressRelease = waitForLongPressRelease;
            m_EventName = eventName;
        }

        /// <summary>
        /// Returns true if the button at the specified type has been triggered.
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>True if the button at the specified type has been triggered.</returns>
        public bool HasButtonEvent(PlayerInput playerInput)
        {
            if ((m_InputType == Type.ButtonDown && playerInput.GetButtonDown(m_InputName)) || 
                (m_InputType == Type.ButtonUp && playerInput.GetButtonUp(m_InputName)) ||
                (m_InputType == Type.DoublePress && playerInput.GetDoublePress(m_InputName)) ||
                (m_InputType == Type.LongPress && playerInput.GetLongPress(m_InputName, m_LongPressDuration, m_WaitForLongPressRelease))) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the AxisInputMap is using an axis.
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns></returns>
        public bool HasAxisEvent(PlayerInput playerInput)
        {
            if (m_InputType == Type.Axis) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the axis value which maps to the input at the specified name.
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>The axis value which maps to the input at the specified name.</returns>
        public float GetAxisValue(PlayerInput playerInput)
        {
            return playerInput.GetAxisRaw(m_InputName);
        }
    }
}