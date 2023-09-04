// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// StandardDialogueUI wrapper for AbstractUIRoot.
    /// Not needed for StandardDialogueUI, so does nothing.
    /// </summary>
    [System.Serializable]
    public class StandardUIRoot : AbstractUIRoot
    {
        public override void Show() { }
        public override void Hide() { }
    }

}
