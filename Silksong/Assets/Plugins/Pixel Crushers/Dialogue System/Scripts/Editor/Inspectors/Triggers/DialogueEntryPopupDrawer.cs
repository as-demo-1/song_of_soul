// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(DialogueEntryPopupAttribute))]
    public class DialogueEntryPopupDrawer : PropertyDrawer
    {

        private DialogueDatabase database = null;
        private string[] entryTexts = null;
        private Dictionary<int, int> idToIndex = new Dictionary<int, int>();
        private Dictionary<int, int> indexToId = new Dictionary<int, int>();
        private bool usePicker = true;
        private static string currentConversationTitle;

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Set up property drawer:
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Show database field if specified:
            if (EditorTools.selectedDatabase == null) EditorTools.SetInitialDatabaseIfNull();

            // Set up titles array:
            var conversationProp = FindConversationProperty(prop);
            if (conversationProp != null && conversationProp.stringValue != currentConversationTitle)
            {
                currentConversationTitle = conversationProp.stringValue;
                entryTexts = null;
            }
            if (entryTexts == null) UpdateDialogueEntryList(conversationProp, prop);

            // Update current index:
            var currentIndex = GetIndex(prop.intValue);

            // Draw popup or plain text field:
            var rect = new Rect(position.x, position.y, position.width - 30, position.height);
            if (usePicker)
            {
                var newIndex = EditorGUI.Popup(rect, currentIndex, entryTexts);
                if ((newIndex != currentIndex) && (0 <= newIndex && newIndex < entryTexts.Length))
                {
                    prop.intValue = GetID(newIndex);
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
                entryTexts = null;
            }

            EditorGUI.EndProperty();
        }

        private SerializedProperty FindConversationProperty(SerializedProperty prop)
        {
            return prop.serializedObject.FindProperty("conversation") ??
                prop.serializedObject.FindProperty("conversationName") ??
                prop.serializedObject.FindProperty("conversationTitle") ??
                prop.serializedObject.FindProperty("Conversation") ??
                prop.serializedObject.FindProperty("ConversationName") ??
                prop.serializedObject.FindProperty("ConversationTitle") ??
                prop.serializedObject.FindProperty("m_conversation") ??
                prop.serializedObject.FindProperty("m_conversationName") ??
                prop.serializedObject.FindProperty("m_conversationTitle") ??
                prop.serializedObject.FindProperty("m_Conversation") ??
                prop.serializedObject.FindProperty("m_ConversationName") ??
                prop.serializedObject.FindProperty("m_ConversationTitle") ??
                prop.serializedObject.FindProperty("_conversation") ??
                prop.serializedObject.FindProperty("_conversationName") ??
                prop.serializedObject.FindProperty("_conversationTitle") ??
                prop.serializedObject.FindProperty("_Conversation") ??
                prop.serializedObject.FindProperty("_ConversationName") ??
                prop.serializedObject.FindProperty("_ConversationTitle");
        }

        public void UpdateDialogueEntryList(SerializedProperty conversationProp, SerializedProperty prop)
        {
            database = EditorTools.selectedDatabase;
            var conversation = (conversationProp == null || database == null || database.items == null) ? null
                : database.conversations.Find(x => x.Title == conversationProp.stringValue);
            var list = new List<string>();
            if (conversation != null)
            {
                list.Add("(None)");
                idToIndex = new Dictionary<int, int>();
                indexToId = new Dictionary<int, int>();
                for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                {
                    var entry = conversation.dialogueEntries[i];
                    list.Add("[" + entry.id + "] " + ((entry.id == 0) ? "<START>" : entry.subtitleText).Replace("/", "\u2215"));
                    idToIndex.Add(entry.id, i + 1);
                    indexToId.Add(i + 1, entry.id);
                }
            }
            entryTexts = list.ToArray();
        }

        private int GetID(int index)
        {
            return indexToId.ContainsKey(index) ? indexToId[index] : -1;
        }

        private int GetIndex(int id)
        {
            return idToIndex.ContainsKey(id) ? idToIndex[id] : -1;
        }

    }

}
