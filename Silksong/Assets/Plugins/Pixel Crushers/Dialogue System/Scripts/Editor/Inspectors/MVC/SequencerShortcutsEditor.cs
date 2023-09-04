// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(SequencerShortcuts), true)]
    public class SequencerShortcutsEditor : Editor
    {

        private ReorderableList shortcutsList = null;

        private void OnEnable()
        {
            shortcutsList = new ReorderableList(serializedObject, serializedObject.FindProperty("shortcuts"), true, true, true, true);
            shortcutsList.drawHeaderCallback = OnDrawListHeader;
            shortcutsList.drawElementCallback = OnDrawListElement;
            shortcutsList.elementHeight = 6 + 6 * EditorGUIUtility.singleLineHeight;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("A shortcut is an alias for one or more sequencer commands. If you include the shortcut in a sequence, wrapped in double-braces, it will be replaced by its shortcut value.", MessageType.None);
            serializedObject.Update();
            shortcutsList.DoLayoutList();
            EditorGUILayout.HelpBox("You can optionally assign GameObjects referenced by name in sequencer commands here. This prevents having to search for them at runtime.", MessageType.None);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("referencedSubjects"), true);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Shortcuts");
        }

        private const float LabelWidth = 56;

        private void OnDrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < shortcutsList.count)) return;
            var element = shortcutsList.serializedProperty.GetArrayElementAtIndex(index);
            var shortcutProperty = element.FindPropertyRelative("shortcut");
            var valueProperty = element.FindPropertyRelative("value");
            EditorGUI.LabelField(new Rect(rect.x, rect.y, LabelWidth, EditorGUIUtility.singleLineHeight), new GUIContent("Shortcut", "Shortcut keyword to use in sequences. Omit braces."));
            shortcutProperty.stringValue = EditorGUI.TextField(new Rect(rect.x + LabelWidth, rect.y, rect.width - LabelWidth, EditorGUIUtility.singleLineHeight), 
                GUIContent.none, shortcutProperty.stringValue);
            EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2,LabelWidth, EditorGUIUtility.singleLineHeight), new GUIContent("Value", "In sequences, replace keyword (wrapped in braces) with this."));
            valueProperty.stringValue = EditorGUI.TextArea(new Rect(rect.x + LabelWidth, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width - LabelWidth, 5 * EditorGUIUtility.singleLineHeight), valueProperty.stringValue);
        }

    }
}
