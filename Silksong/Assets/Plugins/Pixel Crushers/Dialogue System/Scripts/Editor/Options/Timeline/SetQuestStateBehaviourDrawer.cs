#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(SetQuestStateBehaviour))]
    public class SetQuestStateBehaviourDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int fieldCount = 6;
            return fieldCount * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PixelCrushers.DialogueSystem.EditorTools.SetInitialDatabaseIfNull();

            SerializedProperty questProp = property.FindPropertyRelative("quest");
            SerializedProperty setQuestStateProp = property.FindPropertyRelative("setQuestState");
            SerializedProperty questStateProp = property.FindPropertyRelative("questState");
            SerializedProperty setQuestEntryStateProp = property.FindPropertyRelative("setQuestEntryState");
            SerializedProperty questEntryNumberProp = property.FindPropertyRelative("questEntryNumber");
            SerializedProperty questEntryStateProp = property.FindPropertyRelative("questEntryState");

            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(singleFieldRect, questProp);

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, setQuestStateProp);

            if (setQuestStateProp.boolValue)
            {
                singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(singleFieldRect, questStateProp);
            }

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, setQuestEntryStateProp);

            if (setQuestEntryStateProp.boolValue)
            {
                singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(singleFieldRect, questEntryNumberProp);

                singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(singleFieldRect, questEntryStateProp);
            }
        }
    }
}
#endif
#endif
