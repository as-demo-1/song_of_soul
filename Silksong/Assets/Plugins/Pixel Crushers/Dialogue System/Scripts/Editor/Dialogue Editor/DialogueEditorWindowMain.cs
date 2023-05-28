// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// Dialogue database editor window. This is the main part of a class that spans 
    /// several files using partial classes. Each file handles one aspect of the 
    /// Dialogue Editor, such as the Actors tab or the Items tab.
    /// </summary>
    public partial class DialogueEditorWindow : EditorWindow
    {

        public static DialogueEditorWindow instance = null;

        public static object inspectorSelection
        {
            get
            {
                return _inspectorSelection;
            }
            set
            {
                _inspectorSelection = value;
                if ((value != null) && (instance != null) && (EditorWindow.focusedWindow == instance))
                {
                    Selection.activeObject = DialogueEditorWindow.instance.database;
                }
                DialogueDatabaseEditor.RepaintInstances();
            }
        }
        [SerializeField]
        private static object _inspectorSelection = null;

        [SerializeField]
        private Toolbar toolbar = new Toolbar();

        [SerializeField]
        private DialogueDatabase database = null;
        public static DialogueDatabase GetCurrentlyEditedDatabase() { return (instance != null) ? instance.database : null; }

        private SerializedObject serializedObject = null;

        [SerializeField]
        private bool debug = false;

        [SerializeField]
        private bool registerCompleteObjectUndo = true;

        private bool verboseDebug = false;

        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;

        private Template template = Template.FromDefault();

        private const string CompleteUndoKey = "PixelCrushers.DialogueSystem.DialogueEditor.registerCompleteObjectUndo";
        private const string ShowNodeEditorKey = "PixelCrushers.DialogueSystem.DialogueEditor.ShowNodeEditor";
        private const string ShowDatabaseNameKey = "PixelCrushers.DialogueSystem.DialogueEditor.ShowDatabaseName";
        private const string SyncOnOpenKey = "PixelCrushers.DialogueSystem.DialogueEditor.SyncOnOpen";
        private const string AutoBackupKey = "PixelCrushers.DialogueSystem.DialogueEditor.AutoBackupFrequency";
        private const string AutoBackupFolderKey = "PixelCrushers.DialogueSystem.DialogueEditor.AutoBackupFolder";
        private const string AddNewNodesToRightKey = "PixelCrushers.DialogueSystem.DialogueEditor.AddNewNodesToRight";
        private const string TrimWhitespaceAroundPipesKey = "PixelCrushers.DialogueSystem.DialogueEditor.TrimWhitespaceAroundPipes";
        private const string LocalizationLanguagesKey = "PixelCrushers.DialogueSystem.DialogueEditor.LocalizationLanguages";
        private const string SequencerDragDropCommandsKey = "PixelCrushers.DialogueSystem.DialogueEditor.SequencerDragDropCommands";
        private const string DialogueEditorPrefsKey = "PixelCrushers.DialogueSystem.DialogueEditor.Prefs";

        private const float RuntimeUpdateFrequency = 0.5f;
        private float timeSinceLastRuntimeUpdate = 0;

        private const float DefaultAutoBackupFrequency = 600;
        private float autoBackupFrequency = DefaultAutoBackupFrequency;
        private float timeForNextAutoBackup = 0;
        private string autoBackupFolder = string.Empty;

        private bool showDatabaseName = true;

        private bool syncOnOpen = true;

        private DialogueEditorPrefs prefs = null;

        private const float MinWidth = 720f;
        private const float MinHeight = 240f;

        public static void ResetPosition()
        {
            if (instance == null)
            {
                instance = OpenDialogueEditorWindow();
            }
            instance.position = new Rect(0f, 0f, MinWidth, MinHeight);
        }

        private void OnEnable()
        {
            if (debug) Debug.Log("<color=green>Dialogue Editor: OnEnable (Selection.activeObject=" + Selection.activeObject + ", database=" + database + ")</color>", Selection.activeObject);
            instance = this;
            minSize = new Vector2(MinWidth, MinHeight);
            if (database != null)
            {
                LoadTemplateFromDatabase();
            }
            else if (Selection.activeObject != null)
            {
                SelectObject(Selection.activeObject);
            }
            else if (EditorWindow.focusedWindow == this)
            {
                SelectObject(database);
            }
            toolbar.UpdateTabNames(template.treatItemsAsQuests);
            if (database != null) ValidateConversationMenuTitleIndex();
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
#endif
            showQuickDialogueTextEntry = false;
            LoadEditorSettings();
            if (database == null) template = TemplateTools.LoadFromEditorPrefs();
        }

        private void OnDisable()
        {
            if (debug) Debug.Log("<color=orange>Dialogue Editor: OnDisable</color>");
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#else
            EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
#endif
            //--- No need to save assets after entering play mode? This was a workaround for Asset Database bugs.
            //try
            //{
            //    EditorApplication.delayCall += AssetDatabase.SaveAssets;
            //}
            //catch (System.NullReferenceException) { } // Some Unity versions w/disabled domain reloading don't allow when entering play mode.
            SaveTemplate();
            inspectorSelection = null;
            instance = null;
            SaveEditorSettings();
        }

        private void SaveTemplate()
        {
            TemplateTools.SaveToEditorPrefs(template);
            SaveTemplateToDatabase();
        }

        private void LoadEditorSettings()
        {
            registerCompleteObjectUndo = EditorPrefs.GetBool(CompleteUndoKey, true);
            showDatabaseName = EditorPrefs.GetBool(ShowDatabaseNameKey, true);
            syncOnOpen = EditorPrefs.GetBool(SyncOnOpenKey, true);
            autoBackupFrequency = EditorPrefs.GetFloat(AutoBackupKey, DefaultAutoBackupFrequency);
            autoBackupFolder = EditorPrefs.GetString(AutoBackupFolderKey, string.Empty);
            timeForNextAutoBackup = Time.realtimeSinceStartup + autoBackupFrequency;
            addNewNodesToRight = EditorPrefs.GetBool(AddNewNodesToRightKey, false);
            trimWhitespaceAroundPipes = EditorPrefs.GetBool(TrimWhitespaceAroundPipesKey, true);
            if (EditorPrefs.HasKey(LocalizationLanguagesKey)) localizationLanguages = JsonUtility.FromJson<LocalizationLanguages>(EditorPrefs.GetString(LocalizationLanguagesKey));
            if (EditorPrefs.HasKey(SequencerDragDropCommandsKey)) SequenceEditorTools.RestoreDragDropCommands(EditorPrefs.GetString(SequencerDragDropCommandsKey));
            if (EditorPrefs.HasKey(DialogueEditorPrefsKey)) prefs = JsonUtility.FromJson<DialogueEditorPrefs>(EditorPrefs.GetString(DialogueEditorPrefsKey));
            if (prefs == null) prefs = new DialogueEditorPrefs();
        }

        private void SaveEditorSettings()
        {
            EditorPrefs.SetBool(CompleteUndoKey, registerCompleteObjectUndo);
            EditorPrefs.SetBool(ShowDatabaseNameKey, showDatabaseName);
            EditorPrefs.SetBool(SyncOnOpenKey, syncOnOpen);
            EditorPrefs.SetFloat(AutoBackupKey, autoBackupFrequency);
            EditorPrefs.SetString(AutoBackupFolderKey, autoBackupFolder);
            EditorPrefs.SetBool(AddNewNodesToRightKey, addNewNodesToRight);
            EditorPrefs.SetBool(TrimWhitespaceAroundPipesKey, trimWhitespaceAroundPipes);
            EditorPrefs.SetString(LocalizationLanguagesKey, JsonUtility.ToJson(localizationLanguages));
            EditorPrefs.SetString(SequencerDragDropCommandsKey, SequenceEditorTools.SaveDragDropCommands());
            EditorPrefs.SetString(DialogueEditorPrefsKey, JsonUtility.ToJson(prefs));
        }

        private void LoadTemplateFromDatabase()
        {
            if (database == null || string.IsNullOrEmpty(database.templateJson)) return;
            var databaseTemplate = JsonUtility.FromJson<Template>(database.templateJson);
            if (databaseTemplate != null) template = databaseTemplate;
        }

        private void SaveTemplateToDatabase()
        {
            if (database == null) return;
            database.templateJson = JsonUtility.ToJson(template);
            SetDatabaseDirty("Save template");
        }

#if UNITY_2017_2_OR_NEWER
        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            HandlePlayModeStateChanged();
        }
