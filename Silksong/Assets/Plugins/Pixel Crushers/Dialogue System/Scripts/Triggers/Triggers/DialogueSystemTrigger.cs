// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The Dialogue System Trigger is a general-purpose trigger that can execute most
    /// Dialogue System functions such as starting conversations, barks, alerts, 
    /// sequences, and Lua code.
    /// </summary>
    /// <remarks>
    /// Note: DialogueSystemTrigger has a custom editor (DialogueSystemTriggerEditor).
    /// If you make a subclass, you can also subclass the editor script and override
    /// its virtual functions.
    /// </remarks>
    [AddComponentMenu("")] // Use wrapper.
    public class DialogueSystemTrigger : MonoBehaviour
    {

        #region Serialized Variables

        /// <summary>
        /// The trigger that this component listens for.
        /// </summary>
        [Tooltip("The trigger that this component listens for.")]
        [DialogueSystemTriggerEvent]
        public DialogueSystemTriggerEvent trigger = DialogueSystemTriggerEvent.OnUse;

        /// <summary>
        /// The conditions under which the trigger will fire.
        /// </summary>
        public Condition condition;

        // //////////////////////////////////////////////////////////////////////////////////
        // Quest:

        /// <summary>
        /// If `true`, set the quest state.
        /// </summary>
        [Tooltip("Set a quest state when triggered.")]
        public bool setQuestState = true;

        /// <summary>
        /// The name of the quest.
        /// </summary>
        [QuestPopup]
        public string questName;

        /// <summary>
        /// The new state of the quest when triggered.
        /// </summary>
        [Tooltip("Set quest's main state.")]
        [QuestState]
        public QuestState questState;

        /// <summary>
        /// If `true`, set the quest entry state.
        /// </summary>
        [Tooltip("Set state of a quest entry.")]
        public bool setQuestEntryState = false;

        /// <summary>
        /// The quest entry number whose state to change.
        /// </summary>
        [QuestEntryPopup]
        public int questEntryNumber = 1;

        /// <summary>
        /// The new state of the quest entry when triggered.
        /// </summary>
        [QuestState]
        public QuestState questEntryState;

        public bool setAnotherQuestEntryState = false;

        [QuestEntryPopup]
        public int anotherQuestEntryNumber = 1;

        [QuestState]
        public QuestState anotherQuestEntryState;

        // //////////////////////////////////////////////////////////////////////////////////
        // Lua:

        /// <summary>
        /// The lua code to run.
        /// </summary>
        [Tooltip("Lua code to run. Leave blank for no message.")]
        public string luaCode = string.Empty;

        // //////////////////////////////////////////////////////////////////////////////////
        // Sequence:

        [TextArea(1, 10)]
        public string sequence = string.Empty;

        /// <summary>
        /// The speaker to use for the sequence (or null if no speaker is needed). Sequence
        /// commands can reference 'speaker' and 'listener', so you may need to define them
        /// in this component.
        /// </summary>
        [Tooltip("Optional GameObject to use if sequence uses 'speaker' keyword.")]
        public Transform sequenceSpeaker;

        /// <summary>
        /// The listener to use for the sequence (or null if no listener is needed). Sequence
        /// commands can reference 'speaker' and 'listener', so you may need to define them
        /// in this component.
        /// </summary>
        [Tooltip("Optional GameObject to use if sequence uses 'listener' keyword.")]
        public Transform sequenceListener;

        public bool waitOneFrameOnStartOrEnable = true;

        // //////////////////////////////////////////////////////////////////////////////////
        // Alert:

        /// <summary>
        /// An optional gameplay alert message. Leave blank for no message.
        /// </summary>
        [Tooltip("Alert message. Leave blank for no message.")]
        public string alertMessage;

        /// <summary>
        /// An optional localized text table to use for the alert message.
        /// </summary>
        [Tooltip("Optional text table to use to localize messages.")]
        public TextTable textTable;

        public float alertDuration = 0;

        // //////////////////////////////////////////////////////////////////////////////////
        // Send Message:

        [Serializable]
        public class SendMessageAction
        {
            [Tooltip("Target GameObject.")]
            public GameObject gameObject = null;
            [Tooltip("Name of method to call on target. One or more scripts on target should have a method with this name.")]
            public string message = "OnUse";
            [Tooltip("Optional method parameter. Specify if method accepts a string parameter.")]
            public string parameter = string.Empty;
        }

        /// <summary>
        /// Targets and messages to send when the trigger fires.
        /// </summary>
        public SendMessageAction[] sendMessages = new SendMessageAction[0];

        // //////////////////////////////////////////////////////////////////////////////////
        // Bark:

        public enum BarkSource { None, Conversation, Text }

        [Tooltip("Where to get content to bark.")]
        public BarkSource barkSource = BarkSource.None;

        /// <summary>
        /// The title of the bark conversation. Used if barkSource is set to conversation.
        /// </summary>
        [Tooltip("Conversation to get bark content from.")]
        [ConversationPopup(false, true)]
        public string barkConversation = string.Empty;

        /// <summary>
        /// Text to bark. Used if barkSource is text. Will be localized through Dialogue Manager's Text Table if assigned.
        /// </summary>
        [Tooltip("Bark this text. Will be localized through Dialogue Manager's Text Table if assigned.")]
        public string barkText = string.Empty;

        /// <summary>
        /// Optional sequence to play when barking text.
        /// </summary>
        [Tooltip("Optional sequence to play when barking text.")]
        public string barkTextSequence = string.Empty;

        /// <summary>
        /// The barker.
        /// </summary>
        [Tooltip("Character that bark comes from. Should have a bark UI or a Dialogue Actor component with a bark UI prefab assigned.")]
        public Transform barker;

        /// <summary>
        /// The target of the bark.
        /// </summary>
        [Tooltip("Optional target of the bark. Receives OnBark events.")]
        public Transform barkTarget;

        /// <summary>
        /// Specifies the order to run through the list of barks.
        /// 
        /// - Random: Choose a random bark from the conversation.
        /// - Sequential: Choose the barks in order from first to last, looping at the end.
        /// </summary>
        public BarkOrder barkOrder = BarkOrder.Random;

        /// <summary>
        /// Are barks allowed during conversations?
        /// </summary>
        public bool allowBarksDuringConversations = false;

        /// <summary>
        /// Skip bark if no valid entries.
        /// </summary>
        [Tooltip("Only trigger if at least one entry's Conditions are currently true.")]
        public bool skipBarkIfNoValidEntries;

        /// <summary>
        /// If ticked, bark info is cached during the first bark. This can reduce stutter
        /// when barking on slower mobile devices, but barks are not reevaluated each time
        /// as the state changes, barks use no em formatting codes, and sequences are not
        /// played with barks.
        /// </summary>
        [Tooltip("Cache all lines during first bark. This can reduce stutter when barking on slower mobile devices, but barks' conditions are not reevaluated each time as the state changes, barks use no em formatting codes, and sequences are not played with barks.")]
        public bool cacheBarkLines = false;

        // //////////////////////////////////////////////////////////////////////////////////
        // Conversation:

        /// <summary>
        /// The title of the conversation to start.
        /// </summary>
        [Tooltip("Conversation to start. Leave blank for no conversation.")]
        [ConversationPopup(false, true)]
        public string conversation = string.Empty;

        /// <summary>
        /// The conversant of the conversation. If not set, this game object. The actor is usually
        /// the entity that caused the trigger (for example, the player that hits the "Use" button
        /// on the conversant, thereby triggering OnUse). 
        /// See https://www.pixelcrushers.com/dialogue_system/manual2x/html/triggers_and_interaction.html
        /// for an explanation of how GameObjects are assigned at runtime.
        /// </summary>
        [Tooltip("Other actor (e.g., NPC). If unassigned, this GameObject.")]
        public Transform conversationConversant;

        /// <summary>
        /// The actor to converse with. If not set, the game object that triggered the event.
        /// </summary>
        [Tooltip("Primary actor (e.g., player). If unassigned, GameObject that triggered conversation.")]
        public Transform conversationActor;

        [Tooltip("Start at this entry ID.")]
        public int startConversationEntryID = -1;

        [Tooltip("Start at entry with this Title.")]
        public string startConversationEntryTitle;

        /// <summary>
        /// Only start if no other conversation is active.
        /// </summary>
        [Tooltip("Only trigger if no other conversation is already active.")]
        public bool exclusive = false;

        /// <summary>
        /// Stop other conversation if one is active.
        /// </summary>
        [Tooltip("Stop other conversation if one is active.")]
        public bool replace = false;

        /// <summary>
        /// If this is <c>true</c> and no valid entries currently link from the start entry,
        /// don't start the conversation.
        /// </summary>
        [Tooltip("Only trigger if at least one entry's Conditions are currently true.")]
        public bool skipIfNoValidEntries = false;

        [Tooltip("Disallow conversation if same conversation just ended on this frame.")]
        public bool preventRestartOnSameFrameEnded = false;

        /// <summary>
        /// Set <c>true</c> to stop the conversation if the actor leaves the trigger area.
        /// </summary>
        [Tooltip("Stop conversation if actor leaves trigger area.")]
        public bool stopConversationOnTriggerExit = false;

        [Tooltip("Stop conversation if Conversation Actor exceeds Max Conversation Distance from this trigger's GameObject.")]
        public bool stopConversationIfTooFar = false;

        [Tooltip("If Stop Conversation If Too Far is ticked, this is too far.")]
        public float maxConversationDistance = 5f;

        [Tooltip("Check distance on this frequency.")]
        public float monitorConversationDistanceFrequency = 1f;

        [Tooltip("Make the cursor visible when the conversation starts. Return to previous visibility state when conversation ends.")]
        public bool showCursorDuringConversation = false;

        [Tooltip("Set Time.timeScale to 0 during conversation, back to previous timeScale when conversation ends.")]
        public bool pauseGameDuringConversation = false;

        // //////////////////////////////////////////////////////////////////////////////////
        // Set Active:

        [Serializable]
        public class SetGameObjectActiveAction
        {
            public Condition condition = new Condition();
            public Transform target;
            public Toggle state;
        }

        public SetGameObjectActiveAction[] setActiveActions = new SetGameObjectActiveAction[0];

        // //////////////////////////////////////////////////////////////////////////////////
        // Set Enabled:

        [Serializable]
        public class SetComponentEnabledAction
        {
            public Condition condition = new Condition();
            public Component target;
            public Toggle state;
        }

        public SetComponentEnabledAction[] setEnabledActions = new SetComponentEnabledAction[0];

        // //////////////////////////////////////////////////////////////////////////////////
        // Set Animator State:

        [Serializable]
        public class SetAnimatorStateAction
        {
            public Condition condition = new Condition();
            [Tooltip("Set the state of the animator on this GameObject. Animator can be on a child GameObject.")]
            public Transform target;
            [Tooltip("State to crossfade to.")]
            public string stateName;
            public float crossFadeDuration = 0.3f;
        }

        public SetAnimatorStateAction[] setAnimatorStateActions = new SetAnimatorStateAction[0];

        // //////////////////////////////////////////////////////////////////////////////////
        // UnityEvent:

        public GameObjectUnityEvent onExecute = new GameObjectUnityEvent();

        // //////////////////////////////////////////////////////////////////////////////////

        [HideInInspector]
        public bool useConversationTitlePicker = true;

        [HideInInspector]
        public bool useBarkTitlePicker = true;

        [HideInInspector]
        public bool useQuestNamePicker = true;

        [HideInInspector]
        public DialogueDatabase selectedDatabase = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sequencer used by the current bark, if a bark is playing.
        /// If a bark is not playing, this is undefined. To check if a bark is
        /// playing, check the bark UI's IsPlaying property.
        /// </summary>
        /// <value>The sequencer.</value>
        public Sequencer sequencer { get; protected set; }

        #endregion

        #region Private/Protected Variables

        protected BarkHistory barkHistory;
        protected ConversationState cachedState = null;
        protected BarkGroupMember barkGroupMember = null;
        protected IBarkUI barkUI = null;
        protected float earliestTimeToAllowTriggerExit = 0;
        protected const float MarginToAllowTriggerExit = 0.2f;
        protected Coroutine monitorDistanceCoroutine = null;
        protected bool wasCursorVisible;
        protected CursorLockMode savedLockState;
        protected bool didIPause = false;
        protected float preConversationTimeScale = 1;
        protected int frameConversationEnded = -1;
        protected bool tryingToStart = false;
        protected bool hasSaveSystem;
        protected Coroutine fireIfNoSaveDataAppliedCoroutine = null;
        protected ActiveConversationRecord activeConversation;

        #endregion

        #region Trigger Checks

        public virtual void Awake()
        {
            barkHistory = new BarkHistory(barkOrder);
            sequencer = null;
            hasSaveSystem = GameObjectUtility.FindFirstObjectByType<SaveSystem>() != null;
            if (hasSaveSystem &&
                ((trigger == DialogueSystemTriggerEvent.OnSaveDataApplied) ||
                 (trigger == DialogueSystemTriggerEvent.OnStart && DialogueManager.instance.onStartTriggerWaitForSaveDataApplied)))
            {
                SaveSystem.saveDataApplied += OnSaveDataApplied;
            }
        }

        public virtual void Start()
        {
            if (trigger == DialogueSystemTriggerEvent.OnCollisionEnter ||
                trigger == DialogueSystemTriggerEvent.OnCollisionExit ||
                trigger == DialogueSystemTriggerEvent.OnTriggerEnter ||
                trigger == DialogueSystemTriggerEvent.OnTriggerExit)
            {
                bool found = false;
                if (GetComponent<Collider>() != null) found = true;
#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER
                if (!found && GetComponent<Collider2D>() != null) found = true;
#endif
                if (!found && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Dialogue System Trigger is set to a mode that requires a collider, but it has no collider component. If your project is 2D, did you enable 2D support? (Tools > Pixel Crushers > Dialogue System > Welcome Window)", this);
            }
            else if (trigger == DialogueSystemTriggerEvent.OnStart)
            {
                if (hasSaveSystem && DialogueManager.instance.onStartTriggerWaitForSaveDataApplied)
                {
                    // Dialogue Manager option has configured OnStart to work like OnSaveDataApplied, so start check here:
                    fireIfNoSaveDataAppliedCoroutine = StartCoroutine(FireIfNoSaveDataApplied());
                }
                else
                {
                    // Wait until end of frame to allow all other components to finish their Start() methods:
                    StartCoroutine(StartAtEndOfFrame());
                }
            }
            else if (trigger == DialogueSystemTriggerEvent.OnSaveDataApplied)
            {
                if (hasSaveSystem)
                {
                    fireIfNoSaveDataAppliedCoroutine = StartCoroutine(FireIfNoSaveDataApplied());
                }
                else
                {
                    StartCoroutine(StartAtEndOfFrame());
                }
            }
            barkGroupMember = GetBarker(barkConversation).GetComponent<BarkGroupMember>();
            if (cacheBarkLines && barkSource == BarkSource.Conversation && !string.IsNullOrEmpty(barkConversation))
            {
                PopulateCache(GetBarker(barkConversation), barkTarget);
            }
        }

        public void OnBarkStart(Transform actor)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnBarkStart)) TryStart(actor);
        }

        public void OnBarkEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnBarkEnd)) TryStart(actor);
        }

        public void OnConversationStart(Transform actor)
        {
            if (!enabled) return;
            if (trigger == DialogueSystemTriggerEvent.OnConversationStart) TryStart(actor);
        }

        public void OnConversationEnd(Transform actor)
        {
            if (!enabled) return;
            if (trigger == DialogueSystemTriggerEvent.OnConversationEnd) TryStart(actor);
        }

        // These methods run even if this DialogueSystemTrigger isn't on the actor or conversant.
        // They handle monitoring distance, showCursorDuringConversation and pauseGameDuringConversation.
        private void OnConversationStartAnywhere(Transform actor)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStartAnywhere;
            if (showCursorDuringConversation)
            {
                wasCursorVisible = Cursor.visible;
                savedLockState = Cursor.lockState;
                StartCoroutine(ShowCursorAfterOneFrame());
            }
            if (pauseGameDuringConversation && string.Equals(DialogueManager.lastConversationStarted, conversation))
            {
                didIPause = true;
                preConversationTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
        }

        protected IEnumerator ShowCursorAfterOneFrame()
        {
            yield return null;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnConversationEndAnywhere(Transform actor)
        {
            var didMyConversationEnd = !DialogueManager.allowSimultaneousConversations ||
                (activeConversation == null) || !activeConversation.conversationController.isActive;
            if (didMyConversationEnd)
            {
                DialogueManager.instance.conversationEnded -= OnConversationEndAnywhere;
                StopMonitoringConversationDistance();
                if (showCursorDuringConversation)
                {
                    Cursor.visible = wasCursorVisible;
                    Cursor.lockState = savedLockState;
                }
                if (pauseGameDuringConversation && didIPause)
                {
                    didIPause = false;
                    Time.timeScale = preConversationTimeScale;
                }
                frameConversationEnded = Time.frameCount;
            }
        }

        public void OnSequenceStart(Transform actor)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnSequenceStart)) TryStart(actor);
        }

        public void OnSequenceEnd(Transform actor)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnSequenceEnd)) TryStart(actor);
        }

        public void OnSequenceEnd()
        {
            OnSequenceEnd(null);
        }

        public void OnUse(Transform actor)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnUse)) TryStart(actor);
        }

        public void OnUse(string message)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnUse)) TryStart(null);
        }

        public void OnUse()
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnUse)) TryStart(null);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnTriggerEnter)) TryStart(other.transform);
        }

        public void OnTriggerExit(Collider other)
        {
            CheckOnTriggerExit(other.transform);
        }

