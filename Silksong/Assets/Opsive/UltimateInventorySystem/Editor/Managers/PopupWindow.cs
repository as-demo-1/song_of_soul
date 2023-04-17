/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A editor pop up menu
    /// </summary>
    public class PopupWindow : EditorWindow
    {
        protected VisualElement m_Content;
        private static PopupWindow s_Instance;

        public VisualElement Content => m_Content;

        /// <summary>
        /// The pop up instance.
        /// </summary>
        private static PopupWindow Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = CreateInstance<PopupWindow>();
                    s_Instance.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Get a pop up window.
        /// </summary>
        /// <param name="rect">The position.</param>
        /// <param name="size">The size.</param>
        /// <param name="content">The pop up content.</param>
        /// <returns>The pop up window.</returns>
        public static PopupWindow OpenWindow(Rect rect, Vector2 size, VisualElement content)
        {
            var window = Instance;
            window.m_Content = content;
            window.ShowAsDropDown(rect, size);

            if (rect.position.y + rect.size.y > Screen.currentResolution.height) {
                rect.position -= new Vector2(0, rect.size.y);
            }

            window.position = rect;
            window.Refresh();
            return window;
        }

        /// <summary>
        /// Redraw the content.
        /// </summary>
        public virtual void Refresh()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(m_Content);
        }
    }
}