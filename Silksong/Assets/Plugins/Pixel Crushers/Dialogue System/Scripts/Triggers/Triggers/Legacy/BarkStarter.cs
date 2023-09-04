// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is the base class for bark trigger components such as BarkOnIdle and BarkTrigger.
    /// </summary>
    [AddComponentMenu("")]
    public abstract class BarkStarter : ConversationStarter
    {

        /// <summary>
        /// Specifies the order to run through the list of barks.
        /// 
        /// - Random: Choose a random bark from the conversation, avoiding sequential repeats if possible.
        /// - Sequential: Choose the barks in order from first to last, looping at the end.
        /// </summary>
        [Tooltip("The order in which to bark dialogue entries.")]
        public BarkOrder barkOrder = BarkOrder.Random;

        /// <summary>
        /// Are barks allowed during conversations?
        /// </summary>
        [Tooltip("Allow barks during active conversations.")]
        public bool allowDuringConversations = false;

        /// <summary>
        /// If ticked, bark info is cached during the first bark. This can reduce stutter
        /// when barking on slower mobile devices, but barks are not reevaluated each time
        /// as the state changes, barks use no em formatting codes, and sequences are not
        /// played with barks.
        /// </summary>
        [Tooltip("Cache all lines during first bark. This can reduce stutter when barking on slower mobile devices, but barks' conditions are not reevaluated each time as the state changes, barks use no em formatting codes, and sequences are not played with barks.")]
        public bool cacheBarkLines = false;

        /// <summary>
        /// Gets the sequencer used by the current bark, if a bark is playing.
        /// If a bark is not playing, this is undefined. To check if a bark is
        /// playing, check the bark UI's IsPlaying property.
        /// </summary>
        /// <value>The sequencer.</value>
        public Sequencer sequencer { get; private set; }

        private BarkHistory barkHistory;

        private bool tryingToBark = false;

        private ConversationState cachedState = null;

        private IBarkUI barkUI = null;

        /// <summary>
        /// Gets or sets the bark index for sequential barks.
        /// </summary>
        /// <value>The index of the bark, starting from <c>0</c>.</value>
        public int BarkIndex
        {
            get { return barkHistory.index; }
            set { barkHistory.index = value; }
        }

        protected BarkGroupMember barkGroupMember = null;

        protected virtual void Awake()
        {
            barkHistory = new BarkHistory(barkOrder);
            sequencer = null;
            barkGroupMember = GetBarker().GetComponent<BarkGroupMember>();
        }

        protected virtual void Start()
        {
            if (cacheBarkLines && cachedState == null) PopulateCache(GetBarker(), null);
        }

        protected virtual void OnEnable()
        {
            PersistentDataManager.RegisterPersistentData(gameObject);
        }

        protected virtual void OnDisable()
        {
            PersistentDataManager.UnregisterPersistentData(gameObject);
        }

        /// <summary>
        /// Barks if the condition is true.
        /// </summary>
        /// <param name="target">Target.</param>
        public void TryBark(Transform target)
        {
            TryBark(target, target);
        }

        /// <summary>
        /// Barks if the condition is true.
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="interactor">Interactor to test the condition against.</param>
        public void TryBark(Transform target, Transform interactor)
        {
            if (!tryingToBark)
            {
                tryingToBark = true;
                try
                {
                    if ((condition == null) || condition.IsTrue(interactor))
                    {
                        if (string.IsNullOrEmpty(conversation))
                        {
                            if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Bark triggered on {1}, but conversation name is blank.", new System.Object[] { DialogueDebug.Prefix, name }), GetBarker());
                        }
                        else if (DialogueManager.isConversationActive && !allowDuringConversations)
                        {
                            if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Bark triggered on {1}, but a conversation is already active.", new System.Object[] { DialogueDebug.Prefix, name }), GetBarker());
                        }
                        else if (cacheBarkLines)
                        {
                            BarkCachedLine(GetBarker(), target);
                        }
                        else
                        {
                            if (barkGroupMember != null)
                            {
                                barkGroupMember.GroupBark(conversation, target, barkHistory);
                            }
                            else
                            {
                                DialogueManager.Bark(conversation, GetBarker(), target, barkHistory);
                            }
                            sequencer = BarkController.LastSequencer;
                        }
                        DestroyIfOnce();
                    }
                }
                finally
                {
                    tryingToBark = false;
                }
            }
        }

        private Transform GetBarker()
        {
            return Tools.Select(conversant, this.transform);
        }

        private string GetBarkerName()
        {
            return DialogueActor.GetActorName(GetBarker());
        }

        private void BarkCachedLine(Transform speaker, Transform listener)
        {
            if (barkUI == null) barkUI = speaker.GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
            if (cachedState == null) PopulateCache(speaker, listener);
            BarkNextCachedLine(speaker, listener);
        }

        private void PopulateCache(Transform speaker, Transform listener)
        {
            if (string.IsNullOrEmpty(conversation) && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): conversation title is blank", new System.Object[] { DialogueDebug.Prefix, speaker, listener }), speaker);
            ConversationModel conversationModel = new ConversationModel(DialogueManager.masterDatabase, conversation, speaker, listener, DialogueManager.allowLuaExceptions, DialogueManager.isDialogueEntryValid);
            cachedState = conversationModel.firstState;
            if ((cachedState == null) && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no START entry", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
            if (!cachedState.hasAnyResponses && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no valid bark lines", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
        }

        private void BarkNextCachedLine(Transform speaker, Transform listener)
        {
            if ((barkUI != null) && (cachedState != null) && cachedState.hasAnyResponses)
            {
                Response[] responses = cachedState.hasNPCResponse ? cachedState.npcResponses : cachedState.pcResponses;
                int index = (barkHistory ?? new BarkHistory(BarkOrder.Random)).GetNextIndex(responses.Length);
                DialogueEntry barkEntry = responses[index].destinationEntry;
                if ((barkEntry == null) && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark entry is null", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
                if (barkEntry != null)
                {
                    Subtitle subtitle = new Subtitle(cachedState.subtitle.listenerInfo, cachedState.subtitle.speakerInfo, FormattedText.Parse(barkEntry.currentDialogueText), string.Empty, string.Empty, barkEntry);
                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}'", new System.Object[] { DialogueDebug.Prefix, speaker, listener, subtitle.formattedText.text }), speaker);
                    if (barkGroupMember != null)
                    {
                        barkGroupMember.GroupBarkString(subtitle.formattedText.text, listener, subtitle.sequence);
                    }
                    else
                    {
                        StartCoroutine(BarkController.Bark(subtitle, speaker, listener, barkUI));
                    }
                }
            }
        }

        /// <summary>
        /// Resets the bark history to the beginning of the list of bark lines.
        /// </summary>
        public void ResetBarkHistory()
        {
            barkHistory.Reset();
        }

        /// <summary>
        /// Listens for the OnRecordPersistentData message and records the current bark index.
        /// </summary>
        public void OnRecordPersistentData()
        {
            if (enabled && barkHistory != null)
            {
                DialogueLua.SetActorField(GetBarkerName(), "Bark_Index", barkHistory.index);
            }
        }

        /// <summary>
        /// Listens for the OnApplyPersistentData message and retrieves the current bark index.
        /// </summary>
        public void OnApplyPersistentData()
        {
            if (enabled)
            {
                if (barkHistory == null) barkHistory = new BarkHistory(barkOrder);
                barkHistory.index = DialogueLua.GetActorField(GetBarkerName(), "Bark_Index").asInt;
            }
        }

    }

}
