#if USE_TWINE
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Twine
{

    /// <summary>
    /// Twine import window. Imports Twine 2 Harlowe stories exported from Twison
    /// into dialogue database conversations.
    /// </summary>
    public class TwineImportWindow : EditorWindow
    {

        #region Prefs

        [Serializable]
        public class StoryInfo
        {
            public string jsonFilename;
            public int actorID;
            public int conversantID;
            public bool splitPipes = true;
        }

        [Serializable]
        public class Prefs
        {
            public string databaseGuid = string.Empty;
            public List<StoryInfo> storyInfoList = new List<StoryInfo>();
            public EncodingType encodingType = EncodingType.Default;
            public bool useTwineNodePositions = false;

            public const string PrefsKey = "PixelCrushers.DialogueSystem.TwineImportPrefs";

            public static Prefs Load()
            {
                return JsonUtility.FromJson<Prefs>(EditorPrefs.GetString(PrefsKey)) ?? new Prefs();
            }

            public void Save()
            {
                EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(this));
            }
        }

        #endregion

        #region Initialization

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Twine 2 (Twison)...", false, 1)]
        public static void Init()
        {
            GetWindow(typeof(TwineImportWindow), false, "Twine Import");
        }

        protected Prefs prefs = null;
        protected Template template = null;
        protected DialogueDatabase database = null;
        protected ReorderableList storyInfoReorderableList = null;
        protected string lastDirectory = string.Empty;
        protected string[] actorNames;
        protected int[] actorIDs;
        protected bool traceExceptions = false;
        protected static GUIContent traceExceptionsGUIContent = new GUIContent("Trace Exceptions (Debug)", "If import process throws an exception, stop and pinpoint error. Used for internal debugging.");
        protected static GUIContent useTwineNodePositionsGUIContent = new GUIContent("Use Twine Node Positions", "Import node positions from Twine. If unticked, auto-arranges nodes on import.");
        protected TwineImporter twineImporter = new TwineImporter();

        protected virtual void OnEnable()
        {
            prefs = Prefs.Load();
            database = GetDatabaseFromGuid(prefs.databaseGuid);
            template = TemplateTools.LoadFromEditorPrefs();
            storyInfoReorderableList = new ReorderableList(prefs.storyInfoList, typeof(string), true, true, true, true);
            storyInfoReorderableList.drawHeaderCallback += OnDrawFilenameListHeader;
            storyInfoReorderableList.drawElementCallback += OnDrawFilenameListElement;
            storyInfoReorderableList.onAddCallback += OnAddFilenameToList;
            lastDirectory = (prefs.storyInfoList.Count > 0) ? Path.GetDirectoryName(prefs.storyInfoList[0].jsonFilename) : string.Empty;
            RefreshActorPopupData();
        }

        protected virtual void OnDisable()
        {
            prefs.Save();
        }

        protected DialogueDatabase GetDatabaseFromGuid(string guid)
        {
            return AssetDatabase.LoadAssetAtPath<DialogueDatabase>(AssetDatabase.GUIDToAssetPath(guid));
        }

        #endregion

        #region GUI

        protected virtual void OnGUI()
        {
            DrawStoryInfoList();
            DrawDatabaseField();
            prefs.useTwineNodePositions = EditorGUILayout.Toggle(useTwineNodePositionsGUIContent, prefs.useTwineNodePositions);
            traceExceptions = EditorGUILayout.Toggle(traceExceptionsGUIContent, traceExceptions);
            DrawImportButton();
        }

        protected virtual void DrawDatabaseField()
        {
            EditorGUI.BeginChangeCheck();
            database = EditorGUILayout.ObjectField("Database", database, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (EditorGUI.EndChangeCheck())
            {
#if UNITY_2017
                if (database != null)
                {
                    var path = AssetDatabase.GetAssetPath(database);
                    prefs.databaseGuid = AssetDatabase.AssetPathToGUID(path);
                }
#else
                long localId;
                try
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(database, out prefs.databaseGuid, out localId);
                }
                catch (NullReferenceException) { }
#endif
                RefreshActorPopupData();
            }
        }

        protected virtual void DrawStoryInfoList()
        {
            storyInfoReorderableList.DoLayoutList();
            prefs.encodingType = (EncodingType)EditorGUILayout.EnumPopup("Encoding", prefs.encodingType);
        }

        protected virtual void DrawImportButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            try
            {
                EditorGUI.BeginDisabledGroup(database == null || prefs.storyInfoList.Count == 0);
                if (GUILayout.Button("Import", GUILayout.Width(128)))
                {
                    Import();
                }
                EditorGUI.EndDisabledGroup();
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private static GUIContent PipesCheckboxGUIContent = new GUIContent("Pipes", "Split dialogue entries at pipe characters.");
        private const float PipesLabelWidth = 36;
        private const float PipesCheckboxWidth = 16;

        protected void OnDrawFilenameListHeader(Rect rect)
        {
            //EditorGUI.LabelField(rect, "JSON Files");
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - PipesLabelWidth, rect.height), "JSON Files");
            EditorGUI.LabelField(new Rect(rect.x + rect.width - PipesLabelWidth, rect.y, PipesLabelWidth, rect.height), PipesCheckboxGUIContent);
        }

        protected void OnDrawFilenameListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < prefs.storyInfoList.Count)) return;
            const float ActorWidth = 80;
            var rect5 = new Rect(rect.x + rect.width - PipesCheckboxWidth, rect.y, PipesCheckboxWidth, EditorGUIUtility.singleLineHeight);
            var rect4 = new Rect(rect.x + rect.width - ActorWidth - PipesCheckboxWidth, rect.y, ActorWidth, EditorGUIUtility.singleLineHeight);
            var rect3 = new Rect(rect4.x - ActorWidth, rect.y, ActorWidth, EditorGUIUtility.singleLineHeight);
            var rect2 = new Rect(rect3.x - 28, rect.y, 28, EditorGUIUtility.singleLineHeight);
            var rect1 = new Rect(rect.x, rect.y, rect2.x - rect.x, EditorGUIUtility.singleLineHeight);
            prefs.storyInfoList[index].jsonFilename = EditorGUI.TextField(rect1, GUIContent.none, prefs.storyInfoList[index].jsonFilename);
            if (GUI.Button(rect2, "...", EditorStyles.miniButton))
            {
                var filename = EditorUtility.OpenFilePanel("Select JSON File", lastDirectory, "json");
                if (string.IsNullOrEmpty(filename)) return;
                prefs.storyInfoList[index].jsonFilename = filename;
            }
            if (database != null)
            {
                prefs.storyInfoList[index].actorID = DrawActorPopup(rect3, prefs.storyInfoList[index].actorID);
                prefs.storyInfoList[index].conversantID = DrawActorPopup(rect4, prefs.storyInfoList[index].conversantID);
            }
            prefs.storyInfoList[index].splitPipes = EditorGUI.Toggle(rect5, GUIContent.none, prefs.storyInfoList[index].splitPipes);
        }

        protected void OnAddFilenameToList(ReorderableList list)
        {
            var filename = EditorUtility.OpenFilePanel("Select JSON File", lastDirectory, "json");
            if (string.IsNullOrEmpty(filename)) return;
            prefs.storyInfoList.Add(new StoryInfo()
            {
                jsonFilename = filename,
                actorID = 1,
                conversantID = 2
            });
        }

        protected int DrawActorPopup(Rect rect, int id)
        {
            var index = EditorGUI.Popup(rect, GetActorIndex(id), actorNames);
            return (0 <= index && index < actorIDs.Length) ? actorIDs[index] : id;
        }

        protected void RefreshActorPopupData()
        {
            if (database == null) return;
            actorNames = new string[database.actors.Count];
            actorIDs = new int[database.actors.Count];
            for (int i = 0; i < database.actors.Count; i++)
            {
                actorNames[i] = database.actors[i].Name;
                actorIDs[i] = database.actors[i].id;
            }
        }

        protected int GetActorIndex(int id)
        {
            if (actorIDs == null) return -1;
            for (int index = 0; index < actorIDs.Length; index++)
            {
                if (actorIDs[index] == id) return index;
            }
            return -1;
        }

        #endregion

        #region Import

        protected virtual void Import()
        {
            try
            {
                Undo.RecordObject(database, "Import Twine");
                var total = prefs.storyInfoList.Count;
                for (int i = 0; i < total; i++)
                {
                    EditorUtility.DisplayProgressBar("Importing Twine Story", prefs.storyInfoList[i].jsonFilename, (float)i / (float)total);
                    TryImportStory(prefs.storyInfoList[i]);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (DialogueEditor.DialogueEditorWindow.instance != null)
                {
                    DialogueEditor.DialogueEditorWindow.instance.RefreshConversation();
                }
            }
        }

        protected virtual void TryImportStory(StoryInfo storyInfo)
        {
            if (traceExceptions)
            {
                ImportStory(storyInfo);
            }
            else
            {
                try
                {
                    ImportStory(storyInfo);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Unable to import " + storyInfo.jsonFilename + ": " + e.Message);
                }
            }
        }


        protected virtual void ImportStory(StoryInfo storyInfo)
        {
            Debug.Log("Importing " + storyInfo.jsonFilename);
            if (string.IsNullOrEmpty(storyInfo.jsonFilename)) return;
            var json = File.ReadAllText(storyInfo.jsonFilename, EncodingTypeTools.GetEncoding(prefs.encodingType));
            if (string.IsNullOrEmpty(json)) throw new Exception("Unable to read Twine JSON from file.");
            var story = JsonUtility.FromJson<TwineStory>(json);
            if (story == null) throw new Exception("Unable to parse JSON.");
            twineImporter.ConvertStoryToConversation(database, template, story, storyInfo.actorID, storyInfo.conversantID, storyInfo.splitPipes, prefs.useTwineNodePositions);
        }

        #endregion

    }
}
#endif
