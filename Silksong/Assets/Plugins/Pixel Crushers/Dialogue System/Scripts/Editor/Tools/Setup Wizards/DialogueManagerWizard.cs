// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This wizard sets up the Dialogue Manager object.
    /// </summary>
    public class DialogueManagerWizard : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Wizards/Dialogue Manager Wizard", false, 1)]
        public static void Init()
        {
            (EditorWindow.GetWindow(typeof(DialogueManagerWizard), false, "Dialogue Mgr") as DialogueManagerWizard).minSize = new Vector2(720, 500);
        }

        // Private fields for the window:

        private enum Stage
        {
            Database,
            UI,
            Localization,
            Subtitles,
            Cutscenes,
            Inputs,
            Alerts,
            Saving,
            Review
        };

        private Stage stage = Stage.Database;

        private string[] stageLabels = new string[] { "Database", "UI", "Localization", "Subtitles", "Cutscenes", "Input", "Alerts", "Saving", "Review" };

        private bool createNewDatabase = false;

        /// <summary>
        /// Draws the window.
        /// </summary>
        void OnGUI()
        {
            try
            {
                DrawProgressIndicator();
                DrawCurrentStage();
                CheckCreateNewDatabase();
            }
            catch (System.Exception e)
            {
                if (!(e is ExitGUIException))
                {
                    Debug.LogWarning("Error updating Dialogue Manager Setup Wizard window: " + e.Message);
                }
            }
        }

        private void CheckCreateNewDatabase()
        {
            if (createNewDatabase)
            {
                createNewDatabase = false;
                DialogueManager.instance.initialDatabase = DialogueSystemMenuItems.CreateDialogueDatabaseInstance();
                DialogueSystemMenuItems.CreateAsset(DialogueManager.instance.initialDatabase, "Dialogue Database");
            }
        }

        private void DrawProgressIndicator()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Toolbar((int)stage, stageLabels, GUILayout.Width(720));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorWindowTools.DrawHorizontalLine();
        }

        private void DrawNavigationButtons(bool backEnabled, bool nextEnabled, bool nextCloses)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                this.Close();
            }
            else if (backEnabled && GUILayout.Button("Back", GUILayout.Width(100)))
            {
                stage--;
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!nextEnabled);
                if (GUILayout.Button(nextCloses ? "Finish" : "Next", GUILayout.Width(100)))
                {
                    if (nextCloses)
                    {
                        Close();
                    }
                    else
                    {
                        stage++;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Height(2));
        }

        private void DrawCurrentStage()
        {
            switch (stage)
            {
                case Stage.Database: DrawDatabaseStage(); break;
                case Stage.UI: DrawUIStage(); break;
                case Stage.Localization: DrawLocalizationStage(); break;
                case Stage.Subtitles: DrawSubtitlesStage(); break;
                case Stage.Cutscenes: DrawCutscenesStage(); break;
                case Stage.Inputs: DrawInputsStage(); break;
                case Stage.Alerts: DrawAlertsStage(); break;
                case Stage.Saving: DrawSavingStage(); break;
                case Stage.Review: DrawReviewStage(); break;
            }
            if (GUI.changed) EditorUtility.SetDirty(DialogueManager.instance);
        }

        private void DrawDatabaseStage()
        {
            if (DialogueManager.instance == null)
            {
                EditorGUILayout.LabelField("Dialogue Manager", EditorStyles.boldLabel);
                EditorWindowTools.StartIndentedSection();
                EditorGUILayout.HelpBox("This wizard will help you configure the Dialogue Manager GameObject.\n\nThe Dialogue Manager coordinates all Dialogue System activity. It should be in the first scene that uses any Dialogue System functionality. By default, it's configured to persist across scene changes, replacing any Dialogue Managers in the newly-loaded scene. This allows you to retain your runtime dialogue information as you change scenes.\n\n" +
                    "This scene doesn't have a Dialogue Manager. The recommended way to add a Dialogue Manager is to add the prefab located in " +
                    "Plugins / Pixel Crushers / Dialogue System / Prefabs / Dialogue Manager.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Add Prefab", "Add Dialogue Manager starter prefab to scene."), GUILayout.Width(100)))
                {
                    var prefab = AssetDatabase.LoadMainAssetAtPath("Assets/Plugins/Pixel Crushers/Dialogue System/Prefabs/Dialogue Manager.prefab");
                    if (prefab == null)
                    {
                        EditorUtility.DisplayDialog("Prefab Not Found", "This wizard can't find the Dialogue Manager prefab. You may have moved the Dialogue System to a different folder in your project. You'll need to add the prefab manually.", "OK");
                    }
                    else
                    {
                        PrefabUtility.InstantiatePrefab(prefab);
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                    }
                }
                //--- Discourage blank. Use prefab:
                //if (GUILayout.Button(new GUIContent("Create Blank", "Create a blank Dialogue Manager that you'll need to configure from scratch."), GUILayout.Width(100)))
                //{
                //    new GameObject("Dialogue Manager").AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueSystemController)));
                //}
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Dialogue Database", EditorStyles.boldLabel);
                EditorWindowTools.StartIndentedSection();
                EditorGUILayout.HelpBox("This wizard is optional. It can help you set up the Dialogue Manager GameObject, which is the GameObject that coordinates Dialogue System activity. If you prefer, you can configure the Dialogue Manager through the Inspector view instead of using the wizard.\n\nThe first step is to assign a dialogue database asset. This asset contains your conversations and related data.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                DialogueManager.instance.initialDatabase = EditorGUILayout.ObjectField("Database", DialogueManager.instance.initialDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
                bool disabled = (DialogueManager.instance.initialDatabase != null);
                EditorGUI.BeginDisabledGroup(disabled);
                if (GUILayout.Button("Create New", GUILayout.Width(100))) createNewDatabase = true;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorWindowTools.EndIndentedSection();
            if (DialogueManager.instance != null)
            {
                DrawNavigationButtons(false, (DialogueManager.instance.initialDatabase != null), false);
            }
        }

        private void DrawUIStage()
        {
            EditorGUILayout.LabelField("User Interface", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The Dialogue Manager uses a dialogue UI to display conversations and alert messages. Assign a dialogue UI. You can find pre-built UIs in Plugins/Pixel Crushers/Dialogue System/Prefabs/Standard UI Prefabs. You can also assign a UI scene object.", MessageType.Info);
            if (DialogueManager.instance.displaySettings == null) DialogueManager.instance.displaySettings = new DisplaySettings();
            EditorGUI.BeginChangeCheck();
            DialogueManager.instance.displaySettings.dialogueUI = EditorGUILayout.ObjectField("Dialogue UI", DialogueManager.instance.displaySettings.dialogueUI, typeof(GameObject), true) as GameObject;
            var assignedNewUI = EditorGUI.EndChangeCheck();
            if (DialogueManager.instance.displaySettings.dialogueUI == null)
            {
                EditorGUILayout.HelpBox("If you continue without assigning a UI, the Dialogue System will use a very plain default UI.", MessageType.Warning);
            }
            else
            {
                var legacy = DialogueManager.instance.displaySettings.dialogueUI.GetComponentsInChildren<PixelCrushers.DialogueSystem.UnityGUI.UnityDialogueUI>(true);
                if (legacy.Length > 0)
                {
                    EditorGUILayout.HelpBox("This is a Legacy Unity GUI dialogue UI.", MessageType.None);
                }
                else
                {
                    var newUIs = DialogueManager.instance.displaySettings.dialogueUI.GetComponentsInChildren<CanvasDialogueUI>(true);
                    if (newUIs.Length > 0)
                    {
                        EditorGUILayout.HelpBox("This is a " + newUIs[0].GetType().Name + ".", MessageType.None);
                        if (assignedNewUI)
                        {
                            HandleCanvasDialogueUI(newUIs[0], DialogueManager.instance);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Make sure this GameObject has an active dialogue UI component.", MessageType.None);
                    }
                }
            }
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        public static void HandleCanvasDialogueUI(CanvasDialogueUI ui, DialogueSystemController dialogueManager)
        {
            if (ui == null) return;
            if (Tools.IsPrefab(ui.gameObject) && (ui.GetComponentInParent<Canvas>() == null))
            {
                if (EditorUtility.DisplayDialog("Canvas-Based Dialogue UI", "Would you like to add an instance of this dialogue UI prefab as a child of the Dialogue Manager?",
                    "Add Instance", "Use Prefab"))
                {
                    SetupCanvas(ui, dialogueManager);
                }
                else
                {
                    //--- Allow prefabs now: dialogueManager.displaySettings.dialogueUI = null; 
                }
            }
        }

        private static void SetupCanvas(CanvasDialogueUI ui, DialogueSystemController dialogueManager)
        {
            if (GameObjectUtility.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
            var canvas = dialogueManager.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("Canvas");
                canvasObject.layer = 5; // Built-in UI layer.
                canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.transform.SetParent(dialogueManager.transform);
            }
            var uiInstance = Instantiate(ui) as CanvasDialogueUI;
            uiInstance.name = uiInstance.name.Replace("(Clone)", string.Empty);
            uiInstance.transform.SetParent(canvas.transform, false);
            dialogueManager.displaySettings.dialogueUI = uiInstance.gameObject;
        }

        private void DrawLocalizationStage()
        {
            if (DialogueManager.instance.displaySettings.localizationSettings == null) DialogueManager.instance.displaySettings.localizationSettings = new DisplaySettings.LocalizationSettings();
            EditorGUILayout.LabelField("Localization Settings", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The Dialogue System supports language localization. If you don't want to set up localization right now, you can just click Next.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.localizationSettings.useSystemLanguage = EditorGUILayout.Toggle("Use System Language", DialogueManager.instance.displaySettings.localizationSettings.useSystemLanguage);
            EditorGUILayout.HelpBox("Tick to set the game to the current system language when starting.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            if (!DialogueManager.instance.displaySettings.localizationSettings.useSystemLanguage)
            {
                EditorGUILayout.BeginHorizontal();
                DialogueManager.instance.displaySettings.localizationSettings.language = EditorGUILayout.TextField("Starting Language", DialogueManager.instance.displaySettings.localizationSettings.language);
                EditorGUILayout.HelpBox("Use the language defined by a language code (e.g., 'ES-es'). Leave blank to use the default language/no localization.", MessageType.None);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            var localizationManager = DialogueManager.instance.GetComponentInChildren<UILocalizationManager>();
            var hasLocalizationManager = (localizationManager != null);
            var wantsLocalizationManager = EditorGUILayout.Toggle("UI Localization Manager", hasLocalizationManager);
            EditorGUILayout.HelpBox("The UI Localization Manager component records the current language setting in PlayerPrefs and automatically updates any UI elements that have LocalizeUI components.", MessageType.None);
            if (!wantsLocalizationManager && hasLocalizationManager)
            {
                DestroyImmediate(localizationManager);
            }
            else if (wantsLocalizationManager && !hasLocalizationManager)
            {
                DialogueManager.instance.gameObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.UILocalizationManager)));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.localizationSettings.textTable = (TextTable)EditorGUILayout.ObjectField("Text Table", DialogueManager.instance.displaySettings.localizationSettings.textTable, typeof(TextTable), false, GUILayout.Width(360));
            EditorGUILayout.HelpBox("Assign an optional Text Table asset containing translations to use for UI elements outside of your dialogue database content.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawSubtitlesStage()
        {
            if (DialogueManager.instance.displaySettings.subtitleSettings == null) DialogueManager.instance.displaySettings.subtitleSettings = new DisplaySettings.SubtitleSettings();
            EditorGUILayout.LabelField("Subtitle Settings", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("In this section, you'll specify how subtitles are displayed.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine = EditorGUILayout.Toggle("NPC Subtitles During Line", DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine);
            EditorGUILayout.HelpBox("Tick to display NPC subtitles while NPCs are speaking. Subtitles are the lines of dialogue defined in your dialogue database asset. You might untick this if, for example, you use lip-synced voiceovers without onscreen text.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses = EditorGUILayout.Toggle("     With Response Menu", DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses);
            EditorGUILayout.HelpBox("Tick to display the last NPC subtitle during the player response menu. This helps remind the players what they're responding to.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine = EditorGUILayout.Toggle("PC Subtitles", DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine);
            EditorGUILayout.HelpBox("Tick to display PC subtitles while the PC is speaking. If you use different Menu Text and Dialogue Text, this should probably be ticked. Otherwise you may choose to leave it unticked.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            if (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine)
            {
                EditorGUILayout.BeginHorizontal();
                DialogueManager.instance.displaySettings.subtitleSettings.skipPCSubtitleAfterResponseMenu = EditorGUILayout.Toggle("     Skip After Menu", DialogueManager.instance.displaySettings.subtitleSettings.skipPCSubtitleAfterResponseMenu);
                EditorGUILayout.HelpBox("Tick to skip showing PC subtitles that come from response menu selections.", MessageType.None);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.subtitleCharsPerSecond = EditorGUILayout.FloatField("Chars/Second", DialogueManager.instance.displaySettings.subtitleSettings.subtitleCharsPerSecond);
            EditorGUILayout.HelpBox("Determines how long the subtitle is displayed before moving to the next stage of the conversation. (Technically determines the duration of the {{end}} keyword.) If the subtitle is 90 characters and Chars/Second is 30, then it will be displayed for 3 seconds.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.minSubtitleSeconds = EditorGUILayout.FloatField("Min Seconds", DialogueManager.instance.displaySettings.subtitleSettings.minSubtitleSeconds);
            EditorGUILayout.HelpBox("Min Seconds below is the guaranteed minimum amount of time that a subtitle will be displayed (if its corresponding checkbox is ticked).", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.continueButton = (DisplaySettings.SubtitleSettings.ContinueButtonMode)EditorGUILayout.EnumPopup("Continue Button", DialogueManager.instance.displaySettings.subtitleSettings.continueButton);
            //.waitForContinueButton = EditorGUILayout.Toggle("Use Continue Button", DialogueManager.Instance.displaySettings.subtitleSettings.waitForContinueButton);
            EditorGUILayout.HelpBox("- Never: Conversation automatically moves to next stage when subtitle is done." +
                                    "\n- Always: Requires player to click a continue button to progress past each subtitle." +
                                    "\n- Optional Before Response Menu: If player response menu is next, shows but doesn't require clicking." +
                                    "\n- Never Before Response Menu: If player response menu is next, doesn't show." +
                                    "\nFor any setting other than Never, your UI must contain continue button(s).", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.subtitleSettings.richTextEmphases = EditorGUILayout.Toggle("Rich Text", DialogueManager.instance.displaySettings.subtitleSettings.richTextEmphases);
            EditorGUILayout.HelpBox("By default, emphasis tags embedded in dialogue text are applied to the entire subtitle. To convert them to rich text tags instead, tick this checkbox. This allows emphases to affect only parts of the text, but your GUI system must support rich text.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private enum DefaultSequenceStyle
        {
            DefaultCameraAngles,
            Closeups,
            WaitForSubtitle,
            Custom
        };

        private const string DefaultCameraAngleSequence = "Camera(default); required Camera(default,listener)@{{end}}";
        private const string DefaultCloseupSequence = "Camera(Closeup); required Camera(Closeup,listener)@{{end}}";
        private const string DefaultWaitForSubtitleSequence = "Delay({{end}})";

        private void DrawCutscenesStage()
        {
            if (DialogueManager.instance.displaySettings.cameraSettings == null) DialogueManager.instance.displaySettings.cameraSettings = new DisplaySettings.CameraSettings();
            EditorGUILayout.LabelField("Camera & Cutscene Settings", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("The Dialogue System uses an integrated, text-based cutscene sequencer. Every line of dialogue can have a cutscene sequence -- for example to move the camera, play animations on the speaker, or play a lip-synced voiceover.", MessageType.Info);
            EditorWindowTools.DrawHorizontalLine();

            EditorGUILayout.HelpBox("You can set up a camera object or prefab specifically for sequences. This can be useful to apply depth of field effects or other filters that you wouldn't normally apply to your gameplay camera. If you've set up a sequencer camera, assign it below. Otherwise the sequencer will just use the current main camera.", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.cameraSettings.sequencerCamera = EditorGUILayout.ObjectField("Sequencer Camera", DialogueManager.instance.displaySettings.cameraSettings.sequencerCamera, typeof(UnityEngine.Camera), true) as UnityEngine.Camera;
            EditorGUILayout.EndHorizontal();
            if (DialogueManager.instance.displaySettings.cameraSettings.sequencerCamera == null)
            {
                if (HasMainCamera())
                {
                    EditorGUILayout.HelpBox("Will use existing camera that is tagged Main Camera.", MessageType.None);
                }
                else
                {
                    EditorGUILayout.HelpBox("Will use existing camera that is tagged Main Camera. Remember to add a camera that's tagged Main Camera.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Will instantiate a copy of this camera for sequences.", MessageType.None);
            }
            EditorWindowTools.DrawHorizontalLine();

            EditorGUILayout.HelpBox("Cutscene sequence commands can reference camera angles defined on a camera angle prefab. You can set up your own custom camera angle prefab. Otherwise the sequencer will use a default camera angle prefab with basic angles such as Closeup, Medium, and Wide.", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.cameraSettings.cameraAngles = EditorGUILayout.ObjectField("Camera Angles", DialogueManager.instance.displaySettings.cameraSettings.cameraAngles, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();
            if (DialogueManager.instance.displaySettings.cameraSettings.cameraAngles == null) EditorGUILayout.HelpBox("Will use default camera angles.", MessageType.None);
            EditorWindowTools.DrawHorizontalLine();

            EditorGUILayout.HelpBox("If a dialogue entry doesn't define its own cutscene sequence, it will use the default sequence below.", MessageType.None);
            DialogueManager.instance.displaySettings.cameraSettings.defaultSequence = SelectSequence("Default Sequence", DialogueManager.instance.displaySettings.cameraSettings.defaultSequence);
            DialogueManager.instance.displaySettings.cameraSettings.defaultSequence = EditorGUILayout.TextField("Default Sequence", DialogueManager.instance.displaySettings.cameraSettings.defaultSequence);

            var usePlayerSequence = !string.IsNullOrEmpty(DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence);
            usePlayerSequence = EditorGUILayout.Toggle(new GUIContent("Player-Specific Default", "Tick to override Default Sequence for player lines"), usePlayerSequence);
            if (!usePlayerSequence)
            {
                DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence = string.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence))
                {
                    DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence = DefaultCameraAngleSequence;
                }
                DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence = SelectSequence("Player-Specific Default", DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence);
            }

            if (usePlayerSequence)
            {
                DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence = EditorGUILayout.TextField("Player-Specific Default", DialogueManager.instance.displaySettings.cameraSettings.defaultPlayerSequence);
            }
            else
            {
                EditorGUILayout.HelpBox("Will use Default Sequence for players and NPCs.", MessageType.None);
            }
            EditorGUILayout.HelpBox("In default sequences, you can use '{{end}}' to refer to the duration of the subtitle as determined by Chars/Second and Min Seconds.", MessageType.None);
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private bool HasMainCamera()
        {
            foreach (var camera in GameObjectUtility.FindObjectsByType<UnityEngine.Camera>())
            {
                if (camera.CompareTag("MainCamera")) return true;
            }
            return false;
        }

        private string SelectSequence(string label, string sequence)
        {
            DefaultSequenceStyle style = string.Equals(DefaultCameraAngleSequence, sequence)
                ? DefaultSequenceStyle.DefaultCameraAngles
                    : string.Equals(DefaultCloseupSequence, sequence)
                    ? DefaultSequenceStyle.Closeups
                    : (string.Equals(DefaultWaitForSubtitleSequence, sequence)
                       ? DefaultSequenceStyle.WaitForSubtitle
                       : DefaultSequenceStyle.Custom);
            DefaultSequenceStyle newStyle = (DefaultSequenceStyle)EditorGUILayout.EnumPopup(label, style);
            if (newStyle != style)
            {
                style = newStyle;
                switch (style)
                {
                    case DefaultSequenceStyle.DefaultCameraAngles: sequence = DefaultCameraAngleSequence; break;
                    case DefaultSequenceStyle.Closeups: sequence = DefaultCloseupSequence; break;
                    case DefaultSequenceStyle.WaitForSubtitle: sequence = DefaultWaitForSubtitleSequence; break;
                    default: break;
                }
            }
            switch (style)
            {
                case DefaultSequenceStyle.DefaultCameraAngles: EditorGUILayout.HelpBox("Changes to the speaker's default camera angle. At the end of the subtitle, changes to the listener's default angle. If angles aren't specified on the speaker or listener, the default is Closeup. Don't use this if your player is a body-less first person controller, since focusing the camera on the player probably doesn't make sense in this case.", MessageType.None); break;
                case DefaultSequenceStyle.Closeups: EditorGUILayout.HelpBox("Does a camera closeup of the speaker. At the end of the subtitle, changes to a closeup of the listener. Don't use this if your player is a body-less first person controller, since a closeup doesn't make sense in this case.", MessageType.None); break;
                case DefaultSequenceStyle.WaitForSubtitle: EditorGUILayout.HelpBox("Just waits for the subtitle to finish. Doesn't touch the camera.", MessageType.None); break;
                default: EditorGUILayout.HelpBox("Custom default sequence defined below.", MessageType.None); break;
            }
            return sequence;
        }

        private const float DefaultResponseTimeoutDuration = 10f;

        private void DrawInputsStage()
        {
            if (DialogueManager.instance.displaySettings.inputSettings == null) DialogueManager.instance.displaySettings.inputSettings = new DisplaySettings.InputSettings();
            EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("In this section, you'll specify input settings for the dialogue UI.", MessageType.Info);
            EditorWindowTools.StartIndentedSection();

            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.LabelField("Player Response Menu", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.inputSettings.alwaysForceResponseMenu = EditorGUILayout.Toggle("Always Force Menu", DialogueManager.instance.displaySettings.inputSettings.alwaysForceResponseMenu);
            EditorGUILayout.HelpBox("Tick to always force the response menu. If unticked, then when the player only has one valid response, the UI will automatically select it without showing the response menu.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.inputSettings.includeInvalidEntries = EditorGUILayout.Toggle("Include Invalid Entries", DialogueManager.instance.displaySettings.inputSettings.includeInvalidEntries);
            EditorGUILayout.HelpBox("Tick to include entries whose conditions are false in the menu. Their buttons will be visible but non-interactable.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            bool useTimeout = EditorGUILayout.Toggle("Timer", (DialogueManager.instance.displaySettings.inputSettings.responseTimeout > 0));
            EditorGUILayout.HelpBox("Tick to make the response menu timed. If unticked, players can take as long as they want to make their selection.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            if (useTimeout)
            {
                if (Tools.ApproximatelyZero(DialogueManager.instance.displaySettings.inputSettings.responseTimeout)) DialogueManager.instance.displaySettings.inputSettings.responseTimeout = DefaultResponseTimeoutDuration;
                DialogueManager.instance.displaySettings.inputSettings.responseTimeout = EditorGUILayout.FloatField("Timeout Seconds", DialogueManager.instance.displaySettings.inputSettings.responseTimeout);
                DialogueManager.instance.displaySettings.inputSettings.responseTimeoutAction = (ResponseTimeoutAction)EditorGUILayout.EnumPopup("If Time Runs Out", DialogueManager.instance.displaySettings.inputSettings.responseTimeoutAction);
            }
            else
            {
                DialogueManager.instance.displaySettings.inputSettings.responseTimeout = 0;
            }

            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.LabelField("Quick Time Event (QTE) Trigger Buttons", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("QTE trigger buttons may be defined on the Dialogue Manager's inspector under Display Settings > Input Settings > QTE Input Buttons.", MessageType.None);
            if (GUILayout.Button("Inspect Dialogue Manager object", GUILayout.Width(240))) Selection.activeObject = DialogueManager.instance;

            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.LabelField("Cancel Line", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.inputSettings.cancel.key = (KeyCode)EditorGUILayout.EnumPopup("Key", DialogueManager.instance.displaySettings.inputSettings.cancel.key);
            EditorGUILayout.HelpBox("Pressing this key interrupts the currently-playing subtitle.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.inputSettings.cancel.buttonName = EditorGUILayout.TextField("Button Name", DialogueManager.instance.displaySettings.inputSettings.cancel.buttonName);
            EditorGUILayout.HelpBox("Pressing this button interrupts the currently-playing subtitle.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorWindowTools.DrawHorizontalLine();
            EditorGUILayout.LabelField("Cancel Conversation", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.inputSettings.cancelConversation.key = (KeyCode)EditorGUILayout.EnumPopup("Key", DialogueManager.instance.displaySettings.inputSettings.cancel.key);
            EditorGUILayout.HelpBox("Pressing this key cancels the conversation if in the response menu.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.inputSettings.cancelConversation.buttonName = EditorGUILayout.TextField("Button Name", DialogueManager.instance.displaySettings.inputSettings.cancel.buttonName);
            EditorGUILayout.HelpBox("Pressing this button cancels the conversation if in the response menu.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorWindowTools.EndIndentedSection();
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private const float DefaultAlertCheckFrequency = 2f;

        private void DrawAlertsStage()
        {
            if (DialogueManager.instance.displaySettings.alertSettings == null) DialogueManager.instance.displaySettings.alertSettings = new DisplaySettings.AlertSettings();
            EditorGUILayout.LabelField("Alert Settings", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("Alerts are gameplay messages. They can be delivered from conversations or other sources.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.alertSettings.allowAlertsDuringConversations = EditorGUILayout.Toggle("Allow In Conversations", DialogueManager.instance.displaySettings.alertSettings.allowAlertsDuringConversations);
            EditorGUILayout.HelpBox("Tick to allow alerts to be displayed while in conversations. If unticked, alerts won't be displayed until the conversation has ended.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            bool monitorAlerts = EditorGUILayout.Toggle("Monitor Alerts", (DialogueManager.instance.displaySettings.alertSettings.alertCheckFrequency > 0));
            EditorGUILayout.HelpBox("Tick to constantly monitor the Lua value \"Variable['Alert']\" and display its contents as an alert. This runs as a background process. The value is automatically checked at the end of conversations, and you can check it manually. Unless you have a need to constantly monitor the value, it's more efficient to leave this unticked.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            if (monitorAlerts)
            {
                if (Tools.ApproximatelyZero(DialogueManager.instance.displaySettings.alertSettings.alertCheckFrequency)) DialogueManager.instance.displaySettings.alertSettings.alertCheckFrequency = DefaultAlertCheckFrequency;
                DialogueManager.instance.displaySettings.alertSettings.alertCheckFrequency = EditorGUILayout.FloatField("Frequency (Seconds)", DialogueManager.instance.displaySettings.alertSettings.alertCheckFrequency);
            }
            else
            {
                DialogueManager.instance.displaySettings.alertSettings.alertCheckFrequency = 0;
            }
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.alertSettings.alertCharsPerSecond = EditorGUILayout.FloatField("Chars/Second", DialogueManager.instance.displaySettings.alertSettings.alertCharsPerSecond);
            EditorGUILayout.HelpBox("Determines how long the alert is displayed. If zero, use Subtitle Settings > Chars/Second.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DialogueManager.instance.displaySettings.alertSettings.minAlertSeconds = EditorGUILayout.FloatField("Min Seconds", DialogueManager.instance.displaySettings.alertSettings.minAlertSeconds);
            EditorGUILayout.HelpBox("The guaranteed minimum amount of time that an alert will be displayed. If zero, use Subtitle Settings > Min Subtitle Seconds.", MessageType.None);
            EditorGUILayout.EndHorizontal();
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawSavingStage()
        {
            EditorGUILayout.LabelField("Save System Settings", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();

            var hasSaveSystem = DialogueManager.instance.GetComponent<PixelCrushers.SaveSystem>() != null;
            var useSaveSystem = EditorGUILayout.Toggle("Use Save System", hasSaveSystem);
            if (useSaveSystem && !hasSaveSystem)
            {
                DialogueManager.instance.gameObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.SaveSystem)));
                hasSaveSystem = true;
            }
            else if (!useSaveSystem && hasSaveSystem)
            {
                DestroyImmediate(DialogueManager.instance.GetComponent<PixelCrushers.SaveSystem>());
                hasSaveSystem = false;
            }
            if (hasSaveSystem)
            {
                EditorGUILayout.HelpBox("The Dialogue System will automatically add a Player Prefs Saved Game Data Storer and JSON Data Serializer at runtime unless you add a different type of Saved Game Data Storer or Data Serializer component to it. For example, you can manually add a Disk Saved Game Data Storer if you want to save games to local disk files.", MessageType.None);
            }

            if (hasSaveSystem)
            {
                var dsSaver = DialogueManager.instance.GetComponent<DialogueSystemSaver>();
                if (dsSaver == null)
                {
                    dsSaver = DialogueManager.instance.gameObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.DialogueSystem.DialogueSystemSaver))) as PixelCrushers.DialogueSystem.DialogueSystemSaver;
                    dsSaver.saveAcrossSceneChanges = true;
                }

                var transitionManager = DialogueManager.instance.GetComponent<PixelCrushers.SceneTransitionManager>();
                var hasTransitionManager = (transitionManager != null);
                var useTransitionManager = EditorGUILayout.Toggle("Use Scene Transitions", hasTransitionManager);
                if (useTransitionManager && !hasTransitionManager)
                {
                    hasTransitionManager = true;
                    transitionManager = DialogueManager.instance.gameObject.AddComponent(TypeUtility.GetWrapperType(typeof(PixelCrushers.StandardSceneTransitionManager))) as PixelCrushers.StandardSceneTransitionManager;
                    var stdTransitionMgr = transitionManager as StandardSceneTransitionManager;
                    stdTransitionMgr.pauseDuringTransition = true;
                    if (DialogueManager.instance.transform.Find("SceneFaderCanvas") == null)
                    {
                        var faderCanvasPrefab = AssetDatabase.LoadMainAssetAtPath("Assets/Plugins/Pixel Crushers/Dialogue System/Prefabs/Art/SceneFaderCanvas.prefab");
                        if (faderCanvasPrefab == null)
                        {
                            EditorUtility.DisplayDialog("Prefab Not Found", "This wizard can't find the SceneFaderCanvas prefab. You may have moved the Dialogue System to a different folder in your project. Scene transitions will not fade to black.", "OK");
                        }
                        else
                        {
                            var prefabGO = PrefabUtility.InstantiatePrefab(faderCanvasPrefab) as GameObject;
                            prefabGO.transform.SetParent(DialogueManager.instance.transform, false);
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                            stdTransitionMgr.leaveSceneTransition.animator = prefabGO.GetComponent<Animator>();
                            stdTransitionMgr.leaveSceneTransition.trigger = "Show";
                            stdTransitionMgr.leaveSceneTransition.animationDuration = 1;
                            stdTransitionMgr.leaveSceneTransition.minTransitionDuration = 1;
                            stdTransitionMgr.enterSceneTransition.animator = prefabGO.GetComponent<Animator>();
                            stdTransitionMgr.enterSceneTransition.trigger = "Hide";
                        }
                    }
                }
                else if (!useTransitionManager && hasTransitionManager)
                {
                    DestroyImmediate(transitionManager);
                    hasTransitionManager = false;
                }
                if (hasTransitionManager)
                {
                    EditorGUILayout.HelpBox("The Dialogue Manager's Scene Transition Manager will fade to black by default. If you want to use a separate loading scene, inspect the Dialogue Manager and set the Loading Scene Name field.", MessageType.None);
                }
            }

            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, false);
        }

        private void DrawReviewStage()
        {
            EditorGUILayout.LabelField("Review", EditorStyles.boldLabel);
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.HelpBox("Your Dialogue Manager is ready! Below is a brief summary of the configuration.", MessageType.Info);
            EditorGUILayout.LabelField(string.Format("Dialogue database: {0}", DialogueManager.instance.initialDatabase.name));
            EditorGUILayout.LabelField(string.Format("Dialogue UI: {0}", (DialogueManager.instance.displaySettings.dialogueUI == null) ? "(use default)" : DialogueManager.instance.displaySettings.dialogueUI.name));
            EditorGUILayout.LabelField(string.Format("Show subtitles: {0}", GetSubtitlesInfo()));
            EditorGUILayout.LabelField(string.Format("Always force response menu: {0}", DialogueManager.instance.displaySettings.inputSettings.alwaysForceResponseMenu));
            EditorGUILayout.LabelField(string.Format("Response menu timeout: {0}", Tools.ApproximatelyZero(DialogueManager.instance.displaySettings.inputSettings.responseTimeout) ? "Unlimited Time" : DialogueManager.instance.displaySettings.inputSettings.responseTimeout.ToString()));
            EditorGUILayout.LabelField(string.Format("Allow alerts during conversations: {0}", DialogueManager.instance.displaySettings.alertSettings.allowAlertsDuringConversations));
            EditorGUILayout.LabelField(string.Format("Use save system: {0}", (DialogueManager.instance.GetComponent<PixelCrushers.SaveSystem>() != null) ? "Yes" : "No"));
            EditorGUILayout.HelpBox("The Dialogue Manager's inspector has other settings, too, such as Persistent Data Settings. If you want to view those settings, click the button below.", MessageType.None);
            if (GUILayout.Button("Inspect Dialogue Manager object", GUILayout.Width(240))) Selection.activeObject = DialogueManager.instance;
            EditorWindowTools.EndIndentedSection();
            DrawNavigationButtons(true, true, true);
        }

        private string GetSubtitlesInfo()
        {
            if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
                (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
                (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true))
            {
                return "All";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false))
            {
                return "NPC only";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true))
            {
                return "While speaking lines";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false))
            {
                return "Only while NPC is speaking";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true))
            {
                return "Only PC and response menu reminder";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false))
            {
                return "Only response menu reminder";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true))
            {
                return "PC Only";
            }
            else if ((DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
                     (DialogueManager.instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false))
            {
                return "None";
            }
            else
            {
                return "See inspector";
            }
        }

    }

}
