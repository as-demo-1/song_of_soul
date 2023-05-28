// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add [ConversationPopup] to a string show a popup of conversation titles in the inspector.
    /// </summary>
    public class ConversationPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public ConversationPopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
