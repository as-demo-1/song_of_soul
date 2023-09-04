// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomPropertyDrawer(typeof(UIDropdownField), true)]
    public class UIDropdownFieldDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                var uiDropdownProperty = property.FindPropertyRelative("m_uiDropdown");
                var tmpDropdownProperty = property.FindPropertyRelative("m_tmpDropdown");
                var isUIDropdownAssigned = (uiDropdownProperty != null) && (uiDropdownProperty.objectReferenceValue != null);
                var isTMPDropdownAssigned = (tmpDropdownProperty != null) && (tmpDropdownProperty.objectReferenceValue != null);
                var isContentAssigned = isUIDropdownAssigned || isTMPDropdownAssigned;
                int numUnassignedLines = 1;
                if (tmpDropdownProperty != null) numUnassignedLines++;
                return (isContentAssigned ? 1 : numUnassignedLines) * EditorGUIUtility.singleLineHeight;
            }
            catch (System.ArgumentException) // Handles IMGUI->UITK bug in Unity 2022.2.
            {
                return 2 * EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                var uiDropdownProperty = property.FindPropertyRelative("m_uiDropdown");
                var tmpDropdownProperty = property.FindPropertyRelative("m_tmpDropdown");
                if (uiDropdownProperty == null)
                {
                    Debug.LogError("Sorry! There was an internal editor error with a UI Dropdown Field. Please contact Pixel Crushers for support.");
                    return;
                }
                var isUIDropdownAssigned = (uiDropdownProperty != null) && (uiDropdownProperty.objectReferenceValue != null);
                var isTMPDropdownAssigned = (tmpDropdownProperty != null) && (tmpDropdownProperty.objectReferenceValue != null);
                var isContentAssigned = isUIDropdownAssigned || isTMPDropdownAssigned;

                float yOffset = 0;

                if (isUIDropdownAssigned || !isContentAssigned)
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), uiDropdownProperty, GUIContent.none);
                    yOffset += EditorGUIUtility.singleLineHeight;
                }

                if (isTMPDropdownAssigned || (tmpDropdownProperty != null && !isContentAssigned))
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), tmpDropdownProperty, GUIContent.none);
                }
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

    }
}
