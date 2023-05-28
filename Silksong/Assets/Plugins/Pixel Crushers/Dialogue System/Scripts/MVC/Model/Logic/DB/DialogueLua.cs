// Copyright (c) Pixel Crushers. All rights reserved.

#if !(USE_NLUA || OVERRIDE_LUA)
using Language.Lua;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static class that adds additional Chat Mapper-related functionality to the Lua 
    /// environment, since Chat Mapper projects can define conditions and actions in Lua, 
    /// modify Lua variables, and use Chat Mapper-specific Lua functions.
    /// 
    /// For speed, this class occasionally bypasses the Lua wrapper class and works directly
    /// with the underlying Lua implementation, Lua Interpreter.
    /// </summary>
    public static class DialogueLua
    {

        // SimStatus values:
        public const string SimStatus = "SimStatus";
        public const string Untouched = "Untouched";
        public const string WasDisplayed = "WasDisplayed";
        public const string WasOffered = "WasOffered";

        /// <summary>
        /// Gets or sets a value indicating whether to record SimStatus in the Lua environment.
        /// SimStatus comes from Chat Mapper, and is a way to remember whether a dialogue entry
        /// has been offered to the player ('WasOffered'), spoken by an actor ('WasDisplayed'),
        /// or neither ('Untouched'). If this information isn't useful in your project, you
        /// can disable it to save memory.
        /// </summary>
        /// <value><c>true</c> to include sim status; otherwise, <c>false</c>.</value>
        public static bool includeSimStatus = true;

        /// <summary>
        /// DANGEROUS: Version 2.2.5 introduced a fix that replaces "/" with "_" in table indices.
        /// In some existing projects with many forward slashes, it may not be practical to globally
        /// search and replace all variable names in Conditions and Script fields. In this case,
        /// you can revert the fix (and not replace "/" with "_") by setting this bool to false.
        /// </summary>
        public static bool replaceSlashWithUnderscore = true;

        /// <summary>
        /// The status table used by the Chat Mapper GetStatus() and SetStatus() Lua functions. 
        /// See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// </summary>
        private static Dictionary<string, string> statusTable = new Dictionary<string, string>();

        /// <summary>
        /// The relationship table used by the Chat Mapper Set/Get/Inc/DecRelationship() functions. 
        /// See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// </summary>
        private static Dictionary<string, float> relationshipTable = new Dictionary<string, float>();

        // If we're delaying Lua function registration while starting a conversation, cache the
        // actor and conversant so we can set them after registering.
        private static bool isRegistering = false;
        private static bool hasCachedParticipants = false;
        private static string cachedActorName;
        private static string cachedConversantName;
        private static string cachedActorIndex;
        private static string cachedConversantIndex;

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            isRegistering = false;
            hasCachedParticipants = false;
            includeSimStatus = true;
            statusTable = new Dictionary<string, string>();
            relationshipTable = new Dictionary<string, float>();
            if (DialogueManager.instance != null) DialogueManager.instance.StartCoroutine(RegisterLuaFunctionsAfterFrame());
            else RegisterLuaFunctions();
        }
