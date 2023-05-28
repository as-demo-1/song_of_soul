// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class UIAnimationTransitions
    {
        [Tooltip("To show the panel, play this state/trigger.")]
        public string showTrigger = "Show";

        [Tooltip("To hide the panel, play this state/trigger.")]
        public string hideTrigger = "Hide";

        [Tooltip("Specifies whether Show Trigger and Hide Trigger are animator states or trigger parameters.")]
        public UIShowHideController.TransitionMode transitionMode = UIShowHideController.TransitionMode.State;

        public bool debug = false;

        public void ClearTriggers(UIShowHideController showHideController)
        {
            if (showHideController != null && transitionMode == UIShowHideController.TransitionMode.Trigger)
            {
                showHideController.ClearTrigger(showTrigger);
                showHideController.ClearTrigger(hideTrigger);
            }
        }
    }

}
