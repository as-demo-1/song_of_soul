// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    public class ConversationPicker
    {

        public DialogueDatabase database = null;
        public string currentConversation = string.Empty;
        public bool usePicker = false;

        private string[] titles = null;
        private int currentIndex = -1;

        private DialogueDatabase initialDatabase = null;

        public ConversationPicker(DialogueDatabase database, string currentConversation, bool usePicker)
        {
            initialDatabase = EditorTools.FindInitialDatabase();
            this.database = database ?? initialDatabase;
            this.currentConversation = currentConversation;
            this.usePicker = usePicker;
            UpdateTitles();
            bool currentConversationIsInDatabase = (database != null) || (currentIndex >= 0);
            if (usePicker && !string.IsNullOrEmpty(currentConversation) && !currentConversationIsInDatabase)
            {
                this.usePicker = false;
            }
        }

        public void UpdateTitles()
        {
            currentIndex = -1;
            if (database == null || database.conversations == null)
            {
                titles = new string[0];
            }
            else
            {
                titles = new string[database.conversations.Count];
                for (int i = 0; i < database.conversations.Count; i++)
                {
                    titles[i] = database.conversations[i].Title;
                    if (string.Equals(currentConversation, titles[i]))
                    {
                        currentIndex = i;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the conversation picker.
        /// </summary>
        /// <param name="showReferenceDatabase">Show a field where the user can specify a dialogue database.</param>
        /// <returns>True if the conversation selection changed; otherwise false. The conversation is in the public string `conversation`.</returns>
		public bool Draw(bool showReferenceDatabase = true)
        {
            bool changed = false;
            EditorGUILayout.BeginHorizontal();
            if (usePicker)
            {
                if (showReferenceDatabase)
                {
                    var newDatabase = EditorGUILayout.ObjectField("Reference Database", database, typeof(DialogueDatabase), false) as DialogueDatabase;
                    if (newDatabase != database)
                    {
                        database = newDatabase;
                        UpdateTitles();
                        changed = true;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Conversation Picker");
                }
            }
            else
            {
                var newConversation = EditorGUILayout.TextField("Conversation", currentConversation);
                if (newConversation != currentConversation)
                {
                    changed = true;
                    currentConversation = newConversation;
                }
            }
            var newToggleValue = EditorGUILayout.Toggle(usePicker, EditorStyles.radioButton, GUILayout.Width(20));
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (database == null)) database = EditorTools.FindInitialDatabase();
                UpdateTitles();
                changed = true;
            }
            EditorGUILayout.EndHorizontal();
            if (usePicker)
            {
                EditorGUILayout.BeginHorizontal();
                var newIndex = EditorGUILayout.Popup("Conversation", currentIndex, titles);
                if ((newIndex != currentIndex) && (0 <= newIndex) && (newIndex < titles.Length))
                {
                    changed = true;
                    currentIndex = newIndex;
                    currentConversation = titles[currentIndex];
                    changed = true;
                }
                if (database != initialDatabase && database != null && initialDatabase != null)
                {
                    EditorGUILayout.HelpBox("The Dialogue Manager's Initial Database is " + initialDatabase.name +
                                            ". Make sure to load " + this.database.name +
                                            " before using this conversation. You can use the Extra Databases component to load additional databases.",
                                            MessageType.Info);
                }
                if (DrawClearButton()) changed = true;
                EditorGUILayout.EndHorizontal();
            }
            return changed;
        }

        private bool DrawClearButton()
        {
            if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(14)))
            {
                currentConversation = string.Empty;
                currentIndex = -1;
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
