// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static utility class to work with toggle values.
    /// </summary>
    public static class ToggleUtility
    {

        /// <summary>
        /// Returns the new value of a bool after applying a toggle.
        /// </summary>
        /// <returns>
        /// The new value.
        /// </returns>
        /// <param name='oldValue'>
        /// The original value to toggle.
        /// </param>
        /// <param name='state'>
        /// The toggle to apply.
        /// </param>
        public static bool GetNewValue(bool oldValue, Toggle state)
        {
            switch (state)
            {
                case Toggle.True:
                    return true;
                case Toggle.False:
                    return false;
                case Toggle.Flip:
                default:
                    return !oldValue;
            }
        }

    }

}
