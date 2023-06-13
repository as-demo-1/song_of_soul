#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

    /// <summary>
    /// This wrapper class keeps references intact if you switch between the 
    /// compiled assembly and source code versions of the original class.
    /// </summary>
    [HelpURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/timeline_trigger.html")]
    [AddComponentMenu("Pixel Crushers/Dialogue System/Trigger/Timeline Trigger")]
    public class TimelineTrigger : PixelCrushers.DialogueSystem.TimelineTrigger
    {
    }

}
#endif
#endif
