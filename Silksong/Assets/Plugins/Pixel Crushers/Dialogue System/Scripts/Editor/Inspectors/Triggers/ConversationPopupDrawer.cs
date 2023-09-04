// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(ConversationPopupAttribute))]
    public class ConversationPopupDrawer : PropertyDrawer
    {

        private DialogueDatabase titlesDatabase = null;
        private string[] titles = null;
        private bool showReferenceDatabase = false;
        private bool usePicker = true;
        private bool showFilter = false;
        private string filter = string.Empty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) +
                (showReferenceDatabase ? EditorGUIUtility.singleLineHeight : 0) +
                (showFilter ? EditorGUIUtility.singleLineHeight : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Set up property drawer:
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            // Show database field if specified:
            showReferenceDatabase = (attribute as ConversationPopupAttribute).showReferenceDatabase;
            showFilter = (attribute as ConversationPopupAttribute).showFilter;
            if (EditorTools.selectedDatabase == null) EditorTools.SetInitialDatabaseIfNull();
            if (showReferenceDatabase)
            {
                var dbPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight);
                var newDatabase = EditorGUI.ObjectField(dbPosition, EditorTools.selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
                if (newDatabase != EditorTools.selectedDatabase)
                {
                    EditorTools.selectedDatabase = newDatabase;
                    titlesDatabase = null;
                    titles = null;
                }
            }
            if (EditorTools.selectedDatabase == null) usePicker = false;

            // Filter:
            if (showFilter)
            {
                var filterLabelWidth = 48;
                EditorGUI.LabelField(new Rect(position.x, position.y, position.width - filterLabelWidth, EditorGUIUtility.singleLineHeight), "Filter:");
                EditorGUI.BeginChangeCheck();
                filter = EditorGUI.TextField(new Rect(position.x + filterLabelWidth, position.y, position.width - filterLabelWidth, EditorGUIUtility.singleLineHeight), filter);
                if (EditorGUI.EndChangeCheck())
                {
                    titles = null; // Need to update filtered title list.
                }
                position.y += EditorGUIUtility.singleLineHeight;
                position.height -= EditorGUIUtility.singleLineHeight;
            }

            // Set up titles array:
            if (titles == null || titlesDatabase != EditorTools.selectedDatabase) UpdateTitles(prop.stringValue, filter);

            // Update current index:
            var currentIndex = GetIndex(prop.stringValue);

            // Draw popup or plain text field:
            var rect = new Rect(position.x, position.y, position.width - 48, position.height);
            if (usePicker)
            {
                var newIndex = EditorGUI.Popup(rect, currentIndex, titles);
                if ((newIndex != currentIndex) && (0 <= newIndex && newIndex < titles.Length))
                {
                    currentIndex = newIndex;
                    prop.stringValue = titles[currentIndex];
                    GUI.changed = true;
                }
                if (GUI.Button(new Rect(position.x + position.width - 45, position.y, 18, 14), "x"))
                {
                    prop.stringValue = string.Empty;
                    currentIndex = -1;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                string value = EditorGUI.TextField(rect, prop.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.stringValue = value;
                }
            }

            // Radio button toggle between popup and plain text field:
            rect = new Rect(position.x + position.width - 22, position.y, 22, position.height);
            var newToggleValue = EditorGUI.Toggle(rect, usePicker, EditorStyles.radioButton);
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (EditorTools.selectedDatabase == null)) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
                titles = null;
            }

            EditorGUI.EndProperty();
        }

        public void UpdateTitles(string currentConversation, string filter)
        {
            titlesDatabase = EditorTools.selectedDatabase;
            var foundCurrent = false;
            var list = new List<string>();
            var lowercaseFilter = string.IsNullOrEmpty(filter) ? string.Empty : filter.ToLower();
            if (titlesDatabase != null && titlesDatabase.conversations != null)
            {
                for (int i = 0; i < titlesDatabase.conversations.Count; i++)
                {
                    var title = titlesDatabase.conversations[i].Title;
                    if (!string.IsNullOrEmpty(filter) && !title.ToLower().Contains(filter)) continue;
                    list.Add(title);
                    if (string.Equals(currentConversation, title))
                    {
                        foundCurrent = true;
                    }
                }
                if (!foundCurrent && !string.IsNullOrEmpty(currentConversation))
                {
                    list.Add(currentConversation);
                }
            }
            titles = list.ToArray();
        }

        public int GetIndex(string currentConversation)
        {
            for (int i = 0; i < titles.Length; i++)
            {
                if (string.Equals(currentConversation, titles[i])) return i;
            }
            return -1;
        }

    }

}
