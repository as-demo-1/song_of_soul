// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Display settings to apply to the dialogue UI and sequencer.
    /// </summary>
    [System.Serializable]
    public class DisplaySettings
    {

        [HideInInspector]
        public ConversationOverrideDisplaySettings conversationOverrideSettings = null;

        [Tooltip("Assign a GameObject that contains an active dialogue UI component. Can be a prefab. If unassigned, Dialogue Manager will search its children for an active dialogue UI component.")]
        public GameObject dialogueUI;

        [Tooltip("Optional. Assign Canvas into which dialogue UI will be instantiated if it's a prefab.")]
        public Canvas defaultCanvas;

        [System.Serializable]
        public class LocalizationSettings
        {
            /// <summary>
            /// The current language, or blank to use the default language.
            /// </summary>
            [Tooltip("Current language, or blank to use the default language.")]
            public string language = string.Empty;

            /// <summary>
            /// Set <c>true</c> to automatically use the system language at startup.
            /// </summary>
            [Tooltip("Use the system language at startup.")]
            public bool useSystemLanguage = false;

            /// <summary>
            /// An optional text table. Used by DialogueSystemController.GetLocalizedText()
            /// and ShowAlert() if assigned.
            /// </summary>
            [Tooltip("Optional localized text for alerts and other general text. Note: Now uses Text Table instead of Localized Text Table.")]
            public TextTable textTable = null;
            //---Was: public LocalizedTextTable localizedText = null;
        }

        public LocalizationSettings localizationSettings = new LocalizationSettings();

        [System.Serializable]
        public class SubtitleSettings
        {
            /// <summary>
            /// Specifies whether to show NPC subtitles while speaking a line of dialogue.
            /// </summary>
            [Tooltip("Show NPC subtitle text while NPC speaks a line of dialogue.")]
            public bool showNPCSubtitlesDuringLine = true;

            /// <summary>
            /// Specifies whether to should show NPC subtitles while presenting the player's follow-up
            /// responses.
            /// </summary>
            [Tooltip("Show NPC subtitle reminder text while showing the player response menu. If you're using Standard Dialogue UI, the subtitle panel's Visiblity value takes precedenece over this.")]
            public bool showNPCSubtitlesWithResponses = true;

            /// <summary>
            /// Specifies whether to show PC subtitles while speaking a line of dialogue.
            /// </summary>
            [Tooltip("Show PC subtitle text while PC speaks a line of dialogue. If Skip PC Subtitle After Response Menu (below) is ticked, PC subtitles from response menu selections will be skipped.")]
            public bool showPCSubtitlesDuringLine = false;

            /// <summary>
            /// Set <c>true</c> to allow PC subtitles to be used for the reminder line
            /// during the response menu.
            /// </summary>
            [Tooltip("Allow PC subtitles to be used for reminder text while showing the response menu.")]
            public bool allowPCSubtitleReminders = false;

            /// <summary>
            /// If the PC's subtitle came from a response menu selection, don't show the subtitle even if showPCSubtitlesDuringLine is true.
            /// </summary>
            [Tooltip("If the PC's subtitle came from a response menu selection, don't show the subtitle even if Show PC Subtitles During Line is ticked.")]
            public bool skipPCSubtitleAfterResponseMenu = false;

            /// <summary>
            /// The default subtitle characters per second. This value is used to compute the default 
            /// duration to display a subtitle if no sequence is specified for a line of dialogue.
            /// This value is also used when displaying alerts.
            /// </summary>
            [Tooltip("Used to compute default duration to display subtitle. Typewriter effects have their own separate setting.")]
            public float subtitleCharsPerSecond = 30f;

            /// <summary>
            /// The minimum duration to display a subtitle if no sequence is specified for a line of 
            /// dialogue. This value is also used when displaying alerts.
            /// </summary>
            [Tooltip("Minimum default duration to display subtitle.")]
            public float minSubtitleSeconds = 2f;

            public enum ContinueButtonMode
            {
                /// <summary>
                /// Never wait for the continue button. Use this if your UI doesn't have continue buttons.
                /// </summary>
                Never,

                /// <summary>
                /// Always wait for the continue button.
                /// </summary>
                Always,

                /// <summary>
                /// Show the continue button but don't wait for it.
                /// </summary>
                Optional,

                /// <summary>
                /// Wait for the continue button, except when the response menu is next show but don't wait.
                /// </summary>
                OptionalBeforeResponseMenu,

                /// <summary>
                /// Wait for the continue button, except when the response menu is next hide it.
                /// </summary>
                NotBeforeResponseMenu,

                /// <summary>
                /// Wait for the continue button, except when a PC auto-select response or response
                /// menu is next, show but don't wait.
                /// </summary>
                OptionalBeforePCAutoresponseOrMenu,

                /// <summary>
                /// Wait for the continue button, except with a PC auto-select response or response
                /// menu is next, hide it.
                /// </summary>
                NotBeforePCAutoresponseOrMenu,

                /// <summary>
                /// Wait for the continue button, except when delivering PC lines show but don't wait.
                /// </summary>
                OptionalForPC,

                /// <summary>
                /// Wait for the continue button except when delivering PC lines.
                /// </summary>
                NotForPC,

                /// <summary>
                /// Wait for the continue button, except when preceding response menus or delivering PC lines don't wait.
                /// </summary>
                OptionalForPCOrBeforeResponseMenu,

                /// <summary>
                /// Wait for the continue button only for NPC lines that don't precede response menus.
                /// </summary>
                NotForPCOrBeforeResponseMenu,

                /// <summary>
                /// Wait for the continue button, except for PC lines and lines preceding a response menu or PC auto-select response don't wait.
                /// </summary>
                OptionalForPCOrBeforePCAutoresponseOrMenu,

                /// <summary>
                /// Wait for the continue button only for NPC lines that don't precede response menus or PC auto-select responses.
                /// </summary>
                NotForPCOrBeforePCAutoresponseOrMenu,

                /// <summary>
                /// Wait for continue button for PC lines but not for NPC lines.
                /// </summary>
                OnlyForPC
            }

            /// <summary>
            /// How to handle continue buttons.
            /// </summary>
            [Tooltip("How to handle continue buttons.")]
            public ContinueButtonMode continueButton = ContinueButtonMode.Never;

            [Tooltip("If ticked, always require continue button on subtitle that ends conversation. Overrides Continue Button dropdown above.")]
            public bool requireContinueOnLastLine = false;

            /// <summary>
            /// Set <c>true</c> to convert "[em#]" tags to rich text codes in formatted text.
            /// Your implementation of IDialogueUI must support rich text.
            /// </summary>
            [Tooltip("Use rich text codes for [em#] markup tags. If unticked, [em#] tag will apply color to entire text.")]
            public bool richTextEmphases = true;

            /// <summary>
            /// Set <c>true</c> to send OnSequenceStart and OnSequenceEnd messages with 
            /// every dialogue entry's sequence.
            /// </summary>
            [Tooltip("Send OnSequenceStart and OnSequenceEnd messages with every dialogue entry's sequence.")]
            public bool informSequenceStartAndEnd = false;
        }

        /// <summary>
        /// The subtitle settings.
        /// </summary>
        public SubtitleSettings subtitleSettings = new SubtitleSettings();

        [System.Serializable]
        public class CameraSettings
        {
            /// <summary>
            /// The camera (or prefab) to use for sequences. If unassigned, the sequencer will use the
            /// main camera; when the sequence is done, it will restore the main camera's original
            /// position.
            /// </summary>
            [Tooltip("Camera or prefab to use for sequences. If unassigned, sequences use the current main camera.")]
            public Camera sequencerCamera = null;

            /// <summary>
            /// An alternate camera object to use instead of sequencerCamera. Use this, for example,
            /// if you have an Oculus VR GameObject that's a parent of two cameras.  Currently this 
            /// <em>must</em> be an object in the scene, not a prefab.
            /// </summary>
            [Tooltip("If assigned, use instead of Sequencer Camera -- for example, Oculus VR GameObject. Can't be a prefab.")]
            public GameObject alternateCameraObject = null;

            /// <summary>
            /// The camera angle object (or prefab) to use for the "Camera()" sequence command. See
            /// @ref sequencerCommandCamera for more information.
            /// </summary>
            [Tooltip("Camera angle object or prefab. If unassigned, use default camera angle definitions.")]
            public GameObject cameraAngles = null;

            /// <summary>
            /// If conversation's sequences use Main Camera, leave camera in current position at end of conversation instead of restoring pre-conversation position.
            /// </summary>
            [Tooltip("If conversation's sequences use Main Camera, leave camera in current position at end of conversation instead of restoring pre-conversation position.")]
            public bool keepCameraPositionAtConversationEnd = false;

            /// <summary>
            /// The default sequence to use if the dialogue entry doesn't have a sequence defined 
            /// in its Sequence field. See @ref dialogueCreation and @ref sequencer for
            /// more information. The special keyword "{{end}}" gets replaced by the default
            /// duration for the subtitle being displayed.
            /// </summary>
            [Tooltip("Used when a dialogue entry doesn't define its own Sequence. Set to Delay({{end}}) to leave the camera untouched.")]
            [TextArea]
            public string defaultSequence = "Delay({{end}})";

            [Tooltip("If defined, overrides Default Sequence for player (PC) lines only.")]
            [TextArea]
            public string defaultPlayerSequence = string.Empty;

            [Tooltip("Used when a dialogue entry doesn't define its own Response Menu Sequence.")]
            [TextArea]
            public string defaultResponseMenuSequence = string.Empty;

            /// <summary>
            /// The format to use for the <c>entrytag</c> keyword.
            /// </summary>
            [Tooltip("Format to use for the 'entrytag' keyword.")]
            public EntrytagFormat entrytagFormat = EntrytagFormat.ActorName_ConversationID_EntryID;

            /// <summary>
            /// By default, Audio() and AudioWait() sequencer commands don't report 
            /// missing audio files to reduce Console spam during development. Set this
            /// true to report missing audio files.
            /// </summary>
            [Tooltip("By default, Audio() and AudioWait() sequencer commands don't report missing audio files to reduce Console spam during development.")]
            public bool reportMissingAudioFiles = false;

            /// <summary>
            /// Set <c>true</c> to disable the internal sequencer commands -- for example, if you
            /// want to replace them with your own.
            /// </summary>
            [HideInInspector]
            public bool disableInternalSequencerCommands = false;
        }

        /// <summary>
        /// The camera settings.
        /// </summary>
        public CameraSettings cameraSettings = new CameraSettings();

        [System.Serializable]
        public class InputSettings
        {

            /// <summary>
            /// If <c>true</c>, always forces the response menu even if there's only one response.
            /// If <c>false</c>, you can use the <c>[f]</c> tag to force a response.
            /// </summary>
            [Tooltip("Show the response menu even if there's only one response.")]
            public bool alwaysForceResponseMenu = true;

            /// <summary>
            /// If `true`, includes responses whose Conditions are false. The `enabled` field of
            /// those responses will be `false`.
            /// </summary>
            [Tooltip("Include responses whose Conditions are false. typically shown in a disabled state.")]
            public bool includeInvalidEntries = false;

            /// <summary>
            /// If not <c>0</c>, the duration in seconds that the player has to choose a response; 
            /// otherwise the currently-focused response is auto-selected. If no response is
            /// focused (e.g., hovered over), the first response is auto-selected. If <c>0</c>,
            /// there is no timeout; the player can take as long as desired to choose a response.
            /// </summary>
            [Tooltip("If nonzero, the duration in seconds until the response menu times out.")]
            public float responseTimeout = 0f;

            /// <summary>
            /// The response timeout action.
            /// </summary>
            [Tooltip("What to do if the response menu times out.")]
            public ResponseTimeoutAction responseTimeoutAction = ResponseTimeoutAction.ChooseFirstResponse;

            /// <summary>
            /// The em tag to wrap around old responses. A response is old if its SimStatus 
            /// is "WasDisplayed". You can change this from EmTag.None if you want to visually
            /// mark old responses in the player response menu.
            /// </summary>
            [Tooltip("The [em#] tag to wrap around responses that have been previously chosen.")]
            public EmTag emTagForOldResponses = EmTag.None;

            /// <summary>
            /// The em tag to wrap around invalid responses. You can change this from EmTag.None 
            /// if you want to visually mark invalid responses in the player response menu.
            /// </summary>
            [Tooltip("The [em#] tag to wrap around invalid responses. These responses are only shown if Include Invalid Entries is ticked.")]
            public EmTag emTagForInvalidResponses = EmTag.None;

            /// <summary>
            /// The buttons QTE (Quick Time Event) buttons. QTE 0 & 1 default to the buttons
            /// Fire1 and Fire2.
            /// </summary>
            [Tooltip("Input buttons mapped to QTEs.")]
            public string[] qteButtons = new string[] { "Fire1", "Fire2" };

            /// <summary>
            /// The key and/or button that allows the player to cancel subtitle sequences.
            /// </summary>
            [Tooltip("Key or button that cancels subtitle sequences.")]
            public InputTrigger cancel = new InputTrigger(KeyCode.Escape);

            /// <summary>
            /// The key and/or button that allows the player to cancel conversations.
            /// </summary>
            [Tooltip("Key or button that cancels active conversation while in response menu.")]
            public InputTrigger cancelConversation = new InputTrigger(KeyCode.Escape);
        }

        /// <summary>
        /// The input settings.
        /// </summary>
        public InputSettings inputSettings = new InputSettings();

        [System.Serializable]
        public class BarkSettings
        {

            /// <summary>
            /// Set <c>true</c> to allow barks to play during conversations.
            /// </summary>
            [Tooltip("Allow barks to play during conversations.")]
            public bool allowBarksDuringConversations = true;

            /// <summary>
            /// Show barks for this many characters per second. If zero, use Subtitle Settings > Subtitle Chars Per Second.
            /// </summary>
            [Tooltip("Show barks for this many characters per second. If zero, use Subtitle Settings > Subtitle Chars Per Second.")]
            public float barkCharsPerSecond = 0;

            /// <summary>
            /// Show barks for at least this many seconds. If zero, use Subtitle Settings > Min Subtitle Seconds.
            /// </summary>
            [Tooltip("Show barks  for at least this many seconds. If zero, use Subtitle Settings > Min Subtitle Seconds.")]
            public float minBarkSeconds = 0;

            /// <summary>
            /// If non-blank, play this sequence with barks that don't specify their own Sequence.
            /// </summary>
            [Tooltip("If non-blank, play this sequence with barks that don't specify their own Sequence.")]
            public string defaultBarkSequence = string.Empty;

        }

        /// <summary>
        /// The gameplay alert message settings.
        /// </summary>
        public BarkSettings barkSettings = new BarkSettings();

        [System.Serializable]
        public class AlertSettings
        {

            /// <summary>
            /// Set <c>true</c> to allow the dialogue UI to show alerts during conversations.
            /// </summary>
            [Tooltip("Allow the dialogue UI to show alerts during conversations.")]
            public bool allowAlertsDuringConversations = false;

            /// <summary>
            /// How often to check if the Lua Variable['Alert'] has been set. To disable
            /// automatic monitoring, set this to <c>0</c>.
            /// </summary>
            [Tooltip("If nonzero, check Variable['Alert'] at this frequency to show alert messages.")]
            public float alertCheckFrequency = 0f;

            /// <summary>
            /// Show alerts for this many characters per second. If zero, use Subtitle Settings > Subtitle Chars Per Second.
            /// </summary>
            [Tooltip("Show alerts for this many characters per second. If zero, use Subtitle Settings > Subtitle Chars Per Second.")]
            public float alertCharsPerSecond = 0;

            /// <summary>
            /// Show alerts for at least this many seconds. If zero, use Subtitle Settings > Min Subtitle Seconds.
            /// </summary>
            [Tooltip("Show alerts for at least this many seconds. If zero, use Subtitle Settings > Min Subtitle Seconds.")]
            public float minAlertSeconds = 0;

        }

        /// <summary>
        /// The gameplay alert message settings.
        /// </summary>
        public AlertSettings alertSettings = new AlertSettings();

        public bool ShouldUseOverrides()
        {
            return (conversationOverrideSettings != null) && conversationOverrideSettings.useOverrides;
        }

        public bool ShouldUseSubtitleOverrides()
        {
            return ShouldUseOverrides() && conversationOverrideSettings.overrideSubtitleSettings;
        }

        public bool GetShowNPCSubtitlesDuringLine()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.showNPCSubtitlesDuringLine :
                ((subtitleSettings != null) ? subtitleSettings.showNPCSubtitlesDuringLine : true);
        }

        public bool GetShowNPCSubtitlesWithResponses()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.showNPCSubtitlesWithResponses :
                ((subtitleSettings != null) ? subtitleSettings.showNPCSubtitlesWithResponses : true);
        }

        public bool GetShowPCSubtitlesDuringLine()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.showPCSubtitlesDuringLine :
                ((subtitleSettings != null) ? subtitleSettings.showPCSubtitlesDuringLine : true);
        }

        public bool GetSkipPCSubtitleAfterResponseMenu()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.skipPCSubtitleAfterResponseMenu :
                ((subtitleSettings != null) ? subtitleSettings.skipPCSubtitleAfterResponseMenu: true);
        }

        public float GetSubtitleCharsPerSecond()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.subtitleCharsPerSecond :
                ((subtitleSettings != null) ? subtitleSettings.subtitleCharsPerSecond : 30);
        }


        public float GetMinSubtitleSeconds()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.minSubtitleSeconds :
                ((subtitleSettings != null) ? subtitleSettings.minSubtitleSeconds : 2);
        }

        public SubtitleSettings.ContinueButtonMode GetContinueButtonMode()
        {
            return ShouldUseSubtitleOverrides() ? conversationOverrideSettings.continueButton :
                ((subtitleSettings != null) ? subtitleSettings.continueButton : SubtitleSettings.ContinueButtonMode.Never);
        }

        public bool ShouldUseSequenceOverrides()
        {
            return ShouldUseOverrides() && conversationOverrideSettings.overrideSequenceSettings;
        }

        public string GetDefaultSequence()
        {
            return ShouldUseSequenceOverrides() && !string.IsNullOrEmpty(conversationOverrideSettings.defaultSequence) ? conversationOverrideSettings.defaultSequence :
                ((cameraSettings != null) ? cameraSettings.defaultSequence : string.Empty);
        }

        public string GetDefaultPlayerSequence()
        {
            return ShouldUseSequenceOverrides() && !string.IsNullOrEmpty(conversationOverrideSettings.defaultPlayerSequence) ? conversationOverrideSettings.defaultPlayerSequence :
                ((cameraSettings != null) ? cameraSettings.defaultPlayerSequence : string.Empty);
        }

        public string GetDefaultResponseMenuSequence()
        {
            return ShouldUseSequenceOverrides() && !string.IsNullOrEmpty(conversationOverrideSettings.defaultResponseMenuSequence) ? conversationOverrideSettings.defaultResponseMenuSequence :
                ((cameraSettings != null) ? cameraSettings.defaultResponseMenuSequence : string.Empty);
        }

        public bool ShouldUseInputOverrides()
        {
            return ShouldUseOverrides() && conversationOverrideSettings.overrideInputSettings;
        }

        public bool GetAlwaysForceResponseMenu()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.alwaysForceResponseMenu :
                ((inputSettings != null) ? inputSettings.alwaysForceResponseMenu : true);
        }

        public bool GetIncludeInvalidEntries()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.includeInvalidEntries :
                ((inputSettings != null) ? inputSettings.includeInvalidEntries : true);
        }

        public float GetResponseTimeout()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.responseTimeout :
                ((inputSettings != null) ? inputSettings.responseTimeout : 0);
        }

        public EmTag GetEmTagForOldResponses()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.emTagForOldResponses :
                ((inputSettings != null) ? inputSettings.emTagForOldResponses : EmTag.None);
        }

        public EmTag GetEmTagForInvalidResponses()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.emTagForInvalidResponses :
                ((inputSettings != null) ? inputSettings.emTagForInvalidResponses : EmTag.None);
        }

        public InputTrigger GetCancelSubtitleInput()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.cancelSubtitle :
                ((inputSettings != null) ? inputSettings.cancel : null);
        }

        public InputTrigger GetCancelConversationInput()
        {
            return ShouldUseInputOverrides() ? conversationOverrideSettings.cancelConversation :
                ((inputSettings != null) ? inputSettings.cancelConversation : null);
        }

    }

    /// <summary>
    /// Response timeout action specifies what to do if the response menu times out.
    /// </summary>
    public enum ResponseTimeoutAction
    {
        /// <summary>
        /// Auto-select the first menu choice.
        /// </summary>
        ChooseFirstResponse,

        /// <summary>
        /// Auto-select a random menu choice.
        /// </summary>
        ChooseRandomResponse,

        /// <summary>
        /// End of conversation.
        /// </summary>
        EndConversation,

        /// <summary>
        /// Auto-select current menu choice.
        /// </summary>
        ChooseCurrentResponse,

        /// <summary>
        /// Auto-select the last menu choice.
        /// </summary>
        ChooseLastResponse,

        /// <summary>
        /// Use a custom handler.
        /// </summary>
        Custom
    };

    public enum EmTag
    {
        None,
        Em1,
        Em2,
        Em3,
        Em4
    }

}
