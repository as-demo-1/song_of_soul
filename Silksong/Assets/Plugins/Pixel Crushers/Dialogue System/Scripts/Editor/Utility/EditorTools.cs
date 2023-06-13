// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Utility functions used by the Dialogue System's custom editors.
    /// </summary>
    public static class EditorTools
    {

        public static DialogueDatabase selectedDatabase = null;

        private static GUIStyle m_textAreaGuiStyle = null;
        public static GUIStyle textAreaGuiStyle
        {
            get
            {
                if (m_textAreaGuiStyle == null)
                {
                    m_textAreaGuiStyle = new GUIStyle(EditorStyles.textArea);
                    m_textAreaGuiStyle.fixedHeight = 0;
                    m_textAreaGuiStyle.stretchHeight = true;
                    m_textAreaGuiStyle.wordWrap = true;
                }
                return m_textAreaGuiStyle;
            }
        }

        private static GUIStyle m_dropDownGuiStyle = null;
        public static GUIStyle dropDownGuiStyle
        {
            get
            {
                if (m_dropDownGuiStyle == null)
                {
                    m_dropDownGuiStyle = GUI.skin.GetStyle("DropDown");
                    if (m_dropDownGuiStyle == null) m_dropDownGuiStyle = GUI.skin.label;
                }
                return m_dropDownGuiStyle;
            }
        }
        public static float GetPopupWidth(GUIContent guiContent)
        {
            var size = dropDownGuiStyle.CalcSize(guiContent);
            return (dropDownGuiStyle == GUI.skin.label) ? (size.x + 16) : size.x;
        }
        public static float GetPopupWidth(string text)
        {
            return GetPopupWidth(new GUIContent(text));
        }
        public static GUILayoutOption GUILayoutPopupWidth(object obj)
        {            
            return GUILayout.Width(GetPopupWidth(new GUIContent(obj.ToString())));
        }


        public static GUILayoutOption GUILayoutStyleWidth(GUIStyle style, GUIContent guiContent)
        {
            if (style == null) return GUILayout.Width(60);
            var size = style.CalcSize(guiContent);
            return GUILayout.Width(size.x);
        }
        public static GUILayoutOption GUILayoutStyleWidth(GUIStyle style, string s)
        {
            return GUILayoutStyleWidth(style, new GUIContent(s));
        }

        public static GUILayoutOption GUILayoutLabelWidth(string s)
        {
            return GUILayoutStyleWidth(GUI.skin.label, s);
        }
        public static GUILayoutOption GUILayoutButtonWidth(GUIContent guiContent)
        {
            return GUILayoutStyleWidth(GUI.skin.button, guiContent);
        }
        public static GUILayoutOption GUILayoutButtonWidth(string s)
        {
            return GUILayoutButtonWidth(new GUIContent(s));
        }
        public static GUILayoutOption GUILayoutToggleWidth(GUIContent guiContent)
        {
            return GUILayoutStyleWidth(GUI.skin.toggle, guiContent);
        }
        public static GUILayoutOption GUILayoutToggleWidth(string s)
        {
            return GUILayoutStyleWidth(GUI.skin.toggle, new GUIContent(s));
        }

        public static DialogueDatabase FindInitialDatabase()
        {
            var dialogueSystemController = Object.FindObjectOfType<DialogueSystemController>();
            return (dialogueSystemController == null) ? null : dialogueSystemController.initialDatabase;
        }

        public static void SetInitialDatabaseIfNull()
        {
            if (selectedDatabase == null)
            {
                selectedDatabase = FindInitialDatabase();
            }
        }

        public static void DrawReferenceDatabase()
        {
            selectedDatabase = EditorGUILayout.ObjectField(new GUIContent("Reference Database", "Database to use for pop-up menus. Assumes this database will be in memory at runtime."), selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
        }

        public static void DrawReferenceDatabase(Rect rect)
        {
            selectedDatabase = EditorGUI.ObjectField(rect, new GUIContent("Reference Database", "Database to use for pop-up menus. Assumes this database will be in memory at runtime."), selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
        }

        public static void DrawSerializedProperty(SerializedObject serializedObject, string propertyName)
        {
            serializedObject.Update();
            var property = serializedObject.FindProperty(propertyName);
            if (property == null) return;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public static Color NodeColorStringToColor(string s)
        {
            switch (s)
            {
                case "Aqua":
                    return Color.cyan;
                case "Blue":
                    return NodeColor_Blue;
                case "Gray":
                    return NodeColor_Gray;
                case "Green":
                    return NodeColor_Green;
                case "Grey":
                    return Color.gray;
                case "Orange":
                    return NodeColor_Orange;
                case "Red":
                    return NodeColor_Red;
                case "Yellow":
                    return Color.yellow;
                default:
                    return Tools.WebColor(s);
            }
        }

        //---No longer used, now that we allow a full color palette:
        //public static string[] StylesColorStrings = new string[]
        //{
        //    "Aqua", "Blue", "Gray", "Green", "Orange", "Red", "Yellow"
        //};

        // Node style colors:
#if UNITY_2019_3_OR_NEWER // Use lighter colors for Pro in 2019.3+:
        public static Color NodeColor_Orange_Dark = new Color(1f, 0.5f, 0);
        public static Color NodeColor_Gray_Dark = new Color(0.9f, 0.9f, 0.9f);
        public static Color NodeColor_Blue_Dark = new Color(0.4f, 0.6f, 1f);
        public static Color NodeColor_Green_Dark = new Color(0, 1f, 0);
        public static Color NodeColor_Red_Dark = new Color(1f, 0.1f, 0.1f);
#else
        public static Color NodeColor_Orange_Dark = new Color(0.875f, 0.475f, 0);
        public static Color NodeColor_Gray_Dark = new Color(0.33f, 0.33f, 0.33f);
        public static Color NodeColor_Blue_Dark = new Color(0.22f, 0.38f, 0.64f);
        public static Color NodeColor_Green_Dark = new Color(0, 0.6f, 0);
        public static Color NodeColor_Red_Dark = new Color(0.7f, 0.1f, 0.1f);
#endif

        public static Color NodeColor_Orange_Light = new Color(1f, 0.7f, 0.4f);
        public static Color NodeColor_Gray_Light = new Color(0.7f, 0.7f, 0.7f);
        public static Color NodeColor_Blue_Light = new Color(0.375f, 0.64f, 0.95f);
        public static Color NodeColor_Green_Light = new Color(0, 0.85f, 0);
        public static Color NodeColor_Red_Light = new Color(0.7f, 0.1f, 0.1f);

        public static Color NodeColor_Orange { get { return EditorGUIUtility.isProSkin ? NodeColor_Orange_Dark : NodeColor_Orange_Light; } }
        public static Color NodeColor_Gray { get { return EditorGUIUtility.isProSkin ? NodeColor_Gray_Dark : NodeColor_Gray_Light; } }
        public static Color NodeColor_Blue { get { return EditorGUIUtility.isProSkin ? NodeColor_Blue_Dark : NodeColor_Blue_Light; } }
        public static Color NodeColor_Green { get { return EditorGUIUtility.isProSkin ? NodeColor_Green_Dark : NodeColor_Green_Light; } }
        public static Color NodeColor_Red { get { return EditorGUIUtility.isProSkin ? NodeColor_Red_Dark : NodeColor_Red_Light; } }

        public static void SetDirtyBeforeChange(UnityEngine.Object obj, string name)
        {
            Undo.RecordObject(obj, name);
        }

        public static void SetDirtyAfterChange(UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
        }

        public static void TryAddScriptingDefineSymbols(string newDefine)
        {
            MoreEditorUtility.TryAddScriptingDefineSymbols(newDefine);
        }

        public static void ReimportScripts()
        {
            Debug.Log("Recompiled scripts with updated options. If options are not working, please right-click on the Dialogue System's Scripts and Wrappers folders and select Reimport.");
            AssetDatabase.ImportAsset("Assets/Plugins/Pixel Crushers/Dialogue System/Scripts");
            AssetDatabase.ImportAsset("Assets/Plugins/Pixel Crushers/Dialogue System/Wrappers");
        }

        public static string GetAssetName(Asset asset)
        {
            if (asset == null) return string.Empty;
            return (asset is Conversation) ? (asset as Conversation).Title : asset.Name;
        }

        public static bool IsAssetInFilter(Asset asset, string filter)
        {

            if (asset == null || string.IsNullOrEmpty(filter)) return true;
            var assetName = asset.Name;
            return string.IsNullOrEmpty(assetName) ? false : (assetName.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

    }

}
