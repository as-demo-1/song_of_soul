// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Database tab.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        #region Database Tab Variables

        [Serializable]
        private class DatabaseFoldouts
        {
            public bool database = true;
            public bool emphasisSettings = false;
            public List<bool> emphasisSetting = new List<bool>();
            public bool globalReplace = false;
            public bool merge = false;
            public bool export = false;
            public bool localization = false;
            public bool editorSettings = false;
        }

        [SerializeField]
        private string globalSearchText = string.Empty;
        [SerializeField]
        private string globalReplaceText = string.Empty;
        [SerializeField]
        private bool globalSearchSpecificConversation = false;
        [SerializeField]
        private int globalSearchConversationIndex = -1;
        [SerializeField]
        private bool globalSearchUseRegex = false;

        [SerializeField]
        private DatabaseFoldouts databaseFoldouts = new DatabaseFoldouts();
        [SerializeField]
        private DialogueDatabase databaseToMerge = null;
        [SerializeField]
        private DatabaseMerger.ConflictingIDRule conflictingIDRule = DatabaseMerger.ConflictingIDRule.ReplaceConflictingIDs;
        [SerializeField]
        private bool mergeProperties = true;
        [SerializeField]
        private bool mergeEmphases = true;
        [SerializeField]
        private bool mergeActors = true;
        [SerializeField]
        private bool mergeItems = true;
        [SerializeField]
        private bool mergeLocations = true;
        [SerializeField]
        private bool mergeVariables = true;
        [SerializeField]
        private bool mergeConversations = true;

        public enum ExportFormat { ChatMapperXML, JSON, CSV, VoiceoverScript, LanguageText, Screenplay };
        [SerializeField]
        public ExportFormat exportFormat = ExportFormat.ChatMapperXML;
        [SerializeField]
        public string chatMapperExportPath = string.Empty;
        [SerializeField]
        public string csvExportPath = string.Empty;
        [SerializeField]
        public string jsonExportPath = string.Empty;
        [SerializeField]
        public string voiceoverExportPath = string.Empty;
        [SerializeField]
        public string languageTextExportPath = string.Empty;
        [SerializeField]
        public string screenplayExportPath = string.Empty;
        [SerializeField]
        public static bool exportActors = true;
        [SerializeField]
        public static bool exportItems = true;
        [SerializeField]
        public static bool exportLocations = true;
        [SerializeField]
        public static bool exportVariables = true;
        [SerializeField]
        public static bool exportConversations = true;
        [SerializeField]
        public static bool exportCanvasRect = true;
        [SerializeField]
        public static bool exportConversationsAfterEntries = false;
        [SerializeField]
        public static bool omitNoneSequenceEntriesInScreenplay = false;
        [SerializeField]
        private EntrytagFormat entrytagFormat = EntrytagFormat.ActorName_ConversationID_EntryID;
        [SerializeField]
        private EncodingType encodingType = EncodingType.UTF8;

        private static GUIContent GlobalSearchLabel = new GUIContent("Search For:");
        private static GUIContent RegexSearchLabel = new GUIContent("Regex", "Use regular expressions in searches.");

        private Regex globalSearchRegex;

        private void ResetDatabaseTab()
        {
            ResetLocalizationFoldout();
        }

        #endregion

        #region Database Properties

        private void DrawDatabaseSection()
        {
            EditorGUILayout.LabelField(database.name, EditorStyles.boldLabel);
            databaseFoldouts.database = EditorGUILayout.Foldout(databaseFoldouts.database, new GUIContent("Database Properties"));
            if (databaseFoldouts.database) DrawDatabasePropertiesSection();
            databaseFoldouts.globalReplace = EditorGUILayout.Foldout(databaseFoldouts.globalReplace, new GUIContent("Global Search & Replace", "Globally search & replace text across entire database."));
            if (databaseFoldouts.globalReplace) DrawGlobalReplaceSection();
            databaseFoldouts.merge = EditorGUILayout.Foldout(databaseFoldouts.merge, new GUIContent("Merge Database", "Options to merge another database into this one."));
            if (databaseFoldouts.merge) DrawMergeSection();
            databaseFoldouts.export = EditorGUILayout.Foldout(databaseFoldouts.export, new GUIContent("Export Database", "Options to export the database to Chat Mapper XML format."));
            if (databaseFoldouts.export) DrawExportSection();
            databaseFoldouts.localization = EditorGUILayout.Foldout(databaseFoldouts.localization, new GUIContent("Localization Export/Import", "Options to export and import localization files."));
            if (databaseFoldouts.localization) DrawLocalizationSection();
            databaseFoldouts.editorSettings = EditorGUILayout.Foldout(databaseFoldouts.editorSettings, new GUIContent("Editor Settings", "Editor settings."));
            if (databaseFoldouts.editorSettings) DrawEditorSettings();
        }

        private void DrawDatabasePropertiesSection()
        {
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.BeginVertical(GroupBoxStyle);
            database.author = EditorGUILayout.TextField("Author", database.author);
            database.version = EditorGUILayout.TextField(new GUIContent("Version", "By default, this is the version of the Chat Mapper data model, but you can use it for your own purposes"), database.version);
            EditorGUILayout.LabelField("Description");
            database.description = EditorGUILayout.TextArea(database.description);
            EditorGUILayout.LabelField(new GUIContent("Global User Script", "Optional Lua code to run when this database is loaded at runtime."));
            database.globalUserScript = EditorGUILayout.TextArea(database.globalUserScript);
            databaseFoldouts.emphasisSettings = EditorGUILayout.Foldout(databaseFoldouts.emphasisSettings, new GUIContent("Emphasis Settings", "Settings to use for [em#] tags in dialogue text."));
            if (databaseFoldouts.emphasisSettings) DrawEmphasisSettings();
            database.baseID = EditorGUILayout.IntField(new GUIContent("Base ID", "Assign internal IDs to actors, variables, conversations, etc., starting from this base value. Useful when working with multiple databases."), database.baseID);
            EditorGUILayout.EndVertical();
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawEmphasisSettings()
        {
            EditorWindowTools.StartIndentedSection();
            var newLength = EditorGUILayout.IntField("Size", database.emphasisSettings.Length);
            newLength = Mathf.Max(DialogueDatabase.NumEmphasisSettings, newLength);
            if (newLength != database.emphasisSettings.Length)
            {
                var temp = new EmphasisSetting[newLength];
                for (int i = 0; i < newLength; i++)
                {
                    temp[i] = (i < database.emphasisSettings.Length) ? database.emphasisSettings[i] : new EmphasisSetting(Color.white, false, false, false);
                }
                database.emphasisSettings = temp;
            }
            for (int i = 0; i < database.emphasisSettings.Length; i++)
            {
                if (i >= databaseFoldouts.emphasisSetting.Count) databaseFoldouts.emphasisSetting.Add(false);
                databaseFoldouts.emphasisSetting[i] = EditorGUILayout.Foldout(databaseFoldouts.emphasisSetting[i], string.Format("Emphasis {0}", i + 1));
                if (databaseFoldouts.emphasisSetting[i]) DrawEmphasisSetting(database.emphasisSettings[i]);
            }
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawEmphasisSetting(EmphasisSetting setting)
        {
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.BeginVertical(GroupBoxStyle);
            setting.color = EditorGUILayout.ColorField("Color", setting.color);
            setting.bold = EditorGUILayout.Toggle("Bold", setting.bold);
            setting.italic = EditorGUILayout.Toggle("Italic", setting.italic);
            setting.underline = EditorGUILayout.Toggle("Underline", setting.underline);
            EditorGUILayout.EndVertical();
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawEditorSettings()
        {
            EditorWindowTools.StartIndentedSection();

            EditorGUI.BeginChangeCheck();
            var newFreq = EditorGUILayout.FloatField(new GUIContent("Auto Backup Frequency", "Seconds between auto backups. Set to zero for no auto backups."), autoBackupFrequency);
            if (newFreq != autoBackupFrequency)
            {
                autoBackupFrequency = newFreq;
                timeForNextAutoBackup = Time.realtimeSinceStartup + autoBackupFrequency;
            }
            EditorGUILayout.BeginHorizontal();
            autoBackupFolder = EditorGUILayout.TextField(new GUIContent("Auto Backup Folder", "Location to write auto backups."), autoBackupFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                var newAutoBackupFolder = EditorUtility.OpenFolderPanel("Auto Backup Folder", autoBackupFolder, string.Empty);
                if (!string.IsNullOrEmpty(newAutoBackupFolder))
                {
                    if (!newAutoBackupFolder.Contains(Application.dataPath))
                    {
                        EditorUtility.DisplayDialog("Auto Backup Folder", "Destination for backups must be in your Assets folder or a subfolder.", "OK");
                    }
                    else
                    {
                        autoBackupFolder = "Assets" + newAutoBackupFolder.Replace(Application.dataPath, string.Empty);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            showDatabaseName = EditorGUILayout.ToggleLeft(new GUIContent("Show Database Name", "Show the database name in the lower left of the editor window."), showDatabaseName);
            syncOnOpen = EditorGUILayout.ToggleLeft(new GUIContent("Sync On Open", "If any database sections are configured to sync content from another database, automatically sync when opening database."), syncOnOpen);
            registerCompleteObjectUndo = EditorGUILayout.ToggleLeft(new GUIContent("Fast Undo for Large Databases", "Use Undo.RegisterCompleteObjectUndo instead of Undo.RegisterUndo. Tick if operations such as deleting a conversation become slow in very large databases."), registerCompleteObjectUndo);
            debug = EditorGUILayout.ToggleLeft(new GUIContent("Debug", "For internal debugging of the dialogue editor."), debug);

            if (EditorGUI.EndChangeCheck())
            {
                SaveEditorSettings();
            }

            EditorWindowTools.EndIndentedSection();
        }

        #endregion

        #region Global Replace Section

        private void OpenGlobalSearchAndReplace()
        {
            toolbar.current = Toolbar.Tab.Database;
            databaseFoldouts.globalReplace = true;
        }

        private void DrawGlobalReplaceSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GlobalSearchLabel, GUILayout.Width(130));
            globalSearchUseRegex = EditorGUILayout.ToggleLeft(RegexSearchLabel, globalSearchUseRegex);
            EditorGUILayout.EndHorizontal();
            globalSearchText = EditorGUILayout.TextArea(globalSearchText);
            EditorGUILayout.LabelField("Replace With:");
            globalReplaceText = EditorGUILayout.TextArea(globalReplaceText);
            globalSearchSpecificConversation = EditorGUILayout.Toggle("Specific Conversation", globalSearchSpecificConversation);
            if (globalSearchSpecificConversation)
            {
                ValidateConversationMenuTitleIndex();
                if (conversationTitles == null) conversationTitles = GetConversationTitles();
                globalSearchConversationIndex = EditorGUILayout.Popup(globalSearchConversationIndex, conversationTitles, GUILayout.Height(30));
            }
            var ready = database != null && !string.IsNullOrEmpty(globalSearchText) &&
                (!globalSearchSpecificConversation || (0 <= globalSearchConversationIndex && globalSearchConversationIndex < conversationTitles.Length));
            EditorGUI.BeginDisabledGroup(!ready);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var logMatches = GUILayout.Button(new GUIContent("Search", "Log all matches to the Console window."), GUILayout.Width(120));
            var searchAndReplace = GUILayout.Button(new GUIContent("Search & Replace", "Interactively search and replace."), GUILayout.Width(120));
            var replaceAll = GUILayout.Button(new GUIContent("Replace All", "Replace all non-interactively."), GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;

            if (logMatches) LogGlobalSearchResults();
            if (searchAndReplace) RunGlobalSearchAndReplace(true);
            if (replaceAll && EditorUtility.DisplayDialog("Replace All", "Replace all instances of '" + globalSearchText +
                "' with '" + globalReplaceText + "' in " + (globalSearchSpecificConversation ? "selected conversation?" : "entire database?"), "Replace All", "Cancel")) RunGlobalSearchAndReplace(false);
        }

        private void LogGlobalSearchResults()
        {
            try
            {
                globalSearchRegex = new Regex(globalSearchText);

                var specificConversation = globalSearchSpecificConversation ? conversationTitles[globalSearchConversationIndex] : string.Empty;
                var result = globalSearchSpecificConversation ? "Conversation '" + specificConversation + "' matches for '" + globalSearchText + "': (click this log entry to see full report)"
                    : "Database matches for '" + globalSearchText + "': (click this log entry to see full report)";

                if (!globalSearchSpecificConversation && !string.IsNullOrEmpty(database.globalUserScript) && GlobalSearchMatch(database.globalUserScript))
                {
                    result += "\nGlobal User Script: " + database.globalUserScript;
                }

                if (!globalSearchSpecificConversation && !string.IsNullOrEmpty(database.description) && GlobalSearchMatch(database.description))
                {
                    result += "\nDescription: " + database.description;
                }

                float size = database.actors.Count + database.items.Count + database.locations.Count + database.variables.Count + database.conversations.Count;

                if (!globalSearchSpecificConversation)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Searching Database", "Searching actors for '" + globalSearchText + "'. Please wait...", 0)) return;
                    result += LogSearchResultsInAssetList<Actor>(database.actors, "Actor");
                    if (EditorUtility.DisplayCancelableProgressBar("Searching Database", "Searching quests/items for '" + globalSearchText + "'. Please wait...", database.actors.Count / size)) return;
                    result += LogSearchResultsInAssetList<Item>(database.items, "Quest/Item");
                    if (EditorUtility.DisplayCancelableProgressBar("Searching Database", "Searching locations for '" + globalSearchText + "'. Please wait...", (database.actors.Count + database.items.Count) / size)) return;
                    result += LogSearchResultsInAssetList<Location>(database.locations, "Location");
                    if (EditorUtility.DisplayCancelableProgressBar("Searching Database", "Searching variables for '" + globalSearchText + "'. Please wait...", (database.actors.Count + database.items.Count + database.locations.Count) / size)) return;
                    result += LogSearchResultsInAssetList<Variable>(database.variables, "Variable");
                }                

                int numConversationsDone = 0;
                foreach (var conversation in database.conversations)
                {
                    numConversationsDone++;
                    if (globalSearchSpecificConversation && !string.Equals(conversation.Title, specificConversation)) continue;
                    if (EditorUtility.DisplayCancelableProgressBar("Searching Database", "Searching conversation '" + conversation.Title + "' for '" + globalSearchText + "'. Please wait...", (database.actors.Count + database.items.Count + database.locations.Count + numConversationsDone) / size)) return;
                    foreach (var field in conversation.fields)
                    {
                        if (string.IsNullOrEmpty(field.title) || string.IsNullOrEmpty(field.value)) continue;
                        if (GlobalSearchMatch(field))
                        {
                            result += "\nConversation: '" + conversation.Title + "': Field '" + field.title + "': " + field.value;
                        }
                    }
                    foreach (var entry in conversation.dialogueEntries)
                    {
                        foreach (var field in entry.fields)
                        {
                            if (string.IsNullOrEmpty(field.title) || string.IsNullOrEmpty(field.value)) continue;
                            if (GlobalSearchMatch(field))
                            {
                                result += "\nConversation '" + conversation.Title + "' entry " + entry.id + ": Field '" + field.title + "': " + field.value;
                            }
                        }
                        if (!string.IsNullOrEmpty(entry.conditionsString) && GlobalSearchMatch(entry.conditionsString))
                        {
                            result += "\nConversation '" + conversation.Title + "' entry " + entry.id + ": Script: " + entry.conditionsString;
                        }
                        if (!string.IsNullOrEmpty(entry.userScript) && GlobalSearchMatch(entry.userScript))
                        {
                            result += "\nConversation '" + conversation.Title + "' entry " + entry.id + ": Script: " + entry.userScript;
                        }
                    }
                }

                Debug.Log(result);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private bool GlobalSearchMatch(Field field)
        {
            if (field == null) return false;
            return GlobalSearchMatch(field.title) || GlobalSearchMatch(field.value);
        }

        private bool GlobalSearchMatch(string s)
        {
            return globalSearchUseRegex ? globalSearchRegex.IsMatch(s) : s.Contains(globalSearchText);
        }

        private string LogSearchResultsInAssetList<T>(List<T> assets, string assetTypeName) where T : Asset
        {
            var result = string.Empty;
            foreach (var asset in assets)
            {
                foreach (var field in asset.fields)
                {
                    if (string.IsNullOrEmpty(field.title) || string.IsNullOrEmpty(field.value)) continue;
                    if (GlobalSearchMatch(field))
                    {
                        if (asset is Item)
                        {
                            result += ((asset as Item).IsItem ? "\nItem '" : "\nQuest '") + asset.Name + "': Field '" + field.title + "': " + field.value;
                        }
                        else
                        {
                            result += "\n" + assetTypeName + " '" + asset.Name + "': Field '" + field.title + "': " + field.value;
                        }
                    }
                }
            }
            return result;
        }

        private void RunGlobalSearchAndReplace(bool interactive)
        {
            int matches = 0;
            try
            {
                globalSearchRegex = new Regex(globalSearchText);

                var specificConversation = globalSearchSpecificConversation ? conversationTitles[globalSearchConversationIndex] : string.Empty;

                bool cancel = false;
                if (!globalSearchSpecificConversation && !string.IsNullOrEmpty(database.globalUserScript) && GlobalSearchMatch(database.globalUserScript))
                {
                    matches++;
                    var confirmed = !interactive || ConfirmReplacement("Global User Script:\n" + database.globalUserScript, out cancel);
                    if (cancel) return;
                    if (confirmed)
                    {
                        database.globalUserScript = database.globalUserScript.Replace(globalSearchText, globalReplaceText);
                    }
                }

                if (!globalSearchSpecificConversation && !string.IsNullOrEmpty(database.description) && GlobalSearchMatch(database.description))
                {
                    matches++;
                    var confirmed = !interactive || ConfirmReplacement("Description:\n" + database.description, out cancel);
                    if (cancel) return;
                    if (confirmed)
                    {
                        database.description = database.description.Replace(globalSearchText, globalReplaceText);
                    }
                }

                float size = database.actors.Count + database.items.Count + database.locations.Count + database.variables.Count + database.conversations.Count;

                if (!globalSearchSpecificConversation)
                {
                    if (!interactive && EditorUtility.DisplayCancelableProgressBar("Search & Replace", "Replacing '" + globalSearchText + "' with '" + globalReplaceText + "' in actors. Please wait...", 0)) return;
                    matches += RunGlobalSearchAndReplaceAssetList<Actor>(database.actors, interactive, out cancel);
                    if (cancel) return;
                    if (!interactive && EditorUtility.DisplayCancelableProgressBar("Search & Replace", "Replacing '" + globalSearchText + "' with '" + globalReplaceText + "' in quests/items. Please wait...", database.actors.Count / size)) return;
                    matches += RunGlobalSearchAndReplaceAssetList<Item>(database.items, interactive, out cancel);
                    if (cancel) return;
                    if (!interactive && EditorUtility.DisplayCancelableProgressBar("Search & Replace", "Replacing '" + globalSearchText + "' with '" + globalReplaceText + "' in locations. Please wait...", (database.actors.Count + database.items.Count) / size)) return;
                    matches += RunGlobalSearchAndReplaceAssetList<Location>(database.locations, interactive, out cancel);
                    if (cancel) return;
                    if (!interactive && EditorUtility.DisplayCancelableProgressBar("Search & Replace", "Replacing '" + globalSearchText + "' with '" + globalReplaceText + "' in variables. Please wait...", (database.actors.Count + database.items.Count + database.locations.Count) / size)) return;
                    matches += RunGlobalSearchAndReplaceAssetList<Variable>(database.variables, interactive, out cancel);
                    if (cancel) return;
                }

                int numConversationsDone = 0;
                foreach (var conversation in database.conversations)
                {
                    numConversationsDone++;
                    if (globalSearchSpecificConversation && !string.Equals(conversation.Title, specificConversation)) continue;
                    if (!interactive && EditorUtility.DisplayCancelableProgressBar("Search & Replace", "Replacing '" + globalSearchText + "' with '" + globalReplaceText + "' in conversation '" + conversation.Title + "'. Please wait...", (database.actors.Count + database.items.Count + database.locations.Count + database.variables.Count + numConversationsDone) / size)) return;
                    matches += RunGlobalSearchAndReplaceFieldList(conversation.fields, conversation, interactive, out cancel);
                    if (cancel) return;
                    foreach (var entry in conversation.dialogueEntries)
                    {
                        matches += RunGlobalSearchAndReplaceFieldList(entry.fields, null, interactive, out cancel);
                        if (cancel) return;
                        if (!string.IsNullOrEmpty(entry.conditionsString) && GlobalSearchMatch(entry.conditionsString))
                        {
                            matches++;
                            var confirmed = !interactive || ConfirmReplacement("Dialogue Entry Conditions:\n" + entry.conditionsString, out cancel);
                            if (cancel) return;
                            if (confirmed)
                            {
                                entry.conditionsString = entry.conditionsString.Replace(globalSearchText, globalReplaceText);
                            }
                        }
                        if (!string.IsNullOrEmpty(entry.userScript) && GlobalSearchMatch(entry.userScript))
                        {
                            matches++;
                            var confirmed = !interactive || ConfirmReplacement("Dialogue Entry Script:\n" + entry.userScript, out cancel);
                            if (cancel) return;
                            if (confirmed)
                            {
                                entry.userScript = entry.userScript.Replace(globalSearchText, globalReplaceText);
                            }
                        }
                    }
                }

            }
            finally
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Search & Replace", "Search and replace complete. " + matches + " matches found.", "OK");
                SetDatabaseDirty("Search and Replace");
                UpdateConversationTitles();
                ResetDialogueEntryText();
                Repaint();
            }
        }

        private bool ConfirmReplacement(string message, out bool cancel)
        {
            cancel = false;
            switch (EditorUtility.DisplayDialogComplex("Search & Replace", message, "Replace", "Skip", "Cancel"))
            {
                case 0:
                    return true;
                case 1:
                    return false;
                default:
                case 2:
                    cancel = true;
                    return false;
            }
        }

        private int RunGlobalSearchAndReplaceAssetList<T>(List<T> assets, bool interactive, out bool cancel) where T : Asset
        {
            int matches = 0;
            cancel = false;
            foreach (var asset in assets)
            {
                matches += RunGlobalSearchAndReplaceFieldList(asset.fields, asset, interactive, out cancel);
                if (cancel) return matches;
            }
            return matches;
        }

        private int RunGlobalSearchAndReplaceFieldList(List<Field> fields, Asset asset, bool interactive, out bool cancel)
        {
            int matches = 0;
            cancel = false;
            foreach (var field in fields)
            {
                if (GlobalSearchMatch(field))
                {
                    matches++;
                    var confirmed = true;
                    if (interactive)
                    {
                        var currentAssetName = (asset is Item) ? (((asset as Item).IsItem ? "\nItem '" : "\nQuest '") + asset.Name + "'")
                            : ((asset != null) ? asset.GetType().Name + " '" + ((asset is Conversation) ? (asset as Conversation).Title : asset.Name) + "'" : "Dialogue Entry");
                        confirmed = ConfirmReplacement(currentAssetName + "\n" + field.title + ": " + field.value, out cancel);
                        if (cancel) return matches;
                    }
                    if (confirmed)
                    {
                        field.title = field.title.Replace(globalSearchText, globalReplaceText);
                        field.value = field.value.Replace(globalSearchText, globalReplaceText);
                    }
                }
            }
            return matches;
        }

        #endregion

        #region Merge Section

        private void DrawMergeSection()
        {
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.BeginVertical(GroupBoxStyle);
            EditorGUILayout.HelpBox("Use this feature to add the contents of another database to this database.", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Database to Merge into " + database.name);
            databaseToMerge = EditorGUILayout.ObjectField(databaseToMerge, typeof(DialogueDatabase), false) as DialogueDatabase;
            EditorGUILayout.EndHorizontal();
            mergeProperties = EditorGUILayout.Toggle("Merge DB Properties", mergeProperties);
            if (mergeProperties)
            {
                mergeEmphases = EditorGUILayout.Toggle("  Merge [em#] Settings", mergeEmphases);
            }
            mergeActors = EditorGUILayout.Toggle("Merge Actors", mergeActors);
            mergeItems = EditorGUILayout.Toggle("Merge Items", mergeItems);
            mergeLocations = EditorGUILayout.Toggle("Merge Locations", mergeLocations);
            mergeVariables = EditorGUILayout.Toggle("Merge Variables", mergeVariables);
            mergeConversations = EditorGUILayout.Toggle("Merge Conversations", mergeConversations);
            EditorGUILayout.BeginHorizontal();
            conflictingIDRule = (DatabaseMerger.ConflictingIDRule)EditorGUILayout.EnumPopup(new GUIContent("If IDs Conflict", "Replace Existing IDs: If the same ID exists in both databases, replace the original one with the new one.\nAllow Conflicting IDs: Append assets even if IDs conflict.\nAssign Unique IDs: Append and assign new IDs to assets from source database."), conflictingIDRule, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(databaseToMerge == null);
            if (GUILayout.Button("Merge...", GUILayout.Width(100)))
            {
                if (ConfirmMerge()) MergeDatabase();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorWindowTools.EndIndentedSection();
        }

        private bool ConfirmMerge()
        {
            return EditorUtility.DisplayDialog("Confirm Merge", GetMergeWarning(), "Merge", "Cancel");
        }

        private string GetMergeWarning()
        {
            switch (conflictingIDRule)
            {
                case DatabaseMerger.ConflictingIDRule.ReplaceConflictingIDs:
                    return string.Format("If assets with the same IDs exist in both databases, the versions in {0} will replace the versions in this database. Continue to merge?", databaseToMerge.name);
                case DatabaseMerger.ConflictingIDRule.AllowConflictingIDs:
                    return string.Format("If IDs in {0} already exist in this database, you'll get overlaps that may break conversations. Continue to merge?", databaseToMerge.name);
                case PixelCrushers.DialogueSystem.DatabaseMerger.ConflictingIDRule.AssignUniqueIDs:
                    return string.Format("IDs in {0} will be automatically renumbered with different IDs than existing existing IDs in this database. Continue to merge?", databaseToMerge.name);
                default:
                    return string.Format("Internal error. Merge type {0} is unsupported. Please inform support@pixelcrushers.com.", conflictingIDRule);
            }
        }

        private void MergeDatabase()
        {
            if (databaseToMerge != null)
            {
                DatabaseMerger.Merge(database, databaseToMerge, conflictingIDRule, mergeProperties, mergeEmphases, mergeActors, mergeItems, mergeLocations, mergeVariables, mergeConversations);
                Debug.Log(string.Format("{0}: Merged contents of {1} into {2}.", DialogueDebug.Prefix, databaseToMerge.name, database.name));
                databaseToMerge = null;
                SetDatabaseDirty("Merge Database");
            }
        }

        #endregion

        #region Export Section

        private void DrawExportSection()
        {
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.BeginVertical(GroupBoxStyle);
            switch (exportFormat)
            {
                default:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external text-based formats.", MessageType.None);
                    break;
                case ExportFormat.CSV:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external text-based formats.\nYou can import CSV format into spreadsheet programs such as Excel and Google Sheets. To reimport into the Dialogue System, use Tools > Pixel Crushers > Dialogue System > Import > CSV.", MessageType.None);
                    break;
                case ExportFormat.JSON:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external JSON text-based format.\nTo reimport back into the Dialogue System, use Tools > Pixel Crushers > Dialogue System > Import > JSON.", MessageType.None);
                    break;
                case ExportFormat.ChatMapperXML:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external text-based formats.\nIf exporting to Chat Mapper format for import into Chat Mapper, you must also prepare a Chat Mapper template project that contains all the fields defined in this database. You can use the Dialogue System Chat Mapper template project as a base. To reimport into the Dialogue System, use Tools > Pixel Crushers > Dialogue System > Import > Chat Mapper.", MessageType.None);
                    break;
                case ExportFormat.LanguageText:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external text-based formats.\nThe Language Text format will export a file for each language containing all the localized text for the language. You can use these text dumps to determine which characters your language-specific fonts need to support.", MessageType.None);
                    break;
                case ExportFormat.VoiceoverScript:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external text-based formats.\nThe voiceover script option will export a separate CSV file for each language that you can use as a guide to record voice actors. Each row specifies the entrytag filename for use with entrytags.", MessageType.None);
                    break;
                case ExportFormat.Screenplay:
                    EditorGUILayout.HelpBox("Use this feature to export your database to external text-based formats.\nThe screenplay script option will export a separate text file for each language.", MessageType.None);
                    break;
            }
            if (exportFormat != ExportFormat.LanguageText && exportFormat != ExportFormat.Screenplay && exportFormat != ExportFormat.JSON)
            {
                exportActors = EditorGUILayout.Toggle("Export Actors", exportActors);
                exportItems = EditorGUILayout.Toggle("Export Items/Quests", exportItems);
                exportLocations = EditorGUILayout.Toggle("Export Locations", exportLocations);
                exportVariables = EditorGUILayout.Toggle("Export Variables", exportVariables);
                exportConversations = EditorGUILayout.Toggle("Export Conversations", exportConversations);
                if (exportFormat == ExportFormat.ChatMapperXML) exportCanvasRect = EditorGUILayout.Toggle(new GUIContent("Export Canvas Positions", "Export the positions of dialogue entry nodes in the Dialogue Editor's canvas"), exportCanvasRect);
                if (exportFormat == ExportFormat.CSV) exportConversationsAfterEntries = EditorGUILayout.Toggle(new GUIContent("Convs. After Entries", "Put the Conversations section after the DialogueEntries section in the CSV file. Normally the Conversations section is before."), exportConversationsAfterEntries);
                entrytagFormat = (EntrytagFormat)EditorGUILayout.EnumPopup("Entrytag Format", entrytagFormat, GUILayout.Width(400));
            }
            if (exportFormat == ExportFormat.Screenplay)
            {
                omitNoneSequenceEntriesInScreenplay = EditorGUILayout.Toggle(new GUIContent("Omit Hidden Lines", "Omit entries whose Sequence fields are None() or Continue()."), omitNoneSequenceEntriesInScreenplay);
            }
            encodingType = (EncodingType)EditorGUILayout.EnumPopup("Encoding", encodingType, GUILayout.Width(400));
            EditorGUILayout.BeginHorizontal();
            exportFormat = (ExportFormat)EditorGUILayout.EnumPopup("Format", exportFormat, GUILayout.Width(400));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Export...", GUILayout.Width(100)))
            {
                switch (exportFormat)
                {
                    case ExportFormat.ChatMapperXML:
                        TryExportToChatMapperXML();
                        break;
                    case ExportFormat.CSV:
                        TryExportToCSV();
                        break;
                    case ExportFormat.JSON:
                        TryExportToJSON();
                        break;
                    case ExportFormat.VoiceoverScript:
                        TryExportToVoiceoverScript();
                        break;
                    case ExportFormat.LanguageText:
                        TryExportToLanguageText();
                        break;
                    case ExportFormat.Screenplay:
                        TryExportToScreenplay();
                        break;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorWindowTools.EndIndentedSection();
        }

        private void TryExportToChatMapperXML()
        {
            string newChatMapperExportPath = EditorUtility.SaveFilePanel("Save Chat Mapper XML", EditorWindowTools.GetDirectoryName(chatMapperExportPath), chatMapperExportPath, "xml");
            if (!string.IsNullOrEmpty(newChatMapperExportPath))
            {
                chatMapperExportPath = newChatMapperExportPath;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    chatMapperExportPath = chatMapperExportPath.Replace("/", "\\");
                }
                if (exportCanvasRect) AddCanvasRectTemplateField();
                ValidateDatabase(database, false);
                ConfirmSyncAssetsAndTemplate();
                ChatMapperExporter.Export(database, chatMapperExportPath, exportActors, exportItems, exportLocations, exportVariables, exportConversations, exportCanvasRect);
                string templatePath = chatMapperExportPath.Replace(".xml", "_Template.txt");
                ExportTemplate(chatMapperExportPath, templatePath);
                EditorUtility.DisplayDialog("Export Complete", "The dialogue database was exported to Chat Mapper XML format.\n\n" +
                                            "Remember to apply a template after importing it into Chat Mapper. " +
                                            "The required fields in the template are listed in " + templatePath + ".\n\n" +
                                            "If you have any issues importing, please contact us at support@pixelcrushers.com.", "OK");
            }
        }

        private void AddCanvasRectTemplateField()
        {
            var field = template.dialogueEntryFields.Find(f => string.Equals(f.title, "canvasRect"));
            if (field == null)
            {
                template.dialogueEntryFields.Add(new Field("canvasRect", string.Empty, FieldType.Text));
            }
        }

        private void ExportTemplate(string chatMapperExportPath, string templatePath)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(templatePath))
                {
                    file.WriteLine("Required Chat Mapper Template Fields for: " + chatMapperExportPath);
                    file.WriteLine("\nACTORS (in this order):");
                    ExportTemplateFields(file, template.actorFields);
                    file.WriteLine("\nITEMS (in this order):");
                    ExportTemplateFields(file, template.itemFields);
                    file.WriteLine("\nLOCATIONS (in this order):");
                    ExportTemplateFields(file, template.locationFields);
                    file.WriteLine("\nUSER VARIABLES (in this order):");
                    ExportTemplateFields(file, template.variableFields);
                    file.WriteLine("\nCONVERSATIONS (in this order):");
                    ExportTemplateFields(file, template.conversationFields);
                    file.WriteLine("\nDIALOGUE NODES (in this order):");
                    ExportTemplateFields(file, template.dialogueEntryFields);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Error writing Chat Mapper template file {1}: {2}", DialogueDebug.Prefix, templatePath, e.Message));
            }
        }

        private void ExportTemplateFields(System.IO.StreamWriter file, List<Field> fields)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                Field field = fields[i];
                file.WriteLine(string.Format("[{0}] {1}: {2}", i + 1, field.title, field.type.ToString()));
            }
        }

        public void TryExportToCSV()
        {
            string newCSVExportPath = EditorUtility.SaveFilePanel("Save CSV", EditorWindowTools.GetDirectoryName(csvExportPath), csvExportPath, "csv");
            if (!string.IsNullOrEmpty(newCSVExportPath))
            {
                csvExportPath = newCSVExportPath;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    csvExportPath = csvExportPath.Replace("/", "\\");
                }
                CSVExporter.Export(database, csvExportPath, exportActors, exportItems, exportLocations, exportVariables, exportConversations, exportConversationsAfterEntries, entrytagFormat);
                EditorUtility.DisplayDialog("Export Complete", "The dialogue database was exported to CSV (comma-separated values) format. ", "OK");
            }
        }

        public void TryExportToJSON()
        {
            string newJSONExportPath = EditorUtility.SaveFilePanel("Save JSON", EditorWindowTools.GetDirectoryName(jsonExportPath), jsonExportPath, "json");
            if (!string.IsNullOrEmpty(newJSONExportPath))
            {
                jsonExportPath = newJSONExportPath;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    jsonExportPath = csvExportPath.Replace("/", "\\");
                }
                System.IO.File.WriteAllText(newJSONExportPath, JsonUtility.ToJson(database), System.Text.Encoding.UTF8);
                EditorUtility.DisplayDialog("Export Complete", "The dialogue database was exported to JSON format. ", "OK");
            }
        }

        public void TryExportToVoiceoverScript()
        {
            string newVoiceoverPath = EditorUtility.SaveFilePanel("Save Voiceover Scripts", EditorWindowTools.GetDirectoryName(voiceoverExportPath), voiceoverExportPath, "csv");
            if (!string.IsNullOrEmpty(newVoiceoverPath))
            {
                voiceoverExportPath = newVoiceoverPath;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    voiceoverExportPath = voiceoverExportPath.Replace("/", "\\");
                }
                VoiceoverScriptExporter.Export(database, voiceoverExportPath, exportActors, entrytagFormat, encodingType);
                EditorUtility.DisplayDialog("Export Complete", "The voiceover scripts were exported to CSV (comma-separated values) files in " + voiceoverExportPath + ".", "OK");
            }
        }

        public void TryExportToScreenplay()
        {
            string newScreenplayPath = EditorUtility.SaveFilePanel("Save Screenplays", EditorWindowTools.GetDirectoryName(screenplayExportPath), voiceoverExportPath, "txt");
            if (!string.IsNullOrEmpty(newScreenplayPath))
            {
                screenplayExportPath = newScreenplayPath;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    screenplayExportPath = screenplayExportPath.Replace("/", "\\");
                }
                ScreenplayExporter.Export(database, screenplayExportPath, encodingType, omitNoneSequenceEntriesInScreenplay);
                EditorUtility.DisplayDialog("Export Complete", "The screenplay files were exported to " + screenplayExportPath + ".", "OK");
            }
        }

        public void TryExportToLanguageText()
        {
            string newLanguageTextPath = EditorUtility.SaveFilePanel("Save Language Text", EditorWindowTools.GetDirectoryName(languageTextExportPath), languageTextExportPath, "txt");
            if (!string.IsNullOrEmpty(newLanguageTextPath))
            {
                languageTextExportPath = newLanguageTextPath;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    languageTextExportPath = languageTextExportPath.Replace("/", "\\");
                }
                LanguageTextExporter.Export(database, languageTextExportPath, encodingType);
                EditorUtility.DisplayDialog("Export Complete", "The language texts have been exported to " + languageTextExportPath + " with the language code appended to the end of each filename. ", "OK");
            }
        }

        #endregion

        #region No Database Section

        private void DrawNoDatabaseSection()
        {
            EditorGUILayout.BeginHorizontal();
            var database = EditorGUILayout.ObjectField("Select dialogue database", null, typeof(DialogueDatabase), false);
            if (GUILayout.Button("Create New", GUILayout.Width(120))) CreateNewDatabase();
            EditorGUILayout.EndHorizontal();
            if (database != null) SelectObject(database);
        }

        private void CreateNewDatabase()
        {
            DialogueSystemMenuItems.CreateDialogueDatabase();
        }

        #endregion

        #region Validate Database

        // This method will probably move to a more appropriate location in a future version:
        private void ValidateDatabase(DialogueDatabase database, bool debug)
        {
            if (database == null || database.actors == null || database.conversations == null) return;
            if (database.actors.Count < 1) database.actors.Add(template.CreateActor(1, "Player", true));

            // Keeps track of nodes that are already connected (link's IsConnector=false):
            HashSet<string> alreadyConnected = new HashSet<string>();

            // Check each conversation:
            foreach (Conversation conversation in database.conversations)
            {

                // Check item and location fields:
                foreach (Field field in conversation.fields)
                {
                    if (field.type == FieldType.Location)
                    {
                        if (database.GetLocation(Tools.StringToInt(field.value)) == null)
                        {
                            if (debug) Debug.Log(string.Format("Fixing location field '{0}' for conversation {1}", field.title, conversation.id));
                            if (database.locations.Count < 1) database.locations.Add(template.CreateLocation(1, "Nowhere"));
                            field.value = database.locations[0].id.ToString();
                        }
                    }
                    else if (field.type == FieldType.Item)
                    {
                        if (database.GetItem(Tools.StringToInt(field.value)) == null)
                        {
                            if (debug) Debug.Log(string.Format("Fixing item field '{0}' for conversation {1}", field.title, conversation.id));
                            if (database.items.Count < 1) database.items.Add(template.CreateItem(1, "No Item"));
                            field.value = database.items[0].id.ToString();
                        }
                    }
                }

                // Get valid actor IDs for conversation:
                int conversationActorID = (database.GetActor(conversation.ActorID) != null) ? conversation.ActorID : database.actors[0].id;
                if (conversationActorID != conversation.ActorID)
                {
                    if (debug) Debug.Log(string.Format("Fixing actor ID for conversation {0}", conversation.id));
                    conversation.ActorID = conversationActorID;
                }
                int conversationConversantID = (database.GetActor(conversation.ConversantID) != null) ? conversation.ConversantID : database.actors[0].id;
                if (conversationConversantID != conversation.ConversantID)
                {
                    if (debug) Debug.Log(string.Format("Fixing conversant ID for conversation {0}", conversation.id));
                    conversation.ConversantID = conversationConversantID;
                }

                // Check all dialogue entries:
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {

                    // Make sure actor IDs are valid:
                    if (database.GetActor(entry.ActorID) == null)
                    {
                        if (debug) Debug.Log(string.Format("Fixing actor ID for conversation {0}, entry {1}: actor ID {2}-->{3}", conversation.id, entry.id, entry.ActorID, ((entry.ConversantID == conversationConversantID) ? conversationActorID : conversationConversantID)));
                        entry.ActorID = (entry.ConversantID == conversationConversantID) ? conversationActorID : conversationConversantID;
                    }
                    if (database.GetActor(entry.ConversantID) == null)
                    {
                        if (debug) Debug.Log(string.Format("Fixing conversant ID for conversation {0}, entry {1}: conversant ID {2}-->{3}", conversation.id, entry.id, entry.ConversantID, ((entry.ActorID == conversationConversantID) ? conversationActorID : conversationConversantID)));
                        entry.ConversantID = (entry.ActorID == conversationConversantID) ? conversationActorID : conversationConversantID;
                    }

                    // Make sure all outgoing links' origins point to this conversation and entry, and set as connector:
                    foreach (Link link in entry.outgoingLinks)
                    {
                        if (link.originConversationID != conversation.id)
                        {
                            if (debug) Debug.Log(string.Format("Fixing link.originConversationID convID={0}, entryID={1}", conversation.id, entry.id));
                            link.originConversationID = conversation.id;
                        }
                        if (link.originDialogueID != entry.id)
                        {
                            if (debug) Debug.Log(string.Format("Fixing link.originDialogueID convID={0}, entryID={1}", conversation.id, entry.id));
                            link.originDialogueID = entry.id;
                        }
                        link.isConnector = true;
                    }
                }

                // Traverse tree, assigning non-connector to the first occurrence in the tree:
                AssignConnectors(conversation.GetFirstDialogueEntry(), conversation, alreadyConnected, new HashSet<int>(), 0);
            }
        }

        private void AssignConnectors(DialogueEntry entry, Conversation conversation, HashSet<string> alreadyConnected, HashSet<int> entriesVisited, int level)
        {
            // Sanity check to prevent infinite recursion:
            if (level > 10000) return;

            // Set non-connectors:
            foreach (Link link in entry.outgoingLinks)
            {
                if (link.originConversationID == link.destinationConversationID)
                {
                    string destination = string.Format("{0}.{1}", link.destinationConversationID, link.destinationDialogueID);
                    if (alreadyConnected.Contains(destination))
                    {
                        link.isConnector = true;
                    }
                    else
                    {
                        link.isConnector = false;
                        alreadyConnected.Add(destination);
                    }
                }
            }

            // Then process each child:
            foreach (Link link in entry.outgoingLinks)
            {
                if (link.originConversationID == link.destinationConversationID)
                {
                    if (!entriesVisited.Contains(link.destinationDialogueID))
                    {
                        entriesVisited.Add(link.destinationDialogueID);
                        var childEntry = conversation.GetDialogueEntry(link.destinationDialogueID);
                        if (childEntry != null)
                        {
                            AssignConnectors(childEntry, conversation, alreadyConnected, entriesVisited, level + 1);
                        }
                    }
                }
            }
        }

        #endregion

    }

}
