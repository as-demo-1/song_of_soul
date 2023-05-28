// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Custom inspector editor for DialogueSystemController (e.g., Dialogue Manager).
    /// </summary>
    [CustomEditor(typeof(DialogueSystemController), true)]
    public class DialogueSystemControllerEditor : Editor
    {

        private const string LightSkinIconFilename = "Dialogue System/DialogueManager Inspector Light.png";
        private const string DarkSkinIconFilename = "Dialogue System/DialogueManager Inspector Dark.png";
        private const string DefaultLightSkinIconFilepath = "Assets/Dialogue System/DLLs/DialogueManager Inspector Light.png";
        private const string DefaultDarkSkinIconFilepath = "Assets/Dialogue System/DLLs/DialogueManager Inspector Dark.png";
        private const string IconFilepathEditorPrefsKey = "PixelCrushers.DialogueSystem.IconFilename";
        private const string InspectorEditorPrefsKey = "PixelCrushers.DialogueSystem.DialogueSystemControllerPrefs";

        private static Texture2D icon = null;
        private static GUIStyle iconButtonStyle = null;
        private static GUIContent iconButtonContent = null;

        [Serializable]
        public class Foldouts
        {
            public bool displaySettingsFoldout = true;
            public bool localizationSettingsFoldout = false;
            public bool subtitleSettingsFoldout = false;
            public bool cameraSettingsFoldout = false;
            public bool inputSettingsFoldout = false;
            public bool barkSettingsFoldout = false;
            public bool alertSettingsFoldout = false;
            public bool persistentDataSettingsFoldout = false;
            public bool otherSettingsFoldout = false;
        }

        private Foldouts foldouts = null;
        private DialogueSystemController dialogueSystemController = null;
        private SerializedProperty displaySettingsProperty = null;
        private SerializedProperty persistentDataSettingsProperty = null;
        private bool createDatabase = false;

        public static Texture2D FindIcon()
        {
            var iconPath = EditorGUIUtility.isProSkin ? DarkSkinIconFilename : LightSkinIconFilename;
            return EditorGUIUtility.Load(iconPath) as Texture2D;
        }

        private void OnEnable()
        {
            dialogueSystemController = target as DialogueSystemController;
            displaySettingsProperty = serializedObject.FindProperty("displaySettings");
            persistentDataSettingsProperty = serializedObject.FindProperty("persistentDataSettings");
            foldouts = EditorPrefs.HasKey(InspectorEditorPrefsKey) ? JsonUtility.FromJson<Foldouts>(EditorPrefs.GetString(InspectorEditorPrefsKey)) : new Foldouts();
            if (foldouts == null) foldouts = new Foldouts();
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(InspectorEditorPrefsKey, JsonUtility.ToJson(foldouts));
        }

        /// <summary>
        /// Draws the inspector GUI. Adds a Dialogue System banner.
        /// </summary>
        public override void OnInspectorGUI()
        {
            createDatabase = false;
            DrawExtraFeatures();
            var originalDialogueUI = GetCurrentDialogueUI();
            serializedObject.Update();
            DrawDatabaseField();
            DrawDisplaySettings();
            DrawPersistentDataSettings();
            DrawOtherSettings();
            serializedObject.ApplyModifiedProperties();
            var newDialogueUI = GetCurrentDialogueUI();
            if (newDialogueUI != originalDialogueUI) CheckDialogueUI(newDialogueUI);
            if (createDatabase) CreateInitialDatabase();
        }

        private GameObject GetCurrentDialogueUI()
        {
            if (dialogueSystemController == null || dialogueSystemController.displaySettings == null) return null;
            return dialogueSystemController.displaySettings.dialogueUI;
        }

        private void CheckDialogueUI(GameObject newDialogueUIObject)
        {
            if (dialogueSystemController.displaySettings.dialogueUI == null) return;
            var newUIs = dialogueSystemController.displaySettings.dialogueUI.GetComponentsInChildren<CanvasDialogueUI>(true);
            if (newUIs.Length > 0)
            {
                DialogueManagerWizard.HandleCanvasDialogueUI(newUIs[0], DialogueManager.instance);
            }
        }

        private void DrawExtraFeatures()
        {
            if (icon == null) icon = FindIcon();
            if (dialogueSystemController == null || icon == null) return;
            if (iconButtonStyle == null)
            {
                iconButtonStyle = new GUIStyle(EditorStyles.label);
                iconButtonStyle.normal.background = icon;
                iconButtonStyle.active.background = icon;
            }
            if (iconButtonContent == null)
            {
                iconButtonContent = new GUIContent(string.Empty, "Click to open Dialogue Editor.");
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(iconButtonContent, iconButtonStyle, GUILayout.Width(icon.width), GUILayout.Height(icon.height)))
            {
                Selection.activeObject = dialogueSystemController.initialDatabase;
                PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Wizard...", GUILayout.Width(64)))
            {
                DialogueManagerWizard.Init();
            }
            GUILayout.EndHorizontal();
            EditorWindowTools.DrawHorizontalLine();
        }

        private void DrawDatabaseField()
        {
            EditorGUILayout.BeginHorizontal();
            var initialDatabaseProperty = serializedObject.FindProperty("initialDatabase");
            EditorGUILayout.PropertyField(initialDatabaseProperty, true);
            if (initialDatabaseProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    createDatabase = true; // Must delay until after drawing GUI to not interrupt GUI layout.
                }
            }
            else
            {
                if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(36)))
                {
                    Selection.activeObject = dialogueSystemController.initialDatabase;
                    PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateInitialDatabase()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dialogue Database", "Dialogue Database", "asset", "Save dialogue database asset as", "Assets");
            if (string.IsNullOrEmpty(path)) return;
            var database = DialogueSystemMenuItems.CreateDialogueDatabaseInstance();
            serializedObject.Update();
            serializedObject.FindProperty("initialDatabase").objectReferenceValue = database;
            serializedObject.ApplyModifiedProperties();
            if (AssetDatabase.LoadAssetAtPath<DialogueDatabase>(path)) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = database;
            PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
        }

        private void DrawDisplaySettings()
        {
            foldouts.displaySettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Display Settings", "Settings related to display and input.", foldouts.displaySettingsFoldout);
            if (foldouts.displaySettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUILayout.PropertyField(displaySettingsProperty.FindPropertyRelative("dialogueUI"), true);
                    EditorGUILayout.PropertyField(displaySettingsProperty.FindPropertyRelative("defaultCanvas"), true);
                    DrawLocalizationSettings();
                    DrawSubtitleSettings();
                    DrawCameraSettings();
                    DrawInputSettings();
                    DrawBarkSettings();
                    DrawAlertSettings();
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawLocalizationSettings()
        {
            foldouts.localizationSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Localization Settings", "Language localization settings.", foldouts.localizationSettingsFoldout, false);
            if (foldouts.localizationSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var localizationSettings = displaySettingsProperty.FindPropertyRelative("localizationSettings");
                    EditorGUILayout.PropertyField(localizationSettings.FindPropertyRelative("language"), true);
                    EditorGUILayout.PropertyField(localizationSettings.FindPropertyRelative("useSystemLanguage"), true);
                    EditorGUILayout.PropertyField(localizationSettings.FindPropertyRelative("textTable"), true);
                    EditorGUILayout.HelpBox("To use more than one Text Table, add a UILocalizationManager component to the Dialogue Manager and assign them there.", MessageType.None);
                    EditorGUILayout.HelpBox("Play mode not using the language you specified here? Click Reset Language PlayerPrefs.", MessageType.None);
                    if (GUILayout.Button(new GUIContent("Reset Language PlayerPrefs", "Delete the language selection saved in PlayerPrefs.")))
                    {
                        ResetLanguagePlayerPrefs();
                    }

                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void ResetLanguagePlayerPrefs()
        {
            var uiLocalizationManager = dialogueSystemController.GetComponent<UILocalizationManager>();
            if (uiLocalizationManager != null)
            {
                PlayerPrefs.DeleteKey(uiLocalizationManager.currentLanguagePlayerPrefsKey);
            }
            else
            {
                PlayerPrefs.DeleteKey("Language");
            }
        }

        private void DrawSubtitleSettings()
        {
            foldouts.subtitleSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Subtitle Settings", "Subtitle settings.", foldouts.subtitleSettingsFoldout, false);
            if (foldouts.subtitleSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var subtitleSettings = displaySettingsProperty.FindPropertyRelative("subtitleSettings");
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("richTextEmphases"), new GUIContent("Use Rich Text For [em#] Tags", "Use rich text codes for [em#] markup tags. If unticked, [em#] tag will apply color to entire text."), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("showNPCSubtitlesDuringLine"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("showNPCSubtitlesWithResponses"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("showPCSubtitlesDuringLine"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("allowPCSubtitleReminders"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("skipPCSubtitleAfterResponseMenu"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("subtitleCharsPerSecond"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("minSubtitleSeconds"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("continueButton"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("requireContinueOnLastLine"), true);
                    EditorGUILayout.PropertyField(subtitleSettings.FindPropertyRelative("informSequenceStartAndEnd"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawCameraSettings()
        {
            foldouts.cameraSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Camera & Cutscene Settings", "Camera and cutscene sequencer settings.", foldouts.cameraSettingsFoldout, false);
            if (foldouts.cameraSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var cameraSettings = displaySettingsProperty.FindPropertyRelative("cameraSettings");
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("sequencerCamera"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("alternateCameraObject"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("cameraAngles"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("keepCameraPositionAtConversationEnd"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("defaultSequence"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("defaultPlayerSequence"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("defaultResponseMenuSequence"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("entrytagFormat"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("reportMissingAudioFiles"), true);
                    EditorGUILayout.PropertyField(cameraSettings.FindPropertyRelative("disableInternalSequencerCommands"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawInputSettings()
        {
            foldouts.inputSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Input Settings", "Input and response menu settings.", foldouts.inputSettingsFoldout, false);
            if (foldouts.inputSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var inputSettings = displaySettingsProperty.FindPropertyRelative("inputSettings");
                    EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("alwaysForceResponseMenu"), true);
                    EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("includeInvalidEntries"), true);
                    EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("responseTimeout"), true);
                    EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("responseTimeoutAction"), true);
                    EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("emTagForOldResponses"), new GUIContent("[em#] Tag For Old Responses", "The [em#] tag to wrap around responses that have been previously chosen."), true);
                    EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("emTagForInvalidResponses"), new GUIContent("[em#] Tag For Invalid Responses", "The [em#] tag to wrap around invalid responses. These responses are only shown if Include Invalid Entries is ticked."), true);
                    try
                    {
                        EditorWindowTools.StartIndentedSection();
                        EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("qteButtons"), new GUIContent("QTE Input Buttons", "Input buttons mapped to QTEs."), true);
                        EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("cancel"), new GUIContent("Cancel Subtitle Input", "Key or button that cancels subtitle sequences."), true);
                        EditorGUILayout.PropertyField(inputSettings.FindPropertyRelative("cancelConversation"), new GUIContent("Cancel Conversation Input", "Key or button that cancels active conversation while in response menu."), true);
                    }
                    finally
                    {
                        EditorWindowTools.EndIndentedSection();
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawBarkSettings()
        {
            foldouts.barkSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Bark Settings", "Bark settings.", foldouts.barkSettingsFoldout, false);
            if (foldouts.barkSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var barkSettings = displaySettingsProperty.FindPropertyRelative("barkSettings");
                    EditorGUILayout.PropertyField(barkSettings.FindPropertyRelative("allowBarksDuringConversations"), true);
                    EditorGUILayout.PropertyField(barkSettings.FindPropertyRelative("barkCharsPerSecond"), true);
                    EditorGUILayout.PropertyField(barkSettings.FindPropertyRelative("minBarkSeconds"), true);
                    EditorGUILayout.PropertyField(barkSettings.FindPropertyRelative("defaultBarkSequence"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawAlertSettings()
        {
            foldouts.alertSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Alert Settings", "Alert settings.", foldouts.alertSettingsFoldout, false);
            if (foldouts.alertSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var alertSettings = displaySettingsProperty.FindPropertyRelative("alertSettings");
                    EditorGUILayout.PropertyField(alertSettings.FindPropertyRelative("allowAlertsDuringConversations"), true);
                    EditorGUILayout.PropertyField(alertSettings.FindPropertyRelative("alertCheckFrequency"), true);
                    EditorGUILayout.PropertyField(alertSettings.FindPropertyRelative("alertCharsPerSecond"), true);
                    EditorGUILayout.PropertyField(alertSettings.FindPropertyRelative("minAlertSeconds"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawPersistentDataSettings()
        {
            foldouts.persistentDataSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Persistent Data Settings", "Settings used by the save subsystem.", foldouts.persistentDataSettingsFoldout);
            if (foldouts.persistentDataSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("recordPersistentDataOn"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("includeStatusAndRelationshipData"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("includeActorData"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("includeAllItemData"), new GUIContent("Include All Item & Quest Data"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("includeLocationData"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("includeAllConversationFields"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("saveConversationSimStatusWithField"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("saveDialogueEntrySimStatusWithField"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("asyncGameObjectBatchSize"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("asyncDialogueEntryBatchSize"), true);
                    EditorGUILayout.PropertyField(persistentDataSettingsProperty.FindPropertyRelative("initializeNewVariables"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void DrawOtherSettings()
        {
            foldouts.otherSettingsFoldout = EditorWindowTools.EditorGUILayoutFoldout("Other Settings", "Miscellaneous settings.", foldouts.otherSettingsFoldout);
            if (foldouts.otherSettingsFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    var dontDestroyOnLoadProperty = serializedObject.FindProperty("dontDestroyOnLoad");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allowOnlyOneInstance"), true);
                    var previousDontDestroyOnLoadValue = dontDestroyOnLoadProperty.boolValue;
                    EditorGUILayout.PropertyField(dontDestroyOnLoadProperty, true);
                    if (previousDontDestroyOnLoadValue == true && dontDestroyOnLoadProperty.boolValue == false)
                    {
                        if (!VerifyDisableDontDestroyOnLoad())
                        {
                            dontDestroyOnLoadProperty.boolValue = true;
                        }
                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("preloadResources"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("warmUpConversationController"), true);
                    if (serializedObject.FindProperty("warmUpConversationController").enumValueIndex != (int)DialogueSystemController.WarmUpMode.Off)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dontHideImmediateDuringWarmup"), true);
                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("instantiateDatabase"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("includeSimStatus"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allowSimultaneousConversations"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onStartTriggerWaitForSaveDataApplied"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueTimeMode"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("debugLevel"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private bool VerifyDisableDontDestroyOnLoad()
        {
            var controller = target as DialogueSystemController;
            if (controller == null) return true;
            var inputDeviceManager = controller.GetComponent<InputDeviceManager>();
            var dontDestroyInputDeviceManager = (inputDeviceManager != null && inputDeviceManager.singleton);
            var saveSystem = controller.GetComponent<SaveSystem>();
            if (saveSystem != null && dontDestroyInputDeviceManager)
            {
                EditorUtility.DisplayDialog("Can't Untick Don't Destroy On Load", "The Dialogue Manager has a Save System component, which always sets the GameObject to Don't Destroy On Load. Before unticking this checkbox, move the Save System to a different GameObject. Also untick the Input Device Manager's Singleton checkbox, which also sets Don't Destroy On Load.", "OK");
                return false;
            }
            else if (saveSystem != null)
            {
                EditorUtility.DisplayDialog("Can't Untick Don't Destroy On Load", "The Dialogue Manager has a Save System component, which always sets the GameObject to Don't Destroy On Load. Before unticking this checkbox, move the Save System to a different GameObject.", "OK");
                return false;
            }
            else if (dontDestroyInputDeviceManager)
            {
                if (EditorUtility.DisplayDialog("Unticking Don't Destroy On Load", "The Input Device Manager's Singleton checkbox is ticked, which will also set this GameObject to Don't Destroy On Load. Untick its Singleton checkbox too?", "Yes", "No"))
                {
                    Undo.RecordObject(inputDeviceManager, "Untick Singleton");
                    inputDeviceManager.singleton = false;
                }
                return true;
            }
            return true;
        }

    }

}
