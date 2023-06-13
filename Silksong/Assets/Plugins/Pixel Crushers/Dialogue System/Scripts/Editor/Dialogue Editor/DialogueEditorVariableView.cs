// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// Handles the variables list view in the Dialogue Editor window and the
    /// separate Variable View window.
    /// </summary>
    [Serializable]
    public class DialogueEditorVariableView
    {
        public DialogueDatabase database = null;

        private Template template = null;

        // Dialogue Editor window shows additional menu items.
        [SerializeField]
        private bool isDialogueEditorWindow = false;

        // Current variable name filter.
        [SerializeField]
        private string variableFilter = string.Empty;

        // If true, show variable groups in foldouts.
        [SerializeField]
        private bool showVariableGroupFoldouts = false;

        // List of variable groups whose foldouts are expanded.
        [SerializeField]
        private List<string> expandedVariableGroups = new List<string>();

        private class RuntimeValue
        {
            public FieldType fieldType;
            public bool boolValue = false;
            public int intValue = 0;
            public float floatValue = 0;
            public string stringValue = string.Empty;

            public RuntimeValue(bool b) { this.fieldType = FieldType.Boolean; this.boolValue = b; }
            public RuntimeValue(float f) { this.fieldType = FieldType.Number; this.floatValue = f; }
            public RuntimeValue(FieldType ft, int i) { this.fieldType = ft; this.intValue = i; }
            public RuntimeValue(FieldType ft, string s) { this.fieldType = ft; this.stringValue = s; }
        }

        private static RuntimeValue UnknownRuntimeValue = new RuntimeValue(FieldType.Text, "???");

        private Dictionary<string, RuntimeValue> runtimeValues = new Dictionary<string, RuntimeValue>();

        // Track list of conflicted variable names (two or more variables share same name).
        private const double VariableNameCheckFrequency = 0.5f;
        private double lastTimeVariableNamesChecked = 0;
        private HashSet<string> conflictedVariableNames = new HashSet<string>();

        private HashSet<int> syncedVariableIDs = null;

        // VariableGroup class is defined below.
        private const string UngroupedVariableGroup = "(Ungrouped)";
        private Dictionary<string, VariableGroup> m_variableGroups = null;
        private Dictionary<string, VariableGroup> variableGroups
        {
            get
            {
                if (m_variableGroups == null) m_variableGroups = GenerateGroupedVariableList();
                return m_variableGroups;
            }
        }

        private const string ShowGroupsPrefsKey = "PixelCrushers.DialogueSystem.VariableViewer.ShowGroups";

        private delegate Variable CreateNewVariableDelegate(string group);

        public void Initialize(DialogueDatabase database, Template template, bool isDialogueEditorWindow)
        {
            this.database = database;
            this.template = template;
            this.isDialogueEditorWindow = isDialogueEditorWindow;
            this.showVariableGroupFoldouts = EditorPrefs.GetBool(ShowGroupsPrefsKey, false);
            if (EditorApplication.isPlaying)
            {
                RefreshRuntimeValues();
            }
            RefreshView();
        }

        public void RefreshView()
        {
            m_variableGroups = null;
            syncedVariableIDs = null;
        }

        private void RecordSyncedVariableIDs()
        {
            syncedVariableIDs = new HashSet<int>();
            if (database.syncInfo.syncVariables && database.syncInfo.syncVariablesDatabase != null)
            {
                database.syncInfo.syncVariablesDatabase.variables.ForEach(x => syncedVariableIDs.Add(x.id));
            }
        }

        private List<Variable> GenerateFilteredVariableList()
        {
            var list = new List<Variable>();
            if (database == null) return list;
            if (string.IsNullOrEmpty(variableFilter))
            {
                list.AddRange(database.variables);
            }
            else
            {
                for (int i = 0; i < database.variables.Count; i++)
                {
                    var variable = database.variables[i];
                    if (EditorTools.IsAssetInFilter(variable, variableFilter))
                    {
                        list.Add(variable);
                    }
                }
            }
            return list;
        }

        private Dictionary<string, VariableGroup> GenerateGroupedVariableList()
        {
            var dict = new Dictionary<string, VariableGroup>();
            if (syncedVariableIDs == null) RecordSyncedVariableIDs();
            if (string.IsNullOrEmpty(variableFilter) && !showVariableGroupFoldouts)
            {
                // If no filter and not showing groups, add a single group that uses database.variables directly to allow dragging:
                var variableGroup = new VariableGroup(database, UngroupedVariableGroup, database.variables,
                    expandedVariableGroups, conflictedVariableNames, !isDialogueEditorWindow, runtimeValues,
                    UpdateVariableWindows, CreateNewVariable, syncedVariableIDs);
                variableGroup.group = UngroupedVariableGroup;
                variableGroup.variableList = database.variables;
                dict.Add(UngroupedVariableGroup, variableGroup);
            }
            else
            {
                // Otherwise add groups:
                var filteredVariableList = GenerateFilteredVariableList();
                for (int i = 0; i < filteredVariableList.Count; i++)
                {
                    var variable = filteredVariableList[i];
                    var variableName = variable.Name;
                    var dotPos = variable.Name.IndexOf('.');
                    var group = (showVariableGroupFoldouts && dotPos > 0) ? variableName.Substring(0, dotPos) : UngroupedVariableGroup;
                    if (!dict.ContainsKey(group))
                    {
                        var variableGroup = new VariableGroup(database, group, new List<Variable>(),
                            expandedVariableGroups, conflictedVariableNames, !isDialogueEditorWindow, runtimeValues,
                            UpdateVariableWindows, CreateNewVariable, syncedVariableIDs);
                        dict.Add(group, variableGroup);
                    }
                    dict[group].variableList.Add(variable);
                }
            }
            return dict;
        }

        public void Draw()
        {
            if (!isDialogueEditorWindow)
            {
                EditorGUI.BeginChangeCheck();
                database = EditorGUILayout.ObjectField(GUIContent.none, database, typeof(DialogueDatabase), false) as DialogueDatabase;
                if (EditorGUI.EndChangeCheck())
                {
                    Initialize(database, template, isDialogueEditorWindow);
                }
            }
            if (database == null) return;

            EditorGUILayout.BeginHorizontal();
            if (isDialogueEditorWindow)
            {
                EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
            }

            EditorGUI.BeginChangeCheck();
            variableFilter = EditorGUILayout.TextField(GUIContent.none, variableFilter, "ToolbarSeachTextField");
            GUILayout.Label(string.Empty, "ToolbarSeachCancelButtonEmpty");
            if (EditorGUI.EndChangeCheck()) RefreshView();

            DrawVariableMenu();
            EditorGUILayout.EndHorizontal();

            if (database.syncInfo.syncVariables)
            {
                DrawVariableSyncDatabase();
                if (syncedVariableIDs == null) RecordSyncedVariableIDs();
            }
            DrawVariables();
        }

        private void DrawVariableMenu()
        {
            if (!isDialogueEditorWindow)
            {
                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                if (GUILayout.Button(new GUIContent("Refresh", "Refresh runtime values."), "MiniButton", GUILayout.Width(64)))
                {
                    RefreshRuntimeValues();
                }
                EditorGUI.EndDisabledGroup();
            }

            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Variable"), false, () => { CreateNewVariable(string.Empty); });
                menu.AddItem(new GUIContent("Sort/By Name"), false, SortVariablesByName);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortVariablesByID);
                if (isDialogueEditorWindow)
                {
                    menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncVariables, ToggleSyncVariablesFromDB);
                }
                menu.AddItem(new GUIContent("Use Group Foldouts"), showVariableGroupFoldouts, ToggleShowVariableGroupFoldouts);
                if (isDialogueEditorWindow)
                {
                    menu.AddItem(new GUIContent("Variable Viewer..."), false, () => { VariableViewWindow.OpenVariableViewWindow(); });
                }
                menu.ShowAsContext();
            }
        }

        private void SortVariablesByName()
        {
            database.variables.Sort((x, y) => x.Name.CompareTo(y.Name));
            RefreshView();
            if (database != null) EditorUtility.SetDirty(database);
        }

        private void SortVariablesByID()
        {
            database.variables.Sort((x, y) => x.id.CompareTo(y.id));
            RefreshView();
            EditorUtility.SetDirty(database);
        }

        private void ToggleSyncVariablesFromDB()
        {
            database.syncInfo.syncVariables = !database.syncInfo.syncVariables;
            EditorUtility.SetDirty(database);
        }

        private void ToggleShowVariableGroupFoldouts()
        {
            showVariableGroupFoldouts = !showVariableGroupFoldouts;
            EditorPrefs.SetBool(ShowGroupsPrefsKey, showVariableGroupFoldouts);
            RefreshView();
        }

        private void DrawVariableSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync variables from."),
                                                                       database.syncInfo.syncVariablesDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (newDatabase != database.syncInfo.syncVariablesDatabase)
            {
                database.syncInfo.syncVariablesDatabase = newDatabase;
                database.SyncVariables();
                RefreshView();
                if (database != null) EditorUtility.SetDirty(database);
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                database.SyncVariables();
                RefreshView();
                if (database != null) EditorUtility.SetDirty(database);
            }
            EditorGUILayout.EndHorizontal();
        }

        public void RefreshRuntimeValues()
        {
            runtimeValues.Clear();
            if (database == null) return;
            foreach (var variable in database.variables)
            {
                var variableName = variable.Name;
                try
                {
                    switch (variable.Type)
                    {
                        case FieldType.Boolean:
                            runtimeValues[variableName] = new RuntimeValue(DialogueLua.GetVariable(variableName).asBool);
                            break;
                        case FieldType.Number:
                            runtimeValues[variableName] = new RuntimeValue(DialogueLua.GetVariable(variableName).asFloat);
                            break;
                        case FieldType.Actor:
                        case FieldType.Item:
                        case FieldType.Location:
                            int assetID = DialogueLua.GetVariable(variableName).asInt;
                            runtimeValues[variableName] = new RuntimeValue(variable.Type, assetID);
                            switch (variable.Type)
                            {
                                case FieldType.Actor:
                                    var actor = database.GetActor(assetID);
                                    runtimeValues[variableName].stringValue = (actor != null) ? actor.Name : ("ID " + assetID);
                                    break;
                                case FieldType.Item:
                                    var item = database.GetItem(assetID);
                                    runtimeValues[variableName].stringValue = (item != null) ? item.Name : ("ID " + assetID);
                                    break;
                                case FieldType.Location:
                                    var location = database.GetLocation(assetID);
                                    runtimeValues[variableName].stringValue = (location != null) ? location.Name : ("ID " + assetID);
                                    break;
                            }
                            break;
                        default:
                            runtimeValues[variableName] = new RuntimeValue(variable.Type, DialogueLua.GetVariable(variableName).asString);
                            break;
                    }
                }
                catch (Exception)
                {
                    runtimeValues[variableName] = UnknownRuntimeValue;
                }
            }
        }

        private void DrawVariables()
        {
            if (EditorApplication.timeSinceStartup - lastTimeVariableNamesChecked >= VariableNameCheckFrequency)
            {
                lastTimeVariableNamesChecked = EditorApplication.timeSinceStartup;
                CheckVariableNamesForConflicts();
            }
            if (variableGroups.Count == 0)
            {
                return;
            }
            else if (!showVariableGroupFoldouts || (variableGroups.Count == 1 && variableGroups.ContainsKey(UngroupedVariableGroup)))
            {
                variableGroups[UngroupedVariableGroup].Draw(false, string.IsNullOrEmpty(variableFilter));
            }
            else
            {
                foreach (var variableGroup in variableGroups.Values)
                {
                    variableGroup.Draw(true, false);
                }
            }
        }

        private void CheckVariableNamesForConflicts()
        {
            if (database == null) return;
            conflictedVariableNames.Clear();
            var variableNames = new HashSet<string>();
            for (int i = 0; i < database.variables.Count; i++)
            {
                var variableName = database.variables[i].Name;
                if (variableNames.Contains(variableName)) conflictedVariableNames.Add(variableName);
                variableNames.Add(variableName);
            }
        }

        private Variable CreateNewVariable(string group)
        {
            if (template == null) template = TemplateTools.LoadFromEditorPrefs();
            var id = template.GetNextVariableID(database);
            var newVariableName = "New Variable " + id;
            var addGroup = !(string.IsNullOrEmpty(group) || group == UngroupedVariableGroup);
            if (addGroup) newVariableName = group + "." + newVariableName;
            var newVariable = template.CreateVariable(id, newVariableName, string.Empty);
            if (!Field.FieldExists(newVariable.fields, "Name")) newVariable.fields.Add(new Field("Name", string.Empty, FieldType.Text));
            if (!Field.FieldExists(newVariable.fields, "Initial Value")) newVariable.fields.Add(new Field("Initial Value", string.Empty, FieldType.Text));
            if (!Field.FieldExists(newVariable.fields, "Description")) newVariable.fields.Add(new Field("Description", string.Empty, FieldType.Text));
            database.variables.Add(newVariable);
            EditorUtility.SetDirty(database);
            UpdateVariableWindows();
            return newVariable;
        }

        private void UpdateVariableWindows()
        {
            if (VariableViewWindow.instance != null) VariableViewWindow.instance.RefreshView();
            if (DialogueEditorWindow.instance != null) DialogueEditorWindow.instance.RefreshVariableView();
        }

        private class VariableGroup
        {
            public DialogueDatabase database;
            public string group = string.Empty;
            public List<Variable> variableList;
            public List<string> variableGroupFoldouts;
            public HashSet<string> conflictedVariableNames;
            public bool canShowRuntimeValues = false;
            public Dictionary<string, RuntimeValue> runtimeValues;
            public HashSet<int> syncedVariableIDs;
            public ReorderableList reorderableList;

            public System.Action refreshVariablesView = null;
            public CreateNewVariableDelegate createNewVariable = null;

            public VariableGroup(DialogueDatabase database, string group, List<Variable> variableList,
                List<string> variableGroupFoldouts, HashSet<string> conflictedVariableNames, 
                bool canShowRuntimeValues, Dictionary<string, RuntimeValue> runtimeValues,
                System.Action refreshVariablesView, CreateNewVariableDelegate createNewVariable, HashSet<int> syncedVariableIDs)
            {
                this.database = database;
                this.group = group;
                this.variableList = variableList;
                this.variableGroupFoldouts = variableGroupFoldouts;
                this.conflictedVariableNames = conflictedVariableNames;
                this.canShowRuntimeValues = canShowRuntimeValues;
                this.runtimeValues = runtimeValues;
                this.refreshVariablesView = refreshVariablesView;
                this.createNewVariable = createNewVariable;
                this.syncedVariableIDs = syncedVariableIDs;
                reorderableList = null;
            }

            public void Draw(bool drawFoldout, bool draggable)
            {
                if (database == null) return;

                if (drawFoldout)
                {
                    var foldout = variableGroupFoldouts.Contains(group);
                    var newFoldout = EditorGUILayout.Foldout(foldout, group);
                    if (newFoldout != foldout)
                    {
                        foldout = newFoldout;
                        if (foldout) variableGroupFoldouts.Add(group);
                        else variableGroupFoldouts.Remove(group);
                    }
                    if (!foldout) return;
                }
                if (reorderableList == null)
                {
                    reorderableList = new ReorderableList(variableList, typeof(Variable), draggable, true, true, true);
                    reorderableList.drawHeaderCallback = OnDrawVariableHeader;
                    reorderableList.drawElementCallback = OnDrawVariableElement;
                    reorderableList.onAddDropdownCallback = OnAddVariableDropdown;
                    reorderableList.onRemoveCallback = OnRemoveVariable;
                }
                EditorWindowTools.StartIndentedSection();
                reorderableList.DoLayoutList();
                EditorWindowTools.EndIndentedSection();
            }

            private void OnDrawVariableHeader(Rect rect)
            {
                var showRuntimeValue = canShowRuntimeValues && Application.isPlaying;
                var handleWidth = 16f;
                var wholeWidth = rect.width - 6f - handleWidth;
                var typeWidth = Mathf.Min(wholeWidth / 4, 80f);
                var fieldWidth = (wholeWidth - typeWidth) / (showRuntimeValue ? 4 : 3);
                rect.x += handleWidth;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Name");
                rect.x += fieldWidth + 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Initial Value");
                rect.x += fieldWidth + 2;
                if (showRuntimeValue)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Runtime");
                    rect.x += fieldWidth + 2;
                }
                EditorGUI.LabelField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Description");
                rect.x += fieldWidth + 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, typeWidth, EditorGUIUtility.singleLineHeight), "Type");
            }

            private void OnDrawVariableElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (!(reorderableList != null && 0 <= index && index < reorderableList.count)) return;
                var variable = reorderableList.list[index] as Variable;
                if (variable == null) return;

                EditorGUI.BeginDisabledGroup(syncedVariableIDs != null && syncedVariableIDs.Contains(variable.id));
                if (!variable.FieldExists("Initial Value")) variable.fields.Add(new Field("Initial Value", string.Empty, FieldType.Text));
                if (!variable.FieldExists("Description")) variable.fields.Add(new Field("Description", string.Empty, FieldType.Text));
                var variableNameField = Field.Lookup(variable.fields, "Name");
                var initialValueField = Field.Lookup(variable.fields, "Initial Value");
                var descriptionField = Field.Lookup(variable.fields, "Description");
                var showRuntimeValue = canShowRuntimeValues && Application.isPlaying;
                var wholeWidth = rect.width - 6f;
                var typeWidth = Mathf.Min(wholeWidth / 4, 80f);
                var fieldWidth = (wholeWidth - typeWidth) / (showRuntimeValue ? 4 : 3);
                var originalColor = GUI.backgroundColor;
                var variableName = variableNameField.value;
                var conflicted = conflictedVariableNames.Contains(variableName);
                if (conflicted) GUI.backgroundColor = Color.red;
                EditorGUI.BeginChangeCheck();

                variableNameField.value = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), variableName);
                if (EditorGUI.EndChangeCheck()) refreshVariablesView();
                if (conflicted) GUI.backgroundColor = originalColor;
                rect.x += fieldWidth + 2;

                initialValueField.value = CustomFieldTypeService.DrawField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), initialValueField, database);
                rect.x += fieldWidth + 2;

                if (showRuntimeValue)
                {
                    var runtimeRect = new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight);
                    RuntimeValue runtimeValue;
                    if (!runtimeValues.TryGetValue(variableName, out runtimeValue))
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.TextField(runtimeRect, UnknownRuntimeValue.stringValue);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        switch (runtimeValue.fieldType)
                        {
                            case FieldType.Boolean:
                                EditorGUI.BeginChangeCheck();
                                runtimeValue.boolValue = (BooleanType)EditorGUI.EnumPopup(runtimeRect, runtimeValue.boolValue ? BooleanType.True : BooleanType.False) == BooleanType.True;
                                if (EditorGUI.EndChangeCheck()) DialogueLua.SetVariable(variableName, runtimeValue.boolValue);
                                break;

                            case FieldType.Number:
                                EditorGUI.BeginChangeCheck();
                                runtimeValue.floatValue = EditorGUI.FloatField(runtimeRect, runtimeValue.floatValue);
                                if (EditorGUI.EndChangeCheck()) DialogueLua.SetVariable(variableName, runtimeValue.floatValue);
                                break;

                            case FieldType.Actor:
                            case FieldType.Item:
                            case FieldType.Location:
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUI.TextField(runtimeRect, runtimeValue.stringValue);
                                EditorGUI.EndDisabledGroup();
                                break;

                            default:
                            case FieldType.Files:
                            case FieldType.Localization:
                            case FieldType.Text:
                                EditorGUI.BeginChangeCheck();
                                runtimeValue.stringValue = EditorGUI.TextField(runtimeRect, runtimeValue.stringValue);
                                if (EditorGUI.EndChangeCheck()) DialogueLua.SetVariable(variableName, runtimeValue.stringValue);
                                break;
                        }
                    }
                    rect.x += fieldWidth + 2;
                }

                descriptionField.value = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), descriptionField.value);
                rect.x += fieldWidth + 2;

                CustomFieldTypeService.DrawFieldType(new Rect(rect.x, rect.y + 2, typeWidth, EditorGUIUtility.singleLineHeight), initialValueField);

                EditorGUI.EndDisabledGroup();
            }

            private void OnRemoveVariable(ReorderableList list)
            {
                if (!(reorderableList != null && 0 <= list.index && list.index < reorderableList.count)) return;
                var variable = reorderableList.list[list.index] as Variable;
                if (variable == null) return;
                if (syncedVariableIDs != null && syncedVariableIDs.Contains(variable.id)) return;
                if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(variable)), "Are you sure you want to delete this?", "Delete", "Cancel"))
                {
                    database.variables.Remove(variable);
                    EditorUtility.SetDirty(database);
                    refreshVariablesView();
                }
            }

            private void OnAddVariableDropdown(Rect buttonRect, ReorderableList list)
            {
                var menu = new GenericMenu();
                string[] fieldTypes = CustomFieldTypeService.GetDialogueSystemTypes();
                string[] fieldPublicNames = CustomFieldTypeService.GetDialogueSystemPublicNames();
                for (int i = 0; i < fieldPublicNames.Length; i++)
                {
                    var fieldType = (i < fieldTypes.Length) ? fieldTypes[i] : "CustomFieldType_Text";
                    menu.AddItem(new GUIContent(fieldPublicNames[i]), false, OnSelectVariableTypeToAdd, fieldType);
                }
                menu.ShowAsContext();
            }

            private void OnSelectVariableTypeToAdd(object data)
            {
                if (data == null || data.GetType() != typeof(string) || createNewVariable == null) return;
                var typeName = (string)data;
                Dictionary<string, CustomFieldType> types = CustomFieldTypeService.GetTypesDictionary();
                var variable = createNewVariable(group);
                var field = Field.Lookup(variable.fields, "Initial Value");
                if (types.ContainsKey(typeName) && field != null)
                {
                    field.type = types[typeName].storeFieldAsType;
                    field.typeString = typeName;
                }
            }
        }

     }

}