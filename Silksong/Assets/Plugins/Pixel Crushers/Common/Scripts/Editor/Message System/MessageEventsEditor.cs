// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomEditor(typeof(MessageEvents), true)]
    public class MessageEventsEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("In Messages To Listen For, add messages to listen for and the events that should occur when these messages are received.", MessageType.None);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_messagesToListenFor"), true);
            EditorGUILayout.HelpBox("In Messages To Send, configure messages that you want to send. To send them, call SendToMessageSystem(index) with the index of the element in Messages To Send.", MessageType.None);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_messagesToSend"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }
}