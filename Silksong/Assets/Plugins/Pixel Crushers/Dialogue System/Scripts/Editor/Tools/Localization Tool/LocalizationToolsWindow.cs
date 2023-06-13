// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections.Generic;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Extra localization tools for dialogue databases.
    /// </summary>
    public class LocalizationToolsWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Localization Tools", false, 3)]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(LocalizationToolsWindow), false, "Loc Tools");
        }

        protected LocalizationToolsWindowPrefs prefs = null;
        protected bool verbose = false;
        protected bool dryRun = false;
        protected ReorderableList databaseList;
        protected string report;

        protected Vector2 scrollPosition = Vector2.zero;
        protected GUIContent dryRunLabel = new GUIContent("Dry Run", "Log changes that would be made but don't actually make the changes.");
        protected GUIContent mainLocalizationLanguageLabel = new GUIContent("Main Language", "Localization language to copy default text into.");
        protected GUIContent copyButtonLabel = null;
        protected float copyButtonWidth = 100;

        protected virtual void OnEnable()
        {
            minSize = new Vector2(340, 128);
            prefs = LocalizationToolsWindowPrefs.Load();
            databaseList = new ReorderableList(prefs.databases, typeof(DialogueDatabase), true, true, true, true);
            databaseList.drawHeaderCallback += OnDrawDatabaseListHeader;
            databaseList.drawElementCallback += OnDrawDatabaseListElement;
            databaseList.onAddCallback += OnAddDatabaseListElement;
        }

        protected virtual void OnDisable()
        {
            prefs.Save();
        }

        protected virtual void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            try
            {
                databaseList.DoLayoutList();
                EditorGUI.BeginChangeCheck();
                prefs.mainLocalizationLanguage = EditorGUILayout.TextField(mainLocalizationLanguageLabel, prefs.mainLocalizationLanguage);
                if (EditorGUI.EndChangeCheck()) copyButtonLabel = null;
                DrawButtonSection();
                DrawExtraContent();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        protected virtual void DrawExtraContent()
        {
            // For subclassing.
        }

        protected void OnDrawDatabaseListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Databases");
            if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 48, rect.height), "All", EditorStyles.miniButtonLeft))
            {
                if (EditorUtility.DisplayDialog("Add All Databases in Project",
                                                string.Format("Do you want to find and add all dialogue databases in the entire project?", EditorWindowTools.GetCurrentDirectory()),
                                                "Ok", "Cancel"))
                {
                    AddAllDatabasesInProject();
                }
            }
            if (GUI.Button(new Rect(rect.x + rect.width - 50, rect.y, 48, rect.height), "Folder", EditorStyles.miniButtonLeft))
            {
                if (EditorUtility.DisplayDialog("Add All Databases in Folder",
                                                string.Format("Do you want to find and add all dialogue databases in the folder {0}?", EditorWindowTools.GetCurrentDirectory()),
                                                "Ok", "Cancel"))
                {
                    AddAllDatabasesInFolder(EditorWindowTools.GetCurrentDirectory(), false);
                }
            }
        }

        protected void OnDrawDatabaseListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < prefs.databases.Count)) return;
            prefs.databases[index] = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), GUIContent.none, prefs.databases[index], typeof(DialogueDatabase), false) as DialogueDatabase;
        }


        protected void OnAddDatabaseListElement(ReorderableList list)
        {
            prefs.databases.Add(null);
        }

        protected void AddAllDatabasesInProject()
        {
            prefs.databases.Clear();
            AddAllDatabasesInFolder("Assets", true);
            prefs.Save();
        }

        protected void AddAllDatabasesInFolder(string folderPath, bool recursive)
        {
            string[] filePaths = Directory.GetFiles(folderPath, "*.asset", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (string filePath in filePaths)
            {
                string assetPath = filePath.Replace("\\", "/");
                DialogueDatabase database = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueDatabase)) as DialogueDatabase;
                if ((database != null) && (!prefs.databases.Contains(database)))
                {
                    prefs.databases.Add(database);
                }
            }
            prefs.Save();
        }

        protected virtual void DrawButtonSection()
        {
            verbose = EditorGUILayout.Toggle("Verbose Logging", verbose);
            dryRun = EditorGUILayout.Toggle(dryRunLabel, dryRun);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(100)))
            {
                prefs.Clear();
            }
            if (copyButtonLabel == null)
            {
                copyButtonLabel = new GUIContent("Copy to " + prefs.mainLocalizationLanguage, "Copy default text (such as Dialogue Text) to " + prefs.mainLocalizationLanguage + " localization fields.");
                copyButtonWidth = Mathf.Max(100, GUI.skin.button.CalcSize(new GUIContent(copyButtonLabel)).x);
            }
            EditorGUI.BeginDisabledGroup(prefs.databases.Count == 0 || string.IsNullOrEmpty(prefs.mainLocalizationLanguage));
            if (GUILayout.Button(copyButtonLabel, GUILayout.Width(copyButtonWidth)))
            {
                CopyToMainLocalizationLanguage();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void CopyToMainLocalizationLanguage()
        {
            report = "Results: (also copied to clipboard)\n";
            try
            {
                for (int i = 0; i < prefs.databases.Count; i++)
                {
                    var progress = (i + 1) / prefs.databases.Count;
                    var database = prefs.databases[i];
                    EditorUtility.DisplayProgressBar("Copying To " + prefs.mainLocalizationLanguage, database.name, progress);
                    CopyToMainLocalizationLanguageInDatabase(database);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                report += "ERROR: " + e.Message;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Debug.Log(report);
                GUIUtility.systemCopyBuffer = report;
            }
        }

        protected void CopyToMainLocalizationLanguageInDatabase(DialogueDatabase database)
        {
            if (database == null) return;
            report += "<b>Database: " + database.name + "</b>\n";
            CopyActorsToMainLocalizationLanguageInDatabase(database);
            CopyQuestsToMainLocalizationLanguageInDatabase(database);
            CopyConversationsToMainLocalizationLanguageInDatabase(database);
        }

        protected virtual void CopyActorsToMainLocalizationLanguageInDatabase(DialogueDatabase database)
        {
            // Actors: Display Name
            foreach (Actor actor in database.actors)
            {
                var actorIdentifier = "[" + actor.Name + "]: ";
                CopyToField(actor.fields, "Display Name", actorIdentifier, false);
            }
        }

        protected virtual void CopyQuestsToMainLocalizationLanguageInDatabase(DialogueDatabase database)
        {
            // Quests: Display Name, Description, Success Description, Failure Description, Entry #
            foreach (Item item in database.items)
            {
                if (item.IsItem) continue;
                var quest = item;
                var questIdentifier = "[" + quest.Name + "]: ";
                CopyToField(quest.fields, "Display Name", questIdentifier, false);
                CopyToField(quest.fields, "Description", questIdentifier, false);
                CopyToField(quest.fields, "Success Description", questIdentifier, false);
                CopyToField(quest.fields, "Failure Description", questIdentifier, false);
                var entryCount = quest.LookupInt("Entry Count");
                for (int i = 1; i <= entryCount; i++)
                {
                    CopyToField(quest.fields, "Entry " + i, questIdentifier, false);
                }
            }
        }

        protected virtual void CopyConversationsToMainLocalizationLanguageInDatabase(DialogueDatabase database)
        {
            // Conversations: Dialogue Text, Menu Text, Sequence
            foreach (Conversation conversation in database.conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    var entryIdentifier = "[" + conversation.id + ":" + entry.id + "]: ";
                    var dialogueText = entry.DialogueText;
                    if (!string.IsNullOrEmpty(dialogueText))
                    {
                        if (!dryRun) Field.SetValue(entry.fields, prefs.mainLocalizationLanguage, dialogueText, FieldType.Localization);
                        if (verbose || dryRun) report += entryIdentifier + prefs.mainLocalizationLanguage + " =  " + dialogueText + "\n";
                    }
                    CopyToField(entry.fields, "Menu Text", entryIdentifier, false);
                    CopyToField(entry.fields, "Sequence", entryIdentifier, true);
                }
            }
        }

        protected void CopyToField(List<Field> fields, string originalFieldTitle, string identifier, bool locFieldMustExist)
        {
            if (fields == null || string.IsNullOrEmpty(originalFieldTitle)) return;
            var value = Field.LookupValue(fields, originalFieldTitle);
            if (string.IsNullOrEmpty(value)) return;
            var locFieldTitle = originalFieldTitle + " " + prefs.mainLocalizationLanguage;
            if (locFieldMustExist && !Field.FieldExists(fields, locFieldTitle)) return;
            if (!dryRun) Field.SetValue(fields, originalFieldTitle + " " + prefs.mainLocalizationLanguage, value, FieldType.Localization);
            if (verbose || dryRun) report += identifier + originalFieldTitle + " " + prefs.mainLocalizationLanguage + " =  " + value + "\n";
        }

    }

}
