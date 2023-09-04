#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(PlaySequenceBehaviour))]
    public class PlaySequenceBehaviourDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 6 * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = new Rect(position.x, position.y, position.width, 5 * EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("sequence"));
        }
    }
}
#endif
#endif
