/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Utility
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Contains the styles for the inspector buttons.
    /// </summary>
    public static class InspectorStyles
    {
        private static GUIStyle s_WordWrapLabel;
        public static GUIStyle WordWrapLabel
        {
            get
            {
                if (s_WordWrapLabel == null) {
                    s_WordWrapLabel = new GUIStyle(EditorStyles.label);
                    s_WordWrapLabel.wordWrap = true;
                }
                return s_WordWrapLabel;
            }
        }
        private static GUIStyle s_WordWrapLabelCenter;
        public static GUIStyle WordWrapLabelCenter
        {
            get
            {
                if (s_WordWrapLabelCenter == null) {
                    s_WordWrapLabelCenter = new GUIStyle(EditorStyles.label);
                    s_WordWrapLabelCenter.alignment = TextAnchor.MiddleCenter;
                    s_WordWrapLabelCenter.wordWrap = true;
                }
                return s_WordWrapLabelCenter;
            }
        }
        private static GUIStyle s_WordWrapTextArea;
        public static GUIStyle WordWrapTextArea
        {
            get
            {
                if (s_WordWrapTextArea == null) {
                    s_WordWrapTextArea = new GUIStyle(EditorStyles.textArea);
                    s_WordWrapTextArea.wordWrap = true;
                }
                return s_WordWrapTextArea;
            }
        }
        private static GUIStyle s_BoldTextField;
        public static GUIStyle BoldTextField
        {
            get
            {
                if (s_BoldTextField == null) {
                    s_BoldTextField = new GUIStyle(EditorStyles.textField);
                    s_BoldTextField.fontStyle = FontStyle.Bold;
                }
                return s_BoldTextField;
            }
        }
        private static GUIStyle s_BoldLabel;
        public static GUIStyle BoldLabel
        {
            get
            {
                if (s_BoldLabel == null) {
                    s_BoldLabel = new GUIStyle(EditorStyles.largeLabel);
                    s_BoldLabel.fontStyle = FontStyle.Bold;
                }
                return s_BoldLabel;
            }
        }
        private static GUIStyle s_LargeBoldLabel;
        public static GUIStyle LargeBoldLabel
        {
            get
            {
                if (s_LargeBoldLabel == null) {
                    s_LargeBoldLabel = new GUIStyle(EditorStyles.largeLabel);
                    s_LargeBoldLabel.fontStyle = FontStyle.Bold;
                    s_LargeBoldLabel.fontSize = 16;
                }
                return s_LargeBoldLabel;
            }
        }
        private static GUIStyle s_CenterLabel;
        public static GUIStyle CenterLabel
        {
            get
            {
                if (s_CenterLabel == null) {
                    s_CenterLabel = new GUIStyle(EditorStyles.largeLabel);
                    s_CenterLabel.alignment = TextAnchor.UpperCenter;
                    s_CenterLabel.padding = new RectOffset();
                }
                return s_CenterLabel;
            }
        }
        private static GUIStyle s_CenterBoldLabel;
        public static GUIStyle CenterBoldLabel
        {
            get
            {
                if (s_CenterBoldLabel == null) {
                    s_CenterBoldLabel = new GUIStyle(EditorStyles.largeLabel);
                    s_CenterBoldLabel.alignment = TextAnchor.UpperCenter;
                    s_CenterBoldLabel.padding = new RectOffset();
                    s_CenterBoldLabel.fontStyle = FontStyle.Bold;
                }
                return s_CenterBoldLabel;
            }
        }
        private static GUIStyle s_FieldStyle;
        public static GUIStyle FieldStyle
        {
            get
            {
                if (s_FieldStyle == null) {
                    s_FieldStyle = new GUIStyle("HelpBox");
                    s_FieldStyle.fontSize = 9;
                    s_FieldStyle.wordWrap = true;
                    s_FieldStyle.alignment = TextAnchor.MiddleLeft;
                    s_FieldStyle.padding.top = 35;
                    s_FieldStyle.padding.left = 15;
                    s_FieldStyle.padding.bottom = 5;
                    s_FieldStyle.padding.right = 15;
                }
                return s_FieldStyle;
            }
        }
        private static GUIStyle s_ItemStyle;
        public static GUIStyle ItemStyle
        {
            get
            {
                if (s_ItemStyle == null) {
                    s_ItemStyle = new GUIStyle("TE NodeBoxSelected");
                    s_ItemStyle.fontSize = 9;
                    s_ItemStyle.wordWrap = false;
                    s_ItemStyle.clipping = TextClipping.Clip;
                    s_ItemStyle.contentOffset = new Vector2(22, 5);
                    s_ItemStyle.padding.right = 25;
                    s_ItemStyle.normal.textColor = new Color(0, 0, 0, 0.75f);
                }
                return s_ItemStyle;
            }
        }
        private static GUIStyle s_LinkStyle;
        public static GUIStyle LinkStyle
        {
            get
            {
                if (s_LinkStyle == null) {
                    s_LinkStyle = new GUIStyle("Label");
                    s_LinkStyle.normal.textColor = (EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 1.0f, 1.0f) : Color.blue);
                }
                return s_LinkStyle;
            }
        }
        public static Texture2D AddIcon { get { if (s_AddIcon == null) { s_AddIcon = LoadTexture("aff49c3d1ef455d488b96b6ed77550b0", "21eff06b5e4af0e478ea75270aca329a"); } return s_AddIcon; } }
        private static Texture2D s_AddIcon;
        public static Texture2D ActivateIcon { get { if (s_ActivateIcon == null) { s_ActivateIcon = LoadTexture("e152e9061481aab429f2b07a70c12d2e", "a4d2cff4316a64946b786d75f681cd33"); } return s_ActivateIcon; } }
        private static Texture2D s_ActivateIcon;
        public static Texture2D DeleteIcon { get { if (s_DeleteIcon == null) { s_DeleteIcon = LoadTexture("01290b743a16c0946b599beba004ca10", "cc50d62765d4d79479ddf540cac0989b"); } return s_DeleteIcon; } }
        private static Texture2D s_DeleteIcon;
        public static Texture2D DuplicateIcon { get { if (s_DuplicateIcon == null) { s_DuplicateIcon = LoadTexture("ba8a6a7b5fe943b48bef5d532c257d38", "4d9d8217015eee543ac1b745a096016d"); } return s_DuplicateIcon; } }
        private static Texture2D s_DuplicateIcon;
        public static Texture2D InfoIcon { get { if (s_InfoIcon == null) { s_InfoIcon = LoadTexture("487848bbccf6d414bacf8e840467e094", "14890942c77df924d95bfdf229fcffe5"); } return s_InfoIcon; } }
        private static Texture2D s_InfoIcon;
        public static Texture2D PersistIcon { get { if (s_PersistIcon == null) { s_PersistIcon = LoadTexture("ee727c0b6401c8f4db0126f95f518541", "20a2a7d0dc8114f4083f51b8883d06b6"); } return s_PersistIcon; } }
        private static Texture2D s_PersistIcon;
        public static Texture2D RemoveIcon { get { if (s_RemoveIcon == null) { s_RemoveIcon = LoadTexture("71fd51a904de3f14086279a41bec5b78", "088c80b2883e964498350b2075ecde18"); } return s_RemoveIcon; } }
        private static Texture2D s_RemoveIcon;
        public static Texture2D UpdateIcon { get { if (s_UpdateIcon == null) { s_UpdateIcon = LoadTexture("c1502f6cab2c3a542bb77c572d8567de", "ffb6d9e2798b5f8418dd9d5c9fc9118f"); } return s_UpdateIcon; } }
        private static Texture2D s_UpdateIcon;

        public static GUIStyle NoPaddingButtonStyle {
            get {
                if (s_NoPaddingButtonStyle == null) {
                    s_NoPaddingButtonStyle = new GUIStyle(GUI.skin.button);
                    s_NoPaddingButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                }
                return s_NoPaddingButtonStyle;
            }
        }
        public static GUIStyle s_NoPaddingButtonStyle;

        /// <summary>
        /// Loads the texture with the specified guid.
        /// </summary>
        /// <param name="lightSkinGUID">The guid for the texture with the light skin.</param>
        /// <param name="darkSkinGUID">The guid for the texture with the dark skin.</param>
        /// <returns></returns>
        private static Texture2D LoadTexture(string lightSkinGUID, string darkSkinGUID)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(EditorGUIUtility.isProSkin ? darkSkinGUID : lightSkinGUID);
            if (!string.IsNullOrEmpty(assetPath)) {
                return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
            }
            return null;
        }
    }
}