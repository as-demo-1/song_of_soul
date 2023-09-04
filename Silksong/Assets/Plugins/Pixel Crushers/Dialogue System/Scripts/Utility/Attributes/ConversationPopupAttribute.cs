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
        public bool showFilter = false;

        public ConversationPopupAttribute(bool showReferenceDatabase = false, bool showFilter = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
            this.showFilter = showFilter;
        }
    }
}
