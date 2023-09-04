// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This part of the Dialogue Editor window contains common code for the 
    /// Conditions and Script wizards.
    /// </summary>
    public class LuaWizardBase
    {

        public DialogueDatabase database;

        public enum ConditionWizardResourceType { Quest, QuestEntry, Variable, Actor, Item, Location, SimStatus, Custom, ManualEnter }

        public enum ScriptWizardResourceType { Quest, QuestEntry, Variable, Actor, Item, Location, SimStatus, Alert, Custom, ManualEnter }

        public enum EqualityType { Is, IsNot }

        public enum ComparisonType { Is, IsNot, Less, Greater, LessEqual, GreaterEqual, Between }

        public enum LogicalOperatorType { All, Any }

        public enum BooleanType { True, False }

        public enum SimStatusType { Untouched, WasOffered, WasDisplayed }

        public string[] questNames = new string[0];

        public string[] complexQuestNames = new string[0];

        public string[] variableNames = new string[0];

        public string[] variablePopupNames = new string[0];

        public FieldType[] variableTypes = new FieldType[0];

        public string[] actorNames = new string[0];

        public string[] actorFieldNames = new string[0];

        public FieldType[] actorFieldTypes = new FieldType[0];

        public string[] itemNames = new string[0];

        public string[] itemFieldNames = new string[0];

        public FieldType[] itemFieldTypes = new FieldType[0];

        public string[] locationNames = new string[0];

        public string[] locationFieldNames = new string[0];

        public FieldType[] locationFieldTypes = new FieldType[0];

        // These allow the wizards to remember the last info used,
        // so the next wizard can start using it by default:

        public static ConditionWizardResourceType s_lastWizardResourceType = ConditionWizardResourceType.Quest;
        public static int s_lastActorNamesIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastActorNamesIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastActorNamesIndex", value); }
        }
        public static int s_lastActorFieldIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastActorFieldIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastActorFieldIndex", value); }
        }
        public static int s_lastQuestNamesIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastQuestNamesIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastQuestNamesIndex", value); }
        }
        public static int s_lastQuestEntryIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastQuestEntryIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastQuestEntryIndex", value); }
        }
        public static int s_lastItemNamesIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastItemNamesIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastItemNamesIndex", value); }
        }
        public static int s_lastItemFieldIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastItemFieldIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastItemFieldIndex", value); }
        }
        public static int s_lastLocationNameIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastLocationNameIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastLocationNameIndex", value); }
        }
        public static int s_lastLocationFieldIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastLocationFieldIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastLocationFieldIndex", value); }
        }
        public static int s_lastVariableNameIndex
        {
            get { return EditorPrefs.GetInt("LuaWizard.s_lastVariableNameIndex", 0); }
            set { EditorPrefs.SetInt("LuaWizard.s_lastVariableNameIndex", value); }
        }

        public static ScriptWizardResourceType GetConditionResourceTypeToScriptResourceType(ConditionWizardResourceType conditionResourceType)
        {
            switch (conditionResourceType)
            {
                default:
                case ConditionWizardResourceType.Quest: return ScriptWizardResourceType.Quest;
                case ConditionWizardResourceType.QuestEntry: return ScriptWizardResourceType.QuestEntry;
                case ConditionWizardResourceType.Actor: return ScriptWizardResourceType.Actor;
                case ConditionWizardResourceType.Item: return ScriptWizardResourceType.Item;
                case ConditionWizardResourceType.Variable: return ScriptWizardResourceType.Variable;
                case ConditionWizardResourceType.Location: return ScriptWizardResourceType.Location;
                case ConditionWizardResourceType.SimStatus: return ScriptWizardResourceType.SimStatus;
                case ConditionWizardResourceType.ManualEnter: return ScriptWizardResourceType.ManualEnter;
            }
        }

        public static ConditionWizardResourceType GetScriptResourceTypeToConditionResourceType(ScriptWizardResourceType scriptResourceType)
        {
            switch (scriptResourceType)
            {
                default:
                case ScriptWizardResourceType.Quest: return ConditionWizardResourceType.Quest;
                case ScriptWizardResourceType.QuestEntry: return ConditionWizardResourceType.QuestEntry;
                case ScriptWizardResourceType.Actor: return ConditionWizardResourceType.Actor;
                case ScriptWizardResourceType.Item: return ConditionWizardResourceType.Item;
                case ScriptWizardResourceType.Variable: return ConditionWizardResourceType.Variable;
                case ScriptWizardResourceType.Location: return ConditionWizardResourceType.Location;
                case ScriptWizardResourceType.SimStatus: return ConditionWizardResourceType.SimStatus;
                case ScriptWizardResourceType.ManualEnter: return ConditionWizardResourceType.ManualEnter;
            }
        }

        public LuaWizardBase(DialogueDatabase database)
        {
            this.database = database;
        }

        public void RefreshWizardResources()
        {
            RefreshQuestNames();
            RefreshVariableNames();
            RefreshActorNames();
            RefreshItemNames();
            RefreshLocationNames();
        }

        public void RefreshQuestNames()
        {
            List<string> questList = new List<string>();
            List<string> complexQuestList = new List<string>();
            if (database != null)
            {
                foreach (Item item in database.items)
                {
                    if (!item.IsItem)
                    {
                        questList.Add(item.Name);
                        if (item.LookupInt("Entry Count") > 0)
                        {
                            complexQuestList.Add(item.Name);
                        }
                    }
                }
            }
            questNames = questList.ToArray();
            complexQuestNames = complexQuestList.ToArray();
        }

        public void RefreshVariableNames()
        {
            List<string> nameList = new List<string>();
            List<string> popupNameList = new List<string>();
            List<FieldType> typeList = new List<FieldType>();
            if (database != null)
            {
                // Add (New Variable) item to popup list:
                nameList.Add(string.Empty);
                typeList.Add(FieldType.Text);
                popupNameList.Add("(New)");
                // Add all variables:
                database.variables.ForEach(variable =>
                {
                    var variableName = variable.Name;
                    nameList.Add(variableName);
                    popupNameList.Add(variableName.Replace(".", "/"));
                    typeList.Add(variable.Type);
                });
            }
            variableNames = nameList.ToArray();
            variablePopupNames = popupNameList.ToArray();
            variableTypes = typeList.ToArray();
        }

        public void RefreshActorNames()
        {
            List<string> nameList = new List<string>();
            List<string> fieldList = new List<string>();
            List<FieldType> typeList = new List<FieldType>();
            if (database != null)
            {
                foreach (var actor in database.actors)
                {
                    nameList.Add(actor.Name);
                    foreach (var field in actor.fields)
                    {
                        if (!fieldList.Contains(field.title))
                        {
                            fieldList.Add(field.title);
                            typeList.Add(field.type);
                        }
                    }
                }
            }
            actorNames = nameList.ToArray();
            actorFieldNames = fieldList.ToArray();
            actorFieldTypes = typeList.ToArray();
        }

        public void RefreshItemNames()
        {
            List<string> nameList = new List<string>();
            List<string> fieldList = new List<string>();
            List<FieldType> typeList = new List<FieldType>();
            if (database != null)
            {
                foreach (var item in database.items)
                {
                    if (item.IsItem)
                    {
                        nameList.Add(item.Name);
                        foreach (var field in item.fields)
                        {
                            if (!fieldList.Contains(field.title))
                            {
                                fieldList.Add(field.title);
                                typeList.Add(field.type);
                            }
                        }
                    }
                }
            }
            itemNames = nameList.ToArray();
            itemFieldNames = fieldList.ToArray();
            itemFieldTypes = typeList.ToArray();
        }

        public void RefreshLocationNames()
        {
            List<string> nameList = new List<string>();
            List<string> fieldList = new List<string>();
            List<FieldType> typeList = new List<FieldType>();
            if (database != null)
            {
                foreach (var location in database.locations)
                {
                    nameList.Add(location.Name);
                    foreach (var field in location.fields)
                    {
                        if (!fieldList.Contains(field.title))
                        {
                            fieldList.Add(field.title);
                            typeList.Add(field.type);
                        }
                    }
                }
            }
            locationNames = nameList.ToArray();
            locationFieldNames = fieldList.ToArray();
            locationFieldTypes = typeList.ToArray();
        }

        public string[] GetQuestEntryNames(string questName)
        {
            List<string> questEntryList = new List<string>();
            Item item = database.GetItem(questName);
            if (item != null)
            {
                int entryCount = item.LookupInt("Entry Count");
                if (entryCount > 0)
                {
                    for (int i = 1; i <= entryCount; i++)
                    {
                        string entryText = item.LookupValue(string.Format("Entry {0}", i)) ?? string.Empty;
                        string s = string.Format("{0}. {1}",
                                                 i,
                                                 ((entryText.Length < 20)
                                                     ? entryText
                                                     : entryText.Substring(0, 17) + "..."));
                        questEntryList.Add(s);
                    }
                }
            }
            return questEntryList.ToArray();
        }

        public string GetWizardQuestName(string[] questNames, int index)
        {
            return (0 <= index && index < questNames.Length) ? questNames[index] : "UNDEFINED";
        }

        public string GetLogicalOperatorText(LogicalOperatorType logicalOperator)
        {
            return (logicalOperator == LogicalOperatorType.All) ? "and" : "or";
        }

        public FieldType GetWizardVariableType(int variableIndex)
        {
            return (0 <= variableIndex && variableIndex < variableTypes.Length) ? variableTypes[variableIndex] : FieldType.Text;
        }

        public FieldType GetWizardActorFieldType(int actorFieldIndex)
        {
            return (0 <= actorFieldIndex && actorFieldIndex < actorFieldTypes.Length) ? actorFieldTypes[actorFieldIndex] : FieldType.Text;
        }

        public FieldType GetWizardItemFieldType(int itemFieldIndex)
        {
            return (0 <= itemFieldIndex && itemFieldIndex < itemFieldTypes.Length) ? itemFieldTypes[itemFieldIndex] : FieldType.Text;
        }

        public FieldType GetWizardLocationFieldType(int locationFieldIndex)
        {
            return (0 <= locationFieldIndex && locationFieldIndex < locationFieldTypes.Length) ? locationFieldTypes[locationFieldIndex] : FieldType.Text;
        }

        public string GetWizardEqualityText(EqualityType equalityType)
        {
            return (equalityType == EqualityType.Is) ? "==" : "~=";
        }

        public string GetWizardComparisonText(ComparisonType comparisonType)
        {
            switch (comparisonType)
            {
                case ComparisonType.Is:
                    return "==";
                case ComparisonType.IsNot:
                    return "~=";
                case ComparisonType.Less:
                    return "<";
                case ComparisonType.LessEqual:
                    return "<=";
                case ComparisonType.Greater:
                    return ">";
                case ComparisonType.GreaterEqual:
                    return ">=";
                default:
                    return "==";
            }
        }

        protected CustomFieldType GetCustomFieldType<T>(List<T> assets, int assetIndex, int fieldIndex) where T : Asset
        {
            if (0 <= assetIndex && assetIndex < assets.Count)
            {
                var asset = assets[assetIndex];
                if (asset != null && 0 <= fieldIndex && fieldIndex < asset.fields.Count)
                {
                    var field = asset.fields[fieldIndex];
                    return CustomFieldTypeService.GetFieldCustomType(field.typeString);
                }
            }
            return null;
        }

        public void FindAllCustomLuaFuncs(bool findConditionFuncs, out CustomLuaFunctionInfoRecord[] customLuaFuncs, out string[] customLuaFuncNames)
        {
            var recordList = new List<CustomLuaFunctionInfoRecord>();
            var nameList = new List<string>();
            var guids = AssetDatabase.FindAssets("t:CustomLuaFunctionInfo");
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CustomLuaFunctionInfo>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null) continue;
                var records = findConditionFuncs ? asset.conditionFunctions : asset.scriptFunctions;
                foreach (var record in records)
                {
                    if (record == null || string.IsNullOrEmpty(record.functionName)) continue;
                    recordList.Add(record);
                    nameList.Add(record.functionName);
                }
            }
            customLuaFuncs = recordList.ToArray();
            customLuaFuncNames = nameList.ToArray();
        }

        public void InitCustomParamValues(CustomLuaFunctionInfoRecord record, out object[] customParamValues)
        {
            if (record == null)
            {
                customParamValues = new object[0];
                return;
            }
            customParamValues = new object[record.parameters.Length];
            for (int i = 0; i < record.parameters.Length; i++)
            {
                switch (record.parameters[i])
                {
                    case CustomLuaParameterType.Bool:
                        customParamValues[i] = BooleanType.False;
                        break;
                    case CustomLuaParameterType.Double:
                        customParamValues[i] = (float)0;
                        break;
                    case CustomLuaParameterType.String:
                        customParamValues[i] = string.Empty;
                        break;
                    case CustomLuaParameterType.Actor:
                    case CustomLuaParameterType.Quest:
                    case CustomLuaParameterType.QuestEntry:
                    case CustomLuaParameterType.Variable:
                    case CustomLuaParameterType.VariableName:
                    case CustomLuaParameterType.Item:
                        customParamValues[i] = (int)0;
                        break;
                }
            }
        }

        public void AddNewVariable(string newVariableName, FieldType newVariableType)
        {
            if (database == null) return;
            if (database.GetVariable(newVariableName) != null) return;
            var template = Template.FromDefault();
            var newVariableID = template.GetNextVariableID(database);
            var newVariable = template.CreateVariable(newVariableID, newVariableName, GetDefaultNewVariableValue(newVariableType), newVariableType);
            database.variables.Add(newVariable);
        }

        protected string GetDefaultNewVariableValue(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                    return "false";
                case FieldType.Actor:
                case FieldType.Item:
                case FieldType.Location:
                    return "0";
                default:
                    return string.Empty;
            }
        }

    }

}