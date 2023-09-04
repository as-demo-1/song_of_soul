// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Database tab's Stats section.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        public class DatabaseStats
        {
            public bool isValid = false;

            public int numActors;
            public int numQuests;
            public int numVariables;
            public int numConversations;
            public int numDialogueEntries;
            public int numDialogueEntriesNonBlank;
            public int numSceneEvents;

            public int questWordCount;
            public int conversationWordCount;
            public int totalWordCount;

        }

        private DatabaseStats stats = new DatabaseStats();

        private void DrawStatsSection()
        {
            EditorWindowTools.StartIndentedSection();

            EditorGUI.BeginDisabledGroup(database == null);
            if (GUILayout.Button("Update Stats"))
            {
                UpdateStats();
            }
            EditorGUI.EndDisabledGroup();
            if (stats.isValid)
            {
                EditorGUILayout.LabelField("Asset Count", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Actors", stats.numActors);
                EditorGUILayout.IntField("Quests", stats.numQuests);
                EditorGUILayout.IntField("Variables", stats.numVariables);
                EditorGUILayout.IntField("Conversations", stats.numConversations);
                EditorGUILayout.IntField("Dialogue Entries", stats.numDialogueEntries);
                EditorGUILayout.IntField("Entries non-blank", stats.numDialogueEntriesNonBlank);
                EditorGUILayout.IntField("Scene Events", stats.numSceneEvents);
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.LabelField("Word Count", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Quests", stats.questWordCount);
                EditorGUILayout.IntField("Conversations", stats.conversationWordCount);
                EditorGUILayout.IntField("Total", stats.totalWordCount);
                EditorGUI.EndDisabledGroup();
            }
            EditorWindowTools.EndIndentedSection();
        }

        private void UpdateStats()
        {
            if (database == null) return;
            stats.isValid = true;

            try
            {
                stats.numDialogueEntries = stats.numDialogueEntriesNonBlank = stats.numSceneEvents = 0;
                stats.questWordCount = stats.conversationWordCount = 0;

                EditorUtility.DisplayProgressBar("Computing Stats", "Actors", 0);
                stats.numActors = database.actors.Count;

                EditorUtility.DisplayProgressBar("Computing Stats", "Quests", 1);
                foreach (var quest in database.items)
                {
                    if (quest.IsItem) continue;
                    stats.numQuests++;
                    foreach (var field in quest.fields)
                    {
                        if (!(field.type == FieldType.Text || field.type == FieldType.Localization)) continue;
                        stats.questWordCount += GetWordCount(field.value);
                    }
                }

                EditorUtility.DisplayProgressBar("Computing Stats", "Variables", 2);
                stats.numVariables = database.variables.Count;

                stats.numConversations = database.conversations.Count;
                for (int i = 0; i < stats.numConversations; i++)
                {
                    var progress = (float)i / (float)stats.numConversations;
                    EditorUtility.DisplayProgressBar("Computing Stats", "Conversations", progress);
                    var conversation = database.conversations[i];
                    foreach (var entry in conversation.dialogueEntries)
                    {
                        stats.numDialogueEntries++;
                        var menuText = entry.MenuText;
                        var dialogueText = entry.DialogueText;
                        if (!(string.IsNullOrEmpty(menuText) && string.IsNullOrEmpty(dialogueText)))
                        {
                            stats.numDialogueEntriesNonBlank++;
                        }
                        stats.conversationWordCount += GetWordCount(menuText) + GetWordCount(dialogueText);
                        foreach (var field in entry.fields)
                        {
                            if (field.type == FieldType.Localization)
                            {
                                stats.conversationWordCount += GetWordCount(field.value);
                            }
                        }
                        if (!string.IsNullOrEmpty(entry.sceneEventGuid))
                        {
                            stats.numSceneEvents++;
                        }
                    }
                }
                stats.totalWordCount = stats.questWordCount + stats.conversationWordCount;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static char[] wordDelimiters = new char[] { ' ', '\r', '\n' };

        private int GetWordCount(string s)
        {
            return s.Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }

    }
}
