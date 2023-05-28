// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Unity UI UIRoot wrapper for AbstractUIRoot.
    /// </summary>
    [System.Serializable]
    public class UnityUIRoot : AbstractUIRoot
    {

        /// <summary>
        /// Shows the root. In Unity UI, does nothing.
        /// </summary>
        public override void Show()
        {
        }

        /// <summary>
        /// Hides the root. In Unity UI, does nothing.
        /// </summary>
        public override void Hide()
        {
        }

    }

}
