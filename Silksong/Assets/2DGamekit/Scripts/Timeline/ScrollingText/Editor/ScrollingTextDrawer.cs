using UnityEditor;
using UnityEngine;

namespace Gamekit2D
{
    [CustomPropertyDrawer(typeof(ScrollingTextBehaviour))]
    public class ScrollingTextDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            int fieldCount = 3;
            return fieldCount * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty messageProp = property.FindPropertyRelative("message");
            SerializedProperty startDelayProp = property.FindPropertyRelative ("startDelay");
            SerializedProperty holdDelayProp = property.FindPropertyRelative("holdDelay");

            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(singleFieldRect, messageProp);

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, startDelayProp);

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, holdDelayProp);
        }
    }
}