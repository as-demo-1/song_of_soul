// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the outline-style dialogue tree editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private readonly string[] falseConditionActionStrings = { "Block", "Passthrough" };
        private readonly string[] priorityStrings = { "Low", "Below Normal", "Normal", "Above Normal", "High" };
        private const float DialogueEntryIndent = 16;
        private const int MaxNodeTextLength = 26;
        private Rect sequenceRect;

        private class DialogueNode
        {
            public DialogueEntry entry;
            public Link originLink;
            public GUIStyle guiStyle;
            public float indent;
            public bool isEditable;
            public bool hasFoldout;
            public List<DialogueNode> children;

            public DialogueNode(DialogueEntry entry, Link originLink, GUIStyle guiStyle, float indent, bool isEditable, bool hasFoldout)
            {
                this.entry = entry;
                this.originLink = originLink;
                this.guiStyle = guiStyle;
                this.indent = indent;
                this.isEditable = isEditable;
                this.hasFoldout = hasFoldout;
                this.children = new List<DialogueNode>();
            }
        }

        private bool dialogueTreeFoldout = false;
        private bool orphansFoldout = false;
        private Dictionary<int, string> dialogueEntryText = new Dictionary<int, string>();
        private Dictionary<int, string> dialogueEntryNodeText = new Dictionary<int, string>();
        private Dictionary<int, GUIContent> dialogueEntryNodeDescription = new Dictionary<int, GUIContent>();
        private Dictionary<int, bool> dialogueEntryNodeHasSequence = new Dictionary<int, bool>();
        private Dictionary<int, bool> dialogueEntryFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> dialogueEntryHasEvent = new Dictionary<int, bool>();
        private DialogueNode dialogueTree = null;
        private List<DialogueNode> orphans = new List<DialogueNode>();
        private Field currentEntryActor = null;
        private Field currentEntryConversant = null;
        private bool entryEventFoldout = false;
        private bool entryFieldsFoldout = false;
        private SequenceSyntaxState sequenceSyntaxState = SequenceSyntaxState.Unchecked;
        private GUIContent[] linkToDestinations = new GUIContent[0];
        private DialogueEntry linkToDestinationsFromEntry = null;

        public static bool linkToDebug = false;

        private DialogueEntry _currentEntry = null;
        [SerializeField]
        private int currentEntryID = -1;
        private DialogueEntry currentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                _currentEntry = value;
                sequenceSyntaxState = SequenceSyntaxState.Unchecked;
                if (value != null)
                {
                    currentEntryID = value.id;
                    if (value.fields != null) BuildLanguageListFromFields(value.fields);
                }
                else
                {
                    CloseQuickDialogueTextEntry();
                }
                if (verboseDebug && value != null) Debug.Log("<color=magenta>Set current entry ID to " + currentEntryID + "</color>");
            }
        }

        private LuaConditionWizard luaConditionWizard = new LuaConditionWizard(null);
        private LuaScriptWizard luaScriptWizard = new LuaScriptWizard(null);

        private void SetCurrentEntryByID()
        {
            if (verboseDebug) Debug.Log("<color=magenta>Set current entry to ID " + currentEntryID + "</color>");
            var entry = (currentConversation != null) ? currentConversation.GetDialogueEntry(currentEntryID) : null;
            SetCurrentEntry(entry);
            ResetNodeEditorConversationList();
            dialogueTreeFoldout = true;
            InitializeDialogueTree();
        }

        private void ResetCurrentEntryID()
        {
            if (verboseDebug) Debug.Log("<color=magenta>Reset current entry ID</color>");
            currentEntryID = -1;

        }

        private void ResetDialogueTreeSection()
        {
            dialogueTreeFoldout = false;
            orphansFoldout = false;
            ResetDialogueEntryText();
            dialogueEntryFoldouts.Clear();
            ResetDialogueTree();
            currentEntry = null;
            ResetLuaWizards();
        }

        private void ResetDialogueEntryText()
        {
            dialogueEntryText.Clear();
            dialogueEntryNodeText.Clear();
            dialogueEntryNodeDescription.Clear();
            dialogueEntryHasEvent.Clear();
            dialogueEntryNodeHasSequence.Clear();
        }

        public void ResetDialogueEntryText(DialogueEntry entry)
        {
            if (entry == null) return;
            if (dialogueEntryText.ContainsKey(entry.id)) dialogueEntryText[entry.id] = null;
            if (dialogueEntryNodeText.ContainsKey(entry.id)) dialogueEntryNodeText[entry.id] = null;
            dialogueEntryHasEvent[entry.id] = FullCheckDoesDialogueEntryHaveEvent(entry);
        }

        private void ResetDialogueTreeCurrentEntryParticipants()
        {
            currentEntryActor = null;
            currentEntryConversant = null;
        }

        private void ResetCurrentEntry()
        {
            currentEntry = null;
            ResetConditionsWizard();
            ResetScriptWizard();
            ResetDialogueTreeCurrentEntryParticipants();
            ResetUnityEventSection();
        }

        public void ResetLuaWizards()
        {
            CheckLuaWizards();
            ResetConditionsWizard();
            ResetScriptWizard();
        }

        private void ResetConditionsWizard()
        {
            luaConditionWizard.ResetWizard();
        }

        private void ResetScriptWizard()
        {
            luaScriptWizard.ResetWizard();
        }

        private void CheckLuaWizards()
        {
            if (currentEntry == null) return;
            if (luaConditionWizard.IsOpen) currentEntry.conditionsString = luaConditionWizard.AcceptConditionsWizard(); //[WIP]
            if (luaScriptWizard.IsOpen) currentEntry.userScript = luaScriptWizard.AcceptScriptWizard();
        }

        private void DrawDialogueTreeFoldout()
        {
            CheckDialogueTreeGUIStyles();
            if (AreConversationParticipantsValid())
            {
                bool isDialogueTreeOpen = EditorGUILayout.Foldout(dialogueTreeFoldout, "Dialogue Tree");
                if (isDialogueTreeOpen && !dialogueTreeFoldout) InitializeDialogueTree();
                dialogueTreeFoldout = isDialogueTreeOpen;
                if (dialogueTreeFoldout) DrawDialogueTree();
            }
            else
            {
                EditorGUILayout.LabelField("Dialogue Tree: Assign Actor and Conversant first.");
            }
        }

        private void InitializeDialogueTree()
        {
            ValidateStartEntryID();
            ResetDialogueTree();
            BuildDialogueTree();
            ResetOrphanIDs();
        }

        private void ValidateStartEntryID()
        {
            if (startEntry == null) startEntry = (currentConversation != null) ? currentConversation.GetFirstDialogueEntry() : null;
            if (startEntry != null)
            {
                if (startEntry.conversationID != currentConversation.id)
                {
                    startEntry.conversationID = currentConversation.id;
                    SetDatabaseDirty("Check/Set START entry conversation ID");
                }
            }
        }

        private void ResetDialogueTree()
        {
            dialogueTree = null;
            orphans.Clear();
            ResetLanguageList();
        }

        private void BuildDialogueTree()
        {
            if (currentConversation == null) return;
            List<DialogueEntry> visited = new List<DialogueEntry>();
            dialogueTree = BuildDialogueNode(startEntry, null, 0, visited);
            RecordOrphans(visited);
            BuildLanguageListFromConversation();
        }

        private void BuildLanguageListFromConversation()
        {
            for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
            {
                var entry = currentConversation.dialogueEntries[i];
                BuildLanguageListFromFields(entry.fields);
            }
        }

        private DialogueNode BuildDialogueNode(DialogueEntry entry, Link originLink, int level, List<DialogueEntry> visited)
        {
            if (entry == null) return null;
            bool wasEntryAlreadyVisited = visited.Contains(entry);
            if (!wasEntryAlreadyVisited) visited.Add(entry);

            // Create this node:
            float indent = DialogueEntryIndent * level;
            bool isLeaf = (entry.outgoingLinks.Count == 0);
            bool hasFoldout = !(isLeaf || wasEntryAlreadyVisited);
            GUIStyle guiStyle = wasEntryAlreadyVisited ? grayGUIStyle
                : isLeaf ? GetDialogueEntryLeafStyle(entry) : GetDialogueEntryStyle(entry);
            DialogueNode node = new DialogueNode(entry, originLink, guiStyle, indent, !wasEntryAlreadyVisited, hasFoldout);
            if (!dialogueEntryFoldouts.ContainsKey(entry.id)) dialogueEntryFoldouts[entry.id] = true;

            // Add children:
            if (!wasEntryAlreadyVisited)
            {
                for (int i = 0; i < entry.outgoingLinks.Count; i++)
                {
                    var link = entry.outgoingLinks[i];
                    if (link.destinationConversationID == currentConversation.id) // Only show connection if within same conversation.
                    {
                        node.children.Add(BuildDialogueNode(currentConversation.GetDialogueEntry(link.destinationDialogueID), link, level + 1, visited));
                    }
                }
            }
            return node;
        }

        private void RecordOrphans(List<DialogueEntry> visited)
        {
            if (visited.Count < currentConversation.dialogueEntries.Count)
            {
                for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
                {
                    var entry = currentConversation.dialogueEntries[i];
                    if (!visited.Contains(entry))
                    {
                        orphans.Add(new DialogueNode(entry, null, GetDialogueEntryStyle(entry), 0, false, false));
                    }
                }
            }
        }

        private GUIStyle GetDialogueEntryStyle(DialogueEntry entry)
        {
            return ((entry != null) && database.IsPlayerID(entry.ActorID)) ? pcLineGUIStyle : npcLineGUIStyle;
        }

        private GUIStyle GetDialogueEntryLeafStyle(DialogueEntry entry)
        {
            return ((entry != null) && database.IsPlayerID(entry.ActorID)) ? pcLineLeafGUIStyle : npcLineLeafGUIStyle;
        }

        private GUIStyle GetLinkButtonStyle(DialogueEntry entry)
        {
            return ((database != null) && (entry != null) && database.IsPlayerID(entry.ActorID)) ? pcLinkButtonGUIStyle : npcLinkButtonGUIStyle;
        }

        private string GetDialogueEntryText(DialogueEntry entry)
        {
            if (entry == null) return string.Empty;
            if (!dialogueEntryText.ContainsKey(entry.id) || (dialogueEntryText[entry.id] == null))
            {
                dialogueEntryText[entry.id] = BuildDialogueEntryText(entry);
            }
            return dialogueEntryText[entry.id];
        }

        private string BuildDialogueEntryText(DialogueEntry entry)
        {
            string text = entry.currentMenuText;
            if (string.IsNullOrEmpty(text)) text = entry.currentDialogueText;
            if (string.IsNullOrEmpty(text)) text = "<" + entry.Title + ">";
            if (entry.isGroup) text = "{group} " + text;
            if (text.Contains("\n")) text = text.Replace("\n", string.Empty);
            string speaker = GetActorNameByID(entry.ActorID);
            text = string.Format("[{0}] {1}: {2}", entry.id, speaker, text);
            if (!showNodeEditor)
            { // Only show for Outline editor. For node editor, we draw small icons on node.
                if (!string.IsNullOrEmpty(entry.conditionsString)) text += " [condition]";
                if (!string.IsNullOrEmpty(entry.userScript)) text += " {script}";
            }
            if ((entry.outgoingLinks == null) || (entry.outgoingLinks.Count == 0)) text += " [END]";
            return text;
        }

        private string GetDialogueEntryNodeText(DialogueEntry entry)
        {
            if (entry == null) return string.Empty;
            string text;
            if (!dialogueEntryNodeText.TryGetValue(entry.id, out text) || text == null)
            {
                text = BuildDialogueEntryNodeText(entry);
                if (text == null) text = string.Empty;
                dialogueEntryNodeText[entry.id] = text;
            }
            return text;
        }

        private string BuildDialogueEntryNodeText(DialogueEntry entry)
        {
            var text = string.Empty;
            if (showTitlesInsteadOfText)
            {
                var title = entry.Title;
                if (!(string.IsNullOrEmpty(title) || string.Equals(title, "New Dialogue Entry"))) text = title;
            }
            if (string.IsNullOrEmpty(text)) text = entry.currentMenuText;
            if (string.IsNullOrEmpty(text)) text = entry.currentDialogueText;
            if (string.IsNullOrEmpty(text)) text = "<" + entry.Title + ">";
            if (entry.isGroup) text = "{group} " + text;
            if (text.Contains("\n")) text = text.Replace("\n", string.Empty);
            int extraLength = 0;
            if (showAllActorNames)
            {
                string actorName = GetActorNameByID(entry.ActorID);
                if (actorName != null) extraLength = actorName.Length;
                text = string.Format("{0}:\n{1}", actorName, text);
            }
            else if (showOtherActorNames && entry.ActorID != currentConversation.ActorID && (entry.ActorID != currentConversation.ConversantID))
            {
                text = string.Format("{0}: {1}", GetActorNameByID(entry.ActorID), text);
            }
            if (showNodeIDs)
            {
                text = "[" + entry.id + "] " + text;
            }
            if (!showNodeEditor)
            {
                if (!string.IsNullOrEmpty(entry.conditionsString)) text += " [condition]";
                if (!string.IsNullOrEmpty(entry.userScript)) text += " {script}";
            }
            if ((entry.outgoingLinks == null) || (entry.outgoingLinks.Count == 0)) text += " [END]";
            return text.Substring(0, Mathf.Min(text.Length, canvasRectWidthMultiplier * MaxNodeTextLength + extraLength));
        }

        private GUIContent GetDialogueEntryNodeDescription(DialogueEntry entry)
        {
            if (entry == null) return null;
            GUIContent description;
            if (!dialogueEntryNodeDescription.TryGetValue(entry.id, out description) || description == null)
            {
                var descriptionText = Field.LookupValue(entry.fields, "Description");
                if (descriptionText == null) descriptionText = string.Empty;
                dialogueEntryNodeDescription[entry.id] = !string.IsNullOrEmpty(descriptionText) ? new GUIContent(descriptionText) : null;
            }
            return description;
        }

        private void ResetDialogueEntryNodeDescription(DialogueEntry entry)
        {
            dialogueEntryNodeDescription[entry.id] = null;
        }

        private bool DoesDialogueEntryHaveSequence(DialogueEntry entry)
        {
            if (entry == null) return false;
            bool value;
            if (!dialogueEntryNodeHasSequence.TryGetValue(entry.id, out value))
            {
                var sequence = entry.Sequence;
                value = !string.IsNullOrEmpty(sequence) && 
                    !(entry.id == 0 && (sequence == "None()" || sequence == "Continue()"));
                dialogueEntryNodeHasSequence[entry.id] = value;
            }
            return value;
        }

        private bool DoesDialogueEntryHaveEvent(DialogueEntry entry)
        {
            if (entry == null) return false;
            bool value;
            if (!dialogueEntryHasEvent.TryGetValue(entry.id, out value))
            {
                value = FullCheckDoesDialogueEntryHaveEvent(entry);
                dialogueEntryHasEvent[entry.id] = value;
            }
            return value;
        }

        private bool FullCheckDoesDialogueEntryHaveEvent(DialogueEntry entry)
        {
            if (entry == null) return false;
            if (entry.onExecute != null && entry.onExecute.GetPersistentEventCount() > 0) return true;
            if (!string.IsNullOrEmpty(entry.sceneEventGuid)) return true;
            return false;
        }

        private void DrawDialogueTree()
        {
            // Setup:
            Link linkToDelete = null;
            DialogueEntry entryToLinkFrom = null;
            DialogueEntry entryToLinkTo = null;
            bool linkToAnotherConversation = false;

            // Draw the tree:
            EditorWindowTools.StartIndentedSection();
            DrawDialogueNode(dialogueTree, null, ref linkToDelete, ref entryToLinkFrom, ref entryToLinkTo, ref linkToAnotherConversation);
            EditorWindowTools.EndIndentedSection();

            // Handle deletion:
            if (linkToDelete != null)
            {
                DeleteLink(linkToDelete);
                InitializeDialogueTree();
            }

            // Handle linking:
            if (entryToLinkFrom != null)
            {
                if (entryToLinkTo == null)
                {
                    if (linkToAnotherConversation)
                    {
                        LinkToAnotherConversation(entryToLinkFrom);
                    }
                    else
                    {
                        LinkToNewEntry(entryToLinkFrom);
                    }
                }
                else
                {
                    CreateLink(entryToLinkFrom, entryToLinkTo);
                }
                InitializeDialogueTree();
            }

            // Draw orphans:
            DrawOrphansFoldout();
        }

        private const float FoldoutRectWidth = 10;

        private void DrawDialogueNode(
            DialogueNode node,
            Link originLink,
            ref Link linkToDelete,
            ref DialogueEntry entryToLinkFrom,
            ref DialogueEntry entryToLinkTo,
            ref bool linkToAnotherConversation)
        {

            if (node == null) return;

            // Setup:
            bool deleted = false;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Empty, GUILayout.Width(node.indent));
            if (node.isEditable)
            {

                // Draw foldout if applicable:
                if (node.hasFoldout && node.entry != null)
                {
                    Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(FoldoutRectWidth));
                    dialogueEntryFoldouts[node.entry.id] = EditorGUI.Foldout(rect, dialogueEntryFoldouts[node.entry.id], string.Empty);
                }

                // Draw label/button to edit:
                if (GUILayout.Button(GetDialogueEntryText(node.entry), node.guiStyle))
                {
                    GUIUtility.keyboardControl = 0;
                    currentEntry = (currentEntry != node.entry) ? node.entry : null;
                    ResetLuaWizards();
                    ResetDialogueTreeCurrentEntryParticipants();
                }

                // Draw delete-node button:
                GUI.enabled = (originLink != null);
                deleted = GUILayout.Button(new GUIContent(" ", "Delete entry."), "OL Minus", GUILayout.Width(16));
                if (deleted) linkToDelete = originLink;
                GUI.enabled = true;
            }
            else
            {

                // Draw uneditable node:
                EditorGUILayout.LabelField(GetDialogueEntryText(node.entry), node.guiStyle);
                GUI.enabled = false;
                GUILayout.Button(" ", "OL Minus", GUILayout.Width(16));
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            // Draw contents if this is the currently-selected entry:
            if (!deleted && (node.entry == currentEntry) && node.isEditable)
            {
                DrawDialogueEntryContents(currentEntry, ref linkToDelete, ref entryToLinkFrom, ref entryToLinkTo, ref linkToAnotherConversation);
            }

            // Draw children:
            if (!deleted && node.hasFoldout && (node.entry != null) && dialogueEntryFoldouts[node.entry.id])
            {
                for (int i = 0; i < node.children.Count; i++)
                {
                    var child = node.children[i];
                    if (child != null)
                    {
                        DrawDialogueNode(child, child.originLink, ref linkToDelete, ref entryToLinkFrom, ref entryToLinkTo, ref linkToAnotherConversation);
                    }
                }
            }
        }

        private void DrawOrphansFoldout()
        {
            if (orphans.Count > 0)
            {
                orphansFoldout = EditorGUILayout.Foldout(orphansFoldout, "Orphan Entries");
                if (orphansFoldout)
                {
                    DialogueEntry entryToDelete = null;
                    DrawOrphans(ref entryToDelete);
                    if (entryToDelete != null)
                    {
                        currentConversation.dialogueEntries.Remove(entryToDelete);
                        InitializeDialogueTree();
                        SetDatabaseDirty("Delete Dialogue Entry");
                    }
                }
            }
        }

        private void DrawOrphans(ref DialogueEntry entryToDelete)
        {
            EditorWindowTools.StartIndentedSection();
            entryToDelete = null;
            for (int i = 0; i < orphans.Count; i++)
            {
                var node = orphans[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(GetDialogueEntryText(node.entry), node.guiStyle);
                bool deleted = GUILayout.Button(new GUIContent(" ", "Delete entry."), "OL Minus", GUILayout.Width(16));
                if (deleted) entryToDelete = node.entry;
                EditorGUILayout.EndHorizontal();
            }
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawDialogueEntryContents(
            DialogueEntry entry,
            ref Link linkToDelete,
            ref DialogueEntry entryToLinkFrom,
            ref DialogueEntry entryToLinkTo,
            ref bool linkToAnotherConversation)
        {
            bool changed = false;
            try
            {
                EditorGUILayout.BeginVertical("button");

                changed = DrawDialogueEntryFieldContents();

                // Links:
                changed = DrawDialogueEntryLinks(entry, ref linkToDelete, ref entryToLinkFrom, ref entryToLinkTo, ref linkToAnotherConversation) || changed;
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }

            if (changed)
            {
                ResetDialogueEntryText(entry);
                SetDatabaseDirty("Links Changed [2]");
            }
        }

        public bool DrawDialogueEntryFieldContents()
        {
            if (currentEntry == null) return false;

            EditorGUI.BeginChangeCheck();

            DialogueEntry entry = currentEntry;
            bool isStartEntry = (entry == startEntry) || (entry.id == 0);

            EditorGUI.BeginDisabledGroup(true); // Don't let user modify ID. Breaks things way more often than not.
            entry.id = StringToInt(EditorGUILayout.TextField(new GUIContent("ID", "Internal ID. Change at your own risk."), entry.id.ToString()), entry.id);
            EditorGUI.EndDisabledGroup();

            // Title:
            EditorGUI.BeginDisabledGroup(isStartEntry);
            entry.Title = EditorGUILayout.TextField(new GUIContent("Title", "Optional title for your reference only."), entry.Title);
            EditorGUI.EndDisabledGroup();

            if (isStartEntry)
            {
                EditorGUILayout.HelpBox("This is the START entry. In most cases, you should leave this entry alone and begin your conversation with its child entries.", MessageType.Warning);
            }

            // Description:
            var description = Field.Lookup(entry.fields, "Description");
            if (description != null)
            {
                EditorGUILayout.LabelField(new GUIContent("Description", "Description of this entry; notes for the author"));
                EditorGUI.BeginChangeCheck();
                description.value = EditorGUILayout.TextArea(description.value);
                if (EditorGUI.EndChangeCheck())
                {
                    ResetDialogueEntryNodeDescription(entry);
                }
            }

            // Actor & conversant:
            DrawDialogueEntryParticipants(entry);

            // Is this a group or regular entry:
            entry.isGroup = EditorGUILayout.Toggle(new GUIContent("Group", "Tick to organize children as a group."), entry.isGroup);

            if (!entry.isGroup)
            {
                EditorWindowTools.EditorGUILayoutBeginGroup();

                EditorGUI.BeginChangeCheck();

                // Menu text (including localized if defined in template):
                var menuText = entry.MenuText;
                var menuTextLabel = string.IsNullOrEmpty(menuText) ? "Menu Text" : ("Menu Text (" + menuText.Length + " chars)");
                EditorGUILayout.LabelField(new GUIContent(menuTextLabel, "Response menu text (e.g., short paraphrase). If blank, uses Dialogue Text"));
                entry.MenuText = EditorGUILayout.TextArea(menuText);
                DrawLocalizedVersions(entry.fields, "Menu Text {0}", false, FieldType.Text);

                // Dialogue text (including localized):
                var dialogueText = entry.DialogueText;
                var dialogueTextLabel = string.IsNullOrEmpty(dialogueText) ? "Dialogue Text" : ("Dialogue Text (" + dialogueText.Length + " chars)");
                EditorGUILayout.LabelField(new GUIContent(dialogueTextLabel, "Line spoken by actor. If blank, uses Menu Text."));
                entry.DialogueText = EditorGUILayout.TextArea(dialogueText);
                DrawLocalizedVersions(entry.fields, "{0}", true, FieldType.Localization);

                if (EditorGUI.EndChangeCheck())
                {
                    if (string.Equals(entry.Title, "New Dialogue Entry")) entry.Title = string.Empty;
                }

                EditorWindowTools.EditorGUILayoutEndGroup();

                // Sequence (including localized if defined):
                EditorWindowTools.EditorGUILayoutBeginGroup();

                var sequence = entry.Sequence;
                EditorGUI.BeginChangeCheck();
                sequence = SequenceEditorTools.DrawLayout(new GUIContent("Sequence", "Cutscene played when speaking this entry. If set, overrides Dialogue Manager's Default Sequence. Drag audio clips to add AudioWait() commands."), entry.Sequence, ref sequenceRect, ref sequenceSyntaxState);
                if (EditorGUI.EndChangeCheck())
                {
                    entry.Sequence = sequence;
                    dialogueEntryNodeHasSequence[entry.id] = !string.IsNullOrEmpty(sequence);
                }
                DrawLocalizedVersions(entry.fields, "Sequence {0}", false, FieldType.Text, true);

                // Response Menu Sequence:
                bool hasResponseMenuSequence = entry.HasResponseMenuSequence();
                if (hasResponseMenuSequence)
                {
                    EditorGUILayout.LabelField(new GUIContent("Response Menu Sequence", "Cutscene played during response menu following this entry."));
                    entry.ResponseMenuSequence = EditorGUILayout.TextArea(entry.ResponseMenuSequence);
                    DrawLocalizedVersions(entry.fields, "Response Menu Sequence {0}", false, FieldType.Text);
                }
                else
                {
                    hasResponseMenuSequence = EditorGUILayout.ToggleLeft(new GUIContent("Add Response Menu Sequence", "Tick to add a cutscene that plays during the response menu that follows this entry."), false);
                    if (hasResponseMenuSequence) entry.ResponseMenuSequence = string.Empty;
                }

                EditorWindowTools.EditorGUILayoutEndGroup();
            }

            // Conditions:
            EditorWindowTools.EditorGUILayoutBeginGroup();
            luaConditionWizard.database = database;
            entry.conditionsString = luaConditionWizard.Draw(new GUIContent("Conditions", "Optional Lua statement that must be true to use this entry."), entry.conditionsString);
            int falseConditionIndex = EditorGUILayout.Popup("False Condition Action", GetFalseConditionIndex(entry.falseConditionAction), falseConditionActionStrings);
            entry.falseConditionAction = falseConditionActionStrings[falseConditionIndex];
            EditorWindowTools.EditorGUILayoutEndGroup();

            // Script:
            EditorWindowTools.EditorGUILayoutBeginGroup();
            luaScriptWizard.database = database;
            entry.userScript = luaScriptWizard.Draw(new GUIContent("Script", "Optional Lua code to run when entry is spoken."), entry.userScript);
            EditorWindowTools.EditorGUILayoutEndGroup();

            // Other primary fields defined in template:
            DrawOtherDialogueEntryPrimaryFields(entry);

            // Events:
            entryEventFoldout = EditorGUILayout.Foldout(entryEventFoldout, "Events");
            if (entryEventFoldout) DrawUnityEvents();

            // Notes: (special handling to use TextArea)
            Field notes = Field.Lookup(entry.fields, "Notes");
            if (notes != null)
            {
                EditorGUILayout.LabelField("Notes");
                notes.value = EditorGUILayout.TextArea(notes.value);
            }

            // Custom inspector code hook:
            if (customDrawDialogueEntryInspector != null)
            {
                customDrawDialogueEntryInspector(database, entry);
            }

            // All Fields foldout:
            bool changed = EditorGUI.EndChangeCheck();
            try
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                entryFieldsFoldout = EditorGUILayout.Foldout(entryFieldsFoldout, "All Fields");
                if (entryFieldsFoldout)
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Template", "Add any missing fields from the template."), EditorStyles.miniButton, GUILayout.Width(68)))
                    {
                        ApplyDialogueEntryTemplate(entry.fields);
                    }
                    if (GUILayout.Button(new GUIContent("Copy", "Copy these fields to the clipboard."), EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        CopyFields(entry.fields);
                    }
                    EditorGUI.BeginDisabledGroup(clipboardFields == null);
                    if (GUILayout.Button(new GUIContent("Paste", "Paste the clipboard into these fields."), EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        PasteFields(entry.fields);
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button(new GUIContent(" ", "Add new field."), "OL Plus", GUILayout.Width(16))) entry.fields.Add(new Field());
                }
                EditorGUILayout.EndHorizontal();
                if (entryFieldsFoldout)
                {
                    DrawFieldsSection(entry.fields);
                }
            }
            finally
            {
                changed = EditorGUI.EndChangeCheck() || changed;
            }
            if (changed)
            {
                BuildLanguageListFromFields(entry.fields);
                SetDatabaseDirty("Dialogue Entry Fields Changed");
            }
            return changed;
        }

        public bool DrawMultinodeSelectionInspector()
        {
            // Multinode inspection is more restricted and doesn't show all the fields that 
            // single node does. When you change a field, all selected nodes get updated.

            if (multinodeSelection.nodes.Count == 0) return false;

            var changed = false;

            // Get a reference to first selected node for dropdowns:
            var entry = multinodeSelection.nodes[0];

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(new GUIContent("ID", "Internal ID. Change at your own risk."), "-");
            EditorGUI.EndDisabledGroup();

            // Title:
            var title = GetMultinodeSelectionFieldValue(DialogueSystemFields.Title);
            EditorGUI.BeginChangeCheck();
            title = EditorGUILayout.TextField(new GUIContent("Title", "Optional title for your reference only."), title);
            if (EditorGUI.EndChangeCheck()) { changed = true; SetMultinodeSelectionFieldValue(DialogueSystemFields.Title, title); }

            // Actor & conversant:
            EditorGUI.BeginChangeCheck();
            DrawDialogueEntryParticipants(entry);
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
                SetMultinodeSelectionFieldValue(DialogueSystemFields.Actor, Field.LookupValue(entry.fields, DialogueSystemFields.Actor));
                SetMultinodeSelectionFieldValue(DialogueSystemFields.Conversant, Field.LookupValue(entry.fields, DialogueSystemFields.Conversant));
            }

            if (!entry.isGroup)
            {
                // Sequence (including localized if defined):
                var sequence = GetMultinodeSelectionFieldValue(DialogueSystemFields.Sequence);
                EditorGUI.BeginChangeCheck();
                sequence = SequenceEditorTools.DrawLayout(new GUIContent("Sequence", "Cutscene played when speaking these entries. If set, overrides Dialogue Manager's Default Sequence. Drag audio clips to add AudioWait() commands."), sequence, ref sequenceRect, ref sequenceSyntaxState);
                if (EditorGUI.EndChangeCheck()) 
                { 
                    changed = true; 
                    SetMultinodeSelectionFieldValue(DialogueSystemFields.Sequence, sequence); 
                }

                // Response Menu Sequence:
                bool hasResponseMenuSequence = entry.HasResponseMenuSequence();
                if (hasResponseMenuSequence)
                {
                    var responseMenuSequence = GetMultinodeSelectionFieldValue("Response Menu Sequence");
                    EditorGUILayout.LabelField(new GUIContent("Response Menu Sequence", "Cutscene played during response menu following these entries."));
                    EditorGUI.BeginChangeCheck();
                    responseMenuSequence = EditorGUILayout.TextArea(responseMenuSequence);
                    if (EditorGUI.EndChangeCheck()) { changed = true; SetMultinodeSelectionFieldValue("Response Menu Sequence", responseMenuSequence); }
                }
            }

            // Conditions and Script:
            EditorGUI.BeginChangeCheck();
            int falseConditionIndex = EditorGUILayout.Popup("False Condition Action", GetFalseConditionIndex(entry.falseConditionAction), falseConditionActionStrings);
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
                for (int i = 0; i < multinodeSelection.nodes.Count; i++)
                {
                    multinodeSelection.nodes[i].falseConditionAction = falseConditionActionStrings[falseConditionIndex];
                }
            }

            // Conditions:
            luaConditionWizard.database = database;
            var conditionsString = multinodeSelection.nodes[0].conditionsString;
            for (int i = 1; i < multinodeSelection.nodes.Count; i++)
            {
                if (!string.Equals(multinodeSelection.nodes[i].conditionsString, conditionsString))
                {
                    conditionsString = string.Empty;
                }
            }
            EditorGUI.BeginChangeCheck();
            conditionsString = luaConditionWizard.Draw(new GUIContent("Conditions", "Optional Lua statement that must be true to use these entries."), conditionsString);
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
                for (int i = 0; i < multinodeSelection.nodes.Count; i++)
                {
                    multinodeSelection.nodes[i].conditionsString = conditionsString;
                }
            }

            // Script:
            luaScriptWizard.database = database;
            var userScript = multinodeSelection.nodes[0].userScript;
            for (int i = 1; i < multinodeSelection.nodes.Count; i++)
            {
                if (!string.Equals(multinodeSelection.nodes[i].userScript, userScript))
                {
                    userScript = string.Empty;
                }
            }
            EditorGUI.BeginChangeCheck();
            userScript = luaScriptWizard.Draw(new GUIContent("Script", "Optional Lua code to run when entries are spoken."), userScript);
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
                for (int i = 0; i < multinodeSelection.nodes.Count; i++)
                {
                    multinodeSelection.nodes[i].userScript = userScript;
                }
            }

            // All Fields:
            entryFieldsFoldout = EditorGUILayout.Foldout(entryFieldsFoldout, "All Fields");
            if (entryFieldsFoldout)
            {
                if (DrawMultinodeFieldsSection())
                {
                    changed = true;
                    BuildLanguageListFromFields(entry.fields);
                }
            }

            if (changed) SetDatabaseDirty("Dialogue Entry Fields Changed");
            return changed;
        }

        private string GetMultinodeSelectionFieldValue(string fieldName)
        {
            var value = "-";
            if (multinodeSelection.nodes.Count > 0)
            {
                value = Field.LookupValue(multinodeSelection.nodes[0].fields, fieldName);
                for (int i = 1; i < multinodeSelection.nodes.Count; i++)
                {
                    if (!string.Equals(Field.LookupValue(multinodeSelection.nodes[i].fields, fieldName), value))
                    {
                        return "-";
                    }
                }
            }
            return value;
        }

        private void SetMultinodeSelectionFieldValue(string fieldName, string value)
        {
            for (int i = 0; i < multinodeSelection.nodes.Count; i++)
            {
                Field.SetValue(multinodeSelection.nodes[i].fields, fieldName, value);
            }
        }

        private static List<string> dialogueEntryBuiltInFieldTitles = new List<string>(new string[] { "Title", "Description", "Actor", "Conversant", "Dialogue Text" });

        private void DrawOtherDialogueEntryPrimaryFields(DialogueEntry entry)
        {
            if (entry == null || entry.fields == null || template.dialogueEntryPrimaryFieldTitles == null) return;
            foreach (var field in entry.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.dialogueEntryPrimaryFieldTitles.Contains(field.title)) continue;
                if (dialogueEntryBuiltInFieldTitles.Contains(fieldTitle)) continue;
                if (fieldTitle.StartsWith("Menu Text") || fieldTitle.StartsWith("Sequence") || fieldTitle.StartsWith("Response Menu Sequence")) continue;
                DrawMainSectionField(field);
            }
        }

        private DialogueEntry serializedObjectCurrentEntry = null;
        private SerializedProperty onExecuteProperty = null;
        private DialogueSystemSceneEvents dialogueSystemSceneEvents = null;
        private SerializedObject dialogueSystemSceneEventsSerializedObject = null;

        private void ResetUnityEventSection()
        {
            serializedObjectCurrentEntry = null;
            onExecuteProperty = null;
        }

        private void DrawUnityEvents()
        {
            // Draw scene-independent OnExecute() event:
            EditorGUILayout.LabelField(new GUIContent("Scene-Independent Event", "This UnityEvent cannot point to scene objects. It can point to assets and prefabs."), EditorStyles.boldLabel);
            if (serializedObject == null)
            {
                EditorGUILayout.LabelField("Error displaying UnityEvent. Please report to developer.");
                return;
            }
            if (serializedObjectCurrentEntry != currentEntry)
            {
                serializedObject.Update();
                serializedObjectCurrentEntry = currentEntry;
                var conversationsProperty = serializedObject.FindProperty("conversations");
                if (conversationsProperty == null || !conversationsProperty.isArray) return;
                SerializedProperty conversationProperty = null;
                for (int i = 0; i < conversationsProperty.arraySize; i++)
                {
                    var sp = conversationsProperty.GetArrayElementAtIndex(i);
                    if (sp.FindPropertyRelative("id").intValue == currentConversation.id)
                    {
                        conversationProperty = sp;
                        break;
                    }
                }
                if (conversationProperty == null) return;
                var entriesProperty = conversationProperty.FindPropertyRelative("dialogueEntries");
                if (entriesProperty == null || !entriesProperty.isArray) return;
                SerializedProperty entryProperty = null;
                for (int i = 0; i < entriesProperty.arraySize; i++)
                {
                    var sp = entriesProperty.GetArrayElementAtIndex(i);
                    if (sp.FindPropertyRelative("id").intValue == currentEntry.id)
                    {
                        entryProperty = sp;
                        break;
                    }
                }
                if (entryProperty == null) return;
                onExecuteProperty = entryProperty.FindPropertyRelative("onExecute");
            }
            if (onExecuteProperty != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(onExecuteProperty);
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }

            // Draw scene-specific event:
            var sceneEventGuid = currentEntry.sceneEventGuid;
            int sceneEventIndex = -1;
            if (string.IsNullOrEmpty(sceneEventGuid))
            {
                // If this entry is not associated with a scene event, show Add button:
                if (GUILayout.Button(new GUIContent("Add Scene Event", "Add a UnityEvent that operates on GameObjects in the currently-open scene.")))
                {
                    MakeSureDialogueSystemSceneEventsExists();
                    sceneEventIndex = DialogueSystemSceneEvents.AddNewDialogueEntrySceneEvent(out sceneEventGuid);
                    currentEntry.sceneEventGuid = sceneEventGuid;
                }
            }
            else
            {
                // Otherwise check if the entry's scene event is defined in this scene:
                sceneEventIndex = DialogueSystemSceneEvents.GetDialogueEntrySceneEventIndex(sceneEventGuid);
            }
            if (sceneEventIndex == -1 && !string.IsNullOrEmpty(sceneEventGuid))
            {
                // If scene event is assigned but not in this scene, show Delete button:
                GUILayout.Label("Scene Event operates in another scene.");
                if (GUILayout.Button("Delete Scene Event"))
                {
                    currentEntry.sceneEventGuid = string.Empty;
                }
            }
            if (sceneEventIndex != -1)
            {
                // Make sure our serialized object points to this scene's DialogueSystemSceneEvents:
                if (dialogueSystemSceneEvents != DialogueSystemSceneEvents.sceneInstance || dialogueSystemSceneEventsSerializedObject == null)
                {
                    dialogueSystemSceneEvents = DialogueSystemSceneEvents.sceneInstance;
                    dialogueSystemSceneEventsSerializedObject = new SerializedObject(dialogueSystemSceneEvents);
                }
            }
            if (sceneEventIndex != -1 && dialogueSystemSceneEventsSerializedObject != null)
            {
                // Scene event is in this scene. Draw it:
                dialogueSystemSceneEventsSerializedObject.Update();
                var sceneEventsListProperty = dialogueSystemSceneEventsSerializedObject.FindProperty("dialogueEntrySceneEvents");
                if (sceneEventsListProperty != null && 0 <= sceneEventIndex && sceneEventIndex < sceneEventsListProperty.arraySize)
                {
                    var sceneEventProperty = sceneEventsListProperty.GetArrayElementAtIndex(sceneEventIndex);
                    EditorGUILayout.LabelField("Scene Event", EditorStyles.boldLabel);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(sceneEventProperty.FindPropertyRelative("guid"), true);
                    EditorGUI.EndDisabledGroup();
                    if (sceneEventProperty != null)
                    {
                        EditorGUILayout.PropertyField(sceneEventProperty.FindPropertyRelative("onExecute"), true);
                    }
                }
                dialogueSystemSceneEventsSerializedObject.ApplyModifiedProperties();
                if (GUILayout.Button("Delete Scene Event"))
                {
                    DialogueSystemSceneEvents.RemoveDialogueEntrySceneEvent(sceneEventGuid);
                    currentEntry.sceneEventGuid = string.Empty;
                }
            }
        }

        private void MakeSureDialogueSystemSceneEventsExists()
        {
            if (DialogueSystemSceneEvents.sceneInstance == null)
            {
                var go = new GameObject("Dialogue System Scene Events");
                DialogueSystemSceneEvents.sceneInstance = go.AddComponent(PixelCrushers.TypeUtility.GetWrapperType(typeof(DialogueSystemSceneEvents))) as DialogueSystemSceneEvents;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                dialogueSystemSceneEvents = null;
            }
        }

        public bool DrawDialogueEntryInspector()
        {
            // Draw field contents:
            bool changedFieldContents = DrawDialogueEntryFieldContents();

            // Draw links:
            Link linkToDelete = null;
            DialogueEntry entryToLinkFrom = null;
            DialogueEntry entryToLinkTo = null;
            bool linkToAnotherConversation = false;
            bool changedLinks = DrawDialogueEntryLinks(currentEntry, ref linkToDelete, ref entryToLinkFrom, ref entryToLinkTo, ref linkToAnotherConversation);
            // Handle deletion:
            if (linkToDelete != null)
            {
                changedLinks = true;
                DeleteLink(linkToDelete);
                InitializeDialogueTree();
            }
            // Handle linking:
            if (entryToLinkFrom != null)
            {
                changedLinks = true;
                if (entryToLinkTo == null)
                {
                    if (linkToAnotherConversation)
                    {
                        LinkToAnotherConversation(entryToLinkFrom);
                    }
                    else
                    {
                        LinkToNewEntry(entryToLinkFrom);
                    }
                }
                else
                {
                    CreateLink(entryToLinkFrom, entryToLinkTo);
                }
                InitializeDialogueTree();
            }

            return changedFieldContents || changedLinks;
        }

        private void ApplyDialogueEntryTemplate(List<Field> fields)
        {
            if (template == null || template.dialogueEntryFields == null || fields == null) return;
            ApplyTemplate(fields, template.dialogueEntryFields);
        }

        private void DrawDialogueEntryParticipants(DialogueEntry entry)
        {
            // Make sure we have references to the actor and conversant fields:
            VerifyParticipantField(entry, "Actor", ref currentEntryActor);
            VerifyParticipantField(entry, "Conversant", ref currentEntryConversant);

            // If actor is unassigned, use conversation's values: (conversant may be set to None)
            if (IsActorIDUnassigned(currentEntryActor)) currentEntryActor.value = currentConversation.ActorID.ToString();
            //if (IsActorIDUnassigned(currentEntryConversant)) currentEntryConversant.value = currentConversation.ConversantID.ToString(); ;

            // Participant IDs:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            DrawParticipantField(currentEntryActor, "Speaker of this entry.");
            DrawParticipantField(currentEntryConversant, "Listener.");
            EditorGUILayout.EndVertical();
            var swap = GUILayout.Button(new GUIContent(" ", "Swap participants."), "Popup", GUILayout.Width(24));
            EditorGUILayout.EndHorizontal();

            if (swap) SwapParticipants(ref currentEntryActor, ref currentEntryConversant);
        }

        private void VerifyParticipantField(DialogueEntry entry, string fieldTitle, ref Field participantField)
        {
            if (participantField == null) participantField = Field.Lookup(entry.fields, fieldTitle);
            if (participantField == null)
            {
                participantField = new Field(fieldTitle, string.Empty, FieldType.Actor);
                entry.fields.Add(participantField);
                SetDatabaseDirty("Add Participant Field");
            }
        }

        private bool IsActorIDUnassigned(Field field)
        {
            return (field == null) || string.IsNullOrEmpty(field.value) || string.Equals(field.value, "-1");
        }

        private void DrawParticipantField(Field participantField, string tooltipText)
        {
            string newValue = DrawAssetPopup<Actor>(participantField.value, (database != null) ? database.actors : null, new GUIContent(participantField.title, tooltipText));
            if (newValue != participantField.value)
            {
                participantField.value = newValue;
                ResetDialogueEntryText();
                SetDatabaseDirty("Change Participant");
            }
        }

        private void SwapParticipants(ref Field currentActor, ref Field currentConversant)
        {
            var newActorValue = currentConversant.value;
            var newConversantValue = currentActor.value;
            currentActor.value = newActorValue;
            currentConversant.value = newConversantValue;
        }

        private int GetFalseConditionIndex(string falseConditionString)
        {
            for (int i = 0; i < falseConditionActionStrings.Length; i++)
            {
                if (string.Equals(falseConditionString, falseConditionActionStrings[i]))
                {
                    return i;
                }
            }
            return 0;
        }

        private void PrepareLinkToDestinations(DialogueEntry entry)
        {
            List<GUIContent> destinationList = new List<GUIContent>();
            destinationList.Add(new GUIContent("(Link To)", string.Empty));
            destinationList.Add(new GUIContent("(Another Conversation)", string.Empty));
            destinationList.Add(new GUIContent("(New Entry)", string.Empty));
            for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
            {
                var destinationEntry = currentConversation.dialogueEntries[i];
                if (destinationEntry != entry)
                {
                    if (linkToDebug)
                    {
                        destinationList.Add(new GUIContent("[" + destinationEntry.id + "]"));
                    }
                    else
                    {
                        var text = (prefs.preferTitlesForLinksTo && !string.IsNullOrEmpty(destinationEntry.Title))
                            ? ("<" + destinationEntry.Title + ">")
                            : GetDialogueEntryText(destinationEntry);
                        destinationList.Add(new GUIContent(Tools.StripRichTextCodes(text)));
                    }
                }
            }
            linkToDestinations = destinationList.ToArray();
            linkToDestinationsFromEntry = entry;
        }

        private bool DrawDialogueEntryLinks(
            DialogueEntry entry,
            ref Link linkToDelete,
            ref DialogueEntry entryToLinkFrom,
            ref DialogueEntry entryToLinkTo,
            ref bool linkToAnotherConversation)
        {

            if (currentConversation == null) return false;

            bool changed = false;
            try
            {
                EditorGUI.BeginChangeCheck();
#if UNITY_EDITOR_OSX
                linkToDebug = EditorGUILayout.Toggle(new GUIContent("Hide Link Text", "In Unity 2019.2-2019.3, a bug in Unity for Mac can hang the editor when link text contains characters that the editor handles improperly. If Unity hangs when opening the Links To dropdown, tick this."), linkToDebug);
#endif
                if (EditorGUI.EndChangeCheck() || linkToDestinationsFromEntry != entry)
                {
                    PrepareLinkToDestinations(entry);
                }

                EditorGUI.BeginChangeCheck();

                int destinationIndex = EditorGUILayout.Popup(new GUIContent("Links To:", "Add a link to another entry. Select (New Entry) to create and link to a new entry."), 0, linkToDestinations);
                if (destinationIndex > 0)
                {
                    entryToLinkFrom = entry;
                    if (destinationIndex == 1)
                    { // (Another Conversation)                        
                        entryToLinkTo = null;
                        linkToAnotherConversation = true;
                    }
                    else if (destinationIndex == 2)
                    { // (New Entry)
                        entryToLinkTo = null;
                        linkToAnotherConversation = false;
                    }
                    else
                    {
                        int destinationID = AssetListIndexToID(destinationIndex, linkToDestinations);
                        entryToLinkTo = currentConversation.dialogueEntries.Find(e => e.id == destinationID);
                        if (entryToLinkTo == null)
                        {
                            entryToLinkFrom = null;
                            Debug.LogError(string.Format("{0}: Couldn't find destination dialogue entry in database.", DialogueDebug.Prefix));
                        }
                    }
                    //EditorGUILayout.EndHorizontal();
                    return false;
                }
                int linkIndexToMoveUp = -1;
                int linkIndexToMoveDown = -1;
                if ((entry != null) && (entry.outgoingLinks != null))
                {
                    for (int linkIndex = 0; linkIndex < entry.outgoingLinks.Count; linkIndex++)
                    {
                        Link link = entry.outgoingLinks[linkIndex];
                        EditorGUILayout.BeginHorizontal();

                        if (link.destinationConversationID == currentConversation.id)
                        {
                            // Fix any links whose originDialogueID doesn't match the origin entry's ID:
                            if (link.originDialogueID != entry.id) link.originDialogueID = entry.id;

                            // Is a link to an entry in the current conversation, so handle normally:
                            DialogueEntry linkEntry = database.GetDialogueEntry(link);
                            if (linkEntry != null)
                            {
                                string linkText = (linkEntry == null) ? string.Empty
                                    : (linkEntry.isGroup ? GetDialogueEntryText(linkEntry) : linkEntry.responseButtonText);
                                if (string.IsNullOrEmpty(linkText) ||
                                    (prefs.preferTitlesForLinksTo && linkEntry != null && !string.IsNullOrEmpty(linkEntry.Title)))
                                {
                                    linkText = "<" + linkEntry.Title + ">";
                                }
                                GUIStyle linkButtonStyle = GetLinkButtonStyle(linkEntry);
                                if (GUILayout.Button(linkText, linkButtonStyle))
                                {
                                    if (linkEntry != null && showNodeEditor)
                                    {
                                        MoveToEntry(linkEntry);
                                        SetCurrentEntry(linkEntry);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    return false;
                                }
                            }
                        }
                        else
                        {

                            // Cross-conversation link:
                            link.destinationConversationID = DrawConversationsPopup(link.destinationConversationID);
                            link.destinationDialogueID = DrawCrossConversationEntriesPopup(link.destinationConversationID, link.destinationDialogueID);
                            if (showNodeEditor && GUILayout.Button(new GUIContent("Go", "Jump to this dialogue entry."), EditorStyles.miniButton, GUILayout.Width(28)))
                            {
                                var linkEntry = database.GetDialogueEntry(link);
                                if (linkEntry != null)
                                {
                                    SetCurrentEntry(linkEntry);
                                    MoveToEntry(linkEntry);
                                }
                            }
                        }

                        EditorGUI.BeginDisabledGroup(linkIndex == 0);
                        if (GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButton, GUILayout.Width(22))) linkIndexToMoveUp = linkIndex;
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(linkIndex == entry.outgoingLinks.Count - 1);
                        if (GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButton, GUILayout.Width(22))) linkIndexToMoveDown = linkIndex;
                        EditorGUI.EndDisabledGroup();
                        link.priority = (ConditionPriority)EditorGUILayout.Popup((int)link.priority, priorityStrings, GUILayout.Width(100));
                        bool deleted = GUILayout.Button(new GUIContent(" ", "Delete link."), "OL Minus", GUILayout.Width(16));
                        if (deleted) linkToDelete = link;
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (linkIndexToMoveUp != -1) MoveLink(entry, linkIndexToMoveUp, -1);
                if (linkIndexToMoveDown != -1) MoveLink(entry, linkIndexToMoveDown, 1);

            }
            catch (NullReferenceException)
            {
                // Hide error if it occurs.
            }
            finally
            {
                changed = EditorGUI.EndChangeCheck();
                if (changed) SetDatabaseDirty("Links Changed [1]");
            }
            return changed;
        }

        private int DrawConversationsPopup(int conversationID)
        {
            List<string> conversations = new List<string>();
            int index = -1;
            for (int i = 0; i < database.conversations.Count; i++)
            {
                var conversation = database.conversations[i];
                conversations.Add(conversation.Title + " [" + conversation.id + "]");
                if (conversation.id == conversationID)
                {
                    index = i;
                }
            }
            index = EditorGUILayout.Popup(index, conversations.ToArray());
            if (0 <= index && index < database.conversations.Count && (database.conversations[index].id != currentConversation.id))
            {
                return database.conversations[index].id;
            }
            else
            {
                return -1;
            }
        }

        private const int MaxEntriesForCrossConversationPopupNoSubmenus = 50;
        private const int CrossConversationPopupSubmenuSize = 20;

        private int DrawCrossConversationEntriesPopup(int conversationID, int entryID)
        {
            var conversation = database.GetConversation(conversationID);
            List<string> entries = new List<string>();
            int index = -1;
            if (conversation != null)
            {
                var useSubmenus = conversation.dialogueEntries.Count > MaxEntriesForCrossConversationPopupNoSubmenus;
#if UNITY_EDITOR_OSX
                useSubmenus = false; // There may be a bug in Unity editor's call to NSMenuItem on MacOS.
#endif
                for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                {
                    var entry = conversation.dialogueEntries[i];
                    entries.Add(GetCrossConversationEntryText(entry, useSubmenus, i));
                    if (entry.id == entryID)
                    {
                        index = i;
                    }
                }
            }
            EditorGUI.BeginDisabledGroup(conversation == null);
            index = EditorGUILayout.Popup(index, entries.ToArray());
            EditorGUI.EndDisabledGroup();
            if ((conversation != null) && (0 <= index && index < conversation.dialogueEntries.Count))
            {
                return conversation.dialogueEntries[index].id;
            }
            else
            {
                return -1;
            }
        }

        private string GetCrossConversationEntryText(DialogueEntry entry, bool useSubmenus, int menuItemNumber)
        {
            var text = entry.currentDialogueText;
            if (string.IsNullOrEmpty(text))
            {
                text = entry.currentMenuText;
                if (string.IsNullOrEmpty(text))
                {
                    var title = entry.Title;
                    if (!string.IsNullOrEmpty(title)) text = "<" + title + ">";
                    if (string.IsNullOrEmpty(text))
                    {
                        text = Field.LookupValue(entry.fields, "Description");
                    }
                    if (entry.isGroup) text = "{group} " + text;
                }
            }
            text = entry.id + ": " + text;
            text = text.Replace("/", "\u2215"); // Prevent embedded forward slashes from acting as submenus.
            if (useSubmenus)
            {
                text = "Group " + ((menuItemNumber / CrossConversationPopupSubmenuSize) + 1) + "/" + text;
            }
            return Tools.StripRichTextCodes(text);
        }

        private void MoveLink(DialogueEntry entry, int linkIndex, int direction)
        {
            if ((entry != null) && (0 <= linkIndex && linkIndex < entry.outgoingLinks.Count))
            {
                int newIndex = Mathf.Clamp(linkIndex + direction, 0, entry.outgoingLinks.Count - 1);
                Link link = entry.outgoingLinks[linkIndex];
                entry.outgoingLinks.RemoveAt(linkIndex);
                entry.outgoingLinks.Insert(newIndex, link);
                SetDatabaseDirty("Move Link");
            }
        }

        private int AssetListIndexToID(int index, GUIContent[] list)
        {
            if ((0 <= index) && (index < list.Length))
            {
                Regex rx = new Regex(@"^\[[0-9]+\]");
                Match match = rx.Match(list[index].text);
                if (match.Success)
                {
                    int id = 0;
                    string matchString = match.ToString();
                    string idString = matchString.Substring(1, matchString.Length - 2);
                    int.TryParse(idString, out id);
                    return id;
                }
            }
            return -1;
        }

        private void CreateLink(DialogueEntry source, DialogueEntry destination)
        {
            if ((source != null) && (destination != null))
            {
                Link link = new Link();
                link.originConversationID = currentConversation.id;
                link.originDialogueID = source.id;
                link.destinationConversationID = currentConversation.id;
                link.destinationDialogueID = destination.id;
                source.outgoingLinks.Add(link);
                SetDatabaseDirty("Create Link");
            }
        }

        private void LinkToNewEntry(DialogueEntry source, bool useSameActorAssignments = false)
        {
            if (source != null)
            {
                DialogueEntry newEntry = CreateNewDialogueEntry(string.Empty);
                if (useSameActorAssignments)
                {
                    newEntry.ActorID = (source.ActorID == source.ConversantID) ? database.playerID : source.ActorID;
                    newEntry.ConversantID = source.ConversantID;
                }
                else
                {
                    newEntry.ActorID = source.ConversantID;
                    newEntry.ConversantID = (source.ActorID == source.ConversantID) ? database.playerID : source.ActorID;
                }
                newEntry.canvasRect = new Rect(source.canvasRect.x, source.canvasRect.y + source.canvasRect.height + 20, source.canvasRect.width, source.canvasRect.height);
                Link link = new Link();
                link.originConversationID = currentConversation.id;
                link.originDialogueID = source.id;
                link.destinationConversationID = currentConversation.id;
                link.destinationDialogueID = newEntry.id;
                source.outgoingLinks.Add(link);
                currentEntry = newEntry;
                SetDatabaseDirty("Link to New Entry");
            }
        }

        private void LinkToAnotherConversation(DialogueEntry source)
        {
            Link link = new Link();
            link.originConversationID = currentConversation.id;
            link.originDialogueID = source.id;
            link.destinationConversationID = -1;
            link.destinationDialogueID = -1;
            source.outgoingLinks.Add(link);
            SetDatabaseDirty("Link to Conversation");
        }

        private DialogueEntry CreateNewDialogueEntry(string title)
        {
            DialogueEntry entry = template.CreateDialogueEntry(GetNextDialogueEntryID(), currentConversation.id, title ?? string.Empty);
            currentConversation.dialogueEntries.Add(entry);
            SetDatabaseDirty("Create New Dialogue Entry");
            return entry;
        }

        private int GetNextDialogueEntryID()
        {
            int highestID = -1;
            currentConversation.dialogueEntries.ForEach(entry => highestID = Mathf.Max(highestID, entry.id));
            return highestID + 1;
        }

        private void DeleteLink(Link linkToDelete)
        {
            if ((currentConversation != null) && (linkToDelete != null))
            {

                if (EditorUtility.DisplayDialog("Delete Link?", "Are you sure you want to delete this link?", "Delete", "Cancel"))
                {
                    // Count # dialogue entries linking to same:
                    int numLinksToDestination = 0;
                    for (int i = 0; i < currentConversation.dialogueEntries.Count; i++)
                    {
                        var entry = currentConversation.dialogueEntries[i];
                        for (int j = 0; j < entry.outgoingLinks.Count; j++)
                        {
                            var link = entry.outgoingLinks[j];
                            if (link.destinationDialogueID == linkToDelete.destinationDialogueID)
                            {
                                numLinksToDestination++;
                            }
                        }
                    }

                    // Delete link:
                    DialogueEntry origin = currentConversation.dialogueEntries.Find(e => e.id == linkToDelete.originDialogueID);
                    if (origin != null)
                    {
                        origin.outgoingLinks.Remove(linkToDelete);
                    }

                    // If only 1 linking to same, delete target dialogue entry:
                    //--- Removed this behavior to keep consistent with right-clicking on link and selecting Delete:
                    //DialogueEntry destination = currentConversation.dialogueEntries.Find(e => e.id == linkToDelete.destinationDialogueID);
                    //if ((numLinksToDestination <= 1) && (destination != null))
                    //{
                    //    if (currentEntry == destination) ResetCurrentEntry();
                    //    currentConversation.dialogueEntries.Remove(destination);
                    //}

                    //if (currentConversation.dialogueEntries.Count <= 0)
                    //{
                    //    database.conversations.Remove(currentConversation);
                    //    ResetDialogueTree();
                    //}
                    SetDatabaseDirty("Delete Link");
                }
            }
        }

    }

}