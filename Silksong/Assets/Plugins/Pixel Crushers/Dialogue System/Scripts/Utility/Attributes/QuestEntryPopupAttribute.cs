// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add [QuestEntryPopup] to a string to show a popup of quest names in the inspector.
    /// Depends on the serialized object having a string field named 'quest' or 'questName'.
    /// </summary>
    public class QuestEntryPopupAttribute : PropertyAttribute
    {
        public QuestEntryPopupAttribute()
        {
        }
    }
}
