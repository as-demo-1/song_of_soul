// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This attribute marks a bit mask enum so it will use a custom property drawer to allow the
    /// designer to select a mask, similar to how the built-in LayerMask works.
    /// </summary>
    public class BitMaskAttribute : PropertyAttribute
    {

        /// <summary>
        /// The type of the property.
        /// </summary>
        public System.Type propType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.BitMaskAttribute"/> class.
        /// </summary>
        /// <param name='propType'>
        /// Property type.
        /// </param>
        public BitMaskAttribute(System.Type propType)
        {
            this.propType = propType;
        }
    }

}
