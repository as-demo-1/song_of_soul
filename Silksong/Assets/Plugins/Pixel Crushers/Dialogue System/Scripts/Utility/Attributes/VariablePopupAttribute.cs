// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add [VariablePopup] to a string to show a popup of database variable names
    /// in the inspector.
    /// </summary>
    public class VariablePopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public VariablePopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
