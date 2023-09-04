// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

#if USE_CINEMACHINE

    /// <summary>
    /// This wrapper class keeps references intact if you switch between the 
    /// compiled assembly and source code versions of the original class.
    /// </summary>
    [HelpURL("https://www.pixelcrushers.com/dialogue_system/manual2x/html/cinemachine_priority_on_dialogue_event.html")]
    [AddComponentMenu("Pixel Crushers/Dialogue System/Trigger/Cinemachine Camera Priority On Dialogue Event")]
    public class CinemachineCameraPriorityOnDialogueEvent : PixelCrushers.DialogueSystem.CinemachineCameraPriorityOnDialogueEvent
    {
    }

#else

    [AddComponentMenu("")]
    public class CinemachineCameraPriorityOnDialogueEvent : PixelCrushers.DialogueSystem.CinemachineCameraPriorityOnDialogueEvent
    {
        private void Reset()
        {
            Debug.LogWarning("Support for " + GetType().Name + " must be enabled using Tools > Pixel Crushers > Dialogue System > Welcome Window.", this);
        }
    }

#endif

}
