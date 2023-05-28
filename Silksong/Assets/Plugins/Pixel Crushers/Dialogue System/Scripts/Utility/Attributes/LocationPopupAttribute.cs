// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{


    /// <summary>
    /// Add [LocationPopup] to a string to show a popup of location names in the inspector.
    /// </summary>
    public class LocationPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public LocationPopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
