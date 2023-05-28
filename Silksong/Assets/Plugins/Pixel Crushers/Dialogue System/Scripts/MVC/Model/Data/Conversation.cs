// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A conversation asset. A conversation is a collection of dialogue entries (see 
    /// DialogueEntry) that are linked together to form branching, interactive dialogue between two
    /// actors (see Actor).
    /// </summary>
    [System.Serializable]
    public class Conversation : Asset
    {

        /// <summary>
        /// Optional settings to override the Dialogue Manager's Display Settings.
        /// </summary>
        public ConversationOverrideDisplaySettings overrideSettings = new ConversationOverrideDisplaySettings();

        /// <summary>
        /// Currently unused by the dialogue system, this is the nodeColor value defined in Chat 
        /// Mapper.
        /// </summary>
        public string nodeColor = null;

        /// <summary>
        /// The dialogue entries in the conversation.
        /// </summary>
        public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

        public List<EntryGroup> entryGroups = new List<EntryGroup>();

        /// <summary>
        /// Conversation's scroll position in Dialogue Editor window canvas.
        /// </summary>
        [HideInInspector]
        public Vector2 canvasScrollPosition = Vector2.zero;

        /// <summary>
        /// Conversation's zoom level in Dialogue Editor window.
        /// </summary>
        [HideInInspector]
        public float canvasZoom = 1;

        /// <summary>
        /// Gets or sets the Title field.
        /// </summary>
        /// <value>
        /// The title of the conversation, most often used to look up and start a specific 
        /// conversation.
        /// </value>
        public string Title
        {
            get { return LookupValue(DialogueSystemFields.Title); }
            set { Field.SetValue(fields, DialogueSystemFields.Title, value); }
        }

        /// <summary>
        /// Gets or sets the Actor ID. The actor is the primary participant in the conversation.
        /// </summary>
        /// <value>
        /// The actor ID.
        /// </value>
        public int ActorID
        {
            get { return LookupInt(DialogueSystemFields.Actor); }
            set { Field.SetValue(fields, DialogueSystemFields.Actor, value.ToString(), FieldType.Actor); }
        }

        /// <summary>
        /// Gets or sets the Conversant ID. The conversant is the other participant in the 
        /// conversation.
        /// </summary>
        /// <value>
        /// The conversant ID.
        /// </value>
        public int ConversantID
        {
            get { return LookupInt(DialogueSystemFields.Conversant); }
            set { Field.SetValue(fields, DialogueSystemFields.Conversant, value.ToString(), FieldType.Actor); }
        }

        /// <summary>
        /// Initializes a new Conversation.
        /// </summary>
        public Conversation() { }

        public Conversation(Conversation sourceConversation) : base(sourceConversation as Asset)
        {
            this.nodeColor = sourceConversation.nodeColor;
            this.overrideSettings = sourceConversation.overrideSettings;
            this.dialogueEntries = CopyDialogueEntries(sourceConversation.dialogueEntries);
            this.entryGroups = CopyEntryGroups(sourceConversation.entryGroups);
        }

        /// <summary>
        /// Initializes a new Conversation copied from a Chat Mapper conversation.
        /// </summary>
        /// <param name='chatMapperConversation'>
        /// The Chat Mapper conversation.
        /// </param>
        public Conversation(ChatMapper.Conversation chatMapperConversation, bool putEndSequenceOnLastSplit = true)
        {
            Assign(chatMapperConversation, putEndSequenceOnLastSplit);
        }

        /// <summary>
        /// Copies a Chat Mapper conversation.
        /// </summary>
        /// <param name='chatMapperConversation'>
        /// The Chat Mapper conversation.
        /// </param>
        public void Assign(ChatMapper.Conversation chatMapperConversation, bool putEndSequenceOnLastSplit = true)
        {
            if (chatMapperConversation != null)
            {
                Assign(chatMapperConversation.ID, chatMapperConversation.Fields);
                nodeColor = chatMapperConversation.NodeColor;
                foreach (var chatMapperEntry in chatMapperConversation.DialogEntries)
                {
                    AddConversationDialogueEntry(chatMapperEntry);
                }
                SplitPipesIntoEntries(putEndSequenceOnLastSplit);

                // Set priority of links to the destination entry's priority:
                foreach (var entry in dialogueEntries)
                {
                    foreach (var link in entry.outgoingLinks)
                    {
                        if (link.destinationConversationID != id) continue;
                        var dest = GetDialogueEntry(link.destinationDialogueID);
                        if (dest == null) continue;
                        link.priority = dest.conditionPriority;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the conversation dialogue entry. Starting in Chat Mapper 1.6, XML entries don't 
        /// include the conversation ID, so we set it manually here.
        /// </summary>
        /// <param name='chatMapperEntry'>
        /// Chat Mapper entry.
        /// </param>
        private void AddConversationDialogueEntry(ChatMapper.DialogEntry chatMapperEntry)
        {
            var entry = new DialogueEntry(chatMapperEntry);
            entry.conversationID = id;
            dialogueEntries.Add(entry);
        }

        /// <summary>
        /// Looks up a dialogue entry by title.
        /// </summary>
        /// <returns>
        /// The dialogue entry whose title matches, or <c>null</c> if no such entry exists.
        /// </returns>
        /// <param name='title'>
        /// The title of the dialogue entry.
        /// </param>
        public DialogueEntry GetDialogueEntry(string title)
        {
            return dialogueEntries.Find(e => string.Equals(e.Title, title));
        }

        /// <summary>
        /// Looks up a dialogue entry by its ID.
        /// </summary>
        /// <returns>
        /// The dialogue entry whose Id matches, or <c>null</c> if no such entry exists.
        /// </returns>
        /// <param name='dialogueEntryID'>
        /// The dialogue entry ID.
        /// </param>
        public DialogueEntry GetDialogueEntry(int dialogueEntryID)
        {
            return dialogueEntries.Find(e => e.id == dialogueEntryID);
        }

        /// <summary>
        /// Looks up the first dialogue entry in the conversation, defined (as in Chat Mapper) as 
        /// the entry titled START.
        /// </summary>
        /// <returns>
        /// The first dialogue entry in the conversation.
        /// </returns>
        public DialogueEntry GetFirstDialogueEntry()
        {
            return dialogueEntries.Find(e => string.Equals(e.Title, "START"));
        }

        /// <summary>
        /// Processes all dialogue entries, splitting entries containing pipe characters ("|")
        /// into multiple entries.
        /// </summary>
        /// <param name="putEndSequenceOnLastSplit">
        /// Put sequencer commands with end keyword on the last split entry, other commands on the
        /// first entry, and use default delay for middle entries.
        /// </param>
        /// <param name="trimWhitespace">Trim whitespace such as newlines.</param>
        public void SplitPipesIntoEntries(bool putEndSequenceOnLastSplit = true, bool trimWhitespace = false)
        {
            if (dialogueEntries != null)
            {
                var count = dialogueEntries.Count;
                for (int entryIndex = 0; entryIndex < count; entryIndex++)
                {
                    var dialogueText = dialogueEntries[entryIndex].DialogueText;
                    if (!string.IsNullOrEmpty(dialogueText))
                    {
                        if (dialogueText.Contains("|"))
                        {
                            SplitEntryAtPipes(entryIndex, dialogueText, putEndSequenceOnLastSplit, trimWhitespace);
                        }
                    }
                }
            }
        }

        private void SplitEntryAtPipes(int originalEntryIndex, string dialogueText, bool putEndSequenceOnLastSplit, bool trimWhitespace)
        {
            // Split by Dialogue Text:
            var substrings = dialogueText.Split(new char[] { '|' });
            var originalEntry = dialogueEntries[originalEntryIndex];
            originalEntry.DialogueText = trimWhitespace ? substrings[0].Trim() : substrings[0];
            var originalOutgoingLinks = originalEntry.outgoingLinks;
            ConditionPriority priority = ((originalOutgoingLinks != null) && (originalOutgoingLinks.Count > 0)) ? originalOutgoingLinks[0].priority : ConditionPriority.Normal;
            var currentEntry = originalEntry;
            var entries = new List<DialogueEntry>();
            entries.Add(currentEntry);

            // Split Menu Text:
            var defaultMenuText = (originalEntry != null && originalEntry.MenuText != null) ? originalEntry.MenuText : string.Empty;
            var menuTextSubstrings = defaultMenuText.Split(new char[] { '|' });

            // Split Audio Files:
            var audioFilesText = originalEntry.AudioFiles;
            audioFilesText = ((audioFilesText != null) && (audioFilesText.Length >= 2)) ? audioFilesText.Substring(1, audioFilesText.Length - 2) : string.Empty;
            var audioFiles = audioFilesText.Split(new char[] { ';' });
            currentEntry.AudioFiles = string.Format("[{0}]", new System.Object[] { (audioFiles.Length > 0) ? audioFiles[0] : string.Empty });

            // Create new dialogue entries for the split parts:
            int i = 1;
            while (i < substrings.Length)
            {
                var newEntryDialogueText = substrings[i];
                var newEntryMenuText = (i < menuTextSubstrings.Length) ? menuTextSubstrings[i] : string.Empty;
                if (trimWhitespace)
                {
                    newEntryDialogueText = newEntryDialogueText.Trim();
                    newEntryMenuText = newEntryMenuText.Trim();
                }
                var newEntry = AddNewDialogueEntry(originalEntry, newEntryDialogueText, i, trimWhitespace);
                newEntry.canvasRect = new Rect(originalEntry.canvasRect.x + i * 20, originalEntry.canvasRect.y + i * 10, originalEntry.canvasRect.width, originalEntry.canvasRect.height);
                newEntry.currentMenuText = newEntryMenuText;
                newEntry.AudioFiles = string.Format("[{0}]", new System.Object[] { (i < audioFiles.Length) ? audioFiles[i] : string.Empty });
                currentEntry.outgoingLinks = new List<Link>() { NewLink(currentEntry, newEntry, priority) };
                currentEntry = newEntry;
                entries.Add(newEntry);
                i++;
            }

            // Set the last entry's links to the original outgoing links:
            currentEntry.outgoingLinks = originalOutgoingLinks;

            // Fix up the other splittable fields in the original entry:
            foreach (var field in originalEntry.fields)
            {
                if (string.IsNullOrEmpty(field.title)) continue;
                string fieldValue = (field.value != null) ? field.value : string.Empty;
                bool isSequence = field.title.StartsWith(DialogueSystemFields.Sequence);
                bool isLocalization = (field.type == FieldType.Localization);
                bool containsPipes = fieldValue.Contains("|");
                bool isSplittable = (isSequence || isLocalization) &&
                    !string.IsNullOrEmpty(field.value) && containsPipes;
                if (isSplittable)
                {
                    substrings = field.value.Split(new char[] { '|' });
                    if (substrings.Length > 1)
                    {
                        fieldValue = trimWhitespace ? substrings[0].Trim() : substrings[0];
                        field.value = fieldValue;
                    }
                }
                else if (isSequence && putEndSequenceOnLastSplit && !containsPipes)
                {
                    if (!string.IsNullOrEmpty(field.value) && field.value.Contains(SequencerKeywords.End))
                    {
                        PutEndSequenceOnLastSplit(entries, field);
                    }
                }
            }
        }

        private void PutEndSequenceOnLastSplit(List<DialogueEntry> entries, Field field)
        {
            var commands = field.value.Split(new char[] { ';' });
            for (int entryNum = 0; entryNum < entries.Count; entryNum++)
            {
                var entry = entries[entryNum];
                var entryField = Field.Lookup(entry.fields, field.title);
                entryField.value = string.Empty;
                if (entryNum == 0)
                {
                    foreach (var command in commands)
                    {
                        if (!command.Contains(SequencerKeywords.End))
                        {
                            entryField.value += command.Trim() + "; ";
                        }
                    }
                    entryField.value += SequencerKeywords.DelayEndCommand;
                }
                else if (entryNum == (entries.Count - 1))
                {
                    foreach (var command in commands)
                    {
                        if (command.Contains(SequencerKeywords.End))
                        {
                            entryField.value += command.Trim() + "; ";
                        }
                    }
                }
                else
                {
                    entryField.value = SequencerKeywords.DelayEndCommand;
                }
            }
        }

        private DialogueEntry AddNewDialogueEntry(DialogueEntry originalEntry, string dialogueText, int partNum, bool trimWhitespace)
        {
            var newEntry = new DialogueEntry();
            newEntry.id = GetHighestDialogueEntryID() + 1;
            newEntry.conversationID = originalEntry.conversationID;
            newEntry.isRoot = originalEntry.isRoot;
            newEntry.isGroup = originalEntry.isGroup;
            newEntry.nodeColor = originalEntry.nodeColor;
            newEntry.delaySimStatus = originalEntry.delaySimStatus;
            newEntry.falseConditionAction = originalEntry.falseConditionAction;
            newEntry.conditionsString = string.Equals(originalEntry.falseConditionAction, "Passthrough") ? originalEntry.conditionsString : string.Empty;
            newEntry.userScript = string.Empty;
            newEntry.fields = new List<Field>();
            foreach (var field in originalEntry.fields)
            {
                if (string.IsNullOrEmpty(field.title)) continue;
                string fieldValue = field.value;
                bool isSplittable = (field.title.StartsWith(DialogueSystemFields.Sequence) || (field.type == FieldType.Localization)) &&
                    !string.IsNullOrEmpty(field.value) && field.value.Contains("|");
                if (isSplittable)
                {
                    string[] substrings = field.value.Split(new char[] { '|' });
                    if (partNum < substrings.Length)
                    {
                        fieldValue = trimWhitespace ? substrings[partNum].Trim() : substrings[partNum].Trim();
                    }
                }
                newEntry.fields.Add(new Field(field.title, fieldValue, field.type));
            }
            newEntry.DialogueText = dialogueText;
            dialogueEntries.Add(newEntry);
            return newEntry;
        }

        private int GetHighestDialogueEntryID()
        {
            int highest = 0;
            foreach (var entry in dialogueEntries)
            {
                highest = Mathf.Max(entry.id, highest);
            }
            return highest;
        }

        private Link NewLink(DialogueEntry origin, DialogueEntry destination, ConditionPriority priority = ConditionPriority.Normal)
        {
            var newLink = new Link();
            newLink.originConversationID = origin.conversationID;
            newLink.originDialogueID = origin.id;
            newLink.destinationConversationID = destination.conversationID;
            newLink.destinationDialogueID = destination.id;
            newLink.isConnector = (origin.conversationID != destination.conversationID);
            newLink.priority = priority;
            return newLink;
        }

        private List<DialogueEntry> CopyDialogueEntries(List<DialogueEntry> sourceEntries)
        {
            var entries = new List<DialogueEntry>();
            foreach (var sourceEntry in sourceEntries)
            {
                entries.Add(new DialogueEntry(sourceEntry));
            }
            return entries;
        }

        private List<EntryGroup> CopyEntryGroups(List<EntryGroup> sourceGroups)
        {
            var groups = new List<EntryGroup>();
            foreach (var group in sourceGroups)
            {
                groups.Add(new EntryGroup(group));
            }
            return groups;
        }

    }

}
