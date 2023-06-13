using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Specifies a control's auto-size settings.
    /// </summary>
    [System.Serializable]
    public class AutoSize
    {

        /// <summary>
        /// If <c>true</c>, auto-size the width to fit the contents.
        /// </summary>
        public bool autoSizeWidth = false;

        /// <summary>
        /// If <c>true</c>, auto-size the height to fit the contents.
        /// </summary>
        public bool autoSizeHeight = false;

        /// <summary>
        /// The maximum width to auto-size to.
        /// </summary>
        public ScaledValue maxWidth = new ScaledValue(ScaledValue.max);

        /// <summary>
        /// The maximum height to auto-size to.
        /// </summary>
        public ScaledValue maxHeight = new ScaledValue(ScaledValue.max);

        /// <summary>
        /// The amount of padding, in pixels, to ensure around the control's content
        /// when auto-sizing.
        /// </summary>
        public RectOffset padding;

        /// <summary>
        /// Gets a value indicating whether auto-sizing is enabled.
        /// </summary>
        public bool IsEnabled { get { return autoSizeWidth || autoSizeHeight; } }
    }

}
