#if USE_CELTX3
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using PixelCrushers.DialogueSystem.Celtx;
using System.Reflection;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The Celtx Converter editor window converts a Celtx project into a 
    /// DialogueDatabase asset.
    /// </summary>
    [InitializeOnLoad]
    public class CeltxConverterWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Gem 3...", false, 1)]
        public static void Init()
        {
            CeltxConverterWindow window = EditorWindow.GetWindow(typeof(CeltxConverterWindow), false, "Gem Import") as CeltxConverterWindow;
            window.minSize = new Vector2(320, 166);
        }

        public static CeltxConverterWindow Instance { get { return instance; } }

        private static CeltxConverterWindow instance = null;

        public const string PrefsKey = "PixelCrushers.DialogueSystem.Celtx3ConverterPrefs";

        /// <summary>
        /// This is the prefs (converter window settings) class. The converter
        /// window uses an instance of this class to store the user's current settings.
        /// </summary>
        [System.Serializable]
        public class Prefs
        {
            public string projectFilename = string.Empty;
            public string portraitFolder = string.Empty;
            public bool resetNodePositions = false;
            public string outputFolder = "Assets";
            public bool overwrite = false;
            public bool useCeltxFilename = true;
            public bool importGameplayScriptText;
            public bool importGameplayAsEmptyNodes;
            public bool importBreakdownCatalogContent;
            public bool sortConversationTitles = false;
            public bool checkSequenceSyntax = true;
            public string databaseName = "Dialogue Database";
            public EncodingType encodingType = EncodingType.Default;

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
        }

        private void LoadPrefs()
        {
            prefs = Prefs.Load(PrefsKey);
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

            // Paths to Celtx Project:
            EditorGUILayout.BeginHorizontal();
            prefs.projectFilename = EditorGUILayout.TextField("Filename", prefs.projectFilename);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.projectFilename = EditorUtility.OpenFilePanel("Select Gem Project", EditorWindowTools.GetDirectoryName(prefs.projectFilename), "json");
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();

            // Portrait Folder:
            EditorGUILayout.BeginHorizontal();
            prefs.portraitFolder = EditorGUILayout.TextField(new GUIContent("Portrait Folder", "Optional folder containing actor portrait textures. The converter will search this folder for textures matching any actor pictures defined in the Gem project."), prefs.portraitFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.portraitFolder = EditorUtility.OpenFolderPanel("Location of Portrait Textures", prefs.portraitFolder, "");
                prefs.portraitFolder = "Assets" + prefs.portraitFolder.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();

            // Sort Conversation Titles:
            prefs.sortConversationTitles = EditorGUILayout.Toggle(new GUIContent("Sort Conversations", "Sort conversation by title."), prefs.sortConversationTitles);

            // Reset CanvasRects:
            prefs.resetNodePositions = EditorGUILayout.Toggle(new GUIContent("Reset Node Positions", "If all the nodes end up in the upper left corner when importing, tick this to fix it."), prefs.resetNodePositions);

            // Check Sequence syntax:
            prefs.checkSequenceSyntax = EditorGUILayout.Toggle(new GUIContent("Check Sequence Syntax", "Check the syntax of [SEQ] sequencer commands."), prefs.checkSequenceSyntax);

            // Import Breakdown content:
            prefs.importBreakdownCatalogContent = EditorGUILayout.Toggle(new GUIContent("Import Breakdown Items", "Import breakdown and mult-tag breakdown items as custom fields."), prefs.importBreakdownCatalogContent);

            // Import Gameplay content:
            prefs.importGameplayAsEmptyNodes = EditorGUILayout.Toggle(new GUIContent("Import Gameplay As Empty Nodes", "When a Gameplay section doesn't start with [SEQ], [COND], or [SCRIPT], import it as a separate empty node instead of ignoring it."), prefs.importGameplayAsEmptyNodes);

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
            prefs.useCeltxFilename = EditorGUILayout.Toggle(new GUIContent("Use Gem Filename", "Tick to use Gem export filename as Dialogue Database name, untick to specify a name."), prefs.useCeltxFilename);
            if (!prefs.useCeltxFilename)
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
            bool disabled = string.IsNullOrEmpty(prefs.outputFolder) ||
                (!prefs.useCeltxFilename && string.IsNullOrEmpty(prefs.databaseName));
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Import", GUILayout.Width(100)))
            {
                ConvertCeltxProject();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                SavePrefs();
            }
        }

        public void ConvertCeltxProject()
        {
            if (string.IsNullOrEmpty(prefs.projectFilename)) return;
            string projectFilename = prefs.projectFilename;
            if (!System.IO.Directory.Exists(prefs.outputFolder))
            {
                Debug.LogError(string.Format("{0}: Folder '{1}' doesn't exist. Please create this folder before converting.", DialogueDebug.Prefix, prefs.outputFolder));
                return;
            }
            dynamic celtxDataObject = null;
            try
            {
                celtxDataObject = LoadCeltxDataFromJsonFile(projectFilename);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: There was an error loading and converting {1}. Error: {2}", DialogueDebug.Prefix, projectFilename, e.Message));
            }

            if (celtxDataObject != null)
            {
                Debug.Log(string.Format("{0}: Loaded {1}.", DialogueDebug.Prefix, projectFilename));
                try
                {
                    CreateDialogueDatabase(celtxDataObject);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("{0}: An internal conversion error occurred while converting {1}. Please check the Console view and contact Pixel Crushers support. Error: {2}", DialogueDebug.Prefix, projectFilename, e.Message));
                    EditorUtility.DisplayDialog("Internal Conversion Error", "Please check the Console view and contact Pixel Crushers support.", "OK");
                }
            }
            else
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}.", DialogueDebug.Prefix, projectFilename));
            }
        }

        private static dynamic LoadCeltxDataFromJsonFile(string path)
        {
            string jsonData = File.ReadAllText(path);
            dynamic celtxGem3 = null;
            try
            {
                celtxGem3 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Gem Data could not be deserialized : {0}", e.Message));
            }
            return celtxGem3;
        }

        private void CreateDialogueDatabase(dynamic celtxDataObject)
        {
            try
            {
                string databaseAssetName = prefs.useCeltxFilename ? Path.GetFileNameWithoutExtension(prefs.projectFilename) : prefs.databaseName;
                DialogueDatabase database = LoadOrCreateDatabase(databaseAssetName);
                if (database == null)
                {
                    Debug.LogError(string.Format("{0}: Couldn't create asset '{1}'.", DialogueDebug.Prefix, databaseAssetName));
                }
                else
                {
                    // Process Raw Data
                    if (celtxDataObject == null)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Unable to import Gem data.");
                    }
                    else
                    {
                        CeltxGem3ToDialogueDatabase celtxProcessor = new CeltxGem3ToDialogueDatabase();
                        celtxProcessor.ProcessCeltxGem3DataObject(celtxDataObject, database, prefs.importGameplayAsEmptyNodes, prefs.importGameplayScriptText, prefs.importBreakdownCatalogContent, prefs.checkSequenceSyntax);
                        database.actors.ForEach(a => FindPortraitTextures(a));
                        if (prefs.sortConversationTitles) database.conversations.Sort((x, y) => x.Title.CompareTo(y.Title));
                        SaveDatabase(database, databaseAssetName);
                        Debug.Log(string.Format("{0}: Created database '{1}' containing {2} actors, {3} conversations, {4} items (quests), {5} variables, and {6} locations.",
                            DialogueDebug.Prefix, databaseAssetName, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count), database);
                        if (DialogueEditor.DialogueEditorWindow.instance != null)
                        {
                            DialogueEditor.DialogueEditorWindow.instance.Reset();
                            DialogueEditor.DialogueEditorWindow.instance.Repaint();
                        }
                    }
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
                if (database != null)
                {
                    database.Clear();
                    database.SyncAll();
                }
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

        private void FindPortraitTextures(Actor actor)
        {
            if (actor == null) return;
            var pictures = actor.LookupValue(CeltxFields.Pictures);
            if (string.IsNullOrEmpty(pictures) || pictures.Length <= 2) 
            {
                FindPortraitTexture(actor, actor.Name, false);
            }
            var textureNames = pictures.Substring(1, pictures.Length - 2).Split(';');
            foreach (var textureName in textureNames)
            {
                FindPortraitTexture(actor, textureName, true);
            }
        }

        private void FindPortraitTexture(Actor actor, string textureName, bool reportIfNotFound)
        {
            try { 
                if (actor == null) return;
                if (!string.IsNullOrEmpty(textureName))
                {
                    string filename = Path.GetFileNameWithoutExtension(textureName).Replace('\\', '/');
                    string assetPath = string.Format("{0}/{1}", prefs.portraitFolder, filename);
                    Sprite sprite = EditorTools.TryLoadSprite(assetPath);
                    if (sprite == null && reportIfNotFound)
                    {
                        Debug.LogWarning(string.Format("{0}: Can't find portrait sprite {1} for {2}.", DialogueDebug.Prefix, assetPath, actor.Name));
                    }
                    if (actor.spritePortrait == null)
                    {
                        actor.spritePortrait = sprite;
                    }
                    else
                    {
                        if (actor.spritePortraits == null) actor.spritePortraits = new System.Collections.Generic.List<Sprite>();
                        actor.spritePortraits.Add(sprite);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error in " + MethodBase.GetCurrentMethod() + " : actor=" + actor.Name + " | textureName=" + actor.textureName + e.ToString());
            }
        }
    }
}
#endif
