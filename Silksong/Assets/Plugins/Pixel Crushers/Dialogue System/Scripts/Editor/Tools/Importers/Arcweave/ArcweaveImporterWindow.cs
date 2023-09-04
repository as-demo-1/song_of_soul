#if USE_ARCWEAVE

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

#region Prefs

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
        public int numPlayers = 1;
        public string globalVariables = "day, time";

        public string prefsPath;

        public string apiKey;
        public string projectHash;
    }

#endregion

    public class ArcweaveImporterWindow : AbstractConverterWindow<ArcweaveImporterWindowPrefs>
    {

#region Variables

        public static bool isOpen;

        public override string prefsKey { get { return "DialogueSystem.ArcweavePrefs"; } }

        protected static GUIContent ProjectPathLabel = new GUIContent("Project Folder", "Folder containing Arcweave JSON and assets. Should be inside your Unity project.");
        protected static GUIContent APIKeyLabel = new GUIContent("API Key (Team Only)", "API key for downloading Arcweave JSON from Arcweave web API. Only applies to Arcweave Team plans.");
        protected static GUIContent ProjectHashLabel = new GUIContent("Project Hash (Team Only)", "Project hash for downloading Arcweave JSON from Arcweave web API. Only applies to Arcweave Team plans. Find hash in URL of project in Arcweave web app.");
        protected static GUIContent DownloadJsonLabel = new GUIContent("Download JSON", "Download Arcweave project info using web API key above.");
        protected static GUIContent LoadJsonLabel = new GUIContent("Load JSON", "Load Arcweave project info from Project Folder.");
        protected static GUIContent SavePrefsLabel = new GUIContent("Save Prefs...", "Save import settings to JSON file.");
        protected static GUIContent LoadPrefsLabel = new GUIContent("Load Prefs...", "Load import settings from JSON file.");
        protected static GUIContent ClearPrefsLabel = new GUIContent("Clear Prefs...", "Clear import settings.");
        protected static GUIContent ImportPortraitsLabel = new GUIContent("Import Portraits", "Import portrait images from image files in Arcweave project folder.");
        protected static GUIContent ImportGuidsLabel = new GUIContent("Import Guids As Fields", "Add Guid field containing GUID for each object (element, component, etc.) imported from Arcweave project.");
        protected static GUIContent NumPlayersLabel = new GUIContent("# Player Actors", "Set to 1 for single player games. Increase for local multiplayer games. Will create player-specific versions of variables.");
        protected static GUIContent GlobalVariablesLabel = new GUIContent("Global Variables", "Comma-separated list of global variables. Arcscript/Lua code referencing global variables that aren't player-specific. Importer won't prepend 'Player#_' to front of variable name.");

        public ArcweaveImporterWindowPrefs importerPrefs { get { return prefs; } }

        protected ArcweaveImporter arcweaveImporter = new ArcweaveImporter();

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
            DrawNumPlayersSection();
            DrawDestinationSection();
            DrawConversionButtons();
        }

        protected override void ClearPrefs()
        {
            base.ClearPrefs();
            arcweaveImporter.Clear();
        }

        protected override void DrawSourceSection()
        {
            EditorGUILayout.BeginHorizontal();
            prefs.arcweaveProjectPath = EditorGUILayout.TextField(ProjectPathLabel, prefs.arcweaveProjectPath);
            if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.arcweaveProjectPath = EditorUtility.OpenFolderPanel("Arcweave Project Location", prefs.arcweaveProjectPath, "");
                prefs.arcweaveProjectPath = "Assets" + prefs.arcweaveProjectPath.Replace(Application.dataPath, string.Empty);
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            prefs.apiKey = EditorGUILayout.TextField(APIKeyLabel, prefs.apiKey);
            prefs.projectHash = EditorGUILayout.TextField(ProjectHashLabel, prefs.projectHash);
        }

        protected void DrawComponentTypeSection()
        {
            if (!arcweaveImporter.IsJsonLoaded()) return;
            prefs.componentsFoldout = EditorGUILayout.Foldout(prefs.componentsFoldout, "Components");
            if (!prefs.componentsFoldout) return;
            EditorGUI.indentLevel++;
            foreach (var kvp in arcweaveImporter.arcweaveProject.components)
            {
                string componentGuid = kvp.Key;
                ArcweaveComponent arcweaveComponent = kvp.Value;
                if (arcweaveComponent.root) continue;
                var componentType = prefs.playerComponentGuids.Contains(componentGuid) ? ArcweaveComponentType.Player
                    : prefs.npcComponentGuids.Contains(componentGuid) ? ArcweaveComponentType.NPC
                    : prefs.itemComponentGuids.Contains(componentGuid) ? ArcweaveComponentType.Item
                    : prefs.locationComponentGuids.Contains(componentGuid) ? ArcweaveComponentType.Location
                    : ArcweaveComponentType.Ignore;
                EditorGUI.BeginChangeCheck();
                componentType = (ArcweaveComponentType)EditorGUILayout.EnumPopup(arcweaveComponent.name, componentType);
                if (EditorGUI.EndChangeCheck())
                {
                    prefs.playerComponentGuids.Remove(componentGuid);
                    prefs.npcComponentGuids.Remove(componentGuid);
                    prefs.itemComponentGuids.Remove(componentGuid);
                    prefs.locationComponentGuids.Remove(componentGuid);
                    switch (componentType)
                    {
                        case ArcweaveComponentType.Player:
                            prefs.playerComponentGuids.Add(componentGuid);
                            break;
                        case ArcweaveComponentType.NPC:
                            prefs.npcComponentGuids.Add(componentGuid);
                            break;
                        case ArcweaveComponentType.Item:
                            prefs.itemComponentGuids.Add(componentGuid);
                            break;
                        case ArcweaveComponentType.Location:
                            prefs.locationComponentGuids.Add(componentGuid);
                            break;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        protected void DrawBoardTypeSection()
        {
            if (!arcweaveImporter.IsJsonLoaded()) return;
            prefs.boardsFoldout = EditorGUILayout.Foldout(prefs.boardsFoldout, "Boards");
            if (!prefs.boardsFoldout) return;
            EditorGUI.indentLevel++;
            foreach (var kvp in arcweaveImporter.leafBoards)
            {
                EditorGUILayout.BeginHorizontal();
                string boardGuid = kvp.Key;
                Board board = kvp.Value;

                // Draw board type popup:
                var conversationInfo = prefs.conversationInfo.Find(x => x.boardGuid == boardGuid);
                var boardType = (conversationInfo != null) ? ArcweaveBoardType.Conversation
                    : prefs.questBoardGuids.Contains(boardGuid) ? ArcweaveBoardType.Quest
                    : ArcweaveBoardType.Ignore;
                EditorGUI.BeginChangeCheck();
                boardType = (ArcweaveBoardType)EditorGUILayout.EnumPopup(board.name, boardType);
                if (EditorGUI.EndChangeCheck())
                {
                    prefs.conversationInfo.Remove(conversationInfo);
                    prefs.questBoardGuids.Remove(boardGuid);
                    switch (boardType)
                    {
                        case ArcweaveBoardType.Conversation:
                            conversationInfo = new ArcweaveConversationInfo(boardGuid);
                            prefs.conversationInfo.Add(conversationInfo);
                            break;
                        case ArcweaveBoardType.Quest:
                            prefs.questBoardGuids.Add(boardGuid);
                            break;
                    }
                }

                // Conversations - draw starting element popup:
                if (boardType == ArcweaveBoardType.Conversation)
                {

                    if (!arcweaveImporter.conversationElements.ContainsKey(boardGuid))
                    {
                        // Sort board's elements:
                        board.elements.Sort((a, b) => arcweaveImporter.elementNames[a].CompareTo(arcweaveImporter.elementNames[b]));

                        // Then make list of their names:
                        var elements = new List<string>();
                        foreach (var elementGuid in board.elements)
                        {
                            elements.Add(arcweaveImporter.elementNames[elementGuid]);
                        }
                        arcweaveImporter.conversationElements[boardGuid] = elements.ToArray();
                    }
                    conversationInfo.startIndex = EditorGUILayout.Popup(GUIContent.none, conversationInfo.startIndex, arcweaveImporter.conversationElements[boardGuid]);
                    conversationInfo.actorIndex = EditorGUILayout.Popup(GUIContent.none, conversationInfo.actorIndex, arcweaveImporter.componentNames);
                    conversationInfo.conversantIndex = EditorGUILayout.Popup(GUIContent.none, conversationInfo.conversantIndex, arcweaveImporter.componentNames);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        protected void DrawImportPortraitsSection()
        {
            if (!arcweaveImporter.IsJsonLoaded()) return;
            prefs.importPortraits = EditorGUILayout.Toggle(ImportPortraitsLabel, prefs.importPortraits);
        }

        protected void DrawImportGuidSection()
        {
            if (!arcweaveImporter.IsJsonLoaded()) return;
            prefs.importGuids = EditorGUILayout.Toggle(ImportGuidsLabel, prefs.importGuids);
        }

        protected void DrawNumPlayersSection()
        {
            if (!arcweaveImporter.IsJsonLoaded()) return;
            EditorGUI.BeginChangeCheck();
            prefs.numPlayers = EditorGUILayout.IntField(NumPlayersLabel, prefs.numPlayers);
            if (EditorGUI.EndChangeCheck())
            {
                prefs.numPlayers = Mathf.Clamp(prefs.numPlayers, 1, 64);
            };
            if (prefs.numPlayers > 1)
            {
                prefs.globalVariables = EditorGUILayout.TextField(GlobalVariablesLabel, prefs.globalVariables);
            }
        }

        protected override void DrawConversionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawLoadJsonButtons();
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
                        SetupArcweaveImporter();
                        arcweaveImporter.LoadJson();
                    }
                }
            }
        }

        protected void DrawLoadJsonButtons()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prefs.apiKey) || string.IsNullOrEmpty(prefs.projectHash));
            if (GUILayout.Button(DownloadJsonLabel, GUILayout.Width(110)))
            {
                if (DownloadJson())
                {
                    SetupArcweaveImporter();
                    arcweaveImporter.LoadJson();
                }
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(LoadJsonLabel, GUILayout.Width(100)))
            {
                SetupArcweaveImporter();
                arcweaveImporter.LoadJson();
            }
        }

