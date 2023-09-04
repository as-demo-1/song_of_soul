// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component implements IDialogueUI using Unity UI. It's based on 
    /// CanvasDialogueUI and compiles the Unity UI versions of the controls defined in 
    /// UnityUISubtitleControls, UnityUIResponseMenuControls, UnityUIAlertControls, etc.
    ///
    /// To use this component, build a UI layout (or drag a pre-built one in the Prefabs folder
    /// into your scene) and assign the UI control properties. You must assign a scene instance 
    /// to the DialogueManager; you can't use prefabs with Unity UI dialogue UIs.
    /// 
    /// The required controls are:
    /// - NPC subtitle line
    /// - PC subtitle line
    /// - Response menu buttons
    /// 
    /// The other control properties are optional. This component will activate and deactivate
    /// controls as they are needed in the conversation.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUIDialogueUI : CanvasDialogueUI
    {

        /// <summary>
        /// The UI root.
        /// </summary>
        [HideInInspector]
        public UnityUIRoot unityUIRoot;

        /// <summary>
        /// The dialogue controls used in conversations.
        /// </summary>
        public UnityUIDialogueControls dialogue;

        /// <summary>
        /// QTE (Quick Time Event) indicators.
        /// </summary>
        public UnityEngine.UI.Graphic[] qteIndicators;

        /// <summary>
        /// The alert message controls.
        /// </summary>
        public UnityUIAlertControls alert;

        /// <summary>
        /// Set <c>true</c> to always keep a control focused; useful for gamepads.
        /// </summary>
        [Tooltip("Always keep a control focused; useful for gamepads and keyboard.")]
        public bool autoFocus = false;

        /// <summary>
        /// Allow the dialogue UI to steal focus if a non-dialogue UI panel has it.
        /// </summary>
        [Tooltip("Allow the dialogue UI to steal focus if a non-dialogue UI panel has it.")]
        public bool allowStealFocus = false;

        /// <summary>
        /// If auto focusing, check on this frequency in seconds that the control is focused.
        /// </summary>
        [Tooltip("If auto focusing, check on this frequency in seconds that the control is focused.")]
        public float autoFocusCheckFrequency = 0.5f;

        /// <summary>
        /// Set <c>true</c> to look for OverrideUnityUIDialogueControls on actors.
        /// </summary>
		[Tooltip("Look for OverrideUnityUIDialogueControls on actors.")]
        public bool findActorOverrides = true;

        /// <summary>
        /// Set <c>true</c> to add an EventSystem if one isn't in the scene.
        /// </summary>
        [Tooltip("Add an EventSystem if one isn't in the scene.")]
        public bool addEventSystemIfNeeded = true;

        private UnityUIQTEControls m_qteControls;
        private float m_nextAutoFocusCheckTime = 0;
        private GameObject m_lastSelection = null;

        public override AbstractUIRoot uiRootControls
        {
            get { return unityUIRoot; }
        }

        public override AbstractDialogueUIControls dialogueControls
        {
            get { return dialogue; }
        }

        public override AbstractUIQTEControls qteControls
        {
            get { return m_qteControls; }
        }

        public override AbstractUIAlertControls alertControls
        {
            get { return alert; }
        }

        private class QueuedAlert
        {
            public string message;
            public float duration;
            public QueuedAlert(string message, float duration)
            {
                this.message = message;
                this.duration = duration;
            }
        }

        private Queue<QueuedAlert> alertQueue = new Queue<QueuedAlert>();

        // References to the original controls in case an actor temporarily overrides them:
        protected UnityUISubtitleControls originalNPCSubtitle;
        protected UnityUISubtitleControls originalPCSubtitle;
        protected UnityUIResponseMenuControls originalResponseMenu;

        // Caches overrides by actor so we only need to search an actor once:
        private Dictionary<Transform, OverrideUnityUIDialogueControls> overrideCache = new Dictionary<Transform, OverrideUnityUIDialogueControls>();

        private bool isShowingNpcSubtitle = false;
        private bool isShowingPcSubtitle = false;
        private bool isShowingResponses = false;

        #region Initialization

        /// <summary>
        /// Sets up the component.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            FindControls();
            alert.DeactivateUIElements();
            dialogue.DeactivateUIElements();
            Tools.DeprecationWarning(this, "Use StandardDialogueUI instead.");
        }

