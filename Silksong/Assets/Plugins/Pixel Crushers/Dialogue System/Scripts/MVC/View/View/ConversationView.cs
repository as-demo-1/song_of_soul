// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    public delegate void DialogueEntrySpokenDelegate(Subtitle subtitle);

    public delegate float GetDefaultSubtitleDurationDelegate(string text);

    /// <summary>
    /// Handles the user interaction part of a conversation. The ConversationController 
    /// provides the content. ConversationView processes UI events and hands control to 
    /// ConversationController.
    /// </summary>
    public class ConversationView : MonoBehaviour
    {

        /// <summary>
        /// You can assign a function here to override the method used to 
        /// determine the default subtitle duration -- that is, the value of {{end}}.
        /// </summary>
        public static GetDefaultSubtitleDurationDelegate overrideGetDefaultSubtitleDuration = null;

        /// <summary>
        /// Called when a subtitle is finished displaying (including text delay and cutscene
        /// sequence).
        /// </summary>
        public event EventHandler FinishedSubtitleHandler = null;

        /// <summary>
        /// Called when the player selects a response.
        /// </summary>
        public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler = null;

        private delegate bool IsCancelKeyDownDelegate();

        private IDialogueUI ui = null;
        private Sequencer m_sequencer = null;
        private DisplaySettings settings = null;
        private Subtitle lastNPCSubtitle = null;
        private Subtitle lastPCSubtitle = null;
        private Subtitle lastSubtitle = null;
        private IsCancelKeyDownDelegate IsCancelKeyDown = null;
        private Action CancelledHandler = null;
        private DialogueEntrySpokenDelegate dialogueEntrySpokenHandler = null;
        private bool waitForContinue = false;
        private bool notifyOnFinishSubtitle = false;
        private bool isPlayingResponseMenuSequence = false;
        private int initialFrameCount;

        public DisplaySettings displaySettings { get { return settings; } }

        public bool isWaitingForContinue { get { return waitForContinue; } }

        public Sequencer sequencer { get { return m_sequencer; } }

        public IDialogueUI dialogueUI 
        { 
            get 
            { 
                return ui; 
            } 
            set 
            {
                if (ui != value)
                {
                    ui.SelectedResponseHandler -= OnSelectedResponse;
                    ui.Close();
                    ui = value;
                    ui.Open();
                    ui.SelectedResponseHandler += OnSelectedResponse;
                }
            } 
        }

        /// <summary>
        /// Initialize a UI and sequencer with displaySettings.
        /// </summary>
        /// <param name='ui'>
        /// Dialogue UI.
        /// </param>
        /// <param name='sequencer'>
        /// Sequencer.
        /// </param>
        /// <param name='displaySettings'>
        /// Display settings to initiate the UI and sequencer with.
        /// </param>
        public void Initialize(IDialogueUI ui, Sequencer sequencer, DisplaySettings displaySettings, DialogueEntrySpokenDelegate dialogueEntrySpokenHandler)
        {
            this.ui = ui;
            this.m_sequencer = sequencer;
            this.settings = displaySettings;
            this.dialogueEntrySpokenHandler = dialogueEntrySpokenHandler;
            this.initialFrameCount = Time.frameCount;
            ui.Open();
            sequencer.Open();
            ui.SelectedResponseHandler += OnSelectedResponse;
            sequencer.FinishedSequenceHandler += OnFinishedSubtitle;
        }

        /// <summary>
        /// Close the conversation view.
        /// </summary>
        public void Close()
        {
            ui.SelectedResponseHandler -= OnSelectedResponse;
            m_sequencer.FinishedSequenceHandler -= OnFinishedSubtitle;
            ui.Close();
            m_sequencer.Close();
            Destroy(this);
        }

        /// <summary>
        /// Checks if the player has cancelled the conversation.
        /// </summary>
        public void Update()
        {
            if (Cancelled() && (CancelledHandler != null)) CancelledHandler();
        }

        private bool Cancelled()
        {
            return (IsCancelKeyDown == null) ? false : IsCancelKeyDown();
        }

        private bool IsSubtitleCancelKeyDown()
        {
            return settings.GetCancelSubtitleInput().isDown;
        }

        private bool IsConversationCancelKeyDown()
        {
            return settings.GetCancelConversationInput().isDown;
        }

        /// <summary>
        /// Starts displaying a subtitle.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to display.
        /// </param>
        /// <param name='isPCResponseNext'> 
        /// Indicates whether the next stage is the player or NPC.
        /// </param>
        /// <param name='isPCAutoResponseNext'> 
        /// Indicates whether the next stage is a player auto-response.
        /// </param>
        public void StartSubtitle(Subtitle subtitle, bool isPCResponseMenuNext, bool isPCAutoResponseNext)
        {
            notifyOnFinishSubtitle = true;
            if (subtitle != null)
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: {1} says '{2}'", new System.Object[] { DialogueDebug.Prefix, Tools.GetGameObjectName(subtitle.speakerInfo.transform), subtitle.formattedText.text }));
                NotifyParticipantsOnConversationLine(subtitle);

                m_sequencer.SetParticipants(subtitle.speakerInfo.transform, subtitle.listenerInfo.transform);
                m_sequencer.entrytag = subtitle.entrytag;
                m_sequencer.subtitleEndTime = GetDefaultSubtitleDuration(subtitle.formattedText.text);
                if (!string.IsNullOrEmpty(subtitle.sequence) && subtitle.sequence.Contains("{{default}}"))
                {
                    subtitle.sequence = subtitle.sequence.Replace("{{default}}", GetDefaultSequence(subtitle));
                }
                subtitle.sequence = string.IsNullOrEmpty(subtitle.sequence) ? GetDefaultSequence(subtitle) : PreprocessSequence(subtitle);

                if (ShouldShowSubtitle(subtitle))
                {
                    ui.ShowSubtitle(subtitle);

                    // Save this info in case SetContinueMode() sequencer command forces reevaluation:
                    _subtitle = subtitle;
                    _isPCResponseMenuNext = isPCResponseMenuNext;
                    _isPCAutoResponseNext = isPCAutoResponseNext;

                    SetupContinueButton(subtitle, isPCResponseMenuNext, isPCAutoResponseNext);

                }
                else
                {
                    waitForContinue = false;
                }
                if (subtitle.speakerInfo.isNPC)
                {
                    lastNPCSubtitle = subtitle;
                }
                else
                {
                    lastPCSubtitle = subtitle;
                }
                lastSubtitle = subtitle;
                if (dialogueEntrySpokenHandler != null) dialogueEntrySpokenHandler(subtitle);
                m_sequencer.PlaySequence(subtitle.sequence, settings.subtitleSettings.informSequenceStartAndEnd, false);
            }
            else
            {
                FinishSubtitle();
            }
            IsCancelKeyDown = IsSubtitleCancelKeyDown;
            CancelledHandler = OnCancelSubtitle;
            if (!string.IsNullOrEmpty(subtitle.formattedText.text)) _lastModeWasResponseMenu = false;
        }

        private Subtitle _subtitle = null;
        private bool _isPCResponseMenuNext = false;
        private bool _isPCAutoResponseNext = false;
        private bool _lastModeWasResponseMenu = false;

        /// <summary>
        /// Determines whether the continue button should be shown, and shows or hides it.
        /// Call this if you've manually changed the continue button mode while the
        /// conversation is displaying a line. In most cases you won't ever need to call this
        /// manually.
        /// </summary>
        public void SetupContinueButton()
        {
            SetupContinueButton(_subtitle, _isPCResponseMenuNext, _isPCAutoResponseNext);
        }

        private void SetupContinueButton(Subtitle subtitle, bool isPCResponseMenuNext, bool isPCAutoResponseNext)
        {
            if (subtitle == null) return;
            var isPCLine = subtitle.speakerInfo.characterType == CharacterType.PC;
            waitForContinue = ShouldWaitForContinueButton(isPCLine, isPCResponseMenuNext, isPCAutoResponseNext);
            var showContinueButton = ShouldShowContinueButton(isPCLine, isPCResponseMenuNext, isPCAutoResponseNext);
            if (waitForContinue)
            {
                if (string.IsNullOrEmpty(subtitle.formattedText.text) && (subtitle.dialogueEntry.id == 0))
                {
                    waitForContinue = false;
                }
            }
            var abstractDialogueUI = ui as AbstractDialogueUI;
            if (abstractDialogueUI != null)
            {
                if (showContinueButton)
                {
                    abstractDialogueUI.ShowContinueButton(subtitle);
                }
                else
                {
                    abstractDialogueUI.HideContinueButton(subtitle);
                }
            }
        }

        private bool ShouldWaitForContinueButton(bool isPCLine, bool isPCResponseMenuNext, bool isPCAutoResponseNext)
        {
            switch (settings.GetContinueButtonMode())
            {
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.Always:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.Never:
                    return false;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.Optional:
                    return false;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforeResponseMenu:
                    return !isPCResponseMenuNext;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforeResponseMenu:
                    return !isPCResponseMenuNext;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforePCAutoresponseOrMenu:
                    return !(isPCResponseMenuNext || isPCAutoResponseNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforePCAutoresponseOrMenu:
                    return !(isPCResponseMenuNext || isPCAutoResponseNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalForPC:
                    return !isPCLine;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotForPC:
                    return !isPCLine;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalForPCOrBeforeResponseMenu:
                    return !(isPCLine || isPCResponseMenuNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotForPCOrBeforeResponseMenu:
                    return !(isPCLine || isPCResponseMenuNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalForPCOrBeforePCAutoresponseOrMenu:
                    return !(isPCLine || isPCResponseMenuNext || isPCAutoResponseNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotForPCOrBeforePCAutoresponseOrMenu:
                    return !(isPCLine || isPCResponseMenuNext || isPCAutoResponseNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OnlyForPC:
                    return isPCLine;
                default:
                    return false;
            }
        }

        private bool ShouldShowContinueButton(bool isPCLine, bool isPCResponseMenuNext, bool isPCAutoResponseNext)
        {
            // If we require continue button on last line, we have to grab the current conversation state to check it:
            if (settings.subtitleSettings.requireContinueOnLastLine && !DialogueManager.instance.currentConversationState.hasAnyResponses)
            {
                // Note: side effect - set waitForContinue true
                waitForContinue = true;
                return true;
            }
            // Should we show the continue button? (Even if optional and not waiting for it)
            switch (settings.GetContinueButtonMode())
            {
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.Always:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.Never:
                    return false;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.Optional:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforeResponseMenu:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforeResponseMenu:
                    return !isPCResponseMenuNext;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforePCAutoresponseOrMenu:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforePCAutoresponseOrMenu:
                    return !(isPCResponseMenuNext || isPCAutoResponseNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalForPC:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotForPC:
                    return !isPCLine;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalForPCOrBeforeResponseMenu:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotForPCOrBeforeResponseMenu:
                    return !(isPCLine || isPCResponseMenuNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalForPCOrBeforePCAutoresponseOrMenu:
                    return true;
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotForPCOrBeforePCAutoresponseOrMenu:
                    return !(isPCLine || isPCResponseMenuNext || isPCAutoResponseNext);
                case DisplaySettings.SubtitleSettings.ContinueButtonMode.OnlyForPC:
                    return isPCLine;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Shows the most recently displayed subtitle.
        /// </summary>
        public void ShowLastNPCSubtitle()
        {
            if (ShouldShowLastNPCSubtitle()) ui.ShowSubtitle(lastNPCSubtitle);
            FinishSubtitle();
        }

        private bool ShouldShowLastNPCSubtitle()
        {
            return (settings != null) && settings.GetShowNPCSubtitlesWithResponses() &&
                (lastNPCSubtitle != null) && (lastNPCSubtitle.speakerInfo.characterType == CharacterType.NPC);
        }

        private bool ShouldShowLastPCSubtitle()
        {
            return (settings != null) && settings.GetShowNPCSubtitlesWithResponses() &&
                settings.subtitleSettings.allowPCSubtitleReminders &&
                (lastPCSubtitle != null) && (lastSubtitle == lastPCSubtitle) &&
                (lastPCSubtitle.speakerInfo.characterType == CharacterType.PC);
        }

        private bool ShouldShowSubtitle(Subtitle subtitle)
        {
            if ((subtitle != null) && (settings != null) && (settings.subtitleSettings != null))
            {
                if (subtitle.formattedText.noSubtitle || 
                    string.Equals(subtitle.sequence, "None()") || string.Equals(subtitle.sequence, "None();") ||
                    string.Equals(subtitle.sequence, "Continue()") || string.Equals(subtitle.sequence, "Continue();"))
                {
                    return false;
                }
                if ((subtitle.speakerInfo.characterType == CharacterType.NPC) && settings.GetShowNPCSubtitlesDuringLine())
                {
                    return true;
                }
                if ((subtitle.speakerInfo.characterType == CharacterType.PC) && settings.GetShowPCSubtitlesDuringLine())
                {
                    return !(_lastModeWasResponseMenu && settings.GetSkipPCSubtitleAfterResponseMenu());
                }
            }
            return false;
        }

        /// <summary>
        /// Continues this conversation if the dialogue UI matches.
        /// </summary>
        /// <param name="dialogueUI"></param>
        public void OnConversationContinue(IDialogueUI dialogueUI)
        {
            if (dialogueUI == this.ui)
            {
                HandleContinueButtonClick();
            }
        }

        /// <summary>
        /// Continues all conversations.
        /// </summary>
        public void OnConversationContinueAll()
        {
            HandleContinueButtonClick();
        }

        private void HandleContinueButtonClick()
        {
            // If we just started and another conversation just ended, ignore the continue:
            if (Time.frameCount == initialFrameCount && initialFrameCount == ConversationController.frameLastConversationEnded) return;
            waitForContinue = false;
            FinishSubtitle();
        }

        private void OnCancelSubtitle()
        {
            if (lastSubtitle == null) // Need to create a dummy subtitle to OnConversationLineCancelled's signature.
            {
                var dummySubtitle = new Subtitle(null, null, null, string.Empty, string.Empty, null);
                BroadcastMessage(DialogueSystemMessages.OnConversationLineCancelled, dummySubtitle, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                BroadcastMessage(DialogueSystemMessages.OnConversationLineCancelled, lastSubtitle, SendMessageOptions.DontRequireReceiver);
            }
            waitForContinue = false;
            FinishSubtitle();
        }

        private void FinishSubtitle()
        {
            if (!waitForContinue)
            {
                if (m_sequencer != null) m_sequencer.Stop();
                ui.HideSubtitle(lastSubtitle);
                if (notifyOnFinishSubtitle)
                {
                    notifyOnFinishSubtitle = false;
                    if (_subtitle != null) NotifyParticipantsOnConversationLineEnd(lastSubtitle);
                    if (FinishedSubtitleHandler != null) FinishedSubtitleHandler(this, EventArgs.Empty);
                }
            }
        }

        private void OnFinishedSubtitle()
        {
            FinishSubtitle();
        }

        /// <summary>
        /// Displays the player response menu.
        /// </summary>
        /// <param name='subtitle'>
        /// Last subtitle, to display as a reminder of what the player is responding to.
        /// </param>
        /// <param name='responses'>
        /// Responses.
        /// </param>
        public void StartResponses(Subtitle subtitle, Response[] responses)
        {
            PlayResponseMenuSequence(subtitle.responseMenuSequence);
            Subtitle lastSubtitle = ShouldShowLastPCSubtitle()
                ? lastPCSubtitle
                : ShouldShowLastNPCSubtitle()
                    ? lastNPCSubtitle
                    : null;
            NotifyOnResponseMenu(responses);
            ui.ShowResponses(lastSubtitle, responses, settings.GetResponseTimeout());
            IsCancelKeyDown = IsConversationCancelKeyDown;
            CancelledHandler = OnCancelResponseMenu;
            _lastModeWasResponseMenu = true;
        }

        private void PlayResponseMenuSequence(string responseMenuSequence)
        {
            if (string.IsNullOrEmpty(responseMenuSequence) && !string.IsNullOrEmpty(settings.GetDefaultResponseMenuSequence()))
            {
                responseMenuSequence = settings.GetDefaultResponseMenuSequence();
            }
            if (!string.IsNullOrEmpty(responseMenuSequence))
            {
                m_sequencer.FinishedSequenceHandler -= OnFinishedSubtitle;
                m_sequencer.Stop();
                m_sequencer.PlaySequence(responseMenuSequence);
                isPlayingResponseMenuSequence = true;
            }
        }

        private void StopResponseMenuSequence()
        {
            if (isPlayingResponseMenuSequence)
            {
                isPlayingResponseMenuSequence = false;
                m_sequencer.Stop();
                m_sequencer.StopAllCoroutines();
                m_sequencer.FinishedSequenceHandler += OnFinishedSubtitle;
            }
        }

        private void OnCancelResponseMenu()
        {
            NotifyParticipantsOnConversationCancelled();
            //---Was: BroadcastMessage(DialogueSystemMessages.OnConversationCancelled, sequencer.speaker, SendMessageOptions.DontRequireReceiver);
            SelectResponse(new SelectedResponseEventArgs(null));
        }

        private void OnSelectedResponse(object sender, SelectedResponseEventArgs e)
        {
            SelectResponse(e);
        }

        public void SelectResponse(SelectedResponseEventArgs e)
        {
            StopResponseMenuSequence();
            ui.HideResponses();
            if (SelectedResponseHandler != null) SelectedResponseHandler(this, e);
        }

        /// <summary>
        /// Gets the default sequence for a subtitle.
        /// </summary>
        /// <returns>
        /// The default sequence.
        /// </returns>
        /// <param name='subtitle'>
        /// Subtitle.
        /// </param>
        public string GetDefaultSequence(Subtitle subtitle)
        {
            float duration = m_sequencer.subtitleEndTime;
            var isPlayerLine = (subtitle.speakerInfo.characterType == CharacterType.PC);
            if (isPlayerLine && (!settings.GetShowPCSubtitlesDuringLine() || (_lastModeWasResponseMenu && settings.GetSkipPCSubtitleAfterResponseMenu())))
            {
                var playerSequence = settings.GetDefaultPlayerSequence();
                return Sequencer.ReplaceShortcuts(playerSequence).Replace(SequencerKeywords.End, duration.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                var line = settings.GetDefaultSequence();
                if (isPlayerLine && !string.IsNullOrEmpty(settings.GetDefaultPlayerSequence()))
                {
                    line = settings.GetDefaultPlayerSequence();
                }
                if (string.IsNullOrEmpty(line))
                {
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Delay({0})", new System.Object[] { duration });
                }
                else
                {
                    return Sequencer.ReplaceShortcuts(line).Replace(SequencerKeywords.End, duration.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        /// <returns>
        /// A duration based on the text length and the Dialogue Manager's 
        /// Subtitle Settings > Min Subtitle Seconds and Subtitle Chars Per Second.
        /// Also factors in time for RPGMaker-style pause codes.
        /// </returns>
        public float GetDefaultSubtitleDuration(string text)
        {
            return GetDefaultSubtitleDurationInSeconds(text, settings);
            //if (overrideGetDefaultSubtitleDuration != null) return overrideGetDefaultSubtitleDuration(text);
            //int numCharacters = string.IsNullOrEmpty(text) ? 0 : Tools.StripRichTextCodes(text).Length;
            //float numRPGMakerPauses = 0;
            //if (text.Contains("\\"))
            //{
            //    var numFullPauses = (text.Length - text.Replace("\\.", string.Empty).Length) / 2;
            //    var numQuarterPauses = (text.Length - text.Replace("\\,", string.Empty).Length) / 2;
            //    numRPGMakerPauses = (1.0f * numFullPauses) + (0.25f * numQuarterPauses);
            //}
            //return Mathf.Max(settings.GetMinSubtitleSeconds(), numRPGMakerPauses + (numCharacters / Mathf.Max(1, settings.GetSubtitleCharsPerSecond())));
        }

        /// <summary>
        /// A duration based on the text length and the Dialogue Manager's 
        /// Subtitle Settings > Min Subtitle Seconds and Subtitle Chars Per Second.
        /// Also factors in time for RPGMaker-style pause codes.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="displaySettings">If null, uses Dialogue Manager's Display Settings.</param>
        public static float GetDefaultSubtitleDurationInSeconds(string text, DisplaySettings displaySettings = null)
        {
            if (overrideGetDefaultSubtitleDuration != null) return overrideGetDefaultSubtitleDuration(text);
            var settings = displaySettings ?? DialogueManager.displaySettings;
            int numCharacters = string.IsNullOrEmpty(text) ? 0 : Tools.StripRichTextCodes(text).Length;
            float numRPGMakerPauses = 0;
            if (text.Contains("\\"))
            {
                var numFullPauses = (text.Length - text.Replace("\\.", string.Empty).Length) / 2;
                var numQuarterPauses = (text.Length - text.Replace("\\,", string.Empty).Length) / 2;
                numRPGMakerPauses = (1.0f * numFullPauses) + (0.25f * numQuarterPauses);
            }
            return Mathf.Max(settings.GetMinSubtitleSeconds(), numRPGMakerPauses + (numCharacters / Mathf.Max(1, settings.GetSubtitleCharsPerSecond())));
        }

        private string PreprocessSequence(Subtitle subtitle)
        {
            if ((subtitle == null) || (string.IsNullOrEmpty(subtitle.sequence))) return string.Empty;
            subtitle.sequence = Sequencer.ReplaceShortcuts(subtitle.sequence);
            if (!subtitle.sequence.Contains(SequencerKeywords.End)) return subtitle.sequence;
            float duration = m_sequencer.subtitleEndTime;
            return subtitle.sequence.Replace(SequencerKeywords.End, duration.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private void NotifyParticipantsOnConversationLine(Subtitle subtitle)
        {
            NotifyParticipants(DialogueSystemMessages.OnConversationLine, subtitle);
        }

        private void NotifyParticipantsOnConversationLineEnd(Subtitle subtitle)
        {
            NotifyParticipants(DialogueSystemMessages.OnConversationLineEnd, subtitle);
        }

        private void NotifyParticipants(string message, Subtitle subtitle)
        {
            if (subtitle != null)
            {
                bool validSpeakerTransform = CharacterInfoHasValidTransform(subtitle.speakerInfo);
                bool validListenerTransform = CharacterInfoHasValidTransform(subtitle.listenerInfo);
                bool speakerIsListener = validSpeakerTransform && validListenerTransform && (subtitle.speakerInfo.transform == subtitle.listenerInfo.transform);
                if (validSpeakerTransform) subtitle.speakerInfo.transform.BroadcastMessage(message, subtitle, SendMessageOptions.DontRequireReceiver);
                if (validListenerTransform && !speakerIsListener) subtitle.listenerInfo.transform.BroadcastMessage(message, subtitle, SendMessageOptions.DontRequireReceiver);
                DialogueManager.instance.BroadcastMessage(message, subtitle, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void NotifyOnResponseMenu(Response[] responses)
        {
            if (responses != null)
            {
                if (lastSubtitle != null)
                {
                    bool validSpeakerTransform = CharacterInfoHasValidTransform(lastSubtitle.speakerInfo);
                    bool validListenerTransform = CharacterInfoHasValidTransform(lastSubtitle.listenerInfo);
                    bool speakerIsListener = validSpeakerTransform && validListenerTransform && (lastSubtitle.speakerInfo.transform == lastSubtitle.listenerInfo.transform);
                    if (validSpeakerTransform) lastSubtitle.speakerInfo.transform.BroadcastMessage(DialogueSystemMessages.OnConversationResponseMenu, responses, SendMessageOptions.DontRequireReceiver);
                    if (validListenerTransform && !speakerIsListener) lastSubtitle.listenerInfo.transform.BroadcastMessage(DialogueSystemMessages.OnConversationResponseMenu, responses, SendMessageOptions.DontRequireReceiver);
                }
                DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnConversationResponseMenu, responses, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void NotifyParticipantsOnConversationCancelled()
        {
            if (lastSubtitle != null)
            {
                bool validSpeakerTransform = CharacterInfoHasValidTransform(lastSubtitle.speakerInfo);
                bool validListenerTransform = CharacterInfoHasValidTransform(lastSubtitle.listenerInfo);
                bool speakerIsListener = validSpeakerTransform && validListenerTransform && (lastSubtitle.speakerInfo.transform == lastSubtitle.listenerInfo.transform);
                if (validSpeakerTransform) lastSubtitle.speakerInfo.transform.BroadcastMessage(DialogueSystemMessages.OnConversationCancelled, m_sequencer.listener ?? transform, SendMessageOptions.DontRequireReceiver);
                if (validListenerTransform && !speakerIsListener) lastSubtitle.listenerInfo.transform.BroadcastMessage(DialogueSystemMessages.OnConversationCancelled, m_sequencer.speaker ?? transform, SendMessageOptions.DontRequireReceiver);
            }
            DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnConversationCancelled, m_sequencer.speaker ?? transform, SendMessageOptions.DontRequireReceiver);
        }

        private bool CharacterInfoHasValidTransform(CharacterInfo characterInfo)
        {
            return (characterInfo != null) && (characterInfo.transform != null);
        }

        /// <summary>
        /// Sets the PC portrait to use for the response menu.
        /// </summary>
        /// <param name="pcSprite">PC sprite.</param>
        /// <param name="pcName">PC name.</param>
        public void SetPCPortrait(Sprite pcSprite, string pcName)
        {
            var ui = DialogueManager.dialogueUI as AbstractDialogueUI;
            if (ui == null) return;
            ui.SetPCPortrait(pcSprite, pcName);
        }

        /// <summary>
        /// Sets the portrait sprite to use in the UI for an actor.
        /// This is used when the SetPortrait() sequencer command changes an actor's image.
        /// </summary>
        /// <param name="actorName">Actor name.</param>
        /// <param name="sprite">Portrait sprite.</param>
        public void SetActorPortraitSprite(string actorName, Sprite sprite)
        {
            var ui = DialogueManager.dialogueUI as AbstractDialogueUI;
            if (ui == null) return;
            ui.SetActorPortraitSprite(actorName, sprite);
        }

    }

}