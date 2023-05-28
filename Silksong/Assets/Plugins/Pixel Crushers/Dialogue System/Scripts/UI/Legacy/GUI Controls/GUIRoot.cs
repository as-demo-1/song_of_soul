using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// The root of a GUI layout. Place all child controls under the root's hierarchy. The root
    /// applies a GUI skin to the layout, and refreshes the entire layout as necessary.
    /// 
    /// Only the root has an OnGUI method. All child controls are drawn from the root's OnGUI.
    /// This is much more efficient than giving each child its own OnGUI, since each OnGUI call
    /// requires substantial internal overhead to set up in Unity. To hide a GUI layout, you
    /// only need to disable the GUIRoot component or deactivate its game object. 
    /// </summary>
    /// <remarks>
    /// GUIRoot updates in editor mode. This allows you to view changes on the fly (WYSIWYG 
    /// editing).
    /// </remarks>
    [ExecuteInEditMode]
    [AddComponentMenu("")] // Deprecated
    public class GUIRoot : GUIControl
    {

        /// <summary>
        /// The GUI skin to apply to all of the child controls.
        /// </summary>
        public GUISkin guiSkin;

        /// <summary>
        /// Draws the GUI layout.
        /// </summary>
        public void OnGUI()
        {
            UseGUISkin();
            if (!Application.isPlaying) ManualRefresh();
            Vector2 screenMousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Draw(screenMousePosition);
        }

        /// <summary>
        /// Tells the root to recalculate the sizes, positions, and appearance of all of its
        /// children.
        /// </summary>
        public void ManualRefresh()
        {
            Refresh(new Vector2(Screen.width, Screen.height));
        }

        private void UseGUISkin()
        {
            if (guiSkin != null) GUI.skin = guiSkin;
        }

    }

}
