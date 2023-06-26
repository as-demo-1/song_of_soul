/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input
{
    using Opsive.Shared.Input.VirtualControls;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Acts as a common base class for input using the Unity Input Manager. Works with keyboard/mouse, controller, and mobile input.
    /// </summary>
    public class UnityInput : PlayerInput
    {
        /// <summary>
        /// Specifies if any input type should be forced.
        /// </summary>
        public enum ForceInputType { None, Standalone, Virtual }

        [Tooltip("Specifies if any input type should be forced.")]
        [SerializeField] protected ForceInputType m_ForceInput;
        [Tooltip("Should the cursor be disabled?")]
        [SerializeField] protected bool m_DisableCursor = true;
        [Tooltip("Should the cursor be enabled when the escape key is pressed?")]
        [SerializeField] protected bool m_EnableCursorWithEscape = true;
        [Tooltip("If the cursor is enabled with escape should the look vector be prevented from updating?")]
        [SerializeField] protected bool m_PreventLookVectorChanges = true;
        [Tooltip("The joystick is considered up when the raw value is less than the specified threshold.")]
        [Range(0, 1)] [SerializeField] protected float m_JoystickUpThreshold = 1;

        public bool DisableCursor { get { return m_DisableCursor; }
            set
            {
                if (m_Input is VirtualInput) {
                    m_DisableCursor = false;
                }
                m_DisableCursor = value;
                if (m_DisableCursor && Cursor.visible) {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                } else if (!m_DisableCursor && !Cursor.visible) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        public bool EnableCursorWithEscape { get { return m_EnableCursorWithEscape; } set { m_EnableCursorWithEscape = value; } }
        public bool PreventLookMovementWithEscape { get { return m_PreventLookVectorChanges; } set { m_PreventLookVectorChanges = value; } }
        public float JoystickUpThreshold { get { return m_JoystickUpThreshold; } set { m_JoystickUpThreshold = value; } }

        private InputBase m_Input;
        private bool m_UseVirtualInput;
        private HashSet<string> m_JoystickDownSet;
        private HashSet<string> m_ToAddJoystickDownSet;
        private HashSet<string> m_ToRemoveJoystickDownSet;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_UseVirtualInput = m_ForceInput == ForceInputType.Virtual;
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP_8_1 || UNITY_BLACKBERRY)
            if (m_ForceInput != ForceInputType.Standalone) {
                m_UseVirtualInput = true;
            }
#endif
            if (m_UseVirtualInput) {
                m_Input = new VirtualInput();
                // The cursor must be enabled for virtual controls to allow the drag events to occur.
                m_DisableCursor = false;
            } else {
                m_Input = new StandaloneInput();
            }
            m_Input.Initialize(this);
        }

        /// <summary>
        /// The component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            if (!m_UseVirtualInput && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Associates the VirtualControlsManager with the VirtualInput object.
        /// </summary>
        /// <param name="virtualControlsManager">The VirtualControlsManager to associate with the VirtualInput object.</param>
        /// <returns>True if the virtual controls were registered.</returns>
        public bool RegisterVirtualControlsManager(VirtualControlsManager virtualControlsManager)
        {
            if (m_Input is VirtualInput) {
                (m_Input as VirtualInput).RegisterVirtualControlsManager(virtualControlsManager);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the VirtualControlsManager association.
        /// </summary>
        public void UnegisterVirtualControlsManager()
        {
            if (m_Input is VirtualInput) {
                (m_Input as VirtualInput).UnregisterVirtualControlsManager();
            }
        }

        /// <summary>
        /// Is the cursor visible?
        /// </summary>
        /// <returns>True if the cursor is visible.</returns>
        public override bool IsCursorVisible()
        {
            if (m_Input is VirtualInput) {
                return false;
            }
            return base.IsCursorVisible();
        }

        /// <summary>
        /// Update the joystick and cursor state values.
        /// </summary>
        private void LateUpdate()
        {
            if (m_UseVirtualInput) {
                return;
            }
            // The joystick is no longer down after the axis is 0.
            if (IsControllerConnected()) {
                // GetButtonUp/Down doesn't immediately add the button name to the set to prevent the GetButtonUp/Down from returning false
                // if it is called twice within the same frame. Add it after the frame has ended.
                if (m_JoystickDownSet != null) {
                    foreach (var item in m_JoystickDownSet) {
                        if (Mathf.Abs(m_Input.GetAxisRaw(item)) < m_JoystickUpThreshold) {
                            m_ToRemoveJoystickDownSet.Add(item);
                        }
                    }
                    foreach (var item in m_ToRemoveJoystickDownSet) {
                        m_JoystickDownSet.Remove(item);
                    }
                    m_ToRemoveJoystickDownSet.Clear();
                }
                if (m_ToAddJoystickDownSet != null && m_ToAddJoystickDownSet.Count > 0) {
                    if (m_JoystickDownSet == null) {
                        m_JoystickDownSet = new HashSet<string>();
                        m_ToRemoveJoystickDownSet = new HashSet<string>();
                    }
                    foreach (var item in m_ToAddJoystickDownSet) {
                        m_JoystickDownSet.Add(item);
                    }
                    m_ToAddJoystickDownSet.Clear();
                }
            }

            // Enable the cursor if the escape key is pressed. Disable the cursor if it is visbile but should be disabled upon press.
            if (m_EnableCursorWithEscape && UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (m_PreventLookVectorChanges) {
                    OnApplicationFocus(false);
                }
            } else if (Cursor.visible && m_DisableCursor && !IsPointerOverUI() && (UnityEngine.Input.GetKeyDown(KeyCode.Mouse0) || UnityEngine.Input.GetKeyDown(KeyCode.Mouse1))) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (m_PreventLookVectorChanges) {
                    OnApplicationFocus(true);
                }
            }
#if UNITY_EDITOR
            // The cursor should be visible when the game is paused.
            if (!Cursor.visible && UnityEditor.EditorApplication.isPaused && !m_DisableCursor) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
#endif
        }

        /// <summary>
        /// Internal method which returns true if the button is being pressed.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        protected override bool GetButtonInternal(string buttonName)
        {
            if (m_Input.GetButton(buttonName, InputBase.ButtonAction.GetButton)) {
                return true;
            }
            if (IsControllerConnected()) {
                if (Mathf.Abs(m_Input.GetAxisRaw(buttonName)) == 1) {
                    if (m_JoystickDownSet == null || !m_JoystickDownSet.Contains(buttonName)) {
                        if (m_ToAddJoystickDownSet == null) {
                            m_ToAddJoystickDownSet = new HashSet<string>();
                        }
                        m_ToAddJoystickDownSet.Add(buttonName);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Internal method which returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        protected override bool GetButtonDownInternal(string buttonName)
        {
            if (IsControllerConnected() && Mathf.Abs(m_Input.GetAxisRaw(buttonName)) == 1) {
                // The button should only be considered down on the first frame.
                if (m_JoystickDownSet != null && m_JoystickDownSet.Contains(buttonName)) {
                    return false;
                }
                if (m_ToAddJoystickDownSet == null) {
                    m_ToAddJoystickDownSet = new HashSet<string>();
                }
                m_ToAddJoystickDownSet.Add(buttonName);
                return true;
            }
            return m_Input.GetButton(buttonName, InputBase.ButtonAction.GetButtonDown);
        }

        /// <summary>
        /// Internal method which returnstrue if the button is up.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        protected override bool GetButtonUpInternal(string buttonName)
        {
            if (IsControllerConnected()) {
                if (Mathf.Abs(m_Input.GetAxisRaw(buttonName)) == 1 && (m_JoystickDownSet == null || !m_JoystickDownSet.Contains(buttonName))) {
                    if (m_ToAddJoystickDownSet == null) {
                        m_ToAddJoystickDownSet = new HashSet<string>();
                    }
                    m_ToAddJoystickDownSet.Add(buttonName);
                    return false;
                }
                if (m_JoystickDownSet != null && m_JoystickDownSet.Contains(buttonName) && Mathf.Abs(m_Input.GetAxisRaw(buttonName)) < m_JoystickUpThreshold) {
                    return true;
                }
                return false;
            }
            return m_Input.GetButton(buttonName, InputBase.ButtonAction.GetButtonUp);
        }

        /// <summary>
        /// Internal method which returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        protected override float GetAxisInternal(string buttonName)
        {
            return m_Input.GetAxis(buttonName);
        }

        /// <summary>
        /// Internal method which returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        protected override float GetAxisRawInternal(string buttonName)
        {
            return m_Input.GetAxisRaw(buttonName);
        }

        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        public override bool IsPointerOverUI()
        {
            // The input will always be over a UI element with virtual inputs.
            if (m_Input is VirtualInput) {
                return false;
            }
            return base.IsPointerOverUI();
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        protected override void EnableGameplayInput(bool enable)
        {
            base.EnableGameplayInput(enable);

            if (enable && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Does the game have focus?
        /// </summary>
        /// <param name="hasFocus">True if the game has focus.</param>
        protected override void OnApplicationFocus(bool hasFocus)
        {
            base.OnApplicationFocus(hasFocus);

            if (enabled && hasFocus && m_DisableCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}