// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Renames Dialogue System database assets (e.g., variables) across databases, prefabs,
    /// and asset files. Used by DialogueSystemAssetRenamerWindow.
    /// </summary>
    public static class DialogueSystemAssetRenamerUtility
    {

        #region Variables

        public delegate void ProcessFileDelegate(string filename, string assetPath, string originalName, string replacementName, ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler);
        public delegate bool ProcessSceneDelegate(string originalName, string replacementName);
        public delegate bool ProcessPrefabDelegate(string assetPath, string originalName, string replacementName);

        public const string ExternalVersionControlVisibleMetaFiles = "Visible Meta Files";
        public static string versionControlMode
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                return VersionControlSettings.mode;
#else
                return EditorSettings.externalVersionControl;
#endif
            }
            set
            {
#if UNITY_2020_1_OR_NEWER
                VersionControlSettings.mode = value;
#else
                EditorSettings.externalVersionControl = value;
#endif
            }
        }

        public static string report;
        public static string ignoreFilesRegex;

        #endregion

        #region Process

        public static void ProcessAssets(string title, string originalName, string replacementName,
            ProcessFileDelegate processFileHandler, ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler)
        {
            AssetDatabase.SaveAssets();
            var cancel = false;

            // Save open scenes:
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                report += "Cancelled.\n";
                return;
            }

            if (!string.IsNullOrEmpty(replacementName))
            {
                // Close open assets:
                if (Selection.activeObject is DialogueDatabase || Selection.activeObject is LocalizedTextTable) Selection.activeObject = null;
                if (DialogueEditor.DialogueEditorWindow.instance != null) DialogueEditor.DialogueEditorWindow.instance.Close();
            }

            if (!string.IsNullOrEmpty(ignoreFilesRegex)) report += "Ignoring files matching: " + ignoreFilesRegex + "\n";

            // Record serialization mode and force text:
            var originalSerializationMode = EditorSettings.serializationMode;
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                cancel = EditorUtility.DisplayCancelableProgressBar("Text Serialization Required", "Switching to Force Text serialization mode. This may take a while.", 0);
                report += "Set serialization mode to Force Text.\n";
                EditorSettings.serializationMode = SerializationMode.ForceText;
                if (cancel)
                {
                    report += "Update process cancelled. Serialization mode is set to Force Text. To set it back to " + originalSerializationMode + " use menu item Edit > Project Settings > Editor.\n";
                    return;
                }
            }

            // Force visible meta files:
            var originalExternalVersionControl = versionControlMode;
            if (!versionControlMode.Equals(ExternalVersionControlVisibleMetaFiles))
            {
                cancel = EditorUtility.DisplayCancelableProgressBar("Visible Meta Files Required", "Switching Version Control Mode to Visible Meta Files. This may take a while.", 0);
                report += "Set version control mode to Visible Meta Files.\n";
                versionControlMode = ExternalVersionControlVisibleMetaFiles;
                if (cancel)
                {
                    report += "Update process cancelled. Serialization mode is " + EditorSettings.serializationMode + " and Version Control Mode is " +
                        versionControlMode + ". To change these settings, use menu item Edit > Project Settings > Editor.\n";
                    return;
                }
            }

            // Record currently open scene:
            var originalScenePath = EditorSceneManager.GetActiveScene().path;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var isWindows = Application.platform == RuntimePlatform.WindowsEditor;
            var hasRegex = !string.IsNullOrEmpty(ignoreFilesRegex);
            try
            {
                // Update all scene, scriptable object, and prefab files:
                var files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".unity") || s.EndsWith(".asset") || s.EndsWith(".prefab")).ToArray();
                for (int i = 0; i < files.Length; i++)
                {
                    var filename = files[i];
                    var assetPath = filename;
                    if (isWindows) assetPath = assetPath.Replace("\\", "/");
                    assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                    if (hasRegex && Regex.IsMatch(assetPath, ignoreFilesRegex)) continue; // Ignore this file.
                    cancel = EditorUtility.DisplayCancelableProgressBar(title, filename, (float)i / (float)files.Length);
                    ProcessAssetFile(filename, assetPath, originalName, replacementName, processFileHandler, processSceneHandler, processPrefabHandler);
                    if (cancel)
                    {
                        if (EditorSettings.serializationMode != originalSerializationMode)
                        {
                            report += "Update process cancelled. Serialization mode is set to Force Text. To set it back to " + originalSerializationMode + " use menu item Edit > Project Settings > Editor.\n";
                        }
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                report += e.Message + "\n";
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();

                if (!cancel)
                {
                    // Restore original serialization mode:
                    if (originalSerializationMode != SerializationMode.ForceText)
                    {
                        EditorUtility.DisplayProgressBar(title, "Restoring original serialization mode. This may take a while.", 0.99f);
                        report += "Set serialization mode to " + originalSerializationMode + "\n.";
                        EditorSettings.serializationMode = originalSerializationMode;
                    }
                    // Restore original version control mode:
                    if (originalExternalVersionControl != ExternalVersionControlVisibleMetaFiles)
                    {
                        EditorUtility.DisplayProgressBar(title, "Restoring original version control mode. This may take a while.", 0.995f);
                        report += "Set version control mode to " + originalExternalVersionControl + ".\n";
                        versionControlMode = originalExternalVersionControl;
                    }
                }

                // Return to original scene:
                if (!string.IsNullOrEmpty(originalScenePath) && EditorSceneManager.GetActiveScene().path != originalScenePath)
                {
                    EditorSceneManager.OpenScene(originalScenePath);
                }
            }
        }

        public static void ProcessAssetFile(string filename, string assetPath, string originalName, string replacementName,
            ProcessFileDelegate processFileHandler, ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler)
        {
            try
            {
                processFileHandler(filename, assetPath, originalName, replacementName, processSceneHandler, processPrefabHandler);
            }
            catch (System.Exception e)
            {
                if (assetPath.Contains("NavMesh"))
                {
                    report += "Did not process " + filename + " but it appears to be a NavMesh so this should be fine. Result: " + e.Message + "\n";
                }
                else if (assetPath.Contains("Terrain"))
                {
                    report += "Did not process " + filename + " but it appears to be a Terrain so this should be fine. Result: " + e.Message + "\n";
                }
                else
                {
                    report += "Unable to process " + filename + ": " + e.Message + "\n";
                }
            }
        }

        #endregion

        #region Find Variable

        public static string FindVariable(string variableName, string replacementName)
        {
            if (string.IsNullOrEmpty(variableName)) return string.Empty;
            var title = string.IsNullOrEmpty(replacementName) ? "Find Variable" : "Rename Variable";
            report = string.IsNullOrEmpty(replacementName)
                ? ("Searching for variable '" + variableName + "'\nResults:\n")
                : ("Replacing variable '" + variableName + "' with '" + replacementName + "'\nResults:\n");
            ProcessAssets(title, variableName, replacementName, ProcessVariableInFile, ProcessVariableInScene, ProcessVariableInPrefab);
            return report;
        }

        public static void ProcessVariableInFile(string filename, string assetPath, string originalName, string replacementName,
            ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(originalName)) return;
            var text = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(text)) return;
            var replace = !string.IsNullOrEmpty(replacementName);

            // Find/replace Lua expressions that use the original variable name:
            var originalLuaExpression = "Variable[\"" + DialogueLua.StringToTableIndex(originalName) + "\"]";
            var replacementLuaExpression = string.IsNullOrEmpty(replacementName.Trim()) ? string.Empty : "Variable[\"" + DialogueLua.StringToTableIndex(replacementName) + "\"]";
            var found = text.Contains(originalLuaExpression);
            if (found && replace)
            {
                text = text.Replace(originalLuaExpression, replacementLuaExpression);
            }

            // Find/replace [var=variable] tags that use the original variable name:
            var originalVarTag = "[var=" + DialogueLua.StringToTableIndex(originalName) + "]";
            var replacementVarTag = string.IsNullOrEmpty(replacementName.Trim()) ? string.Empty : "Variable[\"" + DialogueLua.StringToTableIndex(replacementName) + "\"]";
            var foundVarTag = text.Contains(originalVarTag);
            if (foundVarTag)
            {
                found = true;
                if (replace)
                {
                    text = text.Replace(originalVarTag, replacementVarTag);
                }
            }

            if (found && replace)
            {
                File.WriteAllText(filename, text);
                AssetDatabase.Refresh();
            }

            // If it's a dialogue database, find/replace actual variable name, too:
            if (text.Contains("globalUserScript")) // DBs contain this string; used to minimize attempts to load non-database assets.
            {
                AssetDatabase.Refresh();
                var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                if (database != null)
                {
                    var variable = database.GetVariable(originalName);
                    if (variable != null)
                    {
                        found = true;
                        if (replace)
                        {
                            variable.Name = replacementName;
                            EditorUtility.SetDirty(database);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            // If file is a scene, open it to check for any components such as IncrementOnDestroy that use VariablePopup:
            if (assetPath.EndsWith(".unity"))
            {
                // Open scene:
                var sceneName = Path.GetFileNameWithoutExtension(filename);
                EditorSceneManager.OpenScene(filename);
                var scene = EditorSceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid()) return;

                // Call scene checking delegate:
                var foundInScene = processSceneHandler(originalName, replacementName);
                if (foundInScene)
                {
                    // If found and replaced, save scene:
                    found = true;
                    if (replace)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }
            // If file is a prefab, check it:
            else if (assetPath.EndsWith(".prefab"))
            {
                var foundInPrefab = processPrefabHandler(assetPath, originalName, replacementName);
                if (foundInPrefab)
                {
                    found = true;
                    if (replace) AssetDatabase.SaveAssets();
                }
            }

            // Update report:
            if (found)
            {
                report += (replace ? "Replaced in: " : "Found in: ") + assetPath + "\n";
            }
        }

        public static bool ProcessVariableInScene(string originalName, string replacementName)
        {
            var replace = !string.IsNullOrEmpty(replacementName);
            var found = false;
            foreach (var incrementOnDestroy in GameObject.FindObjectsOfType<IncrementOnDestroy>())
            {
                if (incrementOnDestroy.variable == originalName)
                {
                    found = true;
                    if (replace)
                    {
                        incrementOnDestroy.variable = replacementName;
                    }
                }
            }
            return found;
        }

        public static bool ProcessVariableInPrefab(string assetPath, string originalName, string replacementName)
        {
            var replace = !string.IsNullOrEmpty(replacementName);
            var found = false;
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var thisObject in objects)
            {
                if (thisObject is IncrementOnDestroy)
                {
                    var incrementOnDestroy = thisObject as IncrementOnDestroy;
                    if (incrementOnDestroy.variable == originalName)
                    {
                        found = true;
                        if (replace)
                        {
                            incrementOnDestroy.variable = replacementName;
                            EditorUtility.SetDirty(incrementOnDestroy);
                        }
                    }
                }
            }
            return found;
        }

        #endregion

        #region Find Actor

        public static string FindActor(string actorName, string replacementName)
        {
            if (string.IsNullOrEmpty(actorName)) return string.Empty;
            var title = string.IsNullOrEmpty(replacementName) ? "Find Actor" : "Rename Actor";
            report = string.IsNullOrEmpty(replacementName)
                ? ("Searching for actor'" + actorName + "'\nResults:\n")
                : ("Replacing actor '" + actorName + "' with '" + replacementName + "'\nResults:\n");
            ProcessAssets(title, actorName, replacementName, ProcessActorInFile, ProcessActorInScene, ProcessActorInPrefab);
            return report;
        }

        public static void ProcessActorInFile(string filename, string assetPath, string originalName, string replacementName,
            ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(originalName)) return;
            var text = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(text)) return;
            var replace = !string.IsNullOrEmpty(replacementName);

            // Find/replace Lua expressions that use the original actor name:
            var originalLuaExpression = "Actor[\"" + DialogueLua.StringToTableIndex(originalName) + "\"]";
            var replacementLuaExpression = string.IsNullOrEmpty(replacementName.Trim()) ? string.Empty : "Actor[\"" + DialogueLua.StringToTableIndex(replacementName) + "\"]";
            var found = text.Contains(originalLuaExpression);
            if (found && replace)
            {
                text = text.Replace(originalLuaExpression, replacementLuaExpression);
            }

            if (found && replace)
            {
                File.WriteAllText(filename, text);
                AssetDatabase.Refresh();
            }

            // If it's a dialogue database, find/replace actual actor name, too:
            if (text.Contains("globalUserScript")) // DBs contain this string; used to minimize attempts to load non-database assets.
            {
                AssetDatabase.Refresh();
                var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                if (database != null)
                {
                    var actor = database.GetActor(originalName);
                    if (actor != null)
                    {
                        found = true;
                        if (replace)
                        {
                            actor.Name = replacementName;
                            EditorUtility.SetDirty(database);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            // If file is a scene, open it to check for any components such as DialogueActor that use ActorPopup:
            if (assetPath.EndsWith(".unity"))
            {
                // Open scene:
                var sceneName = Path.GetFileNameWithoutExtension(filename);
                EditorSceneManager.OpenScene(filename);
                var scene = EditorSceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid()) return;

                // Call scene checking delegate:
                var foundInScene = processSceneHandler(originalName, replacementName);
                if (foundInScene)
                {
                    // If found and replaced, save scene:
                    found = true;
                    if (replace)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }
            // If file is a prefab, check it:
            else if (assetPath.EndsWith(".prefab"))
            {
                var foundInPrefab = processPrefabHandler(assetPath, originalName, replacementName);
                if (foundInPrefab)
                {
                    found = true;
                    if (replace) AssetDatabase.SaveAssets();
                }
            }

            // Update report:
            if (found)
            {
                report += (replace ? "Replaced in: " : "Found in: ") + assetPath + "\n";
            }
        }

        public static bool ProcessActorInScene(string originalName, string replacementName)
        {
            var replace = !string.IsNullOrEmpty(replacementName);
            var found = false;
            foreach (var dialogueActor in GameObject.FindObjectsOfType<DialogueActor>())
            {
                if (dialogueActor.actor == originalName)
                {
                    found = true;
                    if (replace)
                    {
                        dialogueActor.actor = replacementName;
                    }
                }
            }
            return found;
        }

        public static bool ProcessActorInPrefab(string assetPath, string originalName, string replacementName)
        {
            var replace = !string.IsNullOrEmpty(replacementName);
            var found = false;
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var thisObject in objects)
            {
                if (thisObject is DialogueActor)
                {
                    var dialogueActor = thisObject as DialogueActor;
                    if (dialogueActor.actor == originalName)
                    {
                        found = true;
                        if (replace)
                        {
                            dialogueActor.actor = replacementName;
                            EditorUtility.SetDirty(dialogueActor);
                        }
                    }
                }
            }
            return found;
        }

        #endregion

        #region Find Quest

        public static string FindQuest(string questName, string replacementName)
        {
            if (string.IsNullOrEmpty(questName)) return string.Empty;
            var title = string.IsNullOrEmpty(replacementName) ? "Find Quest" : "Rename Quest";
            report = string.IsNullOrEmpty(replacementName)
                ? ("Searching for quest'" + questName + "'\nResults:\n")
                : ("Replacing quest '" + questName + "' with '" + replacementName + "'\nResults:\n");
            ProcessAssets(title, questName, replacementName, ProcessQuestInFile, ProcessQuestInScene, ProcessQuestInPrefab);
            return report;
        }

        public static bool FindText(string originalSubstring, string replacementSubstring, bool replace, ref string text)
        {
            var found = text.Contains(originalSubstring);
            if (found && replace)
            {
                text = text.Replace(originalSubstring, replacementSubstring);
            }
            return found;
        }

        public static bool FindLuaFunction(string funcName, string originalName, string replacementName, bool replace, ref string text)
        {
            var originalLuaCode = funcName + "(\"" + DialogueLua.StringToTableIndex(originalName) + "\"";
            var replacementLuaCode = string.IsNullOrEmpty(replacementName.Trim()) ? string.Empty : funcName + "(\"" + DialogueLua.StringToTableIndex(replacementName) + "\"";
            return FindText(originalLuaCode, replacementLuaCode, replace, ref text);
        }

        public static void ProcessQuestInFile(string filename, string assetPath, string originalName, string replacementName,
            ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(originalName)) return;
            var text = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(text)) return;
            var replace = !string.IsNullOrEmpty(replacementName);

            // Find/replace Lua expressions that use the original quest name:
            // Item[]
            var originalLuaExpression = "Item[\"" + DialogueLua.StringToTableIndex(originalName) + "\"]";
            var replacementLuaExpression = string.IsNullOrEmpty(replacementName.Trim()) ? string.Empty : "Item[\"" + DialogueLua.StringToTableIndex(replacementName) + "\"]";
            var found = FindText(originalLuaExpression, replacementLuaExpression, replace, ref text);
            // Quest[]
            originalLuaExpression = "Quest[\"" + DialogueLua.StringToTableIndex(originalName) + "\"]";
            replacementLuaExpression = string.IsNullOrEmpty(replacementName.Trim()) ? string.Empty : "Quest[\"" + DialogueLua.StringToTableIndex(replacementName) + "\"]";
            found = FindText(originalLuaExpression, replacementLuaExpression, replace, ref text) || found;
            // Quest functions
            found = FindLuaFunction("CurrentQuestState", originalName, replacementName, replace, ref text) || found;
            found = FindLuaFunction("SetQuestState", originalName, replacementName, replace, ref text) || found;
            found = FindLuaFunction("CurrentQuestEntryState", originalName, replacementName, replace, ref text) || found;
            found = FindLuaFunction("SetQuestEntryState", originalName, replacementName, replace, ref text) || found;

            if (found && replace)
            {
                File.WriteAllText(filename, text);
                AssetDatabase.Refresh();
            }

            // If it's a dialogue database, find/replace actual quest name, too:
            if (text.Contains("globalUserScript")) // DBs contain this string; used to minimize attempts to load non-database assets.
            {
                AssetDatabase.Refresh();
                var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                if (database != null)
                {
                    var quest = database.GetItem(originalName);
                    if (quest != null)
                    {
                        found = true;
                        if (replace)
                        {
                            quest.Name = replacementName;
                            EditorUtility.SetDirty(database);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            // If file is a scene, open it to check for any components such as DialogueSystemTrigger that use QuestPopup:
            if (assetPath.EndsWith(".unity"))
            {
                // Open scene:
                var sceneName = Path.GetFileNameWithoutExtension(filename);
                EditorSceneManager.OpenScene(filename);
                var scene = EditorSceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid()) return;

                // Call scene checking delegate:
                var foundInScene = processSceneHandler(originalName, replacementName);
                if (foundInScene)
                {
                    // If found and replaced, save scene:
                    found = true;
                    if (replace)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }
            // If file is a prefab, check it:
            else if (assetPath.EndsWith(".prefab"))
            {
                var foundInPrefab = processPrefabHandler(assetPath, originalName, replacementName);
                if (foundInPrefab)
                {
                    found = true;
                    if (replace) AssetDatabase.SaveAssets();
                }
            }

            // Update report:
            if (found)
            {
                report += (replace ? "Replaced in: " : "Found in: ") + assetPath + "\n";
            }
        }

        public static bool ProcessQuestInScene(string originalName, string replacementName)
        {
            var replace = !string.IsNullOrEmpty(replacementName);
            var found = false;
            foreach (var dialogueSystemTrigger in GameObject.FindObjectsOfType<DialogueSystemTrigger>())
            {
                found = FindQuestInDialogueSystemTrigger(dialogueSystemTrigger, originalName, replacementName, replace, false) || found;
            }
            foreach (var questStateListener in GameObject.FindObjectsOfType<QuestStateListener>())
            {
                found = FindQuestInQuestStateListener(questStateListener, originalName, replacementName, replace, false) || found;
            }
            return found;
        }

        public static bool ProcessQuestInPrefab(string assetPath, string originalName, string replacementName)
        {
            var replace = !string.IsNullOrEmpty(replacementName);
            var found = false;
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var thisObject in objects)
            {
                if (thisObject is DialogueSystemTrigger)
                {
                    found = FindQuestInDialogueSystemTrigger(thisObject as DialogueSystemTrigger, originalName, replacementName, replace, true) || found;
                }
                else if (thisObject is QuestStateListener)
                {
                    found = FindQuestInQuestStateListener(thisObject as QuestStateListener, originalName, replacementName, replace, true) || found;
                }
            }
            return found;
        }

        public static bool FindQuestInDialogueSystemTrigger(DialogueSystemTrigger dialogueSystemTrigger, string originalName, string replacementName, bool replace, bool isPrefab)
        {
            var found = false;
            if (dialogueSystemTrigger != null)
            {
                foreach (var questCondition in dialogueSystemTrigger.condition.questConditions)
                {
                    if (questCondition.questName == originalName)
                    {
                        found = true;
                        if (replace)
                        {
                            questCondition.questName = replacementName;
                        }
                    }
                }
                if (dialogueSystemTrigger.questName == originalName)
                {
                    found = true;
                    if (replace)
                    {
                        dialogueSystemTrigger.questName = replacementName;
                    }
                }
            }
            if (found && replace && isPrefab) EditorUtility.SetDirty(dialogueSystemTrigger);
            return found;
        }

        public static bool FindQuestInQuestStateListener(QuestStateListener questStateListener, string originalName, string replacementName, bool replace, bool isPrefab)
        {
            var found = false;
            if (questStateListener != null)
            {
                if (questStateListener.questName == originalName)
                {
                    found = true;
                    if (replace)
                    {
                        questStateListener.questName = replacementName;
                        if (isPrefab) EditorUtility.SetDirty(questStateListener);
                    }
                }
            }
            return found;
        }

        #endregion

        #region Find Conversation

        public static string FindConversation(string conversationTitle, string replacementName)
        {
            if (string.IsNullOrEmpty(conversationTitle)) return string.Empty;
            var title = string.IsNullOrEmpty(replacementName) ? "Find Conversation" : "Rename Conversation";
            report = string.IsNullOrEmpty(replacementName)
                ? ("Searching for Conversation'" + conversationTitle + "'\nResults:\n")
                : ("Replacing Conversation '" + conversationTitle + "' with '" + replacementName + "'\nResults:\n");
            ProcessAssets(title, conversationTitle, replacementName, ProcessConversationInFile, ProcessConversationInScene, ProcessConversationInPrefab);
            return report;
        }

        public static void ProcessConversationInFile(string filename, string assetPath, string originalTitle, string replacementTitle,
            ProcessSceneDelegate processSceneHandler, ProcessPrefabDelegate processPrefabHandler)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(originalTitle)) return;
            var text = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(text)) return;
            var replace = !string.IsNullOrEmpty(replacementTitle);

            var found = false;

            // No need to find/replace Lua expressions that use the original Conversation name, since Lua uses conversation IDs.

            // If it's a dialogue database, find/replace actual conversation title:
            if (text.Contains("globalUserScript")) // DBs contain this string; used to minimize attempts to load non-database assets.
            {
                AssetDatabase.Refresh();
                var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                if (database != null)
                {
                    var conversation = database.GetConversation(originalTitle);
                    if (conversation != null)
                    {
                        found = true;
                        if (replace)
                        {
                            conversation.Title = replacementTitle;
                            EditorUtility.SetDirty(database);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            // If file is a scene, open it to check for any components such as DialogueSystemTrigger that use ConversationPopup:
            if (assetPath.EndsWith(".unity"))
            {
                // Open scene:
                var sceneName = Path.GetFileNameWithoutExtension(filename);
                EditorSceneManager.OpenScene(filename);
                var scene = EditorSceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid()) return;

                // Call scene checking delegate:
                var foundInScene = processSceneHandler(originalTitle, replacementTitle);
                if (foundInScene)
                {
                    // If found and replaced, save scene:
                    found = true;
                    if (replace)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }
            // If file is a prefab, check it:
            else if (assetPath.EndsWith(".prefab"))
            {
                var foundInPrefab = processPrefabHandler(assetPath, originalTitle, replacementTitle);
                if (foundInPrefab)
                {
                    found = true;
                    if (replace) AssetDatabase.SaveAssets();
                }
            }

            // Update report:
            if (found)
            {
                report += (replace ? "Replaced in: " : "Found in: ") + assetPath + "\n";
            }
        }

        public static bool ProcessConversationInScene(string originalTitle, string replacementTitle)
        {
            var replace = !string.IsNullOrEmpty(replacementTitle);
            var found = false;
            foreach (var dialogueSystemTrigger in GameObject.FindObjectsOfType<DialogueSystemTrigger>())
            {
                found = FindConversationInDialogueSystemTrigger(dialogueSystemTrigger, originalTitle, replacementTitle, replace, false) || found;
            }
            foreach (var conversationStarter in GameObject.FindObjectsOfType<ConversationStarter>())
            {
                found = FindConversationInConversationStarter(conversationStarter, originalTitle, replacementTitle, replace, false) || found;
            }
            return found;
        }

        public static bool ProcessConversationInPrefab(string assetPath, string originalTitle, string replacementTitle)
        {
            var replace = !string.IsNullOrEmpty(replacementTitle);
            var found = false;
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var thisObject in objects)
            {
                if (thisObject is DialogueSystemTrigger)
                {
                    found = FindConversationInDialogueSystemTrigger(thisObject as DialogueSystemTrigger, originalTitle, replacementTitle, replace, true) || found;
                }
                else if (thisObject is ConversationStarter)
                {
                    found = FindConversationInConversationStarter(thisObject as ConversationStarter, originalTitle, replacementTitle, replace, true) || found;
                }
            }
            return found;
        }

        public static bool FindConversationInDialogueSystemTrigger(DialogueSystemTrigger dialogueSystemTrigger, string originalTitle, string replacementTitle, bool replace, bool isPrefab)
        {
            var found = false;
            if (dialogueSystemTrigger != null)
            {
                if (dialogueSystemTrigger.conversation == originalTitle)
                {
                    found = true;
                    if (replace)
                    {
                        dialogueSystemTrigger.conversation = replacementTitle;
                    }
                }
                if (dialogueSystemTrigger.barkConversation == originalTitle)
                {
                    found = true;
                    if (replace)
                    {
                        dialogueSystemTrigger.barkConversation = replacementTitle;
                    }
                }
            }
            if (found && replace && isPrefab) EditorUtility.SetDirty(dialogueSystemTrigger);
            return found;
        }

        public static bool FindConversationInConversationStarter(ConversationStarter conversationStarter, string originalTitle, string replacementTitle, bool replace, bool isPrefab)
        {
            var found = false;
            if (conversationStarter != null)
            {
                if (conversationStarter.conversation == originalTitle)
                {
                    found = true;
                    if (replace)
                    {
                        conversationStarter.conversation = replacementTitle;
                        if (isPrefab) EditorUtility.SetDirty(conversationStarter);
                    }
                }
            }
            return found;
        }

        #endregion

    }
}
