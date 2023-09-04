#if USE_ARCWEAVE

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

    #region Helper Types

    [Serializable]
    public class ArcweaveConversationInfo
    {
        public string boardGuid;
        public int startIndex; // Index into board's sorted elements list.
        public int actorIndex; // Index into sorted components list.
        public int conversantIndex; // Index into sorted components list.
        public ArcweaveConversationInfo() { }
        public ArcweaveConversationInfo(string boardGuid) { this.boardGuid = boardGuid; }
    }

    public enum ArcweaveBoardType { Ignore, Conversation, Quest }
    public enum ArcweaveComponentType { Ignore, Player, NPC, Item, Location }

    // Temporary tree structure for boards so we only show leaf nodes as possible conversations, and can prepend parents to conversation titles.
    public class ArcweaveBoardNode
    {
        public string guid;
        public Board board;
        public List<ArcweaveBoardNode> children = new List<ArcweaveBoardNode>();
        public ArcweaveBoardNode parent;

        public ArcweaveBoardNode(string guid, Board board, ArcweaveBoardNode parent)
        {
            this.guid = guid;
            this.board = board;
            this.parent = parent;
        }
    }

    [Serializable]
    public class ArcweaveImportPrefs
    {
        public string sourceFilename = string.Empty;
        public string outputFolder = "Assets";
        public string databaseFilename = "Dialogue Database";
        public bool overwrite = false;
        public bool merge = false;

        public string arcweaveProjectPath;
        public string contentJson;

        public List<string> questBoardGuids = new List<string>();

        public List<ArcweaveConversationInfo> conversationInfo = new List<ArcweaveConversationInfo>();

        public List<string> playerComponentGuids = new List<string>();
        public List<string> npcComponentGuids = new List<string>();
        public List<string> itemComponentGuids = new List<string>();
        public List<string> locationComponentGuids = new List<string>();

        public bool boardsFoldout = true;
        public bool componentsFoldout = true;

        public bool importPortraits = true;
        public bool importGuids = false;
        public int numPlayers = 1;
        public string globalVariables;

        public string prefsPath;
    }

    #endregion

    public class ArcweaveImporter
    {

        #region Variables

        public bool merge;

        public string arcweaveProjectPath;
        public string contentJson;

        public List<string> questBoardGuids = new List<string>();

        public List<ArcweaveConversationInfo> conversationInfo = new List<ArcweaveConversationInfo>();

        public List<string> playerComponentGuids = new List<string>();
        public List<string> npcComponentGuids = new List<string>();
        public List<string> itemComponentGuids = new List<string>();
        public List<string> locationComponentGuids = new List<string>();

        public bool boardsFoldout = true;
        public bool componentsFoldout = true;
        public int numPlayers = 1;
        public List<string> globalVariables = new List<string>();

        public bool importPortraits = true;
        public bool importGuids = false;

        public Template template = Template.FromDefault();

        public DialogueDatabase database;

        public ArcweaveProject arcweaveProject { get; protected set; } = null;

        public Dictionary<string, string[]> conversationElements { get; protected set; } = new Dictionary<string, string[]>();
        public Dictionary<string, string> elementNames = new Dictionary<string, string>();
        public string[] componentNames = new string[0];

        public ArcweaveBoardNode rootBoardNode = null;
        public Dictionary<string, Board> leafBoards { get; protected set; } = new Dictionary<string, Board>();

        protected Dictionary<string, ArcweaveType> arcweaveLookup = new Dictionary<string, ArcweaveType>();
        protected Dictionary<string, DialogueEntry> dialogueEntryLookup = new Dictionary<string, DialogueEntry>();
        protected Dictionary<string, Actor> actorLookup = new Dictionary<string, Actor>();
        protected int currentPlayerID;
        protected int currentNpcID;
        protected const string NoneSequence = "None()";
        protected const string ContinueSequence = "Continue()";
        private const string DeleteTag = "$$Delete$$";

        #endregion

        #region Setup

        /// <summary>
        /// Prepares the Arcweave importer with parameters ready to perform an import.
        /// </summary>
        /// <param name="arcweaveProjectPath">Path to Arcweave project files. This path should contain project_settings.json exported from Arcweave.</param>
        /// <param name="contentJson">Arcweave project_settings.json text. Can be blank. If non-blank, import uses this instead of reading JSON file from arcweaveProjectPath.</param>
        /// <param name="questBoardGuids">Settings from Arcweave importer window's prefs.</param>
        /// <param name="conversationInfo">Settings from Arcweave importer window's prefs.</param>
        /// <param name="playerComponentGuids">Settings from Arcweave importer window's prefs.</param>
        /// <param name="npcComponentGuids">Settings from Arcweave importer window's prefs.</param>
        /// <param name="itemComponentGuids">Settings from Arcweave importer window's prefs.</param>
        /// <param name="locationComponentGuids">Settings from Arcweave importer window's prefs.</param>
        /// <param name="boardsFoldout">Settings from Arcweave importer window's prefs.</param>
        /// <param name="componentsFoldout">Settings from Arcweave importer window's prefs.</param>
        /// <param name="importPortraits">Assign portrait images to actors. (Editor only)</param>
        /// <param name="importGuids">In actors, locations, conversations, and quests, add a field containing Arcweave GUID.</param>
        /// <param name="merge">Merge into existing database, keeping/overwriting existing assets, instead of clearing database first.</param>
        /// <param name="numPlayers">Set to value greater than 1 to import set of variables for each player.</param>
        /// <param name="globalVariables">If numPlayers > 1, this is a comma-separated list of global variables that aren't player-specific.</param>
        /// <param name="template">Template to use to create new actors, conversations, etc.</param>
        public void Setup(string arcweaveProjectPath,
            string contentJson,
            List<string> questBoardGuids,
            List<ArcweaveConversationInfo> conversationInfo,
            List<string> playerComponentGuids,
            List<string> npcComponentGuids,
            List<string> itemComponentGuids,
            List<string> locationComponentGuids,
            bool boardsFoldout,
            bool componentsFoldout,
            bool importPortraits,
            bool importGuids,
            int numPlayers,
            string globalVariables,
            bool merge,
            Template template)
        {
            this.arcweaveProjectPath = arcweaveProjectPath;
            this.contentJson = contentJson;
            this.questBoardGuids = questBoardGuids;
            this.conversationInfo = conversationInfo;
            this.playerComponentGuids = playerComponentGuids;
            this.npcComponentGuids = npcComponentGuids;
            this.itemComponentGuids = itemComponentGuids;
            this.locationComponentGuids = locationComponentGuids;
            this.boardsFoldout = boardsFoldout;
            this.componentsFoldout = componentsFoldout;
            this.importPortraits = importPortraits;
            this.importGuids = importGuids;
            this.numPlayers = numPlayers;
            this.globalVariables = ParseGlobalVariables(globalVariables);
            this.merge = merge;
            this.template = (template != null) ? template : Template.FromDefault();
        }

        /// <summary>
        /// Prepares the Arcweave importer with parameters ready to perform an import.
        /// </summary>
        /// <param name="jsonPrefs">JSON text of prefs from Arcweave importer window, with optional contentJson to use instead of reading project_settings.json file.</param>
        /// <param name="database">Database in which to add the imported content.</param>
        /// <param name="template">Template to use to create new actors, conversations, etc.</param>
        public void Setup(string jsonPrefs, DialogueDatabase database, Template template)
        {
            var prefs = JsonUtility.FromJson<ArcweaveImportPrefs>(jsonPrefs);
            if (prefs != null)
            {
                Setup(prefs.arcweaveProjectPath,
                    prefs.contentJson,
                    prefs.questBoardGuids,
                    prefs.conversationInfo,
                    prefs.playerComponentGuids,
                    prefs.npcComponentGuids,
                    prefs.itemComponentGuids,
                    prefs.locationComponentGuids,
                    prefs.boardsFoldout,
                    prefs.componentsFoldout,
                    prefs.importPortraits,
                    prefs.importGuids,
                    prefs.numPlayers,
                    prefs.globalVariables,
                    prefs.merge,
                    template);
                this.database = database;
            }
        }

        public void Clear()
        {
            arcweaveProject = null;
        }

        protected List<string> ParseGlobalVariables(string s)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var v in s.Split(','))
                {
                    list.Add(v.Trim());
                }
            }
            return list;
        }

        #endregion

        #region Load JSON

        /// <summary>
        /// Imports Arcweave exported content into a dialogue database.
        /// </summary>
        /// <param name="jsonPrefs">JSON text of prefs from Arcweave importer window, with optional contentJson to use instead of reading project_settings.json file.</param>
        /// <param name="database">Database in which to add the imported content.</param>
        /// <param name="template">Template to use to create new actors, conversations, etc.</param>
        public void Import(string jsonPrefs, DialogueDatabase database, Template template)
        {
            Setup(jsonPrefs, database, template);
            LoadAndConvert();
        }

        public void LoadAndConvert()
        {
            LoadJson();
            Convert();
        }

        public bool IsJsonLoaded()
        {
            return arcweaveProject != null && arcweaveProject.boards != null && arcweaveProject.boards.Count > 0;
        }

        public bool LoadJson()
        {
            try
            {
                arcweaveProject = null;
                var pathInProject = (!string.IsNullOrEmpty(arcweaveProjectPath) && arcweaveProjectPath.StartsWith("Assets"))
                    ? arcweaveProjectPath.Substring("Assets".Length) : string.Empty;
                var jsonFilename = Application.dataPath + pathInProject + "/project_settings.json";

                //Check if file exists, if not check for content json. If both are not provided an import is not possible
                var json = "";
                if (!String.IsNullOrEmpty(contentJson))
                {
                    json = contentJson;
                }
                else if (System.IO.File.Exists(jsonFilename))
                {
                    json = System.IO.File.ReadAllText(jsonFilename);
                }
                else
                {
                    Debug.LogError($"Dialogue System: Arcweave JSON file '{jsonFilename}' doesn't exist.");
                    return false;
                }
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"Dialogue System: Unable to read '{jsonFilename}'.");
                    return false;
                }
                arcweaveProject = Newtonsoft.Json.JsonConvert.DeserializeObject<ArcweaveProject>(json);
                if (!IsJsonLoaded())
                {
                    Debug.LogError($"Dialogue System: Arcweave project '{jsonFilename}' is empty or could not be loaded.");
                    return false;
                }
                Debug.Log($"Dialogue System: Successfully loaded {jsonFilename} containing {arcweaveProject.boards.Count} boards.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Dialogue System: Arcweave Project could not be deserialized: {e.Message}");
            }
            try
            {
                CatalogAllArcweaveTypes();
                RecordElementNames();
                RecordComponentNames();
                RecordBoardHierarchy();
                SortBoardElements();
                conversationElements.Clear();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Dialogue System: Unable to cache element names in deserialized Arcweave Project: {e.Message}");
                arcweaveProject = null;
                return false;
            }
        }

        protected void SortBoardElements()
        {
            foreach (var board in leafBoards.Values)
            {
                board.elements.Sort((a, b) => elementNames[a].CompareTo(elementNames[b]));
            }
        }

        protected void RecordBoardHierarchy()
        {
            // Record hierarchy so we can identify leaf nodes and path for conversation titles.
            rootBoardNode = null;
            foreach (var kvp in arcweaveProject.boards)
            {
                string guid = kvp.Key;
                Board board = kvp.Value;
                if (board.root)
                {
                    rootBoardNode = new ArcweaveBoardNode(guid, board, null);
                    break;
                }
            }
            if (rootBoardNode == null)
            {
                Debug.LogError("Dialogue System: Can't find root board in Arcweave Project.");
                arcweaveProject = null;
                return;
            }
            leafBoards.Clear();
            RecordBoardChildren(rootBoardNode);
        }

        protected void RecordBoardChildren(ArcweaveBoardNode boardNode)
        {
            if (boardNode == null || boardNode.board == null) return;
            if (boardNode.board.children == null)
            {
                if (!leafBoards.ContainsKey(boardNode.guid))
                {
                    leafBoards.Add(boardNode.guid, boardNode.board);
                }
            }
            else
            {
                foreach (var childGuid in boardNode.board.children)
                {
                    var childBoard = arcweaveLookup[childGuid] as Board;
                    var childNode = new ArcweaveBoardNode(childGuid, childBoard, boardNode);
                    boardNode.children.Add(childNode);
                    RecordBoardChildren(childNode);
                }
            }
        }

        protected void RecordElementNames()
        {
            // Sort elements:
            var elementList = arcweaveProject.elements.ToList();
            elementList.Sort((kvp1, kvp2) => GetElementName(kvp1.Value).CompareTo(GetElementName(kvp2.Value)));
            arcweaveProject.elements.Clear();
            elementList.ForEach(x => arcweaveProject.elements.Add(x.Key, x.Value));

            // Record elements:
            elementNames.Clear();
            foreach (var kvp in arcweaveProject.elements)
            {
                var name = GetElementName(kvp.Value);
                elementNames[kvp.Key] = name;
            }
        }

        protected string GetElementName(Element element)
        {
            if (element == null) return string.Empty;
            var name = TouchUpRichText(element.title);
            if (string.IsNullOrEmpty(name)) name = TouchUpRichText(element.content);
            return name.Replace("/", "\u2215");
        }

        protected void RecordComponentNames()
        {
            // Add default Player actor:
            var list = new List<string>();
            list.Add("Player");

            // Sort components:
            var componentList = arcweaveProject.components.ToList();
            componentList.Sort((kvp1, kvp2) => TouchUpRichText(kvp1.Value.name).CompareTo(TouchUpRichText(kvp2.Value.name)));
            arcweaveProject.components.Clear();
            componentList.ForEach(x => arcweaveProject.components.Add(x.Key, x.Value));

            // Add components:
            foreach (var kvp in arcweaveProject.components)
            {
                list.Add(TouchUpRichText(kvp.Value.name));
            }
            componentNames = list.ToArray();
        }

        #endregion

        #region Import

        public void Convert()
        {
            if (IsJsonLoaded())
            {
                CopySourceToDialogueDatabase(database);
                TouchUpDialogueDatabase(database);
            }
        }

        public void CopySourceToDialogueDatabase(DialogueDatabase database)
        {
            this.database = database;
            database.description = arcweaveProject.name;
            AddVariables();
            AddLocations();
            AddActors(playerComponentGuids, "Player", true);
            AddActors(npcComponentGuids, "NPC", false);
            AddConversations();
        }

        protected void CatalogAllArcweaveTypes()
        {
            arcweaveLookup.Clear();
            CatalogDictionary(arcweaveProject.boards);
            CatalogDictionary(arcweaveProject.notes);
            CatalogDictionary(arcweaveProject.elements);
            CatalogDictionary(arcweaveProject.jumpers);
            CatalogDictionary(arcweaveProject.connections);
            CatalogDictionary(arcweaveProject.branches);
            CatalogDictionary(arcweaveProject.components);
            CatalogDictionary(arcweaveProject.attributes);
            CatalogDictionary(arcweaveProject.assets);
            CatalogDictionary(arcweaveProject.variables);
            CatalogDictionary(arcweaveProject.conditions);
        }

        protected void CatalogDictionary<T>(Dictionary<string, T> dict) where T : ArcweaveType
        {
            foreach (var kvp in dict)
            {
                arcweaveLookup[kvp.Key] = kvp.Value;
            }
        }

        protected T LookupArcweave<T>(string guid) where T : ArcweaveType
        {
            ArcweaveType value;
            return arcweaveLookup.TryGetValue(guid, out value) ? (value as T) : null;
        }

        protected void AddVariables()
        {
            AddVariables("");
            if (numPlayers > 1)
            {
                for (var i = 0; i < numPlayers; i++)
                {
                    AddVariables("Player" + i + "_");
                }
            }
        }

        protected void AddVariables(string prefix)
        {
            foreach (var kvp in arcweaveProject.variables)
            {
                ArcweaveVariable arcweaveVariable = kvp.Value;
                if (!string.IsNullOrEmpty(arcweaveVariable.name))
                {
                    if (merge) database.variables.RemoveAll(x => x.Name == arcweaveVariable.name);
                    //---Was: var variable = template.CreateVariable(template.GetNextVariableID(database), arcweaveVariable.name, string.Empty);
                    var isGlobalVariable = globalVariables.Contains(arcweaveVariable.name);
                    if ((isGlobalVariable && !string.IsNullOrEmpty(prefix))) // Is global so don't need player-specific version.
                    {
                        continue;
                    }
                    var variable = template.CreateVariable(template.GetNextVariableID(database), prefix + arcweaveVariable.name, string.Empty);
                    database.variables.Add(variable);
                    switch (arcweaveVariable.type)
                    {
                        case "boolean":
                            variable.Type = FieldType.Boolean;
                            variable.InitialBoolValue = (bool)(arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value;
                            break;
                        case "float":
                            variable.Type = FieldType.Number;
                            variable.InitialFloatValue = (float)(arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value;
                            break;
                        case "integer":
                            variable.Type = FieldType.Number;
                            if ((arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value.GetType() == typeof(Int64))
                            {
                                variable.InitialFloatValue = (Int64)(arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value;
                            }
                            else if ((arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value.GetType() == typeof(int))
                            {
                                variable.InitialFloatValue = (int)(arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value;
                            }
                            else
                            {
                                Debug.Log($"Variable {kvp.Key} '{arcweaveVariable.name}' is not an int, type is: {(arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value.GetType().Name}");
                            }
                            break;
                        case "string":
                            variable.Type = FieldType.Text;
                            variable.InitialValue = (string)(arcweaveVariable.value as Newtonsoft.Json.Linq.JValue).Value;
                            break;
                        default:
                            Debug.LogWarning($"Dialogue System: Can't import variable {variable.Name} type '{arcweaveVariable.type}'.");
                            break;
                    }
                }
            }

            // Sort variables alphabetically:
            database.variables.Sort((a, b) => a.Name.CompareTo(b.Name));
        }

        protected void AddLocations()
        {
            foreach (string guid in locationComponentGuids)
            {
                ArcweaveComponent component;
                if (arcweaveProject.components.TryGetValue(guid, out component))
                {
                    if (merge) database.locations.RemoveAll(x => x.Name == component.name);
                    var location = template.CreateLocation(template.GetNextLocationID(database), component.name);
                    database.locations.Add(location);
                    AddAttributes(location.fields, component.attributes);
                }
            }
        }

        protected void AddActors(List<string> guids, string defaultActorName, bool isPlayer)
        {
            Actor actor = null;
            foreach (var guid in guids)
            {
                ArcweaveComponent component;
                if (arcweaveProject.components.TryGetValue(guid, out component))
                {
                    if (merge) database.actors.RemoveAll(x => x.Name == component.name);
                    actor = template.CreateActor(template.GetNextActorID(database), component.name, isPlayer);
                    actorLookup[guid] = actor;
                    database.actors.Add(actor);

                    AddAttributes(actor.fields, component.attributes);

#if UNITY_EDITOR
                    // Portrait:
                    if (importPortraits && component.assets != null && component.assets.cover != null)
                    {
                        ArcweaveAsset asset;
                        if (arcweaveProject.assets.TryGetValue(component.assets.cover.id, out asset))
                        {
                            var pathInProject = (!string.IsNullOrEmpty(arcweaveProjectPath) && arcweaveProjectPath.StartsWith("Assets"))
                                ? arcweaveProjectPath : "Assets";
                            var assetPath = pathInProject + "/assets/" + asset.name;
                            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
                            if (sprite != null)
                            {
                                actor.spritePortrait = sprite;
                            }
                            else
                            {
                                Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
                                if (texture != null)
                                {
                                    actor.portrait = texture;
                                }
                                else
                                {
                                    Debug.LogWarning($"Dialogue System: Can't find portrait image '{assetPath}' for actor {actor.Name}.");
                                }
                            }
                        }
                    }
#endif
                }
            }
            if (actor == null)
            {
                if (merge) database.actors.RemoveAll(x => x.Name == defaultActorName);
                actor = template.CreateActor(template.GetNextActorID(database), defaultActorName, isPlayer);
                database.actors.Add(actor);
            }
        }

        private void AddAttributes(List<Field> fields, List<string> attributes)
        {
            foreach (string attributeGuid in attributes)
            {
                Attribute attribute;
                if (arcweaveProject.attributes.TryGetValue(attributeGuid, out attribute))
                {
                    switch (attribute.value.type)
                    {
                        case "string":
                            var text = TouchUpRichText((string)(attribute.value.data as Newtonsoft.Json.Linq.JValue).Value);
                            Field.SetValue(fields, attribute.name, text);
                            break;
                    }
                }
            }
        }

        protected Actor FindActorByComponentIndex(int index)
        {
            return (0 <= index && index < componentNames.Length) ? database.actors.Find(x => x.Name == componentNames[index]) : null;
        }

        protected void AddConversations()
        {
            dialogueEntryLookup.Clear();

            foreach (var conversationInfo in conversationInfo)
            {
                Board board;
                var boardGuid = conversationInfo.boardGuid;
                if (arcweaveProject.boards.TryGetValue(boardGuid, out board))
                {
                    // Determine current conversation participants:
                    var player = FindActorByComponentIndex(conversationInfo.actorIndex) ?? database.actors.Find(x => x.IsPlayer);
                    var npc = FindActorByComponentIndex(conversationInfo.conversantIndex) ?? database.actors.Find(x => !x.IsPlayer);
                    currentPlayerID = (player != null) ? player.id : 0;
                    currentNpcID = (npc != null) ? npc.id : 1;

                    // Create conversation:
                    var conversationTitle = GetConversationTitle(board);
                    if (merge) database.conversations.RemoveAll(x => x.Title == conversationTitle);
                    var conversation = template.CreateConversation(template.GetNextConversationID(database), conversationTitle);
                    database.conversations.Add(conversation);
                    conversation.ActorID = currentPlayerID;
                    conversation.ConversantID = currentNpcID;

                    // Create <START> node:
                    var startEntry = template.CreateDialogueEntry(0, conversation.id, "START");
                    dialogueEntryLookup[boardGuid] = startEntry;
                    conversation.dialogueEntries.Add(startEntry);
                    startEntry.ActorID = currentPlayerID;
                    startEntry.ConversantID = currentNpcID;

                    // Add Elements:
                    foreach (var elementGuid in board.elements)
                    {
                        var element = LookupArcweave<Element>(elementGuid);
                        if (element == null) continue;
                        var entry = GetOrCreateDialogueEntry(conversation, elementGuid);
                        entry.Title = element.title;
                        entry.ActorID = GetActorIDFromTitle(element, currentNpcID);
                        entry.ConversantID = currentPlayerID;
                        SetActorIDsFromComponents(entry, element);
                        ProcessContent(entry, element.content);
                    }

                    // Add Connections:
                    foreach (var connectionGuid in board.connections)
                    {
                        var connection = LookupArcweave<Connection>(connectionGuid);
                        if (connection == null) continue;
                        string code;
                        var connectionLabel = ExtractCode(connection.label, out code);
                        var isBlank = string.IsNullOrEmpty(connectionLabel);
                        var entry = GetOrCreateDialogueEntry(conversation, connectionGuid);
                        entry.ActorID = isBlank ? currentNpcID : currentPlayerID;
                        entry.ConversantID = currentPlayerID;
                        entry.isGroup = isBlank;
                        entry.DialogueText = TouchUpRichText(connectionLabel);
                        //--- Added later: entry.userScript = string.IsNullOrEmpty(code) ? string.Empty : ConvertArcscriptToLua(code);
                        if (isBlank && string.IsNullOrEmpty(code)) entry.Title = DeleteTag;
                    }

                    // Add Branches:
                    foreach (var branchGuid in board.branches)
                    {
                        var branch = LookupArcweave<Branch>(branchGuid);
                        if (branch == null) continue;
                        var entry = GetOrCreateDialogueEntry(conversation, branchGuid);
                        entry.ActorID = currentNpcID;
                        entry.ConversantID = currentPlayerID;
                        entry.isGroup = true;
                    }

                    // Add Jumpers:
                    foreach (var jumperGuid in board.jumpers)
                    {
                        var jumper = LookupArcweave<Jumper>(jumperGuid);
                        if (jumper == null) continue;
                        var entry = GetOrCreateDialogueEntry(conversation, jumperGuid);
                        entry.ActorID = currentNpcID;
                        entry.ConversantID = currentPlayerID;
                        entry.Title = "Jumper." + jumper.elementId; // Will connect jumpers after creating all conversations and nodes.
                        entry.isGroup = true;
                    }

                    // Add Conditions:
                    foreach (var branchGuid in board.branches)
                    {
                        var branch = LookupArcweave<Branch>(branchGuid);
                        if (branch == null) continue;
                        var currentCumulativeCondition = string.Empty;
                        var ifEntry = CreateConditionEntry(conversation, branch.conditions.ifCondition, ref currentCumulativeCondition);
                        if (ifEntry != null)
                        {
                            ifEntry.Title = "if " + (arcweaveLookup[branch.conditions.ifCondition] as Condition).script;
                        }
                        if (branch.conditions.elseIfConditions != null)
                        {
                            var jArray = (Newtonsoft.Json.Linq.JArray)(branch.conditions.elseIfConditions);
                            foreach (var jToken in jArray)
                            {
                                var elseIfConditionGuid = (string)(jToken as Newtonsoft.Json.Linq.JValue).Value;
                                var elseIfEntry = CreateConditionEntry(conversation, elseIfConditionGuid, ref currentCumulativeCondition);
                                if (elseIfEntry != null)
                                {
                                    elseIfEntry.Title = "elseif " + (arcweaveLookup[elseIfConditionGuid] as Condition).script;
                                }
                            }
                        }
                        var elseEntry = CreateConditionEntry(conversation, branch.conditions.elseCondition, ref currentCumulativeCondition);
                        if (elseEntry != null)
                        {
                            elseEntry.Title = "else";
                        }
                    }

                    // Connect <START> node to starting element:
                    var startIndex = conversationInfo.startIndex;
                    if (!(0 <= startIndex && startIndex < board.elements.Count)) continue;
                    DialogueEntry firstRealEntry;
                    if (dialogueEntryLookup.TryGetValue(board.elements[startIndex], out firstRealEntry))
                    {
                        startEntry.outgoingLinks.Add(new Link(conversation.id, startEntry.id, conversation.id, firstRealEntry.id));
                    }

                    // Add Connections:
                    foreach (var connectionGuid in board.connections)
                    {
                        var connection = LookupArcweave<Connection>(connectionGuid);
                        if (connection == null) continue;
                        var entry = GetOrCreateDialogueEntry(conversation, connectionGuid);
                        entry.ActorID = currentPlayerID;
                        entry.ConversantID = currentNpcID;
                        ProcessContent(entry, connection.label);
                        if (string.IsNullOrEmpty(connection.label))
                        {
                            entry.ActorID = currentNpcID;
                            entry.isGroup = true;
                        }
                    }

                    //=======================================================================

                    // Connections are processed after adding all elements in all conversations.

                    // Add Condition links:
                    foreach (var branchGuid in board.branches)
                    {
                        var branch = LookupArcweave<Branch>(branchGuid);
                        if (branch == null) continue;
                        if (!string.IsNullOrEmpty(branch.conditions.ifCondition))
                        {
                            AddLink(branchGuid, branch.conditions.ifCondition);
                        }
                        if (branch.conditions.elseIfConditions != null)
                        {
                            var jArray = (Newtonsoft.Json.Linq.JArray)(branch.conditions.elseIfConditions);
                            foreach (var jToken in jArray)
                            {
                                var elseIfConditionGuid = (string)(jToken as Newtonsoft.Json.Linq.JValue).Value;
                                AddLink(branchGuid, elseIfConditionGuid);
                            }
                        }
                        if (!string.IsNullOrEmpty(branch.conditions.elseCondition))
                        {
                            AddLink(branchGuid, branch.conditions.elseCondition);
                        }
                    }
                }
            }

            // Handle connections here to handle cross-conversation links too:
            foreach (var conversationInfo in conversationInfo)
            {
                Board board;
                var boardGuid = conversationInfo.boardGuid;
                if (arcweaveProject.boards.TryGetValue(boardGuid, out board))
                {
                    // Add Connection links:
                    foreach (var connectionGuid in board.connections)
                    {
                        var connection = LookupArcweave<Connection>(connectionGuid);
                        if (connection == null) continue;
                        var a = AddLink(connection.sourceid, connectionGuid);
                        var b = AddLink(connectionGuid, connection.targetid);
                    }
                }
            }

            // Note: Jumpers are handled separately to handle cross-conversation jumps.

            SetElementOrderByOutputs();

            DeleteUnnecessaryConnectionEntries();
        }

        /// <summary>
        /// Extracts code tags and code (if present) from a connection label.
        /// It appears that a connection can only have label text or code, not both.
        /// </summary>
        /// <param name="label">Original label.</param>
        /// <param name="code">Extracted code.</param>
        /// <returns>Full label if no code tag, or empty string if extracted code.</returns>
        protected string ExtractCode(string label, out string code)
        {
            code = string.Empty;
            if (string.IsNullOrEmpty(label) || !label.Contains("<code>")) return label;
            var codeOpenTagPos = label.IndexOf("<code>");
            var codeCloseTagPos = label.IndexOf("</code>");
            var codePos = codeOpenTagPos + "<code>".Length;
            code = label.Substring(codePos, codeCloseTagPos - codePos);
            return string.Empty;
        }

        protected void SetElementOrderByOutputs()
        {
            foreach (var conversationInfo in conversationInfo)
            {
                Board board;
                var boardGuid = conversationInfo.boardGuid;
                if (arcweaveProject.boards.TryGetValue(boardGuid, out board))
                {
                    foreach (var elementGuid in board.elements)
                    {
                        var element = LookupArcweave<Element>(elementGuid);
                        var entry = dialogueEntryLookup[elementGuid];
                        if (element == null || entry == null) continue;
                        entry.outgoingLinks.Sort((x, y) => CompareOutputsPosition(x, y, element.outputs));
                    }
                }
            }
        }

        protected void DeleteUnnecessaryConnectionEntries()
        {
            foreach (var conversation in database.conversations)
            {
                var deletionList = new List<DialogueEntry>();
                foreach (var entry in conversation.dialogueEntries)
                {
                    foreach (var link in entry.outgoingLinks)
                    {
                        if (link.destinationConversationID != entry.conversationID) continue;
                        var linkedEntry = conversation.GetDialogueEntry(link.destinationDialogueID);
                        if (linkedEntry == null) continue;
                        if (linkedEntry.isGroup && linkedEntry.outgoingLinks.Count > 0 && linkedEntry.Title == DeleteTag)
                        {
                            link.destinationDialogueID = linkedEntry.outgoingLinks[0].destinationDialogueID;
                            deletionList.Add(linkedEntry);
                        }
                    }
                }
                deletionList.ForEach(entryToDelete => conversation.dialogueEntries.Remove(entryToDelete));
            }
        }

        private int CompareOutputsPosition(Link x, Link y, List<string> outputs)
        {
            if (outputs == null) return 0;
            var destinationX = database.GetDialogueEntry(x);
            var destinationY = database.GetDialogueEntry(y);
            if (destinationX == null || destinationY == null) return 0;
            var guidX = dialogueEntryLookup.FirstOrDefault(e => e.Value == destinationX).Key;
            var guidY = dialogueEntryLookup.FirstOrDefault(e => e.Value == destinationY).Key;
            if (string.IsNullOrEmpty(guidX) || string.IsNullOrEmpty(guidY)) return 0;
            var indexX = outputs.IndexOf(guidX);
            var indexY = outputs.IndexOf(guidY);
            if (indexX == -1 || indexY == -1 || indexX == indexY) return 0;
            return (indexX < indexY) ? -1 : 1;
        }

        protected string GetConversationTitle(Board conversationBoard)
        {
            string title;
            foreach (var boardNode in rootBoardNode.children)
            {
                if (TryGetConversationTitle(conversationBoard, boardNode, out title)) return title;
            }
            return conversationBoard.name;
        }

        protected bool TryGetConversationTitle(Board conversationBoard, ArcweaveBoardNode boardNode, out string title)
        {
            if (boardNode.board == conversationBoard)
            {
                title = boardNode.board.name;
                return true;
            }
            else
            {
                foreach (var childBoardNode in boardNode.children)
                {
                    if (TryGetConversationTitle(conversationBoard, childBoardNode, out title))
                    {
                        title = boardNode.board.name + "/" + title;
                        return true;
                    }
                }
                title = string.Empty;
                return false;
            }
        }

        protected int GetActorIDFromTitle(Element element, int currentNpcID)
        {
            if (element != null && element.title != null && element.title.Contains("Speaker:"))
            {
                var strippedTitle = Tools.StripRichTextCodes(element.title);
                var actorName = strippedTitle.Substring("Speaker:".Length).Trim();
                var actor = database.GetActor(actorName);
                if (actor != null) return actor.id;
            }
            return currentNpcID;
        }

        protected void SetActorIDsFromComponents(DialogueEntry entry, Element element)
        {
            if (entry == null || element == null || element.components == null) return;
            if (element.components.Count >= 1)
            {
                var firstComponentGuid = element.components[0];
                Actor actor;
                if (actorLookup.TryGetValue(firstComponentGuid, out actor))
                {
                    entry.ActorID = actor.id;
                }
            }
            if (element.components.Count >= 2)
            {
                var firstComponentGuid = element.components[1];
                Actor actor;
                if (actorLookup.TryGetValue(firstComponentGuid, out actor))
                {
                    entry.ConversantID = actor.id;
                }
            }
        }

        protected bool AddLink(string sourceGuid, string targetGuid)
        {
            DialogueEntry sourceEntry, targetEntry;
            if (!(dialogueEntryLookup.TryGetValue(sourceGuid, out sourceEntry) &&
                  dialogueEntryLookup.TryGetValue(targetGuid, out targetEntry))) return false;
            sourceEntry.outgoingLinks.Add(new Link(sourceEntry.conversationID, sourceEntry.id, targetEntry.conversationID, targetEntry.id));
            return true;
        }

        protected DialogueEntry GetOrCreateDialogueEntry(Conversation conversation, string guid)
        {
            DialogueEntry entry;
            if (!dialogueEntryLookup.TryGetValue(guid, out entry))
            {
                entry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(conversation), conversation.id, string.Empty);
                conversation.dialogueEntries.Add(entry);
                dialogueEntryLookup[guid] = entry;
                entry.DialogueText = string.Empty;
                entry.MenuText = string.Empty;
                entry.Sequence = string.Empty;
                if (importGuids)
                {
                    entry.fields.Add(new Field("Guid", guid, FieldType.Text));
                }
            }
            return entry;
        }

        protected DialogueEntry CreateConditionEntry(Conversation conversation, string conditionGuid, ref string currentCumulativeCondition)
        {
            if (string.IsNullOrEmpty(conditionGuid)) return null;
            ArcweaveType value;
            if (!arcweaveLookup.TryGetValue(conditionGuid, out value)) return null;
            var condition = value as Condition;
            var entry = GetOrCreateDialogueEntry(conversation, conditionGuid);
            entry.ActorID = currentNpcID;
            entry.ConversantID = currentPlayerID;
            entry.isGroup = true;
            var doesThisNodeHaveCondition = !string.IsNullOrEmpty(condition.script);
            var havePreviousConditions = !string.IsNullOrEmpty(currentCumulativeCondition);
            var sanitizedConditionScript = doesThisNodeHaveCondition ? ConvertArcscriptToLua(condition.script) : string.Empty;
            if (doesThisNodeHaveCondition && (sanitizedConditionScript.Contains(" and ") || sanitizedConditionScript.Contains(" or ")))
            {
                sanitizedConditionScript = $"({sanitizedConditionScript})";
            }
            var completeConditions = sanitizedConditionScript;
            if (havePreviousConditions)
            {
                // We have previous conditions in this if..elseif..else block, so prepend them:
                if (!doesThisNodeHaveCondition)
                {
                    // This node doesn't have conditions, so just require that it's not the previous conditions:
                    completeConditions = $"not ({currentCumulativeCondition})";
                }
                else
                {
                    // This node has conditions, so require its conditions and that it's not the previous conditions:
                    if (currentCumulativeCondition.StartsWith("("))
                    {
                        completeConditions = $"(not {currentCumulativeCondition}) and {sanitizedConditionScript}";
                    }
                    else
                    {
                        completeConditions = $"(not ({currentCumulativeCondition})) and {sanitizedConditionScript}";
                    }
                }
            }
            if (doesThisNodeHaveCondition)
            {
                // This node has conditions, so add then to the cumulative conditions for the next node:
                if (!havePreviousConditions)
                {
                    // This is our first cumulative condition, so just set it:
                    currentCumulativeCondition = sanitizedConditionScript;
                }
                else
                {
                    // We have previous cumulative conditions, so add this one:
                    currentCumulativeCondition = $"{currentCumulativeCondition} or {sanitizedConditionScript}";
                }
            }
            entry.conditionsString = completeConditions;
            return entry;
        }

        protected void ConnectJumpers()
        {
            // Handle jumpers here to handle cross-conversation links too:
            foreach (var conversationInfo in conversationInfo)
            {
                Board board;
                var boardGuid = conversationInfo.boardGuid;
                if (arcweaveProject.boards.TryGetValue(boardGuid, out board))
                {
                    foreach (var jumperGuid in board.jumpers)
                    {
                        var jumper = LookupArcweave<Jumper>(jumperGuid);
                        var jumperEntry = dialogueEntryLookup[jumperGuid];
                        if (jumper == null || jumper.elementId == null || jumperEntry == null) continue;
                        DialogueEntry destinationEntry;
                        if (!dialogueEntryLookup.TryGetValue(jumper.elementId, out destinationEntry)) continue;
                        if (destinationEntry == null) continue;
                        var destinationText = destinationEntry.Title;
                        if (string.IsNullOrEmpty(destinationText)) destinationText = destinationEntry.DialogueText;
                        jumperEntry.Title = string.IsNullOrEmpty(destinationText) ? "Jumper" : "Jumper: " + Tools.StripRichTextCodes(destinationText.Replace("\n", "").Replace("\r", "").Replace("<p>", "").Replace("</p>", ""));
                        jumperEntry.outgoingLinks.Clear();
                        AddLink(jumperGuid, jumper.elementId);
                    }
                }
            }
        }

        #endregion

        #region Touch Up Database

        public void TouchUpDialogueDatabase(DialogueDatabase database)
        {
            SetStartCutscenesToNone(database);
            ConnectJumpers();
            AddInlineCodeNodes();
            ExtractSequences();
            TouchUpRichText();
            SplitPipes();
        }

        protected void SetStartCutscenesToNone(DialogueDatabase database)
        {
            foreach (var conversation in database.conversations)
            {
                SetConversationStartCutsceneToNone(conversation);
            }
        }

        protected void SetConversationStartCutsceneToNone(Conversation conversation)
        {
            DialogueEntry entry = conversation.GetFirstDialogueEntry();
            if (entry == null)
            {
                Debug.LogWarning($"Dialogue System: Conversation '{conversation.Title}' doesn't have a START dialogue entry.");
            }
            else
            {
                if (string.IsNullOrEmpty(entry.currentSequence))
                {
                    Field.SetValue(entry.fields, "Sequence", "None()");
                }
            }
        }

        protected void TouchUpRichText()
        {
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    entry.Title = TouchUpRichText(entry.Title);
                    entry.DialogueText = TouchUpRichText(entry.DialogueText);
                    if (!string.IsNullOrEmpty(entry.userScript)) entry.userScript = ConvertArcscriptToLua(entry.userScript, true);
                    if (!string.IsNullOrEmpty(entry.conditionsString)) entry.conditionsString = ConvertArcscriptToLua(entry.conditionsString);
                    if (!entry.isGroup && string.IsNullOrEmpty(entry.DialogueText) && string.IsNullOrEmpty(entry.Sequence))
                    {
                        entry.Sequence = (entry.id == 0) ? NoneSequence : ContinueSequence;
                    }
                }
            }
        }

        protected void SplitPipes()
        {
            foreach (var conversation in database.conversations)
            {
                conversation.SplitPipesIntoEntries();
                foreach (var entry in conversation.dialogueEntries)
                {
                    entry.DialogueText = entry.DialogueText.Trim();
                }
            }
        }

        protected static Regex SequenceRegex = new Regex(@"\[SEQUENCE:[^\]]*\]");
        protected static Regex BlockRegex = new Regex(@"<p[^>]*>|</p>|<blockquote[^>]*>|</blockquote>|<span[^>]*>|</span>");
        protected static Regex CodeStartRegex = new Regex(@"<pre[^>]*><code>");
        protected static Regex CodeEndRegex = new Regex(@"</code></pre>");
        protected static Regex IdentifierRegex = new Regex(@"(?<![^\s+!*/-])\w+(?![^\s+*/-])");
        protected static Regex IncrementorRegex = new Regex(@"\+=|\-=");
        protected static Regex VisitsRegex = new Regex(@"visits\(<[^\)]+\)");
        protected static List<string> ReservedKeywords = new List<string>("if|elseif|else|endif|is|not|and|or|true|false|abs|sqr|sqrt|random|reset|resetAll|roll|show|visits".Split('|'));
        protected static string[] CodeFieldPrefixes = new string[] { "_IF", "_ELSEIF", "_ELSE" };

        protected enum CodeState { None, InIf, InElseIf, InElse }

        protected void ExtractSequences()
        {
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    ExtractSequence(entry);
                }
            }
        }

        protected void ExtractSequence(DialogueEntry entry)
        {
            var text = entry.DialogueText;
            if (string.IsNullOrEmpty(text)) return;
            var match = SequenceRegex.Match(text);
            if (match.Success)
            {
                if (!string.IsNullOrEmpty(entry.Sequence)) entry.Sequence += ";\n";
                entry.Sequence = text.Substring(match.Index + "[SEQUENCE:".Length, match.Length - "[SEQUENCE:]".Length).Trim();
                entry.DialogueText = text.Remove(match.Index, match.Length);
            }
        }

        protected string TouchUpRichText(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            // Replace emphasis tags:
            if (s.Contains("<em>"))
            {
                s = s.Replace("<em>", "<i>").Replace("</em>", "</i>");
            }

            var matches = BlockRegex.Matches(s);

            // Insert newlines for blocks containing <p>..</p>:
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                if (match.Value.StartsWith("<p>") || match.Value.StartsWith("</p>"))
                {
                    s = s.Insert(match.Index, "\n");
                }
            }

            s = BlockRegex.Replace(s, string.Empty);
            return Tools.RemoveHtml(s).Trim();
        }

        protected enum ContentPieceType { Text, Code }
        protected class ContentPiece
        {
            public ContentPieceType type;
            public string text;
            public ContentPiece(ContentPieceType type, string text)
            {
                this.type = type;
                this.text = text;
            }
        }

        protected void ProcessContent(DialogueEntry entry, string content)
        {
            // Put conditional code in temporary fields. They will be processed in the
            // touch-up phase as new child nodes. 
            // Put code outside of conditional blocks in the userScript variable.
            if (string.IsNullOrEmpty(content))
            {
                entry.DialogueText = string.Empty;
            }
            else if (!content.Contains("<code>"))
            {
                entry.DialogueText = TouchUpRichText(content);
            }
            else
            {
                // Split the content into pieces: text, code, text, code, text, etc.
                string s = content;
                var pieces = new List<ContentPiece>();
                var match = CodeStartRegex.Match(s);
                while (match.Success)
                {
                    // Create a piece for the text before the begin code tag:
                    var text = TouchUpRichText(s.Substring(0, match.Index));
                    if (!string.IsNullOrEmpty(text.Trim()))
                    {
                        pieces.Add(new ContentPiece(ContentPieceType.Text, text));
                    }

                    // Strip the text and begin code tag:
                    s = s.Substring(match.Index + match.Length);

                    // Locate the end code tag:
                    match = CodeEndRegex.Match(s);
                    if (match.Success)
                    {
                        // Create a piece for the code inside the code tags:
                        var code = s.Substring(0, match.Index);
                        pieces.Add(new ContentPiece(ContentPieceType.Code, code));

                        // Strip the code and end code tag:
                        s = s.Substring(match.Index + match.Length);
                    }

                    // Look for the next begin code tag:
                    match = CodeStartRegex.Match(s);
                }

                // Create a piece for the text after all of the code tags:
                s = TouchUpRichText(s);
                if (!string.IsNullOrEmpty(s.Trim()))
                {
                    pieces.Add(new ContentPiece(ContentPieceType.Text, s));
                }

                // Process pieces:
                entry.DialogueText = string.Empty;
                var hasIfStatement = false;
                var codeState = CodeState.None;
                var codeFieldPrefix = string.Empty;
                int numElseIf = 0;
                foreach (ContentPiece piece in pieces)
                {
                    switch (piece.type)
                    {
                        case ContentPieceType.Text:
                            switch (codeState)
                            {
                                case CodeState.None:
                                    if (hasIfStatement)
                                    {
                                        entry.fields.Add(new Field($"_POST_IF_TEXT", piece.text, FieldType.Text));
                                    }
                                    else
                                    {
                                        entry.DialogueText += piece.text;
                                    }
                                    break;
                                case CodeState.InIf:
                                case CodeState.InElseIf:
                                case CodeState.InElse:
                                    entry.fields.Add(new Field($"{codeFieldPrefix}_TEXT", piece.text, FieldType.Text));
                                    break;
                            }
                            break;
                        case ContentPieceType.Code:
                            var code = piece.text;
                            if (code.StartsWith("if "))
                            {
                                hasIfStatement = true;
                                codeState = CodeState.InIf;
                                codeFieldPrefix = "_IF";
                                code = code.Substring("if ".Length);
                            }
                            else if (code.StartsWith("elseif "))
                            {
                                codeState = CodeState.InElseIf;
                                codeFieldPrefix = $"_ELSEIF.{numElseIf}";
                                code = code.Substring("elseif ".Length);
                                numElseIf++;
                            }
                            else if (code.StartsWith("else "))
                            {
                                codeState = CodeState.InElse;
                                codeFieldPrefix = "_ELSE";
                                code = code.Substring("else ".Length);
                            }
                            else if (code.StartsWith("endif"))
                            {
                                codeState = CodeState.None;
                                codeFieldPrefix = string.Empty;
                            }
                            else
                            {
                                if (codeState == CodeState.None)
                                {
                                    if (!string.IsNullOrEmpty(entry.userScript)) entry.userScript += "\n";
                                    entry.userScript += code;
                                }
                                else
                                {
                                    entry.fields.Add(new Field($"{codeFieldPrefix}_INNER_CODE", code, FieldType.Text));
                                }
                            }
                            if (!string.IsNullOrEmpty(codeFieldPrefix))
                            {
                                entry.fields.Add(new Field($"{codeFieldPrefix}_CODE", code, FieldType.Text));
                            }
                            break;
                    }
                }
                entry.fields.Add(new Field("_NUM_ELSEIF", numElseIf.ToString(), FieldType.Number));
            }
        }

        protected void AddInlineCodeNodes()
        {
            foreach (var conversation in database.conversations)
            {
                int numEntries = conversation.dialogueEntries.Count;
                for (int i = numEntries - 1; i >= 0; i--)
                {
                    var entry = conversation.dialogueEntries[i];
                    var hasCodeToProcess = Field.FieldExists(entry.fields, "_IF_CODE");
                    if (!hasCodeToProcess) continue;

                    // Add post-if node (containing text after if block):
                    var postIfEntry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(conversation), conversation.id, string.Empty);
                    conversation.dialogueEntries.Add(postIfEntry);
                    postIfEntry.ActorID = entry.ActorID;
                    postIfEntry.ConversantID = entry.ConversantID;
                    foreach (var link in entry.outgoingLinks)
                    {
                        postIfEntry.outgoingLinks.Add(new Link(link));
                    }
                    var postIfField = Field.Lookup(entry.fields, "_POST_IF_TEXT");
                    entry.fields.Remove(postIfField);
                    if (postIfField == null || string.IsNullOrEmpty(postIfField.value))
                    {
                        postIfEntry.isGroup = true;
                    }
                    else
                    {
                        postIfEntry.DialogueText = TouchUpRichText(postIfField.value);
                    }

                    // Clear entry's links (since postIfEntry now links to them instead):
                    entry.outgoingLinks.Clear();

                    // Add code nodes between entry and postIfEntry:
                    foreach (var prefix in CodeFieldPrefixes)
                    {
                        if (prefix == "_ELSEIF")
                        {
                            var numElseIf = Field.LookupInt(entry.fields, "_NUM_ELSEIF");
                            for (int elseIndex = 0; elseIndex < numElseIf; elseIndex++)
                            {
                                InsertCodeEntry(conversation, entry, postIfEntry, $"{prefix}.{elseIndex}");
                            }
                        }
                        else
                        {
                            InsertCodeEntry(conversation, entry, postIfEntry, prefix);
                        }
                    }

                    // Finally add a fallthrough link from entry to postIfEntry:
                    entry.outgoingLinks.Add(new Link(conversation.id, entry.id, conversation.id, postIfEntry.id));
                }
            }
        }

        protected void InsertCodeEntry(Conversation conversation, DialogueEntry entry, DialogueEntry postIfEntry, string prefix)
        {
            var codeField = Field.Lookup(entry.fields, prefix + "_CODE");
            if (codeField == null) return;
            var textField = Field.Lookup(entry.fields, prefix + "_TEXT");
            var innerCodeField = Field.Lookup(entry.fields, prefix + "_INNER_CODE");
            entry.fields.Remove(codeField);
            entry.fields.Remove(textField);
            entry.fields.Remove(innerCodeField);
            var codeEntry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(conversation), conversation.id, string.Empty);
            conversation.dialogueEntries.Add(codeEntry);
            codeEntry.ActorID = entry.ActorID;
            codeEntry.ConversantID = entry.ConversantID;
            codeEntry.outgoingLinks.Add(new Link(conversation.id, codeEntry.id, conversation.id, postIfEntry.id));
            codeEntry.isGroup = string.IsNullOrEmpty(textField.value);
            codeEntry.Sequence = codeEntry.isGroup ? string.Empty : ContinueSequence;
            codeEntry.DialogueText = TouchUpRichText(textField.value);
            codeEntry.conditionsString = ConvertArcscriptToLua(codeField.value);
            if (innerCodeField != null) codeEntry.userScript = ConvertArcscriptToLua(innerCodeField.value, true);
            entry.outgoingLinks.Add(new Link(conversation.id, entry.id, conversation.id, codeEntry.id));
        }

        protected string ConvertArcscriptToLua(string code, bool convertIncrementors = false)
        {
            if (string.IsNullOrEmpty(code)) return code;
            code = Tools.RemoveHtml(code);
            code = ConvertVisits(code);
            if (convertIncrementors) code = ConvertIncrementors(code);
            code = ConvertArcscriptVariablesToLua(code);
            code = code.Replace("!=", "~=")
                .Replace("is not", "~=")
                .Replace("!", "not ")
                .Replace("&&", "and")
                .Replace("||", "or");
            return code;
        }

        // Convert visits() function calls.
        protected string ConvertVisits(string code)
        {
            if (!code.Contains("visits(")) return code; ;

            // Convert visits(...data-id="GUID"...) to visits("GUID")
            var matches = VisitsRegex.Matches(code);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                var visitsCall = code.Substring(match.Index, match.Length);
                var dataIdPos = visitsCall.IndexOf("data-id") + match.Index;
                var guid = code.Substring(dataIdPos + "data-id=\"".Length + 1);
                guid = guid.Substring(0, guid.IndexOf("\""));
                code = Replace(code, match.Index, match.Length, $"visits(\"{guid}\")");
            }

            return code.Replace("visits()", "visits(\"\")");
        }

        protected string ConvertIncrementors(string code)
        {
            // Convert "x +=" to "x = x +" and same for "-=":
            var matches = IncrementorRegex.Matches(code);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                // Get '+' or '-' of '+=' or '-=':
                var mathOp = match.Value.Substring(0, 1);

                // Get the variable name prior to the incrementor:
                var left = code.Substring(0, match.Index).TrimEnd();
                var variableStartPos = Mathf.Max(0, Mathf.Max(left.LastIndexOf('\n'), left.LastIndexOf(';')));
                var variableName = code.Substring(variableStartPos, match.Index - variableStartPos).Trim();

                // Replace "+= " with "= x +":
                code = Replace(code, match.Index, 2, $"= {variableName} {mathOp}");
            }
            return code;
        }

        protected string ConvertArcscriptVariablesToLua(string code)
        {
            var matches = IdentifierRegex.Matches(code);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                var identifier = match.Value;
                if (ReservedKeywords.Contains(identifier)) continue;
                if (database.variables.Find(x => x.Name == identifier) == null) continue;
                var luaVariable = $"Variable[\"{identifier}\"]";
                if (numPlayers > 1)
                {
                    //--- Blue Goo Games contribution to support multiple actors:
                    // If identifier begins with global, set luaVariable = $"Variable[\"{identifier}\"]";	
                    // Else set luaVariable = $Variable[Variable[\"ActorIndex\"] .. \"_{identifier}\"]	
                    // Create string array of words that are considered as global variables	
                    bool isGlobalVariable = identifier.StartsWith("global") || globalVariables.Contains(identifier);
                    luaVariable = isGlobalVariable ? $"Variable[\"{identifier}\"]" : $"Variable[Variable[\"ActorIndex\"] .. \"_{identifier}\"]";
                }
                code = Replace(code, match.Index, match.Length, luaVariable);
            }
            return code;
        }

        protected string Replace(string s, int index, int length, string replacement)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(s.Substring(0, index));
            builder.Append(replacement);
            builder.Append(s.Substring(index + length));
            return builder.ToString();
        }

        #endregion

    }
}

#endif
