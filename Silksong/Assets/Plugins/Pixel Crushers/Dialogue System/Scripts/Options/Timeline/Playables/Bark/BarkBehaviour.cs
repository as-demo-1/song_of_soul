#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class BarkBehaviour : PlayableBehaviour
    {

        [Tooltip("Get bark text from a conversation.")]
        public bool useConversation = true;

        [Tooltip("Get the bark text from this conversation.")]
        [ConversationPopup(true)]
        public string conversation;

        [Tooltip("Bark a specific dialogue entry instead of starting from the conversation's START node.")]
        public bool barkSpecificEntry;

        [Tooltip("Dialogue entry to bark.")]
        public int entryID;

        [Tooltip("Bark this text.")]
        public string text;

        [Tooltip("(Optional) Barker is barking to this listener.")]
        public Transform listener;

        public string GetEditorBarkText()
        {
            return useConversation 
                ? ("[" + conversation + "] '" + PreviewUI.GetDialogueText(conversation, -1) + "'")
                : text;
        }

    }
}
#endif
#endif