#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnTriggerEnter)) TryStart(other.transform);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            CheckOnTriggerExit(other.transform);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnCollisionEnter)) TryStart(collision.collider.transform);
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnCollisionExit)) TryStart(collision.collider.transform);
        }

#endif

        protected void CheckOnTriggerExit(Transform otherTransform)
        {
            if (!enabled) return;
            if (stopConversationOnTriggerExit &&
                DialogueManager.isConversationActive &&
                (GetCurrentDialogueTime() > earliestTimeToAllowTriggerExit) &&
                ((DialogueManager.currentActor == otherTransform) || (DialogueManager.currentConversant == otherTransform)))
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Stopping conversation because " + otherTransform + " exited trigger area.", this);
                StopActiveConversation();
            }
            else if (trigger == DialogueSystemTriggerEvent.OnTriggerExit)
            {
                TryStart(otherTransform.transform);
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnCollisionEnter)) TryStart(collision.collider.transform);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (enabled && (trigger == DialogueSystemTriggerEvent.OnTriggerExit)) TryStart(collision.collider.transform);
        }

        protected bool listenForOnDestroy = false;

        public void OnEnable()
        {
            PersistentDataManager.RegisterPersistentData(gameObject);
            listenForOnDestroy = true;
            // Waits one frame to allow all other components to finish their OnEnable() methods.
            if (trigger == DialogueSystemTriggerEvent.OnEnable) StartCoroutine(StartAtEndOfFrame());
        }

        public void OnDisable()
        {
            StopMonitoringConversationDistance();
            StopAllCoroutines();
            PersistentDataManager.UnregisterPersistentData(gameObject);
            if (listenForOnDestroy && trigger == DialogueSystemTriggerEvent.OnDisable) TryStart(null);
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
            if (hasSaveSystem)
            {
                SaveSystem.saveDataApplied -= OnSaveDataApplied;
                if (fireIfNoSaveDataAppliedCoroutine != null)
                {
                    StopCoroutine(fireIfNoSaveDataAppliedCoroutine);
                    fireIfNoSaveDataAppliedCoroutine = null;
                }
            }
            if (listenForOnDestroy && trigger == DialogueSystemTriggerEvent.OnDestroy)
            {
                TryStart(null);
            }
        }

        #endregion

        #region Execution

        protected IEnumerator StartAtEndOfFrame()
        {
            // Several Unity versions have a bug with execution order and the first frame.
            // (WaitForEndOfFrame on frame 1 will skip to frame 2.) So, if on frame 1,
            // start immediately instead of waiting for end of frame.
            if (Time.frameCount > 1)
            {
                yield return CoroutineUtility.endOfFrame;
            }
            TryStart(null);
        }

        protected virtual void OnSaveDataApplied()
        {
            if (fireIfNoSaveDataAppliedCoroutine != null)
            {
                StopCoroutine(fireIfNoSaveDataAppliedCoroutine);
                fireIfNoSaveDataAppliedCoroutine = null;
            }
            if (enabled)
            {
                TryStart(null);
            }
        }

        protected virtual IEnumerator FireIfNoSaveDataApplied()
        {
            if (!hasSaveSystem) yield break;
            // Wait for SaveSystem.framesToWaitBeforeApplyData + 1.
            // If OnSaveDataApplied hasn't killed this coroutine, fire.
            for (int i = 0; i < (SaveSystem.framesToWaitBeforeApplyData + 1); i++)
            {
                yield return null;
            }
            fireIfNoSaveDataAppliedCoroutine = null;
            TryStart(null);
        }

        public void TryStart(Transform actor)
        {
            TryStart(actor, actor);
        }

        /// <summary>
        /// Sets the quest status if the condition is true.
        /// </summary>
        public virtual void TryStart(Transform actor, Transform interactor)
        {
            if (tryingToStart) return;
            tryingToStart = true;
            try
            {
                if (((condition == null) || condition.IsTrue(interactor)))
                {
                    Fire(actor);
                }
            }
            finally
            {
                tryingToStart = false;
            }
        }

        public virtual void Fire(Transform actor)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Dialogue System Trigger is firing " + trigger + ".", this);
            DoQuestAction();
            DoLuaAction(actor);
            DoSequenceAction(actor);
            DoAlertAction();
            DoSendMessageActions();
            DoBarkAction(actor);
            DoConversationAction(actor);
            DoSetActiveActions(actor);
            DoSetEnabledActions(actor);
            DoSetAnimatorStateActions(actor);
            if (onExecute != null) onExecute.Invoke((actor != null) ? actor.gameObject : null);
            DialogueManager.SendUpdateTracker();
        }

        #endregion

        #region Quest Action

        protected virtual void DoQuestAction()
        {
            if (string.IsNullOrEmpty(questName)) return;
            if (setQuestState) QuestLog.SetQuestState(questName, questState);
            if (setQuestEntryState)
            {
                QuestLog.SetQuestEntryState(questName, questEntryNumber, questEntryState);
                if (setAnotherQuestEntryState) QuestLog.SetQuestEntryState(questName, anotherQuestEntryNumber, anotherQuestEntryState);
            }
        }

        #endregion

        #region Lua Action

        protected virtual void DoLuaAction(Transform actor)
        {
            if (string.IsNullOrEmpty(luaCode)) return;
            if (actor != null)
            {
                var dialogueActor = DialogueActor.GetDialogueActorComponent(actor);
                var actorName = (dialogueActor != null) ? dialogueActor.actor : actor.name;
                DialogueLua.SetVariable("ActorIndex", actorName);
                DialogueLua.SetVariable("Actor", DialogueActor.GetActorName(actor));
            }
            DoLuaAction();
        }

        protected virtual void DoLuaAction()
        {
            if (string.IsNullOrEmpty(luaCode)) return;
            Lua.Run(luaCode, DialogueDebug.logInfo);
        }

        #endregion

        #region Sequence Action

        protected virtual void DoSequenceAction(Transform actor)
        {
            if (string.IsNullOrEmpty(sequence)) return;
            DialogueManager.PlaySequence(sequence, Tools.Select(sequenceSpeaker, transform), Tools.Select(sequenceListener, actor));
        }

        #endregion

        #region Alert Action

        protected virtual void DoAlertAction()
        {
            if (string.IsNullOrEmpty(alertMessage)) return;
            string localizedAlertMessage;
            if ((textTable != null) && textTable.HasFieldTextForLanguage(alertMessage, Localization.GetCurrentLanguageID(textTable)))
            {
                localizedAlertMessage = textTable.GetFieldTextForLanguage(alertMessage, Localization.GetCurrentLanguageID(textTable));
            }
            else
            {
                localizedAlertMessage = DialogueManager.GetLocalizedText(alertMessage);
            }
            if (Mathf.Approximately(0, alertDuration))
            {
                DialogueManager.ShowAlert(localizedAlertMessage);
            }
            else
            {
                DialogueManager.ShowAlert(localizedAlertMessage, alertDuration);
            }
        }

        #endregion

        #region SendMessage Action

        protected virtual void DoSendMessageActions()
        {
            for (int i = 0; i < sendMessages.Length; i++)
            {
                var sma = sendMessages[i];
                if (sma != null && sma.gameObject != null && !string.IsNullOrEmpty(sma.message))
                {
                    sma.gameObject.SendMessage(sma.message, sma.parameter, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        #endregion

        #region Bark Action

        protected virtual void DoBarkAction(Transform actor)
        {
            switch (barkSource)
            {
                case BarkSource.Conversation:
                    if (string.IsNullOrEmpty(barkConversation)) return;
                    if (DialogueManager.isConversationActive && !allowBarksDuringConversations)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Bark triggered on " + name + ", but a conversation is already active.", GetBarker(barkConversation));
                    }
                    else if (cacheBarkLines)
                    {
                        BarkCachedLine(GetBarker(barkConversation), Tools.Select(barkTarget, actor));
                    }
                    else
                    {
                        if (barkGroupMember != null)
                        {
                            barkGroupMember.GroupBark(barkConversation, Tools.Select(barkTarget, actor), barkHistory);
                        }
                        else
                        {
                            DialogueManager.Bark(barkConversation, GetBarker(barkConversation), Tools.Select(barkTarget, actor), barkHistory);
                        }
                        sequencer = BarkController.LastSequencer;
                    }
                    break;
                case BarkSource.Text:
                    if (string.IsNullOrEmpty(barkText)) return;
                    if (DialogueManager.isConversationActive && !allowBarksDuringConversations)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Bark triggered on " + name + ", but a conversation is already active.", GetBarker(null));
                    }
                    else
                    {
                        if (barkGroupMember != null)
                        {
                            barkGroupMember.GroupBarkString(barkText, Tools.Select(barkTarget, actor), barkTextSequence);
                        }
                        else
                        {
                            DialogueManager.BarkString(barkText, GetBarker(null), Tools.Select(barkTarget, actor), barkTextSequence);
                        }
                        sequencer = BarkController.LastSequencer;
                    }
                    break;
            }
        }

        protected virtual Transform GetBarker(string conversation)
        {
            if (barker != null) return barker;
            if (!string.IsNullOrEmpty(conversation))
            {
                var conversationAsset = DialogueManager.MasterDatabase.GetConversation(conversation);
                if (conversationAsset != null)
                {
                    var barkActor = DialogueManager.MasterDatabase.GetActor(conversationAsset.ConversantID);
                    var registeredTransform = (barkActor != null) ? CharacterInfo.GetRegisteredActorTransform(barkActor.Name) : null;
                    if (registeredTransform != null) return registeredTransform;
                }
            }
            return this.transform;
        }

        protected virtual string GetBarkerName()
        {
            return DialogueActor.GetActorName(GetBarker((barkSource == BarkSource.Conversation) ? barkConversation : null));
        }

        protected virtual void BarkCachedLine(Transform speaker, Transform listener)
        {
            if (barkUI == null) barkUI = speaker.GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
            if (cachedState == null) PopulateCache(speaker, listener);
            BarkNextCachedLine(speaker, listener);
        }

        protected void PopulateCache(Transform speaker, Transform listener)
        {
            if (string.IsNullOrEmpty(barkConversation) && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): conversation title is blank", new System.Object[] { DialogueDebug.Prefix, speaker, listener }), speaker);
            ConversationModel conversationModel = new ConversationModel(DialogueManager.masterDatabase, barkConversation, speaker, listener, DialogueManager.allowLuaExceptions, DialogueManager.isDialogueEntryValid);
            cachedState = conversationModel.firstState;
            if ((cachedState == null) && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no START entry", new System.Object[] { DialogueDebug.Prefix, speaker, listener, barkConversation }), speaker);
            if (!cachedState.hasAnyResponses && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no valid bark lines", new System.Object[] { DialogueDebug.Prefix, speaker, listener, barkConversation }), speaker);
        }

        protected void BarkNextCachedLine(Transform speaker, Transform listener)
        {
            if ((barkUI != null) && (cachedState != null) && cachedState.hasAnyResponses)
            {
                Response[] responses = cachedState.hasNPCResponse ? cachedState.npcResponses : cachedState.pcResponses;
                int index = (barkHistory ?? new BarkHistory(BarkOrder.Random)).GetNextIndex(responses.Length);
                DialogueEntry barkEntry = responses[index].destinationEntry;
                if ((barkEntry == null) && DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark entry is null", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
                if (barkEntry != null)
                {
                    Subtitle subtitle = new Subtitle(cachedState.subtitle.listenerInfo, cachedState.subtitle.speakerInfo, new FormattedText(barkEntry.currentDialogueText), barkEntry.currentSequence, string.Empty, barkEntry);
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

        #endregion

        #region Conversation Action

        protected virtual void DoConversationAction(Transform actor)
        {
            if (string.IsNullOrEmpty(conversation)) return;
            if (replace && DialogueManager.isConversationActive)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Stopping current active conversation " + DialogueManager.lastConversationStarted + " and starting " + conversation + ".", this);
                DialogueManager.StopAllConversations();
            }
            if (exclusive && DialogueManager.isConversationActive)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Conversation triggered on " + name + " but skipping because another conversation is active.", this);
            }
            else
            {
                var actorTransform = Tools.Select(conversationActor, actor);
                var conversantTransform = conversationConversant;
                if (conversantTransform == null)
                {
                    var conversationAsset = DialogueManager.MasterDatabase.GetConversation(conversation);
                    var conversationConversantActor = (conversationAsset != null) ? DialogueManager.MasterDatabase.GetActor(conversationAsset.ConversantID) : null;
                    var registeredTransform = (conversationConversantActor != null) ? CharacterInfo.GetRegisteredActorTransform(conversationConversantActor.Name) : null;
                    conversantTransform = (registeredTransform != null) ? registeredTransform : this.transform;
                }
                var entryID = !string.IsNullOrEmpty(startConversationEntryTitle) ? GetEntryIDFromTitle(conversation, startConversationEntryTitle)
                    : startConversationEntryID;
                if (skipIfNoValidEntries && !DialogueManager.ConversationHasValidEntry(conversation, actorTransform, conversantTransform, entryID))
                {
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Conversation triggered on " + name + " but skipping because no entries are currently valid.", this);
                }
                else if (preventRestartOnSameFrameEnded && frameConversationEnded == Time.frameCount && DialogueManager.lastConversationStarted == conversation)
                {
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Conversation triggered on " + name + " but skipping because same conversation just ended on this frame.", this);
                }
                else
                {

                    if (stopConversationIfTooFar || showCursorDuringConversation || pauseGameDuringConversation || preventRestartOnSameFrameEnded)
                    { // Trigger may not be on actor or conversant, so we need to hook into these events:
                        DialogueManager.instance.conversationStarted += OnConversationStartAnywhere;
                        DialogueManager.instance.conversationEnded += OnConversationEndAnywhere;
                    }

                    DialogueManager.StartConversation(conversation, actorTransform, conversantTransform, entryID);
                    activeConversation = DialogueManager.instance.activeConversation;
                    earliestTimeToAllowTriggerExit = GetCurrentDialogueTime() + MarginToAllowTriggerExit;
                    if (stopConversationIfTooFar)
                    {
                        monitorDistanceCoroutine = StartCoroutine(MonitorDistance(DialogueManager.currentActor));
                    }
                }
            }
        }

        private float GetCurrentDialogueTime()
        {
            return DialogueTime.mode == DialogueTime.TimeMode.Gameplay ? Time.time : Time.realtimeSinceStartup;
        }

        private int GetEntryIDFromTitle(string conversation, string entryTitle)
        {
            if (string.IsNullOrEmpty(conversation) || string.IsNullOrEmpty(entryTitle)) return -1;
            var conversationAsset = DialogueManager.MasterDatabase.GetConversation(conversation);
            if (conversationAsset == null) return -1;
            var entry = conversationAsset.dialogueEntries.Find(x => string.Equals(x.Title, entryTitle));
            if (entry == null) return -1;
            return entry.id;
        }

        protected virtual void StopActiveConversation()
        {
            if (activeConversation != null && activeConversation.conversationController != null)
            {
                activeConversation.conversationController.Close();
                activeConversation = null;
            }
        }

        protected void StopMonitoringConversationDistance()
        {
            if (monitorDistanceCoroutine != null) StopCoroutine(monitorDistanceCoroutine);
            monitorDistanceCoroutine = null;
        }

        protected IEnumerator MonitorDistance(Transform actor)
        {
            if (actor == null) yield break;
            Transform myTransform = transform;
            while (true)
            {
                yield return StartCoroutine(DialogueTime.WaitForSeconds(monitorConversationDistanceFrequency));
                if (Vector3.Distance(myTransform.position, actor.position) > maxConversationDistance)
                {
                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Stopping conversation. Exceeded max distance {1} between {2} and {3}", new System.Object[] { DialogueDebug.Prefix, maxConversationDistance, name, actor.name }));
                    StopActiveConversation();
                    yield break;
                }
            }
        }

        #endregion

        #region Set GameObject Active Action

        protected virtual void DoSetActiveActions(Transform actor)
        {
            for (int i = 0; i < setActiveActions.Length; i++)
            {
                var action = setActiveActions[i];
                if (action != null && action.condition != null && action.condition.IsTrue(actor))
                {
                    var target = Tools.Select(action.target, this.transform);
                    bool newValue = ToggleUtility.GetNewValue(target.gameObject.activeSelf, action.state);
                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Trigger: {1}.SetActive({2})", new System.Object[] { DialogueDebug.Prefix, target.name, newValue }));
                    target.gameObject.SetActive(newValue);
                }
            }
        }

        #endregion

        #region Set Components Enabled Action

        protected virtual void DoSetEnabledActions(Transform actor)
        {
            for (int i = 0; i < setEnabledActions.Length; i++)
            {
                var action = setEnabledActions[i];
                if (action != null && action.condition != null && action.condition.IsTrue(actor))
                {
                    Tools.SetComponentEnabled(action.target, action.state);
                }
            }
        }

        #endregion

        #region Set Animator State Action

        protected virtual void DoSetAnimatorStateActions(Transform actor)
        {
            for (int i = 0; i < setAnimatorStateActions.Length; i++)
            {
                var action = setAnimatorStateActions[i];
                if (action != null && action.condition != null && action.condition.IsTrue(actor))
                {
                    Transform target = Tools.Select(action.target, this.transform);
                    Animator animator = target.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Trigger: {1}.SetAnimatorState() can't find Animator", new System.Object[] { DialogueDebug.Prefix, target.name }));
                    }
                    else
                    {
                        if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Trigger: {1}.SetAnimatorState({2})", new System.Object[] { DialogueDebug.Prefix, target.name, action.stateName }));
                        animator.CrossFade(action.stateName, action.crossFadeDuration);
                    }
                }
            }
        }

        #endregion

        #region Save & Load

        /// <summary>
        /// Listens for the OnRecordPersistentData message and records the current bark index.
        /// </summary>
        public void OnRecordPersistentData()
        {
            if (enabled && !string.IsNullOrEmpty(barkConversation))
            {
                DialogueLua.SetActorField(GetBarkerName(), "Bark_Index", barkHistory.index);
            }
        }

        /// <summary>
        /// Listens for the OnApplyPersistentData message and retrieves the current bark index.
        /// </summary>
        public void OnApplyPersistentData()
        {
            if (enabled && !string.IsNullOrEmpty(barkConversation))
            {
                if (barkHistory == null) barkHistory = new BarkHistory(barkOrder);
                barkHistory.index = DialogueLua.GetActorField(GetBarkerName(), "Bark_Index").asInt;
            }
        }

        #endregion

    }

}
