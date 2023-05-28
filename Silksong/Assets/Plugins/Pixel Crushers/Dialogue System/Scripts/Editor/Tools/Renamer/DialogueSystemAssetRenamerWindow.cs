// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Renames Dialogue System database assets (e.g., variables) across databases, prefabs,
    /// and asset files. Uses by DialogueSystemAssetRenamerUtility to actually rename the assets.
    /// </summary>
    public class DialogueSystemAssetRenamerWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Asset Renamer", false, 4)]
        public static void Open()
        {
            var window = GetWindow<DialogueSystemAssetRenamerWindow>(false, "Renamer");
            window.minSize = new Vector2(400, 500);
        }

        #region Variables

        private const string AssetRenamerDatabasePrefsKey = "PixelCrushers.DialogueSystem.AssetRenamer";
        private const string AssetRenamerIgnoreRegexPrefsKey = "PixelCrushers.DialogueSystem.AssetRenamer.Ignore";

        protected Vector2 scrollPosition = Vector2.zero;

        protected DialogueDatabase database;

        // Variables:
        protected int variableIndex;
        protected string replacementVariableName;
        protected string[] variableNames;

        // Actors:
        protected int actorIndex;
        protected string replacementActorName;
        protected string[] actorNames;

        // Quests:
        protected int questIndex;
        protected string replacementQuestName;
        protected string[] questNames;

        // Quests:
        protected int conversationIndex;
        protected string replacementConversationTitle;
        protected string[] conversationTitles;

        protected string report;
        protected GUIStyle reportStyle = null;

        protected virtual string DefaultIgnoreFilesRegex { get { return string.Empty; } }
        protected static GUIContent IgnoreFilesRegexLabel = new GUIContent("Ignore Files (Regex)", "Matches partial filenames; separate names with pipes (|).");

        #endregion

        #region Entrypoints

        protected virtual void OnEnable()
        {
            if (EditorPrefs.HasKey(AssetRenamerDatabasePrefsKey))
            {
                var guid = EditorPrefs.GetString(AssetRenamerDatabasePrefsKey);
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                    RefreshAssetCaches();
                }
            }
            DialogueSystemAssetRenamerUtility.ignoreFilesRegex = EditorPrefs.GetString(AssetRenamerIgnoreRegexPrefsKey, DefaultIgnoreFilesRegex);
        }

        protected virtual void OnDisable()
        {
            if (database != null)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(database));
                EditorPrefs.SetString(AssetRenamerDatabasePrefsKey, guid);
            }
            EditorPrefs.SetString(AssetRenamerIgnoreRegexPrefsKey, DialogueSystemAssetRenamerUtility.ignoreFilesRegex);
        }

        protected virtual void OnGUI()
        {
            try
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawDatabaseSelection();
                DrawIgnoreFilesRegex();
                DrawRenameVariableSection();
                DrawRenameActorsSection();
                DrawRenameQuestsSection();
                DrawRenameConversationsSection();
                DrawReport();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        #endregion

        #region Database

        protected virtual void DrawDatabaseSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            database = EditorGUILayout.ObjectField(database, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (EditorGUI.EndChangeCheck()) RefreshAssetCaches();
            EditorGUI.BeginDisabledGroup(database == null);
            if (GUILayout.Button("Refresh", GUILayout.Width(64))) RefreshAssetCaches();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void RefreshAssetCaches()
        {
            MakeVariableNamesArray();
            MakeActorNamesArray();
            MakeQuestNamesArray();
            MakeConversationTitlesArray();
        }

        #endregion

        #region Ignore Files Regex

        protected virtual void DrawIgnoreFilesRegex()
        {
            DialogueSystemAssetRenamerUtility.ignoreFilesRegex = EditorGUILayout.TextField(IgnoreFilesRegexLabel, DialogueSystemAssetRenamerUtility.ignoreFilesRegex);
        }

        #endregion

        #region Variables

        protected virtual void DrawRenameVariableSection()
        {
            if (database == null) return;
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
            variableIndex = EditorGUILayout.Popup("Variable", variableIndex, variableNames);
            replacementVariableName = EditorGUILayout.TextField("Rename To", replacementVariableName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Find Variable",
                    "Find '" + GetSelectedVariableName() + "' in all databases, scenes, and prefabs?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindVariable(false);
                }
            }
            EditorGUI.BeginDisabledGroup(!IsVariableSelected() || string.IsNullOrEmpty(replacementVariableName));
            if (GUILayout.Button("Rename", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Rename Variable",
                    "Rename '" + GetSelectedVariableName() + "' to '" + replacementVariableName + "'?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindVariable(true);
                    MakeVariableNamesArray();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void MakeVariableNamesArray()
        {
            var list = new List<string>();
            if (database != null)
            {
                for (int i = 0; i < database.variables.Count; i++)
                {
                    list.Add(database.variables[i].Name);
                }
            }
            variableNames = list.ToArray();
        }

        protected virtual bool IsVariableSelected()
        {
            return database != null && 0 <= variableIndex && variableIndex < database.variables.Count;
        }

        protected virtual string GetSelectedVariableName()
        {
            return IsVariableSelected() ? database.variables[variableIndex].Name : string.Empty;
        }

        protected virtual string FindVariable(bool replace)
        {
            return DialogueSystemAssetRenamerUtility.FindVariable(GetSelectedVariableName(), replace ? replacementVariableName.Trim() : string.Empty);
        }

        #endregion

        #region Actors

        protected virtual void DrawRenameActorsSection()
        {
            if (database == null) return;
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Actors", EditorStyles.boldLabel);
            actorIndex = EditorGUILayout.Popup("Actor", actorIndex, actorNames);
            replacementActorName = EditorGUILayout.TextField("Rename To", replacementActorName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Find Actor",
                    "Find '" + GetSelectedActorName() + "' in all databases, scenes, and prefabs?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindActor(false);
                }
            }
            EditorGUI.BeginDisabledGroup(!IsActorSelected() || string.IsNullOrEmpty(replacementActorName));
            if (GUILayout.Button("Rename", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Rename Actor",
                    "Rename '" + GetSelectedActorName() + "' to '" + replacementActorName + "'?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindActor(true);
                    MakeActorNamesArray();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void MakeActorNamesArray()
        {
            var list = new List<string>();
            if (database != null)
            {
                for (int i = 0; i < database.actors.Count; i++)
                {
                    list.Add(database.actors[i].Name);
                }
            }
            actorNames = list.ToArray();
        }

        protected virtual bool IsActorSelected()
        {
            return database != null && 0 <= actorIndex && actorIndex < database.actors.Count;
        }

        protected virtual string GetSelectedActorName()
        {
            return IsActorSelected() ? database.actors[actorIndex].Name : string.Empty;
        }

        protected virtual string FindActor(bool replace)
        {
            return DialogueSystemAssetRenamerUtility.FindActor(GetSelectedActorName(), replace ? replacementActorName.Trim() : string.Empty);
        }

        #endregion

        #region Quests

        protected virtual void DrawRenameQuestsSection()
        {
            if (database == null) return;
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Quests", EditorStyles.boldLabel);
            questIndex = EditorGUILayout.Popup("Quest", questIndex, questNames);
            replacementQuestName = EditorGUILayout.TextField("Rename To", replacementQuestName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Find Quest",
                    "Find '" + GetSelectedQuestName() + "' in all databases, scenes, and prefabs?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindQuest(false);
                }
            }
            EditorGUI.BeginDisabledGroup(!IsQuestSelected() || string.IsNullOrEmpty(replacementQuestName));
            if (GUILayout.Button("Rename", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Rename Quest",
                    "Rename '" + GetSelectedQuestName() + "' to '" + replacementQuestName + "'?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindQuest(true);
                    MakeQuestNamesArray();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void MakeQuestNamesArray()
        {
            var list = new List<string>();
            if (database != null)
            {
                for (int i = 0; i < database.items.Count; i++)
                {
                    list.Add(database.items[i].Name);
                }
            }
            questNames = list.ToArray();
        }

        protected virtual bool IsQuestSelected()
        {
            return database != null && 0 <= questIndex && questIndex < database.items.Count;
        }

        protected virtual string GetSelectedQuestName()
        {
            return IsQuestSelected() ? database.items[questIndex].Name : string.Empty;
        }

        protected virtual string FindQuest(bool replace)
        {
            return DialogueSystemAssetRenamerUtility.FindQuest(GetSelectedQuestName(), replace ? replacementQuestName.Trim() : string.Empty);
        }

        #endregion

        #region Conversations

        protected virtual void DrawRenameConversationsSection()
        {
            if (database == null) return;
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Conversations", EditorStyles.boldLabel);
            conversationIndex = EditorGUILayout.Popup("Conversation", conversationIndex, conversationTitles);
            replacementConversationTitle = EditorGUILayout.TextField("Rename To", replacementConversationTitle);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Find Conversation",
                    "Find '" + GetSelectedConversationTitle() + "' in all databases, scenes, and prefabs?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindConversation(false);
                }
            }
            EditorGUI.BeginDisabledGroup(!IsConversationSelected() || string.IsNullOrEmpty(replacementConversationTitle));
            if (GUILayout.Button("Rename", GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog("Rename Conversation",
                    "Rename '" + GetSelectedConversationTitle() + "' to '" + replacementConversationTitle + "'?\nThis may take a long time.", "OK", "Cancel"))
                {
                    report = FindConversation(true);
                    MakeConversationTitlesArray();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void MakeConversationTitlesArray()
        {
            var list = new List<string>();
            if (database != null)
            {
                for (int i = 0; i < database.conversations.Count; i++)
                {
                    list.Add(database.conversations[i].Title);
                }
            }
            conversationTitles = list.ToArray();
        }

        protected virtual bool IsConversationSelected()
        {
            return database != null && 0 <= conversationIndex && conversationIndex < database.conversations.Count;
        }

        protected virtual string GetSelectedConversationTitle()
        {
            return IsConversationSelected() ? database.conversations[conversationIndex].Title : string.Empty;
        }

        protected virtual string FindConversation(bool replace)
        {
            return DialogueSystemAssetRenamerUtility.FindConversation(GetSelectedConversationTitle(), replace ? replacementConversationTitle.Trim() : string.Empty);
        }

        #endregion

        #region Report

        protected virtual void DrawReport()
        {
            if (string.IsNullOrEmpty(report)) return;
            if (reportStyle == null || reportStyle.normal.textColor != GUI.skin.textArea.normal.textColor)
            {
                reportStyle = new GUIStyle(GUI.skin.textArea);
                reportStyle.wordWrap = true;
            }
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Report", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(report));
            if (GUILayout.Button("Copy", GUILayout.Width(64)))
            {
                GUIUtility.systemCopyBuffer = report;
            }
            if (GUILayout.Button("Clear", GUILayout.Width(64)))
            {
                report = string.Empty;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(report, reportStyle, GUILayout.ExpandHeight(true));
            EditorGUI.EndDisabledGroup();
        }

        #endregion

    }

}
