// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Utility methods for custom editors.
    /// </summary>
    public static class EditorWindowTools
    {

        #region File IO

        public static string GetDirectoryName(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                try
                {
                    return Path.GetDirectoryName(filename);
                }
                catch (ArgumentException)
                {
                }
            }
            return string.Empty;
        }

        public static string GetCurrentDirectory()
        {
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path)) path = Path.GetDirectoryName(path);
                return path;
            }
            return "Assets";
        }

        #endregion

        #region General GUI 
        public static void DrawHorizontalLine()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        }

        public static void StartIndentedSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
            EditorGUILayout.BeginVertical();
        }

        public static void EndIndentedSection()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawDeprecatedTriggerHelpBox()
        {
            //--- Skip. No need to scare people unnecessarily:
            //EditorGUILayout.HelpBox("This component has been deprecated. It still works fine, but the preferred trigger component is now Dialogue System Trigger.", MessageType.Info);
        }

        #endregion

        #region Editor GUI

        public static bool EditorGUILayoutFoldout(string label, string tooltip, bool foldout, bool topLevel = true)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = foldout ? DialogueEditorStyles.collapsibleHeaderOpenColor : DialogueEditorStyles.collapsibleHeaderClosedColor;
#if UNITY_2019_1_OR_NEWER
                var text = label;
#else
                var text = topLevel ? ("<b>" + label + "</b>") : label;
#endif
                var guiContent = new GUIContent((foldout ? DialogueEditorStyles.FoldoutOpenArrow : DialogueEditorStyles.FoldoutClosedArrow) + text, tooltip);
                var guiStyle = topLevel ? DialogueEditorStyles.CollapsibleHeaderButtonStyleName : DialogueEditorStyles.CollapsibleSubheaderButtonStyleName;
                if (!GUILayout.Toggle(true, guiContent, guiStyle))
                {
                    foldout = !foldout;
                }
                GUI.backgroundColor = Color.white;
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
            return foldout;
        }

        public static void EditorGUILayoutVerticalSpace(float pixels)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(pixels);
            EditorGUILayout.EndVertical();
        }

        public static void EditorGUILayoutBeginGroup()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal(DialogueEditorStyles.GroupBoxStyle, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight));
            GUILayout.BeginVertical();
            GUILayout.Space(2);
        }

        public static void EditorGUILayoutEndGroup()
        {
            try
            {
                GUILayout.Space(3);
                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
                GUILayout.Space(3);
            }
            catch (ArgumentException)
            {
                // If Unity opens a popup bwindow such as a color picker, it raises an exception.
            }
        }

        public static void EditorGUILayoutBeginIndent()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(18);
            EditorGUILayout.BeginVertical();
        }

        public static void EditorGUILayoutEndIndent()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void RemoveReorderableListElementWithoutLeavingNull(UnityEditorInternal.ReorderableList list)
        {
            // If an objectReferenceValue is assigned to the list element, the default DoRemoveButton method
            // will only unassign it but it won't actually delete the element. To cleanly delete the element,
            // this method first unassigns the objectReferenceValue, then calls DoRemoveButton.
            if (!(list != null && 0 <= list.index && list.index < list.serializedProperty.arraySize)) return;
            var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                element.objectReferenceValue = null;
            }
            UnityEditorInternal.ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        public static void SetTextFieldProperty(SerializedProperty textFieldProperty, string value)
        {
            if (textFieldProperty == null) return;
            var textProperty = textFieldProperty.FindPropertyRelative("m_text");
            var stringAssetProperty = textFieldProperty.FindPropertyRelative("m_stringAsset");
            var textTableProperty = textFieldProperty.FindPropertyRelative("m_textTable");
            if (textProperty != null) textProperty.stringValue = value;
            if (stringAssetProperty != null) stringAssetProperty.objectReferenceValue = null;
            if (textTableProperty != null) textTableProperty.objectReferenceValue = null;
        }

#endregion



    }

}