#endif

        /// <summary>
        /// Initializes the DialogueLua class. Registers the Chat Mapper functions (xxxStatus() and 
        /// xxxRelationship()), and initializes the Chat Mapper tables.
        /// </summary>
        static DialogueLua()
        {
            InitializeChatMapperVariables();
            RegisterLuaFunctions();
        }

        public static void RegisterLuaFunctions()
        {
            var originalWarnValue = Lua.warnRegisteringExistingFunction;
            Lua.warnRegisteringExistingFunction = false;
            RegisterChatMapperFunctions();
            RegisterDialogueSystemFunctions();
            Lua.warnRegisteringExistingFunction = originalWarnValue;
        }

        static System.Collections.IEnumerator RegisterLuaFunctionsAfterFrame()
        {
            isRegistering = true;
            yield return new WaitForEndOfFrame();
            RegisterLuaFunctions();
            isRegistering = false;
            if (hasCachedParticipants)
            {
                hasCachedParticipants = false;
                SetParticipants(cachedActorName, cachedConversantName, cachedActorIndex, cachedConversantIndex);
            }
        }

        /// <summary>
        /// Initializes the Chat Mapper tables.
        /// </summary>
        public static void InitializeChatMapperVariables()
        {
            Lua.Run("Actor = {}; Item = {}; Quest = Item; Location = {}; Conversation = {}; Variable = {}; Variable[\"Alert\"] = \"\"", DialogueDebug.LogInfo);
            Lua.Run("unassigned='unassigned'; active='active'; success='success'; failure='failure'; abandoned='abandoned'", DialogueDebug.LogInfo);
            statusTable.Clear();
            relationshipTable.Clear();
        }

        /// <summary>
        /// Adds the assets defined in a DialogueDatabase to the Chat Mapper tables. Doesn't add 
        /// assets that are contained in loadedDatabases. Runs the global user script if defined.
        /// </summary>
        /// <param name='database'>
        /// The DialogueDatabase containing assets to add.
        /// </param>
        /// <param name='loadedDatabases'>
        /// List of databases that have already been loaded and added to Lua.
        /// </param>
        public static void AddChatMapperVariables(DialogueDatabase database, List<DialogueDatabase> loadedDatabases)
        {
            if (database == null) return;
            var first = (loadedDatabases == null || loadedDatabases.Count == 0);
            AddToTable<Actor>("Actor", database.actors, loadedDatabases, first);
            AddToTable<Item>("Item", database.items, loadedDatabases, first);
            AddToTable<Location>("Location", database.locations, loadedDatabases, first);
            AddToVariableTable(database.variables, loadedDatabases, first);
            AddToConversationTable(database.conversations, loadedDatabases, first);
            if (!string.IsNullOrEmpty(database.globalUserScript)) Lua.Run(database.globalUserScript, DialogueDebug.LogInfo);
        }

        /// <summary>
        /// Removes the assets defined in a DialogueDatabase from the Chat Mapper tables. Doesn't
        /// remove any assets that are contains in loadedDatabases.
        /// </summary>
        /// <param name='database'>
        /// A DialogueDatabase containing assets to remove.
        /// </param>
        /// <param name='loadedDatabases'>
        /// List of databases containing items that should be kept in Lua.
        /// </param>
        public static void RemoveChatMapperVariables(DialogueDatabase database, List<DialogueDatabase> loadedDatabases)
        {
            if (database != null)
            {
                RemoveFromTable<Actor>("Actor", database.actors, loadedDatabases);
                RemoveFromTable<Item>("Item", database.items, loadedDatabases);
                RemoveFromTable<Location>("Location", database.locations, loadedDatabases);
                RemoveFromTable<Variable>("Variable", database.variables, loadedDatabases);
                RemoveFromTable<Conversation>("Conversation", database.conversations, loadedDatabases);
            }
        }

        /// <summary>
        /// Sets the conversation participant name variables (Variable['Actor'] and
        /// Variable['Conversant']) and, optionally, participant indices in Actor table.
        /// </summary>
        /// <param name='actorName'>Actor display name.</param>
        /// <param name='conversantName'>Conversant name.</param>
        /// <param name='actorIndex'>Actor table index.</param>
        /// <param name='conversantIndex'>Conversant table index.</param>
        public static void SetParticipants(string actorName, string conversantName, string actorIndex = null, string conversantIndex = null)
        {
            //---Was: Lua.Run(string.Format("Variable[\"Actor\"] = \"{0}\"; Variable[\"Conversant\"] = \"{1}\"", new System.Object[] { DoubleQuotesToSingle(actorName), DoubleQuotesToSingle(conversantName) }), DialogueDebug.LogInfo);
            SetVariable("Actor", actorName);
            SetVariable("Conversant", conversantName);
            SetVariable("ActorIndex", StringToTableIndex(string.IsNullOrEmpty(actorIndex) ? actorName : actorIndex));
            SetVariable("ConversantIndex", StringToTableIndex(string.IsNullOrEmpty(conversantIndex) ? actorName : conversantIndex));
            if (isRegistering) // Cache participants to set after Lua funcs are registered.
            {
                hasCachedParticipants = true;
                cachedActorName = actorName;
                cachedConversantName = conversantName;
                cachedActorIndex = actorIndex;
                cachedConversantIndex = conversantIndex;
            }
        }

        /// <summary>
        /// Returns the SimStatus of a dialogue entry, or a blank string if Include Sim Status isn't ticked.
        /// </summary>
        public static string GetSimStatus(DialogueEntry dialogueEntry)
        {
            return (dialogueEntry != null) ? GetSimStatus(dialogueEntry.conversationID, dialogueEntry.id) : string.Empty;
        }

        /// <summary>
        /// Returns the SimStatus of a dialogue entry, or a blank string if Include Sim Status isn't ticked.
        /// </summary>
        public static string GetSimStatus(int conversationID, int entryID)
        {
            if (includeSimStatus)
            {
                var simStatusTable = GetSimStatusTable(conversationID, entryID);
                var result = simStatusTable.GetValue(DialogueLua.SimStatus) as LuaString;
                if (result != null) return result.Text;
            }
            return Untouched;
        }

        private static LuaTable GetSimStatusTable(int conversationID, int entryID)
        {
            LuaTable conversationTable = Lua.Environment.GetValue("Conversation") as LuaTable;
            LuaTable conversationFieldsTable = conversationTable.GetValue(conversationID) as LuaTable;
            if (conversationFieldsTable == null) // Missing conversation.
            {
                AddToConversationTable(conversationTable, DialogueManager.masterDatabase.GetConversation(conversationID), true);
                conversationFieldsTable = conversationTable.GetValue(conversationID) as LuaTable;
            }
            LuaTable dialogTable = (conversationFieldsTable != null)
                ? conversationFieldsTable.GetValue("Dialog") as LuaTable
                : AddToConversationTable(conversationTable, DialogueManager.MasterDatabase.GetConversation(conversationID), true);
            if (dialogTable == null) // Missing Dialog[] table.
            {
                dialogTable = AddNewDialogTableToConversationFields(conversationFieldsTable, DialogueManager.masterDatabase.GetConversation(conversationID));
            }
            var simStatusTable = dialogTable.GetValue(entryID) as LuaTable;
            if (simStatusTable == null)
            {
                simStatusTable = new LuaTable();
                simStatusTable.AddRaw(SimStatus, new LuaString(DialogueLua.Untouched));
                dialogTable.AddRaw(entryID, simStatusTable);
            }
            return simStatusTable;
        }

        /// <summary>
        /// Marks a dialogue entry as untouched, by setting the Lua variable 
        /// <c>Conversation[#].Dialog[#].SimStatus="Untouched"</c>, where <c>#</c> is the 
        /// conversation and/or dialogue entry ID. The ConversationModel marks entries 
        /// displayed when they are used in a conversation.
        /// </summary>
        /// <param name='dialogueEntry'>
        /// Dialogue entry to mark.
        /// </param>
        public static void MarkDialogueEntryUntouched(DialogueEntry dialogueEntry)
        {
            MarkDialogueEntry(dialogueEntry, DialogueLua.Untouched);
        }

        /// <summary>
        /// Marks a dialogue entry as displayed, by setting the Lua variable 
        /// <c>Conversation[#].Dialog[#].SimStatus="WasDisplayed"</c>, where <c>#</c> is the 
        /// conversation and/or dialogue entry ID. The ConversationModel marks entries 
        /// displayed when they are used in a conversation.
        /// </summary>
        /// <param name='dialogueEntry'>
        /// Dialogue entry to mark.
        /// </param>
        public static void MarkDialogueEntryDisplayed(DialogueEntry dialogueEntry)
        {
            MarkDialogueEntry(dialogueEntry, DialogueLua.WasDisplayed);
        }

        /// <summary>
        /// Marks a dialogue entry as offered, by setting the Lua variable
        /// <c>Conversation[#].Dialog[#].SimStatus="WasOffered"</c> , where <c>#</c> is the 
        /// conversation and/or dialogue entry ID. If the value is already "WasDisplayed", this 
        /// method leaves it as "WasDisplayed". The ConversationModel marks entries as 
        /// offered when they are presented to the player as a response option.
        /// </summary>
        /// <param name='dialogueEntry'>
        /// Dialogue entry to mark.
        /// </param>
        public static void MarkDialogueEntryOffered(DialogueEntry dialogueEntry)
        {
            if (includeSimStatus && (dialogueEntry != null))
            {
                string simStatus = GetSimStatus(dialogueEntry);
                if (!string.Equals(simStatus, DialogueLua.WasDisplayed))
                {
                    MarkDialogueEntry(dialogueEntry, DialogueLua.WasOffered);
                }
            }
        }

        /// <summary>
        /// Marks a dialogue entry with a status such as "WasOffered". Typically you will
        /// use the more-specific functions MarkDialogueEntryDisplayed, 
        /// MarkDialogueEntryOffered, and MarkDialogueEntryUntouched instead of this method.
        /// </summary>
        /// <param name="dialogueEntry">Dialogue entry to mark</param>
        /// <param name="status">Mark entry with this status ("Untouched", "WasOffered", or "WasDisplayed")</param>
		public static void MarkDialogueEntry(DialogueEntry dialogueEntry, string status)
        {
            if (includeSimStatus && (dialogueEntry != null))
            {
                var simStatusTable = GetSimStatusTable(dialogueEntry.conversationID, dialogueEntry.id);
                simStatusTable.SetNameValue(SimStatus, new LuaString(status));
            }
        }

        private static HashSet<string> GetExistingAssetNames(string arrayName, List<DialogueDatabase> loadedDatabases)
        {
            if (loadedDatabases == null || loadedDatabases.Count == 0) return null;
            HashSet<string> existingAssetNames = new HashSet<string>();
            switch (arrayName)
            {
                case "Actor":
                    foreach (var db in loadedDatabases)
                    {
                        foreach (var actor in db.actors)
                        {
                            existingAssetNames.Add(actor.Name);
                        }
                    }
                    break;
                case "Item":
                    foreach (var db in loadedDatabases)
                    {
                        foreach (var item in db.items)
                        {
                            existingAssetNames.Add(item.Name);
                        }
                    }
                    break;
                case "Location":
                    foreach (var db in loadedDatabases)
                    {
                        foreach (var location in db.locations)
                        {
                            existingAssetNames.Add(location.Name);
                        }
                    }
                    break;
                case "Variable":
                    foreach (var db in loadedDatabases)
                    {
                        foreach (var variable in db.variables)
                        {
                            existingAssetNames.Add(variable.Name);
                        }
                    }
                    break;
            }
            return existingAssetNames;
        }

        private static void AddToTable<T>(string arrayName, List<T> assets, List<DialogueDatabase> loadedDatabases, bool addRaw) where T : Asset
        {
            // Note: No longer overwrite existing values.
            Lua.WasInvoked = true;
            LuaTable assetTable = Lua.Environment.GetValue(arrayName) as LuaTable;
            if (assetTable == null) return;
            var existingAssetNames = GetExistingAssetNames(arrayName, loadedDatabases);
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var assetName = asset.Name;
                if (existingAssetNames != null && existingAssetNames.Contains(assetName)) continue;
                var assetIndex = StringToTableIndex(assetName);
                LuaTable fieldTable = new LuaTable();
                for (int j = 0; j < asset.fields.Count; j++)
                {
                    var field = asset.fields[j];
                    var fieldIndex = StringToFieldName(field.title);
                    fieldTable.AddRaw(fieldIndex, GetFieldLuaValue(field));
                }
                if (addRaw)
                {
                    assetTable.AddRaw(assetIndex, fieldTable);
                }
                else
                {
                    assetTable.SetKeyValue(new LuaString(assetIndex), fieldTable);
                }
            }
        }

        private static void AddToVariableTable(List<Variable> variables, List<DialogueDatabase> loadedDatabases, bool addRaw)
        {
            // Note: No longer overwrite existing values.
            Lua.WasInvoked = true;
            LuaTable assetTable = Lua.Environment.GetValue("Variable") as LuaTable;
            if (assetTable == null) return;
            var existingAssetNames = GetExistingAssetNames("Variable", loadedDatabases);
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                var variableName = variable.Name;
                if (existingAssetNames != null && existingAssetNames.Contains(variableName)) continue;
                string variableIndex = StringToTableIndex(variableName);
                if (addRaw)
                {
                    assetTable.AddRaw(variableIndex, GetVariableLuaValue(variable));
                }
                else
                {
                    assetTable.SetNameValue(variableIndex, GetVariableLuaValue(variable));
                }
            }
        }

        public static void AddToConversationTable(List<Conversation> conversations, List<DialogueDatabase> loadedDatabases, bool addRaw = false)
        {
            // Note: Optimized: Overwrite existing values.
            Lua.WasInvoked = true;
            LuaTable conversationTable = Lua.Environment.GetValue("Conversation") as LuaTable;
            if (conversationTable == null) return;
            for (int i = 0; i < conversations.Count; i++)
            {
                var conversation = conversations[i];
                AddToConversationTable(conversationTable, conversation, addRaw);
            }
        }

        // Returns Dialog table if applicable.
        public static LuaTable AddToConversationTable(LuaTable conversationTable, Conversation conversation, bool addRaw = false)
        {
            LuaTable dialogTable = null;

            // Add fields to new conversation field table:
            LuaTable fieldTable = new LuaTable();
            for (int j = 0; j < conversation.fields.Count; j++)
            {
                var field = conversation.fields[j];
                var fieldIndex = StringToFieldName(field.title);
                fieldTable.AddRaw(fieldIndex, GetFieldLuaValue(field));
            }

            // Add Dialog SimStatus sub-table as a field to the new conversation field table:
            if (includeSimStatus)
            {
                dialogTable = AddNewDialogTableToConversationFields(fieldTable, conversation);
            }

            // Add conversation to Conversation[] table:
            if (addRaw)
            {
                conversationTable.AddRaw(conversation.id, fieldTable);
            }
            else
            {
                conversationTable.SetKeyValue(new LuaNumber(conversation.id), fieldTable);
            }
            return dialogTable;
        }

        private static LuaTable AddNewDialogTableToConversationFields(LuaTable fieldTable, Conversation conversation)
        {
            var dialogTable = new LuaTable();
            for (int j = 0; j < conversation.dialogueEntries.Count; j++)
            {
                var dialogueEntry = conversation.dialogueEntries[j];
                // [NOTE] To reduce Lua memory use, we only record SimStatus of dialogue entries:
                var simStatusTable = new LuaTable();
                simStatusTable.AddRaw(DialogueLua.SimStatus, new LuaString(DialogueLua.Untouched));
                dialogTable.AddRaw(dialogueEntry.id, simStatusTable);
            }
            fieldTable.AddRaw("Dialog", dialogTable);
            return dialogTable;
        }

        public static LuaValue GetFieldLuaValue(Field field)
        {
            if (field == null) return LuaNil.Nil;
            switch (field.type)
            {
                case FieldType.Actor:
                case FieldType.Item:
                case FieldType.Location:
                case FieldType.Number:
                    return new LuaNumber(Tools.StringToFloat(field.value));
                case FieldType.Boolean:
                    return Tools.StringToBool(field.value) ? LuaBoolean.True : LuaBoolean.False;
                default:
                    return new LuaString(field.value);
            }
        }

        private static LuaValue GetVariableLuaValue(Variable field)
        {
            if (field == null) return LuaNil.Nil;
            switch (field.Type)
            {
                case FieldType.Actor:
                case FieldType.Item:
                case FieldType.Location:
                case FieldType.Number:
                    return new LuaNumber(Tools.StringToFloat(field.InitialValue));
                case FieldType.Boolean:
                    return Tools.StringToBool(field.InitialValue) ? LuaBoolean.True : LuaBoolean.False;
                default:
                    return new LuaString(field.InitialValue);
            }
        }

        private static void SetFields(string record, List<Field> fields, string extraField = null)
        {
            StringBuilder sb = new StringBuilder(string.Format("{0} = {{ Status = \"\", ", new System.Object[] { record }), 1024);
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (!string.IsNullOrEmpty(field.title))
                {
                    sb.AppendFormat("{0} = {1}, ", new System.Object[] { StringToFieldName(field.title), FieldValueAsString(field) });
                }
            }
            if (!string.IsNullOrEmpty(extraField)) sb.Append(extraField);
            sb.Append('}');
            Lua.Run(sb.ToString(), DialogueDebug.LogInfo);
        }

        /// <summary>
        /// Returns a field's value as a string for use in Lua. If it's a numeric field, the string 
        /// will just contain the number. If it's a Boolean, it will be true or false (lowercase).
        /// Otherwise it will be a string enclosed in double-quotes, with any internal double 
        /// quotes converted to single quotes.
        /// </summary>
        /// <returns>
        /// The value as a string.
        /// </returns>
        /// <param name='field'>
        /// The field.
        /// </param>
        public static string FieldValueAsString(Field field)
        {
            return ValueAsString(field.type, field.value);
        }

        /// <summary>
        /// Returns a Chat Mapper value as a string for use in Lua. If it's a numeric field, the string 
        /// will just contain the number. If it's a Boolean, it will be true or false (lowercase).
        /// Otherwise it will be a string enclosed in double-quotes, with any internal double 
        /// quotes converted to single quotes.
        /// </summary>
        /// <returns>
        /// The value as a string.
        /// </returns>
        /// <param name='fieldType'>
        /// Field type.
        /// </param>
        /// <param name='fieldValue'>
        /// Field value.
        /// </param>
        public static string ValueAsString(FieldType fieldType, string fieldValue)
        {
            switch (fieldType)
            {
                case FieldType.Actor:
                case FieldType.Item:
                case FieldType.Location:
                case FieldType.Number: return string.IsNullOrEmpty(fieldValue) ? "0" : fieldValue;
                case FieldType.Boolean: return string.IsNullOrEmpty(fieldValue) ? "false" : fieldValue.ToLower();
                default: return string.Format("\"{0}\"", new System.Object[] { DoubleQuotesToSingle(fieldValue) });
            }
        }

        private static void RemoveFromTable<T>(string arrayName, List<T> assets, List<DialogueDatabase> loadedDatabases) where T : Asset
        {
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (!DialogueDatabase.Contains(loadedDatabases, asset))
                {
                    if (asset is Conversation)
                    {
                        Lua.Run(string.Format("{0}[{1}] = nil", new System.Object[] { arrayName, asset.id }), DialogueDebug.LogInfo);
                    }
                    else
                    {
                        Lua.Run(string.Format("{0}[\"{1}\"] = nil", new System.Object[] { arrayName, StringToTableIndex(asset.Name) }), DialogueDebug.LogInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Registers the Dialogue System functions into the Lua environment.
        /// </summary>
        private static void RegisterDialogueSystemFunctions()
        {
            // Unity 2017.3 bug IL2CPP can't do lambdas:
            //Lua.RegisterFunction("RandomElement", null, SymbolExtensions.GetMethodInfo(() => RandomElement(string.Empty)));
            //Lua.RegisterFunction("GetLocalizedText", null, SymbolExtensions.GetMethodInfo(() => GetLocalizedText(string.Empty, string.Empty, string.Empty)));

            Lua.RegisterFunction("RandomElement", null, typeof(DialogueLua).GetMethod("RandomElement"));
            Lua.RegisterFunction("GetLocalizedText", null, typeof(DialogueLua).GetMethod("GetLocalizedText"));
        }

        /// <summary>
        /// Returns a random element in a list. The list is defined in a string.
        /// </summary>
        /// <returns>A random element.</returns>
        /// <param name="args">An array of one LuaValue which is a string with elements
        /// separated by the horizontal bar character ('|').</param>
        public static string RandomElement(string list)
        {
            if (string.IsNullOrEmpty(list)) return string.Empty;
            string[] elements = list.Split(new char[] { '|' }, System.StringSplitOptions.None);
            return elements[Random.Range(0, elements.Length)];
        }

        /// <summary>
        /// Gets the localized text for a field in a table element.
        /// </summary>
        /// <returns>The localized text.</returns>
        /// <param name="tableName">Table name.</param>
        /// <param name="elementName">Element name.</param>
        /// <param name="fieldName">Field name.</param>
        public static string GetLocalizedText(string tableName, string elementName, string fieldName)
        {
            return GetLocalizedTableField(tableName, elementName, fieldName).AsString;
        }

        /// <summary>
        /// Registers the Chat Mapper functions into the Lua environment.
        /// </summary>
        private static void RegisterChatMapperFunctions()
        {
            // Unity 2017.3 bug IL2CPP can't do lambdas:
            //Lua.RegisterFunction("GetStatus", null, SymbolExtensions.GetMethodInfo(() => GetStatus(null, null)));
            //Lua.RegisterFunction("SetStatus", null, SymbolExtensions.GetMethodInfo(() => SetStatus(null, null, null)));
            //Lua.RegisterFunction("GetRelationship", null, SymbolExtensions.GetMethodInfo(() => GetRelationship(null, null, null)));
            //Lua.RegisterFunction("SetRelationship", null, SymbolExtensions.GetMethodInfo(() => SetRelationship(null, null, null, 0)));
            //Lua.RegisterFunction("IncRelationship", null, SymbolExtensions.GetMethodInfo(() => IncRelationship(null, null, null, 0)));
            //Lua.RegisterFunction("DecRelationship", null, SymbolExtensions.GetMethodInfo(() => DecRelationship(null, null, null, 0)));

            Lua.RegisterFunction("GetStatus", null, typeof(DialogueLua).GetMethod("GetStatus"));
            Lua.RegisterFunction("SetStatus", null, typeof(DialogueLua).GetMethod("SetStatus"));
            Lua.RegisterFunction("GetRelationship", null, typeof(DialogueLua).GetMethod("GetRelationship"));
            Lua.RegisterFunction("SetRelationship", null, typeof(DialogueLua).GetMethod("SetRelationship"));
            Lua.RegisterFunction("IncRelationship", null, typeof(DialogueLua).GetMethod("IncRelationship"));
            Lua.RegisterFunction("DecRelationship", null, typeof(DialogueLua).GetMethod("DecRelationship"));
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// </summary>
        private static string GetStatusKey(LuaTable asset1, LuaTable asset2)
        {
            if ((asset1 == null) || (asset2 == null))
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": Syntax error in status function");
                return "INVALID";
            }
            string asset1Name = StringToTableIndex(asset1.GetValue("Name").ToString());
            string asset2Name = StringToTableIndex(asset2.GetValue("Name").ToString());
            return asset1Name + ',' + asset2Name;
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// </summary>
        private static string GetRelationshipKey(LuaTable actor1, LuaTable actor2, string relationshipType)
        {
            if ((actor1 == null) || (actor2 == null) || (relationshipType == null))
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": Syntax error in relationship function");
                return "INVALID";
            }
            string actor1Name = StringToTableIndex(actor1.GetValue("Name").ToString());
            string actor2Name = StringToTableIndex(actor2.GetValue("Name").ToString());
            string relationshipString = SanitizeForStatusTable(relationshipType.ToString());
            return string.Format("{0},{1},'{2}'", new System.Object[] { actor1Name, actor2Name, relationshipString });
        }

        private static string SanitizeForStatusTable(string s)
        {
            return string.Join("_", s.Split(',', ';', '"', '\''));
        }

        private static float GetLuaFloat(LuaNumber luaNumber)
        {
            return (luaNumber != null) ? (float)luaNumber.Number : 0;
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// 
        /// Syntax:
        /// - string GetStatus(asset1, asset2)
        /// </summary>
        public static string GetStatus(LuaTable asset1, LuaTable asset2)
        {
            string key = GetStatusKey(asset1, asset2);
            return statusTable.ContainsKey(key) ? statusTable[key] : string.Empty;
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// 
        /// Syntax:
        /// - void SetStatus(asset1, asset2, "statusValue")
        /// </summary>
        public static void SetStatus(LuaTable asset1, LuaTable asset2, string statusValue)
        {
            string key = GetStatusKey(asset1, asset2);
            statusTable[key] = statusValue ?? string.Empty;
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// 
        /// Syntax:
        /// - number GetRelationship(actor1, actor2, "relationshipType")
        /// </summary>
        public static float GetRelationship(LuaTable actor1, LuaTable actor2, string relationshipType)
        {
            string key = GetRelationshipKey(actor1, actor2, relationshipType);
            return relationshipTable.ContainsKey(key) ? relationshipTable[key] : 0;
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// 
        /// Syntax:
        /// - void SetRelationship(actor1, actor2, "relationshipType", number)
        /// </summary>
        public static void SetRelationship(LuaTable actor1, LuaTable actor2, string relationshipType, float value)
        {
            relationshipTable[GetRelationshipKey(actor1, actor2, relationshipType)] = value;
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// 
        /// Syntax:
        /// - void IncRelationship(actor1, actor2, "relationshipType", incrementAmount)
        /// </summary>
        public static void IncRelationship(LuaTable actor1, LuaTable actor2, string relationshipType, float incrementAmount)
        {
            string key = GetRelationshipKey(actor1, actor2, relationshipType);
            if (relationshipTable.ContainsKey(key))
            {
                relationshipTable[key] += incrementAmount;
            }
            else
            {
                relationshipTable.Add(key, incrementAmount);
            }
        }

        /// <summary>
        /// Chat Mapper function. See the online Chat Mapper manual for more details: http://www.chatmapper.com
        /// 
        /// Syntax:
        /// - void DecRelationship(actor1, actor2, "relationshipType", decrementAmount)
        /// </summary>
        public static void DecRelationship(LuaTable actor1, LuaTable actor2, string relationshipType, float decrementAmount)
        {
            string key = GetRelationshipKey(actor1, actor2, relationshipType);
            if (relationshipTable.ContainsKey(key))
            {
                relationshipTable[key] -= decrementAmount;
            }
            else
            {
                relationshipTable.Add(key, -decrementAmount);
            }
        }

        /// <summary>
        /// Gets the Lua commands necessary to rebuild the status table.
        /// </summary>
        /// <returns>
        /// The Lua code.
        /// </returns>
        public static string GetStatusTableAsLua()
        {
            StringBuilder sb = new StringBuilder(1024, 65536);
            sb.Append("StatusTable = \"");
            foreach (KeyValuePair<string, string> kvp in statusTable)
            {
                sb.AppendFormat("{0},'{1}';", new System.Object[] { kvp.Key, SanitizeForStatusTable(kvp.Value) });
            }
            sb.Append("\"; ");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the Lua commands necessary to rebuild the relationship table.
        /// </summary>
        /// <returns>
        /// The Lua code.
        /// </returns>
        public static string GetRelationshipTableAsLua()
        {
            StringBuilder sb = new StringBuilder(1024, 65536);
            sb.Append("RelationshipTable = \"");
            foreach (KeyValuePair<string, float> kvp in relationshipTable)
            {
                sb.AppendFormat("{0},{1};", new System.Object[] { kvp.Key, kvp.Value });
            }
            sb.Append("\"; ");
            return sb.ToString();
        }

        public static void RefreshStatusTableFromLua()
        {
            statusTable.Clear();
            string statusTableString = Lua.Run("return StatusTable").AsString;
            char[] semicolons = new char[] { ';' };
            char[] commas = new char[] { ',' };
            foreach (var entry in statusTableString.Split(semicolons))
            {
                string[] fields = entry.Split(commas);
                if (fields.Length > 2)
                {
                    string key = fields[0] + ',' + fields[1];
                    string statusString = fields[2].Substring(1, fields[2].Length - 2);
                    statusTable[key] = statusString;
                }
            }
        }

        public static void RefreshRelationshipTableFromLua()
        {
            relationshipTable.Clear();
            string relationshipTableString = Lua.Run("return RelationshipTable").AsString;
            char[] semicolons = new char[] { ';' };
            char[] commas = new char[] { ',' };
            foreach (var entry in relationshipTableString.Split(semicolons))
            {
                string[] fields = entry.Split(commas);
                if (fields.Length > 3)
                {
                    string key = string.Format("{0},{1},{2}", new string[] { fields[0], fields[1], fields[2] });
                    float value = Tools.StringToFloat(fields[3]);
                    relationshipTable[key] = value;
                }
            }
        }

        /// <summary>
        /// Replaces all double-quotes with escaped double-quotes, newlines with escapes newlines, 
        /// and removes carriage returns. This utility function is used to pre-process strings
        /// before inserting them in Lua commands to help prevent syntax errors. 
        /// <em>Note:</em>: This method previously replaced all double-quotes with single-quotes. 
        /// The same method name was kept to prevent any customer code that uses it from breaking.
        /// </summary>
        /// <returns>
        /// The string, but with all double-quotes converted to escaped double-quotes.
        /// </returns>
        /// <param name='s'>
        /// The original string.
        /// </param>
        public static string DoubleQuotesToSingle(string s)
        {
            //---Was: return (string.IsNullOrEmpty(s)) ? string.Empty : s.Replace('"', '\'');
            return (string.IsNullOrEmpty(s)) ? string.Empty : s.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", string.Empty);
        }

        /// <summary>
        /// Replaces all spaces with underscores.
        /// </summary>
        /// <returns>
        /// The string with all spaces replaced by underscores.
        /// </returns>
        /// <param name='s'>
        /// The original string.
        /// </param>
        public static string SpacesToUnderscores(string s)
        {
            return (string.IsNullOrEmpty(s)) ? string.Empty : s.Replace(' ', '_');
        }

        /// <summary>
        /// Returns the version of a string usable as the index to a Chat Mapper table. This 
        /// utility function is used when looking up assets in tables to make it consistent 
        /// with Chat Mapper. For example, if you have an actor named "Adam Birch", Chat 
        /// Mapper creates a table entry <c>Actor["Adam_Birch"]</c> (note the underscore).
        /// </summary>
        /// <returns>
        /// The string as it should be used to find an asset in a Chat Mapper table, with
        /// double-quotes replaced by single-quotes, spaces with underscores, and hyphens 
        /// with underscores.
        /// </returns>
        /// <param name='s'>
        /// The original asset/field name.
        /// </param>
        public static string StringToTableIndex(string s)
        {
            if (replaceSlashWithUnderscore)
            {
                return string.IsNullOrEmpty(s) ? string.Empty : SpacesToUnderscores(DoubleQuotesToSingle(s.Replace('\"', '_'))).Replace('-', '_').
                    Replace('(', '_').Replace(')', '_').Replace("/", "_");
            }
            else
            {
                return string.IsNullOrEmpty(s) ? string.Empty : SpacesToUnderscores(DoubleQuotesToSingle(s.Replace('\"', '_'))).Replace('-', '_').
                    Replace('(', '_').Replace(')', '_');
            }
        }

        /// <summary>
        /// Returns a StringToTableIndex() value after adding the current language code
        /// to the end of s.
        /// </summary>
        /// <returns>The table index for the localized entry.</returns>
        /// <param name="s">The original asset/field name.</param>
        public static string StringToLocalizedTableIndex(string s)
        {
            if (Localization.IsDefaultLanguage || string.IsNullOrEmpty(s))
            {
                return StringToTableIndex(s);
            }
            else
            {
                return StringToTableIndex(s + "_" + Localization.Language);
            }
        }

        /// <summary>
        /// Returns the version of a string usable as the field of an element in a Chat Mapper table.
        /// </summary>
        public static string StringToFieldName(string s)
        {
            return StringToTableIndex(s).Replace('.', '_');
        }

        /// <summary>
        /// Returns a StringToFieldName() value after adding the current language code
        /// to the end of s.
        /// </summary>
        public static string StringToLocalizedFieldName(string s)
        {
            if (Localization.IsDefaultLanguage || string.IsNullOrEmpty(s))
            {
                return StringToFieldName(s);
            }
            else
            {
                return StringToFieldName(s + "_" + Localization.Language);
            }
        }

        //--- Unused now that methods are optimized to use LuaInterpreter directly.
        //private static Lua.Result SafeGetLuaResult(string luaCode) {
        //	try {
        //		return Lua.Run(luaCode, DialogueDebug.LogInfo);
        //	} catch (System.Exception) {
        //		return Lua.NoResult;
        //	}
        //}

        /// <summary>
        /// Checks if a table element exists.
        /// </summary>
        /// <returns><c>true</c>, if the table entry exists, <c>false</c> otherwise.</returns>
        /// <param name="table">Table name (e.g., "Actor").</param>
        /// <param name="element">Element name (e.g., "Player").</param>
        public static bool DoesTableElementExist(string table, string element)
        {
            LuaTable luaTable = Lua.Environment.GetValue(table) as LuaTable;
            return (luaTable != null) ? (luaTable.GetKey(StringToTableIndex(element)) != LuaNil.Nil) : false;
            //--- Was (unoptimized):
            //LuaTableWrapper luaTable = SafeGetLuaResult(string.Format("return {0}", new System.Object[] { table })).AsTable;
            //if (luaTable != null) {
            //	string tableIndex = StringToTableIndex(element);
            //	foreach (var o in luaTable.Keys) {
            //		if ((o.GetType() == typeof(string)) && (string.Equals((string) o, tableIndex))) return true;
            //	}
            //}
            //return false;
        }

        /// <summary>
        /// Gets the value of a field in a Lua table element.
        /// </summary>
        /// <returns>The field value as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="table">Table name.</param>
        /// <param name="element">Name of an element in the table.</param>
        /// <param name="field">Name of a field in the element.</param>
        public static Lua.Result GetTableField(string table, string element, string field)
        {
            LuaTable luaTable = Lua.Environment.GetValue(table) as LuaTable;
            if (luaTable == null) return Lua.NoResult;
            LuaTable elementTable = luaTable.GetValue(StringToTableIndex(element)) as LuaTable;
            if (elementTable == null) return Lua.NoResult;
            LuaValue luaValue = elementTable.GetValue(StringToTableIndex(field));
            return (luaValue != null && luaValue != LuaNil.Nil) ? new Lua.Result(luaValue) : Lua.NoResult;
            //--- Was (unoptimized):
            //string tableIndex = StringToTableIndex(element);
            //LuaTableWrapper luaTable = SafeGetLuaResult(string.Format("return {0}", new System.Object[] { table })).AsTable;
            //if (luaTable == null) return Lua.NoResult;
            //foreach (var o in luaTable.Keys) {
            //	if ((o.GetType() == typeof(string)) && (string.Equals((string) o, tableIndex))) {
            //		return SafeGetLuaResult(string.Format("return {0}[\"{1}\"].{2}", new System.Object[] { table, tableIndex, StringToTableIndex(field) }));
            //	}
            //}
            //return Lua.NoResult;
        }

        /// <summary>
        /// Sets the value of a field in a Lua table element.
        /// </summary>
        /// <param name="table">Table name.</param>
        /// <param name="element">Name of an element in the table.</param>
        /// <param name="field">Name of a field in the element.</param>
        /// <param name="value">The value to set the field to.</param>
        public static void SetTableField(string table, string element, string field, object value)
        {
            Lua.WasInvoked = true;
            LuaTable luaTable = Lua.Environment.GetValue(table) as LuaTable;
            if (luaTable == null) throw new System.NullReferenceException("Table not found in Lua environment: " + table);
            LuaTable elementTable = luaTable.GetValue(StringToTableIndex(element)) as LuaTable;
            if (elementTable == null)
            {
                Lua.Run(string.Format("{0}[\"{1}\"] = {{}}", table, StringToTableIndex(element)));
                elementTable = luaTable.GetValue(StringToTableIndex(element)) as LuaTable;
                if (elementTable == null) throw new System.NullReferenceException("Unable to find or add element: " + element);
            }
            elementTable.SetNameValue(StringToTableIndex(field), LuaInterpreterExtensions.ObjectToLuaValue(value));
        }

        /// <summary>
        /// Gets the value of a field in the Lua Actor table.
        /// </summary>
        /// <returns>The actor field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="actor">Actor name.</param>
        /// <param name="field">Name of the field in the actor element.</param>
        /// <example><code>int age = GetActorField("Player", "Age").AsInt;</code></example>
        public static Lua.Result GetActorField(string actor, string field)
        {
            return GetTableField("Actor", actor, field);
        }

        /// <summary>
        /// Sets the value of a field in the Lua Actor table.
        /// </summary>
        /// <param name="actor">Actor name.</param>
        /// <param name="field">Name of the field in the actor element.</param>
        /// <param name="value">Value to set the field to.</param>
        /// <example><code>SetActorField("Player", "Age", 21);</code></example>
        public static void SetActorField(string actor, string field, object value)
        {
            SetTableField("Actor", actor, field, value);
        }

        /// <summary>
        /// Gets the value of a field in the Lua Item table.
        /// </summary>
        /// <returns>The item field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="item">Item/quest name.</param>
        /// <param name="field">Name of the field in the item element.</param>
        /// <example><code>character.xp += GetItemField("Kill 5 Rats", "XP").AsInt;</code></example>
        public static Lua.Result GetItemField(string item, string field)
        {
            return GetTableField("Item", item, field);
        }

        /// <summary>
        /// Sets the value of a field in the Lua Item table.
        /// </summary>
        /// <param name="item">Item/quest name.</param>
        /// <param name="field">Name of the field in the item element.</param>
        /// <param name="value">Value to set the field to.</param>
        /// <example><code>SetItemField("Kill 5 Rats", "XP", 500);</code></example>
        public static void SetItemField(string item, string field, object value)
        {
            SetTableField("Item", item, field, value);
        }

        /// <summary>
        /// Gets the value of a field in the Lua Quest table, which is an alias for the Item table.
        /// </summary>
        /// <returns>The field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="quest">Quest name.</param>
        /// <param name="field">Name of the field.</param>
        /// <example><code>character.xp += GetQuestField("Kill 5 Rats", "XP").AsInt;</code></example>
        public static Lua.Result GetQuestField(string quest, string field)
        {
            return GetTableField("Item", quest, field);
        }

        /// <summary>
        /// Sets the value of a field in the Lua Quest table, which is an alias for the Item table.
        /// </summary>
        /// <param name="item">Quest name.</param>
        /// <param name="field">Name of the field.</param>
        /// <param name="value">Value to set the field to.</param>
        /// <example><code>SetQuestField("Kill 5 Rats", "XP", 500);</code></example>
        public static void SetQuestField(string quest, string field, object value)
        {
            SetTableField("Item", quest, field, value);
        }

        /// <summary>
        /// Gets the value of a field in the Lua Location table.
        /// </summary>
        /// <returns>The location field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="location">Location name.</param>
        /// <param name="field">Name of the field in the location element.</param>
        public static Lua.Result GetLocationField(string location, string field)
        {
            return GetTableField("Location", location, field);
        }

        /// <summary>
        /// Sets the value of a field in the Lua Location table.
        /// </summary>
        /// <param name="location">Location name.</param>
        /// <param name="field">Name of the field in the location element.</param>
        /// <param name="value">Value to set the field to.</param>
        public static void SetLocationField(string location, string field, object value)
        {
            SetTableField("Location", location, field, value);
        }

        /// <summary>
        /// Returns a list of all runtime variable names.
        /// </summary>
        public static string[] GetAllVariables()
        {
            var list = new List<string>();
            var variableTable = Lua.Run("return Variable").asTable;
            if (variableTable != null)
            {
                list.AddRange(variableTable.keys);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Checks if a variable exists in the Lua Variable table.
        /// </summary>
        /// <returns><c>true</c> if the variable exists, <c>false</c> otherwise.</returns>
        /// <param name="variable">Variable name.</param>
        public static bool DoesVariableExist(string variable)
        {
            return DoesTableElementExist("Variable", variable);
        }

        /// <summary>
        /// Gets the value of a variable in the Lua Variable table.
        /// </summary>
        /// <returns>The variable value as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="variable">Variable name.</param>
        /// <example><code>int numKills = GetVariable("Kills").AsInt</code></example>
        public static Lua.Result GetVariable(string variable)
        {
            LuaTable luaTable = Lua.Environment.GetValue("Variable") as LuaTable;
            return (luaTable != null) ? new Lua.Result(luaTable.GetValue(StringToTableIndex(variable))) : Lua.NoResult;
            //--- Was (unoptimized):
            //return SafeGetLuaResult(string.Format("return Variable[\"{0}\"]", new System.Object[] { StringToTableIndex(variable) }));
        }

        /// <summary>
        /// Sets the value of a variable in the Lua Variable table.
        /// </summary>
        /// <param name="variable">Variable name.</param>
        /// <param name="value">Value to set the variable to.</param>
        /// <example><code>SetVariable("Met_Wizard", true);</code></example>
        public static void SetVariable(string variable, object value)
        {
            Lua.WasInvoked = true;
            LuaTable luaTable = Lua.Environment.GetValue("Variable") as LuaTable;
            if (luaTable == null) return;
            luaTable.SetNameValue(StringToTableIndex(variable), LuaInterpreterExtensions.ObjectToLuaValue(value));
        }

        /// <summary>
        /// Gets the value of a localized field in a Lua table element.
        /// </summary>
        /// <returns>The field value as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="table">Table name.</param>
        /// <param name="element">Name of an element in the table.</param>
        /// <param name="field">The base name (non-localized) of a field in the element.</param>
        public static Lua.Result GetLocalizedTableField(string table, string element, string field)
        {
            //---Was: return GetTableField(table, element, StringToLocalizedTableIndex(field));
            var result = GetTableField(table, element, StringToLocalizedTableIndex(field));
            if (Localization.UseDefaultIfUndefined && ((result.luaValue == null) || (result.luaValue is LuaNil) || (result.luaValue is LuaString) && string.IsNullOrEmpty((result.luaValue as LuaString).Text)))
            {
                return GetTableField(table, element, field);
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Sets the value of a localized field in a Lua table element.
        /// </summary>
        /// <param name="table">Table name.</param>
        /// <param name="element">Name of an element in the table.</param>
        /// <param name="field">Base name (non-localized) of a field in the element.</param>
        /// <param name="value">The value to set the field to.</param>
        public static void SetLocalizedTableField(string table, string element, string field, object value)
        {
            SetTableField(table, element, StringToLocalizedTableIndex(field), value);
        }

        /// <summary>
        /// Gets the value of a localized field in the Lua Actor table.
        /// </summary>
        /// <returns>The actor field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="actor">Actor name.</param>
        /// <param name="field">Base name (non-localized) of the field in the actor element.</param>
        /// <example><code>int age = GetActorField("Player", "Age").AsInt;</code></example>
        public static Lua.Result GetLocalizedActorField(string actor, string field)
        {
            return GetLocalizedTableField("Actor", actor, field);
        }

        /// <summary>
        /// Sets the value of a localized field in the Lua Actor table.
        /// </summary>
        /// <param name="actor">Actor name.</param>
        /// <param name="field">Base name (non-localized) of the field in the actor element.</param>
        /// <param name="value">Value to set the field to.</param>
        /// <example><code>SetActorField("Player", "Age", 21);</code></example>
        public static void SetLocalizedActorField(string actor, string field, object value)
        {
            SetActorField(actor, StringToLocalizedTableIndex(field), value);
        }

        /// <summary>
        /// Gets the value of a localized field in the Lua Item table.
        /// </summary>
        /// <returns>The item field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="item">Item/quest name.</param>
        /// <param name="field">Base name (non-localized) of the field in the item element.</param>
        public static Lua.Result GetLocalizedItemField(string item, string field)
        {
            return GetLocalizedTableField("Item", item, field);
        }

        /// <summary>
        /// Sets the value of a localized field in the Lua Item table.
        /// </summary>
        /// <param name="item">Item/quest name.</param>
        /// <param name="field">Base name (non-localized) of the field in the item element.</param>
        /// <param name="value">Value to set the field to.</param>
        public static void SetLocalizedItemField(string item, string field, object value)
        {
            SetItemField(item, StringToLocalizedTableIndex(field), value);
        }

        /// <summary>
        /// Gets the value of a localized field in the Lua Quest table, which is an alias for the Item table.
        /// </summary>
        /// <returns>The field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="quest">Quest name.</param>
        /// <param name="field">Base name (non-localized) of the field.</param>
        public static Lua.Result GetLocalizedQuestField(string quest, string field)
        {
            return GetLocalizedItemField(quest, field);
        }

        /// <summary>
        /// Sets the value of a localized field in the Lua Quest table, which is an alias for the Item table.
        /// </summary>
        /// <param name="item">Quest name.</param>
        /// <param name="field">Base name (non-localized) of the field.</param>
        /// <param name="value">Value to set the field to.</param>
        public static void SetLocalizedQuestField(string quest, string field, object value)
        {
            SetLocalizedItemField(quest, field, value);
        }

        /// <summary>
        /// Gets the value of a localized field in the Lua Location table.
        /// </summary>
        /// <returns>The location field as a Lua.Result. To get a basic data type, addend <c>.AsString</c>, <c>.AsBool</c>, etc., to it.</returns>
        /// <param name="location">Location name.</param>
        /// <param name="field">Base name (non-localized) of the field in the location element.</param>
        public static Lua.Result GetLocalizedLocationField(string location, string field)
        {
            return GetLocalizedTableField("Location", location, field);
        }

        /// <summary>
        /// Sets the value of a localized field in the Lua Location table.
        /// </summary>
        /// <param name="location">Location name.</param>
        /// <param name="field">Base name (non-localized) of the field in the location element.</param>
        /// <param name="value">Value to set the field to.</param>
        public static void SetLocalizedLocationField(string location, string field, object value)
        {
            SetLocationField(location, StringToLocalizedTableIndex(field), value);
        }

        /// <summary>
        /// Gets the value of a conversation field.
        /// </summary>
        /// <param name="conversationID">Conversation ID.</param>
        /// <param name="field">Field name.</param>
        /// <returns>The value of the field.</returns>
        public static Lua.Result GetConversationField(int conversationID, string field)
        {
            return Lua.Run(string.Format("return Conversation[{0}].{1}", new System.Object[] { conversationID, StringToTableIndex(field) }), false, true);
        }

        /// <summary>
        /// Gets the value of a conversation field.
        /// </summary>
        /// <param name="conversationID">Conversation ID.</param>
		/// <param name="field">Base name (non-localized) of the field in the location element.</param>
        /// <returns>The value of the field.</returns>
        public static Lua.Result GetLocalizedConversationField(int conversationID, string field)
        {
            return Lua.Run(string.Format("return Conversation[{0}].{1}", new System.Object[] { conversationID, StringToLocalizedTableIndex(field) }), false, true);
        }
    }

}
#endif
