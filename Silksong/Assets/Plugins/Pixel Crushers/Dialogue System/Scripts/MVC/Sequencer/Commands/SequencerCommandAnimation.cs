// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: Animation(animation[, gameobject|speaker|listener[, animations...]])
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandAnimation : SequencerCommand
    {

        Transform subject = null;
        private int nextAnimationIndex = 2;
        private float stopTime = 0;
        private Animation anim = null;

        public void Start()
        {
            string firstClipName = GetParameter(0);
            subject = GetSubject(1);
            nextAnimationIndex = 2;
            anim = (subject == null) ? null : subject.GetComponent<Animation>();
            if ((subject == null) || (anim == null))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation({1}, {2},...) command: No Animation component found on {2}.", new System.Object[] { DialogueDebug.Prefix, firstClipName, (subject != null) ? subject.name : GetParameter(1) }));
            }
            else if (string.IsNullOrEmpty(firstClipName))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation({1}, {2},...) command: Animation name is blank.", new System.Object[] { DialogueDebug.Prefix, firstClipName, subject.name }));
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: Animation({1}, {2},...)", new System.Object[] { DialogueDebug.Prefix, firstClipName, Tools.GetObjectName(subject) }));
                TryAnimationClip(firstClipName);
            }
        }

        private void TryAnimationClip(string clipName)
        {
            try
            {
                anim.CrossFade(clipName);
                stopTime = DialogueTime.time + Mathf.Max(0.1f, anim[clipName].length - 0.3f);
            }
            catch (System.Exception)
            {
                stopTime = 0;
            }
        }

        public void Update()
        {
            if (DialogueTime.time >= stopTime)
            {
                if (nextAnimationIndex < parameters.Length)
                {
                    TryAnimationClip(GetParameter(nextAnimationIndex));
                    nextAnimationIndex++;
                }
                if (nextAnimationIndex >= parameters.Length) Stop();
            }
        }

    }

}
