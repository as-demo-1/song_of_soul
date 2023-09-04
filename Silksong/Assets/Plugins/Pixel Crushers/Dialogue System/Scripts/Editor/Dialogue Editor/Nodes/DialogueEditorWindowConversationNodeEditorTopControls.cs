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
            DrawAIGenerateConversationButton();
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
            conversationTitleFilter = EditorGUILayout.TextField(GUIContent.none, conversationTitleFilter, MoreEditorGuiUtility.ToolbarSearchTextFieldName);
            GUI.SetNextControlName("ConversationClearClearButton");
            if (GUILayout.Button("Clear", MoreEditorGuiUtility.ToolbarSearchCancelButtonName))
            {
                conversationTitleFilter = string.Empty;
                GUI.FocusControl("ConversationClearClearButton"); // Need to deselect text field to clear text field's display.
            }
            if (EditorGUI.EndChangeCheck())
            {
                ResetNodeEditorConversationList();
            }
            // [Contributed by Vladimir Beletsky: Pressing Return jumps to first conversation matching filter.
            if (Event.current.keyCode == KeyCode.Return &&
                !string.IsNullOrEmpty(conversationTitleFilter) &&
                GUI.GetNameOfFocusedControl() == "ConversationFilterTextField" &&
                database != null)
            {
                var filter = conversationTitleFilter.ToLower();
                foreach (var conversation in database.conversations)
                {
                    if (conversation == null) continue;
                    var title = conversation.Title.ToLower();
                    if (title.Contains(filter))
                    {
                        OpenConversation(conversation);
                        break;
                    }
                }
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
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Duplicate Conversation"));
                    menu.AddDisabledItem(new GUIContent("Delete Conversation"));
                }
                menu.AddItem(new GUIContent("Templates/New From Template/Built-In/Quest Conversation"), false, CreateQuestConversationFromTemplate);
                menu.AddItem(new GUIContent("Templates/New From Template/From Template JSON..."), false, CreateConversationFromTemplate);
                if (currentConversation != null)
                {
                    menu.AddItem(new GUIContent("Templates/Save Template JSON..."), false, SaveConversationTemplate);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Templates/Save Template JSON..."));
                }
                if (currentConversation != null)
                {
                    menu.AddItem(new GUIContent("Split Pipes Into Nodes/Process Conversation"), false, SplitPipesIntoEntries, null);
                    menu.AddItem(new GUIContent("Split Pipes Into Nodes/Trim Whitespace Around Pipes"), trimWhitespaceAroundPipes, ToggleTrimWhitespaceAroundPipes);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Split Pipes Into Nodes/Process Conversation"));
                    menu.AddDisabledItem(new GUIContent("Split Pipes Into Nodes/Trim Whitespace Around Pipes"));
                }
                menu.AddItem(new GUIContent("Sort/By Title"), false, SortConversationsByTitle);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortConversationsByID);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/This Conversation"), false, ConfirmReorderIDsThisConversation);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/All Conversations"), false, ConfirmReorderIDsAllConversations);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/Depth First Reordering"), reorderIDsDepthFirst, () => { reorderIDsDepthFirst = !reorderIDsDepthFirst; });
                menu.AddItem(new GUIContent("Show/Show All Actor Names"), prefs.showAllActorNames, ToggleShowAllActorNames);
                menu.AddItem(new GUIContent("Show/Show Non-Primary Actor Names"), prefs.showOtherActorNames, ToggleShowOtherActorNames);
                menu.AddItem(new GUIContent("Show/Show Actor Portraits"), prefs.showActorPortraits, ToggleShowActorPortraits);
                menu.AddItem(new GUIContent("Show/Show Descriptions"), prefs.showDescriptions, ToggleShowDescriptions);
                menu.AddItem(new GUIContent("Show/Show Full Text On Hover"), prefs.showFullTextOnHover, ToggleShowFullTextOnHover);
                menu.AddItem(new GUIContent("Show/Show Link Order On Arrows"), prefs.showLinkOrderOnConnectors, () => { prefs.showLinkOrderOnConnectors = !prefs.showLinkOrderOnConnectors; });
                menu.AddItem(new GUIContent("Show/Show End Node Markers"), prefs.showEndNodeMarkers, ToggleShowEndNodeMarkers);
                menu.AddItem(new GUIContent("Show/Show Node IDs"), prefs.showNodeIDs, ToggleShowNodeIDs);
                menu.AddItem(new GUIContent("Show/Show Titles Instead of Text"), prefs.showTitlesInsteadOfText, ToggleShowTitlesBeforeText);
                menu.AddItem(new GUIContent("Show/Show Primary Actors in Lower Right"), prefs.showParticipantNames, ToggleShowParticipantNames);
                menu.AddItem(new GUIContent("Show/Prefer Titles For 'Links To' Menus"), prefs.preferTitlesForLinksTo, TogglePreferTitlesForLinksTo);
                menu.AddItem(new GUIContent("Show/Node Width/1x"), canvasRectWidthMultiplier == 1, SetNodeWidthMultiplier, (int)1);
                menu.AddItem(new GUIContent("Show/Node Width/2x"), canvasRectWidthMultiplier == 2, SetNodeWidthMultiplier, (int)2);
                menu.AddItem(new GUIContent("Show/Node Width/3x"), canvasRectWidthMultiplier == 3, SetNodeWidthMultiplier, (int)3);
                menu.AddItem(new GUIContent("Show/Node Width/4x"), canvasRectWidthMultiplier == 4, SetNodeWidthMultiplier, (int)4);
                menu.AddItem(new GUIContent("Grid/No Snap"), prefs.snapToGridAmount < MinorGridLineWidth, SetSnapToGrid, 0f);
                menu.AddItem(new GUIContent("Grid/12 pixels"), Mathf.Approximately(12f, prefs.snapToGridAmount), SetSnapToGrid, 12f);
                menu.AddItem(new GUIContent("Grid/24 pixels"), Mathf.Approximately(24f, prefs.snapToGridAmount), SetSnapToGrid, 24f);
                menu.AddItem(new GUIContent("Grid/36 pixels"), Mathf.Approximately(36f, prefs.snapToGridAmount), SetSnapToGrid, 36f);
                menu.AddItem(new GUIContent("Grid/48 pixels"), Mathf.Approximately(48f, prefs.snapToGridAmount), SetSnapToGrid, 48f);
                menu.AddItem(new GUIContent("Grid/Snap All Nodes To Grid"), false, SnapAllNodesToGrid);
                menu.AddItem(new GUIContent("Search/Search Bar"), isSearchBarOpen, ToggleDialogueTreeSearchBar);
                menu.AddItem(new GUIContent("Search/Global Search and Replace..."), false, OpenGlobalSearchAndReplace);
                menu.AddItem(new GUIContent("Settings/Auto Arrange After Adding Node"), prefs.autoArrangeOnCreate, ToggleAutoArrangeOnCreate);
                menu.AddItem(new GUIContent("Settings/Add New Nodes to Right"), prefs.addNewNodesToRight, ToggleAddNewNodesToRight);
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
            prefs.showAllActorNames = !prefs.showAllActorNames;
            ResetDialogueEntryText();
        }

        private void ToggleShowOtherActorNames()
        {
            prefs.showOtherActorNames = !prefs.showOtherActorNames;
            ResetDialogueEntryText();
        }

        private void ToggleShowParticipantNames()
        {
            prefs.showParticipantNames = !prefs.showParticipantNames;
        }

        private void ToggleShowActorPortraits()
        {
            prefs.showActorPortraits = !prefs.showActorPortraits;
            ClearActorInfoCaches();
        }

        private void ToggleShowDescriptions()
        {
            prefs.showDescriptions = !prefs.showDescriptions;
            ClearActorInfoCaches();
        }

        private void ToggleShowFullTextOnHover()
        {
            prefs.showFullTextOnHover = !prefs.showFullTextOnHover;
        }

        private void ToggleShowEndNodeMarkers()
        {
            prefs.showEndNodeMarkers = !prefs.showEndNodeMarkers;
        }

        private void ToggleShowNodeIDs()
        {
            prefs.showNodeIDs = !prefs.showNodeIDs;
            dialogueEntryNodeText.Clear();
        }

        private void ToggleShowTitlesBeforeText()
        {
            prefs.showTitlesInsteadOfText = !prefs.showTitlesInsteadOfText;
            dialogueEntryNodeText.Clear();
        }

        private void ToggleAddNewNodesToRight()
        {
            prefs.addNewNodesToRight = !prefs.addNewNodesToRight;
        }

        private void ToggleAutoArrangeOnCreate()
        {
            prefs.autoArrangeOnCreate = !prefs.autoArrangeOnCreate;
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
            var activeConversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
            if (currentConversation == null || currentConversation.id != activeConversationID)
            {
                OpenConversation(database.GetConversation(activeConversationID));
            }
            if (currentConversation == null) return;
            var currentEntry = currentConversation.GetDialogueEntry(DialogueManager.currentConversationState.subtitle.dialogueEntry.id);
            if (currentEntry == null) return;
            canvasScrollPosition = new Vector2(Mathf.Max(0, currentEntry.canvasRect.x - ((position.width - currentEntry.canvasRect.width) / 2)), Mathf.Max(0, currentEntry.canvasRect.y - 8));
        }

        private void SetSnapToGrid(object data)
        {
            prefs.snapToGridAmount = (data == null || data.GetType() != typeof(float)) ? 0 : (float)data;
        }

    }

}