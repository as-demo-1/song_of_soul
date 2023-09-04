// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    public enum EntrytagFormat
    {
        /// <summary>
        /// Format entrytag as "ActorName_ConversationID_EntryID".
        /// Special characters in ActorName will be replaced with underscores.
        /// Example: <c>Private_Hart_9_42</c>
        /// </summary>
        ActorName_ConversationID_EntryID,

        /// <summary>
        /// Format entrytag as "ConversationTitle_EntryID".
        /// Special characters in ConversationTitle will be replaced with underscores.
        /// Example: <c>Boardroom_Discussion_42</c>
        /// </summary>
        ConversationTitle_EntryID,

        /// <summary>
        /// Format entrytag Adventure Creator-style as "(ActorName)(LineNumber)".
        /// Special characters in ActorName will be replaced with underscores.
        /// The Dialogue System will attempt to assign a unique line number to 
        /// every entry using the formula ConversationID*500 + EntryID.
        /// Example: <c>Player42</c>
        /// </summary>
        ActorNameLineNumber,

        /// <summary>
        /// Format entrytag as "ConversationID_ActorName_EntryID".
        /// Special characters in ActorName will be replaced with underscores.
        /// Example: <c>9_Private_Hart_42</c>
        /// </summary>
        ConversationID_ActorName_EntryID,

        /// <summary>
        /// Format entrytag as "ActorName_ConversationTitle_EntryDescriptor".
        /// EntryDescriptor looks for an entry title first, then menu text, and
        /// if none, uses the ID instead.
        /// Special characters will be replaced with underscores.
        /// Example: <c>Private_Hart_Boardroom_Discussions_No_Information_Available</c>
        /// </summary>
        ActorName_ConversationTitle_EntryDescriptor,

        /// <summary>
        /// Set entrytag to the value of the dialogue entry's VoiceOverFile field.
        /// </summary>
        VoiceOverFile,

        /// <summary>
        /// Set entrytag to value of dialogue entry's Title field, defaulting to 
        /// ActorName_ConversationID_EntryID if Title isn't set.
        /// </summary>
        Title,

        /// <summary>
        /// You must assign a delegate function to DialogueDatabase.getCustomEntrytagFormat(Conversation, DialogueEntry).
        /// </summary>
        Custom = 99
    }

}
