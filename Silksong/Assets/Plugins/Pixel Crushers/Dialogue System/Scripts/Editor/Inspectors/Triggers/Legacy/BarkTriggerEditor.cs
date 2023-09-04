// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(BarkTrigger), true)]
    public class BarkTriggerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorWindowTools.DrawDeprecatedTriggerHelpBox();
            var trigger = target as BarkTrigger;
            if (trigger == null) return;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversation"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("barkOrder"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversant"), new GUIContent("Barker", "The actor speaking the bark. If unassigned, this GameObject."), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("once"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skipIfNoValidEntries"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowDuringConversations"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cacheBarkLines"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("condition"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}
