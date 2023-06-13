#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(ShowAlertBehaviour))]
    public class ShowAlertBehaviourDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int fieldCount = 2;
            return fieldCount * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(singleFieldRect, property.FindPropertyRelative("message"));
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, property.FindPropertyRelative("useTextLengthForDuration"));
        }
    }
}
#endif
#endif
