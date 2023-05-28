// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Response button alignment specifies how to add response buttons to the dialogue UI. A 
    /// dialogue UI may predefine any number of response button slots. For example, a "dialogue
    /// wheel" layout could define six response button slots going clockwise around the wheel. The 
    /// response button alignment specifies whether to add responses starting from the first slot
    /// (clockwise) or the last slot (counter-clockwise).
    /// 
    /// The meanings of the enum values are:
    /// 
    /// - ToFirst: Align responses to the first slot.
    /// - ToLast: Align responses to the last slot.
    /// 
    /// If there are more responses than predefined slots, it's up to the IDialogueUI 
    /// implementation to decide how to handle this case.
    /// 
    /// If the dialogue author has specified a position (via the "[position #]" tag in the 
    /// response's text), then the response will always go in that position, regardless of the 
    /// alignment setting.
    /// </summary>
    public enum ResponseButtonAlignment
    {

        /// <summary>
        /// Align responses to the first slot
        /// </summary>
        ToFirst,

        /// <summary>
        /// Align responses to the last slot
        /// </summary>
        ToLast
    }

}
