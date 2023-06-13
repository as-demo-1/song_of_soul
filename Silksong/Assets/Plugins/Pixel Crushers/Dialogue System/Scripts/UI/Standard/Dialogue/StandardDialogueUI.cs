// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [AddComponentMenu("")] // Use wrapper.
    public class StandardDialogueUI : CanvasDialogueUI, IStandardDialogueUI
    {

        #region Serialized Fields

        public StandardUIAlertControls alertUIElements;
        public StandardUIDialogueControls conversationUIElements;
        public StandardUIQTEControls QTEIndicatorElements;

        [Tooltip("Add an EventSystem if one isn't in the scene.")]
        public bool addEventSystemIfNeeded = true;

        #endregion

        #region Properties & Private Fields

        private Queue<QueuedUIAlert> m_alertQueue = new Queue<QueuedUIAlert>();
        private StandardUIRoot m_uiRoot = new StandardUIRoot();
        private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        public override AbstractUIRoot uiRootControls { get { return m_uiRoot; } }
        public override AbstractUIAlertControls alertControls { get { return alertUIElements; } }
        public override AbstractDialogueUIControls dialogueControls { get { return conversationUIElements; } }
        public override AbstractUIQTEControls qteControls { get { return QTEIndicatorElements; } }

        protected Queue<QueuedUIAlert> alertQueue { get { return m_alertQueue; } }

        protected Coroutine closeCoroutine = null;

        protected const float WaitForOpenTimeoutDuration = 8f;

        #endregion

        #region Initialization

        /// <summary>
        /// Sets up the component.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            VerifyAssignments();
            conversationUIElements.Initialize();
            alertUIElements.HideImmediate();
            conversationUIElements.HideImmediate();
            QTEIndicatorElements.HideImmediate();
        }

        private void VerifyAssignments()
        {
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
            if (DialogueDebug.logWarnings)
            {
                if (alertUIElements.alertText.gameObject == null) Debug.LogWarning("Dialogue System: No UI text element is assigned to Standard Dialogue UI's Alert UI Elements.", this);
                if (conversationUIElements.subtitlePanels.Length == 0) Debug.LogWarning("Dialogue System: No subtitle panels are assigned to Standard Dialogue UI.", this);
                if (conversationUIElements.menuPanels.Length == 0) Debug.LogWarning("Dialogue System: No response menu panels are assigned to Standard Dialogue UI.", this);
            }
        }

#if UNITY_5_3 // SceneManager.sceneLoaded wasn't implemented for all Unity 5.3.x versions.
        public void OnLevelWasLoaded(int level)
        {
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
        }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
#else
        public virtual void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public virtual void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
        }
