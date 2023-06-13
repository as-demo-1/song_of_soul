// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    public static class MoreEditorGuiUtility
    {

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
