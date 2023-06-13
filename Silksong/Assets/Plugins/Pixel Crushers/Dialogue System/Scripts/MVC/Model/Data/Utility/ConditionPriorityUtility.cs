// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static tool class for working with ConditionPriority.
    /// </summary>
    public static class ConditionPriorityUtility
    {

        /// <summary>
        /// Converts a Chat Mapper condition priority string to a ConditionPriority value.
        /// </summary>
        /// <returns>
        /// The condition priority associated with the string, defaulting to Normal if the string 
        /// isn't recognized.
        /// </returns>
        /// <param name='s'>
        /// The Chat Mapper condition priority string, which should be one of: Low, BelowNormal, 
        /// Normal, AboveNormal, or High.
        /// </param>
        public static ConditionPriority StringToConditionPriority(string s)
        {
            if (string.Equals(s, "High")) return ConditionPriority.High;
            if (string.Equals(s, "AboveNormal")) return ConditionPriority.AboveNormal;
            if (string.Equals(s, "BelowNormal")) return ConditionPriority.BelowNormal;
            if (string.Equals(s, "Low")) return ConditionPriority.Low;
            return ConditionPriority.Normal;
        }

    }

}
