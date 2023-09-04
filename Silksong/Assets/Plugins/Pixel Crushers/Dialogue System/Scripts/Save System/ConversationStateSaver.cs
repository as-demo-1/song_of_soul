// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this script to your Dialogue Manager to keep track of the 
    /// current conversation and dialogue entry. When you load a game,
    /// it will resume the conversation at that point.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ConversationStateSaver : Saver
    {
        [Serializable]
        public class Data
        {
            public int conversationID;
            public int entryID;
            public string actorName;
            public string conversantName;
            public List<string> actorGOs = null;
            public List<SubtitlePanelNumber> actorGOPanels = null;
            public List<int> actorIDs = null;
            public List<SubtitlePanelNumber> actorIDPanels = null;
        }

        /// <summary>
        /// Override to make default key value "ConversationState".
        /// </summary>
        public override string key
        {
            get
            {
                if (string.IsNullOrEmpty(m_runtimeKey))
                {
                    m_runtimeKey = !string.IsNullOrEmpty(_internalKeyValue) ? _internalKeyValue : "ConversationState";
                    if (appendSaverTypeToKey)
                    {
                        var typeName = GetType().Name;
                        if (typeName.EndsWith("Saver")) typeName.Remove(typeName.Length - "Saver".Length);
                        m_runtimeKey += typeName;
                    }
                }
                return m_runtimeKey;
            }
            set
            {
                _internalKeyValue = value;
                m_runtimeKey = value;
            }
        }

        public override string RecordData()
        {
            if (!DialogueManager.isConversationActive) return string.Empty;
            var data = new Data();
            var state = DialogueManager.currentConversationState;
            var entry = state.subtitle.dialogueEntry;
            data.conversationID = entry.conversationID;
            data.entryID = state.subtitle.dialogueEntry.id;
            data.actorName = (DialogueManager.currentActor != null) ? DialogueManager.currentActor.name : string.Empty;
            data.conversantName = (DialogueManager.currentConversant != null) ? DialogueManager.currentConversant.name : string.Empty;
            var ui = DialogueManager.dialogueUI as StandardDialogueUI;
            if (ui != null)
            {
                ui.conversationUIElements.standardSubtitleControls.RecordActorPanelCache(out data.actorGOs, out data.actorGOPanels, out data.actorIDs, out data.actorIDPanels);
            }
            return SaveSystem.Serialize(data);
        }

        public override void ApplyData(string s)
        {
            if (!enabled || string.IsNullOrEmpty(s)) return;
            var data = SaveSystem.Deserialize<Data>(s);
            if (data == null) return;
            StartCoroutine(StartSavedConversation(data));
        }

        protected System.Collections.IEnumerator StartSavedConversation(Data data)
        {
            var dialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
            DialogueManager.StopConversation();
            if (dialogueUI != null)
            {
                float safeguardTimeout = Time.realtimeSinceStartup + 5f;
                while (dialogueUI.isOpen && Time.realtimeSinceStartup < safeguardTimeout)
                {
                    yield return null;
                }
            }
            var conversationID = data.conversationID;
            var entryID = data.entryID;
            var conversation = DialogueManager.masterDatabase.GetConversation(conversationID);
            var actorName = DialogueLua.GetVariable("CurrentConversationActor").AsString;
            var conversantName = DialogueLua.GetVariable("CurrentConversationConversant").AsString;
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: ConversationStateSaver is resuming conversation " + conversation.Title + " with actor=" + actorName + " and conversant=" + conversantName + " at entry " + entryID + ".", this);
            var actor = string.IsNullOrEmpty(actorName) ? null : GameObject.Find(actorName);
            var conversant = string.IsNullOrEmpty(conversantName) ? null : GameObject.Find(conversantName);
            var actorTransform = (actor != null) ? actor.transform : null;
            var conversantTransform = (conversant != null) ? conversant.transform : null;
            var ui = DialogueManager.dialogueUI as StandardDialogueUI;
            if (ui != null)
            {
                ui.conversationUIElements.standardSubtitleControls.QueueSavedActorPanelCache(data.actorGOs, data.actorGOPanels, data.actorIDs, data.actorIDPanels);
            }
            DialogueManager.StartConversation(conversation.Title, actorTransform, conversantTransform, entryID);
        }
    }
}
