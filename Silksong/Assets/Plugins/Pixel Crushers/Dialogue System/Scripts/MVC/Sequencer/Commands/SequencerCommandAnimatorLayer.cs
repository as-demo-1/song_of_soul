// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "AnimatorLayer(layerIndex[, weight[, subject[, duration]])", 
    /// which sets the layer weight on a subject's Animator.
    /// 
    /// Arguments:
    /// -# Index number of a layer on the subject's animator controller. Default: 1.
    /// -# (Optional) New weight. Default: <c>1f</c>.
    /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
    /// -# (Optional) Duration in seconds to smooth to the new weight.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandAnimatorLayer : SequencerCommand
    {

        private const float SmoothMoveCutoff = 0.05f;

        private int layerIndex = 1;
        private float weight = 0;
        private Transform subject = null;
        private float duration = 0;
        private Animator animator = null;
        float startTime = 0;
        float endTime = 0;
        float originalWeight = 0;

        public void Start()
        {
            // Get the values of the parameters:
            layerIndex = GetParameterAsInt(0, 1);
            weight = GetParameterAsFloat(1, 1);
            subject = GetSubject(2);
            duration = GetParameterAsFloat(3, 0);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorLayer({1}, {2}, {3}, {4})", new System.Object[] { DialogueDebug.Prefix, layerIndex, weight, subject, duration }));

            // Check the parameters:
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer(): subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(2) }));
                Stop();
            }
            else
            {
                animator = subject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer(): no Animator found on '{1}'.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                    Stop();
                }
                else if ((layerIndex < 1) || (layerIndex >= animator.layerCount))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer(): layer index {1} is invalid.", new System.Object[] { DialogueDebug.Prefix, layerIndex }));
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
                    originalWeight = animator.GetLayerWeight(layerIndex);
                }
            }
        }


        public void Update()
        {
            // Lerp for the specified duration:
            if (DialogueTime.time < endTime)
            {
                float elapsed = (DialogueTime.time - startTime) / duration;
                float current = Mathf.Lerp(originalWeight, weight, elapsed / duration);
                if (animator != null) animator.SetLayerWeight(layerIndex, current);
            }
            else
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            // Final weight:
            if ((animator != null) && (0 < layerIndex) && (layerIndex < animator.layerCount))
            {
                animator.SetLayerWeight(layerIndex, weight);
            }
        }

    }

}
