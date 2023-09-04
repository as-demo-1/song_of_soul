// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: AudioWWW(url,...)
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandAudioWWW : SequencerCommand
    {

#if UNITY_2018_1_OR_NEWER

        public void Start()
        {
            Debug.Log("Dialogue System: Sequencer: AudioWWW() is deprecated in Unity 2018+.");
            Stop();
        }

#else

        private enum State { Idle, Loading, Playing }
        private State state = State.Idle;
        AudioSource audioSource = null;
        private Queue<string> audioURLs = new Queue<string>();
        private WWW www = null;
        private float stopTime = 0;

        public void Start()
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWWW({1}).", new System.Object[] { DialogueDebug.Prefix, string.Join(", ", parameters) }));
            // Create audio queue:
            for (int i = 0; i < (parameters.Length - 1); i++)
            {
                string url = GetParameter(i);
                if (!string.IsNullOrEmpty(url))
                {
                    audioURLs.Enqueue(url);
                }
            }

            // Get subject or last clip:
            Transform subject = null;
            string lastParam = (parameters.Length > 0) ? GetParameter(parameters.Length - 1) : string.Empty;
            if (!string.IsNullOrEmpty(lastParam))
            {
                subject = GetSubject(parameters.Length - 1, null);
                if (subject == null)
                {
                    subject = sequencer.speaker;
                    audioURLs.Enqueue(lastParam);
                }
            }

            if (audioURLs.Count == 0)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWWW(): No URLs specified.", new System.Object[] { DialogueDebug.Prefix }));
                Stop();
            }
            else
            {

                // Setup audio source:
                AudioSource audioSource = SequencerTools.GetAudioSource(subject);
                if (audioSource == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWWW(): can't find or add AudioSource to {1}.", new System.Object[] { DialogueDebug.Prefix, Tools.GetGameObjectName(subject) }));
                    Stop();
                }
                else
                {
                    if (Tools.ApproximatelyZero(audioSource.volume)) audioSource.volume = 1f;
                    if (IsAudioMuted())
                    {
                        if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWWW({1}): audio is muted; waiting but not playing.", new System.Object[] { DialogueDebug.Prefix, string.Join(", ", parameters) }));
                    }
                }
            }
        }

        public void Update()
        {
            switch (state)
            {
                case State.Idle:
                    LoadNextAudio();
                    break;
                case State.Loading:
                    CheckLoadProgress();
                    break;
                case State.Playing:
                    CheckPlayProgress();
                    break;
            }
        }

        private void LoadNextAudio()
        {
            if (audioURLs.Count > 0)
            {
                string url = audioURLs.Dequeue();
                if (!string.IsNullOrEmpty(url))
                {
                    if (url.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: AudioWWW(): Retrieving {1}", new System.Object[] { DialogueDebug.Prefix, url }));
                        www = new WWW(url);
                        if (www != null)
                        {
                            state = State.Loading;
                        }
                        else
                        {
                            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: AudioWWW(): Failed to retrieve {1}", new System.Object[] { DialogueDebug.Prefix, url }));
                        }
                    }
                    else
                    {
                        if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: AudioWWW(): Sorry, the player only supports .ogg files. Can't load {1}", new System.Object[] { DialogueDebug.Prefix, url }));
                    }
                }
            }
            else
            {
                // If no more to load, stop:
                Stop();
            }
        }

        private void CheckLoadProgress()
        {
            if (www.isDone)
            {
                AudioClip audioClip = www.GetAudioClip(false);
                if (audioClip != null)
                {
                    if (!IsAudioMuted())
                    {
                        audioSource.PlayOneShot(audioClip);
                    }
                    stopTime = DialogueTime.time + audioClip.length;
                    state = State.Playing;
                }
                else
                {
                    state = State.Idle;
                }
            }
        }

        private void CheckPlayProgress()
        {
            if (DialogueTime.time >= stopTime)
            {
                state = State.Idle;
            }
        }

        private void OnDestroy()
        {
            if ((audioSource != null) && audioSource.isPlaying) audioSource.Stop();
        }

#endif

    }

}
