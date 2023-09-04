// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class UIAutonumberSettings
    {
        [Tooltip("Enable autonumbering of responses.")]
        public bool enabled = false;

        [Tooltip("Bind regular number keys as hotkeys.")]
        public bool regularNumberHotkeys = true;

        [Tooltip("Bind numpad keys as hotkeys.")]
        public bool numpadHotkeys = false;

        [Tooltip("Format for response button text, where {0} is hotkey number and {1} is menu text.")]
        public string format = "{0}. {1}";
    }

}
