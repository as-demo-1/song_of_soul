// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(QuestCondition))]
    public class QuestConditionDrawer : PropertyDrawer
    {

        private string currentQuestName;
        private string[] questEntryNames = null;
        private bool useEntryPicker = true;

        private void OnEnable()
        {
            currentQuestName = string.Empty;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 2 * EditorGUIUtility.singleLineHeight;
            if (property.FindPropertyRelative("checkQuestEntry").boolValue)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects:
            float halfWidth = position.width / 2;
            float questStateWidth = Mathf.Min(halfWidth, 120f);
            float questNameWidth = position.width - questStateWidth - 2;
            Rect questNameRect = new Rect(position.x, position.y, questNameWidth, EditorGUIUtility.singleLineHeight); //position.height);
            Rect questStateRect = new Rect(questNameRect.x + questNameRect.width + 2, position.y, questStateWidth, EditorGUIUtility.singleLineHeight); //position.height);
            Rect checkQuestEntryRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

            EditorTools.SetInitialDatabaseIfNull();

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            var questName = property.FindPropertyRelative("questName");
            if (EditorTools.selectedDatabase == null)
            {
                EditorGUI.PropertyField(questNameRect, questName, GUIContent.none);
            }
            else
            {
                int questNameIndex;
                string[] questNames = GetQuestNames(questName.stringValue, out questNameIndex);
                int newQuestNameIndex = EditorGUI.Popup(questNameRect, questNameIndex, questNames);
                if (newQuestNameIndex != questNameIndex)
                {
                    questName.stringValue = GetQuestName(questNames, newQuestNameIndex);
                }
            }

            var questState = property.FindPropertyRelative("questState");
            EditorGUI.PropertyField(questStateRect, questState, GUIContent.none, false);

            var checkQuestEntry = property.FindPropertyRelative("checkQuestEntry");
            checkQuestEntry.boolValue = EditorGUI.ToggleLeft(checkQuestEntryRect, "Check Quest Entry State", checkQuestEntry.boolValue);
            if (checkQuestEntry.boolValue)
            {
                Rect questEntryRect = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, questNameWidth, EditorGUIUtility.singleLineHeight);
                Rect questEntryStateRect = new Rect(questNameRect.x + questNameRect.width + 2, position.y + 2 * EditorGUIUtility.singleLineHeight, questStateWidth, EditorGUIUtility.singleLineHeight);
                var questEntryNumber = property.FindPropertyRelative("entryNumber");

                if (questName.stringValue != currentQuestName)
                {
                    currentQuestName = questName.stringValue;
                    questEntryNames = null;
                }
                if (questEntryNames == null) UpdateQuestEntryNames(currentQuestName);

                // Update current index:
                var currentIndex = questEntryNumber.intValue;

                // Draw popup or plain text field:
                var rect = new Rect(position.x, position.y, position.width - 30, position.height);
                if (useEntryPicker)
                {
                    var newIndex = EditorGUI.Popup(questEntryRect, currentIndex, questEntryNames);
                    if ((newIndex != currentIndex) && (0 <= newIndex && newIndex < questEntryNames.Length))
                    {
                        questEntryNumber.intValue = newIndex;
                        GUI.changed = true;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(rect, questEntryNumber, GUIContent.none, true);
                }

                // Radio button toggle between popup and plain text field:
                rect = new Rect(position.x + position.width - 22, position.y, 22, position.height);
                var newToggleValue = EditorGUI.Toggle(questEntryRect, useEntryPicker, EditorStyles.radioButton);
                if (newToggleValue != useEntryPicker)
                {
                    useEntryPicker = newToggleValue;
                    if (useEntryPicker && (EditorTools.selectedDatabase == null)) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
                    questEntryNames = null;
                }
                //---Was: EditorGUI.PropertyField(questEntryRect, property.FindPropertyRelative("entryNumber"), GUIContent.none);

                EditorGUI.PropertyField(questEntryStateRect, property.FindPropertyRelative("questEntryState"), GUIContent.none);
            }

            EditorGUI.EndProperty();
        }

        private string[] GetQuestNames(string currentQuestName, out int questNameIndex)
        {
            questNameIndex = -1;
            var database = EditorTools.selectedDatabase;
            if (database == null || database.items == null)
            {
                return new string[0];
            }
            else
            {
                List<string> questNames = new List<string>();
                foreach (var item in database.items)
                {
                    if (!item.IsItem)
                    {
                        string questName = item.Name;
                        if (string.Equals(currentQuestName, questName))
                        {
                            questNameIndex = questNames.Count;
                        }
                        questNames.Add(questName);
                    }
                }
                return questNames.ToArray();
            }
        }

        private string GetQuestName(string[] questNames, int questNameIndex)
        {
            return (0 <= questNameIndex && questNameIndex < questNames.Length) ? questNames[questNameIndex] : string.Empty;
        }

        private void UpdateQuestEntryNames(string questName)
        {
            var database = EditorTools.selectedDatabase;
            if (database == null)
            {
                questEntryNames = new string[0];
                return;
            }
            var quest = database.items.Find(x => x.Name == questName);
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
            questEntryNames = list.ToArray();
        }

    }

}
