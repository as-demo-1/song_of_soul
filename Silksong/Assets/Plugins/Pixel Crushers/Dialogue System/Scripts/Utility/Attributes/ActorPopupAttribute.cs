// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add [ActorPopup] to a string to use a popup of actor names in the inspector.
    /// </summary>
    public class ActorPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public ActorPopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
