// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(BarkOnDialogueEvent), true)]
    public class BarkOnDialogueEventEditor : Editor
    {

        public void OnEnable()
        {
            var trigger = target as BarkOnDialogueEvent;
            if (trigger == null) return;
            trigger.selectedDatabase = trigger.selectedDatabase ?? EditorTools.FindInitialDatabase();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorWindowTools.DrawDeprecatedTriggerHelpBox();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedDatabase"), new GUIContent("Reference Database"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("barkOrder"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("once"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStart"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onEnd"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}
