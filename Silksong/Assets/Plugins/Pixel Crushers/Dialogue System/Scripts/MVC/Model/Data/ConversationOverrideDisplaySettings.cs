// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class ConversationOverrideDisplaySettings
    {
        public bool useOverrides = false;

        // Subtitle Settings:

        public bool overrideSubtitleSettings = false;
        public bool showNPCSubtitlesDuringLine = true;
        public bool showNPCSubtitlesWithResponses = true;
        public bool showPCSubtitlesDuringLine = false;
        public bool skipPCSubtitleAfterResponseMenu = false;
        public float subtitleCharsPerSecond = 30;
        public float minSubtitleSeconds = 2;
        public DisplaySettings.SubtitleSettings.ContinueButtonMode continueButton;

        // Camera & Cutscene Settings:

        public bool overrideSequenceSettings = false;
        [TextArea]
        public string defaultSequence;
        [TextArea]
        public string defaultPlayerSequence;
        [TextArea]
        public string defaultResponseMenuSequence;

        // Input Settings:

        public bool overrideInputSettings = false;
        public bool alwaysForceResponseMenu = true;
        public bool includeInvalidEntries = false;
        public float responseTimeout = 0;
        public EmTag emTagForOldResponses = EmTag.None;
        public EmTag emTagForInvalidResponses = EmTag.None;
        public InputTrigger cancelSubtitle = new InputTrigger(KeyCode.Escape);
        public InputTrigger cancelConversation = new InputTrigger(KeyCode.Escape);
    }

}
