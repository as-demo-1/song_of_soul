using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// The quest trigger component sets a quest status when the game object receives a specified 
    /// trigger event. For example, you can add a quest trigger and a static trigger collider 
    /// to an area. When the player enters the trigger area, this component could set a quest status
    /// to success.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class QuestTrigger : DialogueEventStarter
    {

        /// <summary>
        /// The trigger that starts the conversation.
        /// </summary>
        [DialogueTriggerEvent]
        public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;

        /// <summary>
        /// The conditions under which the trigger will fire.
        /// </summary>
        public Condition condition;

        /// <summary>
        /// The name of the quest.
        /// </summary>
        public string questName;

        /// <summary>
        /// If `true`, set the quest state.
        /// </summary>
        [Tooltip("Set the main state of the quest.")]
        public bool setQuestState = true;

        /// <summary>
        /// The new state of the quest when triggered.
        /// </summary>
        [QuestState]
        public QuestState questState;

        /// <summary>
        /// If `true`, set the quest entry state.
        /// </summary>
        [Tooltip("Set the state of a quest entry (subtask) in the quest.")]
        public bool setQuestEntryState = false;

        /// <summary>
        /// The quest entry number whose state to change.
        /// </summary>
        [Tooltip("Quest entry number whose state to change.")]
        public int questEntryNumber = 1;

        /// <summary>
        /// The new state of the quest entry when triggered.
        /// </summary>
        [QuestState]
        public QuestState questEntryState;

        /// <summary>
        /// The lua code to run.
        /// </summary>
        [Tooltip("(Optional) Run this Lua code after setting the quest state.")]
        public string luaCode = string.Empty;

        /// <summary>
        /// An optional gameplay alert message. Leave blank for no message.
        /// </summary>
        [Tooltip("(Optional) Show this alert message after setting the quest state.")]
        public string alertMessage;

        /// <summary>
        /// An optional localized text table to use for the alert message.
        /// </summary>
        [Tooltip("Localized text table to use for the alert message.")]
        public LocalizedTextTable localizedTextTable;

        [Serializable]
        public class SendMessageAction
        {
            public GameObject gameObject = null;
            public string message = "OnUse";
            public string parameter = string.Empty;
        }

        /// <summary>
        /// Targets and messages to send when the trigger fires.
        /// </summary>
        public SendMessageAction[] sendMessages = new SendMessageAction[0];

        [HideInInspector]
        public bool useQuestNamePicker = true;

        [HideInInspector]
        public DialogueDatabase selectedDatabase = null;

        private bool tryingToStart = false;

        public void OnBarkEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnBarkEnd)) TryStart(actor);
        }

        public void OnConversationEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnConversationEnd)) TryStart(actor);
        }

        public void OnSequenceEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnSequenceEnd)) TryStart(actor);
        }

        public void OnUse(Transform actor)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStart(actor);
        }

        public void OnUse(string message)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStart(null);
        }

        public void OnUse()
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStart(null);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStart(other.transform);
        }

        public void OnTriggerExit(Collider other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStart(other.transform);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnCollisionEnter)) TryStart(collision.collider.transform);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStart(collision.collider.transform);
        }

#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStart(other.transform);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStart(other.transform);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStart(collision.collider.transform);
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueTriggerEvent.OnTriggerExit)) TryStart(collision.collider.transform);
        }

#endif

        public void Start()
        {
            // Waits one frame to allow all other components to finish their Start() methods.
            if (trigger == DialogueTriggerEvent.OnStart) StartCoroutine(StartAfterOneFrame());
        }

        private bool listenForOnDestroy = false;

        public void OnEnable()
        {
            listenForOnDestroy = true;
            // Waits one frame to allow all other components to finish their OnEnable() methods.
            if (trigger == DialogueTriggerEvent.OnEnable) StartCoroutine(StartAfterOneFrame());
        }

        public void OnDisable()
        {
            if (listenForOnDestroy && trigger == DialogueTriggerEvent.OnDisable) TryStart(null);
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
            if (listenForOnDestroy && trigger == DialogueTriggerEvent.OnDestroy) TryStart(null);
        }

        private IEnumerator StartAfterOneFrame()
        {
            yield return null;
            TryStart(null);
        }

        /// <summary>
        /// Sets the quest status if the condition is true.
        /// </summary>
        public void TryStart(Transform actor)
        {
            if (tryingToStart) return;
            tryingToStart = true;
            try
            {
                if (((condition == null) || condition.IsTrue(actor)) && !string.IsNullOrEmpty(questName))
                {
                    Fire();
                }
            }
            finally
            {
                tryingToStart = false;
            }
        }

        public void Fire()
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Setting quest '{1}' state to '{2}'", new System.Object[] { DialogueDebug.Prefix, questName, QuestLog.StateToString(questState) }));

            // Quest states:
            if (!string.IsNullOrEmpty(questName))
            {
                if (setQuestState) QuestLog.SetQuestState(questName, questState);
                if (setQuestEntryState) QuestLog.SetQuestEntryState(questName, questEntryNumber, questEntryState);
            }

            // Lua:
            if (!string.IsNullOrEmpty(luaCode))
            {
                Lua.Run(luaCode, DialogueDebug.logInfo);
            }

            // Alert:
            if (!string.IsNullOrEmpty(alertMessage))
            {
                string localizedAlertMessage = alertMessage;
                if ((localizedTextTable != null) && localizedTextTable.ContainsField(alertMessage))
                {
                    localizedAlertMessage = localizedTextTable[alertMessage];
                }
                DialogueManager.ShowAlert(localizedAlertMessage);
            }

            // Send Messages:
            foreach (var sma in sendMessages)
            {
                if (sma.gameObject != null && !string.IsNullOrEmpty(sma.message))
                {
                    sma.gameObject.SendMessage(sma.message, sma.parameter, SendMessageOptions.DontRequireReceiver);
                }
            }

            DialogueManager.SendUpdateTracker();

            // Once?
            DestroyIfOnce();
        }

    }

}
