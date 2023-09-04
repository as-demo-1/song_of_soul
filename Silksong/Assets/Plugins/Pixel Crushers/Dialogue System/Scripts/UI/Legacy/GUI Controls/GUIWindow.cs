using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements GUI.Window, a draggable window.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUIWindow : GUIVisibleControl
    {

        /// <summary>
        /// Gets the default GUI style to use for this type of control. It can be overridden on a per-control
        /// basis using guiStyleName.
        /// </summary>
        /// <value>The default GUI style.</value>
        protected override GUIStyle DefaultGUIStyle
        {
            get { return GUI.skin.window; }
        }

        private Vector2 currentChildMousePosition;

        /// <summary>
        /// Draws the control, but not its children.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position within the window containing this control.</param>
        public override void DrawSelf(Vector2 relativeMousePosition)
        {
            SetGUIStyle();
            ApplyAlphaToGUIColor();
            currentChildMousePosition = new Vector2(relativeMousePosition.x - rect.x, relativeMousePosition.y - rect.y);
            Rect newRect = GUI.Window(0, rect, WindowFunction, text, GuiStyle);
            RestoreGUIColor();
            rect = newRect;
        }

        /// <summary>
        /// Draws the children, taking into account key/controller navigation if enabled.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position.</param>
        public override void DrawChildren(Vector2 relativeMousePosition)
        {
        }

        private void WindowFunction(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            foreach (var child in Children)
            {
                child.Draw(currentChildMousePosition);
            }
        }



    }

}