#if !(UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
        public virtual void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public virtual void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#endif

        /// <summary>
        /// Logs warnings if any critical controls are unassigned.
        /// </summary>
        private void FindControls()
        {
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
            m_qteControls = new UnityUIQTEControls(qteIndicators);
            if (DialogueDebug.logErrors)
            {
                if (DialogueDebug.logWarnings)
                {
                    if (dialogue.npcSubtitle.line == null) Debug.LogWarning(string.Format("{0}: UnityUIDialogueUI NPC Subtitle Line needs to be assigned.", DialogueDebug.Prefix));
                    if (dialogue.pcSubtitle.line == null) Debug.LogWarning(string.Format("{0}: UnityUIDialogueUI PC Subtitle Line needs to be assigned.", DialogueDebug.Prefix));
                    if (dialogue.responseMenu.buttons.Length == 0 && dialogue.responseMenu.buttonTemplate == null) Debug.LogWarning(string.Format("{0}: UnityUIDialogueUI Response buttons need to be assigned.", DialogueDebug.Prefix));
                    if (alert.line == null) Debug.LogWarning(string.Format("{0}: UnityUIDialogueUI Alert Line needs to be assigned.", DialogueDebug.Prefix));
                }
            }
            originalNPCSubtitle = dialogue.npcSubtitle;
            originalPCSubtitle = dialogue.pcSubtitle;
            originalResponseMenu = dialogue.responseMenu;
        }

        public OverrideUnityUIDialogueControls FindActorOverride(Transform actor)
        {
            if (actor == null) return null;
            if (!overrideCache.ContainsKey(actor))
            {
                overrideCache.Add(actor, (actor != null) ? actor.GetComponentInChildren<OverrideUnityUIDialogueControls>() : null);
            }
            return overrideCache[actor];
        }

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
        public void OnLevelWasLoaded(int level)
        {
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
        }
#else
        public void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
        }
