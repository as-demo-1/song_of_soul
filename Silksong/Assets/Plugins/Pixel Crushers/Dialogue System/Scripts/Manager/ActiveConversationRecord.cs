// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Keeps track of an active conversation. As conversations finish, another
    /// active conversation record can be promoted to the "current" conversation.
    /// </summary>
    public class ActiveConversationRecord
    {
        public string conversationTitle { get; set; }

        public Transform actor { get; set; }

        public Transform conversant { get; set; }

        public ConversationController conversationController { get; set; }

        public ConversationModel conversationModel { get { return (conversationController != null) ? conversationController.conversationModel : null; } }

        public ConversationView conversationView { get { return (conversationController != null) ? conversationController.conversationView : null; } }

        public bool isConversationActive { get { return (conversationController != null) && conversationController.isActive; } }

        public IDialogueUI originalDialogueUI;
        public DisplaySettings originalDisplaySettings;
        public bool isOverrideUIPrefab;
        public bool dontDestroyPrefabInstance;

        /// @cond FOR_V1_COMPATIBILITY
        public Transform Actor { get { return actor; } set { actor = value; } }
        public Transform Conversant { get { return conversant; } set { conversant = value; } }
        public ConversationController ConversationController { get { return conversationController; } set { conversationController = value; } }
        public ConversationModel ConversationModel { get { return conversationModel; } }
        public ConversationView ConversationView { get { return conversationView; } }
        public bool IsConversationActive { get { return isConversationActive; } }
        /// @endcond

    }

}
