namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// The TextStyle enum is used by several dialogue system components that draw text using Unity
    /// GUI. The meanings of the enum values are:
    /// 
    /// - None: Draw the text plain, without any special effects.
    /// - Shadow: Draw a black drop-shadow to the lower right of the text.
    /// - Outline: Draw a black outline around the text.
    /// </summary>
    public enum TextStyle
    {

        /// <summary>
        /// Draw the text plain, without any special effects
        /// </summary>
        None,

        /// <summary>
        /// Draw a black drop-shadow to the lower right of the text
        /// </summary>
        Shadow,

        /// <summary>
        /// Draw a black outline around the text
        /// </summary>
        Outline
    };

}
