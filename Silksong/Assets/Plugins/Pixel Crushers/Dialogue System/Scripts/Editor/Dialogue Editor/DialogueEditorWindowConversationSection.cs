// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Conversations tab. If the user
    /// has selected the node editor (default), it uses the node editor part. Otherwise
    /// it uses the outline-style dialogue tree part.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private const int NoID = -1;

        [SerializeField]
        public bool showNodeEditor = true;

        private Conversation _currentConversation = null;
        [SerializeField]
        private int currentConversationID;
        private Conversation currentConversation
        {
            get
            {
                return _currentConversation;
            }
            set
            {
                _currentConversation = value;
                if (value != null) currentConversationID = value.id;
            }
        }

        private bool conversationFieldsFoldout = false;
        private Field actorField = null;
        private Field conversantField = null;
        private int actorID = NoID;
        private int conversantID = NoID;
        private bool areParticipantsValid = false;
        private DialogueEntry startEntry = null;
        private ReorderableList conversationReorderableList = null;
        private HashSet<Conversation> conversationOutlineSelections = new HashSet<Conversation>();

        private void SetCurrentConversation(Conversation conversation)
        {
            if (verboseDebug) Debug.Log("<color=magenta>Set current conversation to ID=" + ((conversation != null) ? conversation.id : -1) + "</color>");
            ClearActorInfoCaches();
            if (conversation != null && conversation.id != currentConversationID) ResetCurrentEntryID();
            currentConversation = conversation;
            if (currentConversation != null)
            {
                canvasScrollPosition = currentConversation.canvasScrollPosition;
                _zoom = currentConversation.canvasZoom;
            }
        }

        private void SetCurrentConversationByID()
        {
            if (verboseDebug) Debug.Log("<color=magenta>Set conversation ID to " + currentConversationID + "</color>");
            conversationTitles = null;
            OpenConversation(database.GetConversation(currentConversationID));
            if (toolbar.Current == Toolbar.Tab.Conversations && Selection.activeObject == database)
            {
                SetCurrentEntryByID();
            }
            else
            {
                currentEntry = null;
            }
        }

        public void RefreshConversation()
        {
            if (currentConversation == null)
            {
                ResetConversationSection();
            }
            else
            {
                OpenConversation(currentConversation);
            }
        }

        private void SelectDialogueEntry(int conversationID, int dialogueEntryID)
        {
            if (database == null) return;
            toolbar.Current = Toolbar.Tab.Conversations;
            var conversation = database.GetConversation(conversationID);
            if (conversation == null) return;
            OpenConversation(conversation);
            var entry = conversation.GetDialogueEntry(dialogueEntryID);
            if (entry == null) return;
            SetCurrentEntry(entry);
            ResetNodeEditorConversationList();
            dialogueTreeFoldout = true;
            InitializeDialogueTree();
        }

        private void ResetConversationSection()
        {
            SetCurrentConversation(null);
            conversationFieldsFoldout = false;
            actorField = null;
            conversantField = null;
            areParticipantsValid = false;
            startEntry = null;
            selectedLink = null;
            actorNamesByID.Clear();
            ResetDialogueTreeSection();
            ResetConversationNodeSection();
        }

        private void OpenConversation(Conversation conversation)
        {
            ResetConversationSection();
            SetCurrentConversation(conversation);
            startEntry = GetOrCreateFirstDialogueEntry();
            CheckNodeWidths();
            if (showNodeEditor) CheckNodeArrangement();
        }

        private void OpenPreviousConversation()
        {
            if (database == null) return;
            Conversation conversationToOpen = (database.conversations.Count > 0) ? database.conversations[0] : null;
            if (currentConversation != null)
            {
                conversationIndex = database.conversations.IndexOf(currentConversation);
                conversationIndex = (conversationIndex == 0) ? (database.conversations.Count - 1) : (conversationIndex - 1);
                conversationToOpen = database.conversations[conversationIndex];
            }
            OpenConversation(conversationToOpen);
        }

        private void OpenNextConversation()
        {
            if (database == null) return;
            Conversation conversationToOpen = (database.conversations.Count > 0) ? database.conversations[0] : null;
            if (currentConversation != null)
            {
                conversationIndex = database.conversations.IndexOf(currentConversation);
                conversationIndex = (conversationIndex == database.conversations.Count - 1) ? 0 : (conversationIndex + 1);
                conversationToOpen = database.conversations[conversationIndex];
            }
            OpenConversation(conversationToOpen);
        }

        private void DrawConversationSection()
        {
            if (showNodeEditor)
            {
                DrawConversationSectionNodeStyle();
            }
            else
            {
                DrawConversationSectionOutlineStyle();
            }
        }

        private void DrawConversationSectionOutlineStyle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Conversations", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            DrawOutlineEditorMenu();
            EditorGUILayout.EndHorizontal();
            DrawConversations();
        }

        private const string RIConversationReferenceBackend = "RelationsInspector.Backend.DialogueSystem.ConversationReferenceBackend";
        private const string RIConversationLinkBackend = "RelationsInspector.Backend.DialogueSystem.ConversationLinkBackend";
        private const string RIDialogueEntryLinkBackend = "RelationsInspector.Backend.DialogueSystem.DialogueEntryLinkBackend";

        private void DrawOutlineEditorMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Conversation"), false, AddNewConversationToOutlineEditor);
                if (currentConversation != null)
                {
                    menu.AddItem(new GUIContent("Duplicate Conversation"), false, CopyConversationCallback, null);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Duplicate Conversation"));
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
                    menu.AddItem(new GUIContent("Split Pipes Into Entries"), false, SplitPipesIntoEntries, null);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Split Pipes Into Entries"));
                }
                menu.AddItem(new GUIContent("Sort/By Title"), false, SortConversationsByTitle);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortConversationsByID);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/This Conversation"), false, ConfirmReorderIDsThisConversation);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/All Conversations"), false, ConfirmReorderIDsAllConversations);
                menu.AddItem(new GUIContent("Sort/Reorder IDs/Depth First Reordering"), reorderIDsDepthFirst, () => { reorderIDsDepthFirst = !reorderIDsDepthFirst; });
                menu.AddItem(new GUIContent("Show/Prefer Titles For 'Links To' Menus"), prefs.preferTitlesForLinksTo, TogglePreferTitlesForLinksTo);
                menu.AddItem(new GUIContent("Search Bar"), isSearchBarOpen, ToggleDialogueTreeSearchBar);
                menu.AddItem(new GUIContent("Nodes"), false, ActivateNodeEditorMode);
                if (currentConversation == null)
                {
                    menu.AddDisabledItem(new GUIContent("Refresh"));
                }
                else
                {
                    menu.AddItem(new GUIContent("Refresh"), false, RefreshConversation);
                }
                AddRelationsInspectorMenuItems(menu);
                menu.ShowAsContext();
            }
        }

        private void TogglePreferTitlesForLinksTo()
        {
            prefs.preferTitlesForLinksTo = !prefs.preferTitlesForLinksTo;
            linkToDestinationsFromEntry = null;
        }

        private void AddRelationsInspectorMenuItems(GenericMenu menu)
        {
            if (RelationsInspectorLink.RIisAvailable)
            {
                if (RelationsInspectorLink.HasBackend(RIConversationReferenceBackend))
                {
                    menu.AddItem(new GUIContent("RelationsInspector/Conversation References"), false, OpenRelationsInspectorConversationReferenceBackend);
                }
                if (RelationsInspectorLink.HasBackend(RIConversationLinkBackend))
                {
                    menu.AddItem(new GUIContent("RelationsInspector/Conversation Links"), false, OpenRelationsInspectorConversationLinkBackend);
                }
                //--- Not working yet:
                //if (RelationsInspectorLink.HasBackend(RIDialogueEntryLinkBackend))
                //{
                //    menu.AddItem(new GUIContent("RelationsInspector/Dialogue Entry Links"), false, OpenRelationsInspectorDialogueEntryLinkBackend);
                //}
            }
        }

        private void AddNewConversationToOutlineEditor()
        {
            AddNewConversation();
        }

        private Conversation AddNewConversation()
        {
            Undo.RegisterCompleteObjectUndo(database, "New Conversation");
            Conversation newConversation = AddNewAsset<Conversation>(database.conversations);

            // Use same actors as previous conversation:
            if (currentConversation != null)
            {
                newConversation.ActorID = currentConversation.ActorID;
                newConversation.ConversantID = currentConversation.ConversantID;
                var startEntry = newConversation.GetFirstDialogueEntry();
                if (startEntry != null)
                {
                    startEntry.ActorID = currentConversation.ActorID;
                    startEntry.ConversantID = currentConversation.ConversantID;
                }
            }

            SetDatabaseDirty("Add New Conversation");

            if (newConversation != null) OpenConversation(newConversation);
            return newConversation;
        }

        private void SortConversationsByTitle()
        {
            database.conversations.Sort((x, y) => x.Title.CompareTo(y.Title));
            ResetNodeEditorConversationList();
            SetDatabaseDirty("Sort Conversations by Title");
        }

        private void SortConversationsByID()
        {
            database.conversations.Sort((x, y) => x.id.CompareTo(y.id));
            ResetNodeEditorConversationList();
            SetDatabaseDirty("Sort Conversation by ID");
        }

        private void OpenRelationsInspectorConversationLinkBackend()
        {
            RelationsInspectorLink.ResetTargets(new object[] { database }, RIConversationLinkBackend);
        }

        private void OpenRelationsInspectorConversationReferenceBackend()
        {
            RelationsInspectorLink.ResetTargets(new object[] { database }, RIConversationReferenceBackend);
        }

        private void OpenRelationsInspectorDialogueEntryLinkBackend()
        {
            RelationsInspectorLink.ResetTargets(new object[] { currentConversation }, RIDialogueEntryLinkBackend);
        }

        private void DrawConversations()
        {
            if (conversationReorderableList == null)
            {
                conversationReorderableList = new ReorderableList(database.conversations, typeof(Conversation), true, true, true, true);
                conversationReorderableList.drawHeaderCallback = DrawConversationListHeader;
                conversationReorderableList.drawElementCallback = DrawConversationListElement;
                //conversationReorderableList.drawElementBackgroundCallback = DrawConversationListElementBackground;
                conversationReorderableList.onAddCallback = OnConversationListAdd;
                conversationReorderableList.onRemoveCallback = OnConversationListRemove;
                conversationReorderableList.onSelectCallback = OnConversationListSelect;
                //conversationReorderableList.onReorderCallback = OnConversationListReorder;
                conversationReorderableList.onReorderCallbackWithDetails = OnConversationListReorderWithDetails; // Unity 2018+
            }
            conversationReorderableList.DoLayoutList();
        }

        private void DrawConversationListHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 32f, rect.y, rect.width, rect.height), "Title");
            float buttonWidth = 128f;
            EditorGUI.BeginDisabledGroup(!(database != null && conversationOutlineSelections.Count < database.conversations.Count));
            if (GUI.Button(new Rect(rect.x + rect.width - 2 * buttonWidth, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "Select All"))
            {
                database.conversations.ForEach(x => conversationOutlineSelections.Add(x));
                Repaint();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(database == null || conversationOutlineSelections.Count == 0);
            if (GUI.Button(new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "Deselect All"))
            {
                conversationOutlineSelections.Clear();
                Repaint();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawConversationListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.conversations.Count)) return;
            var nameControl = "ConversationTitle" + index;
            var conversation = database.conversations[index];
            var conversationTitle = conversation.Title;

            float checkboxWidth = 16f;
            bool selected = conversationOutlineSelections.Contains(conversation);
            EditorGUI.BeginChangeCheck();
            selected = EditorGUI.Toggle(new Rect(rect.x, rect.y, checkboxWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, selected);
            if (EditorGUI.EndChangeCheck())
            {
                if (selected) conversationOutlineSelections.Add(conversation);
                else conversationOutlineSelections.Remove(conversation);
            }

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(nameControl);
            conversationTitle = EditorGUI.TextField(new Rect(rect.x + checkboxWidth, rect.y, rect.width - checkboxWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, conversationTitle);
            if (EditorGUI.EndChangeCheck()) conversation.Title = conversationTitle;
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl))
            {
                inspectorSelection = conversation;
                if (conversation != currentConversation) OpenConversation(conversation);
            }
        }

        //private void DrawConversationListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        //{
        //    if (!(0 <= index && index < database.conversations.Count)) return;
        //    var conversation = database.conversations[index];
        //    ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
        //}

        private void OnConversationListAdd(ReorderableList list)
        {
            AddNewConversation();
        }

        private void OnConversationListRemove(ReorderableList list)
        {
            if (conversationOutlineSelections.Count > 1)
            {
                if (EditorUtility.DisplayDialog("Delete Multiple Conversations", "***WARNING:***\nMultiple conversations selected.\n\nDelete " + conversationOutlineSelections.Count + " conversations?", "OK", "Cancel"))
                {
                    DeleteConversationOutlineSelections();
                }
            }

            if (!(0 <= list.index && list.index < database.conversations.Count)) return;
            var conversation = database.conversations[list.index];
            if (conversation == null) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(conversation)), "Are you sure you want to delete this conversation?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else
                {
                    var nextConversation = (list.index < list.count) ? database.conversations[list.index] : (list.count > 0) ? database.conversations[list.count - 1] : null;
                    if (nextConversation != null)
                    {
                        OpenConversation(nextConversation);
                    }
                    else
                    {
                        currentConversation = null;
                        inspectorSelection = null;
                    }
                }
                SetDatabaseDirty("Remove Conversation");
            }
        }

        private void DeleteConversationOutlineSelections()
        {
            database.conversations.RemoveAll(x => conversationOutlineSelections.Contains(x));
            conversationOutlineSelections.Clear();
            SetDatabaseDirty("Remove Conversations");
        }

        private void OnConversationListReorderWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            if (conversationOutlineSelections.Count > 1)
            {
                var movedConversation = database.conversations[newIndex]; // This one was selected, so it already moved.
                var selectedIndex = database.conversations.IndexOf(movedConversation);

                if (newIndex > oldIndex) // Moved down, so index of selected one will change:
                {
                    var numSelectedAboveMoved = 0;
                    for (int i = 0; i < selectedIndex; i++)
                    {
                        if (conversationOutlineSelections.Contains(database.conversations[i])) numSelectedAboveMoved++;
                    }
                    selectedIndex -= numSelectedAboveMoved;
                }

                // Move other selected conversations:
                var conversationsToMove = database.conversations.FindAll(x => conversationOutlineSelections.Contains(x) && x != movedConversation);
                database.conversations.RemoveAll(x => conversationsToMove.Contains(x));
                var nextIndex = Mathf.Clamp(selectedIndex + 1, 0, database.conversations.Count);
                for (int i = conversationsToMove.Count - 1; i >= 0; i--)
                {
                    database.conversations.Insert(nextIndex, conversationsToMove[i]);
                }
            }
            SetDatabaseDirty("Reorder Conversations");
        }

        private void OnConversationListSelect(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.conversations.Count)) return;
            var conversation = database.conversations[list.index];
            if (conversation != currentConversation) OpenConversation(conversation);
            inspectorSelection = conversation;
        }

        public void DrawConversationOutline()
        {
            if (currentConversation == null) return;
            EditorWindowTools.StartIndentedSection();
            EditorGUILayout.BeginVertical("HelpBox");
            DrawConversationProperties();
            DrawConversationFieldsFoldout();
            DrawDialogueTreeFoldout();
            EditorGUILayout.EndVertical();
            EditorWindowTools.EndIndentedSection();
        }

        public bool DrawConversationProperties()
        {
            if (currentConversation == null) return false;
            EditorGUI.BeginDisabledGroup(true); // Don't let user modify ID. Breaks things way more often than not.
            int newID = EditorGUILayout.IntField(new GUIContent("ID", "Internal ID. Change at your own risk."), currentConversation.id);
            EditorGUI.EndDisabledGroup();
            if (newID != currentConversation.id) SetNewConversationID(newID);

            bool changed = false;

            string newTitle = EditorGUILayout.TextField(new GUIContent("Title", "Conversation triggers reference conversations by this."), currentConversation.Title);
            if (!string.Equals(newTitle, currentConversation.Title))
            {
                currentConversation.Title = RemoveLeadingSlashes(newTitle); ;
                changed = true;
                SetDatabaseDirty("Change Conversation Title");
            }

            EditorGUILayout.HelpBox("Tip: You can organize conversations into submenus by using forward slashes ( / ) in conversation titles.", MessageType.Info);

            var description = Field.Lookup(currentConversation.fields, "Description");
            if (description != null)
            {
                EditorGUILayout.LabelField("Description");
                description.value = EditorGUILayout.TextArea(description.value);
            }

            actorField = DrawConversationParticipant(new GUIContent("Actor", "Primary actor, usually the PC."), actorField);
            conversantField = DrawConversationParticipant(new GUIContent("Conversant", "Other actor, usually an NPC."), conversantField);

            DrawOverrideSettings(currentConversation.overrideSettings);

            DrawOtherConversationPrimaryFields();

            if (customDrawAssetInspector != null)
            {
                customDrawAssetInspector(database, currentConversation);
            }

            return changed;
        }

        private string RemoveLeadingSlashes(string s)
        {
            int safeguard = 0; // Disallow leading forward slashes.
            while (s.StartsWith("/") && safeguard < 99)
            {
                safeguard++;
                s = s.Remove(0, 1);
            }
            return s;
        }

        private static List<string> conversationBuiltInFieldTitles = new List<string>(new string[] { "Title", "Description", "Actor", "Conversant" });

        private void DrawOtherConversationPrimaryFields()
        {
            if (currentConversation == null || currentConversation.fields == null || template.conversationPrimaryFieldTitles == null) return;
            foreach (var field in currentConversation.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.conversationPrimaryFieldTitles.Contains(field.title)) continue;
                if (conversationBuiltInFieldTitles.Contains(fieldTitle)) continue;
                DrawMainSectionField(field);
            }
        }

        private Field DrawConversationParticipant(GUIContent fieldTitle, Field participantField)
        {
            EditorGUILayout.BeginHorizontal();
            if (participantField == null) participantField = LookupConversationParticipantField(fieldTitle.text);

            string originalValue = participantField.value;
            DrawField(participantField, false, false);
            if (!string.Equals(originalValue, participantField.value))
            {
                int newParticipantID = Tools.StringToInt(participantField.value);
                UpdateConversationParticipant(Tools.StringToInt(originalValue), newParticipantID);
                startEntry = GetOrCreateFirstDialogueEntry();
                if (string.Equals(fieldTitle.text, "Actor"))
                {
                    if (startEntry != null) startEntry.ActorID = newParticipantID;
                    actorID = newParticipantID;
                }
                else
                {
                    if (startEntry != null) startEntry.ConversantID = newParticipantID;
                    conversantID = newParticipantID;
                }
                areParticipantsValid = false;
                ResetDialogueEntryText();
                SetDatabaseDirty("Change Participant");
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            return participantField;
        }

        private DialogueEntry GetOrCreateFirstDialogueEntry()
        {
            if (currentConversation == null) return null;
            if (currentConversation.ActorID == 0) { currentConversation.ActorID = GetFirstPCID(); SetDatabaseDirty("Set Default Conversation Actor"); }
            if (currentConversation.ConversantID == 0) { currentConversation.ConversantID = GetFirstNPCID(); SetDatabaseDirty("Set Default Conversation Conversant"); }
            DialogueEntry entry = currentConversation.GetFirstDialogueEntry();
            if (entry == null)
            {
                entry = CreateNewDialogueEntry("START");
                entry.ActorID = currentConversation.ActorID;
                entry.ConversantID = currentConversation.ConversantID;
                SetDatabaseDirty("Create START dialogue entry");
            }
            if (entry.ActorID == 0) { entry.ActorID = currentConversation.ActorID; SetDatabaseDirty("Set START Actor"); }
            if (entry.conversationID == 0) { entry.ConversantID = currentConversation.ConversantID; SetDatabaseDirty("Set START Conversant"); }
            return entry;
        }

        private int GetFirstPCID()
        {
            for (int i = 0; i < database.actors.Count; i++)
            {
                var actor = database.actors[i];
                if (actor.IsPlayer) return actor.id;
            }
            int id = GetHighestActorID() + 1;
            database.actors.Add(template.CreateActor(id, "Player", true));
            SetDatabaseDirty("Create Player Actor");
            return id;
        }

        private int GetFirstNPCID()
        {
            for (int i = 0; i < database.actors.Count; i++)
            {
                var actor = database.actors[i];
                if (!actor.IsPlayer) return actor.id;
            }
            int id = GetHighestActorID() + 1;
            database.actors.Add(template.CreateActor(id, "NPC", false));
            SetDatabaseDirty("Create NPC Actor");
            return id;
        }

        private int GetHighestActorID()
        {
            int highestID = 0;
            for (int i = 0; i < database.actors.Count; i++)
            {
                var actor = database.actors[i];
                highestID = Mathf.Max(highestID, actor.id);
            }
            return highestID;
        }

        private Field LookupConversationParticipantField(string fieldTitle)
        {
            Field participantField = Field.Lookup(currentConversation.fields, fieldTitle);
            if (participantField == null)
            {
                participantField = new Field(fieldTitle, NoIDString, FieldType.Actor);
                currentConversation.fields.Add(participantField);
                SetDatabaseDirty("Add Participant Field " + fieldTitle);
            }
            return participantField;
        }

        private void UpdateConversationParticipant(int oldID, int newID)
        {
            if (newID != oldID)
            {
                for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
                {
                    var entry = currentConversation.dialogueEntries[i];
                    if (entry.ActorID == oldID) entry.ActorID = newID;
                    if (entry.ConversantID == oldID) entry.ConversantID = newID;
                }
                ResetDialogueTreeCurrentEntryParticipants();
                ResetDialogueEntryText();
                SetDatabaseDirty("Update Conversation Participant");
            }
        }

        private void SetNewConversationID(int newID)
        {
            for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
            {
                var entry = currentConversation.dialogueEntries[i];
                for (int j = 0; j < entry.outgoingLinks.Count; j++)
                {
                    var link = entry.outgoingLinks[j];
                    if (link.originConversationID == currentConversation.id) link.originConversationID = newID;
                    if (link.destinationConversationID == currentConversation.id) link.destinationConversationID = newID;
                }
            }
            currentConversation.id = newID;
            SetDatabaseDirty("Change Conversation ID");
        }

        public void DrawConversationFieldsFoldout()
        {
            EditorGUILayout.BeginHorizontal();
            conversationFieldsFoldout = EditorGUILayout.Foldout(conversationFieldsFoldout, "All Fields");
            if (conversationFieldsFoldout)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Template", "Add any missing fields from the template."), EditorStyles.miniButton, GUILayout.Width(68)))
                {
                    ApplyTemplate(currentConversation.fields, GetTemplateFields(currentConversation));
                }
                if (GUILayout.Button(new GUIContent("Copy", "Copy these fields to the clipboard."), EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    CopyFields(currentConversation.fields);
                }
                EditorGUI.BeginDisabledGroup(clipboardFields == null);
                if (GUILayout.Button(new GUIContent("Paste", "Paste the clipboard into these fields."), EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    PasteFields(currentConversation.fields);
                }
                EditorGUI.EndDisabledGroup();
            }
            if (GUILayout.Button(new GUIContent(" ", "Add new field."), "OL Plus", GUILayout.Width(16)))
            {
                currentConversation.fields.Add(new Field());
                SetDatabaseDirty("Add Conversation Field");
            }
            EditorGUILayout.EndHorizontal();
            if (conversationFieldsFoldout)
            {
                if (actorID == NoID) actorID = currentConversation.ActorID;
                if (conversantID == NoID) conversantID = currentConversation.ConversantID;
                int oldActorID = actorID;
                int oldConversantID = conversantID;
                EditorGUI.BeginChangeCheck();
                DrawFieldsSection(currentConversation.fields);
                if (EditorGUI.EndChangeCheck())
                {
                    actorID = currentConversation.ActorID;
                    conversantID = currentConversation.ConversantID;
                    UpdateConversationParticipant(oldActorID, actorID);
                    UpdateConversationParticipant(oldConversantID, conversantID);
                }
            }
        }

        private bool AreConversationParticipantsValid()
        {
            if (!areParticipantsValid)
            {
                areParticipantsValid = (database.GetActor(currentConversation.ActorID) != null) && (database.GetActor(currentConversation.ConversantID) != null);
            }
            return areParticipantsValid;
        }

        private void DrawOverrideSettings(ConversationOverrideDisplaySettings settings)
        {
            settings.useOverrides = EditorGUILayout.ToggleLeft("Override Display Settings", settings.useOverrides);
            if (!settings.useOverrides) return;
            EditorWindowTools.StartIndentedSection();

            settings.overrideSubtitleSettings = EditorGUILayout.ToggleLeft("Subtitle Settings", settings.overrideSubtitleSettings);
            if (settings.overrideSubtitleSettings)
            {
                EditorWindowTools.StartIndentedSection();
                settings.showNPCSubtitlesDuringLine = EditorGUILayout.Toggle("Show NPC Subtitles During Line", settings.showNPCSubtitlesDuringLine);
                settings.showNPCSubtitlesWithResponses = EditorGUILayout.Toggle("Show NPC Subtitles With Responses", settings.showNPCSubtitlesWithResponses);
                settings.showPCSubtitlesDuringLine = EditorGUILayout.Toggle("Show PC Subtitles During Line", settings.showPCSubtitlesDuringLine);
                settings.skipPCSubtitleAfterResponseMenu = EditorGUILayout.Toggle("Skip PC Subtitle After Response Menu", settings.skipPCSubtitleAfterResponseMenu);
                settings.subtitleCharsPerSecond = EditorGUILayout.FloatField("Subtitle Chars Per Second", settings.subtitleCharsPerSecond);
                settings.minSubtitleSeconds = EditorGUILayout.FloatField("Min Subtitle Seconds", settings.minSubtitleSeconds);
                settings.continueButton = (DisplaySettings.SubtitleSettings.ContinueButtonMode)EditorGUILayout.EnumPopup("Continue Button", settings.continueButton);
                EditorWindowTools.EndIndentedSection();
            }

            settings.overrideSequenceSettings = EditorGUILayout.ToggleLeft("Sequence Settings", settings.overrideSequenceSettings);
            if (settings.overrideSequenceSettings)
            {
                EditorWindowTools.StartIndentedSection();
                settings.defaultSequence = EditorGUILayout.TextField("Default Sequence", settings.defaultSequence);
                settings.defaultPlayerSequence = EditorGUILayout.TextField("Default Player Sequence", settings.defaultPlayerSequence);
                settings.defaultResponseMenuSequence = EditorGUILayout.TextField("Default Response Menu Sequence", settings.defaultResponseMenuSequence);
                EditorWindowTools.EndIndentedSection();
            }

            settings.overrideInputSettings = EditorGUILayout.ToggleLeft("Input Settings", settings.overrideInputSettings);
            if (settings.overrideInputSettings)
            {
                settings.alwaysForceResponseMenu = EditorGUILayout.Toggle("Always Force Response Menu", settings.alwaysForceResponseMenu);
                settings.includeInvalidEntries = EditorGUILayout.Toggle("Include Invalid Entries", settings.includeInvalidEntries);
                settings.responseTimeout = EditorGUILayout.FloatField("Response Timeout", settings.responseTimeout);
                settings.emTagForOldResponses = (EmTag)EditorGUILayout.EnumPopup("Em Tag For Old Responses", settings.emTagForOldResponses);
                settings.emTagForInvalidResponses= (EmTag)EditorGUILayout.EnumPopup("Em Tag For Invalid Responses", settings.emTagForInvalidResponses);
                settings.cancelSubtitle.key = (KeyCode)EditorGUILayout.EnumPopup("Cancel Subtitle Key", settings.cancelSubtitle.key);
                settings.cancelSubtitle.buttonName = EditorGUILayout.TextField("Cancel Subtitle Button", settings.cancelSubtitle.buttonName);
                settings.cancelConversation.key = (KeyCode)EditorGUILayout.EnumPopup("Cancel Conversation Key", settings.cancelConversation.key);
                settings.cancelConversation.buttonName = EditorGUILayout.TextField("Cancel Conversation Button", settings.cancelConversation.buttonName);
            }

            EditorWindowTools.EndIndentedSection();
        }

    }

}
