using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A scaled value, which is used by ScaledRect. The value can be scaled in pixel measurements
    /// or normalized [0,,1] to the size of the window/screen.
    /// </summary>
    [System.Serializable]
    public class ScaledValue
    {

        /// <summary>
        /// Represents a length of zero.
        /// </summary>
        public static readonly ScaledValue zero = new ScaledValue(ValueScale.Pixel, 0);

        /// <summary>
        /// Represents a length equal to the size of the window. If the window is the whole screen,
        /// this value is <c>Screen.width</c> for horizontal values or <c>Screen.height</c> for 
        /// vertical values.
        /// </summary>
        public static readonly ScaledValue max = new ScaledValue(ValueScale.Normalized, 1);

        /// <summary>
        /// The scale used by the value (pixel-scale or normalized-scale).
        /// </summary>
        public ValueScale scale;

        /// <summary>
        /// The value in the scale.
        /// </summary>
        public float value;

        /// <summary>
        /// Initializes a new ScaledValue.
        /// </summary>
        /// <param name='scale'>
        /// Scale to use.
        /// </param>
        /// <param name='value'>
        /// Value to use.
        /// </param>
        public ScaledValue(ValueScale scale, float value)
        {
            this.scale = scale;
            this.value = value;
        }

        /// <summary>
        /// Copy constructor. Initializes a new ScaledValue.
        /// </summary>
        /// <param name='source'>
        /// Source to copy.
        /// </param>
        public ScaledValue(ScaledValue source)
        {
            if (source != null)
            {
                this.scale = source.scale;
                this.value = source.value;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ScaledValue()
        {
        }

        /// <summary>
        /// Gets the pixel value of this ScaledValue.
        /// </summary>
        /// <returns>
        /// The pixel value.
        /// </returns>
        /// <param name='windowSize'>
        /// For normalized values, windowSize is the pixel value represented by the normalized value <c>1</c>.
        /// </param>
        public float GetPixelValue(float windowSize)
        {
            if (scale == ValueScale.Pixel)
            {
                return value;
            }
            else
            {
                return value * windowSize;
            }
        }

        /// <summary>
        /// Static utility function to create a ScaledValue from a pixel value.
        /// </summary>
        /// <returns>
        /// The ScaledValue that represents the pixel value.
        /// </returns>
        /// <param name='value'>
        /// The pixel value.
        /// </param>
        public static ScaledValue FromPixelValue(float value)
        {
            return new ScaledValue(ValueScale.Pixel, value);
        }

        /// <summary>
        /// Static utility function to create a ScaledValue from a normalized value.
        /// </summary>
        /// <returns>
        /// The ScaledValue that represents the pixel value.
        /// </returns>
        /// <param name='value'>
        /// The normalized value.
        /// </param>
        public static ScaledValue FromNormalizedValue(float value)
        {
            return new ScaledValue(ValueScale.Normalized, value);
        }

    }

}