#endregion

#region Convert

        protected void SetupArcweaveImporter()
        {
            arcweaveImporter.Setup(prefs.arcweaveProjectPath,
                string.Empty,
                prefs.questBoardGuids,
                prefs.conversationInfo,
                prefs.playerComponentGuids,
                prefs.npcComponentGuids,
                prefs.itemComponentGuids,
                prefs.locationComponentGuids,
                prefs.boardsFoldout,
                prefs.componentsFoldout,
                prefs.importPortraits,
                prefs.importGuids,
                prefs.numPlayers,
                prefs.globalVariables,
                prefs.merge,
                template ?? TemplateTools.LoadFromEditorPrefs());
        }

        protected bool DownloadJson()
        {
            try
            {
                var endPoint = "https://arcweave.com/api/" + prefs.projectHash + "/json";
                var request = new UnityWebRequest(endPoint, "GET");
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + prefs.apiKey);
                request.SetRequestHeader("Accept", "application/json");
                request.SendWebRequest();
                Debug.Log($"Arcweave Importer sending web request to {endPoint}...");

                // Wait for the request to finish
                var cancel = false;
                while (!request.isDone && !cancel)
                {
                    cancel = EditorUtility.DisplayCancelableProgressBar("Connecting to Arcweave", "Attempting to download latest Arcweave project data using Arcweave web API.", 0);
                }

                if (cancel)
                {
                    Debug.Log("Arcweave Importer cancelled web request.");
                    return false;
                }

                // Check if the request was successful
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Arcweave Importer web request failed: " + request.error);
                    return false;
                }

                // Save the response to a file
                var filePath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) +
                    prefs.arcweaveProjectPath + "/project_settings.json";
                System.IO.File.WriteAllText(filePath, request.downloadHandler.text);
                AssetDatabase.ImportAsset(prefs.arcweaveProjectPath + "/project_settings.json");
                Debug.Log("Arcweave Importer received web response and saved to " + filePath);
                return true;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public void LoadAndConvert()
        {
            SetupArcweaveImporter();
            arcweaveImporter.LoadJson();
            if (IsReadyToConvert())
            {
                Convert();
            }
        }

        protected override void Convert()
        {
            SetupArcweaveImporter();
            base.Convert();
        }

        protected override bool IsSourceAssigned()
        {
            return arcweaveImporter.IsJsonLoaded();
        }

        protected override void CopySourceToDialogueDatabase(DialogueDatabase database)
        {
            arcweaveImporter.CopySourceToDialogueDatabase(database);
        }

        protected override void TouchUpDialogueDatabase(DialogueDatabase database)
        {
            base.TouchUpDialogueDatabase(database);
            arcweaveImporter.TouchUpDialogueDatabase(database);
        }

#endregion

    }
}

#endif
