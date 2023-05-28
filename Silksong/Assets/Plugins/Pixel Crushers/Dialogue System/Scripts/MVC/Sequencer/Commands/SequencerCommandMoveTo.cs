// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "MoveTo(target, [, subject[, duration]])", which matches the 
    /// subject to the target's position and rotation. If the subject has a rigidbody, uses
    /// Rigidbody.MovePosition/Rotation; otherwise sets the transform directly.
    /// 
    /// Arguments:
    /// -# The target. 
    /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
    /// Default: speaker.
    /// -# (Optional) Duration in seconds.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandMoveTo : SequencerCommand
    {

        private const float SmoothMoveCutoff = 0.05f;

        private Transform target;
        private Transform subject;
        private Rigidbody subjectRigidbody;
        private float duration;
        float startTime;
        float endTime;
        Vector3 originalPosition;
        Quaternion originalRotation;

        public void Start()
        {
            // Get the values of the parameters:
            target = GetSubject(0);
            subject = GetSubject(1);
            duration = GetParameterAsFloat(2, 0);
            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: MoveTo({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, target, subject, duration }));
            if ((target == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() target '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(0) }));
            if ((subject == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(1) }));

            // Set up the move:
            if ((subject != null) && (target != null) && (subject != target))
            {
                subjectRigidbody = subject.GetComponent<Rigidbody>();

                // If duration is above the cutoff, smoothly move toward target:
                if (duration > SmoothMoveCutoff)
                {
                    startTime = DialogueTime.time;
                    endTime = startTime + duration;
                    originalPosition = subject.position;
                    originalRotation = subject.rotation;
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

        private void SetPosition(Vector3 newPosition, Quaternion newRotation)
        {
            // For efficiency, doesn't warp NavMeshAgent.
            if (subjectRigidbody != null && !subjectRigidbody.isKinematic)
            {
                subjectRigidbody.MoveRotation(newRotation);
                subjectRigidbody.MovePosition(newPosition);
            }
            else
            {
                subject.rotation = newRotation;
                subject.position = newPosition;
            }
        }

        public void Update()
        {
            // Keep smoothing for the specified duration:
            if (DialogueTime.time < endTime)
            {
                float elapsed = (DialogueTime.time - startTime) / duration;
                SetPosition(Vector3.Lerp(originalPosition, target.position, elapsed), Quaternion.Lerp(originalRotation, target.rotation, elapsed));
            }
            else
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            // Final position:
            if ((subject != null) && (target != null) && (subject != target))
            {
                SetPosition(target.position, target.rotation);
            }

        }

    }

}
