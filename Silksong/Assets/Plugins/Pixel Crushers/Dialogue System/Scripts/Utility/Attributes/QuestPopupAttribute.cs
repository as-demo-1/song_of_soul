// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add [QuestPopup] to a string to show a popup of quest names in the inspector.
    /// </summary>
    public class QuestPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public QuestPopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
