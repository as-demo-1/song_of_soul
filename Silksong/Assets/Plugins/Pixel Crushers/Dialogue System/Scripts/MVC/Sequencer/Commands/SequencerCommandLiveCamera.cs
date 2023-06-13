// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: LiveCamera(angle[, gameobject|speaker|listener[, duration]])
    /// 
    /// This command differs from Camera() because it tracks moving subjects.
    /// This command was written and donated by Patricio Jeri.
    /// 
    /// Arguments:
    /// -# Name of a camera angle (child transform) defined in cameraAngles. If the angle isn't 
    /// defined in Sequencer.CameraAngles, looks for a game object in the scene. Default: Closeup.
    /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default:
    /// speaker.
    /// -# (Optional) Duration over which to move the camera. Default: immediate.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandLiveCamera : SequencerCommand
    {

        private const float SmoothMoveCutoff = 0.05f;

        private Transform subject;
        private Transform angleTransform;
        private Transform cameraTransform;
        private bool isLocalTransform;
        private Quaternion targetRotation;
        private Vector3 targetPosition;
        private float duration;
        private float startTime;
        private float endTime;
        private Quaternion originalRotation;
        private Vector3 originalPosition;
        //private float localTime = 0f;
        //private float targetLocalTime = 5f;
        private bool isOriginal;

        public void Start()
        {
            // Get the values of the parameters:
            string angle = GetParameter(0, "Closeup");
            subject = GetSubject(1);
            duration = GetParameterAsFloat(2, 0);

            // Get angle:
            bool isDefault = string.Equals(angle, "default");
            if (isDefault) angle = SequencerTools.GetDefaultCameraAngle(subject);
            isOriginal = string.Equals(angle, "original");
            angleTransform = isOriginal
                ? Camera.main.transform
                : ((sequencer.cameraAngles != null) ? sequencer.cameraAngles.transform.Find(angle) : null);
            isLocalTransform = true;
            if (angleTransform == null)
            {
                isLocalTransform = false;
                GameObject go = GameObject.Find(angle);
                if (go != null) angleTransform = go.transform;
            }

            // Log:
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: Camera({1}, {2}, {3}s)", new System.Object[] { DialogueDebug.Prefix, angle, Tools.GetGameObjectName(subject), duration }));
            if ((angleTransform == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera angle '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, angle }));
            if ((subject == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(1) }));

            // If we have a camera angle and subject, move the camera to it:
            sequencer.TakeCameraControl();
            if (isOriginal || (angleTransform != null && subject != null))
            {
                cameraTransform = sequencer.sequencerCameraTransform;
                if (isOriginal)
                {
                    targetRotation = sequencer.originalCameraRotation;
                    targetPosition = sequencer.originalCameraPosition;
                }
                else if (isLocalTransform)
                {
                    targetRotation = subject.rotation * angleTransform.localRotation;
                    targetPosition = subject.position + subject.rotation * angleTransform.localPosition;
                }
                else
                {
                    targetRotation = angleTransform.rotation;
                    targetPosition = angleTransform.position;
                }

                // If duration is above the cutoff, smoothly move camera toward camera angle:
                if (duration > SmoothMoveCutoff)
                {
                    startTime = DialogueTime.time;
                    endTime = startTime + duration;
                    originalRotation = cameraTransform.rotation;
                    originalPosition = cameraTransform.position;

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

                if (isOriginal || (angleTransform != null && subject != null))
                {
                    cameraTransform = sequencer.sequencerCameraTransform;
                    if (isOriginal)
                    {
                        targetRotation = sequencer.originalCameraRotation;
                        targetPosition = sequencer.originalCameraPosition;
                    }
                    else if (isLocalTransform)
                    {
                        targetRotation = subject.rotation * angleTransform.localRotation;
                        targetPosition = subject.position + subject.rotation * angleTransform.localPosition;
                    }
                    else
                    {
                        targetRotation = angleTransform.rotation;
                        targetPosition = angleTransform.position;
                    }
                }

                float elapsed = (DialogueTime.time - startTime) / duration;
                cameraTransform.rotation = Quaternion.Lerp(originalRotation, targetRotation, elapsed);
                cameraTransform.position = Vector3.Lerp(originalPosition, targetPosition, elapsed);
            }
            else
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            // Final position:
            if (angleTransform != null && subject != null && cameraTransform != null)
            {
                cameraTransform.rotation = targetRotation;
                cameraTransform.position = targetPosition;
            }
        }

    }

}
