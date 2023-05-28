// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Used by DialogueEntry to define the priority of a dialogue entry. Entries are Normal by 
    /// default. When the dialogue system chooses lines and responses, it evaluates them in order 
    /// of priority from High to Low.
    /// </summary>
    public enum ConditionPriority
    {

        /// <summary>
        /// Low priority - evaluated last
        /// </summary>
        Low,

        /// <summary>
        /// Below normal priority
        /// </summary>
        BelowNormal,

        /// <summary>
        /// Normal priority (the default)
        /// </summary>
        Normal,

        /// <summary>
        /// Above normal priority
        /// </summary>
        AboveNormal,

        /// <summary>
        /// High priority - evaluated first
        /// </summary>
        High
    }

}
