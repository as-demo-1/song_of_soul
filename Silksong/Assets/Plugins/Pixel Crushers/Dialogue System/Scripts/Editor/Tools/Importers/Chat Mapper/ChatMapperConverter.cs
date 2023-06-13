// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The Chat Mapper Converter editor window converts a Chat Mapper project into a 
    /// DialogueDatabase asset. With a Chat Mapper Indie License, you must first export your 
    /// project to XML. With a Commercial License, the Chat Mapper Converter can run Chat 
    /// Mapper on the command line to automatically export to XML.
    /// </summary>
    [InitializeOnLoad]
    public class ChatMapperConverter : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Chat Mapper...", false, 1)]
        public static void Init()
        {
            ChatMapperConverter window = EditorWindow.GetWindow(typeof(ChatMapperConverter), false, "Chat Mapper") as ChatMapperConverter;
            window.minSize = new Vector2(320, 240);
            window.LoadPrefs();
        }

        /// <summary>
        /// Registers a ProjectWindowItemOnGUI event handler to check for double-clicks on Chat Mapper Projects.
        /// </summary>
        static ChatMapperConverter()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        /// <summary>
        /// Checks for double-clicks on a Chat Mapper Project file to open Chat Mapper.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="selectionRect">Selection rect.</param>
        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.clickCount == 2 && selectionRect.Contains(Event.current.mousePosition) &&
                (AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".cmp", System.StringComparison.OrdinalIgnoreCase)))
            {
                OpenInChatMapper();
            }
        }

        private static void OpenInChatMapper()
        {
            string chatMapperExe = EditorPrefs.GetString(ChatMapperExeKey);
            if (string.IsNullOrEmpty(chatMapperExe))
            {
                Debug.Log(string.Format("{0}: Set the path to ChatMapper.exe in the Chat Mapper Converter window first.", DialogueDebug.Prefix));
            }
            else
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(Selection.activeObject));
                string chatMapperArgs = string.Format("\"{0}\"", ForwardSlashesToBackslashes(filePath));
                Debug.Log(string.Format("{0}: Running '\"{1}\" {2}'.", DialogueDebug.Prefix, ForwardSlashesToBackslashes(chatMapperExe), chatMapperArgs));
                System.Diagnostics.Process.Start(chatMapperExe, chatMapperArgs);
            }
        }

        public static ChatMapperConverter Instance { get { return instance; } }

        private static ChatMapperConverter instance = null;

        public enum ChatMapperProjectFormat { Cmp, Xml }

        private const string ChatMapperExeKey = "PixelCrushers.DialogueSystem.ChatMapperExe";
        public const string PrefsKey = "PixelCrushers.DialogueSystem.ChatMapperConverterPrefs";

        public enum PortraitType { Texture, Sprite }

        /// <summary>
        /// This is the prefs (converter window settings) class. The converter
        /// window uses an instance of this class to store the user's current settings.
        /// </summary>
        [System.Serializable]
        public class Prefs
        {

            public string pathToChatMapperExe = string.Empty;
            public ChatMapperProjectFormat projectFormat = ChatMapperProjectFormat.Xml;
            public List<string> projectFilenames = new List<string>();
            public List<bool> includeProjectFilenames = new List<bool>();
            public string portraitFolder = string.Empty;
            public PortraitType portraitType = PortraitType.Texture;
            public bool putEndSequenceOnLastSplit = true;
            public bool resetNodePositions = false;
            public string outputFolder = "Assets";
            public bool overwrite = false;
            public bool useProjectName = true;
            public string databaseName = "Dialogue Database";
            public EncodingType encodingType = EncodingType.Default;

            public void LoadPathToChatMapperExe()
            {
                pathToChatMapperExe = EditorPrefs.GetString(ChatMapperExeKey);
            }

            public static Prefs Load(string key)
            {
                string xml = EditorPrefs.GetString(key);
                Prefs prefs = null;
                if (!string.IsNullOrEmpty(xml))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Prefs));
                    prefs = xmlSerializer.Deserialize(new StringReader(xml)) as Prefs;
                }
                return prefs ?? new Prefs();
            }

            public static void Save(string key, Prefs prefs)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Prefs));
                StringWriter writer = new StringWriter();
                xmlSerializer.Serialize(writer, prefs);
                EditorPrefs.SetString(key, writer.ToString());
                EditorPrefs.SetString(ChatMapperExeKey, prefs.pathToChatMapperExe);
            }
        }

        /// <summary>
        /// The prefs for the converter window.
        /// </summary>
        protected Prefs prefs = null;

        /// <summary>
        /// A reference to the Dialogue System template, used to create new dialogue database
        /// assets such as Actors, Items, and Conversations.
        /// </summary>
        protected Template template = null;

        /// <summary>
        /// The current scroll position of the converter window. If the contents of the window
        /// are larger than the current window size, the user can scroll up and down.
        /// </summary>
        protected Vector2 scrollPosition = Vector2.zero;

        private GUIContent[] FormatOptions = new GUIContent[2] { new GUIContent("cmp", "CMP file; requires Chat Mapper Commercial"), new GUIContent("xml", "XML file; export from Chat Mapper first") };

        public static bool IsOpen { get { return (instance != null); } }

        public void OnEnable()
        {
            instance = this;
            LoadPrefs();
        }

        public void OnDisable()
        {
            SavePrefs();
            instance = null;
        }

        private void ClearPrefs()
        {
            prefs = new Prefs();
            prefs.LoadPathToChatMapperExe();
        }

        private void LoadPrefs()
        {
            prefs = Prefs.Load(PrefsKey);
            prefs.LoadPathToChatMapperExe();
        }

        private void SavePrefs()
        {
            Prefs.Save(PrefsKey, prefs);
        }

        public void OnGUI()
        {
            if (prefs == null) LoadPrefs();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            try
            {
                DrawControls();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawControls()
        {
            //EditorGUIUtility.LookLikeControls();

            // Project Format:
            EditorGUILayout.BeginHorizontal();
            ChatMapperProjectFormat newFormat = (ChatMapperProjectFormat)EditorGUILayout.Popup(new GUIContent("Project Format", "Importing from CMP requires Chat Mapper Commercial."), (int)prefs.projectFormat, FormatOptions);
            if (newFormat != prefs.projectFormat)
            {
                for (int i = 0; i < prefs.projectFilenames.Count; i++)
                {
                    if (newFormat == ChatMapperProjectFormat.Cmp)
                    {
                        prefs.projectFilenames[i] = prefs.projectFilenames[i].Replace("xml", "cmp");
                    }
                    else
                    {
                        prefs.projectFilenames[i] = prefs.projectFilenames[i].Replace("cmp", "xml");
                    }
                }
                prefs.projectFormat = newFormat;
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();

            // Path to ChatMapper.exe:
            if (prefs.projectFormat == ChatMapperProjectFormat.Cmp)
            {
                if (string.IsNullOrEmpty(prefs.pathToChatMapperExe))
                {
                    EditorGUILayout.HelpBox("To directly convert CMP files, the Dialogue System will run ChatMapper.exe in the background. Specify the location of ChatMapper.exe in the field below.", MessageType.Info);
                }
            }
            EditorGUILayout.BeginHorizontal();
            prefs.pathToChatMapperExe = EditorGUILayout.TextField(new GUIContent("Path to ChatMapper.exe", "Optional if converting XML. Also used to open CMP files in Project with double-click."), prefs.pathToChatMapperExe);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.pathToChatMapperExe = EditorUtility.OpenFilePanel("Path to ChatMapper.exe", "", "exe");
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();

            // Paths to Chat Mapper Projects:
            if (!HasValidProjectFilenames())
            {
                EditorGUILayout.HelpBox("Specify the Chat Mapper project(s) to convert. Click '+' to add a Chat Mapper project", MessageType.Info);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Chat Mapper Projects", "Specify project to convert here."));
            if (GUILayout.Button(new GUIContent("+", "Add another Chat Mapper project to convert"), EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.projectFilenames.Add(string.Empty);
                prefs.includeProjectFilenames.Add(true);
            }
            EditorGUILayout.EndHorizontal();
            if (prefs.includeProjectFilenames.Count < prefs.projectFilenames.Count)
            {
                for (int i = prefs.includeProjectFilenames.Count; i < prefs.projectFilenames.Count; i++)
                {
                    prefs.includeProjectFilenames.Add(true);
                }
            }
            int projectFilenameToDelete = -1;
            for (int i = 0; i < prefs.projectFilenames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefs.projectFilenames[i] = EditorGUILayout.TextField("    Filename", prefs.projectFilenames[i]);
                prefs.includeProjectFilenames[i] = EditorGUILayout.Toggle(prefs.includeProjectFilenames[i], GUILayout.Width(16));
                if (GUILayout.Button("...", EditorStyles.miniButtonMid, GUILayout.Width(22)))
                {
                    prefs.projectFilenames[i] = EditorUtility.OpenFilePanel("Select Chat Mapper Project", EditorWindowTools.GetDirectoryName(prefs.projectFilenames[i]), GetFormatExtension(prefs.projectFormat));
                    GUIUtility.keyboardControl = 0;
                }
                if (GUILayout.Button(new GUIContent("-", "Remove this slot."), EditorStyles.miniButtonRight, GUILayout.Width(22))) projectFilenameToDelete = i;
                EditorGUILayout.EndHorizontal();
            }
            if (projectFilenameToDelete >= 0)
            {
                prefs.projectFilenames.RemoveAt(projectFilenameToDelete);
            }

            // Portrait Folder, Format:
            EditorGUILayout.BeginHorizontal();
            prefs.portraitFolder = EditorGUILayout.TextField(new GUIContent("Portrait Folder", "Optional folder containing actor portrait textures. The converter will search this folder for textures matching any actor pictures defined in the Chat Mapper project."), prefs.portraitFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.portraitFolder = EditorUtility.OpenFolderPanel("Location of Portrait Textures", prefs.portraitFolder, "");
                prefs.portraitFolder = "Assets" + prefs.portraitFolder.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            prefs.portraitType = (PortraitType)EditorGUILayout.EnumPopup(new GUIContent("Portait Type", "Are portrait images Default/Texture or Sprite?"), prefs.portraitType);

            // Piped sequences:
            prefs.putEndSequenceOnLastSplit = EditorGUILayout.Toggle(new GUIContent("Smart Split", "When splitting at pipes (|), put {{end}} sequences on the last entry."), prefs.putEndSequenceOnLastSplit);

            // Reset CanvasRects:
            prefs.resetNodePositions = EditorGUILayout.Toggle(new GUIContent("Reset Node Positions", "If all the nodes end up in the upper left corner when importing, tick this to fix it."), prefs.resetNodePositions);

            // Save To:
            if (string.IsNullOrEmpty(prefs.outputFolder))
            {
                EditorGUILayout.HelpBox("In the field below, specify the folder to create the dialogue database asset(s) in.", MessageType.Info);
            }
            EditorGUILayout.BeginHorizontal();
            prefs.outputFolder = EditorGUILayout.TextField(new GUIContent("Save To", "Folder where dialogue database assets will be saved."), prefs.outputFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.outputFolder = EditorUtility.SaveFolderPanel("Path to Save Database", prefs.outputFolder, "");
                prefs.outputFolder = "Assets" + prefs.outputFolder.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();

            // Project/Database Name:
            bool hasMultipleProjects = (prefs.projectFilenames.Count > 1);
            if (hasMultipleProjects) prefs.useProjectName = true;
            EditorGUI.BeginDisabledGroup(hasMultipleProjects);
            prefs.useProjectName = EditorGUILayout.Toggle(new GUIContent("Use Project Name", "Tick to use project name defined in Chat Mapper project, untick to specify a name."), prefs.useProjectName);
            EditorGUI.EndDisabledGroup();
            if (!prefs.useProjectName)
            {
                prefs.databaseName = EditorGUILayout.TextField(new GUIContent("Dialogue Database Name", "Filename to create in Save To folder."), prefs.databaseName);
            }

            // Overwrite:
            prefs.overwrite = EditorGUILayout.Toggle(new GUIContent("Overwrite", "Tick to overwrite the dialogue database if it exists, untick to create a new copy."), prefs.overwrite);

            // Buttons:
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(100)))
            {
                ClearPrefs();
            }
            bool disabled = (string.IsNullOrEmpty(prefs.pathToChatMapperExe) && (prefs.projectFormat == ChatMapperProjectFormat.Cmp)) ||
                !HasValidProjectFilenames() ||
                string.IsNullOrEmpty(prefs.outputFolder) ||
                (!prefs.useProjectName && string.IsNullOrEmpty(prefs.databaseName));
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Import", GUILayout.Width(100)))
            {
                ConvertChatMapperProjects();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                SavePrefs();
            }
        }

        private string GetFormatExtension(ChatMapperProjectFormat format)
        {
            return (format == ChatMapperProjectFormat.Cmp) ? "cmp" : "xml";
        }

        private bool HasValidProjectFilenames()
        {
            return (prefs.projectFilenames.Count > 0) && !string.IsNullOrEmpty(prefs.projectFilenames[0]);
        }

        public void ConvertChatMapperProjects()
        {
            // If CMP, export to XML:
            if (prefs.projectFormat == ChatMapperProjectFormat.Cmp)
            {
                string projectFilenames = "\"";
                int numToExport = 0;
                for (int i = 0; i < prefs.projectFilenames.Count; i++)
                {
                    if ((i >= prefs.includeProjectFilenames.Count) || prefs.includeProjectFilenames[i])
                    {
                        projectFilenames += string.Format("{0}{1}", (numToExport == 0) ? string.Empty : ",", prefs.projectFilenames[i]);
                        numToExport++;
                    }
                }
                projectFilenames += "\"";
                if (numToExport > 0) ExportProjectsToXml(projectFilenames);
            }
            // Load each XML and convert to dialogue database:
            for (int i = 0; i < prefs.projectFilenames.Count; i++)
            {
                if ((i >= prefs.includeProjectFilenames.Count) || prefs.includeProjectFilenames[i])
                {
                    ConvertChatMapperProject(prefs.projectFilenames[i]);
                }
            }
        }

        private void ExportProjectsToXml(string projectFilenames)
        {
            string dataPath = Application.dataPath;
            if (dataPath.EndsWith("Assets")) dataPath = dataPath.Remove(dataPath.Length - "Assets".Length, "Assets".Length);
            string xmlDir = ForwardSlashesToBackslashes(dataPath + prefs.outputFolder);
            string chatMapperArgs = string.Format("-xml \"{0}\" {1}", xmlDir, ForwardSlashesToBackslashes(projectFilenames));
            Debug.Log(string.Format("{0}: Running \"{1}\" {2}", DialogueDebug.Prefix, ForwardSlashesToBackslashes(prefs.pathToChatMapperExe), chatMapperArgs));
            using (var chatMapperProcess = System.Diagnostics.Process.Start(prefs.pathToChatMapperExe, chatMapperArgs))
            {
                chatMapperProcess.WaitForExit();
            }
        }

        public void ConvertChatMapperProject(string projectFilename)
        {
            if (string.IsNullOrEmpty(projectFilename)) return;
            if (!System.IO.Directory.Exists(prefs.outputFolder))
            {
                Debug.LogError(string.Format("{0}: Folder '{1}' doesn't exist. Please create this folder before converting.", DialogueDebug.Prefix, prefs.outputFolder));
                return;
            }
            string xmlFilename = projectFilename;
            if (prefs.projectFormat == ChatMapperProjectFormat.Cmp)
            {
                xmlFilename = projectFilename.Substring(0, projectFilename.Length - 3) + "xml";
            }
            PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject = null;
            try
            {
                chatMapperProject = LoadChatMapperProject(xmlFilename);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Chat Mapper encountered an error converting {1}. Please check the Chat Mapper console window. Error: {2}", DialogueDebug.Prefix, projectFilename, e.Message));
            }
            if (IsValidChatMapperProject(chatMapperProject, projectFilename))
            {
                Debug.Log(string.Format("{0}: Loaded {1}.", DialogueDebug.Prefix, xmlFilename));
                try
                {
                    CreateDialogueDatabase(chatMapperProject);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("{0}: An internal conversion error occurred while converting {1}. Please check the Console view and contact Pixel Crushers support. Error: {2}", DialogueDebug.Prefix, projectFilename, e.Message));
                    EditorUtility.DisplayDialog("Internal Conversion Error", "Please check the Console view and contact Pixel Crushers support.", "OK");
                }
            }
            else
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}.", DialogueDebug.Prefix, xmlFilename));
            }
        }

        private PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject LoadChatMapperProject(string filename)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject));
            return xmlSerializer.Deserialize(new StreamReader(filename)) as PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject;
        }

        private static string ForwardSlashesToBackslashes(string s)
        {
            return s.Replace('/', '\\');
        }

        private string FullPath(string s)
        {
            return Application.dataPath + "/../" + s;
        }

        private bool IsValidChatMapperProject(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, string projectFilename)
        {
            if (chatMapperProject == null)
            {
                return false;
            }
            else if (IsChatMapperProjectMissingValues(chatMapperProject))
            {
                Debug.LogError(string.Format("{0}: Chat Mapper may not have exported {1} correctly. Please use Chat Mapper 1.3.x or 1.6.1.1+.", DialogueDebug.Prefix, projectFilename));
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool IsChatMapperProjectMissingValues(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject)
        {
            return (chatMapperProject == null) ||
                ((chatMapperProject.Assets.Actors.Count > 0) &&
                 (chatMapperProject.Assets.Actors[0].Fields.Count > 0) &&
                 (chatMapperProject.Assets.Actors[0].Fields[0].Value == null));
        }

        private void CreateDialogueDatabase(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject)
        {
            try
            {
                string databaseAssetName = prefs.useProjectName ? chatMapperProject.Title : prefs.databaseName;
                DialogueDatabase database = LoadOrCreateDatabase(databaseAssetName);
                if (database == null)
                {
                    Debug.LogError(string.Format("{0}: Couldn't create asset '{1}'.", DialogueDebug.Prefix, databaseAssetName));
                }
                else
                {
                    ConvertProjectAttributes(chatMapperProject, database);
                    ConvertActors(chatMapperProject, database);
                    ConvertItems(chatMapperProject, database);
                    ConvertLocations(chatMapperProject, database);
                    ConvertVariables(chatMapperProject, database);
                    ConvertConversations(chatMapperProject, database);
                    SaveDatabase(database, databaseAssetName);
                    Debug.Log(string.Format("{0}: Created database '{1}' containing {2} actors, {3} conversations, {4} items (quests), {5} variables, and {6} locations.",
                        DialogueDebug.Prefix, databaseAssetName, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private DialogueDatabase LoadOrCreateDatabase(string filename)
        {
            if (prefs.outputFolder.EndsWith("/")) prefs.outputFolder = prefs.outputFolder.Substring(0, prefs.outputFolder.Length - 1);
            string assetPath = string.Format("{0}/{1}.asset", prefs.outputFolder, filename);
            DialogueDatabase database = null;
            if (prefs.overwrite)
            {
                database = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueDatabase)) as DialogueDatabase;
                if (database != null) database.Clear();
            }
            if (database == null)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", prefs.outputFolder, filename));
                database = DatabaseUtility.CreateDialogueDatabaseInstance();
                AssetDatabase.CreateAsset(database, assetPath);
            }
            return database;
        }

        private void SaveDatabase(DialogueDatabase database, string filename)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        private void ConvertProjectAttributes(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.version = chatMapperProject.Version;
            database.author = chatMapperProject.Author;
            database.description = chatMapperProject.Description;
            database.globalUserScript = chatMapperProject.UserScript;
            database.emphasisSettings[0] = new EmphasisSetting(chatMapperProject.EmphasisColor1, chatMapperProject.EmphasisStyle1);
            database.emphasisSettings[1] = new EmphasisSetting(chatMapperProject.EmphasisColor2, chatMapperProject.EmphasisStyle2);
            database.emphasisSettings[2] = new EmphasisSetting(chatMapperProject.EmphasisColor3, chatMapperProject.EmphasisStyle3);
            database.emphasisSettings[3] = new EmphasisSetting(chatMapperProject.EmphasisColor4, chatMapperProject.EmphasisStyle4);
        }

        private void ConvertActors(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.actors = new List<Actor>();
            chatMapperProject.Assets.Actors.ForEach(a => database.actors.Add(new Actor(a)));
            database.actors.ForEach(a => FindPortraitTextures(a));
        }

        private void ConvertItems(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.items = new List<Item>();
            chatMapperProject.Assets.Items.ForEach(i => database.items.Add(new Item(i)));
        }

        private void ConvertLocations(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.locations = new List<Location>();
            chatMapperProject.Assets.Locations.ForEach(l => database.locations.Add(new Location(l)));
        }

        private void ConvertVariables(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.variables = new List<Variable>();
            int id = 0;
            foreach (var chatMapperVariable in chatMapperProject.Assets.UserVariables)
            {
                Variable variable = new Variable(chatMapperVariable);
                variable.id = id;
                id++;
                database.variables.Add(variable);
            }
        }

        private void ConvertConversations(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.conversations = new List<Conversation>();
            foreach (var chatMapperConversation in chatMapperProject.Assets.Conversations)
            {
                Conversation conversation = new Conversation(chatMapperConversation, prefs.putEndSequenceOnLastSplit);
                SetConversationStartCutsceneToNone(conversation);
                ChatMapperToDialogueDatabase.ConvertAudioFilesToSequences(conversation);
                ChatMapperToDialogueDatabase.ConvertOverridesFieldsToDisplaySettingsOverrides(conversation);
                database.conversations.Add(conversation);
            }
            ChatMapperToDialogueDatabase.FixConversationsLinkedToFirstEntry(database);
        }

        private void SetConversationStartCutsceneToNone(Conversation conversation)
        {
            DialogueEntry entry = conversation.GetFirstDialogueEntry();
            if (entry == null)
            {
                Debug.LogWarning(string.Format("{0}: Conversation '{1}' doesn't have a START dialogue entry.", DialogueDebug.Prefix, conversation.Title));
            }
            else
            {
                if (string.IsNullOrEmpty(entry.currentSequence))
                {
                    if (Field.FieldExists(entry.fields, "Sequence"))
                    {
                        entry.currentSequence = "Continue()";
                    }
                    else
                    {
                        entry.fields.Add(new Field("Sequence", "Continue()", FieldType.Text));
                    }
                }
            }
        }

        private void FindPortraitTextures(Actor actor)
        {
            if (actor == null) return;

            var field = Field.Lookup(actor.fields, DialogueSystemFields.Pictures);
            if ((field == null) || (field.value == null))
            {
                return;
            }
            var names = new List<string>(field.value.Split(new char[] { '[', ';', ']' }));
            names.RemoveAll(s => string.IsNullOrEmpty(s.Trim()));
            for (int i = 0; i < names.Count; i++)
            {
                string textureName = names[i];
                if (!string.IsNullOrEmpty(textureName.Trim()))
                {
                    string filename = Path.GetFileName(textureName).Replace('\\', '/');
                    string assetPath = string.Format("{0}/{1}", prefs.portraitFolder, filename);
                    switch (prefs.portraitType)
                    {
                        default:
                        case PortraitType.Texture:
                            Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
                            if (texture == null)
                            {
                                Debug.LogWarning(string.Format("{0}: Can't find portrait texture {1} for {2}.", DialogueDebug.Prefix, assetPath, actor.Name));
                            }
                            else if (i == 0) // First portrait.
                            {
                                actor.portrait = texture;
                            }
                            else
                            {
                                actor.alternatePortraits.Add(texture);
                            }
                            break;
                        case PortraitType.Sprite:
                            Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
                            if (sprite == null)
                            {
                                Debug.LogWarning(string.Format("{0}: Can't find portrait sprite {1} for {2}.", DialogueDebug.Prefix, assetPath, actor.Name));
                            }
                            else if (i == 0) // First portrait.
                            {
                                actor.spritePortrait = sprite;
                            }
                            else
                            {
                                actor.spritePortraits.Add(sprite);
                            }
                            break;
                    }
                }
            }
        }

    }

}
