// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract base class for OverrideDialogueUI and OverrideDisplaySettings.
    /// </summary>
    public abstract class OverrideUIBase : MonoBehaviour
    {

        /// <summary>
        /// When both participants have overrides, the higher priority takes precedence.
        /// </summary>
        [Tooltip("When both participants have overrides, higher priority takes precedence.")]
        public int priority = 0;

    }

}
