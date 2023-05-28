// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// SMS-style dialogue UI.
    /// </summary>
    public class SMSDialogueUI : StandardDialogueUI
    {

        [Header("Heading")]

        [Tooltip("(Optional) If assigned, set to the conversation title.")]
        public UnityEngine.UI.Text headingText;

        [Header("Message Panel")]

        [Tooltip("The scroll rect containing the content panel.")]
        public UnityEngine.UI.ScrollRect scrollRect;

        [Tooltip("The content panel inside the scroll rect containing the message panel and response panel.")]
        public RectTransform contentPanel;

        [Tooltip("Add messages to this panel.")]
        public RectTransform messagePanel;

        [Tooltip("If non-zero, drop older messages when the number of messages in the history reaches this value.")]
        public int maxMessages = 0;

        [Tooltip("Speed at which to smoothly scroll down.")]
        public float scrollSpeed = 5f;

        [Serializable]
        public class PreDelaySettings
        {
            [Tooltip("Show this when waiting before showing subtitle. Often a '...' image suggesting NPC is typing.")]
            public GameObject preDelayIcon;

            [Tooltip("Before showing subtitle, delay based on the Dialogue Text length and Dialogue Manager > Subtitle Settings > Subtitle Chars Per Second.")]
            public bool basedOnTextLength = false;

            [Tooltip("Before showing subtitle, also delay for this many seconds.")]
            public float additionalSeconds = 0;

            public float GetDelayDuration(Subtitle subtitle)
            {
                return (basedOnTextLength ? Mathf.Max(DialogueManager.DisplaySettings.subtitleSettings.minSubtitleSeconds,
                    subtitle.formattedText.text.Length / Mathf.Max(1, DialogueManager.DisplaySettings.subtitleSettings.subtitleCharsPerSecond)) : 0) +
                    additionalSeconds;
            }

            public void CopyTo(PreDelaySettings dest)
            {
                dest.basedOnTextLength = basedOnTextLength;
                dest.additionalSeconds = additionalSeconds;
            }
        }

        [Tooltip("Before showing NPC subtitles, delay for this duration.")]
        public PreDelaySettings npcPreDelaySettings = new PreDelaySettings();

        [Tooltip("Before showing PC subtitles, delay for this duration.")]
        public PreDelaySettings pcPreDelaySettings = new PreDelaySettings();

        [Header("Save/Load")]

        [Tooltip("Load the saved conversation specified in the Conversation variable.")]
        public bool useConversationVariable = false;

        [Tooltip("When resuming conversation, don't play sequence of last entry.")]
        public bool dontRepeatLastSequence = false;

        [Tooltip("Disable Audio() and AudioWait() sequencer commands when resuming last entry.")]
        public bool disableAudioOnLastSequence = true;

        [Tooltip("When entering these scene(s), don't resume the conversation. Typically used for the start menu (scene 0).")]
        public int[] dontLoadConversationInScenes = new int[] { 0 };

        public static string conversationVariableOverride;

        [Serializable]
        public class DialogueEntryRecord
        {
            public int conversationID;
            public int entryID;
            public DialogueEntryRecord(int conversationID, int entryID)
            {
                this.conversationID = conversationID;
                this.entryID = entryID;
            }
        }

        protected List<DialogueEntryRecord> records = new List<DialogueEntryRecord>();

        protected List<GameObject> instantiatedMessages = new List<GameObject>();

        protected int currentSceneIndex = -1;

        protected PreDelaySettings npcPreDelaySettingsCopy = new PreDelaySettings();
        protected PreDelaySettings pcPreDelaySettingsCopy = new PreDelaySettings();

        protected bool isLoadingGame = false;
        protected bool skipNextRecord = false;
        protected bool isInPreDelay = false;
        protected Coroutine scrollCoroutine = null;

        protected bool shouldShowContinueButton = false;
        protected UnityEngine.UI.Button continueButton = null;

        protected Dictionary<Transform, DialogueActor> dialogueActorCache = new Dictionary<Transform, DialogueActor>();

        protected virtual void CheckAssignments()
        {
            if (scrollRect == null) Debug.LogWarning("Textline: Assign the dialogue UI's Scroll Rect", this);
            if (contentPanel == null) Debug.LogWarning("Textline: Assign the dialogue UI's Content Panel", this);
            if (messagePanel == null) Debug.LogWarning("Textline: Assign the dialogue UI's Message Panel", this);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            SceneManager.sceneLoaded -= RecordCurrentScene;
            SceneManager.sceneLoaded += RecordCurrentScene;
            PersistentDataManager.RegisterPersistentData(gameObject);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            SceneManager.sceneLoaded -= RecordCurrentScene;
            PersistentDataManager.UnregisterPersistentData(gameObject);
        }

        protected void RecordCurrentScene(Scene scene, LoadSceneMode mode)
        {
            currentSceneIndex = scene.buildIndex;
        }

        public override void Open()
        {
            base.Open();
            CheckAssignments();
            DestroyInstantiatedMessages(); // Start with clean slate.
            dialogueActorCache.Clear();

            if (headingText != null)
            {
                var conversation = DialogueManager.masterDatabase.GetConversation(DialogueManager.lastConversationID);
                if (conversation != null)
                {
                    headingText.text = Field.LookupLocalizedValue(conversation.fields, "Title");
                }
            }

            Tools.SetGameObjectActive(npcPreDelaySettings.preDelayIcon, false);
            Tools.SetGameObjectActive(pcPreDelaySettings.preDelayIcon, false);
        }

        public override void Close()
        {
            StopAllCoroutines();
            base.Close();
            if (!isLoadingGame) records.Clear();
        }

        public override void ShowSubtitle(Subtitle subtitle)
        {
            if (subtitle.dialogueEntry.id == 0) return; // Don't need to show START entry.
            if (string.IsNullOrEmpty(subtitle.formattedText.text)) return;
            var preDelay = subtitle.speakerInfo.IsNPC ? npcPreDelaySettings.GetDelayDuration(subtitle) : pcPreDelaySettings.GetDelayDuration(subtitle);
            if (Mathf.Approximately(0, preDelay))
            {
                AddMessage(subtitle);
            }
            else
            {
                StartCoroutine(AddMessageWithPreDelay(preDelay, subtitle));
            }
            AddRecord(subtitle);
        }

        public override void HideSubtitle(Subtitle subtitle)
        {
            // Don't hide the subtitle.
        }

        public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
        {
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            if (isInPreDelay && !isLoadingGame)
            {
                StartCoroutine(ShowResponsesAfterPreDelay(subtitle, responses, timeout));
            }
            else
            {
                ShowResponsesNow(subtitle, responses, timeout);
            }
        }

        protected IEnumerator ShowResponsesAfterPreDelay(Subtitle subtitle, Response[] responses, float timeout)
        {
            var safeguardTime = Time.time + 10f; // Avoid infinite loops.
            while (isInPreDelay && Time.time < safeguardTime)
            {
                yield return null;
            }
            ShowResponsesNow(subtitle, responses, timeout);
        }

        protected virtual void ShowResponsesNow(Subtitle subtitle, Response[] responses, float timeout)
        {
            Tools.SetGameObjectActive(npcPreDelaySettings.preDelayIcon, false);
            Tools.SetGameObjectActive(pcPreDelaySettings.preDelayIcon, false);
            base.ShowResponses(subtitle, responses, timeout);
            ScrollToBottom(); //--- Now does smooth scroll: StartCoroutine(JumpToBottom());
        }

        protected virtual DialogueActor GetDialogueActor(Subtitle subtitle)
        {
            if (subtitle.speakerInfo.transform == null) return null;
            DialogueActor dialogueActor;
            if (dialogueActorCache.TryGetValue(subtitle.speakerInfo.transform, out dialogueActor))
            {
                return dialogueActor;
            }
            dialogueActor = DialogueActor.GetDialogueActorComponent(subtitle.speakerInfo.transform);
            dialogueActorCache[subtitle.speakerInfo.transform] = dialogueActor;
            return dialogueActor;
        }

        protected virtual StandardUISubtitlePanel GetTemplate(Subtitle subtitle, DialogueActor dialogueActor)
        {
            var panelNumber = (dialogueActor != null) ? dialogueActor.GetSubtitlePanelNumber() : SubtitlePanelNumber.Default;
            return (panelNumber == SubtitlePanelNumber.Default)
                ? (subtitle.speakerInfo.IsNPC ? conversationUIElements.defaultNPCSubtitlePanel : conversationUIElements.defaultPCSubtitlePanel)
                : conversationUIElements.subtitlePanels[PanelNumberUtility.GetSubtitlePanelIndex(panelNumber)];
        }

        // This method exists in case a user has subclassed SMSDialogueUI and overridden the old GetTemplate() signature.
        protected virtual StandardUISubtitlePanel GetTemplate(Subtitle subtitle)
        {
            return GetTemplate(subtitle, null);
        }

        protected virtual IEnumerator AddMessageWithPreDelay(float preDelay, Subtitle subtitle)
        {
            isInPreDelay = true;
            var preDelayIcon = subtitle.speakerInfo.isNPC ? npcPreDelaySettings.preDelayIcon : pcPreDelaySettings.preDelayIcon;
            Tools.SetGameObjectActive(preDelayIcon, true);
            if (preDelayIcon != null) preDelayIcon.transform.SetAsLastSibling();
            ScrollToBottom();
            yield return new WaitForSeconds(preDelay);
            Sequencer.Message(subtitle.speakerInfo.isNPC ? "Received" : "Sent");
            Tools.SetGameObjectActive(preDelayIcon, false);
            AddMessage(subtitle);
            isInPreDelay = false;
            yield return null;
        }

        /// <summary>
        /// Adds the subtitle as a message in the UI.
        /// </summary>
        protected virtual void AddMessage(Subtitle subtitle)
        {
            var dialogueActor = GetDialogueActor(subtitle);
            var template = GetTemplate(subtitle, dialogueActor);
            var go = Instantiate(template.panel.gameObject) as GameObject;
            var text = subtitle.formattedText.text;
            go.name = (text.Length <= 20) ? text : text.Substring(0, Mathf.Min(20, text.Length)) + "...";
            instantiatedMessages.Add(go);
            go.transform.SetParent(messagePanel.transform, false);
            var panel = go.GetComponent<StandardUISubtitlePanel>();
            if (panel.addSpeakerName)
            {
                subtitle.formattedText.text = string.Format(panel.addSpeakerNameFormat, new object[] { subtitle.speakerInfo.Name, subtitle.formattedText.text });
            }
            if (dialogueActor != null && dialogueActor.standardDialogueUISettings.setSubtitleColor)
            {
                subtitle.formattedText.text = dialogueActor.AdjustSubtitleColor(subtitle);
            }
            panel.ShowSubtitle(subtitle);
            continueButton = panel.continueButton;
            if (shouldShowContinueButton && !isLoadingGame)
            {
                panel.ShowContinueButton();
            }
            else
            {
                panel.HideContinueButton();
            }
            if (isLoadingGame)
            {
                var typewriter = panel.GetTypewriter();
                if (typewriter != null) typewriter.Stop();
            }
            if (maxMessages > 0 && instantiatedMessages.Count > maxMessages)
            {
                Destroy(instantiatedMessages[0]);
                instantiatedMessages.RemoveAt(0);
            }
            ScrollToBottom(); //--- Now does smooth scroll: StartCoroutine(JumpToBottom());
        }

        public override void ShowContinueButton(Subtitle subtitle)
        {
            shouldShowContinueButton = true;
        }

        public override void OnContinueConversation()
        {
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            base.OnContinueConversation();
        }

        protected virtual T FindUIElement<T>(Transform t, string name1, string name2) where T : MonoBehaviour
        {
            if (t == null) return null;
            if (string.Equals(t.name, name1) || string.Equals(t.name, name2)) return t.GetComponent<T>();
            foreach (Transform child in t)
            {
                var childResult = FindUIElement<T>(child, name1, name2);
                if (childResult != null) return childResult;
            }
            return null;
        }

        protected virtual void ScrollToBottom()
        {
            if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
            scrollCoroutine = StartCoroutine(ScrollToBottomCoroutine());
        }

        protected virtual IEnumerator ScrollToBottomCoroutine()
        {
            if (scrollRect == null) yield break;
            yield return null;
            var contentHeight = contentPanel.rect.height;
            var scrollRectHeight = scrollRect.GetComponent<RectTransform>().rect.height;
            var needToScroll = contentHeight > scrollRectHeight;
            if (needToScroll)
            {
                var ratio = scrollRectHeight / contentHeight;
                var timeout = Time.time + 10f; // Avoid infinite loops by maxing out at 10 seconds.
                while (scrollRect.verticalNormalizedPosition > 0.01f && Time.time < timeout)
                {
                    var newPos = scrollRect.verticalNormalizedPosition - scrollSpeed * Time.deltaTime * ratio;
                    scrollRect.verticalNormalizedPosition = Mathf.Max(0, newPos);
                    yield return null;
                }
            }
            scrollRect.verticalNormalizedPosition = 0;
            scrollCoroutine = null;
        }

        /// <summary>
        /// Records the subtitle in the history so it can be included in saved games.
        /// </summary>
        /// <param name="subtitle"></param>
        protected virtual void AddRecord(Subtitle subtitle)
        {
            if (skipNextRecord)
            {
                skipNextRecord = false;
                return;
            }
            records.Add(new DialogueEntryRecord(subtitle.dialogueEntry.conversationID, subtitle.dialogueEntry.id));
            if (maxMessages > 0 && records.Count > maxMessages)
            {
                records.RemoveAt(0);
            }
        }

        public virtual void ClearContent()
        {
            records.Clear();
            DestroyInstantiatedMessages();
        }

        protected virtual void DestroyInstantiatedMessages()
        {
            for (int i = 0; i < instantiatedMessages.Count; i++)
            {
                Destroy(instantiatedMessages[i]);
            }
            instantiatedMessages.Clear();
        }

        protected virtual bool DontLoadInThisScene()
        {
            if (dontLoadConversationInScenes == null) return false;
            for (int i = 0; i < dontLoadConversationInScenes.Length; i++)
            {
                if (dontLoadConversationInScenes[i] == currentSceneIndex) return true;
            }
            return false;
        }

        public string conversationVariableValue
        {
            get { return DialogueLua.GetVariable("Conversation").AsString; }
        }

        public string currentConversationActor
        {
            get { return useConversationVariable ? "CurrentConversationActor_" + conversationVariableValue : "CurrentConversationActor"; }
        }

        public string currentConversationConversant
        {
            get { return useConversationVariable ? "CurrentConversationConversant_" + conversationVariableValue : "CurrentConversationConversant"; }
        }

        public string currentDialogueEntryRecords
        {
            get { return useConversationVariable ? "DialogueEntryRecords_" + conversationVariableValue : "DialogueEntryRecords"; }
        }

        /// <summary>
        /// When saving game data, save the current actor, conversant, and dialogue entry records.
        /// </summary>
        public virtual void OnRecordPersistentData()
        {
            if (DontLoadInThisScene()) return;
            if (!DialogueManager.IsConversationActive) return;
            if (Debug.isDebugBuild) Debug.Log("TextlineDialogueUI.OnRecordPersistentData: Saving current conversation to " + currentDialogueEntryRecords);
            // Save actor & conversant:
            var actorName = (DialogueManager.CurrentActor != null) ? DialogueManager.CurrentActor.name : string.Empty;
            var conversantName = (DialogueManager.CurrentConversant != null) ? DialogueManager.CurrentConversant.name : string.Empty;
            DialogueLua.SetVariable(currentConversationActor, actorName);
            DialogueLua.SetVariable(currentConversationConversant, conversantName);

            // Save dialogue entry records:
            var s = records.Count + ";";
            foreach (var record in records)
            {
                s += record.conversationID + ";" + record.entryID + ";";
            }
            DialogueLua.SetVariable(currentDialogueEntryRecords, s);
        }

        /// <summary>
        /// When loading a game, load the dialogue entry records and resume the conversation.
        /// </summary>
        public virtual void OnApplyPersistentData()
        {
            if (!string.IsNullOrEmpty(conversationVariableOverride))
            {
                DialogueLua.SetVariable("Conversation", conversationVariableOverride);
            }

            if (DontLoadInThisScene()) Debug.Log("OnApplyPersistentData Dont Load in this scene: " + SceneManager.GetActiveScene().buildIndex);
            if (DontLoadInThisScene()) return;
            records.Clear();
            if (!DialogueLua.DoesVariableExist(currentDialogueEntryRecords)) return;
            StopAllCoroutines();

            // Load dialogue entry records:
            var s = DialogueLua.GetVariable(currentDialogueEntryRecords).AsString;
            if (Debug.isDebugBuild) Debug.Log("TextlineDialogueUI.OnApplyPersistentData: Restoring current conversation from " + currentDialogueEntryRecords + ": " + s);
            var ints = s.Split(';');
            var numRecords = Tools.StringToInt(ints[0]);
            for (int i = 0; i < numRecords; i++)
            {
                var conversationID = Tools.StringToInt(ints[1 + i * 2]);
                var entryID = Tools.StringToInt(ints[2 + i * 2]);
                records.Add(new DialogueEntryRecord(conversationID, entryID));
            }

            // If we have records, resume the conversation:
            if (records.Count == 0) return;
            var lastRecord = records[records.Count - 1];
            if (lastRecord.conversationID >= 0 && lastRecord.entryID > 0)
            {
                UnityEngine.UI.Button lastContinueButton = null;
                try
                {
                    // Resume conversation:
                    isLoadingGame = true;
                    var conversation = DialogueManager.MasterDatabase.GetConversation(lastRecord.conversationID);
                    var actorName = DialogueLua.GetVariable(currentConversationActor).AsString;
                    var conversantName = DialogueLua.GetVariable(currentConversationConversant).AsString;
                    var actor = GameObject.Find(actorName);
                    var conversant = GameObject.Find(conversantName);
                    var actorTransform = (actor != null) ? actor.transform : null;
                    var conversantTransform = (conversant != null) ? conversant.transform : null;
                    if (Debug.isDebugBuild) Debug.Log("Resuming '" + conversation.Title + "' at entry " + lastRecord.entryID);
                    DialogueManager.StopConversation();
                    var lastEntry = DialogueManager.MasterDatabase.GetDialogueEntry(lastRecord.conversationID, lastRecord.entryID);
                    var originalSequence = lastEntry.Sequence; // Handle last entry's sequence differently if end entry.
                    npcPreDelaySettings.CopyTo(npcPreDelaySettingsCopy);
                    pcPreDelaySettings.CopyTo(pcPreDelaySettingsCopy);
                    npcPreDelaySettings.basedOnTextLength = false;
                    npcPreDelaySettings.additionalSeconds = 0;
                    pcPreDelaySettings.basedOnTextLength = false;
                    pcPreDelaySettings.additionalSeconds = 0;
                    var isEndEntry = lastEntry.Sequence.Contains("WaitForMessage(Forever)") || lastEntry.outgoingLinks.Count == 0;
                    if (isEndEntry)
                    {
                        if (!lastEntry.Sequence.Contains("WaitForMessage(Forever)"))
                        {
                            lastEntry.Sequence = "WaitForMessage(Forever); " + lastEntry.Sequence;
                        }
                    }
                    else if (dontRepeatLastSequence)
                    {
                        lastEntry.Sequence = "None()";
                    }
                    else
                    {
                        //--- Replay entire last sequence: lastEntry.Sequence = "Delay(0.1)";
                        //--- Will send Sent/Received messages later in method in case sequences wait for them.
                        if (disableAudioOnLastSequence)
                        {
                            if (string.IsNullOrEmpty(lastEntry.Sequence))
                            {
                                lastEntry.Sequence = DialogueManager.displaySettings.cameraSettings.defaultSequence;
                            }
                            lastEntry.Sequence = lastEntry.Sequence.Replace("AudioWait(", "None(").Replace("Audio(", "None(");
                        }
                    }
                    skipNextRecord = true;
                    isInPreDelay = false;
                    DialogueManager.StartConversation(conversation.Title, actorTransform, conversantTransform, lastRecord.entryID);
                    lastContinueButton = continueButton;
                    lastEntry.Sequence = originalSequence;
                    npcPreDelaySettingsCopy.CopyTo(npcPreDelaySettings);
                    pcPreDelaySettingsCopy.CopyTo(pcPreDelaySettings);

                    // Populate UI with previous records:
                    var lastInstance = (instantiatedMessages.Count > 0) ? instantiatedMessages[instantiatedMessages.Count - 1] : null;
                    instantiatedMessages.Remove(lastInstance);
                    DestroyInstantiatedMessages();
                    for (int i = 0; i < records.Count - 1; i++)
                    {
                        var entry = DialogueManager.MasterDatabase.GetDialogueEntry(records[i].conversationID, records[i].entryID);
                        var speakerInfo = DialogueManager.ConversationModel.GetCharacterInfo(entry.ActorID);
                        var listenerInfo = DialogueManager.ConversationModel.GetCharacterInfo(entry.ConversantID);
                        var formattedText = FormattedText.Parse(entry.currentDialogueText, DialogueManager.MasterDatabase.emphasisSettings);
                        var subtitle = new Subtitle(speakerInfo, listenerInfo, formattedText, "None()", entry.ResponseMenuSequence, entry);
                        AddMessage(subtitle);
                    }
                    if (lastInstance != null)
                    {
                        instantiatedMessages.Add(lastInstance);
                        lastInstance.transform.SetAsLastSibling();
                    }

                    // Advance conversation if playing last sequence and it's configured to wait for Sent/Received messages:
                    if (!dontRepeatLastSequence)
                    {
                        Sequencer.Message("Sent");
                        Sequencer.Message("Received");
                    }
                }
                finally
                {
                    isLoadingGame = false;
                    scrollRect.verticalNormalizedPosition = 0;
                    continueButton = lastContinueButton;
                    if (shouldShowContinueButton && lastContinueButton != null) lastContinueButton.gameObject.SetActive(true);
                }
            }
            ScrollToBottom();
        }

        public void ClearRecords()
        {
            records.Clear();
        }
    }
}