// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This static utility class converts Chat Mapper projects to dialogue databases.
    /// </summary>
    public static class ChatMapperToDialogueDatabase
    {

        /// <summary>
        /// Converts a Chat Mapper project to a dialogue database.
        /// </summary>
        /// <returns>The dialogue database, or <c>null</c> if a conversation error occurred.</returns>
        /// <param name="chatMapperProject">Chat Mapper project.</param>
        public static DialogueDatabase ConvertToDialogueDatabase(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject)
        {
            var database = DatabaseUtility.CreateDialogueDatabaseInstance();
            if (database == null)
            {
                if (DialogueDebug.logErrors) Debug.LogError(string.Format("{0}: Couldn't convert Chat Mapper project '{1}'.", new System.Object[] { DialogueDebug.Prefix, chatMapperProject.Title }));
            }
            else
            {
                ConvertProjectAttributes(chatMapperProject, database);
                ConvertActors(chatMapperProject, database);
                ConvertItems(chatMapperProject, database);
                ConvertLocations(chatMapperProject, database);
                ConvertVariables(chatMapperProject, database);
                ConvertConversations(chatMapperProject, database);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Converted Chat Mapper project '{1}' containing {2} actors, {3} conversations, {4} items (quests), {5} variables, and {6} locations.",
                                                                   new System.Object[] { DialogueDebug.Prefix, chatMapperProject.Title, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count }));
            }
            return database;
        }

        private static void ConvertProjectAttributes(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.version = chatMapperProject.Version;
            database.author = chatMapperProject.Author;
            database.description = chatMapperProject.Description;
            database.emphasisSettings = new EmphasisSetting[4];
            database.emphasisSettings[0] = new EmphasisSetting(chatMapperProject.EmphasisColor1, chatMapperProject.EmphasisStyle1);
            database.emphasisSettings[1] = new EmphasisSetting(chatMapperProject.EmphasisColor2, chatMapperProject.EmphasisStyle2);
            database.emphasisSettings[2] = new EmphasisSetting(chatMapperProject.EmphasisColor3, chatMapperProject.EmphasisStyle3);
            database.emphasisSettings[3] = new EmphasisSetting(chatMapperProject.EmphasisColor4, chatMapperProject.EmphasisStyle4);
        }

        private static void ConvertActors(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.actors = new List<Actor>();
            foreach (var chatMapperActor in chatMapperProject.Assets.Actors)
            {
                database.actors.Add(new Actor(chatMapperActor));
            }
        }

        private static void ConvertItems(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.items = new List<Item>();
            foreach (var chatMapperItem in chatMapperProject.Assets.Items)
            {
                database.items.Add(new Item(chatMapperItem));
            }
        }

        private static void ConvertLocations(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.locations = new List<Location>();
            foreach (var chatMapperLocation in chatMapperProject.Assets.Locations)
            {
                database.locations.Add(new Location(chatMapperLocation));
            }
        }

        private static void ConvertVariables(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.variables = new List<Variable>();
            int id = 0;
            foreach (var chatMapperVariable in chatMapperProject.Assets.UserVariables)
            {
                Variable variable = new Variable(chatMapperVariable);
                variable.id = id;
                id++;
                database.variables.Add(variable);
            }
        }

        private static void ConvertConversations(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database)
        {
            database.conversations = new List<Conversation>();
            foreach (var chatMapperConversation in chatMapperProject.Assets.Conversations)
            {
                Conversation conversation = new Conversation(chatMapperConversation);
                SetConversationStartCutsceneToNone(conversation);
                ConvertAudioFilesToSequences(conversation);
                ConvertOverridesFieldsToDisplaySettingsOverrides(conversation);
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    foreach (Link link in entry.outgoingLinks)
                    {
                        if (link.destinationConversationID == 0) link.destinationConversationID = conversation.id;
                        if (link.originConversationID == 0) link.originConversationID = conversation.id;
                    }
                }
                database.conversations.Add(conversation);
            }
            FixConversationsLinkedToFirstEntry(database);
        }

        private static void SetConversationStartCutsceneToNone(Conversation conversation)
        {
            DialogueEntry entry = conversation.GetFirstDialogueEntry();
            if (entry == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Conversation '{1}' doesn't have a START dialogue entry.", new System.Object[] { DialogueDebug.Prefix, conversation.Title }));
            }
            else
            {
                if (string.IsNullOrEmpty(entry.currentSequence))
                {
                    if (Field.FieldExists(entry.fields, DialogueSystemFields.Sequence))
                    {
                        entry.currentSequence = SequencerKeywords.NoneCommand;
                    }
                    else
                    {
                        entry.fields.Add(new Field(DialogueSystemFields.Sequence, SequencerKeywords.NoneCommand, FieldType.Text));
                    }
                }
            }
        }

        public static void FixConversationsLinkedToFirstEntry(DialogueDatabase database, bool resetNodePositions = false)
        {
            // If there's a link to a conversation's START entry, swap the participants.
            // This increases the likelihood that the NPC will "speak" the START line
            // so it doesn't present to the player as a blank response button.

            // NOTE: This also handles prefs.resetNodePositions if set.

            try
            {
                // First identify conversations whose START entries to fix:
                var conversationsToFix = new List<int>();
                foreach (var conversation in database.conversations)
                {
                    foreach (var entry in conversation.dialogueEntries)
                    {

                        // Also handle prefs.resetNodePositions:
                        if (resetNodePositions) entry.canvasRect = new Rect(0, 0, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);

                        // Check links:
                        foreach (var link in entry.outgoingLinks)
                        {
                            if (link.destinationDialogueID == 0) // START
                            {
                                if (!conversationsToFix.Contains(link.destinationConversationID))
                                {
                                    conversationsToFix.Add(link.destinationConversationID);
                                }
                            }
                        }
                    }
                }

                // Then fix them:
                foreach (var conversationID in conversationsToFix)
                {
                    var entry = database.GetConversation(conversationID).GetFirstDialogueEntry();
                    var temp = entry.ActorID;
                    entry.ActorID = entry.ConversantID;
                    entry.ConversantID = temp;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error fixing up linked conversation: " + e.Message);
            }
        }

        public static void ConvertAudioFilesToSequences(Conversation conversation)
        {
            if (conversation == null || conversation.dialogueEntries == null) return;
            foreach (var entry in conversation.dialogueEntries)
            {
                var audioFiles = entry.AudioFiles;
                if (!(string.IsNullOrEmpty(audioFiles) || string.Equals("[]", audioFiles)))
                {
                    var audioClipName = audioFiles.Substring(1, audioFiles.IndexOfAny(new char[] { ';', ']' }) - 1);
                    audioClipName = audioClipName.Substring(0, audioClipName.LastIndexOf('.'));
                    audioClipName = audioClipName.Replace("\\", "/");
                    if (audioClipName.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
                    {
                        audioClipName = audioClipName.Substring(10); // Remove Resources folder from front.
                    }
                    var audioWaitCommand = string.Format("AudioWait({0})", audioClipName);
                    if (entry.currentSequence != null && !entry.currentSequence.Contains(audioWaitCommand))
                    {
                        entry.currentSequence = string.Format("AudioWait({0}); {1}", audioClipName, entry.currentSequence);
                    }
                }
            }
        }

        public static void ConvertOverridesFieldsToDisplaySettingsOverrides(Conversation conversation)
        {
            if (conversation == null) return;
            var overridesField = conversation.fields.Find(field => field.title == "Overrides");
            if (overridesField == null) return;
            if (string.IsNullOrEmpty(overridesField.value)) return;
            var overrides = JsonUtility.FromJson<ConversationOverrideDisplaySettings>(overridesField.value);
            if (overrides == null) return;
            conversation.overrideSettings = overrides;
            conversation.fields.Remove(overridesField);
        }

    }
}
