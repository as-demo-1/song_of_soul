// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(PersistentActiveData), true)]
    public class PersistentActiveDataEditor : Editor
    {

        public void OnEnable()
        {
            EditorTools.SetInitialDatabaseIfNull();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"), true);
            EditorTools.DrawReferenceDatabase();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("condition"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("checkOnStart"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}
