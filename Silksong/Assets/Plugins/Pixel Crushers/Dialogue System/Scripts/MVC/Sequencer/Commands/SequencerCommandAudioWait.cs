// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: AudioWait(audioClip[, subject[, audioClips...]])
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandAudioWait : SequencerCommand
    {

        protected float stopTime = 0;
        protected AudioSource audioSource = null;
        protected int nextClipIndex = 2;
        protected string audioClipName;
        protected AudioClip currentClip = null;
        protected AudioClip originalClip = null;
        protected bool restoreOriginalClip = false; // Don't restore; could stop next entry's AudioWait that runs same frame.

        public IEnumerator Start()
        {
            audioClipName = GetParameter(0);
            Transform subject = GetSubject(1);
            nextClipIndex = 2;
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWait({1})", new System.Object[] { DialogueDebug.Prefix, GetParameters() }));
            audioSource = GetAudioSource(subject);
            if (audioSource == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: can't find or add AudioSource to {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                Stop();
            }
            else
            {
                originalClip = audioSource.clip;
                stopTime = DialogueTime.time + 1; // Give time for yield return null.
                yield return null;
                originalClip = audioSource.clip;
                TryAudioClip(audioClipName);
            }
        }

        protected virtual AudioSource GetAudioSource(Transform subject)
        {
            return SequencerTools.GetAudioSource(subject);
        }

        private void TryAudioClip(string audioClipName)
        {
            try
            {
                if (string.IsNullOrEmpty(audioClipName))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Audio clip name is blank.", new System.Object[] { DialogueDebug.Prefix }));
                    stopTime = 0;
                    if (nextClipIndex >= parameters.Length)
                    {
                        Stop();
                    }
                }
                else
                {
                    this.audioClipName = audioClipName;
                    DialogueManager.LoadAsset(audioClipName, typeof(AudioClip),
                        (asset) =>
                        {
                            var audioClip = asset as AudioClip;
                            if (audioClip == null)
                            {
                                if (DialogueDebug.logWarnings && Sequencer.reportMissingAudioFiles) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Clip '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, audioClipName }));
                                stopTime = 0;
                                if (nextClipIndex >= parameters.Length)
                                {
                                    Stop();
                                }
                            }
                            else
                            {
                                if (IsAudioMuted())
                                {
                                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWait(): waiting but not playing '{1}'; audio is muted.", new System.Object[] { DialogueDebug.Prefix, audioClipName }));
                                }
                                else if (audioSource != null) // Check in case AudioSource was destroyed while loading Addressable.
                                {
                                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWait(): playing '{1}'.", new System.Object[] { DialogueDebug.Prefix, audioClipName }));
                                    currentClip = audioClip;
                                    audioSource.clip = audioClip;
                                    audioSource.Play();
                                }
                                stopTime = DialogueTime.time + audioClip.length;
                            }
                        });
                }

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
                if (currentClip != null)
                {
                    DialogueManager.UnloadAsset(currentClip);
                }
                currentClip = null;
                if (nextClipIndex < parameters.Length)
                {
                    TryAudioClip(GetParameter(nextClipIndex));
                    nextClipIndex++;
                }
                else
                {
                    Stop();
                }
            }
        }

        public void OnDialogueSystemPause()
        {
            if (audioSource == null) return;
            audioSource.Pause();
        }

        public void OnDialogueSystemUnpause()
        {
            if (audioSource == null) return;
            audioSource.Play();
        }

        public void OnDestroy()
        {
            if (audioSource != null)
            {
                if (audioSource.isPlaying && 
                    (audioSource.clip == currentClip) &&
                    (audioSource.clip != null))
                {
                    audioSource.Stop();
                }
                if (restoreOriginalClip) audioSource.clip = originalClip;
                DialogueManager.UnloadAsset(currentClip);
            }
        }

    }

}
