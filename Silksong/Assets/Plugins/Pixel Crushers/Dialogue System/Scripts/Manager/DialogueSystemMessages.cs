// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The Dialogue System may send these messages using SendMessage or BroadcastMessage.
    /// </summary>
    public class DialogueSystemMessages
    {

        public const string OnConversationStart = "OnConversationStart";
        public const string OnConversationEnd = "OnConversationEnd";
        public const string OnConversationCancelled = "OnConversationCancelled";
        public const string OnPrepareConversationLine = "OnPrepareConversationLine";
        public const string OnConversationLine = "OnConversationLine";
        public const string OnConversationLineEnd = "OnConversationLineEnd";
        public const string OnConversationLineCancelled = "OnConversationLineCancelled";
        public const string OnTextChange = "OnTextChange";
        public const string OnConversationContinue = "OnConversationContinue";
        public const string OnConversationContinueAll = "OnConversationContinueAll";
        public const string OnConversationResponseMenu = "OnConversationResponseMenu";
        public const string OnConversationTimeout = "OnConversationTimeout";
        public const string OnLinkedConversationStart = "OnLinkedConversationStart";

        public const string OnBarkStart = "OnBarkStart";
        public const string OnBarkEnd = "OnBarkEnd";
        public const string OnBarkLine = "OnBarkLine";

        public const string OnSequenceStart = "OnSequenceStart";
        public const string OnSequenceEnd = "OnSequenceEnd";
        public const string OnSequencerMessage = "OnSequencerMessage";

        public const string OnQuestStateChange = "OnQuestStateChange";
        public const string OnQuestEntryStateChange = "OnQuestEntryStateChange";
        public const string OnQuestTrackingEnabled = "OnQuestTrackingEnabled";
        public const string OnQuestTrackingDisabled = "OnQuestTrackingDisabled";
        public const string UpdateTracker = "UpdateTracker";

        public const string OnDialogueSystemPause = "OnDialogueSystemPause";
        public const string OnDialogueSystemUnpause = "OnDialogueSystemUnpause";

        public const string OnShowAlert = "OnShowAlert";

    }
}
