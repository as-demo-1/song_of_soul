// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The Condition Observer component evaluates a condition on a set frequency. When the
    /// condition is true, it sends a message to a list of GameObjects and shows a gameplay
    /// alert message.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ConditionObserver : MonoBehaviour
    {

        /// <summary>
        /// The frequency at which to check the condition.
        /// </summary>
        [Tooltip("Frequency in seconds between checks.")]
        public float frequency = 1;

        /// <summary>
        /// When observed condition becomes true, run actions and then deactivate this component.
        /// </summary>
        [Tooltip("When observed condition becomes true, run actions and then deactivate this component.")]
        public bool once;

        /// <summary>
        /// Observe this game object when evaluating the condition.
        /// </summary>
        [Tooltip("Refer to this GameObject when evaluating the Condition.")]
        public GameObject observeGameObject = null;

        /// <summary>
        /// The conditions under which the trigger will fire.
        /// </summary>
        public Condition condition = new Condition();

        /// <summary>
        /// The name of the quest to update when the condition is true. Blank for none.
        /// </summary>
        [Tooltip("Set this quest's state when the condition is true.")]
        public string questName = string.Empty;

        /// <summary>
        /// The new state of the quest.
        /// </summary>
        [Tooltip("Set the quest to this state when the condition is true.")]
        [QuestState]
        public QuestState questState;

        /// <summary>
        /// The lua code to run.
        /// </summary>
        [Tooltip("Run this Lua code when the condition is true. Leave blank to skip.")]
        public string luaCode = string.Empty;

        /// <summary>
        /// The sequence to play.
        /// </summary>
        [Tooltip("Play this sequence when the condition is true. Leave blank to skip.")]
        [TextArea(1, 20)]
        public string sequence = string.Empty;

        /// <summary>
        /// An optional gameplay alert message. Leave blank for no message.
        /// </summary>
        [Tooltip("Show this alert message when the condition is true. Leave blank to skip.")]
        public string alertMessage = string.Empty;

        /// <summary>
        /// An optional localized text table to use for the alert message.
        /// </summary>
        [Tooltip("Text table to use to localize alert message.")]
        public TextTable textTable = null;

        [Serializable]
        public class SendMessageAction
        {
            public GameObject gameObject = null;
            public string message = "OnUse";
            public string parameter = string.Empty;
        }

        public SendMessageAction[] sendMessages = new SendMessageAction[0];

        [HideInInspector]
        public bool useQuestNamePicker = true;

        private bool started = false;

        private void Start()
        {
            started = true;
            StartObserving();
        }

        private void OnEnable()
        {
            if (started) StartObserving();
        }

        private void OnDisable()
        {
            StopObserving();
        }

        private void StartObserving()
        {
            StopObserving();
            StartCoroutine(Observe());
        }

        private void StopObserving()
        {
            StopAllCoroutines();
        }

        private IEnumerator Observe()
        {
            yield return new WaitForSeconds(UnityEngine.Random.value);
            while (true)
            {
                Check();
                yield return new WaitForSeconds(frequency);
            }
        }

        /// <summary>
        /// Call this method to manually check the condition and fire the action
        /// if it's true.
        /// </summary>
        public void Check()
        {
            var observeTransform = (observeGameObject == null) ? null : observeGameObject.transform;
            if (condition.IsTrue(observeTransform))
            {
                Fire();
            }
        }

        /// <summary>
        /// Sets the observed GameObject and checks the condition.
        /// </summary>
        /// <param name="gameObject">Game object.</param>
        public void Check(GameObject gameObject)
        {
            observeGameObject = gameObject;
            Check();
        }

        /// <summary>
        /// Sets the observed GameObject to the named GameObject and checks 
        /// the condition.
        /// </summary>
        /// <param name="gameObjectName">Game object name.</param>
        public void Check(string gameObjectName)
        {
            var newGameObject = Tools.GameObjectHardFind(gameObjectName);
            if (newGameObject != null) observeGameObject = newGameObject;
            Check();
        }

        /// <summary>
        /// Call this method to manually run the action.
        /// </summary>
        public void Fire()
        {
            // Quest:
            if (!string.IsNullOrEmpty(questName))
            {
                QuestLog.SetQuestState(questName, questState);
            }

            // Lua:
            if (!string.IsNullOrEmpty(luaCode))
            {
                Lua.Run(luaCode, DialogueDebug.logInfo);
                DialogueManager.CheckAlerts();
            }

            // Sequence:
            if (!string.IsNullOrEmpty(sequence))
            {
                DialogueManager.PlaySequence(sequence);
            }

            // Alert:
            if (!string.IsNullOrEmpty(alertMessage))
            {
                string localizedAlertMessage;
                if ((textTable != null) && textTable.HasFieldTextForLanguage(alertMessage, Localization.GetCurrentLanguageID(textTable)))
                {
                    localizedAlertMessage = textTable.GetFieldTextForLanguage(alertMessage, Localization.GetCurrentLanguageID(textTable));
                }
                else
                {
                    localizedAlertMessage = DialogueManager.GetLocalizedText(alertMessage);
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

            if (once)
            {
                StopObserving();
                enabled = false;
            }
        }
    }

}
