using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Specifies keyboard and/or controller navigation settings.
    /// </summary>
    [System.Serializable]
    public class Navigation
    {

        /// <summary>
        /// Set <c>true</c> to enable keyboard/controller navigation of GUI controls.
        /// </summary>
        public bool enabled = false;

        /// <summary>
        /// Set <c>true</c> to always focus the first control when the parent is enabled.
        /// </summary>
        public bool focusFirstControlOnEnable = true;

        /// <summary>
        /// If the mouse hovers over a control to focus it, set this <c>true<c/c> to jump the current focus 
        /// to that control.
        /// </summary>
        public bool jumpToMousePosition = true;

        /// <summary>
        /// The list of child controls that can be navigated. You must set populate this list or navigation won't work.
        /// </summary>
        public GUIControl[] order;

        /// <summary>
        /// The button used to click the current control.
        /// </summary>
        public string clickButton = "Fire1";

        /// <summary>
        /// The key used to click the current control.
        /// </summary>
        public KeyCode click = KeyCode.Space;

        /// <summary>
        /// The key used to navigate to the previous control.
        /// </summary>
        public KeyCode previous = KeyCode.UpArrow;

        /// <summary>
        /// The key used to navigate to the next control.
        /// </summary>
        public KeyCode next = KeyCode.DownArrow;

        /// <summary>
        /// The controller axis that controls navigation.
        /// </summary>
        public string axis = "Vertical";

        public bool invertAxis = true;

        /// <summary>
        /// When holding the axis in a direction, the amount of time between navigating to the next/previous control.
        /// </summary>
        public float axisRepeatDelay = 1f;

        public float mouseWheelSensitivity = 5f;

        private int current = 0;

        private float axisRepeatTime = 0;

        private const float AxisThreshold = 0.5f;

        private const float MinorAxisThreshold = 0.01f;

        private float mouseWheelY = 0;

        private bool isAxisPrevDown = false;

        private bool isAxisNextDown = false;

        private float timeNextRelease = 0;

        /// <summary>
        /// Gets the name of the control that should currently have focus.
        /// </summary>
        /// <value>The name of the focused control.</value>
        public string FocusedControlName
        {
            get
            {
                return IsCurrentValid ? order[current].FullName : string.Empty;
            }
        }

        private bool IsCurrentValid
        {
            get
            {
                return IsOrderArrayValid && (0 <= current) && (current < order.Length);
            }
        }

        private bool IsOrderArrayValid
        {
            get
            {
                return (order != null) && (order.Length > 0);
            }
        }

        public bool IsClicked
        {
            get { return (Event.current.type == EventType.KeyDown) && (Event.current.keyCode == click); }
        }


        /// <summary>
        /// Focuses the first control.
        /// </summary>
        public void FocusFirstControl()
        {
            if (IsOrderArrayValid && IsClickableButton(order[0]))
            {
                current = 0;
            }
            else
            {
                current = (order != null) ? order.Length + 1 : 0;
                Navigate(1);
            }
        }

        /// <summary>
        /// Checks the navigation input and updates the current control if necessary.
        /// </summary>
        public void CheckNavigationInput(Vector2 relativeMousePosition)
        {
            CheckMouseWheel();
            float axisValue = GetNavigationAxis();
            if (IsPreviousControlInputDown(axisValue))
            {
                Navigate(-1);
            }
            else if (IsNextControlInputDown(axisValue))
            {
                Navigate(1);
            }
            else if (jumpToMousePosition)
            {
                NavigateToMousePosition(relativeMousePosition);
            }
        }

        private void NavigateToMousePosition(Vector2 relativeMousePosition)
        {
            for (int i = 0; i < order.Length; i++)
            {
                if (order[i].gameObject.activeInHierarchy && order[i].visible && IsClickableButton(order[i]) && order[i].rect.Contains(relativeMousePosition))
                {
                    current = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Navigates in a specified direction.
        /// </summary>
        /// <param name="direction">Direction (-1/+1).</param>
        public void Navigate(int direction)
        {
            if (IsOrderArrayValid)
            {
                int original = current;
                current = NextControlIndex(direction);
                int safeguard = 0;
                while (!(IsClickableButton(order[current]) || (current == original) || (safeguard > 999)))
                {
                    current = NextControlIndex(direction);
                    safeguard++;
                }
            }
        }

        private bool IsClickableButton(GUIControl control)
        {
            return ((control != null) && control.visible && (control is GUIButton) && (control as GUIButton).clickable);
        }

        private int NextControlIndex(int direction)
        {
            if (IsOrderArrayValid)
            {
                int result = (current + direction) % order.Length;
                return (result >= 0) ? result : order.Length - 1;
            }
            else
            {
                return 0;
            }
        }

        private void CheckMouseWheel()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                mouseWheelY += Event.current.delta.y;
            }
        }

        private bool IsNextControlInputDown(float axisValue)
        {
            if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == next))
            {
                Event.current.Use();
                isAxisNextDown = true;
            }
            else if (mouseWheelY >= mouseWheelSensitivity)
            {
                mouseWheelY = 0;
                return true;
            }
            bool justReleased = isAxisNextDown && (axisValue <= MinorAxisThreshold) && (Time.time >= timeNextRelease);
            isAxisNextDown = (axisValue > MinorAxisThreshold);
            if (axisValue > AxisThreshold)
            {
                if (DialogueTime.time >= axisRepeatTime)
                {
                    axisRepeatTime = DialogueTime.time + axisRepeatDelay;
                    timeNextRelease = Time.time + 0.5f;
                    return true;
                }
            }
            else
            {
                if (axisValue >= 0)
                {
                    axisRepeatTime = 0;
                }
                if (justReleased) return true;
            }
            return false;
        }

        private bool IsPreviousControlInputDown(float axisValue)
        {
            if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == previous))
            {
                Event.current.Use();
                isAxisPrevDown = true;
            }
            else if (mouseWheelY <= -mouseWheelSensitivity)
            {
                mouseWheelY = 0;
                return true;
            }
            bool justReleased = isAxisPrevDown && (axisValue >= -MinorAxisThreshold) && (Time.time >= timeNextRelease);
            isAxisPrevDown = (axisValue < -MinorAxisThreshold);
            if (axisValue < -AxisThreshold)
            {
                if (DialogueTime.time >= axisRepeatTime)
                {
                    axisRepeatTime = DialogueTime.time + axisRepeatDelay;
                    timeNextRelease = Time.time + 0.5f;
                    return true;
                }
            }
            else
            {
                if (axisValue <= 0)
                {
                    axisRepeatTime = 0;
                }
                if (justReleased) return true;
            }
            return false;
        }

        private float GetNavigationAxis()
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(axis)) return 0;
            try
            {
                return Input.GetAxis(axis) * (invertAxis ? -1 : 1);
            }
            catch (UnityException)
            {
                return 0;
            }
        }

    }

}
