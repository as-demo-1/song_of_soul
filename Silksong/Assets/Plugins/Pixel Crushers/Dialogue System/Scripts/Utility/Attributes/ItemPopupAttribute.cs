// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{


    /// <summary>
    /// Add [ItemPopup] to a string to show a popup of item names in the inspector.
    /// </summary>
    public class ItemPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public ItemPopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
