using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// The sequence trigger component starts a sequence when the game object receives a specified 
    /// dialogue event. For example, you can add a sequence trigger and a static trigger collider 
    /// to a cutscene area. When the player enters the trigger area, this component will start the
    /// sequence.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class SequenceTrigger : SequenceStarter
    {

        /// <summary>
        /// The trigger that starts the conversation.
        /// </summary>
        [Tooltip("Trigger that starts the sequence.")]
        [DialogueTriggerEvent]
        public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;

        [Tooltip("Tick to wait one frame to allow other components to finish their OnStart/OnEnable.")]
        public bool waitOneFrameOnStartOrEnable = true;

        public void OnBarkEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnBarkEnd)) TryStartSequence(actor);
        }

        public void OnConversationEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnConversationEnd)) TryStartSequence(actor);
        }

        public void OnSequenceEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnSequenceEnd)) TryStartSequence(actor);
        }

        public void OnUse(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartSequence(actor);
        }

        public void OnUse(string message)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartSequence(null);
        }

        public void OnUse()
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartSequence(null);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartSequence(other.transform, other.transform);
        }

        public void OnTriggerExit(Collider other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStartSequence(other.transform, other.transform);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnCollisionEnter)) TryStartSequence(collision.collider.transform, collision.collider.transform);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStartSequence(collision.collider.transform, collision.collider.transform);
        }

#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartSequence(other.transform, other.transform);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStartSequence(other.transform, other.transform);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartSequence(collision.collider.transform, collision.collider.transform);
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStartSequence(collision.collider.transform, collision.collider.transform);
        }

#endif

        public void Start()
        {
            // Waits one frame to allow all other components to finish their Start() methods.
            if (trigger == DialogueTriggerEvent.OnStart) StartCoroutine(StartSequenceAfterOneFrame());
        }

        private bool listenForOnDestroy = false;

        public void OnEnable()
        {
            listenForOnDestroy = true;
            // Waits one frame to allow all other components to finish their OnEnable() methods.
            if (trigger == DialogueTriggerEvent.OnEnable) StartCoroutine(StartSequenceAfterOneFrame());
        }

        public void OnDisable()
        {
            if (listenForOnDestroy && trigger == DialogueTriggerEvent.OnDisable) TryStartSequence(null);
        }

        public void OnLevelWillBeUnloaded()
        {
            listenForOnDestroy = false;
        }

        public void OnApplicationQuit()
        {
            listenForOnDestroy = false;
        }

        public void OnDestroy()
        {
            if (listenForOnDestroy && trigger == DialogueTriggerEvent.OnDestroy) TryStartSequence(null);
        }

        private IEnumerator StartSequenceAfterOneFrame()
        {
            if (waitOneFrameOnStartOrEnable) yield return null;
            TryStartSequence(null);
        }

    }

}
