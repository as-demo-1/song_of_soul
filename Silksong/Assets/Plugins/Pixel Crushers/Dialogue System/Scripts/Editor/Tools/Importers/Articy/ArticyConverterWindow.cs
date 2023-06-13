#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// articy:draft converter window.
    /// </summary>
    public class ArticyConverterWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/articy:draft...", false, 1)]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(ArticyConverterWindow), false, "articy Import");
        }

        public static ArticyConverterWindow Instance { get { return instance; } }

        private static ArticyConverterWindow instance = null;

        // Private fields for the window:

        private ConverterPrefs prefs = new ConverterPrefs();
        private ArticyData articyData = null;
        private string projectTitle = null;
        private string projectAuthor = null;
        private string projectVersion = null;

        private Vector2 scrollPosition = Vector2.zero;
        private bool articyAttributesFoldout = true;
        private bool articyEntitiesFoldout = false;
        private bool articyLocationsFoldout = false;
        private bool articyVariablesFoldout = false;
        private bool articyDialoguesFoldout = false;
        private bool articyDropdownOverridesFoldout = false;
        private bool articyEmVarsFoldout = false;
        private bool articyFlowFoldout = false;
        private Template template = null;
        private bool mustReloadData = false;
        private string[] articyVarNameList = null;

        private const float ToggleWidth = 16;

        public static bool IsOpen { get { return (instance != null); } }

        void OnEnable()
        {
            instance = this;
            minSize = new Vector2(320, 128);
            template = TemplateTools.LoadFromEditorPrefs();
            prefs = ConverterPrefsTools.Load();
        }

        void OnDisable()
        {
            ConverterPrefsTools.Save(prefs);
            instance = null;
        }

        /// <summary>
        /// Draws the converter window.
        /// </summary>
        void OnGUI()
        {
            EditorStyles.textField.wordWrap = true;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            DrawProjectFilenameField();
            DrawEncodingPopup();
            DrawStageDirectionsPopup();
            DrawDropdownsPopup();
            DrawSlotsPopup();
            DrawRecursionMode();
            DrawFlowFragmentMode();
            DrawOtherScriptsField();
            DrawUseTechnicalNamesToggle();
            DrawDirectConversationLinksToEntry1Toggle();
            DrawConversationsForLooseFlow();
            DrawDefaultActorsToggle();
            DrawConvertMarkupToggle();
            DrawSplitPipesToggle();
            DrawDocumentsSubmenu();
            DrawVoiceOverOptions();
            DrawPortraitFolderField();
            DrawArticyContent();
            DrawSaveToField();
            DrawOverwriteCheckbox();
            DrawConversionButtons();
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the articy:draft Project filename field.
        /// </summary>
        private void DrawProjectFilenameField()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(new GUIContent("articy:draft Project", "The XML file that you exported from articy:draft."), prefs.ProjectFilename);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.ProjectFilename = EditorUtility.OpenFilePanel("Select articy:draft Project", EditorWindowTools.GetDirectoryName(prefs.ProjectFilename), "xml");
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                ConverterPrefsTools.Save(prefs);
                articyData = null;
            }
        }

        private void DrawVoiceOverOptions()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            prefs.VoiceOverProperty = EditorGUILayout.TextField(new GUIContent("VoiceOverFile Property", "If using Nevigo's beta voice-over plugin, specify the property as defined in the plugin."), prefs.VoiceOverProperty);
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
            if (GUILayout.Button(new GUIContent("Info", "How to get the voice-over plugin."), GUILayout.Width(50))) Application.OpenURL("http://www.nevigo.com/redirect/Mdk.VoiceOverHelper.Installer");
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the Portrait Folder field.
        /// </summary>
        private void DrawPortraitFolderField()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            prefs.PortraitFolder = EditorGUILayout.TextField(new GUIContent("Portrait Folder", "The importer will look here for actor entities' portrait textures."), prefs.PortraitFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.PortraitFolder = EditorUtility.OpenFolderPanel("Location of Portrait Textures", prefs.PortraitFolder, "");
                prefs.PortraitFolder = "Assets" + prefs.PortraitFolder.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        /// <summary>
        /// Draws the Stage Dirs handling popup.
        /// </summary>
        private void DrawStageDirectionsPopup()
        {
            var mode = prefs.StageDirectionsMode;
            mode = (ConverterPrefs.StageDirModes)EditorGUILayout.EnumPopup(new GUIContent("Stage Directions are ", "Specify whether articy:draft Stage Directions contain Dialogue System sequences."), mode, GUILayout.Width(300));
            EditorGUI.BeginChangeCheck();
            prefs.StageDirectionsMode = mode;
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        /// <summary>
        /// Draws the Dropdowns handling checkbox.
        /// </summary>
        private void DrawDropdownsPopup()
        {
            var newValue = (ConverterPrefs.ConvertDropdownsModes)EditorGUILayout.EnumPopup(new GUIContent("Dropdowns as ", "Specify how to import dropdowns."), prefs.ConvertDropdownsAs, GUILayout.Width(300));
            if (newValue != prefs.ConvertDropdownsAs)
            {
                prefs.ConvertDropdownsAs = newValue;
                articyData = null;
                ConverterPrefsTools.Save(prefs);
            }
        }

        /// <summary>
        /// Draws the Slots handling checkbox.
        /// </summary>
        private void DrawSlotsPopup()
        {
            var newValue = (ConverterPrefs.ConvertSlotsModes)EditorGUILayout.EnumPopup(new GUIContent("Slots as ", "Specify how to import slots."), prefs.ConvertSlotsAs, GUILayout.Width(300));
            if (newValue != prefs.ConvertSlotsAs)
            {
                prefs.ConvertSlotsAs = newValue;
                articyData = null;
                ConverterPrefsTools.Save(prefs);
            }
        }

        /// <summary>
        /// Draws the recursion mode dropdown.
        /// </summary>
        private void DrawRecursionMode()
        {
            EditorGUI.BeginChangeCheck();
            prefs.RecursionMode = (ConverterPrefs.RecursionModes)EditorGUILayout.EnumPopup(new GUIContent("Recursion", "Specify whether to link nested conversations."), prefs.RecursionMode, GUILayout.Width(300));
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        /// <summary>
        /// Draws the flow fragments dropdown.
        /// </summary>
        private void DrawFlowFragmentMode()
        {
            EditorGUI.BeginChangeCheck();
            prefs.FlowFragmentMode = (ConverterPrefs.FlowFragmentModes)EditorGUILayout.EnumPopup(new GUIContent("FlowFragments are ", "Specify how flow fragments should be treated."), prefs.FlowFragmentMode, GUILayout.Width(300));
            if (prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.ConversationGroups || prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.NestedConversationGroups)
            {
                prefs.FlowFragmentScript = EditorGUILayout.TextField(new GUIContent("FlowFragment Script", "When a dialogue references a flow fragment, invoke this Lua function, passing the flow fragment's display name. Leave blank for no script."), prefs.FlowFragmentScript);
                if (!string.IsNullOrEmpty(prefs.FlowFragmentScript))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Note: Because FlowFragment Script is specified, you must register a custom Lua function named '" + prefs.FlowFragmentScript + "' or the Dialogue System will report errors. If you don't want to do this, clear FlowFragment Script.", MessageType.Info);
                    if (GUILayout.Button("Info", GUILayout.Width(40)))
                    {
                        Application.OpenURL("https://pixelcrushers.com/dialogue_system/manual2x/html/logic_and_lua.html#luaClassRegisterFunction");
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        private void DrawOtherScriptsField()
        {
            EditorGUI.BeginChangeCheck();
            prefs.OtherScriptFields = EditorGUILayout.TextField(new GUIContent("Other Script Fields", "(Optional) Other fields that contain articy:expresso code that should be converted to Lua. Separate fields with semicolons (;)."), prefs.OtherScriptFields);
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        private void DrawDocumentsSubmenu()
        {
            EditorGUI.BeginChangeCheck();
            prefs.ImportDocuments = EditorGUILayout.Toggle(new GUIContent("Import Documents",
                "Also import documents as conversations."), prefs.ImportDocuments);
            if (prefs.ImportDocuments)
            {
                prefs.DocumentsSubmenu = EditorGUILayout.TextField(new GUIContent("Documents Submenu", "When converting documents to conversations, group them under this submenu. Leave blank for no submenu."), prefs.DocumentsSubmenu);
            }
            prefs.TextTableDocument = EditorGUILayout.TextField(new GUIContent("TextTable Document", "Optional name of document whose text should be written to a TextTable asset."), prefs.TextTableDocument);
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        private void DrawUseTechnicalNamesToggle()
        {
            prefs.UseTechnicalNames = EditorGUILayout.Toggle(new GUIContent("Use Technical Names",
                "Name dialogue database elements by their articy technical name instead of display name."),
                prefs.UseTechnicalNames);
            if (!prefs.UseTechnicalNames)
            {
                prefs.SetDisplayName = EditorGUILayout.Toggle(new GUIContent("Set Display Name",
                    "Set Display Name field to entity's default display name."),
                    prefs.SetDisplayName);
            }
            if (!prefs.SetDisplayName)
            {
                prefs.CustomDisplayName = EditorGUILayout.Toggle(new GUIContent("Custom DisplayName",
                    "Instead of using entity's name as Display Name, use a custom field named 'DisplayName'."),
                    prefs.CustomDisplayName);
            }
        }

        private void DrawDirectConversationLinksToEntry1Toggle()
        {
            prefs.DirectConversationLinksToEntry1 = EditorGUILayout.Toggle(new GUIContent("Conv. Links to Entry 1",
                "When a link points to a conversation's START node, redirect it to entry 1 instead."),
                prefs.DirectConversationLinksToEntry1);
        }

        private void DrawConversationsForLooseFlow()
        {
            prefs.CreateConversationsForLooseFlow = EditorGUILayout.Toggle(new GUIContent("Conv. Loose Flow Frags",
                "Make conversations for flow fragments that aren't inside dialogues."),
                prefs.CreateConversationsForLooseFlow);
        }

        private void DrawDefaultActorsToggle()
        {
            prefs.UseDefaultActorsIfNoneAssignedToDialogue = EditorGUILayout.Toggle(new GUIContent("Use Default Actors If None",
                "If no actors are assigned to a dialogue, assign default 'Player' and 'NPC' actors. If unticked, leave conversation unassigned."),
                prefs.UseDefaultActorsIfNoneAssignedToDialogue);
        }

        private void DrawConvertMarkupToggle()
        {
            prefs.ConvertMarkupToRichText = EditorGUILayout.Toggle(new GUIContent("Convert Markup",
                "Convert articy markup to rich text codes that Unity can display."),
                prefs.ConvertMarkupToRichText);
        }

        private void DrawSplitPipesToggle()
        {
            prefs.SplitTextOnPipes = EditorGUILayout.Toggle(new GUIContent("Split Text On Pipes",
                "When dialogue text contains pipe characters ( | ), split into separate dialogue entry nodes."),
                prefs.ConvertMarkupToRichText);
        }

        /// <summary>
        /// Draws the encoding popup.
        /// </summary>
        private void DrawEncodingPopup()
        {
            EditorGUI.BeginChangeCheck();
            prefs.EncodingType = (EncodingType)EditorGUILayout.EnumPopup(new GUIContent("Encoding", "articy:draft exports to XML using your system's current text encoding setting. Use the same encoding value here to properly import special characters."), prefs.EncodingType, GUILayout.Width(300));
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        /// <summary>
        /// Draws the Save To field.
        /// </summary>
        private void DrawSaveToField()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            prefs.OutputFolder = EditorGUILayout.TextField("Save To", prefs.OutputFolder);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.OutputFolder = EditorUtility.SaveFolderPanel("Path to Save Database", prefs.OutputFolder, "");
                prefs.OutputFolder = "Assets" + prefs.OutputFolder.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        /// <summary>
        /// Draws the Overwrite checkbox.
        /// </summary>
        private void DrawOverwriteCheckbox()
        {
            EditorGUI.BeginChangeCheck();
            prefs.Overwrite = EditorGUILayout.Toggle("Overwrite", prefs.Overwrite);
            if (EditorGUI.EndChangeCheck()) ConverterPrefsTools.Save(prefs);
        }

        /// <summary>
        /// Draws the buttons Review, Clear, and Convert.
        /// </summary>
        private void DrawConversionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawReadXMLButton();
            DrawClearButton();
            DrawSaveDatabaseButton();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the Review button, and loads/reviews the project if clicked.
        /// </summary>
        private void DrawReadXMLButton()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prefs.ProjectFilename));
            if (GUILayout.Button(new GUIContent("Read XML", "Load the XML file so you can adjust conversion parameters such as what to include in the dialogue database and which actor is the player. Also click this button after re-exporting from articy to reload the updated XML file."), GUILayout.Width(120))) ReviewArticyProject();
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Draws the Clear button, and clears the project if clicked.
        /// </summary>
        private void DrawClearButton()
        {
            if (GUILayout.Button("Clear", GUILayout.Width(120))) ClearArticyProject();
        }

        /// <summary>
        /// Draws the Import button, and converts the project into a dialogue database if clicked.
        /// </summary>
        private void DrawSaveDatabaseButton()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prefs.ProjectFilename) || string.IsNullOrEmpty(prefs.OutputFolder));
            if (GUILayout.Button(new GUIContent("Save Database", "Convert the XML data loaded in memory into a dialogue database and save it in the project. If you change your articy project and re-export XML, click Read XML again before clicking this button."), GUILayout.Width(120)))
            {
                if (!prefs.OutputFolder.EndsWith("/")) prefs.OutputFolder += "/";
                if (!FolderExists(prefs.OutputFolder))
                {
                    EditorUtility.DisplayDialog("Invalid Output Folder", "The Output Folder is not valid. Please correct this and click Import again.", "OK");
                    return;
                }
                ConvertArticyProject();
                if (DialogueEditor.DialogueEditorWindow.instance != null)
                {
                    DialogueEditor.DialogueEditorWindow.instance.Repaint();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private bool FolderExists(string folder)
        {
            var assetsFolder = Application.dataPath + "/" + (prefs.OutputFolder.StartsWith("Assets/") ? prefs.OutputFolder.Remove(0, "Assets/".Length) : prefs.OutputFolder);
            return Directory.Exists(assetsFolder);
        }

        /// <summary>
        /// Draws the articy content for review.
        /// </summary>
        private void DrawArticyContent()
        {
            if (articyData != null)
            {
                DrawHorizontalLine();
                DrawArticyAttributes();
                DrawArticyEntities();
                DrawArticyLocations();
                if (prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.Quests)
                {
                    DrawArticyFlow();
                }
                DrawArticyVariables();
                DrawArticyDialogues();
                DrawDropdownOverrides();
                DrawEmVars();
                DrawHorizontalLine();
            }
        }

        private void DrawArticyAttributes()
        {
            articyAttributesFoldout = EditorGUILayout.Foldout(articyAttributesFoldout, "Attributes");
            if (articyAttributesFoldout)
            {
                StartIndentedSection();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Title", projectTitle);
                EditorGUILayout.TextField("Author", projectAuthor);
                EditorGUILayout.TextField("Version", projectVersion);
                EditorGUI.EndDisabledGroup();
                EndIndentedSection();
            }
        }

        private void DrawArticyEntities()
        {
            articyEntitiesFoldout = EditorGUILayout.Foldout(articyEntitiesFoldout, "Entities (Actors, Items & Quests)");
            if (articyEntitiesFoldout)
            {
                StartIndentedSection();
                EditorGUILayout.HelpBox("If an entity has a custom property named IsPlayer, IsNPC, IsItem, or IsQuest, it will override the dropdown value below.", MessageType.None);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectEntities(true);
                if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectEntities(false);
                EditorGUILayout.EndHorizontal();
                foreach (ArticyData.Entity entity in articyData.entities.Values)
                {
                    EditorGUILayout.BeginHorizontal();
                    bool needToSetCategory = !prefs.ConversionSettings.ConversionSettingExists(entity.id);
                    ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(entity.id);
                    if (needToSetCategory) conversionSetting.Category = entity.technicalName.StartsWith("Chr") ? EntityCategory.NPC : EntityCategory.Item;
                    conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
                    conversionSetting.Category = (EntityCategory)EditorGUILayout.EnumPopup(conversionSetting.Category, GUILayout.Width(64));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(entity.displayName.DefaultText);
                    EditorGUILayout.TextField(entity.technicalName);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                EndIndentedSection();
            }
        }

        private void MassSelectEntities(bool value)
        {
            foreach (ArticyData.Entity entity in articyData.entities.Values)
            {
                prefs.ConversionSettings.GetConversionSetting(entity.id).Include = value;
            }
        }

        private void DrawArticyLocations()
        {
            articyLocationsFoldout = EditorGUILayout.Foldout(articyLocationsFoldout, "Locations");
            if (articyLocationsFoldout)
            {
                StartIndentedSection();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectLocations(true);
                if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectLocations(false);
                EditorGUILayout.EndHorizontal();
                foreach (ArticyData.Location location in articyData.locations.Values)
                {
                    EditorGUILayout.BeginHorizontal();
                    ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(location.id);
                    conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField(location.displayName.DefaultText);
                    EditorGUILayout.TextField(location.technicalName);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea(location.text.DefaultText);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                EndIndentedSection();
            }
        }

        private void MassSelectLocations(bool value)
        {
            foreach (ArticyData.Location location in articyData.locations.Values)
            {
                prefs.ConversionSettings.GetConversionSetting(location.id).Include = value;
            }
        }

        private void DrawArticyVariables()
        {
            articyVariablesFoldout = EditorGUILayout.Foldout(articyVariablesFoldout, "Variables");
            if (articyVariablesFoldout)
            {
                StartIndentedSection();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectVariables(true);
                if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectVariables(false);
                EditorGUILayout.EndHorizontal();
                foreach (ArticyData.VariableSet variableSet in articyData.variableSets.Values)
                {
                    foreach (ArticyData.Variable variable in variableSet.variables)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string id = ArticyData.FullVariableName(variableSet, variable);
                        ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(id);
                        conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField(id);
                        EditorGUILayout.TextField(variable.technicalName);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EndIndentedSection();
            }
        }

        private void MassSelectVariables(bool value)
        {
            foreach (ArticyData.VariableSet variableSet in articyData.variableSets.Values)
            {
                foreach (ArticyData.Variable variable in variableSet.variables)
                {
                    string id = ArticyData.FullVariableName(variableSet, variable);
                    ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(id);
                    conversionSetting.Include = value;
                }
            }
        }

        private void DrawArticyDialogues()
        {
            articyDialoguesFoldout = EditorGUILayout.Foldout(articyDialoguesFoldout, "Dialogues");
            if (articyDialoguesFoldout)
            {
                StartIndentedSection();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectDialogues(true);
                if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectDialogues(false);
                EditorGUILayout.EndHorizontal();
                foreach (ArticyData.Dialogue dialogue in articyData.dialogues.Values)
                {
                    EditorGUILayout.BeginHorizontal();
                    ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(dialogue.id);
                    conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField(dialogue.displayName.DefaultText);
                    EditorGUILayout.TextField(dialogue.technicalName);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea(dialogue.text.DefaultText);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                EndIndentedSection();
            }
        }

        private void DrawDropdownOverrides()
        {
            articyDropdownOverridesFoldout = EditorGUILayout.Foldout(articyDropdownOverridesFoldout, "Dropdowns");
            if (articyDropdownOverridesFoldout)
            {
                StartIndentedSection();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All Ints", EditorStyles.miniButton, GUILayout.Width(72))) prefs.ConversionSettings.AllDropdownOverrides(ConversionSettings.DropdownOverrideMode.Int);
                if (GUILayout.Button("All Strings", EditorStyles.miniButton, GUILayout.Width(72))) prefs.ConversionSettings.AllDropdownOverrides(ConversionSettings.DropdownOverrideMode.Int);
                if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Width(72))) prefs.ConversionSettings.AllDropdownOverrides(ConversionSettings.DropdownOverrideMode.UseGlobalSetting);
                EditorGUILayout.EndHorizontal();
                foreach (var setting in prefs.ConversionSettings.dropdownOverrideList)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField(setting.id);
                    EditorGUI.EndDisabledGroup();
                    var newMode = (ConversionSettings.DropdownOverrideMode)EditorGUILayout.EnumPopup(setting.mode);
                    if (newMode != setting.mode)
                    {
                        setting.mode = newMode;
                        mustReloadData = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EndIndentedSection();
            }
        }

        private void DrawEmVars()
        {
            articyEmVarsFoldout = EditorGUILayout.Foldout(articyEmVarsFoldout, "Emphasis Settings");
            if (articyEmVarsFoldout)
            {
                if (prefs.emVarSet == null) prefs.emVarSet = new ArticyEmVarSet();
                if (prefs.emVarSet.emVars == null || prefs.emVarSet.emVars.Length < DialogueDatabase.NumEmphasisSettings || prefs.emVarSet.emVars[0] == null)
                {
                    prefs.emVarSet.InitializeEmVars();
                }
                StartIndentedSection();
                for (int i = 0; i < DialogueDatabase.NumEmphasisSettings; i++)
                {
                    var emName = "[em" + (i + 1) + "]";
                    var emVars = prefs.emVarSet.emVars[i];
                    if (emVars == null) continue;
                    emVars.color = DrawVarPopup(emName + " Color", emVars.color);
                    emVars.bold = DrawVarPopup(emName + " Bold", emVars.bold);
                    emVars.italic = DrawVarPopup(emName + " Italic", emVars.italic);
                    emVars.underline = DrawVarPopup(emName + " Underline", emVars.underline);
                }
                EndIndentedSection();
            }
        }

        private string DrawVarPopup(string label, string currentVar)
        {
            if (articyVarNameList == null) BuildArticyVarNameList();
            var index = EditorGUILayout.Popup(label, GetVarNameListIndex(currentVar), articyVarNameList);
            if (0 <= index && index < articyVarNameList.Length)
            {
                currentVar = articyVarNameList[index];
            }
            return currentVar;
        }

        private void BuildArticyVarNameList()
        {
            var list = new List<string>();
            foreach (ArticyData.VariableSet variableSet in articyData.variableSets.Values)
            {
                foreach (ArticyData.Variable variable in variableSet.variables)
                {
                    list.Add(ArticyData.FullVariableName(variableSet, variable));
                }
            }
            articyVarNameList = list.ToArray();
        }

        private int GetVarNameListIndex(string varName)
        {
            if (articyVarNameList == null || string.IsNullOrEmpty(varName)) return -1;
            for (int i = 0; i < articyVarNameList.Length; i++)
            {
                if (string.Equals(articyVarNameList[i], varName)) return i;
            }
            return -1;
        }

        private void MassSelectDialogues(bool value)
        {
            foreach (ArticyData.Dialogue dialogue in articyData.dialogues.Values)
            {
                prefs.ConversionSettings.GetConversionSetting(dialogue.id).Include = value;
            }
        }

        private void DrawArticyFlow()
        {
            articyFlowFoldout = EditorGUILayout.Foldout(articyFlowFoldout, "Flow (Quests)");
            if (articyFlowFoldout)
            {
                StartIndentedSection();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectFlowFragments(true);
                if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(40))) MassSelectFlowFragments(false);
                EditorGUILayout.EndHorizontal();
                foreach (ArticyData.FlowFragment flowFragment in articyData.flowFragments.Values)
                {
                    EditorGUILayout.BeginHorizontal();
                    ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(flowFragment.id);
                    conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField(flowFragment.displayName.DefaultText);
                    EditorGUILayout.TextField(flowFragment.technicalName);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea(flowFragment.text.DefaultText);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                EndIndentedSection();
            }
        }

        private void MassSelectFlowFragments(bool value)
        {
            foreach (ArticyData.FlowFragment flowFragment in articyData.flowFragments.Values)
            {
                prefs.ConversionSettings.GetConversionSetting(flowFragment.id).Include = value;
            }
        }

        private void DrawHorizontalLine()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        }

        private void StartIndentedSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
            EditorGUILayout.BeginVertical();
        }

        private void EndIndentedSection()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public void ReviewArticyProject()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Loading articy:draft project", "Please wait...", 0);
                articyData = ArticySchemaEditorTools.LoadArticyDataFromXmlFile(prefs.ProjectFilename, prefs.Encoding, prefs.ConvertDropdownsAs, prefs);
                articyVarNameList = null;
                mustReloadData = false;
                if (articyData != null)
                {
                    projectTitle = articyData.ProjectTitle;
                    projectVersion = articyData.ProjectVersion;
                    projectAuthor = articyData.ProjectAuthor;
                    prefs.ReviewSpecialProperties(articyData);
                    Debug.Log(string.Format("{0}: Loaded {1}", DialogueDebug.Prefix, prefs.ProjectFilename));
                }
                else
                {
                    Debug.LogError(string.Format("{0}: Failed to load {1}", DialogueDebug.Prefix, prefs.ProjectFilename));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ClearArticyProject()
        {
            articyData = null;
            prefs.ConversionSettings.Clear();
            ConverterPrefsTools.DeleteEditorPrefs();
            prefs = new ConverterPrefs();
        }

        /// <summary>
        /// Converts the articy project.
        /// </summary>
        public void ConvertArticyProject()
        {
            if (articyData == null || mustReloadData) ReviewArticyProject();
            if (articyData != null) try
                {
                    ArticyConverter.onProgressCallback += OnProgressCallback;
                    string assetName = articyData.ProjectTitle.Replace(':', '_');
                    DialogueDatabase database = LoadOrCreateDatabase(assetName);
                    if (database == null)
                    {
                        Debug.LogError(string.Format("{0}: Couldn't create asset '{1}'.", DialogueDebug.Prefix, assetName));
                    }
                    else
                    {
                        ArticyConverter.ConvertArticyDataToDatabase(articyData, prefs, template, database);
                        ArticyEditorTools.FindPortraitTexturesInAssetDatabase(articyData, prefs.PortraitFolder, database);
                        EditorUtility.SetDirty(database);
                        ConvertTextTable(assetName);
                        AssetDatabase.SaveAssets();
                        Debug.Log(string.Format("{0}: Created database '{1}' containing {2} actors, {3} conversations, {4} items/quests, {5} variables, and {6} locations.",
                            DialogueDebug.Prefix, assetName, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count), database);
                    }
                }
                finally
                {
                    ArticyConverter.onProgressCallback -= OnProgressCallback;
                    EditorUtility.ClearProgressBar();
                    if (DialogueEditor.DialogueEditorWindow.instance != null) DialogueEditor.DialogueEditorWindow.instance.Reset();
                }
        }

        private void OnProgressCallback(string info, float progress)
        {
            EditorUtility.DisplayProgressBar("Importing articy:draft project", info, progress);
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
        private DialogueDatabase LoadOrCreateDatabase(string filename)
        {
            var assetPath = prefs.OutputFolder;
            if (!assetPath.EndsWith("/")) assetPath += "/";
            assetPath += filename + ".asset";
            DialogueDatabase database = null;
            if (prefs.Overwrite)
            {
                database = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueDatabase)) as DialogueDatabase;
                if (database != null) database.Clear();
            }
            if (database == null)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", prefs.OutputFolder, filename));
                database = DatabaseUtility.CreateDialogueDatabaseInstance();
                AssetDatabase.CreateAsset(database, assetPath);
            }
            return database;
        }

        /// <summary>
        /// If a TextTable Document has been specified, convert the Document to a TextTable.
        /// </summary>
        private void ConvertTextTable(string databaseFilename)
        {
            if (articyData == null || articyData.textTableFields.Count == 0 || string.IsNullOrEmpty(prefs.TextTableDocument)) return;
            var assetPath = prefs.OutputFolder;
            if (!assetPath.EndsWith("/")) assetPath += "/";
            var filename = databaseFilename + "_TextTable";
            assetPath += filename + ".asset";
            TextTable textTable = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextTable)) as TextTable;
            if (textTable != null && prefs.Overwrite)
            {
                textTable.languages.Clear();
                textTable.fields.Clear();
            }
            if (textTable == null)
            {
                textTable = AssetUtility.CreateAssetWithFilename<TextTable>(assetPath, false);
            }
            if (textTable != null)
            {
                articyData.textTableFields.ForEach(field => textTable.AddField(field));
            }
        }

    }

}
#endif