#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Sequencer command Timeline(mode, timeline, [nowait], [nostop], [#:binding]...)
    /// or Timeline(speed, timeline, value)
    /// 
    /// Controls a Timeline (PlayableDirector).
    /// 
    /// - mode: Can be play, pause, resume, stop, or speed. If speed, the third parameter should be the new speed.
    /// - timeline: Name of a GameObject with a PlayableDirector, or a Timeline asset in a Resources folder or asset bundle. Default: speaker.
    /// - nowait: If specified, doesn't wait for the director to finish.
    /// - nostop: If specified, doesn't force the director to stop at the end of the sequencer command.
    /// - #:binding: If specified, the number indicates the track number, starting from zero. The binding is the name of a GameObject to bind to that track.
    /// </summary>
    public class SequencerCommandTimeline : SequencerCommand
    {

        private PlayableDirector playableDirector = null;
        private TimelineAsset timelineAsset = null;
        private string mode;
        private Transform subject;
        private bool nostop = false;
        private bool nowait = false;
        private bool mustDestroyPlayableDirector = false;
        private bool mustDestroyPlayableAsset = false;

        public IEnumerator Start()
        {
            if (parameters == null || parameters.Length == 0)
            {
                Stop();
                yield break;
            }
            mode = GetParameter(0).ToLower();
            subject = GetSubject(1, Sequencer.Speaker);
            nowait = string.Equals(GetParameter(2), "nowait", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetParameter(3), "nowait", System.StringComparison.OrdinalIgnoreCase);
            nostop = string.Equals(GetParameter(2), "nostop", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetParameter(3), "nostop", System.StringComparison.OrdinalIgnoreCase);
            playableDirector = (subject != null) ? subject.GetComponent<PlayableDirector>() : null;

            // If no suitable PlayableDirector was found, look for a Timeline asset in Resources:
            if (playableDirector == null || playableDirector.playableAsset == null)
            {
                DialogueManager.LoadAsset(GetParameter(1), typeof(TimelineAsset),
                    (asset) =>
                    {
                        timelineAsset = asset as TimelineAsset;
                        if (timelineAsset != null)
                        {
                            if (playableDirector == null)
                            {
                                playableDirector = new GameObject(GetParameter(1), typeof(PlayableDirector)).GetComponent<PlayableDirector>();
                                mustDestroyPlayableDirector = true;
                            }
                            playableDirector.playableAsset = timelineAsset;
                            mustDestroyPlayableAsset = true;
                        }
                        StartCoroutine(Proceed());
                    });
            }
            else
            {
                yield return Proceed();
            }
        }

        private IEnumerator Proceed()
        { 
            if (playableDirector == null)
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: Timeline(" + GetParameters() +
                    "): Can't find playable director '" + GetParameter(1) + "'.");
            }
            else if (playableDirector.playableAsset == null)
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: Timeline(" + GetParameters() +
                    "): No timeline asset is assigned to the Playable Director.", playableDirector);
            }
            else
            {
                var isModeValid = (mode == "play" || mode == "pause" || mode == "resume" || mode == "stop" || mode == "speed");
                if (!isModeValid)
                {
                    if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: Timeline(" + GetParameters() +
                        "): Invalid mode '" + mode + "'. Expected 'play', 'pause', 'resume', or 'stop'.");
                }
                else
                {
                    if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Sequencer: Timeline(" + GetParameters() + ")");

                    // Check for rebindings:
                    timelineAsset = playableDirector.playableAsset as TimelineAsset;
                    if (timelineAsset != null)
                    {
                        for (int i = 2; i < Parameters.Length; i++)
                        {
                            var s = GetParameter(i);
                            if (s.Contains(":"))
                            {
                                var colonPos = s.IndexOf(":");
                                var trackIndex = Tools.StringToInt(s.Substring(0, colonPos)) + 1;
                                var bindName = s.Substring(colonPos + 1);
                                var track = timelineAsset.GetOutputTrack(trackIndex);
                                if (track != null)
                                {
                                    playableDirector.SetGenericBinding(track, Tools.GameObjectHardFind(bindName));
                                }
                            }
                        }
                    }

                    switch (mode)
                    {
                        case "play":
                            playableDirector.Play();
                            var endTime = nowait ? 0 : DialogueTime.time + playableDirector.playableAsset.duration;
                            while (DialogueTime.time < endTime)
                            {
                                yield return null;
                            }
                            break;
                        case "pause":
                            playableDirector.Pause();
                            nostop = true;
                            break;
                        case "resume":
                            playableDirector.Resume();
                            var resumedEndTime = nowait ? 0 : DialogueTime.time + playableDirector.playableAsset.duration - playableDirector.time;
                            while (DialogueTime.time < resumedEndTime)
                            {
                                yield return null;
                            }
                            break;
                        case "stop":
                            playableDirector.Stop();
                            nostop = false;
                            break;
                        case "speed":
                            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(GetParameterAsFloat(2));
                            nostop = true;
                            break;
                        default:
                            isModeValid = false;
                            break;
                    }
                }
            }
            Stop();
        }

        public void OnDestroy()
        {
            if (playableDirector != null && !nostop)
            {
                playableDirector.Stop();
                if (mustDestroyPlayableAsset)
                {
                    DialogueManager.UnloadAsset(playableDirector.playableAsset);
                    playableDirector.playableAsset = null;
                }
                if (mustDestroyPlayableDirector) Destroy(playableDirector.gameObject);
            }
        }

    }

}
#endif
#endif
