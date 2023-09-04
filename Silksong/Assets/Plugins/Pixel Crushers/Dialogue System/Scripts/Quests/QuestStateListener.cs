// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this to a GameObject such as an NPC that wants to know about quest state changes
    /// to a specific quest. You can add multiple QuestStateListener components to listen
    /// to multiple quests.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class QuestStateListener : MonoBehaviour
    {

        [QuestPopup(true)]
        public string questName;

        [Serializable]
        public class QuestStateIndicatorLevel
        {
            [Tooltip("Quest state to listen for.")]
            public QuestState questState;

            [Tooltip("Conditions that must also be true.")]
            public Condition condition;

            [Tooltip("Indicator level to use when this quest state is reached.")]
            public int indicatorLevel;

            public UnityEvent onEnterState = new UnityEvent();
        }

        public QuestStateIndicatorLevel[] questStateIndicatorLevels = new QuestStateIndicatorLevel[0];

        [Serializable]
        public class QuestEntryStateIndicatorLevel
        {
            [Tooltip("Quest entry number.")]
            public int entryNumber;

            [Tooltip("Quest entry state to listen for.")]
            public QuestState questState;

            [Tooltip("Conditions that must also be true.")]
            public Condition condition;

            [Tooltip("Indicator level to use when this quest state is reached.")]
            public int indicatorLevel;

            public UnityEvent onEnterState = new UnityEvent();
        }

        public QuestEntryStateIndicatorLevel[] questEntryStateIndicatorLevels = new QuestEntryStateIndicatorLevel[0];

        [Tooltip("When starting component, do not invoke any OnEnterState() events.")]
        public bool suppressOnEnterStateEventsOnStart = false;

        protected QuestStateDispatcher m_questStateDispatcher;
        protected QuestStateDispatcher questStateDispatcher
        {
            get
            {
                if (m_questStateDispatcher == null)
                {
                    if (DialogueManager.instance != null)
                    {
                        m_questStateDispatcher = DialogueManager.instance.GetComponent<QuestStateDispatcher>();
                        if (m_questStateDispatcher == null)
                        {
                            m_questStateDispatcher = GameObjectUtility.FindFirstObjectByType<QuestStateDispatcher>();
                            if (m_questStateDispatcher == null)
                            {
                                m_questStateDispatcher = DialogueManager.instance.gameObject.AddComponent<QuestStateDispatcher>();
                            }
                        }
                    }
                    else
                    {
                        m_questStateDispatcher = GameObjectUtility.FindFirstObjectByType<QuestStateDispatcher>();
                        if (m_questStateDispatcher == null)
                        {
                            var go = new GameObject("QuestStateDispatcher");
                            DontDestroyOnLoad(go);
                            m_questStateDispatcher = go.AddComponent<QuestStateDispatcher>();
                        }
                    }
                }
                return m_questStateDispatcher;
            }
        }
        protected QuestStateIndicator m_questStateIndicator;
        protected QuestStateIndicator questStateIndicator
        {
            get
            {
                if (m_questStateIndicator == null) m_questStateIndicator = GetComponent<QuestStateIndicator>();
                return m_questStateIndicator;
            }
        }
        private bool m_started = false;
        protected bool started
        {
            get { return m_started; }
            set { m_started = value; }
        }

        protected bool m_suppressOnEnterStateEvent = false;

        protected virtual void OnApplicationQuit()
        {
            enabled = false;
        }

        protected virtual IEnumerator Start()
        {
            if (enabled)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": Listening for state changes to quest '" + questName + "'.", this);
                started = true;
                if (questStateDispatcher == null)
                {
                    if (DialogueDebug.logErrors) Debug.LogWarning("Dialogue System: Unexpected error. Quest State Listener on " + name + " can't find or create a Quest State Dispatcher.", this);
                }
                else
                {
                    questStateDispatcher.AddListener(this);
                }
                yield return null;
                m_suppressOnEnterStateEvent = suppressOnEnterStateEventsOnStart;
                UpdateIndicator();
                m_suppressOnEnterStateEvent = false;
            }
        }

        protected virtual void OnEnable()
        {
            if (started) questStateDispatcher.AddListener(this);
        }

        protected virtual void OnDisable()
        {
            if (m_questStateDispatcher != null) m_questStateDispatcher.RemoveListener(this); // Use private; don't create new quest state dispatcher.
        }

        public virtual void OnChange()
        {
            UpdateIndicator();
        }

        /// <summary>
        /// Update the current quest state indicator based on the specified quest state indicator 
        /// levels and quest entry state indicator levels.
        /// </summary>
        public virtual void UpdateIndicator()
        {
            // Check quest state:
            var questState = QuestLog.GetQuestState(questName);
            for (int i = 0; i < questStateIndicatorLevels.Length; i++)
            {
                var questStateIndicatorLevel = questStateIndicatorLevels[i];
                if (((questState & questStateIndicatorLevel.questState) != 0) && questStateIndicatorLevel.condition.IsTrue(null))
                {
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": Quest '" + questName + "' changed to state " + questState + ".", this);
                    if (questStateIndicator != null) questStateIndicator.SetIndicatorLevel(this, questStateIndicatorLevel.indicatorLevel);
                    if (!m_suppressOnEnterStateEvent)
                    {
                        questStateIndicatorLevel.onEnterState.Invoke();
                    }
                }
            }

            // Check quest entry states:
            for (int i = 0; i < questEntryStateIndicatorLevels.Length; i++)
            {
                var questEntryStateIndicatorLevel = questEntryStateIndicatorLevels[i];
                var questEntryState = QuestLog.GetQuestEntryState(questName, questEntryStateIndicatorLevel.entryNumber);
                if (((questEntryState & questEntryStateIndicatorLevel.questState) != 0) && questEntryStateIndicatorLevel.condition.IsTrue(null))
                {
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": Quest '" + questName + "' entry " + questEntryStateIndicatorLevel.entryNumber + " changed to state " + questEntryState + ".", this);
                    if (questStateIndicator != null) questStateIndicator.SetIndicatorLevel(this, questEntryStateIndicatorLevel.indicatorLevel);
                    if (!m_suppressOnEnterStateEvent)
                    {
                        questEntryStateIndicatorLevel.onEnterState.Invoke();
                    }
                }
            }
        }

    }
}