#endif

        public override void Open()
        {
            overrideCache.Clear();
            base.Open();
            dialogue.npcSubtitle.CheckSubtitlePortrait(CharacterType.NPC);
            dialogue.pcSubtitle.CheckSubtitlePortrait(CharacterType.PC);
        }

        #endregion

        #region Alerts

        public override void ShowAlert(string message, float duration)
        {
            if (alert.queueAlerts)
            {
                alertQueue.Enqueue(new QueuedAlert(message, duration));
            }
            else
            {
                StartShowingAlert(message, duration);
            }
        }

        private void ShowNextQueuedAlert()
        {
            if (alertQueue.Count > 0)
            {
                var queuedAlert = alertQueue.Dequeue();
                StartShowingAlert(queuedAlert.message, queuedAlert.duration);
            }
        }

        private void StartShowingAlert(string message, float duration)
        {
            base.ShowAlert(message, duration);
            if (autoFocus) alert.AutoFocus();
        }

        #endregion

        #region Subtitles

        public override void ShowSubtitle(Subtitle subtitle)
        {
            SetIsShowingSubtitle(subtitle, true);
            if (findActorOverrides && subtitle != null)
            {
                var overrideControls = (subtitle.speakerInfo != null) ? FindActorOverride(subtitle.speakerInfo.transform) : null;
                if (overrideControls != null) overrideControls.ApplyToDialogueUI(this);
                if (subtitle.speakerInfo == null || subtitle.speakerInfo.characterType == CharacterType.NPC)
                {
                    dialogue.npcSubtitle = (overrideControls != null) ? overrideControls.subtitle : originalNPCSubtitle;
                }
                else
                {
                    dialogue.pcSubtitle = (overrideControls != null) ? overrideControls.subtitle : originalPCSubtitle;
                }
            }
            HideResponses();
            CheckForSupercededSubtitle(subtitle.speakerInfo.characterType);
            base.ShowSubtitle(subtitle); // Calls UnityUISubtitleControls.ShowSubtitle().
            ClearSelection();
            CheckSubtitleAutoFocus(subtitle);
        }

        protected void CheckForSupercededSubtitle(CharacterType characterType)
        {
            var otherSubtitle = (characterType == CharacterType.NPC) ? dialogue.pcSubtitle : dialogue.npcSubtitle;
            if (otherSubtitle.uiVisibility == UIVisibility.UntilSuperceded && otherSubtitle.isVisible)
            {
                otherSubtitle.ForceHide();
            }
        }

        public void CheckSubtitleAutoFocus(Subtitle subtitle)
        {
            if (autoFocus)
            {
                if (subtitle.speakerInfo.isPlayer)
                {
                    dialogue.pcSubtitle.AutoFocus(allowStealFocus);
                }
                else
                {
                    dialogue.npcSubtitle.AutoFocus(allowStealFocus);
                }
            }
        }

        protected void SetIsShowingSubtitle(Subtitle subtitle, bool value)
        {
            if (subtitle == null) return;
            if (subtitle.speakerInfo.isNPC)
            {
                isShowingNpcSubtitle = value;
            }
            else
            {
                isShowingPcSubtitle = value;
            }
        }

        public override void HideSubtitle(Subtitle subtitle)
        {
            SetIsShowingSubtitle(subtitle, false);
            base.HideSubtitle(subtitle);
        }

        #endregion

        #region Responses

        public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
        {
            isShowingResponses = true;
            if (findActorOverrides)
            {
                // Use speaker's (NPC's) world space canvas for subtitle reminder, and for menu if set:
                var overrideControls = (subtitle != null && subtitle.speakerInfo != null) ? FindActorOverride(subtitle.speakerInfo.transform) : null;
                var subtitleReminder = (overrideControls != null) ? overrideControls.subtitleReminder : originalResponseMenu.subtitleReminder;
                if (overrideControls != null && overrideControls.responseMenu.panel != null)
                {
                    dialogue.responseMenu = (overrideControls != null && overrideControls.responseMenu.panel != null) ? overrideControls.responseMenu : originalResponseMenu;
                }
                else
                {
                    // Otherwise use PC's world space canvas for menu if set:
                    overrideControls = (subtitle != null && subtitle.listenerInfo != null) ? FindActorOverride(subtitle.listenerInfo.transform) : null;
                    dialogue.responseMenu = (overrideControls != null && overrideControls.responseMenu.panel != null) ? overrideControls.responseMenu : originalResponseMenu;
                }
                // Either way, use speaker's (NPC's) subtitle reminder:
                dialogue.responseMenu.subtitleReminder = subtitleReminder;
            }
            if (dialogue.responseMenu.showHideController.state == UIShowHideController.State.Hiding)
            {
                StartCoroutine(ShowResponsesAfterHidden(subtitle, responses, timeout));
            }
            else
            {
                base.ShowResponses(subtitle, responses, timeout);
                ClearSelection();
                CheckResponseMenuAutoFocus();
            }
        }

        private IEnumerator ShowResponsesAfterHidden(Subtitle subtitle, Response[] responses, float timeout)
        {
            var safeguardTime = Time.realtimeSinceStartup + 5f;
            while (dialogue.responseMenu.showHideController.state == UIShowHideController.State.Hiding && Time.realtimeSinceStartup < safeguardTime)
            {
                yield return null;
            }
            base.ShowResponses(subtitle, responses, timeout);
            ClearSelection();
            CheckResponseMenuAutoFocus();

        }

        public void CheckResponseMenuAutoFocus()
        {
            if (autoFocus) dialogue.responseMenu.AutoFocus(m_lastSelection, allowStealFocus);
        }

        public override void HideResponses()
        {
            isShowingResponses = false;
            dialogue.responseMenu.DestroyInstantiatedButtons();
            base.HideResponses();
            if (isShowingNpcSubtitle && dialogue.responseMenu.subtitleReminder.panel == dialogue.npcSubtitle.panel)
            {
                dialogue.npcSubtitle.ForceShow(); // We hid the NPC subtitle that's supposed to stay visible. Show it.
            }
        }

        public void ClearSelection()
        {
            if (autoFocus)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                m_lastSelection = null;
            }
        }

        #endregion

        #region Update

        protected int alertQueueCount = 0;
        protected bool alertIsVisible;
        protected bool alertIsHiding;

        public override void Update() // Handle alert queue and auto-focus.
        {
            base.Update();

            alertQueueCount = alertQueue.Count;
            alertIsVisible = alert.IsVisible;
            alertIsHiding = alert.IsHiding;

            // Check alert queue:
            if (alertQueue.Count > 0 && alert.queueAlerts && !alert.IsVisible && !(alert.waitForHideAnimation && alert.IsHiding))
            {
                ShowNextQueuedAlert();
            }

            // Auto focus dialogue:
            if (autoFocus && isOpen)
            {
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
                {
                    m_lastSelection = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                }
                if (autoFocusCheckFrequency > 0.001f && Time.realtimeSinceStartup > m_nextAutoFocusCheckTime)
                {
                    m_nextAutoFocusCheckTime = Time.realtimeSinceStartup + autoFocusCheckFrequency;
                    if (isShowingResponses)
                    {
                        dialogue.responseMenu.AutoFocus(m_lastSelection, allowStealFocus);
                    }
                    else if (isShowingPcSubtitle)
                    {
                        dialogue.pcSubtitle.AutoFocus(allowStealFocus);
                    }
                    else if (isShowingNpcSubtitle)
                    {
                        dialogue.npcSubtitle.AutoFocus(allowStealFocus);
                    }
                }
            }
        }

        #endregion

    }

}
