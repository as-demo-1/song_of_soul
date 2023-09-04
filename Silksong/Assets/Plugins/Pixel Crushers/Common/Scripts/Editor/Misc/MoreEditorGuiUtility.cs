// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    public static class MoreEditorGuiUtility
    {

        //        // Evaluation version headaches. Get it at runtime instead:
        //#if UNITY_2022_3_OR_NEWER && !UNITY_2022_3_0
        //        public const string ToolbarSearchTextFieldName = "ToolbarSearchTextField";
        //        public const string ToolbarSearchCancelButtonName = "ToolbarSearchCancelButton";
        //        public const string ToolbarSearchCancelButtonEmpty = "ToolbarSearchCancelButtonEmpty";
        //#else
        //        public const string ToolbarSearchTextFieldName = "ToolbarSeachTextField";
        //        public const string ToolbarSearchCancelButtonName = "ToolbarSeachCancelButton";
        //        public const string ToolbarSearchCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
        //#endif

        private static GUIStyle toolbarSearchTextFieldStyle = null;
        private static GUIStyle toolbarSearchCancelButtonStyle = null;
        private static GUIStyle toolbarSearchCancelButtonEmptyStyle = null;
        public static string ToolbarSearchTextFieldName
        {
            get
            {
                if (toolbarSearchTextFieldStyle == null)
                {
                    toolbarSearchTextFieldStyle = FindCustomStyle("ToolbarSearchTextField", "ToolbarSeachTextField");
                }
                return toolbarSearchTextFieldStyle.name;
            }
        }
        public static string ToolbarSearchCancelButtonName
        {
            get
            {
                if (toolbarSearchCancelButtonStyle == null)
                {
                    toolbarSearchCancelButtonStyle = FindCustomStyle("ToolbarSearchCancelButton", "ToolbarSeachCancelButton");
                }
                return toolbarSearchCancelButtonStyle.name;
            }
        }
        public static string ToolbarSearchCancelButtonEmpty
        {
            get
            {
                if (toolbarSearchCancelButtonEmptyStyle == null)
                {
                    toolbarSearchCancelButtonEmptyStyle = FindCustomStyle("ToolbarSearchCancelButtonEmpty", "ToolbarSeachCancelButtonEmpty");
                }
                return toolbarSearchCancelButtonEmptyStyle.name;
            }
        }
        private static GUIStyle FindCustomStyle(string name1, string name2)
        {
            foreach (var style in GUI.skin.customStyles)
            {
                if ((style.name == name1) || (style.name == name2)) return style;
            }
            return GUI.skin.label;
        }

        public const float GearWidth = 15;
        public const float GearHeight = 14;

        private static GUIStyle m_gearMenuGuiStyle = null;

        /// <summary>
        /// Draws a gear menu button.
        /// </summary>
        /// <returns><c>true</c> if the button was clicked; otherwise <c>false</c>.</returns>
        public static bool DoGearMenu(Rect rect)
        {
            if (!LoadGearStyle()) return false;
            return GUI.Button(rect, GUIContent.none, m_gearMenuGuiStyle);
        }

        /// <summary>
        /// Draws a gear menu button in GUI layout mode.
        /// </summary>
        /// <returns><c>true</c> if the button was clicked; otherwise <c>false</c>.</returns>
        public static bool DoLayoutGearMenu()
        {
            if (!LoadGearStyle()) return false;
            return GUILayout.Button(GUIContent.none, m_gearMenuGuiStyle, GUILayout.Width(GearWidth), GUILayout.Height(GearHeight));
        }

        private static bool LoadGearStyle()
        {
            if (m_gearMenuGuiStyle == null)
            {
                var textureName = EditorGUIUtility.isProSkin ? "icons/d__Popup.png" : "icons/_Popup.png";
                Texture2D gearTexture = EditorGUIUtility.Load(textureName) as Texture2D;
                m_gearMenuGuiStyle = new GUIStyle(GUI.skin.label);
                m_gearMenuGuiStyle.normal.background = gearTexture;
                m_gearMenuGuiStyle.active.background = gearTexture;
                m_gearMenuGuiStyle.focused.background = gearTexture;
                m_gearMenuGuiStyle.hover.background = gearTexture;
                m_gearMenuGuiStyle.onNormal.background = gearTexture;
                m_gearMenuGuiStyle.onActive.background = gearTexture;
                m_gearMenuGuiStyle.onFocused.background = gearTexture;
                m_gearMenuGuiStyle.onHover.background = gearTexture;
            }
            return (m_gearMenuGuiStyle != null);
        }

    }
}
