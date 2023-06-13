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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (showReferenceDatabase ? EditorGUIUtility.singleLineHeight : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Set up property drawer:
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            // Show database field if specified:
            showReferenceDatabase = (attribute as ConversationPopupAttribute).showReferenceDatabase;
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

            // Set up titles array:
            if (titles == null || titlesDatabase != EditorTools.selectedDatabase) UpdateTitles(prop.stringValue);

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

        public void UpdateTitles(string currentConversation)
        {
            titlesDatabase = EditorTools.selectedDatabase;
            var foundCurrent = false;
            var list = new List<string>();
            if (titlesDatabase != null && titlesDatabase.conversations != null)
            {
                for (int i = 0; i < titlesDatabase.conversations.Count; i++)
                {
                    var title = titlesDatabase.conversations[i].Title;
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
