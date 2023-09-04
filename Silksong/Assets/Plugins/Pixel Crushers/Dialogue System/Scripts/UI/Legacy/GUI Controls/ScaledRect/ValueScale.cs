using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Specifies the scale of a ScaledValue, which is used by ScaledRect.
    /// 
    /// - Pixel: The value is in screen pixels.
    /// - Normalized: The value is normalized [0..1] to the size of the window/screen.
    /// </summary>
    public enum ValueScale
    {

        /// <summary>
        /// Value is in screen pixels
        /// </summary>
        Pixel,

        /// <summary>
        /// Value is normalized [0..1] to the size of the window/screen
        /// </summary>
        Normalized
    }

}
