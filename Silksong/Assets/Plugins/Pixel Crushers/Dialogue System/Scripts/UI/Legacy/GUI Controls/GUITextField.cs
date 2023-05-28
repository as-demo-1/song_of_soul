using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements GUI.TextField for text input.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUITextField : GUIVisibleControl
    {

        /// <summary>
        /// The maximum number of characters the user can enter, or <c>0</c> for unlimited.
        /// </summary>
        public int maxLength = 0;

        /// <summary>
        /// Gets the default GUI style to use for this type of control. It can be overridden on a per-control
        /// basis using guiStyleName.
        /// </summary>
        /// <value>The default GUI style.</value>
        protected override GUIStyle DefaultGUIStyle
        {
            get { return GUI.skin.textField; }
        }

        private bool takeFocus = false;

        /// <summary>
        /// Makes this control take focus.
        /// </summary>
        public void TakeFocus()
        {
            takeFocus = true;
        }

        /// <summary>
        /// Draws the control, but not its children.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position within the window containing this control.</param>
        public override void DrawSelf(Vector2 relativeMousePosition)
        {
            SetGUIStyle();
            if (takeFocus) GUI.SetNextControlName(FullName);
            if (text == null) text = string.Empty;
            if (maxLength == 0)
            {
                text = GUI.TextField(rect, text, GuiStyle);
            }
            else
            {
                text = GUI.TextField(rect, text, maxLength, GuiStyle);
            }
            if (takeFocus)
            {
                GUI.FocusControl(FullName);
                takeFocus = false;
            }
        }

    }

}
