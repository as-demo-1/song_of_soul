using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the top
    /// controls for the conversation node editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private string[] conversationTitles = null;

        [SerializeField]
        private int conversationIndex = -1;

        private DialogueEntry nodeToDrag = null;

        [SerializeField]
        private float snapToGridAmount = 0;

        private bool hasStartedSnapToGrid = false;

        [SerializeField]
        private bool confirmDelete = true;

        [SerializeField]
        private bool trimWhitespaceAroundPipes = false;

        [SerializeField]
        private string conversationTitleFilter = string.Empty;

        [SerializeField]
        private List<string> prevConversationStack = new List<string>();

        [SerializeField]
        private List<string> nextConversationStack = new List<string>();

        private const int MaxConversationSelectionStack = 32;

        private void SetConversationDropdownIndex(int index)
        {
            conversationIndex = index;
        }

        private void ResetConversationNodeEditor()
        {
            conversationTitles = null;
            conversationIndex = -1;
            ResetConversationNodeSection();
        }

        private void ResetConversationNodeSection()
        {
            isMakingLink = false;
            multinodeSelection.nodes.Clear();
            currentHoveredEntry = null;
            currentHoverGUIContent = null;
            ValidateConversationMenuTitleIndex();
        }

        private void ValidateConversationMenuTitleIndex()
        {
            UpdateConversationTitles();
            if (database != null && conversationIndex >= database.conversations.Count) conversationIndex = -1;
            if (conversationIndex == -1 && currentConversation != null)
            {
                var currentTitle = currentConversation.Title;
                for (int i = 0; i < conversationTitles.Length; i++)
                {
                    if (conversationTitles[i] == currentTitle)
                    {
                        conversationIndex = i;
                        break;
                    }
                }
            }
        }

        private void SetShowNodeEditor(bool value)
        {
            showNodeEditor = value;
            EditorPrefs.SetBool(ShowNodeEditorKey, value);
        }

        private void ActivateOutlineMode()
        {
            SetShowNodeEditor(false);
        }

        private void ActivateNodeEditorMode()
        {
            SetShowNodeEditor(true);
            ResetNodeEditorConversationList();
            if (currentConversation != null) OpenConversation(currentConversation);
            isMakingLink = false;
        }

        private void ResetNodeEditorConversationList()
        {
            conversationTitles = GetConversationTitles();
            SetConversationDropdownIndex(GetCurrentConversationIndex());
        }

        private void DrawNodeEditorTopControls()
        {
            EditorGUILayout.BeginHorizontal();
            DrawPrevConversationButton();
            DrawNodeEditorConversationPopup();
            if (GUILayout.Button(new GUIContent("+", "Create a new conversation"), EditorStyles.miniButtonRight, GUILayout.Width(21)))
            {
                AddNewConversationToNodeEditor();
            }
            DrawNextConversationButton();
            DrawConversationFilter();
            DrawZoomSlider();
            DrawNodeEditorMenu();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPrevConversationButton()
        {
            EditorGUI.BeginDisabledGroup(prevConversationStack.Count == 0);
            if (GUILayout.Button(GUIContent.none, GUI.skin.GetStyle("AC LeftArrow"), GUILayout.Width(12), GUILayout.Height(16)))
            {
                GotoPrevConversation();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawNextConversationButton()
        {
            EditorGUI.BeginDisabledGroup(nextConversationStack.Count == 0);
            if (GUILayout.Button(GUIContent.none, GUI.skin.GetStyle("AC RightArrow"), GUILayout.Width(12), GUILayout.Height(16)))
            {
                GotoNextConversation();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawConversationFilter()
        {
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("ConversationFilterTextField");
            conversationTitleFilter = EditorGUILayout.TextField(GUIContent.none, conversationTitleFilter, "ToolbarSeachTextField");
            GUI.SetNextControlName("ConversationClearClearButton");
            if (GUILayout.Button("Clear", "ToolbarSeachCancelButton"))
            {
                conversationTitleFilter = string.Empty;
                GUI.FocusControl("ConversationClearClearButton"); // Need to deselect text field to clear text field's display.
            }
            if (EditorGUI.EndChangeCheck())
            {
                ResetNodeEditorConversationList();
            }
        }

        private void AddNewConversationToNodeEditor()
        {
            AddNewConversation();
            ActivateNodeEditorMode();
            inspectorSelection = currentConversation;
        }

        private bool IsConversationActive()
        {
            return Application.isPlaying && DialogueManager.instance != null && DialogueManager.isConversationActive;
        }

        private void DrawNodeEditorMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Home Position"), false, GotoCanvasHomePosition);
                if (currentConversation != null)
                {
                    menu.AddItem(new GUIContent("Center on START"), false, GotoStartNodePosition);
                    if (IsConversationActive())
                    {
                        menu.AddItem(new GUIContent("Center on Current Entry"), false, GotoCurrentRuntimeEntry);
                    }
                    menu.AddItem(new GUIContent("Conversation Properties"), false, InspectConversationProperties);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Center on START"));
                    menu.AddDisabledItem(new GUIContent("Conversation Properties"));
                }
                menu.AddItem(new GUIContent("New Conversation"), false, AddNewConversationToNodeEditor);
                if (currentConversation != null)
                {
                    menu.AddItem(new GUIContent("Duplicate Conversation"), false, CopyConversationCallback, null);
                    menu.AddItem(new GUIContent("Delete Conversation"), false, DeleteConversationCallback, null);
                    menu.AddItem(new GUIContent("Split Pipes Into Nodes/Process Conversation"), false, SplitPipesIntoEntries, null);
                    menu.AddItem(new GUIContent("Split Pipes Into Nodes/Trim Whitespace Around Pipes"), trimWhitespaceAroundPipes, ToggleTrimWhitespaceAroundPipes);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Copy Conversation"));
                    menu.AddDisabledItem(new GUIContent("Delete Conversation"));
                    menu.AddDisabledItem(new GUIContent("Split Pipes Into Nodes/Process Conversation"));
                    menu.AddDisabledItem(new GUIContent("Split Pipes Into Nodes/Trim Whitespace Around Pipes"));
                }
                menu.AddItem(new GUIContent("Sort/By Title"), false, SortConversationsByTitle);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortConversationsByID);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/This Conversation"), false, ConfirmReorderIDsThisConversation);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/All Conversations"), false, ConfirmReorderIDsAllConversations);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/Depth First Reordering"), reorderIDsDepthFirst, () => { reorderIDsDepthFirst = !reorderIDsDepthFirst; });
                menu.AddItem(new GUIContent("Show/Show All Actor Names"), showAllActorNames, ToggleShowAllActorNames);
                menu.AddItem(new GUIContent("Show/Show Non-Primary Actor Names"), showOtherActorNames, ToggleShowOtherActorNames);
                menu.AddItem(new GUIContent("Show/Show Actor Portraits"), showActorPortraits, ToggleShowActorPortraits);
                menu.AddItem(new GUIContent("Show/Show Descriptions"), showDescriptions, ToggleShowDescriptions);
                menu.AddItem(new GUIContent("Show/Show Full Text On Hover"), showFullTextOnHover, ToggleShowFullTextOnHover);
                menu.AddItem(new GUIContent("Show/Show End Node Markers"), showEndNodeMarkers, ToggleShowEndNodeMarkers);
                menu.AddItem(new GUIContent("Show/Show Node IDs"), showNodeIDs, ToggleShowNodeIDs);
                menu.AddItem(new GUIContent("Show/Show Titles Instead of Text"), showTitlesInsteadOfText, ToggleShowTitlesBeforeText);
                menu.AddItem(new GUIContent("Show/Show Primary Actors in Lower Right"), showParticipantNames, ToggleShowParticipantNames);
                menu.AddItem(new GUIContent("Show/Prefer Titles For 'Links To' Menus"), prefs.preferTitlesForLinksTo, TogglePreferTitlesForLinksTo);
                menu.AddItem(new GUIContent("Show/Node Width/1x"), canvasRectWidthMultiplier == 1, SetNodeWidthMultiplier, (int)1);
                menu.AddItem(new GUIContent("Show/Node Width/2x"), canvasRectWidthMultiplier == 2, SetNodeWidthMultiplier, (int)2);
                menu.AddItem(new GUIContent("Show/Node Width/3x"), canvasRectWidthMultiplier == 3, SetNodeWidthMultiplier, (int)3);
                menu.AddItem(new GUIContent("Show/Node Width/4x"), canvasRectWidthMultiplier == 4, SetNodeWidthMultiplier, (int)4);
                menu.AddItem(new GUIContent("Grid/No Snap"), snapToGridAmount < MinorGridLineWidth, SetSnapToGrid, 0f);
                menu.AddItem(new GUIContent("Grid/12 pixels"), Mathf.Approximately(12f, snapToGridAmount), SetSnapToGrid, 12f);
                menu.AddItem(new GUIContent("Grid/24 pixels"), Mathf.Approximately(24f, snapToGridAmount), SetSnapToGrid, 24f);
                menu.AddItem(new GUIContent("Grid/36 pixels"), Mathf.Approximately(36f, snapToGridAmount), SetSnapToGrid, 36f);
                menu.AddItem(new GUIContent("Grid/48 pixels"), Mathf.Approximately(48f, snapToGridAmount), SetSnapToGrid, 48f);
                menu.AddItem(new GUIContent("Grid/Snap All Nodes To Grid"), false, SnapAllNodesToGrid);
                menu.AddItem(new GUIContent("Search/Search Bar"), isSearchBarOpen, ToggleDialogueTreeSearchBar);
                menu.AddItem(new GUIContent("Search/Global Search and Replace..."), false, OpenGlobalSearchAndReplace);
                menu.AddItem(new GUIContent("Settings/Auto Arrange After Adding Node"), autoArrangeOnCreate, ToggleAutoArrangeOnCreate);
                menu.AddItem(new GUIContent("Settings/Add New Nodes to Right"), addNewNodesToRight, ToggleAddNewNodesToRight);
                menu.AddItem(new GUIContent("Settings/Confirm Node and Link Deletion"), confirmDelete, ToggleConfirmDelete);
                menu.AddItem(new GUIContent("Outline Mode"), false, ActivateOutlineMode);
                if (currentConversation == null)
                {
                    menu.AddDisabledItem(new GUIContent("Refresh"));
                }
                else
                {
                    menu.AddItem(new GUIContent("Refresh"), false, RefreshConversation);
                }
                AddRelationsInspectorMenuItems(menu);
                if (customNodeMenuSetup != null) customNodeMenuSetup(database, menu);
                menu.ShowAsContext();
            }
        }

        private void DrawZoomSlider()
        {
            _zoom = EditorGUILayout.Slider(_zoom, kZoomMin, kZoomMax, GUILayout.Width(200));
            zoomLocked = GUILayout.Toggle(zoomLocked, GUIContent.none, "IN LockButton");
        }

        private void ToggleShowAllActorNames()
        {
            showAllActorNames = !showAllActorNames;
            ResetDialogueEntryText();
        }

        private void ToggleShowOtherActorNames()
        {
            showOtherActorNames = !showOtherActorNames;
            ResetDialogueEntryText();
        }

        private void ToggleShowParticipantNames()
        {
            showParticipantNames = !showParticipantNames;
        }

        private void ToggleShowActorPortraits()
        {
            showActorPortraits = !showActorPortraits;
            ClearActorInfoCaches();
        }

        private void ToggleShowDescriptions()
        {
            showDescriptions = !showDescriptions;
            ClearActorInfoCaches();
        }

        private void ToggleShowFullTextOnHover()
        {
            showFullTextOnHover = !showFullTextOnHover;
        }

        private void ToggleShowEndNodeMarkers()
        {
            showEndNodeMarkers = !showEndNodeMarkers;
        }

        private void ToggleShowNodeIDs()
        {
            showNodeIDs = !showNodeIDs;
            dialogueEntryNodeText.Clear();
        }

        private void ToggleShowTitlesBeforeText()
        {
            showTitlesInsteadOfText = !showTitlesInsteadOfText;
            dialogueEntryNodeText.Clear();
        }

        private void ToggleAddNewNodesToRight()
        {
            addNewNodesToRight = !addNewNodesToRight;
        }

        private void ToggleAutoArrangeOnCreate()
        {
            autoArrangeOnCreate = !autoArrangeOnCreate;
        }

        private void ToggleConfirmDelete()
        {
            confirmDelete = !confirmDelete;
        }

        private void ToggleTrimWhitespaceAroundPipes()
        {
            trimWhitespaceAroundPipes = !trimWhitespaceAroundPipes;
        }

        private void GotoPrevConversation()
        {
            if (currentConversation != null)
            {
                nextConversationStack.Insert(0, currentConversation.Title);
            }
            string conversationTitle = prevConversationStack[prevConversationStack.Count - 1];
            prevConversationStack.RemoveAt(prevConversationStack.Count - 1);
            OpenConversation(database.GetConversation(conversationTitle));
            InitializeDialogueTree();
            inspectorSelection = currentConversation;
            conversationIndex = GetCurrentConversationIndex();
        }

        private void GotoNextConversation()
        {
            if (currentConversation != null)
            {
                prevConversationStack.Add(currentConversation.Title);
            }
            string conversationTitle = nextConversationStack[0];
            nextConversationStack.RemoveAt(0);
            OpenConversation(database.GetConversation(conversationTitle));
            InitializeDialogueTree();
            inspectorSelection = currentConversation;
            conversationIndex = GetCurrentConversationIndex();
        }

        private void DrawNodeEditorConversationPopup()
        {
            if (conversationTitles == null) conversationTitles = GetConversationTitles();
            if (conversationIndex == -1 && currentConversation != null)
            {
                conversationIndex = GetCurrentConversationIndex();
            }
            int newIndex = EditorGUILayout.Popup(conversationIndex, conversationTitles, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (newIndex != conversationIndex)
            {
                if (currentConversation != null)
                {
                    prevConversationStack.Add(currentConversation.Title);
                }
                nextConversationStack.Clear();
                SetConversationDropdownIndex(newIndex);
                OpenConversation(GetConversationByTitleIndex(conversationIndex));
                InitializeDialogueTree();
                inspectorSelection = currentConversation;
            }
        }

        private string[] GetConversationTitles()
        {
            int numDuplicates = 0;
            var titles = new List<string>();
            var titlesWithoutAmpersand = new List<string>();
            var useFilter = !string.IsNullOrEmpty(conversationTitleFilter);
            var lowercaseFilter = useFilter ? conversationTitleFilter.ToLower() : string.Empty;
            if (database != null)
            {
                foreach (var conversation in database.conversations)
                {
                    if (conversation == null) continue;
                    // Make sure titles will work with GUI popup menus:
                    var title = conversation.Title;
                    if (title == null) continue;
                    if (useFilter && !title.ToLower().Contains(lowercaseFilter)) continue;
                    if (title.StartsWith("/"))
                    {
                        title = "?" + title;
                        conversation.Title = title;
                    }
                    if (title.EndsWith("/"))
                    {
                        title += "?";
                        conversation.Title = title;
                    }
                    if (title.Contains("//"))
                    {
                        title = title.Replace("//", "/");
                        conversation.Title = title;
                    }
                    if (titles.Contains(title))
                    {
                        numDuplicates++;
                        title += " " + numDuplicates;
                        conversation.Title = title;
                    }
                    titles.Add(title);
                    titlesWithoutAmpersand.Add(title.Replace("&", "<AMPERSAND>"));
                }
            }
            return titlesWithoutAmpersand.ToArray();
        }

        private int GetCurrentConversationIndex()
        {
            if (currentConversation != null)
            {
                if (conversationTitles == null) conversationTitles = GetConversationTitles();
                for (int i = 0; i < conversationTitles.Length; i++)
                {
                    if (string.Equals(currentConversation.Title, conversationTitles[i])) return i;
                }
            }
            return -1;
        }

        private Conversation GetConversationByTitleIndex(int index)
        {
            if (conversationTitles == null) conversationTitles = GetConversationTitles();
            if (0 <= index && index < conversationTitles.Length)
            {
                return database.GetConversation(conversationTitles[index].Replace("<AMPERSAND>", "&"));
            }
            else
            {
                return null;
            }
        }

        public void UpdateConversationTitles()
        {
            conversationTitles = GetConversationTitles();
        }

        private void GotoStartNodePosition()
        {
            var startEntry = currentConversation.GetFirstDialogueEntry();
            if (startEntry == null) return;
            canvasScrollPosition = new Vector2(Mathf.Max(0, startEntry.canvasRect.x - ((position.width - startEntry.canvasRect.width) / 2)), Mathf.Max(0, startEntry.canvasRect.y - 8));
        }

        private void GotoCurrentRuntimeEntry()
        {
            if (!(Application.isPlaying && DialogueManager.isConversationActive)) return;
            if (currentConversation == null || !string.Equals(currentConversation.Title, DialogueManager.lastConversationStarted))
            {
                OpenConversation(database.GetConversation(DialogueManager.lastConversationStarted));
            }
            if (currentConversation == null) return;
            var currentEntry = currentConversation.GetDialogueEntry(DialogueManager.currentConversationState.subtitle.dialogueEntry.id);
            if (currentEntry == null) return;
            canvasScrollPosition = new Vector2(Mathf.Max(0, currentEntry.canvasRect.x - ((position.width - currentEntry.canvasRect.width) / 2)), Mathf.Max(0, currentEntry.canvasRect.y - 8));
        }

        private void SetSnapToGrid(object data)
        {
            snapToGridAmount = (data == null || data.GetType() != typeof(float)) ? 0 : (float)data;
        }

    }

}