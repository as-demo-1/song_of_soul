// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: SwitchCamera(cameraName)
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandSwitchCamera : SequencerCommand
    {

        public void Start()
        {
            Transform cameraTransform = GetSubject(0);
            Camera newCamera = (cameraTransform != null) ? cameraTransform.GetComponent<Camera>() : null;
            if (newCamera != null)
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SwitchCamera({1})", new System.Object[] { DialogueDebug.Prefix, newCamera.name }));
                sequencer.SwitchCamera(newCamera);
            }
            else
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SwitchCamera({1}): Camera not found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(0) }));
            }
            Stop();
        }

    }

}
