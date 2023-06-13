#if USE_ARCWEAVE

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

#region Prefs

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

    [Serializable]
    public class ArcweaveImporterWindowPrefs : AbstractConverterWindowPrefs
    {
        public string arcweaveProjectPath;

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

        public string prefsPath;
    }

#endregion

    public class ArcweaveImporterWindow : AbstractConverterWindow<ArcweaveImporterWindowPrefs>
    {

#region Variables

        public static bool isOpen;

        public override string prefsKey { get { return "DialogueSystem.ArcweavePrefs"; } }

        protected enum BoardType { Ignore, Conversation, Quest }
        protected enum ComponentType { Ignore, Player, NPC, Item, Location }

        protected static GUIContent LoadJsonLabel = new GUIContent("Load JSON", "Load Arcweave project info.");
        protected static GUIContent SavePrefsLabel = new GUIContent("Save Prefs...", "Save import settings to JSON file.");
        protected static GUIContent LoadPrefsLabel = new GUIContent("Load Prefs...", "Load import settings from JSON file.");
        protected static GUIContent ClearPrefsLabel = new GUIContent("Clear Prefs...", "Clear import settings.");
        protected static GUIContent ImportPortraitsLabel = new GUIContent("Import Portraits", "Import portrait images from image files in Arcweave project folder.");
        protected static GUIContent ImportGuidsLabel = new GUIContent("Import Guids As Fields", "Add Guid field containing GUID for each object (element, component, etc.) imported from Arcweave project.");

        protected ArcweaveProject arcweaveProject = null;
        protected Dictionary<string, string[]> conversationElements = new Dictionary<string, string[]>();
        protected Dictionary<string, string> elementNames = new Dictionary<string, string>();
        protected string[] componentNames = new string[0];

        protected Dictionary<string, ArcweaveType> arcweaveLookup = new Dictionary<string, ArcweaveType>();
        protected Dictionary<string, DialogueEntry> dialogueEntryLookup = new Dictionary<string, DialogueEntry>();
        protected DialogueDatabase database;
        protected int currentPlayerID;
        protected int currentNpcID;
        protected const string NoneSequence = "None()";
        protected const string ContinueSequence = "Continue()";

        // Temporary tree structure for boards so we only show leaf nodes as possible conversations, and can prepend parents to conversation titles.
        public class BoardNode
        {
            public string guid;
            public Board board;
            public List<BoardNode> children = new List<BoardNode>();
            public BoardNode parent;

            public BoardNode(string guid, Board board, BoardNode parent)
            {
                this.guid = guid;
                this.board = board;
                this.parent = parent;
            }
        }

        protected BoardNode rootBoardNode = null;
        protected Dictionary<string, Board> leafBoards = new Dictionary<string, Board>();

        public ArcweaveImporterWindowPrefs importerPrefs { get { return prefs; } }

#endregion

#region GUI

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Arcweave...", false, 1)]
        public static ArcweaveImporterWindow Init()
        {
            var window = EditorWindow.GetWindow<ArcweaveImporterWindow>(false, "Arcweave Import");
            window.minSize = new Vector2(840f, 300f);
            return window;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            isOpen = true;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            isOpen = false;
        }

        protected override void DrawControls()
        {
            DrawSourceSection();
            DrawComponentTypeSection();
            DrawBoardTypeSection();
            DrawImportPortraitsSection();
            DrawImportGuidSection();
            DrawDestinationSection();
            DrawConversionButtons();
        }

        protected override void ClearPrefs()
        {
            base.ClearPrefs();
            arcweaveProject = null;
        }

        protected override void DrawSourceSection()
        {
            EditorGUILayout.BeginHorizontal();
            prefs.arcweaveProjectPath = EditorGUILayout.TextField(new GUIContent("Project Folder", "Folder containing Arcweave JSON and assets. Should be inside your Unity project."), prefs.arcweaveProjectPath);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.arcweaveProjectPath = EditorUtility.OpenFolderPanel("Arcweave Project Location", prefs.arcweaveProjectPath, "");
                prefs.arcweaveProjectPath = "Assets" + prefs.arcweaveProjectPath.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawComponentTypeSection()
        {
            if (!IsJsonLoaded()) return;
            prefs.componentsFoldout = EditorGUILayout.Foldout(prefs.componentsFoldout, "Components");
            if (!prefs.componentsFoldout) return;
            EditorGUI.indentLevel++;
            foreach (var kvp in arcweaveProject.components)
            {
                string componentGuid = kvp.Key;
                ArcweaveComponent arcweaveComponent = kvp.Value;
                if (arcweaveComponent.root) continue;
                var componentType = prefs.playerComponentGuids.Contains(componentGuid) ? ComponentType.Player
                    : prefs.npcComponentGuids.Contains(componentGuid) ? ComponentType.NPC
                    : prefs.itemComponentGuids.Contains(componentGuid) ? ComponentType.Item
                    : prefs.locationComponentGuids.Contains(componentGuid) ? ComponentType.Location
                    : ComponentType.Ignore;
                EditorGUI.BeginChangeCheck();
                componentType = (ComponentType)EditorGUILayout.EnumPopup(arcweaveComponent.name, componentType);
                if (EditorGUI.EndChangeCheck())
                {
                    prefs.playerComponentGuids.Remove(componentGuid);
                    prefs.npcComponentGuids.Remove(componentGuid);
                    prefs.itemComponentGuids.Remove(componentGuid);
                    prefs.locationComponentGuids.Remove(componentGuid);
                    switch (componentType)
                    {
                        case ComponentType.Player:
                            prefs.playerComponentGuids.Add(componentGuid);
                            break;
                        case ComponentType.NPC:
                            prefs.npcComponentGuids.Add(componentGuid);
                            break;
                        case ComponentType.Item:
                            prefs.itemComponentGuids.Add(componentGuid);
                            break;
                        case ComponentType.Location:
                            prefs.locationComponentGuids.Add(componentGuid);
                            break;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        protected void DrawBoardTypeSection()
        {
            if (!IsJsonLoaded()) return;
            prefs.boardsFoldout = EditorGUILayout.Foldout(prefs.boardsFoldout, "Boards");
            if (!prefs.boardsFoldout) return;
            EditorGUI.indentLevel++;
            foreach (var kvp in leafBoards)
            //foreach (var kvp in arcweaveProject.boards)
            {
                EditorGUILayout.BeginHorizontal();
                string boardGuid = kvp.Key;
                Board board = kvp.Value;

                // Draw board type popup:
                var conversationInfo = prefs.conversationInfo.Find(x => x.boardGuid == boardGuid);
                var boardType = (conversationInfo != null) ? BoardType.Conversation
                    : prefs.questBoardGuids.Contains(boardGuid) ? BoardType.Quest
                    : BoardType.Ignore;
                EditorGUI.BeginChangeCheck();
                boardType = (BoardType)EditorGUILayout.EnumPopup(board.name, boardType);
                if (EditorGUI.EndChangeCheck())
                {
                    prefs.conversationInfo.Remove(conversationInfo);
                    prefs.questBoardGuids.Remove(boardGuid);
                    switch (boardType)
                    {
                        case BoardType.Conversation:
                            conversationInfo = new ArcweaveConversationInfo(boardGuid);
                            prefs.conversationInfo.Add(conversationInfo);
                            break;
                        case BoardType.Quest:
                            prefs.questBoardGuids.Add(boardGuid);
                            break;
                    }
                }

                // Conversations - draw starting element popup:
                if (boardType == BoardType.Conversation)
                {

                    if (!conversationElements.ContainsKey(boardGuid))
                    {
                        // Sort board's elements:
                        board.elements.Sort((a, b) => elementNames[a].CompareTo(elementNames[b]));

                        // Then make list of their names:
                        var elements = new List<string>();
                        foreach (var elementGuid in board.elements)
                        {
                            elements.Add(elementNames[elementGuid]);
                        }
                        conversationElements[boardGuid] = elements.ToArray();
                    }
                    conversationInfo.startIndex = EditorGUILayout.Popup(GUIContent.none, conversationInfo.startIndex, conversationElements[boardGuid]);
                    conversationInfo.actorIndex = EditorGUILayout.Popup(GUIContent.none, conversationInfo.actorIndex, componentNames);
                    conversationInfo.conversantIndex = EditorGUILayout.Popup(GUIContent.none, conversationInfo.conversantIndex, componentNames);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        protected void DrawImportPortraitsSection()
        {
            if (!IsJsonLoaded()) return;
            prefs.importPortraits = EditorGUILayout.Toggle(ImportPortraitsLabel, prefs.importPortraits);
        }

        protected void DrawImportGuidSection()
        {
            if (!IsJsonLoaded()) return;
            prefs.importGuids = EditorGUILayout.Toggle(ImportGuidsLabel, prefs.importGuids);
        }

        protected override void DrawConversionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawLoadJsonButton();
            DrawSaveLoadPrefsButtons();
            DrawClearButton();
            DrawConvertButton();
            EditorGUILayout.EndHorizontal();
        }

        protected override void DrawClearButton()
        {
            if (GUILayout.Button(ClearPrefsLabel, GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Clear Import Settings", "Reset all Arcweave import settings?", "OK", "Cancel"))
                {
                    ClearPrefs();
                }
            }
        }

        protected void DrawSaveLoadPrefsButtons()
        {
            if (GUILayout.Button(SavePrefsLabel, GUILayout.Width(100)))
            {
                var path = EditorUtility.SaveFilePanel("Save Import Settings",
                    System.IO.Path.GetDirectoryName(prefs.prefsPath), System.IO.Path.GetFileName(prefs.prefsPath), "json");
                if (!string.IsNullOrEmpty(path))
                {
                    prefs.prefsPath = path;
                    System.IO.File.WriteAllText(path, JsonUtility.ToJson(prefs));
                }
            }
            if (GUILayout.Button(LoadPrefsLabel, GUILayout.Width(100)))
            {
                var path = EditorUtility.OpenFilePanel("Load Import Settings",
                    System.IO.Path.GetDirectoryName(prefs.prefsPath), "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var newPrefs = JsonUtility.FromJson<ArcweaveImporterWindowPrefs>(System.IO.File.ReadAllText(path));
                    if (newPrefs == null)
                    {
                        EditorUtility.DisplayDialog("Load Failed", $"Could not load Arcweave import settings from {path}.", "OK");
                    }
                    else
                    {
                        prefs = newPrefs;
                        prefs.prefsPath = path;
                        LoadJson();
                    }
                }
            }
        }

        protected void DrawLoadJsonButton()
        {
            if (GUILayout.Button(LoadJsonLabel, GUILayout.Width(100)))
            {
                LoadJson();
            }
        }

#endregion

#region Load JSON

        public void LoadAndConvert()
        {
            LoadJson();
            if (IsReadyToConvert())
            {
                Convert();
            }
        }

        protected bool IsJsonLoaded()
        {
            return arcweaveProject != null && arcweaveProject.boards != null && arcweaveProject.boards.Count > 0;
        }

        protected override bool IsSourceAssigned()
        {
            return IsJsonLoaded();
        }

        protected void LoadJson()
        {
            try
            {
                if (!System.IO.Directory.Exists(prefs.outputFolder))
                {
                    Debug.LogError($"Dialogue System: Output Folder '{prefs.outputFolder}' doesn't exist. Please create this folder before importing.");
                    return;
                }
                var pathInProject = (!string.IsNullOrEmpty(prefs.arcweaveProjectPath) && prefs.arcweaveProjectPath.StartsWith("Assets"))
                    ? prefs.arcweaveProjectPath.Substring("Assets".Length) : string.Empty;
                var jsonFilename = Application.dataPath + pathInProject + "/project_settings.json";
                if (!System.IO.File.Exists(jsonFilename))
                {
                    Debug.LogError($"Dialogue System: {jsonFilename} doesn't exist.");
                    return;
                }
                var json = System.IO.File.ReadAllText(jsonFilename);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"Dialogue System: Unable to read '{jsonFilename}'.");
                    return;
                }
                arcweaveProject = Newtonsoft.Json.JsonConvert.DeserializeObject<ArcweaveProject>(json);
                if (!IsJsonLoaded())
                {
                    Debug.LogError($"Dialogue System: Arcweave project '{jsonFilename}' is empty or could not be loaded.");
                    return;
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
                conversationElements.Clear();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Dialogue System: Unable to cache element names in deserialized Arcweave Project: {e.Message}");
                arcweaveProject = null;
            }
        }

        // Record hierarchy so we can identify leaf nodes and path for conversation titles.
        protected void RecordBoardHierarchy()
        {
            rootBoardNode = null;
            foreach (var kvp in arcweaveProject.boards)
            {
                string guid = kvp.Key;
                Board board = kvp.Value;
                if (board.root)
                {
                    rootBoardNode = new BoardNode(guid, board, null);
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

        protected void RecordBoardChildren(BoardNode boardNode)
        {
            if (boardNode == null || boardNode.board == null) return;
            if (boardNode.board.children == null)
            {
                leafBoards.Add(boardNode.guid, boardNode.board);
            }
            else
            {
                foreach (var childGuid in boardNode.board.children)
                {
                    var childBoard = arcweaveLookup[childGuid] as Board;
                    var childNode = new BoardNode(childGuid, childBoard, boardNode);
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

        protected override void CopySourceToDialogueDatabase(DialogueDatabase database)
        {
            this.database = database;
            database.description = arcweaveProject.name;
            AddVariables();
            AddLocations();
            AddActors(prefs.playerComponentGuids, "Player", true);
            AddActors(prefs.npcComponentGuids, "NPC", false);
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
            foreach (var kvp in arcweaveProject.variables)
            {
                ArcweaveVariable arcweaveVariable = kvp.Value;
                if (!string.IsNullOrEmpty(arcweaveVariable.name))
                {
                    var variable = template.CreateVariable(template.GetNextVariableID(database), arcweaveVariable.name, string.Empty);
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
            foreach (string guid in prefs.locationComponentGuids)
            {
                ArcweaveComponent component;
                if (arcweaveProject.components.TryGetValue(guid, out component))
                {
                    var location = template.CreateLocation(template.GetNextLocationID(database), component.name);
                    database.locations.Add(location);
                    location.Description = GetDescription(component.attributes);
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
                    actor = template.CreateActor(template.GetNextActorID(database), component.name, isPlayer);
                    database.actors.Add(actor);

                    actor.Description = GetDescription(component.attributes);

                    // Portrait:
                    if (prefs.importPortraits && component.assets != null && component.assets.cover != null)
                    {
                        ArcweaveAsset asset;
                        if (arcweaveProject.assets.TryGetValue(component.assets.cover.id, out asset))
                        {
                            var pathInProject = (!string.IsNullOrEmpty(prefs.arcweaveProjectPath) && prefs.arcweaveProjectPath.StartsWith("Assets"))
                                ? prefs.arcweaveProjectPath : "Assets";
                            var assetPath = pathInProject + "/assets/" + asset.name;
                            Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
                            if (sprite != null)
                            {
                                actor.spritePortrait = sprite;
                            }
                            else
                            {
                                Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
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
                }
            }
            if (actor == null)
            {
                actor = template.CreateActor(template.GetNextActorID(database), defaultActorName, isPlayer);
                database.actors.Add(actor);
            }
        }

        private string GetDescription(List<string> attributes)
        {
            foreach (string attributeGuid in attributes)
            {
                Attribute attribute;
                if (arcweaveProject.attributes.TryGetValue(attributeGuid, out attribute))
                {
                    if (attribute.name == "Description")
                    {
                        return TouchUpRichText((string)(attribute.value.data as Newtonsoft.Json.Linq.JValue).Value);
                    }
                }
            }
            return string.Empty;
        }

        protected Actor FindActorByComponentIndex(int index)
        {
            return (0 <= index && index < componentNames.Length) ? database.actors.Find(x => x.Name == componentNames[index]) : null;
        }

        protected void AddConversations()
        {
            dialogueEntryLookup.Clear();

            foreach (var conversationInfo in prefs.conversationInfo)
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
                    var conversation = template.CreateConversation(template.GetNextConversationID(database), GetConversationTitle(board));
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
                        entry.ActorID = GetActorIDFromTitle(element, currentNpcID);
                        entry.ConversantID = currentPlayerID;
                        entry.Title = element.title;
                        ProcessContent(entry, element.content);
                    }

                    // Add Connections:
                    foreach (var connectionGuid in board.connections)
                    {
                        var connection = LookupArcweave<Connection>(connectionGuid);
                        if (connection == null) continue;
                        var entry = GetOrCreateDialogueEntry(conversation, connectionGuid);
                        var isBlank = string.IsNullOrEmpty(connection.label);
                        entry.ActorID = isBlank ? currentNpcID : currentPlayerID;
                        entry.ConversantID = currentPlayerID;
                        entry.isGroup = isBlank;
                        //ProcessContent(entry, connection.label);
                        entry.DialogueText = connection.label;
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
                        var ifEntry = CreateConditionEntry(conversation, branch.conditions.ifCondition);
                        if (ifEntry != null)
                        {
                            ifEntry.Title = "if " + (arcweaveLookup[branch.conditions.ifCondition] as Condition).script;
                        }
                        var elseEntry = CreateConditionEntry(conversation, branch.conditions.elseCondition);
                        if (elseEntry != null)
                        {
                            elseEntry.Title = "else";
                        }
                        if (branch.conditions.elseIfConditions != null)
                        {
                            var jArray = (Newtonsoft.Json.Linq.JArray)(branch.conditions.elseIfConditions);
                            foreach (var jToken in jArray)
                            {
                                var elseIfConditionGuid = (string)(jToken as Newtonsoft.Json.Linq.JValue).Value;
                                var elseIfEntry = CreateConditionEntry(conversation, elseIfConditionGuid);
                                if (elseIfEntry != null)
                                {
                                    elseIfEntry.Title = "elseif " + (arcweaveLookup[elseIfConditionGuid] as Condition).script;
                                }
                            }
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

                    //--- Handled after adding all elements in all conversations: Add Connection links:
                    //foreach (var connectionGuid in board.connections)
                    //{
                    //    var connection = LookupArcweave<Connection>(connectionGuid);
                    //    if (connection == null) continue;
                    //    AddLink(connection.sourceid, connectionGuid);
                    //    AddLink(connectionGuid, connection.targetid);
                    //}

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
            foreach (var conversationInfo in prefs.conversationInfo)
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
        }

        protected void SetElementOrderByOutputs()
        {
            foreach (var conversationInfo in prefs.conversationInfo)
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

        protected bool TryGetConversationTitle(Board conversationBoard, BoardNode boardNode, out string title)
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
                if (prefs.importGuids)
                {
                    entry.fields.Add(new Field("Guid", guid, FieldType.Text));
                }
            }
            return entry;
        }

        protected DialogueEntry CreateConditionEntry(Conversation conversation, string conditionGuid)
        {
            if (string.IsNullOrEmpty(conditionGuid)) return null;
            ArcweaveType value;
            if (!arcweaveLookup.TryGetValue(conditionGuid, out value)) return null;
            var condition = value as Condition;
            var entry = GetOrCreateDialogueEntry(conversation, conditionGuid);
            entry.ActorID = currentNpcID;
            entry.ConversantID = currentPlayerID;
            entry.isGroup = true;
            entry.conditionsString = condition.script;
            return entry;
        }

        protected void ConnectJumpers()
        {
            // Handle jumpers here to handle cross-conversation links too:
            foreach (var conversationInfo in prefs.conversationInfo)
            {
                Board board;
                var boardGuid = conversationInfo.boardGuid;
                if (arcweaveProject.boards.TryGetValue(boardGuid, out board))
                {
                    foreach (var jumperGuid in board.jumpers)
                    {
                        var jumper = LookupArcweave<Jumper>(jumperGuid);
                        var jumperEntry = dialogueEntryLookup[jumperGuid];
                        if (jumper == null || jumperEntry == null) continue;
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

        protected override void TouchUpDialogueDatabase(DialogueDatabase database)
        {
            base.TouchUpDialogueDatabase(database);
            ConnectJumpers();
            AddInlineCodeNodes();
            ExtractSequences();
            TouchUpRichText();
            SplitPipes();
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
        protected static List<string> ReservedKeywords = new List<string>("if|elseif|else|endif|is|not|and|or|true|false|abs|sqr|sqrt|random|reset|resetAll|roll|show".Split('|'));
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
            var matches = BlockRegex.Matches(s);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                s = s.Insert(match.Index, "\n");
            }
            s = BlockRegex.Replace(s, string.Empty);
            return s.Replace(@"&lt;", "<")
                .Replace(@"&gt;", ">")
                .Replace(@"<strong>", "<b>")
                .Replace(@"</strong>", "</b>").Trim();
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
                entry.DialogueText = content;
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
                        postIfEntry.DialogueText = postIfField.value;
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
            codeEntry.DialogueText = textField.value;
            codeEntry.conditionsString = ConvertArcscriptToLua(codeField.value);
            if (innerCodeField != null) codeEntry.userScript = ConvertArcscriptToLua(innerCodeField.value, true);
            entry.outgoingLinks.Add(new Link(conversation.id, entry.id, conversation.id, codeEntry.id));
        }

        protected string ConvertArcscriptToLua(string code, bool convertIncrementors = false)
        {
            if (string.IsNullOrEmpty(code)) return code;
            if (convertIncrementors) code = ConvertIncrementors(code);
            code = ConvertArcscriptVariablesToLua(code);
            code = code.Replace("!=", "~=")
                .Replace("is not", "~=")
                .Replace("!", "not ")
                .Replace("&&", "and")
                .Replace("||", "or");
            return code;
        }

        protected string ConvertIncrementors(string code)
        {
            // Convert "x +=" to "x = x +" and same for "-=":
            var matches = IncrementorRegex.Matches(code);
            foreach (var match in matches.Cast<Match>().Reverse())
            {
                // Get the string (variable name) prior to the incrementor:
                var s = code.Substring(0, match.Index).TrimEnd(); ;
                var variableName = s.Contains(' ') ? s.Substring(s.LastIndexOf(' ') + 1) : s;

                // Replace "+=" with "= x +":
                var mathOp = match.Value.Substring(0, 1);
                code = Replace(code, match.Index, match.Length, $"= {variableName} {mathOp}");
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
