// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// GUI styles, images, etc., used by the custom editors.
    /// </summary>
    public static class DialogueEditorStyles
    {

#if UNITY_2019_1_OR_NEWER
        public const string CollapsibleHeaderButtonStyleName = "Popup";
#else
        public const string CollapsibleHeaderButtonStyleName = "dragtab"; // Alternate choice: "OL Title";
#endif
        public const string CollapsibleSubheaderButtonStyleName = "MiniToolbarButtonLeft"; // Alternate choice: "ObjectFieldThumb";

#if UNITY_5 && !(EVALUATION_VERSION || ACADEMIC)
        public const string GroupBoxStyle = "AS TextArea";
#else
        public static GUIStyle GroupBoxStyle = EditorStyles.helpBox;
#endif

        public const string FoldoutOpenArrow = "\u25BC ";
        public const string FoldoutClosedArrow = "\u25BA ";

        private static Texture2D GetImage(string imagePath, ref Texture2D image)
        {
            if (image == null) image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            return image;
        }

        private static Texture2D GetEditorImage(string imagePath, ref Texture2D image)
        {
            if (image == null) image = EditorGUIUtility.Load(imagePath) as Texture2D;
            return image;
        }

        private static Color ProSkinCollapsibleHeaderOpenColor = new Color(0.3f, 0.8f, 1f);
        private static Color ProSkinCollapsibleHeaderClosedColor = new Color(0.6f, 0.6f, 0.6f);
        private static Color LightSkinCollapsibleHeaderOpenColor = new Color(0.3f, 0.8f, 1f);
        private static Color LightSkinCollapsibleHeaderClosedColor = new Color(0.6f, 0.6f, 0.6f);
        public static Color collapsibleHeaderOpenColor { get { return EditorGUIUtility.isProSkin ? ProSkinCollapsibleHeaderOpenColor : LightSkinCollapsibleHeaderOpenColor; } }
        public static Color collapsibleHeaderClosedColor { get { return EditorGUIUtility.isProSkin ? ProSkinCollapsibleHeaderClosedColor : LightSkinCollapsibleHeaderClosedColor; } }

    }
}
