#if USE_CELTX
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using PixelCrushers.DialogueSystem.Celtx;
using System.Text.RegularExpressions;
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

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Celtx GVR 2...", false, 1)]
        public static void Init()
        {
            CeltxConverterWindow window = EditorWindow.GetWindow(typeof(CeltxConverterWindow), false, "Celtx") as CeltxConverterWindow;
            window.minSize = new Vector2(320, 166);
        }

        public static CeltxConverterWindow Instance { get { return instance; } }

        private static CeltxConverterWindow instance = null;

        public const string PrefsKey = "PixelCrushers.DialogueSystem.CeltxConverterPrefs";

        /// <summary>
        /// This is the prefs (converter window settings) class. The converter
        /// window uses an instance of this class to store the user's current settings.
        /// </summary>
        [System.Serializable]
        public class Prefs
        {
            public string projectFilename = string.Empty;
            public string portraitFolder = string.Empty;
            //public bool putEndSequenceOnLastSplit = true;
            public bool resetNodePositions = false;
            public string outputFolder = "Assets";
            public bool overwrite = false;
            public bool useCeltxFilename = true;
            public bool importGameplayScriptText;
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
                prefs.projectFilename = EditorUtility.OpenFilePanel("Select Celtx Project", EditorWindowTools.GetDirectoryName(prefs.projectFilename), "json");
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();

            // Portrait Folder:
            EditorGUILayout.BeginHorizontal();
            prefs.portraitFolder = EditorGUILayout.TextField(new GUIContent("Portrait Folder", "Optional folder containing actor portrait textures. The converter will search this folder for textures matching any actor pictures defined in the Celtx project."), prefs.portraitFolder);
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

            // Check Sequence Syntax:
            prefs.checkSequenceSyntax = EditorGUILayout.Toggle(new GUIContent("Check Sequence Syntax", "Check the syntax of [SEQ] sequencer commands."), prefs.checkSequenceSyntax);

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
            prefs.useCeltxFilename = EditorGUILayout.Toggle(new GUIContent("Use Celtx Filename", "Tick to use Celtx export filename as Dialogue Database name, untick to specify a name."), prefs.useCeltxFilename);
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
            Celtx.CeltxDataRaw rawCeltxData = null;
            try
            {
                rawCeltxData = LoadCeltxDataFromJsonFile(projectFilename);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: There was an error loading and converting {1}. Error: {2}", DialogueDebug.Prefix, projectFilename, e.Message));
            }

            if (rawCeltxData != null)
            {
                Debug.Log(string.Format("{0}: Loaded {1}.", DialogueDebug.Prefix, projectFilename));
                try
                {
                    CreateDialogueDatabase(rawCeltxData);
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

        private static CeltxDataRaw LoadCeltxDataFromJsonFile(string path)
        {
            string jsonData = File.ReadAllText(path);
            jsonData = PreProcessCeltxData(jsonData);
            CeltxDataRaw rawCeltxData = null;
            try
            {
                rawCeltxData = Newtonsoft.Json.JsonConvert.DeserializeObject<CeltxDataRaw>(jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Celtx Data could not be deserialized : {0}", e.Message));
            }
            //JsonUtility.FromJson<CeltxDataRaw>(jsonData);
            return rawCeltxData;
        }

        /// <summary>
        /// Pre-processes the Celtx json data, converting the "item_data':'" pattern to "item_data_string':'"
        /// </summary> 
        /// <param name="jsonData">Celtx json data to be processed</param>
        /// <returns></returns>
        private static string PreProcessCeltxData(string jsonData)
        {
            string preProcessedJson = jsonData;
            try
            {
                // Edit out string conflicts
                preProcessedJson = Regex.Replace(jsonData, "item_data\":\"", "item_data_string\":\"");
                preProcessedJson = Regex.Replace(preProcessedJson, "linked\":{", "linked_jump\":{");
                preProcessedJson = Regex.Replace(preProcessedJson, "lock\":{", "lock_info\":{");

                Regex.Matches(preProcessedJson, "\"defaultValue\"");

                preProcessedJson = Regex.Replace(preProcessedJson, "\"defaultValue\":(false|true)", "\"defaultBool\":$1");
                preProcessedJson = Regex.Replace(preProcessedJson, "\"defaultValue\":\"", "\"defaultString\":\"");
                preProcessedJson = Regex.Replace(preProcessedJson, "\"defaultValue\":(-|\\d)", "\"defaultNum\":$1");

                Regex.Matches(preProcessedJson, "\"defaultValue\"");

                preProcessedJson = Regex.Replace(preProcessedJson, "\"value\":(false|true)", "\"boolVal\":$1");
                preProcessedJson = Regex.Replace(preProcessedJson, "\"value\":\"", "\"strVal\":\"");
                preProcessedJson = Regex.Replace(preProcessedJson, "\"value\":(-|\\d)", "\"longVal\":$1");

                preProcessedJson = Regex.Replace(preProcessedJson, "literals\":{", "literals\":[");
                preProcessedJson = Regex.Replace(preProcessedJson, "},(\\s*)\"clause", "],$1\"clause");
                preProcessedJson = Regex.Replace(preProcessedJson, "\"l(\\d*)\":{(\\s*)\"name", "{$2\"lit_name\":\"l$1\",$2\"name");
            } 
            catch (System.Exception e)
            {
                Debug.LogError("Error preprocessing json data : " + e.ToString());
            }
            

            return preProcessedJson;
        }

        private void CreateDialogueDatabase(CeltxDataRaw rawCeltxData)
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
                    if (rawCeltxData == null)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: unable to import celtx data.");
                    }
                    else
                    {
                        CeltxToDialogueDatabase celtxProcessor = new CeltxToDialogueDatabase();
                        celtxProcessor.ProcessRawCeltxData(rawCeltxData, database, prefs.importGameplayScriptText, prefs.importBreakdownCatalogContent, prefs.checkSequenceSyntax);
                        database.actors.ForEach(a => FindPortraitTexture(a));
                        if (prefs.sortConversationTitles) database.conversations.Sort((x, y) => x.Title.CompareTo(y.Title));
                        SaveDatabase(database, databaseAssetName);
                        Debug.Log(string.Format("{0}: Created database '{1}' containing {2} actors, {3} conversations, {4} items (quests), {5} variables, and {6} locations.",
                            DialogueDebug.Prefix, databaseAssetName, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count));
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

        private void FindPortraitTexture(Actor actor)
        {
            try { 
                if (actor == null) return;
                string textureName = actor.textureName;
                if (!string.IsNullOrEmpty(textureName))
                {
                    string filename = Path.GetFileName(textureName).Replace('\\', '/');
                    string assetPath = string.Format("{0}/{1}", prefs.portraitFolder, filename);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
                    if (texture == null)
                    {
                        Debug.LogWarning(string.Format("{0}: Can't find portrait texture {1} for {2}.", DialogueDebug.Prefix, assetPath, actor.Name));
                    }
                    actor.portrait = texture;
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