// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(QuestEntryPopupAttribute))]
    public class QuestEntryPopupDrawer : PropertyDrawer
    {

        private DialogueDatabase database = null;
        private string[] names = null;
        private bool usePicker = true;
        private static string currentQuestName;

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Set up property drawer:
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Show database field if specified:
            if (EditorTools.selectedDatabase == null) EditorTools.SetInitialDatabaseIfNull();

            // Set up titles array:
            var questProp = FindQuestProperty(prop);
            if (questProp != null && questProp.stringValue != currentQuestName)
            {
                currentQuestName = questProp.stringValue;
                names = null;
            }
            if (names == null) UpdateNames(questProp, prop);

            // Update current index:
            var currentIndex = prop.intValue;

            // Draw popup or plain text field:
            var rect = new Rect(position.x, position.y, position.width - 30, position.height);
            if (usePicker)
            {
                var newIndex = EditorGUI.Popup(rect, currentIndex, names);
                if ((newIndex != currentIndex) && (0 <= newIndex && newIndex < names.Length))
                {
                    prop.intValue = newIndex;
                    GUI.changed = true;
                }
            }
            else
            {
                EditorGUI.PropertyField(rect, prop, GUIContent.none, true);
            }

            // Radio button toggle between popup and plain text field:
            rect = new Rect(position.x + position.width - 22, position.y, 22, position.height);
            var newToggleValue = EditorGUI.Toggle(rect, usePicker, EditorStyles.radioButton);
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (EditorTools.selectedDatabase == null)) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
                names = null;
            }

            EditorGUI.EndProperty();
        }

        private SerializedProperty FindQuestProperty(SerializedProperty prop)
        {
            return prop.serializedObject.FindProperty("quest") ??
                prop.serializedObject.FindProperty("questName") ??
                prop.serializedObject.FindProperty("questTitle") ??
                prop.serializedObject.FindProperty("Quest") ??
                prop.serializedObject.FindProperty("QuestName") ??
                prop.serializedObject.FindProperty("QuestTitle");
        }

        public void UpdateNames(SerializedProperty questProp, SerializedProperty prop)
        {
            database = EditorTools.selectedDatabase;
            var quest = (questProp == null || database == null || database.items == null) ? null
                : database.items.Find(x => x.Name == questProp.stringValue);
            var list = new List<string>();
            if (quest != null)
            {
                list.Add("(None)");
                var entryCount = quest.LookupInt("Entry Count");
                for (int i = 1; i <= entryCount; i++)
                {
                    list.Add(i + ". " + quest.LookupValue("Entry " + i));
                }
            }
            names = list.ToArray();
        }

    }

}
