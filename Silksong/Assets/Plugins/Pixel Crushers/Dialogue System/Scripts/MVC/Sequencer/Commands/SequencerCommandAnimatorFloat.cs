// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "AnimatorFloat(animatorParameter[, value[, gameobject|speaker|listener[, duration]]])",
    /// which smoothly changes a float parameter on a subject's Animator.
    /// 
    /// Arguments:
    /// -# Name of a Mecanim animator parameter.
    /// -# (Optional) Float value. Default: <c>1f</c>.
    /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
    /// -# (Optional) Duration in seconds to smooth to the value.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandAnimatorFloat : SequencerCommand
    {

        private const float SmoothMoveCutoff = 0.05f;

        private int animatorParameterHash = -1;
        private float targetValue;
        private Transform subject = null;
        private float duration = 0;
        private Animator animator = null;
        float startTime = 0;
        float endTime = 0;
        float originalValue = 0;

        public void Start()
        {
            // Get the values of the parameters:
            string animatorParameter = GetParameter(0);
            animatorParameterHash = Animator.StringToHash(animatorParameter);
            targetValue = GetParameterAsFloat(1, 1);
            subject = GetSubject(2);
            duration = GetParameterAsFloat(3, 0);
            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: AnimatorFloat({1}, {2}, {3}, {4})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, targetValue, subject, duration }));

            // Check the parameters:
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorFloat(): subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(2) }));
                Stop();
            }
            else
            {
                animator = subject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorFloat(): no Animator found on '{1}'.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                    Stop();
                }
                else if (duration < SmoothMoveCutoff)
                {
                    Stop();
                }
                else
                {

                    // Set up the lerp:
                    startTime = DialogueTime.time;
                    endTime = startTime + duration;
                    originalValue = animator.GetFloat(animatorParameterHash);
                }
            }
        }

        public void Update()
        {
            // Lerp for the specified duration:
            if (DialogueTime.time < endTime)
            {
                float elapsed = (DialogueTime.time - startTime) / duration;
                float current = Mathf.Lerp(originalValue, targetValue, elapsed / duration);
                if (animator != null) animator.SetFloat(animatorParameterHash, current);
            }
            else
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            // Final value:
            if (animator != null) animator.SetFloat(animatorParameterHash, targetValue);
        }

    }

}