#endif

        public override void Open()
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }
            base.Open();
            conversationUIElements.OpenSubtitlePanelsOnStart(this);
        }

        public override void Close()
        {
            if (conversationUIElements.waitForClose && (AreAnyPanelsClosing() || !IsMainPanelClosed()))
            {
                closeCoroutine = StartCoroutine(CloseAfterPanelsAreClosed());
            }
            else
            {
                CloseNow();
            }
        }

        protected virtual void CloseNow()
        {
            base.Close();
            conversationUIElements.ClearCaches();
        }

        protected IEnumerator CloseAfterPanelsAreClosed()
        {
            // Close subtitle/menu panels and wait for them to finish:
            conversationUIElements.ClosePanels();
            while (AreAnyPanelsClosing())
            {
                yield return null;
            }
            // Close main panel and wait for it to finish:
            if (conversationUIElements.mainPanel != null)
            {
                if (DialogueSystemController.isWarmingUp)
                {
                    conversationUIElements.mainPanel.animatorMonitor.CancelCurrentAnimation();
                    conversationUIElements.mainPanel.gameObject.SetActive(false);
                    conversationUIElements.mainPanel.panelState = UIPanel.PanelState.Closed;
                }
                else
                {
                    conversationUIElements.mainPanel.Close();
                    while (conversationUIElements.mainPanel.panelState == UIPanel.PanelState.Closing)
                    {
                        yield return null;
                    }
                }
            }
            CloseNow();
        }

        protected virtual bool IsMainPanelClosed()
        {
            return conversationUIElements.mainPanel == null ||
                conversationUIElements.mainPanel.panelState == UIPanel.PanelState.Closed;
        }

        // extraSubtitlePanel may be a custom (e.g., bubble) panel that isn't part of the dialogue UI's regular list.
        public virtual bool AreAnyPanelsClosing(StandardUISubtitlePanel extraSubtitlePanel = null)
        {
            return conversationUIElements.AreAnyPanelsClosing(extraSubtitlePanel);
        }

        #endregion

        #region Update

        public override void Update()
        {
            base.Update();
            UpdateAlertQueue();
        }

        #endregion

        #region Alerts

        public override void ShowAlert(string message, float duration)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (alertUIElements.dontQueueDuplicates)
            {
                if (alertUIElements.isVisible && string.Equals(alertUIElements.alertText.text, message)) return;
                foreach (var queuedItem in alertQueue)
                {
                    if (string.Equals(message, queuedItem.message)) return;
                }
            }
            if (alertUIElements.allowForceImmediate && message.Contains("[f]"))
            {
                base.ShowAlert(message.Replace("[f]", string.Empty), duration);
            }
            else if (alertUIElements.queueAlerts)
            {
                m_alertQueue.Enqueue(new QueuedUIAlert(message, duration));
            }
            else
            {
                base.ShowAlert(message, duration);
            }
        }

        private void UpdateAlertQueue()
        {
            if (alertUIElements.queueAlerts && m_alertQueue.Count > 0 && !alertUIElements.isVisible && !(alertUIElements.waitForHideAnimation && alertUIElements.isHiding))
            {
                ShowNextQueuedAlert();
            }
        }

        private void ShowNextQueuedAlert()
        {
            if (m_alertQueue.Count > 0)
            {
                var queuedAlert = m_alertQueue.Dequeue();
                base.ShowAlert(queuedAlert.message, queuedAlert.duration);
            }
        }

        #endregion

        #region Subtitles

        public override void ShowSubtitle(Subtitle subtitle)
        {
            if (conversationUIElements.waitForMainPanelOpen && conversationUIElements.mainPanel != null && conversationUIElements.mainPanel.panelState != UIPanel.PanelState.Open)
            {
                StartCoroutine(ShowSubtitleWhenMainPanelOpen(subtitle));
            }
            else
            {
                ShowSubtitleImmediate(subtitle);
            }
        }

        protected virtual IEnumerator ShowSubtitleWhenMainPanelOpen(Subtitle subtitle)
        {
            if (conversationUIElements.mainPanel == null)
            {
                ShowSubtitleImmediate(subtitle);
            }
            else
            {
                var focusedPanel = conversationUIElements.standardSubtitleControls.StageFocusedPanel(subtitle);
                float timeout = Time.realtimeSinceStartup + WaitForOpenTimeoutDuration;
                var showContinueButton = false;
                while (conversationUIElements.mainPanel.panelState != UIPanel.PanelState.Open && Time.realtimeSinceStartup < timeout)
                {
                    yield return endOfFrame;
                    var isContinueButtonActive = focusedPanel != null && focusedPanel.continueButton != null && focusedPanel.continueButton.gameObject.activeSelf;
                    showContinueButton = showContinueButton || isContinueButtonActive;
                    if (isContinueButtonActive)
                    {
                        focusedPanel.continueButton.gameObject.SetActive(false);
                    }
                    yield return null;
                }
                ShowSubtitleImmediate(subtitle);
                if (showContinueButton) focusedPanel.ShowContinueButton();
            }
        }

        protected virtual void ShowSubtitleImmediate(Subtitle subtitle)
        {
            conversationUIElements.standardMenuControls.Close();
            conversationUIElements.standardSubtitleControls.ShowSubtitle(subtitle);
        }

        public override void HideSubtitle(Subtitle subtitle)
        {
            conversationUIElements.standardSubtitleControls.HideSubtitle(subtitle);
        }

        /// <summary>
        /// Returns the speed of the first typewriter effect found.
        /// </summary>
        public virtual float GetTypewriterSpeed()
        {
            return conversationUIElements.standardSubtitleControls.GetTypewriterSpeed();
        }

        /// <summary>
        /// Sets the speed of all typewriter effects.
        /// </summary>
        public virtual void SetTypewriterSpeed(float charactersPerSecond)
        {
            conversationUIElements.standardSubtitleControls.SetTypewriterSpeed(charactersPerSecond);
        }

        /// <summary>
        /// Changes a dialogue actor's subtitle panel for the currently active conversation.
        /// </summary>
        public virtual void SetActorSubtitlePanelNumber(DialogueActor dialogueActor, SubtitlePanelNumber subtitlePanelNumber)
        {
            conversationUIElements.standardSubtitleControls.SetActorSubtitlePanelNumber(dialogueActor, subtitlePanelNumber);
        }

        /// <summary>
        /// Changes a dialogue actor's menu panel for the currently active conversation.
        /// </summary>
        public virtual void SetActorMenuPanelNumber(DialogueActor dialogueActor, MenuPanelNumber menuPanelNumber)
        {
            conversationUIElements.standardMenuControls.SetActorMenuPanelNumber(dialogueActor, menuPanelNumber);
        }

        public virtual void OverrideActorPanel(Actor actor, SubtitlePanelNumber subtitlePanelNumber)
        {
            conversationUIElements.standardSubtitleControls.OverrideActorPanel(actor, subtitlePanelNumber);
        }

        public virtual void ForceOverrideSubtitlePanel(StandardUISubtitlePanel customPanel)
        {
            conversationUIElements.standardSubtitleControls.ForceOverrideSubtitlePanel(customPanel);
        }

        #endregion

        #region Response Menu

        public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
        {
            if (conversationUIElements.waitForMainPanelOpen && conversationUIElements.mainPanel != null && conversationUIElements.mainPanel.panelState != UIPanel.PanelState.Open)
            {
                StartCoroutine(ShowResponsesWhenMainPanelOpen(subtitle, responses, timeout));
            }
            else
            {
                ShowResponsesImmediate(subtitle, responses, timeout);
            }
        }

        protected virtual IEnumerator ShowResponsesWhenMainPanelOpen(Subtitle subtitle, Response[] responses, float timeout)
        {
            if (conversationUIElements.mainPanel == null) yield break;
            float waitForOpenTimeout = Time.realtimeSinceStartup + WaitForOpenTimeoutDuration;
            while (conversationUIElements.mainPanel.panelState != UIPanel.PanelState.Open && Time.realtimeSinceStartup < waitForOpenTimeout)
            {
                yield return null;
            }
            ShowResponsesImmediate(subtitle, responses, timeout);
        }

        protected virtual void ShowResponsesImmediate(Subtitle subtitle, Response[] responses, float timeout)
        { 
            conversationUIElements.standardSubtitleControls.UnfocusAll();
            base.ShowResponses(subtitle, responses, timeout);
        }

        public override void OnClick(object data)
        {
            conversationUIElements.standardMenuControls.MakeButtonsNonclickable();
            base.OnClick(data);
        }

        public virtual void OverrideActorMenuPanel(Transform actorTransform, MenuPanelNumber menuPanelNumber, StandardUIMenuPanel customPanel)
        {
            conversationUIElements.standardMenuControls.OverrideActorMenuPanel(actorTransform, menuPanelNumber, customPanel ?? conversationUIElements.defaultMenuPanel);
        }

        public virtual void OverrideActorMenuPanel(Actor actor, MenuPanelNumber menuPanelNumber, StandardUIMenuPanel customPanel)
        {
            conversationUIElements.standardMenuControls.OverrideActorMenuPanel(actor, menuPanelNumber, customPanel ?? conversationUIElements.defaultMenuPanel);
        }

        public virtual void ForceOverrideMenuPanel(StandardUIMenuPanel customPanel)
        {
            conversationUIElements.standardMenuControls.ForceOverrideMenuPanel(customPanel);
        }

        #endregion

    }

}
