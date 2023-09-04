// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This defines the syntax of the optional delegate that conversations can use
    /// to double-check that a dialogue entry is valid to use.
    /// </summary>
    public delegate bool IsDialogueEntryValidDelegate(DialogueEntry dialogueEntry);

    /// <summary>
    /// Handles logic for the data model of a conversation. This class retrieves dialogue entries
    /// and updates the data state, which includes the dialogue database and Lua environment. It 
    /// does not keep track of state or handle the user interface. The ConversationController 
    /// controls the current state of the conversation, and ConversationView handles the user 
    /// interface.
    /// </summary>
    public class ConversationModel
    {

        /// <summary>
        /// Title of the conversation that started this ConversationModel.
        /// </summary>
        public string conversationTitle { get; private set; }

        /// <summary>
        /// The first state in the conversation, which is the root of the dialogue tree.
        /// </summary>
        /// <value>
        /// The first state.
        /// </value>
        public ConversationState firstState { get; private set; }

        /// <summary>
        /// Info about the character that is specified as the actor in the conversation. This is 
        /// typically the character that initiates the conversation, such as the PC.
        /// </summary>
        public CharacterInfo actorInfo { get { return m_actorInfo; } }

        /// <summary>
        /// Info about the character that is specified as the conversant in the conversation. This
        /// is the other character in the conversation (e.g., the NPC in a PC-NPC conversation).
        /// </summary>
        public CharacterInfo conversantInfo { get { return m_conversantInfo; } }

        /// <summary>
        /// Indicates whether the conversation has any responses linked from the start entry.
        /// </summary>
        /// <value><c>true</c> if the conversation has responses; otherwise, <c>false</c>.</value>
        public bool hasValidEntry { get { return (firstState != null) && (firstState.hasAnyResponses || !IsStartEntryState(firstState)); } }

        /// <summary>
        /// Gets or sets the IsDialogueEntryValid delegate.
        /// </summary>
        public IsDialogueEntryValidDelegate isDialogueEntryValid { get; set; }


        /// @cond FOR_V1_COMPATIBILITY
        public ConversationState FirstState { get { return firstState; } private set { firstState = value; } }
        public CharacterInfo ActorInfo { get { return actorInfo; } }
        public CharacterInfo ConversantInfo { get { return conversantInfo; } }
        public bool HasValidEntry { get { return hasValidEntry; } }
        public IsDialogueEntryValidDelegate IsDialogueEntryValid { get { return isDialogueEntryValid; } private set { isDialogueEntryValid = value; } }
        /// @endcond

        // Public accessors for cached private variables below:
        public bool allowLuaExceptions { get { return m_allowLuaExceptions; } set { m_allowLuaExceptions = value; } }
        public EntrytagFormat entrytagFormat { get { return m_entrytagFormat; } set { m_entrytagFormat = value; } }
        public EmTag emTagForOldResponses { get { return m_emTagForOldResponses; } set { m_emTagForOldResponses = value; } }
        public EmTag emTagForInvalidResponses { get { return m_emTagForInvalidResponses; } set { m_emTagForInvalidResponses = value; } }
        public bool includeInvalidEntries { get { return m_includeInvalidEntries; } set { m_includeInvalidEntries = value; } }

        private DialogueDatabase m_database = null;
        private CharacterInfo m_actorInfo = null;
        private CharacterInfo m_conversantInfo = null;
        private bool m_allowLuaExceptions = false;
        private Dictionary<int, CharacterInfo> m_characterInfoCache = new Dictionary<int, CharacterInfo>();
        private EntrytagFormat m_entrytagFormat = EntrytagFormat.ActorName_ConversationID_EntryID;
        private EmTag m_emTagForOldResponses = EmTag.None;
        private EmTag m_emTagForInvalidResponses = EmTag.None;
        private bool m_includeInvalidEntries = false;
        private string pcPortraitName = null;
        private Sprite pcPortraitSprite = null;
        private DialogueEntry forceLinkEntry = null;

        /// <summary>
        /// The current conversation ID. When this changes (in GotoState), the Lua environment
        /// needs to set the Dialog[] table to the new conversation's table.
        /// </summary>
        private int m_currentDialogTableConversationID = -1;

        /// <summary>
        /// Initializes a new ConversationModel.
        /// </summary>
        /// <param name="database">The database to use.</param>
        /// <param name="title">The title of the conversation in the database.</param>
        /// <param name="actor">Actor.</param>
        /// <param name="conversant">Conversant.</param>
        /// <param name="allowLuaExceptions">If set to <c>true</c> allow Lua exceptions.</param>
        /// <param name="isDialogueEntryValid">Is dialogue entry valid.</param>
        /// <param name="initialDialogueEntryID">Initial dialogue entry ID (-1 to start at beginning).</param>
        /// <param name="stopAtFirstValid">If set to <c>true</c> stop at first valid link from the initial entry.</param>
        /// <param name="skipExecution">IF set to <c>true</c>, doesn't run the Lua Script or OnExecute event on the initial entry.</param>
        public ConversationModel(DialogueDatabase database, string title, Transform actor, Transform conversant,
                                 bool allowLuaExceptions, IsDialogueEntryValidDelegate isDialogueEntryValid,
                                 int initialDialogueEntryID = -1, bool stopAtFirstValid = false, bool skipExecution = false)
        {
            this.m_allowLuaExceptions = allowLuaExceptions;
            this.m_database = database;
            this.conversationTitle = title;
            this.isDialogueEntryValid = isDialogueEntryValid;
            Conversation conversation = database.GetConversation(title);
            if (conversation != null)
            {
                DisplaySettings displaySettings = DialogueManager.displaySettings;
                if (displaySettings != null)
                {
                    if (displaySettings.cameraSettings != null)
                    {
                        m_entrytagFormat = displaySettings.cameraSettings.entrytagFormat;
                    }
                    if (displaySettings.inputSettings != null)
                    {
                        m_emTagForOldResponses = displaySettings.inputSettings.emTagForOldResponses;
                        m_emTagForInvalidResponses = displaySettings.inputSettings.emTagForInvalidResponses;
                        m_includeInvalidEntries = displaySettings.inputSettings.includeInvalidEntries;
                    }
                }
                if (conversation.overrideSettings != null)
                {
                    if (conversation.overrideSettings.overrideInputSettings)
                    {
                        m_emTagForOldResponses = conversation.overrideSettings.emTagForOldResponses;
                        m_emTagForInvalidResponses = conversation.overrideSettings.emTagForInvalidResponses;
                        m_includeInvalidEntries = conversation.overrideSettings.includeInvalidEntries;
                    }
                }

                SetParticipants(conversation, actor, conversant);
                if (initialDialogueEntryID == -1)
                {
                    firstState = GetState(conversation.GetFirstDialogueEntry(), true, stopAtFirstValid, skipExecution);
                    FixFirstStateSequence();
                }
                else
                {
                    firstState = GetState(conversation.GetDialogueEntry(initialDialogueEntryID), true, stopAtFirstValid, skipExecution);
                }
            }
            else
            {
                firstState = null;
                if (DialogueDebug.logErrors) Debug.LogWarning(string.Format("{0}: Conversation '{1}' not found in database.", new System.Object[] { DialogueDebug.Prefix, title }));
            }
        }

        public int GetConversationID(ConversationState state)
        {
            return (state != null && state.subtitle != null && state.subtitle.dialogueEntry != null) ? state.subtitle.dialogueEntry.conversationID : -1;
        }

        public ConversationOverrideDisplaySettings GetConversationOverrideSettings(ConversationState state)
        {
            var conversation = m_database.GetConversation(GetConversationID(state));
            return (conversation != null) ? conversation.overrideSettings : null;
        }

        /// <summary>
        /// If the START entry's sequence is empty and there's no subtitle text, don't
        /// use the default sequence. Instead, use None().
        /// </summary>
        private void FixFirstStateSequence()
        {
            if ((firstState != null) &&
                (firstState.subtitle != null) &&
                string.IsNullOrEmpty(firstState.subtitle.sequence) &&
                string.IsNullOrEmpty(firstState.subtitle.formattedText.text))
            {
                firstState.subtitle.sequence = SequencerKeywords.NoneCommand;
            }
        }

        /// <summary>
        /// Determines whether this is the START entry.
        /// </summary>
        /// <returns><c>true</c> if is start entry; otherwise, <c>false</c>.</returns>
        /// <param name="state">State.</param>
        private bool IsStartEntryState(ConversationState state)
        {
            return (state != null) && (state.subtitle != null) && (state.subtitle.dialogueEntry != null) &&
                (state.subtitle.dialogueEntry.id == 0);
        }

        /// <summary>
        /// Sends a message to the actor and conversant. Used to send OnConversationStart and 
        /// OnConversationEnd messages.
        /// </summary>
        /// <param name='message'>
        /// The message (e.g., OnConversationStart or OnConversationEnd).
        /// </param>
        public void InformParticipants(string message, bool informDialogueManager = false)
        {
            if (DialogueSystemController.isWarmingUp) return; // If warming up, don't send messages.
            Transform actor = (m_actorInfo == null) ? null : m_actorInfo.transform;
            Transform conversant = (m_conversantInfo == null) ? null : m_conversantInfo.transform;
            Transform target = null;
            if (actor != null)
            {
                target = (conversant != null) ? conversant : actor;
                if (target != null) actor.BroadcastMessage(message, target, SendMessageOptions.DontRequireReceiver);
            }
            if ((conversant != null) && (conversant != actor))
            {
                target = (actor != null) ? actor : conversant;
                if (target != null) conversant.BroadcastMessage(message, target, SendMessageOptions.DontRequireReceiver);
            }
            if (informDialogueManager)
            {
                var dialogueManagerTransform = DialogueManager.instance.transform;
                if (dialogueManagerTransform != actor && dialogueManagerTransform != conversant)
                {
                    target = (actor != null) ? actor : conversant;
                    if (target == null) target = DialogueManager.instance.transform;
                    DialogueManager.instance.BroadcastMessage(message, target, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        /// <summary>
        /// "Follows" a dialogue entry and returns its full conversation state. This method updates 
        /// the Lua environment (marking the entry as visited). If includeLinks is <c>true</c>, 
        /// it evaluates all links from the dialogue entry and records valid links in the state.
        /// </summary>
        /// <returns>The state.</returns>
        /// <param name="entry">The dialogue entry to "follow."</param>
        /// <param name="includeLinks">If set to <c>true</c> records all links from the dialogue entry whose conditions are true.</param>
        /// <param name="stopAtFirstValid">If set to <c>true</c> ,stops including at the first valid link.</param>
        /// <param name="skipExecution">IF set to <c>true</c>, doesn't run the Lua Script or OnExecute event.</param>
        public ConversationState GetState(DialogueEntry entry, bool includeLinks, bool stopAtFirstValid = false, bool skipExecution = false)
        {
            if (entry != null)
            {
                if (DialogueManager.instance.activeConversations.Count > 1)
                {
                    // If multiple conversations are active, set the right participants in Lua:
                    DialogueLua.SetParticipants(m_actorInfo.Name, m_conversantInfo.Name, m_actorInfo.nameInDatabase, m_conversantInfo.nameInDatabase);
                }
                DialogueManager.instance.SendMessage(DialogueSystemMessages.OnPrepareConversationLine, entry, SendMessageOptions.DontRequireReceiver);
                DialogueLua.MarkDialogueEntryDisplayed(entry);
                Lua.Run("thisID = " + entry.id);
                SetDialogTable(entry.conversationID);
                if (!skipExecution)
                {
                    Lua.Run(entry.userScript, DialogueDebug.logInfo, m_allowLuaExceptions);
                    try
                    {
                        entry.onExecute.Invoke();
                    }
                    catch (System.Exception e)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning("Non-scene OnExecute() event failed on dialogue entry " + entry.conversationID + ":" + entry.id + ": " + e.Message);
                    }
                }
                CharacterInfo actorInfo = GetCharacterInfo(entry.ActorID);
                CharacterInfo listenerInfo = GetCharacterInfo(entry.ConversantID);
                if (!skipExecution)
                {
                    var sceneEvent = DialogueSystemSceneEvents.GetDialogueEntrySceneEvent(entry.sceneEventGuid);
                    var eventGameObject = (actorInfo.transform != null) ? actorInfo.transform.gameObject : DialogueManager.instance.gameObject;
                    if (sceneEvent != null)
                    {
                        try
                        {
                            sceneEvent.onExecute.Invoke(eventGameObject);
                        }
                        catch (System.Exception e)
                        {
                            if (DialogueDebug.logWarnings) Debug.LogWarning("Scene OnExecute() event failed on dialogue entry " + entry.conversationID + ":" + entry.id + ": " + e.Message);
                        }
                    }
                }
                FormattedText formattedText = FormattedText.Parse(entry.subtitleText, m_database.emphasisSettings);
                CheckSequenceField(entry);
                string entrytag = m_database.GetEntrytag(entry.conversationID, entry.id, m_entrytagFormat);
                Subtitle subtitle = new Subtitle(actorInfo, listenerInfo, formattedText, entry.currentSequence, entry.currentResponseMenuSequence, entry, entrytag);
                List<Response> npcResponses = new List<Response>();
                List<Response> pcResponses = new List<Response>();
                if (includeLinks)
                {
                    if (forceLinkEntry != null)
                    {
                        AddForcedLink(npcResponses, pcResponses);
                    }
                    else
                    {
                        EvaluateLinks(entry, npcResponses, pcResponses, new List<DialogueEntry>(), stopAtFirstValid);
                    }
                }
                return new ConversationState(subtitle, npcResponses.ToArray(), pcResponses.ToArray(), entry.isGroup);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Forces the next state to link to a specific dialogue entry instead of its designed links.
        /// </summary>
        public void ForceNextStateToLinkToEntry(DialogueEntry entry)
        {
            forceLinkEntry = entry;
        }

        private void AddForcedLink(List<Response> npcResponses, List<Response> pcResponses)
        {
            if (m_database.GetCharacterType(forceLinkEntry.ActorID) == CharacterType.NPC)
            {
                npcResponses.Add(new Response(FormattedText.Parse(forceLinkEntry.subtitleText, m_database.emphasisSettings), forceLinkEntry));
            }
            else
            {
                pcResponses.Add(new Response(FormattedText.Parse(forceLinkEntry.subtitleText, m_database.emphasisSettings), forceLinkEntry));
            }
            forceLinkEntry = null;
        }

        /// <summary>
        /// "Follows" a dialogue entry and returns its full conversation state. This method updates 
        /// the Lua environment (marking the entry as visited) and evaluates all links from the 
        /// dialogue entry and records valid links in the state.
        /// </summary>
        /// <returns>
        /// The state representing the dialogue entry.
        /// </returns>
        /// <param name='entry'>
        /// The dialogue entry to "follow."
        /// </param>
        public ConversationState GetState(DialogueEntry entry)
        {
            return GetState(entry, true);
        }

        /// <summary>
        /// Updates the responses in the specified state.
        /// </summary>
        /// <param name="state">State to check.</param>
        public void UpdateResponses(ConversationState state)
        {
            List<Response> npcResponses = new List<Response>();
            List<Response> pcResponses = new List<Response>();
            EvaluateLinks(state.subtitle.dialogueEntry, npcResponses, pcResponses, new List<DialogueEntry>());
            state.npcResponses = npcResponses.ToArray();
            state.pcResponses = pcResponses.ToArray();
        }

        private void SetDialogTable(int newConversationID)
        {
            if (m_currentDialogTableConversationID != newConversationID)
            {
                m_currentDialogTableConversationID = newConversationID;
                Lua.Run(string.Format("Dialog = Conversation[{0}].Dialog", new System.Object[] { newConversationID }));
            }
        }

        private void CheckSequenceField(DialogueEntry entry)
        {
            if (string.IsNullOrEmpty(entry.currentSequence) && !string.IsNullOrEmpty(entry.VideoFile))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Dialogue entry '{1}' Video File field is assigned but Sequence is blank. Cutscenes now use Sequence field.", new System.Object[] { DialogueDebug.Prefix, entry.currentDialogueText }));
            }
        }

        /// <summary>
        /// Evaluates a dialogue entry's links. Evaluation follows the same rules as Chat Mapper:
        /// - Links are evaluated from highest to lowest priority; once links are found in a
        /// priority level, evaluation stops. Lower priority links aren't evaluated.
        /// - If a link evaluates <c>false</c> and the false condition action is "Passthrough",
        /// the link's children are evaluated.
        /// </summary>
        /// <param name='entry'>
        /// Dialogue entry.
        /// </param>
        /// <param name='npcResponses'>
        /// Links from the entry that are NPC responses are added to this list.
        /// </param>
        /// <param name='pcResponses'>
        /// Links from the entry that are PC responses are added to this list.
        /// </param>
        /// <param name='visited'>
        /// Keeps track of links that have already been visited so we don't loop back on ourselves
        /// and get frozen in an infinite loop.
        /// </param>
        private void EvaluateLinks(DialogueEntry entry, List<Response> npcResponses, List<Response> pcResponses,
                                   List<DialogueEntry> visited, bool stopAtFirstValid = false)
        {
            if ((entry != null) && !visited.Contains(entry))
            {
                visited.Add(entry);
                for (int i = (int)ConditionPriority.High; i >= 0; i--)
                {
                    EvaluateLinksAtPriority((ConditionPriority)i, entry, npcResponses, pcResponses, visited, stopAtFirstValid);
                    if ((npcResponses.Count > 0) || (pcResponses.Count > 0)) return;
                }
            }
        }

        private void EvaluateLinksAtPriority(ConditionPriority priority, DialogueEntry entry, List<Response> npcResponses,
                                             List<Response> pcResponses, List<DialogueEntry> visited,
                                             bool stopAtFirstValid = false)
        {
            if (entry != null)
            {
                for (int ol = 0; ol < entry.outgoingLinks.Count; ol++)
                {
                    var link = entry.outgoingLinks[ol];
                    DialogueEntry destinationEntry = m_database.GetDialogueEntry(link);
                    if ((destinationEntry != null) && (/*(destinationEntry.conditionPriority == priority) ||*/ (link.priority == priority))) // Note: Only observe link priority. Why does Chat Mapper even have conditionPriority?
                    {
                        CharacterType characterType = m_database.GetCharacterType(destinationEntry.ActorID);
                        Lua.Run("thisID = " + destinationEntry.id);
                        bool isValid = Lua.IsTrue(destinationEntry.conditionsString, DialogueDebug.logInfo, m_allowLuaExceptions) &&
                            ((isDialogueEntryValid == null) || isDialogueEntryValid(destinationEntry));
                        if (isValid || (m_includeInvalidEntries && (characterType == CharacterType.PC)))
                        {

                            // Condition is true (or blank), so add this link:
                            if (destinationEntry.isGroup)
                            {

                                // For groups, evaluate their links (after running the group node's Lua code and OnExecute() event):
                                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Evaluate Group ({1}): ID={2}:{3} '{4}' ({5})", new System.Object[] { DialogueDebug.Prefix, GetActorName(m_database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, destinationEntry.Title, isValid }));
                                Lua.Run(destinationEntry.userScript, DialogueDebug.logInfo, m_allowLuaExceptions);
                                destinationEntry.onExecute.Invoke();
                                isValid = false; // Assume invalid until at least one group's child is true.
                                for (int i = (int)ConditionPriority.High; i >= 0; i--)
                                {
                                    int originalResponseCount = npcResponses.Count + pcResponses.Count;
                                    EvaluateLinksAtPriority((ConditionPriority)i, destinationEntry, npcResponses, pcResponses, visited);
                                    if ((npcResponses.Count + pcResponses.Count) > originalResponseCount)
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {

                                // For regular entries, just add them:
                                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Add Link ({1}): ID={2}:{3} '{4}' ({5})", new System.Object[] { DialogueDebug.Prefix, GetActorName(m_database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, GetLinkText(characterType, destinationEntry), isValid }));
                                if (characterType == CharacterType.NPC)
                                {

                                    // Add NPC response:
                                    npcResponses.Add(new Response(FormattedText.Parse(destinationEntry.subtitleText, m_database.emphasisSettings), destinationEntry, isValid));
                                }
                                else
                                {

                                    // Add PC response, wrapping old responses in em tags if specified:
                                    string text = destinationEntry.responseButtonText;
                                    if (m_emTagForOldResponses != EmTag.None)
                                    {
                                        string simStatus = Lua.Run(string.Format("return Conversation[{0}].Dialog[{1}].SimStatus", new System.Object[] { destinationEntry.conversationID, destinationEntry.id })).asString;
                                        bool isOldResponse = string.Equals(simStatus, DialogueLua.WasDisplayed);
                                        if (isOldResponse)
                                        {
                                            text = UITools.StripEmTags(text);
                                            text = string.Format("[em{0}]{1}[/em{0}]", (int)m_emTagForOldResponses, text);
                                        }
                                    }
                                    if (m_emTagForInvalidResponses != EmTag.None)
                                    {
                                        if (!isValid)
                                        {
                                            text = UITools.StripEmTags(text);
                                            text = string.Format("[em{0}]{1}[/em{0}]", (int)m_emTagForInvalidResponses, text);
                                        }
                                    }
                                    var formattedText = FormattedText.Parse(text, m_database.emphasisSettings);
                                    if (!isValid)
                                    {
                                        formattedText.forceAuto = false;
                                        formattedText.forceMenu = false;
                                    }
                                    pcResponses.Add(new Response(formattedText, destinationEntry, isValid));
                                    DialogueLua.MarkDialogueEntryOffered(destinationEntry);
                                }
                            }
                            if (isValid && stopAtFirstValid) return;

                        }
                        else
                        {

                            // Condition is false, so block or pass through according to destination entry's setting:
                            if (LinkUtility.IsPassthroughOnFalse(destinationEntry))
                            {
                                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Passthrough on False Link ({1}): ID={2}:{3} '{4}' Condition='{5}'", new System.Object[] { DialogueDebug.Prefix, GetActorName(m_database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, GetLinkText(characterType, destinationEntry), destinationEntry.conditionsString }));
                                List<Response> linkNpcResponses = new List<Response>();
                                List<Response> linkPcResponses = new List<Response>();
                                EvaluateLinks(destinationEntry, linkNpcResponses, linkPcResponses, visited);
                                npcResponses.AddRange(linkNpcResponses);
                                pcResponses.AddRange(linkPcResponses);
                            }
                            else
                            {
                                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Block on False Link ({1}): ID={2}:{3} '{4}' Condition='{5}'", new System.Object[] { DialogueDebug.Prefix, GetActorName(m_database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, GetLinkText(characterType, destinationEntry), destinationEntry.conditionsString }));
                            }
                        }
                    }
                }
            }
        }

        private string GetActorName(Actor actor)
        {
            return (actor != null) ? actor.Name : "null";
        }

        private string GetLinkText(CharacterType characterType, DialogueEntry entry)
        {
            return (characterType == CharacterType.NPC) ? entry.subtitleText : entry.responseButtonText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="actor"></param>
        /// <param name="conversant"></param>
        public void SetParticipants(Conversation conversation, Transform actor, Transform conversant)
        {
            m_characterInfoCache.Clear();
            m_actorInfo = GetCharacterInfo(conversation.ActorID, actor);
            m_conversantInfo = GetCharacterInfo(conversation.ConversantID, conversant);
            if (m_actorInfo != null) m_characterInfoCache[m_actorInfo.id] = m_actorInfo;
            if (m_conversantInfo != null) m_characterInfoCache[m_conversantInfo.id] = m_conversantInfo;
            DialogueLua.SetParticipants(m_actorInfo.Name, m_conversantInfo.Name, m_actorInfo.nameInDatabase, m_conversantInfo.nameInDatabase);
            IdentifyPCPortrait(conversation);
        }

        /// <summary>
        /// Call when linking to another conversation. If the player actor has changed, this method
        /// updates the PC portrait name and image.
        /// </summary>
        public void UpdateParticipantsOnLinkedConversation(int newConversationID)
        {
            var conversation = m_database.GetConversation(newConversationID);
            if (conversation == null) return;

            var newConversationActorID = conversation.ActorID;
            if (newConversationActorID != m_actorInfo.id)
            {
                var newConversationActor = m_database.GetActor(newConversationActorID);
                if (newConversationActor != null && newConversationActor.IsPlayer)
                {
                    m_actorInfo = GetCharacterInfo(newConversationActorID);
                    IdentifyPCPortrait(conversation);
                    return;
                }
            }
            var newConversationConversantID = conversation.ConversantID;
            if (newConversationConversantID != m_conversantInfo.id)
            {
                var newConversationConversant = m_database.GetActor(newConversationConversantID);
                if (newConversationConversant != null && newConversationConversant.IsPlayer)
                {
                    m_conversantInfo = GetCharacterInfo(newConversationConversantID);
                    IdentifyPCPortrait(conversation);
                    return;
                }
            }
        }

        private void IdentifyPCPortrait(Conversation conversation)
        {
            if (conversation == null) return;

            // Find player info if neither actor nor conversant is player:
            if (m_actorInfo.isPlayer)
            {
                pcPortraitName = m_actorInfo.Name;
                pcPortraitSprite = m_actorInfo.portrait;
            }
            else if (m_conversantInfo.isPlayer)
            {
                pcPortraitName = m_conversantInfo.Name;
                pcPortraitSprite = m_conversantInfo.portrait;
            }
            else
            {
                for (int i = 0; i < conversation.dialogueEntries.Count; i++)
                {
                    var entry = conversation.dialogueEntries[i];
                    var entryActor = m_database.GetActor(entry.ActorID);
                    if (entryActor != null && entryActor.IsPlayer)
                    {
                        pcPortraitName = entryActor.Name;
                        //pcPortraitSprite = entryActor.GetPortraitSprite();
                        entryActor.AssignPortraitSprite((sprite) => { pcPortraitSprite = sprite; });
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Overrides the conversation model's cached info for a character.
        /// </summary>
        /// <param name="id">Actor ID</param>
        /// <param name="character">Character's transform. Will use its Dialogue Actor info if present.</param>
        public void OverrideCharacterInfo(int id, Transform character)
        {
            m_characterInfoCache.Remove(id);
            GetCharacterInfo(id, character);
        }

        /// <summary>
        /// Gets the character info for a character given its actor ID and transform.
        /// </summary>
        /// <returns>
        /// The character info.
        /// </returns>
        /// <param name='id'>
        /// The character's actor ID in the dialogue database.
        /// </param>
        /// <param name='character'>
        /// The transform of the character's GameObject.
        /// </param>
        public CharacterInfo GetCharacterInfo(int id, Transform character)
        {
            if (!m_characterInfoCache.ContainsKey(id))
            {
                Actor actor = null;
                var dialogueActor = DialogueActor.GetDialogueActorComponent(character);
                if (dialogueActor != null)
                {
                    actor = m_database.GetActor(dialogueActor.actor);
                }
                if (actor == null) actor = m_database.GetActor(id);
                string nameInDatabase = (dialogueActor != null) ? dialogueActor.actor : string.Empty;
                if (string.IsNullOrEmpty(nameInDatabase) && actor != null) nameInDatabase = actor.Name;
                if (character == null && !string.IsNullOrEmpty(nameInDatabase))
                {
                    character = CharacterInfo.GetRegisteredActorTransform(nameInDatabase);
                }
                var actorID = (actor != null) ? actor.id : id;
                var portrait = (dialogueActor != null) ? dialogueActor.GetPortraitSprite() : null;
                if (portrait == null) portrait = GetPortrait(character, actor);
                CharacterInfo characterInfo = new CharacterInfo(actorID, nameInDatabase, character, m_database.GetCharacterType(id), portrait);
                if (characterInfo.portrait == null && actor != null)
                {
                    actor.AssignPortraitSprite((sprite) => { characterInfo.portrait = sprite; });
                }
                // Don't cache null actor ID -1:
                if (id == -1) return characterInfo;
                // Otherwise cache to speed up lookups:
                m_characterInfoCache.Add(id, characterInfo);
            }
            return m_characterInfoCache[id];
        }

        /// <summary>
        /// Gets the character info for a character given its actor ID.
        /// </summary>
        /// <returns>The character info.</returns>
        /// <param name="id">The character's actor ID in the dialogue database.</param>
        public CharacterInfo GetCharacterInfo(int id)
        {
            return GetCharacterInfo(id, GetCharacterTransform(id));
        }

        private Transform GetCharacterTransform(int id)
        {
            if (id == m_actorInfo.id)
            {
                return m_actorInfo.transform;
            }
            else if (id == m_conversantInfo.id)
            {
                return m_conversantInfo.transform;
            }
            else
            {
                return null;
            }
        }

        // Handles the easy cases (without Addressables). May return null.
        private Sprite GetPortrait(Transform character, Actor actor)
        {
            Sprite portrait = null;
            if (character != null)
            {
                portrait = GetPortraitByActorName(DialogueActor.GetActorName(character), actor);
            }
            if ((portrait == null) && (actor != null))
            {
                portrait = GetPortraitByActorName(actor.Name, actor);
                if (portrait == null) portrait = actor.GetPortraitSprite();
            }
            return portrait;
        }

        private Sprite GetPortraitByActorName(string actorName, Actor actor)
        {
            // Also suppress logging for Lua return Actor[].Current_Portrait.
            var originalDebugLevel = DialogueDebug.level;
            DialogueDebug.level = DialogueDebug.DebugLevel.Warning;
            string imageName = DialogueLua.GetActorField(actorName, DialogueSystemFields.CurrentPortrait).asString;
            DialogueDebug.level = originalDebugLevel;
            if (string.IsNullOrEmpty(imageName))
            {
                return (actor != null) ? actor.GetPortraitSprite() : null;
            }
            else if (imageName.StartsWith("pic="))
            {
                if (actor == null)
                {
                    return null;
                }
                else
                {
                    return actor.GetPortraitSprite(Tools.StringToInt(imageName.Substring("pic=".Length)));
                }
            }
            else
            {
                // Check if named image is already assigned to actor. Otherwise load as asset.
                var sprite = actor.GetPortraitSprite(imageName);
                return (sprite != null) ? sprite
                    : UITools.CreateSprite(DialogueManager.LoadAsset(imageName) as Texture2D);
            }
        }

        /// <summary>
        /// Updates the actor portrait sprite for any cached character info.
        /// </summary>
        /// <param name="actorName">Actor name.</param>
        /// <param name="sprite">Portrait sprite.</param>
        public void SetActorPortraitSprite(string actorName, Sprite sprite)
        {
            foreach (CharacterInfo characterInfo in m_characterInfoCache.Values)
            {
                if (string.Equals(characterInfo.Name, actorName) || string.Equals(characterInfo.nameInDatabase, actorName))
                {
                    characterInfo.portrait = sprite;
                }
            }
        }

        /// <summary>
        /// Returns the name of the PC in this conversation.
        /// </summary>
        /// <returns>The PC name, or <c>null</c> if both are NPCs.</returns>
        public string GetPCName()
        {
            if (m_database.IsPlayerID(m_actorInfo.id))
            {
                return m_actorInfo.Name;
            }
            else if (m_database.IsPlayerID(m_conversantInfo.id))
            {
                return m_conversantInfo.Name;
            }
            else
            {
                return pcPortraitName;
            }
        }

        /// <summary>
        /// Returns the portrait sprite of the PC in this conversation.
        /// </summary>
        /// <returns>The PC sprite, or <c>null</c> if both are NPCs.</returns>
        public Sprite GetPCSprite()
        {
            if (m_database.IsPlayerID(m_actorInfo.id))
            {
                return m_actorInfo.portrait;
            }
            else if (m_database.IsPlayerID(m_conversantInfo.id))
            {
                return m_conversantInfo.portrait;
            }
            else
            {
                return pcPortraitSprite;
            }
        }

    }

}
