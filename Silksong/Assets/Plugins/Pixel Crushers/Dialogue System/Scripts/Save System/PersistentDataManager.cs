// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    public delegate string GetCustomSaveDataDelegate();

    /// <summary>
    /// A static class for saving and loading game data using the Dialogue System's 
    /// Lua environment. It allows you to save or load a game with a single line of code.
    /// 
    /// For more information, see @ref saveLoadSystem
    /// </summary>
    public static class PersistentDataManager
    {

        #region Variables

        /// <summary>
        /// Set <c>true</c> to include actor data in save data, <c>false</c> to exclude.
        /// </summary>
        public static bool includeActorData = true;

        /// <summary>
        /// Set this <c>true</c> to include all item fields in saved-game data. This is
        /// <c>false</c> by default to minimize the size of the saved-game data by only
        /// recording State and Track (for quests).
        /// </summary>
        public static bool includeAllItemData = false;

        /// <summary>
        /// Set <c>true</c> to include location data in save data, <c>false</c> to exclude.
        /// </summary>
        public static bool includeLocationData = false;

        /// <summary>
        /// Set <c>true</c> to include all conversation fields, <c>false</c> to exclude.
        /// </summary>
        public static bool includeAllConversationFields = false;

        /// <summary>
        /// Set this <c>true</c> to exclude Conversation[#].Dialog[#].SimStatus values from
        /// saved-game data. If you don't use SimStatus in your Lua conditions, there's no
        /// need to save it.
        /// </summary>
        public static bool includeSimStatus = false;

        /// <summary>
        /// Optional field to use when saving a conversation's SimStatus info (e.g., Title). 
        /// This feature is handy if you can't guarantee that conversation IDs will be the 
        /// same across saved games. If set, saves the conversation's SimStatus info into
        /// a field. If blank, uses conversation ID. 
        /// </summary>
        public static string saveConversationSimStatusWithField = string.Empty;

        /// <summary>
        /// Optional field to use when saving a dialogue entry's SimStatus info (e.g,. Title).
        /// This feature is handy if you can't guarantee that dialogue entry IDs will be the 
        /// same across saved games. If set, saves the entry's SimStatus value into a field.
        /// If blank, uses entry's ID. 
        /// </summary>
        public static string saveDialogueEntrySimStatusWithField = string.Empty;

        /// <summary>
        /// Set <c>true</c> to include the status & relationship tables in save data,
        /// <c>false</c> to exclude.
        /// </summary>
        public static bool includeRelationshipAndStatusData = true;

        /// <summary>
        /// Initialize variables and quests that were added to database after saved game.")]
        /// </summary>
        public static bool initializeNewVariables = true;

        /// <summary>
        /// Initialize new SimStatus values for entries that were added to database after saved game.
        /// </summary>
        public static bool initializeNewSimStatus = true;

        /// <summary>
        /// PersistentDataManager will call this delegate (if set) to add custom data
        /// to the saved-game data string. The custom data should be valid Lua code.
        /// </summary>
        public static GetCustomSaveDataDelegate GetCustomSaveData = null;

        public enum RecordPersistentDataOn
        {
            /// <summary>
            /// Inform all components on all GameObjects in the scene to record their persistent data
            /// if supported.
            /// </summary>
            AllGameObjects,

            /// <summary>
            /// Inform only components that have registered to receive notifications to record their
            /// persistent data.
            /// </summary>
            OnlyRegisteredGameObjects,

            /// <summary>
            /// Entirely skip informing any GameObjects to record their persistent data.
            /// </summary>
            NoGameObjects
        }

        public static RecordPersistentDataOn recordPersistentDataOn = RecordPersistentDataOn.AllGameObjects;

        private static HashSet<GameObject> listeners = new HashSet<GameObject>();

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            GetCustomSaveData = null;
            listeners = new HashSet<GameObject>();
        }
