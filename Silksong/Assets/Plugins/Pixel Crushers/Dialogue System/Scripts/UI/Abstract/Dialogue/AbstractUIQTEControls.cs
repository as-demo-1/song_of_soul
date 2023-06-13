// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract QTE controls. Each GUI system implementation derives its own subclass
    /// from this.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractUIQTEControls : AbstractUIControls
    {

        /// <summary>
        /// Gets a value indicating whether any QTE indicators are visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public abstract bool areVisible { get; }

        /// <summary>
        /// Shows the QTE indicator at the specified index.
        /// </summary>
        /// <param name='index'>
        /// Zero-based index of the QTE indicator.
        /// </param>
        public abstract void ShowIndicator(int index);

        /// <summary>
        /// Hides the QTE indicator at the specified index.
        /// </summary>
        /// <param name='index'>
        /// Zero-based index of the QTE indicator.
        /// </param>
        public abstract void HideIndicator(int index);

    }

}
