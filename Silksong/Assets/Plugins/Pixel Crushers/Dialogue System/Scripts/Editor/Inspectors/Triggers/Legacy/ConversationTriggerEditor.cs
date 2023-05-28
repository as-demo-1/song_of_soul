// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(ConversationTrigger), true)]
    public class ConversationTriggerEditor : Editor
    {

        private void OnEnable()
        {
            EditorTools.selectedDatabase = (target as ConversationTrigger).selectedDatabase;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorWindowTools.DrawDeprecatedTriggerHelpBox();
            var trigger = target as ConversationTrigger;
            if (trigger == null) return;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversation"), true);
            var dbProperty = serializedObject.FindProperty("selectedDatabase");
            if (dbProperty.objectReferenceValue != EditorTools.selectedDatabase) dbProperty.objectReferenceValue = EditorTools.selectedDatabase;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actor"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversant"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("once"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exclusive"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skipIfNoValidEntries"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stopConversationOnTriggerExit"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("condition"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}
