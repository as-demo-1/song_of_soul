// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomEditor(typeof(TimedEvent), true)]
    public class TimedEventEditor : Editor
    {
        private SerializedProperty modeProperty;
        private SerializedProperty durationProperty;
        private SerializedProperty framesProperty;
        private SerializedProperty activateOnStartProperty;
        private SerializedProperty onTimeReachedProperty;

        private void OnEnable()
        {
            modeProperty = serializedObject.FindProperty("m_mode");
            durationProperty = serializedObject.FindProperty("m_duration");
            framesProperty = serializedObject.FindProperty("m_frames");
            activateOnStartProperty = serializedObject.FindProperty("m_activateOnStart");
            onTimeReachedProperty = serializedObject.FindProperty("m_onTimeReached");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(modeProperty);
            if (modeProperty.enumValueIndex == (int)TimedEvent.TimingMode.Frames)
            {
                EditorGUILayout.PropertyField(framesProperty);
            }
            else
            {
                EditorGUILayout.PropertyField(durationProperty);
            }
            EditorGUILayout.PropertyField(activateOnStartProperty);
            EditorGUILayout.PropertyField(onTimeReachedProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
