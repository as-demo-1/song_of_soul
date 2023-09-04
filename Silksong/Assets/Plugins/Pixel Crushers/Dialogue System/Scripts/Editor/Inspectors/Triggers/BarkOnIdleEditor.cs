// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(BarkOnIdle), true)]
    public class BarkOnIdleEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var trigger = target as BarkOnIdle;
            if (trigger == null) return;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversation"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("barkOrder"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversant"), new GUIContent("Barker", "The actor speaking the bark. If unassigned, this GameObject."), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minSeconds"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSeconds"), true);
            //--- Removed: EditorGUILayout.PropertyField(serializedObject.FindProperty("once"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skipIfNoValidEntries"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowDuringConversations"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cacheBarkLines"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("condition"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}
