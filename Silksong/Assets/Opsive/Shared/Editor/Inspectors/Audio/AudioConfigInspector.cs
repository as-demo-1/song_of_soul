/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Audio
{
    using Opsive.Shared.Audio;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a PropertyDrawer for the FloatOverride.
    /// </summary>
    [CustomPropertyDrawer(typeof(FloatOverride))]
    public class FloatValueOptionDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the property inside the given rect.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label.
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't allow child fields to be indented.
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects.
            var option = new Rect(position.x, position.y, 90, position.height);
            var width = Mathf.CeilToInt((position.width - 90) / 2f);
            var constant1 = new Rect(position.x + 90 + 5, position.y, width - 5, position.height);
            var constant2 = new Rect(position.x + 90 + width + 5, position.y, width - 5, position.height);

            var valueOverride = (FloatOverride.Override)property.FindPropertyRelative("m_ValueOverride").enumValueIndex;

            // Draw fields - passs GUIContent.none to each so they are drawn without labels.
            EditorGUI.PropertyField(option, property.FindPropertyRelative("m_ValueOverride"), GUIContent.none);
            if (valueOverride != FloatOverride.Override.NoOverride) {
                EditorGUI.PropertyField(constant1, property.FindPropertyRelative("m_Constant1"), GUIContent.none);
            }
            if (valueOverride == FloatOverride.Override.Random) {
                EditorGUI.PropertyField(constant2, property.FindPropertyRelative("m_Constant2"), GUIContent.none);
            }

            // Set indent back to what it was.
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    /// <summary>
    /// Draws a PropertyDrawer for the BoolOverride.
    /// </summary>
    [CustomPropertyDrawer(typeof(BoolOverride))]
    public class BoolOptionDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the property inside the given rect.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label.
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't allow child fields to be indented.
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects.
            var option = new Rect(position.x, position.y, 90, position.height);
            var constant1 = new Rect(position.x + 90 + 5, position.y, position.width - 90 - 5, position.height);

            var valueOverride = (BoolOverride.Override)property.FindPropertyRelative("m_ValueOverride").enumValueIndex;

            // Draw fields - passs GUIContent.none to each so they are drawn without labels.
            EditorGUI.PropertyField(option, property.FindPropertyRelative("m_ValueOverride"), GUIContent.none);
            if (valueOverride == BoolOverride.Override.Constant) {
                EditorGUI.PropertyField(constant1, property.FindPropertyRelative("m_Value"), GUIContent.none);
            }

            // Set indent back to what it was.
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}