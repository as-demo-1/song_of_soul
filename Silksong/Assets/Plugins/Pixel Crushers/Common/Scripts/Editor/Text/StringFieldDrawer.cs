// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomPropertyDrawer(typeof(StringField), true)]
    public class StringFieldDrawer : PropertyDrawer
    {

        public const int NumExpandedLines = 5;

        private static GUIStyle s_wrappedTextAreaGUIStyle = null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(position, property, label, false);
        }

        public static float GetHeight(SerializedProperty property)
        {
            var textProperty = property.FindPropertyRelative("m_text");
            var stringAssetProperty = property.FindPropertyRelative("m_stringAsset");
            var textTableProperty = property.FindPropertyRelative("m_textTable");
            var isContentAssigned = (textProperty != null && !string.IsNullOrEmpty(textProperty.stringValue)) ||
                (stringAssetProperty != null && stringAssetProperty.objectReferenceValue != null) ||
                (textTableProperty != null && textTableProperty.objectReferenceValue != null);
            return (isContentAssigned ? 1 : 3) * EditorGUIUtility.singleLineHeight;
        }

        public static void Draw(Rect position, SerializedProperty property, GUIContent label, bool expandHeight)
        {
            try
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Get child properties:
                var textProperty = property.FindPropertyRelative("m_text");
                var stringAssetProperty = property.FindPropertyRelative("m_stringAsset");
                var textTableProperty = property.FindPropertyRelative("m_textTable");
                var textTableFieldIDProperty = property.FindPropertyRelative("m_textTableFieldID");
                if (textProperty == null || stringAssetProperty == null || textTableProperty == null || textTableFieldIDProperty == null)
                {
                    Debug.LogError("Sorry! There was an internal editor error with a String Field. Please contact Pixel Crushers for support.");
                    return;
                }
                var isTextAssigned = (!string.IsNullOrEmpty(textProperty.stringValue));
                var isStringAssetAssigned = (stringAssetProperty.objectReferenceValue != null);
                var isTextTableAssigned = (textTableProperty.objectReferenceValue != null);
                var isContentAssigned = isTextAssigned || isStringAssetAssigned || isTextTableAssigned;

                float yOffset = 0;

                // Text row:
                if (isTextAssigned || !isContentAssigned)
                {
                    if (expandHeight)
                    {
                        if (s_wrappedTextAreaGUIStyle == null)
                        {
                            s_wrappedTextAreaGUIStyle = new GUIStyle(EditorStyles.textArea);
                            s_wrappedTextAreaGUIStyle.wordWrap = true;
                        }
                        textProperty.stringValue = EditorGUI.TextArea(new Rect(position.x, position.y, position.width, NumExpandedLines * EditorGUIUtility.singleLineHeight), textProperty.stringValue, s_wrappedTextAreaGUIStyle);
                    }
                    else
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), textProperty, GUIContent.none);
                    }
                    yOffset += (expandHeight ? NumExpandedLines : 1) * EditorGUIUtility.singleLineHeight;
                }

                if (isStringAssetAssigned || !isContentAssigned)
                {
                    float buttonWidth = 40;
                    EditorGUI.PropertyField(new Rect(position.x, position.y + yOffset, position.width - buttonWidth, EditorGUIUtility.singleLineHeight), stringAssetProperty, GUIContent.none);
                    EditorGUI.BeginDisabledGroup(isStringAssetAssigned);
                    bool createNewAsset = GUI.Button(new Rect(position.x + position.width - buttonWidth, position.y + yOffset, buttonWidth, EditorGUIUtility.singleLineHeight),
                        new GUIContent("New", "Create and assign a new String Asset."), EditorStyles.miniButton);
                    EditorGUI.EndDisabledGroup();
                    //--Pre-DLL support: if (createNewAsset) stringAssetProperty.objectReferenceValue = AssetUtility.CreateAsset<Wrappers.StringAsset>("String Asset", false);
                    if (createNewAsset)
                    {
                        var filename = EditorUtility.SaveFilePanelInProject("Create String Asset", ObjectNames.NicifyVariableName(property.name), "asset", "Save new string asset as:");
                        var type = System.Type.GetType("PixelCrushers.Wrappers.StringAsset, Assembly-CSharp-firstpass");
                        if (type == null) type = System.Type.GetType("PixelCrushers.Wrappers.StringAsset, Assembly-CSharp");
                        if (type == null)
                        {
                            Debug.LogError("Internal error: Unable to find wrapper type PixelCrushers.Wrappers.StringAsset. Please contact the developer.");
                        }
                        else
                        {
                            stringAssetProperty.objectReferenceValue = AssetUtility.CreateAssetWithFilename(type, filename, false);
                        }
                    }
                    yOffset += EditorGUIUtility.singleLineHeight;
                }

                if (isTextTableAssigned || !isContentAssigned)
                { 
                    float fieldWidth = isTextTableAssigned ? position.width / 2 : position.width;
                    EditorGUI.PropertyField(new Rect(position.x, position.y + yOffset, fieldWidth, EditorGUIUtility.singleLineHeight), textTableProperty, GUIContent.none);
                    if (isTextTableAssigned)
                    {
                        textTableFieldIDProperty.intValue = DrawTextTableFieldDropdown(new Rect(position.x + fieldWidth, position.y + yOffset, fieldWidth, EditorGUIUtility.singleLineHeight),
                            textTableProperty.objectReferenceValue as TextTable, textTableFieldIDProperty.intValue);
                    }
                }
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        private static int DrawTextTableFieldDropdown(Rect rect, TextTable textTable, int fieldID)
        {
            if (textTable == null || textTable.fields == null) return fieldID;
            var fieldNames = textTable.GetFieldNames();
            var fieldIDs = textTable.GetFieldIDs();
            int index = -1;
            for (int i = 0; i < fieldIDs.Length; i++)
            {
                if (fieldIDs[i] == fieldID)
                {
                    index = i;
                    break;
                }
            }
            var newIndex = EditorGUI.Popup(rect, index, fieldNames);
            return (newIndex != -1 && newIndex != index) ? fieldIDs[newIndex] : fieldID;
        }

        public static string GetStringFieldValue(SerializedProperty stringFieldProperty)
        {
            if (stringFieldProperty == null) return string.Empty;
            var textProperty = stringFieldProperty.FindPropertyRelative("m_text");
            if (textProperty != null && !string.IsNullOrEmpty(textProperty.stringValue))
            {
                return textProperty.stringValue;
            }
            var stringAssetProperty = stringFieldProperty.FindPropertyRelative("m_stringAsset");
            if (stringAssetProperty != null && stringAssetProperty.objectReferenceValue is StringAsset)
            {
                return (stringAssetProperty.objectReferenceValue as StringAsset).text;
            }
            var textTableProperty = stringFieldProperty.FindPropertyRelative("m_textTable");
            if (textTableProperty != null && textTableProperty.objectReferenceValue is TextTable)
            {
                var textTable = textTableProperty.objectReferenceValue as TextTable;
                var textTableFieldIDProperty = stringFieldProperty.FindPropertyRelative("m_textTableFieldID");
                return textTable.GetFieldTextForLanguage(textTableFieldIDProperty.intValue, 
                    (UILocalizationManager.instance != null) ? UILocalizationManager.instance.currentLanguage : string.Empty);
            }
            return string.Empty;
        }

        public static void SetStringFieldValue(SerializedProperty stringFieldProperty, string value)
        {
            if (stringFieldProperty == null) return;
            var textProperty = stringFieldProperty.FindPropertyRelative("m_text");
            if (textProperty != null) textProperty.stringValue = value;
            var stringAssetProperty = stringFieldProperty.FindPropertyRelative("m_stringAsset");
            if (stringAssetProperty != null) stringAssetProperty.objectReferenceValue = null;
            var textTableProperty = stringFieldProperty.FindPropertyRelative("m_textTable");
            if (textTableProperty != null) textTableProperty.objectReferenceValue = null;
        }


    }

}
