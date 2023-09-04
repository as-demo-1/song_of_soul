// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Watches tab, which replaces the 
    /// Templates tab at runtime to allow the user to watch Lua values.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        public enum WatchType { Expression, Variable, Quest }

        [Serializable]
        public class Watch
        {
            public WatchType type;

            public string expression = string.Empty;
            public string expressionValue = string.Empty;

            public int variableIndex = -1;
            public Variable variable = null;
            public Lua.Result variableValue;

            public int questIndex = -1;
            public int questEntryNumber = 0;
            public string[] questEntryTexts = null;
            public QuestState questState;

            public Watch(WatchType type)
            {
                this.type = type;
            }
        }

        [SerializeField]
        private List<Watch> watches = new List<Watch>();

        [SerializeField]
        private bool autoUpdateWatches = false;

        [SerializeField]
        private float watchUpdateFrequency = 1f;

        [SerializeField]
        private double nextWatchUpdateTime = 0f;

        [SerializeField]
        private string[] watchableVariableNames = null;

        [SerializeField]
        private string[] watchableQuestNames = null;

        [SerializeField]
        private string luaCommand = string.Empty;

        //private ReorderableList watchReorderableList = null; [TODO] Watches in ReorderableList, menu to "+" dropdown.

        private void ResetWatchSection()
        {
            if (EditorApplication.isPlaying && DialogueManager.instance != null && DialogueManager.masterDatabase != null)
            {
                RefreshWatchableVariableList();
                RefreshWatchableQuestList();
                UpdateAllWatches();
                //watchReorderableList = null;
            }
        }

        private void DrawWatchSection()
        {
            if (EditorApplication.isPlaying && DialogueManager.instance != null && DialogueManager.masterDatabase != null)
            {
                if (database == null) database = DialogueManager.instance.initialDatabase;

                if (watches == null) watches = new List<Watch>();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Watches", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                DrawWatchMenu();
                EditorGUILayout.EndHorizontal();

                EditorWindowTools.StartIndentedSection();
                DrawGlobalWatchControls();
                DrawWatches();
                EditorWindowTools.EndIndentedSection();
            }
        }

        private void DrawWatchMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Watch"), false, AddWatch);
                menu.AddItem(new GUIContent("Add Runtime Variable"), false, AddRuntimeVariableWatch);
                menu.AddItem(new GUIContent("Add All Runtime Variables"), false, AddAllRuntimeVariableWatches);
                menu.AddItem(new GUIContent("Refresh Runtime Variable List"), false, RefreshWatchableVariableList);
                menu.AddItem(new GUIContent("Add Quest"), false, AddQuestWatch);
                menu.AddItem(new GUIContent("Add All Quests"), false, AddAllQuestWatches);
                menu.AddItem(new GUIContent("Reset"), false, ResetWatches);
                menu.ShowAsContext();
            }
        }

        private void ResetWatches()
        {
            watches.Clear();
        }

        private void AddWatch()
        {
            watches.Add(new Watch(WatchType.Expression));
        }

        private void AddRuntimeVariableWatch()
        {
            RefreshWatchableVariableList();
            CatalogueWatchableVariableNames();
            watches.Add(new Watch(WatchType.Variable));
            watchableVariableNames = null;
        }

        private void AddAllRuntimeVariableWatches()
        {
            RefreshWatchableVariableList();
            CatalogueWatchableVariableNames();
            for (int i = 0; i < watchableVariableNames.Length; i++)
            {
                if (!watches.Exists(x => x.type == WatchType.Variable && string.Equals(GetWatchedVariableName(x.variableIndex), watchableVariableNames[i])))
                {
                    var watch = new Watch(WatchType.Variable);
                    watches.Add(watch);
                    watch.variableIndex = i;
                    watch.variable = FindVariableInDatabase(i);
                    EvaluateVariableWatch(watch);
                }
            }
        }

        private void AddQuestWatch()
        {
            watches.Add(new Watch(WatchType.Quest));
            watchableQuestNames = null;
        }

        private void AddAllQuestWatches()
        {
            CatalogueWatchableQuestNames();
            for (int i = 0; i < watchableQuestNames.Length; i++)
            {
                if (!watches.Exists(x => x.type == WatchType.Quest && string.Equals(GetWatchedQuestName(x.questIndex), watchableQuestNames[i])))
                {
                    var watch = new Watch(WatchType.Quest);
                    watches.Add(watch);
                    watch.questIndex = i;
                    watch.questEntryTexts = (0 <= watch.questIndex && watch.questIndex < watchableQuestNames.Length) ? GetQuestEntries(GetWatchedQuestName(watch.questIndex)) : null;
                    watch.questEntryNumber = 0;
                    EvaluateQuestWatch(watch);
                }
            }
        }

        private void DrawWatches()
        {
            Watch watchToDelete = null;
            foreach (var watch in watches)
            {
                EditorGUILayout.BeginHorizontal();
                DrawWatch(watch);
                if (GUILayout.Button(new GUIContent("Refresh", "Re-evaluate now."), EditorStyles.miniButton, GUILayout.Width(56)))
                {
                    EvaluateWatch(watch);
                }
                if (GUILayout.Button(new GUIContent(" ", "Delete this watch."), "OL Minus", GUILayout.Width(27), GUILayout.Height(16)))
                {
                    watchToDelete = watch;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (watchToDelete != null) watches.Remove(watchToDelete);
        }

        private void DrawWatch(Watch watch)
        {
            if (watch == null) return;
            switch (watch.type)
            {
                case WatchType.Expression:
                    DrawExpressionWatch(watch);
                    break;
                case WatchType.Variable:
                    DrawVariableWatch(watch);
                    break;
                case WatchType.Quest:
                    DrawQuestWatch(watch);
                    break;
            }
        }

        private void EvaluateWatch(Watch watch)
        {
            if (watch == null) return;
            switch (watch.type)
            {
                case WatchType.Expression:
                    EvaluateExpressionWatch(watch);
                    break;
                case WatchType.Variable:
                    EvaluateVariableWatch(watch);
                    break;
                case WatchType.Quest:
                    EvaluateQuestWatch(watch);
                    break;
            }
        }

        #region Expression Watches

        private void EvaluateExpressionWatch(Watch watch)
        {
            if (watch == null) return;
            watch.expressionValue = string.IsNullOrEmpty(watch.expression) ? string.Empty : Lua.Run("return " + watch.expression).asString;
        }

        private void DrawExpressionWatch(Watch watch)
        {
            if (watch == null) return;
            EditorGUI.BeginChangeCheck();
            watch.expression = EditorGUILayout.TextField(watch.expression);
            if (EditorGUI.EndChangeCheck()) watch.expressionValue = string.Empty;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(watch.expressionValue);
            EditorGUI.EndDisabledGroup();
        }

        #endregion

        #region Variable Watches

        private void EvaluateVariableWatch(Watch watch)
        {
            if (watch == null) return;
            var variableName = GetWatchedVariableName(watch.variableIndex);
            watch.variableValue = string.IsNullOrEmpty(variableName) ? Lua.noResult : DialogueLua.GetVariable(variableName);
        }

        private void DrawVariableWatch(Watch watch)
        {
            if (watch == null) return;
            CatalogueWatchableVariableNames();
            var needToEvaluate = false;
            EditorGUI.BeginChangeCheck();
            watch.variableIndex = EditorGUILayout.Popup(watch.variableIndex, watchableVariableNames);
            if (EditorGUI.EndChangeCheck())
            {
                watch.variable = FindVariableInDatabase(watch.variableIndex);
                EvaluateVariableWatch(watch);
            }
            var variableName = GetWatchedVariableName(watch.variableIndex);
            if (string.IsNullOrEmpty(variableName))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(string.Empty);
                EditorGUI.EndDisabledGroup();
            }
            else if (watch.variable != null && watch.variable.Type == FieldType.Actor)
            {
                EditorGUI.BeginChangeCheck();
                var actorIndex = DrawAssetPopup<Actor>(watch.variableValue.asString, database.actors, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    DialogueLua.SetVariable(variableName, Tools.StringToInt(actorIndex));
                    needToEvaluate = true;
                }
            }
            else if (watch.variable != null && watch.variable.Type == FieldType.Location)
            {
                EditorGUI.BeginChangeCheck();
                var locationIndex = DrawAssetPopup<Location>(watch.variableValue.asString, database.locations, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    DialogueLua.SetVariable(variableName, Tools.StringToInt(locationIndex));
                    needToEvaluate = true;
                }
            }
            else if (watch.variableValue.isTable)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("(table)");
                EditorGUI.EndDisabledGroup();
            }
            else if (watch.variableValue.isBool)
            {
                EditorGUI.BeginChangeCheck();
                var booleanTypeValue = (BooleanType)EditorGUILayout.EnumPopup(watch.variableValue.asBool ? BooleanType.True : BooleanType.False);
                if (EditorGUI.EndChangeCheck())
                {
                    DialogueLua.SetVariable(variableName, booleanTypeValue == BooleanType.True);
                    needToEvaluate = true;
                }
            }
            else if (watch.variableValue.isNumber)
            {
                EditorGUI.BeginChangeCheck();
                var floatValue = EditorGUILayout.FloatField(watch.variableValue.asFloat);
                if (EditorGUI.EndChangeCheck())
                {
                    DialogueLua.SetVariable(variableName, floatValue);
                    needToEvaluate = true;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var stringValue = EditorGUILayout.TextField(watch.variableValue.asString);
                if (EditorGUI.EndChangeCheck())
                {
                    DialogueLua.SetVariable(variableName, stringValue);
                    needToEvaluate = true;
                }
            }
            if (needToEvaluate) EvaluateVariableWatch(watch);
        }

        public string GetWatchedVariableName(int variableIndex)
        {
            return (watchableVariableNames != null && 0 <= variableIndex && variableIndex < watchableVariableNames.Length) ? watchableVariableNames[variableIndex] : null;
        }

        public Variable FindVariableInDatabase(int variableIndex)
        {
            return (database == null) ? null : database.variables.Find(x => string.Equals(DialogueLua.StringToTableIndex(x.Name), watchableVariableNames[variableIndex]));
        }

        #endregion

        #region Quest Watches 

        private void EvaluateQuestWatch(Watch watch)
        {
            if (watch == null) return;
            CatalogueWatchableQuestNames();
            var questName = GetWatchedQuestName(watch.questIndex);
            watch.questState = string.IsNullOrEmpty(questName) ? QuestState.Unassigned
                : (watch.questEntryNumber == 0) ? QuestLog.GetQuestState(questName)
                : QuestLog.GetQuestEntryState(questName, watch.questEntryNumber);
        }

        private void DrawQuestWatch(Watch watch)
        {
            if (watch == null) return;
            CatalogueWatchableQuestNames();
            if (watchableQuestNames == null) return;
            EditorGUI.BeginChangeCheck();
            watch.questIndex = EditorGUILayout.Popup(watch.questIndex, watchableQuestNames);
            if (EditorGUI.EndChangeCheck())
            {
                watch.questEntryTexts = (0 <= watch.questIndex && watch.questIndex < watchableQuestNames.Length) ? GetQuestEntries(GetWatchedQuestName(watch.questIndex)) : null;
                watch.questEntryNumber = 0;
                EvaluateQuestWatch(watch);
            }
            if (watch.questEntryTexts != null)
            {
                EditorGUI.BeginChangeCheck();
                watch.questEntryNumber = EditorGUILayout.Popup(watch.questEntryNumber, watch.questEntryTexts);
                if (EditorGUI.EndChangeCheck())
                {
                    EvaluateQuestWatch(watch);
                }
            }
            if (0 <= watch.questIndex && watch.questIndex < watchableQuestNames.Length)
            {
                EditorGUI.BeginChangeCheck();
                watch.questState = (QuestState)EditorGUILayout.EnumPopup(watch.questState);
                if (EditorGUI.EndChangeCheck())
                {
                    var questName = GetWatchedQuestName(watch.questIndex);
                    if (watch.questEntryNumber == 0)
                    {
                        QuestLog.SetQuestState(questName, watch.questState);
                    }
                    else
                    {
                        QuestLog.SetQuestEntryState(questName, watch.questEntryNumber, watch.questState);
                    }
                    EvaluateQuestWatch(watch);
                    DialogueManager.SendUpdateTracker();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(string.Empty);
                EditorGUI.EndDisabledGroup();
            }
        }

        public string GetWatchedQuestName(int questIndex)
        {
            return (watchableQuestNames != null && 0 <= questIndex && questIndex < watchableQuestNames.Length) ? watchableQuestNames[questIndex] : null;
        }

        private void DrawGlobalWatchControls()
        {
            EditorGUILayout.BeginHorizontal();
            autoUpdateWatches = EditorGUILayout.ToggleLeft("Auto-Refresh", autoUpdateWatches, GUILayout.Width(100));
            watchUpdateFrequency = EditorGUILayout.FloatField(watchUpdateFrequency, GUILayout.Width(128));
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(watches.Count == 0);
            if (GUILayout.Button(new GUIContent("Refresh All", "Re-evaluate all now."), EditorStyles.miniButton, GUILayout.Width(56 + 27)))
            {
                UpdateAllWatches();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshWatchableVariableList()
        {
            watchableVariableNames = null;
            CatalogueWatchableVariableNames();
        }

        private void CatalogueWatchableVariableNames()
        {
            if (database == null) return;
            if (watchableVariableNames == null || watchableVariableNames.Length == 0)
            {
                List<string> variableNames = new List<string>();
                var variableTable = Lua.Run("return Variable").asTable;
                if (variableTable != null)
                {
                    foreach (var variableName in variableTable.keys)
                    {
                        variableNames.Add(variableName);
                    }
                }
                variableNames.Sort();
                watchableVariableNames = variableNames.ToArray();
            }
        }

        private void RefreshWatchableQuestList()
        {
            watchableQuestNames = null;
            CatalogueWatchableQuestNames();
        }

        private void CatalogueWatchableQuestNames()
        {
            if (database == null) return;
            if (watchableQuestNames == null || watchableQuestNames.Length == 0)
            {
                List<string> questNames = new List<string>(QuestLog.GetAllQuests(QuestState.Abandoned | QuestState.Active | QuestState.Failure | QuestState.Success | QuestState.Grantable | QuestState.Unassigned));
                questNames.Sort();
                watchableQuestNames = questNames.ToArray();
            }
        }

        private string[] GetQuestEntries(string questName)
        {
            var count = QuestLog.GetQuestEntryCount(questName);
            var result = new string[count + 1];
            result[0] = "(Main State)";
            for (int i = 1; i <= count; i++)
            {
                result[i] = QuestLog.GetQuestEntry(questName, i);
            }
            return result;
        }

        #endregion

        private void UpdateRuntimeWatchesTab()
        {
            if (autoUpdateWatches && EditorApplication.timeSinceStartup > nextWatchUpdateTime)
            {
                UpdateAllWatches();
            }
        }

        private void UpdateAllWatches()
        {
            if (watches != null)
            {
                foreach (var watch in watches)
                {
                    EvaluateWatch(watch);
                }
            }
            Repaint();
            ResetWatchTime();
        }

        private void ResetWatchTime()
        {
            nextWatchUpdateTime = EditorApplication.timeSinceStartup + watchUpdateFrequency;
        }

        private bool IsLuaWatchBarVisible
        {
            get { return Application.isPlaying && (toolbar.Current == Toolbar.Tab.Templates); }
        }

        private void DrawLuaWatchBar()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Run Code:", GUILayout.Width(64));
            GUI.SetNextControlName("LuaEmptyLabel");
            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
            luaCommand = EditorGUILayout.TextField(string.Empty, luaCommand);
            if (GUILayout.Button("Clear", MoreEditorGuiUtility.ToolbarSearchCancelButtonName))
            {
                luaCommand = string.Empty;
                GUI.FocusControl("LuaEmptyLabel"); // Need to deselect field to clear text field's display.
            }
            if (GUILayout.Button("Run", EditorStyles.miniButton, GUILayout.Width(48)))
            {
                Debug.Log("Running: " + luaCommand);
                Lua.Run(luaCommand, true);
                luaCommand = string.Empty;
                GUI.FocusControl("LuaEmptyLabel"); // Need to deselect field to clear text field's display.
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Height(1));
        }

    }

}