// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// This is the abstract base class for all dialogue event handler components (e.g., 
    /// SetActiveOnDialogueEvent, StartConversationOnDialogueEvent, etc). Dialogue events occur
    /// when a bark, conversation, or sequence starts and ends. Subclasses implement 
    /// TryStartActions() and TryEndActions().
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public abstract class ActOnDialogueEvent : MonoBehaviour
    {

        /// <summary>
        /// The base class for actions that will run at the start or end of dialogue events.
        /// </summary>
        [System.Serializable]
        public class Action
        {
            public Condition condition = new Condition();
        }

        /// <summary>
        /// The dialogue event that triggers the actions.
        /// </summary>
        [Tooltip("Trigger when this dialogue event occurs.")]
        public DialogueEvent trigger;

        /// <summary>
        /// Set <c>true</c> if this should only happen once.
        /// </summary>
        [Tooltip("Destroy this component after triggering. If you need to remember across scene changes and saved games, use a Condition instead.")]
        public bool once = false;

        [HideInInspector]
        public DialogueDatabase selectedDatabase = null;

        /// <summary>
        /// Handles OnBarkStart events.
        /// </summary>
        /// <param name='actor'>
        /// Actor that barked.
        /// </param>
        public void OnBarkStart(Transform actor)
        {
            if (enabled && (trigger == DialogueEvent.OnBark)) TryStartActions(actor);
        }

        /// <summary>
        /// Handles OnBarkEnd events.
        /// </summary>
        /// <param name='actor'>
        /// Actor that barked.
        /// </param>
        public void OnBarkEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueEvent.OnBark))
            {
                TryEndActions(actor);
                DestroyIfOnce();
            }
        }

        /// <summary>
        /// Handles OnConversationStart events.
        /// </summary>
        /// <param name='actor'>
        /// Actor that initiated the conversation.
        /// </param>
        public void OnConversationStart(Transform actor)
        {
            if (enabled && (trigger == DialogueEvent.OnConversation)) TryStartActions(actor);
        }

        /// <summary>
        /// Handles OnConversationEnd events.
        /// </summary>
        /// <param name='actor'>
        /// Actor that initiated the conversation.
        /// </param>
        public void OnConversationEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueEvent.OnConversation))
            {
                TryEndActions(actor);
                DestroyIfOnce();
            }
        }

        /// <summary>
        /// Handles OnSequenceStart events.
        /// </summary>
        /// <param name='actor'>
        /// The primary actor in the sequence.
        /// </param>
        public void OnSequenceStart(Transform actor)
        {
            if (enabled && (trigger == DialogueEvent.OnSequence)) TryStartActions(actor);
        }

        /// <summary>
        /// Handles OnSequenceEnd events.
        /// </summary>
        /// <param name='actor'>
        /// The primary actor in the sequence.
        /// </param>
        public void OnSequenceEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueEvent.OnSequence))
            {
                TryEndActions(actor);
                DestroyIfOnce();
            }
        }

        /// <summary>
        /// Tries the actions that should run when the event starts (e.g., OnBarkStart).
        /// </summary>
        /// <param name='actor'>
        /// Actor.
        /// </param>
        public abstract void TryStartActions(Transform actor);

        /// <summary>
        /// Tries the actions that should run when the event ends (e.g., OnBarkEnd).
        /// </summary>
        /// <param name='actor'>
        /// Actor.
        /// </param>
        public abstract void TryEndActions(Transform actor);

        private void DestroyIfOnce()
        {
            if (once) Destroy(this);
        }

    }

}
