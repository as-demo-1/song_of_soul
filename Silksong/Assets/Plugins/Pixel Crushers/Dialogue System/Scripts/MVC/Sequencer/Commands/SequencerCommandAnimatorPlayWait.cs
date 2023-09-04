// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "AnimatorPlayWait(animatorParameter, [gameobject|speaker|listener], [crossfadeDuration], [layer])",
    /// which plays a state on a subject's Animator and waits until it's done.
    /// 
    /// Arguments:
    /// -# Name of a Mecanim animator state.
    /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
    /// -# (Optional) Crossfade duration. Default: 0 (play immediately).
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandAnimatorPlayWait : SequencerCommand
    {

        private const float maxDurationToWaitForStateStart = 1;

        public void Start()
        {

            // Get the values of the parameters:
            string stateName = GetParameter(0);
            Transform subject = GetSubject(1);
            float crossfadeDuration = GetParameterAsFloat(2);
            int layer = GetParameterAsInt(3, -1);

            // Start the state:
            Animator animator = (subject != null) ? subject.GetComponentInChildren<Animator>() : null;
            if (animator == null)
            {
                if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Sequencer: AnimatorPlayWait({1}, {2}, fade={3}, layer={4}): No Animator found on {2}", new object[] { DialogueDebug.Prefix, stateName, (subject != null) ? subject.name : GetParameter(1), crossfadeDuration, layer }));
                Stop();
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorPlayWait({1}, {2}, {3})", new object[] { DialogueDebug.Prefix, stateName, subject, crossfadeDuration }));
                if (!animator.gameObject.activeSelf) animator.gameObject.SetActive(true);
                if (Tools.ApproximatelyZero(crossfadeDuration))
                {
                    animator.Play(stateName, layer, 0);
                }
                else
                {
                    animator.CrossFadeInFixedTime(stateName, crossfadeDuration, layer);
                }
                StartCoroutine(MonitorState(animator, stateName));
            }
        }

        /// <summary>
        /// Monitors the animator state. Stops the sequence when the state is done,
        /// or after maXDurationToWaitForStateStart (1 second) if it never enters
        /// that state.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="stateName">State name.</param>
        private IEnumerator MonitorState(Animator animator, string stateName)
        {

            // Wait to enter the state:
            float maxStartTime = DialogueTime.time + maxDurationToWaitForStateStart;
            AnimatorStateInfo animatorStateInfo;
            bool isInState = CheckIsInState(animator, stateName, out animatorStateInfo);
            while (!isInState && (DialogueTime.time < maxStartTime))
            {
                yield return null;
                isInState = CheckIsInState(animator, stateName, out animatorStateInfo);
            }

            // Wait for state to end, then stop:
            if (isInState)
            {
                yield return StartCoroutine(DialogueTime.WaitForSeconds(animatorStateInfo.length)); //new WaitForSeconds(animatorStateInfo.length);

            }
            Stop();
        }

        /// <summary>
        // Checks if the animator is in a specified state on any of its layers, and
        // returns the state info.
        /// </summary>
        /// <returns><c>true</c>, if in the state, <c>false</c> otherwise.</returns>
        /// <param name="animator">Animator.</param>
        /// <param name="stateName">State name.</param>
        /// <param name="animatorStateInfo">(Out) Animator state info.</param>
        private bool CheckIsInState(Animator animator, string stateName, out AnimatorStateInfo animatorStateInfo)
        {
            if (animator != null)
            {
                for (int layerIndex = 0; layerIndex < animator.layerCount; layerIndex++)
                {
                    AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIndex);
                    if (info.IsName(stateName))
                    {
                        animatorStateInfo = info;
                        return true;
                    }
                }
            }
            animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return false;
        }

    }

}
