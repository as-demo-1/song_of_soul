// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: Zoom2D([gameobject|speaker|listener[, size[, duration]]])
    /// 
    /// Arguments:
    /// -# subject:(Optional) The subject; can be speaker, listener, or the name of a game object. Default:
    /// speaker.
    /// -# size: (Optional) The orthographic camera size to zoom to.
    /// -# duration: (Optional) Duration over which to move the camera. Default: immediate.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandZoom2D : SequencerCommand
    {

        private const float SmoothMoveCutoff = 0.05f;

        private bool original;
        private Transform subject;
        private Vector3 targetPosition;
        private Vector3 originalPosition;
        private float targetSize;
        private float originalSize;
        private float duration;
        private float startTime;
        private float endTime;

        public void Start()
        {
            // Get the values of the parameters:
            original = string.Equals(GetParameter(0), "original");
            subject = original ? null : GetSubject(0, speaker);
            targetSize = GetParameterAsFloat(1, 16);
            duration = GetParameterAsFloat(2, 0);

            // Log to the console:
            if (DialogueDebug.logInfo)
            {
                if (original)
                {
                    Debug.Log(string.Format("{0}: Sequencer: Zoom2D(original, -, {1}s)", new System.Object[] { DialogueDebug.Prefix, duration }));
                }
                else
                {
                    Debug.Log(string.Format("{0}: Sequencer: Zoom2D({1}, {2}, {3}s)", new System.Object[] { DialogueDebug.Prefix, Tools.GetGameObjectName(subject), targetSize, duration }));
                }
            }
            if ((subject == null && !original) && DialogueDebug.logWarnings)
            {
                Debug.LogWarning(string.Format("{0}: Sequencer: Camera subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(0) }));
            }

            // Start moving the camera:
            sequencer.TakeCameraControl();
            if (subject != null || original)
            {
                if (original)
                {
                    targetPosition = sequencer.originalCameraPosition;
                    targetSize = sequencer.originalOrthographicSize;
                }
                else
                {
                    targetPosition = new Vector3(subject.position.x, subject.position.y, sequencer.sequencerCamera.transform.position.z);
                }
                originalPosition = sequencer.sequencerCamera.transform.position;
                originalSize = sequencer.sequencerCamera.orthographicSize;

                // If duration is above the cutoff, smoothly move camera toward camera angle:
                if (duration > SmoothMoveCutoff)
                {
                    startTime = DialogueTime.time;
                    endTime = startTime + duration;
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                Stop();
            }
        }

        public void Update()
        {
            // Keep smoothing for the specified duration:
            if (DialogueTime.time < endTime)
            {
                float elapsed = (DialogueTime.time - startTime) / duration;
                if (sequencer != null && sequencer.sequencerCamera != null)
                {
                    sequencer.sequencerCamera.transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsed);
                    sequencer.sequencerCamera.orthographicSize = Mathf.Lerp(originalSize, targetSize, elapsed);
                }
            }
            else
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            // Final position:
            if (subject != null || original)
            {
                if (sequencer != null && sequencer.sequencerCamera != null)
                {
                    sequencer.sequencerCamera.transform.position = targetPosition;
                    sequencer.sequencerCamera.orthographicSize = targetSize;
                }
            }
        }

    }

}
