using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements GUI.Button with optional additional textures.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUIButton : GUIVisibleControl
    {

        /// <summary>
        /// Is the button clickable (active)?
        /// </summary>
        public bool clickable = true;

        /// <summary>
        /// The disabled image.
        /// </summary>
        public GUIImageParams disabled;

        /// <summary>
        /// The normal image.
        /// </summary>
        public GUIImageParams normal;

        /// <summary>
        /// The hover image.
        /// </summary>
        public GUIImageParams hover;

        /// <summary>
        /// The pressed image.
        /// </summary>
        public GUIImageParams pressed;

        public AudioClip hoverSound = null;

        public AudioClip clickSound = null;

        /// <summary>
        /// The input that triggers the button (an alternative to using the mouse to click it).
        /// </summary>
        public InputTrigger trigger;

        /// <summary>
        /// The message to send to the target when the button is clicked.
        /// </summary>
        public string message = "OnClick";

        /// <summary>
        /// The parameter for the message sent to the target. If the data field (below) is assigned,
        /// the button will send the data. Otherwise, if the parameter string is set, it will send
        /// the string. Otherwise, it will send a reference to this button itself.
        /// </summary>
        public string parameter;

        /// <summary>
        /// The target to send the message to.
        /// </summary>
        public Transform target;

        /// <summary>
        /// The data to send to the target as a parameter to the message. If this field is 
        /// assigned, the button will send it. Otherwise, if the parameter string is set, it will
        /// send the string. Otherwise, it will send a reference to this button itself.
        /// </summary>
        public object data;

        /// <summary>
        /// Gets the default GUI style to use for this type of control. It can be overridden on a per-control
        /// basis using guiStyleName.
        /// </summary>
        /// <value>The default GUI style.</value>
        protected override GUIStyle DefaultGUIStyle
        {
            get { return GUI.skin.button; }
        }

        private bool isHovered = false;

        /// <summary>
        /// Draws the control, but not its children.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position within the window containing this control.</param>
        public override void DrawSelf(Vector2 relativeMousePosition)
        {
            if (clickable)
            {
                DrawClickable(relativeMousePosition);
            }
            else
            {
                DrawUnclickable();
            }
        }

        private void DrawClickable(Vector2 relativeMousePosition)
        {
            if (rect.Contains(relativeMousePosition))
            {
                if (Input.GetMouseButton(0))
                {
                    if (pressed != null) pressed.Draw(rect);
                }
                else
                {
                    if (isHovered == false)
                    {
                        isHovered = true;
                        PlaySound(hoverSound);
                    }
                    if (hover != null) hover.Draw(rect);
                }
            }
            else
            {
                if (isHovered == true) isHovered = false;
                if (normal != null) normal.Draw(rect);
            }
            if (GUI.Button(rect, text, GuiStyle)) Click();
        }

        private void DrawUnclickable()
        {
            if (disabled.texture != null)
            {
                if (disabled != null) disabled.Draw(rect);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                GUI.enabled = false;
                GUI.Button(rect, text, GuiStyle);
                GUI.enabled = true;
            }
        }

        /// <summary>
        /// Checks if the button has been "clicked" by the trigger key or input button.
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (clickable && trigger.isDown)
            {
                Click();
            }
        }

        /// <summary>
        /// Clicks the button. You can call this manually to simulate a mouse click.
        /// </summary>
        public void Click()
        {
            PlaySound(clickSound);
            Transform actualTarget = Tools.Select(target, this.transform);
            object actualData = null;
            if (data != null)
            {
                actualData = data;
            }
            else if (!string.IsNullOrEmpty(parameter))
            {
                actualData = parameter;
            }
            else
            {
                actualData = this;
            }
            actualTarget.SendMessage(message, actualData, SendMessageOptions.DontRequireReceiver);
        }

    }

}
