using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// The bark trigger component can be used to make an NPC bark when it receives a dialogue
    /// trigger -- for example, when the game starts or when the level loads. You can specify an 
    /// optional target to bark at.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class BarkTrigger : BarkStarter
    {

        /// <summary>
        /// The target that the bark is directed to. If assigned, the target will get an
        /// OnBarkEnd event.
        /// </summary>
        [Tooltip("The target that the bark is directed to. If assigned, the target will get an OnBarkEnd event.")]
        public Transform target;

        /// <summary>
        /// The trigger that starts the conversation.
        /// </summary>
        [Tooltip("Event that starts the conversation.")]
        [DialogueTriggerEvent]
        public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;

        public void OnBarkEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnBarkEnd)) TryBark(Tools.Select(target, actor));
        }

        public void OnConversationEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnConversationEnd)) TryBark(Tools.Select(target, actor));
        }

        public void OnSequenceEnd(Transform actor)
        {
            if ((enabled && trigger == DialogueTriggerEvent.OnSequenceEnd)) TryBark(Tools.Select(target, actor));
        }

        public void OnUse(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryBark(Tools.Select(target, actor));
        }

        public void OnUse(string message)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryBark(Tools.Select(target, null));
        }

        public void OnUse()
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryBark(Tools.Select(target, null));
        }

        public void OnTriggerEnter(Collider other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryBark(Tools.Select(target, other.transform), other.transform);
        }

        public void OnTriggerExit(Collider other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryBark(Tools.Select(target, other.transform), other.transform);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnCollisionEnter)) TryBark(Tools.Select(target, collision.collider.transform), collision.collider.transform);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryBark(Tools.Select(target, collision.collider.transform), collision.collider.transform);
        }

#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryBark(Tools.Select(target, other.transform), other.transform);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryBark(Tools.Select(target, other.transform), other.transform);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryBark(Tools.Select(target, collision.collider.transform), collision.collider.transform);
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryBark(Tools.Select(target, collision.collider.transform), collision.collider.transform);
        }

#endif

        protected override void Start()
        {
            base.Start();
            // Waits one frame to allow all other components to finish their Start() methods.
            if (trigger == DialogueTriggerEvent.OnStart) StartCoroutine(BarkAfterOneFrame());
        }

        private bool listenForOnDestroy = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            listenForOnDestroy = true;
            // Waits one frame to allow all other components to finish their OnEnable() methods.
            if (trigger == DialogueTriggerEvent.OnEnable) StartCoroutine(BarkAfterOneFrame());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!listenForOnDestroy) return;
            if (trigger == DialogueTriggerEvent.OnDisable) TryBark(target);
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
            if (!listenForOnDestroy) return;
            if (trigger == DialogueTriggerEvent.OnDestroy) TryBark(target);
        }

        private IEnumerator BarkAfterOneFrame()
        {
            yield return null;
            TryBark(target);
        }

    }

}