#endif

        #endregion

        #region Register Persistent Data Components

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go">GameObject that should receive notifications.</param>
        public static void RegisterPersistentData(GameObject go)
        {
            if (go == null || !Application.isPlaying) return;
            listeners.Add(go);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go">GameObject that should no longer receive notifications.</param>
        public static void UnregisterPersistentData(GameObject go)
        {
            if (!Application.isPlaying) return;
            listeners.Remove(go);
        }

        #endregion

        #region Main Data Management Methods

        /// <summary>
        /// Resets the Lua environment -- for example, when starting a new game.
        /// </summary>
        /// <param name='databaseResetOptions'>
        /// The database reset options can be:
        /// 
        /// - RevertToDefault: Removes all but the default database, then resets it.
        /// - KeepAllLoaded: Keeps all loaded databases in memory and just resets them.
        /// </param>
        public static void Reset(DatabaseResetOptions databaseResetOptions)
        {
            DialogueManager.ResetDatabase(databaseResetOptions);
        }

        /// <summary>
        /// Resets the Lua environment -- for example, when starting a new game -- keeping all loaded database
        /// in memory and just resetting them.
        /// </summary>
        public static void Reset()
        {
            Reset(DatabaseResetOptions.KeepAllLoaded);
        }

        /// <summary>
        /// Sends the OnRecordPersistentData message to all GameObjects in the scene to give them 
        /// an opportunity to record their state in the Lua environment. You can limit which GameObjects
        /// receive messages by changing recordPersistentDataOn.
        /// </summary>
        public static void Record()
        {
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Recording persistent data to Lua environment.", new System.Object[] { DialogueDebug.Prefix }));
            SendPersistentDataMessage("OnRecordPersistentData");
        }

        /// <summary>
        /// Sends the OnApplyPersistentData message to all game objects in the scene to give them an 
        /// opportunity to retrieve their state from the Lua environment. If calling this after loading
        /// a new scene, you may want to wait one frame to allow other GameObject's Start methods
        /// to complete first. You can limit which GameObjects receive messages by changing 
        /// recordPersistentDataOn.
        /// </summary>
        public static void Apply()
        {
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Applying persistent data from Lua environment.", new System.Object[] { DialogueDebug.Prefix }));
            SendPersistentDataMessage("OnApplyPersistentData");
            DialogueManager.SendUpdateTracker(); // Update quest tracker HUD.
        }

        private static void SendPersistentDataMessage(string message)
        {
            switch (recordPersistentDataOn)
            {
                case RecordPersistentDataOn.AllGameObjects:
                    Tools.SendMessageToEveryone(message);
                    break;
                case RecordPersistentDataOn.OnlyRegisteredGameObjects:
                    foreach (var go in listeners)
                    {
                        if (go != null) go.SendMessage(message, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Sends the OnLevelWillBeUnloaded message to all game objects in the scene in case they
        /// need to change their behavior. For example, scripts that do something special when 
        /// destroyed during play may not want to do the same thing when being destroyed by a 
        /// level unload.
        /// </summary>
        public static void LevelWillBeUnloaded()
        {
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Broadcasting that level will be unloaded.", new System.Object[] { DialogueDebug.Prefix }));
            SendPersistentDataMessage("OnLevelWillBeUnloaded");
        }

        /// <summary>
        /// Loads a saved game by applying a saved-game string.
        /// </summary>
        /// <param name='saveData'>
        /// A saved-game string previously returned by GetSaveData().
        /// </param>
        /// <param name='databaseResetOptions'>
        /// Database reset options.
        /// </param>
        public static void ApplySaveData(string saveData, DatabaseResetOptions databaseResetOptions = DatabaseResetOptions.KeepAllLoaded)
        {
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Resetting Lua environment.", new System.Object[] { DialogueDebug.Prefix }));
            DialogueManager.ResetDatabase(databaseResetOptions);
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Updating Lua environment with saved data.", new System.Object[] { DialogueDebug.Prefix }));
            ApplyLuaInternal(saveData);
            Apply();
        }

        /// <summary>
        /// Loads data into the Lua environment.
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="allowExceptions"></param>
        public static void ApplyLuaInternal(string saveData, bool allowExceptions = false)
        {
            if (!string.IsNullOrEmpty(saveData))
            {
                EnsureConversationTablesExistForAllSimX(saveData);
                EnsureQuestsExist(saveData);
                Lua.Run(saveData, DialogueDebug.LogInfo);
                ExpandCompressedSimStatusData();
                RefreshRelationshipAndStatusTablesFromLua();
                if (initializeNewVariables)
                {
                    InitializeNewVariablesFromDatabase();
                    InitializeNewActorFieldsFromDatabase();
                    InitializeNewQuestEntriesFromDatabase();
                    InitializeNewSimStatusFromDatabase();
                }
            }
        }

#if USE_NLUA

        /// <summary>
        /// If using SimStatus, make sure Conversation[#] elements exist for all onversations in
        /// the saved game data.
        /// </summary>
        /// <param name="saveData"></param>
        private static void EnsureConversationTablesExistForAllSimX(string saveData)
        {
            if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus)) return;

            var conversationTable = Lua.Run("return Conversation").asTable;
            if (conversationTable == null) return;
            var keysHashSet = new HashSet<string>(conversationTable.keys);

            var preLength = "Conversation[".Length;
            foreach (Match match in Regex.Matches(saveData, @"Conversation\[\d+\]"))
            {
                var idString = match.Value.Substring(preLength, match.Value.Length - (preLength + 1));
                if (!keysHashSet.Contains(idString)) Lua.Run("Conversation[" + idString + "] = {}");
            }
        }

        /// <summary>
        /// If only saving quest states (and not all item/quest data), make sure Item["x"] elements
        /// exist for all quests in the saved game data.
        /// </summary>
        /// <param name="saveData"></param>
        private static void EnsureQuestsExist(string saveData)
        {
            if (includeAllItemData || DialogueManager.Instance.persistentDataSettings.includeAllItemData) return;

            var itemTable = Lua.Run("return Item").asTable;
            if (itemTable == null) return;
            var keysHashSet = new HashSet<string>(itemTable.keys);

            var preLength = "Item[".Length;
            var postLength = "].State".Length;
            foreach (Match match in Regex.Matches(saveData, @"Item\[[^\]]+\].State"))
            {
                var s = match.Value.Substring(preLength + 1, match.Value.Length - (preLength + postLength + 2));
                if (!keysHashSet.Contains(s)) Lua.Run("Item[" + s + "] = { Name='" + s.Replace("\"", "\\\"") + "', State='unassigned' }");
            }
        }

#else

        /// <summary>
        /// If using SimStatus, make sure Conversation[#] elements exist for all onversations in
        /// the saved game data.
        /// </summary>
        /// <param name="saveData"></param>
        private static void EnsureConversationTablesExistForAllSimX(string saveData)
        {
            if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus)) return;
            var conversationTable = Lua.Environment.GetValue("Conversation") as Language.Lua.LuaTable;
            if (conversationTable == null) return;

            var preLength = "Conversation[".Length;
            foreach (Match match in Regex.Matches(saveData, @"Conversation\[\d+\]"))
            {
                var idString = match.Value.Substring(preLength, match.Value.Length - (preLength + 1));
                var key = new Language.Lua.LuaNumber(SafeConvert.ToInt(idString));
                if (!conversationTable.ContainsKey(key))
                {
                    conversationTable.SetKeyValue(key, new Language.Lua.LuaTable());
                }
            }
        }

        /// <summary>
        /// If only saving quest states (and not all item/quest data), make sure Item["x"] elements
        /// exist for all quests in the saved game data.
        /// </summary>
        /// <param name="saveData"></param>
        private static void EnsureQuestsExist(string saveData)
        {
            if (includeAllItemData || DialogueManager.Instance.persistentDataSettings.includeAllItemData) return;
            var itemTable = Lua.Environment.GetValue("Item") as Language.Lua.LuaTable;
            if (itemTable == null) return;

            var preLength = "Item[".Length;
            var postLength = "].State".Length;
            foreach (Match match in Regex.Matches(saveData, @"Item\[[^\]]+\].State"))
            {
                var s = match.Value.Substring(preLength + 1, match.Value.Length - (preLength + postLength + 2));
                if (itemTable.GetKey(s) == Language.Lua.LuaNil.Nil)
                {
                    var questKey = new Language.Lua.LuaString(s);
                    var table = new Language.Lua.LuaTable();
                    table.RawSetValue("Name", new Language.Lua.LuaString(s));
                    table.RawSetValue("State", new Language.Lua.LuaString("unassigned"));
                    itemTable.SetKeyValue(questKey, table);
                }
            }
        }


#endif

        /// <summary>
        /// Saves a game by retrieving the Lua environment and returning it as a saved-game string. 
        /// This method calls Record() to allow all game objects in the scene to record their state 
        /// to the Lua environment first. The returned string is human-readable Lua code.
        /// </summary>
        /// <returns>
        /// The saved-game data.
        /// </returns>
        /// <remarks>
        /// To reduce saved-game data size, only the following information is recorded from the 
        /// Chat Mapper tables (Item[], Actor[], etc):
        /// 
        /// - <c>Actor[]</c>: all data
        /// - <c>Item[]</c>: only <c>State</c> (for quest log system)
        /// - <c>Location[]</c>: nothing
        /// - <c>Variable[]</c>: current value of each variable
        /// - <c>Conversation[]</c>: SimStatus
        /// - Relationship and status information is recorded
        /// </remarks>
        public static string GetSaveData()
        {
            Record();
            string saveData;
            var sb = new StringBuilder();
            AppendDialogueSystemData(sb);
            saveData = sb.ToString();
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Saved data: {1}", new System.Object[] { DialogueDebug.Prefix, saveData }));
            return saveData;
        }

        #endregion

        #region Save (Non-Conversation Data)

        public static void AppendDialogueSystemData(StringBuilder sb)
        {
            if (sb == null) return;
            AppendVariableData(sb);
            AppendItemData(sb);
            AppendLocationData(sb);
            if (includeActorData) AppendActorData(sb);
            AppendConversationData(sb);
            if (includeRelationshipAndStatusData) AppendRelationshipAndStatusTables(sb);
            if (GetCustomSaveData != null) sb.Append(GetCustomSaveData());
        }

        /// <summary>
        /// Appends the user variable table to a (saved-game) string.
        /// </summary>
        public static void AppendVariableData(StringBuilder sb)
        {
            try
            {
                LuaTableWrapper variableTable = Lua.Run("return Variable").AsTable;
                if (variableTable == null)
                {
                    if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Variable[] table", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                sb.Append("Variable={");
                var first = true;
                foreach (var key in variableTable.Keys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    if (!first) sb.Append(", ");
                    first = false;
                    var value = variableTable[key.ToString()];
                    sb.AppendFormat("{0}={1}", new System.Object[] { GetFieldKeyString(key), GetFieldValueString(value) });
                }
                sb.Append("}; ");
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get variable data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        /// <summary>
        /// Appends the item table to a (saved-game) string.
        /// </summary>
        public static void AppendItemData(StringBuilder sb)
        {
            try
            {
                LuaTableWrapper itemTable = Lua.Run("return Item").AsTable;
                if (itemTable == null)
                {
                    if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Item[] table", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }

                // Cache titles of items in database:
                HashSet<string> itemsInDatabase = new HashSet<string>();
                if (!includeAllItemData)
                {
                    var database = DialogueManager.masterDatabase;
                    for (int i = 0; i < database.items.Count; i++)
                    {
                        itemsInDatabase.Add(DialogueLua.StringToTableIndex(database.items[i].Name));
                    }
                }

                // Process all items:
                foreach (var title in itemTable.Keys)
                {
                    LuaTableWrapper fields = itemTable[title.ToString()] as LuaTableWrapper;
                    bool onlySaveQuestData = !includeAllItemData && itemsInDatabase.Contains(title); //---Was: (DialogueManager.MasterDatabase.items.Find(i => string.Equals(DialogueLua.StringToTableIndex(i.Name), title)) != null);
                    if (fields != null)
                    {
                        if (onlySaveQuestData)
                        {
                            // If in the database, just record quest statuses and tracking:
                            foreach (var fieldKey in fields.Keys)
                            {
                                if (string.IsNullOrEmpty(fieldKey)) continue;
                                string fieldTitle = fieldKey.ToString();
                                if (fieldTitle.EndsWith("State"))
                                {
                                    sb.AppendFormat("Item[\"{0}\"].{1}=\"{2}\"; ", new System.Object[] { DialogueLua.StringToTableIndex(title), (System.Object)fieldTitle, (System.Object)fields[fieldTitle] });
                                }
                                else if (string.Equals(fieldTitle, "Track") || string.Equals(fieldTitle, "Viewed"))
                                {
                                    sb.AppendFormat("Item[\"{0}\"].{1}={2}; ", new System.Object[] { DialogueLua.StringToTableIndex(title), fieldTitle, fields[fieldTitle].ToString().ToLower() });
                                }
                            }
                        }
                        else
                        {
                            // If saving all data or item is not in the database, record all fields:
                            sb.AppendFormat("Item[\"{0}\"]=", new System.Object[] { DialogueLua.StringToTableIndex(title) });
                            AppendFields(sb, fields);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get item data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        private static void AppendFields(StringBuilder sb, LuaTableWrapper fields)
        {
            sb.Append("{");
            try
            {
                if (fields != null)
                {
                    foreach (var key in fields.Keys)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        var value = fields[key];
                        var valueString = GetFieldValueString(value);
                        if (string.Equals(key, "Pictures")) valueString = valueString.Replace("\\", "/"); // Sanitize backslashes in Pictures.
                        sb.AppendFormat("{0}={1}, ", new System.Object[] { GetFieldKeyString(key), valueString });
                    }
                }
            }
            finally
            {
                sb.Append("}; ");
            }

        }

        // Faster to check manually than use Regex:
        private static string GetFieldKeyString(string key)
        {
            key = DialogueLua.StringToTableIndex(key);
            return IsValidVarName(key) ? key : ("[\"" + key + "\"]");
        }

        private static bool IsValidVarName(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            char firstChar = key[0];
            if (!(firstChar == '_' || ('a' <= firstChar && firstChar <= 'z') || ('A' <= firstChar && firstChar <= 'Z'))) return false;
            for (int i = 1; i < key.Length; i++)
            {
                var c = key[i];
                if (!(c == '_' || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || ('0' <= c && c <= '9'))) return false;
            }
            return true;
        }

        private static string GetFieldValueString(object o)
        {
            if (o == null)
            {
                return "nil";
            }
            else
            {
                System.Type type = o.GetType();
                if (type == typeof(string))
                {
                    return string.Format("\"{0}\"", new System.Object[] { DialogueLua.DoubleQuotesToSingle(o.ToString().Replace("\n", "\\n").Replace("\\ ", "/ ")) });
                }
                else if (type == typeof(bool))
                {
                    return o.ToString().ToLower();
                }
                else if (type == typeof(float) || type == typeof(double))
                {
                    return ((float)o).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (type == typeof(LuaTableWrapper))
                {
                    StringBuilder sb = new StringBuilder();
                    AppendFields(sb, (LuaTableWrapper)o);
                    return "{" + sb.ToString() + "}";
                }
                else
                {
                    return o.ToString();
                }
            }
        }

        /// <summary>
        /// Appends the location table to a (saved-game) string. Currently doesn't save anything unless
        /// includeLocationData is true.
        /// </summary>
        public static void AppendLocationData(StringBuilder sb)
        {
            if (!includeLocationData) return;
            try
            {
                LuaTableWrapper locationTable = Lua.Run("return Location").AsTable;
                if (locationTable == null)
                {
                    if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Location[] table", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                sb.Append("Location={");
                var first = true;
                foreach (var key in locationTable.Keys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    LuaTableWrapper fields = locationTable[key] as LuaTableWrapper;
                    if (!first) sb.Append(", ");
                    first = false;
                    sb.Append(GetFieldKeyString(key));
                    sb.Append("={");
                    try
                    {
                        AppendAssetFieldData(sb, fields);
                    }
                    finally
                    {
                        sb.Append("}");
                    }
                }
                sb.Append("}; ");
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get location data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        /// <summary>
        /// Appends the actor table to a (saved-game) string.
        /// </summary>
        public static void AppendActorData(StringBuilder sb)
        {
            try
            {
                LuaTableWrapper actorTable = Lua.Run("return Actor").AsTable;
                if (actorTable == null)
                {
                    if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Actor[] table", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                sb.Append("Actor={");
                var first = true;
                foreach (var key in actorTable.Keys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    LuaTableWrapper fields = actorTable[key] as LuaTableWrapper;
                    if (!first) sb.Append(", ");
                    first = false;
                    sb.Append(GetFieldKeyString(key));
                    sb.Append("={");
                    try
                    {
                        AppendAssetFieldData(sb, fields);
                    }
                    finally
                    {
                        sb.Append("}");
                    }
                }
                sb.Append("}; ");
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get actor data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        /// <summary>
        /// Appends an actor record to a saved-game string.
        /// </summary>
        private static void AppendAssetFieldData(StringBuilder sb, LuaTableWrapper fields)
        {
            if (fields == null) return;
            var first = true;
            foreach (var key in fields.Keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (!first) sb.Append(", ");
                first = false;
                var value = fields[key];
                sb.AppendFormat("{0}={1}", new System.Object[] { GetFieldKeyString(key), GetFieldValueString(value) });
            }
        }

        /// <summary>
        /// Appends the relationship and status tables to a (saved-game) string.
        /// </summary>
        /// <param name="sb">StringBuilder to append to.</param>
        public static void AppendRelationshipAndStatusTables(StringBuilder sb)
        {
            try
            {
                sb.Append(DialogueLua.GetStatusTableAsLua());
                sb.Append(DialogueLua.GetRelationshipTableAsLua());
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get relationship and status data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        /// <summary>
        /// Instructs the Dialogue System to refresh its internal relationship and status tables
        /// from the values in the Lua environment. Call this after putting new values in the
        /// Lua environment, such as when loading a saved game.
        /// </summary>
        public static void RefreshRelationshipAndStatusTablesFromLua()
        {
            DialogueLua.RefreshStatusTableFromLua();
            DialogueLua.RefreshRelationshipTableFromLua();
        }

        #endregion

        #region Save (Conversation Data)

        /// <summary>
        /// Appends the conversation table to a (saved-game) string. To conserve space, only the
        /// SimStatus is recorded. If includeSimStatus is <c>false</c>, nothing is recorded.
        /// The exception is if includeAllConversationFields is true.
        /// </summary>
        public static void AppendConversationData(StringBuilder sb)
        {
            if (includeAllConversationFields || DialogueManager.Instance.persistentDataSettings.includeAllConversationFields)
            {
                AppendAllConversationFields(sb);
            }
            if (includeSimStatus && DialogueManager.Instance.includeSimStatus)
            {
                AppendSimStatus(sb);
            }
        }

        /// <summary>
        /// Appends all conversation fields to a saved-game string. Note that this doesn't
        /// append the fields inside each dialogue entry, just the fields in the conversation
        /// objects themselves.
        /// </summary>
        private static void AppendAllConversationFields(StringBuilder sb)
        {
            try
            {
                LuaTableWrapper conversationTable = Lua.Run("return Conversation").AsTable;
                if (conversationTable == null)
                {
                    if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Conversation[] table", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                foreach (var convIndex in conversationTable.Keys) // Loop through conversations:
                {
                    LuaTableWrapper fields = Lua.Run("return Conversation[" + convIndex + "]").AsTable;
                    if (fields == null) continue;
                    sb.Append("Conversation[" + convIndex + "]={");
                    try
                    {
                        var first = true;
                        foreach (var key in fields.Keys)
                        {
                            if (string.IsNullOrEmpty(key)) continue;
                            if (string.Equals(key, "Dialog")) continue;
                            if (!first) sb.Append(", ");
                            first = false;
                            var value = fields[key.ToString()];
                            sb.AppendFormat("{0}={1}", new System.Object[] { GetFieldKeyString(key), GetFieldValueString(value) });
                        }
                    }
                    finally
                    {
                        sb.Append("}; ");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get conversation data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

#if USE_NLUA

        /// <summary>
        /// Appends SimStatus for all conversations.
        /// </summary>
        private static void AppendSimStatus(StringBuilder sb)
        {
            try
            {
                var useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
                var useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
                foreach (var conversation in DialogueManager.MasterDatabase.conversations)
                {
                    if (useConversationID)
                    {
                        sb.AppendFormat("Conversation[{0}].SimX=\"", conversation.id);
                    }
                    else
                    {
                        var fieldValue = DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField));
                        if (string.IsNullOrEmpty(fieldValue)) fieldValue = conversation.id.ToString();
                        sb.AppendFormat("Variable[\"Conversation_SimX_{0}\"]=\"", fieldValue);
                    }

                    var dialogTable = Lua.Run("return Conversation[" + conversation.id + "].Dialog").asTable;

                    var first = true;
                    for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                    {
                        var entry = conversation.dialogueEntries[i];
                        var entryID = entry.id;
                        var dialogFields = dialogTable[entryID] as NLua.LuaTable;
                        if (dialogFields != null)
                        {
                            if (!first) sb.Append(";");
                            first = false;
                            sb.Append(useEntryID ? entryID.ToString() : Field.LookupValue(entry.fields, saveDialogueEntrySimStatusWithField));
                            sb.Append(";");
                            var simStatus = dialogFields[DialogueLua.SimStatus].ToString();
                            sb.Append(SimStatusToChar(simStatus));
                        }
                    }
                    sb.Append("\"; ");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get conversation data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        private static void ExpandCompressedSimStatusData()
        {
            if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus)) return;
            try
            {
                var useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
                var useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
                var entryDict = new Dictionary<string, DialogueEntry>();
                foreach (var conversation in DialogueManager.MasterDatabase.conversations)
                {
                    // If saving dialogue entries' SimStatus with value of a field, make a lookup table:
                    if (!useEntryID)
                    {
                        entryDict.Clear();
                        for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                        {
                            var entry = conversation.dialogueEntries[i];
                            var entryFieldValue = Field.LookupValue(entry.fields, saveDialogueEntrySimStatusWithField);
                            if (!entryDict.ContainsKey(entryFieldValue)) entryDict.Add(entryFieldValue, entry);
                        }
                    }
                    var sb = new StringBuilder();
                    string simX;
                    if (useConversationID)
                    {
                        simX = Lua.Run("return Conversation[" + conversation.id + "].SimX").AsString;
                    }
                    else
                    {
                        var fieldValue = DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField));
                        if (string.IsNullOrEmpty(fieldValue)) fieldValue = conversation.id.ToString();
                        simX = Lua.Run("return Variable[\"Conversation_SimX_" + fieldValue + "\"]").AsString;
                    }
                    if (string.IsNullOrEmpty(simX) || string.Equals(simX, "nil")) continue;
                    var clearSimXCommand = useConversationID ? ("Conversation[" + conversation.id + "].SimX=nil;")
                        : ("Variable[\"Conversation_SimX_" + DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField)) + "\"]=nil;");
                    sb.Append("Conversation[");
                    sb.Append(conversation.id);
                    sb.Append("].Dialog={}; ");
                    var simXFields = simX.Split(';');
                    var numFields = simXFields.Length / 2;
                    for (int i = 0; i < numFields; i++)
                    {
                        var simXEntryIDValue = simXFields[2 * i];
                        string entryID;
                        if (useEntryID)
                        {
                            entryID = simXEntryIDValue;
                        }
                        else
                        {
                            entryID = entryDict.ContainsKey(simXEntryIDValue) ? entryDict[simXEntryIDValue].id.ToString() : "-1";
                        }
                        var simStatus = CharToSimStatus(simXFields[(2 * i) + 1][0]);
                        sb.Append("Conversation[");
                        sb.Append(conversation.id);
                        sb.Append("].Dialog[");
                        sb.Append(entryID);
                        sb.Append("]={SimStatus='");
                        sb.Append(simStatus);
                        sb.Append("'}; ");
                    }
                    sb.Append(clearSimXCommand);
                    Lua.Run(sb.ToString());
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: ApplySaveData() failed to re-expand compressed SimStatus data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

#else

        // Used by SimStatus methods:
        private static bool useConversationID = true;
        private static bool useEntryID = true;

        /// <summary>
        /// Appends SimStatus for all conversations.
        /// </summary>
        private static void AppendSimStatus(StringBuilder sb)
        {
            try
            {
                useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
                useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
                var conversationTable = Lua.Environment.GetValue("Conversation") as Language.Lua.LuaTable;
                if (conversationTable == null) return;
                for (int i = 0; i < conversationTable.List.Count; i++)
                {
                    var conversationID = i + 1;
                    var fieldTable = conversationTable.List[i] as Language.Lua.LuaTable;
                    AppendSimStatusForConversation(sb, conversationTable, conversationID, fieldTable);
                }
                foreach (var kvp in conversationTable.Dict)
                {
                    if (kvp.Key == null || kvp.Value == null || !(kvp.Value is Language.Lua.LuaTable)) continue;
                    var conversationID = Tools.StringToInt(kvp.Key.ToString());
                    var fieldTable = kvp.Value as Language.Lua.LuaTable;
                    AppendSimStatusForConversation(sb, conversationTable, conversationID, fieldTable);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: GetSaveData() failed to get conversation data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        private static Dictionary<int, string> s_dialogueEntrySimStatusFieldLookupTable = new Dictionary<int, string>();

        /// <summary>
        /// Appends the SimStatus info for a single conversation.
        /// </summary>
        /// <returns>Returns the number of dialogue entries in the conversation.</returns>
        private static int AppendSimStatusForConversation(StringBuilder sb, Language.Lua.LuaTable conversationTable, int conversationID, Language.Lua.LuaTable fieldTable)
        {
            if (sb == null || conversationTable == null || fieldTable == null) return 0;
            var dialogTable = fieldTable.GetValue("Dialog") as Language.Lua.LuaTable;
            if (dialogTable == null) return 0;
            var conversation = DialogueManager.MasterDatabase.GetConversation(conversationID);
            if (conversation == null) return 0;
            if (useConversationID)
            {
                sb.AppendFormat("Conversation[{0}].SimX=\"", conversationID);
            }
            else
            {
                sb.AppendFormat("Variable[\"Conversation_SimX_{0}\"]=\"", DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField)));
            }
            var first = true;
            for (int i = 0; i < dialogTable.List.Count; i++)
            {
                var entryID = i + 1;
                var entryIDString = entryID.ToString();
                var simStatusTable = dialogTable.List[i] as Language.Lua.LuaTable;
                if (!first) sb.Append(";");
                first = false;
                if (useEntryID)
                {
                    sb.Append(entryIDString);
                }
                else
                {
                    var entry = conversation.GetDialogueEntry(entryID);
                    var fieldName = (entry != null) ? Field.LookupValue(entry.fields, saveDialogueEntrySimStatusWithField) : entryIDString;
                    sb.Append(fieldName);
                }
                sb.Append(";");
                var simStatus = simStatusTable.GetValue(DialogueLua.SimStatus).ToString();
                sb.Append(SimStatusToChar(simStatus));
            }

            if (!useEntryID)
            {
                // Create a lookup table to speed up lookups of each dialogue entry's SimStatus field:
                s_dialogueEntrySimStatusFieldLookupTable.Clear();
                for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                {
                    var entry = conversation.dialogueEntries[i];
                    var field = Field.Lookup(entry.fields, saveDialogueEntrySimStatusWithField);
                    var fieldName = (field != null) ? field.value : entry.id.ToString();
                    s_dialogueEntrySimStatusFieldLookupTable.Add(entry.id, fieldName);
                }
            }

            foreach (var kvp2 in dialogTable.KeyValuePairs)
            {
                var entryIDString = kvp2.Key.ToString();
                var simStatusTable = kvp2.Value as Language.Lua.LuaTable;
                if (!first) sb.Append(";");
                first = false;
                if (useEntryID)
                {
                    sb.Append(entryIDString);
                }
                else
                {
                    var entryID = Tools.StringToInt(entryIDString);
                    sb.Append(s_dialogueEntrySimStatusFieldLookupTable[entryID]);
                }
                sb.Append(";");
                var simStatus = simStatusTable.GetValue(DialogueLua.SimStatus).ToString();
                sb.Append(SimStatusToChar(simStatus));
            }
            sb.Append("\"; ");

            s_dialogueEntrySimStatusFieldLookupTable.Clear();

            return conversation.dialogueEntries.Count;
        }

        /// <summary>
        /// When reapplying saved data, expands compress SimX info into conversations' SimStatus tables.
        /// </summary>
        private static void ExpandCompressedSimStatusData()
        {
            if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus)) return;
            // Track conversations so we know which ones were added after the saved game:
            var conversationsLeft = new HashSet<int>();
            var conversations = DialogueManager.MasterDatabase.conversations;
            for (int i = 0; i < conversations.Count; i++)
            {
                conversationsLeft.Add(conversations[i].id);
            }

            // Reusable dialogue entry cache used by ExpandSimStatusForConversation:
            var dialogueEntryCache = new Dictionary<int, DialogueEntry>();

            var luaStringSimX = new Language.Lua.LuaString("SimX");
            useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
            useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
            var conversationTable = Lua.Environment.GetValue("Conversation") as Language.Lua.LuaTable;
            if (conversationTable == null) return;
            var sb = new StringBuilder(16384, System.Int32.MaxValue);
            for (int i = 0; i < conversationTable.List.Count; i++)
            {
                var conversationID = i + 1;                
                var fieldTable = conversationTable.List[i] as Language.Lua.LuaTable;
                if (ExpandSimStatusForConversation(sb, conversationID, conversationID.ToString(), fieldTable, luaStringSimX, dialogueEntryCache))
                {
                    conversationsLeft.Remove(conversationID);
                }
            }
            foreach (var kvp in conversationTable.Dict)
            {
                if (kvp.Key == null || kvp.Value == null || !(kvp.Value is Language.Lua.LuaTable)) continue;
                var conversationIDString = kvp.Key.ToString();
                var conversationID = Tools.StringToInt(conversationIDString);                
                var fieldTable = kvp.Value as Language.Lua.LuaTable;
                if (ExpandSimStatusForConversation(sb, conversationID, conversationIDString, fieldTable, luaStringSimX, dialogueEntryCache))
                {
                    conversationsLeft.Remove(conversationID); 
                }
            }
            Lua.Run(sb.ToString());

            // Add SimStatus for new conversations:
            if (conversationsLeft.Count > 0)
            {
                var enumerator = conversationsLeft.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var conversationID = enumerator.Current;
                    var conversation = DialogueManager.MasterDatabase.GetConversation(conversationID);
                    if (conversation == null) continue;
#if SAFE_SIMSTATUS
                    if (DialogueDebug.logInfo) Debug.Log("DEBUG: Add SimStatus for new conversation [" + conversationID + "]: " + conversation.Title);
#endif
                        DialogueLua.AddToConversationTable(conversationTable, conversation, true);
                }
            }
        }

        /// <summary>
        /// Expands SimX for a conversation.
        /// </summary>
        private static bool ExpandSimStatusForConversation(StringBuilder sb, int conversationID, string conversationIDString, Language.Lua.LuaTable fieldTable, Language.Lua.LuaString luaStringSimX, Dictionary<int, DialogueEntry> dialogueEntryCache)
        {
            // Find our Lua Dialog[] table and conversation asset:
            var dialogTable = fieldTable.GetValue("Dialog") as Language.Lua.LuaTable;
            if (dialogTable == null)
            {
                dialogTable = new Language.Lua.LuaTable();
                fieldTable.AddRaw("Dialog", dialogTable);
            }
            dialogTable.List.Clear();
            dialogTable.Dict.Clear();
            var conversation = DialogueManager.MasterDatabase.GetConversation(conversationID);
            if (conversation == null) return false;

            // Get the compressed SimStatus string:
            string simX;
            if (useConversationID)
            {
                var simXLuaValue = fieldTable.GetValue(luaStringSimX);
                if (simXLuaValue == null) return false;
                simX = simXLuaValue.ToString();
                sb.AppendFormat("Conversation[{0}].SimX=nil;", conversationIDString);
            }
            else
            {
                var fieldValue = DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField));
                if (string.IsNullOrEmpty(fieldValue)) fieldValue = conversation.id.ToString();
                simX = Lua.Run("return Variable[\"Conversation_SimX_" + fieldValue + "\"]").AsString;
                sb.Append("Variable[\"Conversation_SimX_" + fieldValue + "\"]=nil;");
            }
            if (string.IsNullOrEmpty(simX) || string.Equals(simX, "nil")) return false;

            var simXFields = simX.Split(';');
            var numFields = simXFields.Length / 2;

            // Index dialogue entries by ID: (don't worry about unused old entries; this conversation shouldn't reference them)
            DialogueEntry entry;
            for (int i = 0; i < conversation.dialogueEntries.Count; i++)
            {
                entry = conversation.dialogueEntries[i];
                dialogueEntryCache[entry.id] = entry;
            }

            // Make table of SimStatus fields to entry IDs.
            Dictionary<string, int> simStatusFieldValueToID = null;
            if (!useEntryID)
            {
                simStatusFieldValueToID = new Dictionary<string, int>();
                for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                {
                    entry = conversation.dialogueEntries[i];
                    var field = (entry != null) ? Field.Lookup(entry.fields, saveDialogueEntrySimStatusWithField) : null;
                    simStatusFieldValueToID[(field != null) ? field.value : entry.id.ToString()] = entry.id;
                }
            }

            // Iterate through fields of compressed SimStatus string:
            for (int i = 0; i < numFields; i++)
            {
                var simXEntryIDValue = simXFields[2 * i];
                var simStatus = CharToSimStatus(simXFields[(2 * i) + 1][0]);
                var simStatusTable = new Language.Lua.LuaTable();
                simStatusTable.AddRaw(DialogueLua.SimStatus, new Language.Lua.LuaString(simStatus));
                if (!useEntryID && !simStatusFieldValueToID.ContainsKey(simXEntryIDValue)) continue;
                var entryID = useEntryID ? Tools.StringToInt(simXEntryIDValue) : simStatusFieldValueToID[simXEntryIDValue];
                dialogueEntryCache[entryID] = null; // Mark that SimStatus has been added for this entry.
                if (useEntryID)
                {
                    dialogTable.AddRaw(entryID, simStatusTable);
                }
                else
                {
                    if (simStatusFieldValueToID.ContainsKey(simXEntryIDValue))
                    {
                        dialogTable.AddRaw(simStatusFieldValueToID[simXEntryIDValue], simStatusTable);
                    }
                }
            }

            // Backfill any new entries that weren't included in the compressed SimStatus string:
            for (int i = 0; i < conversation.dialogueEntries.Count; i++)
            {
                entry = conversation.dialogueEntries[i];
                if (dialogueEntryCache[entry.id] != null)
                {
#if SAFE_SIMSTATUS
                    if (DialogueDebug.logInfo) Debug.Log("DEBUG: Adding SimStatus for new entry [" + entry.id + "] in existing conversation [" + conversation.id + "]: " + conversation.Title);
#endif
                    // Missing. Need to add:
                    var simStatusTable = new Language.Lua.LuaTable();
                    simStatusTable.AddRaw(DialogueLua.SimStatus, new Language.Lua.LuaString(DialogueLua.Untouched));
                    dialogTable.AddRaw(entry.id, simStatusTable);
                }
            }
            return true;
        }

#endif

        private static char SimStatusToChar(string simStatus)
        {
            switch (simStatus)
            {
                default:
                    return 'X';
                case DialogueLua.Untouched:
                    return 'u';
                case DialogueLua.WasDisplayed:
                    return 'd';
                case DialogueLua.WasOffered:
                    return 'o';
            }
        }

        private static string CharToSimStatus(char c)
        {
            switch (c)
            {
                default:
                    return "ERROR";
                case 'u':
                    return DialogueLua.Untouched;
                case 'd':
                    return DialogueLua.WasDisplayed;
                case 'o':
                    return DialogueLua.WasOffered;
            }
        }

        #endregion

        #region Initialize New Fields After Load

        /// <summary>
        /// Instructs the Dialogue System to add any missing variables that are in the master 
        /// database but not in Lua.
        /// </summary>
        public static void InitializeNewVariablesFromDatabase()
        {
            try
            {
                LuaTableWrapper variableTable = Lua.Run("return Variable").AsTable;
                if (variableTable == null)
                {
                    if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Variable[] table", new System.Object[] { DialogueDebug.Prefix }));
                    return;
                }
                var database = DialogueManager.MasterDatabase;
                if (database == null) return;
                var inLua = new HashSet<string>(variableTable.Keys);
                for (int i = 0; i < database.variables.Count; i++)
                {
                    var variable = database.variables[i];
                    var variableName = variable.Name;
                    var variableIndex = DialogueLua.StringToTableIndex(variableName);
                    if (!inLua.Contains(variableIndex))
                    {
                        switch (variable.Type)
                        {
                            case FieldType.Boolean:
                                DialogueLua.SetVariable(variableName, variable.InitialBoolValue);
                                break;
                            case FieldType.Actor:
                            case FieldType.Item:
                            case FieldType.Location:
                            case FieldType.Number:
                                DialogueLua.SetVariable(variableName, variable.InitialFloatValue);
                                break;
                            default:
                                DialogueLua.SetVariable(variableName, variable.InitialValue);
                                break;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: InitializeNewVariablesFromDatabase() failed to get variable data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

#if USE_NLUA 

        /// <summary>
        /// Adds any new actors or actor fields that were not in the saved data.
        /// </summary>
        public static void InitializeNewActorFieldsFromDatabase()
        {
            try
            {
                var database = DialogueManager.MasterDatabase;
                if (database == null) return;

                var actorTable = Lua.Run("return Actor").asTable;
                if (actorTable == null || !actorTable.IsValid) throw new System.Exception("Internal error: Can't access Actor table");

                for (int i = 0; i < database.actors.Count; i++)
                {
                    var dbActor = database.actors[i];
                    var actorName = dbActor.Name;
                    var actorNameTableIndex = DialogueLua.StringToTableIndex(actorName);

                    var actorRecord = Lua.Run("return Actor[\"" + actorNameTableIndex + "\"]");
                    if (!actorRecord.isTable)
                    {
                        // This is a new actor not in the save data. Add it:
                        var newActorLuaCode = "Actor[\"" + actorNameTableIndex + "\"] = {";
                        for (int j = 0; j < dbActor.fields.Count; j++)
                        {
                            var field = dbActor.fields[j];
                            var fieldIndex = DialogueLua.StringToFieldName(field.title);
                            newActorLuaCode += fieldIndex + DialogueLua.FieldValueAsString(field) + ", ";
                        }
                        newActorLuaCode += "}";
                        Lua.Run(newActorLuaCode);
                    }
                    else
                    {
                        // Existing actor. Add any missing fields:
                        var fieldTable = actorRecord.asTable;
                        if (fieldTable == null) continue;
                        var existingFields = new HashSet<string>(fieldTable.keys);
                        for (int j = 0; j < dbActor.fields.Count; j++)
                        {
                            var field = dbActor.fields[j];
                            var fieldTableIndex = DialogueLua.StringToFieldName(field.title);
                            if (!existingFields.Contains(fieldTableIndex))
                            {
                                Lua.Run("Actor[\"" + actorNameTableIndex + "\"]." + fieldTableIndex + " = " + DialogueLua.FieldValueAsString(field));
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: InitializeNewActorFieldsFromDatabase() failed to get actor data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

#else

        /// <summary>
        /// Adds any new actors or actor fields that were not in the saved data.
        /// </summary>
        public static void InitializeNewActorFieldsFromDatabase()
        {
            try
            {
                var database = DialogueManager.MasterDatabase;
                if (database == null) return;

                var actorTable = Lua.Run("return Actor").AsTable;
                if (actorTable == null || !actorTable.IsValid) throw new System.Exception("Internal error: Can't access Actor table");

                for (int i = 0; i < database.actors.Count; i++)
                {
                    var dbActor = database.actors[i];
                    var actorName = dbActor.Name;
                    var actorNameTableIndex = DialogueLua.StringToTableIndex(actorName);
                    var fieldTable = actorTable.luaTable.GetValue(actorNameTableIndex) as Language.Lua.LuaTable;

                    if (fieldTable == null)
                    {
                        // This is a new actor not in the save data. Add it:
                        fieldTable = new Language.Lua.LuaTable();
                        for (int j = 0; j < dbActor.fields.Count; j++)
                        {
                            var field = dbActor.fields[j];
                            var fieldIndex = DialogueLua.StringToFieldName(field.title);
                            fieldTable.AddRaw(fieldIndex, DialogueLua.GetFieldLuaValue(field));
                        }
                        actorTable.luaTable.AddRaw(actorNameTableIndex, fieldTable);
                    }
                    else
                    {
                        // Existing actor. Add any missing fields:
                        var existingFields = new HashSet<string>();
                        foreach (var key in fieldTable.Keys)
                        {
                            existingFields.Add(key.ToString());
                        }
                        for (int j = 0; j < dbActor.fields.Count; j++)
                        {
                            var field = dbActor.fields[j];
                            var fieldTableIndex = DialogueLua.StringToFieldName(field.title);
                            if (!existingFields.Contains(fieldTableIndex))
                            {
                                var fieldValue = DialogueLua.GetFieldLuaValue(field);
                                fieldTable.AddRaw(fieldTableIndex, fieldValue);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: InitializeNewActorFieldsFromDatabase() failed to get actor data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }


#endif

        /// <summary>
        /// Instructs the Dialogue System to add any missing quests and entries that are in the master 
        /// database but not in Lua.
        /// </summary>
        public static void InitializeNewQuestEntriesFromDatabase()
        {
            try
            {
                var luaCode = string.Empty;
                var database = DialogueManager.MasterDatabase;
                if (database == null) return;
                for (int i = 0; i < database.items.Count; i++)
                {
                    if (database.items[i].IsItem) continue;
                    var dbQuest = database.items[i];
                    var questName = dbQuest.Name;
                    var questNameTableIndex = DialogueLua.StringToTableIndex(questName);

                    // Add any missing quests:
                    if (!DialogueLua.DoesTableElementExist("Item", questName))
                    {
                        var questCode = string.Empty;
                        questCode = "Item[\"" + DialogueLua.StringToTableIndex(questName) + "\"] = {{";
                        for (int j = 0; j < dbQuest.fields.Count; j++)
                        {
                            var field = dbQuest.fields[j];
                            questCode += DialogueLua.StringToFieldName(field.title) + "=" +
                                DialogueLua.ValueAsString(field.type, field.value) + ", ";
                        }
                        questCode += "}}; ";
                        luaCode += questCode;
                    }

                    // Add any missing entries:
                    var dbEntryCount = dbQuest.LookupInt("Entry Count");
                    var luaEntryCount = DialogueLua.GetQuestField(questName, "Entry Count").AsInt;
                    if (luaEntryCount < dbEntryCount)
                    {
                        luaCode += "Item[\"" + questNameTableIndex + "\"].Entry_Count=" + dbEntryCount + "; ";
                        for (int j = 0; j < dbQuest.fields.Count; j++)
                        {
                            var field = dbQuest.fields[j];
                            if (field.title.StartsWith("Entry ") && !field.title.EndsWith(" Count"))
                            {
                                luaCode += "Item[\"" + questNameTableIndex + "\"]." +
                                    DialogueLua.StringToFieldName(field.title) + " = " +
                                    DialogueLua.ValueAsString(field.type, field.value) + "; ";
                            }
                        }
                    }
                    Lua.Run(luaCode);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: InitializeNewQuestEntriesFromDatabase() failed to get quest data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
            }
        }

        /// <summary>
        /// Initializes SimStatus for entries that were added to the database after the saved game.
        /// </summary>
        public static void InitializeNewSimStatusFromDatabase()
        {
            // For LuaInterpreter, ExpandSimStatusForConversation also initializes new SimStatus, 
            // so we only need this for NLua:
#if NLUA || SAFE_SIMSTATUS
            if (!(includeSimStatus && initializeNewSimStatus)) return;
            try
            {
                var database = DialogueManager.MasterDatabase;
                if (database == null) return;
                var missingConversations = new List<Conversation>();
                var fakeLoadedDatabases = new List<DialogueDatabase>();
                var luaCode = string.Empty;
                for (int i = 0; i < database.conversations.Count; i++)
                {
                    var conversation = database.conversations[i];
                    var convTableIdent = "Conversation[" + conversation.id + "]";
                    var convTable = Lua.Run("return " + convTableIdent).AsTable;
                    if (!convTable.IsValid)
                    {
                        missingConversations.Add(conversation);
                    }
                    else
                    {
                        for (int j = 0; j < conversation.dialogueEntries.Count; j++)
                        {
                            var entry = conversation.dialogueEntries[j];
                            var dialogTableIdent = convTableIdent + ".Dialog[" + entry.id + "]";
                            var entryTable = Lua.Run("return " + dialogTableIdent).AsTable;
                            if (!entryTable.IsValid)
                            {
                                luaCode += dialogTableIdent + "={SimStatus=\"Untouched\"}; ";
                            }
                        }
                    }
                }
                Lua.Run(luaCode, DialogueDebug.LogInfo);
                DialogueLua.AddToConversationTable(missingConversations, fakeLoadedDatabases);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Dialogue System: InitializeNewSimStatusFromDatabase() failed: " + e.Message);
            }
#endif
        }

        #endregion

        #region Async Saving

        //======================================================================
        // Asynchronous saving:

        public static int asyncGameObjectBatchSize = 1000;
        public static int asyncDialogueEntryBatchSize = 100;

        public class AsyncSaveOperation
        {
            public bool isDone = false;
            public string content = string.Empty;
        }

        public static AsyncSaveOperation GetSaveDataAsync()
        {
            var asyncOp = new AsyncSaveOperation();
            DialogueManager.Instance.StartCoroutine(GetSaveDataAsyncCoroutine(asyncOp));
            return asyncOp;
        }

        private static IEnumerator GetSaveDataAsyncCoroutine(AsyncSaveOperation asyncOp)
        {
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Saving data asynchronously...", new System.Object[] { DialogueDebug.Prefix, asyncGameObjectBatchSize }));

            switch (recordPersistentDataOn)
            {
                case RecordPersistentDataOn.AllGameObjects:
                    yield return DialogueManager.Instance.StartCoroutine(Tools.SendMessageToEveryoneAsync("OnRecordPersistentData", asyncGameObjectBatchSize));
                    break;
                case RecordPersistentDataOn.OnlyRegisteredGameObjects:
                    int count = 0;
                    foreach (var go in listeners)
                    {
                        if (go != null)
                        {
                            go.SendMessage("OnRecordPersistentData", SendMessageOptions.DontRequireReceiver);
                            count++;
                            if (count > asyncGameObjectBatchSize)
                            {
                                count = 0;
                                yield return null;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            StringBuilder sb = new StringBuilder();
            AppendVariableData(sb);
            yield return null;
            AppendItemData(sb);
            yield return null;
            AppendLocationData(sb);
            yield return null;
            if (includeActorData) AppendActorData(sb);
            yield return null;
            yield return DialogueManager.Instance.StartCoroutine(AppendConversationDataAsync(sb));
            yield return null;
            if (includeRelationshipAndStatusData) AppendRelationshipAndStatusTables(sb);
            if (GetCustomSaveData != null) sb.Append(GetCustomSaveData());
            string saveData = sb.ToString();
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Saved data asynchronously: {1}", new System.Object[] { DialogueDebug.Prefix, saveData }));
            asyncOp.content = saveData;
            asyncOp.isDone = true;
        }

        /// <summary>
        /// Sends the OnRecordPersistentData message to all game objects in the scene to give them 
        /// an opportunity to record their state in the Lua environment. Runs in batches specified
        /// by the value of asyncGameObjectBatchSize.
        /// </summary>
        public static void RecordAsync()
        {
            if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Recording persistent data to Lua environment in batches of {1} GameObjects.", new System.Object[] { DialogueDebug.Prefix, asyncGameObjectBatchSize }));
            DialogueManager.Instance.StartCoroutine(Tools.SendMessageToEveryoneAsync("OnRecordPersistentData", asyncGameObjectBatchSize));
        }

        private static IEnumerator AppendConversationDataAsync(StringBuilder sb)
        {
            if (includeAllConversationFields || DialogueManager.Instance.persistentDataSettings.includeAllConversationFields)
            {
                AppendAllConversationFields(sb);
            }
            if (includeSimStatus && DialogueManager.Instance.includeSimStatus)
            {
                var count = 0;
#if USE_NLUA
                var useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
                var useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
                foreach (var conversation in DialogueManager.MasterDatabase.conversations)
                {
                    if (useConversationID)
                    {
                        sb.AppendFormat("Conversation[{0}].SimX=\"", conversation.id);
                    }
                    else
                    {
                        var fieldValue = DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField));
                        if (string.IsNullOrEmpty(fieldValue)) fieldValue = conversation.id.ToString();
                        sb.AppendFormat("Variable[\"Conversation_SimX_{0}\"]=\"", fieldValue);
                    }
                    var dialogTable = Lua.Run("return Conversation[" + conversation.id + "].Dialog").asTable;
                    var first = true;
                    for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                    {
                        try
                        {
                            var entry = conversation.dialogueEntries[i];
                            var entryID = entry.id;
                            var dialogFields = dialogTable[entryID] as NLua.LuaTable;
                            if (dialogFields != null)
                            {
                                if (!first) sb.Append(";");
                                first = false;
                                sb.Append(useEntryID ? entryID.ToString() : Field.LookupValue(entry.fields, saveDialogueEntrySimStatusWithField));
                                sb.Append(";");
                                var simStatus = dialogFields[DialogueLua.SimStatus].ToString();
                                sb.Append(SimStatusToChar(simStatus));
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(string.Format("{0}: GetSaveData() failed to get conversation data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
                        }
                        count++;
                        if (count >= asyncDialogueEntryBatchSize)
                        {
                            count = 0;
                            yield return null;
                        }
                    }
                    sb.Append("\"; ");
                }
#else
                useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
                useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
                var conversationTable = Lua.Environment.GetValue("Conversation") as Language.Lua.LuaTable;
                if (conversationTable == null) yield break;
                for (int i = 0; i < conversationTable.List.Count; i++)
                {
                    var conversationID = i + 1;
                    var fieldTable = conversationTable.List[i] as Language.Lua.LuaTable;
                    count += AppendSimStatusForConversation(sb, conversationTable, conversationID, fieldTable);
                    if (count >= asyncDialogueEntryBatchSize)
                    {
                        count = 0;
                        yield return null;
                    }
                }
                foreach (var kvp in conversationTable.Dict)
                {
                    if (kvp.Key == null || kvp.Value == null || !(kvp.Value is Language.Lua.LuaTable)) continue;
                    var conversationID = Tools.StringToInt(kvp.Key.ToString());
                    var fieldTable = kvp.Value as Language.Lua.LuaTable;
                    count += AppendSimStatusForConversation(sb, conversationTable, conversationID, fieldTable);
                    if (count >= asyncDialogueEntryBatchSize)
                    {
                        count = 0;
                        yield return null;
                    }
                }
#endif
            }
        }

        #endregion

        #region Raw Dump

#if USE_NLUA

        // Note: NLua doesn't implement raw dump. It just does a passthrough to the regular save technique.

        public static byte[] GetRawData()
        {
            return Encoding.UTF8.GetBytes(GetSaveData());
        }

        public static void ApplyRawData(byte[] bytes)
        {
            string s = Encoding.UTF8.GetString(bytes);
            ApplySaveData(s);
        }

#else

        // Note: Raw dump is only implemented for LuaInterpreter (the default Lua implementation).

        public class AsyncRawDataOperation
        {
            public bool isDone = false;
            public byte[] content = null;
        }

        public static byte[] GetRawData()
        {
            Record();
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                var conversationTable = Lua.Run("return Conversation").AsTable.luaTable;
                PrepSimStatusForRawData(conversationTable);
                WriteValue(writer, Lua.Run("return Actor").AsTable.luaTable);
                WriteValue(writer, Lua.Run("return Item").AsTable.luaTable);
                WriteValue(writer, Lua.Run("return Location").AsTable.luaTable);
                WriteValue(writer, Lua.Run("return Variable").AsTable.luaTable);
                WriteValue(writer, conversationTable);
                WriteExtraData(writer);
                writer.Flush();
                return ms.GetBuffer();
            }
        }

        public static AsyncRawDataOperation GetRawDataAsync()
        {
            var asyncOp = new AsyncRawDataOperation();
            DialogueManager.Instance.StartCoroutine(GetRawDataAsyncCoroutine(asyncOp));
            return asyncOp;
        }

        private static IEnumerator GetRawDataAsyncCoroutine(AsyncRawDataOperation asyncOp)
        {
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Saving raw Lua data asynchronously...");

            // Record persistent data objects async:
            switch (recordPersistentDataOn)
            {
                case RecordPersistentDataOn.AllGameObjects:
                    yield return DialogueManager.Instance.StartCoroutine(Tools.SendMessageToEveryoneAsync("OnRecordPersistentData", asyncGameObjectBatchSize));
                    break;
                case RecordPersistentDataOn.OnlyRegisteredGameObjects:
                    int count = 0;
                    foreach (var go in listeners)
                    {
                        if (go != null)
                        {
                            go.SendMessage("OnRecordPersistentData", SendMessageOptions.DontRequireReceiver);
                            count++;
                            if (count > asyncGameObjectBatchSize)
                            {
                                count = 0;
                                yield return null;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                var conversationTable = Lua.Run("return Conversation").AsTable.luaTable;
                yield return DialogueManager.Instance.StartCoroutine(PrepSimStatusForRawDataAsync(conversationTable));
                WriteValue(writer, Lua.Run("return Actor").AsTable.luaTable);
                yield return null;
                WriteValue(writer, Lua.Run("return Item").AsTable.luaTable);
                yield return null;
                WriteValue(writer, Lua.Run("return Location").AsTable.luaTable);
                yield return null;
                WriteValue(writer, Lua.Run("return Variable").AsTable.luaTable);
                yield return null;
                WriteValue(writer, conversationTable);
                yield return null;
                WriteExtraData(writer);
                writer.Flush();
                asyncOp.content = ms.GetBuffer();
            }
            asyncOp.isDone = true;
        }

        private static void WriteValue(BinaryWriter writer, Language.Lua.LuaValue value)
        {
            if (value is Language.Lua.LuaTable)
            {
                WriteTable(writer, value as Language.Lua.LuaTable);
            }
            else if (value is Language.Lua.LuaString)
            {
                writer.Write('S');
                writer.Write((value as Language.Lua.LuaString).Text);
            }
            else if (value is Language.Lua.LuaNumber)
            {
                writer.Write('N');
                writer.Write((value as Language.Lua.LuaNumber).Number);
            }
            else if (value is Language.Lua.LuaBoolean)
            {
                writer.Write('B');
                writer.Write((value as Language.Lua.LuaBoolean).BoolValue);
            }
            else if (value is Language.Lua.LuaNil)
            {
                writer.Write('X');
            }
            else
            {
                Debug.LogError("WriteValue unhandled " + value.GetType().Name + ": " + value.ToString());
            }
        }

        private static void WriteTable(BinaryWriter writer, Language.Lua.LuaTable table)
        {
            writer.Write('T');
            if (table.List == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write(table.List.Count);
                for (int i = 0; i < table.List.Count; i++)
                {
                    WriteValue(writer, table.List[i]);
                }
            }
            if (table.Dict == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write(table.Dict.Count);
                var enumerator = table.Dict.GetEnumerator(); // Enumerates manually to avoid garbage.
                while (enumerator.MoveNext())
                {
                    WriteValue(writer, enumerator.Current.Key);
                    WriteValue(writer, enumerator.Current.Value);
                }
            }
        }

        public static void ApplyRawData(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    Lua.Run("Actor = {}; Item = {}; Location = {}; Variable = {}; Conversation = {}");
                    ReadTable(reader, Lua.Run("return Actor").AsTable.luaTable);
                    ReadTable(reader, Lua.Run("return Item").AsTable.luaTable);
                    ReadTable(reader, Lua.Run("return Location").AsTable.luaTable);
                    ReadTable(reader, Lua.Run("return Variable").AsTable.luaTable);
                    ReadTable(reader, Lua.Run("return Conversation").AsTable.luaTable);
                    ApplySimStatusFromRawData();
                    ApplyExtraData(reader);
                    RefreshRelationshipAndStatusTablesFromLua();
                    if (initializeNewVariables)
                    {
                        InitializeNewVariablesFromDatabase();
                        InitializeNewQuestEntriesFromDatabase();
                        // Do not need this. It's done implicitly when expanding SimX: InitializeNewSimStatusFromDatabase();
                    }
                }
            }
            Apply();
        }

        private static Language.Lua.LuaValue ReadValue(BinaryReader reader)
        {
            if ((char)reader.PeekChar() == 'T')
            {
                var luaTable = new Language.Lua.LuaTable();
                ReadTable(reader, luaTable);
                return luaTable;
            }
            var typeChar = reader.ReadChar();
            if (typeChar == 'S')
            {
                var s = reader.ReadString();
                return new Language.Lua.LuaString(s);
            }
            else if (typeChar == 'N')
            {
                var n = reader.ReadDouble();
                return new Language.Lua.LuaNumber(n);
            }
            else if (typeChar == 'B')
            {
                var b = reader.ReadBoolean();
                return (b == true) ? Language.Lua.LuaBoolean.True : Language.Lua.LuaBoolean.False;
            }
            else if (typeChar == 'X')
            {
                return Language.Lua.LuaNil.Nil;
            }
            else
            {
                Debug.LogError("ReadValue unhandled type code " + typeChar);
                return Language.Lua.LuaNil.Nil;
            }
        }

        private static void ReadTable(BinaryReader reader, Language.Lua.LuaTable table)
        {
            reader.Read(); // 'T'

            int listLength = reader.ReadInt32();
            for (int i = 0; i < listLength; i++)
            {
                var value = ReadValue(reader);
                table.List.Add(value);
            }

            int dictLength = reader.ReadInt32();
            for (int i = 0; i < dictLength; i++)
            {
                var key = ReadValue(reader);
                var value = ReadValue(reader);
                table.Dict.Add(key, value);
            }
        }

        // Save relationship & status tables, and custom save data delegate.
        private static void WriteExtraData(BinaryWriter writer)
        {
            if (includeRelationshipAndStatusData)
            {
                var sb = new StringBuilder();
                AppendRelationshipAndStatusTables(sb);
                writer.Write(sb.ToString());
            }
            if (GetCustomSaveData != null)
            {
                writer.Write(GetCustomSaveData());
            }
        }

        private static void ApplyExtraData(BinaryReader reader)
        {
            if (includeRelationshipAndStatusData)
            {
                Lua.Run(reader.ReadString());
            }
            if (GetCustomSaveData != null)
            {
                Lua.Run(reader.ReadString(), DialogueDebug.LogInfo);
            }
        }

        // If saveConversationSimStatusWithField or saveDialogueEntrySimStatusWithField are set,
        // copy the current SimStatus into the specified variables/fields.
        private static void PrepSimStatusForRawData(Language.Lua.LuaTable conversationTable)
        {
            // Only need to do if saving to fields:
            if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus && conversationTable != null)) return;
            useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
            useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
            if (useConversationID && useEntryID) return;
            // Reuse these vars to reduce GC:
            var dialogueEntryCache = new Dictionary<int, DialogueEntry>();
            var sb = new StringBuilder(16384, System.Int32.MaxValue);

            for (int i = 0; i < conversationTable.List.Count; i++)
            {
                var conversationID = i + 1;
                var fieldTable = conversationTable.List[i] as Language.Lua.LuaTable;
                PrepConversationSimStatusForRawData(conversationTable, conversationID, fieldTable, dialogueEntryCache, sb);
            }
            foreach (var kvp in conversationTable.Dict)
            {
                if (kvp.Key == null || kvp.Value == null || !(kvp.Value is Language.Lua.LuaTable)) continue;
                var conversationID = Tools.StringToInt(kvp.Key.ToString());
                var fieldTable = kvp.Value as Language.Lua.LuaTable;
                PrepConversationSimStatusForRawData(conversationTable, conversationID, fieldTable, dialogueEntryCache, sb);
            }
        }

        // Async version:
        private static IEnumerator PrepSimStatusForRawDataAsync(Language.Lua.LuaTable conversationTable)
        {
            // Only need to do if saving to fields:
            if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus && conversationTable != null)) yield break;
            useConversationID = string.IsNullOrEmpty(saveConversationSimStatusWithField);
            useEntryID = string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField);
            if (useConversationID && useEntryID) yield break;

            // Reuse these vars to reduce GC:
            var dialogueEntryCache = new Dictionary<int, DialogueEntry>();
            var sb = new StringBuilder(16384, System.Int32.MaxValue);
            int numEntriesDone = 0;

            for (int i = 0; i < conversationTable.List.Count; i++)
            {
                var conversationID = i + 1;
                var fieldTable = conversationTable.List[i] as Language.Lua.LuaTable;
                numEntriesDone += PrepConversationSimStatusForRawData(conversationTable, conversationID, fieldTable, dialogueEntryCache, sb);
                if (numEntriesDone >= asyncDialogueEntryBatchSize)
                {
                    numEntriesDone = 0;
                    yield return null;
                }
            }
            foreach (var kvp in conversationTable.Dict)
            {
                if (kvp.Key == null || kvp.Value == null || !(kvp.Value is Language.Lua.LuaTable)) continue;
                var conversationID = Tools.StringToInt(kvp.Key.ToString());
                var fieldTable = kvp.Value as Language.Lua.LuaTable;
                numEntriesDone += PrepConversationSimStatusForRawData(conversationTable, conversationID, fieldTable, dialogueEntryCache, sb);
                if (numEntriesDone >= asyncDialogueEntryBatchSize)
                {
                    numEntriesDone = 0;
                    yield return null;
                }
            }
        }

        private static int PrepConversationSimStatusForRawData(Language.Lua.LuaTable conversationTable, int conversationID,
            Language.Lua.LuaTable fieldTable, Dictionary<int, DialogueEntry> dialogueEntryCache, StringBuilder sb)
        {
            if (conversationTable == null || fieldTable == null) return 0;
            var dialogTable = fieldTable.GetValue("Dialog") as Language.Lua.LuaTable;
            if (dialogTable == null) return 0;
            var conversation = DialogueManager.MasterDatabase.GetConversation(conversationID);
            if (conversation == null) return 0;
            sb.Length = 0;

            // Index dialogue entries by ID: (don't worry about unused old entries; this conversation shouldn't reference them)
            DialogueEntry entry;
            for (int i = 0; i < conversation.dialogueEntries.Count; i++)
            {
                entry = conversation.dialogueEntries[i];
                dialogueEntryCache[entry.id] = entry;
            }

            var first = true;

            // Handle Dialog table's List:
            for (int i = 0; i < dialogTable.List.Count; i++)
            {
                var entryID = i + 1;
                var simStatusTable = dialogTable.List[i] as Language.Lua.LuaTable;
                if (!first) sb.Append(";");
                first = false;
                if (!useEntryID && dialogueEntryCache.TryGetValue(entryID, out entry))
                {
                    sb.Append(Field.LookupValue(entry.fields, saveDialogueEntrySimStatusWithField));
                }
                else
                {
                    sb.Append(entryID);
                }
                sb.Append(";");
                //--- Optimization since we know table only has one field. Was: var simStatus = simStatusTable.GetValue(DialogueLua.SimStatus).ToString();
                var enumerator = simStatusTable.Dict.GetEnumerator();
                enumerator.MoveNext();
                var simStatus = enumerator.Current.Value.ToString();
                sb.Append(SimStatusToChar(simStatus));
            }

            // Handle Dialog table's Dict:
            foreach (var kvp2 in dialogTable.KeyValuePairs)
            {
                var simStatusTable = kvp2.Value as Language.Lua.LuaTable;
                if (!first) sb.Append(";");
                first = false;
                if (!useEntryID)
                {
                    var entryID = (kvp2.Key is Language.Lua.LuaNumber) ? (int)((kvp2.Key as Language.Lua.LuaNumber).Number) : Tools.StringToInt(kvp2.Key.ToString());
                    if (dialogueEntryCache.TryGetValue(entryID, out entry))
                    {
                        sb.Append(Field.LookupValue(entry.fields, saveDialogueEntrySimStatusWithField));
                    }
                    else
                    {
                        sb.Append(entryID);
                    }
                }
                else
                {
                    sb.Append(kvp2.Key.ToString());
                }
                sb.Append(";");
                //--- Optimization since we know table only has one field. Was: var simStatus = simStatusTable.GetValue(DialogueLua.SimStatus).ToString();
                var enumerator = simStatusTable.Dict.GetEnumerator();
                enumerator.MoveNext();
                var simStatus = enumerator.Current.Value.ToString();
                sb.Append(SimStatusToChar(simStatus));
            }

            if (useConversationID)
            {
                Lua.Run("Conversation[" + conversationID + "].SimX=\"" + sb.ToString() + "\"");
            }
            else
            {
                var fieldName = DialogueLua.StringToTableIndex(conversation.LookupValue(saveConversationSimStatusWithField));
                Lua.Run("Variable[\"Conversation_SimX_" + fieldName + "\"]=\"" + sb.ToString() + "\"");

            }
            return conversation.dialogueEntries.Count;
        }

        // If saveConversationSimStatusWithField or saveDialogueEntrySimStatusWithField are set,
        // repopoulate SimStatus from the values in the specified variables/fields.
        private static void ApplySimStatusFromRawData()
        {
            if (includeSimStatus && DialogueManager.Instance.includeSimStatus &&
                (!string.IsNullOrEmpty(saveConversationSimStatusWithField) || !string.IsNullOrEmpty(saveDialogueEntrySimStatusWithField)))
            {
                ExpandCompressedSimStatusData();
            }
        }

#endif

        #endregion

    }

}
