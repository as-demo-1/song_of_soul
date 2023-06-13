#if USE_AURORA
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace PixelCrushers.DialogueSystem.Aurora
{

    public class AuroraConverterWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Aurora (Neverwinter Nights)...", false, 1)]
        public static void Init()
        {
            AuroraConverterWindow window = EditorWindow.GetWindow(typeof(AuroraConverterWindow), false, "Aurora") as AuroraConverterWindow;
            window.LoadPrefs();
        }

        public static AuroraConverterWindow Instance { get { return instance; } }

        private static AuroraConverterWindow instance = null;

        // Private fields for the window:

        private const float ToggleWidth = 16;
        private const int PlayerID = 1;
        private const int DefaultNPCID = 2;

        private AuroraConverterPrefs prefs = null;
        private Template template = null;

        private bool actorsFoldout = true;
        private bool actorsHelp = false;
        private string[] actorList = null;

        private bool dlgFilesFoldout = true;
        private bool dlgFilesHelp = false;

        private bool jrlFilesFoldout = true;
        private bool jrlFilesHelp = false;

        private bool languagesFoldout = false;
        private bool languagesHelp = false;

        private bool variablesFoldout = false;
        private bool variablesHelp = false;

        private bool profilesFoldout = false;
        private bool profilesHelp = true;

        private bool saveAsHelp = false;
        private bool reportQuestsAndScriptsFlag = false;

        private Vector2 scrollPosition = Vector2.zero;

        private Dictionary<string, bool> activeLanguages = new Dictionary<string, bool>();

        private GUIStyle boldFoldoutStyle = null;

        private GUIStyle BoldFoldoutStyle
        {
            get
            {
                if (boldFoldoutStyle == null)
                {
                    boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                    boldFoldoutStyle.fontStyle = FontStyle.Bold;
                }
                return boldFoldoutStyle;
            }
        }

        private string lastXmlPath = string.Empty;

        public static bool IsOpen { get { return (instance != null); } }

        void OnEnable()
        {
            instance = this;
            minSize = new Vector2(320, 128);
        }

        void OnDisable()
        {
            if (prefs != null) prefs.Save();
            instance = null;
        }

        public void LoadPrefs()
        {
            if (prefs == null) prefs = AuroraConverterPrefs.Load();
        }

        /// <summary>
        /// Draws the converter window.
        /// </summary>
        void OnGUI()
        {
            //EditorGUIUtility.LookLikeControls();
            if (prefs == null) prefs = AuroraConverterPrefs.Load();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            try
            {
                EditorStyles.textField.wordWrap = true;
                DrawActorsSection();
                DrawDlgFileSection();
                DrawJrlFileSection();
                DrawLanguagesSection();
                DrawVariablesSection();
                DrawProfilesSection();
                DrawSaveToSection();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// Draws the Actors section.
        /// </summary>
        private void DrawActorsSection()
        {
            if (DrawActorsHeader())
            {
                EditorGUI.BeginChangeCheck();
                DrawActors();
                if (EditorGUI.EndChangeCheck()) actorList = null;
            }
        }

        private bool DrawActorsHeader()
        {
            EditorGUILayout.BeginHorizontal();
            actorsFoldout = EditorGUILayout.Foldout(actorsFoldout, "Actors", BoldFoldoutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButtonLeft, GUILayout.Width(22)))
            {
                actorsHelp = !actorsHelp;
                actorsFoldout = actorsFoldout || actorsHelp;
            }
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                int actorID = GetNextActorID();
                prefs.Actors.Add(new ActorSetting(actorID, string.Format("NPC {0}", actorID), string.Format("NPC{0}", actorID)));
                actorsFoldout = true;
            }
            EditorGUILayout.EndHorizontal();
            return actorsFoldout;
        }

        private void DrawActors()
        {
            EditorWindowTools.StartIndentedSection();
            if (actorsHelp) EditorGUILayout.HelpBox("Every conversation has an actor and a conversant. By default, they will be created between the Player and a generic actor named NPC. You can click '+' to define additional NPCs here or assign them later in the dialogue database editor or conversation triggers.", MessageType.Info);
            ActorSetting actorToDelete = null;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(100));
            EditorGUILayout.LabelField("Name");
            EditorGUILayout.LabelField("Tag");
            EditorGUILayout.LabelField("", GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            foreach (var actor in prefs.Actors)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(actor.id == PlayerID);
                actor.id = EditorGUILayout.IntField(actor.id, GUILayout.Width(100));
                actor.name = EditorGUILayout.TextField(actor.name);
                actor.tag = EditorGUILayout.TextField(actor.tag);
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22))) actorToDelete = actor;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            if (actorToDelete != null) prefs.Actors.Remove(actorToDelete);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load from Dialog Files", GUILayout.Width(200))) LoadActorsFromDlgFiles();
            EditorGUILayout.EndHorizontal();
            EditorWindowTools.EndIndentedSection();
        }

        private int GetNextActorID()
        {
            int lastID = 0;
            foreach (var actor in prefs.Actors)
            {
                lastID = Mathf.Max(lastID, actor.id);
            }
            return lastID + 1;
        }

        private void BuildActorList()
        {
            if (actorList == null)
            {
                List<string> list = new List<string>();
                for (int i = 0; i < prefs.Actors.Count; i++)
                {
                    list.Add(prefs.Actors[i].name);
                }
                actorList = list.ToArray();
            }
        }

        private int ActorListIndexToActorID(int index)
        {
            return ((0 <= index) && (index < prefs.Actors.Count)) ? prefs.Actors[index].id : 1;
        }

        private int ActorIDToActorListIndex(int id)
        {
            for (int i = 0; i < prefs.Actors.Count; i++)
            {
                if (prefs.Actors[i].id == id) return i;
            }
            return 0;
        }

        /// <summary>
        /// Draws the Dialogue Files section.
        /// </summary>
        private void DrawDlgFileSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            if (DrawDlgFileHeader())
            {
                BuildActorList();
                DrawDlgFileList();
            }
        }

        private bool DrawDlgFileHeader()
        {
            EditorGUILayout.BeginHorizontal();
            dlgFilesFoldout = EditorGUILayout.Foldout(dlgFilesFoldout, "Dialog Files", BoldFoldoutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButtonLeft, GUILayout.Width(22)))
            {
                dlgFilesHelp = !dlgFilesHelp;
                dlgFilesFoldout = dlgFilesFoldout || dlgFilesHelp;
            }
            if (GUILayout.Button("+*", EditorStyles.miniButtonMid, GUILayout.Width(22)))
            {
                AddDlgFolder();
                dlgFilesFoldout = true;
            }
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.DlgFiles.Add(new DlgFile(string.Empty, 1, 2));
                dlgFilesFoldout = true;
            }
            EditorGUILayout.EndHorizontal();
            return dlgFilesFoldout;
        }

        private void DrawDlgFileList()
        {
            EditorWindowTools.StartIndentedSection();
            if (dlgFilesHelp) EditorGUILayout.HelpBox("Specify the Aurora Toolset dlg.xml files to convert. Click '+' to add a new file or '+*' to add an entire folder.", MessageType.Info);
            DlgFile dlgFileToDelete = null;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dlg.Xml File");
            EditorGUILayout.LabelField("", GUILayout.Width(22));
            EditorGUILayout.LabelField("Actor", GUILayout.Width(200));
            EditorGUILayout.LabelField("Conversant", GUILayout.Width(200));
            EditorGUILayout.LabelField("", GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            prefs.DlgFiles.ForEach(dlgFile => DrawDlgFile(dlgFile, ref dlgFileToDelete));
            if (dlgFileToDelete != null) prefs.DlgFiles.Remove(dlgFileToDelete);
            EditorWindowTools.EndIndentedSection();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Default Conversant", GUILayout.Width(160))) { prefs.SetDefaultConversant(); actorList = null; }
            if (GUILayout.Button("Clear", GUILayout.Width(100))) prefs.DlgFiles.Clear();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDlgFile(DlgFile dlgFile, ref DlgFile dlgFileToDelete)
        {
            EditorGUILayout.BeginHorizontal();
            dlgFile.filename = EditorGUILayout.TextField(dlgFile.filename);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                dlgFile.filename = EditorUtility.OpenFilePanel("Select Dlg.Xml File", lastXmlPath, "xml");
                lastXmlPath = EditorWindowTools.GetDirectoryName(dlgFile.filename);
                GUIUtility.keyboardControl = 0;
            }
            dlgFile.actorID = ActorListIndexToActorID(EditorGUILayout.Popup(ActorIDToActorListIndex(dlgFile.actorID), actorList, GUILayout.Width(200)));
            dlgFile.conversantID = ActorListIndexToActorID(EditorGUILayout.Popup(ActorIDToActorListIndex(dlgFile.conversantID), actorList, GUILayout.Width(200)));
            if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(22))) dlgFileToDelete = dlgFile;
            EditorGUILayout.EndHorizontal();
        }

        private void AddDlgFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder of Dlg.Xml Files", lastXmlPath, string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                lastXmlPath = path;
                foreach (var filename in Directory.GetFiles(path, "*.dlg.xml"))
                {
                    prefs.DlgFiles.Add(new DlgFile(filename, 1, 2));
                }
            }
        }

        /// <summary>
        /// Draws the Journal Files section.
        /// </summary>
        private void DrawJrlFileSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            if (DrawJrlFileHeader())
            {
                DrawJrlFileList();
            }
        }

        private bool DrawJrlFileHeader()
        {
            EditorGUILayout.BeginHorizontal();
            jrlFilesFoldout = EditorGUILayout.Foldout(jrlFilesFoldout, "Journal Files", BoldFoldoutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButtonLeft, GUILayout.Width(22)))
            {
                jrlFilesHelp = !jrlFilesHelp;
                jrlFilesFoldout = jrlFilesFoldout || jrlFilesHelp;
            }
            if (GUILayout.Button("+*", EditorStyles.miniButtonMid, GUILayout.Width(22)))
            {
                AddJrlFolder();
                jrlFilesFoldout = true;
            }
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.JrlFiles.Add(new JrlFile(string.Empty));
                jrlFilesFoldout = true;
            }
            EditorGUILayout.EndHorizontal();
            return jrlFilesFoldout;
        }

        private void DrawJrlFileList()
        {
            EditorWindowTools.StartIndentedSection();
            if (jrlFilesHelp) EditorGUILayout.HelpBox("Specify the Aurora Toolset jrl.xml files to convert. Click '+' to add a new file or '+*' to add an entire folder.", MessageType.Info);
            JrlFile jrlFileToDelete = null;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Jrl.Xml File");
            EditorGUILayout.LabelField("", GUILayout.Width(22));
            EditorGUILayout.LabelField("", GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            prefs.JrlFiles.ForEach(jrlFile => DrawJrlFile(jrlFile, ref jrlFileToDelete));
            if (jrlFileToDelete != null) prefs.JrlFiles.Remove(jrlFileToDelete);
            EditorWindowTools.EndIndentedSection();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(100))) prefs.JrlFiles.Clear();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawJrlFile(JrlFile jrlFile, ref JrlFile jrlFileToDelete)
        {
            EditorGUILayout.BeginHorizontal();
            jrlFile.filename = EditorGUILayout.TextField(jrlFile.filename);
            if (GUILayout.Button("...", EditorStyles.miniButtonMid, GUILayout.Width(22)))
            {
                jrlFile.filename = EditorUtility.OpenFilePanel("Select Jrl.Xml File", lastXmlPath, "xml");
                lastXmlPath = EditorWindowTools.GetDirectoryName(jrlFile.filename);
                GUIUtility.keyboardControl = 0;
            }
            if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(22))) jrlFileToDelete = jrlFile;
            EditorGUILayout.EndHorizontal();
        }

        private void AddJrlFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder of Jrl.Xml Files", lastXmlPath, string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                lastXmlPath = path;
                foreach (var filename in Directory.GetFiles(path, "*.jrl.xml"))
                {
                    prefs.JrlFiles.Add(new JrlFile(filename));
                }
            }
        }

        /// <summary>
        /// Draws the Languages section.
        /// </summary>
        private void DrawLanguagesSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.BeginHorizontal();
            languagesFoldout = EditorGUILayout.Foldout(languagesFoldout, "Languages", BoldFoldoutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                languagesHelp = !languagesHelp;
                languagesFoldout = languagesFoldout || languagesHelp;
            }
            EditorGUILayout.EndHorizontal();
            if (languagesFoldout)
            {
                EditorWindowTools.StartIndentedSection();
                if (languagesHelp) EditorGUILayout.HelpBox("You can rename language localization codes here.", MessageType.Info);
                foreach (var language in prefs.Languages)
                {
                    language.languageName = EditorGUILayout.TextField(string.Format("Language ID {0}", language.languageId), language.languageName);
                }
                EditorWindowTools.EndIndentedSection();
            }
        }

        /// <summary>
        /// Draws the Variables section.
        /// </summary>
        private void DrawVariablesSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.BeginHorizontal();
            variablesFoldout = EditorGUILayout.Foldout(variablesFoldout, "Variables", BoldFoldoutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                variablesHelp = !variablesHelp;
                variablesFoldout = variablesFoldout || variablesHelp;
            }
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.Variables.Add(new VariableSetting());
                variablesFoldout = true;
            }
            EditorGUILayout.EndHorizontal();
            if (variablesFoldout)
            {
                EditorWindowTools.StartIndentedSection();
                if (variablesHelp) EditorGUILayout.HelpBox("You can define variable names for <CUSTOMxxxx> tokens here. To load all tokens used in the dialogue files above, click the Load from Dlg Files button.", MessageType.Info);
                VariableSetting variableToDelete = null;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("<CUSTOM#>", GUILayout.Width(100));
                EditorGUILayout.LabelField("Variable Name");
                EditorGUILayout.LabelField("Used In");
                EditorGUILayout.LabelField("", GUILayout.Width(22));
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                foreach (var variable in prefs.Variables)
                {
                    EditorGUILayout.BeginHorizontal();
                    variable.customId = EditorGUILayout.IntField(variable.customId, GUILayout.Width(100));
                    variable.variableName = EditorGUILayout.TextField(variable.variableName);
                    variable.usedInDlg = EditorGUILayout.TextField(variable.usedInDlg);
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(22))) variableToDelete = variable;
                    EditorGUILayout.EndHorizontal();
                }
                if (variableToDelete != null) prefs.Variables.Remove(variableToDelete);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Load from Dialog Files", GUILayout.Width(200))) LoadVariablesFromDlgFiles();
                EditorGUILayout.EndHorizontal();
                EditorWindowTools.EndIndentedSection();
            }
        }

        /// <summary>
        /// Draws the Converter Profiles section.
        /// </summary>
        private void DrawProfilesSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.BeginHorizontal();
            profilesFoldout = EditorGUILayout.Foldout(profilesFoldout, "Importer Profiles", BoldFoldoutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                profilesHelp = !profilesHelp;
                profilesFoldout = profilesFoldout || profilesHelp;
            }
            EditorGUILayout.EndHorizontal();
            if (profilesFoldout)
            {
                EditorWindowTools.StartIndentedSection();
                if (profilesHelp) EditorGUILayout.HelpBox("You can save and load importer settings as XML files here. This allows you to switch between different sets of Aurora files without losing your settings.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Load...", GUILayout.Width(100))) LoadConverterProfile();
                if (GUILayout.Button("Save...", GUILayout.Width(100))) SaveConverterProfile();
                if (GUILayout.Button("Run All..", GUILayout.Width(100))) RunAllProfilesInFolder();
                EditorGUILayout.EndHorizontal();
                EditorWindowTools.EndIndentedSection();
            }
        }

        /// <summary>
        /// Draws the "save to" fields.
        /// </summary>
        private void DrawSaveToSection()
        {
            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save As", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(22))) saveAsHelp = !saveAsHelp;
            EditorGUILayout.EndHorizontal();
            EditorWindowTools.StartIndentedSection();
            if (saveAsHelp) EditorGUILayout.HelpBox("A dialogue database asset with the specified filename will be created in the output folder. If overwrite is ticked, any existing asset will be overwritten. Click Convert to create the asset, or Clear to reset all settings. Tick Report Scripts/Quests to see which lines have scripts/quests that won't be converted.", MessageType.Info);
            DrawAlternateSearchFolderField();
            DrawOutputFolderField();
            DrawDatabaseFilenameField();
            DrawEncodingPopup();
            DrawScriptHandlingPopup();
            DrawOverwriteCheckbox();
            //--- Unused: DrawEnforceUniqueIDsCheckbox();
            DrawConversionButtons();
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawAlternateSearchFolderField()
        {
            EditorGUILayout.BeginHorizontal();
            prefs.AlternateSearchFolder = EditorGUILayout.TextField(new GUIContent("Alt. Search Folder", "If XML files aren't found in the paths specified above, search this folder."), prefs.AlternateSearchFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                var newFolder = EditorUtility.SaveFolderPanel("Alternate Search Folder", prefs.AlternateSearchFolder, "");
                if (!string.IsNullOrEmpty(newFolder))
                {
                    prefs.AlternateSearchFolder = newFolder;
                }
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutputFolderField()
        {
            EditorGUILayout.BeginHorizontal();
            prefs.OutputFolder = EditorGUILayout.TextField("Output Folder", prefs.OutputFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                var newFolder = EditorUtility.SaveFolderPanel("Folder to Save Database", prefs.OutputFolder, "");
                if (!string.IsNullOrEmpty(newFolder))
                {
                    prefs.OutputFolder = "Assets" + newFolder.Replace(Application.dataPath, string.Empty);
                }
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDatabaseFilenameField()
        {
            prefs.DatabaseFilename = EditorGUILayout.TextField("Database Filename", prefs.DatabaseFilename);
        }

        private void DrawEncodingPopup()
        {
            prefs.EncodingType = (EncodingType)EditorGUILayout.EnumPopup("Encoding", prefs.EncodingType, GUILayout.Width(300));
        }

        private void DrawScriptHandlingPopup()
        {
            prefs.ScriptHandlingType = (ScriptHandlingType)EditorGUILayout.EnumPopup("Convert Scripts To", prefs.ScriptHandlingType, GUILayout.Width(300));
        }

        private void DrawOverwriteCheckbox()
        {
            prefs.Overwrite = EditorGUILayout.Toggle("Overwrite", prefs.Overwrite);
        }

        //--- Unused: 
        //private void DrawEnforceUniqueIDsCheckbox() {
        //	prefs.EnforceUniqueIDs = EditorGUILayout.Toggle("Unique IDs", prefs.EnforceUniqueIDs);
        //}

        private void DrawConversionButtons()
        {
            reportQuestsAndScriptsFlag = EditorGUILayout.Toggle("Report Scripts/Quests", reportQuestsAndScriptsFlag);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawClearButton();
            DrawImportButton();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the Clear button, and clears the project if clicked.
        /// </summary>
        private void DrawClearButton()
        {
            if (GUILayout.Button("Clear", GUILayout.Width(100))) ClearConverterWindow();
        }

        /// <summary>
        /// Draws the Convert button, and converts the prefs into a dialogue database if clicked.
        /// </summary>
        private void DrawImportButton()
        {
            EditorGUI.BeginDisabledGroup(!IsReadyToConvert());
            if (GUILayout.Button("Import", GUILayout.Width(100))) ConvertAuroraFiles();
            EditorGUI.EndDisabledGroup();
        }

        private bool IsReadyToConvert()
        {
            return
                !string.IsNullOrEmpty(prefs.DatabaseFilename) &&
                !string.IsNullOrEmpty(prefs.OutputFolder) &&
                ((prefs.DlgFiles.Count > 0) || (prefs.JrlFiles.Count > 0));
        }

        private void ClearConverterWindow()
        {
            prefs.Clear();
        }

        private void LoadConverterProfile()
        {
            string filename = EditorUtility.OpenFilePanel("Load Profile XML File", "", "xml");
            if (!string.IsNullOrEmpty(filename))
            {
                LoadConverterProfileFromFile(filename);
            }
        }

        private void LoadConverterProfileFromFile(string filename)
        {
            try
            {
                StreamReader xmlFile = new StreamReader(filename);
                string xmlContents = xmlFile.ReadToEnd();
                xmlFile.Close();
                AuroraConverterPrefs newPrefs = AuroraConverterPrefs.FromXml(xmlContents);
                if (newPrefs != null) prefs = newPrefs;
                actorList = null;
            }
            catch (System.Exception)
            {
                EditorUtility.DisplayDialog("Invalid Importer Profile", string.Format("{0} is not a valid importer profile.", filename), "OK");
            }
        }

        private void SaveConverterProfile()
        {
            string filename = EditorUtility.SaveFilePanel("Save Profile XML File", "", "Aurora Importer Profile", "xml");
            if (!string.IsNullOrEmpty(filename))
            {
                if (!File.Exists(filename) || EditorUtility.DisplayDialog("Confirm Overwrite", string.Format("{0} exists. Overwrite it?", filename), "Overwrite", "Cancel"))
                {
                    try
                    {
                        string xmlContents = prefs.ToXml();
                        StreamWriter xmlFile = new StreamWriter(filename);
                        xmlFile.Write(xmlContents);
                        xmlFile.Close();
                    }
                    catch (System.Exception e)
                    {
                        EditorUtility.DisplayDialog("Error Saving Profile", e.Message, "OK");
                    }
                }
            }
        }

        private void RunAllProfilesInFolder()
        {
            var newFolder = EditorUtility.OpenFolderPanel("Run All Profiles In", prefs.ProfilesFolder, "");
            if (string.IsNullOrEmpty(newFolder)) return;
            prefs.ProfilesFolder = newFolder;
            Debug.Log("Running all profiles in: " + prefs.ProfilesFolder);
            foreach (var profileFilename in Directory.GetFiles(prefs.ProfilesFolder, "*.xml"))
            {
                try
                {
                    LoadConverterProfileFromFile(profileFilename);
                    ConvertAuroraFiles();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Failed to run profile: " + profileFilename + ". Error: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Loads the dialogue database if it already exists and overwrite is ticked; otherwise creates a new one.
        /// </summary>
        /// <returns>
        /// The database.
        /// </returns>
        /// <param name='filename'>
        /// Asset filename.
        /// </param>
        private DialogueDatabase LoadOrCreateDatabase()
        {
            string assetPath = string.Format("{0}/{1}.asset", prefs.OutputFolder, prefs.DatabaseFilename);
            DialogueDatabase database = null;
            if (prefs.Overwrite)
            {
                database = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueDatabase)) as DialogueDatabase;
                if (database != null) database.Clear();
            }
            if (database == null)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", prefs.OutputFolder, prefs.DatabaseFilename));
                database = DatabaseUtility.CreateDialogueDatabaseInstance();
                AssetDatabase.CreateAsset(database, assetPath);
            }
            return database;
        }

        /// <summary>
        /// Converts the Aurora data.
        /// </summary>
        public void ConvertAuroraFiles()
        {
            try
            {
                DialogueDatabase database = LoadOrCreateDatabase();
                if (database == null)
                {
                    Debug.LogError(string.Format("{0}: Couldn't create asset '{1}'.", DialogueDebug.Prefix, prefs.DatabaseFilename));
                }
                else
                {
                    EditorUtility.DisplayProgressBar("Importing Aurora Files", "Please wait...", 0);
                    if (template == null) template = TemplateTools.LoadFromEditorPrefs();
                    database.author = "Aurora Toolset";
                    AddEmphasesToDatabase(database);
                    AddVariablesToDatabase(database);
                    AddActorsToDatabase(database);
                    AddConversationsToDatabase(database);
                    AddJournalEntriesToDatabase(database);
                    EditorUtility.SetDirty(database);
                    AssetDatabase.SaveAssets();
                    Debug.Log(string.Format("{0}: Created database '{1}' containing {2} actors, {3} conversations, {4} items/quests, {5} variables, and {6} locations.",
                        DialogueDebug.Prefix, prefs.DatabaseFilename, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private int CurrentActorID = 1;

        private int CurrentConversantID = 2;

        private const int DefaultLanguageID = 0;

        private const int FirstDialogueEntryID = 0;

        private enum EntryType { Entry, Reply };

        private int StartActionEmphasis = 0;
        private int StartCheckEmphasis = 1;
        private int StartHighlightEmphasis = 2;

        private void AddEmphasesToDatabase(DialogueDatabase database)
        {
            // <StartAction> tag:
            database.emphasisSettings[StartActionEmphasis].bold = true;
            database.emphasisSettings[StartActionEmphasis].italic = true;
            database.emphasisSettings[StartActionEmphasis].color = Color.red;

            // <StartCheck> tag:
            database.emphasisSettings[StartCheckEmphasis].bold = true;
            database.emphasisSettings[StartCheckEmphasis].color = Color.green;

            // <StartHighlight> tag:
            database.emphasisSettings[StartHighlightEmphasis].bold = true;
            database.emphasisSettings[StartHighlightEmphasis].color = Color.blue;
        }

        private void AddVariablesToDatabase(DialogueDatabase database)
        {
            int variableID = 1;

            // Add custom variables:
            foreach (var variable in prefs.Variables)
            {
                database.variables.Add(template.CreateVariable(variableID++, DialogueLua.StringToTableIndex(variable.variableName), string.Empty));
            }

            // Add predefined variables:
            foreach (KeyValuePair<string, string> kvp in AuroraPredefinedTokens.all)
            {
                database.variables.Add(template.CreateVariable(variableID++, kvp.Value, string.Empty));
            }
        }

        private void AddActorsToDatabase(DialogueDatabase database)
        {
            foreach (var actorSetting in prefs.Actors)
            {
                Actor newActor = template.CreateActor(actorSetting.id, actorSetting.name, (actorSetting.id == PlayerID));
                newActor.fields.Add(new Field("Tag", actorSetting.tag, FieldType.Text));
                database.actors.Add(newActor);
            }
        }

        private void AddConversationsToDatabase(DialogueDatabase database)
        {
            int conversationID = 1;
            foreach (var dlgFile in prefs.DlgFiles)
            {
                if (!Tools.IsStringNullOrEmptyOrWhitespace(dlgFile.filename))
                {
                    EditorUtility.DisplayProgressBar("Importing Aurora Files", Path.GetFileName(dlgFile.filename), (float)conversationID / (float)prefs.DlgFiles.Count);
                    ConvertDlg(dlgFile, conversationID++, database);
                }
            }
        }

        private string FindFile(string filename)
        {
            if (!File.Exists(filename) && !string.IsNullOrEmpty(prefs.AlternateSearchFolder))
            {
                var altFilename = prefs.AlternateSearchFolder + "/" + Path.GetFileName(filename);
                if (File.Exists(altFilename)) return altFilename;
            }
            return filename;
        }

        private void ConvertDlg(DlgFile dlgFile, int conversationID, DialogueDatabase database)
        {
            Dlg dlg = null;
            try
            {
                dlg = Dlg.Load(FindFile(dlgFile.filename), prefs.Encoding);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}. Make sure the file is a valid dialog, and check the encoding type. Error: {2}", DialogueDebug.Prefix, FindFile(dlgFile.filename), e.Message));
                return;
            }
            try
            {
                if (dlg == null) return;
                CurrentActorID = dlgFile.actorID;
                CurrentConversantID = dlgFile.conversantID;
                database.version = dlg.version;
                activeLanguages.Clear();
                Conversation conversation = DlgToConversation(dlg, conversationID);
                database.conversations.Add(conversation);
                AddActiveLanguagesToAllDialogueEntries(conversation);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to convert {1}. Error: {2}", DialogueDebug.Prefix, dlgFile.filename, e.Message));
            }
        }

        private Conversation DlgToConversation(Dlg dlg, int conversationID)
        {
            Conversation conversation = template.CreateConversation(conversationID, dlg.name);
            conversation.ActorID = CurrentActorID;
            conversation.ConversantID = CurrentConversantID;

            // Add START dialogue entry:
            DialogueEntry firstDialogueEntry = template.CreateDialogueEntry(FirstDialogueEntryID, conversationID, "START");
            firstDialogueEntry.ActorID = CurrentActorID;
            firstDialogueEntry.ConversantID = CurrentConversantID;
            firstDialogueEntry.currentSequence = "Continue()";
            conversation.dialogueEntries.Add(firstDialogueEntry);

            // Convert EntryList into dialogue entries:
            foreach (var entry in dlg.EntryList)
            {
                int dialogueEntryID = EntryIDToDialogueEntryID(dlg, entry.ID);
                DialogueEntry dialogueEntry = template.CreateDialogueEntry(dialogueEntryID, conversationID, entry.Comment);
                dialogueEntry.ActorID = LookupSpeaker(entry, CurrentConversantID);
                dialogueEntry.ConversantID = CurrentActorID;
                AddTextToDialogueEntry(entry.Text, dialogueEntry, EntryType.Entry);
                if (entry.ShowOnce) dialogueEntry.conditionsString = string.Format("Dialog[{0}] ~= \"WasDisplayed\"", dialogueEntryID);
                conversation.dialogueEntries.Add(dialogueEntry);
                MarkEntryScripts(entry, dialogueEntry, dlg.name);
            }

            // Convert ReplyList into dialogue entries:
            foreach (var reply in dlg.ReplyList)
            {
                int dialogueEntryID = ReplyIDToDialogueEntryID(dlg, reply.ID);
                DialogueEntry dialogueEntry = template.CreateDialogueEntry(dialogueEntryID, conversationID, reply.Comment);
                dialogueEntry.ActorID = CurrentActorID;
                dialogueEntry.ConversantID = CurrentConversantID;
                AddTextToDialogueEntry(reply.Text, dialogueEntry, EntryType.Reply);
                if (reply.ShowOnce) dialogueEntry.conditionsString = string.Format("Dialog[{0}] ~= \"WasDisplayed\"", dialogueEntryID);
                conversation.dialogueEntries.Add(dialogueEntry);
                MarkEntryScripts(reply, dialogueEntry, dlg.name);
            }

            // Convert StartingList links:
            foreach (var startingEntry in dlg.StartingList)
            {
                Link link = new Link();
                link.originConversationID = conversationID;
                link.destinationConversationID = conversationID;
                link.originDialogueID = FirstDialogueEntryID;
                int destinationDialogueID = EntryIDToDialogueEntryID(dlg, startingEntry.Index);
                link.destinationDialogueID = destinationDialogueID;
                firstDialogueEntry.outgoingLinks.Add(link);
                MarkLinkCondition(startingEntry, conversation, destinationDialogueID, dlg.name);
            }

            // Convert EntryList[*].RepliesList links:
            foreach (var entry in dlg.EntryList)
            {
                DialogueEntry dialogueEntry = conversation.GetDialogueEntry(EntryIDToDialogueEntryID(dlg, entry.ID));
                foreach (var reply in entry.RepliesList)
                {
                    Link link = new Link();
                    link.originConversationID = conversation.id;
                    link.destinationConversationID = conversation.id;
                    link.originDialogueID = EntryIDToDialogueEntryID(dlg, entry.ID);
                    int destinationDialogueID = ReplyIDToDialogueEntryID(dlg, reply.Index);
                    link.destinationDialogueID = destinationDialogueID;
                    dialogueEntry.outgoingLinks.Add(link);
                    //---Was: MarkLinkCondition(entry, conversation, destinationDialogueID, dlg.name);
                    MarkLinkCondition(reply, conversation, destinationDialogueID, dlg.name);
                }
            }

            // Convert ReplyList[*].EntriesList links:
            foreach (var reply in dlg.ReplyList)
            {
                DialogueEntry dialogueEntry = conversation.GetDialogueEntry(ReplyIDToDialogueEntryID(dlg, reply.ID));
                foreach (var entry in reply.EntriesList)
                {
                    Link link = new Link();
                    link.originConversationID = conversation.id;
                    link.destinationConversationID = conversation.id;
                    link.originDialogueID = ReplyIDToDialogueEntryID(dlg, reply.ID);
                    int destinationDialogueID = EntryIDToDialogueEntryID(dlg, entry.Index);
                    link.destinationDialogueID = destinationDialogueID;
                    dialogueEntry.outgoingLinks.Add(link);
                    //---Was: MarkLinkCondition(reply, conversation, destinationDialogueID, dlg.name);
                    MarkLinkCondition(entry, conversation, destinationDialogueID, dlg.name);
                }
            }

            // Return the conversation:
            return conversation;
        }

        private int EntryIDToDialogueEntryID(Dlg dlg, int entryID)
        {
            return (1 + entryID);
        }

        private int ReplyIDToDialogueEntryID(Dlg dlg, int replyID)
        {
            return (1 + dlg.EntryList.Count + replyID);
        }

        private int LookupSpeaker(Struct entry, int defaultID)
        {
            string speakerTag = (entry != null) ? entry.Speaker : null;
            if (!string.IsNullOrEmpty(speakerTag))
            {
                ActorSetting actorSetting = prefs.Actors.Find(a => string.Equals(speakerTag, a.tag));
                if (actorSetting == null)
                {
                    Debug.LogWarning(string.Format("{0}: Entry {1} references a speaker with tag '{2}', but this tag isn't in the Actors list.", DialogueDebug.Prefix, entry.ID, speakerTag));
                }
                else
                {
                    return actorSetting.id;
                }
            }
            return defaultID;
        }

        private void AddTextToDialogueEntry(List<LocalString> text, DialogueEntry dialogueEntry, EntryType entryType)
        {
            foreach (var localString in text)
            {
                if (localString.LanguageID == DefaultLanguageID)
                {
                    dialogueEntry.DialogueText = ProcessText(localString.value);
                }
                else
                {
                    string dialogueTextFieldName = LanguageIDToLanguageName(localString.languageId);
                    string menuTextFieldName = string.Format("Menu Text {0}", dialogueTextFieldName);
                    string localizedFieldName = (entryType == EntryType.Entry) ? dialogueTextFieldName : menuTextFieldName;
                    Field.SetValue(dialogueEntry.fields, localizedFieldName, ProcessText(localString.value), FieldType.Localization);
                    if (!string.IsNullOrEmpty(localString.value))
                    {
                        activeLanguages[dialogueTextFieldName] = true;
                        activeLanguages[menuTextFieldName] = true;
                    }
                }
            }
        }

        private string LanguageIDToLanguageName(string languageId)
        {
            foreach (var language in prefs.Languages)
            {
                if (string.Equals(languageId, language.languageId)) return language.languageName;
            }
            return languageId;
        }

        private void AddActiveLanguagesToAllDialogueEntries(Conversation conversation)
        {
            EditorUtility.DisplayProgressBar("Importing Dialog Files", string.Format("Configuring localization in {0}...", conversation.Title), 1);
            foreach (var entry in conversation.dialogueEntries)
            {
                foreach (KeyValuePair<string, bool> kvp in activeLanguages)
                {
                    if (!Field.FieldExists(entry.fields, kvp.Key)) Field.SetValue(entry.fields, kvp.Key, string.Empty, FieldType.Localization);
                }
            }
        }

        private const string OpenStartTagBeginning = "<Start";
        private const string OpenStartActionTag = "<StartAction>";
        private const string OpenStartCheckTag = "<StartCheck>";
        private const string OpenStartHighlightTag = "<StartHighlight>";
        private const string CloseStartTag = "</Start>";

        private string ProcessText(string source)
        {
            if (source == null) return string.Empty;

            // Use FastReplacer in a future version to improve performance:
            string working = source;

            // Replace &apos;
            if (working.Contains("&apos;")) working = working.Replace("&apos;", "'");

            // Replace <StartXXX> tags:
            if (working.Contains("<Start"))
            {
                working = working.Replace("<StartAction>", "[em1]");
                working = working.Replace("<StartCheck>", "[em2]");
                working = working.Replace("<StartHighlight>", "[em3]");
                int sanityCheck = 0; // Make sure we don't infinite loop in case of bug.
                while (working.Contains("</Start>") && (sanityCheck < 1000))
                {
                    sanityCheck++;
                    int closeStartPos = working.IndexOf("</Start>");
                    working = working.Remove(closeStartPos, 8);
                    int EmPos = working.Substring(0, closeStartPos).LastIndexOf("[em");
                    if (EmPos == -1)
                    {
                        working = working.Insert(closeStartPos, "[/em1]");
                    }
                    else
                    {
                        if (working.Substring(EmPos).StartsWith("[em1]"))
                        {
                            working = working.Insert(closeStartPos, "[/em1]");
                        }
                        else if (working.Substring(EmPos).StartsWith("[em2]"))
                        {
                            working = working.Insert(closeStartPos, "[/em2]");
                        }
                        else
                        {
                            working = working.Insert(closeStartPos, "[/em3]");
                        }
                    }
                }
            }

            // Replace <CUSTOMxxxx> tags:
            if (working.Contains("<CUSTOM"))
            {
                string[] substrings = Regex.Split(working, @"(\<CUSTOM[0-9]+\>)");
                working = string.Empty;
                foreach (var substring in substrings)
                {
                    if (substring.StartsWith("<CUSTOM"))
                    {
                        int customId = GetCustomId(substring);
                        VariableSetting variable = prefs.GetVariable(customId);
                        string variableName = (variable != null) ? variable.variableName : string.Format("CUSTOM{0}", customId);
                        working = string.Format("{0}[var={1}]", working, DialogueLua.StringToTableIndex(variableName));
                    }
                    else
                    {
                        working = string.Format("{0}{1}", working, substring);
                    }
                }
            }

            // Replace predefined tokens:
            foreach (KeyValuePair<string, string> kvp in AuroraPredefinedTokens.all)
            {
                if (working.Contains(kvp.Key))
                {
                    working = working.Replace(kvp.Key, string.Format("[var={0}]", kvp.Value));
                }
            }

            return working;
        }

        private void LoadVariablesFromDlgFiles()
        {
            try
            {
                int current = 0;
                foreach (var dlgFile in prefs.DlgFiles)
                {
                    EditorUtility.DisplayProgressBar("Analyzing Aurora Dlg Files", Path.GetFileName(dlgFile.filename), (float)(current++) / (float)prefs.DlgFiles.Count);
                    LoadVariablesFromDlgFile(dlgFile);
                }
                foreach (var jrlFile in prefs.JrlFiles)
                {
                    EditorUtility.DisplayProgressBar("Analyzing Aurora Jrl Files", Path.GetFileName(jrlFile.filename), (float)(current++) / (float)prefs.JrlFiles.Count);
                    LoadVariablesFromJrlFile(jrlFile);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void LoadVariablesFromDlgFile(DlgFile dlgFile)
        {
            try
            {
                Dlg dlg = Dlg.Load(FindFile(dlgFile.filename), prefs.Encoding);
                if (dlg != null)
                {
                    if (dlg.EntryList != null)
                    {
                        dlg.EntryList.ForEach(entry => LoadVariablesFromLocalStrings(entry.Text, dlg.name));
                    }
                    if (dlg.ReplyList != null)
                    {
                        dlg.ReplyList.ForEach(reply => LoadVariablesFromLocalStrings(reply.Text, dlg.name));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}. Error: {2}", DialogueDebug.Prefix, dlgFile.filename, e.Message));
            }
        }

        private void LoadVariablesFromJrlFile(JrlFile jrlFile)
        {
            try
            {
                Jrl jrl = Jrl.Load(FindFile(jrlFile.filename), prefs.Encoding);
                if (jrl != null && jrl.topLevelStruct != null)
                {
                    foreach (var jrlStruct in jrl.Categories)
                    {
                        var entryList = jrlStruct.GetElementStructs("EntryList");
                        entryList.ForEach(entry => LoadVariablesFromLocalStrings(entry.Text, jrl.name));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}. Error: {2}", DialogueDebug.Prefix, jrlFile.filename, e.Message));
            }
        }

        private void LoadVariablesFromLocalStrings(List<LocalString> localStrings, string dlgName)
        {
            if (localStrings != null)
            {
                Regex regex = new Regex(@"\<CUSTOM[0-9]+\>");
                foreach (var localString in localStrings)
                {
                    if (!string.IsNullOrEmpty(localString.value) && (localString.value.Contains("<CUSTOM")))
                    {
                        foreach (Match match in regex.Matches(localString.value))
                        {
                            AddVariableIfNew(GetCustomId(match.Value), localString.value, dlgName);
                        }
                    }
                }
            }
        }

        private void AddVariableIfNew(int customId, string line, string dlgName)
        {
            foreach (var variable in prefs.Variables)
            {
                if (variable.customId == customId) return;
            }
            prefs.Variables.Add(new VariableSetting(customId, string.Format("CUSTOM{0}", customId), string.Format("{0}: {1}", dlgName, line)));
        }

        private int GetCustomId(string customTag)
        {
            return Tools.StringToInt(customTag.Substring(7, customTag.Length - 8));
        }

        private void LoadActorsFromDlgFiles()
        {
            try
            {
                int current = 0;
                foreach (var dlgFile in prefs.DlgFiles)
                {
                    EditorUtility.DisplayProgressBar("Analyzing Aurora Files", Path.GetFileName(dlgFile.filename), (float)(current++) / (float)prefs.DlgFiles.Count);
                    LoadActorsFromDlgFile(dlgFile);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void LoadActorsFromDlgFile(DlgFile dlgFile)
        {
            try
            {
                Dlg dlg = Dlg.Load(FindFile(dlgFile.filename), prefs.Encoding);
                if (dlg != null)
                {
                    if (dlg.EntryList != null)
                    {
                        dlg.EntryList.ForEach(entry => LoadActorsFromEntry(entry));
                    }
                    if (dlg.ReplyList != null)
                    {
                        dlg.ReplyList.ForEach(reply => LoadActorsFromEntry(reply));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}. Error: {2}", DialogueDebug.Prefix, dlgFile.filename, e.Message));
            }
        }

        private void LoadActorsFromEntry(Struct entry)
        {
            if (entry == null) return;
            string tag = entry.Speaker;
            if (string.IsNullOrEmpty(tag)) return;
            if (prefs.Actors.Find(a => string.Equals(a.tag, tag)) != null) return;
            int actorID = GetNextActorID();
            prefs.Actors.Add(new ActorSetting(actorID, TagToActorName(tag), tag));
        }

        private string TagToActorName(string tag)
        {
            return Regex.Replace(tag, "([a-z])([A-Z])", "$1 $2");
        }

        private void MarkEntryScripts(Struct structEntry, DialogueEntry dialogueEntry, string dlgName)
        {
            string actionScript = structEntry.Script;
            string questUpdate = structEntry.Quest;
            bool hasActionScript = !string.IsNullOrEmpty(actionScript);
            bool hasQuestUpdate = !string.IsNullOrEmpty(questUpdate);
            if (hasActionScript || hasQuestUpdate)
            {
                if (prefs.ScriptHandlingType == ScriptHandlingType.CustomLuaFunction)
                {
                    dialogueEntry.userScript = string.Empty;
                    if (hasActionScript)
                    {
                        dialogueEntry.userScript = string.Format("NWScript(\"{0}\")", actionScript);
                    }
                    if (hasQuestUpdate)
                    {
                        if (hasActionScript) dialogueEntry.userScript += "; ";
                        dialogueEntry.userScript += string.Format("Item[\"{0}\"].Entry_{0}_State", DialogueLua.StringToTableIndex(questUpdate), structEntry.QuestEntry);

                    }
                }
                else
                {
                    dialogueEntry.userScript = string.Format("{0}{1} {2}", "--", actionScript, questUpdate);
                }
                if (reportQuestsAndScriptsFlag)
                {
                    Debug.LogWarning(string.Format("{0}: '{1}' entry {2} {3}: {4} {5} -> {6}", DialogueDebug.Prefix, dlgName, structEntry.id, GetTextSnippet(dialogueEntry), actionScript, questUpdate, dialogueEntry.userScript));
                }
            }
        }

        private void MarkLinkCondition(Struct structEntry, Conversation conversation, int destinationDialogueID, string dlgName)
        {
            string conditionScript = structEntry.Active;
            bool hasConditionScript = !string.IsNullOrEmpty(conditionScript);
            if (hasConditionScript)
            {
                DialogueEntry dialogueEntry = conversation.GetDialogueEntry(destinationDialogueID);
                if (dialogueEntry != null)
                {
                    if (prefs.ScriptHandlingType == ScriptHandlingType.CustomLuaFunction)
                    {
                        dialogueEntry.conditionsString = string.Format("NWScript(\"{0}\")", conditionScript);
                    }
                    else
                    {
                        dialogueEntry.conditionsString = string.Format("{0}{1}", "true --", conditionScript);
                    }
                    if (reportQuestsAndScriptsFlag)
                    {
                        Debug.LogWarning(string.Format("{0}: '{1}' entry {2} {3} active condition: {4} -> {5}", DialogueDebug.Prefix, dlgName, structEntry.id, GetTextSnippet(dialogueEntry), conditionScript, dialogueEntry.conditionsString));
                    }
                }
            }
        }

        private const int MaxSnippetLength = 40;

        private string GetTextSnippet(DialogueEntry dialogueEntry)
        {
            try
            {
                string dialogueText = dialogueEntry.currentDialogueText ?? string.Empty;
                return (dialogueText.Length <= MaxSnippetLength)
                    ? string.Format("(\"{0}\")", dialogueText)
                    : string.Format("(\"{0}...\")", dialogueText.Substring(0, MaxSnippetLength));
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private void AddJournalEntriesToDatabase(DialogueDatabase database)
        {
            int itemID = 1;
            foreach (var jrlFile in prefs.JrlFiles)
            {
                if (!Tools.IsStringNullOrEmptyOrWhitespace(jrlFile.filename))
                {
                    EditorUtility.DisplayProgressBar("Importing Journal Files", Path.GetFileName(jrlFile.filename), (float)itemID / (float)prefs.JrlFiles.Count);
                    ConvertJrl(jrlFile, itemID++, database);
                }
            }
        }

        private void ConvertJrl(JrlFile jrlFile, int itemID, DialogueDatabase database)
        {
            Jrl jrl = null;
            try
            {
                jrl = Jrl.Load(FindFile(jrlFile.filename), prefs.Encoding);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to load {1}. Make sure the file is a valid journal, and check the encoding type. Error: {2}", DialogueDebug.Prefix, jrlFile.filename, e.Message));
            }
            try
            {
                if (jrl == null) return;
                foreach (var jrlStruct in jrl.Categories)
                {
                    Item item = JrlStructToItem(jrlStruct, itemID++);
                    database.items.Add(item);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("{0}: Failed to convert {1}. Error: {2}", DialogueDebug.Prefix, jrlFile.filename, e.Message));
            }
        }

        private Item JrlStructToItem(Struct jrlStruct, int itemID)
        {
            // Create the item:
            string itemName = DialogueLua.StringToTableIndex(jrlStruct.GetElementValue("Tag"));
            if (string.IsNullOrEmpty(itemName)) itemName = itemID.ToString();
            Item item = template.CreateItem(itemID, itemName);

            // Set the misc. fields:
            Field.SetValue(item.fields, "Is Item", false);
            Field.SetValue(item.fields, "XP", Tools.StringToInt(jrlStruct.GetElementValue("XP")));
            Field.SetValue(item.fields, "Priority", Tools.StringToInt(jrlStruct.GetElementValue("Priority")));
            Field.SetValue(item.fields, "Comment", jrlStruct.Comment);

            // Set the localized description:
            foreach (var localString in jrlStruct.GetLocalStrings("Name"))
            {
                if (localString != null)
                {
                    if (localString.LanguageID == DefaultLanguageID)
                    {
                        Field.SetValue(item.fields, "Description", ProcessText(localString.value));
                    }
                    else
                    {
                        string languageName = LanguageIDToLanguageName(localString.languageId);
                        string localizedDescriptionName = string.Format("Description {0}", languageName);
                        Field.SetValue(item.fields, localizedDescriptionName, ProcessText(localString.value), FieldType.Localization);
                    }
                }
            }

            // Set the entries:
            //List<Struct> entryList = jrlStruct.GetElementStructs("EntryList");
            List<Struct> entryList = jrlStruct.GetElementStructs("EntryList");
            entryList = entryList.OrderBy(entry => Tools.StringToInt(entry.GetElementValue("ID"))).ToList();

            Field.SetValue(item.fields, "Entry Count", (entryList != null) ? entryList.Count : 0);
            if (entryList != null)
            {
                for (int i = 0; i < entryList.Count; i++)
                {
                    int entryNum = i + 1;
                    int entryID = Tools.StringToInt(entryList[i].GetElementValue("ID"));
                    Field.SetValue(item.fields, string.Format("Entry {0} ID", entryNum), entryID);
                    Field.SetValue(item.fields, string.Format("Entry {0} State", entryNum), "unassigned");
                    foreach (var localString in entryList[i].GetLocalStrings("Text"))
                    {
                        if (localString != null)
                        {
                            if (localString.LanguageID == DefaultLanguageID)
                            {
                                Field.SetValue(item.fields, string.Format("Entry {0}", entryNum), ProcessText(localString.value));
                            }
                            else
                            {
                                string languageName = LanguageIDToLanguageName(localString.languageId);
                                string localizedDescriptionName = string.Format("Entry {0} {1}", entryNum, languageName);
                                Field.SetValue(item.fields, localizedDescriptionName, ProcessText(localString.value), FieldType.Localization);
                            }
                        }
                    }
                    Field.SetValue(item.fields, string.Format("Entry {0} End", entryNum), entryList[i].End);
                }
            }

            return item;
        }

    }

}
#endif
