// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this to the Dialogue Manager and/or participants to hook into various Dialogue System events.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class DialogueSystemEvents : MonoBehaviour
    {

        [System.Serializable]
        public class StringEvent : UnityEvent<string> { }

        [System.Serializable]
        public class TransformEvent : UnityEvent<Transform> { }

        [System.Serializable]
        public class SubtitleEvent : UnityEvent<Subtitle> { }

        [System.Serializable]
        public class ResponsesEvent : UnityEvent<Response[]> { }

        [System.Serializable]
        public class ConversationEvents
        {
            [Tooltip("Invoked when a conversation starts. Transform is primary actor (typically player).")]
            public TransformEvent onConversationStart = new TransformEvent();

            [Tooltip("Invoked when a conversation ends. Transform is primary actor (typically player).")]
            public TransformEvent onConversationEnd = new TransformEvent();

            [Tooltip("Run OnConversationEnd() events at end of frame to allow other scripts to complete their frame processing first.")]
            public bool runOnConversationEndEventsAtEndOfFrame = false;

            [Tooltip("Invoked when a conversation is cancelled. Transform is primary actor (typically player).")]
            public TransformEvent onConversationCancelled = new TransformEvent();

            [Tooltip("Invoked just before a line is delivered. Passes Subtitle.")]
            public SubtitleEvent onConversationLine = new SubtitleEvent();

            [Tooltip("Invoked when a line has finished. Passes Subtitle.")]
            public SubtitleEvent onConversationLineEnd = new SubtitleEvent();

            [Tooltip("Invoked if player presses cancel button while line is being delivered.")]
            public SubtitleEvent onConversationLineCancelled = new SubtitleEvent();

            [Tooltip("Invoked when showing a response menu. Passes Response[] array.")]
            public ResponsesEvent onConversationResponseMenu = new ResponsesEvent();

            [Tooltip("Invoked when a response menu times out.")]
            public UnityEvent onConversationResponseMenuTimeout = new UnityEvent();

            [Tooltip("Invoked when a conversation follows a link to another conversation. Transform is primary actor (typically player).")]
            public TransformEvent onLinkedConversationStart = new TransformEvent();
        }

        [System.Serializable]
        public class BarkEvents
        {
            [Tooltip("Invoked when a bark starts. Transform is recipient of bark.")]
            public TransformEvent onBarkStart = new TransformEvent();

            [Tooltip("Invoked when a bark ends. Transform is recipient of bark.")]
            public TransformEvent onBarkEnd = new TransformEvent();

            [Tooltip("Invoked just before a bark line is delivered. Passes Subtitle.")]
            public SubtitleEvent onBarkLine = new SubtitleEvent();
        }

        [System.Serializable]
        public class SequenceEvents
        {
            [Tooltip("Invoked when a sequence starts. Transform is 'listener' of sequence.")]
            public TransformEvent onSequenceStart = new TransformEvent();

            [Tooltip("Invoked when a sequence ends. Transform is 'listener' of sequence.")]
            public TransformEvent onSequenceEnd = new TransformEvent();
        }

        [System.Serializable]
        public class QuestEvents
        {
            [Tooltip("Invoked when a quest state or quest entry state changes. String is quest title.")]
            public StringEvent onQuestStateChange = new StringEvent();

            [Tooltip("Invoked when tracking is enabled for a quest. String is quest title.")]
            public StringEvent onQuestTrackingEnabled = new StringEvent();

            [Tooltip("Invoked when tracking is disabled for a quest. String is quest title.")]
            public StringEvent onQuestTrackingDisabled = new StringEvent();

            [Tooltip("Invoked when updating quest tracker.")]
            public UnityEvent onUpdateQuestTracker = new UnityEvent();
        }

        [System.Serializable]
        public class PauseEvents
        {
            [Tooltip("Invoked when DialogueManager.Pause() is called.")]
            public UnityEvent onDialogueSystemPause = new UnityEvent();

            [Tooltip("Invoked when DialogueManager.Unpause() is called.")]
            public UnityEvent onDialogueSystemUnpause = new UnityEvent();
        }


        public ConversationEvents conversationEvents = new ConversationEvents();

        public BarkEvents barkEvents = new BarkEvents();

        public SequenceEvents sequenceEvents = new SequenceEvents();

        public QuestEvents questEvents = new QuestEvents();

        public PauseEvents pauseEvents = new PauseEvents();

        private WaitForEndOfFrame endOfFrame = CoroutineUtility.endOfFrame;

        #region Conversation Events

        public void OnConversationStart(Transform actor)
        {
            conversationEvents.onConversationStart.Invoke(actor);
        }

        public void OnConversationEnd(Transform actor)
        {
            if (conversationEvents.runOnConversationEndEventsAtEndOfFrame)
            {
                StartCoroutine(InvokeOnConversationEndAtEndOfFrame(actor));
            }
            else
            {
                conversationEvents.onConversationEnd.Invoke(actor);
            }
        }

        private IEnumerator InvokeOnConversationEndAtEndOfFrame(Transform actor)
        {
            yield return endOfFrame;
            conversationEvents.onConversationEnd.Invoke(actor);
        }

        public void OnConversationCancelled(Transform actor)
        {
            conversationEvents.onConversationCancelled.Invoke(actor);
        }

        public void OnConversationLine(Subtitle subtitle)
        {
            conversationEvents.onConversationLine.Invoke(subtitle);
        }

        public void OnConversationLineEnd(Subtitle subtitle)
        {
            conversationEvents.onConversationLineEnd.Invoke(subtitle);
        }

        public void OnConversationLineCancelled(Subtitle subtitle)
        {
            conversationEvents.onConversationLineCancelled.Invoke(subtitle);
        }

        public void OnConversationResponseMenu(Response[] responses)
        {
            conversationEvents.onConversationResponseMenu.Invoke(responses);
        }

        public void OnConversationTimeout()
        {
            conversationEvents.onConversationResponseMenuTimeout.Invoke();
        }

        public void OnLinkedConversationStart(Transform actor)
        {
            conversationEvents.onLinkedConversationStart.Invoke(actor);
        }

        #endregion

        #region Bark Events

        public void OnBarkStart(Transform actor)
        {
            barkEvents.onBarkStart.Invoke(actor);
        }

        public void OnBarkEnd(Transform actor)
        {
            barkEvents.onBarkEnd.Invoke(actor);
        }

        public void OnBarkLine(Subtitle subtitle)
        {
            barkEvents.onBarkLine.Invoke(subtitle);
        }
        #endregion

        #region Sequence Events

        public void OnSequenceStart(Transform actor)
        {
            sequenceEvents.onSequenceStart.Invoke(actor);
        }

        public void OnSequenceEnd(Transform actor)
        {
            sequenceEvents.onSequenceEnd.Invoke(actor);
        }

        #endregion

        #region Quest Events

        public void OnQuestStateChange(string title)
        {
            questEvents.onQuestStateChange.Invoke(title);
        }

        public void OnQuestTrackingEnabled(string title)
        {
            questEvents.onQuestTrackingEnabled.Invoke(title);
        }

        public void OnQuestTrackingDisabled(string title)
        {
            questEvents.onQuestTrackingDisabled.Invoke(title);
        }

        public void UpdateTracker()
        {
            questEvents.onUpdateQuestTracker.Invoke();
        }

        #endregion

        #region Pause Events

        public void OnDialogueSystemPause()
        {
            pauseEvents.onDialogueSystemPause.Invoke();
        }

        public void OnDialogueSystemUnpause()
        {
            pauseEvents.onDialogueSystemUnpause.Invoke();
        }

        #endregion

    }

}