#else
        private void OnPlaymodeStateChanged()
        {
            HandlePlayModeStateChanged();
        }
#endif

        private void HandlePlayModeStateChanged()
        {
            if (debug) Debug.Log("<color=cyan>Dialogue Editor: OnPlaymodeStateChanged - isPlaying=" + EditorApplication.isPlaying + "/" + EditorApplication.isPlayingOrWillChangePlaymode + "</color>");
            if (!EditorApplication.isPlaying) AssetDatabase.SaveAssets();
            toolbar.UpdateTabNames(template.treatItemsAsQuests);
            currentConversationState = null;
            currentRuntimeEntry = null;
            actorPortraitCache = null;
            UITools.ClearSpriteCache();
            ResetWatchSection();
            SetReorderableListInspectorSelection();
            SelectObject(database);
        }

        private void SetReorderableListInspectorSelection()
        {
            if (database == null) return;
            if (EditorWindow.focusedWindow != instance) return;
            switch (toolbar.current)
            {
                case Toolbar.Tab.Actors:
                    if (0 <= actorListSelectedIndex && actorListSelectedIndex < database.actors.Count) inspectorSelection = database.actors[actorListSelectedIndex];
                    break;
                case Toolbar.Tab.Items:
                    if (0 <= itemListSelectedIndex && itemListSelectedIndex < database.items.Count) inspectorSelection = database.items[itemListSelectedIndex];
                    break;
                case Toolbar.Tab.Locations:
                    if (0 <= locationListSelectedIndex && locationListSelectedIndex < database.locations.Count) inspectorSelection = database.locations[locationListSelectedIndex];
                    break;
            }
        }

        private void OnSelectionChange()
        {
            SelectObject(Selection.activeObject);
        }

        public void SelectObject(UnityEngine.Object obj)
        {
            var newDatabase = obj as DialogueDatabase;
            if (newDatabase != null)
            {
                var databaseID = (database != null) ? database.GetInstanceID() : -1;
                var newDatabaseID = newDatabase.GetInstanceID();
                var needToReset = (database != null) && (databaseID != newDatabaseID);
                if (debug) Debug.Log("<color=yellow>Dialogue Editor: SelectDatabase " + newDatabase + "(ID=" + newDatabaseID + "), old=" + database + "(ID=" + databaseID + "), reset=" + needToReset + "</color>", newDatabase);
                database = newDatabase;
                ClearDatabaseNameStyle();
                LoadTemplateFromDatabase();
                serializedObject = new SerializedObject(database);
                if (needToReset)
                {
                    Reset();
                }
                else
                {
                    ResetVariableSection();
                    ResetConversationSection();
                    ResetConversationNodeEditor();
                    UpdateReferencesByID();
                }
                if (syncOnOpen) database.SyncAll();
                Repaint();
            }
        }

        private void UpdateReferencesByID()
        {
            SetCurrentConversationByID();
            ValidateConversationMenuTitleIndex();
        }

        public void Reset()
        {
            ResetActorSection();
            ResetItemSection();
            ResetLocationSection();
            ResetVariableSection();
            ResetConversationSection();
            ResetConversationNodeEditor();
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                // At runtime, check if we need to update the display:
                timeSinceLastRuntimeUpdate += Time.unscaledDeltaTime;
                if (timeSinceLastRuntimeUpdate > RuntimeUpdateFrequency)
                {
                    timeSinceLastRuntimeUpdate = 0;
                    switch (toolbar.Current)
                    {
                        case Toolbar.Tab.Conversations:
                            UpdateRuntimeConversationsTab();
                            break;
                        case Toolbar.Tab.Templates:
                            UpdateRuntimeWatchesTab();
                            break;
                    }
                }
            }
            else
            {
                if (autoBackupFrequency > 0.001f && Time.realtimeSinceStartup > timeForNextAutoBackup)
                {
                    timeForNextAutoBackup = Time.realtimeSinceStartup + autoBackupFrequency;
                    if (EditorWindow.focusedWindow == this) MakeAutoBackup();
                }
            }
        }

        public void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed")
            {
                HandleUndo();
            }
            else
            {
                try
                {
                    if (instance == null) instance = this;
                    RecordUndo();
                    var isInNodeEditor = (toolbar.Current == Toolbar.Tab.Conversations) && showNodeEditor;
                    if (!isInNodeEditor) DrawDatabaseName(); // Node editor draws name after grid.
                    DrawToolbar();
                    DrawMainBody();
                }
                catch (System.ArgumentException)
                {
                    // Not ideal to hide ArgumentException, but window can change layout during a repaint,
                    // which can cause this exception.
                }
            }
        }

        private Color proDatabaseNameColor = new Color(1, 1, 1, 0.2f);
        private Color freeDatabaseNameColor = new Color(0, 0, 0, 0.2f);
        private Color proDatabaseBackupNameColor = new Color(1, 0, 0, 0.5f);
        private Color freeDatabaseBackupNameColor = new Color(1, 0, 0, 0.5f);

        private GUIStyle _databaseNameStyle = null;
        private GUIStyle databaseNameStyle
        {
            get
            {
                if (_databaseNameStyle == null || _databaseNameStyle.fontSize != 20)
                {
                    _databaseNameStyle = new GUIStyle(EditorStyles.label);
                    _databaseNameStyle.fontSize = 20;
                    _databaseNameStyle.fontStyle = FontStyle.Bold;
                    _databaseNameStyle.alignment = TextAnchor.LowerLeft;
                    _databaseNameStyle.normal.textColor = database.name.EndsWith("(Auto-Backup)")
                        ? EditorGUIUtility.isProSkin
                            ? proDatabaseBackupNameColor
                            : freeDatabaseBackupNameColor
                        : EditorGUIUtility.isProSkin
                            ? proDatabaseNameColor
                            : freeDatabaseNameColor;
                }
                return _databaseNameStyle;
            }
        }
        private void ClearDatabaseNameStyle() { _databaseNameStyle = null; }

        private GUIStyle _conversationParticipantsStyle = null;
        private GUIStyle conversationParticipantsStyle
        {
            get
            {
                if (_conversationParticipantsStyle == null || _conversationParticipantsStyle.fontSize != 20)
                {
                    _conversationParticipantsStyle = new GUIStyle(databaseNameStyle);
                    _conversationParticipantsStyle.alignment = TextAnchor.UpperRight;
                }
                return _conversationParticipantsStyle;
            }
        }

        private void DrawDatabaseName()
        {
            if (!showDatabaseName) return;
            if (database == null) return;
            if (IsSearchBarVisible || IsLuaWatchBarVisible) return;
            EditorGUI.LabelField(new Rect(4, position.height - 30, position.width, 30), database.name, databaseNameStyle);
        }

        private void DrawToolbar()
        {
            Toolbar.Tab previous = toolbar.Current;
            toolbar.Draw();
            if (toolbar.Current != previous)
            {
                ResetAssetLists();
                ResetDialogueTreeGUIStyles();
                ResetLanguageList();
                if (toolbar.Current == Toolbar.Tab.Conversations)
                {
                    UpdateConversationTitles();
                    ResetNodeEditorConversationList();
                }
                if (toolbar.current == Toolbar.Tab.Database)
                {
                    ResetDatabaseTab();
                }
                if (toolbar.Current == Toolbar.Tab.Items)
                {
                    BuildLanguageListFromItems();
                }
                else if (!((toolbar.Current == Toolbar.Tab.Conversations) && showNodeEditor))
                {
                    inspectorSelection = null;
                }
            }
        }

        private void DrawMainBody()
        {
            if ((database != null) || (toolbar.Current == Toolbar.Tab.Templates))
            {
                DrawCurrentSection();
            }
            else
            {
                DrawNoDatabaseSection();
            }
        }

        private void DrawCurrentSection()
        {
            EditorGUI.BeginChangeCheck();
            EditorStyles.textField.wordWrap = true;
            var isInNodeEditor = (toolbar.Current == Toolbar.Tab.Conversations) && showNodeEditor;
            if (isInNodeEditor) HandleNodeEditorScrollWheelEvents();
            if (!isInNodeEditor) scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            try
            {
                switch (toolbar.Current)
                {
                    case Toolbar.Tab.Database:
                        DrawDatabaseSection();
                        break;
                    case Toolbar.Tab.Actors:
                        DrawActorSection();
                        break;
                    case Toolbar.Tab.Items:
                        DrawItemSection();
                        break;
                    case Toolbar.Tab.Locations:
                        DrawLocationSection();
                        break;
                    case Toolbar.Tab.Variables:
                        DrawVariableSection();
                        break;
                    case Toolbar.Tab.Conversations:
                        DrawConversationSection();
                        break;
                    case Toolbar.Tab.Templates:
                        if (Application.isPlaying)
                        {
                            DrawWatchSection();
                        }
                        else
                        {
                            DrawTemplateSection();
                        }
                        break;
                }
            }
            finally
            {
                if (!isInNodeEditor) EditorGUILayout.EndScrollView();
            }
            if (IsSearchBarVisible) DrawDialogueTreeSearchBar();
            if (IsLuaWatchBarVisible) DrawLuaWatchBar();

            try
            {
                if (EditorGUI.EndChangeCheck()) SetDatabaseDirty("Standard Editor Change");
            }
            catch (System.InvalidOperationException) { }
        }

    }

}